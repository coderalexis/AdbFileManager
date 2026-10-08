using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdbFileManager {
	public class Settings {
		//Appearance
		public int ButtonTheme { get; set; } = 0; // 0 = Flat shaded, 1 = Flat, 2 = Fluent gradient
		public bool UseWindows11Icons { get; set; } = true; //true = W11 icons, false = W7 icons
		public bool DarkMode { get; set; } = false; //true = Dark mode, false = Light mode

		public bool ShowTwoProgressBars = true;
		public bool ShowAndroidBackButton { get; set; } = true;

		public ushort? lang = null;
		public int progressWaitTimeMs = 60;


		//Behaviour
		public bool useLegacyCopy { get; set; } = false;
		public bool unwrapFilesLegacy { get; set; } = false;
		public bool useFastCompatibility { get; set; } = false;
		public bool useCompatibilityMode { get; set; } = false;
		public bool previewMediaFiles { get; set; } = false;
		public bool keepFileModificationDate { get; set; } = true;

		public bool rememberDirectory { get; set; } = false;
		public string lastDirectory { get; set; } = "";
    }
	public static class SettingsManager {
		public static Settings settings = new Settings();
        private static readonly SettingsStore store = new(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "tobiksoft", "AdbFileManager", "settings.xml"));
        public static string? LoadWarning { get; private set; }

        public static void SaveSettings() {
            store.Save(settings);
		}
		public static void LoadSettings() {
            SettingsLoadResult loaded = store.Load();
            settings = loaded.Value;
            LoadWarning = loaded.Warning;
			ApplySettings();
		}

		public static void ApplySettings() {
			AdbProgressRunner.ProgressIntervalMs = settings.progressWaitTimeMs;
		}
	}
}
