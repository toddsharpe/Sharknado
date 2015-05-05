using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharkLibrary.Api
{
    public class GetUserLibraryResult
    {
        public bool hasMore { get; set; }
        public int maxSongs { get; set; }
        public int libraryTSModified { get; set; }
        public List<Song> songs { get; set; }
    }
}
