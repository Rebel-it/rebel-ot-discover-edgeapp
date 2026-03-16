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
    public async Task<UAClient?> Create(string uri, string username, string password)
    {
        var userIdentity = new UserIdentity(username, Encoding.UTF8.GetBytes(password));
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

        var connected = await client.ConnectAsync(uri);
        if (!connected)
        {
            telemetryContext
                .LoggerFactory.CreateLogger("UAClientFactory")
                .LogError("Failed to connect to OPC UA server at {Uri}", uri);
            return null;
        }

        return client;
    }
}
