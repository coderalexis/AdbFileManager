//ADB File Manager
//Originally created by T0biasCZe in 2023
//You can use this program comercially, just dont redistribute it without my permission
//If you fork thís, please give me credit

using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Data;
using Microsoft.WindowsAPICodePack.Shell;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection.Metadata;
using System.Text;
using System.Globalization;
using Microsoft.WindowsAPICodePack.ApplicationServices;
using System.Reflection;
using System.Resources;
using Microsoft.WindowsAPICodePack.Dialogs;
using TaskDialogButton = System.Windows.Forms.TaskDialogButton;
using TaskDialog = System.Windows.Forms.TaskDialog;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Timer = System.Windows.Forms.Timer;
using Microsoft.WindowsAPICodePack.Controls;
using Microsoft.Win32;
using TextBox = System.Windows.Forms.TextBox;
using Button = System.Windows.Forms.Button;
using Microsoft.WindowsAPICodePack.Shell.Interop;

namespace AdbFileManager {
    public partial class Form1 : Form {
        public static Form1 _Form1;
        public string directoryPath = "/sdcard/";
        public string tempPath = Path.GetTempPath() + "adbfilemanager\\";
        public bool temp_folder_created = false;

        public static ResourceManager rm = new ResourceManager("AdbFileManager.strings", Assembly.GetExecutingAssembly());
        public Form1() {
            try {
                showConsole();
                _Form1 = this;
                applyLang();

                InitializeComponent();
                explorerBrowser1.HandleCreated += (_, _) => ApplyExplorerDarkMode();
                if (SettingsManager.settings.DarkMode) {
                    explorerBrowser1.NavigationOptions.PaneVisibility.Commands = PaneVisibilityState.Hide;
                    explorerBrowser1.NavigationOptions.PaneVisibility.CommandsOrganize = PaneVisibilityState.Hide;
                    explorerBrowser1.NavigationOptions.PaneVisibility.CommandsView = PaneVisibilityState.Hide;
                }

                UIStyle.ApplyModernTheme(this);
                if (SettingsManager.settings.DarkMode) {
                    UIStyle.LoadDarkMode(this);
                }



                //this.Controls.Add(panel_dolniTlacitka);
                //panel_main.Controls.Remove(panel_dolniTlacitka);
                panel_dolniTlacitka.BringToFront();
                verticalLabel_refresh.BringToFront();
                dataGridView_soubory.RowHeadersWidth = 4;
                Console.WriteLine("datagrid virtual mode: " + dataGridView_soubory.VirtualMode);
                dataGridView_soubory.VirtualMode = false;
                DataTable blank = new DataTable();
                //add header to blank
                blank.Columns.Add("ico", typeof(Icon));
                blank.Columns.Add("Name");
                blank.Columns.Add("Size");
                blank.Columns.Add("Date");
                blank.Columns.Add("Attr");
                string text = rm.GetString("loadingProgram");
                blank.Rows.Add(new Icon(@"icons\file.ico"), text, 0, DateTime.UnixEpoch);
                dataGridView_soubory.DataSource = blank;

                DataGridViewImageColumn img = (DataGridViewImageColumn)dataGridView_soubory.Columns[0];
                img.ImageLayout = DataGridViewImageCellLayout.Zoom;
                dataGridView_soubory.Columns[0].Width = 25;
                dataGridView_soubory.Columns[0].MinimumWidth = 25;
                dataGridView_soubory.Columns[1].MinimumWidth = 307;
                dataGridView_soubory.Columns[1].Width = 307;
                dataGridView_soubory.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

                dataGridView_soubory.Columns[2].Width = 80;
                dataGridView_soubory.Columns[2].MinimumWidth = 80;
                dataGridView_soubory.Columns[3].Width = 115;
                dataGridView_soubory.Columns[3].MinimumWidth = 115;
                dataGridView_soubory.Columns[4].Width = 90;
                dataGridView_soubory.Columns[4].MinimumWidth = 90;



                //set Console app codepage to UTF-8.
                Console.OutputEncoding = System.Text.Encoding.UTF8;
                try {
                    if (!Console.IsOutputRedirected) Console.WindowHeight = 20;
                }
                catch (IOException) {
                    // GUI launches and redirected test runs may not have a resizable console.
                }

                string versionn = AdbFileManager.Properties.Resources.CurrentCommit.Trim();
                label_version.Text = versionn;
                Console.WriteLine(versionn);
                InitializeTransfers();
                InitializeBrowser();

            }
            catch (Exception ex) {
                TaskDialog.ShowDialog(new TaskDialogPage() {
                    Caption = AdbFileManager.strings.error,
                    Text = ex.ToString(),
                    Icon = TaskDialogIcon.Error,
                    Buttons = { TaskDialogButton.Close },
                    Footnote = AdbFileManager.strings.errorReportFootnote,
                    SizeToContent = true
                });
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex);
                Console.ResetColor();
                this.Show();
            }
        }

