using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Agents;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

namespace Rebelit.OT.Discover.EdgeApp.Resolvers;

public interface IDataSourceResolver
{
    /// <summary>
    /// Asynchronously resolves the unique identifier associated with the specified agent and source.
    /// </summary>
    /// <param name="agentId">The identifier of the agent for which to resolve the unique value. Cannot be null or empty.</param>
    /// <param name="sourceName">The name of the source context in which to resolve the agent. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the resolved unique identifier as a
    /// string</returns>
    Task<string> ResolveAsync(string agentId, string sourceName);
}

internal sealed class DataSourceResolver(
    IApiClient apiClient,
    IConfiguration configuration,
    ILogger<DataSourceResolver> logger
) : IDataSourceResolver
{
    private readonly string _opcuaAddress =
        configuration["OPCUA_ServerAddress"]
        ?? throw new InvalidOperationException("OPCUA_ServerAddress configuration is not set.");

    private readonly string _username =
        configuration["OPCUA_Username"]
        ?? throw new InvalidOperationException("OPCUA_Username configuration is not set.");

    private readonly string _password =
        configuration["OPCUA_Password"]
        ?? throw new InvalidOperationException("OPCUA_Password configuration is not set.");

    private readonly string? _configuredDataSourceId = configuration["IXON_DataSourceId"];

    public async Task<string> ResolveAsync(string agentId, string sourceName)
    {
        if (string.IsNullOrWhiteSpace(sourceName))
            sourceName = "OPC UA";

        if (!string.IsNullOrEmpty(_configuredDataSourceId))
        {
            return _configuredDataSourceId;
        }

        logger.LogInformation("No data source ID provided. Creating a new OPC-UA data source...");

        var devicesResponse = await apiClient.GetDevicesAsync(agentId);
        var devices = devicesResponse.Data ?? [];
        var device = FindDeviceByHost(devices);

        if (device?.PublicId is null)
            throw new InvalidOperationException(
                $"Could not resolve device publicId for agent '{agentId}'."
            );

        logger.LogInformation(
            "Using device '{DeviceName}' ({DevicePublicId}) with IP '{IpAddress}'.",
            device.Name,
            device.PublicId,
            device.IpAddress
        );

        var existingDataSource = await FindExistingDataSourceAsync(agentId, device.PublicId, sourceName);
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
        var result = await apiClient.PostDataSourceAsync(agentId, newDataSource);
        var createdId =
            result?.Data.PublicId
            ?? throw new InvalidOperationException("Failed to create a new data source in IXON.");

        logger.LogInformation("Created data source with ID '{DataSourceId}'.", createdId);
        return createdId;
    }

    private async Task<DataSource?> FindExistingDataSourceAsync(
        string agentId,
        string devicePublicId,
        string sourceName
    )
    {
        var dataSourcesResponse = await apiClient.GetDataSourcesAsync(agentId);
        var dataSources = dataSourcesResponse.Data ?? [];
        return dataSources.FirstOrDefault(ds =>
            ds.Device?.PublicId == devicePublicId
            && string.Equals(ds.Slug, sourceName, StringComparison.OrdinalIgnoreCase)
        );
    }

    private DataSource BuildDataSource(string devicePublicId, string sourceName)
    {
        var authenticationType = string.IsNullOrEmpty(_username) ? "anonymous" : "username";
        var slug = SlugResolver.Resolve(sourceName);
        return new DataSource
        {
            Name = sourceName,
            Slug = slug,
            Disabled = false,
            Device = new Source { PublicId = devicePublicId },
            Protocol = new DataSourceProtocol
            {
                PublicId = slug,
                AuthenticationType = authenticationType,
                Username = string.IsNullOrEmpty(_username) ? null : _username,
                Password = string.IsNullOrEmpty(_password) ? null : _password,
            },
        };
    }

    private Device? FindDeviceByHost(Device[] devices)
    {
        var host = ExtractHost(_opcuaAddress);
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
