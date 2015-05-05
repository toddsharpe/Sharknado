using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharkLibrary.ApiModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using xBrainLab.Security.Cryptography;

namespace SharkLibrary.Api
{
	public class GroovesharkApi
	{
        //Constants
        private const string Homepage = "https://html5.grooveshark.com/";
        private const string LibraryPath = "build/app.min.js";
        private const int Privacy = 0;
	    private const string StreamEndpoint = "stream.php?streamKey=";

        //Statics - these are constants that change occasionally, so we load them from JS library
        private static string Endpoint;//more.php
        private static string Client;//mobileshark
        private static string ClientRevision;
	    private static string RevToken;//gooeyFlubber
        private static string Pic500Url;//http://images.gs-cdn.net/static/albums/500_
        private static string Pic40Url;//http://images.gs-cdn.net/static/albums/40_ //picSD:"http://images.gs-cdn.net/static/albums/40_

        //Methods
        private const string searchMethod = "getResultsFromSearch";
        private const string tokenMethod = "getCommunicationToken";
	    private const string streamKeyMethod = "getStreamKeyFromSongIDEx";
	    private const string loginUsermethod = "authenticateUser";
	    private const string logoutMethod = "logoutUser";
	    private const string getPlaylistMethod = "getPlaylistByID";
	    private const string getAlbumMethod = "getAlbumByID";
	    private const string getAlbumSongsMethod = "albumGetAllSongs";
	    private const string getUserPlaylistMethod = "userGetPlaylists";
	    private const string addSongsToPlaylistMethod = "playlistAddSongToExistingEx";
	    private const string markSongDownloadedMethod = "markSongDownloadedEx";
        private const string favoriteMethod = "favorite";
	    private const string getFavoritesMethod = "getFavorites";
	    private const string unfavoriteMethod = "unfavorite";
	    private const string markStream30SecondsMethod = "markStreamKeyOver30Seconds";
	    private const string markSongCompleteMethod = "markSongComplete";

        //Static constants
        private static readonly object[] SearchTypes = { "Songs", "Playlists", "Albums" };

        //Parsing
        //Yes, I could have done this better, take a method and make a regex. I just dont care now
	    private const string Pic500Regex = "pic500:\"(.*?)\"";
        private const string Pic40Regex = "picSD:\"(.*?)\"";
        private const string SessionIdRegex = "\"sessionID\":\"(.*?)\"";
        private const string HeadersRegex = "{client:\"(.*?)\",clientRevision:\"(.*?)\"}";
        private const string EndpointRegex = "defaultEndpoint:\"(.*?)\"";
        private const string ConfigRegex = "GS.config = ([^;]*)";

        //Api
        private readonly HttpClient _client;
        private readonly IPlatformAdaptor _adaptor;
        private JObject _header;
        private string _sessionId;
        private string _currentToken;
	    private JObject _country;// = new JObject { { "ID", 223 }, { "CC1", 0 }, { "CC2", 0 }, { "CC3", 0 }, { "CC4", 1073741824 }, { "DMA", 506 }, { "IPR", 0 } };


        public bool HasLoaded
        {
            get
            {
                return _sessionId != null;
            }
        }

        public GroovesharkApi(IPlatformAdaptor adaptor)
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/38.0.2125.111 Safari/537.36");
            _adaptor = adaptor;
        }

        //Get website, parse session
        public async Task<bool> LoadAsync()
        {
            //Get html
            HttpResponseMessage message = await _client.GetAsync(Homepage);
            if (message.StatusCode == HttpStatusCode.NotFound)
                return false;

            string content = await message.Content.ReadAsStringAsync();
            _sessionId = Regex.Match(content, SessionIdRegex).Groups[1].Value;
            string configJson = Regex.Match(content, ConfigRegex).Groups[1].Value;
            JObject config = (JObject)JsonConvert.DeserializeObject(configJson);
            _country = config["country"] as JObject;

            //Get JS library
            message = await _client.GetAsync(Homepage + "/" + LibraryPath);
            content = await message.Content.ReadAsStringAsync();

            Match match = Regex.Match(content, HeadersRegex);
            Client = match.Groups[1].Value;
            ClientRevision = match.Groups[2].Value;

            match = Regex.Match(content, EndpointRegex);
            Endpoint = match.Groups[1].Value;
            RevToken = "gooeyFlubber";//This should come from app.js, but the proper way will take more time than I feel like it now

            match = Regex.Match(content, Pic500Regex);
            Pic500Url = match.Groups[1].Value;

            match = Regex.Match(content, Pic40Regex);
            Pic40Url = match.Groups[1].Value;

            //Construct the json headers
            _header = new JObject
                {
                    { "client", Client },
                    { "clientRevision", ClientRevision },
                    { "privacy", Privacy },
                    { "country", _country },
                    { "uuid", GenerateUUID() },
                    { "session", _sessionId }
                };

            return true;
        }

