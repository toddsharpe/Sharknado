namespace SharkLibrary.ApiModels
{
    public class ApiResult<T>
    {
        public ApiHeader Header { get; set; }
        public ApiFault Fault { get; set; }
        public T Result { get; set; }
    }
}
