using System.Globalization;
using System.Text.RegularExpressions;

namespace AdbFileManager {
    internal sealed record AndroidFile(string Name, string Permissions, long? Bytes, DateTime? Modified) {
        internal bool IsDirectory => DirectoryEntry.IsDirectory(Permissions);
    }

    internal sealed record AndroidDevice(string Serial, string State, string Model) {
        internal static IReadOnlyList<AndroidDevice> Parse(string output) {
            var devices = new List<AndroidDevice>();
            foreach (string line in output.Split('\n')) {
                string[] fields = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
                if (fields.Length < 2 || fields[0] == "List" || fields[0] == "*") continue;
                if (fields[1] is not ("device" or "offline" or "unauthorized" or "recovery" or "sideload")) continue;
                string model = fields.FirstOrDefault(field => field.StartsWith("model:"))?[6..] ?? fields[0];
                devices.Add(new(fields[0], fields[1], model));
            }
            return devices;
        }
    }

    internal static class AndroidListingParser {
        private static readonly Regex Entry = new(
            @"^(?<permissions>[bcdlps-][rwxstST-]{9}[.+@]?)\s+\d+\s+\S+\s+\S+\s+(?<size>\d+)\s+(?<date>\d{4}-\d{2}-\d{2})\s+(?<time>\d{2}:\d{2}(?::\d{2})?) (?<name>.+)$",
            RegexOptions.CultureInvariant);

        internal static IReadOnlyList<AndroidFile> Parse(string output) {
            var entries = new List<AndroidFile>();
            foreach (string raw in output.Split('\n')) {
                string line = raw.TrimEnd('\r');
                if (string.IsNullOrWhiteSpace(line) || Regex.IsMatch(line, @"^total\s+\d+\s*$")) continue;
                Match match = Entry.Match(line);
                if (!match.Success || !long.TryParse(match.Groups["size"].Value, NumberStyles.None,
                    CultureInfo.InvariantCulture, out long bytes) ||
                    !DateTime.TryParseExact(match.Groups["date"].Value + " " + match.Groups["time"].Value,
                        new[] { "yyyy-MM-dd HH:mm", "yyyy-MM-dd HH:mm:ss" }, CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out DateTime modified))
                    throw new InvalidDataException("The Android file listing has an unsupported format.");
                string name = match.Groups["name"].Value;
                string permissions = match.Groups["permissions"].Value;
                if (permissions[0] == 'l') {
                    int target = name.IndexOf(" -> ", StringComparison.Ordinal);
                    if (target >= 0) name = name[..target];
                }
                if (name is not ("." or "..")) entries.Add(new(name, permissions, bytes, modified));
            }
            return entries;
        }
    }

    internal sealed class AndroidBrowser {
        private readonly Func<string[], string?, CancellationToken, Task<string>> query;
        internal AndroidBrowser(AdbClient client) : this((args, serial, token) => client.QueryAsync(args, serial, token)) { }
        internal AndroidBrowser(Func<string[], string?, CancellationToken, Task<string>> query) { this.query = query; }

        internal async Task<IReadOnlyList<AndroidDevice>> DevicesAsync(CancellationToken token) =>
            AndroidDevice.Parse(await query(new[] { "devices", "-l" }, null, token).ConfigureAwait(false));

        internal async Task<IReadOnlyList<AndroidFile>> ListAsync(string path, string serial,
            bool compatibility, CancellationToken token) {
            string output = await query(new[] { "shell", (compatibility ? "ls -1 " : "ls -lL ") +
                AdbClient.QuoteShell(path) }, serial, token).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            if (!compatibility) return AndroidListingParser.Parse(output);

            var files = new List<AndroidFile>();
            foreach (string name in output.Split('\n').Select(line => line.TrimEnd('\r')).Where(line => line.Length > 0)) {
                if (name is "." or "..") continue;
                string item = AdbClient.QuoteShell(TransferCommand.RemotePath(path, name));
                // Test the actual entry; dots and extensions do not determine its type.
                string kind = (await query(new[] { "shell", $"if [ -d {item} ]; then echo directory; else echo file; fi" },
                    serial, token).ConfigureAwait(false)).Trim();
                token.ThrowIfCancellationRequested();
                if (kind is not ("directory" or "file")) throw new InvalidDataException("Could not read the entry type.");
                files.Add(new(name, kind == "directory" ? "d---------" : "----------", null, null));
            }
            return files;
        }
    }

    // UI requests run on the UI thread. A late result can never replace a newer request.
    internal sealed class LatestBrowserRequest : IDisposable {
        private CancellationTokenSource? active;
        internal CancellationToken Start() {
            Cancel();
            active = new CancellationTokenSource();
            return active.Token;
        }
        internal bool IsCurrent(CancellationToken token) => active != null && active.Token == token && !token.IsCancellationRequested;
        internal void Cancel() {
            var previous = active;
            active = null;
            if (previous == null) return;
            previous.Cancel();
            previous.Dispose();
        }
        public void Dispose() => Cancel();
    }
}
