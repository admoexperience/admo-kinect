using System;
using System.IO;
using System.Threading;

namespace AdmoCertificateManager
{
    internal class Program
    {
        private static void Main(string[] args)
        {
          
           var tempLog= File.Create("CertManagerDebug.log");

            var file = new StreamWriter(tempLog);           
            var portNumber = CertificateHandler.DefaultPort;

            //Usage is AdmoCertificateManger.exe $PORT_NUMBER
            if (args.Length > 0)
            {
                portNumber = args[0];
            }

            var certHandler = new CertificateHandler(portNumber);

            var myCert = certHandler.GetAdmoCert();
            if (!myCert.HasPrivateKey)
                file.WriteLine("Certificate does not have a private key");

            certHandler.AddCertToStore();
            var authloc = certHandler.CheckIfInStore(certHandler.CertStoreAuth, myCert);
            var localloc = certHandler.CheckIfInStore(certHandler.CertStoerLocal, myCert);


            if (authloc && localloc)
            {
                file.WriteLine("Certificate loaded");
                Thread.Sleep(1000);
                file.WriteLine(certHandler.DeleteOldCerts());
                file.WriteLine(certHandler.BindApp2Cert());
                file.WriteLine(certHandler.GrantPermissionToUseUrl());
                Thread.Sleep(2000);
            }
            else
            {
                file.WriteLine("Failed To load certificate " + authloc.ToString() + localloc.ToString());
                Thread.Sleep(1000);
               
            }
            file.Close();
        }
    }
}