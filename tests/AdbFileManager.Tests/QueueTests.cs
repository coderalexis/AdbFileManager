using AdbFileManager.Transfers;
using Xunit;

namespace AdbFileManager.Tests;

public class QueueTests {
    private sealed class Backend : ITransferBackend {
        public Dictionary<string, EntryKind> Destinations { get; } = new();
        public List<(Guid Id, string Device, string Path, bool Replace)> Copies { get; } = new();
        public Func<TransferJob, string, CancellationToken, Task<EntryKind>>? Inspect { get; set; }
        public Func<TransferJob, CancellationToken, Task>? Copy { get; set; }
        public Task<EntryKind> InspectAsync(TransferJob job, string destination, CancellationToken token) =>
            Inspect?.Invoke(job, destination, token) ??
            Task.FromResult(Destinations.GetValueOrDefault(destination, EntryKind.Missing));
        public async Task CopyAsync(TransferJob job, string destination, bool replace, IProgress<int> progress, CancellationToken token) {
            Copies.Add((job.Id, job.DeviceId, destination, replace));
            if (Copy != null) await Copy(job, token);
            Destinations[destination] = job.IsDirectory ? EntryKind.Directory : EntryKind.File;
        }
    }
    private static TransferJob Job(string name = "photo.jpg", string device = "phone-A") => new() {
        BatchId = Guid.NewGuid(), DeviceId = device, Source = "/sdcard/" + name,
        Destination = Path.Combine(Path.GetTempPath(), "backup", name), FromAndroid = true
    };
    private static TransferQueue Queue(Backend backend, ConflictAction action = ConflictAction.Skip) =>
        new(backend, (_, _, _) => Task.FromResult(new ConflictDecision(action))) { RetryDelay = TimeSpan.Zero };

    [Fact]
    public async Task NewJobsAddedWhileRunningAreProcessedWithTheirOwnDevice() {
        var backend = new Backend();
        var queue = Queue(backend);
        var first = Job(); var second = Job("other.jpg", "phone-B");
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        backend.Copy = async (job, _) => { if (job == first) { entered.SetResult(); await release.Task; } };
        queue.Enqueue(new[] { first });
        Task running = queue.RunAsync();
        await entered.Task;
        queue.Enqueue(new[] { second });
        await queue.RunAsync(); // A second start must not create another consumer.
        release.SetResult();
        await running;
        Assert.Equal(new[] { "phone-A", "phone-B" }, backend.Copies.Select(c => c.Device));
        Assert.Equal(2, queue.Summary.Completed);
    }

    [Theory]
    [InlineData(ConflictAction.Skip, TransferState.Skipped, 0)]
    [InlineData(ConflictAction.Replace, TransferState.Completed, 1)]
    [InlineData(ConflictAction.KeepBoth, TransferState.Completed, 1)]
    public async Task ConflictsRespectTheChosenPolicy(ConflictAction action, TransferState state, int copies) {
        var backend = new Backend(); var job = Job();
        backend.Destinations[job.Destination] = EntryKind.File;
        var queue = Queue(backend, action); queue.Enqueue(new[] { job });
        await queue.RunAsync();
        Assert.Equal(state, job.State);
        Assert.Equal(copies, backend.Copies.Count);
        if (action == ConflictAction.Replace) Assert.True(backend.Copies[0].Replace);
        if (action == ConflictAction.KeepBoth) {
            Assert.EndsWith("photo (1).jpg", backend.Copies[0].Path);
            Assert.False(backend.Copies[0].Replace);
        }
    }

    [Fact]
    public async Task KeepBothChecksFurtherCollisions() {
        var backend = new Backend(); var job = Job();
        backend.Destinations[job.Destination] = EntryKind.File;
        backend.Destinations[Path.Combine(Path.GetDirectoryName(job.Destination)!, "photo (1).jpg")] = EntryKind.File;
        var queue = Queue(backend, ConflictAction.KeepBoth); queue.Enqueue(new[] { job });
        await queue.RunAsync();
        Assert.EndsWith("photo (2).jpg", backend.Copies.Single().Path);
    }

    [Fact]
    public async Task ApplyToBatchDoesNotSilentlyReplaceDirectories() {
        var backend = new Backend(); var first = Job(); var second = Job("folder");
        second.BatchId = first.BatchId; second.IsDirectory = true;
        backend.Destinations[first.Destination] = EntryKind.File;
        backend.Destinations[second.Destination] = EntryKind.Directory;
        int prompts = 0;
        var queue = new TransferQueue(backend, (_, _, _) => Task.FromResult(
            ++prompts == 1 ? new ConflictDecision(ConflictAction.Replace, true) : new ConflictDecision(ConflictAction.KeepBoth)));
        queue.Enqueue(new[] { first, second }); await queue.RunAsync();
        Assert.Equal(2, prompts);
        Assert.False(backend.Copies[1].Replace);
        Assert.EndsWith("folder (1)", backend.Copies[1].Path);
    }

