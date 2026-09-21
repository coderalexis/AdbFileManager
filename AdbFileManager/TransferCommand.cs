namespace AdbFileManager {
    internal static class TransferCommand {
        internal static string RemotePath(string directory, string name) {
            return directory.TrimEnd('/') + "/" + name;
        }

        internal static string[] Create(string? deviceId, bool fromAndroid,
            bool preserveTimestamp, string sourceDirectory, string destinationDirectory, string name) {
            var arguments = new List<string>();
            if (!string.IsNullOrWhiteSpace(deviceId)) {
                arguments.Add("-s");
                arguments.Add(deviceId);
            }
            arguments.Add(fromAndroid ? "pull" : "push");
            // ADB only accepts -a on pull. Push preserves timestamps without this option.
            if (fromAndroid && preserveTimestamp) arguments.Add("-a");
            arguments.Add(fromAndroid ? RemotePath(sourceDirectory, name)
                : Path.Combine(sourceDirectory, name));
            arguments.Add(fromAndroid ? Path.Combine(destinationDirectory, name)
                : RemotePath(destinationDirectory, name));
            return arguments.ToArray();
        }
    }
}
