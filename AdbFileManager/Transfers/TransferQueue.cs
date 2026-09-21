namespace AdbFileManager.Transfers {
    public sealed class TransferQueue {
        private readonly ITransferBackend backend;
        private readonly Func<TransferJob, EntryKind, CancellationToken, Task<ConflictDecision>> resolveConflict;
        private readonly Dictionary<Guid, ConflictAction> batchActions = new();
        private CancellationTokenSource? activeCancellation;
        private bool pauseRequested;
        public List<TransferJob> Jobs { get; } = new();
        public bool IsRunning { get; private set; }
        public bool IsPaused => pauseRequested;
        public int AutomaticRetries { get; set; } = 2;
        public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(2);
        public event Action? Changed;
        public event Action? ProgressChanged;
        public event Action? Drained;

        public TransferQueue(ITransferBackend backend,
            Func<TransferJob, EntryKind, CancellationToken, Task<ConflictDecision>> resolveConflict) {
            this.backend = backend;
            this.resolveConflict = resolveConflict;
        }

        public QueueSummary Summary => new(
            Jobs.Count(j => j.State == TransferState.Completed), Jobs.Count(j => j.State == TransferState.Failed),
            Jobs.Count(j => j.State == TransferState.Skipped), Jobs.Count(j => j.State == TransferState.Cancelled),
            Jobs.Count(j => j.State is TransferState.Pending or TransferState.Running or TransferState.Retrying));

        public void Enqueue(IEnumerable<TransferJob> jobs) {
            foreach (var job in jobs) {
                if (string.IsNullOrWhiteSpace(job.DeviceId)) throw new ArgumentException("A device serial is required.");
                Jobs.Add(job);
            }
            Changed?.Invoke();
        }

        public void Pause() { pauseRequested = true; activeCancellation?.Cancel(); }
        public void CancelCurrent() => activeCancellation?.Cancel();

        public void RetryFailed() {
            foreach (var job in Jobs.Where(j => j.State is TransferState.Failed or TransferState.Cancelled)) {
                job.State = TransferState.Pending;
                job.Error = "";
                job.FinishedAt = null;
                job.Percent = -1;
            }
            Changed?.Invoke();
        }

        public void ClearFinished() {
            Jobs.RemoveAll(j => j.State is TransferState.Completed or TransferState.Skipped);
            Changed?.Invoke();
        }

        public async Task RunAsync() {
            if (IsRunning) return;
            IsRunning = true;
            pauseRequested = false;
            Changed?.Invoke();
            try {
                while (!pauseRequested) {
                    var job = Jobs.FirstOrDefault(j => j.State == TransferState.Pending);
                    if (job == null) break;
                    using var cancellation = new CancellationTokenSource();
                    activeCancellation = cancellation;
                    job.State = TransferState.Running;
                    job.Error = "";
                    Changed?.Invoke();
                    try {
                        string destination = job.ResolvedDestination ?? job.Destination;
                        EntryKind kind = await InspectWithRetryAsync(job, destination, cancellation.Token);
                        ConflictAction action = ConflictAction.Ask;
                        if (kind != EntryKind.Missing) {
                            if (!batchActions.TryGetValue(job.BatchId, out action)) action = ConflictAction.Ask;
                            // Replacing a directory/type mismatch would destroy or merge unrelated contents.
                            bool canReplace = !job.IsDirectory && kind == EntryKind.File;
                            if (action == ConflictAction.Ask || (action == ConflictAction.Replace && !canReplace)) {
                                var decision = await resolveConflict(job, kind, cancellation.Token);
                                action = decision.Action;
                                if (decision.ApplyToBatch) batchActions[job.BatchId] = action;
                            }
                            cancellation.Token.ThrowIfCancellationRequested();
                            if (action == ConflictAction.Skip) {
                                job.State = TransferState.Skipped;
                                continue;
                            }
                            if (action == ConflictAction.KeepBoth) {
                                destination = await AvailableNameAsync(job, cancellation.Token);
                            }
                            else if (action != ConflictAction.Replace || !canReplace) {
                                throw new IOException("Choose Skip or Keep both for a directory/type conflict.");
                            }
                        }
                        job.ResolvedDestination = destination;
                        bool replace = kind == EntryKind.File && action == ConflictAction.Replace;
                        var progress = new Progress<int>(percent => {
                            if (job.State != TransferState.Running || cancellation.IsCancellationRequested) return;
                            job.Percent = percent;
                            ProgressChanged?.Invoke();
                        });
                        for (int retry = 0; ; retry++) {
                            cancellation.Token.ThrowIfCancellationRequested();
                            job.State = TransferState.Running;
                            job.Attempts++;
                            job.Percent = -1;
                            Changed?.Invoke();
                            try {
                                await backend.CopyAsync(job, destination, replace, progress, cancellation.Token);
                                job.State = TransferState.Completed;
                                job.Percent = 100;
                                job.Error = "";
                                break;
                            }
                            catch (AdbCommandException ex) when (ex.IsTransient && retry < AutomaticRetries) {
                                job.State = TransferState.Retrying;
                                job.Error = ex.Message;
                                Changed?.Invoke();
                                await Task.Delay(RetryDelay, cancellation.Token);
                            }
                        }
                    }
                    catch (OperationCanceledException) when (cancellation.IsCancellationRequested) {
                        job.State = TransferState.Cancelled;
                        job.Error = "Cancelled";
                    }
                    catch (Exception ex) {
                        job.State = TransferState.Failed;
                        job.Error = ex.Message;
                    }
                    finally {
                        job.FinishedAt = DateTimeOffset.Now;
                        activeCancellation = null;
                        Changed?.Invoke();
                    }
                }
            }
            finally {
                IsRunning = false;
                activeCancellation = null;
                Changed?.Invoke();
                if (!pauseRequested) Drained?.Invoke();
            }
        }

        private async Task<EntryKind> InspectWithRetryAsync(TransferJob job, string destination,
            CancellationToken cancellationToken) {
            for (int retry = 0; ; retry++) {
                try {
                    return await backend.InspectAsync(job, destination, cancellationToken);
                }
                catch (AdbCommandException ex) when (ex.IsTransient && retry < AutomaticRetries) {
                    job.State = TransferState.Retrying;
                    job.Error = ex.Message;
                    Changed?.Invoke();
                    await Task.Delay(RetryDelay, cancellationToken);
                    job.State = TransferState.Running;
                    Changed?.Invoke();
                }
            }
        }

        private async Task<string> AvailableNameAsync(TransferJob job, CancellationToken cancellationToken) {
            string path = job.Destination;
            string extension = job.IsDirectory ? "" : Path.GetExtension(path);
            string stem = path[..(path.Length - extension.Length)];
            for (int suffix = 1; suffix <= 10000; suffix++) {
                string candidate = $"{stem} ({suffix}){extension}";
                if (await InspectWithRetryAsync(job, candidate, cancellationToken) == EntryKind.Missing)
                    return candidate;
            }
            throw new IOException("No available destination name was found.");
        }
    }
}
