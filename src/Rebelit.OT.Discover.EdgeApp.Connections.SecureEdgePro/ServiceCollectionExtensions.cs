using Microsoft.Extensions.DependencyInjection;

namespace Rebelit.OT.Discover.EdgeApp.Connections.SecureEdgePro;

public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Registers the SecureEdgePro API client with the given base address.
    /// </summary>
    public static IServiceCollection AddSecureEdgeProClient(this IServiceCollection services, string baseAddress)
    {
        services
            .AddHttpClient(ApiClient.HttpClientName)
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri(baseAddress);
                client.Timeout = TimeSpan.FromSeconds(5);
            });

        services.AddTransient<IApiClient, ApiClient>();

        return services;
    }
}
