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
            this.Cursor = Cursors.Wait;
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
                var unit = JsonHelper.ConvertFromApiRequest<Unit>(result);
                if (!unit.ContainsErrors())
                {
                    File.WriteAllText(classes.Config.GetLocalConfig("ApiKey"), unit.ApiKey);

                    var unitApi = new CmsApi(unit.ApiKey);
                    var stringval = await unitApi.GetConfig();
                    File.WriteAllText(classes.Config.GetCmsConfigCacheFile(), stringval);
                    var main = new MainWindow();
                    main.Show();
                    Close();
                }
                else
                {
                    Logger.Error("Error: "+ unit.Error);
                    ErrorsField.Text = "errors: " + unit.Error;
                }
            }
            catch (Exception ee)
            {
                Logger.Error("Unable to parse json from server for RegisterDevice", ee);
                Logger.Error(ee.ToString);
                ErrorsField.Text = ee.ToString();
            }
            this.Cursor = Cursors.Arrow;
        }
    }
}