using Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Clients;

namespace Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Factory;

public interface IUAClientFactory
{
    Task<UAClient?> Create(string uri);
    Task<UAClient?> Create(string opcServerAddress, string username, string password);
}
