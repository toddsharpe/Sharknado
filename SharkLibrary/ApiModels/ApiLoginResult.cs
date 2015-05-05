using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharkLibrary.ApiModels
{
    public class ApiLoginResult
    {
        public int userID { get; set; }
        public string FName { get; set; }
        public string LName { get; set; }
        public string isPremium { get; set; }
        public bool autoAutoplay { get; set; }
        public int authRealm { get; set; }
        public int favoritesLimit { get; set; }
        public int librarySizeLimit { get; set; }
        public int uploadsEnabled { get; set; }
        public string themeID { get; set; }
        public string authToken { get; set; }
        public bool badAuthToken { get; set; }
        public int Privacy { get; set; }
        public string Sex { get; set; }
        public string TSDOB { get; set; }
        public int flags { get; set; }
        public string Email { get; set; }
        public object City { get; set; }
        public object Country { get; set; }
        public object Picture { get; set; }
        public object State { get; set; }
        public string TSAdded { get; set; }
        public long userTrackingID { get; set; }
        public UserPrivacyTokens userPrivacyTokens { get; set; }
        public object Zip { get; set; }
        public object About { get; set; }
        public string NotificationEmailPrefs { get; set; }

        public bool IsValid()
        {
            return userID != 0;
        }
    }
}
