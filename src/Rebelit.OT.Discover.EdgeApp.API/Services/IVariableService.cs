using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

namespace Rebelit.OT.Discover.EdgeApp.API.Services;

public interface IVariableService
{
    /// <summary>
    /// Returns a list variables
    /// </summary>
    Task<IReadOnlyList<Variable>> GetVariablesAsync();
}
