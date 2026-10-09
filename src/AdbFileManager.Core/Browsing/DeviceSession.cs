namespace AdbFileManager.Core.Browsing;

public sealed class DeviceSession
{
    public string? SelectedSerial
    {
        get; private set;
    }
    public IReadOnlyList<AndroidDevice> Devices { get; private set; } = Array.Empty<AndroidDevice>();
    public void Select(string? serial) => SelectedSerial = serial;
    public void UpdateDevices(IReadOnlyList<AndroidDevice> devices) => Devices = Array.AsReadOnly(devices.ToArray());
}