        public static string adb(params string[] arguments) {
            try {
                string? device = arguments[0] is "devices" or "version" or "connect" or "pair"
                    ? null : selectedDevice?.adbId;
                var result = AdbClient.Default.ExecuteAsync(arguments, device).GetAwaiter().GetResult();
                return result.CombinedOutput;
            }
            catch (Exception ex) { return "ADB error: " + ex.Message; }
        }
        private async void verticalLabel1_Click(object sender, EventArgs e) {
            await LoadAndroidDirectoryAsync(directoryPath);
        }

        private void explorerBrowser1_Load(object sender, EventArgs e) {
            try {
                if(SettingsManager.settings.rememberDirectory && !string.IsNullOrEmpty(SettingsManager.settings.lastDirectory) && Path.Exists(SettingsManager.settings.lastDirectory)) {
                    string path = SettingsManager.settings.lastDirectory;
                    ShellObject Shell = ShellObject.FromParsingName(path);
                    explorerBrowser1.Navigate(Shell);
                    explorer_path.Text = path;
                    return;
                }
                else {
                    string path = Environment.ExpandEnvironmentVariables("%UserProfile%\\pictures\\");
                    ShellObject Shell = ShellObject.FromParsingName(path);
                    explorerBrowser1.Navigate(Shell);
                    explorer_path.Text = path;
                }
            }
            catch {
                string path = Environment.ExpandEnvironmentVariables("C:\\");
                ShellObject Shell = ShellObject.FromParsingName(path);
                explorerBrowser1.Navigate(Shell);
                explorer_path.Text = path;
            }
            finally {
                ApplyExplorerDarkMode();
            }
        }

        private void ApplyExplorerDarkMode() {
            if (!SettingsManager.settings.DarkMode || !explorerBrowser1.IsHandleCreated) return;

            void ApplyNativeTree() {
                if (!explorerBrowser1.IsDisposed && explorerBrowser1.IsHandleCreated)
                    DarkModeStartup.ApplyToControlTree(explorerBrowser1.Handle);
            }

            // ExplorerBrowser creates its DirectUI child windows asynchronously.
            if (IsHandleCreated) BeginInvoke(ApplyNativeTree);
            else ApplyNativeTree();
        }

