using System;
namespace BabySmash.Linux.Core.Interfaces;

public interface IAudioService
{
    void PlaySound(string resourceName);
    void StopAll();
}
