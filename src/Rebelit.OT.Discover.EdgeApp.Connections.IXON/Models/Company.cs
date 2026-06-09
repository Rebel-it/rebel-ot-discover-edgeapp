using System.Text.Json.Serialization;

namespace Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

public class Company
{
    /// <summary>
    /// Id of the company in the Ixon fleet manager
    /// </summary>
    [JsonPropertyName("publicId")]
    public string? PublicId { get; set; }

    /// <summary>
    /// Name of the company in the Ixon fleet manager
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}