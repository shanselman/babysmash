namespace BabySmash.Core.Interfaces;

public interface IAudioService
{
    void PlaySound(string resourceName);
    void StopAll();
}
