using Xunit;

namespace AdbFileManager.Tests;

public sealed class PresenterTests
{
    private sealed class Store : ISettingsStore
    {
        public Settings Value { get; } = new();
        public SettingsLoadResult Load() => new(Value, null);
        public void Save(Settings value)
        {
        }
    }
    private sealed class Browser : IAndroidBrowser
    {
        public IReadOnlyList<AndroidDevice> Devices { get; set; } = new[] { new AndroidDevice("phone-A", DeviceState.Ready, "Test") };
        public Func<string, CancellationToken, Task<IReadOnlyList<AndroidFile>>>? List
        {
            get; set;
        }
        public List<(string Path, string Serial)> Queries { get; } = new();
        public List<(string Path, string Serial)> Created { get; } = new();
        public Task<IReadOnlyList<AndroidDevice>> DevicesAsync(CancellationToken token) => Task.FromResult(Devices);
        public Task<IReadOnlyList<AndroidFile>> ListAsync(string path, string serial, bool compatibility, CancellationToken token)
        {
            Queries.Add((path, serial));
            return List?.Invoke(path, token) ?? Task.FromResult<IReadOnlyList<AndroidFile>>(Array.Empty<AndroidFile>());
        }
        public Task CreateDirectoryAsync(string path, string serial, CancellationToken token)
        {
            Created.Add((path, serial));
            return Task.CompletedTask;
        }
    }
    private sealed class View : IBrowserView
    {
        public BrowserStatus? Status
        {
            get; private set;
        }
        public string? Device
        {
            get; private set;
        }
        public IReadOnlyList<AndroidFile>? Files
        {
            get; private set;
        }
        public void ShowDevices(IReadOnlyList<AndroidDevice> devices, string? selectedSerial)
        {
        }
        public void ShowStatus(BrowserStatus status, string? detail = null)
        {
            Status = status;
            Device = null;
            Files = null;
        }
        public void ShowFiles(string path, string deviceSerial, IReadOnlyList<AndroidFile> files)
        {
            Device = deviceSerial;
            Files = files;
            Status = files.Count == 0 ? BrowserStatus.Empty : null;
        }
    }
    [Fact]
    public async Task EmptyFolderRemainsReadyForUploads()
    {
        var view = new View();
        using var presenter = new BrowserPresenter(new Browser(), new DeviceSession(), new SettingsService(new Store()), view);
        await presenter.NavigateAsync("/sdcard");
        Assert.Equal("/sdcard/", presenter.CurrentPath);
        Assert.Equal(BrowserStatus.Empty, view.Status);
        Assert.Equal("phone-A", presenter.ReadySerial);
        Assert.True(presenter.IsReady);
    }
    [Theory]
    [InlineData(DeviceState.Unauthorized, BrowserStatus.Unauthorized)]
    [InlineData(DeviceState.Offline, BrowserStatus.Offline)]
    [InlineData(DeviceState.Recovery, BrowserStatus.Offline)]
    public async Task UnreadyDevicesCannotSupplyFiles(DeviceState state, BrowserStatus expected)
    {
        var browser = new Browser { Devices = new[] { new AndroidDevice("phone-A", state, "Test") } };
        var view = new View();
        using var presenter = new BrowserPresenter(browser, new DeviceSession(), new SettingsService(new Store()), view);
        await presenter.NavigateAsync("/sdcard/");
        Assert.Equal(expected, view.Status);
        Assert.False(presenter.IsReady);
        Assert.Empty(browser.Queries);
    }
    [Fact]
    public async Task DisconnectedSelectionCannotSwitchToAnotherPhone()
    {
        var session = new DeviceSession();
        session.Select("phone-B");
        var browser = new Browser();
        var view = new View();
        using var presenter = new BrowserPresenter(browser, session, new SettingsService(new Store()), view);
        await presenter.NavigateAsync("/sdcard/");
        Assert.Equal(BrowserStatus.Disconnected, view.Status);
        Assert.Equal("phone-B", session.SelectedSerial);
        Assert.False(presenter.IsReady);
        Assert.Empty(browser.Queries);
    }
    [Fact]
    public async Task MultipleDevicesRequireSelection()
    {
        var browser = new Browser { Devices = new[] { new AndroidDevice("A", DeviceState.Ready, "A"), new AndroidDevice("B", DeviceState.Ready, "B") } };
        var view = new View();
        using var presenter = new BrowserPresenter(browser, new DeviceSession(), new SettingsService(new Store()), view);
        await presenter.NavigateAsync("/");
        Assert.Equal(BrowserStatus.ChooseDevice, view.Status);
        Assert.Empty(browser.Queries);
    }
    [Fact]
    public async Task NoDeviceIsSeparateFromAnEmptyDirectory()
    {
        var view = new View();
        var browser = new Browser { Devices = Array.Empty<AndroidDevice>() };
        using var presenter = new BrowserPresenter(browser, new DeviceSession(), new SettingsService(new Store()), view);
        await presenter.NavigateAsync("/");
        Assert.Equal(BrowserStatus.NoDevice, view.Status);
        Assert.False(presenter.IsReady);
    }
    [Fact]
    public async Task LateResultCannotReplaceNewerFolderOrDevice()
    {
        var session = new DeviceSession();
        var browser = new Browser();
        var view = new View();
        var late = new TaskCompletionSource<IReadOnlyList<AndroidFile>>(TaskCreationOptions.RunContinuationsAsynchronously);
        browser.List = (path, _) => path == "/old/" ? late.Task : Task.FromResult<IReadOnlyList<AndroidFile>>(Array.Empty<AndroidFile>());
        using var presenter = new BrowserPresenter(browser, session, new SettingsService(new Store()), view);
        Task old = presenter.NavigateAsync("/old/");
        session.Select("phone-B");
        browser.Devices = new[] { new AndroidDevice("phone-B", DeviceState.Ready, "B") };
        await presenter.NavigateAsync("/new/");
        late.SetResult(new[] { new AndroidFile("old.txt", "-rw-r--r--", 12, null) });
        await old;
        Assert.Equal("/new/", presenter.CurrentPath);
        Assert.Equal("phone-B", presenter.ReadySerial);
        Assert.Empty(view.Files!);
    }
    [Theory]
    [InlineData("relative")]
    [InlineData("")]
    public async Task InvalidPathsDoNotStartAdb(string path)
    {
        var browser = new Browser();
        var view = new View();
        using var presenter = new BrowserPresenter(browser, new DeviceSession(), new SettingsService(new Store()), view);
        await presenter.NavigateAsync(path);
        Assert.Equal(BrowserStatus.InvalidPath, view.Status);
        Assert.Empty(browser.Queries);
    }
    [Fact]
    public async Task ListingFailureClearsReadinessAndMapsTypedStatus()
    {
        var browser = new Browser { List = (_, _) => Task.FromException<IReadOnlyList<AndroidFile>>(new BrowserException(BrowserStatus.InvalidListing, "bad format")) };
        var view = new View();
        using var presenter = new BrowserPresenter(browser, new DeviceSession(), new SettingsService(new Store()), view);
        await presenter.NavigateAsync("/");
        Assert.Equal(BrowserStatus.InvalidListing, view.Status);
        Assert.False(presenter.IsReady);
    }
    [Fact]
    public async Task CreatingFolderTargetsTheDisplayedDeviceAndRefreshes()
    {
        var browser = new Browser();
        using var presenter = new BrowserPresenter(browser, new DeviceSession(), new SettingsService(new Store()), new View());
        await presenter.NavigateAsync("/sdcard/");
        await presenter.CreateDirectoryAsync("Fotos.2026");
        Assert.Equal(("/sdcard/Fotos.2026", "phone-A"), Assert.Single(browser.Created));
        Assert.Equal(2, browser.Queries.Count);
    }
    [Fact]
    public async Task DisposingPresenterRejectsPendingResult()
    {
        var pending = new TaskCompletionSource<IReadOnlyList<AndroidFile>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var browser = new Browser { List = (_, _) => pending.Task };
        var view = new View();
        var presenter = new BrowserPresenter(browser, new DeviceSession(), new SettingsService(new Store()), view);
        Task load = presenter.NavigateAsync("/");
        presenter.Dispose();
        pending.SetResult(Array.Empty<AndroidFile>());
        await load;
        Assert.False(presenter.IsReady);
        Assert.Null(view.Files);
    }
}
