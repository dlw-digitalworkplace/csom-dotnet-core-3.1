using System;
using System.Net;
using System.Threading.Tasks;
using Demo.CsomDotnetCore.Core.Models;
using Microsoft.SharePoint.Client;

namespace Demo.CsomDotnetCore.Core.Extensions
{
    public static class ClientContextExtensions
    {
        private const string PnPSettingsKey = "SharePointPnP$Settings$ContextCloning";

        public static async Task ExecuteQueryRetry(this ClientContext clientContext, int retryCount = 10, int delay = 500)
        {
            if (retryCount <= 0)
            {
                throw new ArgumentException("Provide a retry count greater than zero.");

            }
            if (delay <= 0)
            {
                throw new ArgumentException("Provide a delay greater than zero.");
            }

            bool retry = false;
            ClientRequestWrapper wrapper = null;
            var retryNumber = 0;

            while (retryNumber < retryCount)
            {
                try
                {
                    if (!retry || wrapper == null || wrapper.Value == null)
                    {
                        await clientContext.ExecuteQueryAsync();
                    }
                    else
                    {
                        await clientContext.RetryQueryAsync(wrapper.Value);
                    }
                    break;
                }
                catch (WebException wex)
                {
                    var response = wex.Response as HttpWebResponse;
                    Console.WriteLine(wex.Message);

                    // Check if request was throttled - http status code 429
                    // Check is request failed due to server unavailable - http status code 503
                    if (response != null &&
                        (response.StatusCode == (HttpStatusCode)429
                        || response.StatusCode == (HttpStatusCode)503
                        ))
                    {
                        retryNumber++;
                        if (retryNumber >= retryCount)
                        {
                            throw new Exception($"Maximum retry attempts {retryCount} has been attempted.");
                        }
                        else
                        {
                            wrapper = (ClientRequestWrapper)wex.Data["ClientRequest"];
                            retry = true;
                            await Task.Delay(delay);
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (ServerException servex)
                {
                    Console.WriteLine(servex.Message);
                    throw;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw;
                }
            }


        }

        public static void AddContextSettings(this ClientRuntimeContext clientContext, ClientContextSettings contextData)
        {
            clientContext.StaticObjects[PnPSettingsKey] = contextData;
        }
        public static ClientContextSettings GetContextSettings(this ClientRuntimeContext clientContext)
        {
            if (!clientContext.StaticObjects.TryGetValue(PnPSettingsKey, out object settingsObject))
            {
                return null;
            }

            return (ClientContextSettings)settingsObject;
        }
    }
}
