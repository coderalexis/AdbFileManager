namespace AdbFileManager.Core.Browsing;

public sealed record AndroidFile(string Name, string Permissions, long? Bytes, DateTime? Modified)
{
    public bool IsDirectory => DirectoryEntry.IsDirectory(Permissions);
}
