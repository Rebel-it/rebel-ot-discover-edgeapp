using Rebelit.OT.Discover.EdgeApp.API;
using Rebelit.OT.Discover.EdgeApp.API.Exporters;
using Rebelit.OT.Discover.EdgeApp.API.Filters;
using Rebelit.OT.Discover.EdgeApp.API.Mappers;
using Rebelit.OT.Discover.EdgeApp.API.Resolvers;
using Rebelit.OT.Discover.EdgeApp.API.Services;
using Rebelit.OT.Discover.EdgeApp.API.Synchronizers;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON;
using Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Extensions;
using Rebelit.OT.Discover.EdgeApp.Connections.SecureEdgePro;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
    Path.Combine(AppContext.BaseDirectory, "settings.json"),
    optional: true,
    reloadOnChange: true
);

var logLevelStr = builder.Configuration["LOG_LEVEL"] ?? "Information";
var logLevel = Enum.TryParse<LogEventLevel>(logLevelStr, ignoreCase: true, out var parsed)
    ? parsed
    : LogEventLevel.Information;

Log.Logger = new LoggerConfiguration().MinimumLevel.Is(logLevel).WriteTo.Console().CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddScoped<AuthenticationFilter>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("LocalFrontend", policy =>
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod());
    });
}

builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<IVariableService, VariableService>();
builder.Services.AddSingleton<ICsvExporters, CsvExporters>();
builder.Services.AddScoped<IScraper, Scraper>();
builder.Services.AddSingleton<IOpcUaVariableMapper, OpcUaVariableMapper>();
builder.Services.AddScoped<IDataSourceResolver, DataSourceResolver>();
builder.Services.AddScoped<INodeSynchronizer, NodeSynchronizer>();
builder.Services.AddScoped<ICompanyConfigurationService, CompanyConfigurationService>();
builder.Services.AddScoped<IIxonSettingService, IxonSettingService>();
builder.Services.AddOPCUAClient("Rebelit.OT.Scraper");
builder.Services.AddIXONClient();

var secureEdgeProBaseAddress = builder.Configuration["SECUREEDGEPRO_BaseAddress"]
                               ?? throw new InvalidOperationException("SECUREEDGEPRO_BaseAddress is not configured. " +
                                                                      "Set it in appsettings.Development.json for development or as an environment variable in production.");
builder.Services.AddSecureEdgeProClient(secureEdgeProBaseAddress);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("LocalFrontend");
}

app.UseHttpsRedirection();
app.MapControllers();
await app.RunAsync();

