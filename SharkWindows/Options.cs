using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SharkWindows
{
    public class Options
    {
        private const string FileName = "SharkOptions.xml";
        
        public enum SaveOption
        {
            GroovesharkFolder, Organized
        }
        
        public string MusicFolder { get; set; }
        public bool UseGroovesharkFolder { get; set; }

        public static readonly Options Default = new Options
            {
                MusicFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
                UseGroovesharkFolder = true
            };

        public void Save()
        {
            using (Stream stream = new FileStream(FileName, FileMode.Create))
            {
                using (TextWriter writer = new StreamWriter(stream))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Options));
                    serializer.Serialize(writer, this);
                }
            }
        }

        public static Options Load()
        {
            if (!File.Exists(FileName))
            {
                return Default;
            }

            using (Stream stream = new FileStream(FileName, FileMode.Open))
            {
                using (TextReader reader = new StreamReader(stream))
                {
                    XmlSerializer deserializer = new XmlSerializer(typeof(Options));
                    return (Options)deserializer.Deserialize(reader);
                }
            }
        }
    }
}
