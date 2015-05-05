using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Serialization;
using Microsoft.Phone.BackgroundAudio;
using Microsoft.Phone.Reactive;
using Microsoft.Phone.Shell;
using SharkLibrary.Api;
using SharkLibrary.Models;

namespace SharkAudioPlaybackAgent
{
    public class AudioPlayer : AudioPlayerAgent
    {
        //App Syncronization
        private static readonly Mutex FileLock;
        private static readonly Semaphore AppSemaphore;
        private static readonly Semaphore AgentSemaphore;
        private const string PlaylistFileName = "playlist.xml";

        //Grooveshark API
        public const string ApiKey = "<GroovesharkApiKey>";
        public const string ApiSecret = "<GroovesharkApiSecret>";
        private static readonly GroovesharkPublicApi Api;
        private static readonly TimeSpan Mark30Seconds = TimeSpan.FromSeconds(30);

        private const int RetryMax = 3;

        private static int _songId;
        private static StreamKeyResult _data;

        private static int _position;
        private static List<Song> _songs;

        static AudioPlayer()
        {
            // Subscribe to the managed exception handler
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                Application.Current.UnhandledException += UnhandledException;
            });

            FileLock = Mutex.OpenExisting("SharkPhone_PlayRequestMutex");
            AppSemaphore = Semaphore.OpenExisting("SharkPhone_AppLock");
            AgentSemaphore = Semaphore.OpenExisting("SharkPhone_AgentLock");
            Api = new GroovesharkPublicApi(ApiKey, ApiSecret);
        }

        /// Code to execute on Unhandled Exceptions
        private static void UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }

        //Copy paste from App.cs, should be in a library, huh?
        public static PlayRequest LoadPlayRequest()
        {
            FileLock.WaitOne();
            
            while (true)
            {
                using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (storage.FileExists(PlaylistFileName))
                    {
                        PlayRequest request;
                        using (IsolatedStorageFileStream file = storage.OpenFile(PlaylistFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(PlayRequest));
                            request = (PlayRequest)serializer.Deserialize(file);
                        }

                        storage.DeleteFile(PlaylistFileName);
                        FileLock.ReleaseMutex();
                        return request;
                    }
                }
                Thread.Sleep(100);
            }

        }

        public static bool HasRequestPending()
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                return storage.FileExists(PlaylistFileName);
            }
        }

        public static async Task LoadSongsAsync(PlayRequest request)
        {
            switch (request.Type)
            {
                case PlayRequestType.Playlist:
                    var playlistResult = await Api.GetPlaylistAsync(request.ObjectID);
                    _songs = playlistResult.Songs;
                    break;

                case PlayRequestType.Album:
                    var albumResult = await Api.GetAlbumSongs(request.ObjectID);
                    _songs = albumResult.songs;
                    break;

                case PlayRequestType.Song:
                    var songResult = await Api.GetSongsInfoAsync(request.ObjectID);
                    Thread.Sleep(5000);
                    if (songResult.songs.Count == 0)
                        throw new Exception("GetSongsInfoAsync returned no results");
                    _songs = new List<Song> {songResult.songs.First()};
                    break;

                case PlayRequestType.Favorites:
                    var favoritesResult = await Api.GetUserFavoritesAsync();
                    _songs = favoritesResult.songs;
                    break;

                case PlayRequestType.Library:
                    var libraryResult = await Api.GetUserLibraryAsync();
                    _songs = libraryResult.songs;
                    break;
            }

            if (request.Position < _songs.Count)
                _position = request.Position;
        }

        protected async override void OnPlayStateChanged(BackgroundAudioPlayer player, AudioTrack track, PlayState playState)
        {
            //When we need a new track, we have to hit disk to check for latest playlist
            switch (playState)
            {
                case PlayState.TrackEnded:
                    //we need to mark the stream as complete
                    await TryMarkCompleted();

                    if (_position < _songs.Count - 1)
                    {
                        try
                        {
                            //Advance
                            AudioTrack nextTrack = await GetNextTrackAsync();
                            if (nextTrack != null)
                                player.Track = nextTrack;
                        }
                        catch (Exception ex)
                        {
                            OnError(player, track, ex, false);
                            return;
                        }
                    }
                    break;
                case PlayState.TrackReady:
                    player.Play();
                    break;
                case PlayState.Shutdown:
                    break;
                case PlayState.Unknown:
                    break;
                case PlayState.Stopped:
                    break;
                case PlayState.Paused:
                    break;
                case PlayState.Playing:
                    break;
                case PlayState.BufferingStarted:
                    break;
                case PlayState.BufferingStopped:
                    break;
                case PlayState.Rewinding:
                    break;
                case PlayState.FastForwarding:
                    break;
            }

            NotifyComplete();
        }

        protected async override void OnUserAction(BackgroundAudioPlayer player, AudioTrack track, UserAction action, object param)
        {
            if (AppSemaphore.WaitOne(0))
            {
                var request = LoadPlayRequest();
                Api.SessionId = request.SessionId;
                Api.Country = request.Country;

                NotifyComplete();
                AgentSemaphore.Release();
                return;
            }
            
            switch (action)
            {
                case UserAction.Play:

                    if (HasRequestPending())
                    {
                        PlayRequest request = LoadPlayRequest();

                        try
                        {
                            await LoadSongsAsync(request);
                        }
                        catch (Exception ex)
                        {
                            OnError(player, track, ex, false);
                            return;
                        }

                        AudioTrack current;
                        try
                        {
                            current = await GetCurrentTrackAsync();
                        }
                        catch (Exception ex)
                        {
                            OnError(player, track, ex, false);
                            return;
                        }

                        player.Track = current;
                    }
                    else
                    {
                        if (player.PlayerState != PlayState.Playing)
                        {
                            player.Play();
                        }
                    }

                    break;
                case UserAction.Stop:
                    player.Stop();
                    break;
                case UserAction.Pause:
                    player.Pause();
                    break;
                case UserAction.FastForward:
                    player.FastForward();
                    break;
                case UserAction.Rewind:
                    player.Rewind();
                    break;
                case UserAction.Seek:
                    player.Position = (TimeSpan)param;
                    break;
                case UserAction.SkipNext:
                    await TryMark30Seconds(player);
                    try
                    {
                        //Advance
                        AudioTrack nextTrack = await GetNextTrackAsync();
                        if (nextTrack != null)
                            player.Track = nextTrack;
                    }
                    catch (Exception ex)
                    {
                        OnError(player, track, ex, false);
                        return;
                    }
                    break;

                case UserAction.SkipPrevious:
                    await TryMark30Seconds(player);
                    try
                    {
                        //Rewind
                        AudioTrack previousTrack = await GetPreviousTrackAsync();
                        if (previousTrack != null)
                            player.Track = previousTrack;
                    }
                    catch (Exception ex)
                    {
                        OnError(player, track, ex, false);
                        return;
                    }    
                    break;
            }

            NotifyComplete();
        }

        private static async Task TryMarkCompleted()
        {
            if (_data == null)
                return;
            
            try
            {
                await Api.MarkSongCompleteAsync(_songId, _data);
            }
            catch (Exception)
            {

            }
        }

        private static async Task TryMark30Seconds(BackgroundAudioPlayer player)
        {
            if ((_data != null) && (player.Position > Mark30Seconds))
            {
                try
                {
                    await Api.MarkStreamKeyOver30SecsAsync(_data);
                }
                catch (Exception)
                {

                }
            }
        }

        private static async Task<AudioTrack> GetNextTrackAsync()
        {            
            //Check bounds
            if (_position == _songs.Count - 1)
                return null;

            _position++;
            Song song = _songs[_position];
            return await GetAudioTrackAsync(song);
        }

        private static async Task<AudioTrack> GetPreviousTrackAsync()
        {
            //Check bounds
            if (_position == 0)
                return null;

            _position--;
            Song song = _songs[_position];
            return await GetAudioTrackAsync(song);
        }

        private static async Task<AudioTrack> GetCurrentTrackAsync()
        {
            if (_position > _songs.Count)
                return null;
            
            Song song = _songs[_position];
            return await GetAudioTrackAsync(song);
        }

        private static async Task<AudioTrack> GetAudioTrackAsync(Song song)
        {
            _songId = song.SongID;

            int i = 0;
            do
            {
                _data = await Api.GetStreamKeyStreamServerAsync(song.SongID, false);
                
                if (!String.IsNullOrEmpty(_data.url))
                {
                    string coverArtUrl = Api.GetCoverArtImageUrl(song.CoverArtFilename, CoverArtImageType.Pic500);
                    return new AudioTrack(new Uri(_data.url, UriKind.Absolute), song.SongName ?? song.Name, song.ArtistName, song.AlbumName, new Uri(coverArtUrl, UriKind.Absolute), song.SongID.ToString(), EnabledPlayerControls.All);
                }

                i++;
            } while (i < RetryMax);

            ShowErrorToast(new Exception("Couldn't load song"));
            return null;
        }

        /// <summary>
        /// Called whenever there is an error with playback, such as an AudioTrack not downloading correctly
        /// </summary>
        /// <param name="player">The BackgroundAudioPlayer</param>
        /// <param name="track">The track that had the error</param>
        /// <param name="error">The error that occured</param>
        /// <param name="isFatal">If true, playback cannot continue and playback of the track will stop</param>
        /// <remarks>
        /// This method is not guaranteed to be called in all cases. For example, if the background agent
        /// itself has an unhandled exception, it won't get called back to handle its own errors.
        /// </remarks>
        /// //TODO: Determine if I should be throwing fatal errors or not
        protected override void OnError(BackgroundAudioPlayer player, AudioTrack track, Exception error, bool isFatal)
        {
            ShowErrorToast(error);

            if (isFatal)
            {
                Abort();
            }
            else
            {
                NotifyComplete();
            }

        }

        private static void ShowErrorToast(Exception exception)
        {
            //Test
            ShellToast toast = new ShellToast { Title = "SharkPhone Error", Content = exception.Message };
            toast.Show();
        }

        /// <summary>
        /// Called when the agent request is getting cancelled
        /// </summary>
        /// <remarks>
        /// Once the request is Cancelled, the agent gets 5 seconds to finish its work,
        /// by calling NotifyComplete()/Abort().
        /// </remarks>
        protected override void OnCancel()
        {

        }
    }
}