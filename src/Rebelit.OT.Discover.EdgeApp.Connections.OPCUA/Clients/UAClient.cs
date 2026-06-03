using System.Collections;
using Microsoft.Extensions.Logging;
using Opc.Ua;
using Opc.Ua.Client;

namespace Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Clients;

/// <summary>
///     A simple OPC UA client that supports connecting to a server, handling keep-alives, and reconnecting on communication errors. It also supports durable subscription transfer and reverse connections.
/// </summary>
public class UAClient : IDisposable
{
    private readonly Lock _lock = new();
    private readonly ReverseConnectManager? _reverseConnectManager;
    private readonly ApplicationConfiguration _configuration;
    private SessionReconnectHandler? _reconnectHandler;
    private readonly ILogger _logger;
    private readonly ITelemetryContext _telemetry;
    private bool _disposed;

    /// <summary>
    /// Action used to validate responses.
    /// </summary>
    internal Action<IList, IList>? ValidateResponse { get; }

    /// <summary>
    /// Gets the client session.
    /// </summary>
    public ISession? Session { get; private set; }

    /// <summary>
    /// Raised whenever the session state changes.
    /// </summary>
    public event EventHandler<SessionStateChangedEventArgs>? SessionStateChanged;

    /// <summary>
    /// Gets whether the current session is connected.
    /// </summary>
    public bool IsConnected => Session?.Connected == true;

    /// <summary>
    /// The timeout in ms for establishing a connection. Default: 30 000 ms.
    /// </summary>
    public int ConnectTimeout { get; set; } = 30_000;

    /// <summary>
    /// The timeout in ms for waiting on a reverse connection endpoint. Default: 30 000 ms.
    /// </summary>
    public int ReverseConnectTimeout { get; set; } = 30_000;

    /// <summary>
    /// The session keepalive interval to be used in ms.
    /// </summary>
    public int KeepAliveInterval { get; set; } = 5000;

    /// <summary>
    /// The reconnect period to be used in ms.
    /// </summary>
    public int ReconnectPeriod { get; set; } = 1000;

    /// <summary>
    /// The reconnect period exponential backoff to be used in ms.
    /// </summary>
    public int ReconnectPeriodExponentialBackoff { get; set; } = 15000;

    /// <summary>
    /// The session lifetime.
    /// </summary>
    public uint SessionLifeTime { get; set; } = 60 * 1000;

    /// <summary>
    /// The user identity to use to connect to the server.
    /// </summary>
    public IUserIdentity UserIdentity { get; set; } = new UserIdentity();

    /// <summary>
    /// Auto accept untrusted certificates.
    /// </summary>
    public bool AutoAccept { get; set; }

    /// <summary>
    /// The file to use for log output.
    /// </summary>
    public string LogFile { get; set; }

    /// <summary>
    /// Initializes a new instance of the UAClient class.
    /// </summary>
    public UAClient(
        ApplicationConfiguration configuration,
        ITelemetryContext telemetry,
        Action<IList, IList> validateResponse
    )
        : this(configuration, null, telemetry, validateResponse) { }

