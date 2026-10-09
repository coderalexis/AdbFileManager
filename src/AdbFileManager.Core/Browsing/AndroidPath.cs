namespace AdbFileManager.Core.Browsing;

public static class AndroidPath
{
    public static string Combine(string directory, string name) => directory.TrimEnd('/') + "/" + name;
    public static string? Parent(string path)
    {
        string trimmed = path.TrimEnd('/');
        int separator = trimmed.LastIndexOf('/');
        return separator < 0 ? null : trimmed[..(separator + 1)];
    }
}
