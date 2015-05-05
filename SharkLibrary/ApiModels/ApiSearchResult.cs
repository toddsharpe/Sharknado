using System.Collections.Generic;

namespace SharkLibrary.ApiModels
{
    public class ApiSearchResult
    {
        public List<ApiSong> Songs { get; set; }
        public List<ApiPlaylist> Playlists { get; set; }
        public List<ApiAlbum> Albums { get; set; }
    }
}
