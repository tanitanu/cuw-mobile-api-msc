using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using United.Mobile.DataAccess.Product.Interfaces;
using United.Utility.Http;
using United.Definition;
using United.Utility.Helper;

namespace United.Mobile.DataAccess.Product.Services
{
    public class SeatMapService : ISeatMapService
    {
        private readonly IResilientClient _resilientClient;
        private readonly ICacheLog<SeatMapService> _logger;
        private readonly IConfiguration _configuration;

        public SeatMapService(
            [KeyFilter("SeatMapClientKey")] IResilientClient resilientClient
            , ICacheLog<SeatMapService> logger
            , IConfiguration configuration)
        {
            _resilientClient = resilientClient;
            _logger = logger;
            _configuration = configuration;
        }
        public async Task<T> SeatEngine<T>(string token, string action, string request, string sessionId)
        {

            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},//"application/xml;"
                          { "Authorization", token }
                     };
            string path = string.Format("/{0}", action);

            _logger.LogInformation("CSL service-SeatEngine  parameters Request:{Request} Path:{Path}", request, path);

            //using (_logger.BeginTimedOperation("Total time taken for SeatEngine business call", transationId: sessionId))
            //{
                try 
                {    
                    var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, request, headers).ConfigureAwait(false);
                    if (responseData.statusCode != HttpStatusCode.OK)
                    {
                        _logger.LogError("CSL service-SeatMap {@RequestUrl} error {response}", responseData.url, responseData.response);
                        if (responseData.statusCode != HttpStatusCode.BadRequest)
                            return default;
                    }

                    _logger.LogInformation("CSL service-SeatMap {@RequestUrl},{response}", responseData.url, responseData.response);
                    return JsonConvert.DeserializeObject<T>(responseData.response);
                }
                catch (WebException ex)
                {
                    var exReader = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                    string seatMapUnavailable = string.Empty;
                    if (!string.IsNullOrEmpty(_configuration.GetValue<string>("SeatMapUnavailable-MinorDescription")))
                    {
                        seatMapUnavailable = _configuration.GetValue<string>("SeatMapUnavailable-MinorDescription");
                        string[] seatMapUnavailableMinorDescription = seatMapUnavailable.Split('|');

                        if (!string.IsNullOrEmpty(exReader))
                        {
                            var exceptionDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<MOBFlightStatusError>(exReader);
                            if (exceptionDetails.Errors != null)
                                if (!string.IsNullOrEmpty(exceptionDetails.Errors[0].MinorDescription))
                                    //if (exceptionDetails.Errors[0].MinorDescription.Contains("SEAT DISPLAY NOT AVAILABLE FOR DATE") || exceptionDetails.Errors[0].MinorDescription.Contains("UNABLE TO DISPLAY INTERLINE SEAT MAP"))

                                    foreach (string minorDescription in seatMapUnavailableMinorDescription)
                                        if (exceptionDetails.Errors[0].MinorDescription.Contains(minorDescription))
                                            throw new MOBUnitedException(_configuration.GetValue<string>("OASeatMapUnavailableMessage"));
                        }
                    }
                    else
                    {
                        throw ex;
                    }

                }
                return default;  
            //}
            
        }

    }
}
