using System;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace Demo.CsomDotnetCore.Core.Authentication
{
    public class DefaultAzureCredentialAccessTokenProvider : IAccessTokenProvider
    {
        private const string CachePrefix = nameof(DefaultAzureCredentialAccessTokenProvider);

        private IConfiguration _config;
        private readonly IMemoryCache _memoryCache;

        public DefaultAzureCredentialAccessTokenProvider(IMemoryCache memoryCache, IConfiguration config)
        {
            _config = config;
            _memoryCache = memoryCache;
        }

        public async Task<AccessToken> EnsureAsync()
        {
            var tenantName = _config.GetValue<string>(Globals.AppSettings.TenantName);
            var resource = $"https://{tenantName}.sharepoint.com/.default";

            var cacheKey = $"{CachePrefix}_{resource}";
            
            if (
                !_memoryCache.TryGetValue<AccessToken>(cacheKey, out var token) ||
                token.ExpiresOn < DateTimeOffset.Now.AddSeconds(-5)
            )
            {
                var at = await AcquireTokenAsync(resource);
                token = new AccessToken(at.Token, at.ExpiresOn);

                _memoryCache.Set(cacheKey, token.Token, token.ExpiresOn.AddSeconds(-5));
            }

            return token;
        }

        private async Task<Azure.Core.AccessToken> AcquireTokenAsync(string resource)
        {
            var accessToken = await new DefaultAzureCredential(new DefaultAzureCredentialOptions
                {
                    ExcludeInteractiveBrowserCredential = true
                })
                .GetTokenAsync(new TokenRequestContext(new[] { resource }));

            return accessToken;
        }
    }
}