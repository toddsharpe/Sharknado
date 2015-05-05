using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using SharkLibrary.Api;
using SharkLibrary.ApiModels;

namespace SharkWindows
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Options Options;
        public static ObservableCollection<ApiSong> Downloads = new ObservableCollection<ApiSong>();
        public static GroovesharkApi Api = new GroovesharkApi(new WpfPlatformAdaptor());
 
        protected override void OnStartup(StartupEventArgs e)
        {
            Options = Options.Load();
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Options.Save();
            base.OnExit(e);
        }
    }
}
