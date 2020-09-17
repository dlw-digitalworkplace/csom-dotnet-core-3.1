using System;

namespace Demo.CsomDotnetCore.Core.Authentication
{
    public class AccessToken
    {
        public AccessToken(string token, DateTimeOffset expiresOn)
        {
            Token = token;
            ExpiresOn = expiresOn;
        }

        public string Token { get; set; }

        public DateTimeOffset ExpiresOn { get; set; }
    }
}