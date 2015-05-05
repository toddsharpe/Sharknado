using System;
using System.Collections.Generic;
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
    /// Interaction logic for OptionsControl.xaml
    /// </summary>
    public partial class OptionsControl : UserControl
    {
        private static readonly ApiSong Example = new ApiSong
            {
                SongName = "SongName",
                AlbumName = "AlbumName",
                ArtistName = "ArtistName",
                TrackNum = "#"
            };

        private readonly WpfPlatformAdaptor _adaptor;

        public OptionsControl()
        {
            InitializeComponent();

            _adaptor = new WpfPlatformAdaptor();
        }

        private void Grooveshark_OnChecked(object sender, RoutedEventArgs e)
        {
            if (_adaptor == null)
                return;//Not sure how this is possible, but it is
            
            string path = _adaptor.GetSavePath(Example);

            HelpTextBlock.Text = path;
        }

        private void OptionsControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            Grooveshark_OnChecked(null, null);
        }
    }
}