    /// <summary>
    /// Initializes a new instance of the UAClient class for reverse connections.
    /// </summary>
    public UAClient(
        ApplicationConfiguration configuration,
        ReverseConnectManager? reverseConnectManager,
        ITelemetryContext telemetry,
        Action<IList, IList>? validateResponse
    )
    {
        ValidateResponse = validateResponse;
        _logger = telemetry.CreateLogger<UAClient>();
        _telemetry = telemetry;
        _configuration = configuration;
        _reverseConnectManager = reverseConnectManager;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Do a Durable Subscription Transfer
    /// </summary>
    public async Task<bool> DurableSubscriptionTransferAsync(
        string serverUrl,
        bool useSecurity = true,
        CancellationToken ct = default
    )
    {
        SubscriptionCollection subscriptions = [.. Session!.Subscriptions];
        ISession? previousSession = Session;
        Session = null;

        if (
            !await ConnectAsync(serverUrl, useSecurity, ct).ConfigureAwait(false)
            || Session == null
        )
        {
            Session = previousSession;
            return false;
        }

        _logger.LogInformation(
            "Transferring {Count} subscriptions from old session to new session...",
            subscriptions.Count
        );
        bool success = await Session
            .TransferSubscriptionsAsync(subscriptions, true, ct)
            .ConfigureAwait(false);

        if (success)
        {
            _logger.LogInformation("Subscriptions transferred.");
        }

        return success;
    }

    /// <summary>
    /// Creates a session with the UA server
    /// </summary>
    /// <exception cref="ObjectDisposedException"></exception>
    /// <exception cref="ArgumentNullException"><paramref name="serverUrl"/> is <c>null</c>.</exception>
    /// <exception cref="ServiceResultException"></exception>
    public async Task<bool> ConnectAsync(
        string serverUrl,
        bool useSecurity = true,
        CancellationToken ct = default
    )
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(UAClient));
        }

        ArgumentNullException.ThrowIfNull(serverUrl);

        try
        {
            if (Session?.Connected == true)
            {
                _logger.LogInformation("Session already connected!");
                return true;
            }

            using var connectCts = new CancellationTokenSource(ConnectTimeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                ct,
                connectCts.Token
            );

            var (endpointDescription, connection) = await ResolveEndpointAsync(
                    serverUrl,
                    useSecurity,
                    linkedCts.Token
                )
                .ConfigureAwait(false);

            var endpointConfiguration = EndpointConfiguration.Create(_configuration);
            var endpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);
            var sessionFactory = new DefaultSessionFactory(_telemetry);

            ISession session = await sessionFactory
                .CreateAsync(
                    _configuration,
                    connection!,
                    endpoint,
                    connection == null,
                    false,
                    _configuration.ApplicationName,
                    SessionLifeTime,
                    UserIdentity,
                    null,
                    linkedCts.Token
                )
                .ConfigureAwait(false);

            if (session?.Connected != true)
            {
                _logger.LogWarning("Session created but not in Connected state.");
                session?.Dispose();
                return false;
            }

            ConfigureSession(session);
            _logger.LogInformation(
                "New Session Created with SessionName = {SessionName}",
                session.SessionName
            );

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Create Session Error");
            return false;
        }
    }

    private async Task<(
        EndpointDescription EndpointDescription,
        ITransportWaitingConnection? Connection
    )> ResolveEndpointAsync(string serverUrl, bool useSecurity, CancellationToken ct)
    {
        if (_reverseConnectManager != null)
        {
            return await WaitForReverseConnectionEndpointAsync(serverUrl, useSecurity, ct)
                .ConfigureAwait(false);
        }

        _logger.LogInformation("Connecting to... {ServerUrl}", serverUrl);
        var endpointDescription = await CoreClientUtils
            .SelectEndpointAsync(_configuration, serverUrl, useSecurity, _telemetry, ct)
            .ConfigureAwait(false);
        return (endpointDescription!, null);
    }

    private async Task<(
        EndpointDescription EndpointDescription,
        ITransportWaitingConnection? Connection
    )> WaitForReverseConnectionEndpointAsync(
        string serverUrl,
        bool useSecurity,
        CancellationToken ct
    )
    {
        _logger.LogInformation("Waiting for reverse connection to {ServerUrl}...", serverUrl);
        EndpointDescription? endpointDescription = null;
        ITransportWaitingConnection? connection = null;
        do
        {
            using var cts = new CancellationTokenSource(ReverseConnectTimeout);
            using var linkedCTS = CancellationTokenSource.CreateLinkedTokenSource(ct, cts.Token);
            connection = await _reverseConnectManager
                .WaitForConnectionAsync(new Uri(serverUrl), null, linkedCTS.Token)
                .ConfigureAwait(false);
            if (connection == null)
            {
                throw new ServiceResultException(
                    StatusCodes.BadTimeout,
                    "Waiting for a reverse connection timed out."
                );
            }
            if (endpointDescription == null)
            {
                _logger.LogInformation("Discovering reverse connection endpoints...");
                endpointDescription = await CoreClientUtils
                    .SelectEndpointAsync(_configuration, connection, useSecurity, _telemetry, ct)
                    .ConfigureAwait(false);
                connection = null;
            }
        } while (connection == null);

        return (endpointDescription!, connection);
    }

    private void ConfigureSession(ISession session)
    {
        Session = session;
        Session.KeepAliveInterval = KeepAliveInterval;
        Session.DeleteSubscriptionsOnClose = false;
        Session.TransferSubscriptionsOnReconnect = true;
        Session.KeepAlive += Session_KeepAlive;
        _reconnectHandler = new SessionReconnectHandler(
            _telemetry,
            true,
            ReconnectPeriodExponentialBackoff
        );
        OnSessionStateChanged(new SessionStateChangedEventArgs(UAClientState.Connected, session));
    }

    /// <summary>
    /// Disconnects the session.
    /// </summary>
    /// <param name="leaveChannelOpen">Leaves the channel open.</param>
    public async Task DisconnectAsync(bool leaveChannelOpen = false, CancellationToken ct = default)
    {
        try
        {
            if (Session != null)
            {
                _logger.LogInformation("Disconnecting...");

                lock (_lock)
                {
                    Session.KeepAlive -= Session_KeepAlive;
                    Dispose();
                    _reconnectHandler = null;
                }

                await Session.CloseAsync(!leaveChannelOpen, ct).ConfigureAwait(false);
                if (leaveChannelOpen)
                {
                    Session.DetachChannel();
                }
                Session.Dispose();
                Session = null;

                _logger.LogInformation("Session Disconnected.");
                OnSessionStateChanged(new SessionStateChangedEventArgs(UAClientState.Disconnected));
            }
            else
            {
                _logger.LogWarning("DisconnectAsync called but no active session.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Disconnect Error");
        }
    }

    /// <summary>
    /// Overridable Dispose pattern implementation.
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }
        if (disposing)
        {
            _reconnectHandler?.Dispose();
            Utils.SilentDispose(Session);
            _configuration.CertificateValidator.CertificateValidation -= CertificateValidation;
        }
        _disposed = true;
    }

    /// <summary>
    /// Handles a keep alive event from a session and triggers a reconnect if necessary.
    /// </summary>
    private void Session_KeepAlive(ISession session, KeepAliveEventArgs e)
    {
        try
        {
            // check for events from discarded sessions.
            if (Session == null || !Session.Equals(session))
            {
                return;
            }

            // start reconnect sequence on communication error.
            if (ServiceResult.IsBad(e.Status))
            {
                if (ReconnectPeriod <= 0)
                {
                    _logger.LogWarning(
                        "KeepAlive status {StatusCode}, but reconnect is disabled.",
                        e.Status
                    );
                    return;
                }

                SessionReconnectHandler.ReconnectState state = _reconnectHandler.BeginReconnect(
                    Session,
                    _reverseConnectManager,
                    ReconnectPeriod,
                    Client_ReconnectComplete
                );
                if (state == SessionReconnectHandler.ReconnectState.Triggered)
                {
                    _logger.LogInformation(
                        "KeepAlive status {StatusCode}, reconnect status {State}, reconnect period {ReconnectPeriod}ms.",
                        e.Status,
                        state,
                        ReconnectPeriod
                    );
                    OnSessionStateChanged(
                        new SessionStateChangedEventArgs(UAClientState.Reconnecting, Session)
                    );
                }
                else
                {
                    _logger.LogInformation(
                        "KeepAlive status {StatusCode}, reconnect status {State}.",
                        e.Status,
                        state
                    );
                }

                // cancel sending a new keep alive request, because reconnect is triggered.
                e.CancelKeepAlive = true;
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error in OnKeepAlive.");
        }
    }

    /// <summary>
    /// Called when the reconnect attempt was successful.
    /// </summary>
    private void Client_ReconnectComplete(object sender, EventArgs e)
    {
        // ignore callbacks from discarded objects.
        if (!ReferenceEquals(sender, _reconnectHandler))
        {
            return;
        }

        lock (_lock)
        {
            // if session recovered, Session property is null
            if (_reconnectHandler.Session != null)
            {
                // ensure only a new instance is disposed
                // after reactivate, the same session instance may be returned
                if (!ReferenceEquals(Session, _reconnectHandler.Session))
                {
                    _logger.LogInformation(
                        "--- RECONNECTED TO NEW SESSION --- {SessionId}",
                        _reconnectHandler.Session.SessionId
                    );
                    ISession session = Session;
                    Session = _reconnectHandler.Session;
                    Utils.SilentDispose(session);
                }
                else
                {
                    _logger.LogInformation(
                        "--- REACTIVATED SESSION --- {SessionId}",
                        _reconnectHandler.Session.SessionId
                    );
                }
                OnSessionStateChanged(
                    new SessionStateChangedEventArgs(UAClientState.Reconnected, Session)
                );
            }
            else
            {
                _logger.LogInformation("--- RECONNECT KeepAlive recovered ---");
                OnSessionStateChanged(
                    new SessionStateChangedEventArgs(UAClientState.Reconnected, Session)
                );
            }
        }
    }

    private void OnSessionStateChanged(SessionStateChangedEventArgs args)
    {
        SessionStateChanged?.Invoke(this, args);
    }

    /// <summary>
    /// Handles the certificate validation event.
    /// This event is triggered every time an untrusted certificate is received from the server.
    /// </summary>
    protected virtual void CertificateValidation(
        CertificateValidator sender,
        CertificateValidationEventArgs e
    )
    {
        bool certificateAccepted = false;

        // ****
        // Implement a custom logic to decide if the certificate should be
        // accepted or not and set certificateAccepted flag accordingly.
        // The certificate can be retrieved from the e.Certificate field
        // ***

        ServiceResult error = e.Error;
        _logger.LogInformation("{Error}", error);
        if (error.StatusCode == StatusCodes.BadCertificateUntrusted && AutoAccept)
        {
            certificateAccepted = true;
        }

        if (certificateAccepted)
        {
            _logger.LogInformation(
                "Untrusted Certificate accepted. Subject = {Subject}",
                e.Certificate.Subject
            );
            e.Accept = true;
        }
        else
        {
            _logger.LogInformation(
                "Untrusted Certificate rejected. Subject = {Subject}",
                e.Certificate.Subject
            );
        }
    }
}
