namespace AdbFileManager.Infrastructure.Adb;

internal static class AndroidDeviceParser
{
    internal static IReadOnlyList<AndroidDevice> Parse(string output)
    {
        var devices = new List<AndroidDevice>();
        foreach (string line in output.Split('\n'))
        {
            string[] fields = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (fields.Length < 2 || fields[0] == "List" || fields[0] == "*")
                continue;
            DeviceState? state = fields[1] switch
            {
                "device" => DeviceState.Ready,
                "offline" => DeviceState.Offline,
                "unauthorized" => DeviceState.Unauthorized,
                "recovery" => DeviceState.Recovery,
                "sideload" => DeviceState.Sideload,
                _ => null
            };
            if (!state.HasValue)
                continue;
            string model = fields.FirstOrDefault(field => field.StartsWith("model:"))?[6..] ?? fields[0];
            devices.Add(new(fields[0], state.Value, model));
        }
        return devices;
    }
}
