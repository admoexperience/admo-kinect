using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using AdmoCertificateManager;
using Microsoft.Deployment.WindowsInstaller;
using Microsoft.Win32;
using System.Windows;

namespace AdmoInstallerCustomAction
{
    public class CustomActions
    {
        [STAThread]
        [CustomAction]
        public static ActionResult DownLoadRuntime(Session session)
        {
            //To do put form with progress bar
            session.Log("Begin DownLoadRuntime");
#if DEBUG
            Debugger.Launch();
#endif
            if (CheckInstalled("Kinect")) return ActionResult.Success;
            session.Log("Begin Downloading");
            var t = new Thread(() =>
            {
                var app = new Application();

                app.Run(new DownloadRuntimeWPF());
            });
            t.SetApartmentState(ApartmentState.STA);

            t.Start();
            t.Join();
            t.Abort();
            session.Log("DownLoad Complete");

          
            session.Log("Install Complete");
            //    return p.StandardOutput.ReadToEnd();

            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult GetPcSpecs(Session session)
        {
            //To do put form with progress bar
            session.Log("Begin GetPcSpecs");
#if DEBUG
            Debugger.Launch();
#endif
            //  if (CheckInstalled("Kinect")) return ActionResult.Success;
            session.Log("Begin Downloading");
            var t = new Thread(() =>
            {
                var app = new Application();

                app.Run(new CheckPCStats());
            });
            t.SetApartmentState(ApartmentState.STA);

            t.Start();
            t.Join();
             t.Abort();
            // app.Run(new DownloadRuntimeWPF());
            // form.ShowDialog();
            session.Log("PC Spec Check Complete");
            session.Log("Install Complete");
            //    return p.StandardOutput.ReadToEnd();

           return ActionResult.Success;
        }


        [CustomAction]
        public static ActionResult LoadCertificates(Session session)
        {

#if DEBUG
            Debugger.Launch();
#endif
            var installLoc = session.CustomActionData["InstallLoc"];
            if (installLoc == "")
            {
                installLoc = @"C:\Admo";
            }

            session.Log("Begin LoadCertificates");
            const string portNumber = CertificateHandler.DefaultPort;

            ////Usage is AdmoCertificateManger.exe $PORT_NUMBER

            var certHandler = new CertificateHandler(portNumber);
            try
            {

                var myCert = certHandler.GetAdmoCert(installLoc);
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

        public static bool CheckInstalled(string cName)
        {

            string registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKey);
            if (CheckIfKeyPresentCheckInstalled(cName, key)) return true;

            registryKey = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
            key = Registry.LocalMachine.OpenSubKey(registryKey);
            if (CheckIfKeyPresentCheckInstalled(cName, key)) return true;

            registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            key = Registry.CurrentUser.OpenSubKey(registryKey);
            if (CheckIfKeyPresentCheckInstalled(cName, key)) return true;

            registryKey = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
            key = Registry.CurrentUser.OpenSubKey(registryKey);
            if (CheckIfKeyPresentCheckInstalled(cName, key)) return true;

            return false;
        }

        private static bool CheckIfKeyPresentCheckInstalled(string cName, RegistryKey key)
        {
            if (key != null)
            {
                foreach (RegistryKey subkey in key.GetSubKeyNames().Select(keyName => key.OpenSubKey(keyName)))
                {
                    string displayName = subkey.GetValue("DisplayName") as string;
                  //  displayName.Trim() == cName;
                    if (displayName != null && displayName.Contains(cName))
                    {
                        return true;
                    }
                }
                key.Close();
            }
            return false;
        }
    }
}
