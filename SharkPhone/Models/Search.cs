using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharkPhone.Models
{
    public class Search
    {
        public string Query { get; set; }
        public string Type { get; set; }

        public override int GetHashCode()
        {
            return Query.GetHashCode() ^ Type.GetHashCode();
        }
        
        public override bool Equals(object obj)
        {
            if (!(obj is Search))
                return false;

            Search search = obj as Search;
            return (this.Query == search.Query) && (this.Type == search.Type);
        }
    }
}
