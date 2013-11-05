using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Admo.Api;
using Admo.Api.Dto;
using Admo.classes;
using Admo.classes.lib;
using Admo.forms;
using Admo.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;

namespace Admo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private async void ApplicationStartup(object sender, StartupEventArgs e)
        {
            //
            Admo.classes.Config.InitDirs();
            var api = new CmsApi("api_key");
            var jsonAppList = await api.GetAppList();
            var podList = JsonHelper.ConvertFromApiRequest<PodApp>(jsonAppList);
            Logger.Debug(podList);
            
            var mainWindow = new MainWindow();
            var bootstrapWindow = new BootstrapUnit();
            var hasConfig = Admo.classes.Config.HasApiKey();
            if (hasConfig)
            {
                mainWindow.WindowState = WindowState.Minimized;
                mainWindow.Show(); 
            }
            else
            {
                Logger.Info("ApiKey not found loading bootstrapscreen");
                bootstrapWindow.Show();
            }
        }
    }
}
