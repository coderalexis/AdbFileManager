namespace AdbFileManager.Views.Transfers
{
    internal sealed class TransferQueueForm : Form
    {
        private readonly AppTheme _theme;
        private readonly TransferQueue queue;
        private readonly Func<Task> run;
        private readonly DataGridView grid = new()
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        private readonly Label summary = new() { Dock = DockStyle.Fill, AutoEllipsis = true, TextAlign = ContentAlignment.MiddleLeft };
        private readonly TextBox details = new() { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical };
        private readonly Dictionary<Guid, DataGridViewRow> rows = new();
        private readonly Label activity = new() { Dock = DockStyle.Fill, AutoEllipsis = true, TextAlign = ContentAlignment.MiddleLeft };
        private readonly ProgressBar activityProgress = new() { Dock = DockStyle.Bottom, Height = 6, MarqueeAnimationSpeed = 30 };
        private readonly System.Windows.Forms.Timer statisticsTimer = new() { Interval = 500 };
        internal bool AllowClose
        {
            get; set;
        }

        internal TransferQueueForm(TransferQueue queue, Func<Task> run, AppTheme theme)
        {
            _theme = theme;
            this.queue = queue;
            this.run = run;
            Text = QueueText.Get("title");
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1460, 660);
            MinimumSize = new Size(1000, 460);
            Font = new Font("Segoe UI", 9);
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(10), ColumnCount = 1, RowCount = 5 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 82));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 85));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            var statusPanel = new Panel { Dock = DockStyle.Fill };
            statusPanel.Controls.Add(activity);
            statusPanel.Controls.Add(activityProgress);
            layout.Controls.Add(statusPanel, 0, 0);
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill };
            AddButton(buttons, "resume", async () => await run());
            AddButton(buttons, "pause", () => { queue.Pause(); return Task.CompletedTask; });
            AddButton(buttons, "cancelCurrent", () => { queue.CancelCurrent(); return Task.CompletedTask; });
            AddButton(buttons, "retry", async () => { queue.RetryFailed(); await run(); });
            AddButton(buttons, "clear", () => { queue.ClearFinished(); return Task.CompletedTask; });
            AddButton(buttons, "clearCancelled", () => { queue.ClearCancelled(); return Task.CompletedTask; });
            AddButton(buttons, "export", ExportAsync);
            layout.Controls.Add(buttons, 0, 1);
            foreach (string key in new[] { "file", "direction", "device", "destination", "state", "attempts", "progress", "bytes", "elapsed", "average" })
                grid.Columns.Add(key, QueueText.Get(key));
            grid.Columns["destination"].FillWeight = 200;
            grid.Columns["file"].FillWeight = 140;
            grid.Columns["attempts"].FillWeight = 55;
            grid.Columns["progress"].FillWeight = 55;
            grid.Columns["attempts"].MinimumWidth = 95;
            grid.Columns["progress"].MinimumWidth = 120;
            grid.Columns["file"].MinimumWidth = 140;
            grid.Columns["direction"].MinimumWidth = 145;
            grid.Columns["device"].MinimumWidth = 120;
            grid.Columns["destination"].MinimumWidth = 200;
            grid.Columns["state"].MinimumWidth = 130;
            foreach (string key in new[] { "bytes", "elapsed", "average" })
            {
                grid.Columns[key].FillWeight = 70;
                grid.Columns[key].MinimumWidth = key == "elapsed" ? 120 : 150;
            }
            grid.SelectionChanged += (_, _) => UpdateDetails();
            layout.Controls.Add(grid, 0, 2);
            layout.Controls.Add(details, 0, 3);
            layout.Controls.Add(summary, 0, 4);
            Controls.Add(layout);
            _theme.Apply(this);
            queue.Changed += RefreshQueue;
            queue.ProgressChanged += RefreshProgress;
            statisticsTimer.Tick += (_, _) => RefreshStatistics();
            VisibleChanged += (_, _) => { if (Visible) statisticsTimer.Start(); else statisticsTimer.Stop(); };
            FormClosing += (_, e) => { if (!AllowClose) { e.Cancel = true; Hide(); } };
            FormClosed += (_, _) => { queue.Changed -= RefreshQueue; queue.ProgressChanged -= RefreshProgress; };
            Disposed += (_, _) =>
            {
                statisticsTimer.Dispose();
                queue.Changed -= RefreshQueue;
                queue.ProgressChanged -= RefreshProgress;
            };
            RefreshQueue();
        }

        private void AddButton(FlowLayoutPanel panel, string key, Func<Task> action)
        {
            var button = new Button { Name = key, Text = QueueText.Get(key), AutoSize = true, Height = 30 };
            button.Click += async (_, _) =>
            {
                try
                {
                    await action();
                }
                catch (Exception ex) { MessageBox.Show(this, ex.Message, QueueText.Get("title"), MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };
            panel.Controls.Add(button);
        }

        private void RefreshQueue()
        {
            if (IsDisposed)
                return;
            var ids = queue.Jobs.Select(j => j.Id).ToHashSet();
            foreach (var id in rows.Keys.Where(id => !ids.Contains(id)).ToArray())
            {
                grid.Rows.Remove(rows[id]);
                rows.Remove(id);
            }
            foreach (var job in queue.Jobs)
            {
                if (!rows.TryGetValue(job.Id, out var row))
                {
                    row = grid.Rows[grid.Rows.Add()];
                    row.Tag = job;
                    rows.Add(job.Id, row);
                }
                row.SetValues(job.Name, job.FromAndroid ? "Android → PC" : "PC → Android", job.DeviceId,
                    job.ResolvedDestination ?? job.Destination, QueueText.Get(job.State.ToString()), job.Attempts,
                    job.Percent < 0 ? "—" : $"{job.Percent}%");
                row.DefaultCellStyle.ForeColor = job.State == TransferState.Failed
                    ? (_theme.IsDark ? AppTheme.Error : Color.Firebrick)
                    : (_theme.IsDark ? AppTheme.Text : SystemColors.ControlText);
            }
            summary.Text = (queue.IsRunning ? QueueText.Get("active") : QueueText.Get("stopped")) + " · " + QueueText.Summary(queue.Summary);
            UpdateDetails();
            RefreshStatistics();
        }

        private void RefreshProgress()
        {
            foreach (var job in queue.Jobs.Where(j => j.State == TransferState.Running))
                if (rows.TryGetValue(job.Id, out var row))
                    row.Cells["progress"].Value = job.Percent < 0 ? "—" : $"{job.Percent}%";
            RefreshStatistics();
        }
        private void RefreshStatistics()
        {
            if (IsDisposed)
                return;
            foreach (var job in queue.Jobs)
            {
                if (!rows.TryGetValue(job.Id, out var row))
                    continue;
                row.Cells["bytes"].Value = job.TransferredBytes.HasValue ? $"{job.TransferredBytes.Value / 1048576d:N2} MiB" : "—";
                row.Cells["elapsed"].Value = job.StartedAt.HasValue ? FormatElapsed(job.Elapsed) : "—";
                row.Cells["average"].Value = job.AverageMiBPerSecond.HasValue ? $"{job.AverageMiBPerSecond.Value:N2} MiB/s" : "—";
            }
            var current = queue.Jobs.FirstOrDefault(job => job.State is TransferState.Running or TransferState.Retrying);
            activity.Text = current == null ? QueueText.Get("hint") :
                current.Name + " · " + QueueText.Get("elapsed") + ": " + FormatElapsed(current.Elapsed) +
                (current.Percent < 0 ? " · " + QueueText.Get("measuring") : $" · {current.Percent}%");
            activityProgress.Visible = current != null;
            activityProgress.Style = current is { State: TransferState.Running, Percent: >= 0 } ? ProgressBarStyle.Continuous : ProgressBarStyle.Marquee;
            if (current is { State: TransferState.Running, Percent: >= 0 })
                activityProgress.Value = Math.Clamp(current.Percent, 0, 100);
        }
        private static string FormatElapsed(TimeSpan elapsed) => $"{(int)elapsed.TotalHours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}";
        private void UpdateDetails()
        {
            if (grid.CurrentRow?.Tag is TransferJob job)
                details.Text = job.Source + Environment.NewLine + "→ " + (job.ResolvedDestination ?? job.Destination) +
                    Environment.NewLine + QueueText.Get("statisticsHint") + Environment.NewLine + job.Error;
            else
                details.Clear();
        }
        private Task ExportAsync()
        {
            using var dialog = new SaveFileDialog { Filter = "JSON (*.json)|*.json", FileName = "transfer-summary.json" };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                System.IO.File.WriteAllText(dialog.FileName, System.Text.Json.JsonSerializer.Serialize(
                    new
                    {
                        Summary = queue.Summary,
                        Transfers = queue.Jobs
                    },
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
            }
            return Task.CompletedTask;
        }
    }
}
