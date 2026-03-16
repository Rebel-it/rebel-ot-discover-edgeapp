using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Opc.Ua;
using Opc.Ua.Configuration;
using Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Factory;
using Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Telemetry;

namespace Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOPCUAClient(
        this IServiceCollection services,
        string applicationName
    )
    {
        var telemetry = new ConsoleTelemetry();
        var appInstance = new ApplicationInstance(telemetry)
        {
            ApplicationName = applicationName,
            ApplicationType = ApplicationType.Client,
            ConfigSectionName = applicationName,
            ApplicationConfiguration = new ApplicationConfiguration
            {
                ApplicationName = applicationName,
                ApplicationType = ApplicationType.Client,
                SecurityConfiguration = new SecurityConfiguration
                {
                    AutoAcceptUntrustedCertificates = true,
                    RejectSHA1SignedCertificates = false,
                    MinimumCertificateKeySize = 1024,
                },
                ClientConfiguration = new ClientConfiguration
                {
                    DefaultSessionTimeout = 60_000,
                    MinSubscriptionLifetime = 60_000,
                },
                TransportConfigurations = [],
            },
        };

        telemetry.ConfigureLogging(
            appInstance.ApplicationConfiguration,
            applicationName,
            true,
            false,
            true,
            LogLevel.Debug
        );

        services.AddSingleton(_ => appInstance);
        services.AddSingleton<ITelemetryContext>(_ => telemetry);
        services.AddSingleton<IUAClientFactory, UAClientFactory>();
        services.AddSingleton<IClientSamplerFactory, ClientSamplerFactory>();
        return services;
    }
}
