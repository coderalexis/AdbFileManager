namespace AdbFileManager.Core.Settings;

public sealed class SettingsService
{
    private readonly ISettingsStore store;
    public Settings Current
    {
        get;
    }
    public string? LoadWarning
    {
        get;
    }
    public SettingsService(ISettingsStore store)
    {
        this.store = store;
        var loaded = store.Load();
        Current = loaded.Value;
        LoadWarning = loaded.Warning;
        SettingsValidator.Normalize(Current);
    }
    public void Save()
    {
        SettingsValidator.Normalize(Current);
        store.Save(Current);
    }
}
