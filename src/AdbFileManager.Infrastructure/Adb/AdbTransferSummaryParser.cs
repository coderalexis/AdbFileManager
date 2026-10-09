using System.Globalization;
using System.Text.RegularExpressions;

namespace AdbFileManager.Infrastructure.Adb;

internal static class AdbTransferSummaryParser
{
    internal static TransferStatistics Parse(string output, TimeSpan elapsed)
    {
        var matches = Regex.Matches(output, @"\((?<bytes>\d+) bytes in \d+(?:\.\d+)?s\)", RegexOptions.CultureInvariant);
        long? bytes = matches.Count > 0 && long.TryParse(matches[^1].Groups["bytes"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out long parsed) ? parsed : null;
        return new(bytes, elapsed.TotalSeconds);
    }
}
