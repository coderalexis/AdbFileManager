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
    internal partial class ApkInstallWizard : Form
    {
        private readonly IAdbClient _adb;
        private readonly string? _deviceSerial;
        internal ApkInstallWizard(string path, string? deviceSerial, IAdbClient adb, AppTheme theme)
        {
            _adb = adb;
            _deviceSerial = deviceSerial;
            InitializeComponent();
            theme.Apply(this);
            apkPathTextBox.Text = path;
        }
        private void UpdateGeneratedString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("adb install ");
            if (checkBox_allPermissions.Checked)
                sb.Append("--grant-all-permissions ");
            if (checkBox_allowDowngrade.Checked)
                sb.Append("--downgrade ");
            if (checkBox_noStreaming.Checked)
                sb.Append("--no-streaming ");
            if (checkBox_tooOldBypass.Checked)
                sb.Append("--bypass-low-target-sdk-block ");
            if (checkBox_replace.Checked)
                sb.Append("--replace ");

            if (radioButton_destInternal.Checked)
                sb.Append("--install-location 1 ");
            if (radioButton_destSD.Checked)
                sb.Append("--install-location 2 ");

            sb.Append(customFlagsTextBox.Text);
            sb.Append(" ");

            sb.Append(apkPathTextBox.Text);

            commandPreviewTextBox.Text = sb.ToString();
        }

        private void radioButton_dest_CheckedChanged(object sender, EventArgs e)
        {
            if (sender as RadioButton is RadioButton rb && rb.Checked)
            {
                UpdateGeneratedString();
            }
        }

        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateGeneratedString();
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            UpdateGeneratedString();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            installButton.Enabled = false;
            try
            {
                var arguments = new List<string> { "install" };
                if (checkBox_allPermissions.Checked)
                    arguments.Add("--grant-all-permissions");
                if (checkBox_allowDowngrade.Checked)
                    arguments.Add("--downgrade");
                if (checkBox_noStreaming.Checked)
                    arguments.Add("--no-streaming");
                if (checkBox_tooOldBypass.Checked)
                    arguments.Add("--bypass-low-target-sdk-block");
                if (checkBox_replace.Checked)
                    arguments.Add("--replace");
                if (radioButton_destInternal.Checked)
                    arguments.AddRange(new[] { "--install-location", "1" });
                if (radioButton_destSD.Checked)
                    arguments.AddRange(new[] { "--install-location", "2" });
                arguments.AddRange(CommandArguments.Parse(customFlagsTextBox.Text));
                arguments.Add(apkPathTextBox.Text);
                var result = await _adb.ExecuteAsync(arguments, _deviceSerial);
                result.EnsureSuccess();
                MessageBox.Show(result.CombinedOutput, "APK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "APK", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { if (!IsDisposed) installButton.Enabled = true; }
        }
    }
}
