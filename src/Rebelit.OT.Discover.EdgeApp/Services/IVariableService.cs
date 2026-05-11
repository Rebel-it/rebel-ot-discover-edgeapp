using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

namespace Rebelit.OT.Discover.EdgeApp.Services;

public interface IVariableService
{
    Task<IReadOnlyList<Variable>> GetVariablesAsync(CancellationToken cancellationToken = default);
    Task<Variable?> CreateVariableAsync(Variable variable, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Variable>> CreateVariablesAsync(IEnumerable<Variable> variables, CancellationToken cancellationToken = default);
}
