using System.Text.Json.Serialization;

namespace Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

/// <summary>
/// Represents the request body for creating a new tag in the IXON API.
/// </summary>
public class Tag
{
    /// <summary>
    /// Defines the type of event that triggers data logging for this tag.
    /// </summary>
    /// <remarks>
    /// Supported values:
    /// <list type="bullet">
    ///   <item><description><c>interval</c> — logs data at a fixed time interval defined by <see cref="LoggingInterval"/>.</description></item>
    ///   <item><description><c>change</c> — logs data only when the value changes. Use <see cref="OnChangeExpiry"/> to also log at a fixed interval when the value remains unchanged.</description></item>
    /// </list>
    /// </remarks>
    [JsonPropertyName("logEvent")]
    public required string LogEvent { get; set; }

    /// <summary>
    /// The interval at which data is logged, expressed as a duration string.
    /// </summary>
    /// <remarks>
    /// Format: a numeric value followed by a unit suffix.
    /// <list type="bullet">
    ///   <item><description><c>ms</c> — milliseconds (e.g. <c>180ms</c>)</description></item>
    ///   <item><description><c>s</c> — seconds (e.g. <c>30s</c>)</description></item>
    ///   <item><description><c>m</c> — minutes (e.g. <c>5m</c>)</description></item>
    ///   <item><description><c>h</c> — hours (e.g. <c>1h</c>)</description></item>
    /// </list>
    /// </remarks>
    [JsonPropertyName("loggingInterval")]
    public required string LoggingInterval { get; set; }

    /// <summary>
    /// The display name of the tag.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// Specifies how long to wait before logging an unchanged value when <see cref="LogEvent"/> is set to <c>change</c>.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <item><description><see langword="null"/> — only logs on value changes (default).</description></item>
    ///   <item><description>Duration string (e.g. <c>1h</c>) — also logs at the given interval when the value has not changed.</description></item>
    /// </list>
    /// Only applies when <see cref="LogEvent"/> is <c>change</c>.
    /// </remarks>
    [JsonPropertyName("onChangeExpiry")]
    public object? OnChangeExpiry { get; set; }

    /// <summary>
    /// How long the logged data is retained in IXON, expressed as a duration string (e.g. <c>104w</c> for 2 years).
    /// </summary>
    [JsonPropertyName("retentionPolicy")]
    public required string RetentionPolicy { get; set; }

    /// <summary>
    /// A unique URL-friendly identifier for the tag. Must not contain spaces.
    /// </summary>
    [JsonPropertyName("slug")]
    public required string Slug { get; set; }

    /// <summary>
    /// A reference to the data variable this tag is linked to.
    /// </summary>
    [JsonPropertyName("variable")]
    public required TagVariable Variable { get; set; }

    /// <summary>
    /// The aggregation formula applied to the tag value at the edge before it is sent to the cloud.
    /// </summary>
    /// <remarks>
    /// Supported values:
    /// <list type="bullet">
    ///   <item><description><c>Mean</c> — average of values in the interval.</description></item>
    ///   <item><description><c>Min</c> — minimum value in the interval.</description></item>
    ///   <item><description><c>Max</c> — maximum value in the interval.</description></item>
    ///   <item><description><c>Last</c> — last recorded value in the interval (default).</description></item>
    ///   <item><description><c>Increase</c> — total increase of the value in the interval.</description></item>
    /// </list>
    /// Set to <see langword="null"/> to use the default (<c>Last</c>).
    /// </remarks>
    [JsonPropertyName("edgeAggregator")]
    public object? EdgeAggregator { get; set; }
}

/// <summary>
/// A lightweight reference to an IXON data variable, identified by its public ID.
/// </summary>
public class TagVariable
{
    /// <summary>
    /// The public identifier of the data variable in IXON.
    /// </summary>
    [JsonPropertyName("publicId")]
    public required string PublicId { get; set; }
}
