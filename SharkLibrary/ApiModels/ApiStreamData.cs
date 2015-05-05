namespace SharkLibrary.ApiModels
{
    public class ApiStreamData
    {
        public string FileID { get; set; }
        public string uSecs { get; set; }
        public string FileToken { get; set; }
        public int ts { get; set; }
        public bool isMobile { get; set; }
        public int SongID { get; set; }
        public string streamKey { get; set; }
        public int Expires { get; set; }
        public int streamServerID { get; set; }
        public string ip { get; set; }
    }
}
