using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.BackgroundAudio;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SharkLibrary.Api;
using SharkLibrary.ApiModels;
using SharkLibrary.Models;

namespace SharkPhone.UI
{
    public partial class UserFavoritesPage : PhoneApplicationPage
    {
        public UserFavoritesPage()
        {
            InitializeComponent();

            AdBorder.Child = AdManager.MakeAdControl();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //await App.Api.GetUserFavoritesAsync();
        }

        private void SongsList_OnItemRealized(object sender, ItemRealizationEventArgs e)
        {
            var container = e.Container;
            var content = container.Content;
            Song song = content as Song;
            song.ThumbUrl = App.Api.GetCoverArtImageUrl(song, CoverArtImageType.Pic40);
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            //button.IsEnabled = false;
            Song song = button.Tag as Song;

            int index = App.UserFavorites.songs.IndexOf(song);

            PlayRequest playlist = new PlayRequest { Type = PlayRequestType.Favorites, Position = index };
            App.SetPlayRequest(playlist, "User Favorites");
            BackgroundAudioPlayer.Instance.Play();
        }

        private void UserFavoritesPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (App.Api.CurrentUser == null)
            {
                var result =
                    MessageBox.Show(
                        "Must be logged in to see your playlists" + Environment.NewLine + "Go to Login page?",
                        "Must be Logged In", MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.OK)
                    NavigationService.Navigate(new Uri("/UI/LoginPage.xaml", UriKind.Relative));
                else
                    NavigationService.GoBack();
            }
            else
            {
                this.SongsList.ItemsSource = App.UserFavorites.songs;
            }
        }

        private void ContextMenu_OnOpened(object sender, RoutedEventArgs e)
        {
            //http://stackoverflow.com/questions/15181441/windows-phone-toolkit-context-menu-items-have-wrong-object-bound-to-them-when-an
            var menu = (ContextMenu)sender;
            var owner = (FrameworkElement)menu.Owner;
            if (owner.DataContext != menu.DataContext)
                menu.DataContext = owner.DataContext;
        }

        private async void UnfavoriteMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var selected = (sender as MenuItem).DataContext as Song;
            if (selected == null)
                throw new Exception("Selected is null");

            //Removing
            try
            {
                var unfavoriteResult = await App.Api.RemoveUserFavoriteSongsAsync(selected.SongID);
                if (!unfavoriteResult.success)
                {
                    MessageBox.Show("Unable to remove from favorites. If I knew why, I'd tell you");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to connect");
                return;
            }

            //Remove locally
            App.UserFavorites.songs.Remove(selected);

            //Rebind - we could eliminate this by making it observable, but that would change serialization logic
            this.SongsList.ItemsSource = null;
            this.SongsList.ItemsSource = App.UserFavorites.songs;
        }
    }
}