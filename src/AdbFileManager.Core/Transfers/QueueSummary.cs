namespace AdbFileManager.Core.Transfers;

public sealed record QueueSummary(int Completed, int Failed, int Skipped, int Cancelled, int Pending);
