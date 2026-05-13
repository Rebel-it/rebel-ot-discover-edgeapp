using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Agents;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

namespace Rebelit.OT.Discover.EdgeApp.Services;

internal sealed class VariableService(
    IApiClient apiClient,
    IConfiguration configuration,
    ILogger<VariableService> logger) : IVariableService
{

    private readonly string _agentId =
        configuration["IXON_AgentId"]
        ?? throw new InvalidOperationException("IXON_AgentId configuration is not set.");

    public async Task<IReadOnlyList<Variable>> GetVariablesAsync(CancellationToken cancellationToken = default)
    {
        var response = await apiClient.GetDataVariablesAsync();
        var variables = response.Data ?? [];

        logger.LogInformation("Retrieved {Count} variables for agent {AgentId}.", variables.Length, _agentId);
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
            _agentId);

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

        logger.LogInformation("Created {Count} variables for agent {AgentId}.", createdVariables.Length, _agentId);
        return createdVariables;
    }
}
