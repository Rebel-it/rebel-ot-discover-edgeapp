using Microsoft.Extensions.DependencyInjection;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Agents;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

namespace Rebelit.OT.Discover.EdgeApp.Connections.IXON;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIXONClient(
        this IServiceCollection services,
        string applicationId,
        string companyId,
        string bearerToken
    )
    {
        services
            .AddOptions<Configuration>()
            .Configure(options =>
            {
                options.ApplicationId = applicationId;
                options.CompanyId = companyId;
                options.BearerToken = bearerToken;
            })
            .ValidateOnStart();
        services.AddSingleton<IApiClient, ApiClient>();
        services.AddSingleton(TimeProvider.System);
        return services;
    }
}
