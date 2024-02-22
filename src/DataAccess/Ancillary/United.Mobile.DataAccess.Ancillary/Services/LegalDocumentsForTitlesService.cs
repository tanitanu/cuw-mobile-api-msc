using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using United.Definition;
using United.Mobile.DataAccess.Product.Interfaces;
using United.Utility.Helper;
using United.Utility.Http;

namespace United.Mobile.DataAccess.Product.Services
{
    public class LegalDocumentsForTitlesService : ILegalDocumentsForTitlesService
    {
        private readonly ICacheLog<LegalDocumentsForTitlesService> _logger;
        private readonly IResilientClient _resilientClient;

        public LegalDocumentsForTitlesService(ICacheLog<LegalDocumentsForTitlesService> logger, [KeyFilter("LegalDocumentsOnPremSqlClientKey")] IResilientClient resilientClient)
        {
            _logger = logger;
            _resilientClient = resilientClient;
        }

        public async Task<List<MOBLegalDocument>> GetLegalDocumentsForTitles(string titles, string transactionId)
        {
            if (titles == null)
            {
                _logger.LogError("GetLegalDocumentsForTitles titles is null {transactionId}", transactionId);
                return default;
            }

            using (_logger.BeginTimedOperation("Total time taken for GetLegalDocumentsForTitles OnPrem service call", transationId: transactionId))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"}
                     };

                var requestObj = string.Format("/GetLegalDocumentsForTitles?Titles={0}&transactionId={1}", titles, transactionId);

                _logger.LogInformation("GetLegalDocumentsForTitles-OnPrem Service {requestObj} and {transactionId} ", requestObj, transactionId);

                var responseData = await _resilientClient.GetHttpAsyncWithOptions(requestObj, headers).ConfigureAwait(false);

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("GetLegalDocumentsForTitles-OnPrem Service {requestUrl} error {statusCode} for {transactionId}", responseData.url, responseData.statusCode, transactionId);
                    #region If this api fails it should be a softfailure thats the reason not throwingexception back to client
                    //if (responseData.statusCode != HttpStatusCode.BadRequest)
                    //    throw new Exception(responseData.response);
                    #endregion 
                }

                _logger.LogInformation("GetLegalDocumentsForTitles-OnPrem Service {requestUrl} {response}  and {transactionId}", responseData.url, JsonConvert.SerializeObject(responseData.response), transactionId);

                return JsonConvert.DeserializeObject<List<MOBLegalDocument>>(responseData.response);
            }
        }
        public async Task<List<MOBLegalDocument>> GetNewLegalDocumentsForTitles(string titles, string transactionId, bool isTermsnConditions)
        {
            if (titles == null)
            {
                _logger.LogError("GetNewLegalDocumentsForTitles titles is null {transactionId}", transactionId);
                return default;
            }

            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"}
                     };

            var requestObj = string.Format("/GetNewLegalDocumentsForTitles?Titles={0}&transactionId={1}&isTermsnConditions={2}", titles, transactionId, isTermsnConditions);

            _logger.LogInformation("GetNewLegalDocumentsForTitles-OnPrem Service {requestObj} ", requestObj);

            var responseData = await _resilientClient.GetHttpAsyncWithOptions(requestObj, headers).ConfigureAwait(false);

            if (responseData.statusCode != HttpStatusCode.OK)
            {
                _logger.LogError("GetNewLegalDocumentsForTitles-OnPrem Service {@RequestUrl} error {statusCode}", responseData.url, responseData.statusCode);
                #region If this api fails it should be a softfailure thats the reason not throwingexception back to client
                //if (responseData.statusCode != HttpStatusCode.BadRequest)
                //    throw new Exception(null, new Exception(responseData.response));
                #endregion
            }

            _logger.LogInformation("GetNewLegalDocumentsForTitles-OnPrem Service {@RequestUrl} {response}", responseData.url, JsonConvert.SerializeObject(responseData.response));
            var _documentLibraryData = JsonConvert.DeserializeObject<List<MOBLegalDocument>>(responseData.response);
            _documentLibraryData.ForEach(x => x.Document = string.IsNullOrEmpty(x.Document) ? x.LegalDocument : x.Document); // code sync with onperem
            return _documentLibraryData;
        }
    }
}
