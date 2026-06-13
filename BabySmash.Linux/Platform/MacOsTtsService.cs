using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BabySmash.Linux.Core.Interfaces;

namespace BabySmash.Linux.Platform;

public class MacOsTtsService : ITtsService, IDisposable
{
    private const string SayPath = "/usr/bin/say";
    private readonly List<Process> _runningProcesses = new();
    private readonly object _processLock = new();
    private readonly List<SayVoice> _voices;
    private readonly bool _sayAvailable;
    private bool _warningShown;
    private string? _currentVoice;

    public MacOsTtsService()
    {
        _sayAvailable = File.Exists(SayPath);
        _voices = _sayAvailable ? ProbeVoices() : new List<SayVoice>();
    }

    public void Speak(string text)
    {
        if (!_sayAvailable)
        {
            ShowTtsWarningOnce();
            return;
        }

        try
        {
            CancelSpeech();

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = SayPath,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true
                },
                EnableRaisingEvents = true
            };

            if (!string.IsNullOrWhiteSpace(_currentVoice))
            {
                process.StartInfo.ArgumentList.Add("-v");
                process.StartInfo.ArgumentList.Add(_currentVoice);
            }
            process.StartInfo.ArgumentList.Add(text);

            process.Exited += (s, e) =>
            {
                lock (_processLock)
                {
                    _runningProcesses.Remove(process);
                }
                process.Dispose();
            };

            if (!TryStartSpeechProcess(process))
            {
                process.Dispose();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"TTS error: {ex.Message}");
        }
    }

    private bool TryStartSpeechProcess(Process process)
    {
        lock (_processLock)
        {
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

    public bool TrySetLanguage(CultureInfo culture)
    {
        if (!_sayAvailable)
        {
            _currentVoice = null;
            return false;
        }

        var normalizedName = NormalizeCulture(culture.Name);
        var languageName = NormalizeCulture(culture.TwoLetterISOLanguageName);

        var voice = _voices.FirstOrDefault(v => v.NormalizedLocale == normalizedName)
            ?? _voices.FirstOrDefault(v => v.NormalizedLocale.StartsWith(languageName + "-", StringComparison.OrdinalIgnoreCase))
            ?? _voices.FirstOrDefault(v => v.NormalizedLocale.Equals(languageName, StringComparison.OrdinalIgnoreCase));

        _currentVoice = voice?.Name;
        return voice != null || _voices.Count == 0;
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

    public void Dispose()
    {
        CancelSpeech();
    }

    private static List<SayVoice> ProbeVoices()
    {
        var voices = new List<SayVoice>();

        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = SayPath,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            process.StartInfo.ArgumentList.Add("-v");
            process.StartInfo.ArgumentList.Add("?");

            process.Start();
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            if (!process.WaitForExit(2000))
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                }
                process.WaitForExit();
                Task.WaitAll(outputTask, errorTask);
                return voices;
            }

            Task.WaitAll(outputTask, errorTask);
            var output = outputTask.Result;
            foreach (var line in output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var match = Regex.Match(line, @"^(?<name>.+?)\s+(?<locale>[a-z]{2}[_-][A-Z]{2}|[a-z]{2})\s+#", RegexOptions.CultureInvariant);
                if (match.Success)
                {
                    voices.Add(new SayVoice(
                        match.Groups["name"].Value.Trim(),
                        NormalizeCulture(match.Groups["locale"].Value)));
                }
            }
        }
        catch
        {
            // Voice probing is best-effort; say can still use its default voice.
        }

        return voices;
    }

    private void ShowTtsWarningOnce()
    {
        if (_warningShown) return;

        Console.WriteLine();
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  WARNING: macOS text-to-speech is not available              ║");
        Console.WriteLine("║                                                              ║");
        Console.WriteLine("║  BabySmash expects /usr/bin/say for speech support.          ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.WriteLine();
        _warningShown = true;
    }

    private static string NormalizeCulture(string culture)
    {
        return culture.Replace('_', '-').ToLowerInvariant();
    }

    private sealed record SayVoice(string Name, string NormalizedLocale);
}
