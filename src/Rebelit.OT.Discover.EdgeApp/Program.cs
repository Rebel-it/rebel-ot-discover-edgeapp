﻿using Rebelit.OT.Discover.EdgeApp;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON;
using Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Extensions;
using Rebelit.OT.Discover.EdgeApp.Exporters;
using Rebelit.OT.Discover.EdgeApp.Filters;
using Rebelit.OT.Discover.EdgeApp.Mappers;
using Rebelit.OT.Discover.EdgeApp.Resolvers;
using Rebelit.OT.Discover.EdgeApp.Services;
using Rebelit.OT.Discover.EdgeApp.SharedKernel.IxonAuthentication;
using Rebelit.OT.Discover.EdgeApp.Synchronizers;
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
builder.Services.AddScoped<IIxonAuthenticationContext, IxonAuthenticationContext>();
builder.Services.AddScoped<AuthenticationFilter>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("LocalFrontend", policy =>
            policy.WithOrigins("http://localhost:5174")
                  .AllowAnyHeader()
                  .AllowAnyMethod());
    });
}

builder.Services.AddSingleton<ICsvExporters, CsvExporters>();
builder.Services.AddScoped<IScraper, Scraper>();
builder.Services.AddSingleton<IOpcUaVariableMapper, OpcUaVariableMapper>();
builder.Services.AddScoped<IDataSourceResolver, DataSourceResolver>();
builder.Services.AddScoped<INodeSynchronizer, NodeSynchronizer>();
builder.Services.AddScoped<ICompanyConfigurationService, CompanyConfigurationService>();
builder.Services.AddOPCUAClient("Rebelit.OT.Scraper");
builder.Services.AddIXONClient();

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

