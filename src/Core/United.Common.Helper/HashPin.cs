using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.DynamoDB;
using United.Mobile.DataAccess.Product.Interfaces;
using United.Mobile.Model.Common.CloudDynamoDB;
using United.Mobile.Model.DynamoDb.Common;
using United.Service.Presentation.ReferenceDataModel;
using United.Utility.Helper;

namespace United.Common.Helper
{
    public class HashPin
    {
        private readonly ICacheLog _logger;
        private readonly IConfiguration _configuration;
        private readonly IValidateHashPinService _validateHashPinService;
        private readonly IDynamoDBService _dynamoDBService;
        private readonly IHeaders _headers;
        private readonly IFeatureSettings _featureSettings;

        public HashPin(ICacheLog logger
            , IConfiguration configuration
            , IValidateHashPinService validateHashPinService
            , IDynamoDBService dynamoDBService
            , IHeaders headers
            , IFeatureSettings featureSettings)
        {
            _logger = logger;
            _configuration = configuration;
            _validateHashPinService = validateHashPinService;
            _dynamoDBService = dynamoDBService;
            _headers = headers;
            _featureSettings = featureSettings;
        }

        public async Task<bool> ValidateHashPinAndGetAuthToken(string accountNumber, string hashPinCode, int applicationId, string deviceId, string appVersion)
        {
            bool ok = false;
            var mileagePlusDynamoDB = new MileagePlusDynamoDB(_configuration, _dynamoDBService);
            var list = await mileagePlusDynamoDB.ValidateHashPinAndGetAuthToken<List<MileagePlusValidationData>>(accountNumber, hashPinCode, applicationId, deviceId, appVersion, _headers.ContextValues.SessionId).ConfigureAwait(false);

            try
            {
                foreach (var item in list)
                {
                    if (Convert.ToInt32(item.AccountFound) == 1)
                    {
                        ok = true;
                        //validAuthToken = item.AuthenticatedToken.ToString();
                    }
                }

            }
            catch (Exception ex) { string msg = ex.Message; }

            if (ok == false && _configuration.GetValue<string>("ByPassMPByPassCheckForDpMPSignCall2_1_41") != null &&
                _configuration.GetValue<string>("ByPassMPByPassCheckForDpMPSignCall2_1_41").ToUpper().Trim() == appVersion.ToUpper().Trim())
            {
                var deviceDynamodb = new DeviceDynamDB(_configuration, _dynamoDBService);
                ok = await deviceDynamodb.ValidateDeviceIDAPPID(deviceId, applicationId, accountNumber, appVersion);
            }
            return ok;
        }

        public async Task<bool> ValidateHashPinAndGetAuthToken(string accountNumber, string hashPinCode, int applicationId, string deviceId, string appVersion, string validAuthToken)
        {
            var list = await ValidateHashPinAndGetAuthTokenDynamoDB(accountNumber, hashPinCode, applicationId, deviceId, appVersion, _headers.ContextValues.SessionId).ConfigureAwait(false);

            var ok = (list != null && !string.IsNullOrEmpty(list.HashPincode)) ? true : false;

            if (ok == false && _configuration.GetValue<string>("ByPassMPByPassCheckForDpMPSignCall2_1_41") != null &&
                _configuration.GetValue<string>("ByPassMPByPassCheckForDpMPSignCall2_1_41").ToUpper().Trim() == appVersion.ToUpper().Trim())
            {
                var deviceDynamodb = new DeviceDynamDB(_configuration, _dynamoDBService);
                ok = await deviceDynamodb.ValidateDeviceIDAPPID(deviceId, applicationId, accountNumber, appVersion);
            }
            return ok;
        }

