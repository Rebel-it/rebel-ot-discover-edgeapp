using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Agents;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Authentication;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

namespace Rebelit.OT.Discover.EdgeApp.Connections.IXON;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIXONClient(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddOptions<Configuration>()
            .Configure<IConfiguration>((options, config) =>
            {
                options.ApplicationId = config["IXON_ApplicationId"] ?? string.Empty;
                options.CompanyId = config["IXON_CompanyId"] ?? string.Empty;
                options.BearerToken = config["IXON_BearerToken"] ?? string.Empty;
                options.Email = config["OPCUA_Username"] ?? string.Empty;
                options.Password = config["OPCUA_Password"] ?? string.Empty;
                options.OtpCode = config["IXON_OtpCode"];
            });

        services.AddSingleton<IOptionsChangeTokenSource<Configuration>>(sp =>
            new ConfigurationChangeTokenSource<Configuration>(
                string.Empty,
                sp.GetRequiredService<IConfiguration>()
            ));

        services.AddSingleton<IxonAuthentication>();
        services.AddSingleton<IApiClient, ApiClient>();
        services.AddSingleton(TimeProvider.System);
        return services;
    }
}
