using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using xBrainLab.Security.Cryptography;

namespace SharkLibrary.Api
{
    //https://github.com/fastest963/GroovesharkAPI-PHP/blob/v2/gsAPI.php
    //There were many ways to write this, and I decided to make one instance paired with one user. Therefore it is indeed statful. If many users are logged in at once (not going to happen on an app) many instances of this class are needed
    public class GroovesharkPublicApi
    {
        private const string EndpointBase = "http://api.grooveshark.com/ws3.php?sig=";
        private const string SecureEndpointBase = "https://api.grooveshark.com/ws3.php?sig=";
        private const string Pic500Url = "http://images.gs-cdn.net/static/albums/500_";
        private const string Pic80Url = "http://images.gs-cdn.net/static/albums/80_";
        private const string Pic40Url = "http://images.gs-cdn.net/static/albums/40_";

        private static readonly Dictionary<CoverArtImageType, string> ImageUrls = new Dictionary<CoverArtImageType, string>
        {
            { CoverArtImageType.Pic40, Pic40Url },
            { CoverArtImageType.Pic80, Pic80Url },
            { CoverArtImageType.Pic500, Pic500Url },
        };

        //Members
        private readonly string _key;
        private readonly string _secret;
        private readonly HttpClient _client;

        //Session
        public string SessionId { get; set; }
        public Country Country { get; set; }
        
        public AuthenticateResult CurrentUser { get; private set; }
        public int? Limit { get; set; }

        //Properties
        public bool HasLoaded
        {
            get { return SessionId != null; }
        }

        public GroovesharkPublicApi(string key, string secret, int? limit = null)
        {
            _key = key;
            _secret = secret;
            Limit = limit;

            _client = new HttpClient();
            _client.DefaultRequestHeaders.ExpectContinue = false;
        }

        #region Loading

        public async Task<bool> StartSessionAsync()
        {
            Result<StartSessionResult> root = await GetApiResultAsync<StartSessionResult>("startSession", secure:true);

            if (root.result.success)
                SessionId = root.result.sessionID;
            
            return root.result.success;
        }
        public async Task GetCountryAsync()
        {
            Result<Country> root = await GetApiResultAsync<Country>("getCountry");
            Country = root.result;
        }

        #endregion

        #region Membership

        //The server does a good job verifying, so just let them do it
        //TODO: flush out format of gender and birthdate
        public async Task<RegisterUserResult> RegisterUserAsync(string emailAddress, string password, string fullName, string username = null, string gender = null, DateTime? birthDate = null)
        {
            JObject parameters = new JObject
            {
                { "emailAddress" , emailAddress },
                { "password" , password },
                { "fullName" , fullName },
                { "username", username },

            };

            //Verify email

            //Verify password
            //if ((password.Length < 5) || (password.Length > 32))
            //    throw new ArgumentException("Passwords must be between 5 and 32 characters.", "password");

            //if (username != null)
            //{
            //    if ((username.Length < 5) || (username.Length > 32))
            //        throw new ArgumentException("Passwords must be between 5 and 32 characters.", "username");

            //    //This can probably be a lot neater
            //    for (int i = 0; i < username.Length; i++)
            //    {
            //        char c = username[i];
            //        if ((c == '.') || (c == '-') || (c == '_'))
            //        {
            //            if ((i == 0) || (i == username.Length - 1))
            //                throw new ArgumentException("Invalid username");
            //        }
            //        else if ((!Char.IsUpper(c)) && (!Char.IsLower(c)) && (!Char.IsDigit(c)))
            //            throw new ArgumentException("Invalid username");
            //    }

            //    parameters["username"] = username;
            //}

            Result<RegisterUserResult> root = await GetApiResultAsync<RegisterUserResult>("registerUser", parameters);
            return root.result;
        }

        public async Task<AuthenticateResult> AuthenticateAsync(string login, string password)
        {
            JObject parameters = new JObject
            {
                { "login" , login },
                { "password",  MD5.GetHashString(password) }
            };

            Result<AuthenticateResult> root = await GetApiResultAsync<AuthenticateResult>("authenticate", parameters, true);
            if (root.result.success)
                CurrentUser = root.result;

            return root.result;
        }

        public async Task<bool> LogoutAsync()
        {
            Result<SuccessResult> root = await GetApiResultAsync<SuccessResult>("logout");
            CurrentUser = null;
            return root.result.success;
        }

