#nullable enable
namespace AdbFileManager;

internal partial class MainForm
{
    private System.ComponentModel.IContainer? components;
    protected override void Dispose(bool disposing)
    {
        if (disposing) { components?.Dispose(); _browser.Dispose(); }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        var resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        androidFilesGrid = new DataGridView { Name = "androidFilesGrid", Dock = DockStyle.Fill, ReadOnly = true,
            EditMode = DataGridViewEditMode.EditProgrammatically, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false, RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AccessibleName = UserInterfaceText.Get("android"), TabIndex = 6 };
        androidFilesGrid.CellMouseDoubleClick += OnAndroidFileDoubleClick;
        androidFilesGrid.ColumnHeaderMouseDoubleClick += OnAndroidHeaderDoubleClick;
        androidFilesGrid.KeyDown += OnAndroidGridKeyDown;
        initialLoadTimer = new System.Windows.Forms.Timer(components) { Interval = 500 };
        initialLoadTimer.Tick += OnInitialLoad;
        toolTip1 = new ToolTip(components) { AutoPopDelay = 10000 };
        androidPathTextBox = new TextBox { Name = "androidPathTextBox", Dock = DockStyle.Fill, TabIndex = 3 };
        localPathTextBox = new TextBox { Name = "localPathTextBox", Dock = DockStyle.Fill, TabIndex = 10 };
        localPathTextBox.KeyPress += OnLocalPathKeyPress;
        parentDirectoryButton = new Button { Name = "parentDirectoryButton", AutoSize = true, Text = "↑ " + UserInterfaceText.Get("up") };
        parentDirectoryButton.Click += OnParentDirectoryClick;
        downloadButton = new Button { Name = "downloadButton", Text = UserInterfaceText.Get("download") + " →", Dock = DockStyle.Fill, TabIndex = 7 };
        uploadButton = new Button { Name = "uploadButton", Text = "← " + UserInterfaceText.Get("upload"), Dock = DockStyle.Fill, TabIndex = 8 };
        downloadButton.Click += OnDownloadClick;
        uploadButton.Click += OnUploadClick;
        refreshButton = new Button { Name = "refreshButton", AutoSize = true, Text = UserInterfaceText.Get("refresh"), TabIndex = 1 };
        refreshButton.Click += OnRefreshClick;
        createDirectoryButton = new Button { Name = "createDirectoryButton", AutoSize = true, Text = "+ " + UserInterfaceText.Get("newFolder"), TabIndex = 2 };
        createDirectoryButton.Click += OnCreateDirectoryClick;
        settingsButton = new Button { Name = "settingsButton", AutoSize = true, Text = UserInterfaceText.Get("settings") };
        settingsButton.Click += OnSettingsClick;
        unlockButton = new Button { Name = "unlockButton", AutoSize = true, Text = UserInterfaceText.Get("unlock") };
        unlockButton.Click += OnUnlockClick;
        consoleButton = new Button { Name = "consoleButton", AutoSize = true, Text = ">_", AccessibleName = UserInterfaceText.Get("console") };
        consoleButton.Click += OnConsoleToggle;
        deviceComboBox = new ComboBox { Name = "deviceComboBox", DropDownStyle = ComboBoxStyle.DropDownList, Width = 230, TabIndex = 0 };
        deviceComboBox.SelectedIndexChanged += OnDeviceSelectionChanged;
        localBackButton = new Button { Name = "localBackButton", Width = 34, Height = 30, AccessibleName = UserInterfaceText.Get("back") };
        localForwardButton = new Button { Name = "localForwardButton", Width = 34, Height = 30, AccessibleName = UserInterfaceText.Get("forward") };
        localBackButton.Click += OnLocalBackClick;
        localForwardButton.Click += OnLocalForwardClick;
        localBackButton.MouseEnter += OnLocalBackEnter; localBackButton.MouseLeave += OnLocalBackLeave;
        localBackButton.MouseDown += OnLocalBackDown; localBackButton.MouseUp += OnLocalBackUp;
        localForwardButton.MouseEnter += OnLocalForwardEnter; localForwardButton.MouseLeave += OnLocalForwardLeave;
        localForwardButton.MouseDown += OnLocalForwardDown; localForwardButton.MouseUp += OnLocalForwardUp;
        localFilesBrowser = new Microsoft.WindowsAPICodePack.Controls.WindowsForms.ExplorerBrowser {
            Name = "localFilesBrowser", Dock = DockStyle.Fill, AccessibleName = UserInterfaceText.Get("pc"), TabIndex = 11 };
        localFilesBrowser.NavigationComplete += OnLocalNavigationComplete;
        localFilesBrowser.SelectionChanged += OnLocalSelectionChanged;
        localFilesBrowser.Load += OnLocalBrowserLoad;
        localFilesBrowser.PreviewKeyDown += OnLocalBrowserKeyDown;
        footerPanel = new Panel { Dock = DockStyle.Fill, Name = "statusFooter", Height = 24 };
        versionLabel = new LinkLabel { AutoSize = true, Dock = DockStyle.Right, Name = "versionLabel" };
        versionLabel.LinkClicked += OnVersionLinkClick;
        apkAssistantPanel = new Panel { Visible = false, AutoSize = true, Dock = DockStyle.Top };
        installApkLink = new DarkModeControls.DarkCommandLink { AutoSize = true, Text = UserInterfaceText.Get("installApk"), Dock = DockStyle.Fill };
        installApkLink.Click += OnInstallApkClick;
        dismissApkLink = new DarkModeControls.DarkCommandLink { Visible = false };
        apkAssistantLabel = new Label { Visible = false };
        Font = new Font("Segoe UI", 9F);
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(1280, 840);
        MinimumSize = new Size(900, 600);
        Text = UserInterfaceText.Get("title");
        Icon = resources.GetObject("$this.Icon") as Icon ?? SystemIcons.Application;
        KeyPreview = true;
        FormClosing += OnMainClosing;
        FormClosed += OnMainClosed;
        Load += OnMainLoad;
        Resize += OnMainResize;
    }

    internal DataGridView androidFilesGrid = null!;
    private System.Windows.Forms.Timer initialLoadTimer = null!;
    internal TextBox androidPathTextBox = null!, localPathTextBox = null!;
    private ToolTip toolTip1 = null!;
    internal Button parentDirectoryButton = null!, downloadButton = null!, uploadButton = null!, refreshButton = null!, createDirectoryButton = null!;
    internal Button settingsButton = null!, unlockButton = null!, consoleButton = null!, localBackButton = null!, localForwardButton = null!;
    internal ComboBox deviceComboBox = null!;
    internal Microsoft.WindowsAPICodePack.Controls.WindowsForms.ExplorerBrowser localFilesBrowser = null!;
    private Panel footerPanel = null!, apkAssistantPanel = null!;
    private LinkLabel versionLabel = null!;
    private DarkModeControls.DarkCommandLink installApkLink = null!, dismissApkLink = null!;
    private Label apkAssistantLabel = null!;
}
