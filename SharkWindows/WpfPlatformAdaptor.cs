using System.IO;
using SharkLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SharkLibrary.ApiModels;

namespace SharkWindows
{
    internal class WpfPlatformAdaptor : IPlatformAdaptor
    {
        private const string DefaultFolder = "Grooveshark";
        
        public string ComputeSha1Hash(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            using (var hasher = SHA1.Create())
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
            if (App.Options.UseGroovesharkFolder)
                fileName = Path.Combine(DefaultFolder, fileName);
            return Path.Combine(App.Options.MusicFolder, fileName);
        }
    }
}
