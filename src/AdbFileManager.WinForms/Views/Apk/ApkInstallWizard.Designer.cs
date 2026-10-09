namespace AdbFileManager {
	partial class ApkInstallWizard {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ApkInstallWizard));
            installButton = new Button();
            apkPathTextBox = new TextBox();
            label1 = new Label();
            label2 = new Label();
            checkBox_tooOldBypass = new CheckBox();
            checkBox_allowDowngrade = new CheckBox();
            toolTip1 = new ToolTip(components);
            customFlagsTextBox = new TextBox();
            checkBox_noStreaming = new CheckBox();
            label3 = new Label();
            checkBox_allPermissions = new CheckBox();
            checkBox_replace = new CheckBox();
            panel1 = new Panel();
            radioButton_destSD = new RadioButton();
            radioButton_destInternal = new RadioButton();
            radioButton_destAuto = new RadioButton();
            commandPreviewTextBox = new RichTextBox();
            panel1.SuspendLayout();
            SuspendLayout();
            //
            // installButton
            //
            installButton.FlatAppearance.BorderColor = Color.Black;
            installButton.Font = new Font("Segoe UI", 12F);
            installButton.ImageAlign = ContentAlignment.TopCenter;
            installButton.Location = new Point(29, 337);
            installButton.Name = "installButton";
            installButton.Size = new Size(87, 34);
            installButton.TabIndex = 0;
            installButton.Text = "Install";
            installButton.UseVisualStyleBackColor = true;
            installButton.Click += button1_Click;
            //
            // apkPathTextBox
            //
            apkPathTextBox.BackColor = Color.White;
            apkPathTextBox.Location = new Point(31, 32);
            apkPathTextBox.Name = "apkPathTextBox";
            apkPathTextBox.PlaceholderText = "Enter whole path of the apk file here";
            apkPathTextBox.Size = new Size(344, 23);
            apkPathTextBox.TabIndex = 1;
            apkPathTextBox.TextChanged += textBox_TextChanged;
            //
            // label1
            //
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 238);
            label1.Location = new Point(31, 12);
            label1.Name = "label1";
            label1.Size = new Size(74, 15);
            label1.TabIndex = 2;
            label1.Text = "Apk file path";
            //
            // label2
            //
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 238);
            label2.Location = new Point(31, 97);
            label2.Name = "label2";
            label2.Size = new Size(206, 15);
            label2.TabIndex = 3;
            label2.Text = "Extra flags [ADVANCED USERS ONLY]";
            //
            // checkBox_tooOldBypass
            //
            checkBox_tooOldBypass.AutoSize = true;
            checkBox_tooOldBypass.Location = new Point(44, 115);
            checkBox_tooOldBypass.Name = "checkBox_tooOldBypass";
            checkBox_tooOldBypass.Size = new Size(178, 19);
            checkBox_tooOldBypass.TabIndex = 4;
            checkBox_tooOldBypass.Text = "Bypass old application check";
            toolTip1.SetToolTip(checkBox_tooOldBypass, "Android 14 started blocking installation of software\r\ntargetting Android 5 or older.\r\nAdding \"--bypass-low-target-sdk-block\" bypasses this block.");
            checkBox_tooOldBypass.UseVisualStyleBackColor = true;
            checkBox_tooOldBypass.CheckedChanged += checkBox_CheckedChanged;
            //
            // checkBox_allowDowngrade
            //
            checkBox_allowDowngrade.AutoSize = true;
            checkBox_allowDowngrade.Location = new Point(44, 140);
            checkBox_allowDowngrade.Name = "checkBox_allowDowngrade";
            checkBox_allowDowngrade.Size = new Size(119, 19);
            checkBox_allowDowngrade.TabIndex = 5;
            checkBox_allowDowngrade.Text = "Allow downgrade";
            toolTip1.SetToolTip(checkBox_allowDowngrade, "If newer version of the app is already installed, \r\nenabling this allows downgrade to older version.");
            checkBox_allowDowngrade.UseVisualStyleBackColor = true;
            checkBox_allowDowngrade.CheckedChanged += checkBox_CheckedChanged;
            //
            // customFlagsTextBox
            //
            customFlagsTextBox.BackColor = Color.White;
            customFlagsTextBox.Location = new Point(43, 235);
            customFlagsTextBox.Name = "customFlagsTextBox";
            customFlagsTextBox.PlaceholderText = "Enter custom install flags here";
            customFlagsTextBox.Size = new Size(202, 23);
            customFlagsTextBox.TabIndex = 6;
            toolTip1.SetToolTip(customFlagsTextBox, "Allows you to enter any custom install flag\r\nwhich will be executed.");
            customFlagsTextBox.TextChanged += textBox_TextChanged;
            //
            // checkBox_noStreaming
            //
            checkBox_noStreaming.AutoSize = true;
            checkBox_noStreaming.Location = new Point(44, 165);
            checkBox_noStreaming.Name = "checkBox_noStreaming";
            checkBox_noStreaming.Size = new Size(98, 19);
            checkBox_noStreaming.TabIndex = 7;
            checkBox_noStreaming.Text = "No streaming";
            toolTip1.SetToolTip(checkBox_noStreaming, "By default the apk file is streamed during install.\r\nWhen enabled, the file is first copied over before installing it.");
            checkBox_noStreaming.UseVisualStyleBackColor = true;
            checkBox_noStreaming.CheckedChanged += checkBox_CheckedChanged;
            //
            // label3
            //
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 238);
            label3.Location = new Point(0, 4);
            label3.Name = "label3";
            label3.Size = new Size(85, 15);
            label3.TabIndex = 9;
            label3.Text = "Install location";
            toolTip1.SetToolTip(label3, "Sets where the app should be installed.\r\nWhether on internal storage, on external memory card, or \r\nlet the phone decide automatically.");
            //
            // checkBox_allPermissions
            //
            checkBox_allPermissions.AutoSize = true;
            checkBox_allPermissions.Location = new Point(44, 190);
            checkBox_allPermissions.Name = "checkBox_allPermissions";
            checkBox_allPermissions.Size = new Size(136, 19);
            checkBox_allPermissions.TabIndex = 9;
            checkBox_allPermissions.Text = "Grant all permissions";
            toolTip1.SetToolTip(checkBox_allPermissions, "Grants all permissions the app wants automatically.\r\n");
            checkBox_allPermissions.UseVisualStyleBackColor = true;
            checkBox_allPermissions.CheckedChanged += checkBox_CheckedChanged;
            //
            // checkBox_replace
            //
            checkBox_replace.AutoSize = true;
            checkBox_replace.Location = new Point(44, 215);
            checkBox_replace.Name = "checkBox_replace";
            checkBox_replace.Size = new Size(70, 19);
            checkBox_replace.TabIndex = 10;
            checkBox_replace.Text = "Reinstall";
            toolTip1.SetToolTip(checkBox_replace, "Reinstalls the app even if same version is already installed");
            checkBox_replace.UseVisualStyleBackColor = true;
            checkBox_replace.CheckedChanged += checkBox_CheckedChanged;
            //
            // panel1
            //
            panel1.Controls.Add(label3);
            panel1.Controls.Add(radioButton_destSD);
            panel1.Controls.Add(radioButton_destInternal);
            panel1.Controls.Add(radioButton_destAuto);
            panel1.Location = new Point(31, 55);
            panel1.Name = "panel1";
            panel1.Size = new Size(290, 41);
            panel1.TabIndex = 8;
            //
            // radioButton_destSD
            //
            radioButton_destSD.AutoSize = true;
            radioButton_destSD.Location = new Point(202, 18);
            radioButton_destSD.Name = "radioButton_destSD";
            radioButton_destSD.Size = new Size(67, 19);
            radioButton_destSD.TabIndex = 2;
            radioButton_destSD.Text = "SD Card";
            radioButton_destSD.UseVisualStyleBackColor = true;
            radioButton_destSD.CheckedChanged += radioButton_dest_CheckedChanged;
            //
            // radioButton_destInternal
            //
            radioButton_destInternal.AutoSize = true;
            radioButton_destInternal.Location = new Point(90, 18);
            radioButton_destInternal.Name = "radioButton_destInternal";
            radioButton_destInternal.Size = new Size(106, 19);
            radioButton_destInternal.TabIndex = 1;
            radioButton_destInternal.Text = "Built-in storage";
            radioButton_destInternal.UseVisualStyleBackColor = true;
            radioButton_destInternal.CheckedChanged += radioButton_dest_CheckedChanged;
            //
            // radioButton_destAuto
            //
            radioButton_destAuto.AutoSize = true;
            radioButton_destAuto.Checked = true;
            radioButton_destAuto.Location = new Point(4, 18);
            radioButton_destAuto.Name = "radioButton_destAuto";
            radioButton_destAuto.Size = new Size(81, 19);
            radioButton_destAuto.TabIndex = 0;
            radioButton_destAuto.TabStop = true;
            radioButton_destAuto.Text = "Automatic";
            radioButton_destAuto.UseVisualStyleBackColor = true;
            radioButton_destAuto.CheckedChanged += radioButton_dest_CheckedChanged;
            //
            // commandPreviewTextBox
            //
            commandPreviewTextBox.BackColor = Color.Black;
            commandPreviewTextBox.Font = new Font("Consolas", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 238);
            commandPreviewTextBox.ForeColor = Color.LightBlue;
            commandPreviewTextBox.Location = new Point(31, 280);
            commandPreviewTextBox.Name = "commandPreviewTextBox";
            commandPreviewTextBox.ScrollBars = RichTextBoxScrollBars.Vertical;
            commandPreviewTextBox.Size = new Size(344, 51);
            commandPreviewTextBox.TabIndex = 11;
            commandPreviewTextBox.Text = "adb install";
            //
            // ApkInstallWizard
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(399, 382);
            Controls.Add(commandPreviewTextBox);
            Controls.Add(checkBox_replace);
            Controls.Add(checkBox_allPermissions);
            Controls.Add(panel1);
            Controls.Add(checkBox_noStreaming);
            Controls.Add(customFlagsTextBox);
            Controls.Add(checkBox_allowDowngrade);
            Controls.Add(checkBox_tooOldBypass);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(apkPathTextBox);
            Controls.Add(installButton);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "ApkInstallWizard";
            Text = "Apk installation";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button installButton;
		private TextBox apkPathTextBox;
		private Label label1;
		private Label label2;
		private CheckBox checkBox_tooOldBypass;
		private CheckBox checkBox_allowDowngrade;
		private ToolTip toolTip1;
		private TextBox customFlagsTextBox;
		private CheckBox checkBox_noStreaming;
		private Panel panel1;
		private RadioButton radioButton_destInternal;
		private RadioButton radioButton_destAuto;
		private RadioButton radioButton_destSD;
		private Label label3;
		private CheckBox checkBox_allPermissions;
		private CheckBox checkBox_replace;
		private RichTextBox commandPreviewTextBox;
	}
}