using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

namespace Rebelit.OT.Discover.EdgeApp.API.Services;

public interface IVariableService
{
    Task<IReadOnlyList<Variable>> GetVariablesAsync();
}
