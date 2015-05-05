using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Phone.Reactive;

namespace SharkPhone.Models
{
    public class EventArgs<T> : EventArgs
    {
        public T Result { get; private set; }

        public EventArgs(T result) : base()
        {
            Result = result;
        }
    }
}
