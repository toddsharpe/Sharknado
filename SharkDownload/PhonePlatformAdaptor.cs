using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SharkLibrary;
using SharkLibrary.ApiModels;

namespace SharkDownload
{
    class PhonePlatformAdaptor : IPlatformAdaptor
    {
        private const string DefaultFolder = "Music";

        public string ComputeSha1Hash(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            using (var hasher = new SHA1Managed())
            {
                byte[] hash = hasher.ComputeHash(buffer);
                return hash.Select(i => i.ToString("x2")).Aggregate((a, b) => a + b);
            }
        }

        public void SaveStream(ApiSong song, Stream stream)
        {
            string fullPath = GetSavePath(song);

            string directory = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            using (stream)
            using (StreamWriter writer = new StreamWriter(fullPath))
            {
                stream.CopyTo(writer.BaseStream);
            }
        }

        public string GetSavePath(ApiSong song)
        {
            string fileName = Path.Combine(song.GetSongFolder(), song.GetSongFileName());
            return Path.Combine(DefaultFolder, fileName);
        }
    }
}
