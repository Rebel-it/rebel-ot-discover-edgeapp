using System.Text.Json.Serialization;

namespace Rebelit.OT.Discover.EdgeApp.API.Models;

public class CreateTagRequest
{
    [JsonPropertyName("logEvent")]
    public required string LogEvent { get; init; }

    /// <summary>
    /// Interval when a new "log" is written on IXON
    /// </summary>
    [JsonPropertyName("loggingInterval")]
    public required string LoggingInterval { get; init; }

    /// <summary>
    /// Name of the tag
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// Filled when the specification is hanged to 
    /// On value change and each hour of unchanged value
    /// </summary>
    [JsonPropertyName("onChangeExpiry")]
    public string? OnChangeExpiry { get; init; }


    /// <summary>
    /// How long the data of the tag is stored in IXON
    /// </summary>
    [JsonPropertyName("retentionPolicy")]
    public required string RetentionPolicy { get; init; }

    /// <summary>
    /// Slug is the unique identifier for the tag, it is used to reference the tag in the IXON platform. It must be unique across all tags of the agent and can only contain lowercase letters, numbers, underscores and hyphens. It must start with a letter and be between 1 and 64 characters long.
    /// </summary>
    [JsonPropertyName("slug")]
    public required string Slug { get; init; }

    /// <summary>
    /// The Variable that the tag is linked too
    /// </summary>
    [JsonPropertyName("variable")]
    public required string Variable { get; init; }

    /// <summary>
    /// The type of formula 
    /// </summary>
    [JsonPropertyName("edgeAggregator")]
    public string? EdgeAggregator { get; init; }
}