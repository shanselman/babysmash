using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using BabySmash.Linux.Core.Interfaces;

namespace BabySmash.Linux.Platform;

public sealed class LinuxKeyboardHookService : IKeyboardHookService
{
    private static readonly GSettingsKey[] ScreenshotShortcutKeys =
    [
        new("org.cinnamon.desktop.keybindings.media-keys", "screenshot"),
        new("org.gnome.settings-daemon.plugins.media-keys", "screenshot")
    ];

    private readonly Dictionary<GSettingsKey, string> _disabledShortcuts = new();
    private int _activeWindows;

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

        if (_activeWindows++ == 0)
        {
            DisableDesktopScreenshotShortcuts();
        }
    }

    public void RestoreSystemScreenshotShortcut()
    {
        if (_activeWindows > 0)
        {
            _activeWindows--;
        }

        if (_activeWindows == 0)
        {
            RestoreDesktopScreenshotShortcuts();
        }
    }

    public void Stop()
    {
        _activeWindows = 0;
        RestoreDesktopScreenshotShortcuts();
        IsActive = false;
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
        foreach (var shortcut in ScreenshotShortcutKeys)
        {
            if (_disabledShortcuts.ContainsKey(shortcut) || !GSettingsKeyExists(shortcut))
            {
                continue;
            }

            var currentValue = RunGSettings("get", shortcut.Schema, shortcut.Name);
            if (string.IsNullOrWhiteSpace(currentValue) || currentValue.Trim() is "@as []" or "[]")
            {
                continue;
            }

            if (RunGSettings("set", shortcut.Schema, shortcut.Name, "[]") is not null)
            {
                _disabledShortcuts[shortcut] = currentValue.Trim();
            }
        }
    }

    private void RestoreDesktopScreenshotShortcuts()
    {
        foreach (var shortcut in _disabledShortcuts)
        {
            RunGSettings("set", shortcut.Key.Schema, shortcut.Key.Name, shortcut.Value);
        }

        _disabledShortcuts.Clear();
    }

    private static bool GSettingsKeyExists(GSettingsKey shortcut)
    {
        var keys = RunGSettings("list-keys", shortcut.Schema);
        return keys?.Split('\n').Contains(shortcut.Name) == true;
    }

    private static string? RunGSettings(params string[] arguments)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "gsettings",
                RedirectStandardError = true,
                RedirectStandardOutput = true
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

            process.WaitForExit();
            return process.ExitCode == 0 ? process.StandardOutput.ReadToEnd() : null;
        }
        catch (Win32Exception)
        {
            return null;
        }
    }

    private readonly record struct GSettingsKey(string Schema, string Name);
}