        public async Task GetCommunicationTokenAsync()
        {
            if (!HasLoaded)
                throw new Exception();
            
            JObject parameters = new JObject
                {
                    { "secretKey", MD5.GetHashString(_sessionId).ToLower() }
                };

            ApiResult<string> result = await GetApiResult<string>(tokenMethod, parameters, false);
            if (result.Fault != null)
                throw result.Fault.Exception;

            _currentToken = result.Result;
        }

	    public async Task<ApiLoginResult> AuthenticateUserAsync(string username, string password)
	    {
            if (!HasLoaded)
                throw new Exception();
            
            JObject parameters = new JObject
                {
                    { "username", username },
                    { "password", password }
                };

            ApiResult<ApiLoginResult> result = await GetApiResult<ApiLoginResult>(loginUsermethod, parameters);
            if (result.Fault != null)
                throw result.Fault.Exception;

	        return result.Result;
	    }

        public async Task<ApiFavoriteResult> UnfavoriteSongAsync(int songId)
        {
            if (!HasLoaded)
                throw new Exception();

            JObject parameters = new JObject
                {
                    { "what", "Song" },
                    { "ID", songId },
                };

            ApiResult<ApiFavoriteResult> result = await GetApiResult<ApiFavoriteResult>(favoriteMethod, parameters);
            if (result.Fault != null)
                throw result.Fault.Exception;

            return result.Result;
        }

	    public async Task<ApiFavoriteResult> FavoriteSongAsync(int songId, ApiFavoriteSong song)
	    {
            if (!HasLoaded)
                throw new Exception();

            JObject parameters = new JObject
                {
                    { "what", "Song" },
                    { "ID", songId },
                    { "details",  JObject.FromObject(song) }
                };

            ApiResult<ApiFavoriteResult> result = await GetApiResult<ApiFavoriteResult>(favoriteMethod, parameters);
            if (result.Fault != null)
                throw result.Fault.Exception;

            return result.Result;
	    }

	    public async Task<List<ApiAlbumSong>> GetFavoritesAsync(int userId)
	    {
            if (!HasLoaded)
                throw new Exception();

            JObject parameters = new JObject
                {
                    { "ofWhat", "Songs" },
                    { "userID", userId },
                };

            ApiResult<List<ApiAlbumSong>> result = await GetApiResult<List<ApiAlbumSong>>(getFavoritesMethod, parameters);
            if (result.Fault != null)
                throw result.Fault.Exception;

            return result.Result;
	    }

	    public async Task<bool> AddSongToPlaylist(int playlistId, int songId)
	    {
            if (!HasLoaded)
                throw new Exception();

            JObject parameters = new JObject
                {
                    { "playlistID", playlistId },
                    { "songID", songId }
                };

            ApiResult<int> result = await GetApiResult<int>(addSongsToPlaylistMethod, parameters, client: "htmlshark");
            if (result.Fault != null)
                throw result.Fault.Exception;

	        return result.Result == 1;
	    }

        public async Task<bool> MarkStream30SecondsAsync(ApiStreamData data)
        {
            if (!HasLoaded)
                throw new Exception();

            JObject parameters = new JObject
                {
                    { "streamKey", data.streamKey },
                    { "streamServerID", data.streamServerID },
                    { "songID", data.SongID }
                };

            ApiResult<ApiStream30Result> result = await GetApiResult<ApiStream30Result>(markStream30SecondsMethod, parameters);
            if (result.Fault != null)
                throw result.Fault.Exception;

            return result.Result.success;
        }

