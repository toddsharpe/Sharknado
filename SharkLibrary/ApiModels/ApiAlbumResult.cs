using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharkLibrary.ApiModels
{
    public class ApiAlbumResult
    {
        public string AlbumID { get; set; }
        public string AlbumNameID { get; set; }
        public string Name { get; set; }
        public string ArtistID { get; set; }
        public string Year { get; set; }
        public string CoverArtFilename { get; set; }
        public string ArtistName { get; set; }
        public string IsVerified { get; set; }
        public string GenreID { get; set; }
        public string ReleaseType { get; set; }
    }
}
