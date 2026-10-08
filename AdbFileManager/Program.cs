namespace AdbFileManager {
    internal static class Program {
        [STAThread]
        static void Main() {
            Directory.SetCurrentDirectory(AppContext.BaseDirectory);
            SettingsManager.LoadSettings();
            if (SettingsManager.settings.DarkMode) DarkModeStartup.Initialize();
            Application.SetHighDpiMode(HighDpiMode.DpiUnawareGdiScaled);
            ApplicationConfiguration.Initialize();
            using var main = new Form1();
            if (SettingsManager.LoadWarning != null) main.Shown += (_, _) => MessageBox.Show(main,
                strings.ResourceManager.GetString("settings_recovered") + Environment.NewLine + SettingsManager.LoadWarning,
                strings.settings_title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            Application.Run(main);
        }
    }
}
