using System.Globalization;
using Xunit;

namespace AdbFileManager.Tests;

public sealed class BrowserTests {
    private const string Listing = "drwxrwx--- 2 root everybody 4096 2026-09-22 10:46 Fotos.2026\n" +
        "-rw-rw---- 1 root everybody 1234 2026-10-08 14:02:03 teléfono  con espacios.bin\n";

    [Theory]
    [InlineData("")]
    [InlineData("total 8\n")]
    public void FirstEntryIsPreservedWithOrWithoutTotalHeader(string header) {
        var files = AndroidListingParser.Parse(header + Listing);
        Assert.Equal(2, files.Count);
        Assert.Equal("Fotos.2026", files[0].Name);
        Assert.True(files[0].IsDirectory);
        Assert.Equal("teléfono  con espacios.bin", files[1].Name);
        Assert.Equal(1234, files[1].Bytes);
        Assert.False(files[1].IsDirectory);
    }

    [Theory]
    [InlineData("es-MX")]
    [InlineData("en-US")]
    [InlineData("ar-SA")]
    public void DatesAndSizesDoNotDependOnSystemCulture(string culture) {
        var original = CultureInfo.CurrentCulture;
        try {
            CultureInfo.CurrentCulture = new(culture);
            var file = AndroidListingParser.Parse(Listing)[1];
            Assert.Equal(new DateTime(2026, 10, 8, 14, 2, 3), file.Modified);
            Assert.Equal(1234, file.Bytes);
        }
        finally { CultureInfo.CurrentCulture = original; }
    }

    [Theory]
    [InlineData("")]
    [InlineData("total 0\r\n")]
    public void EmptyListingContainsNoSyntheticFiles(string output) => Assert.Empty(AndroidListingParser.Parse(output));

    [Theory]
    [InlineData("ls: /sdcard: Permission denied")]
    [InlineData("-rw-r--r-- 1 root root 20 invalid date README")]
    [InlineData("-rw-r--r-- 1 root root 20 2026-13-32 10:00 README")]
    public void MalformedListingIsReportedRatherThanPartiallyHidden(string line) =>
        Assert.Throws<InvalidDataException>(() => AndroidListingParser.Parse(Listing + line));

    [Fact]
    public void DeviceParsingAcceptsTabsAndCrLfAndPreservesAuthorizationState() {
        var devices = AndroidDevice.Parse("* daemon started successfully *\nList of devices attached\r\n" +
            "abc\tdevice product:test model:SM_G780G transport_id:3\r\n" +
            "def\tunauthorized\r\nxyz offline\n");
        Assert.Equal(3, devices.Count);
        Assert.Equal("SM_G780G", devices[0].Model);
        Assert.Equal("unauthorized", devices[1].State);
        Assert.Equal("offline", devices[2].State);
    }

    [Fact]
    public async Task BrowserQueriesUseCapturedDeviceAndQuotedLiteralPath() {
        string[]? sent = null;
        string? target = null;
        var browser = new AndroidBrowser((args, serial, _) => {
            sent = args; target = serial;
            return Task.FromResult(Listing);
        });
        await browser.ListAsync("/sdcard/a'b & test/", "phone-A", false, default);
        Assert.Equal("phone-A", target);
        Assert.Equal(new[] { "shell", "ls -lL '/sdcard/a'\"'\"'b & test/'" }, sent);
    }

    [Fact]
    public async Task CompatibilityUsesActualTypeForDottedDirectoriesAndExtensionlessFiles() {
        var browser = new AndroidBrowser((args, _, _) => Task.FromResult(
            args[1].StartsWith("ls ") ? "Fotos.2026\nREADME\n" :
            args[1].Contains("Fotos.2026") ? "directory\n" : "file\n"));
        var files = await browser.ListAsync("/sdcard/", "phone", true, default);
        Assert.True(files[0].IsDirectory);
        Assert.False(files[1].IsDirectory);
        Assert.Null(files[0].Bytes);
        Assert.Null(files[0].Modified);
    }

    [Fact]
    public async Task QueryFailureIsNotTurnedIntoAnEmptyFolder() {
        var browser = new AndroidBrowser((_, _, _) => Task.FromException<string>(
            new AdbCommandException(1, "Permission denied")));
        await Assert.ThrowsAsync<AdbCommandException>(() => browser.ListAsync("/data/", "phone", false, default));
    }

    [Fact]
    public async Task LatestRequestCancelsQueryAndRejectsLateResult() {
        using var requests = new LatestBrowserRequest();
        var completion = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        CancellationToken observed = default;
        var browser = new AndroidBrowser((_, _, token) => { observed = token; return completion.Task; });
        var first = requests.Start();
        Task<IReadOnlyList<AndroidFile>> pending = browser.ListAsync("/old/", "phone", false, first);
        var second = requests.Start();
        Assert.True(observed.IsCancellationRequested);
        Assert.False(requests.IsCurrent(first));
        Assert.True(requests.IsCurrent(second));
        completion.SetResult(Listing);
        await Assert.ThrowsAsync<OperationCanceledException>(() => pending);
        requests.Cancel();
        Assert.True(second.IsCancellationRequested);
        Assert.False(requests.IsCurrent(second));
    }
}
