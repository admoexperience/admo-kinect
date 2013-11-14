using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
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
            PasswordMaskBox.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(PasswordMaskBox_MouseDown), true);
            DeviceNameField.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(DeviceNameField_Selected), true);
            LoginBox.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(Login_MouseUp), true);

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
        }

        private async void Login()
        {

     
           
            Cursor = Cursors.Wait;
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
                    ErrorsField.Document.Blocks.Clear();
                    ErrorsField.Document.Blocks.Add(new Paragraph(new Run("errors: " + unit.Error)));
                   
                }
            }
            catch (Exception ee)
            {
                Logger.Error("Unable to parse json from server for RegisterDevice", ee);
                Logger.Error(ee.ToString);
                ErrorsField.Document.Blocks.Clear();
                ErrorsField.Document.Blocks.Add(new Paragraph(new Run(ee.ToString())));

            }
            this.Cursor = Cursors.Arrow;

            ErrorsField.Document.Blocks.Clear();
            ErrorsField.Document.Blocks.Add(new Paragraph(new Run("Don't have an account?")));


            FlowDocument doc = new FlowDocument();
            ErrorsField.Document = doc;
            ErrorsField.IsReadOnly = true;
            ErrorsField.IsDocumentEnabled = true;
 
            Paragraph para = new Paragraph();
            doc.Blocks.Add(para);
            
            var link = new Hyperlink {IsEnabled = true};
            link.Inlines.Add("Sign Up");
            link.NavigateUri = new Uri("https://cms.admoexperience.com/users/sign_in");
            link.Click += link_Click;
          //  para.FontSize = 12;
            para.Inlines.Add("Don't have an account?");
            para.Inlines.Add(link);

         //   var hyperl = new Hyperlink { NavigateUri = new Uri("https://cms.admoexperience.com/users/sign_in") };

          //  hyperl.RequestNavigate += RequestNavigateHandler;
            // To handle all Hyperlinks in the RichTextBox
            ErrorsField.AddHandler(Hyperlink.RequestNavigateEvent,
               new RequestNavigateEventHandler(RequestNavigateHandler));
        }

        private void link_Click(object sender, RoutedEventArgs e)
        {
            //Process.Start(e.ToString());
        }

        private void RequestNavigateHandler(object sender, RequestNavigateEventArgs e)
        {
                Process.Start(e.Uri.ToString());
        }

        private void PasswordMaskBox_MouseDown(object sender, MouseButtonEventArgs e)
        {

            PasswordMaskBox.Visibility = Visibility.Hidden;
            PasswordField.Focus();
        }

        private void Login_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Login();
        }


        private void DeviceNameField_Selected(object sender, MouseButtonEventArgs e)
        {
            DeviceNameField.Text = Environment.MachineName;
        }
    }
}