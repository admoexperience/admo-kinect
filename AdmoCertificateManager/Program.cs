using System;
using System.Threading;
using Admo.classes;

namespace AdmoCertificateManager
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length == 0 )
            {
                var myCert = CertificateHandler.AdmoCert;

                CertificateHandler.AddCertToStore();
                var authloc = CertificateHandler.CheckIfInStore(CertificateHandler.CertStorAuth, myCert);
                var localloc = CertificateHandler.CheckIfInStore(CertificateHandler.CertStorLocal, myCert);
                if (authloc && localloc)
                {
                    Console.WriteLine(@"Certificate in loaded");
                }
                else
                {
                    Console.WriteLine(@"Failed To load certificate" + authloc.ToString()+ localloc.ToString());
                    Thread.Sleep(1000);
                }
            }
            else if(args[0] == "TestIt")
            {

                var myCert = CertificateHandler.AdmoCert;
                CertificateHandler.RemoveFromStore();
                Console.WriteLine((!CertificateHandler.CheckIfInStore(CertificateHandler.CertStorAuth, myCert)).ToString() + "\n");
                Console.WriteLine((!CertificateHandler.CheckIfInStore(CertificateHandler.CertStorLocal, myCert)).ToString() + "\n");
                CertificateHandler.AddCertToStore();

                Console.WriteLine((CertificateHandler.CheckIfInStore(CertificateHandler.CertStorAuth, myCert)).ToString() + "\n");
                Console.WriteLine((CertificateHandler.CheckIfInStore(CertificateHandler.CertStorLocal, myCert)).ToString() + "\n");
                
            }
        }

       
    }
}
