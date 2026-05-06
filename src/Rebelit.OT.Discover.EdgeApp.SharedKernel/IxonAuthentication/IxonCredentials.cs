namespace Rebelit.OT.Discover.EdgeApp.SharedKernel.IxonAuthentication;

public class IxonCredentials
{
    /// <summary>
    ///     Application ID associated with the IXON service account.
    /// </summary>
    public required string ApplicationId { get; set; }
    
    /// <summary>
    ///     Access Token associated with the IXON service account.
    /// </summary>
    public required string AccessToken { get; set; }
    
    /// <summary>
    ///     The Company ID that is linked with the IXON service account.
    /// </summary>
    public string? CompanyId { get; set; } = null!;
    
    /// <summary>
    ///     The Agent ID that represents the IXON device.
    /// </summary>
    public string? AgentId { get; set; } = null!;
}