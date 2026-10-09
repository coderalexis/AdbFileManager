using System.Runtime.InteropServices;

namespace AdbFileManager;

internal static class ConsoleWindow
{
    [DllImport("kernel32.dll")] private static extern IntPtr GetConsoleWindow();
    [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr window, int command);
    internal static void Show() => ShowWindow(GetConsoleWindow(), 5);
    internal static void Hide() => ShowWindow(GetConsoleWindow(), 0);
}
