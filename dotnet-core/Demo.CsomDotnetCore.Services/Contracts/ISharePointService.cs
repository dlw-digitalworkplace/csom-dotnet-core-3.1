using System.Threading.Tasks;

namespace Demo.CsomDotnetCore.Services.Contracts
{
    public interface ISharePointService
    {
        Task<string> GetWebTitle(string site);
    }
}
