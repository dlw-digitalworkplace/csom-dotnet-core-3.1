using System;
using System.Globalization;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Demo.CsomDotnetCore.Core.Enums;
using Demo.CsomDotnetCore.Core.Extensions;
using Demo.CsomDotnetCore.Core.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.SharePoint.Client;

namespace Demo.CsomDotnetCore.Core.Utility
{
    public class AuthenticationManager
    {
        public ClientContext GetAzureADAppOnlyAuthenticatedContext(string siteUrl, string clientId, string tenant, StoreName storeName, StoreLocation storeLocation, string thumbPrint, AzureEnvironment environment = AzureEnvironment.Production)
        {
            var cert = X509CertificateUtility.LoadCertificate(storeName, storeLocation, thumbPrint);

            return GetAzureADAppOnlyAuthenticatedContext(siteUrl, clientId, tenant, cert, environment);
        }

        public ClientContext GetAzureADAppOnlyAuthenticatedContext(string siteUrl, string clientId, string tenant, string certificatePath, string certificatePassword, AzureEnvironment environment = AzureEnvironment.Production)
        {
            var certPassword = EncryptionUtility.ToSecureString(certificatePassword);

            return GetAzureADAppOnlyAuthenticatedContext(siteUrl, clientId, tenant, certificatePath, certPassword, environment);
        }

        public ClientContext GetAzureADAppOnlyAuthenticatedContext(string siteUrl, string clientId, string tenant, string certificatePath, SecureString certificatePassword, AzureEnvironment environment = AzureEnvironment.Production)
        {
            var certfile = System.IO.File.OpenRead(certificatePath);
            var certificateBytes = new byte[certfile.Length];
            certfile.Read(certificateBytes, 0, (int)certfile.Length);
            var cert = new X509Certificate2(
                certificateBytes,
                certificatePassword,
                X509KeyStorageFlags.Exportable |
                X509KeyStorageFlags.MachineKeySet |
                X509KeyStorageFlags.PersistKeySet);

            return GetAzureADAppOnlyAuthenticatedContext(siteUrl, clientId, tenant, cert, environment);
        }

        public ClientContext GetAzureADAppOnlyAuthenticatedContext(string siteUrl, string clientId, string tenant, X509Certificate2 certificate, AzureEnvironment environment = AzureEnvironment.Production)
        {
            var clientContext = new ClientContext(siteUrl);

            string authority = string.Format(CultureInfo.InvariantCulture, "{0}/{1}/", "https://login.microsoftonline.com", tenant);

            var authContext = new AuthenticationContext(authority);

            var clientAssertionCertificate = new ClientAssertionCertificate(clientId, certificate);

            var host = new Uri(siteUrl);

            clientContext.ExecutingWebRequest += (object sender, WebRequestEventArgs args) =>
            {
                var ar = Task.Run(() => authContext
                    .AcquireTokenAsync(host.Scheme + "://" + host.Host + "/", clientAssertionCertificate))
                    .GetAwaiter().GetResult();
                args.WebRequestExecutor.WebRequest.Headers["Authorization"] = "Bearer " + ar.AccessToken;
                args.WebRequestExecutor.WebRequest.UserAgent = "NONISV|Customer|Project/1.0"; // todo: fill out customer & project in UserAgent
            };

            ClientContextSettings clientContextSettings = new ClientContextSettings
            {
                Type = ClientContextType.AzureADCertificate,
                SiteUrl = siteUrl,
                AuthenticationManager = this,
                ClientId = clientId,
                Tenant = tenant,
                Certificate = certificate,
                Environment = environment
            };

            clientContext.AddContextSettings(clientContextSettings);

            return clientContext;
        }

    }
}
