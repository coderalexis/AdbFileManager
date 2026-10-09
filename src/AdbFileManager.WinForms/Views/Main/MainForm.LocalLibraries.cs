using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace AdbFileManager;

internal partial class MainForm
{
    private static string? SelectLibraryFolder(string libraryName)
    {
        using var library = ShellLibrary.Load(libraryName, true);
        var folders = new List<string>();
        foreach (var item in library)
        {
            if (item.ParsingName is string path)
                folders.Add(path);
        }
        if (folders.Count == 1)
            return folders[0];
        string? selectedFolder = null;
        using var dialog = new Microsoft.WindowsAPICodePack.Dialogs.TaskDialog
        {
            Caption = strings.selectLibraryFolderCaption,
            InstructionText = strings.selectLibraryFolderInstruction,
            Text = strings.selectLibraryFolderText,
            StandardButtons = TaskDialogStandardButtons.Cancel
        };
        foreach (string folder in folders)
        {
            var button = new TaskDialogCommandLink(folder, folder);
            button.Click += (_, _) => { selectedFolder = folder; dialog.Close(TaskDialogResult.Ok); };
            dialog.Controls.Add(button);
        }
        dialog.Show();
        return selectedFolder;
    }

    private void OnLocalNavigationComplete(object sender, Microsoft.WindowsAPICodePack.Controls.NavigationCompleteEventArgs e)
    {
        ApplyExplorerDarkMode();
        var location = localFilesBrowser.NavigationLog.CurrentLocation;
        string? currentPath = location?.Properties?.System?.ItemPathDisplay?.Value ?? location?.ParsingName;
        if (currentPath == null)
            return;
        localPathTextBox.Text = currentPath;
        _settings.Current.LastDirectory = currentPath;
        string? library = currentPath switch
        {
            var path when path.Contains("Music.library-ms") => "Music",
            var path when path.Contains("Saved Pictures.library-ms") => "SavedPictures",
            var path when path.Contains("Pictures.library-ms") => "Pictures",
            var path when path.Contains("Documents.library-ms") => "Documents",
            var path when path.Contains("Camera Roll.library-ms") => "CameraRoll",
            var path when path.Contains("Videos.library-ms") => "Videos",
            _ => null
        };
        if (library == null)
            return;
        string? chosen = SelectLibraryFolder(library);
        if (chosen == null)
            return;
        _settings.Current.LastDirectory = chosen;
        var folder = ShellObject.FromParsingName(chosen);
        if (folder != null)
            localFilesBrowser.Navigate(folder);
    }

    private void OnLocalPathKeyPress(object sender, KeyPressEventArgs e)
    {
        if (e.KeyChar != '\r')
            return;
        e.Handled = true;
        string previous = localFilesBrowser.NavigationLog.CurrentLocation?.ParsingName ?? _settings.Current.LastDirectory;
        try
        {
            var folder = ShellObject.FromParsingName(localPathTextBox.Text) ?? throw new IOException(strings.invalidPath);
            localFilesBrowser.Navigate(folder);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            MessageBox.Show(this, strings.invalidPath, strings.error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            localPathTextBox.Text = previous;
        }
    }
}
