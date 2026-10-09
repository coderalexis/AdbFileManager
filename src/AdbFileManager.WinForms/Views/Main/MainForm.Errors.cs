namespace AdbFileManager;

internal partial class MainForm
{
    private void ShowErrorDetails(string message)
    {
        using var dialog = new Form
        {
            Text = Ux("errorDetails"),
            Size = new Size(700, 400),
            MinimumSize = new Size(450, 250),
            StartPosition = FormStartPosition.CenterParent,
            AutoScaleMode = AutoScaleMode.Dpi
        };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 1, RowCount = 2 };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.Controls.Add(new TextBox { Text = message, Multiline = true, ReadOnly = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical, AccessibleName = Ux("errorDetails") }, 0, 0);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        var close = new Button { Text = Ux("close"), AutoSize = true, DialogResult = DialogResult.OK };
        var copy = new Button { Text = Ux("copyDetails"), AutoSize = true };
        copy.Click += (_, _) => { try { Clipboard.SetText(message); } catch (System.Runtime.InteropServices.ExternalException) { } };
        buttons.Controls.Add(close);
        buttons.Controls.Add(copy);
        layout.Controls.Add(buttons, 0, 1);
        dialog.Controls.Add(layout);
        dialog.AcceptButton = dialog.CancelButton = close;
        _theme.Apply(dialog);
        dialog.ShowDialog(this);
    }
}