    [Fact]
    public async Task TransientFailureRetriesAndThenCompletes() {
        var backend = new Backend(); int attempts = 0;
        backend.Copy = (_, _) => { if (++attempts < 3) throw new AdbCommandException(1, "device offline"); return Task.CompletedTask; };
        var job = Job(); var queue = Queue(backend); queue.Enqueue(new[] { job });
        await queue.RunAsync();
        Assert.Equal(3, job.Attempts); Assert.Equal(TransferState.Completed, job.State);
        Assert.Empty(job.Error);
    }

    [Fact]
    public async Task TransientInspectionFailureRetriesBeforeCopying() {
        var backend = new Backend(); int inspections = 0;
        backend.Inspect = (_, _, _) => ++inspections < 3
            ? Task.FromException<EntryKind>(new AdbCommandException(1, "device offline"))
            : Task.FromResult(EntryKind.Missing);
        var job = Job(); var queue = Queue(backend); queue.Enqueue(new[] { job });
        await queue.RunAsync();
        Assert.Equal(3, inspections);
        Assert.Equal(TransferState.Completed, job.State);
        Assert.Single(backend.Copies);
    }

    [Fact]
    public async Task RetryLimitStopsAndContinuesWithOtherJobs() {
        var backend = new Backend(); var first = Job(); var second = Job("good.jpg");
        backend.Copy = (job, _) => job == first ? Task.FromException(new AdbCommandException(1, "device offline")) : Task.CompletedTask;
        var queue = Queue(backend); queue.Enqueue(new[] { first, second }); await queue.RunAsync();
        Assert.Equal(3, first.Attempts); Assert.Equal(TransferState.Failed, first.State);
        Assert.Equal(TransferState.Completed, second.State);
    }

    [Fact]
    public async Task PermanentFailureIsNotAutomaticallyRetriedAndManualRetrySkipsSuccesses() {
        var backend = new Backend(); var first = Job(); var second = Job("good.jpg");
        backend.Copy = (job, _) => job == first ? Task.FromException(new AdbCommandException(1, "Permission denied")) : Task.CompletedTask;
        var queue = Queue(backend); queue.Enqueue(new[] { first, second }); await queue.RunAsync();
        Assert.Equal(1, first.Attempts);
        backend.Copy = null; queue.RetryFailed(); await queue.RunAsync();
        Assert.Equal(2, first.Attempts); Assert.Equal(1, second.Attempts);
        Assert.Equal(2, queue.Summary.Completed);
    }

    [Fact]
    public async Task PauseCancelsCurrentAndLeavesPendingUntilResume() {
        var backend = new Backend(); var first = Job(); var second = Job("next.jpg");
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        backend.Copy = async (_, token) => { started.TrySetResult(); await Task.Delay(Timeout.Infinite, token); };
        var queue = Queue(backend); queue.Enqueue(new[] { first, second });
        Task running = queue.RunAsync(); await started.Task; queue.Pause(); await running;
        Assert.Equal(TransferState.Cancelled, first.State); Assert.Equal(TransferState.Pending, second.State);
        Assert.True(queue.IsPaused);
        backend.Copy = null; await queue.RunAsync();
        Assert.Equal(TransferState.Completed, second.State); Assert.Equal(1, first.Attempts);
    }

    [Fact]
    public async Task CancelCurrentDoesNotCancelTheRestOfTheQueue() {
        var backend = new Backend(); var first = Job(); var second = Job("next.jpg");
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        backend.Copy = async (job, token) => { if (job == first) { started.TrySetResult(); await Task.Delay(Timeout.Infinite, token); } };
        var queue = Queue(backend); queue.Enqueue(new[] { first, second });
        Task running = queue.RunAsync(); await started.Task; queue.CancelCurrent(); await running;
        Assert.Equal(TransferState.Cancelled, first.State); Assert.Equal(TransferState.Completed, second.State);
    }

    [Fact]
    public void PersistenceKeepsDestinationsAndConvertsInterruptedWorkToCancelled() {
        string path = Path.GetTempFileName();
        try {
            var active = Job(); active.State = TransferState.Running; active.ResolvedDestination = active.Destination + ".copy";
            var done = Job("done.jpg"); done.State = TransferState.Completed;
            var store = new QueueStore(path); store.Save(new[] { active, done });
            var restored = store.Load();
            Assert.Equal(TransferState.Cancelled, restored[0].State);
            Assert.Equal(active.ResolvedDestination, restored[0].ResolvedDestination);
            Assert.Equal(active.DeviceId, restored[0].DeviceId);
            Assert.Equal(TransferState.Completed, restored[1].State);
        }
        finally { System.IO.File.Delete(path); }
    }

    [Fact]
    public void CorruptQueueIsReportedRatherThanSilentlyDiscarded() {
        string path = Path.GetTempFileName();
        try { System.IO.File.WriteAllText(path, "not json"); Assert.Throws<System.Text.Json.JsonException>(() => new QueueStore(path).Load()); }
        finally { System.IO.File.Delete(path); }
    }
}
