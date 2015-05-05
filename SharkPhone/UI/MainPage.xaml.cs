using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.BackgroundAudio;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using SharkLibrary;
using SharkLibrary.Api;
using SharkLibrary.Models;
using SharkPhone.Controls;
using SharkPhone.Models;

namespace SharkPhone.UI
{
    public partial class MainPage : PhoneApplicationPage
    {
        public MainPage()
        {
            InitializeComponent();

            //Wire controls
            SearchControl.OnError += control_OnError;
            SearchControl.BusyStarting += BusyUserControl_OnBusyStarting;
            SearchControl.BusyFinished += BusyUserControl_OnBusyFinished;
            SearchControl.OnSearch += SearchControl_OnSearch;
            SearchHistoryControl.SearchControl = SearchControl;

            LoginControl.OnLogin += (sender, error) =>
            {
                LoginControl.VerticalAlignment = VerticalAlignment.Bottom;
                AccountLinkStackPanel.Visibility = Visibility.Visible;
            };
            LoginControl.OnLogout += (sender, error) =>
            {
                LoginControl.VerticalAlignment = VerticalAlignment.Top;
                AccountLinkStackPanel.Visibility = Visibility.Collapsed;
            };
            LoginControl.BusyStarting += BusyUserControl_OnBusyStarting;
            LoginControl.BusyFinished += BusyUserControl_OnBusyFinished;

            var control = AdManager.MakeAdControl();
            AdBorder.Child = control;
        }

        void SearchControl_OnSearch(object sender, Search e)
        {
            if (Panorama.SelectedIndex != 0)
                Panorama.SlideToPage(0);
        }

        private void BusyUserControl_OnBusyStarting(object sender, string message)
        {
            SystemTray.ProgressIndicator.IsIndeterminate = true;
            SystemTray.ProgressIndicator.Text = message;
            SystemTray.IsVisible = true;
        }

        private void BusyUserControl_OnBusyFinished(object sender, EventArgs e)
        {
            SystemTray.IsVisible = false;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (App.ShowNowPlaying)
            {
                Panorama.DefaultItem = Panorama.Items[2];
                App.ShowNowPlaying = false;
            }
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (Panorama.SelectedItem != Panorama.Items[0])
            {
                Panorama.SlideToPage(0);
                e.Cancel = true;
            }

            base.OnBackKeyPress(e);
        }

        void control_OnError(object sender, ErrorEventArgs error)
        {
            MessageBox.Show(error.Message);
        }

        private async void MainPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (App.Api.HasLoaded)
                return;

            try
            {
                await App.Api.StartSessionAsync();
                await App.Api.GetCountryAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to connect to Grooveshark." + Environment.NewLine + "Check data connection and try again." + Environment.NewLine + ex.Message, "Unable to connect", MessageBoxButton.OK);
                Application.Current.Terminate();
            }

            App.LoadPlaybackAgent();

            //enable
            Panorama.IsEnabled = true;

            Credentials credentials = App.GetCredentials();
            if (credentials != null)
                await LoginControl.LoginAsync(credentials.Username, credentials.Password);

            SystemTray.IsVisible = false;
        }

        private void PlaylistsButton_OnClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/UI/UserPlaylistsPage.xaml", UriKind.Relative));
        }

        private void FavoritesButton_OnClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/UI/UserFavoritesPage.xaml", UriKind.Relative));
        }

        private void SearchControl_OnOnItemSelected(object sender, EventArgs error)
        {
            object selected = ((SearchControl) sender).SelectedItem;

            if (selected is Song)
            {
                App.SelectedSong = (Song)selected;
                NavigationService.Navigate(new Uri("/UI/SongPage.xaml", UriKind.Relative));
            }
            else if (selected is Album)
            {
                Album album = (Album)selected;
                NavigationService.Navigate(new Uri("/UI/AlbumPage.xaml?AlbumID=" + album.AlbumID, UriKind.Relative));
            }
            else if (selected is Playlist)
            {
                Playlist playlist = (Playlist)selected;
                NavigationService.Navigate(new Uri("/UI/PlaylistPage.xaml?PlaylistID=" + playlist.PlaylistID, UriKind.Relative));
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}