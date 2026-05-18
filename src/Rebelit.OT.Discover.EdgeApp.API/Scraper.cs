using Opc.Ua;
using Rebelit.OT.Discover.EdgeApp.API.Exporters;
using Rebelit.OT.Discover.EdgeApp.API.Resolvers;
using Rebelit.OT.Discover.EdgeApp.API.Synchronizers;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;
using Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Clients;
using Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Factory;
using Rebelit.OT.Discover.EdgeApp.SharedKernel.IxonAuthentication;

namespace Rebelit.OT.Discover.EdgeApp.API;

public class Scraper(
    IUAClientFactory clientFactory,
    ICsvExporters csvExporters,
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
    private List<Variable> _CreatedVariables = [];

    public IReadOnlyList<Variable> CreatedVariables => _CreatedVariables;

    protected virtual async Task<ReferenceDescriptionCollection?> FetchReferenceDescriptionsAsync(
        CancellationToken cancellationToken
    )
    {
        var client = await clientFactory.Create(ixonAuthenticationContext.IxonHeaders.PlcUrl, ixonAuthenticationContext.IxonHeaders.PlcUsername, ixonAuthenticationContext.IxonHeaders.PlcPassword);
        if (client == null)
        {
            logger.LogError("Failed to create UAClient. Aborting execution.");
            return null;
        }

        return await FetchReferenceDescriptionsAsync(client, cancellationToken);
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
        var dataSourceId = await dataSourceResolver.ResolveAsync("");

        var client = await clientFactory.Create(ixonAuthenticationContext.IxonHeaders.PlcUrl, ixonAuthenticationContext.IxonHeaders.PlcUsername, ixonAuthenticationContext.IxonHeaders.PlcPassword);
        if (client is null)
        {
            logger.LogError("Failed to create UAClient. Aborting execution.");
            return;
        }

        var nodes = await FetchReferenceDescriptionsAsync(client, cancellationToken);
        if (nodes is null)
            return;

        logger.LogInformation("Found {NodeCount} nodes in the OPC UA address space.", nodes.Count);
        foreach (var rd in nodes)
            logger.LogTrace("Found node {NodeId} ({DisplayName}).", rd.NodeId, rd.DisplayName);

        await nodeSynchronizer.InitializeAsync(ixonAuthenticationContext.IxonHeaders.AgentId);

        var filteredNodes = nodes.Where(rd =>
        {
            if (rd.NodeId.NamespaceIndex == 0)
            {
                logger.LogDebug("Skipping node {NodeId} in namespace 0.", rd.NodeId);
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
