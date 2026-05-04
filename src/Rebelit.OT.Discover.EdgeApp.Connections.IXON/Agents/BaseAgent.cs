using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Factories;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

namespace Rebelit.OT.Discover.EdgeApp.Connections.IXON.Agents;

internal abstract class BaseAgent
{
    private readonly IOptionsMonitor<Configuration> _configuration;
    private readonly ILogger<BaseAgent> _logger;
    private readonly ResiliencePipeline<HttpResponseMessage> _pipeline;

    protected BaseAgent(
        IOptionsMonitor<Configuration> configuration,
        ILogger<BaseAgent> logger,
        TimeProvider timeProvider
    )
    {
        _configuration = configuration;
        _logger = logger;
        _pipeline = IxonResiliencePipelineFactory.Create(timeProvider, logger);
    }

    protected async Task<T> Get<T>(string uri)
    {
        var response = await ExecuteRequestAsync(http =>
            http.GetAsync($"{_configuration.CurrentValue.BaseUrl}{uri}")
        );
        await HandleResponseErrors(response);
        var content = await response.Content.ReadAsStringAsync();
        return System.Text.Json.JsonSerializer.Deserialize<T>(content)!;
    }

    protected async Task<T?> Post<T>(string uri, object body)
    {
        var response = await ExecuteRequestAsync(http =>
            http.PostAsync($"{_configuration.CurrentValue.BaseUrl}{uri}", CreateJsonContent(body))
        );
        await HandleResponseErrors(response);
        return System.Text.Json.JsonSerializer.Deserialize<T>(
            await response.Content.ReadAsStringAsync()
        );
    }

    protected async Task Post(string uri, object body)
    {
        var response = await ExecuteRequestAsync(http =>
            http.PostAsync($"{_configuration.CurrentValue.BaseUrl}{uri}", CreateJsonContent(body))
        );
        await HandleResponseErrors(response);
    }

    protected async Task<T?> PostCsv<T>(string uri, string csv)
    {
        var content = new StringContent(csv, System.Text.Encoding.UTF8, "text/csv");
        var response = await ExecuteRequestAsync(http =>
            http.PostAsync($"{_configuration.CurrentValue.BaseUrl}{uri}", content)
        );
        await HandleResponseErrors(response);
        return System.Text.Json.JsonSerializer.Deserialize<T>(
            await response.Content.ReadAsStringAsync()
        );
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
                _configuration.CurrentValue.BearerToken
            );
        httpClient.DefaultRequestHeaders.Add("Api-Application", _configuration.CurrentValue.ApplicationId);
        httpClient.DefaultRequestHeaders.Add("Api-Company", _configuration.CurrentValue.CompanyId);
        httpClient.DefaultRequestHeaders.Add("Api-Version", _configuration.CurrentValue.Version.ToString());
        return httpClient;
    }

    private async Task HandleResponseErrors(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError(
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
