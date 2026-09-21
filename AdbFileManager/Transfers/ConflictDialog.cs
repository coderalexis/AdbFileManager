namespace AdbFileManager.Transfers {
    internal sealed class ConflictDialog : Form {
        internal ConflictDecision Decision { get; private set; } = new(ConflictAction.Skip);
        internal ConflictDialog(TransferJob job, EntryKind destinationKind) {
            Text = QueueText.Get("conflict");
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(600, 310);
            Size = new Size(680, 340);
            Font = new Font("Segoe UI", 10);
            MaximizeBox = MinimizeBox = false;
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 1, RowCount = 4 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 65));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            bool canReplace = !job.IsDirectory && destinationKind == EntryKind.File;
            layout.Controls.Add(new Label { Text = QueueText.Get(canReplace ? "fileConflict" : "folderConflict"), Dock = DockStyle.Fill, AutoSize = false }, 0, 0);
            layout.Controls.Add(new TextBox { Text = job.Source + Environment.NewLine + "→ " + (job.ResolvedDestination ?? job.Destination),
                ReadOnly = true, Multiline = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill, TabStop = false }, 0, 1);
            var apply = new CheckBox { Text = QueueText.Get("applyBatch"), Dock = DockStyle.Fill };
            layout.Controls.Add(apply, 0, 2);
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            foreach (var action in new[] { ConflictAction.Skip, ConflictAction.KeepBoth, ConflictAction.Replace }) {
                var button = new Button { Text = QueueText.Get(action.ToString()), AutoSize = true, MinimumSize = new Size(115, 32), Enabled = action != ConflictAction.Replace || canReplace };
                button.Click += (_, _) => { Decision = new(action, apply.Checked); DialogResult = DialogResult.OK; Close(); };
                buttons.Controls.Add(button);
                if (action == ConflictAction.Skip) { AcceptButton = button; CancelButton = button; }
            }
            layout.Controls.Add(buttons, 0, 3);
            Controls.Add(layout);
        }
    }
}
