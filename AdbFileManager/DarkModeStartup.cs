using System;
using System.Runtime.InteropServices;

namespace AdbFileManager {

	static class DarkModeStartup {
		[DllImport("dwmapi.dll")]
		private static extern int DwmSetWindowAttribute(
			IntPtr hwnd,
			int attr,
			ref int attrValue,
			int attrSize);

		private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20; // Win10 1809+ (19 on earlier insider builds)
		private const int DWMWA_USE_IMMERSIVE_DARK_MODE_OLD = 19;

		[DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
		private static extern int SetWindowTheme(IntPtr hwnd, string? subAppName, string? subIdList);

		public static void Initialize() {
			// Controls are themed explicitly. Avoid version-dependent, undocumented uxtheme ordinals.
		}

		public static void ApplyToWindow(IntPtr hwnd) {
			if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763)) return;
			try {
				int dark = 1;
				if (DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref dark, sizeof(int)) != 0)
					DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE_OLD, ref dark, sizeof(int));
			}
			catch (DllNotFoundException) { }
			catch (EntryPointNotFoundException) { }
		}

		public static void ApplyToControl(IntPtr hwnd) {
			try { SetWindowTheme(hwnd, "DarkMode_Explorer", null); }
			catch (DllNotFoundException) { }
			catch (EntryPointNotFoundException) { }
		}
	}

}
