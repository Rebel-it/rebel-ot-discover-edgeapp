using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Opc.Ua;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;
using Rebelit.OT.Discover.EdgeApp.Connections.OPCUA;
using Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Clients;
using Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Factory;
using Rebelit.OT.Discover.EdgeApp.Exporters;
using Rebelit.OT.Discover.EdgeApp.Resolvers;
using Rebelit.OT.Discover.EdgeApp.Synchronizers;

namespace Rebelit.OT.Discover.EdgeApp.Tests;

[TestFixture]
public class ScraperTests
{
    private SpyNodeSynchronizer _nodeSynchronizer = null!;
    private StubDataSourceResolver _dataSourceResolver = null!;
    private NullUAClientFactory _uaClientFactory = null!;
    private NullClientSamplerFactory _clientSamplerFactory = null!;
    private StubCsvExporter _csvExporter = null!;

    [SetUp]
    public void SetUp()
    {
        _nodeSynchronizer = new SpyNodeSynchronizer();
        _dataSourceResolver = new StubDataSourceResolver("resolved-ds-id");
        _uaClientFactory = new NullUAClientFactory();
        _clientSamplerFactory = new NullClientSamplerFactory();
        _csvExporter = new StubCsvExporter();

        Environment.SetEnvironmentVariable("OPCUA_ServerAddress", "opc.tcp://localhost:4840");
        Environment.SetEnvironmentVariable("IXON_AgentId", "test-agent-id");
        Environment.SetEnvironmentVariable("OPCUA_Username", "user");
        Environment.SetEnvironmentVariable("OPCUA_Password", "pass");
    }

    [TearDown]
    public void TearDown()
    {
        Environment.SetEnvironmentVariable("OPCUA_ServerAddress", null);
        Environment.SetEnvironmentVariable("IXON_AgentId", null);
        Environment.SetEnvironmentVariable("OPCUA_Username", null);
        Environment.SetEnvironmentVariable("OPCUA_Password", null);
    }

