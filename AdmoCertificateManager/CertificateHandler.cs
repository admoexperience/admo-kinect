using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;


namespace AdmoCertificateManager
{
    public class CertificateHandler
    {
       
        public X509Store CertStoreAuth = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
        public X509Store CertStoerLocal = new X509Store(StoreName.My, StoreLocation.LocalMachine);
        public readonly string Port;
        public const string DefaultPort = "5001";
        public const string DefaultCertFile = "bundle.p12";
        public const string DefaultPassword = "1234";

        public X509Certificate2 AdmoCert
        {
            get
            {
                return new X509Certificate2(DefaultCertFile, DefaultPassword,
                    X509KeyStorageFlags.PersistKeySet |
                    X509KeyStorageFlags.Exportable | 
                    X509KeyStorageFlags.MachineKeySet);
            }
        }

        public CertificateHandler(String portNumber)
        {
            Port = portNumber;
        }


        public void AddCertToStore()
        {
            Console.WriteLine("Attempting to add it to AuthStore");
            AddCertToStore(CertStoreAuth);
            Console.WriteLine("Attempting to add it to MyStore");
            AddCertToStore(CertStoerLocal);
        }

        public void RemoveFromStore()
        {
            Console.WriteLine("Attempting to remove it from AuthStore");
            RemoveFromStore(CertStoreAuth);
            Console.WriteLine("Attempting to remove it from MyStore");
            RemoveFromStore(CertStoerLocal);
        }


        public bool CheckIfInStore(X509Store store, X509Certificate2 cert)
        {
            store.Open(OpenFlags.ReadOnly);
            var storeCerts = store.Certificates;
            var found = storeCerts.Cast<X509Certificate2>().Any(storecert => storecert.GetHashCode() == cert.GetHashCode());
            store.Close();
            return found;
        }

        public string BindApp2Cert()
        {
            return RunNetshCommand("http add sslcert ipport=0.0.0.0:" + Port +" "+
                               "appid={74CE5CF2-1171-4AAC-935E-F3E1A0267AD8} certhash=" +
                               AdmoCert.GetCertHashString());
        }


        public string DeleteOldCerts()
        {
            return RunNetshCommand(@"http delete sslcert ipport=0.0.0.0:" + Port);
        }


        public string GrantPermissionToUseUrl()
        {
            return RunNetshCommand("http add urlacl url=https://+:" + Port + @"/ user=""NT AUTHORITY\Authenticated Users""");
        }


        private void AddCertToStore(X509Store store)
        {
            try
            {
                store.Open(OpenFlags.MaxAllowed);
                store.Add(AdmoCert);
                store.Close();
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to add cert to " + store.Name);
            }
        }


        private void RemoveFromStore(X509Store store)
        {
            try
            {
                store.Open(OpenFlags.ReadWrite);
                store.Remove(AdmoCert);
                store.Close();
            }
            catch (Exception)
            {
                Console.WriteLine("failed to Remove cert to " + store.Name);
            }
        }


        private static string RunNetshCommand(string cmd)
        {
            var p = new Process
            {
                StartInfo =
                {
                    FileName = "netsh.exe",
                    Arguments = cmd,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                }
            };
            p.Start();
            return p.StandardOutput.ReadToEnd();
        }


    }
}
