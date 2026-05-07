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
    public string? CompanyId { get; set; } = null!;
    
    /// <summary>
    ///     The Agent ID that represents the IXON device.
    /// </summary>
    public string? AgentId { get; set; } = null!;
}