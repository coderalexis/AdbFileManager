namespace AdbFileManager.Core.Browsing;

public sealed record AndroidBreadcrumb(string Label, string Path)
{
    public static IReadOnlyList<AndroidBreadcrumb> Build(string path)
    {
        var result = new List<AndroidBreadcrumb> { new("/", "/") };
        string current = "";
        foreach (string segment in path.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            current += "/" + segment;
            result.Add(new(segment, current + "/"));
        }
        return result;
    }
}
