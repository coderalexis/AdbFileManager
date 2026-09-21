namespace AdbFileManager.Transfers {
    internal static class QueueText {
        internal static string Get(string key) => Form1.rm.GetString("queue_" + key) ?? key;
        internal static string Summary(QueueSummary summary) => string.Format(Get("summary"),
            summary.Completed, summary.Failed, summary.Skipped, summary.Cancelled, summary.Pending);
    }
}
