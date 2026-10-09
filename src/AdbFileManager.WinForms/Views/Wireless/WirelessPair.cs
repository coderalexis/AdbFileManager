using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdbFileManager
{
    internal partial class WirelessPair : Form
    {
        private readonly IAdbClient _adb;
        private readonly AppTheme _theme;
        internal WirelessPair(IAdbClient adb, AppTheme theme)
        {
            _adb = adb;
            _theme = theme;
            InitializeComponent();
            ApplyLocalization();
            _theme.Apply(this);
        }

        private void ApplyLocalization()
        {
            this.Text = AdbFileManager.strings.wireless_title;
            addressTextBox.PlaceholderText = AdbFileManager.strings.wireless_ipAddress;
            portTextBox.PlaceholderText = AdbFileManager.strings.wireless_port;
            pairingCodeTextBox.PlaceholderText = AdbFileManager.strings.wireless_pairingCode;
            button_pair.Text = AdbFileManager.strings.wireless_pair;
            button_reconnect.Text = AdbFileManager.strings.wireless_reconnect;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string ip = addressTextBox.Text.Trim();
            string port = portTextBox.Text.Trim();
            string pairingCode = pairingCodeTextBox.Text.Trim();
            if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(port))
            {
                MessageBox.Show(AdbFileManager.strings.wireless_enterIpPort, AdbFileManager.strings.error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrEmpty(pairingCode))
            {
                MessageBox.Show(AdbFileManager.strings.wireless_enterPairingCode, AdbFileManager.strings.error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try
            {
                button_pair.Enabled = false;
                var result = await _adb.ExecuteAsync(new[] { "pair", $"{ip}:{port}", pairingCode });
                result.EnsureSuccess();
                MessageBox.Show(AdbFileManager.strings.wireless_pairingFinished + "\n" + result.Output,
                    AdbFileManager.strings.success, MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{AdbFileManager.strings.exception} {ex.Message}", AdbFileManager.strings.error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { if (!IsDisposed) button_pair.Enabled = true; }
        }

        private async void button_reconnect_Click(object sender, EventArgs e)
        {
            string ip = addressTextBox.Text.Trim();
            string port = portTextBox.Text.Trim();
            if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(port))
            {
                MessageBox.Show(AdbFileManager.strings.wireless_enterIpPort, AdbFileManager.strings.error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try
            {
                var result = await _adb.ExecuteAsync(new[] { "connect", $"{ip}:{port}" });
                result.EnsureSuccess();
                MessageBox.Show(AdbFileManager.strings.wireless_reconnectFinished + "\n" + result.CombinedOutput,
                    AdbFileManager.strings.info, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{AdbFileManager.strings.exception} {ex.Message}", AdbFileManager.strings.error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
