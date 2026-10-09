namespace AdbFileManager
{
    internal partial class MainForm
    {
        private void OnUnlockClick(object? sender, EventArgs e)
        {
            var unlock = new UnlockForm(_adb, _session, _theme);
            unlock.Show(this);
        }
        private async void OnCreateDirectoryClick(object? sender, EventArgs e)
        {
            if (!IsBrowserReady)
                return;
            using var directoryNameForm = new Form
            {
                Text = strings.enterDirectoryName,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MinimizeBox = false,
                MaximizeBox = false,
                AutoScaleMode = AutoScaleMode.Dpi,
                Font = Font,
                ClientSize = new Size(420, 125)
            };
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), ColumnCount = 1, RowCount = 2 };
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var dirName = new TextBox { Dock = DockStyle.Top, AccessibleName = strings.enterDirectoryName };
            var okButton = new Button { Text = strings.ok, AutoSize = true, DialogResult = DialogResult.OK };
            layout.Controls.Add(dirName, 0, 0);
            layout.Controls.Add(okButton, 0, 1);
            directoryNameForm.Controls.Add(layout);
            directoryNameForm.AcceptButton = okButton;
            _theme.Apply(directoryNameForm);
            DialogResult result = directoryNameForm.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                string directoryName = dirName.Text;
                try
                {
                    await _browser.CreateDirectoryAsync(directoryName);
                }
                catch (Exception ex)
                {
                    if (!IsDisposed)
                        MessageBox.Show(this, ex.Message, strings.error);
                }

            }
        }



        private void OnSettingsClick(object? sender, EventArgs e)
        {
            using var settingsForm = new SettingsForm(_settings, _adb, _theme);
            settingsForm.ShowDialog(this);
        }

        private async void OnDeviceSelectionChanged(object? sender, EventArgs e)
        {
            if (updatingDeviceList)
                return;
            if (deviceComboBox.SelectedIndex == 1)
            {
                using var wirelessPair = new WirelessPair(_adb, _theme);
                wirelessPair.ShowDialog(this);
                _session.Select(null);
            }
            else if (deviceComboBox.SelectedIndex >= 2)
            {
                int index = deviceComboBox.SelectedIndex - 2;
                if (index >= _session.Devices.Count)
                    return;
                _session.Select(_session.Devices[index].Serial);
            }
            else
                _session.Select(null);
            await LoadAndroidDirectoryAsync(CurrentAndroidPath);
        }

        private bool updatingDeviceList;

        bool hideApkInstallPanel = false;
        private void OnLocalSelectionChanged(object? sender, EventArgs e)
        {
            var selected = localFilesBrowser.SelectedItems?.FirstOrDefault();
            if (selected != null)
            {
                if (selected.Name?.EndsWith(".apk", StringComparison.OrdinalIgnoreCase) == true && !hideApkInstallPanel)
                {
                    apkAssistantPanel.Visible = true;
                }
                else
                {
                    apkAssistantPanel.Visible = false;
                }
            }
            else
                apkAssistantPanel.Visible = false;
            localSelectionActive = selected != null;
            UpdateCopyActions();
        }

        bool installWizardDisplayed = false;
        private void OnInstallApkClick(object? sender, EventArgs e)
        {
            string? path = localFilesBrowser.SelectedItems?.FirstOrDefault()?.ParsingName;
            if (!installWizardDisplayed && path != null)
            {
                installWizardDisplayed = true;
                using var wizard = new ApkInstallWizard(path, ReadyDeviceSerial ?? _session.SelectedSerial, _adb, _theme);
                wizard.ShowDialog(this);
                installWizardDisplayed = false;
            }
        }
        private void OnDismissApkClick(object? sender, EventArgs e)
        {
            apkAssistantPanel.Visible = false;
            hideApkInstallPanel = true;

        }
    }

}
