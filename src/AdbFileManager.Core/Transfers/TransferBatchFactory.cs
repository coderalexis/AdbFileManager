namespace AdbFileManager.Core.Transfers;

public sealed record TransferSource(string Path, bool IsDirectory);

public static class TransferBatchFactory
{
    public static IReadOnlyList<TransferJob> Create(IEnumerable<TransferSource> sources, string destinationDirectory,
        string deviceSerial, bool fromAndroid, bool preserveTimestamp)
    {
        if (string.IsNullOrWhiteSpace(deviceSerial))
            throw new ArgumentException("A device serial is required.", nameof(deviceSerial));
        Guid batch = Guid.NewGuid();
        return sources.Select(source =>
        {
            string name = fromAndroid ? source.Path.TrimEnd('/').Split('/')[^1] : System.IO.Path.GetFileName(source.Path.TrimEnd('\\'));
            if (fromAndroid && (name.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0 || name is "." or ".."))
                throw new IOException($"The name cannot be copied to Windows: {name}");
            return new TransferJob
            {
                BatchId = batch,
                DeviceId = deviceSerial,
                Source = source.Path,
                Destination = fromAndroid ? System.IO.Path.Combine(destinationDirectory, name) : AndroidPath.Combine(destinationDirectory, name),
                FromAndroid = fromAndroid,
                IsDirectory = source.IsDirectory,
                PreserveTimestamp = preserveTimestamp
            };
        }).ToArray();
    }
}
