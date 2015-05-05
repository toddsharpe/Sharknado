using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharkPhone.Models
{
    public class ErrorEventArgs : EventArgs
    {
        public string Message { get; private set; }
        public Exception Exception { get; set; }

        public ErrorEventArgs(string message, Exception ex = null) : base()
        {
            Message = message;
            Exception = ex;
        }
    }
}
