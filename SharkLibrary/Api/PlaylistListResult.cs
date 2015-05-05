using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharkLibrary.Api
{
    public class PlaylistListResult
    {
        public Pager pager { get; set; }
        public List<Playlist> playlists { get; set; }
    }
}
