using System;
using System.Threading;

namespace AdmoCertificateManager
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var portNumber = CertificateHandler.DefaultPort;

            //Usage is AdmoCertificateManger.exe $PORT_NUMBER
            if (args.Length > 0)
            {
                portNumber = args[0];
            }

            var certHandler = new CertificateHandler(portNumber);

            var myCert = certHandler.AdmoCert;
            if (!myCert.HasPrivateKey)
                Console.WriteLine("Certificate does not have a private key");

            certHandler.AddCertToStore();
            var authloc = certHandler.CheckIfInStore(certHandler.CertStoreAuth, myCert);
            var localloc = certHandler.CheckIfInStore(certHandler.CertStoerLocal, myCert);


            if (authloc && localloc)
            {
                Console.WriteLine("Certificate loaded");
                Thread.Sleep(1000);
                Console.WriteLine(certHandler.DeleteOldCerts());
                Console.WriteLine(certHandler.BindApp2Cert());
                Console.WriteLine(certHandler.GrantPermissionToUseUrl());
                Thread.Sleep(2000);
            }
            else
            {
                Console.WriteLine("Failed To load certificate " + authloc.ToString() + localloc.ToString());
                Thread.Sleep(1000);
            }
        }
    }
}