        #endregion

        #region Library

        public async Task<GetUserLibraryResult> GetUserLibraryAsync(int? limit = null)
        {
            JObject parameters = new JObject
            {
                { "limit", limit ?? Limit }
            };

            Result<GetUserLibraryResult> root = await GetApiResultAsync<GetUserLibraryResult>("getUserLibrarySongs", parameters);
            return root.result;
        }

        public async Task<UserLibrarySongsResult> AddUserLibrarySongsExAsync(Song song)
        {
            return await AddUserLibrarySongsExAsync(new[] {song});
        }

        public async Task<UserLibrarySongsResult> AddUserLibrarySongsExAsync(Song[] songs)
        {
            LibrarySong[] librarySongs = songs.Select(
                i => new LibrarySong
                    {
                        albumID = i.AlbumID,
                        artistID = i.ArtistID,
                        songID = i.SongID,
                        trackNum = i.TrackNum != null ?  int.Parse(i.TrackNum) : 0
                    }).ToArray();
            
            JObject parameters = new JObject
            {
                { "songs", JToken.FromObject(librarySongs) }
            };

            Result<UserLibrarySongsResult> root = await GetApiResultAsync<UserLibrarySongsResult>("addUserLibrarySongsEx", parameters);
            return root.result;
        }

        public async Task<UserLibrarySongsResult> RemoveUserLibrarySongsAsync(Song song)
        {
            return await RemoveUserLibrarySongsAsync(new [] {song});
        }

        public async Task<UserLibrarySongsResult> RemoveUserLibrarySongsAsync(Song[] songs)
        {
            int[] songIds = songs.Select(i => i.SongID).ToArray();
            int[] albumIds = songs.Select(i => i.AlbumID).ToArray();
            int[] artistIds = songs.Select(i => i.ArtistID).ToArray();
            
            JObject parameters = new JObject
            {
                { "songIDs", JArray.FromObject(songIds) },
                { "albumIDs", JArray.FromObject(albumIds) },
                { "artistIDs", JArray.FromObject(artistIds) },
            };

            Result<UserLibrarySongsResult> root = await GetApiResultAsync<UserLibrarySongsResult>("removeUserLibrarySongs", parameters);
            return root.result;
        }

        #endregion

        #region Favorites

        public async Task<SongListResult> GetUserFavoritesAsync(int? limit = null)
        {
            JObject parameters = new JObject
            {
                { "limit", limit ?? Limit }
            };

            Result<SongListResult> root = await GetApiResultAsync<SongListResult>("getUserFavoriteSongs", parameters);
            return root.result;
        }

        public async Task<SuccessResult> AddUserFavoriteSongAsync(int songId)
        {
            JObject parameters = new JObject
            {
                { "songID", songId }
            };

            Result<SuccessResult> root = await GetApiResultAsync<SuccessResult>("addUserFavoriteSong", parameters);
            return root.result;
        }

        public async Task<SuccessResult> RemoveUserFavoriteSongsAsync(int songId)
        {
            return await RemoveUserFavoriteSongsAsync(new[] {songId});
        }
        
        public async Task<SuccessResult> RemoveUserFavoriteSongsAsync(int[] songIds)
        {
            if (songIds == null)
                throw new ArgumentNullException("songIds", "Parameter can't be null");
            
            JObject parameters = new JObject
            {
                { "songIDs", JArray.FromObject(songIds) }
            };

            Result<SuccessResult> root = await GetApiResultAsync<SuccessResult>("removeUserFavoriteSongs", parameters);
            return root.result;
        }

        #endregion

        #region Streaming

        public async Task<StreamKeyResult> GetStreamKeyStreamServerAsync(int songId, bool? lowBitate = null)
        {
            if (Country == null)
                throw new InvalidOperationException("Call GetCountryAsync first");
            
            JObject parameters = new JObject
            {
                { "songID" , songId },
                { "country",  JToken.FromObject(Country) },
            };

            if (lowBitate.HasValue)
                parameters.Add("lowBitrate", Convert.ToInt32(lowBitate.Value));

            Result<StreamKeyResult> root = await GetApiResultAsync<StreamKeyResult>("getStreamKeyStreamServer", parameters);
            return root.result;
        }

