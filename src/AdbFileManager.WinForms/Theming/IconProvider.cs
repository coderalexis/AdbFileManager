namespace AdbFileManager;

internal sealed class IconProvider(string assetRoot, bool windows11Icons) : IDisposable
{
    private readonly Dictionary<string, Icon> icons = new();
    private readonly Dictionary<string, Bitmap> images = new();
    private string AssetPath(string name)
    {
        string preferred = Path.Combine(assetRoot, windows11Icons ? "iconsW11" : "icons", name);
        return System.IO.File.Exists(preferred) ? preferred : Path.Combine(assetRoot, "icons", name);
    }
    internal Icon GetIcon(string name)
    {
        if (!icons.TryGetValue(name, out var icon))
            icons[name] = icon = new Icon(AssetPath(name + ".ico"));
        return icon;
    }
    internal Bitmap GetNavigation(string name)
    {
        if (!images.TryGetValue(name, out var image))
            images[name] = image = new Bitmap(AssetPath(name + ".png"));
        return image;
    }
    internal Bitmap GetSmallImage(string name)
    {
        string key = "small-" + name;
        if (!images.TryGetValue(key, out var image))
        {
            using var small = new Icon(GetIcon(name), new Size(16, 16));
            images[key] = image = small.ToBitmap();
        }
        return image;
    }
    internal Icon GetForFile(string name, bool isDirectory)
    {
        string icon;
        if (isDirectory)
        {
            string lower = name.ToLowerInvariant();
            icon = lower.Contains("dcim") || lower.EndsWith("pictures") ? "folder_image"
                : lower.EndsWith("download") ? "folder_downloads" : lower.EndsWith("music") ? "folder_music"
                : lower.EndsWith("movies") ? "folder_video" : lower.EndsWith("documents") ? "folder_document"
                : lower.EndsWith("android") ? "folder_android" : "folder2";
        }
        else
        {
            string extension = Path.GetExtension(name);
            icon = MediaFileTypes.ImageExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase) ? "image2"
                : MediaFileTypes.VideoExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase) ? "video2"
                : MediaFileTypes.AudioExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase) ? "music2"
                : new[] { ".zip", ".rar", ".7z", ".tar", ".gz", ".bz2", ".xz" }.Contains(extension, StringComparer.OrdinalIgnoreCase) ? "archive"
                : new[] { ".pdf", ".txt", ".docx", ".xlsx", ".pptx", ".csv", ".md" }.Contains(extension, StringComparer.OrdinalIgnoreCase) ? "doc2"
                : new[] { ".exe", ".dll", ".bat", ".msi", ".apk" }.Contains(extension, StringComparer.OrdinalIgnoreCase) ? "exe" : "file";
        }
        return GetIcon(icon);
    }
    public void Dispose()
    {
        foreach (var image in images.Values)
            image.Dispose();
        foreach (var icon in icons.Values)
            icon.Dispose();
        images.Clear();
        icons.Clear();
    }
}
