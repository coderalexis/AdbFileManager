using DarkModeControls;
using Timer = System.Windows.Forms.Timer;

namespace AdbFileManager {
	partial class MainForm {
		/// <summary>
		///  Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		///  Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if(disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support_do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            androidFilesGrid = new DataGridView();
            initialLoadTimer = new Timer(components);
            androidPathTextBox = new TextBox();
            toolTip1 = new ToolTip(components);
            createDirectoryButton = new randz.CustomControls.VerticalLabel();
            localPathTextBox = new TextBox();
            parentDirectoryButton = new Button();
            unlockButton = new FluentButton();
            downloadButton = new FluentButton();
            uploadButton = new FluentButton();
            localForwardButton = new Button();
            localBackButton = new Button();
            deco_panel4 = new Panel();
            settingsButton = new FluentButton();
            footerPanel = new Panel();
            deviceComboBox = new ComboBox();
            consoleButton = new Button();
            deco_panel6 = new Panel();
            comboBox_lang = new ComboBox();
            versionLabel = new LinkLabel();
            transferActionsPanel = new Panel();
            refreshButton = new randz.CustomControls.VerticalLabel();
            deco_panel1 = new Panel();
            deco_panel5 = new Panel();
            deco_panel3 = new Panel();
            deco_panel2 = new Panel();
            localFilesBrowser = new Microsoft.WindowsAPICodePack.Controls.WindowsForms.ExplorerBrowser();
            apkAssistantPanel = new Panel();
            apkAssistantLabel = new Label();
            installApkLink = new DarkCommandLink();
            dismissApkLink = new DarkCommandLink();
            pictureBox1 = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)androidFilesGrid).BeginInit();
            deco_panel4.SuspendLayout();
            footerPanel.SuspendLayout();
            transferActionsPanel.SuspendLayout();
            apkAssistantPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            //
            // androidFilesGrid
            //
            androidFilesGrid.AllowUserToAddRows = false;
            androidFilesGrid.AllowUserToDeleteRows = false;
            androidFilesGrid.AllowUserToResizeRows = false;
            resources.ApplyResources(androidFilesGrid, "androidFilesGrid");
            androidFilesGrid.BackgroundColor = SystemColors.ButtonHighlight;
            androidFilesGrid.GridColor = Color.White;
            androidFilesGrid.Name = "androidFilesGrid";
            androidFilesGrid.ReadOnly = true;
            androidFilesGrid.EditMode = DataGridViewEditMode.EditProgrammatically;
            androidFilesGrid.RowHeadersVisible = false;
            androidFilesGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            androidFilesGrid.CellMouseDoubleClick += OnAndroidFileDoubleClick;
            androidFilesGrid.ColumnHeaderMouseDoubleClick += OnAndroidHeaderDoubleClick;
            androidFilesGrid.KeyDown += OnAndroidGridKeyDown;
            //
            // initialLoadTimer
            //
            initialLoadTimer.Interval = 500;
            initialLoadTimer.Tick += OnInitialLoad;
            //
            // androidPathTextBox
            //
            resources.ApplyResources(androidPathTextBox, "androidPathTextBox");
            androidPathTextBox.Name = "androidPathTextBox";
            //
            // createDirectoryButton
            //
            resources.ApplyResources(createDirectoryButton, "createDirectoryButton");
            createDirectoryButton.BackColor = SystemColors.ControlLight;
            createDirectoryButton.ForeColor = SystemColors.ControlText;
            createDirectoryButton.Name = "createDirectoryButton";
            createDirectoryButton.RenderingMode = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            createDirectoryButton.TextDrawMode = randz.CustomControls.DrawMode.TopBottom;
            toolTip1.SetToolTip(createDirectoryButton, resources.GetString("createDirectoryButton.ToolTip"));
            createDirectoryButton.TransparentBackground = false;
            createDirectoryButton.UseFluent = false;
            createDirectoryButton.Click += OnCreateDirectoryClick;
            //
            // localPathTextBox
            //
            resources.ApplyResources(localPathTextBox, "localPathTextBox");
            localPathTextBox.Name = "localPathTextBox";
            toolTip1.SetToolTip(localPathTextBox, resources.GetString("localPathTextBox.ToolTip"));
            localPathTextBox.KeyPress += OnLocalPathKeyPress;
            //
            // parentDirectoryButton
            //
            resources.ApplyResources(parentDirectoryButton, "parentDirectoryButton");
            parentDirectoryButton.Name = "parentDirectoryButton";
            toolTip1.SetToolTip(parentDirectoryButton, resources.GetString("parentDirectoryButton.ToolTip"));
            parentDirectoryButton.UseVisualStyleBackColor = true;
            parentDirectoryButton.Click += OnParentDirectoryClick;
            //
            // unlockButton
            //
            unlockButton.BackColor = SystemColors.ControlLight;
            unlockButton.FlatAppearance.BorderSize = 0;
            resources.ApplyResources(unlockButton, "unlockButton");
            unlockButton.ForeColor = Color.Black;
            unlockButton.Image = Properties.Resources.unlock16;
            unlockButton.Name = "unlockButton";
            unlockButton.UseFluent = false;
            unlockButton.UseVisualStyleBackColor = false;
            unlockButton.Click += OnUnlockClick;
            //
            //
            // downloadButton
            //
            downloadButton.BackColor = SystemColors.ControlLight;
            downloadButton.FlatAppearance.BorderSize = 0;
            resources.ApplyResources(downloadButton, "downloadButton");
            downloadButton.ForeColor = Color.Black;
            downloadButton.Name = "downloadButton";
            downloadButton.UseFluent = false;
            downloadButton.UseVisualStyleBackColor = false;
            downloadButton.Click += OnDownloadClick;
            //
            // uploadButton
            //
            uploadButton.BackColor = SystemColors.ControlLight;
            uploadButton.FlatAppearance.BorderSize = 0;
            resources.ApplyResources(uploadButton, "uploadButton");
            uploadButton.ForeColor = Color.Black;
            uploadButton.Name = "uploadButton";
            uploadButton.UseFluent = false;
            uploadButton.UseVisualStyleBackColor = false;
            uploadButton.Click += OnUploadClick;
            //
            // localForwardButton
            //
            localForwardButton.BackColor = Color.Transparent;
            localForwardButton.FlatAppearance.BorderSize = 0;
            resources.ApplyResources(localForwardButton, "localForwardButton");
            localForwardButton.Name = "localForwardButton";
            localForwardButton.UseVisualStyleBackColor = false;
            localForwardButton.Click += OnLocalForwardClick;
            localForwardButton.MouseDown += OnLocalForwardDown;
            localForwardButton.MouseEnter += OnLocalForwardEnter;
            localForwardButton.MouseLeave += OnLocalForwardLeave;
            localForwardButton.MouseUp += OnLocalForwardUp;
            //
            // localBackButton
            //
            localBackButton.BackColor = Color.Transparent;
            localBackButton.FlatAppearance.BorderSize = 0;
            resources.ApplyResources(localBackButton, "localBackButton");
            localBackButton.Name = "localBackButton";
            localBackButton.UseVisualStyleBackColor = false;
            localBackButton.Click += OnLocalBackClick;
            localBackButton.MouseDown += OnLocalBackDown;
            localBackButton.MouseEnter += OnLocalBackEnter;
            localBackButton.MouseLeave += OnLocalBackLeave;
            localBackButton.MouseUp += OnLocalBackUp;
            //
            // deco_panel4
            //
            deco_panel4.BackColor = Color.Gray;
            deco_panel4.Controls.Add(settingsButton);
            resources.ApplyResources(deco_panel4, "deco_panel4");
            deco_panel4.Name = "deco_panel4";
            //
            // settingsButton
            //
            settingsButton.BackColor = SystemColors.ControlLight;
            settingsButton.FlatAppearance.BorderSize = 0;
            resources.ApplyResources(settingsButton, "settingsButton");
            settingsButton.ForeColor = Color.Black;
            settingsButton.Name = "settingsButton";
            settingsButton.UseFluent = false;
            settingsButton.UseVisualStyleBackColor = false;
            settingsButton.Click += OnSettingsClick;
            //
            // footerPanel
            //
            footerPanel.BackColor = Color.FromArgb(192, 255, 255);
            footerPanel.Controls.Add(deviceComboBox);
            footerPanel.Controls.Add(consoleButton);
            footerPanel.Controls.Add(unlockButton);
            footerPanel.Controls.Add(deco_panel6);
            footerPanel.Controls.Add(deco_panel4);
            footerPanel.Controls.Add(comboBox_lang);
            footerPanel.Controls.Add(versionLabel);
            resources.ApplyResources(footerPanel, "footerPanel");
            footerPanel.Name = "footerPanel";
            //
            // deviceComboBox
            //
            deviceComboBox.AutoCompleteMode = AutoCompleteMode.Suggest;
            resources.ApplyResources(deviceComboBox, "deviceComboBox");
            deviceComboBox.FormattingEnabled = true;
            deviceComboBox.Name = "deviceComboBox";
            deviceComboBox.SelectedIndexChanged += OnDeviceSelectionChanged;
            //
            // consoleButton
            //
            consoleButton.BackColor = Color.Transparent;
            consoleButton.FlatAppearance.BorderSize = 0;
            resources.ApplyResources(consoleButton, "consoleButton");
            consoleButton.Name = "consoleButton";
            consoleButton.UseVisualStyleBackColor = false;
            consoleButton.Click += OnConsoleToggle;
            //
            // deco_panel6
            //
            deco_panel6.BackColor = Color.Gray;
            resources.ApplyResources(deco_panel6, "deco_panel6");
            deco_panel6.Name = "deco_panel6";
            //
            // comboBox_lang
            //
            comboBox_lang.FormattingEnabled = true;
            resources.ApplyResources(comboBox_lang, "comboBox_lang");
            comboBox_lang.Name = "comboBox_lang";
            //
            // versionLabel
            //
            resources.ApplyResources(versionLabel, "versionLabel");
            versionLabel.Name = "versionLabel";
            versionLabel.TabStop = true;
            versionLabel.LinkClicked += OnVersionLinkClick;
            //
            // transferActionsPanel
            //
            resources.ApplyResources(transferActionsPanel, "transferActionsPanel");
            transferActionsPanel.Controls.Add(createDirectoryButton);
            transferActionsPanel.Controls.Add(refreshButton);
            transferActionsPanel.Controls.Add(downloadButton);
            transferActionsPanel.Controls.Add(uploadButton);
            transferActionsPanel.Controls.Add(deco_panel1);
            transferActionsPanel.Controls.Add(deco_panel5);
            transferActionsPanel.Controls.Add(deco_panel3);
            transferActionsPanel.Controls.Add(deco_panel2);
            transferActionsPanel.Name = "transferActionsPanel";
            //
            // refreshButton
            //
            resources.ApplyResources(refreshButton, "refreshButton");
            refreshButton.BackColor = SystemColors.ControlLight;
            refreshButton.ForeColor = SystemColors.ControlText;
            refreshButton.Name = "refreshButton";
            refreshButton.RenderingMode = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            refreshButton.TextDrawMode = randz.CustomControls.DrawMode.TopBottom;
            refreshButton.TransparentBackground = false;
            refreshButton.UseFluent = false;
            refreshButton.Click += OnRefreshClick;
            //
            // deco_panel1
            //
            deco_panel1.BackColor = Color.Gray;
            deco_panel1.ForeColor = SystemColors.ControlText;
            resources.ApplyResources(deco_panel1, "deco_panel1");
            deco_panel1.Name = "deco_panel1";
            //
            // deco_panel5
            //
            deco_panel5.BackColor = Color.Gray;
            resources.ApplyResources(deco_panel5, "deco_panel5");
            deco_panel5.Name = "deco_panel5";
            //
            // deco_panel3
            //
            deco_panel3.BackColor = Color.Gray;
            resources.ApplyResources(deco_panel3, "deco_panel3");
            deco_panel3.Name = "deco_panel3";
            //
            // deco_panel2
            //
            deco_panel2.BackColor = Color.Gray;
            resources.ApplyResources(deco_panel2, "deco_panel2");
            deco_panel2.Name = "deco_panel2";
            //
            // localFilesBrowser
            //
            resources.ApplyResources(localFilesBrowser, "localFilesBrowser");
            localFilesBrowser.Name = "localFilesBrowser";
            localFilesBrowser.PropertyBagName = "Microsoft.WindowsAPICodePack.Controls.WindowsForms.ExplorerBrowser";
            localFilesBrowser.SelectionChanged += OnLocalSelectionChanged;
            localFilesBrowser.NavigationComplete += OnLocalNavigationComplete;
            localFilesBrowser.Load += OnLocalBrowserLoad;
            localFilesBrowser.PreviewKeyDown += OnLocalBrowserKeyDown;
            //
            // apkAssistantPanel
            //
            apkAssistantPanel.Controls.Add(apkAssistantLabel);
            apkAssistantPanel.Controls.Add(installApkLink);
            apkAssistantPanel.Controls.Add(dismissApkLink);
            apkAssistantPanel.Controls.Add(pictureBox1);
            resources.ApplyResources(apkAssistantPanel, "apkAssistantPanel");
            apkAssistantPanel.Name = "apkAssistantPanel";
            //
            // apkAssistantLabel
            //
            resources.ApplyResources(apkAssistantLabel, "apkAssistantLabel");
            apkAssistantLabel.BackColor = Color.Transparent;
            apkAssistantLabel.Name = "apkAssistantLabel";
            //
            // installApkLink
            //
            installApkLink.BackColor = Color.Transparent;
            resources.ApplyResources(installApkLink, "installApkLink");
            installApkLink.MainTextColor = Color.FromArgb(0, 192, 0);
            installApkLink.Name = "installApkLink";
            installApkLink.Note = "";
            installApkLink.NoteTextColor = Color.LightGray;
            installApkLink.UseVisualStyleBackColor = false;
            installApkLink.Click += OnInstallApkClick;
            //
            // dismissApkLink
            //
            dismissApkLink.BackColor = Color.Transparent;
            resources.ApplyResources(dismissApkLink, "dismissApkLink");
            dismissApkLink.MainTextColor = Color.FromArgb(192, 0, 0);
            dismissApkLink.Name = "dismissApkLink";
            dismissApkLink.Note = "";
            dismissApkLink.NoteTextColor = Color.LightGray;
            dismissApkLink.UseVisualStyleBackColor = false;
            dismissApkLink.Click += OnDismissApkClick;
            //
            // pictureBox1
            //
            pictureBox1.BackColor = Color.Transparent;
            pictureBox1.Image = Properties.Resources.assistant;
            resources.ApplyResources(pictureBox1, "pictureBox1");
            pictureBox1.Name = "pictureBox1";
            pictureBox1.TabStop = false;
            //
            // MainForm
            //
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(apkAssistantPanel);
            Controls.Add(parentDirectoryButton);
            Controls.Add(localPathTextBox);
            Controls.Add(localFilesBrowser);
            Controls.Add(androidPathTextBox);
            Controls.Add(androidFilesGrid);
            Controls.Add(transferActionsPanel);
            Controls.Add(localBackButton);
            Controls.Add(localForwardButton);
            Controls.Add(footerPanel);
            Name = "MainForm";
            TransparencyKey = Color.FromArgb(192, 0, 192);
            FormClosing += OnMainClosing;
            FormClosed += OnMainClosed;
            Load += OnMainLoad;
            Resize += OnMainResize;
            ((System.ComponentModel.ISupportInitialize)androidFilesGrid).EndInit();
            deco_panel4.ResumeLayout(false);
            footerPanel.ResumeLayout(false);
            footerPanel.PerformLayout();
            transferActionsPanel.ResumeLayout(false);
            apkAssistantPanel.ResumeLayout(false);
            apkAssistantPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }




        #endregion
        public DataGridView androidFilesGrid;
		private System.Windows.Forms.Timer initialLoadTimer;
		private ToolTip toolTip1;
		public ComboBox comboBox_lang;
		private Panel transferActionsPanel;
		public Microsoft.WindowsAPICodePack.Controls.WindowsForms.ExplorerBrowser localFilesBrowser;
		public Panel deco_panel1;
		public Panel deco_panel2;
		public Panel deco_panel3;
		public Panel deco_panel4;
		public Panel deco_panel5;
		public Button localForwardButton;
		public Button localBackButton;
		public randz.CustomControls.VerticalLabel createDirectoryButton;
		public FluentButton downloadButton;
		public randz.CustomControls.VerticalLabel refreshButton;
		public FluentButton unlockButton;
		public FluentButton uploadButton;
		public FluentButton settingsButton;
		public Panel deco_panel6;
		public Button consoleButton;
		public LinkLabel versionLabel;
		public Panel footerPanel;
		public TextBox androidPathTextBox;
		public TextBox localPathTextBox;
		public ComboBox deviceComboBox;
		public Button parentDirectoryButton;
		private PictureBox pictureBox1;
		public Panel apkAssistantPanel;
		public Label apkAssistantLabel;
		public DarkCommandLink dismissApkLink;
		public DarkCommandLink installApkLink;
	}
}
