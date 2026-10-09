namespace AdbFileManager.Infrastructure.Adb;

public sealed record AdbResult(int ExitCode, string Output, string Error)
{
    public string CombinedOutput => Output + Error;
    public void EnsureSuccess()
    {
        if (ExitCode != 0)
            throw new AdbCommandException(ExitCode, string.IsNullOrWhiteSpace(Error) ? Output : Error);
    }
}
