using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using SharkPhone.Models;

namespace SharkPhone.Controls
{
    public class BusyUserControl : UserControl
    {
        public event EventHandler<string> BusyStarting;
        public event EventHandler BusyFinished;

        protected virtual void OnBusyStarting(string message = null)
        {
            if (BusyStarting != null)
                BusyStarting(this, message);
        }

        protected virtual void OnBusyFinished()
        {
            if (BusyFinished != null)
                BusyFinished(this, new EventArgs());
        }
    }
}
