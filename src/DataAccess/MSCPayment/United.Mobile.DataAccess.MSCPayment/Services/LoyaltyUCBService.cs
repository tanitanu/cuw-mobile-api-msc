using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using United.Utility.Http;

namespace United.Mobile.DataAccess.MSCPayment.Services
{
    public class LoyaltyUCBService : ILoyaltyUCBService
    {
        private readonly ILogger<LoyaltyUCBService> _logger;
        private readonly IResilientClient _resilientClient;

        public LoyaltyUCBService([KeyFilter("LoyaltyUCBClientKey")] IResilientClient resilientClient, ILogger<LoyaltyUCBService> logger)
        {
            _logger = logger;
            _resilientClient = resilientClient;
        }

        public async Task<string> GetLoyaltyBalance(string token, string mpnumber, string sessionId)
        {
            try
            {

                Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          { "Authorization", token }
                     };
                var glbData = await _resilientClient.GetHttpAsyncWithOptions(mpnumber, headers, true);

                if (glbData.statusCode == HttpStatusCode.OK)
                {
                    _logger.LogInformation("CSL Service-GetLoyaltyBalance {@RequestUrl} and {response}", glbData.url, glbData.response);
                    return glbData.response;
                }

                _logger.LogError("CSL Service-GetLoyaltyBalance {@RequestUrl} {url} error {response}", _resilientClient?.BaseURL, glbData.url, glbData.statusCode);
            }
            catch (WebException wex)
            {
                if (wex.Response != null)
                {
                    var errorResponse = new StreamReader(wex.Response.GetResponseStream()).ReadToEnd();
                    _logger.LogError("CSL Service-GetLoyaltyBalance WebException {errorResponse}", errorResponse);
                }
            }

            return default;
        }
    }
}
