using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace SharkLibrary.ApiModels
{
    public class ApiSong : INotifyPropertyChanged
    {
        //http://json2csharp.com/
        public string SongID { get; set; }
        public string AlbumID { get; set; }
        public string ArtistID { get; set; }
        public string SongName { get; set; }
        public string AlbumName { get; set; }
        public string ArtistName { get; set; }
        public string Year { get; set; }
        public string TrackNum { get; set; }
        public string CoverArtFilename { get; set; }
        public string ArtistCoverArtFilename { get; set; }
        public string TSAdded { get; set; }
        public string AvgRating { get; set; }
        public string AvgDuration { get; set; }
        public string EstimateDuration { get; set; }
        public int Flags { get; set; }
        public string IsLowBitrateAvailable { get; set; }
        public string IsVerified { get; set; }
        public int Popularity { get; set; }
        public double Score { get; set; }
        public int RawScore { get; set; }
        public int PopularityIndex { get; set; }

        private TimeSpan _duration;
        public TimeSpan Duration
        {
            get
            {
                double seconds = double.Parse(EstimateDuration);
                return _duration != TimeSpan.Zero ? _duration : (_duration = TimeSpan.FromSeconds(seconds));
            }
        }

        private int _downloadProgress;

        public int DownloadProgress
        {
            get { return _downloadProgress; }
            set
            {
                if (_downloadProgress != value)
                {
                    _downloadProgress = value;
                    OnPropertyChanged("DownloadProgress");
                }
            }
        }

        private SongDownloadStatus _downloadStatus;
        public SongDownloadStatus DownloadStatus
        {
            get { return _downloadStatus; }
            set
            {
                if (_downloadStatus != value)
                {
                    _downloadStatus = value;
                    OnPropertyChanged("DownloadStatus");
                }
            }
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

        private string _artistAlbum;

        public string ArtistAlbum
        {
            get { return _artistAlbum ?? (_artistAlbum = ArtistName + " - " + AlbumName); }
        }

        public ApiSong()
        {
            DownloadStatus = SongDownloadStatus.None;
        }

        public string GetSongFolder()
        {
            //Use properties
            string folder = this.ArtistName + "\\" + this.AlbumName;

            return Utilities.TrimToAscii(folder);
        }

        public string GetSongFileName()
        {
            string fileName = this.TrackNum + " - " + this.SongName + ".mp3";

            return Utilities.TrimToAscii(fileName);
        }

        public override string ToString()
        {
            return ArtistName + " - " + SongName;
        }

        public enum SongDownloadStatus
        {
            None,
            Queued,
            Loading,
            InProgress,
            Finished,
            Invalid,
            InLibrary
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}