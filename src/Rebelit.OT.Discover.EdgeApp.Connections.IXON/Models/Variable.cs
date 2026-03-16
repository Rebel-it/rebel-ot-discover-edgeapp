using System.Text.Json.Serialization;

namespace Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

/// <summary>
///     Represents a Variable in the IXON API, containing properties that define the variable's identifiers, address, type, source, and other metadata. This class is used to model the data structure of variables as returned by the IXON API and to facilitate interactions with variables when using the API client.
/// </summary>
/// <remarks>
///    Variables represent data points in the IXON system, defined by their address and type to the node.
/// </remarks>
public class Variable
{
    [JsonPropertyName("publicId")]
    public string PublicId { get; set; } = null!;

    [JsonPropertyName("address")]
    public string Address { get; set; } = null!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("slug")]
    public string Slug { get; set; } = null!;

    [JsonPropertyName("type")]
    public string Type { get; set; } = null!;

    [JsonPropertyName("width")]
    public string Width { get; set; } = null!;

    [JsonPropertyName("source")]
    public Source Source { get; set; } = null!;

    [JsonPropertyName("signed")]
    public bool? Signed { get; set; }
}
