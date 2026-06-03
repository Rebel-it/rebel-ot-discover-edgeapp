using System.Text.Json.Serialization;

namespace Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

/// <summary>
///     Represents a Variable in the IXON API, containing properties that define the variable's identifiers, address, type, source, and other metadata.
///     This class is used to model the data structure of variables as returned by the IXON API and to facilitate interactions with variables when using the API client.
/// </summary>
/// <remarks>
///    Variables represent data points in the IXON system, defined by their address and type to the node.
/// </remarks>
public class Variable
{
    [JsonPropertyName("publicId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PublicId { get; set; }

    [JsonPropertyName("address")]
    public string Address { get; set; } = null!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("slug")]
    public string Slug { get; set; } = null!;

    [JsonPropertyName("type")]
    public string Type { get; set; } = null!;

    [JsonPropertyName("width")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Width { get; set; }

    [JsonPropertyName("maxStringLength")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? MaxStringLength { get; set; }

    [JsonPropertyName("source")]
    public Source Source { get; set; } = null!;

    [JsonPropertyName("signed")]
    public bool? Signed { get; set; }

    // Conditional serialization methods
    public bool ShouldSerializeWidth() => Type != "str" && Width != null;

    public bool ShouldSerializeMaxStringLength() => Type == "str" && MaxStringLength != null;

    public bool ShouldSerializeSigned() => Type != "str" && Type != "bool";
}
