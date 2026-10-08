using System.Globalization;
using AdbFileManager.Transfers;
using Xunit;

namespace AdbFileManager.Tests;

public sealed class StatisticsTests {
    [Theory]
    [InlineData("1 file pulled. 50.1 MB/s (1048576 bytes in 0.020s)", 1048576)]
    [InlineData("151 files pushed. (5450000000 bytes in 2100.50s)", 5450000000)]
    [InlineData("1 file pulled. (0 bytes in 0.000s)", 0)]
    public void SummaryBytesAreParsedIndependentlyOfCulture(string output, long expected) {
        var original = CultureInfo.CurrentCulture;
        try {
            CultureInfo.CurrentCulture = new("es-MX");
            var statistics = TransferStatistics.FromOutput(output, TimeSpan.FromSeconds(2));
            Assert.Equal(expected, statistics.Bytes);
            Assert.Equal(2, statistics.Seconds);
        }
        finally { CultureInfo.CurrentCulture = original; }
    }

    [Theory]
    [InlineData("[50%] file.bin")]
    [InlineData("completed without a summary")]
    [InlineData("(999999999999999999999999 bytes in 1.0s)")]
    public void MissingSummaryNeverInventsBytesOrAverageSpeed(string output) {
        var statistics = TransferStatistics.FromOutput(output, TimeSpan.FromSeconds(2));
        Assert.Null(statistics.Bytes);
        var job = new TransferJob { State = TransferState.Completed, TransferSeconds = statistics.Seconds, TransferredBytes = statistics.Bytes };
        Assert.Null(job.AverageMiBPerSecond);
    }

    [Fact]
    public void AverageIsComputedInMebibytesPerSecondOnlyForSuccessfulJobs() {
        var job = new TransferJob { TransferredBytes = 10485760, TransferSeconds = 2 };
        Assert.Null(job.AverageMiBPerSecond);
        job.State = TransferState.Completed;
        Assert.Equal(5, job.AverageMiBPerSecond);
        job.State = TransferState.Failed;
        Assert.Null(job.AverageMiBPerSecond);
    }

    [Fact]
    public void CompletedStatisticsSurviveQueuePersistence() {
        string path = Path.GetTempFileName();
        try {
            var job = new TransferJob { DeviceId = "phone", Source = "/sdcard/file", Destination = @"C:\Backup\file",
                State = TransferState.Completed, TransferredBytes = 10485760, TransferSeconds = 2, ElapsedSeconds = 3 };
            var store = new QueueStore(path);
            store.Save(new[] { job });
            var restored = Assert.Single(store.Load());
            Assert.Equal(5, restored.AverageMiBPerSecond);
            Assert.Equal(TimeSpan.FromSeconds(3), restored.Elapsed);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void DamagedStatisticsCannotCrashDurationRendering() {
        string path = Path.GetTempFileName();
        try {
            var job = new TransferJob { DeviceId = "phone", Source = "/file", Destination = "/copy", ElapsedSeconds = double.MaxValue };
            var store = new QueueStore(path);
            store.Save(new[] { job });
            Assert.Throws<InvalidDataException>(() => store.Load());
        }
        finally { File.Delete(path); }
    }
}
