using AdbFileManager.Transfers;
using Xunit;

namespace AdbFileManager.Tests;

public class AdbBackendTests {
    private static AdbClient Client => new(Path.ChangeExtension(typeof(FakeAdb.Program).Assembly.Location, ".exe"));
    private sealed class ProgressSink : IProgress<int> { public void Report(int value) { } }

    [Fact]
    public async Task VersionIsReadFromTheConfiguredExecutable() {
        Assert.Equal("37.0.1-test", await Client.VersionAsync());
    }

    [Theory]
    [InlineData("a'b", "'a'\"'\"'b'")]
    [InlineData("a & $(echo bad)", "'a & $(echo bad)'")]
    public void RemotePathsAreQuotedAsLiteralShellArguments(string value, string expected) {
        Assert.Equal(expected, AdbClient.QuoteShell(value));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PullStagesFileAndReplacesOnlyAfterSuccess(bool fail) {
        string root = Directory.CreateTempSubdirectory("afm-backend-").FullName;
        try {
            string source = Path.Combine(root, fail ? "fail-photo.txt" : "photo.txt");
            string output = Path.Combine(root, "output"); Directory.CreateDirectory(output);
            string destination = Path.Combine(output, Path.GetFileName(source));
            await System.IO.File.WriteAllTextAsync(source, "new contents");
            await System.IO.File.WriteAllTextAsync(destination, "original contents");
            var job = new TransferJob { Source = source, Destination = destination, FromAndroid = true, DeviceId = "test" };
            var backend = new AdbTransferBackend(Client);
            if (fail) await Assert.ThrowsAsync<AdbCommandException>(() => backend.CopyAsync(job, destination, true, new ProgressSink(), default));
            else await backend.CopyAsync(job, destination, true, new ProgressSink(), default);
            Assert.Equal(fail ? "original contents" : "new contents", await System.IO.File.ReadAllTextAsync(destination));
            Assert.Empty(Directory.GetDirectories(output, ".afm-*"));
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public async Task FolderPullDoesNotIntroduceExtraNesting() {
        string root = Directory.CreateTempSubdirectory("afm-backend-").FullName;
        try {
            string source = Path.Combine(root, "Fotos.2026"); Directory.CreateDirectory(source);
            await System.IO.File.WriteAllTextAsync(Path.Combine(source, "README"), "photo");
            string destination = Path.Combine(root, "output", "Fotos.2026 (1)");
            var job = new TransferJob { Source = source, Destination = destination, FromAndroid = true, IsDirectory = true, DeviceId = "test" };
            await new AdbTransferBackend(Client).CopyAsync(job, destination, false, new ProgressSink(), default);
            Assert.True(System.IO.File.Exists(Path.Combine(destination, "README")));
            Assert.False(Directory.Exists(Path.Combine(destination, "Fotos.2026")));
        }
        finally { Directory.Delete(root, true); }
    }

    [Theory]
    [InlineData(EntryKind.Directory, true)]
    [InlineData(EntryKind.Other, true)]
    [InlineData(EntryKind.File, false)]
    public void DestinationChangesCannotSilentlyOverwriteData(EntryKind kind, bool replace) {
        Assert.Throws<IOException>(() => AdbTransferBackend.VerifyDestination(new TransferJob(), kind, replace));
    }
}
