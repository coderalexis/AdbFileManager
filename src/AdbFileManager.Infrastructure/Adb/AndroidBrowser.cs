namespace AdbFileManager.Infrastructure.Adb
{
    public sealed class AndroidBrowser : IAndroidBrowser
    {
        private readonly Func<string[], string?, CancellationToken, Task<string>> query;
        public AndroidBrowser(IAdbClient client) : this((args, serial, token) => client.QueryAsync(args, serial, token)) { }
        internal AndroidBrowser(Func<string[], string?, CancellationToken, Task<string>> query)
        {
            this.query = query;
        }

        public async Task<IReadOnlyList<AndroidDevice>> DevicesAsync(CancellationToken token) =>
            AndroidDeviceParser.Parse(await query(new[] { "devices", "-l" }, null, token).ConfigureAwait(false));

        public async Task CreateDirectoryAsync(string path, string serial, CancellationToken token) =>
            await query(new[] { "shell", "mkdir " + AdbClient.QuoteShell(path) }, serial, token).ConfigureAwait(false);

        public async Task<string?> FindExternalStorageAsync(string serial, CancellationToken token)
        {
            string output = await query(new[] { "shell", "for path in /storage/????-????; do if [ -d \"$path\" ] && [ -r \"$path\" ]; then printf '%s\\n' \"$path\"; break; fi; done; exit 0" }, serial, token).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            string? path = output.Split('\n').Select(line => line.TrimEnd('\r')).FirstOrDefault(line => line.StartsWith("/storage/", StringComparison.Ordinal));
            return path;
        }

        public async Task<IReadOnlyList<AndroidFile>> ListAsync(string path, string serial,
            bool compatibility, CancellationToken token)
        {
            string output = await query(new[] { "shell", (compatibility ? "ls -1 " : "ls -lL ") +
                AdbClient.QuoteShell(path) }, serial, token).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            if (!compatibility)
                return AndroidListingParser.Parse(output);

            var files = new List<AndroidFile>();
            foreach (string name in output.Split('\n').Select(line => line.TrimEnd('\r')).Where(line => line.Length > 0))
            {
                if (name is "." or "..")
                    continue;
                string item = AdbClient.QuoteShell(AndroidPath.Combine(path, name));
                // Test the actual entry; dots and extensions do not determine its type.
                string kind = (await query(new[] { "shell", $"if [ -d {item} ]; then echo directory; else echo file; fi" },
                    serial, token).ConfigureAwait(false)).Trim();
                token.ThrowIfCancellationRequested();
                if (kind is not ("directory" or "file"))
                    throw new BrowserException(BrowserStatus.InvalidListing, "Could not read the entry type.");
                files.Add(new(name, kind == "directory" ? "d---------" : "----------", null, null));
            }
            return files;
        }
    }

}
