namespace AdbFileManager
{
    internal partial class MainForm
    {
        private void OnUnlockClick(object sender, EventArgs e)
        {
            var unlock = new UnlockForm(_adb, _session, _theme);
            unlock.Show(this);
        }
        private async void OnCreateDirectoryClick(object sender, EventArgs e)
        {
            if (!IsBrowserReady)
                return;
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
            okButton.Click += (sender, e) =>
            {
                directoryNameForm.DialogResult = DialogResult.OK;
                directoryNameForm.Close();
            };
            DialogResult result = directoryNameForm.ShowDialog();
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



        private void OnSettingsClick(object sender, EventArgs e)
        {
            using var settingsForm = new SettingsForm(_settings, _adb, _theme);
            settingsForm.ShowDialog(this);
        }

        private async void OnDeviceSelectionChanged(object sender, EventArgs e)
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
        private void OnLocalSelectionChanged(object sender, EventArgs e)
        {
            var selected = localFilesBrowser.SelectedItems?.FirstOrDefault();
            if (selected != null)
            {
                if (selected.Name?.EndsWith(".apk", StringComparison.OrdinalIgnoreCase) == true && !hideApkInstallPanel)
                {
                    apkAssistantPanel.Left = 28;
                }
                else if (apkAssistantPanel.Left != 10000)
                {
                    apkAssistantPanel.Left = 10000;
                }
            }
        }

        bool installWizardDisplayed = false;
        private void OnInstallApkClick(object sender, EventArgs e)
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
        private void OnDismissApkClick(object sender, EventArgs e)
        {
            apkAssistantPanel.Left = 10000;
            hideApkInstallPanel = true;

        }
    }

}
