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
    public partial class AlbumPage : PhoneApplicationPage
    {
        private int _albumId;
        private SongListResult _songsResult;
        private Album _album;

        public AlbumPage()
        {
            InitializeComponent();
            
            AdBorder.Child = AdManager.MakeAdControl();
        }

        //Maybe do these requests in parallel, we'll see
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!NavigationContext.QueryString.ContainsKey("AlbumID"))
                throw new Exception("AlbumID not set");

            _albumId = int.Parse(NavigationContext.QueryString["AlbumID"]);

            var albumInfo = await App.Api.GetAlbumsInfoAsync(_albumId);
            _album = albumInfo.albums.First();
            this.DataContext = _album;

            _songsResult = await App.Api.GetAlbumSongs(_albumId);
            SongListSelector.ItemsSource = _songsResult.songs;

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

            int index = _songsResult.songs.IndexOf(song);

            App.SetPlayRequest(new PlayRequest { Type = PlayRequestType.Album, ObjectID = _albumId, Position = index }, song.Name);
            BackgroundAudioPlayer.Instance.Play();
        }
    }
}