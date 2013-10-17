using System;
using System.Diagnostics;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using NLog;

namespace Admo.classes
{
    public class CertificateHandler
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static X509Certificate2 AdmoCert
        {
            get
            {
                return new X509Certificate2("C:/smartroom/certs/bundle.p12","1234");
            }
        }
        //public static X509Certificate2 AdmoCert = CertificateGenerator.CreateSelfSigned("AdmoCertificate", DateTime.Now,
         //                                                                               DateTime.Now.AddYears(15), "1234");
        public static X509Store CertStorAuth = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
        public static X509Store CertStorLocal = new X509Store(StoreName.My, StoreLocation.LocalMachine);

       

        public static void AddCertToStore()
        {
           
            var certStorAuth = new X509Store(StoreName.Root, StoreLocation.LocalMachine);         
            var certStorLocal = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            
            try
            {
                certStorAuth.Open(OpenFlags.ReadWrite);
                certStorAuth.Add(AdmoCert);
                certStorAuth.Close();
            }
            catch (Exception)
            {

                Logger.Debug("failed to add cert to Authority store");
            }

            try
            {
                certStorLocal.Open(OpenFlags.ReadWrite);
                certStorLocal.Add(AdmoCert);
                certStorLocal.Close();
            }
            catch (Exception)
            {

                Logger.Debug("failed to add cert to my store");
            }
           
        }
        public static void RemoveFromStore()
        {
          
            try
            {
                var certStorAuth = new X509Store(StoreName.AuthRoot, StoreLocation.LocalMachine);
                certStorAuth.Open(OpenFlags.ReadWrite);

                certStorAuth.Remove(AdmoCert);
                certStorAuth.Close();
            }
            catch (Exception)
            {

                Logger.Debug("failed to Remove cert to Authority store");
            }


            try
            {
                var certStorLocal = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                certStorLocal.Open(OpenFlags.ReadWrite);

                certStorLocal.Remove(AdmoCert);
                certStorLocal.Close();


            }
            catch (Exception)
            {

                Logger.Debug("failed to Remove cert to store");
            }

        }
        public static bool CheckIfInStore(X509Store store,X509Certificate2 cert )
        {
            store.Open(OpenFlags.ReadOnly);
            var storeCerts=store.Certificates;
            foreach (X509Certificate2 storecert in storeCerts)
            {
                if (storecert.GetHashCode() == cert.GetHashCode())
                {
                    store.Close();
                    return true;
                }
            }

            store.Close();
            return false;
        }

        public static string BindApp2Cert()
        {
            //var args = new string[] {"http", "add", "sslcert", "ipport=0.0.0.0:9000","appid={74CE5CF2-1171-4AAC-935E-F3E1A0267AD8}","certhash=" +
              //   
            string arguments = "http add sslcert ipport=0.0.0.0:5500 " +
                               "appid={74CE5CF2-1171-4AAC-935E-F3E1A0267AD8} certhash=" +
                               AdmoCert.GetCertHashString();
            var p = new Process
                        {
                            StartInfo =
                                {
                                    FileName = "netsh.exe",
                                    Arguments = arguments,
                                    UseShellExecute = false,
                                    RedirectStandardOutput = true
                                }
                        };
            p.Start();
            
           return p.StandardOutput.ReadToEnd();
        }
        public static string DeleteOldCerts()
        {

            const string arguments2 = "http delete sslcert ipport=0.0.0.0:5500";
            var p = new Process
            {
                StartInfo =
                {
                    FileName = "netsh.exe",
                    Arguments = arguments2,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                }
            };
            p.Start();
            return p.StandardOutput.ReadToEnd();

        }
        public static string CreateCert()
        {
            //Makecert -r -pe -n CN="www.admoexperience.com" C:\smartroom\AdmoCert.cer -b 05/10/2013 -e 12/22/2031 -eku 1.3.6.1.5.5.7.3.1 -sr localmachine    

            var p = new Process
            {
                StartInfo =
                {
                    FileName = "makecert.exe",
                    Arguments = @"Makecert -r -pe -n CN=www.admoexperience.com C:\smartroom\AdmoCert.cer -b 05/10/2013 -e 12/22/2031 -eku 1.3.6.1.5.5.7.3.1 -sr localmachine -sv C:\smartroom\AdmoCert.pvk",
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                }
            };
            p.Start();

            return p.StandardOutput.ReadToEnd();
        }

        
    }
}
