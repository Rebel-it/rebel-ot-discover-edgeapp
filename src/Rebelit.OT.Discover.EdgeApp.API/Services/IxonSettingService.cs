using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Agents;
using Rebelit.OT.Discover.EdgeApp.SharedKernel.IxonAuthentication;

namespace Rebelit.OT.Discover.EdgeApp.API.Services;

public class IxonSettingService(
    IApiClient apiClient,
    IIxonAuthenticationContext authenticationContext,
    Connections.SecureEdgePro.IApiClient secureEdgeApiClient) : IIxonSettingService
{
    public async Task<string?> PushDeviceConfig()
    {
        var response = await apiClient.PushConfigurationAsync(authenticationContext.IxonHeaders.AgentId);
        return response?.Status;
    }
}