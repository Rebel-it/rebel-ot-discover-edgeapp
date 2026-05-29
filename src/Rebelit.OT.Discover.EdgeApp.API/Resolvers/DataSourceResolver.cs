using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Agents;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;
using Rebelit.OT.Discover.EdgeApp.SharedKernel.IxonAuthentication;

namespace Rebelit.OT.Discover.EdgeApp.API.Resolvers;

public interface IDataSourceResolver
{
    /// <summary>
    /// Asynchronously resolves the unique identifier associated with the specified agent and source.
    /// </summary>
    /// <param name="sourceName">The name of the source context in which to resolve the agent. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the resolved unique identifier as a
    /// string</returns>
    Task<string> ResolveAsync( string sourceName);
}

internal sealed class DataSourceResolver(
    IApiClient apiClient,
    IIxonAuthenticationContext authenticationContext,
    ILogger<DataSourceResolver> logger
) : IDataSourceResolver
{
    private const int OpcUaDefaultPort = 4840;
  
    public async Task<string> ResolveAsync( string sourceName)
    {
        if (string.IsNullOrWhiteSpace(sourceName))
            sourceName = "OPC UA";

        logger.LogInformation("Creating a new OPC-UA data source...");

        var devicesResponse = await apiClient.GetDevicesAsync();
        var devices = devicesResponse.Data ?? [];
        var device = FindDeviceByHost(devices);

        if (device?.PublicId is null)
            throw new InvalidOperationException(
                $"Could not resolve device publicId for agent '{authenticationContext.IxonHeaders.AgentId}'."
            );

        logger.LogInformation(
            "Using device '{DeviceName}' ({DevicePublicId}) with IP '{IpAddress}'.",
            device.Name,
            device.PublicId,
            device.IpAddress
        );

        var existingDataSource = await FindExistingDataSourceAsync(device.PublicId, sourceName);
        if (existingDataSource is not null)
        {
            logger.LogInformation(
                "Found existing data source '{Name}' ({PublicId}) for device '{DeviceName}'. Reusing it.",
                existingDataSource.Name,
                existingDataSource.PublicId,
                device.Name
            );
            return existingDataSource.PublicId!;
        }

        var newDataSource = BuildDataSource(device.PublicId, sourceName);
        var result = await apiClient.PostDataSourceAsync(newDataSource);

        var createdId =
            result?.Data.PublicId
            ?? throw new InvalidOperationException("Failed to create a new data source in IXON.");

        logger.LogInformation("Created data source with ID '{DataSourceId}'.", createdId);
        return createdId;
    }

    private async Task<DataSource?> FindExistingDataSourceAsync(
        string devicePublicId,
        string sourceName
    )
    {
        var dataSourcesResponse = await apiClient.GetDataSourcesAsync();
        var dataSources = dataSourcesResponse.Data ?? [];
        return dataSources.FirstOrDefault(ds =>
            ds.Device?.PublicId == devicePublicId
            && ds.Name == sourceName
        );
    }

    private DataSource BuildDataSource(string devicePublicId, string sourceName)
    {
        var authenticationType = string.IsNullOrEmpty(authenticationContext.IxonHeaders.PlcUsername) ? "anonymous" : "username";
        return new DataSource
        {
            Name = sourceName,
            Slug = SlugResolver.Resolve(sourceName),
            Disabled = false,
            Device = new Source { PublicId = devicePublicId },
            Protocol = new DataSourceProtocol
            {
                PublicId = "opc-ua",
                AuthenticationType = authenticationType,
                Username = string.IsNullOrEmpty(authenticationContext.IxonHeaders.PlcUsername) ? null : authenticationContext.IxonHeaders.PlcUsername,
                Password = string.IsNullOrEmpty(authenticationContext.IxonHeaders.PlcPassword) ? null : authenticationContext.IxonHeaders.PlcPassword,
            },
            Port = OpcUaDefaultPort
        };
    }

    private Device? FindDeviceByHost(Device[] devices)
    {
        var host = ExtractHost(authenticationContext.IxonHeaders.PlcUrl);
        if (string.IsNullOrEmpty(host))
            return devices.FirstOrDefault();

        var matched = devices.FirstOrDefault(d =>
            string.Equals(d.IpAddress, host, StringComparison.OrdinalIgnoreCase)
        );

        if (matched is null)
            logger.LogWarning(
                "No device found with IP address '{Host}'. Falling back to first device.",
                host
            );

        return matched ?? devices.FirstOrDefault();
    }

    private static string? ExtractHost(string opcuaAddress) =>
        Uri.TryCreate(opcuaAddress, UriKind.Absolute, out var uri) ? uri.Host : null;
}
