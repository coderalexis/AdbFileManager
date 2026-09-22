using System.Security.Cryptography;
using AdbFileManager.Transfers;
using Xunit;
using Xunit.Abstractions;

namespace AdbFileManager.Tests;

public sealed class PhysicalDeviceFactAttribute : FactAttribute {
    public PhysicalDeviceFactAttribute() {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("AFM_DEVICE_SERIAL")))
            Skip = "Set AFM_DEVICE_SERIAL to run the physical Android device test.";
    }
}

public sealed class PhysicalDeviceTests {
    private readonly ITestOutputHelper output;
    public PhysicalDeviceTests(ITestOutputHelper output) { this.output = output; }

    [PhysicalDeviceFact]
    [Trait("Category", "PhysicalDevice")]
    public async Task TransferBackendAndQueueRoundTripOnAndroidDevice() {
        string serial = Environment.GetEnvironmentVariable("AFM_DEVICE_SERIAL")!;
        string adbPath = Environment.GetEnvironmentVariable("AFM_ADB_PATH") ?? FindBundledAdb();
        var client = new AdbClient(adbPath);
        var backend = new AdbTransferBackend(client);
        string remoteRoot = "/sdcard/Download/AdbFileManager-smoke-" + Guid.NewGuid().ToString("N");
        string localRoot = Directory.CreateTempSubdirectory("AdbFileManager-device-").FullName;
        var progress = new Progress<int>();

        output.WriteLine($"ADB: {await client.VersionAsync()}");
        output.WriteLine($"Device: {Mask(serial)}");
        output.WriteLine($"Model: {(await client.QueryAsync(new[] { "shell", "getprop ro.product.model" }, serial)).Trim()}");
        output.WriteLine($"Remote scratch directory: {remoteRoot}");

        try {
            Assert.Equal("device", (await client.QueryAsync(new[] { "get-state" }, serial)).Trim());
            await client.QueryAsync(new[] { "shell", $"mkdir -p {AdbClient.QuoteShell(remoteRoot)}" }, serial);

            string originalFile = Path.Combine(localRoot, "original", "payload teléfono.bin");
            Directory.CreateDirectory(Path.GetDirectoryName(originalFile)!);
            byte[] originalBytes = new byte[1024 * 1024];
            new Random(12345).NextBytes(originalBytes);
            await File.WriteAllBytesAsync(originalFile, originalBytes);
            string originalHash = Hash(originalBytes);
            string remoteFile = remoteRoot + "/payload teléfono.bin";

            await backend.CopyAsync(new TransferJob {
                DeviceId = serial, Source = originalFile, Destination = remoteFile
            }, remoteFile, false, progress, default);
            Assert.Equal(EntryKind.File, await backend.InspectAsync(new TransferJob {
                DeviceId = serial
            }, remoteFile, default));
            string deviceHash = (await client.QueryAsync(new[] { "shell",
                $"sha256sum {AdbClient.QuoteShell(remoteFile)}" }, serial)).Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
            Assert.Equal(originalHash, deviceHash, ignoreCase: true);
            output.WriteLine($"PASS push 1 MiB Unicode filename: {originalHash}");

            string firstPull = Path.Combine(localRoot, "first-pull", "payload teléfono.bin");
            await backend.CopyAsync(new TransferJob {
                DeviceId = serial, Source = remoteFile, Destination = firstPull, FromAndroid = true
            }, firstPull, false, progress, default);
            Assert.Equal(originalHash, Hash(await File.ReadAllBytesAsync(firstPull)), ignoreCase: true);
            output.WriteLine("PASS pull round trip and SHA-256 comparison");

            string replacementFile = Path.Combine(localRoot, "replacement", "payload teléfono.bin");
            Directory.CreateDirectory(Path.GetDirectoryName(replacementFile)!);
            byte[] replacementBytes = new byte[1024 * 1024];
            new Random(67890).NextBytes(replacementBytes);
            await File.WriteAllBytesAsync(replacementFile, replacementBytes);
            string replacementHash = Hash(replacementBytes);
            var replacementJob = new TransferJob {
                DeviceId = serial, Source = replacementFile, Destination = remoteFile
            };

            await Assert.ThrowsAsync<IOException>(() =>
                backend.CopyAsync(replacementJob, remoteFile, false, progress, default));
            Assert.Equal(originalHash, await RemoteHash(client, serial, remoteFile), ignoreCase: true);
            output.WriteLine("PASS conflict without Replace preserved existing data");

            await backend.CopyAsync(replacementJob, remoteFile, true, progress, default);
            Assert.Equal(replacementHash, await RemoteHash(client, serial, remoteFile), ignoreCase: true);
            output.WriteLine($"PASS explicit file replacement: {replacementHash}");

            var keepBothJob = new TransferJob {
                BatchId = Guid.NewGuid(), DeviceId = serial, Source = originalFile, Destination = remoteFile
            };
            var queue = new TransferQueue(backend, (_, _, _) =>
                Task.FromResult(new ConflictDecision(ConflictAction.KeepBoth))) { RetryDelay = TimeSpan.Zero };
            queue.Enqueue(new[] { keepBothJob });
            await queue.RunAsync();
            Assert.Equal(TransferState.Completed, keepBothJob.State);
            Assert.NotNull(keepBothJob.ResolvedDestination);
            Assert.NotEqual(remoteFile, keepBothJob.ResolvedDestination);
            Assert.Equal(originalHash,
                await RemoteHash(client, serial, keepBothJob.ResolvedDestination!), ignoreCase: true);
            output.WriteLine($"PASS queue Keep both: {keepBothJob.ResolvedDestination}");

            string sourceDirectory = Path.Combine(localRoot, "album test");
            Directory.CreateDirectory(Path.Combine(sourceDirectory, "nested"));
            await File.WriteAllTextAsync(Path.Combine(sourceDirectory, "nested", "README.txt"),
                "AdbFileManager physical device smoke test");
            string remoteDirectory = remoteRoot + "/album test";
            await backend.CopyAsync(new TransferJob {
                DeviceId = serial, Source = sourceDirectory, Destination = remoteDirectory, IsDirectory = true
            }, remoteDirectory, false, progress, default);
            string pulledDirectory = Path.Combine(localRoot, "pulled album test");
            await backend.CopyAsync(new TransferJob {
                DeviceId = serial, Source = remoteDirectory, Destination = pulledDirectory,
                FromAndroid = true, IsDirectory = true
            }, pulledDirectory, false, progress, default);
            Assert.Equal("AdbFileManager physical device smoke test",
                await File.ReadAllTextAsync(Path.Combine(pulledDirectory, "nested", "README.txt")));
            output.WriteLine("PASS nested directory push/pull round trip");
            output.WriteLine("RESULT: ALL PHYSICAL DEVICE CHECKS PASSED");
        }
        finally {
            try {
                await client.QueryAsync(new[] { "shell", $"rm -rf {AdbClient.QuoteShell(remoteRoot)}" }, serial);
                output.WriteLine("PASS remote scratch directory removed");
            }
            catch (Exception ex) { output.WriteLine("CLEANUP WARNING: " + ex.Message); }
            try { Directory.Delete(localRoot, true); }
            catch (Exception ex) { output.WriteLine("LOCAL CLEANUP WARNING: " + ex.Message); }
        }
    }

    private static async Task<string> RemoteHash(AdbClient client, string serial, string path) =>
        (await client.QueryAsync(new[] { "shell", $"sha256sum {AdbClient.QuoteShell(path)}" }, serial))
        .Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];

    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static string Mask(string serial) => serial.Length <= 4 ? "****" : "…" + serial[^4..];

    private static string FindBundledAdb() {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory != null) {
            string candidate = Path.Combine(directory.FullName, "AdbFileManager", "adb.exe");
            if (File.Exists(candidate)) return candidate;
            directory = directory.Parent;
        }
        throw new FileNotFoundException("Set AFM_ADB_PATH to the bundled adb.exe.");
    }
}
