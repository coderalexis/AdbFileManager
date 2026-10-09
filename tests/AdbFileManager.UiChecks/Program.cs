using System.Reflection;
using AdbFileManager;
using AdbFileManager.Core.Browsing;
using AdbFileManager.Core.Settings;
using AdbFileManager.Core.Transfers;
using AdbFileManager.Infrastructure.Adb;

namespace AdbFileManager.UiChecks;

internal static class Program
{
    private sealed class Store : ISettingsStore, IQueueStore
    {
        public Settings Settings { get; } = new() { DarkMode = true, Language = 5, ButtonStyle = 2 };
        public SettingsLoadResult Load() => new(Settings, null);
        List<TransferJob> IQueueStore.Load() => new();
        public void Save(Settings value)
        {
        }
        public void Save(IEnumerable<TransferJob> jobs)
        {
        }
    }
    private sealed class Adb : IAdbClient
    {
        public int ProgressIntervalMs { get; set; } = 60;
        public Task<string> VersionAsync() => Task.FromResult("37.0.1-ui-check");
        public Task<AdbResult> ExecuteAsync(IEnumerable<string> args, string? serial = null, CancellationToken token = default) => Task.FromResult(new AdbResult(0, "", ""));
        public Task<string> QueryAsync(IEnumerable<string> args, string? serial = null, CancellationToken token = default) => Task.FromResult("");
        public Task CopyAsync(IEnumerable<string> args, IProgress<int>? progress, CancellationToken token) => Task.CompletedTask;
        public Task<TransferStatistics> CopyWithStatisticsAsync(IEnumerable<string> args, IProgress<int>? progress, CancellationToken token) => Task.FromResult(new TransferStatistics(null, 0));
    }
    private sealed class Browser : IAndroidBrowser
    {
        public IReadOnlyList<AndroidDevice> Devices { get; set; } = new[] { new AndroidDevice("phone", DeviceState.Ready, "Test phone") };
        public Task<IReadOnlyList<AndroidDevice>> DevicesAsync(CancellationToken token) => Task.FromResult(Devices);
        public async Task<IReadOnlyList<AndroidFile>> ListAsync(string path, string serial, bool compatibility, CancellationToken token)
        {
            if (path == "/slow/")
            {
                await Task.Delay(1200);
                return new[] { new AndroidFile("stale.txt", "-rw-r--r--", 1, null) };
            }
            if (path == "/empty/")
                return Array.Empty<AndroidFile>();
            return new[] { new AndroidFile("Fotos.2026", "drwxrwx---", 4096, new DateTime(2026, 10, 8)), new AndroidFile("teléfono  con espacios.bin", "-rw-r--r--", 1234, null) };
        }
        public Task<string?> FindExternalStorageAsync(string serial, CancellationToken token) => Task.FromResult<string?>("/storage/1234-ABCD/");
        public Task CreateDirectoryAsync(string path, string serial, CancellationToken token) => Task.CompletedTask;
    }
    private sealed class Backend : ITransferBackend
    {
        public Task<EntryKind> InspectAsync(TransferJob job, string destination, CancellationToken token) => Task.FromResult(EntryKind.Missing);
        public Task CopyAsync(TransferJob job, string destination, bool replace, IProgress<int> progress, CancellationToken token) => Task.CompletedTask;
    }
    private static void Check(bool condition, string name)
    {
        if (!condition)
            throw new InvalidOperationException(name);
        Console.WriteLine("PASS " + name);
    }
    [STAThread]
    private static void Main(string[] args)
    {
        if (args.Length != 2)
            throw new ArgumentException("Pass asset directory and screenshot directory.");
        ApplicationConfiguration.Initialize();
        LocalizationText.ApplyLanguage(5);
        DarkModeStartup.Initialize();
        Directory.CreateDirectory(args[1]);
        var store = new Store();
        var settings = new SettingsService(store);
        var adb = new Adb();
        var browser = new Browser();
        var theme = new AppTheme(true);
        var session = new DeviceSession();
        using var icons = new IconProvider(Path.GetFullPath(args[0]), true);
        using var preview = new MediaPreviewService(adb);
        using var main = new MainForm(adb, browser, new Backend(), store, settings, session, theme, icons, preview);
        main.StartPosition = FormStartPosition.Manual;
        main.Location = new Point(-5000, -5000);
        main.Size = new Size(1500, 850);
        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var load = typeof(MainForm).GetMethod("LoadAndroidDirectoryAsync", flags)!;
        Task Navigate(string path) => (Task)load.Invoke(main, new object[] { path })!;
        void Render(Form form, string name)
        {
            using var bitmap = new Bitmap(form.Width, form.Height);
            form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
            bitmap.Save(Path.Combine(args[1], name));
        }
        int exitCode = 0;
        int ticks = 0;
        using var heartbeat = new System.Windows.Forms.Timer { Interval = 25 };
        heartbeat.Tick += (_, _) => ticks++;
        main.Shown += async (_, _) =>
        {
            try
            {
                ((System.Windows.Forms.Timer)typeof(MainForm).GetField("initialLoadTimer", flags)!.GetValue(main)!).Stop();
                heartbeat.Start();
                await Navigate("/sdcard/");
                var localReadyDeadline = DateTime.UtcNow.AddSeconds(5);
                while (!Directory.Exists(main.localFilesBrowser.NavigationLog.CurrentLocation?.ParsingName) && DateTime.UtcNow < localReadyDeadline)
                    await Task.Delay(25);
                Check(Directory.Exists(main.localFilesBrowser.NavigationLog.CurrentLocation?.ParsingName), "Native PC pane completes folder navigation");
                foreach (string culture in new[] { "cs", "de", "es", "ja", "pl", "zh-Hans", "zh-Hant" })
                {
                    var satellite = typeof(MainForm).Assembly.GetSatelliteAssembly(new System.Globalization.CultureInfo(culture));
                    Check(satellite.GetManifestResourceNames().Contains("AdbFileManager.MainForm." + culture + ".resources"), "Localized MainForm resources: " + culture);
                }
                typeof(MainForm).GetMethod("OnMainResize", flags)!.Invoke(main, new object[] { main, EventArgs.Empty });
                Check(main.androidFilesGrid.Rows.Count == 2, "Main form binds real metadata through presenter");
                Check(main.androidFilesGrid.ReadOnly && !main.androidFilesGrid.BeginEdit(false), "Android table rejects editing");
                Check(main.androidFilesGrid.Columns["name"].HeaderText == "Nombre", "Moved resources preserve Spanish headers");
                main.androidFilesGrid.Rows[0].Selected = true;
                Check(main.downloadButton.Text.Contains("Descargar al PC") && main.uploadButton.Text.Contains("Enviar a Android"), "Explicit copy direction labels");
                await Navigate("/sdcard/");
                Check(main.androidFilesGrid.SelectedRows.Count == 1, "Refresh retains selection in the same folder and phone");
                Check(main.downloadButton.Enabled, "Android selection with a PC folder enables download");
                var filter = (TextBox)main.Controls.Find("androidFilterTextBox", true).Single();
                filter.Text = "TELÉFONO";
                Check(main.androidFilesGrid.Rows.Count == 1 && Convert.ToString(main.androidFilesGrid.Rows[0].Cells["name"].Value)!.Contains("teléfono"), "Android name filter is immediate and Unicode aware");
                filter.Clear();
                Check(!main.downloadButton.Enabled && main.androidFilesGrid.SelectedRows.Count == 0, "Filtered-out selection cannot be copied after clearing the filter");
                ((Button)main.Controls.Find("externalStorageButton", true).Single()).PerformClick();
                await Task.Delay(50);
                Check(main.androidPathTextBox.Text == "/storage/1234-ABCD/", "SD shortcut navigates the discovered volume");
                await Navigate("/sdcard/");
                var dock = (AdbFileManager.Views.Transfers.IntegratedTransfersControl)main.Controls.Find("integratedTransfers", true).Single();
                dock.SetExpanded(true);
                foreach (float scale in new[] { 1.25f, 1.5f, 2f })
                {
                    main.ClientSize = new Size((int)(1000 * scale), (int)(700 * scale));
                    main.ApplyWorkspaceScale(scale);
                    main.PerformLayout();
                    Check(main.downloadButton.Width >= 150 && main.androidFilesGrid.Height > 80 && main.localFilesBrowser.Height > 80,
                        "Usable panes and copy actions at layout scale " + scale);
                }
                main.ApplyWorkspaceScale(main.DeviceDpi / 96f);
                main.ClientSize = new Size(1100 * main.DeviceDpi / 96, 700 * main.DeviceDpi / 96);
                main.androidFilesGrid.Rows[1].Selected = true;
                Render(main, "main-dark.png");
                var first = Navigate("/slow/");
                await Task.Delay(700);
                Check(ticks > 0 && !main.downloadButton.Enabled, "Window remains responsive during loading");
                await Navigate("/empty/");
                await first;
                Check(main.androidFilesGrid.Rows.Count == 0, "Late result cannot replace an empty folder");
                Check(main.Controls.Find("integratedTransfers", true).Length == 1, "Transfer dock is integrated into the main window");
                browser.Devices = Array.Empty<AndroidDevice>();
                await Navigate("/");
                Check(!main.downloadButton.Enabled && !main.uploadButton.Enabled, "Disconnected device disables stale copies");
                Check(((Button)main.Controls.Find("browserRecheckButton", true).Single()).Visible, "Disconnected status offers recheck action");
                browser.Devices = new[] { new AndroidDevice("phone", DeviceState.Ready, "Test phone") };
                ((Button)main.Controls.Find("browserRecheckButton", true).Single()).PerformClick();
                await Task.Delay(50);
                Check(main.androidFilesGrid.Rows.Count == 2 && main.createDirectoryButton.Enabled, "Check again recovers the connected browser");
                ((IBrowserView)main).ShowStatus(BrowserStatus.ReadError, "ADB test diagnostic");
                Check(((Button)main.Controls.Find("browserDetailsButton", true).Single()).Visible, "Read failures expose diagnostic details");
                await Navigate("/sdcard/");
                using (var dialog = new SettingsForm(settings, adb, theme))
                {
                    dialog.StartPosition = FormStartPosition.Manual;
                    dialog.Location = new Point(-5000, -5000);
                    dialog.Show();
                    ((TabControl)typeof(SettingsForm).GetField("settingsTabs", flags)!.GetValue(dialog)!).SelectedIndex = 0;
                    Check(dialog.Text == "Configuración", "Settings resources preserved");
                    Render(dialog, "settings-dark.png");
                    dialog.Hide();
                }
                using (var unlock = new UnlockForm(adb, session, theme))
                {
                    unlock.Show();
                    unlock.Hide();
                }
                using (var wireless = new WirelessPair(adb, theme))
                {
                    wireless.Show();
                    wireless.Hide();
                }
                using (var apk = new ApkInstallWizard("example.apk", "phone", adb, theme))
                {
                    apk.Show();
                    apk.Hide();
                }
                Check(true, "Moved dialog and Keypad resources load");
                var queue = new TransferQueue(new Backend(), (_, _, _) => Task.FromResult(new ConflictDecision(ConflictAction.Skip)));
                queue.Jobs.Add(new TransferJob
                {
                    DeviceId = "Test phone",
                    Source = "/video.mp4",
                    Destination = @"C:\Backup\video.mp4",
                    State = TransferState.Completed,
                    TransferredBytes = 104857600,
                    TransferSeconds = 2.5,
                    ElapsedSeconds = 4,
                    StartedAt = DateTimeOffset.Now.AddSeconds(-4)
                });
                var cancelled = new TransferJob { DeviceId = "Test phone", Source = "/cancelled.mp4", Destination = @"C:\Backup\cancelled.mp4", State = TransferState.Cancelled };
                var failed = new TransferJob { DeviceId = "Test phone", Source = "/failed.mp4", Destination = @"C:\Backup\failed.mp4", State = TransferState.Failed };
                queue.Jobs.Add(cancelled);
                queue.Jobs.Add(failed);
                using var queueForm = new AdbFileManager.Views.Transfers.TransferQueueForm(queue, queue.RunAsync, theme);
                queueForm.StartPosition = FormStartPosition.Manual;
                queueForm.Location = new Point(-5000, -5000);
                queueForm.Show();
                var clearCancelled = (Button)queueForm.Controls.Find("clearCancelled", true).Single();
                Check(clearCancelled.Text == "Limpiar cancelados", "Clear cancelled button is localized");
                Render(queueForm, "queue-dark.png");
                clearCancelled.PerformClick();
                Check(!queue.Jobs.Contains(cancelled) && queue.Jobs.Contains(failed) && queue.Jobs.Any(job => job.State == TransferState.Completed),
                    "Clear cancelled button removes only cancelled jobs");
                queueForm.Hide();
                Check(true, "Queue statistics render under unified theme");
                using var dockHost = new Form { ClientSize = new Size(1100, 260), Location = new Point(-5000, -5000), StartPosition = FormStartPosition.Manual };
                using var retryDock = new AdbFileManager.Views.Transfers.IntegratedTransfersControl(queue, queue.RunAsync, () => { }, theme);
                dockHost.Controls.Add(retryDock);
                dockHost.Show();
                retryDock.SetExpanded(true);
                var retryButton = (Button)retryDock.Controls.Find("retryTransfer", true).Single();
                Check(retryButton.Enabled, "Integrated dock exposes retry for failed jobs");
                retryButton.PerformClick();
                await Task.Delay(50);
                Check(failed.State == TransferState.Completed && !retryButton.Enabled, "Integrated retry actually runs the failed transfer");
                int expandedHeight = retryDock.Height;
                ((Button)retryDock.Controls.Find("toggleTransfers", true).Single()).PerformClick();
                Check(retryDock.Height < expandedHeight, "Transfer dock collapses to recover browsing space");
                dockHost.Hide();
                Console.WriteLine("UI_CHECKS_PASSED");
            }
            catch (Exception ex) { Console.Error.WriteLine(ex); exitCode = 1; }
            finally { heartbeat.Stop(); main.Dispose(); Application.ExitThread(); }
        };
        Application.Run(main);
        Environment.ExitCode = exitCode;
    }
}
