using Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Clients;

namespace Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Factory;

public interface IUAClientFactory
{
    Task<UAClient?> CreateAsync(string uri);
    Task<UAClient?> CreateAsync(string uri, string username, string password);
}
