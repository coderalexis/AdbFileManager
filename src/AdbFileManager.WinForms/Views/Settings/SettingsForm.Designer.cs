namespace AdbFileManager {
	partial class SettingsForm {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if(disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            components = new System.ComponentModel.Container();
            TabPage behaviorTab;
            progressIntervalValueLabel = new Label();
            progressIntervalLabel = new Label();
            progressIntervalSlider = new TrackBar();
            languageLabel = new Label();
            languageComboBox = new ComboBox();
            rememberLocationCheckBox = new CheckBox();
            compatibilityCheckBox = new CheckBox();
            previewMediaCheckBox = new CheckBox();
            keepFileDateCheckBox = new CheckBox();
            settingsTabs = new TabControl();
            appearanceTab = new TabPage();
            showBackButtonCheckBox = new CheckBox();
            restartNoteLabel = new Label();
            darkModeCheckBox = new CheckBox();
            buttonStylePanel = new Panel();
            fluentButtonsRadio = new RadioButton();
            shadedButtonsRadio = new RadioButton();
            flatButtonsRadio = new RadioButton();
            buttonStyleLabel = new Label();
            iconStylePanel = new Panel();
            windows11IconsRadio = new RadioButton();
            aeroIconsRadio = new RadioButton();
            iconStyleLabel = new Label();
            settingsActionsPanel = new Panel();
            settingsSaveHint = new Label();
            buttonSaveAndClose = new Button();
            toolTip1 = new ToolTip(components);
            behaviorTab = new TabPage();
            behaviorTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)progressIntervalSlider).BeginInit();
            settingsTabs.SuspendLayout();
            appearanceTab.SuspendLayout();
            buttonStylePanel.SuspendLayout();
            iconStylePanel.SuspendLayout();
            settingsActionsPanel.SuspendLayout();
            SuspendLayout();
            //
            // behaviorTab
            //
            behaviorTab.Controls.Add(progressIntervalValueLabel);
            behaviorTab.Controls.Add(progressIntervalLabel);
            behaviorTab.Controls.Add(progressIntervalSlider);
            behaviorTab.Controls.Add(languageLabel);
            behaviorTab.Controls.Add(languageComboBox);
            behaviorTab.Controls.Add(rememberLocationCheckBox);
            behaviorTab.Controls.Add(compatibilityCheckBox);
            behaviorTab.Controls.Add(previewMediaCheckBox);
            behaviorTab.Controls.Add(keepFileDateCheckBox);
            behaviorTab.Location = new Point(4, 24);
            behaviorTab.Name = "behaviorTab";
            behaviorTab.Padding = new Padding(3);
            behaviorTab.Size = new Size(448, 282);
            behaviorTab.TabIndex = 0;
            behaviorTab.Text = "Behaviour";
            behaviorTab.UseVisualStyleBackColor = true;
            //
            // progressIntervalValueLabel
            //
            progressIntervalValueLabel.AutoSize = true;
            progressIntervalValueLabel.Location = new Point(18, 170);
            progressIntervalValueLabel.Name = "progressIntervalValueLabel";
            progressIntervalValueLabel.Size = new Size(35, 15);
            progressIntervalValueLabel.TabIndex = 12;
            progressIntervalValueLabel.Text = "20ms";
            //
            // progressIntervalLabel
            //
            progressIntervalLabel.AutoSize = true;
            progressIntervalLabel.Location = new Point(21, 105);
            progressIntervalLabel.Name = "progressIntervalLabel";
            progressIntervalLabel.Size = new Size(110, 15);
            progressIntervalLabel.TabIndex = 11;
            progressIntervalLabel.Text = "Progress sync delay";
            toolTip1.SetToolTip(progressIntervalLabel, "Adjusts the timing how long the copy operation waits for copy progress\r\nDO NOT CHANGE IF NOT SURE WHAT THIS DOES!\r\nDefault 50ms");
            //
            // progressIntervalSlider
            //
            progressIntervalSlider.Location = new Point(17, 120);
            progressIntervalSlider.Maximum = 50;
            progressIntervalSlider.Name = "progressIntervalSlider";
            progressIntervalSlider.Size = new Size(147, 45);
            progressIntervalSlider.TabIndex = 10;
            progressIntervalSlider.TickFrequency = 5;
            toolTip1.SetToolTip(progressIntervalSlider, "Adjusts the timing how long the copy operation waits for copy progress");
            progressIntervalSlider.Scroll += trackBar_progressWait_Scroll;
            //
            //
            //
            // languageLabel
            //
            languageLabel.AutoSize = true;
            languageLabel.Location = new Point(6, 198);
            languageLabel.Name = "languageLabel";
            languageLabel.Size = new Size(59, 15);
            languageLabel.TabIndex = 8;
            languageLabel.Text = "Language";
            //
            // languageComboBox
            //
            languageComboBox.FormattingEnabled = true;
            languageComboBox.Items.AddRange(new object[] { "English", "Čeština", "Polski", "Deutsch", "Japanese", "Espanol", "简体中文", "繁體中文" });
            languageComboBox.Location = new Point(8, 216);
            languageComboBox.Name = "languageComboBox";
            languageComboBox.Size = new Size(121, 23);
            languageComboBox.TabIndex = 7;
            languageComboBox.SelectedIndexChanged += comboBox_lang_SelectedIndexChanged;
            //
            //
            //
            // rememberLocationCheckBox
            //
            rememberLocationCheckBox.AutoSize = true;
            rememberLocationCheckBox.Location = new Point(267, 56);
            rememberLocationCheckBox.Name = "rememberLocationCheckBox";
            rememberLocationCheckBox.Size = new Size(135, 19);
            rememberLocationCheckBox.TabIndex = 5;
            rememberLocationCheckBox.Text = "Open in last location";
            rememberLocationCheckBox.UseVisualStyleBackColor = true;
            rememberLocationCheckBox.CheckedChanged += checkBox_rememberLocation_CheckedChanged;
            //
            // compatibilityCheckBox
            //
            compatibilityCheckBox.AutoSize = true;
            compatibilityCheckBox.Location = new Point(267, 6);
            compatibilityCheckBox.Name = "compatibilityCheckBox";
            compatibilityCheckBox.Size = new Size(114, 19);
            compatibilityCheckBox.TabIndex = 3;
            compatibilityCheckBox.Text = "Compatibility fix";
            compatibilityCheckBox.UseVisualStyleBackColor = true;
            compatibilityCheckBox.CheckedChanged += checkBox_compatibilityMode_CheckedChanged;
            //
            //
            //
            // previewMediaCheckBox
            //
            previewMediaCheckBox.AutoSize = true;
            previewMediaCheckBox.Location = new Point(9, 31);
            previewMediaCheckBox.Name = "previewMediaCheckBox";
            previewMediaCheckBox.Size = new Size(211, 19);
            previewMediaCheckBox.TabIndex = 1;
            previewMediaCheckBox.Text = "Preview media files on double click";
            previewMediaCheckBox.UseVisualStyleBackColor = true;
            previewMediaCheckBox.CheckedChanged += checkBox_previewMediaFiles_CheckedChanged;
            //
            // keepFileDateCheckBox
            //
            keepFileDateCheckBox.AutoSize = true;
            keepFileDateCheckBox.Location = new Point(9, 6);
            keepFileDateCheckBox.Name = "keepFileDateCheckBox";
            keepFileDateCheckBox.Size = new Size(168, 19);
            keepFileDateCheckBox.TabIndex = 0;
            keepFileDateCheckBox.Text = "Keep file modification date";
            keepFileDateCheckBox.UseVisualStyleBackColor = true;
            keepFileDateCheckBox.CheckedChanged += checkBox_keepFileModificationDate_CheckedChanged;
            //
            //
            //
            // settingsTabs
            //
            settingsTabs.Controls.Add(behaviorTab);
            settingsTabs.Controls.Add(appearanceTab);
            settingsTabs.Dock = DockStyle.Fill;
            settingsTabs.Location = new Point(0, 0);
            settingsTabs.Name = "settingsTabs";
            settingsTabs.SelectedIndex = 0;
            settingsTabs.Size = new Size(456, 310);
            settingsTabs.TabIndex = 0;
            //
            // appearanceTab
            //
            appearanceTab.Controls.Add(showBackButtonCheckBox);
            appearanceTab.Controls.Add(restartNoteLabel);
            appearanceTab.Controls.Add(darkModeCheckBox);
            appearanceTab.Controls.Add(buttonStylePanel);
            appearanceTab.Controls.Add(buttonStyleLabel);
            appearanceTab.Controls.Add(iconStylePanel);
            appearanceTab.Controls.Add(iconStyleLabel);
            appearanceTab.Location = new Point(4, 24);
            appearanceTab.Name = "appearanceTab";
            appearanceTab.Padding = new Padding(3);
            appearanceTab.Size = new Size(448, 282);
            appearanceTab.TabIndex = 1;
            appearanceTab.Text = "Appearance";
            appearanceTab.UseVisualStyleBackColor = true;
            //
            // showBackButtonCheckBox
            //
            showBackButtonCheckBox.AutoSize = true;
            showBackButtonCheckBox.Checked = true;
            showBackButtonCheckBox.CheckState = CheckState.Checked;
            showBackButtonCheckBox.Location = new Point(154, 101);
            showBackButtonCheckBox.Name = "showBackButtonCheckBox";
            showBackButtonCheckBox.Size = new Size(168, 19);
            showBackButtonCheckBox.TabIndex = 7;
            showBackButtonCheckBox.Text = "Show Android back button";
            toolTip1.SetToolTip(showBackButtonCheckBox, "Shows back button on the android side alongside path.\r\nAlternative to the default way of double clicking the header");
            showBackButtonCheckBox.UseVisualStyleBackColor = true;
            showBackButtonCheckBox.CheckedChanged += checkBox3_CheckedChanged;
            //
            //
            //
            // restartNoteLabel
            //
            restartNoteLabel.AutoSize = true;
            restartNoteLabel.Font = new Font("Segoe UI", 7F);
            restartNoteLabel.Location = new Point(171, 45);
            restartNoteLabel.Name = "restartNoteLabel";
            restartNoteLabel.Size = new Size(186, 24);
            restartNoteLabel.TabIndex = 5;
            restartNoteLabel.Text = "Note: PC File Explorer will only be in dark \r\nif Windows itself has dark mode enabled";
            //
            // darkModeCheckBox
            //
            darkModeCheckBox.AutoSize = true;
            darkModeCheckBox.Location = new Point(154, 25);
            darkModeCheckBox.Name = "darkModeCheckBox";
            darkModeCheckBox.Size = new Size(121, 19);
            darkModeCheckBox.TabIndex = 4;
            darkModeCheckBox.Text = "Enable dark mode";
            darkModeCheckBox.UseVisualStyleBackColor = true;
            darkModeCheckBox.CheckedChanged += checkBox_darkMode_CheckedChanged;
            //
            // buttonStylePanel
            //
            buttonStylePanel.Controls.Add(fluentButtonsRadio);
            buttonStylePanel.Controls.Add(shadedButtonsRadio);
            buttonStylePanel.Controls.Add(flatButtonsRadio);
            buttonStylePanel.Location = new Point(6, 116);
            buttonStylePanel.Name = "buttonStylePanel";
            buttonStylePanel.Size = new Size(118, 73);
            buttonStylePanel.TabIndex = 3;
            //
            // fluentButtonsRadio
            //
            fluentButtonsRadio.AutoSize = true;
            fluentButtonsRadio.Location = new Point(5, 51);
            fluentButtonsRadio.Name = "fluentButtonsRadio";
            fluentButtonsRadio.Size = new Size(105, 19);
            fluentButtonsRadio.TabIndex = 2;
            fluentButtonsRadio.TabStop = true;
            fluentButtonsRadio.Text = "Fluent gradient";
            fluentButtonsRadio.UseVisualStyleBackColor = true;
            fluentButtonsRadio.CheckedChanged += radioButton_buttonStyle_CheckedChanged;
            //
            // shadedButtonsRadio
            //
            shadedButtonsRadio.AutoSize = true;
            shadedButtonsRadio.Location = new Point(5, 3);
            shadedButtonsRadio.Name = "shadedButtonsRadio";
            shadedButtonsRadio.Size = new Size(85, 19);
            shadedButtonsRadio.TabIndex = 1;
            shadedButtonsRadio.TabStop = true;
            shadedButtonsRadio.Text = "Flat shaded";
            shadedButtonsRadio.UseVisualStyleBackColor = true;
            shadedButtonsRadio.CheckedChanged += radioButton_buttonStyle_CheckedChanged;
            //
            // flatButtonsRadio
            //
            flatButtonsRadio.AutoSize = true;
            flatButtonsRadio.Location = new Point(5, 28);
            flatButtonsRadio.Name = "flatButtonsRadio";
            flatButtonsRadio.Size = new Size(44, 19);
            flatButtonsRadio.TabIndex = 0;
            flatButtonsRadio.TabStop = true;
            flatButtonsRadio.Text = "Flat";
            flatButtonsRadio.UseVisualStyleBackColor = true;
            flatButtonsRadio.CheckedChanged += radioButton_buttonStyle_CheckedChanged;
            //
            // buttonStyleLabel
            //
            buttonStyleLabel.AutoSize = true;
            buttonStyleLabel.Location = new Point(9, 98);
            buttonStyleLabel.Name = "buttonStyleLabel";
            buttonStyleLabel.Size = new Size(81, 15);
            buttonStyleLabel.TabIndex = 2;
            buttonStyleLabel.Text = "Button design";
            //
            // iconStylePanel
            //
            iconStylePanel.Controls.Add(windows11IconsRadio);
            iconStylePanel.Controls.Add(aeroIconsRadio);
            iconStylePanel.Location = new Point(8, 22);
            iconStylePanel.Name = "iconStylePanel";
            iconStylePanel.Size = new Size(118, 73);
            iconStylePanel.TabIndex = 1;
            //
            // windows11IconsRadio
            //
            windows11IconsRadio.AutoSize = true;
            windows11IconsRadio.Location = new Point(3, 28);
            windows11IconsRadio.Name = "windows11IconsRadio";
            windows11IconsRadio.Size = new Size(89, 19);
            windows11IconsRadio.TabIndex = 1;
            windows11IconsRadio.TabStop = true;
            windows11IconsRadio.Text = "Windows 11";
            windows11IconsRadio.UseVisualStyleBackColor = true;
            windows11IconsRadio.CheckedChanged += radioButton_iconStyle_CheckedChanged;
            //
            // aeroIconsRadio
            //
            aeroIconsRadio.AutoSize = true;
            aeroIconsRadio.Location = new Point(3, 3);
            aeroIconsRadio.Name = "aeroIconsRadio";
            aeroIconsRadio.Size = new Size(102, 19);
            aeroIconsRadio.TabIndex = 0;
            aeroIconsRadio.TabStop = true;
            aeroIconsRadio.Text = "Windows Aero";
            aeroIconsRadio.UseVisualStyleBackColor = true;
            aeroIconsRadio.CheckedChanged += radioButton_iconStyle_CheckedChanged;
            //
            // iconStyleLabel
            //
            iconStyleLabel.AutoSize = true;
            iconStyleLabel.Location = new Point(11, 4);
            iconStyleLabel.Name = "iconStyleLabel";
            iconStyleLabel.Size = new Size(57, 15);
            iconStyleLabel.TabIndex = 0;
            iconStyleLabel.Text = "Icon style";
            //
            // settingsActionsPanel
            //
            settingsActionsPanel.Controls.Add(settingsSaveHint);
            settingsActionsPanel.Controls.Add(buttonSaveAndClose);
            settingsActionsPanel.Dock = DockStyle.Bottom;
            settingsActionsPanel.Location = new Point(0, 310);
            settingsActionsPanel.Name = "settingsActionsPanel";
            settingsActionsPanel.Size = new Size(456, 48);
            settingsActionsPanel.TabIndex = 1;
            //
            // settingsSaveHint
            //
            settingsSaveHint.AutoSize = true;
            settingsSaveHint.Location = new Point(12, 17);
            settingsSaveHint.Name = "settingsSaveHint";
            settingsSaveHint.Size = new Size(216, 15);
            settingsSaveHint.TabIndex = 0;
            settingsSaveHint.Text = "Changes are saved when this window closes.";
            //
            // buttonSaveAndClose
            //
            buttonSaveAndClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonSaveAndClose.Location = new Point(322, 9);
            buttonSaveAndClose.Name = "buttonSaveAndClose";
            buttonSaveAndClose.Size = new Size(122, 30);
            buttonSaveAndClose.TabIndex = 1;
            buttonSaveAndClose.Text = "Save and close";
            buttonSaveAndClose.UseVisualStyleBackColor = true;
            buttonSaveAndClose.Click += buttonSaveAndClose_Click;
            //
            // SettingsForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(456, 358);
            Controls.Add(settingsTabs);
            Controls.Add(settingsActionsPanel);
            Name = "SettingsForm";
            Text = "SettingsForm";
            FormClosing += SettingsForm_FormClosing;
            behaviorTab.ResumeLayout(false);
            behaviorTab.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)progressIntervalSlider).EndInit();
            settingsTabs.ResumeLayout(false);
            appearanceTab.ResumeLayout(false);
            appearanceTab.PerformLayout();
            buttonStylePanel.ResumeLayout(false);
            buttonStylePanel.PerformLayout();
            iconStylePanel.ResumeLayout(false);
            iconStylePanel.PerformLayout();
            settingsActionsPanel.ResumeLayout(false);
            settingsActionsPanel.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TabControl settingsTabs;
		private TabPage appearanceTab;
		private CheckBox keepFileDateCheckBox;
		private CheckBox rememberLocationCheckBox;
		private CheckBox compatibilityCheckBox;
		private CheckBox previewMediaCheckBox;
		private Label iconStyleLabel;
		private Panel iconStylePanel;
		private RadioButton windows11IconsRadio;
		private RadioButton aeroIconsRadio;
		private Panel buttonStylePanel;
		private RadioButton fluentButtonsRadio;
		private RadioButton shadedButtonsRadio;
		private RadioButton flatButtonsRadio;
		private Label buttonStyleLabel;
		private CheckBox darkModeCheckBox;
		private Label languageLabel;
		private ComboBox languageComboBox;
		private ToolTip toolTip1;
		private Label restartNoteLabel;
		private Label progressIntervalLabel;
		private TrackBar progressIntervalSlider;
		private Label progressIntervalValueLabel;
		private CheckBox showBackButtonCheckBox;
		private Panel settingsActionsPanel;
		private Label settingsSaveHint;
		private Button buttonSaveAndClose;
	}
}
