using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharkLibrary.Api
{
    public class StreamKeyResult
    {
        public string StreamKey { get; set; }
        public string url { get; set; }
        public int StreamServerID { get; set; }
        public int uSecs { get; set; }
    }
}
