using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

/// <summary>
///     Represents a standard response from the IXON API, containing the data and additional metadata about the response.
/// </summary>
/// <typeparam name="T"></typeparam>
public class Response<T>
{
    /// <summary>
    ///     The raw "data" field from the API. It may contain either the expected payload or an error object.
    ///     Use <see cref="Data"/> or <see cref="Error"/> to access the parsed value.
    /// </summary>
    [JsonPropertyName("data")]
    public JsonElement? RawData { private get; set; }
    
    /// <summary>
    ///     Indicates whether there are more results available after the current response. This is typically used for paginated
    ///     responses, where the API returns a subset of the total data and provides a link or token to retrieve the next set of
    ///     results if this property is true.
    /// </summary>
    [JsonPropertyName("moreAfter")]
    public string? MoreAfter { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    ///     The main data payload of the response. Returns <c>null</c> if the response contains an error or the data is absent.
    /// </summary>
    [JsonIgnore]
    public T? Data => TryGetData(out var value) ? value : default;
    
    [JsonIgnore]
    public bool HasError => Status == "error";
    
    [JsonIgnore]
    public string? ErrorMessage => TryGetErrorMessage();

    /// <summary>
    ///     The error response, populated when the API returns an error object in the "data" field.
    /// </summary>
    [JsonIgnore]
    private ErrorResponse[]? Error => TryGetError(out var error) ? error : null;

    private bool TryGetData(out T? value)
    {
        value = default;
        
        if (HasError || RawData is not { } element 
            || element.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return false;
        }

        try
        {
            value = element.Deserialize<T>();
            return value is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private bool TryGetError(out ErrorResponse[]? error)
    {
        error = null;
        if (!HasError || RawData is not { ValueKind: JsonValueKind.Array } element)
        {
            return false;
        }

        try
        {
            error = element.Deserialize<ErrorResponse[]>();
            return error is { Length: > 0 };
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private string? TryGetErrorMessage()
    {
        return Error?.Aggregate("", (acc, next)
            => $"{acc} {next.PropertyName}: {next.Message}".Trim());
    }
}
