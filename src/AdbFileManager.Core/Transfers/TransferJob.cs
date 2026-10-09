using System.Text.Json.Serialization;

namespace AdbFileManager.Core.Transfers
{





    public sealed class TransferJob
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid BatchId
        {
            get; set;
        }
        public string DeviceId { get; set; } = "";
        public string Source { get; set; } = "";
        public string Destination { get; set; } = "";
        public string? ResolvedDestination
        {
            get; set;
        }
        public bool FromAndroid
        {
            get; set;
        }
        public bool IsDirectory
        {
            get; set;
        }
        public bool PreserveTimestamp
        {
            get; set;
        }
        public TransferState State { get; set; } = TransferState.Pending;
        public int Attempts
        {
            get; set;
        }
        public string Error { get; set; } = "";
        public DateTimeOffset? FinishedAt
        {
            get; set;
        }
        public DateTimeOffset? StartedAt
        {
            get; set;
        }
        public long? TransferredBytes
        {
            get; set;
        }
        public double? TransferSeconds
        {
            get; set;
        }
        public double ElapsedSeconds
        {
            get; set;
        }
        private System.Diagnostics.Stopwatch? runClock;
        [JsonIgnore] public TimeSpan Elapsed => TimeSpan.FromSeconds(Math.Max(0, runClock?.Elapsed.TotalSeconds ?? ElapsedSeconds));
        [JsonIgnore]
        public double? AverageMiBPerSecond => State == TransferState.Completed && TransferredBytes >= 0 && TransferSeconds > 0
            ? TransferredBytes.Value / (1024d * 1024d * TransferSeconds.Value) : null;

        internal void ResetStatistics()
        {
            StartedAt = null;
            FinishedAt = null;
            TransferredBytes = null;
            TransferSeconds = null;
            ElapsedSeconds = 0;
            runClock = null;
        }
        internal void BeginRun()
        {
            ResetStatistics();
            StartedAt = DateTimeOffset.Now;
            runClock = System.Diagnostics.Stopwatch.StartNew();
        }
        internal void FinishRun()
        {
            runClock?.Stop();
            ElapsedSeconds = runClock?.Elapsed.TotalSeconds ?? ElapsedSeconds;
            runClock = null;
        }
        [JsonIgnore] public int Percent { get; set; } = -1;
        [JsonIgnore] public string Name => Source.TrimEnd('/', '\\').Split('/', '\\')[^1];
    }




}
