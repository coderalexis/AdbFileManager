namespace AdbFileManager.Transfers {
    public sealed class AdbTransferBackend : ITransferBackend {
        private readonly AdbClient client;
        public AdbTransferBackend(AdbClient client) { this.client = client; }

        public async Task<EntryKind> InspectAsync(TransferJob job, string destination, CancellationToken token) {
            if (job.FromAndroid) {
                if (System.IO.File.Exists(destination) || Directory.Exists(destination)) {
                    var attributes = System.IO.File.GetAttributes(destination);
                    if (attributes.HasFlag(FileAttributes.ReparsePoint)) return EntryKind.Other;
                    return attributes.HasFlag(FileAttributes.Directory) ? EntryKind.Directory : EntryKind.File;
                }
                return EntryKind.Missing;
            }
            string path = AdbClient.QuoteShell(destination);
            string output = await client.QueryAsync(new[] { "shell",
                $"if [ -L {path} ]; then echo other; elif [ -d {path} ]; then echo directory; " +
                $"elif [ -f {path} ]; then echo file; elif [ -e {path} ]; then echo other; else echo missing; fi" },
                job.DeviceId, token);
            return output.Trim() switch {
                "missing" => EntryKind.Missing, "file" => EntryKind.File,
                "directory" => EntryKind.Directory, "other" => EntryKind.Other,
                _ => throw new IOException("Could not determine the destination file type.")
            };
        }

        public async Task CopyAsync(TransferJob job, string destination, bool replace,
            IProgress<int> progress, CancellationToken token) {
            if (job.FromAndroid) await PullAsync(job, destination, replace, progress, token);
            else await PushAsync(job, destination, replace, progress, token);
        }

        private async Task PullAsync(TransferJob job, string destination, bool replace,
            IProgress<int> progress, CancellationToken token) {
            string parent = Path.GetDirectoryName(Path.GetFullPath(destination))!;
            Directory.CreateDirectory(parent);
            string staging = Path.Combine(parent, ".afm-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(staging);
            try {
                var arguments = new List<string> { "-s", job.DeviceId, "pull" };
                if (job.PreserveTimestamp) arguments.Add("-a");
                arguments.Add(job.Source);
                arguments.Add(staging + Path.DirectorySeparatorChar);
                await client.CopyAsync(arguments, progress, token);
                token.ThrowIfCancellationRequested();
                string stagedFile = Path.Combine(staging, job.Name);
                EntryKind current = await InspectAsync(job, destination, token);
                VerifyDestination(job, current, replace);
                if (job.IsDirectory) Directory.Move(stagedFile, destination);
                else System.IO.File.Move(stagedFile, destination, replace);
            }
            finally {
                // This randomly named directory was created by this operation under the target parent.
                if (Directory.Exists(staging)) {
                    try { Directory.Delete(staging, true); }
                    catch (IOException ex) { Console.Error.WriteLine($"Temporary files remain in {staging}: {ex.Message}"); }
                    catch (UnauthorizedAccessException ex) { Console.Error.WriteLine(ex.Message); }
                }
            }
        }

        private async Task PushAsync(TransferJob job, string destination, bool replace,
            IProgress<int> progress, CancellationToken token) {
            int slash = destination.LastIndexOf('/');
            if (slash < 0) throw new IOException("An absolute Android destination is required.");
            string parent = destination[..(slash + 1)];
            string staging = parent + ".afm-" + Guid.NewGuid().ToString("N");
            string quotedStaging = AdbClient.QuoteShell(staging);
            bool created = false;
            try {
                await client.QueryAsync(new[] { "shell", $"mkdir -p {AdbClient.QuoteShell(parent)} && mkdir {quotedStaging}" }, job.DeviceId, token);
                created = true;
                await client.CopyAsync(new[] { "-s", job.DeviceId, "push", job.Source, staging + "/" }, progress, token);
                token.ThrowIfCancellationRequested();
                EntryKind current = await InspectAsync(job, destination, token);
                VerifyDestination(job, current, replace);
                string source = AdbClient.QuoteShell(staging + "/" + job.Name);
                string target = AdbClient.QuoteShell(destination);
                // -T prevents nesting into a directory that appears during the copy; -n prevents clobbering.
                string flags = replace ? "-fT" : "-nT";
                string command = $"mv {flags} {source} {target} && " +
                    $"if [ -e {source} ] || [ -L {source} ]; then echo 'Destination changed during transfer' >&2; exit 1; fi";
                await client.QueryAsync(new[] { "shell", command }, job.DeviceId, token);
            }
            finally {
                if (created) {
                    // Cleanup only our unique staging directory. Do not delete any destination data.
                    using var cleanup = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                    try { await client.QueryAsync(new[] { "shell", $"rm -rf {quotedStaging}" }, job.DeviceId, cleanup.Token); }
                    catch (Exception ex) { Console.Error.WriteLine($"Temporary files may remain in {staging}: {ex.Message}"); }
                }
            }
        }

        internal static void VerifyDestination(TransferJob job, EntryKind current, bool replace) {
            if (current != EntryKind.Missing && !(replace && !job.IsDirectory && current == EntryKind.File))
                throw new IOException("The destination changed during the transfer. Retry to review the conflict.");
        }
    }
}
