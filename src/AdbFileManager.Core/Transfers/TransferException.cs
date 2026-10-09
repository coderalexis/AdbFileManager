namespace AdbFileManager.Core.Transfers;

public class TransferException(string message, bool isTransient) : IOException(message)
{
    public bool IsTransient { get; } = isTransient;
}
