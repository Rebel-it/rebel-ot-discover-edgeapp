namespace Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

/// <summary>
///     Configuration class for IXON API client, containing necessary settings for connecting to the IXON API, such as base URL, authentication credentials, and API version.
/// </summary>
internal class Configuration
{
    /// <summary>
    ///     The base URL for the IXON API. This is the endpoint to which all API requests will be sent. The default value is set to "https://portal.ixon.cloud", but it can be overridden if necessary.
    /// </summary>
    public string BaseUrl { get; set; } = "https://portal.ixon.cloud";

    /// <summary>
    ///     The Application ID provided by IXON for authentication. This is a required field for connecting to the IXON API and must be set to a valid value for successful API requests.
    /// </summary>
    public string ApplicationId { get; set; } = null!;

    /// <summary>
    ///     The Bearer Token used for authenticating API requests. This token is required for accessing the IXON API and must be kept secure.
    /// </summary>
    public string BearerToken { get; set; } = null!;

    /// <summary>
    ///     The Company ID associated with the IXON account. This ID is required for API requests and must be set to a valid value.
    /// </summary>
    public string CompanyId { get; set; } = null!;

    /// <summary>
    ///     User email for token refresh.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    ///     User password for token refresh.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    ///     Optional OTP code for token refresh.
    /// </summary>
    public string? OtpCode { get; set; }

    /// <summary>
    ///     The version of the IXON API to use. This value determines which API endpoints and features are available.
    /// </summary>
    public int Version { get; set; } = 2;
}
