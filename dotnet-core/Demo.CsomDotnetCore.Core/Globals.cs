namespace Demo.CsomDotnetCore.Core
{
    public static class Globals
    {
        public static class AppSettings
        {
            public static readonly string AzureCertificateThumbprint = "AzureCertificateThumbprint";
            public static readonly string ClientId = "ClientId";
            public static readonly string DevelopmentCertificatePath = "DevelopmentCertificatePath";
            public static readonly string DevelopmentCertificatePassword = "DevelopmentCertificatePassword";
            public static readonly string SiteCollectionUri = "SiteCollectionUri"; // e.g. https://<tenantName>.sharepoint.com/sites/<siteName>
            public static readonly string TenantDomain = "TenantDomain"; // e.g. <myTenantName>.onmicrosoft.com
            public static readonly string TenantUrl = "TenantUrl"; // e.g. https://<tenantName>.sharepoint.com
            public static readonly string UserPassword = "UserPassword";
            public static readonly string UserUpn = "UserUpn";
        }
    }
}