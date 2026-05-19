using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace Rebelit.OT.Discover.EdgeApp.API.Models;

public class UpdateTagRequest() : CreateTagRequest
{
    [JsonPropertyName("publicid")]
    public required string PublicId { get; set; }
}