using System;
using System.Windows;
using Microsoft.Phone.Controls;
using SharkLibrary.Api;
using SharkLibrary.Models;

namespace SharkPhone.UI
{
    public partial class AddPlaylistPage : PhoneApplicationPage
    {
        public AddPlaylistPage()
        {
            InitializeComponent();
        }

        private async void SaveButton_OnClick(object sender, EventArgs e)
        {
            var result = await App.Api.CreatePlaylistAsync(PlaylistNameTextBox.Text);
            DateTime date = Utilities.UnixTimeStampToDateTime(result.playlistsTSModified);
            if (result.success)
            {
                App.UserPlaylists.playlists.Add(new Playlist { PlaylistID = result.playlistID, PlaylistName = PlaylistNameTextBox.Text, TSAdded = date.ToLongDateString()});
                NavigationService.GoBack();
            }
            else
                throw new Exception();
        }
    }
}