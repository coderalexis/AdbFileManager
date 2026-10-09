using Xunit;

namespace AdbFileManager.Tests;

public sealed class ArchitectureTests
{
    [Fact]
    public void SettingsServiceHasNoSharedGlobalState()
    {
        var first = new MemorySettingsStore();
        var second = new MemorySettingsStore();
        var serviceA = new SettingsService(first);
        var serviceB = new SettingsService(second);
        serviceA.Current.DarkMode = true;
        Assert.False(serviceB.Current.DarkMode);
        serviceA.Save();
        Assert.True(first.Saved);
        Assert.False(second.Saved);
    }
    private sealed class MemorySettingsStore : ISettingsStore
    {
        public bool Saved
        {
            get; private set;
        }
        public SettingsLoadResult Load() => new(new Settings(), null);
        public void Save(Settings value) => Saved = true;
    }
    [Fact]
    public void CoreDoesNotReferenceInfrastructureOrWindowsUi()
    {
        var references = typeof(TransferQueue).Assembly.GetReferencedAssemblies().Select(reference => reference.Name).ToArray();
        Assert.DoesNotContain(references, name => name!.Contains("Infrastructure") || name.Contains("WinForms") || name.Contains("Windows.Forms") || name.Contains("WindowsAPICodePack"));
    }
    [Fact]
    public void InfrastructureDependsOnCoreAndNotOnWindowsUi()
    {
        var references = typeof(AdbClient).Assembly.GetReferencedAssemblies().Select(reference => reference.Name).ToArray();
        Assert.Contains("AdbFileManager.Core", references);
        Assert.DoesNotContain(references, name => name!.Contains("WinForms") || name.Contains("Windows.Forms") || name.Contains("WindowsAPICodePack"));
    }
    [Fact]
    public void TestTypesComeFromProductionAssemblies()
    {
        Assert.Equal("AdbFileManager.Core", typeof(TransferQueue).Assembly.GetName().Name);
        Assert.Equal("AdbFileManager.Infrastructure", typeof(AdbClient).Assembly.GetName().Name);
    }
    [Fact]
    public void TransferBatchCapturesDeviceAndPolicyWithoutUiDependencies()
    {
        var jobs = TransferBatchFactory.Create(new[] { new TransferSource("/sdcard/a.jpg", false), new TransferSource("/sdcard/album", true) },
            Path.GetTempPath(), "phone-A", true, true);
        Assert.Equal(2, jobs.Count);
        Assert.Equal(jobs[0].BatchId, jobs[1].BatchId);
        Assert.All(jobs, job => { Assert.Equal("phone-A", job.DeviceId); Assert.True(job.PreserveTimestamp); });
        Assert.True(jobs[1].IsDirectory);
    }
}
