using System.Data;

namespace AdbFileManager;

internal partial class MainForm
{
    private Label browserStatusLabel = null!;
    private static string BrowserText(string key) => LocalizationText.Get("browser_" + key);

    private void InitializeBrowser()
    {
        androidFilesGrid.ReadOnly = true;
        androidFilesGrid.EditMode = DataGridViewEditMode.EditProgrammatically;
        androidFilesGrid.AutoGenerateColumns = false;
        androidFilesGrid.Columns.Clear();
        androidFilesGrid.Columns.Add(new DataGridViewImageColumn
        {
            Name = "icon",
            DataPropertyName = "Icon",
            HeaderText = "ico",
            ValuesAreIcons = true,
            ImageLayout = DataGridViewImageCellLayout.Zoom,
            Width = 25,
            MinimumWidth = 25
        });
        foreach (var column in new[] { ("name", "Name", "datagridview_name", 307), ("size", "Size", "datagridview_size", 80),
            ("date", "Date", "datagridview_date", 115), ("permissions", "Permissions", "datagridview_attr", 90) })
        {
            androidFilesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = column.Item1,
                DataPropertyName = column.Item2,
                HeaderText = LocalizationText.Get(column.Item3),
                Width = column.Item4,
                MinimumWidth = column.Item1 == "name" ? 140 : column.Item4
            });
        }
        androidFilesGrid.Columns["name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        browserStatusLabel = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Padding = new Padding(24),
            BackColor = _theme.IsDark ? AppTheme.Surface : SystemColors.Window,
            ForeColor = _theme.IsDark ? AppTheme.Text : SystemColors.ControlText,
            AccessibleName = BrowserText("status"),
            Visible = false
        };
        androidFilesGrid.Controls.Add(browserStatusLabel);
        androidPathTextBox.KeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Enter)
                return;
            e.SuppressKeyPress = true;
            NavigateToDirectory(androidPathTextBox.Text);
        };
        ((IBrowserView)this).ShowStatus(BrowserStatus.Loading);
    }

    private static DataTable BrowserTable()
    {
        var table = new DataTable();
        table.Columns.Add("Icon", typeof(Icon));
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("Size", typeof(decimal));
        table.Columns.Add("Date", typeof(DateTime));
        table.Columns.Add("Permissions", typeof(string));
        table.Columns.Add("Entry", typeof(AndroidFile));
        return table;
    }

    void IBrowserView.ShowStatus(BrowserStatus status, string? detail)
    {
        if (IsDisposed)
            return;
        string key = status switch
        {
            BrowserStatus.InvalidListing => "formatError",
            BrowserStatus.InvalidPath => "invalidPath",
            BrowserStatus.NoDevice => "noDevice",
            BrowserStatus.Disconnected => "disconnected",
            BrowserStatus.ChooseDevice => "chooseDevice",
            BrowserStatus.Unauthorized => "unauthorized",
            BrowserStatus.Offline => "offline",
            BrowserStatus.Empty => "empty",
            BrowserStatus.Loading => "loading",
            _ => "readError"
        };
        androidFilesGrid.DataSource = BrowserTable();
        browserStatusLabel.Text = BrowserText(key) + (string.IsNullOrEmpty(detail) ? "" : Environment.NewLine + detail);
        browserStatusLabel.Visible = true;
        browserStatusLabel.BringToFront();
        downloadButton.Enabled = uploadButton.Enabled = createDirectoryButton.Enabled = false;
    }

    void IBrowserView.ShowFiles(string path, string deviceSerial, IReadOnlyList<AndroidFile> files)
    {
        if (IsDisposed)
            return;
        var table = BrowserTable();
        foreach (var file in files)
            table.Rows.Add(_icons.GetForFile(file.Name, file.IsDirectory), file.Name,
            file.Bytes.HasValue ? (object)decimal.Round((decimal)file.Bytes.Value / 1024, 3) : DBNull.Value,
            file.Modified.HasValue ? (object)file.Modified.Value : DBNull.Value, file.Permissions, file);
        androidFilesGrid.DataSource = table;
        browserStatusLabel.Text = BrowserText("empty");
        browserStatusLabel.Visible = files.Count == 0;
        if (browserStatusLabel.Visible)
            browserStatusLabel.BringToFront();
        downloadButton.Enabled = files.Count > 0;
        uploadButton.Enabled = createDirectoryButton.Enabled = true;
    }

    void IBrowserView.ShowDevices(IReadOnlyList<AndroidDevice> devices, string? selectedSerial)
    {
        if (IsDisposed)
            return;
        updatingDeviceList = true;
        try
        {
            deviceComboBox.Items.Clear();
            deviceComboBox.Items.Add(strings.defaultDevice);
            deviceComboBox.Items.Add(strings.addWireless);
            foreach (var device in devices)
                deviceComboBox.Items.Add(device.Model +
                (device.State == DeviceState.Ready ? "" : " (" + device.State + ")"));
            int index = devices.ToList().FindIndex(device => device.Serial == selectedSerial);
            if (selectedSerial != null && index < 0)
            {
                deviceComboBox.Items.Add(selectedSerial + " (" + BrowserText("disconnected") + ")");
                deviceComboBox.SelectedIndex = deviceComboBox.Items.Count - 1;
            }
            else
                deviceComboBox.SelectedIndex = index >= 0 ? index + 2 : 0;
            deviceComboBox.DropDownWidth = Math.Max(200, deviceComboBox.Items.Cast<string>()
                .Max(item => TextRenderer.MeasureText(item, deviceComboBox.Font).Width) + 20);
        }
        finally { updatingDeviceList = false; }
    }

    private async Task LoadAndroidDirectoryAsync(string path)
    {
        Task request = _browser.NavigateAsync(path);
        androidPathTextBox.Text = _browser.CurrentPath;
        await request;
    }

    private static AndroidFile? FileFromRow(DataGridViewRow? row) =>
        row?.DataBoundItem is DataRowView data && data["Entry"] is AndroidFile file ? file : null;
}
