using System.Globalization;

namespace AdbFileManager;

internal static class LocalizationText
{
    internal static string Get(string key) => strings.ResourceManager.GetString(key) ?? key;
    internal static void ApplyLanguage(ushort? language)
    {
        string[] cultures = { "en", "cs", "pl", "de", "ja", "es", "zh-Hans", "zh-Hant" };
        CultureInfo.CurrentUICulture = new CultureInfo(cultures[language is <= 7 ? language.Value : 0]);
    }
}
