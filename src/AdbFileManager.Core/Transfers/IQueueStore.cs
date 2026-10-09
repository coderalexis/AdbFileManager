namespace AdbFileManager.Core.Transfers;

public interface IQueueStore
{
    List<TransferJob> Load();
    void Save(IEnumerable<TransferJob> jobs);
}
