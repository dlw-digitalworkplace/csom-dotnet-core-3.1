using System.Threading.Tasks;
using Demo.CsomDotnetCore.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Demo.CsomDotnetCore.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class TestController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ISharePointService _spService;

        public TestController(IConfiguration config, ISharePointService spService)
        {
            _config = config;
            _spService = spService;
        }

        public async Task<IActionResult> GetWebTitle(string site)
        {
            var webTitle = await _spService.GetWebTitle(site);
            return Ok(webTitle);
        }
    }
}
