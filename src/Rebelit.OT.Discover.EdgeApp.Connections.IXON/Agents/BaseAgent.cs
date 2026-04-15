using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Factories;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

namespace Rebelit.OT.Discover.EdgeApp.Connections.IXON.Agents;

internal abstract class BaseAgent(
    IOptions<Configuration> configuration,
    ILogger<BaseAgent> logger,
    TimeProvider timeProvider
)
{
    private readonly ResiliencePipeline<HttpResponseMessage> _pipeline =
        IxonResiliencePipelineFactory.Create(timeProvider, logger);

    protected async Task<T> Get<T>(string uri)
    {
        logger.LogInformation("Get<{Type}> called with uri: {Uri}", typeof(T).Name, uri);
        var response = await ExecuteRequestAsync(http =>
            http.GetAsync($"{configuration.Value.BaseUrl}{uri}")
        );
        await HandleResponseErrors(response);
        var content = await response.Content.ReadAsStringAsync();
        return System.Text.Json.JsonSerializer.Deserialize<T>(content)!;
    }

    protected async Task<T?> Post<T>(string uri, object body)
    {
        logger.LogInformation("Post<{Type}> called with uri: {Uri}", typeof(T).Name, uri);
        var response = await ExecuteRequestAsync(http =>
            http.PostAsync($"{configuration.Value.BaseUrl}{uri}", CreateJsonContent(body))
        );
        await HandleResponseErrors(response);
        return System.Text.Json.JsonSerializer.Deserialize<T>(
            await response.Content.ReadAsStringAsync()
        );
    }

    protected async Task Post(string uri, object body)
    {
        logger.LogInformation("Post called with uri: {Uri}", uri);
        var response = await ExecuteRequestAsync(http =>
            http.PostAsync($"{configuration.Value.BaseUrl}{uri}", CreateJsonContent(body))
        );
        await HandleResponseErrors(response);
    }

    private async Task<HttpResponseMessage> ExecuteRequestAsync(
        Func<HttpClient, Task<HttpResponseMessage>> send
    )
    {
        logger.LogDebug("ExecuteRequestAsync called");
        return await _pipeline.ExecuteAsync(async _ =>
        {
            using var httpClient = CreateHttpClient();
            return await send(httpClient);
        });
    }

    protected virtual HttpMessageHandler? GetHttpMessageHandler() => null;

    private HttpClient CreateHttpClient()
    {
        logger.LogDebug("CreateHttpClient called");
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

    private async Task HandleResponseErrors(HttpResponseMessage response)
    {
        logger.LogDebug("HandleResponseErrors called with status code: {StatusCode}", response.StatusCode);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            logger.LogError(
                "Request failed with status code {StatusCode}: {ErrorContent}",
                response.StatusCode,
                errorContent
            );
        }
        response.EnsureSuccessStatusCode();
    }

    private StringContent CreateJsonContent(object body)
    {
        logger.LogDebug("CreateJsonContent called");
        var serialized = System.Text.Json.JsonSerializer.Serialize(body);
        logger.LogInformation("Request JSON: {Json}", serialized);
        return new StringContent(serialized, System.Text.Encoding.UTF8, "application/json");
    }
}
