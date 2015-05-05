using System;
using Microsoft.Phone.Controls;
using SharkLibrary.ApiModels;

namespace SharkPhone
{
    public static class Extensions
    {
        //Grooveshark
        //This is your fault
        public static ApiSong ToApiSong(this ApiAlbumSong song)
        {
            return new ApiSong
            {
                AlbumID = song.AlbumID,
                AlbumName = song.AlbumName,
                ArtistID = song.ArtistID,
                ArtistName = song.ArtistName,
                SongName = song.Name,
                SongID = song.SongID,
                CoverArtFilename = song.CoverArtFilename
            };
        }

        //From sharkdownload
        public static void SlideToPage(this Panorama self, int item)
        {

            var slideTransition = new SlideTransition() { };
            slideTransition.Mode = SlideTransitionMode.SlideLeftFadeIn;
            ITransition transition = slideTransition.GetTransition(self);
            transition.Completed += (s, e) =>
            {
                self.DefaultItem = self.Items[item];
                transition.Stop();
            };
            transition.Begin();
        }
    }
}
