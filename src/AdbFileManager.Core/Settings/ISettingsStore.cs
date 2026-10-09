namespace AdbFileManager.Core.Settings;

public sealed record SettingsLoadResult(Settings Value, string? Warning);
public interface ISettingsStore
{
    SettingsLoadResult Load();
    void Save(Settings value);
}
