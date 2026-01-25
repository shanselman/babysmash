using System;
using System.Diagnostics;
using System.Globalization;
using BabySmash.Core.Interfaces;

namespace BabySmash.Linux.Platform;

public class LinuxTtsService : ITtsService
{
    public void Speak(string text)
    {
        try
        {
            // Try espeak first
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "espeak",
                    Arguments = $"\"{text}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true
                }
            };
            process.Start();
            process.WaitForExit(100); // Don't wait too long
        }
        catch
        {
            // Silently fail if espeak not available
        }
    }

    public bool TrySetLanguage(CultureInfo culture)
    {
        // TODO: Implement language support for espeak
        return true;
    }

    public void CancelSpeech()
    {
        // TODO: Implement speech cancellation
    }
}
