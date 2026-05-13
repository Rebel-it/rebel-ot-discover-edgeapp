using Rebelit.OT.Discover.EdgeApp.SharedKernel;

namespace Rebelit.OT.Discover.EdgeApp.Connections.SecureEdgePro;

public interface IApiClient
{
    /// <summary>
    ///     Get the system info of the IXON device that this software is running on.
    /// </summary>
    /// <returns></returns>
    Task<Result<SecureEdgeSystemInfo>> GetSystemInfoAsync();
}