namespace AdbFileManager.Core.Transfers;

public enum TransferState
{
    Pending, Running, Retrying, Completed, Failed, Skipped, Cancelled
}
