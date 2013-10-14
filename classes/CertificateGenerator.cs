using System;
using System.IO;
using System.Security;
using System.Security.Cryptography.X509Certificates;

namespace Admo.classes
{
    public static class CertificateGenerator
    {
        /// <summary>
        /// Create a self-signed SSL certificate
        /// </summary>
        /// <param name="distinguishedName">Certificate distinguished name</param>
        /// <param name="validFrom">Start time certificate is considered valid from</param>
        /// <param name="validUntil">End time certificate is considered valid until</param>
        /// <returns>Certificate</returns>
        public static X509Certificate2 CreateSelfSigned(string distinguishedName, DateTime validFrom, DateTime validUntil)
        {
            return CreateSelfSigned(distinguishedName, validFrom, validUntil, (SecureString)null);
        }

        /// <summary>
        /// Create a self-signed SSL certificate using a secured passphrase
        /// </summary>
        /// <param name="distinguishedName">Certificate distinguished name</param>
        /// <param name="validFrom">Start time certificate is considered valid from</param>
        /// <param name="validUntil">End time certificate is considered valid until</param>
        /// <param name="passphrase">Passphrase used to protect private key</param>
        /// <returns>Certificate</returns>
        public static X509Certificate2 CreateSelfSigned(string distinguishedName, DateTime validFrom, DateTime validUntil, SecureString passphrase)
        {
            var rawCert = File.ReadAllBytes("AdmoWebUiLocalhostCert.cer");
            return new X509Certificate2(rawCert);

        }

        /// <summary>
        /// Create a self-signed SSL certificate using a passphrase
        /// </summary>
        /// <param name="distinguishedName">Certificate distinguished name</param>
        /// <param name="validFrom">Start time certificate is considered valid from</param>
        /// <param name="validUntil">End time certificate is considered valid until</param>
        /// <param name="passphrase">Passphrase used to protect private key</param>
        /// <returns>Certificate</returns>
        public static X509Certificate2 CreateSelfSigned(string distinguishedName, DateTime validFrom, DateTime validUntil, string passphrase)
        {
            SecureString securePassphrase = null;

            try
            {
                if (!string.IsNullOrEmpty(passphrase))
                {
                    securePassphrase = new SecureString();
                    foreach (var c in passphrase)
                    {
                        securePassphrase.AppendChar(c);
                    }
                    securePassphrase.MakeReadOnly();
                }

                return CreateSelfSigned(distinguishedName, validFrom, validUntil, securePassphrase);
            }
            finally
            {
                if (securePassphrase != null)
                {
                    securePassphrase.Dispose();
                }
            }
        }
    }
}
