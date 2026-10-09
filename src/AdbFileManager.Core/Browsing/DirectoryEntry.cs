namespace AdbFileManager.Core.Browsing
{
    public static class DirectoryEntry
    {
        // Both modern and compatibility listings provide a permissions column.
        public static bool IsDirectory(string? permissions)
        {
            return permissions?.TrimStart().StartsWith("d", StringComparison.OrdinalIgnoreCase) == true;
        }

        public static string? ParentPath(string path)
        {
            string trimmed = path.TrimEnd('/');
            int separator = trimmed.LastIndexOf('/');
            return separator < 0 ? null : trimmed[..(separator + 1)];
        }
    }
}
