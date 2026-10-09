namespace AdbFileManager;

internal static class UserInterfaceText
{
    internal static string Get(string key) => LocalizationText.Get("ux_" + key);
}
