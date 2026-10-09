namespace AdbFileManager;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.SetHighDpiMode(HighDpiMode.DpiUnawareGdiScaled);
        ApplicationConfiguration.Initialize();
        string profile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "tobiksoft", "AdbFileManager");
        var settings = new SettingsService(new SettingsStore(Path.Combine(profile, "settings.xml")));
        LocalizationText.ApplyLanguage(settings.Current.Language);
        if (settings.Current.DarkMode)
            DarkModeStartup.Initialize();
        var theme = new AppTheme(settings.Current.DarkMode);
        var adb = new AdbClient(Path.Combine(AppContext.BaseDirectory, "adb.exe")) { ProgressIntervalMs = settings.Current.ProgressIntervalMs };
        var session = new DeviceSession();
        using var icons = new IconProvider(AppContext.BaseDirectory, settings.Current.UseWindows11Icons);
        using var preview = new MediaPreviewService(adb);
        using var main = new MainForm(adb, new AndroidBrowser(adb), new AdbTransferBackend(adb), new QueueStore(Path.Combine(profile, "transfers.json")),
            settings, session, theme, icons, preview);
        if (settings.LoadWarning != null)
            main.Shown += (_, _) => MessageBox.Show(main,
            LocalizationText.Get("settings_recovered") + Environment.NewLine + settings.LoadWarning,
            strings.settings_title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        Application.Run(main);
    }
}
