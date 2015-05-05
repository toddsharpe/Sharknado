using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Phone.BackgroundAudio;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Media.PhoneExtensions;
using SharkLibrary.Api;
using SharkLibrary.ApiModels;
using SharkLibrary.Models;

namespace SharkDownload
{
    public partial class SongPage : PhoneApplicationPage
    {
        private static readonly Uri PlayIconUri = new Uri("/Assets/Icons/transport.play.png", UriKind.Relative);
        private static readonly Uri PauseIconUri = new Uri("/Assets/Icons/transport.pause.png", UriKind.Relative);
        
        public SongPage()
        {
            InitializeComponent();
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
        }

        private async void PlayButton_OnClick(object sender, EventArgs e)
        {
            ApplicationBarIconButton button = sender as ApplicationBarIconButton;

            if (MediaElement.CurrentState == MediaElementState.Playing)
            {
                MediaElement.Pause();
                button.IconUri = PauseIconUri;
            }
            else if (MediaElement.CurrentState == MediaElementState.Paused)
            {
                MediaElement.Play();
                button.IconUri = PlayIconUri;
            }
            else
            {
                string url = await App.Api.GetStreamUrlAsync(App.SelectedSong);
                MediaElement.Source = new Uri(url, UriKind.Absolute);
                MediaElement.Play();
                button.IconUri = PauseIconUri;
            }
        }

        private void DownloadButton_OnClick(object sender, EventArgs e)
        {
            ApplicationBarIconButton button = sender as ApplicationBarIconButton;
            button.IsEnabled = false;

            //Check to see if we already have it
            MediaLibrary library = new MediaLibrary();
            bool found = library.Songs.Any(i => i.Name == App.SelectedSong.SongName &&
                                                i.Album.Name == App.SelectedSong.AlbumName &&
                                                i.Artist.Name == App.SelectedSong.ArtistName &&
                                                i.TrackNumber == int.Parse(App.SelectedSong.TrackNum));
            if (found)
            {
                MessageBox.Show("Song already in library");
                return;
            }

            //Save meta
            App.SelectedSong.SongName = SongNameTextBox.Text;
            App.SelectedSong.AlbumName = AlbumNameTextBox.Text;
            App.SelectedSong.ArtistName = ArtistNameTextBox.Text;
            App.SelectedSong.TrackNum = TrackNumberTextBox.Text;

            App.SelectedSong.DownloadStatus = ApiSong.SongDownloadStatus.Queued;
            App.Downloads.Add(App.SelectedSong);

            App.ShowQueue = true;
            NavigationService.GoBack();
        }

        private void SongPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            Uri image = new Uri(App.Api.GetImageAlbumArtUrl(App.SelectedSong));
            ContentPanel.Background = new ImageBrush
            {
                Stretch = Stretch.Fill,
                ImageSource = new BitmapImage(image)
            };
        }
    }
}