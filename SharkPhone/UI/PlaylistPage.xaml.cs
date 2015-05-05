using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public partial class PlaylistPage : PhoneApplicationPage
    {
        private GetPlaylistResult _result;
        private int _playlistId;

        public PlaylistPage()
        {
            InitializeComponent();

            AdBorder.Child = AdManager.MakeAdControl();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!NavigationContext.QueryString.ContainsKey("PlaylistID"))
                throw new Exception("PlaylistID not set");

            string playlistIdString = NavigationContext.QueryString["PlaylistID"];
            _playlistId = int.Parse(playlistIdString);

            try
            {
                _result = await App.Api.GetPlaylistAsync(_playlistId);
                this.DataContext = _result;
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to load playlist");
                NavigationService.GoBack();
                return;
            }

            //Determine renaming
            if ((App.Api.CurrentUser != null) && (_result.UserID == App.Api.CurrentUser.UserID))
            {
                RenameControl.Visibility = Visibility.Visible;
                ApplicationBar.IsVisible = true;
            }
            else
            {
                PlaylistNameStackPanel.Visibility = Visibility.Visible;
            }

            SystemTray.IsVisible = false;
        }

        private void SongList_OnItemRealized(object sender, ItemRealizationEventArgs e)
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

            int index = _result.Songs.IndexOf(song);

            PlayRequest playlist = new PlayRequest { Type = PlayRequestType.Playlist, ObjectID = _playlistId, Position = index };
            App.SetPlayRequest(playlist, song.SongName);
            BackgroundAudioPlayer.Instance.Play();
        }

        private void PlayButton_OnClick(object sender, EventArgs e)
        {
            PlayRequest playlist = new PlayRequest { Type = PlayRequestType.Playlist, ObjectID = _playlistId };
            App.SetPlayRequest(playlist, _result.PlaylistName);
            BackgroundAudioPlayer.Instance.Play();
        }

        private async void RenameButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (PlaylistNameTextBox.IsReadOnly)
            {
                //Change to edit mode
                CancelButton.Visibility = Visibility.Visible;
                PlaylistNameTextBox.IsReadOnly = false;
            }
            else
            {
                if (PlaylistNameTextBox.Text != _result.PlaylistName)
                {
                    //Do edits
                    bool success;
                    try
                    {
                        var result = await App.Api.RenamePlaylistAsync(_playlistId, PlaylistNameTextBox.Text);
                        success = result.success;
                    }
                    catch (Exception)
                    {
                        success = false;//not necessary, but visually pleasing haha
                    }

                    if (!success)
                    {
                        MessageBox.Show("Error renaming playlist", "Error", MessageBoxButton.OK);
                    }
                    else
                    {
                        App.UserPlaylists.playlists.Single(i => i.PlaylistID == _playlistId).PlaylistName = PlaylistNameTextBox.Text;
                        _result.PlaylistName = PlaylistNameTextBox.Text;//Do we need this?
                    }
                }

                CancelButton.Visibility = Visibility.Collapsed;
                PlaylistNameTextBox.IsReadOnly = true;
            }
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            PlaylistNameTextBox.Text = _result.PlaylistName;
            CancelButton.Visibility = Visibility.Collapsed;
            PlaylistNameTextBox.IsReadOnly = true;
        }

        private async void DeleteButton_OnClick(object sender, EventArgs e)
        {
            var result = await App.Api.DeletePlaylistAsync(_playlistId);
            if (result.success)
            {
                MessageBox.Show("Playlist deleted");
                var playlist = App.UserPlaylists.playlists.Single(i => i.PlaylistID == _playlistId);
                App.UserPlaylists.playlists.Remove(playlist);

                NavigationService.GoBack();
            }
            else
            {
                MessageBox.Show("Unable to delete playlist");
            }
        }
    }
}