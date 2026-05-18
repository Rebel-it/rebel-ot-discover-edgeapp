using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace Rebelit.OT.Discover.EdgeApp.Models;

public class UpdateTagRequest() : CreateTagRequest
{
    [JsonPropertyName("publicid")]
    public string PublicId { get; set; }
}