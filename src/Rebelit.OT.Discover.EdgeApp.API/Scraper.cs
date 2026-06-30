using Microsoft.Extensions.Logging;
using Opc.Ua;
using Rebelit.OT.Discover.EdgeApp.API.Synchronizers;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;
using Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Clients;
using Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Factory;
using Rebelit.OT.Discover.EdgeApp.SharedKernel.IxonAuthentication;

namespace Rebelit.OT.Discover.EdgeApp.API;

public class Scraper(
    IUAClientFactory clientFactory,
    IClientSamplerFactory clientSamplerFactory,
    INodeSynchronizer nodeSynchronizer,
    IIxonAuthenticationContext ixonAuthenticationContext,
    ILogger<Scraper> logger
) : IScraper
{
    internal const int BatchSize = 5;

    public async Task<IReadOnlyList<Variable>> ScrapeVariablesAsync(CancellationToken cancellationToken)
    {
        var dataSourceId = ixonAuthenticationContext.IxonHeaders.SourceId;

        if (!ValidateDataSourceId(dataSourceId))
            return [];

        var client = await CreateClientAsync();
        if (client is null)
            return [];

        var nodes = await FetchReferenceDescriptionsAsync(client, cancellationToken);
        if (nodes is null)
            return [];

        LogFoundNodes(nodes);
        await nodeSynchronizer.InitializeAsync(dataSourceId);

        var filteredNodes = FilterValidNodes(nodes);
        var createdVariables = await MapVariablesBatchAsync(client, filteredNodes, dataSourceId);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Mapped {VariableCount} variables from OPC UA nodes.",
                createdVariables.Count
            );
        }

        await nodeSynchronizer.SynchronizeVariablesAsync(ixonAuthenticationContext.IxonHeaders.GetRequiredAgentId(), createdVariables);

        return createdVariables;
    }

    private bool ValidateDataSourceId(string? dataSourceId)
    {
        if (dataSourceId == null)
        {
            logger.LogError("Data source ID is missing. Aborting execution.");
            return false;
        }
        return true;
    }

    private void LogFoundNodes(ReferenceDescriptionCollection nodes)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Found {NodeCount} nodes in the OPC UA address space.", nodes.Count);
        }

        if (logger.IsEnabled(LogLevel.Trace))
        {
            foreach (var rd in nodes)
            {
                logger.LogTrace("Found node {NodeId} ({DisplayName}).", rd.NodeId, rd.DisplayName);
            }
        }
    }

    private IEnumerable<ReferenceDescription> FilterValidNodes(ReferenceDescriptionCollection nodes)
    {
        return nodes.Where(IsValidNode);
    }

    private bool IsValidNode(ReferenceDescription rd)
    {
        if (rd.NodeId.NamespaceIndex == 0 || rd.NodeId.NamespaceIndex == 1)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Skipping node {NodeId} in namespace {NamespaceIndex}.", rd.NodeId, rd.NodeId.NamespaceIndex);
            }
            return false;
        }
        return true;
    }

    private async Task<List<Variable>> MapVariablesBatchAsync(UAClient client, IEnumerable<ReferenceDescription> nodes, string dataSourceId)
    {
        var createdVariables = new List<Variable>();
        foreach (var batch in nodes.Chunk(BatchSize))
        {
            var batchVariables = await Task.WhenAll(
                batch.Select(rd => nodeSynchronizer.MapVariableAsync(client, rd, dataSourceId)));
            createdVariables.AddRange(batchVariables.Where(v => v is not null)!);
        }
        return createdVariables;
    }

    private async Task<UAClient?> CreateClientAsync()
    {
        if (!ValidatePlcUrl())
            return null;

        if (HasBothOrNeitherCredentials())
            return await clientFactory.CreateAsync(ixonAuthenticationContext.IxonHeaders.PlcUrl);

        if (!HasCompleteCredentials())
        {
            logger.LogError("PLC credentials are incomplete. Provide both username and password.");
            return null;
        }

        return await clientFactory.CreateAsync(
            ixonAuthenticationContext.IxonHeaders.PlcUrl,
            ixonAuthenticationContext.IxonHeaders.PlcUsername,
            ixonAuthenticationContext.IxonHeaders.PlcPassword);
    }

    private bool ValidatePlcUrl()
    {
        if (string.IsNullOrWhiteSpace(ixonAuthenticationContext.IxonHeaders.PlcUrl))
        {
            logger.LogError("PLC URL is missing. Aborting execution.");
            return false;
        }
        return true;
    }

    private bool HasBothOrNeitherCredentials()
    {
        var username = ixonAuthenticationContext.IxonHeaders.PlcUsername;
        var password = ixonAuthenticationContext.IxonHeaders.PlcPassword;
        return (string.IsNullOrWhiteSpace(username) && string.IsNullOrWhiteSpace(password));
    }

    private bool HasCompleteCredentials()
    {
        var username = ixonAuthenticationContext.IxonHeaders.PlcUsername;
        var password = ixonAuthenticationContext.IxonHeaders.PlcPassword;
        return !string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password);
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

}
