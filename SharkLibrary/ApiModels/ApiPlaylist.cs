namespace SharkLibrary.ApiModels
{
    public class ApiPlaylist
    {
        public string PlaylistID { get; set; }
        public string Name { get; set; }
        public string About { get; set; }
        public string UserID { get; set; }
        public string Username { get; set; }
        public string FName { get; set; }
        public string LName { get; set; }
        public string Picture { get; set; }
        public string TSAdded { get; set; }
        public string IsDeleted { get; set; }
        public string Artists { get; set; }
        public string NumArtists { get; set; }
        public string NumSongs { get; set; }
        public double Score { get; set; }
        public int SphinxSortExpr { get; set; }

        //Added from User call
        public string UUID { get; set; }
        public int TSModified { get; set; }
    }
}
