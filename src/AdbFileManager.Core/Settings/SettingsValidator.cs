namespace AdbFileManager.Core.Settings;

public static class SettingsValidator
{
    public static void Normalize(Settings value)
    {
        if (value.ButtonStyle is < 0 or > 2)
            value.ButtonStyle = 0;
        if (value.Language > 7)
            value.Language = null;
        value.ProgressIntervalMs = 20 + (Math.Clamp(value.ProgressIntervalMs, 20, 520) - 20) / 10 * 10;
        value.LastDirectory ??= "";
    }
}
