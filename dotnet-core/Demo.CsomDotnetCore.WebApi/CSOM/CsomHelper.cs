using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Demo.CsomDotnetCore.Core;
using Demo.CsomDotnetCore.Core.Utility;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.SharePoint.Client;

namespace Demo.CsomDotnetCore.WebApi.CSOM
{
    public class CsomHelper
    {
        public static async Task<string> UsingUserNamePassword(string aadAppId, string username, string password, string siteCollectionUri)
        {
            var site = new Uri(siteCollectionUri);

            // Note: The PnP Sites Core AuthenticationManager class also supports this
            using (var authenticationManager = new AuthenticationManagerUsernamePw(aadAppId))
            using (var context = authenticationManager.GetContext(site, username, password))
            {
                context.Load(context.Web, p => p.Title);
                await context.ExecuteQueryAsync();
                Console.WriteLine($"Title: {context.Web.Title}");
                return context.Web.Title;
            }
        }

        public static async Task<string> UsingManagedIdentity(string tenantUrl, string siteCollectionUri) // Follow steps from: https://www.rickvanrousselt.com/stop-using-clientid-and-secret-to-access-your-office-365-services/
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider(); // uses Microsoft.Azure.Services.AppAuthentication
            var accessToken = await azureServiceTokenProvider.GetAccessTokenAsync(tenantUrl);

            var context = new ClientContext(siteCollectionUri);

            context.ExecutingWebRequest += (sender, args) =>
            {
                args.WebRequestExecutor.RequestHeaders["Authorization"] = "Bearer " + accessToken;
            };

            context.Load(context.Web, p => p.Title);
            await context.ExecuteQueryAsync();
            return context.Web.Title;
        }

        public static async Task<string> UsingCertificate(IConfiguration config, string siteCollectionUri) // recommended approach
        {
            var devCertPath = config.GetValue<string>(Globals.AppSettings.DevelopmentCertificatePath);

            ClientContext context;

            if (string.IsNullOrEmpty(devCertPath))
            {
                context = new AuthenticationManager().GetAzureADAppOnlyAuthenticatedContext(
                    siteCollectionUri,
                    config.GetValue<string>(Globals.AppSettings.ClientId),
                    config.GetValue<string>(Globals.AppSettings.TenantDomain),
                    StoreName.My,
                    StoreLocation.CurrentUser,
                    config.GetValue<string>(Globals.AppSettings.AzureCertificateThumbprint)
                );
            }
            else
            {
                context = new AuthenticationManager().GetAzureADAppOnlyAuthenticatedContext(
                    siteCollectionUri,
                    config.GetValue<string>(Globals.AppSettings.ClientId),
                    config.GetValue<string>(Globals.AppSettings.TenantDomain),
                    config.GetValue<string>(Globals.AppSettings.DevelopmentCertificatePath),
                    config.GetValue<string>(Globals.AppSettings.DevelopmentCertificatePassword)
                );
            }

            context.Load(context.Web, p => p.Title);
            await context.ExecuteQueryAsync();
            return context.Web.Title;
        }
    }
}