using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharkLibrary
{
    public static class Utilities
    {
        public static string TrimToAscii(string s)
        {
            return s.ToCharArray().Where(c => c < 127).Aggregate(String.Empty, (current, c) => current + c);
        }
    }
}
