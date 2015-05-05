using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Phone.BackgroundAudio;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SharkLibrary.Api;
using SharkLibrary.ApiModels;
using SharkLibrary.Models;

namespace SharkPhone.UI
{
    public partial class SongPage : PhoneApplicationPage
    {
        public SongPage()
        {
            InitializeComponent();

            AdBorder.Child = AdManager.MakeAdControl();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (App.SelectedSong == null)
            {
                //TODO: Is this fix legit?
                MessageBox.Show("Error loading song");
                NavigationService.GoBack();
                return;
            }

            DataContext = App.SelectedSong;
            Thumbnail.Source = new BitmapImage(new Uri(App.Api.GetCoverArtImageUrl(App.SelectedSong, CoverArtImageType.Pic500)));
        }

        private void SongPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            //Add our playlists to the list
            if (App.UserPlaylists != null)
            {
                ApplicationBar.MenuItems.Add(new ApplicationBarMenuItem("Add to Playlist...") { IsEnabled = false });

                foreach (Playlist playlist in App.UserPlaylists.playlists)
                {
                    ApplicationBarMenuItem item = new ApplicationBarMenuItem("\t" + playlist.PlaylistName);
                    item.Click += item_Click;
                    ApplicationBar.MenuItems.Add(item);
                }
            }
            else
            {
                ApplicationBar.MenuItems.Add(new ApplicationBarMenuItem("Login for Playlist Support") { IsEnabled = false });
            }
        }

        private async void item_Click(object sender, EventArgs e)
        {
            ApplicationBarMenuItem item = sender as ApplicationBarMenuItem;
            string search = item.Text.Substring(1);//Removes tab character
            var playlist = App.UserPlaylists.playlists.Single(i => i.PlaylistName == search);

            //There is no "add to playlist" in their api, so we use two calls. Not awesome, I know.
            try
            {
                var playlistResult = await App.Api.GetPlaylistAsync(playlist.PlaylistID);
                var songsIds = playlistResult.Songs.Select(i => i.SongID).Concat(new [] { App.SelectedSong.SongID}).ToArray();
                
                bool result = await App.Api.SetPlaylistSongsAsync(playlist.PlaylistID, songsIds);

                MessageBox.Show(result ? "Song successfully added" : "Problem adding song");
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to connect");
            }
        }

        private void PlayButton_OnClick(object sender, EventArgs e)
        {
            ApplicationBarIconButton button = sender as ApplicationBarIconButton;
            button.IsEnabled = false;

            //Not all songs (like SCoM) will get info back. GRR!!!
            //var result = await App.Api.GetSongsInfoAsync(App.SelectedSong.SongID);

            App.SetPlayRequest(new PlayRequest { Type = PlayRequestType.Song, ObjectID = App.SelectedSong.SongID }, App.SelectedSong.SongName);//Song = App.SelectedSong, 
            BackgroundAudioPlayer.Instance.Play();

            //App.ShowNowPlaying = true;
            //NavigationService.GoBack();
        }

        private async void FavoriteButton_OnClick(object sender, EventArgs e)
        {
            if (App.Api.CurrentUser == null)
            {
                var loginBox = MessageBox.Show("Must be logged in to have favorites" + Environment.NewLine + "Go to Login page?", "Must be Logged In", MessageBoxButton.OKCancel);
                if (loginBox == MessageBoxResult.OK)
                    NavigationService.Navigate(new Uri("/UI/LoginPage.xaml", UriKind.Relative));

                return;
            }

            if (App.UserFavorites.songs.Any(i => i.SongID == App.SelectedSong.SongID))
            {
                var message = MessageBox.Show("Song already in your favorites. Visit My Favorits page to remove it. Go there now?", "Song already in Favorites", MessageBoxButton.OKCancel);
                if (message == MessageBoxResult.OK)
                    NavigationService.Navigate(new Uri("/UI/UserFavoritesPage.xaml", UriKind.Relative));

                return;
            }

            //adding to favorites
            try
            {
                SuccessResult favoriteResult = await App.Api.AddUserFavoriteSongAsync(App.SelectedSong.SongID);
                MessageBox.Show(favoriteResult.success ? "Successfully added to favorites" : "Call was rejected");
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to connect");
                return;
            }

            //Now store it locally
            App.UserFavorites.songs.Add(App.SelectedSong);
        }
    }
}