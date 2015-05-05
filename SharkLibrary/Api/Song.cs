using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SharkLibrary.Api
{
    public class Song : BaseModel
    {
        public int SongID { get; set; }
        
        //TODO - determine if we can merge properties
        public string SongName { get; set; }
        public string Name { get; set; }

        
        [XmlIgnore]
        public string TrackNum { get; set; }
        [XmlIgnore]
        public int ArtistID { get; set; }
        public string ArtistName { get; set; }
        [XmlIgnore]
        public int AlbumID { get; set; }
        public string AlbumName { get; set; }
        //public string CoverArtFilename { get; set; }
        [XmlIgnore]
        public string Popularity { get; set; }

        //These properties are returned as strings from a call to getAlbumSongs
        //Someday I'll mark these as objects and do the lifting
        //For now, we can ignore them

        //public bool IsLowBitrateAvailable { get; set; }
        //public bool IsVerified { get; set; }
        //public int Flags { get; set; }


        [XmlIgnore]
        public string TSAdded { get; set; }

        private string _artistAlbum;

        public string ArtistAlbum
        {
            get { return _artistAlbum ?? (_artistAlbum = ArtistName + " - " + AlbumName); }
        }
    }
}
