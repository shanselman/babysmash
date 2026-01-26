using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using BabySmash.Linux.Core.Interfaces;

namespace BabySmash.Linux.Platform;

public class LinuxSettingsService : ISettingsService
{
    private readonly string _settingsPath;
    private Dictionary<string, object> _settings = new();

    public LinuxSettingsService()
    {
        var configDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".config",
            "babysmash"
        );
        Directory.CreateDirectory(configDir);
        _settingsPath = Path.Combine(configDir, "settings.json");
        Load();
    }

    public T Get<T>(string key, T defaultValue = default!)
    {
        if (_settings.TryGetValue(key, out var value))
        {
            if (value is JsonElement jsonElement)
            {
                return jsonElement.Deserialize<T>() ?? defaultValue;
            }
            if (value is T typedValue)
            {
                return typedValue;
            }
        }
        return defaultValue;
    }

    public void Set<T>(string key, T value)
    {
        _settings[key] = value!;
    }

    public void Save()
    {
        try
        {
            var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsPath, json);
        }
        catch
        {
            // Silently fail
        }
    }

    public void Load()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = File.ReadAllText(_settingsPath);
                _settings = JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new();
            }
        }
        catch
        {
            _settings = new();
        }
    }
}