        public async Task<bool> MarkSongCompleteAsync(ApiStreamData data)
        {
            if (!HasLoaded)
                throw new Exception();

            JObject parameters = new JObject
                {
                    { "streamKey", data.streamKey },
                    { "streamServerID", data.streamServerID },
                    { "songID", data.SongID }
                };

            ApiResult<ApiSongCompleteResult> result = await GetApiResult<ApiSongCompleteResult>(markSongDownloadedMethod, parameters);
            if (result.Fault != null)
                throw result.Fault.Exception;

            return result.Result.Return == null;
        }

	    public async Task<bool> MarkSongDownloaded(ApiStreamData data)
	    {
            if (!HasLoaded)
                throw new Exception();

            JObject parameters = new JObject
                {
                    { "streamKey", data.streamKey },
                    { "streamServerID", data.streamServerID },
                    { "songID", data.SongID }
                };

            ApiResult<ApiSongMarkedDownloadedResult> result = await GetApiResult<ApiSongMarkedDownloadedResult>(markSongDownloadedMethod, parameters);
            if (result.Fault != null)
                throw result.Fault.Exception;

            return result.Result.Return;
	    }

	    public async Task LogoutUserAsync(int userId)
	    {
            if (!HasLoaded)
                throw new Exception();
            
            JObject parameters = new JObject
                {
                    { "userID", userId },
                };

            ApiResult<ApiLoginResult> result = await GetApiResult<ApiLoginResult>(logoutMethod, parameters);
            if (result.Fault != null)
                throw result.Fault.Exception;
	    }

        public async Task<ApiSearchResult> SearchAsync(string query)
        {
            if (!HasLoaded)
                throw new Exception();
            
            JObject parameters = new JObject
                {
                    { "query", query },
                    { "type", new JArray(SearchTypes) },
                    { "guts", 0 },
                    {"ppOverride", String.Empty}
                };

            var result = await GetApiResult<ApiSearch>(searchMethod, parameters);
            if (result.Fault != null)
                throw result.Fault.Exception;

            return result.Result.Result;//Yea, blame their API
        }

	    public async Task<ApiUserPlaylistResult> GetUserPlaylists(int userId)
	    {
            if (!HasLoaded)
                throw new Exception();

            JObject parameters = new JObject
	        {
	            {"userID", userId}
	        };

            var result = await GetApiResult<ApiUserPlaylistResult>(getUserPlaylistMethod, parameters);
            if (result.Fault != null)
                throw result.Fault.Exception;

            return result.Result;
	    }

	    public async Task<ApiPlaylistResult> GetPlaylistById(int playlistID)
	    {
	        if (!HasLoaded)
                throw new Exception();

	        JObject parameters = new JObject
	        {
	            {"playlistID", playlistID}
	        };

            var result = await GetApiResult<ApiPlaylistResult>(getPlaylistMethod, parameters);
            if (result.Fault != null)
                throw result.Fault.Exception;

	        return result.Result;
	    }
        public async Task<ApiAlbumResult> GetAlbumById(string albumID)
        {
            if (!HasLoaded)
                throw new Exception();

            JObject parameters = new JObject
	        {
	            {"albumID", albumID}
	        };

            var result = await GetApiResult<ApiAlbumResult>(getAlbumMethod, parameters);
            if (result.Fault != null)
                throw result.Fault.Exception;

            return result.Result;
        }

        public async Task<List<ApiAlbumSong>> GetAlbumSongs(string albumID)
        {
            if (!HasLoaded)
                throw new Exception();

            JObject parameters = new JObject
	        {
	            {"albumID", albumID}
	        };

            var result = await GetApiResult<List<ApiAlbumSong>>(getAlbumSongsMethod, parameters);
            if (result.Fault != null)
                throw result.Fault.Exception;

            return result.Result;
        }

        public async Task DownloadSongAsync(ApiSong song)
        {
            if (!HasLoaded)
                throw new Exception();
            
            string url = await GetStreamUrlAsync(song);
            Stream stream = await _client.GetStreamAsync(url);
            _adaptor.SaveStream(song, stream);
        }