        public async Task<bool> MarkStreamKeyOver30SecsAsync(StreamKeyResult result)
        {
            JObject parameters = new JObject
            {
                { "streamKey" , result.StreamKey },
                { "streamServerID",  result.StreamServerID },
            };

            Result<SuccessResult> root = await GetApiResultAsync<SuccessResult>("markStreamKeyOver30Secs", parameters);
            return root.result.success;
        }

        public async Task<bool> MarkSongCompleteAsync(int songId, StreamKeyResult result)
        {
            JObject parameters = new JObject
            {
                { "songID" , songId },    
                { "streamKey" , result.StreamKey },
                { "streamServerID",  result.StreamServerID },
            };

            Result<SuccessResult> root = await GetApiResultAsync<SuccessResult>("markSongComplete", parameters);
            return root.result.success;
        }

        #endregion

        #region Searching

        public async Task<PlaylistListResult> GetPlaylistSearchResultsAsync(string query, int? limit = null)
        {
            JObject parameters = new JObject
            {
                { "query" , query },
                { "limit", limit ?? Limit }
            };

            Result<PlaylistListResult> root = await GetApiResultAsync<PlaylistListResult>("getPlaylistSearchResults", parameters);
            return root.result;
        }

        public async Task<AlbumListResult> GetAlbumSearchResultsAsync(string query, int? limit = null)
        {
            JObject parameters = new JObject
            {
                { "query" , query },
                { "limit", limit ?? Limit }
            };

            Result<AlbumListResult> root = await GetApiResultAsync<AlbumListResult>("getAlbumSearchResults", parameters);
            return root.result;
        }

        public async Task<ArtistListResult> GetArtistSearchResultsAsync(string query, int? limit = null)
        {
            JObject parameters = new JObject
            {
                { "query" , query },
                { "limit", limit ?? Limit }
            };

            Result<ArtistListResult> root = await GetApiResultAsync<ArtistListResult>("getArtistSearchResults", parameters);
            return root.result;
        }

        public async Task<SongListResult> GetSongSearchResultsAsync(string query, int? limit = null, int? offset = null)
        {
            if (Country == null)
                throw new InvalidOperationException("Call GetCountryAsync first");

            JObject parameters = new JObject
            {
                { "query" , query },
                { "country",  JToken.FromObject(Country) },
                { "limit", limit ?? Limit },
                { "offset", offset }
            };

            Result<SongListResult> root = await GetApiResultAsync<SongListResult>("getSongSearchResults", parameters);
            return root.result;
        }

        #endregion

        #region Playlists
        
        public async Task<PlaylistListResult> GetUserPlaylistsAsync(int? limit = null)
        {
            JObject parameters = new JObject
            {
                { "limit", limit ?? Limit }
            };

            Result<PlaylistListResult> root = await GetApiResultAsync<PlaylistListResult>("getUserPlaylists", parameters);
            return root.result;
        }

        public async Task<GetPlaylistResult> GetPlaylistAsync(int playlistId, int? limit = null)
        {
            JObject parameters = new JObject
            {
                { "playlistID", playlistId },
                { "limit", limit ?? Limit },
            };

            Result<GetPlaylistResult> root = await GetApiResultAsync<GetPlaylistResult>("getPlaylist", parameters);
            return root.result;
        }

        public async Task<PlaylistResult> CreatePlaylistAsync(string name, int[] songIDs = null)
        {
            JObject parameters = new JObject
            {
                { "name", name },
                { "songIDs", songIDs != null ? JArray.FromObject(songIDs) : JArray.FromObject(new int[] {}) }
            };

            Result<PlaylistResult> root = await GetApiResultAsync<PlaylistResult>("createPlaylist", parameters);
            return root.result;
        }

        public async Task<PlaylistResult> RenamePlaylistAsync(int playlistId, string name)
        {
            JObject parameters = new JObject
            {
                { "playlistID", playlistId },
                { "name", name }
            };

            Result<PlaylistResult> root = await GetApiResultAsync<PlaylistResult>("renamePlaylist", parameters);
            return root.result;
        }

        public async Task<PlaylistResult> DeletePlaylistAsync(int playlistId)
        {
            JObject parameters = new JObject
            {
                { "playlistID", playlistId },
            };

            Result<PlaylistResult> root = await GetApiResultAsync<PlaylistResult>("deletePlaylist", parameters);
            return root.result;
        }

