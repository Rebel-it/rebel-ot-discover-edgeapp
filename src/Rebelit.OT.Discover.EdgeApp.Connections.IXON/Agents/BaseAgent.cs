using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Factories;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;
using Rebelit.OT.Discover.EdgeApp.SharedKernel.IxonAuthentication;
using System.Globalization;

namespace Rebelit.OT.Discover.EdgeApp.Connections.IXON.Agents;

internal abstract class BaseAgent
{
    private readonly IOptionsMonitor<Configuration> _configuration;
    private readonly ResiliencePipeline<HttpResponseMessage> _pipeline;
    private readonly IIxonAuthenticationContext _ixonAuthenticationContext;

    protected BaseAgent(
        IOptionsMonitor<Configuration> configuration,
        ILogger<BaseAgent> logger,
        TimeProvider timeProvider,
        IIxonAuthenticationContext ixonAuthenticationContext
    )
    {
        _configuration = configuration;
        _pipeline = IxonResiliencePipelineFactory.Create(timeProvider, logger);
        _ixonAuthenticationContext = ixonAuthenticationContext;
    }

    protected async Task<T> GetAsync<T>(
        Uri uri,
        System.Text.Json.Serialization.Metadata.JsonTypeInfo<T>? jsonTypeInfo = null
    )
    {
        var response = await ExecuteRequestAsync(http =>
            http.GetAsync($"{_configuration.CurrentValue.BaseUrl}{uri}")
        );
        var content = await response.Content.ReadAsStringAsync();

        return jsonTypeInfo is null
            ? System.Text.Json.JsonSerializer.Deserialize<T>(content)!
            : System.Text.Json.JsonSerializer.Deserialize(content, jsonTypeInfo)!;
    }

    protected async Task<T?> PostAsync<T>(
        Uri uri,
        object body,
        System.Text.Json.Serialization.Metadata.JsonTypeInfo<T>? jsonTypeInfo = null
    )
    {
        var response = await ExecuteRequestAsync(http =>
            http.PostAsync($"{_configuration.CurrentValue.BaseUrl}{uri}", CreateJsonContent(body))
        );
        var content = await response.Content.ReadAsStringAsync();

        return jsonTypeInfo is null
            ? System.Text.Json.JsonSerializer.Deserialize<T>(content)
            : System.Text.Json.JsonSerializer.Deserialize(content, jsonTypeInfo);
    }
    protected async Task PostAsync(Uri uri, object body)
    {
        await ExecuteRequestAsync(http =>
            http.PostAsync($"{_configuration.CurrentValue.BaseUrl}{uri}", CreateJsonContent(body))
        );
    }

    protected async Task<T?> PatchAsync<T>(
        Uri uri,
        object body,
        System.Text.Json.Serialization.Metadata.JsonTypeInfo<T>? jsonTypeInfo = null
    )
    {
        var response = await ExecuteRequestAsync(http =>
           http.PatchAsync($"{_configuration.CurrentValue.BaseUrl}{uri}", CreateJsonContent(body))
        );
        var content = await response.Content.ReadAsStringAsync();

        return jsonTypeInfo is null
            ? System.Text.Json.JsonSerializer.Deserialize<T>(content)
            : System.Text.Json.JsonSerializer.Deserialize(content, jsonTypeInfo);
    }

    protected async Task<T?> PutAsync<T>(
        Uri uri,
        object body,
        System.Text.Json.Serialization.Metadata.JsonTypeInfo<T>? jsonTypeInfo = null
    )
    {
        var response = await ExecuteRequestAsync(http =>
            http.PutAsync($"{_configuration.CurrentValue.BaseUrl}{uri}", CreateJsonContent(body))
        );
        var content = await response.Content.ReadAsStringAsync();

        return jsonTypeInfo is null
            ? System.Text.Json.JsonSerializer.Deserialize<T>(content)
            : System.Text.Json.JsonSerializer.Deserialize(content, jsonTypeInfo);
    }

    protected async Task<T?> PostCsvAsync<T>(
        Uri uri,
        string csv,
        System.Text.Json.Serialization.Metadata.JsonTypeInfo<T>? jsonTypeInfo = null
    )
    {
        var content = new StringContent(csv, System.Text.Encoding.UTF8, "text/csv");
        var response = await ExecuteRequestAsync(http =>
            http.PostAsync($"{_configuration.CurrentValue.BaseUrl}{uri}", content)
        );
        var responseContent = await response.Content.ReadAsStringAsync();

        return jsonTypeInfo is null
            ? System.Text.Json.JsonSerializer.Deserialize<T>(responseContent)
            : System.Text.Json.JsonSerializer.Deserialize(responseContent, jsonTypeInfo);
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

    protected string AgentId => _ixonAuthenticationContext.IxonHeaders.GetRequiredAgentId();

    protected virtual HttpMessageHandler? GetHttpMessageHandler() => null;

    private HttpClient CreateHttpClient()
    {
        var headers = _ixonAuthenticationContext.IxonHeaders;

        var httpClient = GetHttpMessageHandler() is { } handler
            ? new HttpClient(handler)
            : new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                headers.ServiceAccount.AccessToken
            );
        httpClient.DefaultRequestHeaders.Add("Api-Application", headers.ServiceAccount.ApiApplicationId);
        httpClient.DefaultRequestHeaders.Add("Api-Version", _configuration.CurrentValue.Version.ToString(CultureInfo.InvariantCulture));

        if (!string.IsNullOrEmpty(headers.CompanyId))
        {
            httpClient.DefaultRequestHeaders.Add("Api-Company", headers.CompanyId);
        }
        
        return httpClient;
    }

    private static StringContent CreateJsonContent(object body)
    {
        var serialized = System.Text.Json.JsonSerializer.Serialize(body);
        return new StringContent(serialized, System.Text.Encoding.UTF8, "application/json");
    }
}
