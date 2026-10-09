namespace AdbFileManager.Infrastructure.Adb;

public interface IAdbClient
{
    int ProgressIntervalMs
    {
        get; set;
    }
    Task<AdbResult> ExecuteAsync(IEnumerable<string> arguments, string? deviceId = null, CancellationToken cancellationToken = default);
    Task<string> QueryAsync(IEnumerable<string> arguments, string? deviceId = null, CancellationToken cancellationToken = default);
    Task CopyAsync(IEnumerable<string> arguments, IProgress<int>? progress, CancellationToken cancellationToken);
    Task<TransferStatistics> CopyWithStatisticsAsync(IEnumerable<string> arguments, IProgress<int>? progress, CancellationToken cancellationToken);
    Task<string> VersionAsync();
}
