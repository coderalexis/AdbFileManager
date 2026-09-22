using System;
using System.Runtime.InteropServices;

namespace AdbFileManager {

	static class DarkModeStartup {
		private const int WM_THEMECHANGED = 0x031A;
		private const uint RDW_INVALIDATE = 0x0001;
		private const uint RDW_UPDATENOW = 0x0100;
		private const uint RDW_FRAME = 0x0400;
		private const uint RDW_ALLCHILDREN = 0x0080;

		[DllImport("dwmapi.dll")]
		private static extern int DwmSetWindowAttribute(
			IntPtr hwnd,
			int attr,
			ref int attrValue,
			int attrSize);

		private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
		private const int DWMWA_USE_IMMERSIVE_DARK_MODE_OLD = 19;

		[DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
		private static extern int SetWindowTheme(IntPtr hwnd, string? subAppName, string? subIdList);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		private static extern IntPtr GetModuleHandle(string moduleName);

		[DllImport("kernel32.dll")]
		private static extern IntPtr GetProcAddress(IntPtr module, IntPtr procedureName);

		[DllImport("user32.dll")]
		private static extern bool EnumChildWindows(IntPtr parent, EnumWindowsProc callback, IntPtr state);

		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		private static extern bool RedrawWindow(IntPtr hwnd, IntPtr updateRectangle, IntPtr updateRegion, uint flags);

		private delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr state);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int SetPreferredAppModeDelegate(int preferredAppMode);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private delegate bool AllowDarkModeForAppDelegate([MarshalAs(UnmanagedType.Bool)] bool allow);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private delegate bool AllowDarkModeForWindowDelegate(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool allow);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void RefreshImmersiveColorPolicyStateDelegate();

		private static AllowDarkModeForWindowDelegate? allowDarkModeForWindow;
		private static bool initialized;

		public static void Initialize() {
			if (initialized || !OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763)) return;
			initialized = true;

			try {
				IntPtr uxTheme = GetModuleHandle("uxtheme.dll");
				if (uxTheme == IntPtr.Zero) return;

				IntPtr appModeAddress = GetProcAddress(uxTheme, (IntPtr)135);
				if (appModeAddress != IntPtr.Zero) {
					if (Environment.OSVersion.Version.Build >= 18362) {
						// PreferredAppMode.ForceDark. Must run before ExplorerBrowser creates its HWND tree.
						Marshal.GetDelegateForFunctionPointer<SetPreferredAppModeDelegate>(appModeAddress)(2);
					}
					else {
						Marshal.GetDelegateForFunctionPointer<AllowDarkModeForAppDelegate>(appModeAddress)(true);
					}
				}

				IntPtr windowModeAddress = GetProcAddress(uxTheme, (IntPtr)133);
				if (windowModeAddress != IntPtr.Zero)
					allowDarkModeForWindow = Marshal.GetDelegateForFunctionPointer<AllowDarkModeForWindowDelegate>(windowModeAddress);

				IntPtr refreshAddress = GetProcAddress(uxTheme, (IntPtr)104);
				if (refreshAddress != IntPtr.Zero)
					Marshal.GetDelegateForFunctionPointer<RefreshImmersiveColorPolicyStateDelegate>(refreshAddress)();
			}
			catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException or MarshalDirectiveException) { }
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
			if (hwnd == IntPtr.Zero) return;
			try {
				allowDarkModeForWindow?.Invoke(hwnd, true);
				SetWindowTheme(hwnd, "DarkMode_Explorer", null);
			}
			catch (DllNotFoundException) { }
			catch (EntryPointNotFoundException) { }
		}

		public static void ApplyToControlTree(IntPtr hwnd) {
			if (hwnd == IntPtr.Zero) return;

			ApplyToControl(hwnd);
			EnumChildWindows(hwnd, (child, _) => {
				ApplyToControl(child);
				SendMessage(child, WM_THEMECHANGED, IntPtr.Zero, IntPtr.Zero);
				return true;
			}, IntPtr.Zero);

			SendMessage(hwnd, WM_THEMECHANGED, IntPtr.Zero, IntPtr.Zero);
			RedrawWindow(hwnd, IntPtr.Zero, IntPtr.Zero,
				RDW_INVALIDATE | RDW_UPDATENOW | RDW_FRAME | RDW_ALLCHILDREN);
		}
	}

}
