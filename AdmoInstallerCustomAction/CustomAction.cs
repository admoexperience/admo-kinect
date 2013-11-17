using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using AdmoCertificateManager;
using Microsoft.Deployment.WindowsInstaller;

namespace AdmoInstallerCustomAction
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult DownLoadRuntime(Session session)
        {

            Debugger.Launch();

            session.Log("Begin DownLoadRuntime");
            var client = new WebClient();
            client.DownloadFile("http://admo-downloads.s3-website-eu-west-1.amazonaws.com/unit-installers/KinectRuntime-v1.8-Setup.exe", @"KinectRuntime-v1.8-Setup.exe");

            var p = new Process
            {
                StartInfo =
                {
                    FileName = "KinectRuntime-v1.8-Setup.exe",
                    Arguments = "",
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                }
            };
            p.Start();

        //    return p.StandardOutput.ReadToEnd();


            return ActionResult.Success;
        }
        [CustomAction]
        public static ActionResult LoadCertificates(Session session)
        {
            Debugger.Launch();
            session.Log("Begin LoadCertificates");
            const string portNumber = CertificateHandler.DefaultPort;

            ////Usage is AdmoCertificateManger.exe $PORT_NUMBER

            var certHandler = new CertificateHandler(portNumber);
            try
            {

                var myCert = certHandler.GetAdmoCert();
                if (!myCert.HasPrivateKey)
                    session.Log("Certificate does not have a private key");




                var authloc = certHandler.CheckIfInStore(certHandler.CertStoreAuth, myCert);
                var localloc = certHandler.CheckIfInStore(certHandler.CertStoerLocal, myCert);


                if (authloc && localloc)
                {
                    session.Log("Certificate loaded");
                    session.Log(certHandler.DeleteOldCerts());
                    session.Log(certHandler.BindApp2Cert());
                    session.Log(certHandler.GrantPermissionToUseUrl());
                }
                else
                {
                    session.Log("Failed To load certificate " + authloc.ToString() + localloc.ToString());
                }
            }
            catch (Exception)
            {
                
            }
            return ActionResult.Success;
        }

    }
}
