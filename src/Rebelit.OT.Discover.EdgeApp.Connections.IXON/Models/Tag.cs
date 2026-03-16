using System.Text.Json.Serialization;

namespace Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

/// <summary>
///     Represents a Tag in the IXON API, containing properties that define the tag's source, associated variable, identifiers, logging settings, and other metadata. This class is used to model the data structure of tags as returned by the IXON API and to facilitate interactions with tags when using the API client.
/// </summary>
/// <remarks>
///     Tags define what data is collected and how it is processed in the IXON system. They can be associated with variables, have specific logging settings, and can be used to trigger events or define retention policies. Understanding the properties of the Tag class is essential for effectively managing data collection and processing in IXON.
/// </remarks>
public class Tag
{
    [JsonPropertyName("source")]
    public Source Source { get; set; }

    [JsonPropertyName("variable")]
    public Variable Variable { get; set; }

    [JsonPropertyName("publicId")]
    public string PublicId { get; set; }

    [JsonPropertyName("tagId")]
    public int TagId { get; set; }

    [JsonPropertyName("slug")]
    public string Slug { get; set; }

    [JsonPropertyName("aggregators")]
    public List<object> Aggregators { get; set; } = new List<object>();

    [JsonPropertyName("edgeAggregator")]
    public object EdgeAggregator { get; set; }

    [JsonPropertyName("logEvent")]
    public string LogEvent { get; set; }

    [JsonPropertyName("logTrigger")]
    public object LogTrigger { get; set; }

    [JsonPropertyName("loggingInterval")]
    public string LoggingInterval { get; set; }

    [JsonPropertyName("retentionPolicy")]
    public string RetentionPolicy { get; set; }

    [JsonPropertyName("onChangeExpiry")]
    public object OnChangeExpiry { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}
