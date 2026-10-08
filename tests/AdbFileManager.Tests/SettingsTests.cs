using System.Xml.Serialization;
using Xunit;

namespace AdbFileManager.Tests;

public sealed class SettingsTests : IDisposable {
    private readonly string root = Directory.CreateTempSubdirectory("afm-settings-").FullName;
    private string SettingsPath => Path.Combine(root, "settings.xml");
    public void Dispose() => Directory.Delete(root, true);

    [Fact]
    public void AtomicSaveKeepsPreviousVersionAsBackup() {
        var store = new SettingsStore(SettingsPath);
        store.Save(new Settings { DarkMode = true, lang = 5 });
        store.Save(new Settings { DarkMode = false, lang = 3 });
        Assert.False(store.Load().Value.DarkMode);
        Assert.Equal((ushort)3, store.Load().Value.lang);
        Assert.True(new SettingsStore(SettingsPath + ".bak").Load().Value.DarkMode);
        Assert.Empty(Directory.GetFiles(root, "settings.xml.tmp-*"));
    }

    [Fact]
    public void CorruptMainRestoresBackupAndPreservesDamagedBytes() {
        var store = new SettingsStore(SettingsPath);
        store.Save(new Settings { DarkMode = true });
        store.Save(new Settings { DarkMode = false });
        File.WriteAllText(SettingsPath, "<Settings><broken");
        var recovered = store.Load();
        Assert.True(recovered.Value.DarkMode);
        Assert.NotNull(recovered.Warning);
        Assert.Equal("<Settings><broken", File.ReadAllText(Assert.Single(Directory.GetFiles(root, "settings.xml.corrupt-*"))));
        store.Save(recovered.Value);
        Assert.True(store.Load().Value.DarkMode);
        Assert.True(new SettingsStore(SettingsPath + ".bak").Load().Value.DarkMode);
    }

    [Fact]
    public void CorruptFileWithoutBackupUsesDefaultsWithoutFailingStartup() {
        File.WriteAllText(SettingsPath, "not XML");
        var recovered = new SettingsStore(SettingsPath).Load();
        Assert.False(recovered.Value.DarkMode);
        Assert.Equal(60, recovered.Value.progressWaitTimeMs);
        Assert.NotNull(recovered.Warning);
        Assert.Single(Directory.GetFiles(root, "settings.xml.corrupt-*"));
    }

    [Fact]
    public void AbandonedTemporaryFileDoesNotReplaceGoodSettings() {
        var store = new SettingsStore(SettingsPath);
        store.Save(new Settings { DarkMode = true });
        File.WriteAllText(SettingsPath + ".tmp-abandoned", "half written XML");
        Assert.True(store.Load().Value.DarkMode);
        Assert.Null(store.Load().Warning);
    }

    [Fact]
    public void FailedReplacementLeavesExistingSettingsIntact() {
        var store = new SettingsStore(SettingsPath);
        store.Save(new Settings { DarkMode = true });
        using (var locked = new FileStream(SettingsPath, FileMode.Open, FileAccess.Read, FileShare.None))
            Assert.ThrowsAny<IOException>(() => store.Save(new Settings { DarkMode = false }));
        Assert.True(store.Load().Value.DarkMode);
        Assert.Empty(Directory.GetFiles(root, "settings.xml.tmp-*"));
    }

    [Theory]
    [InlineData(-1, 100, -100, 20)]
    [InlineData(10, 50, 99999, 520)]
    [InlineData(9, 8, 29, 20)]
    public void InvalidSavedValuesAreNormalizedBeforeControlsLoad(int theme, ushort language, int interval, int expected) {
        var original = new Settings { ButtonTheme = theme, lang = language, progressWaitTimeMs = interval };
        using (var stream = File.Create(SettingsPath)) new XmlSerializer(typeof(Settings)).Serialize(stream, original);
        var loaded = new SettingsStore(SettingsPath).Load().Value;
        Assert.Equal(0, loaded.ButtonTheme);
        Assert.Null(loaded.lang);
        Assert.Equal(expected, loaded.progressWaitTimeMs);
    }

    [Fact]
    public void ExternalXmlEntitiesAreRejected() {
        File.WriteAllText(SettingsPath, "<!DOCTYPE Settings [<!ENTITY external SYSTEM 'file:///missing-file'>]><Settings><lastDirectory>&external;</lastDirectory></Settings>");
        Assert.NotNull(new SettingsStore(SettingsPath).Load().Warning);
    }
}
