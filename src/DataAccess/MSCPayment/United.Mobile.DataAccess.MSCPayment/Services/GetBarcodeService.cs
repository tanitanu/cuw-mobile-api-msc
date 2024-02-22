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
    public class GetBarcodeService : IGetBarcodeService
    {

        private readonly ICacheLog<GetBarcodeService> _logger;
        private readonly IResilientClient _resilientClient;

        public GetBarcodeService(ICacheLog<GetBarcodeService> logger, [KeyFilter("GetBarcodeServiceKey")] IResilientClient resilientClient)
        {
            _logger = logger;
            _resilientClient = resilientClient;
        }

        public async Task<string> GetClubPassCode(string path, string request, string sessionId, string token)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                           {"Accept", "application/json"},
                          { "Authorization", token }
                     };
            _logger.LogInformation("CSL service-GetClubPassCode  parameters Request:{Request} Path:{Path}", request, path);

            try
            {
                var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, request, headers).ConfigureAwait(true);

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("CSL service-GetClubPassCode {@RequestUrl} error {response}", responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                        throw new Exception(responseData.response);
                }

                _logger.LogInformation("CSL service-GetClubPassCode {@RequestUrl}, {response}", responseData.url, responseData.response);

                return responseData.response;
            }
            catch (Exception ex)
            {
                _logger.LogError("CSL service-GetClubPassCode error {Exception}", JsonConvert.SerializeObject(ex));
            }

            return default;
        }
    }
}
