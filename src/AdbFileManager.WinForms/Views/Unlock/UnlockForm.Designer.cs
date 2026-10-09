namespace AdbFileManager {
	partial class UnlockForm {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UnlockForm));
			lockStatePicture = new PictureBox();
			passwordTextBox = new TextBox();
			descriptionTextBox = new RichTextBox();
			pinKeypad = new Keypad();
			devicesTextBox = new RichTextBox();
			((System.ComponentModel.ISupportInitialize)lockStatePicture).BeginInit();
			SuspendLayout();
			//
			// lockStatePicture
			//
			lockStatePicture.Image = Properties.Resources.lockedShadow;
			lockStatePicture.Location = new Point(8, 8);
			lockStatePicture.Name = "lockStatePicture";
			lockStatePicture.Size = new Size(120, 122);
			lockStatePicture.SizeMode = PictureBoxSizeMode.StretchImage;
			lockStatePicture.TabIndex = 0;
			lockStatePicture.TabStop = false;
			//
			// passwordTextBox
			//
			passwordTextBox.Location = new Point(120, 8);
			passwordTextBox.Name = "passwordTextBox";
			passwordTextBox.PlaceholderText = "Password here";
			passwordTextBox.Size = new Size(88, 23);
			passwordTextBox.TabIndex = 2;
			//
			// descriptionTextBox
			//
			descriptionTextBox.BackColor = SystemColors.Control;
			descriptionTextBox.BorderStyle = BorderStyle.None;
			descriptionTextBox.Font = new Font("Segoe UI", 8F, FontStyle.Regular, GraphicsUnit.Point, 238);
			descriptionTextBox.Location = new Point(216, 8);
			descriptionTextBox.Name = "descriptionTextBox";
			descriptionTextBox.Size = new Size(176, 128);
			descriptionTextBox.TabIndex = 14;
			descriptionTextBox.Text = resources.GetString("descriptionTextBox.Text");
			//
			// pinKeypad
			//
			pinKeypad.Location = new Point(127, 32);
			pinKeypad.Name = "pinKeypad";
			pinKeypad.Size = new Size(74, 97);
			pinKeypad.TabIndex = 15;
			pinKeypad.OkClick += keypad1_OkClick;
			pinKeypad.NumberClick += keypad1_NumberClick;
			//
			// devicesTextBox
			//
			devicesTextBox.BackColor = SystemColors.Control;
			devicesTextBox.BorderStyle = BorderStyle.None;
			devicesTextBox.Location = new Point(8, 136);
			devicesTextBox.Name = "devicesTextBox";
			devicesTextBox.Size = new Size(384, 40);
			devicesTextBox.TabIndex = 16;
			devicesTextBox.Text = "";
			//
			// UnlockForm
			//
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(393, 178);
			Controls.Add(devicesTextBox);
			Controls.Add(pinKeypad);
			Controls.Add(descriptionTextBox);
			Controls.Add(passwordTextBox);
			Controls.Add(lockStatePicture);
			Name = "UnlockForm";
			Text = "UnlockForm";
			Load += UnlockForm_Load;
			((System.ComponentModel.ISupportInitialize)lockStatePicture).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private PictureBox lockStatePicture;
		private TextBox passwordTextBox;
		private RichTextBox descriptionTextBox;
		private Keypad pinKeypad;
		private RichTextBox devicesTextBox;
	}
}
