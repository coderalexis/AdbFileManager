using System.Diagnostics;
namespace AdbFileManager
{
    internal partial class MainForm
    {
        private async void OnAndroidFileDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (!IsBrowserReady || e.RowIndex < 0)
                return;
            var file = FileFromRow(androidFilesGrid.Rows[e.RowIndex]);
            if (file == null)
                return;
            if (file.IsDirectory)
            {
                NavigateToDirectory(AndroidPath.Combine(CurrentAndroidPath, file.Name));
                return;
            }
            if (!_settings.Current.PreviewMediaFiles || !MediaFileTypes.CanPreview(file.Name))
            {
                MessageBox.Show(this, string.Format(strings.fileInfo, file.Name, file.Bytes, file.Modified));
                return;
            }
            try
            {
                string destination = await _preview.DownloadAsync(AndroidPath.Combine(CurrentAndroidPath, file.Name), file.Name, ReadyDeviceSerial!, CancellationToken.None);
                if (!IsDisposed)
                    Process.Start(new ProcessStartInfo(destination) { UseShellExecute = true });
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) { if (!IsDisposed) MessageBox.Show(this, ex.Message, strings.error); }
        }

        private async void OnDownloadClick(object sender, EventArgs e)
        {
            if (!IsBrowserReady)
                return;
            string? destination = localFilesBrowser.NavigationLog.CurrentLocation?.ParsingName;
            if (string.IsNullOrWhiteSpace(destination))
                return;
            var sources = androidFilesGrid.SelectedRows.Cast<DataGridViewRow>().Select(FileFromRow).OfType<AndroidFile>()
                .Select(file => (Source: AndroidPath.Combine(CurrentAndroidPath, file.Name), IsDirectory: file.IsDirectory)).ToList();
            await QueueTransfersAsync(sources, destination, true);
        }
        private void OnAndroidHeaderDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            NavigateToParent();
        }
        private void OnParentDirectoryClick(object sender, EventArgs e)
        {
            NavigateToParent();
        }

        private void OnAndroidGridKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                OpenSelectedAndroidEntry();
            }
            else if (e.KeyCode == Keys.Back)
            {
                NavigateToParent();
            }
        }
        private void OnLocalBrowserKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                localFilesBrowser.Navigate(localFilesBrowser.NavigationLog.CurrentLocation);
            }
            else if (e.KeyCode == Keys.Back)
            {
                NavigateToParent();
            }
        }
        private void OpenSelectedAndroidEntry()
        {
            if (!IsBrowserReady)
                return;
            var file = FileFromRow(androidFilesGrid.CurrentRow);
            if (file == null)
                return;
            if (file.IsDirectory)
                NavigateToDirectory(AndroidPath.Combine(CurrentAndroidPath, file.Name));
            else
                MessageBox.Show(this, string.Format(strings.fileInfo, file.Name, file.Bytes, file.Modified));
        }
        private void NavigateToDirectory(string path)
        {
            _ = LoadAndroidDirectoryAsync(path);
        }
        void NavigateToParent()
        {
            string? parent = AndroidPath.Parent(CurrentAndroidPath);
            if (parent != null)
                NavigateToDirectory(parent);
        }

        private async void OnInitialLoad(object sender, EventArgs e)
        {
            initialLoadTimer.Stop();
            initialLoadTimer.Enabled = false;
            ConsoleWindow.Hide();
            androidPathTextBox.Text = CurrentAndroidPath;
            await LoadAndroidDirectoryAsync(CurrentAndroidPath);
            if (!IsDisposed)
                OnMainResize(this, EventArgs.Empty);
        }

        private async void OnUploadClick(object sender, EventArgs e)
        {
            if (!IsBrowserReady)
                return;
            var sources = localFilesBrowser.SelectedItems?.Select(item => item.ParsingName).OfType<string>()
                .Select(path => (Source: path, IsDirectory: Directory.Exists(path))).ToList() ?? new();
            await QueueTransfersAsync(sources, CurrentAndroidPath, false);
        }


        private void OnMainLoad(object sender, EventArgs e)
        {
            initialLoadTimer.Enabled = true;
            initialLoadTimer.Start();
            ApplyExplorerDarkMode();
        }

    }
}
