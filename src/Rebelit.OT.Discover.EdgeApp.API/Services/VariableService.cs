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
        var sourceId = ixonAuthenticationContext.IxonHeaders.SourceId;
        var response = await apiClient.GetDataVariablesAsync();
        var variables = (response.Data ?? []).Where(x => x.Source?.PublicId == sourceId).ToList();
        if(logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Retrieved {Count} variables for agent {AgentId}.", variables.Count, ixonAuthenticationContext.IxonHeaders.AgentId);
        }
        return variables;
    }
}
