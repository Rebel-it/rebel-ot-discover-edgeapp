using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rebelit.OT.Discover.EdgeApp.Tests.Services.Helpers;

public class TestLogger<T>(bool isInfoEnabled) : ILogger<T>
{
    public List<LogEntry> Entries { get; } = [];

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.Information || isInfoEnabled;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        Entries.Add(new LogEntry(logLevel, formatter(state, exception), state?.ToString() ?? string.Empty));
    }

    public sealed record LogEntry(LogLevel LogLevel, string Message, string StateText);

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();
        public void Dispose() { }
    }
}
