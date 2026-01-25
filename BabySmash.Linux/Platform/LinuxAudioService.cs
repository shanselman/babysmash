using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using BabySmash.Core.Interfaces;

namespace BabySmash.Linux.Platform;

public class LinuxAudioService : IAudioService
{
    private bool _warningShown = false;
    private bool _audioAvailable = true;
    private string? _audioPlayer;
    private string _tempDir;

    public LinuxAudioService()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "babysmash-sounds");
        Directory.CreateDirectory(_tempDir);
        
        // Check which audio player is available
        _audioPlayer = FindAudioPlayer();
        _audioAvailable = _audioPlayer != null;
    }

    private string? FindAudioPlayer()
    {
        // Try common Linux audio players in order of preference
        string[] players = { "paplay", "aplay", "ffplay", "play" };
        
        foreach (var player in players)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "which",
                        Arguments = player,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true
                    }
                };
                process.Start();
                process.WaitForExit(1000);
                if (process.ExitCode == 0)
                {
                    return player;
                }
            }
            catch
            {
                continue;
            }
        }
        
        return null;
    }

    public void PlaySound(string resourceName)
    {
        if (!_audioAvailable)
        {
            if (!_warningShown)
            {
                Console.WriteLine();
                Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║  WARNING: Audio playback is not available                    ║");
                Console.WriteLine("║                                                              ║");
                Console.WriteLine("║  Install PulseAudio or ALSA utils for sound:                 ║");
                Console.WriteLine("║    Ubuntu/Debian: sudo apt install pulseaudio-utils          ║");
                Console.WriteLine("║    Fedora:        sudo dnf install pulseaudio-utils          ║");
                Console.WriteLine("║    Arch:          sudo pacman -S pulseaudio                  ║");
                Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
                Console.WriteLine();
                _warningShown = true;
            }
            return;
        }

        try
        {
            // Extract the sound from embedded resources if needed
            var soundFile = ExtractSoundResource(resourceName);
            if (soundFile == null) return;

            var args = _audioPlayer switch
            {
                "paplay" => soundFile,
                "aplay" => $"-q {soundFile}",
                "ffplay" => $"-nodisp -autoexit -loglevel quiet {soundFile}",
                "play" => $"-q {soundFile}",
                _ => soundFile
            };

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _audioPlayer!,
                    Arguments = args,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true
                }
            };
            process.Start();
            // Don't wait - let it play asynchronously
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Audio error: {ex.Message}");
        }
    }

    private string? ExtractSoundResource(string resourceName)
    {
        var outputPath = Path.Combine(_tempDir, resourceName);
        
        // If already extracted, just return the path
        if (File.Exists(outputPath))
        {
            return outputPath;
        }

        // Try to find the resource in Core assembly
        var coreAssembly = typeof(BabySmash.Core.Services.WordFinder).Assembly;
        var resourceFullName = $"BabySmash.Core.Resources.Sounds.{resourceName}";
        
        using var stream = coreAssembly.GetManifestResourceStream(resourceFullName);
        if (stream == null)
        {
            // Try without prefix
            var names = coreAssembly.GetManifestResourceNames();
            var match = Array.Find(names, n => n.EndsWith(resourceName, StringComparison.OrdinalIgnoreCase));
            if (match != null)
            {
                using var matchStream = coreAssembly.GetManifestResourceStream(match);
                if (matchStream != null)
                {
                    using var fileStream = File.Create(outputPath);
                    matchStream.CopyTo(fileStream);
                    return outputPath;
                }
            }
            return null;
        }

        using (var fileStream = File.Create(outputPath))
        {
            stream.CopyTo(fileStream);
        }
        
        return outputPath;
    }

    public void StopAll()
    {
        try
        {
            // Kill any running audio processes
            if (_audioPlayer != null)
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "pkill",
                        Arguments = $"-9 {_audioPlayer}",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    }
                };
                process.Start();
            }
        }
        catch
        {
            // Ignore errors
        }
    }
}
