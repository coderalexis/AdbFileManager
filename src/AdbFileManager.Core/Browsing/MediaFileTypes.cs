using System.Collections.Frozen;

namespace AdbFileManager.Core.Browsing;

public static class MediaFileTypes
{
    public static readonly IReadOnlySet<string> ImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".webp", ".heif", ".heic", ".mpo", ".gif", ".tiff" }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    public static readonly IReadOnlySet<string> VideoExtensions = new[] { ".mp4", ".mkv", ".webm", ".avi", ".mov", ".wmv", ".flv", ".3gp", ".m4v", ".mpg", ".mpeg", ".m2v", ".m2ts", ".mts", ".ts", ".vob", ".divx", ".xvid" }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    public static readonly IReadOnlySet<string> AudioExtensions = new[] { ".mp3", ".wav", ".ogg", ".flac", ".m4a", ".aac", ".wma", ".mod", ".mid", ".s3m", ".midi" }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    public static bool CanPreview(string name) => ImageExtensions.Concat(VideoExtensions).Concat(AudioExtensions)
        .Contains(Path.GetExtension(name), StringComparer.OrdinalIgnoreCase);
}
