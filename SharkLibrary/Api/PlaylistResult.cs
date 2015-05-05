using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharkLibrary.Api
{
    public class PlaylistResult
    {
        public bool success { get; set; }
        public int playlistsTSModified { get; set; }
        public int playlistID { get; set; }
    }
}
