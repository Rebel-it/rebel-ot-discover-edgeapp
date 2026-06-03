using Opc.Ua;
using Rebelit.OT.Discover.EdgeApp.API.Resolvers;
using Rebelit.OT.Discover.EdgeApp.API.Synchronizers;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;
using Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Clients;
using Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Factory;
using Rebelit.OT.Discover.EdgeApp.SharedKernel.IxonAuthentication;

namespace Rebelit.OT.Discover.EdgeApp.API;

public class Scraper(
    IUAClientFactory clientFactory,
    IClientSamplerFactory clientSamplerFactory,
    IDataSourceResolver dataSourceResolver,
    INodeSynchronizer nodeSynchronizer,
    IIxonAuthenticationContext ixonAuthenticationContext,
    ILogger<Scraper> logger
) : IScraper
{
    internal const int BatchSize = 5;

    /// <summary>
    /// Contains all the variables that are mapped on runtime
    /// </summary>
    private readonly List<Variable> _CreatedVariables = new();

    public IReadOnlyList<Variable> CreatedVariables => _CreatedVariables;

    protected virtual async Task<ReferenceDescriptionCollection?> FetchReferenceDescriptionsAsync(
        CancellationToken cancellationToken
    )
    {
        var client = await CreateClientAsync();
        if (client == null)
        {
            logger.LogError("Failed to create UAClient. Aborting execution.");
            return null;
        }

        return await FetchReferenceDescriptionsAsync(client, cancellationToken);
    }

    private async Task<UAClient?> CreateClientAsync()
    {
        var plcUrl = ixonAuthenticationContext.IxonHeaders.PlcUrl;
        var username = ixonAuthenticationContext.IxonHeaders.PlcUsername;
        var password = ixonAuthenticationContext.IxonHeaders.PlcPassword;

        if (string.IsNullOrWhiteSpace(plcUrl))
        {
            logger.LogError("PLC URL is missing. Aborting execution.");
            return null;
        }

        if (string.IsNullOrWhiteSpace(username) && string.IsNullOrWhiteSpace(password))
        {
            return await clientFactory.Create(plcUrl);
        }

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogError("PLC credentials are incomplete. Provide both username and password.");
            return null;
        }

        return await clientFactory.Create(plcUrl, username, password);
    }

    private async Task<ReferenceDescriptionCollection?> FetchReferenceDescriptionsAsync(
        UAClient client,
        CancellationToken cancellationToken
    )
    {
        var sampler = await clientSamplerFactory.CreateAsync();
        return await sampler
            .BrowseFullAddressSpaceAsync(client, Objects.RootFolder, ct: cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task ExecuteVariableScraperAsync(CancellationToken cancellationToken)
    {
        _CreatedVariables.Clear();
        var dataSourceId = ixonAuthenticationContext.IxonHeaders.SourceId;

        if (dataSourceId == null)
        {
            logger.LogError("Data source ID is missing. Aborting execution.");
            return;
        }

        var client = await CreateClientAsync();
        if (client is null)
        {
            logger.LogError("Failed to create UAClient. Aborting execution.");
            return;
        }

        var nodes = await FetchReferenceDescriptionsAsync(client, cancellationToken);
        if (nodes is null)
        {
            return;
        }

        logger.LogInformation("Found {NodeCount} nodes in the OPC UA address space.", nodes.Count);
        foreach (var rd in nodes)
        {
            logger.LogTrace("Found node {NodeId} ({DisplayName}).", rd.NodeId, rd.DisplayName);
        }

        await nodeSynchronizer.InitializeAsync();

        var filteredNodes = nodes.Where(rd =>
        {
            if (rd.NodeId.NamespaceIndex == 0 || rd.NodeId.NamespaceIndex == 1)
            {
                logger.LogDebug("Skipping node {NodeId} in namespace {NamespaceIndex}.", rd.NodeId, rd.NodeId.NamespaceIndex);
                return false;
            }
            return true;
        });

        var createdVariables = new List<Variable>();
        foreach (var batch in filteredNodes.Chunk(BatchSize))
        {
            var batchVariables = await Task.WhenAll(
                batch.Select(rd => nodeSynchronizer.MapVariableAsync(client, rd, dataSourceId)));
            createdVariables.AddRange(batchVariables.Where(v => v is not null)!);
        }

        logger.LogInformation(
            "Mapped {VariableCount} variables from OPC UA nodes.",
            createdVariables.Count
        );

        await nodeSynchronizer.SynchronizeVariablesAsync(ixonAuthenticationContext.IxonHeaders.AgentId, createdVariables);
    }
}
