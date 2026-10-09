namespace AdbFileManager.Views.Transfers
{
    internal static class QueueText
    {
        internal static string Get(string key) => LocalizationText.Get("queue_" + key) ?? key;
        internal static string Summary(QueueSummary summary) => string.Format(Get("summary"),
            summary.Completed, summary.Failed, summary.Skipped, summary.Cancelled, summary.Pending);
    }
}
