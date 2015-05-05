using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace SharkLibrary.ApiModels
{
    //There is literally ZERO reason for this class to exist except Grooveshark's APIs literally suck. Like hard. ApiSong uses SongName and this fucking thing uses Name. Way to go.
    public class ApiAlbumSong : INotifyPropertyChanged
    {
        public string SongID { get; set; }
        public string Name { get; set; }
        public string TrackNum { get; set; }
        public string IsVerified { get; set; }
        public string Popularity { get; set; }
        public string IsLowBitrateAvailable { get; set; }
        public string Flags { get; set; }
        public string EstimateDuration { get; set; }
        public string AlbumID { get; set; }
        public string AlbumName { get; set; }
        public string CoverArtFilename { get; set; }
        public string ArtistID { get; set; }
        public string ArtistName { get; set; }
        public string TSFavorited { get; set; }

        private string _artistAlbum;

        public string ArtistAlbum
        {
            get { return _artistAlbum ?? (_artistAlbum = ArtistName + " - " + AlbumName); }
        }

        private string _thumbUrl;
        public string ThumbUrl
        {
            get { return _thumbUrl; }
            set
            {
                if (_thumbUrl != value)
                {
                    _thumbUrl = value;
                    OnPropertyChanged("ThumbUrl");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public static ApiAlbumSong FromFavoriteSong(ApiFavoriteSong song)
        {
            return new ApiAlbumSong
            {
                Name = song.name,
                SongID = song.songID.ToString(),
                Flags = "0",
                ArtistID = song.artistID.ToString(),
                ArtistName = song.artistName,
                AlbumID = song.albumID.ToString(),
                AlbumName = song.albumName,
                CoverArtFilename = song.coverArtFilename,
                TSFavorited = String.Empty, //If i gave a shit, I could do a Utc.Now and covert. But alas, I don't
                IsLowBitrateAvailable = "1", //Sure, why wouldn't it be?
                EstimateDuration = song.avgDuration.Split('.')[0], //Can't wait for this to fail
                IsVerified = song.isVerified,
                Popularity = song.popularityIndex.ToString(),
                TrackNum = song.trackNum.ToString()
            };
        }
    }
}
