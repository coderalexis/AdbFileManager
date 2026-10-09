namespace AdbFileManager.Core.Transfers
{
    public interface ITransferBackend
    {
        Task<EntryKind> InspectAsync(TransferJob job, string destination, CancellationToken cancellationToken);
        Task CopyAsync(TransferJob job, string destination, bool replace, IProgress<int> progress,
            CancellationToken cancellationToken);
    }
}
