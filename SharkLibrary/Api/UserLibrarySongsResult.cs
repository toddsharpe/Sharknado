using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharkLibrary.Api
{
    public class UserLibrarySongsResult
    {
        public bool success { get; set; }
        public int libraryTSModified { get; set; }
        public int songsAdded { get; set; }
    }
}
