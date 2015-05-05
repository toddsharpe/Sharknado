using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharkLibrary.Api
{
    public class AlbumListResult
    {
        public Pager pager { get; set; }
        public List<Album> albums { get; set; }
    }
}
