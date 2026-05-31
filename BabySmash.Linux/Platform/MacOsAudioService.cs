using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using BabySmash.Linux.Core.Interfaces;

namespace BabySmash.Linux.Platform;

public class MacOsAudioService : IAudioService, IDisposable
{
    private const string AfplayPath = "/usr/bin/afplay";
    private const int MaxConcurrentSounds = 8;
    private readonly string _tempDir;
    private readonly List<Process> _runningProcesses = new();
    private readonly object _processLock = new();
    private bool _warningShown;
    private readonly bool _audioAvailable;

    public MacOsAudioService()
    {
        _tempDir = Path.Combine(GetHomeDirectory(), "Library", "Caches", "BabySmash", "Sounds");
        Directory.CreateDirectory(_tempDir);
        _audioAvailable = File.Exists(AfplayPath);
    }

    public void PlaySound(string resourceName)
    {
        if (!_audioAvailable)
        {
            ShowAudioWarningOnce();
            return;
        }

        try
        {
            var soundFile = ExtractSoundResource(resourceName);
            if (soundFile == null) return;

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = AfplayPath,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true
                },
                EnableRaisingEvents = true
            };
            process.StartInfo.ArgumentList.Add(soundFile);

            process.Exited += (s, e) =>
            {
                lock (_processLock)
                {
                    _runningProcesses.Remove(process);
                }
                process.Dispose();
            };

            if (!TryStartSoundProcess(process))
            {
                process.Dispose();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Audio error: {ex.Message}");
        }
    }

    private bool TryStartSoundProcess(Process process)
    {
        lock (_processLock)
        {
            RemoveExitedProcesses();
            if (_runningProcesses.Count >= MaxConcurrentSounds)
            {
                return false;
            }

            _runningProcesses.Add(process);

            try
            {
                if (process.Start())
                {
                    return true;
                }
            }
            catch
            {
                _runningProcesses.Remove(process);
                process.Dispose();
                throw;
            }

            _runningProcesses.Remove(process);
            return false;
        }
    }

    private void RemoveExitedProcesses()
    {
        for (var i = _runningProcesses.Count - 1; i >= 0; i--)
        {
            var process = _runningProcesses[i];
            try
            {
                if (!process.HasExited)
                {
                    continue;
                }
            }
            catch
            {
                // Treat inaccessible process handles as no longer usable.
            }

            _runningProcesses.RemoveAt(i);
            process.Dispose();
        }
    }

    private string? ExtractSoundResource(string resourceName)
    {
        var outputPath = Path.Combine(_tempDir, resourceName);
        if (File.Exists(outputPath))
        {
            return outputPath;
        }

        var assembly = typeof(BabySmash.Linux.Core.Services.WordFinder).Assembly;
        var resourceFullName = $"BabySmash.Linux.Core.Resources.Sounds.{resourceName}";

        using var stream = assembly.GetManifestResourceStream(resourceFullName);
        if (stream == null)
        {
            var names = assembly.GetManifestResourceNames();
            var match = Array.Find(names, n => n.EndsWith(resourceName, StringComparison.OrdinalIgnoreCase));
            if (match == null) return null;

            using var matchStream = assembly.GetManifestResourceStream(match);
            if (matchStream == null) return null;

            using var matchFileStream = File.Create(outputPath);
            matchStream.CopyTo(matchFileStream);
            return outputPath;
        }

        using var fileStream = File.Create(outputPath);
        stream.CopyTo(fileStream);
        return outputPath;
    }

    public void StopAll()
    {
        lock (_processLock)
        {
            foreach (var process in _runningProcesses.ToArray())
            {
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill(entireProcessTree: true);
                    }
                    process.Dispose();
                }
                catch
                {
                    // Ignore shutdown races.
                }
            }
            _runningProcesses.Clear();
        }
    }

    public void Cleanup()
    {
        try
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, true);
            }
        }
        catch
        {
            // Ignore cleanup errors.
        }
    }

    public void Dispose()
    {
        StopAll();
        Cleanup();
    }

    private void ShowAudioWarningOnce()
    {
        if (_warningShown) return;

        Console.WriteLine();
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  WARNING: macOS audio playback is not available              ║");
        Console.WriteLine("║                                                              ║");
        Console.WriteLine("║  BabySmash expects /usr/bin/afplay for sound playback.       ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.WriteLine();
        _warningShown = true;
    }

    private static string GetHomeDirectory()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return string.IsNullOrWhiteSpace(home) ? Environment.GetEnvironmentVariable("HOME") ?? "." : home;
    }
}
