namespace AdbFileManager;

internal partial class UnlockForm : Form
{
    private readonly IAdbClient _adb;
    private readonly DeviceSession _session;
    internal UnlockForm(IAdbClient adb, DeviceSession session, AppTheme theme)
    {
        _adb = adb;
        _session = session;
        InitializeComponent();
        Text = strings.unlock_title;
        passwordTextBox.PlaceholderText = strings.unlock_passwordPlaceholder;
        descriptionTextBox.Text = strings.unlock_description;
        theme.Apply(this);
    }
    private async void UnlockForm_Load(object sender, EventArgs e) => await RefreshDevicesAsync();
    private void keypad1_NumberClick(object sender, EventArgs e) => passwordTextBox.Text += pinKeypad.RaisedNumber.ToString();
    private async void keypad1_OkClick(object sender, EventArgs e)
    {
        try
        {
            await _adb.QueryAsync(new[] { "shell", "input text " + AdbClient.QuoteShell(passwordTextBox.Text) + " && input keyevent 66" }, _session.SelectedSerial);
            await RefreshDevicesAsync();
        }
        catch (Exception ex) { if (!IsDisposed) MessageBox.Show(this, ex.Message, strings.error); }
    }
    private async Task RefreshDevicesAsync()
    {
        try
        {
            string output = await _adb.QueryAsync(new[] { "devices" });
            if (IsDisposed)
                return;
            devicesTextBox.Text = output.TrimEnd();
            lockStatePicture.Image = output.Contains("unauthorized") || output.Split('\n').Count(line => line.Contains("\tdevice")) == 0
                ? Properties.Resources.lockedShadow : Properties.Resources.unlockedShadow;
        }
        catch (Exception ex) { if (!IsDisposed) devicesTextBox.Text = ex.Message; }
    }
}
