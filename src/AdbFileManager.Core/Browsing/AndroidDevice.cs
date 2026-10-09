namespace AdbFileManager.Core.Browsing;

public enum DeviceState
{
    Ready, Offline, Unauthorized, Recovery, Sideload
}
public sealed record AndroidDevice(string Serial, DeviceState State, string Model);
