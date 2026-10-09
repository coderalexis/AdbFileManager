namespace AdbFileManager.Infrastructure.Adb;

public sealed class MediaPreviewService(IAdbClient client) : IDisposable
{
    private string? temporaryDirectory;
    private readonly CancellationTokenSource lifetime = new();
    private bool disposed;
    public async Task<string> DownloadAsync(string source, string name, string deviceSerial, CancellationToken token)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (string.IsNullOrEmpty(name) || name is "." or ".." || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new IOException("This file name cannot be previewed on Windows.");
        temporaryDirectory ??= Directory.CreateTempSubdirectory("AdbFileManager-preview-").FullName;
        string destination = Path.Combine(temporaryDirectory, name);
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(token, lifetime.Token);
        await client.CopyAsync(AdbClient.TargetArguments(new[] { "pull", source, destination }, deviceSerial), null, cancellation.Token).ConfigureAwait(false);
        cancellation.Token.ThrowIfCancellationRequested();
        return destination;
    }
    public void Dispose()
    {
        if (disposed)
            return;
        disposed = true;
        lifetime.Cancel();
        lifetime.Dispose();
        if (temporaryDirectory == null)
            return;
        // Only this instance's own directory, created above, is eligible for cleanup.
        try
        {
            Directory.Delete(temporaryDirectory, true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { Console.Error.WriteLine(ex.Message); }
        temporaryDirectory = null;
    }
}
