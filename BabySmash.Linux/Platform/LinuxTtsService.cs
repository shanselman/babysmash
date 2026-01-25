using System;
using System.Diagnostics;
using System.Globalization;
using BabySmash.Core.Interfaces;

namespace BabySmash.Linux.Platform;

public class LinuxTtsService : ITtsService
{
    private bool _warningShown = false;
    private bool _espeakAvailable = true;
    private string? _currentLanguage;

    public LinuxTtsService()
    {
        // Check if espeak is available
        _espeakAvailable = CheckEspeakAvailable();
    }

    private bool CheckEspeakAvailable()
    {
        try
        {
            var process = new Process
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
                }
            };
            process.Start();
            // Don't wait - let it play asynchronously
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
        try
        {
            // Kill any running espeak processes
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "pkill",
                    Arguments = "-9 espeak",
                    CreateNoWindow = true,
                    UseShellExecute = false
                }
            };
            process.Start();
        }
        catch
        {
            // Ignore errors
        }
    }
}
