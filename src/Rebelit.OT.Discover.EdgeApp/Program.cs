using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rebelit.OT.Discover.EdgeApp;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Authentication;
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

Console.WriteLine("Enter Username:");
var username = Console.ReadLine();

Console.WriteLine("Enter Password:");
var password = ReadPassword();

Console.WriteLine("Do you use OTP? (y/n):");
var useOtp = Console.ReadLine()?.Trim().ToLower() == "y";

string? otpCode = null;
if (useOtp)
{
    Console.WriteLine("Enter OTP Code:");
    otpCode = Console.ReadLine();
}

static string ReadPassword()
{
    var password = string.Empty;
    ConsoleKey key;
    do
    {
        var keyInfo = Console.ReadKey(intercept: true);
        key = keyInfo.Key;

        if (key == ConsoleKey.Backspace && password.Length > 0)
        {
            password = password[0..^1];
            Console.Write("\b \b");
        }
        else if (!char.IsControl(keyInfo.KeyChar))
        {
            password += keyInfo.KeyChar;
            Console.Write("*");
        }
    } while (key != ConsoleKey.Enter);
    Console.WriteLine();
    return password;
}

var logLevelStr = configuration["LOG_LEVEL"] ?? "Information";
var logLevel = Enum.TryParse<LogEventLevel>(logLevelStr, ignoreCase: true, out var parsed)
    ? parsed
    : LogEventLevel.Information;

Log.Logger = new LoggerConfiguration().MinimumLevel.Is(logLevel).WriteTo.Console().CreateLogger();
Log.Logger.Debug("Starting console app...");

// Generate IXON Bearer Token using user credentials
var applicationId = configuration["IXON_ApplicationId"]
    ?? throw new ArgumentNullException("IXON_ApplicationId environment variable is not set.");

Log.Logger.Information("Generating IXON bearer token...");
var ixonAuth = new IxonAuthentication();
var bearerToken = await ixonAuth.BearerTokenGenerator(
    username ?? throw new ArgumentNullException("Username cannot be empty"),
    password,
    applicationId,
    otpCode
);
Log.Logger.Information("Bearer token generated successfully.");

ServiceCollection services = new();
services.AddSingleton<IConfiguration>(configuration);
services.AddSingleton<Application>();
services.AddSingleton<IScraper, Scraper>();
services.AddSingleton<IOpcUaVariableMapper, OpcUaVariableMapper>();
services.AddSingleton<IDataSourceResolver, DataSourceResolver>();
services.AddSingleton<INodeSynchronizer, NodeSynchronizer>();
services.AddOPCUAClient("Rebelit.OT.Scraper");
services.AddIXONClient(
    applicationId,
    configuration["IXON_CompanyId"]
        ?? throw new ArgumentNullException("IXON_CompanyId environment variable is not set."),
    bearerToken
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
