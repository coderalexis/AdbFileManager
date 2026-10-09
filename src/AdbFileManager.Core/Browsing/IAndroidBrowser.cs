namespace AdbFileManager.Core.Browsing;

public interface IAndroidBrowser
{
    Task<IReadOnlyList<AndroidDevice>> DevicesAsync(CancellationToken token);
    Task<IReadOnlyList<AndroidFile>> ListAsync(string path, string serial, bool compatibility, CancellationToken token);
    Task CreateDirectoryAsync(string path, string serial, CancellationToken token);
}
