using System.Xml.Serialization;

namespace AdbFileManager.Core.Settings;

// XML names remain stable so existing user settings continue to load.
public sealed class Settings
{
    [XmlElement("ButtonTheme")]
    public int ButtonStyle
    {
        get; set;
    }
    public bool UseWindows11Icons { get; set; } = true;
    public bool DarkMode
    {
        get; set;
    }
    public bool ShowAndroidBackButton { get; set; } = true;
    [XmlElement("lang")]
    public ushort? Language
    {
        get; set;
    }
    [XmlElement("progressWaitTimeMs")] public int ProgressIntervalMs { get; set; } = 60;
    [XmlElement("useCompatibilityMode")]
    public bool UseCompatibilityMode
    {
        get; set;
    }
    [XmlElement("previewMediaFiles")]
    public bool PreviewMediaFiles
    {
        get; set;
    }
    [XmlElement("keepFileModificationDate")] public bool KeepFileModificationDate { get; set; } = true;
    [XmlElement("rememberDirectory")]
    public bool RememberDirectory
    {
        get; set;
    }
    [XmlElement("lastDirectory")] public string LastDirectory { get; set; } = "";
}
