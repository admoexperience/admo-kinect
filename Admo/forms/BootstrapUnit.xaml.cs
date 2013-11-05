using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Admo.classes;
using Admo.classes.lib;
using Admo.Api;
using Admo.Api.Dto;
using Admo.Utilities;
using NLog;

namespace Admo.forms
{
    /// <summary>
    /// Interaction logic for BootstrapUnit.xaml
    /// </summary>
    public partial class BootstrapUnit : Window
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public BootstrapUnit()
        {
            InitializeComponent();
        }

        private async void btn1_Click(object sender, RoutedEventArgs e)
        {
            var username = UserNameTextField.Text;
            var password = PasswordField.Password;
            var device = DeviceNameField.Text;
            var api = new CmsAccountApi
                {
                    Email = username,
                    Password = password
                };
            var result = await api.RegisterDevice(device);
            try
            {
                var parsed = Utils.ParseJson(result);
                if (parsed.ContainsKey("unit"))
                {
                    var unitInfo = Utils.ParseJson(parsed["unit"].ToString());
                    var apiKey = unitInfo["api_key"].ToString();
                    File.WriteAllText(Config.GetLocalConfig("ApiKey"), apiKey);

                    var unitApi = new CmsApi(apiKey);
                    var stringval = await unitApi.GetConfig();
                    File.WriteAllText(Config.GetCmsConfigCacheFile(), stringval);
                    var main = new MainWindow();
                    main.Show();
                    Close();
                }
                else
                {
                    ErrorsField.Text = "errors: " + parsed["error"];
                }
            }
            catch (Exception ee)
            {
                Logger.Error("Unable to parse json from server for RegisterDevice", ee);
                ErrorsField.Text = ee.ToString();
            }
        }
    }
}