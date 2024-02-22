﻿using Autofac.Features.AttributeFilters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
using United.Utility.Helper;
using United.Utility.Http;

namespace United.Mobile.DataAccess.Common
{
    public class DPTokenValidationService:IDPTokenValidationService
    {
        private readonly IResilientClient _resilientClient;
        private readonly ICacheLog<DPTokenValidationService> _logger;
   

        public DPTokenValidationService([KeyFilter("dpTokenValidateKey")] IResilientClient resilientClient
            , ICacheLog<DPTokenValidationService> logger
            )
        {
            _resilientClient = resilientClient;
            _logger = logger;
            
        }
        public async Task<bool> isActiveToken(string token)
        {                  
            _logger.LogInformation("isActiveToken {token}", token);
            try
            {
                var dpRequest = new DPTokenRequest
                {
                    AccessToken = token,
                    GrantType = "validate_token"
                };
                Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"}
                     };
                string requestData = JsonConvert.SerializeObject(dpRequest);
                var dpTokenCslResponse = await _resilientClient.PostHttpAsyncWithOptions(string.Empty, requestData, headers);
                var dpTokenResponse = JsonConvert.DeserializeObject<ActiveTokenResponse>(dpTokenCslResponse.response);
                _logger.LogInformation("csl Response {@response}", dpTokenResponse);
                return dpTokenResponse.Active;
            }
            catch (Exception ex)
            {
                _logger.LogError("isActiveToken error{@message} {@stackTrace}", ex.Message, ex.StackTrace);
                throw ex;
            }
        }

    }
}
