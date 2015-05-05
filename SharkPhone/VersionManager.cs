using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharkPhone
{
    //If i could bind better, I wouldn't need this
    public class VersionManager
    {
#if PAID_VERSION
        private const string _appName = "SharkPhone Pro";
#else
        private const string _appName = "SharkPhone";
#endif

        public static string AppName
        {
            get { return _appName; }
        }
    }
}
