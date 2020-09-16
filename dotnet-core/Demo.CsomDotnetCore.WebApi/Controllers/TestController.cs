using System.Threading.Tasks;
using Demo.CsomDotnetCore.Core;
using Demo.CsomDotnetCore.WebApi.CSOM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Demo.CsomDotnetCore.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class TestController : ControllerBase
    {
        private readonly IConfiguration _config;

        public TestController(IConfiguration config)
        {
            _config = config;
        }

        public async Task<IActionResult> Csom()
        {
            var siteCollectionUri = _config.GetValue<string>(Globals.AppSettings.SiteCollectionUri);

            // 1. Easy to set up with username & password (can be used for local development)
            var clientId = _config.GetValue<string>(Globals.AppSettings.ClientId);
            var tenantUrl = _config.GetValue<string>(Globals.AppSettings.TenantUrl);
            var userUpn = _config.GetValue<string>(Globals.AppSettings.UserUpn);
            var userPassword = _config.GetValue<string>(Globals.AppSettings.UserPassword);
            var webTitle = await CsomHelper.UsingUserNamePassword(clientId, userUpn, userPassword, siteCollectionUri);

            // 2. Easy to set up (deployed app only)
//            var webTitle = await CsomHelper.UsingManagedIdentity(tenantUrl, siteCollectionUri);

            // 3. Recommended approach to set up AppOnly context in .NET CORE (consistent across local dev & deployed app), see ReadMe.md on how to configure this.
            //            var webTitle = await CsomHelper.UsingCertificate(_config, siteCollectionUri);

            return Ok(webTitle);
        }
    }
}
