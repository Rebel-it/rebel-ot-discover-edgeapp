using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Agents;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;
using Rebelit.OT.Discover.EdgeApp.API.Resolvers;
using System.Text.Json;

namespace Rebelit.OT.Discover.EdgeApp.Tests;

[TestFixture]
public class DataSourceResolverTests
{
    private const string DefaultAgentId = "test-agent";
    private const string DefaultAddress = "opc.tcp://localhost:4840";
    private const string DefaultUsername = "user";
    private const string DefaultPassword = "pass";

    private static IConfiguration BuildConfig(
        string? dataSourceId = null,
        string address = DefaultAddress
    )
    {
        var dict = new Dictionary<string, string?>
        {
            ["OPCUA_ServerAddress"] = address,
            ["OPCUA_Username"] = DefaultUsername,
            ["OPCUA_Password"] = DefaultPassword,
        };
        if (dataSourceId is not null)
            dict["IXON_DataSourceId"] = dataSourceId;
        return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
    }

    [Test]
    public async Task ResolveAsync_WhenDataSourceIdIsConfigured_ReturnsItWithoutCallingApi()
    {
        var apiClient = new SpyApiClient([]);
        var resolver = new DataSourceResolver(
            apiClient,
            BuildConfig(dataSourceId: "pre-configured-id"),
            NullLogger<DataSourceResolver>.Instance
        );

        var result = await resolver.ResolveAsync(DefaultAgentId, "");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo("pre-configured-id"));
            Assert.That(apiClient.GetDevicesCallCount, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task ResolveAsync_WhenNoDataSourceIdConfigured_CreatesNewDataSourceAndReturnsItsId()
    {
        var device = new Device { PublicId = "device-1", IpAddress = "127.0.0.1" };
        var apiClient = new SpyApiClient([device], newDataSourceId: "new-ds-id");
        var resolver = new DataSourceResolver(
            apiClient,
            BuildConfig(),
            NullLogger<DataSourceResolver>.Instance
        );

        var result = await resolver.ResolveAsync(DefaultAgentId, "");

        Assert.That(result, Is.EqualTo("new-ds-id"));
    }

    [Test]
    public async Task ResolveAsync_WhenExistingDataSourceMatchesDevice_ReturnsExistingIdWithoutPosting()
    {
        var device = new Device { PublicId = "device-1", IpAddress = "127.0.0.1" };
        var existingDataSource = new DataSource
        {
            PublicId = "existing-ds-id",
            Name = "OPC UA",
            Slug = "opcua",
            Device = new Source { PublicId = "device-1" },
        };
        var apiClient = new SpyApiClient(
            [device],
            newDataSourceId: "new-ds-id",
            existingDataSources: [existingDataSource]
        );
        var resolver = new DataSourceResolver(
            apiClient,
            BuildConfig(),
            NullLogger<DataSourceResolver>.Instance
        );

        var result = await resolver.ResolveAsync(DefaultAgentId, "");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo("new-ds-id"));
        });
    }

    [Test]
    public async Task ResolveAsync_WhenExistingDataSourceDoesNotMatchDevice_CreatesNewDataSource()
    {
        var device = new Device { PublicId = "device-1", IpAddress = "127.0.0.1" };
        var nonMatchingDataSource = new DataSource
        {
            PublicId = "other-ds-id",
            Name = "OPC UA",
            Slug = "opcua",
            Device = new Source { PublicId = "device-other" },
        };
        var apiClient = new SpyApiClient(
            [device],
            newDataSourceId: "new-ds-id",
            existingDataSources: [nonMatchingDataSource]
        );
        var resolver = new DataSourceResolver(
            apiClient,
            BuildConfig(),
            NullLogger<DataSourceResolver>.Instance
        );

        var result = await resolver.ResolveAsync(DefaultAgentId, "");

        Assert.That(result, Is.EqualTo("new-ds-id"));
    }

    [Test]
    public async Task ResolveAsync_WhenDeviceIpMatchesOpcuaHost_UsesMatchedDevice()
    {
        Device[] devices =
        [
            new() { PublicId = "dev-1", IpAddress = "10.0.0.1" },
            new() { PublicId = "dev-2", IpAddress = "192.168.1.100" },
        ];
        var apiClient = new SpyApiClient(devices, "new-ds-id");
        var resolver = new DataSourceResolver(
            apiClient,
            BuildConfig(address: "opc.tcp://192.168.1.100:4840"),
            NullLogger<DataSourceResolver>.Instance
        );

        await resolver.ResolveAsync(DefaultAgentId, "");

        Assert.That(apiClient.PostedDataSource?.Device?.PublicId, Is.EqualTo("dev-2"));
    }

    [Test]
    public async Task ResolveAsync_WhenNoDeviceIpMatches_FallsBackToFirstDevice()
    {
        Device[] devices =
        [
            new() { PublicId = "dev-first", IpAddress = "10.0.0.1" },
            new() { PublicId = "dev-second", IpAddress = "10.0.0.2" },
        ];
        var apiClient = new SpyApiClient(devices, "new-ds-id");
        var resolver = new DataSourceResolver(
            apiClient,
            BuildConfig(address: "opc.tcp://192.168.1.99:4840"),
            NullLogger<DataSourceResolver>.Instance
        );

        await resolver.ResolveAsync(DefaultAgentId, "");

        Assert.That(apiClient.PostedDataSource?.Device?.PublicId, Is.EqualTo("dev-first"));
    }

    private sealed class SpyApiClient(
        Device[] devices,
        string? newDataSourceId = null,
        DataSource[]? existingDataSources = null
    ) : IApiClient
    {
        public int GetDevicesCallCount { get; private set; }
        public DataSource? PostedDataSource { get; private set; }

        public Task<Response<Device[]>> GetDevicesAsync(string agentId)
        {
            GetDevicesCallCount++;
            var response = new Response<Device[]>
            {
                RawData = JsonSerializer.SerializeToElement(devices),
            };
            return Task.FromResult(response);
        }

        public Task<Response<DataSource>?> PostDataSourceAsync(string agentId, DataSource newDataSource)
        {
            PostedDataSource = newDataSource;
            var response = new Response<DataSource>
            {
                RawData = JsonSerializer.SerializeToElement(new DataSource { PublicId = newDataSourceId }),
            };
            return Task.FromResult<Response<DataSource>?>(response);
        }

        public Task<Response<DataSource[]>> GetDataSourcesAsync(string agentId)
        {
            var sources = existingDataSources ?? [];
            return Task.FromResult(new Response<DataSource[]>
            {
                RawData = JsonSerializer.SerializeToElement(sources),
            });
        }

        public Task<Response<Variable[]>> GetDataVariablesAsync(string agentId) =>
            throw new NotSupportedException();

        public Task<Response<Tag[]>> GetTagsAsync(string agentId) =>
            throw new NotSupportedException();

        public Task<Response<Variable>?> PostVariableAsync(string agentId, Variable newVariable) =>
            throw new NotSupportedException();

        public Task<Response<Tag>?> PostTagAsync(string agentId, Tag newTag) =>
            throw new NotSupportedException();

        public Task<Response<Variable[]>?> PostVariablesAsync(string agentId, IEnumerable<Variable> variables) =>
            throw new NotSupportedException();

        public Task<Response<Agent>> GetAgentAsync(string agentId) =>
            throw new NotSupportedException();

        public Task<Response<Company[]>> GetAssociatedCompanyAsync() =>
            throw new NotSupportedException();

        public Task<Response<Agent[]>> GetAgentsAsync() =>
            throw new NotSupportedException();
    }
}
