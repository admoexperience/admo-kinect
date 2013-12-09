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
using AdmoShared.Utilities;
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
            PasswordField.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(PasswordField_Selected), true);
            DeviceNameField.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(DeviceNameField_Selected), true);

            UserNameTextField.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(UserNameTextField_Selected), true);

            LoginBox.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(Login_MouseUp), true);

            Closed += BootstrapUnit_Closed;
            Loaded += OnLoaded;
        }

        private void BootstrapUnit_Closed(object sender, EventArgs e)
        {
           Application.Current.Shutdown();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var doc = new FlowDocument();
            ErrorsField.Document = doc;
            ErrorsField.IsReadOnly = true;
            ErrorsField.IsDocumentEnabled = true;

            Paragraph para = new Paragraph();
            doc.Blocks.Add(para);
            //  para.FontSize = 12;
            para.Inlines.Add("Set to (local) if you dont want to sign up");
        }

        private async void Login()
        {
            Cursor = Cursors.Wait;
            var username = UserNameTextField.Text;
            var password = PasswordField.Password;
            var device = DeviceNameField.Text;
            var baseUrl = CmsUrl.Text;
            if (baseUrl == "  Cloud service location")
            {
                baseUrl = "https://cms.admoexperience.com/api/v1";
            }

            if (device == "  Cloud service location")
            {
                device = Environment.MachineName;
            }

            File.WriteAllText(classes.Config.GetLocalConfig("BaseCmsUrl"), baseUrl);
            if (baseUrl=="local")
            {
                //open straight away if local config is used
                var main = new MainWindow();
                main.Show();
                Close();
            }

            var api = new CmsAccountApi(baseUrl)
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
                    var unitApi = new CmsApi(unit.ApiKey, baseUrl);
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
            link.NavigateUri = new Uri("https://cms.admoexperience.com/users/sign_up");
            link.Click += link_Click;
          //  para.FontSize = 12;
            para.Inlines.Add("Don't have an account? ");
            para.Inlines.Add(link);

          //  hyperl.RequestNavigate += RequestNavigateHandler;
            // To handle all Hyperlinks in the RichTextBox
            ErrorsField.AddHandler(Hyperlink.RequestNavigateEvent,
               new RequestNavigateEventHandler(RequestNavigateHandler));
            CmsUrl.Visibility = Visibility.Visible;
        }

        private void link_Click(object sender, RoutedEventArgs e)
        {
            //Process.Start(e.ToString());
        }

        private void RequestNavigateHandler(object sender, RequestNavigateEventArgs e)
        {
                Process.Start(e.Uri.ToString());
        }

        private void PasswordMaskBox_MouseDown(object sender, RoutedEventArgs routedEventArgs)
        {
            RemoveAllBorders();
            PasswordMaskBox.Visibility = Visibility.Hidden;

            PasswordField.Focus();
            PasswordField.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#49A8DE"));
            PasswordField.BorderThickness = new Thickness(2);

        }

        private void Login_MouseUp(object sender, MouseButtonEventArgs e)
        {
            RemoveAllBorders();

            Login();
        }


        private void DeviceNameField_Selected(object sender, RoutedEventArgs routedEventArgs)
        {
            RemoveAllBorders();
            DeviceNameField.Text = Environment.MachineName;
            setBorder(DeviceNameField);

        }

        private void RemoveAllBorders()
        {

            if (DeviceNameField.Text == "")
            {
                DeviceNameField.Text = "  Device Name";
            }
            if (UserNameTextField.Text == "")
            {
                UserNameTextField.Text = "  Email";
            }
            if (PasswordField.Password == "")
            {
                PasswordMaskBox.Visibility = Visibility.Visible;

            }
            DeviceNameField.BorderThickness = new Thickness(0);
            PasswordField.BorderThickness = new Thickness(0);
            UserNameTextField.BorderThickness = new Thickness(0);
            CmsUrl.BorderThickness = new Thickness(0);
        }

        private void ResetifEmpty()
        {
          
            DeviceNameField.BorderThickness = new Thickness(0);
            PasswordField.BorderThickness = new Thickness(0);
            UserNameTextField.BorderThickness = new Thickness(0);
            CmsUrl.BorderThickness = new Thickness(0);

        }
        private void setBorder(TextBox txBox)
        {
            txBox.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#49A8DE"));
            txBox.BorderThickness = new Thickness(2);
        }

        private void UserNameTextField_Selected(object sender, RoutedEventArgs routedEventArgs)
        {
            RemoveAllBorders();
            UserNameTextField.Text = "";
            setBorder(UserNameTextField);
        }

        private void PasswordField_Selected(object sender, RoutedEventArgs routedEventArgs)
        {
            RemoveAllBorders();

            if (PasswordMaskBox.IsVisible)
            {
                PasswordMaskBox.Visibility = Visibility.Hidden;
            }

            PasswordField.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#49A8DE"));
            PasswordField.BorderThickness = new Thickness(2);
        }

        private void CmsUrl_Selected(object sender, RoutedEventArgs e)
        {
            RemoveAllBorders();

            CmsUrl.Text = "https://cms.admoexperience.com/api/v1";
            setBorder(CmsUrl);
        }
    }
}