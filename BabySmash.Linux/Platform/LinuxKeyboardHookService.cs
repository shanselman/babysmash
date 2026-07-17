using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BabySmash.Linux.Core.Interfaces;

namespace BabySmash.Linux.Platform;

public sealed class LinuxKeyboardHookService : IKeyboardHookService
{
    private const int GSettingsTimeoutMilliseconds = 1000;
    private const int ShutdownWaitMilliseconds = 5000;

    private static readonly GSettingsKey[] ScreenshotShortcutKeys =
    [
        new("org.cinnamon.desktop.keybindings.media-keys", "screenshot"),
        new("org.gnome.settings-daemon.plugins.media-keys", "screenshot")
    ];

    private readonly object _operationLock = new();
    private readonly string _shortcutBackupPath;
    private Task _shortcutOperation = Task.CompletedTask;
    private int _activeWindows;
    private volatile bool _shutdownRequested;

    public LinuxKeyboardHookService()
    {
        var configDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".config",
            "babysmash");
        Directory.CreateDirectory(configDirectory);
        _shortcutBackupPath = Path.Combine(configDirectory, "screenshot-shortcuts.json");
    }

    public event EventHandler<KeyboardHookEventArgs> KeyPressed;

    public bool IsActive { get; private set; }

    public bool Start()
    {
        IsActive = true;
        return true;
    }

    public void DisableSystemScreenshotShortcut()
    {
        Start();

        lock (_operationLock)
        {
            if (_activeWindows++ == 0)
            {
                QueueShortcutOperation(DisableDesktopScreenshotShortcuts);
            }
        }
    }

    public void RestoreSystemScreenshotShortcut()
    {
        lock (_operationLock)
        {
            if (_activeWindows > 0)
            {
                _activeWindows--;
            }

            if (_activeWindows == 0)
            {
                QueueShortcutOperation(() => RestoreDesktopScreenshotShortcuts());
            }
        }
    }

    public void Stop()
    {
        Task shortcutOperation;

        lock (_operationLock)
        {
            _shutdownRequested = true;
            _activeWindows = 0;
            QueueShortcutOperation(() => RestoreDesktopScreenshotShortcuts());
            shortcutOperation = _shortcutOperation;
            IsActive = false;
        }

        try
        {
            if (!shortcutOperation.Wait(ShutdownWaitMilliseconds))
            {
                Console.Error.WriteLine("Timed out while restoring screenshot shortcuts.");
            }
        }
        catch (AggregateException exception)
        {
            Console.Error.WriteLine($"Could not restore screenshot shortcuts during shutdown: {exception.GetBaseException().Message}");
        }
    }

    public void SimulateKeyPress(char character)
    {
        KeyPressed?.Invoke(this, new KeyboardHookEventArgs
        {
            Character = character,
            VirtualKeyCode = character
        });
    }

    private void DisableDesktopScreenshotShortcuts()
    {
        if (!RestoreDesktopScreenshotShortcuts())
        {
            Console.Error.WriteLine("Could not restore the previous screenshot shortcut backup; shortcuts were not changed.");
            return;
        }

        var shortcutsToDisable = new List<GSettingsBackup>();

        foreach (var shortcut in ScreenshotShortcutKeys)
        {
            if (_shutdownRequested)
            {
                return;
            }

            var currentValue = RunGSettings("get", shortcut.Schema, shortcut.Name);
            if (string.IsNullOrWhiteSpace(currentValue) || currentValue.Trim() is "@as []" or "[]")
            {
                continue;
            }

            shortcutsToDisable.Add(new GSettingsBackup(shortcut.Schema, shortcut.Name, currentValue.Trim()));
        }

        if (shortcutsToDisable.Count == 0 || !SaveShortcutBackup(shortcutsToDisable))
        {
            return;
        }

        if (_shutdownRequested)
        {
            RestoreDesktopScreenshotShortcuts();
            return;
        }

        foreach (var shortcut in shortcutsToDisable)
        {
            if (_shutdownRequested)
            {
                RestoreDesktopScreenshotShortcuts();
                return;
            }

            if (RunGSettings("set", shortcut.Schema, shortcut.Name, "[]") is null)
            {
                Console.Error.WriteLine($"Could not disable screenshot shortcut {shortcut.Schema}/{shortcut.Name}.");
                RestoreDesktopScreenshotShortcuts();
                return;
            }
        }
    }

    private bool RestoreDesktopScreenshotShortcuts()
    {
        if (!File.Exists(_shortcutBackupPath))
        {
            return true;
        }

        try
        {
            var json = File.ReadAllText(_shortcutBackupPath);
            var shortcuts = JsonSerializer.Deserialize<List<GSettingsBackup>>(json);
            if (shortcuts is null)
            {
                Console.Error.WriteLine("Screenshot shortcut backup is invalid; leaving it in place for recovery.");
                return false;
            }

            var shortcutsNotRestored = new List<GSettingsBackup>();

            foreach (var shortcut in shortcuts)
            {
                if (RunGSettings("set", shortcut.Schema, shortcut.Name, shortcut.Value) is null)
                {
                    Console.Error.WriteLine($"Could not restore screenshot shortcut {shortcut.Schema}/{shortcut.Name}.");
                    shortcutsNotRestored.Add(shortcut);
                }
            }

            if (shortcutsNotRestored.Count > 0)
            {
                SaveShortcutBackup(shortcutsNotRestored);
                return false;
            }

            File.Delete(_shortcutBackupPath);
            return true;
        }
        catch (IOException exception)
        {
            Console.Error.WriteLine($"Could not restore screenshot shortcuts: {exception.Message}");
            return false;
        }
        catch (UnauthorizedAccessException exception)
        {
            Console.Error.WriteLine($"Could not restore screenshot shortcuts: {exception.Message}");
            return false;
        }
        catch (JsonException exception)
        {
            Console.Error.WriteLine($"Could not read screenshot shortcut backup: {exception.Message}");
            return false;
        }
    }

    private bool SaveShortcutBackup(List<GSettingsBackup> shortcuts)
    {
        var temporaryPath = $"{_shortcutBackupPath}.tmp";

        try
        {
            var json = JsonSerializer.Serialize(shortcuts, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(temporaryPath, json);
            File.Move(temporaryPath, _shortcutBackupPath, true);
            return true;
        }
        catch (IOException exception)
        {
            Console.Error.WriteLine($"Could not save screenshot shortcut backup: {exception.Message}");
            return false;
        }
        catch (UnauthorizedAccessException exception)
        {
            Console.Error.WriteLine($"Could not save screenshot shortcut backup: {exception.Message}");
            return false;
        }
    }

    private void QueueShortcutOperation(Action operation)
    {
        _shortcutOperation = _shortcutOperation.ContinueWith(
            _ => operation(),
            CancellationToken.None,
            TaskContinuationOptions.None,
            TaskScheduler.Default);
    }

    private static string? RunGSettings(params string[] arguments)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "gsettings",
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            foreach (var argument in arguments)
            {
                startInfo.ArgumentList.Add(argument);
            }

            using var process = Process.Start(startInfo);
            if (process is null)
            {
                return null;
            }

            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            if (!process.WaitForExit(GSettingsTimeoutMilliseconds))
            {
                process.Kill(entireProcessTree: true);
                process.WaitForExit(GSettingsTimeoutMilliseconds);
                Console.Error.WriteLine($"gsettings timed out: {string.Join(' ', arguments)}");
                return null;
            }

            if (!Task.WaitAll([outputTask, errorTask], GSettingsTimeoutMilliseconds))
            {
                Console.Error.WriteLine($"Timed out while reading gsettings output: {string.Join(' ', arguments)}");
                return null;
            }

            var output = outputTask.GetAwaiter().GetResult();
            var error = errorTask.GetAwaiter().GetResult();
            if (process.ExitCode != 0)
            {
                if (!string.IsNullOrWhiteSpace(error))
                {
                    Console.Error.WriteLine(error.Trim());
                }
                return null;
            }

            return output;
        }
        catch (Win32Exception exception)
        {
            Console.Error.WriteLine($"Could not run gsettings: {exception.Message}");
            return null;
        }
        catch (InvalidOperationException exception)
        {
            Console.Error.WriteLine($"Could not run gsettings: {exception.Message}");
            return null;
        }
        catch (AggregateException exception)
        {
            Console.Error.WriteLine($"Could not read gsettings output: {exception.GetBaseException().Message}");
            return null;
        }
    }

    private readonly record struct GSettingsKey(string Schema, string Name);
    private sealed record GSettingsBackup(string Schema, string Name, string Value);
}
