using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharkLibrary.Api
{
    public class Album : BaseModel
    {
        public int AlbumID { get; set; }
        public string AlbumName { get; set; }
        public int ArtistID { get; set; }
        public string ArtistName { get; set; }
        //public string CoverArtFilename { get; set; }
        public bool IsVerified { get; set; }
    }
}
