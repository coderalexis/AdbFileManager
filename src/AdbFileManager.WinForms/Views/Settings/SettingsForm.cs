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
    internal partial class SettingsForm : Form
    {
        private readonly SettingsService _settings;
        private readonly IAdbClient _adb;
        private readonly AppTheme _theme;
        internal SettingsForm(SettingsService settings, IAdbClient adb, AppTheme theme)
        {
            _settings = settings;
            _adb = adb;
            _theme = theme;
            InitializeComponent();
            SettingsValidator.Normalize(_settings.Current);
            //change the tab to the second tab
            settingsTabs.SelectedTab = appearanceTab;

            // Load settings
            darkModeCheckBox.Checked = _settings.Current.DarkMode;


            aeroIconsRadio.Checked = !_settings.Current.UseWindows11Icons; // Windows Aero
            windows11IconsRadio.Checked = _settings.Current.UseWindows11Icons; // Windows 11

            shadedButtonsRadio.Checked = _settings.Current.ButtonStyle == 0; // Flat shaded
            flatButtonsRadio.Checked = _settings.Current.ButtonStyle == 1; // Flat
            fluentButtonsRadio.Checked = _settings.Current.ButtonStyle == 2; // Fluent gradient

            progressIntervalSlider.Value = (_settings.Current.ProgressIntervalMs - 20) / 10;
            progressIntervalValueLabel.Text = _settings.Current.ProgressIntervalMs + " ms";
            progressIntervalValueLabel.Left = (progressIntervalSlider.Left + progressIntervalSlider.Value * (progressIntervalSlider.Width - 20) / progressIntervalSlider.Maximum) - 8;

            languageComboBox.SelectedIndex = _settings.Current.Language.HasValue ? _settings.Current.Language.Value : 0; // Default to first language


            showBackButtonCheckBox.Checked = _settings.Current.ShowAndroidBackButton;

            compatibilityCheckBox.Checked = _settings.Current.UseCompatibilityMode;
            previewMediaCheckBox.Checked = _settings.Current.PreviewMediaFiles;
            keepFileDateCheckBox.Checked = _settings.Current.KeepFileModificationDate;
            rememberLocationCheckBox.Checked = _settings.Current.RememberDirectory;



            ApplyLocalization();

            loadingSettings = false;
            _theme.Apply(this);
        }

        bool loadingSettings = true;

        private void radioButton_iconStyle_CheckedChanged(object sender, EventArgs e)
        {
            if (loadingSettings)
                return;
            if (aeroIconsRadio.Checked)
            {
                _settings.Current.UseWindows11Icons = false; // Windows Aero
            }
            else if (windows11IconsRadio.Checked)
            {
                _settings.Current.UseWindows11Icons = true; // Windows 11
            }
            restartNeededChangesMade = true;
        }

        private void radioButton_buttonStyle_CheckedChanged(object sender, EventArgs e)
        {
            if (loadingSettings)
                return;
            if (shadedButtonsRadio.Checked)
            {
                _settings.Current.ButtonStyle = 0; // Flat shaded
            }
            else if (flatButtonsRadio.Checked)
            {
                _settings.Current.ButtonStyle = 1; // Flat
            }
            else if (fluentButtonsRadio.Checked)
            {
                _settings.Current.ButtonStyle = 2; // Fluent gradient
            }
            restartNeededChangesMade = true;
        }

        private void checkBox_darkMode_CheckedChanged(object sender, EventArgs e)
        {
            if (loadingSettings)
                return;
            _settings.Current.DarkMode = darkModeCheckBox.Checked;
            restartNeededChangesMade = true;
        }

        private void comboBox_lang_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loadingSettings)
                return;
            _settings.Current.Language = (ushort)languageComboBox.SelectedIndex;
            restartNeededChangesMade = true;
        }
        bool restartNeededChangesMade = false;
        private void SettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                _settings.Save();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
            {
                e.Cancel = true;
                MessageBox.Show(this, LocalizationText.Get("settings_saveError") + Environment.NewLine + ex.Message,
                    strings.settings_title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (restartNeededChangesMade)
            {
                string message = AdbFileManager.strings.restartNeeded;
                MessageBox.Show(message, AdbFileManager.strings.restartRequired, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void buttonSaveAndClose_Click(object sender, EventArgs e)
        {
            Close();
        }





        private void trackBar_progressWait_Scroll(object sender, EventArgs e)
        {
            //move progressIntervalValueLabel according to trackbar position
            progressIntervalValueLabel.Left = (progressIntervalSlider.Left + progressIntervalSlider.Value * (progressIntervalSlider.Width - 20) / progressIntervalSlider.Maximum) - 8;

            int value = progressIntervalSlider.Value * 10 + 20;

            progressIntervalValueLabel.Text = value.ToString() + " ms";

            _adb.ProgressIntervalMs = value;

            if (loadingSettings)
                return;

            _settings.Current.ProgressIntervalMs = value;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (loadingSettings)
                return;
            _settings.Current.ShowAndroidBackButton = showBackButtonCheckBox.Checked;
        }

        private void ApplyLocalization()
        {
            this.Text = AdbFileManager.strings.settings_title;

            // Tabs
            if (settingsTabs.TabPages.Count > 0)
                settingsTabs.TabPages[0].Text = AdbFileManager.strings.settings_tab_behaviour;
            appearanceTab.Text = AdbFileManager.strings.settings_tab_appearance;

            // Behaviour tab
            languageLabel.Text = AdbFileManager.strings.settings_language;
            keepFileDateCheckBox.Text = AdbFileManager.strings.settings_keepFileDate;
            previewMediaCheckBox.Text = AdbFileManager.strings.settings_previewMedia;
            progressIntervalLabel.Text = LocalizationText.Get("settings_progressUpdateInterval");
            toolTip1.SetToolTip(progressIntervalSlider, LocalizationText.Get("settings_progressUpdateInterval"));
            compatibilityCheckBox.Text = AdbFileManager.strings.settings_compatibilityFix;
            rememberLocationCheckBox.Text = AdbFileManager.strings.settings_openLastLocation;

            // Appearance tab
            iconStyleLabel.Text = AdbFileManager.strings.settings_iconStyle;
            aeroIconsRadio.Text = AdbFileManager.strings.settings_windowsAero;
            windows11IconsRadio.Text = AdbFileManager.strings.settings_windows11;
            buttonStylePanel.Visible = false;
            buttonStyleLabel.Text = UserInterfaceText.Get("unifiedStyle");
            buttonStyleLabel.MaximumSize = new Size(370, 0);
            shadedButtonsRadio.Text = AdbFileManager.strings.settings_flatShaded;
            flatButtonsRadio.Text = AdbFileManager.strings.settings_flat;
            fluentButtonsRadio.Text = AdbFileManager.strings.settings_fluentGradient;
            darkModeCheckBox.Text = AdbFileManager.strings.settings_darkMode;
            restartNoteLabel.Text = AdbFileManager.strings.settings_darkModeNote;
            settingsSaveHint.Text = LocalizationText.Get("settings_saveHint") ?? "Changes are saved when this window closes.";
            buttonSaveAndClose.Text = LocalizationText.Get("settings_saveAndClose") ?? "Save and close";
            showBackButtonCheckBox.Text = AdbFileManager.strings.settings_showBackButton;
            showBackButtonCheckBox.Visible = false; // Up is always available in the navigation bar.
        }

        private void checkBox_compatibilityMode_CheckedChanged(object sender, EventArgs e)
        {
            if (loadingSettings)
                return;
            _settings.Current.UseCompatibilityMode = compatibilityCheckBox.Checked;
        }

        private void checkBox_previewMediaFiles_CheckedChanged(object sender, EventArgs e)
        {
            if (loadingSettings)
                return;
            _settings.Current.PreviewMediaFiles = previewMediaCheckBox.Checked;
        }

        private void checkBox_keepFileModificationDate_CheckedChanged(object sender, EventArgs e)
        {
            if (loadingSettings)
                return;
            _settings.Current.KeepFileModificationDate = keepFileDateCheckBox.Checked;
        }

        private void checkBox_rememberLocation_CheckedChanged(object sender, EventArgs e)
        {
            if (loadingSettings)
                return;
            _settings.Current.RememberDirectory = rememberLocationCheckBox.Checked;
        }
    }
}
