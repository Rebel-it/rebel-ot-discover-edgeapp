using Rebelit.OT.Discover.EdgeApp.Connections.SecureEdgePro;
using Rebelit.OT.Discover.EdgeApp.SharedKernel;

namespace Rebelit.OT.Discover.EdgeApp.Tests.Services.Helpers;

public sealed class SecureEdgeApiClientSpy : IApiClient
{
    public Result<SecureEdgeSystemInfo> SystemInfoResult { get; set; } = new();
    public int GetSystemInfoCallCount { get; private set; }

    public Task<Result<SecureEdgeSystemInfo>> GetSystemInfoAsync()
    {
        GetSystemInfoCallCount++;
        return Task.FromResult(SystemInfoResult);
    }
}
