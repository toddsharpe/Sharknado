using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharkLibrary.Api
{
    public class Pager
    {
        public int numPages { get; set; }
        public bool hasPrevPage { get; set; }
        public bool hasNextPage { get; set; }
    }
}
