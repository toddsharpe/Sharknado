using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace SharkLibrary.Api
{
    public class Country
    {
        public ulong ID { get; set; }
        public ulong CC1 { get; set; }
        public ulong CC2 { get; set; }
        public ulong CC3 { get; set; }
        public ulong CC4 { get; set; }
        public ulong DMA { get; set; }
        public ulong IPR { get; set; }
    }
}
