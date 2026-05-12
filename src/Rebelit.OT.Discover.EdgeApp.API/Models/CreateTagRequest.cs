using System.Text.Json.Serialization;

namespace Rebelit.OT.Discover.EdgeApp.API.Models;

public class CreateTagRequest
{
    [JsonPropertyName("logEvent")]
    public required string LogEvent { get; init; }

    [JsonPropertyName("loggingInterval")]
    public required string LoggingInterval { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("onChangeExpiry")]
    public string? OnChangeExpiry { get; init; }

    [JsonPropertyName("retentionPolicy")]
    public required string RetentionPolicy { get; init; }

    [JsonPropertyName("slug")]
    public required string Slug { get; init; }

    [JsonPropertyName("variable")]
    public required string Variable { get; init; }

    [JsonPropertyName("edgeAggregator")]
    public string? EdgeAggregator { get; init; }
}