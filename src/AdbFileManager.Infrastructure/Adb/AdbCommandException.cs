namespace AdbFileManager.Infrastructure.Adb;

public sealed class AdbCommandException : TransferException
{
    public int ExitCode
    {
        get;
    }
    public AdbCommandException(int code, string details) : base($"ADB exited with code {code}.\n{details.Trim()}", IsTransientMessage(details))
    {
        ExitCode = code;
    }
    private static bool IsTransientMessage(string details) =>
        new[] { "device offline", "device not found", "no devices", "disconnected", "connection reset", "connection closed", "protocol fault", "cannot connect", "device '" }
            .Any(value => details.Contains(value, StringComparison.OrdinalIgnoreCase));
}
