using System.Text;
using Microsoft.Extensions.Logging;
using Opc.Ua;
using Opc.Ua.Configuration;
using Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Clients;

namespace Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Factory;

public class UAClientFactory(
    ApplicationInstance applicationInstance,
    ITelemetryContext telemetryContext
) : IUAClientFactory
{
    /// <summary>
    /// Creates and connects a new OPC UA client session using the specified server URI and user credentials.
    /// </summary>
    /// <returns>A connected UAClient instance if the connection is successful; otherwise, <see langword="null"/>.</returns>
    public async Task<UAClient?> Create(string uri, string username, string password)
    {
        var userIdentity = string.IsNullOrWhiteSpace(username)
            ? new UserIdentity()
            : new UserIdentity(username, Encoding.UTF8.GetBytes(password));

        var client = new UAClient(
            applicationInstance.ApplicationConfiguration,
            null,
            telemetryContext,
            null
        )
        {
            AutoAccept = true,
            SessionLifeTime = 60_000,
            UserIdentity = userIdentity,
        };

        try
        {
            var connected = await client.ConnectAsync(uri);
            if (!connected)
            {
                telemetryContext
                    .LoggerFactory.CreateLogger("UAClientFactory")
                    .LogError("Failed to connect to OPC UA server at {Uri}", uri);
                client.Dispose();
                return null;
            }

            return client;
        }
        catch (Exception ex)
        {
            client.Dispose();
            telemetryContext
                .LoggerFactory.CreateLogger("UAClientFactory")
                .LogError(ex, "Failed to create OPC UA client for {Uri}", uri);
            return null;
        }
    }
}
