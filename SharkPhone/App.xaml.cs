using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Xml.Serialization;
using Coding4Fun.Toolkit.Controls;
using Microsoft.Phone.BackgroundAudio;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using SharkLibrary.Api;
using SharkLibrary.Models;
using SharkPhone.Models;
using SharkPhone.Resources;

namespace SharkPhone
{
    public delegate void Error(object sender, ErrorEventArgs args);
    public partial class App : Application
    {
        public const string ApiKey = "<GroovesharkApiKey>";
        public const string ApiSecret = "<GroovesharkApiSecret>";
        
        private static readonly Mutex FileLock = new Mutex(false, "SharkPhone_PlayRequestMutex");
        
        //This is a binary semaphore, as Mutexes must be released on the thread that requested it. No guarantees with the threads in the HeadlessHost.exe that runs the BackgroundAgent code
        //http://msdn.microsoft.com/en-us/library/system.threading.semaphore(v=vs.110).aspx
        public static readonly Semaphore AppSemaphore = new Semaphore(0, 1, "SharkPhone_AppLock");//If the agent sees a record, it will load
        public static readonly Semaphore AgentSemaphore = new Semaphore(0, 1, "SharkPhone_AgentLock");//If the app sees a record, it will load

        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public static PhoneApplicationFrame RootFrame { get; private set; }

        //User collections
        public static PlaylistListResult UserPlaylists { get; private set; }
        public static SongListResult UserFavorites { get; private set; }

        //State
        public static GroovesharkPublicApi Api = new GroovesharkPublicApi(ApiKey, ApiSecret, 100);

        public static Song SelectedSong;//For viewing in Song page
        public static bool ShowNowPlaying;

        //Login
        private const string CredentialsFilename = "credentials.xml";

        //Save playlists so background player can see them
        private const string PlaylistFileName = "playlist.xml";

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions.
            UnhandledException += Application_UnhandledException;

            // Standard XAML initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // Language display initialization
            InitializeLanguage();

            // Show graphics profiling information while debugging.
            if (Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                //Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode,
                // which shows areas of a page that are handed off to GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Prevent the screen from turning off while under the debugger by disabling
                // the application's idle detection.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }

            ThemeManager.ToDarkTheme();

            ShowNowPlaying = false;
            DeletePlayRequest();

            BackgroundAudioPlayer.Instance.PlayStateChanged += Instance_PlayStateChanged;
        }

