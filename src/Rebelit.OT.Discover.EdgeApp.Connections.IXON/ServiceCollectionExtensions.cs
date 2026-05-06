using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Agents;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

namespace Rebelit.OT.Discover.EdgeApp.Connections.IXON;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIXONClient(
        this IServiceCollection services
    )
    {
        services.AddSingleton<IOptionsChangeTokenSource<Configuration>>(sp =>
            new ConfigurationChangeTokenSource<Configuration>(
                string.Empty,
                sp.GetRequiredService<IConfiguration>()
            ));

        services.AddScoped<IApiClient, ApiClient>();
        services.AddSingleton(TimeProvider.System);
        return services;
    }
}
