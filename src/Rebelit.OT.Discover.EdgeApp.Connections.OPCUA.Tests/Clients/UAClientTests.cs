using Moq;
using Opc.Ua;
using Opc.Ua.Client;
using Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Clients;
using Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Telemetry;

namespace Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Tests.Clients;

[TestFixture]
public class UAClientTests
{
    private UAClient _sut = null!;
    private bool _sutDisposed;

    [SetUp]
    public void SetUp()
    {
        _sutDisposed = false;
        var telemetry = new ConsoleTelemetry();
        _sut = new UAClient(
            CreateMinimalApplicationConfiguration(telemetry),
            telemetry,
            validateResponse: (_, _) => { }
        );
    }

    [TearDown]
    public void TearDown()
    {
        if (!_sutDisposed)
        {
            _sut.Dispose();
        }
    }

    [Test]
    public void ConnectTimeout_Default_Is30000()
    {
        Assert.That(_sut.ConnectTimeout, Is.EqualTo(30_000));
    }

    [Test]
    public void ReverseConnectTimeout_Default_Is30000()
    {
        Assert.That(_sut.ReverseConnectTimeout, Is.EqualTo(30_000));
    }

    [Test]
    public void IsConnected_WhenNoSession_ReturnsFalse()
    {
        Assert.That(_sut.IsConnected, Is.False);
    }

    [Test]
    public async Task ConnectAsync_WhenDisposed_ThrowsObjectDisposedException()
    {
        _sut.Dispose();
        _sutDisposed = true;

        Assert.ThrowsAsync<ObjectDisposedException>(() =>
            _sut.ConnectAsync("opc.tcp://localhost:4840")
        );

        await Task.CompletedTask;
    }

    [Test]
    public void ConnectAsync_WhenServerUrlIsNull_ThrowsArgumentNullException()
    {
        Assert.ThrowsAsync<ArgumentNullException>(() => _sut.ConnectAsync(null!));
    }

    [Test]
    public async Task DisconnectAsync_WhenNoSession_DoesNotThrow()
    {
        Assert.DoesNotThrowAsync(() => _sut.DisconnectAsync());

        await Task.CompletedTask;
    }

    [Test]
    public void SessionStateChanged_WhenNoHandlers_RaisesNoException()
    {
        Assert.DoesNotThrow(() =>
        {
            _sut.Dispose();
            _sutDisposed = true;
        });
    }

    [Test]
    public void SessionStateChanged_WhenHandlerRegistered_IsNotFiredOnConstruction()
    {
        int eventCount = 0;
        _sut.SessionStateChanged += (_, _) => eventCount++;

        Assert.That(eventCount, Is.EqualTo(0));
    }

    [Test]
    public async Task ConnectAsync_WhenSessionAlreadyConnected_ReturnsTrue()
    {
        var sessionMock = new Mock<ISession>();
        sessionMock.SetupGet(x => x.Connected).Returns(true);
        SetSession(_sut, sessionMock.Object);

        var result = await _sut.ConnectAsync("opc.tcp://localhost:4840");

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task DisconnectAsync_WhenSessionExists_DoesNotThrowAndDisposesSession()
    {
        var sessionMock = new Mock<ISession>();
        SetSession(_sut, sessionMock.Object);

        await _sut.DisconnectAsync();

        sessionMock.Verify(x => x.Dispose(), Times.AtLeastOnce);
    }

    [Test]
    public async Task DisconnectAsync_WhenLeaveChannelOpen_DoesNotThrow()
    {
        var sessionMock = new Mock<ISession>();
        SetSession(_sut, sessionMock.Object);

        Assert.DoesNotThrowAsync(() => _sut.DisconnectAsync(leaveChannelOpen: true));

        await Task.CompletedTask;
    }

    [Test]
    public void KeepAliveInterval_Default_Is5000()
    {
        Assert.That(_sut.KeepAliveInterval, Is.EqualTo(5000));
    }

    [Test]
    public void ReconnectPeriod_Default_Is1000()
    {
        Assert.That(_sut.ReconnectPeriod, Is.EqualTo(1000));
    }

    [Test]
    public void ReconnectPeriodExponentialBackoff_Default_Is15000()
    {
        Assert.That(_sut.ReconnectPeriodExponentialBackoff, Is.EqualTo(15000));
    }

    [Test]
    public void SessionLifeTime_Default_Is60000()
    {
        Assert.That(_sut.SessionLifeTime, Is.EqualTo(60_000u));
    }

    private static void SetSession(UAClient client, ISession session)
    {
        typeof(UAClient)
            .GetProperty(nameof(UAClient.Session))!
            .SetValue(client, session);
    }

    private static ApplicationConfiguration CreateMinimalApplicationConfiguration(
        ITelemetryContext telemetry
    ) =>
        new()
        {
            ApplicationName = "UAClientTests",
            ApplicationType = ApplicationType.Client,
            CertificateValidator = new CertificateValidator(telemetry),
            TransportConfigurations = [],
            ClientConfiguration = new ClientConfiguration(),
            TransportQuotas = new TransportQuotas(),
        };
}