    [Test]
    public async Task ExecuteAsync_WhenUAClientFails_DoesNotInitializeOrSyncNodes()
    {
        var scraper = CreateSut();

        await scraper.ExecuteAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(_nodeSynchronizer.InitializeCallCount, Is.EqualTo(0));
            Assert.That(_nodeSynchronizer.SynchronizeCallCount, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task ExecuteAsync_WithNodes_InitializesNodeSynchronizerWithAgentId()
    {
        var scraper = CreateFakeNodeScraper(nodeCount: 1);

        await scraper.ExecuteAsync(CancellationToken.None);

        Assert.That(_nodeSynchronizer.LastInitializedAgentId, Is.EqualTo("test-agent-id"));
    }

    [Test]
    public async Task ExecuteAsync_WithNodes_PassesResolvedDataSourceIdToEachSynchronize()
    {
        var scraper = CreateFakeNodeScraper(nodeCount: 2);

        await scraper.ExecuteAsync(CancellationToken.None);

        Assert.That(_nodeSynchronizer.ReceivedDataSourceIds, Is.All.EqualTo("resolved-ds-id"));
    }

    [Test]
    public async Task ExecuteAsync_WithManyNodes_ProcessesInBatchesOfBatchSize()
    {
        const int nodeCount = 11;
        var scraper = CreateFakeNodeScraper(nodeCount);

        await scraper.ExecuteAsync(CancellationToken.None);

        Assert.That(
            _nodeSynchronizer.MaxConcurrentSynchronizeCalls,
            Is.LessThanOrEqualTo(Scraper.BatchSize)
        );
    }

    private Scraper CreateSut()
    {
        var configuration = new ConfigurationBuilder().AddEnvironmentVariables().Build();
        return new(
            _uaClientFactory,
            _csvExporter,
            _clientSamplerFactory,
            _dataSourceResolver,
            _nodeSynchronizer,
            configuration,
            NullLogger<Scraper>.Instance
        );
    }

    private Scraper CreateFakeNodeScraper(int nodeCount)
    {
        var configuration = new ConfigurationBuilder().AddEnvironmentVariables().Build();
        return new FakeNodeScraper(
            _uaClientFactory,
            _csvExporter,
            _clientSamplerFactory,
            _dataSourceResolver,
            _nodeSynchronizer,
            configuration,
            NullLogger<Scraper>.Instance,
            nodeCount
        );
    }

    private sealed class SpyNodeSynchronizer : INodeSynchronizer
    {
        private readonly object _syncLock = new();
        private int _concurrentCalls;

        public int InitializeCallCount { get; private set; }
        public string? LastInitializedAgentId { get; private set; }
        public int SynchronizeCallCount { get; private set; }
        public int MaxConcurrentSynchronizeCalls { get; private set; }
        public List<string> ReceivedDataSourceIds { get; } = [];

        public Task InitializeAsync(string agentId)
        {
            InitializeCallCount++;
            LastInitializedAgentId = agentId;
            return Task.CompletedTask;
        }

        public async Task SynchronizeAsync(UAClient client, ReferenceDescription referenceDescription, string dataSourceId)
        {
            lock (_syncLock)
            {
                _concurrentCalls++;
                SynchronizeCallCount++;
                ReceivedDataSourceIds.Add(dataSourceId);
                if (_concurrentCalls > MaxConcurrentSynchronizeCalls)
                    MaxConcurrentSynchronizeCalls = _concurrentCalls;
            }

            await Task.Yield();

            lock (_syncLock)
            {
                _concurrentCalls--;
            }
        }

        public Task<Variable?> MapVariableAsync(UAClient client, ReferenceDescription referenceDescription, string dataSourceId)
        {
            ReceivedDataSourceIds.Add(dataSourceId);
            return Task.FromResult<Variable?>(new Variable
            {
                Name = referenceDescription.DisplayName.Text,
                Address = referenceDescription.NodeId.ToString(),
                Slug = referenceDescription.DisplayName.Text.ToLower(),
                Type = "int",
                PublicId = Guid.NewGuid().ToString(),
                Source = new Source { PublicId = dataSourceId }
            });
        }

        public Tag CreateTag(Variable variable)
        {
            return new Tag
            {
                Name = variable.Name,
                Slug = variable.Slug,
                Variable = variable,
                Source = variable.Source,
                RetentionPolicy = "260w",
                LogEvent = "change",
                LoggingInterval = "72s"
            };
        }

        public Task SynchronizeVariables(string agentId, IEnumerable<Variable> variables) => Task.CompletedTask;
    }

    private sealed class StubDataSourceResolver(string dataSourceId) : IDataSourceResolver
    {
        public Task<string> ResolveAsync(string agentId, string dataSourceName) => Task.FromResult(dataSourceId);
    }

    private sealed class FakeNodeScraper(
        IUAClientFactory uaClientFactory,
        ICsvExporters csvExporter,
        IClientSamplerFactory clientSamplerFactory,
        IDataSourceResolver dataSourceResolver,
        INodeSynchronizer nodeSynchronizer,
        IConfiguration configuration,
        ILogger<Scraper> logger,
        int nodeCount
    )
        : Scraper(
            uaClientFactory,
            csvExporter,
            clientSamplerFactory,
            dataSourceResolver,
            nodeSynchronizer,
            configuration,
            logger
        )
    {
        protected override Task<ReferenceDescriptionCollection?> FetchReferenceDescriptionsAsync(
            CancellationToken cancellationToken
        )
        {
            var nodes = new ReferenceDescriptionCollection(
                Enumerable.Range(0, nodeCount).Select(BuildFakeReferenceDescription)
            );
            return Task.FromResult<ReferenceDescriptionCollection?>(nodes);
        }

        private static ReferenceDescription BuildFakeReferenceDescription(int index) =>
            new()
            {
                NodeId = new ExpandedNodeId((uint)(1000 + index), namespaceIndex: 2),
                DisplayName = new LocalizedText($"Node{index}"),
                TypeDefinition = new ExpandedNodeId(6u),
            };
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

    private sealed class StubCsvExporter : ICsvExporters
    {
        public string CreateVariableCsv(List<Variable> variables) => string.Empty;
        public string CreateTagCsv(List<Tag> tags) => string.Empty;
        public Task CreateVariableCsvFileAsync(List<Variable> variables, string filePath) => Task.CompletedTask;
        public Task CreateTagCsvFileAsync(List<Tag> tags, string filePath) => Task.CompletedTask;
    }
}
