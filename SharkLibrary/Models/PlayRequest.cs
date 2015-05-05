using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharkLibrary.Api;

namespace SharkLibrary.Models
{
    public class PlayRequest
    {
        public PlayRequestType Type { get; set; }
        public int ObjectID { get; set; }
        public int Position { get; set; }

        public Song Song { get; set; }

        //Api specifics
        public string SessionId { get; set; }
        public Country Country { get; set; }

        //For XML
        public PlayRequest()
        {
            
        }
    }
}
