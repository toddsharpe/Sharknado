using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
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
using SharkLibrary.ApiModels;

namespace SharkWindows.Controls
{
    /// <summary>
    /// Interaction logic for DownloadsControl.xaml
    /// </summary>
    public partial class DownloadsControl : UserControl
    {
        private Task _downloadTask;

        public DownloadsControl()
        {
            InitializeComponent();

            //Wires
            App.Downloads.CollectionChanged += DownloadsOnCollectionChanged;
            ListView.ItemsSource = App.Downloads;
        }

        private void DownloadsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if ((_downloadTask != null) && (_downloadTask.Status == TaskStatus.Running))
                return;

            _downloadTask = new Task(DownloadNext);
            _downloadTask.Start();
        }

        private async void DownloadNext()
        {
            ApiSong current = App.Downloads.FirstOrDefault(i => i.DownloadStatus == ApiSong.SongDownloadStatus.Queued);
            while (current != null)
            {
                //Download current
                current.DownloadStatus = ApiSong.SongDownloadStatus.InProgress;
                await App.Api.DownloadSongAsync(current);
                current.DownloadStatus = ApiSong.SongDownloadStatus.Finished;

                //Get next
                current = App.Downloads.FirstOrDefault(i => i.DownloadStatus == ApiSong.SongDownloadStatus.Queued);
            }
        }

        private void Clear_OnClick(object sender, RoutedEventArgs e)
        {
            var remove = App.Downloads.Where(i => i.DownloadStatus == ApiSong.SongDownloadStatus.Finished).ToList();

            foreach (var item in remove)
            {
                App.Downloads.Remove(item);
            }
        }
    }
}
