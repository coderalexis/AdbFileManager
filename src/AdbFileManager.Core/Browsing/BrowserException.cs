namespace AdbFileManager.Core.Browsing;

public sealed class BrowserException(BrowserStatus status, string message) : IOException(message)
{
    public BrowserStatus Status { get; } = status;
}
