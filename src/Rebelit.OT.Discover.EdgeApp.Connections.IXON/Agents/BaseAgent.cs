using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

namespace Rebelit.OT.Discover.EdgeApp.Connections.IXON.Agents;

/// <summary>
///     Base class for IXON API agents, providing common HTTP request functionality and error handling.
/// </summary>
/// <param name="configuration"></param>
/// <param name="logger"></param>
internal abstract class BaseAgent(IOptions<Configuration> configuration, ILogger<BaseAgent> logger)
{
    /// <summary>
    ///     Sends a GET request to the specified URI and deserializes the response into the specified type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="uri"></param>
    /// <returns></returns>
    protected async Task<T> Get<T>(string uri)
    {
        using var httpClient = CreateHttpClient();
        var response = await httpClient.GetAsync($"{configuration.Value.BaseUrl}{uri}");
        await HandleResponseErrors(response);
        var content = await response.Content.ReadAsStringAsync();
        return System.Text.Json.JsonSerializer.Deserialize<T>(content)!;
    }

    /// <summary>
    ///     Sends a POST request to the specified URI with the provided body and deserializes the response into the specified type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="uri"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    protected async Task<T?> Post<T>(string uri, object body)
    {
        using var httpClient = CreateHttpClient();
        var content = CreateJsonContent(body);
        var response = await httpClient.PostAsync($"{configuration.Value.BaseUrl}{uri}", content);
        await HandleResponseErrors(response);
        return System.Text.Json.JsonSerializer.Deserialize<T>(
            await response.Content.ReadAsStringAsync()
        );
    }

    /// <summary>
    ///     Sends a POST request to the specified URI with the provided body without expecting a response.
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    protected async Task Post(string uri, object body)
    {
        using var httpClient = CreateHttpClient();
        var content = CreateJsonContent(body);
        var response = await httpClient.PostAsync($"{configuration.Value.BaseUrl}{uri}", content);
        await HandleResponseErrors(response);
    }

    /// <summary>
    ///     Returns an optional <see cref="HttpMessageHandler" /> used when constructing the <see cref="HttpClient" />.
    ///     Override in tests to inject a mock handler.
    /// </summary>
    protected virtual HttpMessageHandler? GetHttpMessageHandler() => null;

    /// <summary>
    ///     Creates and configures an HttpClient instance with the necessary headers for IXON API requests.
    /// </summary>
    /// <returns></returns>
    private HttpClient CreateHttpClient()
    {
        var httpClient = GetHttpMessageHandler() is { } handler
            ? new HttpClient(handler)
            : new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                configuration.Value.BearerToken
            );
        httpClient.DefaultRequestHeaders.Add("Api-Application", configuration.Value.ApplicationId);
        httpClient.DefaultRequestHeaders.Add("Api-Company", configuration.Value.CompanyId);
        httpClient.DefaultRequestHeaders.Add("Api-Version", configuration.Value.Version.ToString());
        return httpClient;
    }

    /// <summary>
    ///     Handles HTTP response error logging and ensures success status code.
    /// </summary>
    /// <param name="response"></param>
    private async Task HandleResponseErrors(HttpResponseMessage response)
    {
        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            logger.LogError("Bad Request: {ErrorContent}", errorContent);
        }
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    ///     Creates a StringContent object with JSON serialization.
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    private StringContent CreateJsonContent(object body)
    {
        var serialized = System.Text.Json.JsonSerializer.Serialize(body);
        return new StringContent(serialized, System.Text.Encoding.UTF8, "application/json");
    }
}
