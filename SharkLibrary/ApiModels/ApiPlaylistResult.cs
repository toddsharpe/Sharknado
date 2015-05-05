using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharkLibrary.ApiModels
{
    public class ApiPlaylistResult
    {
        public string UUID { get; set; }
        public string TSAdded { get; set; }
        public string About { get; set; }
        public int SubscriberCount { get; set; }
        public string Picture { get; set; }
        public int TSModified { get; set; }
        public string Name { get; set; }
        public int PlaylistID { get; set; }
        public int UserID { get; set; }
        public List<string> AlbumFiles { get; set; }
        public List<ApiAlbumSong> Songs { get; set; }
        public bool tooBig { get; set; }
        public string FName { get; set; }
        public string LName { get; set; }
        public string Username { get; set; }
    }
}
