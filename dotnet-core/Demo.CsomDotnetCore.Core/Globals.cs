namespace Demo.CsomDotnetCore.Core
{
    public static class Globals
    {
        public static class AppSettings
        {

            public static readonly string ClientId = "ClientId";
            public static readonly string DevCertificatePath = "DevCertificatePath";
            public static readonly string TenantId = "TenantId";
            public static readonly string TenantName = "TenantName";
            public static readonly string TenantUrl = "TenantUrl"; // e.g. https://<tenantName>.sharepoint.com
            public static readonly string UserPassword = "UserPassword";
            public static readonly string UserUpn = "UserUpn";
        }

        public static class AuthSettings
        {
            public static readonly string AZURE_CLIENT_ID = "AZURE_CLIENT_ID";
            public static readonly string AZURE_CLIENT_CERTIFICATE_PATH = "AZURE_CLIENT_CERTIFICATE_PATH";
            public static readonly string AZURE_TENANT_ID = "AZURE_TENANT_ID";
        }
    }
}