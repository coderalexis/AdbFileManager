
namespace AdbFileManager
{
    internal partial class MainForm
    {
        private void OnMainResize(object sender, EventArgs e)
        {
            const int margin = 24;
            const int middleSpace = 52;

            int realWidth = this.Width - 16; //"form size" includes the windows borders for some reason 🤨
            int x = realWidth - 100;

            int listAndroidWidth = x / 2;
            int listPCWidth = x - listAndroidWidth;

            androidFilesGrid.Width = listAndroidWidth;

            if (_settings.Current.ShowAndroidBackButton)
            {
                androidPathTextBox.Width = listAndroidWidth - 45;
                parentDirectoryButton.Visible = true;
                parentDirectoryButton.Left = margin + listAndroidWidth - 45;
            }
            else
            {
                parentDirectoryButton.Visible = false;
                androidPathTextBox.Width = listAndroidWidth;
            }

            transferActionsPanel.Left = margin + listAndroidWidth - 1;

            localFilesBrowser.Width = listPCWidth;
            localFilesBrowser.Left = margin + listAndroidWidth + middleSpace;
            localFilesBrowser.Height = this.Height - 104;

            localBackButton.Left = margin + listAndroidWidth + middleSpace;
            localForwardButton.Left = margin + listAndroidWidth + middleSpace + 26;
            localPathTextBox.Width = listPCWidth - 57;
            localPathTextBox.Left = margin + listAndroidWidth + middleSpace + 57;

            refreshButton.BringToFront();


            footerPanel.Width = this.Width;
            footerPanel.Left = 0;
            versionLabel.Left = this.Width - 121;
            comboBox_lang.Left = this.Width - 242;

            unlockButton.Left = this.Width - 297;
            deco_panel6.Left = this.Width - 297;
            deviceComboBox.Left = this.Width - 424;

            consoleButton.Left = this.Width - 136;

            deco_panel4.Left = this.Width - 216;

            apkAssistantPanel.Top = this.Height - 150;
        }

    }
}
