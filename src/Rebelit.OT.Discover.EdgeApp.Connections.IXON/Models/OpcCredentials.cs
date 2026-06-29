namespace Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

/// <summary>
/// Opc Ua credentials used for IXON connection. This class contains the necessary information to connect to an OPC UA server, 
/// including the server address and optional authentication credentials (username and password).
/// </summary>
public class OpcCredentials
{
    /// <summary>
    /// Opc ua server address 
    /// </summary>
    public required string OpcUaServerAddress { get; set; }

    /// <summary>
    /// Optional username for Opc Ua
    /// </summary>
    public string? OpcUaUsername { get; set; }

    /// <summary>
    /// Optional password for Opc ua
    /// </summary>
    public string? OpcUaPassword { get; set; }
}