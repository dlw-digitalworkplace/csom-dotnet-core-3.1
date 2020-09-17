using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace Demo.CsomDotnetCore.Core.Authentication
{
    public class UserCredentialAccessTokenProvider : IAccessTokenProvider
    {
        private const string CachePrefix = nameof(UserCredentialAccessTokenProvider);
        private const string TokenEndpoint = "https://login.microsoftonline.com/common/oauth2/token";

        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;

        public UserCredentialAccessTokenProvider(IMemoryCache memoryCache, IConfiguration config)
        {
            _config = config;
            _httpClient = new HttpClient();
            _memoryCache = memoryCache;
        }

        public async Task<AccessToken> EnsureAsync()
        {
            var tenantUrl = _config.GetValue<string>(Globals.AppSettings.TenantUrl);
            var cacheKey = $"{CachePrefix}_{tenantUrl}";

            var clientId = _config.GetValue<string>(Globals.AppSettings.ClientId);
            var userUpn = _config.GetValue<string>(Globals.AppSettings.UserUpn);
            var userPassword = _config.GetValue<string>(Globals.AppSettings.UserPassword);


            if (
                !_memoryCache.TryGetValue<AccessToken>(cacheKey, out var token) ||
                token.ExpiresOn < DateTimeOffset.Now.AddSeconds(-5)
            )
            {
                var accessToken = await AcquireTokenAsync(new Uri(tenantUrl), clientId, userUpn, userPassword);

                token = new AccessToken(accessToken, DateTimeOffset.Now.AddMinutes(59));

                _memoryCache.Set(cacheKey, token.Token, token.ExpiresOn.AddSeconds(-5));
            }

            return token;
        }

        private async Task<string> AcquireTokenAsync(Uri resourceUri, string clientId, string username, string password)
        {
            var resource = $"{resourceUri.Scheme}://{resourceUri.DnsSafeHost}";
            var body = $"resource={resource}&client_id={clientId}&grant_type=password&username={HttpUtility.UrlEncode(username)}&password={HttpUtility.UrlEncode(password)}";
            using (var stringContent = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded"))
            {

                var result = await _httpClient.PostAsync(TokenEndpoint, stringContent).ContinueWith((response) =>
                {
                    return response.Result.Content.ReadAsStringAsync().Result;
                }).ConfigureAwait(false);

                var tokenResult = JsonSerializer.Deserialize<JsonElement>(result);
                var token = tokenResult.GetProperty("access_token").GetString();
                return token;
            }
        }
    }
}