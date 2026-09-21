using System.Text.Json.Serialization;

namespace AdbFileManager.Transfers {
    public enum TransferState { Pending, Running, Retrying, Completed, Failed, Skipped, Cancelled }
    public enum ConflictAction { Ask, Replace, Skip, KeepBoth }
    public enum EntryKind { Missing, File, Directory, Other }
    public sealed record ConflictDecision(ConflictAction Action, bool ApplyToBatch = false);

    public sealed class TransferJob {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid BatchId { get; set; }
        public string DeviceId { get; set; } = "";
        public string Source { get; set; } = "";
        public string Destination { get; set; } = "";
        public string? ResolvedDestination { get; set; }
        public bool FromAndroid { get; set; }
        public bool IsDirectory { get; set; }
        public bool PreserveTimestamp { get; set; }
        public TransferState State { get; set; } = TransferState.Pending;
        public int Attempts { get; set; }
        public string Error { get; set; } = "";
        public DateTimeOffset? FinishedAt { get; set; }
        [JsonIgnore] public int Percent { get; set; } = -1;
        [JsonIgnore] public string Name => Source.TrimEnd('/','\\').Split('/','\\')[^1];
    }

    public interface ITransferBackend {
        Task<EntryKind> InspectAsync(TransferJob job, string destination, CancellationToken cancellationToken);
        Task CopyAsync(TransferJob job, string destination, bool replace, IProgress<int> progress,
            CancellationToken cancellationToken);
    }

    public sealed record QueueSummary(int Completed, int Failed, int Skipped, int Cancelled, int Pending);
}
