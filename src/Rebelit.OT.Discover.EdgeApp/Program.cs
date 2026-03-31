using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rebelit.OT.Discover.EdgeApp;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON;
using Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Extensions;
using Rebelit.OT.Discover.EdgeApp.Mappers;
using Rebelit.OT.Discover.EdgeApp.Resolvers;
using Rebelit.OT.Discover.EdgeApp.Synchronizers;
using Serilog;
using Serilog.Events;

Console.WriteLine("Hello, World!");
var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()
    .Build();

var logLevelStr = configuration["LOG_LEVEL"] ?? "Information";
var logLevel = Enum.TryParse<LogEventLevel>(logLevelStr, ignoreCase: true, out var parsed)
    ? parsed
    : LogEventLevel.Information;

Log.Logger = new LoggerConfiguration().MinimumLevel.Is(logLevel).WriteTo.Console().CreateLogger();
Log.Logger.Debug("Starting console app...");

ServiceCollection services = new();
services.AddSingleton<IConfiguration>(configuration);
services.AddSingleton<Application>();
services.AddSingleton<IScraper, Scraper>();
services.AddSingleton<IOpcUaVariableMapper, OpcUaVariableMapper>();
services.AddSingleton<IDataSourceResolver, DataSourceResolver>();
services.AddSingleton<INodeSynchronizer, NodeSynchronizer>();
services.AddOPCUAClient("Rebelit.OT.Scraper");
services.AddIXONClient(
    configuration["IXON_ApplicationId"]
        ?? throw new ArgumentNullException("IXON_ApplicationId environment variable is not set."),
    configuration["IXON_CompanyId"]
        ?? throw new ArgumentNullException("IXON_CompanyId environment variable is not set."),
    configuration["IXON_BearerToken"]
        ?? throw new ArgumentNullException("IXON_BearerToken environment variable is not set.")
);

services.AddLogging(builder =>
{
    builder.AddSerilog(Log.Logger);
});

// Build the provider
var provider = services.BuildServiceProvider();

// Run the app
var app = provider.GetRequiredService<Application>();
await app.Run(CancellationToken.None);

Log.Logger.Debug("Ending console app...");
Log.CloseAndFlush();
