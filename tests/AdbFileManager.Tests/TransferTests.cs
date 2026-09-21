using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using Xunit;

namespace AdbFileManager.Tests;

public class TransferTests {
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TransferTargetsSelectedDeviceInBothDirections(bool fromAndroid) {
        var arguments = TransferCommand.Create("192.168.1.10:5555", fromAndroid, true,
            fromAndroid ? "/sdcard/DCIM" : @"C:\Photos",
            fromAndroid ? @"C:\Backup" : "/sdcard/Backup", "Fotos.2026");
        Assert.Equal(new[] { "-s", "192.168.1.10:5555" }, arguments[..2]);
        Assert.Equal(fromAndroid ? "pull" : "push", arguments[2]);
        Assert.Equal(fromAndroid, arguments.Contains("-a"));
        Assert.DoesNotContain("-p", arguments);
        Assert.Equal(fromAndroid ? "/sdcard/DCIM/Fotos.2026" : "/sdcard/Backup/Fotos.2026",
            fromAndroid ? arguments[^2] : arguments[^1]);
    }

    [Fact]
    public void PullCanDisableTimestampPreservationAndUseDefaultDevice() {
        Assert.Equal(new[] { "pull", "/sdcard/README", Path.Combine(@"C:\Backup", "README") },
            TransferCommand.Create(null, true, false, "/sdcard/", @"C:\Backup", "README"));
    }

    [Theory]
    [InlineData("d---------", true)] // Includes folders named Fotos.2026.
    [InlineData("-rw-r--r--", false)] // Includes files named README.
    [InlineData("----------", false)] // Compatibility mode's file metadata.
    [InlineData(null, false)]
    [InlineData("", false)]
    public void NavigationUsesListingMetadata(string? permissions, bool expected) {
        Assert.Equal(expected, DirectoryEntry.IsDirectory(permissions));
    }

    [Theory]
    [InlineData("/", null)]
    [InlineData("/sdcard/", "/")]
    [InlineData("/sdcard/DCIM", "/sdcard/")]
    [InlineData("/sdcard/DCIM/", "/sdcard/")]
    public void ParentNavigationHandlesRootAndTrailingSlash(string path, string? expected) {
        Assert.Equal(expected, DirectoryEntry.ParentPath(path));
    }

    [Theory]
    [InlineData("[ 42%] file", 42)]
    [InlineData("[100%] file", 100)]
    [InlineData("[101%] file", -1)]
    [InlineData("[ -1%] file", -1)]
    [InlineData("file [42%].jpg", -1)]
    [InlineData("[  ?%] file", -1)]
    public void ParsesOnlyValidProgressRecords(string line, int expected) {
        Assert.Equal(expected, AdbProgressRunner.ParseProgress(line));
    }

    private static string Dotnet => Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") ?? "dotnet";
    private static string[] FakeArguments(params string[] args) =>
        new[] { typeof(FakeAdb.Program).Assembly.Location }.Concat(args).ToArray();

    private sealed class ProgressSink(Action<int> action) : IProgress<int> {
        public void Report(int value) => action(value);
    }

    [Fact]
    public async Task ReadsProgressAcrossChunksAndWithoutFinalNewline() {
        var reports = new ConcurrentQueue<int>();
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        await AdbProgressRunner.RunAsync(Dotnet, FakeArguments("success"),
            new ProgressSink(reports.Enqueue), deadline.Token);
        Assert.Contains(12, reports);
        Assert.Contains(100, reports);
    }

    [Fact]
    public async Task DrainsBothPipesAndReportsNonzeroExitWithBoundedDiagnostics() {
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var error = await Assert.ThrowsAsync<AdbCommandException>(() => AdbProgressRunner.RunAsync(
            Dotnet, FakeArguments("failure"), cancellationToken: deadline.Token));
        Assert.Contains("code 7", error.Message);
        Assert.Contains("Permission denied: teléfono", error.Message);
        Assert.True(error.Message.Length < 8500);
    }

    [Fact]
    public async Task ArgumentsWithSpacesQuotesAndShellCharactersRemainLiteral() {
        string recordPath = Path.GetTempFileName();
        string[] literalArgs = { "Fotos del teléfono", "a&b", "%PATH%", "a\"b", @"C:\Photos\" };
        try {
            await AdbProgressRunner.RunAsync(Dotnet,
                FakeArguments(new[] { "record", recordPath }.Concat(literalArgs).ToArray()));
            Assert.Equal(literalArgs, JsonSerializer.Deserialize<string[]>(await File.ReadAllTextAsync(recordPath)));
        }
        finally { File.Delete(recordPath); }
    }

    [Fact]
    public async Task CancellingTerminatesClientAndAllowsTheNextTransfer() {
        string pidPath = Path.GetTempFileName();
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var ready = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Task running = AdbProgressRunner.RunAsync(Dotnet, FakeArguments("wait", pidPath),
            new ProgressSink(_ => ready.TrySetResult()), cancellation.Token);
        try {
            await ready.Task.WaitAsync(cancellation.Token);
            int pid = int.Parse(await File.ReadAllTextAsync(pidPath));
            using var client = Process.GetProcessById(pid);
            cancellation.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => running);
            Assert.True(client.HasExited);
            await AdbProgressRunner.RunAsync(Dotnet, FakeArguments("success"));
        }
        finally {
            cancellation.Cancel();
            try { await running; } catch (OperationCanceledException) { }
            File.Delete(pidPath);
        }
    }

    [Fact]
    public async Task PreCancelledTransferDoesNotStartAProcess() {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => AdbProgressRunner.RunAsync(
            "nonexistent-adb.exe", Array.Empty<string>(), cancellationToken: cancellation.Token));
    }
}
