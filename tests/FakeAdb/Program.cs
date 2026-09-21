using System.Text.Json;

namespace AdbFileManager.FakeAdb;

public static class Program {
    public static async Task<int> Main(string[] args) {
        Console.OutputEncoding = new System.Text.UTF8Encoding(false);
        if (args[0] == "-s") {
            args = args.Skip(2).ToArray();
            if (args[0] == "pull") {
                string source = args[^2];
                string destination = Path.Combine(args[^1], Path.GetFileName(source));
                if (Directory.Exists(source)) CopyDirectory(source, destination);
                else File.Copy(source, destination);
                if (Path.GetFileName(source).StartsWith("fail")) {
                    Console.Error.WriteLine("Permission denied (simulated)");
                    return 1;
                }
                return 0;
            }
        }
        switch (args[0]) {
            case "version":
                Console.WriteLine("Android Debug Bridge version 1.0.41\nVersion 37.0.1-test");
                return 0;
            case "devices":
                Console.WriteLine("List of devices attached\nsimulated-device\tdevice");
                return 0;
            case "success":
                await Console.Error.WriteAsync("[ 12");
                await Console.Error.FlushAsync();
                await Task.Delay(50);
                await Console.Error.WriteAsync("%] file\r[100%] file");
                return 0;
            case "failure":
                await Task.WhenAll(
                    Console.Out.WriteAsync(new string('o', 100000)),
                    Console.Error.WriteAsync(new string('e', 100000)));
                await Console.Error.WriteAsync("\nPermission denied: teléfono\n");
                return 7;
            case "wait":
                await File.WriteAllTextAsync(args[1], Environment.ProcessId.ToString());
                await Console.Error.WriteAsync("[1%] ready\n");
                await Console.Error.FlushAsync();
                await Task.Delay(Timeout.Infinite);
                return 0;
            case "record":
                await File.WriteAllTextAsync(args[1], JsonSerializer.Serialize(args.Skip(2)));
                return 0;
            default:
                return 1;
        }
    }
    private static void CopyDirectory(string source, string destination) {
        Directory.CreateDirectory(destination);
        foreach (string file in Directory.GetFiles(source)) File.Copy(file, Path.Combine(destination, Path.GetFileName(file)));
        foreach (string directory in Directory.GetDirectories(source)) CopyDirectory(directory, Path.Combine(destination, Path.GetFileName(directory)));
    }

}