        public async Task<bool> SetPlaylistSongsAsync(int playlistId, int[] songIDs)
        {
            JObject parameters = new JObject
            {
                { "playlistID", playlistId },
                { "songIDs", JArray.FromObject(songIDs) }
            };

            Result<SuccessResult> root = await GetApiResultAsync<SuccessResult>("setPlaylistSongs", parameters);
            return root.result.success;
        }

        #endregion

        #region Album

        public async Task<AlbumListResult> GetAlbumsInfoAsync(int albumId)
        {
            return await GetAlbumsInfoAsync(new int[] {albumId});
        }
        
        public async Task<AlbumListResult> GetAlbumsInfoAsync(int[] albumIds)
        {
            if (albumIds == null)
                throw new ArgumentNullException("albumIds", "Can't be null");
            
            JObject parameters = new JObject
            {
                { "albumIDs", JArray.FromObject(albumIds) }
            };

            Result<AlbumListResult> root = await GetApiResultAsync<AlbumListResult>("getAlbumsInfo", parameters);
            return root.result;
        }

        public async Task<SongListResult> GetAlbumSongs(int albumId, int? limit = null)
        {
            JObject parameters = new JObject
            {
                { "albumID", albumId },
                { "limit", limit ?? Limit },
            };

            Result<SongListResult> root = await GetApiResultAsync<SongListResult>("getAlbumSongs", parameters);
            return root.result;
        }

        #endregion

        #region Songs

        public async Task<SongListResult> GetSongsInfoAsync(int songId)
        {
            return await GetSongsInfoAsync(new int[] { songId });
        }

        public async Task<SongListResult> GetSongsInfoAsync(int[] songIds)
        {
            if (songIds == null)
                throw new ArgumentNullException("songIds", "Can't be null");

            JObject parameters = new JObject
            {
                { "songIDs", JArray.FromObject(songIds) }
            };

            Result<SongListResult> root = await GetApiResultAsync<SongListResult>("getSongsInfo", parameters);
            return root.result;
        }

        #endregion

        public async Task<AutocompleteResult> GetAutocompleteSearchResultsAsync(string query, AutoCompleteSearchResultType type, int? limit = null)
        {
            JObject parameters = new JObject
            {
                { "query", query },
                { "type", type.ToString().ToLower() },
                { "limit", limit ?? Limit }
            };

            Result<AutocompleteResult> root = await GetApiResultAsync<AutocompleteResult>("getAutocompleteSearchResults", parameters);
            return root.result;
        }

        public string GetCoverArtImageUrl(BaseModel model, CoverArtImageType type)
        {
            if (!ImageUrls.ContainsKey(type))
                throw new NotImplementedException("CoverArtImageTpe not found");

            return ImageUrls[type] + model.CoverArtFilename;
        }

        public string GetCoverArtImageUrl(string coverArtFilename, CoverArtImageType type)
        {
            if (!ImageUrls.ContainsKey(type))
                throw new NotImplementedException("CoverArtImageTpe not found");

            return ImageUrls[type] + coverArtFilename;
        }

        #region Internals

        private async Task<Result<T>> GetApiResultAsync<T>(string method, JToken parameters = null, bool secure = false)
        {
            JObject payload = BuildPayload(method, parameters);
            string json = JsonConvert.SerializeObject(payload, Formatting.None);
            HttpContent content = new StringContent(json);

            string sig = CreateMessageSignature(json);
            string url = secure ? SecureEndpointBase + sig : EndpointBase + sig;
            HttpResponseMessage result = await _client.PostAsync(url, content);
            //result.EnsureSuccessStatusCode();
            if (result.StatusCode != HttpStatusCode.OK)
                throw new HttpResponseException("Received Status Code " + result.StatusCode);
            string resultJson = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Result<T>>(resultJson);
        }

        private JObject BuildPayload(string method, JToken parameters)
        {
            JObject header = new JObject
            {
                { "wsKey" , _key }
            };

            if (SessionId != null)
                header.Add("sessionID", SessionId);

            JObject payload = new JObject
            {
                { "method", method },
                { "parameters", parameters },
                { "header" , header }
            };

            return payload;
        }

        private string CreateMessageSignature(string data)
        {
            HMACMD5 hmac = new HMACMD5(_secret);
            byte[] hash = hmac.ComputeHash(data);
            return hash.Select(i => i.ToString("x2")).Aggregate((a, b) => a + b);
        }

        #endregion
    }
}
