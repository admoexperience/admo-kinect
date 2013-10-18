﻿using System;
using System.Threading;

namespace AdmoCertificateManager
{
    class Program
    {
        static void Main(string[] args)
        {



            if (args.Length > 0)
            {
                CertificateHandler.Port = args[0];
            }

                var myCert = CertificateHandler.AdmoCert;
                if (!myCert.HasPrivateKey)
                    Console.WriteLine("Thats a bit of a bother");

                CertificateHandler.AddCertToStore();
                var authloc = CertificateHandler.CheckIfInStore(CertificateHandler.CertStorAuth, myCert);
                var localloc = CertificateHandler.CheckIfInStore(CertificateHandler.CertStorLocal, myCert);

               

                if (authloc && localloc)
                {
                    Console.WriteLine("Certificate loaded");
                    Thread.Sleep(1000);
                    Console.WriteLine(CertificateHandler.DeleteOldCerts());
                    Console.WriteLine(CertificateHandler.BindApp2Cert());
                    Console.WriteLine(CertificateHandler.GrantPermissionToUseUrl());
                    Thread.Sleep(2000);
                }
                else
                {
                    Console.WriteLine("Failed To load certificate " + authloc.ToString()+ localloc.ToString());
                    Thread.Sleep(1000);
                }
            

        }

       
    }
}
