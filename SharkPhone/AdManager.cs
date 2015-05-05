using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Windows.ApplicationModel.Store;
using Microsoft.Advertising.Mobile;
using Microsoft.Advertising;
using Microsoft.Advertising.Mobile.UI;

namespace SharkPhone
{
    public class AdManager
    {

#if DEBUG
    public const string AdUnitId = "Image480_80";
    public const string ApplicationId = "test_client";
#else
    public const string AdUnitId = "<AdUnitId>";
    public const string ApplicationId = "<ApplicationId>";
#endif

        private const string AdDuplexAppId = "<AdDuplexAppId>";

        public const string ProductId = "NoAds";
        
        public static bool ShowAds { get; set; }

        public static void UpdateInAppPurchases()
        {
#if PAID_VERSION
            ShowAds = false;
            return;
#else
            ShowAds = true;
#endif

            var allLicenses = CurrentApp.LicenseInformation.ProductLicenses;
            if (allLicenses.ContainsKey(ProductId))
            {
                var license = allLicenses[ProductId];
                if (license.IsActive)
                {
                    ShowAds = false;
                }
            }
        }

        //http://blog.adduplex.com/2011/03/using-adduplex-as-fallback-for.html
        public static UIElement MakeAdControl()
        {
            if (!ShowAds)
                return null;
            
            AdDuplex.AdControl adDuplex = new AdDuplex.AdControl
            {
                Name = "AdDuplexAdControl",
                Height = 80,
                Width = 480,
                Visibility = Visibility.Collapsed,
                AppId = AdDuplexAppId,
                RefreshInterval = 30
            };

            AdControl msAdControl = new AdControl(ApplicationId, AdUnitId, true)
            {
                Name = "MSAdControl",
                Height = 80,
                Width = 480,
                IsAutoCollapseEnabled = true,
                Visibility = Visibility.Visible,
                Tag = adDuplex
            };
            msAdControl.ErrorOccurred += control_ErrorOccurred;
            msAdControl.AdRefreshed += ControlOnAdRefreshed;

            StackPanel stack = new StackPanel();
            stack.Children.Add(msAdControl);
            stack.Children.Add(adDuplex);

            return stack;
        }

        private static void ControlOnAdRefreshed(object sender, EventArgs eventArgs)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                AdControl msAds = sender as AdControl;
                AdDuplex.AdControl adDuplex = msAds.Tag as AdDuplex.AdControl;

                msAds.Visibility = Visibility.Visible;
                adDuplex.Visibility = Visibility.Collapsed;
            });
        }

        static void control_ErrorOccurred(object sender, AdErrorEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                AdControl msAds = sender as AdControl;
                AdDuplex.AdControl adDuplex = msAds.Tag as AdDuplex.AdControl;

                msAds.Visibility = Visibility.Collapsed;
                adDuplex.Visibility = Visibility.Visible;
            });
        }
    }
}
