using System.Text.Json.Serialization;

namespace Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

public class Company
{
    [JsonPropertyName("publicId")]
    public string? PublicId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}