using System;
using System.Threading.Tasks;
using Demo.CsomDotnetCore.Core;
using Demo.CsomDotnetCore.Core.Authentication;
using Demo.CsomDotnetCore.Services.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.SharePoint.Client;

namespace Demo.CsomDotnetCore.Services
{
    public class SharePointService : ISharePointService
    {
        private readonly IAccessTokenProvider _accessTokenProvider;
        private readonly IConfiguration _config;
        private readonly ClientContext _ctx;

        public SharePointService(IAccessTokenProvider accessTokenProvider, IConfiguration config)
        {
            _accessTokenProvider = accessTokenProvider;
            _config = config;

            var tenantUrl = _config.GetValue<string>(Globals.AppSettings.TenantUrl);

            _ctx = GetContext(tenantUrl);
        }

        private ClientContext GetContext(string scUrl)
        {
            var ctx = new ClientContext(new Uri(scUrl));

            ctx.ExecutingWebRequest += (sender, e) =>
            {
                var accessToken = _accessTokenProvider.EnsureAsync().GetAwaiter().GetResult().Token;
                e.WebRequestExecutor.RequestHeaders["Authorization"] = "Bearer " + accessToken;
            };

            return ctx;
        }

        public async Task<string> GetWebTitle(string siteName)
        {
            if (string.IsNullOrEmpty(siteName))
            {
                _ctx.Load(_ctx.Web, p => p.Title, p => p.Url);
                await _ctx.ExecuteQueryAsync();
                return $"web title: {_ctx.Web.Title}";
            }
            else
            {
                var scUrl = _config.GetValue<string>(Globals.AppSettings.TenantUrl) + "/sites/" + siteName;
                var ctx = GetContext(scUrl);
                ctx.Load(ctx.Web, p => p.Title, p => p.Url);
                await ctx.ExecuteQueryAsync();
                return $"web title: {ctx.Web.Title}";
            }
        }
    }
}
