namespace AdbFileManager.Infrastructure.Adb;

public static class AdbTransferArguments
{
    public static string[] Create(string? deviceId, bool fromAndroid, bool preserveTimestamp,
        string sourceDirectory, string destinationDirectory, string name)
    {
        var arguments = new List<string>();
        if (!string.IsNullOrWhiteSpace(deviceId))
            arguments.AddRange(new[] { "-s", deviceId });
        arguments.Add(fromAndroid ? "pull" : "push");
        if (fromAndroid && preserveTimestamp)
            arguments.Add("-a");
        arguments.Add(fromAndroid ? AndroidPath.Combine(sourceDirectory, name) : Path.Combine(sourceDirectory, name));
        arguments.Add(fromAndroid ? Path.Combine(destinationDirectory, name) : AndroidPath.Combine(destinationDirectory, name));
        return arguments.ToArray();
    }
}
