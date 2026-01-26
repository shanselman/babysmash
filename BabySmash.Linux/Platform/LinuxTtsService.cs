using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using BabySmash.Linux.Core.Interfaces;

namespace BabySmash.Linux.Platform;

public class LinuxTtsService : ITtsService, IDisposable
{
    private bool _warningShown = false;
    private bool _espeakAvailable = true;
    private string? _currentLanguage;
    private readonly List<Process> _runningProcesses = new();
    private readonly object _processLock = new();

    public LinuxTtsService()
    {
        // Check if espeak is available
        _espeakAvailable = CheckEspeakAvailable();
    }

    private bool CheckEspeakAvailable()
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "which",
                    Arguments = "espeak",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                }
            };
            process.Start();
            process.WaitForExit(1000);
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public void Speak(string text)
    {
        if (!_espeakAvailable)
        {
            if (!_warningShown)
            {
                Console.WriteLine();
                Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║  WARNING: Text-to-speech is not available                    ║");
                Console.WriteLine("║                                                              ║");
                Console.WriteLine("║  Install espeak for speech support:                          ║");
                Console.WriteLine("║    Ubuntu/Debian: sudo apt install espeak                    ║");
                Console.WriteLine("║    Fedora:        sudo dnf install espeak                    ║");
                Console.WriteLine("║    Arch:          sudo pacman -S espeak                      ║");
                Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
                Console.WriteLine();
                _warningShown = true;
            }
            return;
        }

        try
        {
            // Cancel any current speech first
            CancelSpeech();
            
            var args = $"\"{text}\"";
            if (!string.IsNullOrEmpty(_currentLanguage))
            {
                args = $"-v {_currentLanguage} {args}";
            }
            
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "espeak",
                    Arguments = args,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true
                },
                EnableRaisingEvents = true
            };
            
            process.Exited += (s, e) =>
            {
                lock (_processLock)
                {
                    _runningProcesses.Remove(process);
                }
                process.Dispose();
            };
            
            lock (_processLock)
            {
                _runningProcesses.Add(process);
            }
            
            process.Start();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"TTS error: {ex.Message}");
        }
    }

    public bool TrySetLanguage(CultureInfo culture)
    {
        // Map common cultures to espeak language codes
        _currentLanguage = culture.TwoLetterISOLanguageName switch
        {
            "en" => "en",
            "es" => "es",
            "fr" => "fr",
            "de" => "de",
            "pt" => "pt",
            "ru" => "ru",
            "el" => "el",
            "lv" => "lv",
            _ => "en"
        };
        return true;
    }

    public void CancelSpeech()
    {
        lock (_processLock)
        {
            foreach (var process in _runningProcesses.ToArray())
            {
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill();
                    }
                    process.Dispose();
                }
                catch
                {
                    // Ignore errors
                }
            }
            _runningProcesses.Clear();
        }
    }

    public void Dispose()
    {
        CancelSpeech();
    }
}
