using Microsoft.WindowsAPICodePack.Controls;
namespace AdbFileManager
{
    internal partial class MainForm
    {
        private void OnLocalBackLeave(object? sender, EventArgs e)
        {
            this.localBackButton.Image = _icons.GetNavigation("travel_enabled_back");
        }

        private void OnLocalBackEnter(object? sender, EventArgs e)
        {
            this.localBackButton.Image = _icons.GetNavigation("travel_hot_back");
        }

        private void OnLocalBackDown(object? sender, MouseEventArgs e)
        {
            this.localBackButton.Image = _icons.GetNavigation("travel_pressed_back");
        }
        private void OnLocalBackUp(object? sender, MouseEventArgs e)
        {
            this.localBackButton.Image = _icons.GetNavigation("travel_hot_back");
        }

        private void OnLocalForwardLeave(object? sender, EventArgs e)
        {
            this.localForwardButton.Image = _icons.GetNavigation("travel_enabled_forward");
        }

        private void OnLocalForwardEnter(object? sender, EventArgs e)
        {
            this.localForwardButton.Image = _icons.GetNavigation("travel_hot_forward");
        }

        private void OnLocalForwardDown(object? sender, MouseEventArgs e)
        {
            this.localForwardButton.Image = _icons.GetNavigation("travel_pressed_forward");
        }
        private void OnLocalForwardUp(object? sender, MouseEventArgs e)
        {
            this.localForwardButton.Image = _icons.GetNavigation("travel_hot_forward");
        }

        private void OnLocalBackClick(object? sender, EventArgs e)
        {
            localFilesBrowser.NavigateLogLocation(NavigationLogDirection.Backward);
        }

        private void OnLocalForwardClick(object? sender, EventArgs e)
        {
            localFilesBrowser.NavigateLogLocation(NavigationLogDirection.Forward);
        }
    }
}