        public Task<string> GetStreamUrlAsync(ApiSong song)
        {
            return GetStreamUrlAsync(song.SongID);
        }

	    public async Task<string> GetStreamUrlAsync(string songId)
	    {
            ApiStreamData data = await GetSongStreamDataAsync(songId);
            return GetStreamUrl(data);
	    }

	    public Task<ApiStreamData> GetSongStreamDataAsync(ApiSong song)
	    {
	        return GetSongStreamDataAsync(song.SongID);
	    }
        
        public async Task<ApiStreamData> GetSongStreamDataAsync(string songId)
	    {
            if (!HasLoaded)
                throw new Exception();

            ApiStreamData data = await GetStreamDataAsync(int.Parse(songId));
            return data;
	    }

	    public string GetStreamUrl(ApiStreamData data)
	    {
            if (!HasLoaded)
                throw new Exception();

            return "http://" + data.ip + "/" + StreamEndpoint + data.streamKey;
	    }


	    public string GetImageAlbumArtUrl(ApiSong song)
	    {
	        if (song == null)
	            throw new ArgumentNullException();
            
            return Pic500Url + song.CoverArtFilename;
	    }

        public string GetImageThumbAlbumArtUrl(ApiSong song)
        {
            if (song == null)
                throw new ArgumentNullException();

            return Pic40Url + song.CoverArtFilename;
        }

        public string GetImageThumbAlbumArtUrl(ApiAlbum album)
        {
            if (album == null)
                throw new ArgumentNullException();

            return Pic40Url + album.CoverArtFilename;
        }

	    public string GetImageThumbAlbumArtUrl(string coverArtFilename)
	    {
            return Pic40Url + coverArtFilename;
	    }

	    private async Task<ApiStreamData> GetStreamDataAsync(int songId)
	    {
            if (!HasLoaded)
                throw new Exception();
            
            JObject parameters = new JObject
                {
                    { "prefetch", false },
                    { "mobile", true },
                    { "songID", songId },
                    {"country", _country}
                };

            var result = await GetApiResult<ApiStreamData>(streamKeyMethod, parameters);
            if (result.Fault != null)
                throw result.Fault.Exception;

            return result.Result;
	    }

        private async Task<ApiResult<T>> GetApiResult<T>(string method, JObject parameters, bool header = true, string client = null)
        {
            if (header)
                _header["token"] = GenerateToken(method);

            if (client != null)
                _header["client"] = client;

            JObject request = new JObject
                {
                    { "header", _header },
                    { "method", method },
                    { "parameters", parameters }
                };
            
            string json = JsonConvert.SerializeObject(request, Formatting.None);
            HttpContent content = new StringContent(json);

            HttpResponseMessage result = await _client.PostAsync(Homepage + Endpoint + "?" + method, content);
            string resultJson = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ApiResult<T>>(resultJson);
        }

		private string GenerateUUID()
		{
            Random random = new Random();
            StringBuilder key = new StringBuilder("xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx");
            for (int i = 0; i < key.Length; i++)
            {
                if ((key[i] != 'x') && (key[i] != 'y'))
                    continue;

                int value = random.Next(16);
                if (key[i] != 'x')
                    value = value & 3 | 8;
                key[i] = value.ToString("X")[0];
            }
            
            return key.ToString();
		}

        //app.js - 776
        private string GenerateToken(string methodName)
        {
            //Generate last randomizer - app.js 560
            Random random = new Random();
            string lastRandomizer = String.Empty;
            for (int i = 0; i < 6; i++)
                lastRandomizer +=  random.Next(16).ToString("X").ToLower();

            //Pretty cool code I think
            //string test = new Array[6].Select(i => random.Next(16).ToString("X")).Aggregate((a, b) => a + b).ToLower();

            string[] parts = { methodName, _currentToken, RevToken, lastRandomizer };
            string concat = parts.Aggregate((a, b) => a + ":" + b);
            string hash = _adaptor.ComputeSha1Hash(concat).ToLower();
            return lastRandomizer + hash;
        }
	}
}
