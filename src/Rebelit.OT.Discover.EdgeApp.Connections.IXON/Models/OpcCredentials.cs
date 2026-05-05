namespace Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

public class OpcCredentials
{
    public required string OpcUaServerAddress { get; set; }
    public string? OpcUaUsername { get; set; }
    public string? OpcUaPassword { get; set; }
}