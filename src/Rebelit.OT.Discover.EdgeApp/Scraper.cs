using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Opc.Ua;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Agents;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;
using Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Factory;

namespace Rebelit.OT.Discover.EdgeApp;

public class Scraper(
    IUAClientFactory clientFactory,
    IClientSamplerFactory clientSamplerFactory,
    IApiClient apiClient,
    IConfiguration configuration,
    ILogger<Scraper> logger
) : IScraper
{
    private const int BatchSize = 5;
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

    public string? DataSourceId { get; } = configuration["IXON_DataSourceId"] ?? null;

    /// <summary>
    ///     A set of existing variable addresses that have already been created in the IXON platform. This is used to avoid creating duplicate variables when scraping the OPC UA server. The set is populated at the beginning of the execution by fetching the existing variables from the IXON platform for the specified agent.
    /// </summary>
    public HashSet<string> ExistingAddresses { get; private set; } = new();

    /// <summary>
    ///     A set of existing tag public IDs that have already been created in the IXON platform. This is used to avoid creating duplicate tags when scraping the OPC UA server. The set is populated at the beginning of the execution by fetching the existing tags from the IXON platform for the specified agent.
    /// </summary>
    public HashSet<string> ExistingTags { get; private set; } = new();

    /// <summary>
    ///     Executes the scraping process. This method connects to the OPC UA server, browses the full address space, and creates variables and tags in the IXON platform for each relevant node found in the OPC UA server. It checks for existing variables and tags to avoid duplicates and logs the progress of the scraping process.
    /// </summary>
