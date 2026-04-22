using System.Text.Json;

namespace Rebelit.OT.Discover.EdgeApp.Services;

public interface ISettingsManager
{
    void Save(Dictionary<string, string?> values);
    Dictionary<string, string?> Load();
}

internal sealed class SettingsManager(IConfiguration configuration, ILogger<SettingsManager> logger) : ISettingsManager
{
    private static readonly string SettingsFilePath = Path.Combine(
        AppContext.BaseDirectory, "settings.json"
    );

    public void Save(Dictionary<string, string?> values)
    {
        // Merge with existing settings so steps don't overwrite each other
        var existing = Load();
        foreach (var kvp in values)
            existing[kvp.Key] = kvp.Value;

        File.WriteAllText(
            SettingsFilePath,
            JsonSerializer.Serialize(existing, new JsonSerializerOptions { WriteIndented = true })
        );

        (configuration as IConfigurationRoot)?.Reload();
        logger.LogInformation("Settings saved to {Path}", SettingsFilePath);
    }

    public Dictionary<string, string?> Load()
    {
        if (!File.Exists(SettingsFilePath))
            return [];

        try
        {
            var json = File.ReadAllText(SettingsFilePath);
            return JsonSerializer.Deserialize<Dictionary<string, string?>>(json) ?? [];
        }
        catch
        {
            return [];
        }
    }
}