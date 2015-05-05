using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Navigation;
using Windows.System;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Media.PhoneExtensions;
using SharkDownload.Resources;
using SharkLibrary.Api;
using SharkLibrary.ApiModels;

namespace SharkDownload
{
    public partial class MainPage : PhoneApplicationPage
    {

        private HttpClient _http;
        private Queue<ApiSong> _downloadQueue;
        private Thread _thread;


        private AutoResetEvent _event;
        
        public MainPage()
        {
            InitializeComponent();

            App.Downloads.CollectionChanged += DownloadsOnCollectionChanged;

            _downloadQueue = new Queue<ApiSong>();
            _thread = new Thread(DownloadSongs);
            _http = new HttpClient();
            _event = new AutoResetEvent(false);
        }

        private async void DownloadSongs()
        {
            while (true)
            {
                _event.WaitOne();
                while (_downloadQueue.Count > 0)
                {
                    ApiSong song = _downloadQueue.Dequeue();
                    song.DownloadStatus = ApiSong.SongDownloadStatus.Loading;

                    string url = await App.Api.GetStreamUrlAsync(song);
                    string fileName = "/Music/" + song.SongID + ".mp3";

                    using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        using (var file = storage.CreateFile(fileName))
                        {
                            Stream stream = await _http.GetStreamAsync(url);
                            stream.CopyTo(file);

                            //int read;
                            //byte[] buffer = new byte[1024];
                            //while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                            //{
                            //    file.Write(buffer, 0, read);

                            //    long i = stream.Length;
                            //}
                        }
                    }
                    await Dispatcher.InvokeAsync(() => song.DownloadStatus = ApiSong.SongDownloadStatus.Finished);

                    MediaLibrary library = new MediaLibrary();
                    SongMetadata meta = new SongMetadata
                    {
                        AlbumName = song.AlbumName,
                        ArtistName = song.ArtistName,
                        TrackNumber = int.Parse(song.TrackNum),
                        Name = song.SongName
                    };
                    library.SaveSong(new Uri(fileName, UriKind.Relative), meta, SaveSongOperation.MoveToLibrary);

                    await Dispatcher.InvokeAsync(() => song.DownloadStatus = ApiSong.SongDownloadStatus.InLibrary);
                }                
            }
        }

        private void DownloadsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs.Action != NotifyCollectionChangedAction.Add)
                return;

            if (notifyCollectionChangedEventArgs.NewItems.Count == 0)
                return;

            _downloadQueue.Enqueue(notifyCollectionChangedEventArgs.NewItems[0] as ApiSong);

            _event.Set();
            if (_thread.ThreadState == ThreadState.Unstarted)
                _thread.Start();
        }

        private async void MainPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            DownloadListSelector.ItemsSource = App.Downloads;
            
            SystemTray.IsVisible = true;
            SystemTray.ProgressIndicator.Text = "Loading...";

            await App.Api.LoadAsync();
            await App.Api.GetCommunicationTokenAsync();

            SystemTray.IsVisible = false;
            SearchTextBox.IsEnabled = true;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (App.ShowQueue)
            {
                App.ShowQueue = false;

                Panorama.DefaultItem = Panorama.Items[1];
            }

            base.OnNavigatedTo(e);
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            //if (App.OverrideBack)
            //{
            //    e.Cancel = true;
            //    App.OverrideBack = false;

            //}
            if (Panorama.SelectedItem == Panorama.Items[1])
            {
                Panorama.SlideToPage(0);
                e.Cancel = true;
            }

            base.OnBackKeyPress(e);
        }

        private void ResultsList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;

            ApiSong selected = e.AddedItems[0] as ApiSong;
            if (selected == null)
                return;

            App.SelectedSong = selected;
            NavigationService.Navigate(new Uri("/SongPage.xaml", UriKind.Relative));
            SongListSelector.SelectedItem = null;
        }

        private void ResultsList_OnItemRealized(object sender, ItemRealizationEventArgs e)
        {
            var container = e.Container;
            var content = container.Content;
            ApiSong model = content as ApiSong;
            model.ThumbUrl = App.Api.GetImageThumbAlbumArtUrl(model);
        }

        private async void SearchTextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            //Do search
            SystemTray.IsVisible = true;
            SystemTray.ProgressIndicator.Text = "Searching...";

            try
            {
                var result = await App.Api.SearchAsync(SearchTextBox.Text);
                SongListSelector.ItemsSource = result.Songs;

                SongListSelector.Focus();
            }
            catch (Exception)
            {
                MessageBox.Show("Error receiving search results");
            }

            SystemTray.IsVisible = false;
        }

        private async void DownloadListSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Uri uri = new Uri("xboxmusic:");
            await Launcher.LaunchUriAsync(uri);
        }
    }
}