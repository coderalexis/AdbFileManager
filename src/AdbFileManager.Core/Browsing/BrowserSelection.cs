namespace AdbFileManager.Core.Browsing;

public sealed record BrowserSelection(string Path, string DeviceSerial, IReadOnlySet<string> Names)
{
    public IReadOnlySet<string> Restore(string path, string serial, IEnumerable<AndroidFile> visibleFiles) =>
        Path == path && DeviceSerial == serial
            ? visibleFiles.Where(file => Names.Contains(file.Name)).Select(file => file.Name).ToHashSet(StringComparer.Ordinal)
            : new HashSet<string>(StringComparer.Ordinal);
}
