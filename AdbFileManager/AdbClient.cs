namespace AdbFileManager {
    public sealed record AdbResult(int ExitCode, string Output, string Error) {
        public string CombinedOutput => Output + Error;
        public void EnsureSuccess() {
            if (ExitCode != 0) throw new AdbCommandException(ExitCode,
                string.IsNullOrWhiteSpace(Error) ? Output : Error);
        }
    }

    public sealed class AdbCommandException : IOException {
        public int ExitCode { get; }
        public bool IsTransient { get; }
        public AdbCommandException(int code, string details)
            : base($"ADB exited with code {code}.\n{details.Trim()}") {
            ExitCode = code;
            IsTransient = new[] { "device offline", "device not found", "no devices", "disconnected",
                "connection reset", "connection closed", "protocol fault", "cannot connect", "device '" }
                .Any(value => details.Contains(value, StringComparison.OrdinalIgnoreCase));
        }
    }

    public sealed class AdbClient {
        public static AdbClient Default { get; } = new(Path.Combine(AppContext.BaseDirectory, "adb.exe"));
        public string ExecutablePath { get; }
        public AdbClient(string executablePath) { ExecutablePath = Path.GetFullPath(executablePath); }

        public async Task<AdbResult> ExecuteAsync(IEnumerable<string> arguments, string? deviceId = null,
            CancellationToken cancellationToken = default) {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(45));
            try {
                return await AdbProgressRunner.ExecuteAsync(ExecutablePath,
                    TargetArguments(arguments, deviceId), cancellationToken: timeout.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) {
                throw new TimeoutException("ADB did not respond within 45 seconds.");
            }
        }

        public async Task<string> QueryAsync(IEnumerable<string> arguments, string? deviceId = null,
            CancellationToken cancellationToken = default) {
            var result = await ExecuteAsync(arguments, deviceId, cancellationToken).ConfigureAwait(false);
            result.EnsureSuccess();
            return result.Output;
        }

        public Task CopyAsync(IEnumerable<string> arguments, IProgress<int>? progress,
            CancellationToken cancellationToken) =>
            AdbProgressRunner.RunAsync(ExecutablePath, arguments, progress, cancellationToken);

        public static string[] TargetArguments(IEnumerable<string> arguments, string? deviceId) =>
            (string.IsNullOrWhiteSpace(deviceId) ? arguments : new[] { "-s", deviceId }.Concat(arguments)).ToArray();

        public static string QuoteShell(string value) => "'" + value.Replace("'", "'\"'\"'") + "'";

        public async Task<string> VersionAsync() {
            string output = await QueryAsync(new[] { "version" }).ConfigureAwait(false);
            return output.Split('\n').FirstOrDefault(line => line.StartsWith("Version "))?.Trim()[8..]
                ?? output.Trim();
        }
    }
}
