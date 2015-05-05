using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace SharkLibrary.ApiModels
{
    public class ApiFavoriteSong : INotifyPropertyChanged
    {
        public int songID { get; set; }
        public int albumID { get; set; }
        public int artistID { get; set; }
        public string songName { get; set; }
        public string albumName { get; set; }
        public string artistName { get; set; }
        public int trackNum { get; set; }
        public string coverArtFilename { get; set; }
        public string artistCoverArtFilename { get; set; }
        public string avgRating { get; set; }
        public string avgDuration { get; set; }
        public string isVerified { get; set; }
        public double score { get; set; }
        public int popularityIndex { get; set; }
        public string name { get; set; }

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

        private string _artistAlbum;

        public string ArtistAlbum
        {
            get { return _artistAlbum ?? (_artistAlbum = artistName + " - " + albumName); }
        }

        //Yea, this is right, songName and name are equal to the same thing and both sent. Grooveshark, you've outdone yourself
        public static ApiFavoriteSong FromApiSong(ApiSong song)
        {
            return new ApiFavoriteSong
            {
                songID = int.Parse(song.SongID),
                albumID = int.Parse(song.AlbumID),
                artistID = int.Parse(song.ArtistID),
                songName = song.SongName,
                albumName = song.AlbumName,
                artistName = song.ArtistName,
                trackNum = int.Parse(song.TrackNum),
                coverArtFilename = song.CoverArtFilename,
                artistCoverArtFilename = song.ArtistCoverArtFilename,
                avgRating = song.AvgRating,
                avgDuration = song.AvgDuration,
                isVerified = song.IsVerified,
                score = song.Score,
                popularityIndex = song.PopularityIndex,
                name = song.SongName
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
