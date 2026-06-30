using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Agents;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;
using Rebelit.OT.Discover.EdgeApp.API.Utilities;
using Rebelit.OT.Discover.EdgeApp.SharedKernel;
using Rebelit.OT.Discover.EdgeApp.SharedKernel.IxonAuthentication;

namespace Rebelit.OT.Discover.EdgeApp.API.Resolvers;

internal sealed class DataSourceResolver(
    IApiClient apiClient,
    IIxonAuthenticationContext authenticationContext,
    ILogger<DataSourceResolver> logger
) : IDataSourceResolver
{
    private const int OpcUaDefaultPort = 4840;

    public async Task<Result<string>> ResolveAsync(string sourceName)
    {
        if (string.IsNullOrWhiteSpace(sourceName))
        {
            sourceName = "OPC UA";
        }
        Device device;
        try
        {
            device = await GetDeviceAsync();
        }
        catch (InvalidOperationException ex)
        {
            return new Result<string> { ErrorMessage = ex.Message };
        }

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Using device '{DeviceName}' ({DevicePublicId}) with IP '{IpAddress}'.",
                device.Name,
                device.PublicId,
                device.IpAddress
            );
        }

        var existingDataSourceId = await TryGetExistingDataSourceIdAsync(device.PublicId, sourceName);
        if (existingDataSourceId is not null)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Found existing data source '{PublicId}' for device '{DeviceName}'. Reusing it.",
                    existingDataSourceId,
                    device.Name
                );
            }

            return new Result<string> { Data = existingDataSourceId };
        }


        var newDataSource = BuildDataSource(device.PublicId, sourceName);
        var result = await apiClient.PostDataSourceAsync(newDataSource);

        if (result?.Data?.PublicId == null || result.HasError)
        {
            return new Result<string>
            {
                ErrorMessage = result?.ErrorMessage != null ?
                    $"Failed to create a new data source in IXON. {result.ErrorMessage}"
                    : "Failed to create a new data source in IXON."
            };
        }

        var createdId = result.Data.PublicId;

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Created data source with ID '{DataSourceId}'.", createdId);
        }

        return new Result<string> { Data = createdId };
    }


    private async Task<Device> GetDeviceAsync()
    {
        var devicesResponse = await apiClient.GetDevicesAsync();
        var devices = devicesResponse.Data ?? [];
        var device = FindDeviceByHost(devices);

        if (device?.PublicId is null)
        {
            throw new InvalidOperationException(
                $"Could not resolve device publicId for agent '{authenticationContext.IxonHeaders.AgentId}'."
            );
        }

        return device;
    }

    private async Task<string?> TryGetExistingDataSourceIdAsync(
        string devicePublicId,
        string sourceName
    )
    {
        var dataSourcesResponse = await apiClient.GetDataSourcesAsync();
        var dataSources = dataSourcesResponse.Data ?? [];
        var existingDataSource = dataSources.FirstOrDefault(ds =>
            ds.Device?.PublicId == devicePublicId
            && ds.Name == sourceName
        );

        return existingDataSource?.PublicId;
    }

    private DataSource BuildDataSource(string devicePublicId, string sourceName)
    {
        var authenticationType = string.IsNullOrEmpty(authenticationContext.IxonHeaders.PlcUsername) ? "anonymous" : "username";
        var port = PortExtractor.ExtractPort(authenticationContext.IxonHeaders.PlcUrl) ?? OpcUaDefaultPort;

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
            Port = port
        };
    }

    private Device? FindDeviceByHost(Device[] devices)
    {
        var host = ExtractHost(authenticationContext.IxonHeaders.PlcUrl);
        if (string.IsNullOrEmpty(host))
        {
            return devices.FirstOrDefault();
        }

        var matched = devices.FirstOrDefault(d =>
            string.Equals(d.IpAddress, host, StringComparison.OrdinalIgnoreCase)
        );

        if (matched is null && logger.IsEnabled(LogLevel.Warning))
        {
            logger.LogWarning(
                "No device found with IP address '{Host}'. Falling back to first device.",
                host
            );
        }

        return matched ?? devices.FirstOrDefault();
    }

    private static string? ExtractHost(string? opcuaAddress) =>
        Uri.TryCreate(opcuaAddress, UriKind.Absolute, out var uri) ? uri.Host : null;
}
