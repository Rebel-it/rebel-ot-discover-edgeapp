namespace Rebelit.OT.Discover.EdgeApp.SharedKernel.IxonAuthentication;

/// <summary>
/// Service acount is an account created within a IXON company.
/// </summary>
public record ServiceAccount
{
    /// <summary>
    /// ApplicationID of the servicec account
    /// </summary>
    public required string ApiApplicationId { get; set; }

    /// <summary>
    /// Login token of the service account
    /// </summary>
    public required string AccessToken { get; set; }
}