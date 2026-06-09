using System.Text.Json;
using Microsoft.Extensions.Logging;
using Rebelit.OT.Discover.EdgeApp.SharedKernel;

namespace Rebelit.OT.Discover.EdgeApp.Connections.SecureEdgePro;

public class ApiClient(
    IHttpClientFactory httpClientFactory,
    ILogger<ApiClient> logger) : IApiClient
{
    public static string HttpClientName
    {
        get { return "SecureEdgePro"; }
    }

    private readonly HttpClient _client = httpClientFactory.CreateClient(HttpClientName);

    public async Task<Result<SecureEdgeSystemInfo>> GetSystemInfoAsync()
    {
        try
        {
            using var response = await _client.GetAsync("/api/v1/system/info");
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new Result<SecureEdgeSystemInfo>
                {
                    ErrorMessage = content
                };
            }

            var systemInfo = JsonSerializer.Deserialize<SecureEdgeSystemInfo>(content);
            return new Result<SecureEdgeSystemInfo> { Data = systemInfo };
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError(ex, "Could not reach SecureEdge Pro");
            }
            return new Result<SecureEdgeSystemInfo>
            {
                ErrorMessage = "Could not reach SecureEdge Pro to retrieve system information. Are you connected with the VPN?"
            };
        }
    }
}