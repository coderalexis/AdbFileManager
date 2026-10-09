namespace AdbFileManager.Infrastructure.Adb
{
    public sealed class AdbClient : IAdbClient
    {
        private readonly AdbProgressRunner runner = new();
        public int ProgressIntervalMs
        {
            get => runner.ProgressIntervalMs; set => runner.ProgressIntervalMs = value;
        }
        public string ExecutablePath
        {
            get;
        }
        public AdbClient(string executablePath)
        {
            ExecutablePath = Path.GetFullPath(executablePath);
        }

        public async Task<AdbResult> ExecuteAsync(IEnumerable<string> arguments, string? deviceId = null,
            CancellationToken cancellationToken = default)
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(45));
            try
            {
                return await runner.ExecuteAsync(ExecutablePath,
                    TargetArguments(arguments, deviceId), cancellationToken: timeout.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException("ADB did not respond within 45 seconds.");
            }
        }

        public async Task<string> QueryAsync(IEnumerable<string> arguments, string? deviceId = null,
            CancellationToken cancellationToken = default)
        {
            var result = await ExecuteAsync(arguments, deviceId, cancellationToken).ConfigureAwait(false);
            result.EnsureSuccess();
            return result.Output;
        }

        public Task CopyAsync(IEnumerable<string> arguments, IProgress<int>? progress,
            CancellationToken cancellationToken) =>
            runner.RunAsync(ExecutablePath, arguments, progress, cancellationToken);

        public async Task<TransferStatistics> CopyWithStatisticsAsync(IEnumerable<string> arguments,
            IProgress<int>? progress, CancellationToken cancellationToken)
        {
            var clock = System.Diagnostics.Stopwatch.StartNew();
            var result = await runner.ExecuteAsync(ExecutablePath, arguments, progress,
                cancellationToken, 8192).ConfigureAwait(false);
            clock.Stop();
            result.EnsureSuccess();
            return AdbTransferSummaryParser.Parse(result.CombinedOutput, clock.Elapsed);
        }

        public static string[] TargetArguments(IEnumerable<string> arguments, string? deviceId) =>
            (string.IsNullOrWhiteSpace(deviceId) ? arguments : new[] { "-s", deviceId }.Concat(arguments)).ToArray();

        public static string QuoteShell(string value) => "'" + value.Replace("'", "'\"'\"'") + "'";

        public async Task<string> VersionAsync()
        {
            string output = await QueryAsync(new[] { "version" }).ConfigureAwait(false);
            return output.Split('\n').FirstOrDefault(line => line.StartsWith("Version "))?.Trim()[8..]
                ?? output.Trim();
        }
    }
}
