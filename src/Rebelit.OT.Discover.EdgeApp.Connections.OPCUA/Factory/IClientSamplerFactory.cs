namespace Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Factory;

public interface IClientSamplerFactory
{
    Task<ClientSamples> CreateAsync();
}
