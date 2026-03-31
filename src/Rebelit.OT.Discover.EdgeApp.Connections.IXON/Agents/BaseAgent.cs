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
        var response = await ExecuteRequestAsync(http =>
            http.GetAsync($"{configuration.Value.BaseUrl}{uri}")
        );
        await HandleResponseErrors(response);
        var content = await response.Content.ReadAsStringAsync();
        return System.Text.Json.JsonSerializer.Deserialize<T>(content)!;
    }

    protected async Task<T?> Post<T>(string uri, object body)
    {
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
        var response = await ExecuteRequestAsync(http =>
            http.PostAsync($"{configuration.Value.BaseUrl}{uri}", CreateJsonContent(body))
        );
        await HandleResponseErrors(response);
    }

    private async Task<HttpResponseMessage> ExecuteRequestAsync(
        Func<HttpClient, Task<HttpResponseMessage>> send
    )
    {
        return await _pipeline.ExecuteAsync(async _ =>
        {
            using var httpClient = CreateHttpClient();
            return await send(httpClient);
        });
    }

    protected virtual HttpMessageHandler? GetHttpMessageHandler() => null;

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

    private async Task HandleResponseErrors(HttpResponseMessage response)
    {
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

    private static StringContent CreateJsonContent(object body)
    {
        var serialized = System.Text.Json.JsonSerializer.Serialize(body);
        return new StringContent(serialized, System.Text.Encoding.UTF8, "application/json");
    }
}
