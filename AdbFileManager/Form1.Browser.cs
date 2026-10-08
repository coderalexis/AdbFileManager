using System.Data;

namespace AdbFileManager {
    public partial class Form1 {
        private readonly AndroidBrowser androidBrowser = new(AdbClient.Default);
        private readonly LatestBrowserRequest browserRequests = new();
        private Label browserStatus = null!;
        private string? listedDevice;
        private bool browserReady;

        private static string BrowserText(string key) => strings.ResourceManager.GetString("browser_" + key) ?? key;

        private void InitializeBrowser() {
            dataGridView_soubory.ReadOnly = true;
            dataGridView_soubory.EditMode = DataGridViewEditMode.EditProgrammatically;
            dataGridView_soubory.AutoGenerateColumns = false;
            for (int i = 0; i < dataGridView_soubory.Columns.Count; i++) {
                dataGridView_soubory.Columns[i].DataPropertyName = new[] { "Icon", "Name", "Size", "Date", "Permissions" }[i];
                dataGridView_soubory.Columns[i].HeaderText = i == 0 ? "ico" :
                    rm.GetString(new[] { "", "datagridview_name", "datagridview_size", "datagridview_date", "datagridview_attr" }[i]);
            }
            browserStatus = new Label {
                Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Padding = new Padding(24),
                BackColor = SettingsManager.settings.DarkMode ? AppTheme.Surface : SystemColors.Window,
                ForeColor = SettingsManager.settings.DarkMode ? AppTheme.Text : SystemColors.ControlText,
                AccessibleName = BrowserText("status"), Visible = false
            };
            dataGridView_soubory.Controls.Add(browserStatus);
            cur_path.KeyDown += (_, e) => {
                if (e.KeyCode != Keys.Enter) return;
                e.SuppressKeyPress = true;
                NavigateToDirectory(cur_path.Text);
            };
            FormClosed += (_, _) => browserRequests.Dispose();
            SetBrowserStatus(BrowserText("loading"));
        }

        private DataTable BrowserTable() {
            var table = new DataTable();
            table.Columns.Add("Icon", typeof(Icon));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Size", typeof(decimal));
            table.Columns.Add("Date", typeof(DateTime));
            table.Columns.Add("Permissions", typeof(string));
            return table;
        }

        private void SetBrowserStatus(string message) {
            browserReady = false;
            listedDevice = null;
            dataGridView_soubory.DataSource = BrowserTable();
            browserStatus.Text = message;
            browserStatus.Visible = true;
            browserStatus.BringToFront();
            button_android2pc.Enabled = false;
            button_pc2android.Enabled = false;
            verticalLabel_makedir.Enabled = false;
        }

        private async Task LoadAndroidDirectoryAsync(string path) {
            CancellationToken token = browserRequests.Start();
            string? serial = selectedDevice?.adbId;
            bool compatibility = SettingsManager.settings.useCompatibilityMode;
            SetBrowserStatus(BrowserText("loading"));
            try {
                IReadOnlyList<AndroidDevice> devices = await androidBrowser.DevicesAsync(token);
                if (!browserRequests.IsCurrent(token) || IsDisposed) return;
                UpdateDevices(devices, serial);
                AndroidDevice? device = serial == null
                    ? (devices.Count == 1 ? devices[0] : null)
                    : devices.FirstOrDefault(item => item.Serial == serial);
                if (device == null) {
                    SetBrowserStatus(BrowserText(serial != null ? "disconnected" : devices.Count == 0 ? "noDevice" : "chooseDevice"));
                    return;
                }
                if (device.State != "device") {
                    SetBrowserStatus(BrowserText(device.State == "unauthorized" ? "unauthorized" : "offline"));
                    return;
                }
                IReadOnlyList<AndroidFile> files = await androidBrowser.ListAsync(path, device.Serial, compatibility, token);
                if (!browserRequests.IsCurrent(token) || IsDisposed) return;
                var table = BrowserTable();
                foreach (var file in files) {
                    Icon icon = UIStyle.GetIcon(file.Name, file.IsDirectory) ?? SystemIcons.Application;
                    table.Rows.Add(icon, file.Name, file.Bytes.HasValue ? (object)decimal.Round((decimal)file.Bytes.Value / 1024, 3) : DBNull.Value,
                        file.Modified.HasValue ? (object)file.Modified.Value : DBNull.Value, file.Permissions);
                }
                dataGridView_soubory.DataSource = table;
                browserStatus.Text = BrowserText("empty");
                browserStatus.Visible = files.Count == 0;
                if (browserStatus.Visible) browserStatus.BringToFront();
                listedDevice = device.Serial;
                browserReady = true;
                button_android2pc.Enabled = files.Count > 0;
                button_pc2android.Enabled = true;
                verticalLabel_makedir.Enabled = true;
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested) { }
            catch (Exception ex) {
                if (!browserRequests.IsCurrent(token) || IsDisposed) return;
                string key = ex is InvalidDataException ? "formatError" : "readError";
                SetBrowserStatus(BrowserText(key) + Environment.NewLine + ex.Message);
            }
        }

        private void UpdateDevices(IReadOnlyList<AndroidDevice> devices, string? selectedSerial) {
            modifyingComboBox = true;
            try {
                foundDevices = devices.Select(item => new Device { adbId = item.Serial, state = item.State, model = item.Model }).ToList();
                comboBox_device.Items.Clear();
                comboBox_device.Items.Add(strings.defaultDevice);
                comboBox_device.Items.Add(strings.addWireless);
                foreach (var device in devices)
                    comboBox_device.Items.Add(device.Model + (device.State == "device" ? "" : " (" + device.State + ")"));
                int index = foundDevices.FindIndex(device => device.adbId == selectedSerial);
                // Preserve a disconnected selection so it cannot silently target another phone.
                if (selectedSerial != null && index < 0) {
                    comboBox_device.Items.Add(selectedSerial + " (" + BrowserText("disconnected") + ")");
                    comboBox_device.SelectedIndex = comboBox_device.Items.Count - 1;
                }
                else {
                    comboBox_device.SelectedIndex = index >= 0 ? index + 2 : 0;
                    selectedDevice = index >= 0 ? foundDevices[index] : null;
                }
                comboBox_device.DropDownWidth = Math.Max(200, comboBox_device.Items.Cast<string>()
                    .Max(item => TextRenderer.MeasureText(item, comboBox_device.Font).Width) + 20);
            }
            finally { modifyingComboBox = false; }
        }
    }
}
