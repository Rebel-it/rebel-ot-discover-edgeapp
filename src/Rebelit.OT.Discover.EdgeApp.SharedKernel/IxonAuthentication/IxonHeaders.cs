namespace Rebelit.OT.Discover.EdgeApp.SharedKernel.IxonAuthentication;

public class IxonHeaders
{
    /// <summary>
    ///     The service account as set up in IXON Cloud.
    /// </summary>
    public required ServiceAccount ServiceAccount { get; set; }
    
    /// <summary>
    ///     The Company ID that is linked with the IXON service account.
    /// </summary>
    public string? CompanyId { get; set; }
    
    /// <summary>
    ///     The Agent ID that represents the IXON device.
    /// </summary>
    public string? AgentId { get; set; }

    /// <summary>
    ///     The URL of the PLC.
    /// </summary>
    public string? PlcUrl { get; set; }

    /// <summary>
    ///     The username for authenticating with the PLC.
    /// </summary>
    public string? PlcUsername { get; set; }

    /// <summary>
    ///     The password for authenticating with the PLC.
    /// </summary>
    public string? PlcPassword { get; set; }

    /// <summary>
    /// Sourceid of the opcua datasource
    /// </summary>
    public string? SourceId { get; set; }
}