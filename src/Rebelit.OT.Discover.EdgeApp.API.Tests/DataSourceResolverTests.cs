using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Agents;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;
using Rebelit.OT.Discover.EdgeApp.API.Resolvers;
using System.Text.Json;
using Rebelit.OT.Discover.EdgeApp.SharedKernel.IxonAuthentication;

namespace Rebelit.OT.Discover.EdgeApp.Tests;

[TestFixture]
public class DataSourceResolverTests
{
    private const string DefaultAgentId = "test-agent";
    private const string DefaultAddress = "opc.tcp://localhost:4840";
    private const string DefaultUsername = "user";
    private const string DefaultPassword = "pass";

    private static FakeIxonAuthenticationContext CreateAuthenticationContext(
        string address = DefaultAddress,
        string username = DefaultUsername,
        string password = DefaultPassword,
        string agentId = DefaultAgentId
    ) =>
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
                PlcUrl = address,
                PlcUsername = username,
                PlcPassword = password,
            },
        };

    [Test]
    public async Task ResolveAsync_WhenDataSourceIdIsConfigured_ReturnsItWithoutCallingApi()
    {
        var apiClient = new SpyApiClient([]);
        var resolver = new DataSourceResolver(
            apiClient,
            CreateAuthenticationContext(),
            NullLogger<DataSourceResolver>.Instance
        );
        
        var result = await resolver.ResolveAsync("");

        Assert.Multiple(() =>
        {
            Assert.That(result.ErrorMessage, Is.EqualTo("Could not resolve device publicId for agent 'test-agent'."));
            Assert.That(apiClient.GetDevicesCallCount, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task ResolveAsync_WhenDataSourceIdIsConfigured_ReturnsCorrectErrorMessage_WhenNoDeviceIsResolved()
    {
        var apiClient = new SpyApiClient([]);
        var resolver = new DataSourceResolver(
            apiClient,
            CreateAuthenticationContext(),
            NullLogger<DataSourceResolver>.Instance
        );
        
        var result = await resolver.ResolveAsync("");

        Assert.Multiple(() =>
        {
            Assert.That(result.ErrorMessage, Is.EqualTo("Could not resolve device publicId for agent 'test-agent'."));
            Assert.That(apiClient.GetDevicesCallCount, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task ResolveAsync_WhenNoDataSourceIdConfigured_CreatesNewDataSourceAndReturnsItsId()
    {
        var device = new Device { PublicId = "device-1", IpAddress = "127.0.0.1" };
        var apiClient = new SpyApiClient([device], newDataSourceId: "new-ds-id");
        var resolver = new DataSourceResolver(
            apiClient,
            CreateAuthenticationContext(),
            NullLogger<DataSourceResolver>.Instance
        );

        var result = await resolver.ResolveAsync( "");

        Assert.That(result.Data, Is.EqualTo("new-ds-id"));
    }

    [Test]
    public async Task ResolveAsync_WhenNoDeviceIsResolved_ReturnsCorrectErrorMessage()
    {
        var apiClient = new SpyApiClient([]);
        var resolver = new DataSourceResolver(
            apiClient,
            CreateAuthenticationContext(),
            NullLogger<DataSourceResolver>.Instance
        );

        var result = await resolver.ResolveAsync("");

        Assert.Multiple(() =>
        {
            Assert.That(result.ErrorMessage, Is.EqualTo("Could not resolve device publicId for agent 'test-agent'."));
            Assert.That(apiClient.GetDevicesCallCount, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task ResolveAsync_WhenExistingDataSourceMatchesDeviceAndSourceName_ReturnsExistingIdWithoutPosting()
    {
        var device = new Device { PublicId = "device-1", IpAddress = "127.0.0.1" };
        var existingDataSource = new DataSource
        {
            PublicId = "existing-ds-id",
            Name = "OPC UA",
            Slug = "opc-ua",
            Device = new Source { PublicId = "device-1" },
        };
        var apiClient = new SpyApiClient(
            [device],
            newDataSourceId: "new-ds-id",
            existingDataSources: [existingDataSource]
        );
        var resolver = new DataSourceResolver(
            apiClient,
            CreateAuthenticationContext(),
            NullLogger<DataSourceResolver>.Instance
        );

        var result = await resolver.ResolveAsync("OPC UA");

        Assert.Multiple(() =>
        {
            Assert.That(result.Data, Is.EqualTo("existing-ds-id"));
            Assert.That(apiClient.PostedDataSource, Is.Null);
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
            Slug = "other-source",
            Device = new Source { PublicId = "device-other" },
        };
        var apiClient = new SpyApiClient(
            [device],
            newDataSourceId: "new-ds-id",
            existingDataSources: [nonMatchingDataSource]
        );
        var resolver = new DataSourceResolver(
            apiClient,
            CreateAuthenticationContext(),
            NullLogger<DataSourceResolver>.Instance
        );

        var result = await resolver.ResolveAsync( "");

        Assert.That(result.Data, Is.EqualTo("new-ds-id"));
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
            CreateAuthenticationContext(address: "opc.tcp://192.168.1.100:4840"),
            NullLogger<DataSourceResolver>.Instance
        );

        await resolver.ResolveAsync( "");

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
            CreateAuthenticationContext(address: "opc.tcp://192.168.1.99:4840"),
            NullLogger<DataSourceResolver>.Instance
        );

        await resolver.ResolveAsync( "");

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

        public Task<Response<Device[]>> GetDevicesAsync()
        {
            GetDevicesCallCount++;
            var response = new Response<Device[]>
            {
                RawData = JsonSerializer.SerializeToElement(devices),
            };
            return Task.FromResult(response);
        }

        public Task<Response<DataSource>?> PostDataSourceAsync(DataSource newDataSource)
        {
            PostedDataSource = newDataSource;
            var response = new Response<DataSource>
            {
                RawData = JsonSerializer.SerializeToElement(new DataSource { PublicId = newDataSourceId }),
            };
            return Task.FromResult<Response<DataSource>?>(response);
        }

        public Task<Response<DataSource[]>> GetDataSourcesAsync()
        {
            var sources = existingDataSources ?? [];
            return Task.FromResult(new Response<DataSource[]>
            {
                RawData = JsonSerializer.SerializeToElement(sources),
            });
        }

        public Task<Response<Variable[]>> GetDataVariablesAsync() =>
            throw new NotSupportedException();

        public Task<Response<Tag[]>> GetTagsAsync() =>
            throw new NotSupportedException();

        public Task<Response<Variable>?> PostVariableAsync(Variable newVariable) =>
            throw new NotSupportedException();

        public Task<Response<Tag>?> PostTagAsync(Tag newTag) =>
            throw new NotSupportedException();

        public Task<Response<Tag>?> UpdateTagAsync(string publicId, Tag tag) =>
            throw new NotSupportedException();

        public Task<Response<Variable[]>?> PostVariablesAsync(IEnumerable<Variable> variables) =>
            throw new NotSupportedException();

        public Task<Response<Variable[]>> PostVariablesCsvAsync(string csv) =>
            throw new NotSupportedException();

        public Task<Response<Tag[]>?> PostTagsAsync(IEnumerable<Tag> tags) =>
            throw new NotSupportedException();

        public Task<Response<Agent>> GetAgentAsync() =>
            throw new NotSupportedException();

        public Task<Response<Company[]>> GetAssociatedCompanyAsync() =>
            throw new NotSupportedException();

        public Task<Response<Agent[]>> GetAgentsAsync() =>
            throw new NotSupportedException();

        public Task<Response<string>> PushConfigurationAsync(string agentId) =>
            throw new NotSupportedException();
    }

    private sealed class FakeIxonAuthenticationContext : IIxonAuthenticationContext
    {
        public IxonHeaders IxonHeaders { get; set; } = null!;
    }
}