        void Instance_PlayStateChanged(object sender, EventArgs e)
        {
            var error = BackgroundAudioPlayer.Instance.Error;
            if (error != null)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    ToastPrompt prompt = new ToastPrompt
                    {
                        TextOrientation = Orientation.Vertical,
                        Title = "Unexpected error occured",
                        Message = "Please restart SharkPhone and try again",
                        ImageSource = new BitmapImage(new Uri("/Assets/Icons/questionmark.png", UriKind.Relative))
                    };
                    prompt.Show();
                });
            }
        }

        public static Credentials GetCredentials()
        {
            return Utilities.LoadObject<Credentials>(CredentialsFilename);
        }

        public async static Task<bool> LoginAsync(string username, string password)
        {
            var result = await Api.AuthenticateAsync(username, password);
            if (result.success == false)
            {
                return false;
            }

            //Save credentials and result
            Credentials credentials = new Credentials { Username = username, Password = password };
            Utilities.SaveObject(CredentialsFilename, credentials);

            //Load playlists and libraries
            UserPlaylists = await App.Api.GetUserPlaylistsAsync();

            //Load favorites
            UserFavorites =  await App.Api.GetUserFavoritesAsync();

            return true;
        }

        public async static Task LogoutAsync()
        {
            //Logout
            await App.Api.LogoutAsync();

            //Clear
            App.UserPlaylists = null;
            App.UserFavorites = null;

            //Delete
            using (IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (file.FileExists(CredentialsFilename))
                    file.DeleteFile(CredentialsFilename);
            }
        }

        //Blocks while Agent initializes
        public static void LoadPlaybackAgent()
        {
            App.AppSemaphore.Release();
            
            PlayRequest request = new PlayRequest
            {
                Type = PlayRequestType.Load,
                SessionId = Api.SessionId,
                Country = Api.Country
            };
            SetPlayRequest(request, null);
            BackgroundAudioPlayer.Instance.Play();

            App.AgentSemaphore.WaitOne();
        }

        public static void SetPlayRequest(PlayRequest playlist, string name)
        {
            FileLock.WaitOne();

            if (playlist.Type != PlayRequestType.Load)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    ToastPrompt prompt = new ToastPrompt
                    {
                        TextOrientation = Orientation.Vertical,
                        Title = name,
                        Message = String.Format("Loading {0}...", playlist.Type.ToString()),
                        ImageSource = new BitmapImage(new Uri("/Assets/Icons/download.png", UriKind.Relative))
                    };
                    prompt.Show();
                });
            }

            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream file = storage.OpenFile(PlaylistFileName, FileMode.Create))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(PlayRequest));
                    serializer.Serialize(file, playlist);
                }
            }

            FileLock.ReleaseMutex();
        }

        public static void DeletePlayRequest()
        {
            FileLock.WaitOne();
            
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (storage.FileExists(PlaylistFileName))
                {
                    storage.DeleteFile(PlaylistFileName);
                }
            }

            FileLock.ReleaseMutex();
        }


        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            //Ads
            AdManager.UpdateInAppPurchases();
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            //Ads
            AdManager.UpdateInAppPurchases();
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Handle reset requests for clearing the backstack
            RootFrame.Navigated += CheckForResetNavigation;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        private void CheckForResetNavigation(object sender, NavigationEventArgs e)
        {
            // If the app has received a 'reset' navigation, then we need to check
            // on the next navigation to see if the page stack should be reset
            if (e.NavigationMode == NavigationMode.Reset)
                RootFrame.Navigated += ClearBackStackAfterReset;
        }

        private void ClearBackStackAfterReset(object sender, NavigationEventArgs e)
        {
            // Unregister the event so it doesn't get called again
            RootFrame.Navigated -= ClearBackStackAfterReset;

            // Only clear the stack for 'new' (forward) and 'refresh' navigations
            if (e.NavigationMode != NavigationMode.New && e.NavigationMode != NavigationMode.Refresh)
                return;

            // For UI consistency, clear the entire page stack
            while (RootFrame.RemoveBackEntry() != null)
            {
                ; // do nothing
            }
        }

        #endregion

        // Initialize the app's font and flow direction as defined in its localized resource strings.
        //
        // To ensure that the font of your application is aligned with its supported languages and that the
        // FlowDirection for each of those languages follows its traditional direction, ResourceLanguage
        // and ResourceFlowDirection should be initialized in each resx file to match these values with that
        // file's culture. For example:
        //
        // AppResources.es-ES.resx
        //    ResourceLanguage's value should be "es-ES"
        //    ResourceFlowDirection's value should be "LeftToRight"
        //
        // AppResources.ar-SA.resx
        //     ResourceLanguage's value should be "ar-SA"
        //     ResourceFlowDirection's value should be "RightToLeft"
        //
        // For more info on localizing Windows Phone apps see http://go.microsoft.com/fwlink/?LinkId=262072.
        //
        private void InitializeLanguage()
        {
            try
            {
                // Set the font to match the display language defined by the
                // ResourceLanguage resource string for each supported language.
                //
                // Fall back to the font of the neutral language if the Display
                // language of the phone is not supported.
                //
                // If a compiler error is hit then ResourceLanguage is missing from
                // the resource file.
                RootFrame.Language = XmlLanguage.GetLanguage(AppResources.ResourceLanguage);

                // Set the FlowDirection of all elements under the root frame based
                // on the ResourceFlowDirection resource string for each
                // supported language.
                //
                // If a compiler error is hit then ResourceFlowDirection is missing from
                // the resource file.
                FlowDirection flow = (FlowDirection)Enum.Parse(typeof(FlowDirection), AppResources.ResourceFlowDirection);
                RootFrame.FlowDirection = flow;
            }
            catch
            {
                // If an exception is caught here it is most likely due to either
                // ResourceLangauge not being correctly set to a supported language
                // code or ResourceFlowDirection is set to a value other than LeftToRight
                // or RightToLeft.

                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }

                throw;
            }
        }
    }
}