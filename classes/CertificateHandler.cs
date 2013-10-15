using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using NLog;
using Admo.classes;

namespace Admo.classes
{
    public class CertificateHandler
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static X509Certificate2 AdmoCert =new X509Certificate2(@"C:\smartroom\AdmoCert.cer");
        //public static X509Certificate2 AdmoCert = CertificateGenerator.CreateSelfSigned("AdmoCertificate", DateTime.Now,
                                                                                   //     DateTime.Now.AddYears(15));
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
          

            var certStorAuth = new X509Store(StoreName.AuthRoot, StoreLocation.LocalMachine);
            certStorAuth.Open(OpenFlags.ReadWrite);
            var certStorLocal = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            certStorLocal.Open(OpenFlags.ReadWrite);

            try
            {
                certStorAuth.Remove(AdmoCert);

            }
            catch (Exception)
            {

                Logger.Debug("failed to Remove cert to Authority store");
            }

            try
            {
                certStorLocal.Remove(AdmoCert);

            }
            catch (Exception)
            {

                Logger.Debug("failed to Remove cert to store");
            }

            certStorLocal.Close();
            certStorLocal.Close();
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
           
            var p = new Process
                        {
                            StartInfo =
                                {
                                    FileName = "netsh.exe",
                                    Arguments = "http add sslcert ipport=0.0.0.0:9000 " +
                                                "appid={74CE5CF2-1171-4AAC-935E-F3E1A0267AD8} certhash=" +
                                                AdmoCert.GetCertHashString(),
                                    UseShellExecute = false,
                                    RedirectStandardOutput = true
                                }
                        };
            p.Start();

           return p.StandardOutput.ReadToEnd();
        }
        public static string CreateCert()
        {
            //Makecert -r -pe -n CN="www.admoexperience.com" -b 05/10/2013 -e 12/22/2031 -eku 1.3.6.1.5.5.7.3.1 C:\smartroom\AdmoCert.cer  

            var p = new Process
            {
                StartInfo =
                {
                    FileName = "makecert.exe",
                    Arguments = @"-r -pe -n CN=www.admoExperience.com -b 05/10/2013 -e 12/22/2031 -eku 1.3.6.1.5.5.7.3.1 C:\smartroom\AdmoCert.cer",
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                }
            };
            p.Start();

            return p.StandardOutput.ReadToEnd();
        }

        
    }
}
