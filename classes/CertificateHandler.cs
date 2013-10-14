using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Admo.classes
{
    public class CertificateHandler
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static X509Certificate2 AdmoCert =new X509Certificate2(@"C:\smartroom\AdmoWebUiLocalhostCert.cer");

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
    }
}
