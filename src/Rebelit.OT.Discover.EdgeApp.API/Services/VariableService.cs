using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Agents;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;
using Rebelit.OT.Discover.EdgeApp.SharedKernel.IxonAuthentication;

namespace Rebelit.OT.Discover.EdgeApp.API.Services;

internal sealed class VariableService(
    IApiClient apiClient,
    IConfiguration configuration,
    IIxonAuthenticationContext ixonAuthenticationContext,
    ILogger<VariableService> logger) : IVariableService
{

    public async Task<IReadOnlyList<Variable>> GetVariablesAsync(CancellationToken cancellationToken = default)
    {
        var response = await apiClient.GetDataVariablesAsync();
        var variables = response.Data ?? [];

        logger.LogInformation("Retrieved {Count} variables for agent {AgentId}.", variables.Length, ixonAuthenticationContext.IxonHeaders.AgentId);
        return variables;
    }

    public async Task<Variable?> CreateVariableAsync(Variable variable, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(variable);

        if (string.IsNullOrWhiteSpace(variable.Name))
            throw new ArgumentException("Variable name is required.", nameof(variable));

        if (string.IsNullOrWhiteSpace(variable.Address))
            throw new ArgumentException("Variable address is required.", nameof(variable));

        if (string.IsNullOrWhiteSpace(variable.Type))
            throw new ArgumentException("Variable type is required.", nameof(variable));

        if (variable.Source is null || string.IsNullOrWhiteSpace(variable.Source.PublicId))
            throw new ArgumentException("Variable source public id is required.", nameof(variable));

        var response = await apiClient.PostVariableAsync(variable);
        var createdVariable = response?.Data;

        logger.LogInformation(
            "Created variable {VariableName} for agent {AgentId}.",
            createdVariable?.Name ?? variable.Name,
            ixonAuthenticationContext.IxonHeaders.AgentId);

        return createdVariable;
    }

    public async Task<IReadOnlyList<Variable>> CreateVariablesAsync(IEnumerable<Variable> variables, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(variables);

        var variableList = variables.ToList();
        if (variableList.Count == 0)
            return [];

        var response = await apiClient.PostVariablesAsync(variableList);
        var createdVariables = response?.Data ?? [];

        logger.LogInformation("Created {Count} variables for agent {AgentId}.", createdVariables.Length, ixonAuthenticationContext.IxonHeaders.AgentId);
        return createdVariables;
    }
}
