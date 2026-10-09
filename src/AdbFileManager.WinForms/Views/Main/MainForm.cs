// Originally created by T0biasCZe. This community fork preserves credit in README.md.
using System.Diagnostics;
using Microsoft.WindowsAPICodePack.Controls;

namespace AdbFileManager
{
    internal partial class MainForm : Form, IBrowserView
    {
        private readonly IAdbClient _adb;
        private readonly ITransferBackend _transferBackend;
        private readonly IQueueStore _queueStore;
        private readonly SettingsService _settings;
        private readonly DeviceSession _session;
        private readonly AppTheme _theme;
        private readonly IconProvider _icons;
        private readonly MediaPreviewService _preview;
        private readonly BrowserPresenter _browser;
        private string CurrentAndroidPath => _browser.CurrentPath;
        private bool IsBrowserReady => _browser.IsReady;
        private string? ReadyDeviceSerial => _browser.ReadySerial;

        internal MainForm(IAdbClient adb, IAndroidBrowser browser, ITransferBackend backend, IQueueStore queueStore,
            SettingsService settings, DeviceSession session, AppTheme theme, IconProvider icons, MediaPreviewService preview)
        {
            _adb = adb;
            _transferBackend = backend;
            _queueStore = queueStore;
            _settings = settings;
            _session = session;
            _theme = theme;
            _icons = icons;
            _preview = preview;
            _browser = new BrowserPresenter(browser, session, settings, this);
            InitializeComponent();
            InitializeWorkspace();
            localFilesBrowser.HandleCreated += (_, _) => ApplyExplorerDarkMode();
            if (_theme.IsDark)
            {
                localFilesBrowser.NavigationOptions.PaneVisibility.Commands = PaneVisibilityState.Hide;
                localFilesBrowser.NavigationOptions.PaneVisibility.CommandsOrganize = PaneVisibilityState.Hide;
                localFilesBrowser.NavigationOptions.PaneVisibility.CommandsView = PaneVisibilityState.Hide;
            }
            InitializeBrowser();
            InitializeTransfers();
            InitializeAppearance();
            versionLabel.Text = Properties.Resources.CurrentCommit.Trim();
            FormClosed += (_, _) => _browser.Dispose();
        }

        private void InitializeAppearance()
        {
            localBackButton.Image = _icons.GetNavigation("travel_enabled_back");
            localForwardButton.Image = _icons.GetNavigation("travel_enabled_forward");
            _theme.Apply(this);
        }
        private async void OnRefreshClick(object? sender, EventArgs e)
        {
            await LoadAndroidDirectoryAsync(CurrentAndroidPath);
        }

        private void OnVersionLinkClick(object? sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/coderalexis/AdbFileManager") { UseShellExecute = true });
        }
        private bool consoleShown;
        private void OnConsoleToggle(object? sender, EventArgs e)
        {
            consoleShown = !consoleShown;
            if (consoleShown)
                ConsoleWindow.Show();
            else
                ConsoleWindow.Hide();
        }
        private void OnMainClosing(object? sender, FormClosingEventArgs e)
        {
            _browser.Cancel();
            if (transferQueue?.IsRunning == true)
            {
                e.Cancel = true;
                closeAfterQueue = true;
                transferQueue.Pause();
                return;
            }
            try
            {
                _settings.Save();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
            {
                e.Cancel = true;
                closeAfterQueue = false;
                MessageBox.Show(this, LocalizationText.Get("settings_saveError") + Environment.NewLine + ex.Message,
                    strings.settings_title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (queueWindow != null)
                queueWindow.AllowClose = true;
            SaveAndRefreshQueue();
            //show console
            ConsoleWindow.Show();
            // The ADB server is shared with other applications; leave it running.
        }

        private void OnMainClosed(object? sender, FormClosedEventArgs e)
        {
            Application.Exit();
            ConsoleWindow.Hide();
        }

    }
}
