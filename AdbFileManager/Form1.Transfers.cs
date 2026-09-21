using AdbFileManager.Transfers;

namespace AdbFileManager {
    public partial class Form1 {
        private TransferQueue? transferQueue;
        private QueueStore? queueStore;
        private TransferQueueForm? queueWindow;
        private Task? queueRunTask;
        private bool closeAfterQueue;
        private bool queueStorageWarning;
        private bool queueStorageDisabled;
        private Button? queueButton;
        private Label? adbVersionLabel;

        private void InitializeTransfers() {
            queueStore = new QueueStore(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "tobiksoft", "AdbFileManager", "transfers.json"));
            transferQueue = new TransferQueue(new AdbTransferBackend(AdbClient.Default), ResolveConflictAsync);
            try {
                transferQueue.Jobs.AddRange(queueStore.Load());
                if (transferQueue.Jobs.Any(j => j.State is TransferState.Pending or TransferState.Cancelled or TransferState.Failed))
                    transferQueue.Pause();
            }
            catch (Exception ex) {
                queueStorageDisabled = true; // Preserve a damaged queue rather than overwriting it.
                Shown += (_, _) => MessageBox.Show(this, QueueText.Get("storageError") + ex.Message, QueueText.Get("title"));
            }
            transferQueue.Changed += SaveAndRefreshQueue;
            queueButton = new Button { Left = 6, Top = 0, Width = 150, Height = 25, Text = QueueText.Get("title") };
            queueButton.Click += (_, _) => ShowTransferQueue();
            adbVersionLabel = new Label { Left = 165, Top = 5, Width = 290, Height = 20, AutoEllipsis = true, Text = "ADB …" };
            panel_dolniTlacitka.Controls.Add(queueButton);
            panel_dolniTlacitka.Controls.Add(adbVersionLabel);
            queueButton.BringToFront(); adbVersionLabel.BringToFront();
            MinimumSize = new Size(930, 550);
            Shown += async (_, _) => {
                try { adbVersionLabel.Text = "ADB " + await AdbClient.Default.VersionAsync(); }
                catch (Exception ex) { adbVersionLabel.Text = QueueText.Get("adbError"); Console.Error.WriteLine(ex); }
                if (transferQueue.Jobs.Any(j => j.State is TransferState.Pending or TransferState.Cancelled or TransferState.Failed))
                    ShowTransferQueue(); // Restored work waits for an explicit Start/Retry.
            };
            SaveAndRefreshQueue();
        }

        private void SaveAndRefreshQueue() {
            if (transferQueue == null) return;
            if (queueButton != null) queueButton.Text = QueueText.Get("title") + $" ({transferQueue.Summary.Pending})";
            if (queueStorageDisabled) return;
            try { queueStore!.Save(transferQueue.Jobs); }
            catch (Exception ex) {
                if (!queueStorageWarning) {
                    queueStorageWarning = true;
                    MessageBox.Show(this, QueueText.Get("storageError") + ex.Message, QueueText.Get("title"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void ShowTransferQueue() {
            if (transferQueue == null) return;
            if (queueWindow == null || queueWindow.IsDisposed)
                queueWindow = new TransferQueueForm(transferQueue, RunQueueAsync);
            queueWindow.Show(this);
            queueWindow.BringToFront();
        }

        private Task<ConflictDecision> ResolveConflictAsync(TransferJob job, EntryKind kind, CancellationToken token) {
            token.ThrowIfCancellationRequested();
            ShowTransferQueue();
            using var dialog = new ConflictDialog(job, kind);
            // Closing the application while the dialog is open cancels its current operation too.
            dialog.Shown += (_, _) => { if (token.IsCancellationRequested) dialog.Close(); };
            using var registration = token.Register(() => {
                if (dialog.IsHandleCreated && !dialog.IsDisposed) {
                    try { dialog.BeginInvoke((Action)dialog.Close); }
                    catch (InvalidOperationException) { }
                }
            });
            dialog.ShowDialog(queueWindow);
            token.ThrowIfCancellationRequested();
            return Task.FromResult(dialog.Decision);
        }

        private Task RunQueueAsync() {
            if (queueRunTask is { IsCompleted: false }) return queueRunTask;
            queueRunTask = RunQueueCoreAsync();
            return queueRunTask;
        }

        private async Task RunQueueCoreAsync() {
            try { await transferQueue!.RunAsync(); }
            catch (Exception ex) {
                if (!IsDisposed && !closeAfterQueue) MessageBox.Show(this, ex.Message, QueueText.Get("title"));
            }
            finally {
                if (closeAfterQueue && !IsDisposed) BeginInvoke((Action)Close);
            }
        }

        private async Task QueueTransfersAsync(List<(string Source, bool IsDirectory)> sources,
            string destinationDirectory, bool fromAndroid) {
            if (sources.Count == 0) {
                MessageBox.Show(rm.GetString("no_files_selected"), rm.GetString("no_files_selected_title"));
                return;
            }
            string? selectedSerial = selectedDevice?.adbId;
            bool preserve = SettingsManager.settings.keepFileModificationDate;
            try {
                // Resolve the default selection once, so queued work never switches to another phone.
                string serial = selectedSerial ?? (await AdbClient.Default.QueryAsync(new[] { "get-serialno" })).Trim();
                if (string.IsNullOrWhiteSpace(serial) || serial == "unknown") throw new IOException("No device selected.");
                Guid batch = Guid.NewGuid();
                var jobs = new List<TransferJob>();
                foreach (var source in sources) {
                    string name = fromAndroid ? source.Source.TrimEnd('/').Split('/')[^1] : Path.GetFileName(source.Source.TrimEnd('\\'));
                    if (fromAndroid && (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || name is "." or ".."))
                        throw new IOException($"The name cannot be copied to Windows: {name}");
                    jobs.Add(new TransferJob { BatchId = batch, DeviceId = serial, Source = source.Source,
                        Destination = fromAndroid ? Path.Combine(destinationDirectory, name) : TransferCommand.RemotePath(destinationDirectory, name),
                        FromAndroid = fromAndroid, IsDirectory = source.IsDirectory, PreserveTimestamp = preserve });
                }
                transferQueue!.Enqueue(jobs);
                ShowTransferQueue();
                if (!transferQueue.IsPaused) _ = RunQueueAsync();
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, rm.GetString("error"), MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
    }
}
