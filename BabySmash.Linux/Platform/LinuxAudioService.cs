using System;
using BabySmash.Core.Interfaces;

namespace BabySmash.Linux.Platform;

public class LinuxAudioService : IAudioService
{
    public void PlaySound(string resourceName)
    {
        // TODO: Implement audio playback using NAudio or similar
        Console.WriteLine($"Would play sound: {resourceName}");
    }

    public void StopAll()
    {
        // TODO: Implement stop all sounds
    }
}
