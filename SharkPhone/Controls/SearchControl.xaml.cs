using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SharkLibrary.Api;
using SharkPhone.Models;

namespace SharkPhone.Controls
{
    public partial class SearchControl : BusyUserControl
    {
        public event Error OnError;
        public event EventHandler OnItemSelected;
        public event EventHandler<Search> OnSearch;


        public object SelectedItem { get; private set; }

        public SearchControl()
        {
            InitializeComponent();
        }

        public async void ExecuteSearch(string query, string type)
        {
            //TODO: this is SOOOO dumb
            string[] types = new string[] {"Songs", "Albums", "Playlists"};
            int index = Array.IndexOf(types, type);
            
            //Update UI
            SearchTextBox.Text = query;
            SearchTypePicker.SelectedIndex = index;

            //Do search
            await Search(query, type);
        }

        private async void SearchTextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            //Get search type
            var item = SearchTypePicker.SelectedItem as ListPickerItem;
            string type = item.Content as string;

            await Search(SearchTextBox.Text, type);
        }

        private async Task Search(string query, string type)
        {
            OnBusyStarting("Searching for " + type);

            try
            {
                IList source;
                switch (type)
                {
                    case "Albums":
                        var albumsResult = await App.Api.GetAlbumSearchResultsAsync(query);
                        ResultsListSelector.ItemTemplate = this.Resources["AlbumDataTemplate"] as DataTemplate;
                        source = albumsResult.albums;
                        break;

                    case "Playlists":
                        var playlistResult = await App.Api.GetPlaylistSearchResultsAsync(query);
                        ResultsListSelector.ItemTemplate = this.Resources["PlaylistDataTemplate"] as DataTemplate;
                        source = playlistResult.playlists;
                        break;

                    case "Songs":
                    default:
                        var songResults = await App.Api.GetSongSearchResultsAsync(query);
                        ResultsListSelector.ItemTemplate = this.Resources["SongDataTemplate"] as DataTemplate;
                        source = songResults.songs;
                        break;
                }

                ResultsListSelector.ItemsSource = source;
                ResultsListSelector.Focus();

                if (OnSearch != null)
                    OnSearch(this, new Search {Query = query, Type = type});
            }
            catch (Exception ex)
            {
                if (OnError != null)
                    OnError(this, new ErrorEventArgs("Error receiving search results", ex));
            }

            OnBusyFinished();
        }

        private void ResultsList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;

            if (e.AddedItems[0] == null)
                return;

            SelectedItem = e.AddedItems[0];

            if (OnItemSelected != null)
                OnItemSelected(this, new EventArgs());

            ResultsListSelector.SelectedItem = null;
        }
        
        private void ResultsList_OnItemRealized(object sender, ItemRealizationEventArgs e)
        {
            var container = e.Container;
            var content = container.Content;

            //Playlists don't have album art and aren't of type BaseModel
            if (!(content is BaseModel))
                return;

            BaseModel model = content as BaseModel;
            model.ThumbUrl = App.Api.GetCoverArtImageUrl(model, CoverArtImageType.Pic40);
        }
    }
}
