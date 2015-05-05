using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace SharkLibrary.Api
{
    public class GetPlaylistResult
    {
        public string PlaylistName { get; set; }
        public int TSModified { get; set; }
        public int UserID { get; set; }

        [JsonConverter(typeof(TrimmingConverter))]
        public string PlaylistDescription { get; set; }
        public string CoverArtFilename { get; set; }
        public List<Song> Songs { get; set; }
    }
}
