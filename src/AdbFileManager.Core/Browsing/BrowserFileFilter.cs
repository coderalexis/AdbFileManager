namespace AdbFileManager.Core.Browsing;

public static class BrowserFileFilter
{
    public static IReadOnlyList<AndroidFile> Apply(IEnumerable<AndroidFile> files, string query) =>
        files.Where(file => file.Name.Contains(query, StringComparison.OrdinalIgnoreCase)).ToArray();
}
