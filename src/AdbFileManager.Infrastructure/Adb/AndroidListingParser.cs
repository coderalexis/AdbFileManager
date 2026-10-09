using System.Globalization;
using System.Text.RegularExpressions;
namespace AdbFileManager.Infrastructure.Adb
{
    internal static class AndroidListingParser
    {
        private static readonly Regex Entry = new(
            @"^(?<permissions>[bcdlps-][rwxstST-]{9}[.+@]?)\s+\d+\s+\S+\s+\S+\s+(?<size>\d+)\s+(?<date>\d{4}-\d{2}-\d{2})\s+(?<time>\d{2}:\d{2}(?::\d{2})?) (?<name>.+)$",
            RegexOptions.CultureInvariant);

        internal static IReadOnlyList<AndroidFile> Parse(string output)
        {
            var entries = new List<AndroidFile>();
            foreach (string raw in output.Split('\n'))
            {
                string line = raw.TrimEnd('\r');
                if (string.IsNullOrWhiteSpace(line) || Regex.IsMatch(line, @"^total\s+\d+\s*$"))
                    continue;
                Match match = Entry.Match(line);
                if (!match.Success || !long.TryParse(match.Groups["size"].Value, NumberStyles.None,
                    CultureInfo.InvariantCulture, out long bytes) ||
                    !DateTime.TryParseExact(match.Groups["date"].Value + " " + match.Groups["time"].Value,
                        new[] { "yyyy-MM-dd HH:mm", "yyyy-MM-dd HH:mm:ss" }, CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out DateTime modified))
                    throw new BrowserException(BrowserStatus.InvalidListing, "The Android file listing has an unsupported format.");
                string name = match.Groups["name"].Value;
                string permissions = match.Groups["permissions"].Value;
                if (permissions[0] == 'l')
                {
                    int target = name.IndexOf(" -> ", StringComparison.Ordinal);
                    if (target >= 0)
                        name = name[..target];
                }
                if (name is not ("." or ".."))
                    entries.Add(new(name, permissions, bytes, modified));
            }
            return entries;
        }
    }

}
