using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharkLibrary.Api;

namespace SharkLibrary.Models
{
    public class OfflineManager
    {
        private List<OfflineSong> _songs;
        private List<Playlist> _playlists;//Holds saved playlists, used to find songs

        public OfflineManager()
        {
            _songs = new List<OfflineSong>();
            _playlists = new List<Playlist>();
        }

        public bool IsOffline(int playlistId)
        {
            return _playlists.SingleOrDefault(i => i.PlaylistID == playlistId) != null;
        }

        public void SavePlaylist(Playlist playlist)
        {
            
        }
    }
}
