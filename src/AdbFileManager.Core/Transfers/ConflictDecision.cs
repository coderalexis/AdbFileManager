namespace AdbFileManager.Core.Transfers;

public sealed record ConflictDecision(ConflictAction Action, bool ApplyToBatch = false);
