using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdmoInstallerCustomAction
{
    public partial class DownloadRuntime : Form
    {
        public DownloadRuntime()
        {
            InitializeComponent();
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
            this.BeginInvoke((MethodInvoker)delegate
            {
                double bytesIn = double.Parse(e.BytesReceived.ToString());
                double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
                double percentage = bytesIn / totalBytes * 100;
                label2.Text = "Downloading " + e.BytesReceived + " of " + e.TotalBytesToReceive;
                progressBar1.Value = int.Parse(Math.Truncate(percentage).ToString());
            });
        }
        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            this.BeginInvoke((MethodInvoker)delegate
            {
                label2.Text = "Completed";
            });
            label2.Text = "Installing";
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
    }
}