        private async void dataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e) {
            Console.WriteLine("CellMouseDoubleClick()");
            if (browserReady && e.RowIndex >= 0) {
                string name = dataGridView_soubory.Rows[e.RowIndex].Cells[1].Value.ToString();
                string size = dataGridView_soubory.Rows[e.RowIndex].Cells[2].Value.ToString();
                string date = dataGridView_soubory.Rows[e.RowIndex].Cells[3].Value.ToString();
                string permissions = dataGridView_soubory.Rows[e.RowIndex].Cells[4].Value.ToString();
                if (!DirectoryEntry.IsDirectory(permissions)) {
                    if (SettingsManager.settings.previewMediaFiles) {
                        if (Functions.videoExtensions.Any(x => name.EndsWith(x, StringComparison.OrdinalIgnoreCase)) || Functions.imageExtensions.Any(x => name.EndsWith(x, StringComparison.OrdinalIgnoreCase)) || Functions.audioExtensions.Any(x => name.EndsWith(x, StringComparison.OrdinalIgnoreCase))) {
                            //copy file to temp folder
                            string sourcePath = directoryPath + name;
                            string destinationPath = tempPath + name;

                            if (!temp_folder_created) {
                                Directory.CreateDirectory(tempPath);
                                temp_folder_created = true;
                            }
                            try {
                                var arguments = AdbClient.TargetArguments(new[] { "pull", sourcePath, destinationPath }, listedDevice);
                                await AdbClient.Default.CopyAsync(arguments, null, CancellationToken.None);
                                Process.Start(new ProcessStartInfo(destinationPath) { UseShellExecute = true });
                            }
                            catch (Exception ex) { MessageBox.Show(this, ex.Message, rm.GetString("error")); }

                        }

                    }
                    else MessageBox.Show(string.Format(AdbFileManager.strings.fileInfo, name, size, date));

                }
                else {
                    NavigateToDirectory(TransferCommand.RemotePath(directoryPath, name) + "/");
                }
            }
        }
        private async void android2pc_Click(object sender, EventArgs e) {
            if (!browserReady) return;
            string? destination = explorerBrowser1.NavigationLog.CurrentLocation?.ParsingName;
            if (string.IsNullOrWhiteSpace(destination)) return;
            var sources = new List<(string Source, bool IsDirectory)>();
            foreach (DataGridViewRow row in dataGridView_soubory.SelectedRows) {
                string? name = row.Cells[1].Value?.ToString();
                string? permissions = row.Cells[4].Value?.ToString();
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(permissions)) continue;
                sources.Add((TransferCommand.RemotePath(directoryPath, name), DirectoryEntry.IsDirectory(permissions)));
            }
            await QueueTransfersAsync(sources, destination, true);
        }

        private void dataGridView1_ColumnHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e) {
            goUpDirectory();
        }
        private void button_goUpDirectory_Click(object sender, EventArgs e) {
            goUpDirectory();
        }

        private void dataGridView1_KeyDown(object sender, KeyEventArgs e) {
            Console.WriteLine("Key pressed in datagrid: " + e.KeyValue);
            if (e.KeyCode == Keys.Enter) {
                e.SuppressKeyPress = true;
                clickedFolder();
            }
            else if (e.KeyCode == Keys.Back) {
                goUpDirectory();
            }
        }
        private void explorerBrowser1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) {
            Console.WriteLine("Key pressed in explorer: " + e.KeyValue);
            if (e.KeyCode == Keys.Enter) {
                explorerBrowser1.Navigate(explorerBrowser1.NavigationLog.CurrentLocation);
            }
            else if (e.KeyCode == Keys.Back) {
                goUpDirectory();
            }
        }
        void clickedFolder() {
            if (!browserReady) return;
            int rowIndex = dataGridView_soubory.CurrentCell?.RowIndex ?? -1;
            if (rowIndex < 0 || dataGridView_soubory.Rows[rowIndex].IsNewRow) return;
            var row = dataGridView_soubory.Rows[rowIndex];
            string? name = row.Cells[1].Value?.ToString();
            string? permissions = row.Cells[4].Value?.ToString();
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(permissions)) return;
            if (!DirectoryEntry.IsDirectory(permissions)) {
                MessageBox.Show(string.Format(AdbFileManager.strings.fileInfo,
                    name, row.Cells[2].Value, row.Cells[3].Value));
                return;
            }
            NavigateToDirectory(TransferCommand.RemotePath(directoryPath, name) + "/");
        }

        private void NavigateToDirectory(string path) {
            if (string.IsNullOrEmpty(path) || !path.StartsWith('/') || path.Contains('\0')) {
                MessageBox.Show(this, BrowserText("invalidPath"), strings.error);
                return;
            }
            directoryPath = path.EndsWith('/') ? path : path + "/";
            cur_path.Text = directoryPath;
            _ = LoadAndroidDirectoryAsync(directoryPath);
        }

        void goUpDirectory() {
            string? parent = DirectoryEntry.ParentPath(directoryPath);
            if (parent != null) NavigateToDirectory(parent);
        }

        private async void timer1_Tick(object sender, EventArgs e) {
            timer1.Stop();
            timer1.Enabled = false;
            hideConsole();
            cur_path.Text = directoryPath;
            await LoadAndroidDirectoryAsync(directoryPath);
            if (!IsDisposed) Form1_Resize(this, EventArgs.Empty);
        }

        private async void pc2android_Click(object sender, EventArgs e) {
            if (!browserReady) return;
            var sources = explorerBrowser1.SelectedItems
                .Select(item => (Source: item.ParsingName, IsDirectory: Directory.Exists(item.ParsingName))).ToList();
            await QueueTransfersAsync(sources, directoryPath, false);
        }

        private void cur_path_TextChanged(object sender, EventArgs e) {
            // A typed path is submitted with Enter, rather than querying on every keystroke.
        }

        private void Form1_Load(object sender, EventArgs e) {
            Console.WriteLine("Form loaded, starting timer");
            timer1.Enabled = true;
            timer1.Start();
            ApplyExplorerDarkMode();
        }

        public static string ShowLibraryPopup(string libraryName) {
            using (var library = Microsoft.WindowsAPICodePack.Shell.ShellLibrary.Load(libraryName, true)) {

                var folders = new List<string>();
                foreach (var item in library) {
                    folders.Add(item.ParsingName);
                }
                if (folders.Count == 1) {
                    return folders[0];
                }

                string selectedFolder = null;

                var dialog = new Microsoft.WindowsAPICodePack.Dialogs.TaskDialog();
                dialog.Caption = AdbFileManager.strings.selectLibraryFolderCaption;
                dialog.InstructionText = AdbFileManager.strings.selectLibraryFolderInstruction;
                dialog.Text = AdbFileManager.strings.selectLibraryFolderText;

                dialog.StandardButtons = Microsoft.WindowsAPICodePack.Dialogs.TaskDialogStandardButtons.Cancel;

                foreach (var folder in folders) {
                    var button = new Microsoft.WindowsAPICodePack.Dialogs.TaskDialogCommandLink(folder, folder);
                    button.Click += (s, e) => {
                        selectedFolder = folder;
                        dialog.Close(Microsoft.WindowsAPICodePack.Dialogs.TaskDialogResult.Ok);
                    };
                    dialog.Controls.Add(button);
                }

                dialog.Show();

                return selectedFolder;
            }
        }



        private void explorerBrowser1_NavigationComplete(object sender, Microsoft.WindowsAPICodePack.Controls.NavigationCompleteEventArgs e) {
            ApplyExplorerDarkMode();
            string currentPath = ShellObject.FromParsingName(explorerBrowser1.NavigationLog.CurrentLocation.ParsingName).Properties.System.ItemPathDisplay.Value;
            explorer_path.Text = currentPath;

            SettingsManager.settings.lastDirectory = currentPath;

            string newPath = "";
            if (currentPath.Contains("Music.library-ms")) {
                newPath = ShowLibraryPopup("Music");
            }
            else if (currentPath.Contains("Saved Pictures.library-ms")) {
                newPath = ShowLibraryPopup("SavedPictures");
            }
            else if (currentPath.Contains("Pictures.library-ms")) {
                newPath = ShowLibraryPopup("Pictures");
            }
            else if (currentPath.Contains("Documents.library-ms")) {
                newPath = ShowLibraryPopup("Documents");
            }
            else if (currentPath.Contains("Camera Roll.library-ms")) {
                newPath = ShowLibraryPopup("CameraRoll");
            }
            else if (currentPath.Contains("Videos.library-ms")) {
                newPath = ShowLibraryPopup("Videos");
            }

            if (!string.IsNullOrEmpty(newPath)) {
                SettingsManager.settings.lastDirectory = newPath;
                explorerBrowser1.Navigate(ShellObject.FromParsingName(newPath));
            }
        }

        private void explorer_path_TextChanged(object sender, EventArgs e) {
        }

        private void explorer_path_KeyPress(object sender, KeyPressEventArgs e) {
            //check if enter key was pressed
            if (e.KeyChar == (char)13) {
                string oldPath = ShellObject.FromParsingName(explorerBrowser1.NavigationLog.CurrentLocation.ParsingName).Properties.System.ItemPathDisplay.Value;
                try {
                    ShellObject Shell = ShellObject.FromParsingName(explorer_path.Text);
                    explorerBrowser1.Navigate(Shell);
                }
                catch {
                    MessageBox.Show(AdbFileManager.strings.invalidPath, AdbFileManager.strings.error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    explorer_path.Text = oldPath;
                }
            }
        }

        private void version_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            Process.Start("explorer.exe", "https://github.com/T0biasCZe/AdbFileManager");
        }
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);


        bool console_shown = false;
        private void button1_Click(object sender, EventArgs e) {
            console_shown = !console_shown;
            if (console_shown) {
                showConsole();
            }
            else {
                hideConsole();
            }
        }
        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        public static void hideConsole() {
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);
        }
        public static void showConsole() {
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_SHOW);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            browserRequests.Cancel();
            if (transferQueue?.IsRunning == true) {
                e.Cancel = true;
                closeAfterQueue = true;
                transferQueue.Pause();
                return;
            }
            try { SettingsManager.SaveSettings(); }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException) {
                e.Cancel = true;
                closeAfterQueue = false;
                MessageBox.Show(this, strings.ResourceManager.GetString("settings_saveError") + Environment.NewLine + ex.Message,
                    strings.settings_title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (queueWindow != null) queueWindow.AllowClose = true;
            SaveAndRefreshQueue();
            //show console
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_SHOW);
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine("Closing begin");
            // The ADB server is shared with other applications; leave it running.

            if (Directory.Exists(tempPath)) {
                Console.WriteLine("deleting temp directory...");
                Directory.Delete(tempPath, true);
            }
            Console.WriteLine("Saving new settings...");
            Console.WriteLine("Settings saved!");
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e) {
            Console.WriteLine("Exiting Application... This may take few dozen seconds");
            Application.Exit();
            hideConsole();
        }

        enum Languages {
            English,
            Cestina,
            Polski,
            Deutsch,
            Japanese,
            Espanol,
            ChineseSimplified,
            ChineseTraditional
        }

        private void applyLang() {
            //ushort? loaded_lang = Properties.Settings.Default.lang;
            ushort? loaded_lang = SettingsManager.settings.lang; //load from settings manager
            if (loaded_lang == null) loaded_lang = (ushort)Languages.English;
            switch ((Languages)loaded_lang) {
                case Languages.English:
                    Console.WriteLine("Setting language to English");
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");
                    break;
                case Languages.Cestina:
                    Console.WriteLine("Nastavování jazyka na Češtinu");
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("cs");
                    break;
                case Languages.Polski:
                    Console.WriteLine("Ustawianie języka na Polski");
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("pl");
                    break;
                case Languages.Deutsch:
                    Console.WriteLine("Sprache auf Deutsch eingestellt");
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("de");
                    break;
                case Languages.Japanese:
                    Console.WriteLine("言語を日本語に設定しています");
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("ja");
                    break;
                case Languages.Espanol:
                    Console.WriteLine("Configurando el idioma a Español");
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("es");
                    break;
                case Languages.ChineseSimplified:
                    Console.WriteLine("正在将语言设置为简体中文");
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("zh-Hans");
                    break;
                case Languages.ChineseTraditional:
                    Console.WriteLine("正在將語言設定為繁體中文");
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("zh-Hant");
                    break;
            }

        }
        private void buttonback_MouseLeave(object sender, EventArgs e) {
            //this.button_back.Image = Properties.Resources.travel_back_enabled;
            this.button_back.Image = Icons.travel_enabled_back;
        }

        private void buttonback_MouseEnter(object sender, EventArgs e) {
            //this.button_back.Image = Properties.Resources.travel_hot_back;
            this.button_back.Image = Icons.travel_hot_back;
        }

        private void buttonback_MouseDown(object sender, MouseEventArgs e) {
            //this.button_back.Image = Properties.Resources.travel_pressed_back;
            this.button_back.Image = Icons.travel_pressed_back;
        }
        private void buttonback_MouseUp(object sender, MouseEventArgs e) {
            //this.button_back.Image = Properties.Resources.travel_hot_back;
            this.button_back.Image = Icons.travel_hot_back;
        }

        private void buttonforward_MouseLeave(object sender, EventArgs e) {
            //this.button_forward.Image = Properties.Resources.travel_forward_enabled;
            this.button_forward.Image = Icons.travel_enabled_forward;
        }

        private void buttonforward_MouseEnter(object sender, EventArgs e) {
            //this.button_forward.Image = Properties.Resources.travel_hot_forward;
            this.button_forward.Image = Icons.travel_hot_forward;
        }

        private void buttonforward_MouseDown(object sender, MouseEventArgs e) {
            //this.button_forward.Image = Properties.Resources.travel_hot_forward;
            this.button_forward.Image = Icons.travel_pressed_forward;
        }
        private void buttonforward_MouseUp(object sender, MouseEventArgs e) {
            //this.button_forward.Image = Properties.Resources.travel_hot_forward;
            this.button_forward.Image = Icons.travel_hot_forward;
        }

        private void button_back_Click(object sender, EventArgs e) {
            explorerBrowser1.NavigateLogLocation(NavigationLogDirection.Backward);
        }

        private void button_forward_Click(object sender, EventArgs e) {
            explorerBrowser1.NavigateLogLocation(NavigationLogDirection.Forward);
        }


        //responsivity go brrr
        private void Form1_Resize(object sender, EventArgs e) {
            const int margin = 24;
            const int middleSpace = 52;

            int realWidth = this.Width - 16; //"form size" includes the windows borders for some reason 🤨
            int realHeight = this.Height - 39;
            int x = realWidth - 100;

            //panel_main.Width = realWidth;
            //panel_main.Height = realHeight;
            //panel_main.Left = 0;
            //panel_main.Top = 0;

            int listAndroidWidth = x / 2;
            int listPCWidth = x - listAndroidWidth;

            dataGridView_soubory.Width = listAndroidWidth;

            if (SettingsManager.settings.ShowAndroidBackButton) {
                cur_path.Width = listAndroidWidth - 45;
                button_goUpDirectory.Visible = true;
                button_goUpDirectory.Left = margin + listAndroidWidth - 45;
            }
            else {
                button_goUpDirectory.Visible = false;
                cur_path.Width = listAndroidWidth;
            }

            panel_tlacitkaUprostred.Left = margin + listAndroidWidth - 1;

            explorerBrowser1.Width = listPCWidth;
            explorerBrowser1.Left = margin + listAndroidWidth + middleSpace;
            explorerBrowser1.Height = this.Height - 104;

            button_back.Left = margin + listAndroidWidth + middleSpace;
            button_forward.Left = margin + listAndroidWidth + middleSpace + 26;
            explorer_path.Width = listPCWidth - 57;
            explorer_path.Left = margin + listAndroidWidth + middleSpace + 57;

            verticalLabel_refresh.BringToFront();


            panel_dolniTlacitka.Width = this.Width;
            panel_dolniTlacitka.Left = 0;
            label_version.Left = this.Width - 121;
            button1.Left = this.Width - 134;
            comboBox_lang.Left = this.Width - 242;

            button_unlock.Left = this.Width - 297;
            deco_panel6.Left = this.Width - 297;
            comboBox_device.Left = this.Width - 424;

            button_console.Left = this.Width - 136;

            deco_panel4.Left = this.Width - 216;

            panel_installAssistant.Top = this.Height - 150;
        }

        private void button_unlock_Click(object sender, EventArgs e) {
            UnlockForm unlock = new UnlockForm();
            unlock.Show();
        }

        private async void button_makedir_Click(object sender, EventArgs e) {
            if (!browserReady) return;
            //show form dialog with textbox input for directory name
            Form directoryNameForm = new Form();
            directoryNameForm.Text = AdbFileManager.strings.enterDirectoryName;
            directoryNameForm.Size = new Size(300, 100);
            directoryNameForm.StartPosition = FormStartPosition.CenterParent;
            TextBox dirName = new TextBox();
            dirName.Size = new Size(260, 20);
            dirName.Location = new Point(10, 10);
            Button okButton = new Button();
            okButton.Text = AdbFileManager.strings.ok;
            okButton.Size = new Size(75, 23);
            directoryNameForm.Controls.Add(dirName);
            directoryNameForm.Controls.Add(okButton);

            //set ok button to close the form and return the value from textbox
            okButton.Click += (sender, e) => {
                directoryNameForm.DialogResult = DialogResult.OK;
                directoryNameForm.Close();
            };
            DialogResult result = directoryNameForm.ShowDialog();
            if (result == DialogResult.OK) {
                string directoryName = dirName.Text;
                if (string.IsNullOrEmpty(directoryName) || directoryName.Contains('/') || directoryName is "." or "..") return;
                string targetPath = directoryPath;
                string? targetDevice = listedDevice;
                try {
                    await AdbClient.Default.QueryAsync(new[] { "shell", "mkdir " + AdbClient.QuoteShell(
                        TransferCommand.RemotePath(targetPath, directoryName)) }, targetDevice);
                    if (!IsDisposed && directoryPath == targetPath && listedDevice == targetDevice)
                        await LoadAndroidDirectoryAsync(targetPath);
                }
                catch (Exception ex) {
                    if (!IsDisposed) MessageBox.Show(this, ex.Message, strings.error);
                }

            }
        }

        private void panel_tlacitkaUprostred_Paint(object sender, PaintEventArgs e) {

        }

        private void panel_dolniTlacitka_Paint(object sender, PaintEventArgs e) {

        }

        private void button_openSettings_Click(object sender, EventArgs e) {
            SettingsForm settingsForm = new SettingsForm();
            settingsForm.ShowDialog(this);
        }

        private async void comboBox_device_SelectedIndexChanged(object sender, EventArgs e) {
            if (modifyingComboBox) return;
            if (comboBox_device.SelectedIndex == 1) {
                using var wirelessPair = new WirelessPair();
                wirelessPair.ShowDialog(this);
                selectedDevice = null;
            }
            else if (comboBox_device.SelectedIndex >= 2) {
                int index = comboBox_device.SelectedIndex - 2;
                if (index >= foundDevices.Count) return;
                selectedDevice = foundDevices[index];
            }
            else selectedDevice = null;
            await LoadAndroidDirectoryAsync(directoryPath);
        }

        public static Device? selectedDevice = null;
        public List<Device> foundDevices = new();
        public class Device {
            public string adbId { get; set; } = "";
            public string state { get; set; } = "";
            public string product { get; set; } = "";
            public string model { get; set; } = "";
            public string device { get; set; } = "";
            public string transportId { get; set; } = "";
        }
        bool modifyingComboBox;

        int selectChangedCount = 0;

        bool hideApkInstallPanel = false;
        private void explorerBrowser1_SelectionChanged(object sender, EventArgs e) {
            selectChangedCount++;
            if (explorerBrowser1.SelectedItems.Count > 0) {
                if (explorerBrowser1.SelectedItems[0].Name.EndsWith(".apk") && !hideApkInstallPanel) {
                    panel_installAssistant.Left = 28;
                }
                else if (panel_installAssistant.Left != 10000) {
                    panel_installAssistant.Left = 10000;
                }
            }
        }

        bool installWizardDisplayed = false;
        private void commandLink_installYes_Click(object sender, EventArgs e) {
            if (!installWizardDisplayed) {
                installWizardDisplayed = true;
                //MessageBox.Show($"Number of files selected: {explorerBrowser1.SelectedItems.Count}\nParsingName: {explorerBrowser1.SelectedItems[0].ParsingName}\nName: {explorerBrowser1.SelectedItems[0].Name}");
                string path = explorerBrowser1.SelectedItems[0].ParsingName.ToString();
                ApkInstallWizard wizard = new ApkInstallWizard(path);
                wizard.ShowDialog();

                installWizardDisplayed = true;
            }
        }
        private void commandLink_installGoAway_Click(object sender, EventArgs e) {
            panel_installAssistant.Left = 10000;
            hideApkInstallPanel = true;

        }
        private void label1_Click(object sender, EventArgs e) {

        }
    }

    public static class Functions {
		public static string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".webp", ".heif", ".mpo" };
		public static string[] videoExtensions = { ".mp4", ".mkv", ".webm", ".avi", ".mov", ".wmv", ".flv", ".3gp", ".m4v", ".mpg", ".mpeg", ".m2v", ".m4v", ".m2ts", ".mts", ".ts", ".vob", ".divx", ".xvid" };
		public static string[] romExtensions = { ".nes", ".snes", ".gba", ".gbc", ".gb", ".nds", ".n64", ".psx", ".iso", ".cia", ".3ds", ".3dsx", ".wbfs", ".rvz" };
		public static string[] audioExtensions = { ".mp3", ".wav", ".ogg", ".flac", ".m4a", ".aac", ".wma", ".mod", ".mid", ".s3m", ".midi" };
		public static string[] documentExtensions = { ".docx", ".pdf", ".txt", ".pptx", ".xlsx", ".odt", ".rtf" };
		public static string[] archiveExtensions = { ".zip", ".rar", ".7z", ".tar", ".gz", ".bz2", ".xz" };
		public static string[] executableExtensions = { ".exe", ".dll", ".bat", ".msi", ".jar", ".py", ".sh", ".apk" };
		static bool fastcompatibility = false;
		public static bool isFolder(string path, bool old_android) {
			if(old_android) return legacyAndroid.isFolder(path, fastcompatibility);
			if(path == null) return false;
			if(path.ToLower().Trim()[0] == 'd') return true; //the first character of the line is 'd' if it's a directory
			else return false;
		}
		public static bool isFolder(File file, bool old_android) {
			if(old_android) return legacyAndroid.isFolder(file, fastcompatibility);
			if(file.permissions.ToLower().Trim()[0] == 'd') return true; //the first character of the line is 'd' if it's a directory
			else return false;
		}
		public static string[] CustomSplit(string text, char delimiter) {
			string[] result = Regex.Split(text, $"(?<!\\\\){delimiter}+");

			for(int i = 0; i < result.Length; i++) {
				result[i] = result[i].Replace("\\ ", " ");
			}
			//strip new line symbols from elements
			for(int i = 0; i < result.Length; i++) {
				result[i] = result[i].Replace("\r", "");
				result[i] = result[i].Replace("\n", "");
			}
			return result;
		}
		public static string FixWindowsPath(string path) {
			path.Replace('\\', '/');
			path.Replace("C:/Usuarios", "C:/Users");
			path.Replace("C:/Usuários", "C:/Users");
			
			return path;
		}
	}
	public class File {
		public string name;
		public string size;
		public string date;
		public string permissions;
		public bool isDirectory;
		public File(string name, string size, string date, string permissions, bool isDirectory) {
			this.name = name;
			this.size = size;
			this.date = date;
			this.permissions = permissions;
			this.isDirectory = isDirectory;
		}
	}
	public static class Util {
		static public T Find<T>(Control container) where T : Control {
			foreach(Control child in container.Controls)
				return (child is T ? (T)child : Find<T>(child));
			// Not found.
			return null;
		}
	}
}
