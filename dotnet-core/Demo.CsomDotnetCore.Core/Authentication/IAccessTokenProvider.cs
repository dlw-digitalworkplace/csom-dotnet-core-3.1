using System.Threading.Tasks;

namespace Demo.CsomDotnetCore.Core.Authentication
{
    public interface IAccessTokenProvider
    {
        Task<AccessToken> EnsureAsync();
    }
}