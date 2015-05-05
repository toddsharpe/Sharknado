namespace SharkLibrary.ApiModels
{
    public class ApiHeader
    {
        public string session { get; set; }
        public string serviceVersion { get; set; }
        public bool prefetchEnabled { get; set; }
    }
}
