using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharkLibrary.Api
{
    public class AuthenticateResult
    {
        public int UserID { get; set; }
        public string Email { get; set; }
        public string FName { get; set; }
        public string LName { get; set; }
        public string IsPlus { get; set; }
        public string IsAnywhere { get; set; }
        public string IsPremium { get; set; }
        public bool success { get; set; }
    }
}
