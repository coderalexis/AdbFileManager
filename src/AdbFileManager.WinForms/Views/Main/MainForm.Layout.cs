using Microsoft.WindowsAPICodePack.Shell;

namespace AdbFileManager;

internal partial class MainForm
{
    private TableLayoutPanel workspaceLayout = null!;
    private Panel androidFileArea = null!;
    private Panel transferDockHolder = null!;
    private Label androidPaneTitle = null!, connectionLabel = null!, androidSelectionLabel = null!, localSelectionLabel = null!, copyPreviewLabel = null!;
    private TextBox androidFilterTextBox = null!;
    private ToolStrip androidBreadcrumbs = null!;
    private Button externalStorageButton = null!;
    private bool localSelectionActive;
    private readonly List<TableLayoutPanel> paneLayouts = new();
    private TableLayoutPanel copyActionsLayout = null!, filePanesLayout = null!;
    private float layoutScale;

    private static string Ux(string key) => UserInterfaceText.Get(key);
    private static Label Heading(string text) => new() { Text = text, Dock = DockStyle.Fill, Font = new Font("Segoe UI Semibold", 12F), TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true };
    private Button Shortcut(string text, Action action)
    {
        var button = new Button { Text = text, AutoSize = true, MinimumSize = new Size(70, 28), AccessibleName = text };
        button.Click += (_, _) => action();
        return button;
    }

