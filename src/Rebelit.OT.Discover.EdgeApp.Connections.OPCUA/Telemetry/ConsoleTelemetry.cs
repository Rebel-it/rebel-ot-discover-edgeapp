using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Opc.Ua;
using Serilog;
using Serilog.Events;
using Serilog.Templates;

namespace Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Telemetry;

public sealed class ConsoleTelemetry : ITelemetryContext, IDisposable
{
    private const string SourceName = "Quickstarts";
    private const string SourceVersion = "1.0.0";

    private readonly Action<ILoggingBuilder> _configure;
    private readonly Meter _meter;

    public ConsoleTelemetry(Action<ILoggingBuilder> configure = null)
    {
        _configure = configure;

        LoggerFactory = Microsoft
            .Extensions.Logging.LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Information);
                _configure?.Invoke(builder);
            })
            .AddSerilog(Log.Logger);

        ActivitySource = new ActivitySource(SourceName, SourceVersion);
        _meter = new Meter(SourceName, SourceVersion);
        _logger = LoggerFactory.CreateLogger("Main");

        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException += Unobserved_TaskException;
    }

    /// <inheritdoc/>
    public ILoggerFactory LoggerFactory { get; internal set; }

    /// <inheritdoc/>
    public Meter CreateMeter() => _meter;

    /// <inheritdoc/>
    public ActivitySource ActivitySource { get; }

    /// <inheritdoc/>
    public void Dispose()
    {
        _meter.Dispose();
        ActivitySource.Dispose();
        LoggerFactory.Dispose();

        AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException -= Unobserved_TaskException;
    }

    /// <summary>
    /// Configure the logging providers.
    /// </summary>
    /// <remarks>
    /// Replaces the Opc.Ua.Core default ILogger with a
    /// Microsoft.Extension.Logger with a Serilog file, debug and console logger.
    /// The debug logger is only enabled for debug builds.
    /// The console logger is enabled by the logConsole flag at the consoleLogLevel.
    /// The file logger uses the setting in the ApplicationConfiguration.
    /// The Trace logLevel is chosen if required by the Tracemasks.
    /// </remarks>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="context">The context name for the logger. </param>
    /// <param name="logConsole">Enable logging to the console.</param>
    /// <param name="logFile">Enable logging to a file.</param>
    /// <param name="logApp">Enable application logging.</param>
    /// <param name="consoleLogLevel">The LogLevel to use for the console/debug.</param>
    public void ConfigureLogging(
        ApplicationConfiguration configuration,
        string context,
        bool logConsole,
        bool logFile,
        bool logApp,
        LogLevel consoleLogLevel
    )
    {
        if (!logApp)
        {
            return;
        }

        LoggerConfiguration loggerConfiguration = new LoggerConfiguration().Enrich.FromLogContext();

        if (logConsole)
        {
            loggerConfiguration.WriteTo.Console(
                restrictedToMinimumLevel: (LogEventLevel)consoleLogLevel,
                formatProvider: CultureInfo.InvariantCulture
            );
        }
#if DEBUG
        else
        {
            loggerConfiguration.WriteTo.Debug(
                restrictedToMinimumLevel: (LogEventLevel)consoleLogLevel,
                formatProvider: CultureInfo.InvariantCulture
            );
        }
#endif
        const LogLevel fileLevel = LogLevel.Information;

        // add file logging if configured
        if (logFile)
        {
            string outputFilePath = configuration.TraceConfiguration.OutputFilePath;
            if (!string.IsNullOrWhiteSpace(outputFilePath))
            {
                loggerConfiguration.WriteTo.File(
                    new ExpressionTemplate(
                        "{UtcDateTime(@t):yyyy-MM-dd HH:mm:ss.fff} [{@l:u3}] {@m}\n{@x}"
                    ),
                    Utils.ReplaceSpecialFolderNames(outputFilePath),
                    restrictedToMinimumLevel: (LogEventLevel)fileLevel,
                    rollOnFileSizeLimit: true
                );
            }
        }

        // adjust minimum level
        if (fileLevel < LogLevel.Information || consoleLogLevel < LogLevel.Information)
        {
            loggerConfiguration.MinimumLevel.Verbose();
        }

        // create the serilog logger
        Serilog.Core.Logger serilogger = loggerConfiguration.CreateLogger();

        // Dispose the old LoggerFactory and create a new one with the updated configuration
        ILoggerFactory oldLoggerFactory = LoggerFactory;
        LoggerFactory = Microsoft
            .Extensions.Logging.LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(consoleLogLevel);
                _configure?.Invoke(builder);
            })
            .AddSerilog(serilogger);
        _logger = LoggerFactory.CreateLogger("Main");

        oldLoggerFactory.Dispose();
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
    {
        _logger.LogCritical(
            args.ExceptionObject as Exception,
            "Unhandled Exception: (IsTerminating: {IsTerminating})",
            args.IsTerminating
        );
    }

    private void Unobserved_TaskException(object sender, UnobservedTaskExceptionEventArgs args)
    {
        _logger.LogCritical(
            args.Exception,
            "Unobserved Task Exception (Observed: {Observed})",
            args.Observed
        );
    }

    private Microsoft.Extensions.Logging.ILogger _logger;
}
