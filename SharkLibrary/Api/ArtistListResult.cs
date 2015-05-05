using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharkLibrary.Api
{
    public class ArtistListResult
    {
        public Pager pager { get; set; }
        public List<Artist> artists { get; set; }
    }
}
