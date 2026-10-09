using System.Xml;
using System.Xml.Serialization;

namespace AdbFileManager.Infrastructure.Persistence
{

    public sealed class SettingsStore : ISettingsStore
    {
        private static readonly XmlSerializer Serializer = new(typeof(Settings));
        private readonly string path;
        private bool savingBlocked;
        public SettingsStore(string path)
        {
            this.path = Path.GetFullPath(path);
        }

        public SettingsLoadResult Load()
        {
            savingBlocked = false;
            string? warning = null;
            if (System.IO.File.Exists(path))
            {
                try
                {
                    return new(Read(path), null);
                }
                catch (Exception ex) when (IsReadFailure(ex))
                {
                    warning = ex.Message;
                    try
                    {
                        string damaged = path + ".corrupt-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + "-" + Guid.NewGuid().ToString("N");
                        System.IO.File.Move(path, damaged);
                    }
                    catch (Exception preserveError) when (preserveError is IOException or UnauthorizedAccessException)
                    {
                        savingBlocked = true;
                        warning += Environment.NewLine + preserveError.Message;
                    }
                }
            }
            string backup = path + ".bak";
            if (System.IO.File.Exists(backup))
            {
                try
                {
                    return new(Read(backup), "Backup restored. " + warning);
                }
                catch (Exception ex) when (IsReadFailure(ex)) { warning = (warning ?? "") + Environment.NewLine + ex.Message; }
            }
            return new(new Settings(), warning);
        }

        public void Save(Settings value)
        {
            if (savingBlocked)
                throw new IOException("The damaged settings file could not be preserved. Check permissions before saving.");
            SettingsValidator.Normalize(value);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            string temporary = path + ".tmp-" + Guid.NewGuid().ToString("N");
            try
            {
                using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    Serializer.Serialize(stream, value);
                    stream.Flush(true);
                }
                if (System.IO.File.Exists(path))
                    System.IO.File.Replace(temporary, path, path + ".bak");
                else
                    System.IO.File.Move(temporary, path);
            }
            finally
            {
                if (System.IO.File.Exists(temporary))
                    System.IO.File.Delete(temporary);
            }
        }

        private static Settings Read(string file)
        {
            using var stream = System.IO.File.OpenRead(file);
            using var reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit });
            var value = Serializer.Deserialize(reader) as Settings ?? throw new InvalidDataException("Empty settings file.");
            SettingsValidator.Normalize(value);
            return value;
        }

        private static bool IsReadFailure(Exception ex) => ex is InvalidOperationException or IOException or UnauthorizedAccessException or XmlException;

    }
}
