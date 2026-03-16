using System.Text.Json.Serialization;

namespace Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

/// <summary>
///     Represents a standard response from the IXON API, containing the data and additional metadata about the response.
/// </summary>
/// <typeparam name="T"></typeparam>
public class Response<T>
{
    /// <summary>
    ///     The main data payload of the response, containing the requested information from the IXON API. The type of this property is generic, allowing it to represent various types of data depending on the API endpoint being called.
    /// </summary>
    [JsonPropertyName("data")]
    public T Data { get; set; } = default!;

    /// <summary>
    ///     Indicates whether there are more results available after the current response. This is typically used for paginated responses, where the API returns a subset of the total data and provides a link or token to retrieve the next set of results if this property is true.
    /// </summary>
    [JsonPropertyName("moreAfter")]
    public string? MoreAfter { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}
