using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharkLibrary.Api
{
    public class RegisterUserResult
    {
        public int UserID { get; set; }
        public bool IsPremium { get; set; }
        public string Username { get; set; }
        public bool IsPlus { get; set; }
        public bool IsAnywhere { get; set; }
        public string emailAddress { get; set; }
        public string description { get; set; }
        public int errorCode { get; set; }
    }
}
