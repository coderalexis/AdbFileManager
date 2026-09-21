using System.Runtime.InteropServices;

namespace AdbFileManager {
    internal static class CommandArguments {
        [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr CommandLineToArgvW(string commandLine, out int count);
        [DllImport("kernel32.dll")]
        private static extern IntPtr LocalFree(IntPtr memory);
        internal static string[] Parse(string text) {
            IntPtr pointer = CommandLineToArgvW("adb " + text, out int count);
            if (pointer == IntPtr.Zero) throw new System.ComponentModel.Win32Exception();
            try {
                var arguments = new string[Math.Max(0, count - 1)];
                for (int i = 1; i < count; i++)
                    arguments[i - 1] = Marshal.PtrToStringUni(Marshal.ReadIntPtr(pointer, i * IntPtr.Size)) ?? "";
                return arguments;
            }
            finally { LocalFree(pointer); }
        }
    }
}
