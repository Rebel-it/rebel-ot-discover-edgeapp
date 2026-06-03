using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Agents;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;
using Rebelit.OT.Discover.EdgeApp.SharedKernel.IxonAuthentication;

namespace Rebelit.OT.Discover.EdgeApp.API.Services;

internal sealed class VariableService(
    IApiClient apiClient,
    IIxonAuthenticationContext ixonAuthenticationContext,
    ILogger<VariableService> logger) : IVariableService
{

    public async Task<IReadOnlyList<Variable>> GetVariablesAsync()
    {
        var response = await apiClient.GetDataVariablesAsync();
        var variables = response.Data ?? [];
        if(logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Retrieved {Count} variables for agent {AgentId}.", variables.Length, ixonAuthenticationContext.IxonHeaders.AgentId);
        }
        return variables;
    }
}
