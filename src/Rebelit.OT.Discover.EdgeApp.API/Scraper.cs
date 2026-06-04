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

        if (dataSourceId == null)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError("Data source ID is missing. Aborting execution.");
            }
            return [];
        }

        var client = await CreateClientAsync();
        if (client is null)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError("Failed to create UAClient. Aborting execution.");
            }
            return [];
        }

        var nodes = await FetchReferenceDescriptionsAsync(client, cancellationToken);
        if (nodes is null)
        {
            return [];
        }

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

        await nodeSynchronizer.InitializeAsync();

        var filteredNodes = nodes.Where(rd =>
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
        });

        var createdVariables = new List<Variable>();
        foreach (var batch in filteredNodes.Chunk(BatchSize))
        {
            var batchVariables = await Task.WhenAll(
                batch.Select(rd => nodeSynchronizer.MapVariableAsync(client, rd, dataSourceId)));
            createdVariables.AddRange(batchVariables.Where(v => v is not null)!);
        }

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

    private async Task<UAClient?> CreateClientAsync()
    {
        if (string.IsNullOrWhiteSpace(ixonAuthenticationContext.IxonHeaders.PlcUrl))
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError("PLC URL is missing. Aborting execution.");
            }
            return null;
        }

        if (string.IsNullOrWhiteSpace(ixonAuthenticationContext.IxonHeaders.PlcUsername) &&
            string.IsNullOrWhiteSpace(ixonAuthenticationContext.IxonHeaders.PlcPassword))
        {
            return await clientFactory.CreateAsync(ixonAuthenticationContext.IxonHeaders.PlcUrl);
        }

        if (string.IsNullOrWhiteSpace(ixonAuthenticationContext.IxonHeaders.PlcUsername) ||
            string.IsNullOrWhiteSpace(ixonAuthenticationContext.IxonHeaders.PlcPassword))
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError("PLC credentials are incomplete. Provide both username and password.");
            }
            return null;
        }

        return await clientFactory.CreateAsync(
            ixonAuthenticationContext.IxonHeaders.PlcUrl,
            ixonAuthenticationContext.IxonHeaders.PlcUsername,
            ixonAuthenticationContext.IxonHeaders.PlcPassword);
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
