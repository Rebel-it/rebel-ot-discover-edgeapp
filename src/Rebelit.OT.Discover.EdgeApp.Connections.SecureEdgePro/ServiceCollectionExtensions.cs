using Microsoft.Extensions.DependencyInjection;

namespace Rebelit.OT.Discover.EdgeApp.Connections.SecureEdgePro;

public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Registers the SecureEdgePro API client with the given base address.
    /// </summary>
    public static IServiceCollection AddSecureEdgeProClient(this IServiceCollection services, string baseAddress)
    {
        services.AddHttpClient(ApiClient.HttpClientName, client =>
        {
            client.BaseAddress = new Uri(baseAddress);
        });

        services.AddTransient<IApiClient, ApiClient>();

        return services;
    }
}
