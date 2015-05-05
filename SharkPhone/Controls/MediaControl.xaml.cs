using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using Microsoft.Phone.BackgroundAudio;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace SharkPhone.Controls
{
    public partial class MediaControl : UserControl
    {
        private static readonly Uri PlayIconUri = new Uri("/Assets/Icons/transport.play.png", UriKind.Relative);
        private static readonly Uri PauseIconUri = new Uri("/Assets/Icons/transport.pause.png", UriKind.Relative);

        private static readonly BitmapSource PlayBitmap = new BitmapImage(PlayIconUri);
        private static readonly BitmapSource PauseBitmap = new BitmapImage(PauseIconUri);
        
        
        private AudioTrack _currentTrack;
        private readonly DispatcherTimer _timer;
        
        public MediaControl()
        {
            InitializeComponent();

            BackgroundAudioPlayer.Instance.PlayStateChanged += Instance_PlayStateChanged;

            //Timer
            _timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 250) };
            _timer.Tick += TimerOnTick;
        }

        private void TimerOnTick(object sender, EventArgs eventArgs)
        {
            UpdateProgressBar();
        }

        //TODO: Consider using direct binding to AudioTrack and have these in xaml
        private void DisplayCurrentTrack()
        {
            if (_currentTrack == null)
                return;

            _timer.Start();

            SongNameTextBlock.Text = _currentTrack.Title;
            ArtistNameTextBlock.Text = "\tBy: " + _currentTrack.Artist;
            AlbumArtImage.Source = new BitmapImage(_currentTrack.AlbumArt);

            //Progress bar
            ProgressBar.Minimum = 0;
            ProgressBar.Maximum = _currentTrack.Duration.Minutes * 60 + _currentTrack.Duration.Seconds;
            LengthTextBlock.Text = _currentTrack.Duration.ToString("mm\\:ss");
            UpdateProgressBar();
        }

        //https://gist.github.com/richardszalay/8552812
        private void UpdateProgressBar()
        {
            if (BackgroundAudioPlayer.Instance.PlayerState != PlayState.Playing)
                return;

            ProgressBar.Value = BackgroundAudioPlayer.Instance.Position.Minutes * 60 + BackgroundAudioPlayer.Instance.Position.Seconds;
            PositionTextBlock.Text = BackgroundAudioPlayer.Instance.Position.ToString("mm\\:ss");
        }

        void Instance_PlayStateChanged(object sender, EventArgs e)
        {
            if (_currentTrack != BackgroundAudioPlayer.Instance.Track)
            {
                _currentTrack = BackgroundAudioPlayer.Instance.Track;
                DisplayCurrentTrack();
            }

            switch (BackgroundAudioPlayer.Instance.PlayerState)
            {
                case PlayState.Playing:
                    PlayImage.Source = PauseBitmap;
                    break;

                case PlayState.Paused:
                    PlayImage.Source = PlayBitmap;
                    break;
            }
        }

        private void PlayButton_OnClick(object sender, EventArgs e)
        {
            Button button = sender as Button;
            Image image = button.Content as Image;

            if (BackgroundAudioPlayer.Instance.PlayerState == PlayState.Paused)
            {
                BackgroundAudioPlayer.Instance.Play();
                image.Source = PauseBitmap;
            }
            else if (BackgroundAudioPlayer.Instance.PlayerState == PlayState.Playing)
            {
                if (BackgroundAudioPlayer.Instance.CanPause)
                {
                    BackgroundAudioPlayer.Instance.Pause();
                    image.Source = PlayBitmap;
                }
            }
        }

        private void MediaControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            //Get current state
            _currentTrack = BackgroundAudioPlayer.Instance.Track;
            DisplayCurrentTrack();
        }

        private void PreviousButton_OnClick(object sender, RoutedEventArgs e)
        {
            BackgroundAudioPlayer.Instance.SkipPrevious();
        }

        private void FastForwardButton_OnClick(object sender, RoutedEventArgs e)
        {
            BackgroundAudioPlayer.Instance.SkipNext();
        }
    }
}
