namespace AdbFileManager.Core.Browsing;

// All view notifications stay on the caller's synchronization context (the UI thread in WinForms).
public sealed class BrowserPresenter(IAndroidBrowser browser, DeviceSession session, SettingsService settings, IBrowserView view) : IDisposable
{
    private readonly LatestBrowserRequest requests = new();
    public string CurrentPath { get; private set; } = "/sdcard/";
    public string? ReadySerial
    {
        get; private set;
    }
    public bool IsReady => ReadySerial != null;

    public async Task NavigateAsync(string path)
    {
        CancellationToken token = requests.Start();
        ReadySerial = null;
        if (string.IsNullOrEmpty(path) || !path.StartsWith('/') || path.Contains('\0'))
        {
            view.ShowStatus(BrowserStatus.InvalidPath);
            return;
        }
        CurrentPath = path.EndsWith('/') ? path : path + "/";
        path = CurrentPath;
        string? selectedSerial = session.SelectedSerial;
        bool compatibility = settings.Current.UseCompatibilityMode;
        view.ShowStatus(BrowserStatus.Loading);
        try
        {
            var devices = await browser.DevicesAsync(token);
            if (!requests.IsCurrent(token))
                return;
            session.UpdateDevices(devices);
            view.ShowDevices(devices, selectedSerial);
            var device = selectedSerial == null ? (devices.Count == 1 ? devices[0] : null)
                : devices.FirstOrDefault(item => item.Serial == selectedSerial);
            if (device == null)
            {
                view.ShowStatus(selectedSerial != null ? BrowserStatus.Disconnected : devices.Count == 0 ? BrowserStatus.NoDevice : BrowserStatus.ChooseDevice);
                return;
            }
            if (device.State != DeviceState.Ready)
            {
                view.ShowStatus(device.State == DeviceState.Unauthorized ? BrowserStatus.Unauthorized : BrowserStatus.Offline);
                return;
            }
            var files = await browser.ListAsync(path, device.Serial, compatibility, token);
            if (!requests.IsCurrent(token))
                return;
            ReadySerial = device.Serial;
            view.ShowFiles(path, device.Serial, files);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
        catch (Exception ex)
        {
            if (requests.IsCurrent(token))
                view.ShowStatus(ex is BrowserException error ? error.Status : BrowserStatus.ReadError, ex.Message);
        }
    }

    public void Cancel()
    {
        ReadySerial = null;
        requests.Cancel();
    }
    public async Task CreateDirectoryAsync(string name)
    {
        if (!IsReady || string.IsNullOrEmpty(name) || name.Contains('/') || name.Contains('\0') || name is "." or "..")
            throw new BrowserException(BrowserStatus.InvalidPath, "A valid folder name and a ready device are required.");
        string path = CurrentPath;
        string serial = ReadySerial!;
        await browser.CreateDirectoryAsync(AndroidPath.Combine(path, name), serial, CancellationToken.None);
        if (CurrentPath == path && ReadySerial == serial)
            await NavigateAsync(path);
    }
    public void Dispose() => Cancel();
}
