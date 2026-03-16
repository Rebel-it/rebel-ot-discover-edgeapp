using System.Text.Json.Serialization;

namespace Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

/// <summary>
///     Represents the (data) source of an IXON API response, containing the public identifier of the source entity.
/// </summary>
public class Source
{
    /// <summary>
    ///     The public identifier of the source entity, which can be used to reference the specific resource or object in the IXON API.
    /// </summary>
    [JsonPropertyName("publicId")]
    public string PublicId { get; set; } = null!;
}
