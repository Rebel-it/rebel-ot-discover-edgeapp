using Microsoft.Extensions.Logging;
using Rebelit.OT.Discover.EdgeApp.API.Resolvers;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;
using Rebelit.OT.Discover.EdgeApp.SharedKernel.IxonAuthentication;
using Rebelit.OT.Discover.EdgeApp.Tests.Services.Helpers;

namespace Rebelit.OT.Discover.EdgeApp.Tests.Resolvers;

[TestFixture]
public class DataSourceResolverTests
{
    [Test]
    public async Task ResolveAsync_WhenExistingDataSourceMatches_ReturnsExistingIdWithoutPosting()
    {
        var apiClient = new ApiClientSpy
        {
            Devices = [new Device { PublicId = "device-1", Name = "Device1", IpAddress = "10.0.0.10" }],
            ExistingDataSources = [new DataSource { PublicId = "ds-1", Name = "OPC UA", Device = new Source { PublicId = "device-1" }, Slug = "opc-ua", Protocol = new DataSourceProtocol { PublicId = "opc-ua", AuthenticationType = "username" } }],
        };
        var authContext = CreateAuthenticationContext("opc.tcp://10.0.0.10:4840", "user", "pass");
        var logger = new TestLogger<DataSourceResolver>(true);
        var sut = new DataSourceResolver(apiClient, authContext, logger);

        var result = await sut.ResolveAsync("OPC UA");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo("ds-1"));
            Assert.That(apiClient.PostDataSourceCallCount, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task ResolveAsync_WhenNoExistingDataSource_CreatesNewDataSourceWithUsernameAuth()
    {
        var apiClient = new ApiClientSpy
        {
            Devices = [new Device { PublicId = "device-1", Name = "Device1", IpAddress = "10.0.0.10" }],
            ExistingDataSources = [],
            CreatedDataSourceId = "new-ds-1",
        };
        var authContext = CreateAuthenticationContext("opc.tcp://10.0.0.10:6000", "user1", "pass1");
        var logger = new TestLogger<DataSourceResolver>(true);
        var sut = new DataSourceResolver(apiClient, authContext, logger);

        var result = await sut.ResolveAsync(string.Empty);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo("new-ds-1"));
            Assert.That(apiClient.PostedDataSource, Is.Not.Null);
            Assert.That(apiClient.PostedDataSource!.Name, Is.EqualTo("OPC UA"));
            Assert.That(apiClient.PostedDataSource.Slug, Is.EqualTo("opc-ua"));
            Assert.That(apiClient.PostedDataSource.Port, Is.EqualTo(6000));
            Assert.That(apiClient.PostedDataSource.Protocol.AuthenticationType, Is.EqualTo("username"));
            Assert.That(apiClient.PostedDataSource.Protocol.Username, Is.EqualTo("user1"));
            Assert.That(apiClient.PostedDataSource.Protocol.Password, Is.EqualTo("pass1"));
        });
    }

    [Test]
    public async Task ResolveAsync_WhenUsernameMissing_CreatesAnonymousDataSourceWithDefaultPort()
    {
        var apiClient = new ApiClientSpy
        {
            Devices = [new Device { PublicId = "device-1", Name = "Device1", IpAddress = "10.0.0.10" }],
            ExistingDataSources = [],
            CreatedDataSourceId = "new-ds-1",
        };
        var authContext = CreateAuthenticationContext("opc.tcp://10.0.0.10", string.Empty, string.Empty);
        var logger = new TestLogger<DataSourceResolver>(true);
        var sut = new DataSourceResolver(apiClient, authContext, logger);

        await sut.ResolveAsync("My Source");

        Assert.Multiple(() =>
        {
            Assert.That(apiClient.PostedDataSource, Is.Not.Null);
            Assert.That(apiClient.PostedDataSource!.Port, Is.EqualTo(4840));
            Assert.That(apiClient.PostedDataSource.Protocol.AuthenticationType, Is.EqualTo("anonymous"));
            Assert.That(apiClient.PostedDataSource.Protocol.Username, Is.Null);
            Assert.That(apiClient.PostedDataSource.Protocol.Password, Is.Null);
            Assert.That(apiClient.PostedDataSource.Slug, Is.EqualTo("my-source"));
        });
    }

    [Test]
    public void ResolveAsync_WhenNoDeviceIsResolved_ThrowsInvalidOperationException()
    {
        var apiClient = new ApiClientSpy { Devices = [] };
        var authContext = CreateAuthenticationContext("opc.tcp://10.0.0.10:4840", "user", "pass", "agent-42");
        var logger = new TestLogger<DataSourceResolver>(true);
        var sut = new DataSourceResolver(apiClient, authContext, logger);

        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.ResolveAsync("OPC UA"));

        Assert.That(ex!.Message, Is.EqualTo("Could not resolve device publicId for agent 'agent-42'."));
    }

    [Test]
    public async Task ResolveAsync_WhenHostDoesNotMatch_FallsBackToFirstDeviceAndLogsWarning()
    {
        var apiClient = new ApiClientSpy
        {
            Devices =
            [
                new Device { PublicId = "first", Name = "Device1", IpAddress = "10.0.0.1" },
                new Device { PublicId = "second", Name = "Device2", IpAddress = "10.0.0.2" },
            ],
            ExistingDataSources = [],
            CreatedDataSourceId = "new-ds-1",
        };
        var authContext = CreateAuthenticationContext("opc.tcp://192.168.1.200:4840", "user", "pass");
        var logger = new TestLogger<DataSourceResolver>(true);
        var sut = new DataSourceResolver(apiClient, authContext, logger);

        await sut.ResolveAsync("OPC UA");

        Assert.Multiple(() =>
        {
            Assert.That(apiClient.PostedDataSource!.Device.PublicId, Is.EqualTo("first"));
            Assert.That(logger.Entries.Any(e => e.LogLevel == LogLevel.Warning && e.Message.Contains("Falling back to first device")), Is.True);
        });
    }

    [Test]
    public void ResolveAsync_WhenCreateReturnsNoPublicId_ThrowsInvalidOperationException()
    {
        var apiClient = new ApiClientSpy
        {
            Devices = [new Device { PublicId = "device-1", Name = "Device1", IpAddress = "10.0.0.10" }],
            ExistingDataSources = [],
            CreatedDataSourceId = null,
        };
        var authContext = CreateAuthenticationContext("opc.tcp://10.0.0.10:4840", "user", "pass");
        var logger = new TestLogger<DataSourceResolver>(true);
        var sut = new DataSourceResolver(apiClient, authContext, logger);

        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.ResolveAsync("OPC UA"));

        Assert.That(ex!.Message, Is.EqualTo("Failed to create a new data source in IXON."));
    }

    private static IxonAuthenticationContext CreateAuthenticationContext(
        string plcUrl,
        string username,
        string password,
        string agentId = "agent-1") =>
        new()
        {
            IxonHeaders = new IxonHeaders
            {
                ServiceAccount = new ServiceAccount
                {
                    AccessToken = "token",
                    ApiApplicationId = "app-id",
                },
                AgentId = agentId,
                PlcUrl = plcUrl,
                PlcUsername = username,
                PlcPassword = password,
            },
        };
}
