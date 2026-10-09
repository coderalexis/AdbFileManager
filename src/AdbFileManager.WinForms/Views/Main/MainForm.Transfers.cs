using AdbFileManager.Core.Transfers;

namespace AdbFileManager
{
    internal partial class MainForm
    {
        private TransferQueue? transferQueue;
        private TransferQueueForm? queueWindow;
        private Task? queueRunTask;
        private bool closeAfterQueue;
        private bool queueStorageWarning;
        private bool queueStorageDisabled;
        private Button? queueButton;
        private Label? adbVersionLabel;

        private void InitializeTransfers()
        {
            transferQueue = new TransferQueue(_transferBackend, ResolveConflictAsync);
            try
            {
                transferQueue.Jobs.AddRange(_queueStore.Load());
                if (transferQueue.Jobs.Any(j => j.State is TransferState.Pending or TransferState.Cancelled or TransferState.Failed))
                    transferQueue.Pause();
            }
            catch (Exception ex)
            {
                queueStorageDisabled = true; // Preserve a damaged queue rather than overwriting it.
                Shown += (_, _) => MessageBox.Show(this, QueueText.Get("storageError") + ex.Message, QueueText.Get("title"));
            }
            transferQueue.Changed += SaveAndRefreshQueue;
            queueButton = new Button { Left = 6, Top = 0, Width = 150, Height = 25, Text = QueueText.Get("title") };
            queueButton.Click += (_, _) => ShowTransferQueue();
            adbVersionLabel = new Label { Left = 165, Top = 5, Width = 290, Height = 20, AutoEllipsis = true, Text = "ADB …" };
            footerPanel.Controls.Add(queueButton);
            footerPanel.Controls.Add(adbVersionLabel);
            queueButton.BringToFront();
            adbVersionLabel.BringToFront();
            MinimumSize = new Size(930, 550);
            Shown += async (_, _) =>
            {
                try
                {
                    adbVersionLabel.Text = "ADB " + await _adb.VersionAsync();
                }
                catch (Exception ex) { adbVersionLabel.Text = QueueText.Get("adbError"); Console.Error.WriteLine(ex); }
                if (transferQueue.Jobs.Any(j => j.State is TransferState.Pending or TransferState.Cancelled or TransferState.Failed))
                    ShowTransferQueue(); // Restored work waits for an explicit Start/Retry.
            };
            SaveAndRefreshQueue();
        }

        private void SaveAndRefreshQueue()
        {
            if (transferQueue == null)
                return;
            if (queueButton != null)
                queueButton.Text = QueueText.Get("title") + $" ({transferQueue.Summary.Pending})";
            if (queueStorageDisabled)
                return;
            try
            {
                _queueStore.Save(transferQueue.Jobs);
            }
            catch (Exception ex)
            {
                if (!queueStorageWarning)
                {
                    queueStorageWarning = true;
                    MessageBox.Show(this, QueueText.Get("storageError") + ex.Message, QueueText.Get("title"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void ShowTransferQueue()
        {
            if (transferQueue == null)
                return;
            if (queueWindow == null || queueWindow.IsDisposed)
                queueWindow = new TransferQueueForm(transferQueue, RunQueueAsync, _theme);
            queueWindow.Show(this);
            queueWindow.BringToFront();
        }

        private Task<ConflictDecision> ResolveConflictAsync(TransferJob job, EntryKind kind, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            ShowTransferQueue();
            using var dialog = new ConflictDialog(job, kind, _theme);
            // Closing the application while the dialog is open cancels its current operation too.
            dialog.Shown += (_, _) => { if (token.IsCancellationRequested) dialog.Close(); };
            using var registration = token.Register(() =>
            {
                if (dialog.IsHandleCreated && !dialog.IsDisposed)
                {
                    try
                    {
                        dialog.BeginInvoke((Action)dialog.Close);
                    }
                    catch (InvalidOperationException) { }
                }
            });
            dialog.ShowDialog(queueWindow);
            token.ThrowIfCancellationRequested();
            return Task.FromResult(dialog.Decision);
        }

        private Task RunQueueAsync()
        {
            if (queueRunTask is { IsCompleted: false })
                return queueRunTask;
            queueRunTask = RunQueueCoreAsync();
            return queueRunTask;
        }

        private async Task RunQueueCoreAsync()
        {
            try
            {
                await transferQueue!.RunAsync();
            }
            catch (Exception ex)
            {
                if (!IsDisposed && !closeAfterQueue)
                    MessageBox.Show(this, ex.Message, QueueText.Get("title"));
            }
            finally
            {
                if (closeAfterQueue && !IsDisposed)
                    BeginInvoke((Action)Close);
            }
        }

        private Task QueueTransfersAsync(List<(string Source, bool IsDirectory)> sources,
            string destinationDirectory, bool fromAndroid)
        {
            if (sources.Count == 0)
            {
                MessageBox.Show(LocalizationText.Get("no_files_selected"), LocalizationText.Get("no_files_selected_title"));
                return Task.CompletedTask;
            }
            string? selectedSerial = ReadyDeviceSerial;
            bool preserve = _settings.Current.KeepFileModificationDate;
            try
            {
                if (selectedSerial == null)
                    throw new IOException("No ready device selected.");
                var jobs = TransferBatchFactory.Create(sources.Select(source => new TransferSource(source.Source, source.IsDirectory)),
                    destinationDirectory, selectedSerial, fromAndroid, preserve);
                transferQueue!.Enqueue(jobs);
                ShowTransferQueue();
                if (!transferQueue.IsPaused)
                    _ = RunQueueAsync();
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, LocalizationText.Get("error"), MessageBoxButtons.OK, MessageBoxIcon.Error); }
            return Task.CompletedTask;
        }
    }
}
