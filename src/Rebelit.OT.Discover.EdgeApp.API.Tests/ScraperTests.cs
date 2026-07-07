using Microsoft.Extensions.Logging.Abstractions;
using Opc.Ua;
using Rebelit.OT.Discover.EdgeApp.API;
using Rebelit.OT.Discover.EdgeApp.API.Synchronizers;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;
using Rebelit.OT.Discover.EdgeApp.Connections.OPCUA;
using Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Clients;
using Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Factory;
using Rebelit.OT.Discover.EdgeApp.SharedKernel.IxonAuthentication;

namespace Rebelit.OT.Discover.EdgeApp.Tests;

[TestFixture]
public class ScraperTests
{
    [Test]
    public async Task ScrapeVariablesAsync_WhenSourceIdIsMissing_ReturnsEmptyAndSkipsDependencies()
    {
        // Arrange
        var clientFactory = new FakeUaClientFactory();
        var samplerFactory = new FakeClientSamplerFactory();
        var synchronizer = new FakeNodeSynchronizer();

        var sut = CreateSut(
            clientFactory,
            samplerFactory,
            synchronizer,
            sourceId: null,
            plcUrl: "192.168.1.10:4840"
        );

        // Act
        var result = await sut.ScrapeVariablesAsync(CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Empty);
            Assert.That(clientFactory.UriOnlyCallCount, Is.EqualTo(0));
            Assert.That(clientFactory.CredentialCallCount, Is.EqualTo(0));
            Assert.That(samplerFactory.CallCount, Is.EqualTo(0));
            Assert.That(synchronizer.InitializeCallCount, Is.EqualTo(0));
            Assert.That(synchronizer.SynchronizeCallCount, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task ScrapeVariablesAsync_WhenPlcUrlIsMissing_ReturnsEmptyAndSkipsFactoryCalls()
    {
        // Arrange
        var clientFactory = new FakeUaClientFactory();
        var samplerFactory = new FakeClientSamplerFactory();
        var synchronizer = new FakeNodeSynchronizer();

        var sut = CreateSut(
            clientFactory,
            samplerFactory,
            synchronizer,
            sourceId: "source-1",
            plcUrl: null
        );

        // Act
        var result = await sut.ScrapeVariablesAsync(CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Empty);
            Assert.That(clientFactory.UriOnlyCallCount, Is.EqualTo(0));
            Assert.That(clientFactory.CredentialCallCount, Is.EqualTo(0));
            Assert.That(samplerFactory.CallCount, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task ScrapeVariablesAsync_WhenCredentialsAreIncomplete_ReturnsEmptyAndDoesNotCreateClient()
    {
        // Arrange
        var clientFactory = new FakeUaClientFactory();
        var samplerFactory = new FakeClientSamplerFactory();
        var synchronizer = new FakeNodeSynchronizer();

        var sut = CreateSut(
            clientFactory,
            samplerFactory,
            synchronizer,
            sourceId: "source-1",
            plcUrl: "192.168.1.10:4840",
            plcUsername: "user-only",
            plcPassword: null
        );

        // Act
        var result = await sut.ScrapeVariablesAsync(CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Empty);
            Assert.That(clientFactory.UriOnlyCallCount, Is.EqualTo(0));
            Assert.That(clientFactory.CredentialCallCount, Is.EqualTo(0));
            Assert.That(samplerFactory.CallCount, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task ScrapeVariablesAsync_WhenCredentialsAreBothMissing_UsesUriOnlyFactoryOverload()
    {
        // Arrange
        var clientFactory = new FakeUaClientFactory
        {
            ClientForUriOnlyOverload = null,
        };
        var samplerFactory = new FakeClientSamplerFactory();
        var synchronizer = new FakeNodeSynchronizer();

        var sut = CreateSut(
            clientFactory,
            samplerFactory,
            synchronizer,
            sourceId: "source-1",
            plcUrl: "192.168.1.10:4840",
            plcUsername: null,
            plcPassword: null
        );

        // Act
        var result = await sut.ScrapeVariablesAsync(CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Empty);
            Assert.That(clientFactory.UriOnlyCallCount, Is.EqualTo(1));
            Assert.That(clientFactory.CredentialCallCount, Is.EqualTo(0));
            Assert.That(clientFactory.LastUriOnlyAddress, Is.EqualTo("192.168.1.10:4840"));
            Assert.That(samplerFactory.CallCount, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task ScrapeVariablesAsync_WhenCredentialsAreComplete_UsesCredentialFactoryOverload()
    {
        // Arrange
        var clientFactory = new FakeUaClientFactory
        {
            ClientForCredentialOverload = null,
        };
        var samplerFactory = new FakeClientSamplerFactory();
        var synchronizer = new FakeNodeSynchronizer();

        var sut = CreateSut(
            clientFactory,
            samplerFactory,
            synchronizer,
            sourceId: "source-1",
            plcUrl: "192.168.1.10:4840",
            plcUsername: "user1",
            plcPassword: "pass1"
        );

        // Act
        var result = await sut.ScrapeVariablesAsync(CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Empty);
            Assert.That(clientFactory.UriOnlyCallCount, Is.EqualTo(0));
            Assert.That(clientFactory.CredentialCallCount, Is.EqualTo(1));
            Assert.That(clientFactory.LastCredentialAddress, Is.EqualTo("192.168.1.10:4840"));
            Assert.That(clientFactory.LastUsername, Is.EqualTo("user1"));
            Assert.That(clientFactory.LastPassword, Is.EqualTo("pass1"));
            Assert.That(samplerFactory.CallCount, Is.EqualTo(0));
        });
    }

    private static Scraper CreateSut(
        FakeUaClientFactory clientFactory,
        FakeClientSamplerFactory samplerFactory,
        FakeNodeSynchronizer synchronizer,
        string? sourceId,
        string? plcUrl,
        string? plcUsername = null,
        string? plcPassword = null,
        string? agentId = "agent-1"
    )
    {
        return new Scraper(
            clientFactory,
            samplerFactory,
            synchronizer,
            new FakeIxonAuthenticationContext
            {
                IxonHeaders = new IxonHeaders
                {
                    ServiceAccount = new ServiceAccount
                    {
                        AccessToken = "token",
                        ApiApplicationId = "app-id",
                    },
                    SourceId = sourceId,
                    AgentId = agentId,
                    PlcUrl = plcUrl,
                    PlcUsername = plcUsername,
                    PlcPassword = plcPassword,
                },
            },
            NullLogger<Scraper>.Instance
        );
    }

    private sealed class FakeUaClientFactory : IUAClientFactory
    {
        public UAClient? ClientForUriOnlyOverload { get; init; }
        public UAClient? ClientForCredentialOverload { get; init; }

        public int UriOnlyCallCount { get; private set; }
        public int CredentialCallCount { get; private set; }

        public string? LastUriOnlyAddress { get; private set; }
        public string? LastCredentialAddress { get; private set; }
        public string? LastUsername { get; private set; }
        public string? LastPassword { get; private set; }

        public Task<UAClient?> CreateAsync(string uri)
        {
            UriOnlyCallCount++;
            LastUriOnlyAddress = uri;
            return Task.FromResult(ClientForUriOnlyOverload);
        }

        public Task<UAClient?> CreateAsync(string opcServerAddress, string username, string password)
        {
            CredentialCallCount++;
            LastCredentialAddress = opcServerAddress;
            LastUsername = username;
            LastPassword = password;
            return Task.FromResult(ClientForCredentialOverload);
        }
    }

    private sealed class FakeClientSamplerFactory : IClientSamplerFactory
    {
        public int CallCount { get; private set; }

        public Task<ClientSamples> CreateAsync()
        {
            CallCount++;
            throw new InvalidOperationException("Sampler should not be created in these tests.");
        }
    }

    private sealed class FakeNodeSynchronizer : INodeSynchronizer
    {
        public int InitializeCallCount { get; private set; }
        public int MapCallCount { get; private set; }
        public int SynchronizeCallCount { get; private set; }

        public Task InitializeAsync(string dataSourceId)
        {
            InitializeCallCount++;
            return Task.CompletedTask;
        }

        public Task<Variable?> MapVariableAsync(UAClient client, ReferenceDescription referenceDescription, string dataSourceId)
        {
            MapCallCount++;
            return Task.FromResult<Variable?>(null);
        }

        public Task SynchronizeVariablesAsync(string agentId, IEnumerable<Variable> variables)
        {
            SynchronizeCallCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeIxonAuthenticationContext : IIxonAuthenticationContext
    {
        public IxonHeaders IxonHeaders { get; set; } = null!;
    }
}
