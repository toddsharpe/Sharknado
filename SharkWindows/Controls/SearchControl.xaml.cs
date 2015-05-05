using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NAudio.Wave;
using SharkLibrary.Api;
using SharkLibrary.ApiModels;

namespace SharkWindows.Controls
{
    /// <summary>
    /// Interaction logic for SearchControl.xaml
    /// </summary>
    public partial class SearchControl : UserControl
    {

        private HttpClient _client;
        public SearchControl()
        {
            InitializeComponent();

            _client = new HttpClient();
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            //Busy indicator
            BusyIndicator.BusyContent = "Searching...";
            BusyIndicator.IsBusy = true;

            string searchText = SearchTextBox.Text;
            ApiSearchResult results = await App.Api.SearchAsync(searchText);

            BusyIndicator.IsBusy = false;
            ListView.ItemsSource = results.Songs;
        }

        private async void SearchControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            if (App.Api.HasLoaded)
                return;
            
            //Set indicator
            BusyIndicator.BusyContent = "Initializing Grooveshark API";
            BusyIndicator.IsBusy = true;

            await App.Api.LoadAsync();
            await App.Api.GetCommunicationTokenAsync();

            BusyIndicator.IsBusy = false;
        }

        private void DownloadButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (ListView.SelectedItems.Count == 0)
                return;

            foreach (ApiSong item in ListView.SelectedItems)
            {
                item.DownloadStatus = ApiSong.SongDownloadStatus.Queued;
                App.Downloads.Add(item);
            }
        }

        private void ViewFolderButton_OnClick(object sender, RoutedEventArgs e)
        {
            Process.Start(App.Options.MusicFolder);
        }

        private async void PlayButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (ListView.SelectedItems.Count == 0)
                return;

            ApiSong selected = ListView.SelectedItems[0] as ApiSong;
            string url = await App.Api.GetStreamUrlAsync(selected);

            //Save to temp
            using (Stream stream = await _client.GetStreamAsync(url))
            {
                using (StreamWriter writer = new StreamWriter("temp.mp3", false))
                {
                    stream.CopyTo(writer.BaseStream);
                }
            }

            //Get info
            Mp3FileReader reader = new Mp3FileReader("temp.mp3");
            TimeSpan duration = reader.TotalTime;

            MessageBoxResult result = MessageBox.Show(String.Format("Song Duration: {0}\nWould you like to play the song?", duration.ToString("mm\\:ss")), "SharkWindows", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                Process.Start("temp.mp3");
            }
        }
    }
}
