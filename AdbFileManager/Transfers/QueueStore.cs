using System.Text.Json;

namespace AdbFileManager.Transfers {
    public sealed class QueueStore {
        private readonly string path;
        public QueueStore(string path) { this.path = Path.GetFullPath(path); }
        public List<TransferJob> Load() {
            if (!System.IO.File.Exists(path)) return new();
            var jobs = JsonSerializer.Deserialize<List<TransferJob>>(System.IO.File.ReadAllText(path))
                ?? throw new InvalidDataException("The saved transfer queue is empty or invalid.");
            foreach (var job in jobs) {
                if (job.Id == Guid.Empty || string.IsNullOrWhiteSpace(job.DeviceId) ||
                    string.IsNullOrWhiteSpace(job.Source) || string.IsNullOrWhiteSpace(job.Destination))
                    throw new InvalidDataException("The saved transfer queue contains an invalid entry.");
                if (job.State is TransferState.Running or TransferState.Retrying) {
                    job.State = TransferState.Cancelled;
                    job.Error = "Interrupted. Retry to copy this item again.";
                }
            }
            return jobs;
        }
        public void Save(IEnumerable<TransferJob> jobs) {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            string temporary = path + ".tmp";
            System.IO.File.WriteAllText(temporary, JsonSerializer.Serialize(jobs,
                new JsonSerializerOptions { WriteIndented = true }));
            System.IO.File.Move(temporary, path, true);
        }
    }
}
