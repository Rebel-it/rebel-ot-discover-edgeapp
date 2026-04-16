using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Opc.Ua;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;
using Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Factory;
using Rebelit.OT.Discover.EdgeApp.Resolvers;
using Rebelit.OT.Discover.EdgeApp.Synchronizers;

namespace Rebelit.OT.Discover.EdgeApp;

public class Scraper(
    IUAClientFactory clientFactory,
    IClientSamplerFactory clientSamplerFactory,
    IDataSourceResolver dataSourceResolver,
    INodeSynchronizer nodeSynchronizer,
    IConfiguration configuration,
    ILogger<Scraper> logger
) : IScraper
{
    internal const int BatchSize = 5;

    public string Address { get; } =
        configuration["OPCUA_ServerAddress"]
        ?? throw new InvalidOperationException("OPCUA_ServerAddress configuration is not set.");

    public string AgentId { get; } =
        configuration["IXON_AgentId"]
        ?? throw new InvalidOperationException("IXON_AgentId configuration is not set.");

    public string Username { get; } =
        configuration["OPCUA_Username"]
        ?? throw new InvalidOperationException("OPCUA_Username configuration is not set.");

    public string Password { get; } =
        configuration["OPCUA_Password"]
        ?? throw new InvalidOperationException("OPCUA_Password configuration is not set.");

    /// <summary>
    /// Contains all the variables that are created on runtime
    /// </summary>
    private List<Variable> _CreatedVariables;

    /// <summary>
    /// Contains all the tags that are created on runtime
    /// </summary>
    private List<Tag> _CreatedTags = [];

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var dataSourceId = await dataSourceResolver.ResolveAsync(AgentId);

        var client = await clientFactory.Create(Address, Username, Password);

        var nodes = await FetchReferenceDescriptionsAsync(cancellationToken);
        if (nodes is null)
            return;

        logger.LogInformation("Found {NodeCount} nodes in the OPC UA address space.", nodes.Count);
        foreach (var rd in nodes)
            logger.LogTrace("Found node {NodeId} ({DisplayName}).", rd.NodeId, rd.DisplayName);

        await nodeSynchronizer.InitializeAsync(AgentId);

        var filteredNodes = nodes.Where(rd =>
        {
            if (rd.NodeId.NamespaceIndex == 0)
            {
                logger.LogDebug(
                    "Skipping node {NodeId} in namespace 0.",
                    rd.NodeId
                );
                return false;
            }
            return true;
        });

        _CreatedVariables = [];

        //map all vartiables in a list
        foreach(var batch in filteredNodes.Chunk(BatchSize))
        {
            var batchVariables = await Task.WhenAll(
                batch.Select(rd => nodeSynchronizer.MapVariableAsync(client, rd, dataSourceId)));
            _CreatedVariables.AddRange(batchVariables.Where(v => v is not null)!);
        }

        logger.LogInformation(
            "Mapped {VariableCount} variables from OPC UA nodes.",
            _CreatedVariables.Count
        );

        _CreatedTags = [];

        //Create Tags
        foreach(var variable in _CreatedVariables)
        {
            var tag = nodeSynchronizer.CreateTag(variable);
            if(tag is not null)
            {
                _CreatedTags.Add(tag);
            }
        }
        logger.LogInformation(
            "Built {TagCount} tags from variables.",
            _CreatedTags.Count
        );

        logger.LogInformation(
          "Scraping complete. {VariableCount} variables and {TagCount} tags ready for export.",
          _CreatedVariables.Count,
          _CreatedTags.Count
      );

        //Build and create tags

        //foreach (var batch in filteredNodes.Chunk(BatchSize))
        //    await Task.WhenAll(
        //        batch.Select(rd => nodeSynchronizer.SynchronizeAsync(client, rd, dataSourceId))
        //    );
    }

    protected virtual async Task<ReferenceDescriptionCollection?> FetchReferenceDescriptionsAsync(
        CancellationToken cancellationToken
    )
    {
        var client = await clientFactory.Create(Address, Username, Password);
        if (client == null)
        {
            logger.LogError("Failed to create UAClient. Aborting execution.");
            return null;
        }

        var sampler = await clientSamplerFactory.CreateAsync();
        return await sampler
            .BrowseFullAddressSpaceAsync(client, Objects.RootFolder, ct: cancellationToken)
            .ConfigureAwait(false);
    }
}
