using System.Text.Json;
using Microsoft.Extensions.Logging;
using Rebelit.OT.Discover.EdgeApp.SharedKernel;

namespace Rebelit.OT.Discover.EdgeApp.Connections.SecureEdgePro;

public class ApiClient(
    IHttpClientFactory httpClientFactory,
    ILogger<ApiClient> logger) : IApiClient
{
    public const string HttpClientName = "SecureEdgePro";

    public async Task<Result<SecureEdgeSystemInfo>> GetSystemInfoAsync()
    {
        var client = httpClientFactory.CreateClient(HttpClientName);

        try
        {
            using var response = await client.GetAsync("/api/v1/system/info");
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
            logger.LogError(ex, "Could not reach SecureEdge Pro");
            return new Result<SecureEdgeSystemInfo>
            {
                ErrorMessage = $"Could not reach SecureEdge Pro to retrieve system information."
            };
        }
    }
}