using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

namespace SharkPhone.Controls
{
    public partial class AboutControl : UserControl
    {
        private const string WebsiteUrl = "http://www.SharpeCoding.com";
        private const string TwitterUrl = "http://www.twitter.com/SharpeCoding";
        
        public AboutControl()
        {
            InitializeComponent();


            if (!AdManager.ShowAds)
            {
                GetProStackPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void AboutControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            //Display Version
            Assembly assembly = Assembly.GetExecutingAssembly();
            string versionPart = assembly.FullName.Split(',')[1];
            string version = versionPart.Split('=')[1];
            VersionTextBlock.Text = version;
        }

        private void TwitterButton_OnClick(object sender, RoutedEventArgs e)
        {
            WebBrowserTask task = new WebBrowserTask { Uri = new Uri(TwitterUrl) };
            task.Show();
        }

        private void WebsiteButton_OnClick(object sender, RoutedEventArgs e)
        {
            WebBrowserTask task = new WebBrowserTask { Uri = new Uri(WebsiteUrl) };
            task.Show();
        }

        private void SharkPhoneProButton_OnClick(object sender, RoutedEventArgs e)
        {
            MarketplaceDetailTask marketplaceDetailTask = new MarketplaceDetailTask
            {
                ContentIdentifier = "b0af6d92-4b68-420b-b6af-0caebe446230",
                ContentType = MarketplaceContentType.Applications
            };

            marketplaceDetailTask.Show();
        }
    }
}
