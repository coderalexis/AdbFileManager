using Xunit;

namespace AdbFileManager.Tests;

public sealed class BrowserUxTests
{
    [Fact]
    public void FilteringPreservesLiteralNamesAndUsesCaseInsensitiveMatching()
    {
        var files = new[] { new AndroidFile("a'b [foto].JPG", "-rw-r--r--", 1, null), new AndroidFile("teléfono.bin", "-rw-r--r--", 1, null) };
        Assert.Equal(files[0], Assert.Single(BrowserFileFilter.Apply(files, "[FOTO]")));
        Assert.Equal(files[1], Assert.Single(BrowserFileFilter.Apply(files, "TELÉFONO")));
        Assert.Empty(BrowserFileFilter.Apply(files, "missing"));
    }

    [Fact]
    public void RefreshRestoresOnlySurvivingVisibleNamesInSameFolderAndDevice()
    {
        var saved = new BrowserSelection("/sdcard/", "phone-A", new HashSet<string>(new[] { "same.txt", "removed.txt" }, StringComparer.Ordinal));
        var files = new[] { new AndroidFile("same.txt", "-rw-r--r--", 1, null), new AndroidFile("SAME.txt", "-rw-r--r--", 1, null) };
        Assert.Equal("same.txt", Assert.Single(saved.Restore("/sdcard/", "phone-A", files)));
        Assert.Empty(saved.Restore("/other/", "phone-A", files));
        Assert.Empty(saved.Restore("/sdcard/", "phone-B", files));
        Assert.Empty(saved.Restore("/sdcard/", "phone-A", Array.Empty<AndroidFile>()));
    }

    [Fact]
    public void BreadcrumbSegmentsKeepUnicodeSpacesAndCorrectParentPaths()
    {
        var segments = AndroidBreadcrumb.Build("/sdcard/Fotos del teléfono/DCIM/");
        Assert.Equal(new[] { "/", "sdcard", "Fotos del teléfono", "DCIM" }, segments.Select(segment => segment.Label));
        Assert.Equal("/sdcard/Fotos del teléfono/", segments[2].Path);
        Assert.Equal("/", Assert.Single(AndroidBreadcrumb.Build("/")).Path);
    }

    [Fact]
    public async Task ExternalStorageProbeUsesCapturedDeviceAndIgnoresUnrelatedOutput()
    {
        string? captured = null;
        var browser = new AndroidBrowser((_, serial, _) => { captured = serial; return Task.FromResult("diagnostic\n/storage/1234-ABCD\n"); });
        Assert.Equal("/storage/1234-ABCD", await browser.FindExternalStorageAsync("phone-A", default));
        Assert.Equal("phone-A", captured);
        var missing = new AndroidBrowser((_, _, _) => Task.FromResult(""));
        Assert.Null(await missing.FindExternalStorageAsync("phone-A", default));
    }
}
