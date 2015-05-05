using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using SharkLibrary.Api;
using SharkLibrary.ApiModels;

namespace SharkPhone.UI
{
    public partial class UserPlaylistsPage : PhoneApplicationPage
    {
        public UserPlaylistsPage()
        {
            InitializeComponent();

            AdBorder.Child = AdManager.MakeAdControl();
        }

        private void ListSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;

            Playlist selected = e.AddedItems[0] as Playlist;
            if (selected == null)
                return;

            NavigationService.Navigate(new Uri("/UI/PlaylistPage.xaml?PlaylistID=" + selected.PlaylistID, UriKind.Relative));
            ListSelector.SelectedItem = null;
        }

        private void UserPlaylistsPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (App.Api.CurrentUser == null)
            {
                var result =
                    MessageBox.Show(
                        "Must be logged in to see your playlists" + Environment.NewLine + "Go to Login page?",
                        "Must be Logged In", MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.OK)
                    NavigationService.Navigate(new Uri("/UI/LoginPage.xaml", UriKind.Relative));
                else
                    NavigationService.GoBack();
            }
            else
            {
                ListSelector.ItemsSource = App.UserPlaylists.playlists.OrderBy(i => i.PlaylistName).ToList();
            }
        }

        private void AddButton_OnClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/UI/AddPlaylistPage.xaml", UriKind.Relative));
        }
    }
}