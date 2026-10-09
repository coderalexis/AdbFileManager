namespace AdbFileManager.Core.Browsing;

public enum BrowserStatus
{
    Loading, Empty, NoDevice, Disconnected, ChooseDevice, Unauthorized, Offline, InvalidPath, InvalidListing, ReadError
}

public interface IBrowserView
{
    void ShowDevices(IReadOnlyList<AndroidDevice> devices, string? selectedSerial);
    void ShowStatus(BrowserStatus status, string? detail = null);
    void ShowFiles(string path, string deviceSerial, IReadOnlyList<AndroidFile> files);
}
