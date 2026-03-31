using Opc.Ua;

namespace Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Factory;

public class ClientSamplerFactory(ITelemetryContext telemetryContext) : IClientSamplerFactory
{
    public async Task<ClientSamples> CreateAsync()
    {
        return new ClientSamples(telemetryContext, null, null);
    }
}
