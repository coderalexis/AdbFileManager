namespace AdbFileManager.Views.Transfers;

internal sealed class IntegratedTransfersControl : UserControl
{
    private readonly TransferQueue queue;
    private readonly ToolTip tooltips = new();
    private readonly System.Windows.Forms.Timer clock = new() { Interval = 500 };
    private readonly Label current = new() { Dock = DockStyle.Fill, AutoEllipsis = true, TextAlign = ContentAlignment.MiddleLeft };
    private readonly Label counts = new() { AutoSize = true, Padding = new Padding(10, 8, 0, 0) };
    private readonly Button toggle = new() { Name = "toggleTransfers", AutoSize = true };
    private readonly ProgressBar progress = new() { Dock = DockStyle.Fill, MarqueeAnimationSpeed = 30, AccessibleName = UserInterfaceText.Get("progress") };
    private readonly Button retry = new() { Name = "retryTransfer", AutoSize = true, Text = UserInterfaceText.Get("retry") };
    private readonly Button details = new() { AutoSize = true, Text = UserInterfaceText.Get("details") };
    private readonly Button resume, pause, cancel;
    private readonly TableLayoutPanel body = new() { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
    private bool expanded;
    internal event Action? ExpansionChanged;

    internal IntegratedTransfersControl(TransferQueue queue, Func<Task> run, Action openDetails, AppTheme theme)
    {
        this.queue = queue;
        Name = "integratedTransfers";
        Dock = DockStyle.Top;
        AutoScaleMode = AutoScaleMode.Dpi;
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(0, 4, 0, 0) };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var header = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = true, AutoSize = true };
        header.Controls.Add(toggle);
        header.Controls.Add(counts);
        header.Controls.Add(details);
        layout.Controls.Add(header, 0, 0);
        body.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        body.RowStyles.Add(new RowStyle(SizeType.Absolute, 10));
        body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        current.AccessibleName = UserInterfaceText.Get("activeTransfer");
        current.AutoSize = true;
        current.Padding = new Padding(6);
        body.Controls.Add(current, 0, 0);
        body.Controls.Add(progress, 0, 1);
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = true };
        resume = AddAction(actions, QueueText.Get("resume"), async () => await run());
        pause = AddAction(actions, QueueText.Get("pause"), () => { queue.Pause(); RefreshState(); return Task.CompletedTask; });
        cancel = AddAction(actions, QueueText.Get("cancelCurrent"), () => { queue.CancelCurrent(); return Task.CompletedTask; });
        retry.Click += async (_, _) => { try { queue.RetryFailed(); await run(); } catch (Exception ex) { MessageBox.Show(this, ex.Message, Text); } };
        actions.Controls.Add(retry);
        body.Controls.Add(actions, 0, 2);
        layout.Controls.Add(body, 0, 1);
        Controls.Add(layout);
        toggle.Click += (_, _) => SetExpanded(!expanded);
        details.Click += (_, _) => openDetails();
        tooltips.SetToolTip(details, UserInterfaceText.Get("details"));
        theme.ApplyToView(this);
        queue.Changed += RefreshState;
        queue.ProgressChanged += RefreshState;
        clock.Tick += (_, _) => RefreshState();
        clock.Start();
        SetExpanded(false);
        RefreshState();
    }

    private Button AddAction(FlowLayoutPanel panel, string text, Func<Task> action)
    {
        var button = new Button { Text = text, AccessibleName = text, AutoSize = true };
        button.Click += async (_, _) => { try { await action(); } catch (Exception ex) { MessageBox.Show(this, ex.Message, UserInterfaceText.Get("transfers")); } };
        panel.Controls.Add(button);
        return button;
    }

    internal void SetExpanded(bool value)
    {
        expanded = value;
        body.Visible = value;
        Height = (int)Math.Round((value ? 124 : 42) * DeviceDpi / 96d);
        toggle.Text = UserInterfaceText.Get(value ? "hideTransfers" : "showTransfers");
        toggle.AccessibleName = toggle.Text;
        ExpansionChanged?.Invoke();
    }

    private void RefreshState()
    {
        if (IsDisposed)
            return;
        counts.Text = QueueText.Summary(queue.Summary);
        retry.Enabled = !queue.IsRunning && queue.Jobs.Any(job => job.State is TransferState.Failed or TransferState.Cancelled);
        resume.Enabled = !queue.IsRunning && queue.Jobs.Any(job => job.State == TransferState.Pending);
        pause.Enabled = queue.IsRunning && !queue.IsPaused;
        var active = queue.Jobs.FirstOrDefault(job => job.State is TransferState.Running or TransferState.Retrying);
        var failed = queue.Jobs.LastOrDefault(job => job.State == TransferState.Failed);
        cancel.Enabled = active != null;
        current.Text = active == null ? failed != null ? UserInterfaceText.Get("failed") + " · " + failed.Name : UserInterfaceText.Get(queue.IsPaused ? "paused" : "noActive")
            : active.Name + " · " + active.Elapsed.ToString(@"hh\:mm\:ss") + (active.Percent < 0 ? " · " + QueueText.Get("measuring") : $" · {active.Percent}%");
        progress.Visible = active != null;
        progress.Style = active?.Percent >= 0 ? ProgressBarStyle.Continuous : ProgressBarStyle.Marquee;
        if (active?.Percent >= 0)
            progress.Value = Math.Clamp(active.Percent, 0, 100);
        AccessibleName = UserInterfaceText.Get("transfers") + " · " + counts.Text;
    }

    protected override void OnDpiChangedAfterParent(EventArgs e)
    {
        base.OnDpiChangedAfterParent(e);
        SetExpanded(expanded);
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            queue.Changed -= RefreshState;
            queue.ProgressChanged -= RefreshState;
            clock.Dispose();
            tooltips.Dispose();
        }
        base.Dispose(disposing);
    }
}
