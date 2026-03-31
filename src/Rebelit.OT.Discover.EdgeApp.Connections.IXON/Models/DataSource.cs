using System.Text.Json.Serialization;

namespace Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

/// <summary>
///     Represents a data source in the IXON API, containing properties that define its identity,
///     protocol, device association, and logging behaviour.
/// </summary>
public class DataSource
{
    [JsonPropertyName("publicId")]
    public string? PublicId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("slug")]
    public string Slug { get; set; } = null!;

    [JsonPropertyName("disabled")]
    public bool? Disabled { get; set; }

    [JsonPropertyName("continueLoggingOnError")]
    public bool? ContinueLoggingOnError { get; set; }

    [JsonPropertyName("gracePeriod")]
    public int? GracePeriod { get; set; }

    [JsonPropertyName("port")]
    public int? Port { get; set; }

    [JsonPropertyName("pullDelay")]
    public long? PullDelay { get; set; }

    [JsonPropertyName("mqttTopic")]
    public string? MqttTopic { get; set; }

    [JsonPropertyName("device")]
    public Source Device { get; set; } = null!;

    [JsonPropertyName("agent")]
    public Source? Agent { get; set; }

    [JsonPropertyName("mqttBrokerClient")]
    public Source? MqttBrokerClient { get; set; }

    [JsonPropertyName("protocol")]
    public DataSourceProtocol Protocol { get; set; } = null!;
}
