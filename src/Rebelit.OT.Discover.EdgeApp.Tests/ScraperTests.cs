using Microsoft.Extensions.Logging.Abstractions;
using Opc.Ua;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Agents;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;
using Rebelit.OT.Discover.EdgeApp.Connections.OPCUA;
using Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Factory;

namespace Rebelit.OT.Discover.EdgeApp.Tests;

[TestFixture]
public class ScraperTests
{
    private SpyApiClient _apiClient = null!;
    private NullUAClientFactory _uaClientFactory = null!;
    private NullClientSamplerFactory _clientSamplerFactory = null!;

    [SetUp]
    public void SetUp()
    {
        _apiClient = new SpyApiClient();
        _uaClientFactory = new NullUAClientFactory();
        _clientSamplerFactory = new NullClientSamplerFactory();

        Environment.SetEnvironmentVariable("OPCUA_ServerAddress", "opc.tcp://localhost:4840");
        Environment.SetEnvironmentVariable("IXON_AgentId", "test-agent-id");
        Environment.SetEnvironmentVariable("OPCUA_Username", "user");
        Environment.SetEnvironmentVariable("OPCUA_Password", "pass");
        Environment.SetEnvironmentVariable("IXON_DataSourceId", null);
    }

    [TearDown]
    public void TearDown()
    {
        Environment.SetEnvironmentVariable("OPCUA_ServerAddress", null);
        Environment.SetEnvironmentVariable("IXON_AgentId", null);
        Environment.SetEnvironmentVariable("OPCUA_Username", null);
        Environment.SetEnvironmentVariable("OPCUA_Password", null);
        Environment.SetEnvironmentVariable("IXON_DataSourceId", null);
    }

    [Test]
    public async Task ExecuteAsync_WhenDataSourceIdProvided_UsesProvidedIdForVariables()
    {
        Environment.SetEnvironmentVariable("IXON_DataSourceId", "existing-ds-id");
        var scraper = CreateSut();

        await scraper.ExecuteAsync(CancellationToken.None);

        Assert.That(_apiClient.PostDataSourceCallCount, Is.EqualTo(0));
    }

    [Test]
    public async Task ExecuteAsync_WhenDataSourceIdNotProvided_CreatesNewDataSourceAndUsesItsId()
    {
        _apiClient.PostDataSourceResponse = new Response<DataSource>
        {
            Data = new DataSource { PublicId = "new-ds-id" },
        };
        var scraper = CreateSut();

        await scraper.ExecuteAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(_apiClient.PostDataSourceCallCount, Is.EqualTo(1));
            Assert.That(
                _apiClient.LastPostDataSourceRequest?.Device.PublicId,
                Is.EqualTo("test-agent-id")
            );
        });
    }

    private Scraper CreateSut() =>
        new(_uaClientFactory, _clientSamplerFactory, _apiClient, NullLogger<Scraper>.Instance);

    private sealed class SpyApiClient : IApiClient
    {
        public int PostDataSourceCallCount { get; private set; }
        public DataSource? LastPostDataSourceRequest { get; private set; }
        public Response<DataSource> PostDataSourceResponse { get; set; } =
            new() { Data = new DataSource { PublicId = "default-ds-id" } };

        public Task<Response<DataSource>?> PostDataSourceAsync(
            string agentId,
            DataSource newDataSource
        )
        {
            PostDataSourceCallCount++;
            LastPostDataSourceRequest = newDataSource;
            return Task.FromResult<Response<DataSource>?>(PostDataSourceResponse);
        }

        public Task<Response<Variable[]>> GetDataVariablesAsync(string agentId) =>
            Task.FromResult(new Response<Variable[]> { Data = [] });

        public Task<Response<Tag[]>> GetTagsAsync(string agentId) =>
            Task.FromResult(new Response<Tag[]> { Data = [] });

        public Task<Response<Variable>?> PostVariableAsync(string agentId, Variable newVariable) =>
            Task.FromResult<Response<Variable>?>(null);

        public Task<Response<Tag>?> PostTagAsync(string agentId, Tag newTag) =>
            Task.FromResult<Response<Tag>?>(null);

        public Task<Response<DataSource[]>> GetDataSourcesAsync(string agentId) =>
            Task.FromResult(new Response<DataSource[]> { Data = [] });
    }

    private sealed class NullUAClientFactory : IUAClientFactory
    {
        public Task<Connections.OPCUA.Clients.UAClient?> Create(
            string uri,
            string username,
            string password
        ) => Task.FromResult<Connections.OPCUA.Clients.UAClient?>(null);
    }

    private sealed class NullClientSamplerFactory : IClientSamplerFactory
    {
        public Task<ClientSamples> CreateAsync() =>
            throw new NotSupportedException("Should not be reached when UAClient is null.");
    }
}
