using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharkLibrary
{
    public class HttpResponseException : Exception
    {
        public HttpResponseException() : base()
        {
            
        }

        public HttpResponseException(string message) : base(message)
        {
            
        }
    }
}
