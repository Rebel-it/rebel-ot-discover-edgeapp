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
    ///     The version of the IXON API to use. This value determines which API endpoints and features are available.
    /// </summary>
    public int Version { get; set; } = 2;
}
