using System.ComponentModel;

namespace SharkLibrary.ApiModels
{
    public class ApiAlbum : INotifyPropertyChanged
    {
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
        public string Name { get; set; }

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
    }
}
