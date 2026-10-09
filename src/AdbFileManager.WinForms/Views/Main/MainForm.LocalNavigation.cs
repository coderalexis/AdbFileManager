using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Controls;
namespace AdbFileManager
{
    internal partial class MainForm
    {
        private void OnLocalBrowserLoad(object? sender, EventArgs e)
        {
            try
            {
                if (_settings.Current.RememberDirectory && !string.IsNullOrEmpty(_settings.Current.LastDirectory) && Path.Exists(_settings.Current.LastDirectory))
                {
                    string path = _settings.Current.LastDirectory;
                    var folder = ShellObject.FromParsingName(path) ?? throw new IOException(strings.invalidPath);
                    localFilesBrowser.Navigate(folder);
                    localPathTextBox.Text = path;
                    return;
                }
                else
                {
                    string path = Environment.ExpandEnvironmentVariables("%UserProfile%\\pictures\\");
                    var folder = ShellObject.FromParsingName(path) ?? throw new IOException(strings.invalidPath);
                    localFilesBrowser.Navigate(folder);
                    localPathTextBox.Text = path;
                }
            }
            catch
            {
                string path = Environment.ExpandEnvironmentVariables("C:\\");
                var folder = ShellObject.FromParsingName(path) ?? throw new IOException(strings.invalidPath);
                localFilesBrowser.Navigate(folder);
                localPathTextBox.Text = path;
            }
            finally
            {
                ApplyExplorerDarkMode();
            }
        }

        private void ApplyExplorerDarkMode()
        {
            if (!_theme.IsDark || !localFilesBrowser.IsHandleCreated)
                return;

            void ApplyNativeTree()
            {
                if (!localFilesBrowser.IsDisposed && localFilesBrowser.IsHandleCreated)
                    DarkModeStartup.ApplyToControlTree(localFilesBrowser.Handle);
            }

            // ExplorerBrowser creates its DirectUI child windows asynchronously.
            if (IsHandleCreated)
                BeginInvoke(ApplyNativeTree);
            else
                ApplyNativeTree();
        }

    }
}
