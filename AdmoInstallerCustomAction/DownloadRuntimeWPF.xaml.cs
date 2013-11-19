using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Application = System.Windows.Application;

namespace AdmoInstallerCustomAction
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class DownloadRuntimeWPF:Window
    {
        public DownloadRuntimeWPF()
        {
            InitializeComponent();

            LoginBox.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(DownloadStart_MouseUp), true);

            Closed += BootstrapUnit_Closed;
        }

        private void BootstrapUnit_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();

        }



        public void StartDownload()
        {
            var bgThead = new Thread(() =>
            {
                var client = new WebClient();
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
                client.DownloadFileAsync(new Uri("http://admo-downloads.s3-website-eu-west-1.amazonaws.com/unit-installers/KinectRuntime-v1.8-Setup.exe"),
                    //client.DownloadFileAsync(new Uri("http://silverserver/public/Admo/Installers/KinectRuntime-v1.8-Setup.exe"),
                @"KinectRuntime-v1.8-Setup.exe");
            });
            bgThead.Start();
            //     client.DownloadFileAsync(new Uri("http://admo-downloads.s3-website-eu-west-1.amazonaws.com/unit-installers/KinectRuntime-v1.8-Setup.exe"),

        }
        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Dispatcher.BeginInvoke((MethodInvoker)delegate
            {
                double bytesIn = double.Parse(e.BytesReceived.ToString());
                double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
                double percentage = bytesIn / totalBytes * 100;
                UnderLable.Text = "Downloaded " + e.BytesReceived + " of " + e.TotalBytesToReceive;
                ProgressBar1.Value = int.Parse(Math.Truncate(percentage).ToString());
            });
        }
        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Dispatcher.BeginInvoke((MethodInvoker)delegate
            {
                UnderLable.Text = "Completed";
            });
            UnderLable.Text = "Installing";
            var p = new Process
            {
                StartInfo =
                {
                    FileName = "KinectRuntime-v1.8-Setup.exe",
                    Arguments = "",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    Verb = "runas"
                }
            };
            p.Start();
            //      p.WaitForExit();
            Close();

        }

        private void DownloadStart_MouseUp(object sender, MouseButtonEventArgs e)
        {
            StartDownload();
        }
    }
}