    private void InitializeWorkspace()
    {
        SuspendLayout();
        workspaceLayout = new TableLayoutPanel { Name = "workspaceLayout", Dock = DockStyle.Fill, Padding = new Padding(12), ColumnCount = 1, RowCount = 4 };
        workspaceLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        workspaceLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        workspaceLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        workspaceLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        var toolbar = new FlowLayoutPanel { Name = "mainToolbar", Dock = DockStyle.Fill, AutoSize = true, WrapContents = true, Padding = new Padding(0, 0, 0, 8) };
        toolbar.Controls.Add(new Label { Text = Ux("device"), AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        toolbar.Controls.Add(deviceComboBox);
        connectionLabel = new Label { Name = "connectionLabel", AutoSize = true, Text = Ux("checking"), Padding = new Padding(8, 8, 12, 0), AccessibleName = Ux("device") };
        toolbar.Controls.Add(connectionLabel);
        toolbar.Controls.Add(refreshButton);
        toolbar.Controls.Add(createDirectoryButton);
        toolbar.Controls.Add(settingsButton);
        toolbar.Controls.Add(unlockButton);
        toolbar.Controls.Add(consoleButton);
        workspaceLayout.Controls.Add(toolbar, 0, 0);

        var panes = new TableLayoutPanel { Name = "filePanes", Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Margin = Padding.Empty };
        filePanesLayout = panes;
        panes.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        panes.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
        panes.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        panes.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var androidPane = Pane();
        paneLayouts.Add(androidPane);
        androidPane.Name = "androidPane";
        androidPaneTitle = Heading(Ux("android"));
        androidPaneTitle.Name = "androidPaneTitle";
        androidPane.Controls.Add(androidPaneTitle, 0, 0);
        androidBreadcrumbs = new ToolStrip { Name = "androidBreadcrumbs", Dock = DockStyle.Fill, GripStyle = ToolStripGripStyle.Hidden, AccessibleName = Ux("breadcrumb"), CanOverflow = true };
        androidPane.Controls.Add(androidBreadcrumbs, 0, 1);
        androidPathTextBox.PlaceholderText = Ux("path");
        androidPathTextBox.AccessibleName = Ux("path");
        var androidAddress = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty };
        androidAddress.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        androidAddress.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        androidAddress.Controls.Add(androidPathTextBox, 0, 0);
        androidAddress.Controls.Add(parentDirectoryButton, 1, 0);
        androidPane.Controls.Add(androidAddress, 0, 2);
        var androidShortcuts = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        androidShortcuts.Controls.Add(Shortcut(Ux("camera"), () => NavigateToDirectory("/sdcard/DCIM/")));
        androidShortcuts.Controls.Add(Shortcut(Ux("downloads"), () => NavigateToDirectory("/sdcard/Download/")));
        externalStorageButton = Shortcut(Ux("sd"), () => _ = NavigateExternalStorageAsync());
        externalStorageButton.Name = "externalStorageButton";
        androidShortcuts.Controls.Add(externalStorageButton);
        androidPane.Controls.Add(androidShortcuts, 0, 3);
        var filterRow = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty };
        filterRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        filterRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        androidFilterTextBox = new TextBox { Name = "androidFilterTextBox", Dock = DockStyle.Fill, PlaceholderText = Ux("search"), AccessibleName = Ux("search"), TabIndex = 5 };
        androidFilterTextBox.TextChanged += (_, _) => ApplyAndroidFilter();
        var clearFilter = Shortcut("×", () => androidFilterTextBox.Clear());
        clearFilter.AccessibleName = Ux("clearFilter");
        toolTip1.SetToolTip(clearFilter, Ux("clearFilter"));
        filterRow.Controls.Add(androidFilterTextBox, 0, 0);
        filterRow.Controls.Add(clearFilter, 1, 0);
        androidPane.Controls.Add(filterRow, 0, 4);
        androidFileArea = new Panel { Name = "androidFileArea", Dock = DockStyle.Fill, Margin = Padding.Empty };
        androidFileArea.Controls.Add(androidFilesGrid);
        androidPane.Controls.Add(androidFileArea, 0, 5);
        androidSelectionLabel = new Label { Name = "androidSelectionLabel", Dock = DockStyle.Fill, AutoEllipsis = true, Text = Ux("selectionNone"), AccessibleName = Ux("androidSelection") };
        androidPane.Controls.Add(androidSelectionLabel, 0, 6);
        panes.Controls.Add(androidPane, 0, 0);

        var actions = new TableLayoutPanel { Name = "copyActions", Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Padding = new Padding(10) };
        copyActionsLayout = actions;
        actions.RowStyles.Add(new RowStyle(SizeType.Percent, 30));
        actions.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        actions.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        actions.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
        actions.RowStyles.Add(new RowStyle(SizeType.Percent, 70));
        actions.Controls.Add(downloadButton, 0, 1);
        actions.Controls.Add(uploadButton, 0, 2);
        copyPreviewLabel = new Label { Name = "copyPreviewLabel", Dock = DockStyle.Fill, AutoEllipsis = true, Text = Ux("copyHint"), TextAlign = ContentAlignment.TopLeft, Padding = new Padding(0, 12, 0, 0), AccessibleName = Ux("copyHint") };
        actions.Controls.Add(copyPreviewLabel, 0, 3);
        panes.Controls.Add(actions, 1, 0);

        var localPane = Pane();
        paneLayouts.Add(localPane);
        localPane.Name = "localPane";
        localPane.Controls.Add(Heading(Ux("pc")), 0, 0);
        var localAddress = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Margin = Padding.Empty };
        localAddress.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        localAddress.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        localAddress.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        localPathTextBox.AccessibleName = Ux("localPath");
        localPathTextBox.PlaceholderText = Ux("localPath");
        localAddress.Controls.Add(localBackButton, 0, 0);
        localAddress.Controls.Add(localForwardButton, 1, 0);
        localAddress.Controls.Add(localPathTextBox, 2, 0);
        localPane.Controls.Add(localAddress, 0, 1);
        var localShortcuts = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        localShortcuts.Controls.Add(Shortcut(Ux("pc"), () => NavigateLocal("shell:MyComputerFolder")));
        localShortcuts.Controls.Add(Shortcut(Ux("desktop"), () => NavigateLocal(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory))));
        localShortcuts.Controls.Add(Shortcut(Ux("downloads"), () => NavigateLocal(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"))));
        localPane.Controls.Add(localShortcuts, 0, 2);
        localPane.Controls.Add(new Label { Text = Ux("localHint"), Dock = DockStyle.Fill, AutoEllipsis = true, TextAlign = ContentAlignment.MiddleLeft }, 0, 3);
        apkAssistantPanel.Controls.Add(installApkLink);
        localPane.Controls.Add(apkAssistantPanel, 0, 4);
        localPane.Controls.Add(localFilesBrowser, 0, 5);
        localSelectionLabel = new Label { Name = "localSelectionLabel", Dock = DockStyle.Fill, AutoEllipsis = true, Text = Ux("selectionNone"), AccessibleName = Ux("localSelection") };
        localPane.Controls.Add(localSelectionLabel, 0, 6);
        panes.Controls.Add(localPane, 2, 0);
        workspaceLayout.Controls.Add(panes, 0, 1);
        transferDockHolder = new Panel { Name = "transferDockHolder", Dock = DockStyle.Fill, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
        workspaceLayout.Controls.Add(transferDockHolder, 0, 2);
        footerPanel.Controls.Add(versionLabel);
        workspaceLayout.Controls.Add(footerPanel, 0, 3);
        Controls.Add(workspaceLayout);
        androidFilesGrid.SelectionChanged += (_, _) => UpdateCopyActions();
        androidFilesGrid.Enter += (_, _) => { localSelectionActive = false; UpdateCopyActions(); };
        localFilesBrowser.Enter += (_, _) => { localSelectionActive = true; UpdateCopyActions(); };
        Shown += (_, _) => { ApplyWorkspaceScale(DeviceDpi / 96f); UpdateCopyActions(); };
        DpiChanged += (_, _) => ApplyWorkspaceScale(DeviceDpi / 96f);
        toolTip1.SetToolTip(refreshButton, Ux("refresh") + " (F5)");
        toolTip1.SetToolTip(androidFilterTextBox, "Ctrl+F");
        toolTip1.SetToolTip(androidPathTextBox, Ux("path") + " (Ctrl+L)");
        toolTip1.SetToolTip(localBackButton, Ux("back"));
        toolTip1.SetToolTip(localForwardButton, Ux("forward"));
        toolTip1.SetToolTip(consoleButton, Ux("console"));
        foreach (Control control in toolbar.Controls)
            if (string.IsNullOrEmpty(control.AccessibleName))
                control.AccessibleName = control.Text;
        ResumeLayout(true);
    }

    private static TableLayoutPanel Pane()
    {
        var pane = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 7, Margin = Padding.Empty };
        foreach (int height in new[] { 32, 34, 36, 36, 34 })
            pane.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
        pane.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        pane.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        return pane;
    }

    private void NavigateLocal(string path)
    {
        try
        {
            var location = ShellObject.FromParsingName(path);
            if (location != null)
                localFilesBrowser.Navigate(location);
        }
        catch (Exception ex) { ShowErrorDetails(ex.Message); }
    }

    private void OnMainResize(object? sender, EventArgs e)
    {
        // Docking/table layout handles resizing and DPI. No control position depends on window border estimates.
        workspaceLayout?.PerformLayout();
        if (browserStatusLabel != null && androidFileArea != null)
            browserStatusLabel.MaximumSize = new Size(Math.Max(220, androidFileArea.Width - 60), 0);
    }

    internal void ApplyWorkspaceScale(float scale)
    {
        if (workspaceLayout == null || Math.Abs(layoutScale - scale) < 0.01f)
            return;
        layoutScale = scale;
        MinimumSize = new Size((int)(900 * scale), (int)(680 * scale));
        workspaceLayout.SuspendLayout();
        try
        {
            filePanesLayout.ColumnStyles[1].Width = 210 * scale;
            foreach (var pane in paneLayouts)
            {
                int[] heights = { 34, 34, 36, 36, 34 };
                for (int i = 0; i < heights.Length; i++)
                    pane.RowStyles[i].Height = heights[i] * scale;
                pane.RowStyles[6].Height = 32 * scale;
            }
            copyActionsLayout.RowStyles[1].Height = copyActionsLayout.RowStyles[2].Height = 48 * scale;
            copyActionsLayout.RowStyles[3].Height = 150 * scale;
            workspaceLayout.RowStyles[3].Height = 30 * scale;
            localBackButton.Width = localForwardButton.Width = (int)(34 * scale);
            localBackButton.Height = localForwardButton.Height = (int)(30 * scale);
            deviceComboBox.Width = (int)(230 * scale);
            androidFilesGrid.Columns["name"].MinimumWidth = (int)(140 * scale);
            foreach (var column in new[] { ("icon", 28), ("size", 90), ("date", 135), ("permissions", 90) })
            {
                var gridColumn = androidFilesGrid.Columns[column.Item1];
                gridColumn.MinimumWidth = (int)(column.Item2 * scale);
                gridColumn.Width = gridColumn.MinimumWidth;
            }
        }
        finally { workspaceLayout.ResumeLayout(true); }
    }

    private void UpdateBreadcrumbs()
    {
        androidBreadcrumbs.Items.Clear();
        foreach (var segment in AndroidBreadcrumb.Build(CurrentAndroidPath))
        {
            var button = new ToolStripButton(segment.Label) { AccessibleName = segment.Path, ToolTipText = segment.Path, Tag = segment.Path };
            button.Click += (_, _) => NavigateToDirectory(segment.Path);
            androidBreadcrumbs.Items.Add(button);
        }
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.F5)
        {
            _ = LoadAndroidDirectoryAsync(CurrentAndroidPath);
            return true;
        }
        if (keyData == (Keys.Control | Keys.F))
        {
            androidFilterTextBox.Focus();
            return true;
        }
        if (keyData == (Keys.Control | Keys.L))
        {
            if (localSelectionActive)
                localPathTextBox.Focus();
            else
                androidPathTextBox.Focus();
            return true;
        }
        if (keyData == (Keys.Alt | Keys.Up))
        {
            if (localSelectionActive)
            {
                string? path = localFilesBrowser.NavigationLog.CurrentLocation?.ParsingName;
                if (path != null && Directory.Exists(path) && Directory.GetParent(path)?.FullName is string parent)
                    NavigateLocal(parent);
            }
            else
                NavigateToParent();
            return true;
        }
        if (keyData == Keys.Escape && androidFilterTextBox.Focused)
        {
            androidFilterTextBox.Clear();
            androidFilesGrid.Focus();
            return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }
}
