using System.Text.Json.Serialization;

namespace Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

/// <summary>
///     The format of data prop in case we get a non 200 response from the IXON API.
/// </summary>
public class ErrorResponse
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }
    
    [JsonPropertyName("propertyName")]
    public string? PropertyName { get; set; }
}