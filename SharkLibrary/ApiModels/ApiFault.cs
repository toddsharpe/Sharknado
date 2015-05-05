using System;

namespace SharkLibrary.ApiModels
{
    public class ApiFault
    {
        public int code { get; set; }
        public string message { get; set; }

        private Exception _exception;
        public Exception Exception
        {
            get
            {
                return _exception ?? (_exception = new Exception("Api Fault - Code: " + code + " Message: " + message));
            }
        }
    }
}
