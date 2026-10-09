namespace AdbFileManager.Core.Browsing;

internal sealed class LatestBrowserRequest : IDisposable
{
    private CancellationTokenSource? active;
    internal CancellationToken Start()
    {
        Cancel();
        active = new CancellationTokenSource();
        return active.Token;
    }
    internal bool IsCurrent(CancellationToken token) => active != null && active.Token == token && !token.IsCancellationRequested;
    internal void Cancel()
    {
        var previous = active;
        active = null;
        if (previous == null)
            return;
        previous.Cancel();
        previous.Dispose();
    }
    public void Dispose() => Cancel();
}
