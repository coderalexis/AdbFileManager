using System.Data;

namespace AdbFileManager;

internal partial class MainForm
{
    private Label browserStatusLabel = null!;
    private Panel browserFeedback = null!;
    private Button browserRecheckButton = null!, browserDetailsButton = null!;
    private string? browserErrorDetail;
    private IReadOnlyList<AndroidFile> folderFiles = Array.Empty<AndroidFile>();
    private BrowserSelection? rememberedSelection;
    private bool renderingFiles;
    private static string BrowserText(string key) => LocalizationText.Get("browser_" + key);

    private void InitializeBrowser()
    {
        androidFilesGrid.AutoGenerateColumns = false;
        androidFilesGrid.Columns.Clear();
        androidFilesGrid.Columns.Add(new DataGridViewImageColumn
        {
            Name = "icon",
            DataPropertyName = "Icon",
            HeaderText = "",
            ValuesAreIcons = true,
            ImageLayout = DataGridViewImageCellLayout.Zoom,
            Width = 28,
            MinimumWidth = 28
        });
        foreach (var column in new[] { ("name", "Name", "datagridview_name", 200), ("size", "Size", "datagridview_size", 80),
            ("date", "Date", "datagridview_date", 135), ("permissions", "Permissions", "datagridview_attr", 90) })
            androidFilesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = column.Item1,
                DataPropertyName = column.Item2,
                HeaderText = LocalizationText.Get(column.Item3),
                Width = column.Item4,
                MinimumWidth = column.Item1 == "name" ? 140 : column.Item4
            });
        androidFilesGrid.Columns["name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        androidFilesGrid.Columns["date"].DefaultCellStyle.Format = "g";
        androidFilesGrid.Columns["size"].DefaultCellStyle.Format = "N2";
        browserFeedback = new Panel { Name = "browserFeedback", Dock = DockStyle.Fill, Visible = false };
        var feedback = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(20) };
        feedback.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        feedback.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        feedback.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        feedback.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        browserStatusLabel = new Label
        {
            Name = "browserStatusLabel",
            Anchor = AnchorStyles.None,
            AutoSize = true,
            MaximumSize = new Size(600, 0),
            TextAlign = ContentAlignment.MiddleCenter,
            AccessibleName = BrowserText("status"),
            Padding = new Padding(8)
        };
        feedback.Controls.Add(browserStatusLabel, 0, 1);
        var actions = new FlowLayoutPanel { AutoSize = true, Anchor = AnchorStyles.None, FlowDirection = FlowDirection.LeftToRight };
        browserRecheckButton = Shortcut(Ux("checkAgain"), () => _ = LoadAndroidDirectoryAsync(CurrentAndroidPath));
        browserRecheckButton.Name = "browserRecheckButton";
        browserDetailsButton = Shortcut(Ux("details"), () => ShowErrorDetails(browserErrorDetail ?? browserStatusLabel.Text));
        browserDetailsButton.Name = "browserDetailsButton";
        actions.Controls.Add(browserRecheckButton);
        actions.Controls.Add(browserDetailsButton);
        feedback.Controls.Add(actions, 0, 2);
        browserFeedback.Controls.Add(feedback);
        androidFileArea.Controls.Add(browserFeedback);
        androidPathTextBox.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; NavigateToDirectory(androidPathTextBox.Text); } };
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
        folderFiles = Array.Empty<AndroidFile>();
        renderingFiles = true;
        try
        {
            androidFilesGrid.DataSource = BrowserTable();
        }
        finally { renderingFiles = false; }
        browserErrorDetail = detail;
        string message = status == BrowserStatus.NoExternalStorage ? Ux("noSd") : BrowserText(status switch
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
        });
        browserStatusLabel.Text = message;
        browserStatusLabel.MaximumSize = new Size(Math.Max(220, androidFileArea.Width - 60), 0);
        browserFeedback.Visible = true;
        browserFeedback.BringToFront();
        browserRecheckButton.Visible = status is not (BrowserStatus.Loading or BrowserStatus.Empty);
        browserDetailsButton.Visible = !string.IsNullOrEmpty(detail);
        connectionLabel.Text = Ux(status switch
        {
            BrowserStatus.Loading => "checking",
            BrowserStatus.NoDevice or BrowserStatus.Disconnected => "disconnected",
            BrowserStatus.Unauthorized => "unauthorized",
            BrowserStatus.Offline => "offline",
            BrowserStatus.ChooseDevice => "chooseDevice",
            _ => "attention"
        });
        connectionLabel.ForeColor = status == BrowserStatus.Loading ? (_theme.IsDark ? AppTheme.MutedText : SystemColors.GrayText) : (_theme.IsDark ? AppTheme.Error : Color.Firebrick);
        createDirectoryButton.Enabled = externalStorageButton.Enabled = false;
        UpdateBreadcrumbs();
        UpdateCopyActions();
    }

    void IBrowserView.ShowFiles(string path, string serial, IReadOnlyList<AndroidFile> files)
    {
        if (IsDisposed)
            return;
        folderFiles = files.ToArray();
        connectionLabel.Text = Ux("connected");
        connectionLabel.ForeColor = _theme.IsDark ? Color.LightGreen : Color.DarkGreen;
        browserErrorDetail = null;
        browserRecheckButton.Visible = browserDetailsButton.Visible = false;
        createDirectoryButton.Enabled = externalStorageButton.Enabled = true;
        RenderAndroidFiles();
        UpdateBreadcrumbs();
    }

    private void RememberAndroidSelection()
    {
        if (!IsBrowserReady || renderingFiles)
            return;
        rememberedSelection = new BrowserSelection(CurrentAndroidPath, ReadyDeviceSerial!, androidFilesGrid.SelectedRows.Cast<DataGridViewRow>()
            .Select(FileFromRow).OfType<AndroidFile>().Select(file => file.Name).ToHashSet(StringComparer.Ordinal));
    }

    private void ApplyAndroidFilter()
    {
        if (!IsBrowserReady)
            return;
        RememberAndroidSelection();
        RenderAndroidFiles();
    }

    private void RenderAndroidFiles()
    {
        var visible = BrowserFileFilter.Apply(folderFiles, androidFilterTextBox.Text);
        var restored = rememberedSelection?.Restore(CurrentAndroidPath, ReadyDeviceSerial!, visible) ?? new HashSet<string>();
        var table = BrowserTable();
        foreach (var file in visible)
            table.Rows.Add(_icons.GetForFile(file.Name, file.IsDirectory), file.Name,
            file.Bytes.HasValue && !file.IsDirectory ? (object)decimal.Round((decimal)file.Bytes.Value / 1024, 3) : DBNull.Value,
            file.Modified.HasValue ? (object)file.Modified.Value : DBNull.Value, file.Permissions, file);
        renderingFiles = true;
        try
        {
            androidFilesGrid.DataSource = table;
            androidFilesGrid.ClearSelection();
            foreach (DataGridViewRow row in androidFilesGrid.Rows)
                if (FileFromRow(row) is AndroidFile file && restored.Contains(file.Name))
                    row.Selected = true;
        }
        finally { renderingFiles = false; }
        browserStatusLabel.Text = Ux(folderFiles.Count == 0 ? "selectionNone" : "noMatches");
        if (folderFiles.Count == 0)
            browserStatusLabel.Text = BrowserText("empty");
        browserFeedback.Visible = visible.Count == 0;
        if (browserFeedback.Visible)
            browserFeedback.BringToFront();
        browserRecheckButton.Visible = browserDetailsButton.Visible = false;
        UpdateCopyActions();
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
                deviceComboBox.Items.Add(device.Model.Replace('_', ' ') + (device.State == DeviceState.Ready ? "" : " · " + Ux(device.State == DeviceState.Unauthorized ? "unauthorized" : "offline")));
            int index = devices.ToList().FindIndex(device => device.Serial == selectedSerial);
            if (selectedSerial != null && index < 0)
            {
                deviceComboBox.Items.Add(selectedSerial + " · " + Ux("disconnected"));
                deviceComboBox.SelectedIndex = deviceComboBox.Items.Count - 1;
            }
            else
                deviceComboBox.SelectedIndex = index >= 0 ? index + 2 : 0;
            var shown = index >= 0 ? devices[index] : devices.Count == 1 ? devices[0] : null;
            androidPaneTitle.Text = Ux("android") + (shown == null ? "" : " · " + shown.Model.Replace('_', ' '));
            deviceComboBox.DropDownWidth = Math.Max(240, deviceComboBox.Items.Cast<string>().Max(item => TextRenderer.MeasureText(item, deviceComboBox.Font).Width) + 20);
        }
        finally { updatingDeviceList = false; }
    }

    private async Task LoadAndroidDirectoryAsync(string path)
    {
        RememberAndroidSelection();
        Task request = _browser.NavigateAsync(path);
        androidPathTextBox.Text = _browser.CurrentPath;
        UpdateBreadcrumbs();
        await request;
    }

    private async Task NavigateExternalStorageAsync()
    {
        RememberAndroidSelection();
        await _browser.NavigateToExternalStorageAsync();
        if (!IsDisposed)
        {
            androidPathTextBox.Text = CurrentAndroidPath;
            UpdateBreadcrumbs();
        }
    }

    private static AndroidFile? FileFromRow(DataGridViewRow? row) => row?.DataBoundItem is DataRowView data && data["Entry"] is AndroidFile file ? file : null;

    private void UpdateCopyActions()
    {
        if (renderingFiles || copyPreviewLabel == null)
            return;
        var android = androidFilesGrid.SelectedRows.Cast<DataGridViewRow>().Select(FileFromRow).OfType<AndroidFile>().ToArray();
        var local = localFilesBrowser.SelectedItems?.Select(item => item.ParsingName).OfType<string>().Where(path => System.IO.File.Exists(path) || Directory.Exists(path)).ToArray() ?? Array.Empty<string>();
        androidSelectionLabel.Text = android.Length == 0 ? Ux("selectionNone") : android.Any(file => file.IsDirectory)
            ? string.Format(Ux("selectedFolders"), android.Length, android.Count(file => file.IsDirectory))
            : string.Format(Ux("selectedSize"), android.Length, android.Sum(file => file.Bytes ?? 0) / 1048576d);
        localSelectionLabel.Text = local.Length == 0 ? Ux("selectionNone") : string.Format(Ux("selected"), local.Length);
        string? destination = localFilesBrowser.NavigationLog.CurrentLocation?.ParsingName;
        bool hasLocalFolder = destination != null && Directory.Exists(destination);
        downloadButton.Enabled = IsBrowserReady && android.Length > 0 && hasLocalFolder;
        uploadButton.Enabled = IsBrowserReady && local.Length > 0;
        string down = hasLocalFolder ? string.Format(Ux("previewDown"), android.Length, destination) : Ux("chooseLocal");
        string up = string.Format(Ux("previewUp"), local.Length, CurrentAndroidPath);
        toolTip1.SetToolTip(downloadButton, down);
        toolTip1.SetToolTip(uploadButton, up);
        copyPreviewLabel.Text = localSelectionActive && local.Length > 0 ? up : android.Length > 0 ? down : Ux("copyHint");
        toolTip1.SetToolTip(copyPreviewLabel, copyPreviewLabel.Text);
    }
}
