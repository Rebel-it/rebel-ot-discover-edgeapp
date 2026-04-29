using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Opc.Ua;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;
using Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Factory;
using Rebelit.OT.Discover.EdgeApp.Exporters;
using Rebelit.OT.Discover.EdgeApp.Resolvers;
using Rebelit.OT.Discover.EdgeApp.Synchronizers;

namespace Rebelit.OT.Discover.EdgeApp;

public class Scraper(
    IUAClientFactory clientFactory,
    ICsvExporters csvExporters,
    IClientSamplerFactory clientSamplerFactory,
    IDataSourceResolver dataSourceResolver,
    INodeSynchronizer nodeSynchronizer,
    IConfiguration configuration,
    ILogger<Scraper> logger
) : IScraper
{
    internal const int BatchSize = 5;
    private const string DefaultExportFolder = "exports";

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
    /// Contains all the variables that are mapped on runtime
    /// </summary>
    private List<Variable> _CreatedVariables = [];

    /// <summary>
    /// Contains all the tags that are built on runtime
    /// </summary>
    private List<Tag> _CreatedTags = [];

    public IReadOnlyList<Variable> CreatedVariables => _CreatedVariables;
    public IReadOnlyList<Tag> CreatedTags => _CreatedTags;

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var dataSourceId = await dataSourceResolver.ResolveAsync(AgentId, "");

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

        //Export to csv
        var exportFolder = GetExportFolder();
        EnsureExportFolderExists(exportFolder);

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var variableFilePath = Path.Combine(exportFolder, $"variables_{timestamp}.csv");
        var tagFilePath = Path.Combine(exportFolder, $"tags_{timestamp}.csv");

        await csvExporters.CreateVariableCsvFileAsync(_CreatedVariables, variableFilePath);
        await csvExporters.CreateTagCsvFileAsync(_CreatedTags, tagFilePath);

    }

    private string GetExportFolder()
    {
        var configuredPath = configuration["ExportPath"];

        if (!string.IsNullOrWhiteSpace(configuredPath))
        {
            logger.LogInformation("Using configured export path: {ExportPath}", configuredPath);
            return configuredPath;
        }

        var defaultPath = Path.Combine(Directory.GetCurrentDirectory(), DefaultExportFolder);
        logger.LogInformation("Using default export path: {ExportPath}", defaultPath);
        return defaultPath;
    }

    private void EnsureExportFolderExists(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            logger.LogInformation("Creating export folder: {FolderPath}", folderPath);
            Directory.CreateDirectory(folderPath);
        }
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

    public async Task ExecuteVariableScraper(CancellationToken cancellationToken)
    {
        var dataSourceId = await dataSourceResolver.ResolveAsync(AgentId, "");
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

        //map all vartiables in a list
        foreach (var batch in filteredNodes.Chunk(BatchSize))
        {
            var batchVariables = await Task.WhenAll(
                batch.Select(rd => nodeSynchronizer.MapVariableAsync(client, rd, dataSourceId)));
            _CreatedVariables.AddRange(batchVariables.Where(v => v is not null)!);
        }

        logger.LogInformation(
            "Mapped {VariableCount} variables from OPC UA nodes.",
            _CreatedVariables.Count
        );

        var csv = csvExporters.CreateVariableCsv(_CreatedVariables);
        await nodeSynchronizer.SynchronizeVariables(AgentId, csv);
    }
}
