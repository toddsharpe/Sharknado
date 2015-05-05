using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharkLibrary.Api;

namespace SharkLibrary.Models
{
    public class OfflineSong : Song
    {
        public string LocalFile { get; set; }
    }
}
