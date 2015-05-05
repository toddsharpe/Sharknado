using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SharkLibrary.ApiModels;

namespace SharkLibrary
{
    public interface IPlatformAdaptor
    {
        string ComputeSha1Hash(string buffer);

        void SaveStream(ApiSong song, Stream stream);
    }
}
