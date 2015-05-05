using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharkLibrary.Api
{
    public class Playlist
    {
        public int PlaylistID { get; set; }
        public int UserID { get; set; }
        public string FName { get; set; }
        public string LName { get; set; }
        public string PlaylistName { get; set; }
        public string TSAdded { get; set; }

        //TODO: lazy load this
        public DateTime Added
        {
            get { return DateTime.Parse(TSAdded); }
        }

        //And this
        public string FullName
        {
            get
            {
                string ret = FName;
                if (ret[ret.Length - 1] != ' ')
                    ret += ' ';
                ret += LName;
                return ret;
            }
        }
    }
}
