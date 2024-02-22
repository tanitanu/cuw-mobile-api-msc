using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using United.Mobile.DataAccess.MSCPayment.Interfaces;
using United.Utility.Helper;
using United.Utility.Http;
using United.Utility.Serilog;

namespace United.Mobile.DataAccess.MSCPayment.Services
{
    public class GetTermsandCondtionsService : IGetTermsandCondtionsService
    {
        private readonly ICacheLog<GetTermsandCondtionsService> _logger;
        private readonly IResilientClient _resilientClient;

        public GetTermsandCondtionsService(ICacheLog<GetTermsandCondtionsService> logger, [KeyFilter("GetTermsandCondtionsClientKey")] IResilientClient resilientClient)
        {
            _logger = logger;
            _resilientClient = resilientClient;
        }

        public async Task<string> GetTermsandCondtionsByPromoCode(string path, string sessionId, string token)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                           {"Accept", "application/json"},
                          { "Authorization", token }
                     };

            _logger.LogInformation("CSL service-GetTermsandCondtionsByPromoCode ");

            try
            {
                var responseData = await _resilientClient.GetHttpAsyncWithOptions(path, headers).ConfigureAwait(true);

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("CSL service-GetTermsandCondtionsByPromoCode {@RequestUrl} {url} error {response}", _resilientClient?.BaseURL, responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                        throw new Exception(responseData.response);
                }

                _logger.LogInformation("CSL service-GetTermsandCondtionsByPromoCode {@RequestUrl}, {response}", responseData.url, responseData.response);

                return responseData.response;
            }
            catch (Exception ex)
            {
                _logger.LogError("CSL service-GetTermsandCondtionsByPromoCode error {Exception}", JsonConvert.SerializeObject(ex));
            }

            return default;
        }
    }
}
