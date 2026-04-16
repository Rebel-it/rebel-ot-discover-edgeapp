using Microsoft.Extensions.Logging;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rebelit.OT.Discover.EdgeApp.Exporters;

internal sealed class CsvExporters(
    ILogger<CsvExporters> logger
    ) : ICsvExporters
{
    public async Task CreateTagCsvFileAsync(List<Tag> tags, string filePath)
    {
        logger.LogInformation("Creating tag CSV file at {FilePath} with {Count} tags.", filePath, tags.Count);

        var csv = new StringBuilder();

        // Write header matching the IXON tag CSV format
        csv.AppendLine("Identifier,Address,Name,Logging interval,Retention policy,Edge aggregator,On change expiry,Log event,Trigger address,Trigger condition,Trigger threshold");

        // Write data rows
        foreach (var tag in tags)
        {
            var identifier = EscapeCsvField(tag.Slug ?? "");
            var address = EscapeCsvField(tag.Variable?.Address ?? "");
            var name = EscapeCsvField(tag.Name ?? "");
            var loggingInterval = EscapeCsvField(tag.LoggingInterval ?? "");
            var retentionPolicy = EscapeCsvField(tag.RetentionPolicy ?? "");
            var edgeAggregator = EscapeCsvField(tag.EdgeAggregator?.ToString() ?? "");
            var onChangeExpiry = EscapeCsvField(tag.OnChangeExpiry?.ToString() ?? "");
            var logEvent = EscapeCsvField(tag.LogEvent ?? "");

            // LogTrigger is an object, so we'll need to handle it specially
            // For now, leaving empty as it's typically null
            var triggerAddress = "";
            var triggerCondition = "";
            var triggerThreshold = "";

            csv.AppendLine($"{identifier},{address},{name},{loggingInterval},{retentionPolicy},{edgeAggregator},{onChangeExpiry},{logEvent},{triggerAddress},{triggerCondition},{triggerThreshold}");
        }

        await File.WriteAllTextAsync(filePath, csv.ToString());

        logger.LogInformation("Tag CSV file created successfully at {FilePath}.", filePath);
    }

    public async Task CreateVariableCsvFileAsync(List<Variable> variables, string filePath)
    {
        logger.LogInformation("Creating variable CSV file at {FilePath} with {Count} variables.", filePath, variables.Count);

        var csv = new StringBuilder();

        csv.AppendLine("Identifier,Name,Address,Type,Width,Signed,Max string length");

        foreach (var variable in variables)
        {
            var identifier = EscapeCsvField(variable.Slug ?? "");
            var name = EscapeCsvField(variable.Name ?? "");
            var address = EscapeCsvField(variable.Address ?? "");
            var type = EscapeCsvField(variable.Type ?? "");
            var width = variable.Width ?? "";
            var signed = variable.Signed?.ToString() ?? "";
            var maxStringLength = variable.MaxStringLength?.ToString() ?? "";

            csv.AppendLine($"{identifier},{name},{address},{type},{width},{signed},{maxStringLength}");
        }

        await File.WriteAllTextAsync(filePath, csv.ToString());

        logger.LogInformation("Variable CSV file created successfully at {FilePath}.", filePath);
    }

    /// <summary>
    /// Escapes CSV fields that contain commas, quotes, or newlines
    /// </summary>
    private static string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return field;

        // If field contains comma, quote, or newline, wrap in quotes and escape existing quotes
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }

        return field;
    }
}