        public async Task<MileagePlusDetails> ValidateHashPinAndGetAuthTokenDynamoDB(string accountNumber, string hashPinCode
            , int applicationId, string deviceId, string appVersion, string sessionid = "")
        {
            var mileagePlusDynamoDB = new MileagePlusDynamoDB(_configuration, _dynamoDBService);
            var hashResponse = await mileagePlusDynamoDB.ValidateHashPinAndGetAuthToken<MileagePlusDetails>(accountNumber, hashPinCode, applicationId, deviceId, appVersion, _headers.ContextValues.SessionId);

            if (!await _featureSettings.GetFeatureSettingValue("DisableLoggingHashPinCodeEmpty").ConfigureAwait(false)
                && hashResponse != null && string.IsNullOrEmpty(hashResponse.HashPincode))
            {
                _logger.LogWarning("DynamoDB -GetRecords Hashpincode is null/empty.");
            }
            bool isAuthorized = false;
            if (hashResponse?.HashPincode == hashPinCode)
            {
                isAuthorized = true;
                return hashResponse;
            }
            else
            {
                if (!await _featureSettings.GetFeatureSettingValue("DisableSQLValidateHashPinAndGetAuthToken").ConfigureAwait(false))
                {
                    _logger.LogInformation("ValidateHashPinAndGetAuthToken - OnPremSQL Request {accountNumber} {applicationId} {transactionId} and {SessionId}", accountNumber, applicationId, _headers.ContextValues.TransactionId, _headers.ContextValues.SessionId);
                    var hashpinResponse = await _validateHashPinService.ValidateHashPin<HashPinValidate>(accountNumber, hashPinCode, applicationId, deviceId, appVersion, _headers.ContextValues.TransactionId, _headers.ContextValues.SessionId);
                    _logger.LogInformation("ValidateHashPinAndGetAuthToken - OnPremSQL Response {response} {transactionId} and {SessionId}", JsonConvert.SerializeObject(hashpinResponse), _headers.ContextValues.TransactionId, _headers.ContextValues.SessionId);

                    var authToken = hashpinResponse?.validAuthToken;
                    isAuthorized = true;
                    var Data = new MileagePlusDetails()
                    {
                        MileagePlusNumber = accountNumber,
                        MPUserName = accountNumber,
                        HashPincode = hashPinCode,
                        PinCode = _configuration.GetValue<bool>("LogMPPinCodeOnlyForTestingAtStage") ? hashPinCode : string.Empty,
                        ApplicationID = Convert.ToString(applicationId),
                        AppVersion = appVersion,
                        DeviceID = deviceId,
                        IsTokenValid = Convert.ToString(isAuthorized),
                        TokenExpireDateTime = string.Empty,
                        TokenExpiryInSeconds = string.Empty,
                        IsTouchIDSignIn = string.Empty,
                        IsTokenAnonymous = string.Empty,
                        CustomerID = string.Empty,
                        DataPowerAccessToken = (hashpinResponse?.IsValidHashPin == true) ? authToken : string.Empty,
                        UpdateDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.FFF")
                    };

                    string Key = string.Format("{0}::{1}::{2}", accountNumber, applicationId, deviceId);

                    _logger.LogInformation("ValidateHashPinAndGetAuthToken - DynamoDB Request", Data);
                    bool jsonResponse = await new MPValidationCSSDynamoDB(_configuration, _dynamoDBService).SaveRecords<MileagePlusDetails>(Data, _headers.ContextValues.SessionId, Key, accountNumber, _headers.ContextValues.TransactionId);
                    _logger.LogInformation("ValidateHashPinAndGetAuthToken - DynamoDB Response", jsonResponse);

                    if (!string.IsNullOrEmpty(authToken))
                    {
                        var hashResp = new MileagePlusDetails()
                        {
                            HashPincode = hashPinCode,
                            AuthenticatedToken = authToken
                        };

                        return hashResp;
                    }
                }
            }
            if (!isAuthorized)
            {
                _logger.LogError("ValidateHashPinAndGetAuthToken-UnAutorized", isAuthorized);
            }
            return default(MileagePlusDetails);
        }
    }
}
