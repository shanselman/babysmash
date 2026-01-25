namespace BabySmash.Core.Interfaces;

public interface ISettingsService
{
    T Get<T>(string key, T defaultValue = default!);
    void Set<T>(string key, T value);
    void Save();
    void Load();
}
