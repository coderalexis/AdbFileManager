using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace AdbFileManager {
    public static class AdbProgressRunner {
        public static int ProgressIntervalMs { get; set; } = 60;
        private const int OutputLimit = 8192;

        public static async Task RunAsync(string adbPath, IEnumerable<string> arguments,
            IProgress<int>? progress = null, CancellationToken cancellationToken = default) {
            var result = await ExecuteAsync(adbPath, arguments, progress, cancellationToken, OutputLimit)
                .ConfigureAwait(false);
            result.EnsureSuccess();
        }

        public static async Task<AdbResult> ExecuteAsync(string adbPath, IEnumerable<string> arguments,
            IProgress<int>? progress = null, CancellationToken cancellationToken = default,
            int outputLimit = 16 * 1024 * 1024) {
            cancellationToken.ThrowIfCancellationRequested();
            var startInfo = new ProcessStartInfo(adbPath) {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };
            foreach (string argument in arguments) startInfo.ArgumentList.Add(argument);

            using var process = new Process { StartInfo = startInfo };
            process.Start();
            var output = new StringBuilder();
            var error = new StringBuilder();
            var progressLock = new object();
            var clock = Stopwatch.StartNew();
            int lastPercent = -1;
            long lastReport = -Math.Max(0, ProgressIntervalMs);
            void Report(int percent) {
                lock (progressLock) {
                    if (percent == lastPercent) return;
                    if (percent != 100 && clock.ElapsedMilliseconds - lastReport < ProgressIntervalMs)
                        return;
                    lastPercent = percent;
                    lastReport = clock.ElapsedMilliseconds;
                    progress?.Report(percent);
                }
            }

            // Drain both streams concurrently: either pipe can fill while ADB is running.
            Task stdout = ReadOutputAsync(process.StandardOutput, output, Report, cancellationToken, outputLimit);
            Task stderr = ReadOutputAsync(process.StandardError, error, Report, cancellationToken, outputLimit);
            try {
                await Task.WhenAll(process.WaitForExitAsync(cancellationToken), stdout, stderr)
                    .ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                return new AdbResult(process.ExitCode, output.ToString(), error.ToString());
            }
            catch {
                // Stop only this client, never the shared ADB server or another application's copy.
                if (!process.HasExited) {
                    try { process.Kill(); }
                    catch (InvalidOperationException) { }
                }
                await process.WaitForExitAsync().ConfigureAwait(false);
                try { await Task.WhenAll(stdout, stderr).ConfigureAwait(false); }
                catch (OperationCanceledException) { }
                throw;
            }
        }

        private static async Task ReadOutputAsync(StreamReader reader, StringBuilder output,
            Action<int> report, CancellationToken cancellationToken, int outputLimit) {
            var buffer = new char[1024];
            var record = new StringBuilder();
            int count;
            while ((count = await reader.ReadAsync(buffer.AsMemory(), cancellationToken)
                .ConfigureAwait(false)) > 0) {
                output.Append(buffer, 0, count);
                if (output.Length > outputLimit) output.Remove(0, output.Length - outputLimit);
                for (int i = 0; i < count; i++) {
                    char character = buffer[i];
                    if (character == '\r' || character == '\n') {
                        int percent = ParseProgress(record.ToString());
                        if (percent >= 0) report(percent);
                        record.Clear();
                    }
                    else if (record.Length < OutputLimit) record.Append(character);
                }
            }
            int finalPercent = ParseProgress(record.ToString());
            if (finalPercent >= 0) report(finalPercent);
        }

        internal static int ParseProgress(string line) {
            var match = Regex.Match(line, @"^\s*\[\s*(\d{1,3})%\]",
                RegexOptions.CultureInvariant);
            return match.Success && int.TryParse(match.Groups[1].Value,
                NumberStyles.None, CultureInfo.InvariantCulture, out int percent) && percent <= 100
                ? percent : -1;
        }
    }
}
