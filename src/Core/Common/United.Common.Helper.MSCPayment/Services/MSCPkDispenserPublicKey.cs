using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using United.Common.Helper.MSCPayment.Interfaces;
using United.Definition;
using United.Mobile.DataAccess.Common;
using MOBItem = United.Mobile.Model.Common.MOBItem;
using United.Service.Presentation.SecurityResponseModel;
using United.Utility.Helper;

namespace United.Common.Helper.MSCPayment.Services
{
    public class MSCPkDispenserPublicKey : IMSCPkDispenserPublicKey
    {
        private readonly IConfiguration _configuration;
        private readonly IHeaders _headers;
        private readonly IPKDispenserService _pKDispenserService;
        private readonly ICachingService _cachingService;

        public MSCPkDispenserPublicKey(IConfiguration configuration
            , IPKDispenserService pKDispenserService
            , ICachingService cachingService)
        {
            _configuration = configuration;
            _pKDispenserService = pKDispenserService;
            _cachingService = cachingService;
        }

        public async System.Threading.Tasks.Task<string> GetCachedOrNewpkDispenserPublicKey(int appId, string appVersion, string deviceId, string transactionId, string token, List<MOBItem> catalogItems = null, string flow="")
        {
            string pkDispenserPublicKey = string.Empty;
            if (!ConfigUtility.IsSuppressPkDispenserKey(appId, appVersion, catalogItems))
            {
                if (_configuration.GetValue<bool>("EnablePKDispenserKeyRotationAndOAEPPadding"))
                {
                    var key = string.Format(_configuration.GetValue<string>("PKDispenserKeyTokenKeyFormat"), appId);
                    var pKDispenserKey = await _cachingService.GetCache<string>(key, "TID1").ConfigureAwait(false);
                    try
                    {
                        United.Service.Presentation.SecurityResponseModel.PKDispenserKey obj = DataContextJsonSerializer.DeserializeJsonDataContract<United.Service.Presentation.SecurityResponseModel.PKDispenserKey>(pKDispenserKey);
                        pkDispenserPublicKey = obj == null ? null : obj.PublicKey;
                    }
                    catch { }
                }
                else
                    pkDispenserPublicKey = await _cachingService.GetCache<string>(GetCSSPublicKeyPersistSessionStaticGUID(appId) + "pkDispenserPublicKey", "TID1").ConfigureAwait(false);

                return string.IsNullOrEmpty(pkDispenserPublicKey)
                        ? await GetPkDispenserPublicKey(appId, deviceId, appVersion, transactionId, token).ConfigureAwait(false)
                        : pkDispenserPublicKey;
            }

            
            return pkDispenserPublicKey;
        }

        public async System.Threading.Tasks.Task<string> GetCachedOrNewpkDispenserPublicKey(int appId, string appVersion, string deviceId, string transactionId, string token, string flow, List<MOBItem> catalogItems = null)
        {
            string pkDispenserPublicKey = string.Empty;
            if (!ConfigUtility.IsSuppressPkDispenserKey(appId, appVersion, catalogItems, flow))
            {
                if (_configuration.GetValue<bool>("EnablePKDispenserKeyRotationAndOAEPPadding"))
                {
                    var key = string.Format(_configuration.GetValue<string>("PKDispenserKeyTokenKeyFormat"), appId);
                    var pKDispenserKey = await _cachingService.GetCache<string>(key, "TID1").ConfigureAwait(false);
                    try
                    {
                        United.Service.Presentation.SecurityResponseModel.PKDispenserKey obj = DataContextJsonSerializer.DeserializeJsonDataContract<United.Service.Presentation.SecurityResponseModel.PKDispenserKey>(pKDispenserKey);
                        pkDispenserPublicKey = obj == null ? null : obj.PublicKey;
                    }
                    catch { }
                }
                else
                    pkDispenserPublicKey = await _cachingService.GetCache<string>(GetCSSPublicKeyPersistSessionStaticGUID(appId) + "pkDispenserPublicKey", "TID1").ConfigureAwait(false);

                return string.IsNullOrEmpty(pkDispenserPublicKey)
                        ? await GetPkDispenserPublicKey(appId, deviceId, appVersion, transactionId, token).ConfigureAwait(false)
                        : pkDispenserPublicKey;
            }


            return pkDispenserPublicKey;
        }

        private async System.Threading.Tasks.Task<string> GetPkDispenserPublicKey(int applicationId, string deviceId, string appVersion, string transactionId, string authToken)
        {
            #region
            //**RSA Public Key Implementation**//
            string transId = string.IsNullOrEmpty(transactionId) ? "trans0" : transactionId;
            string key = string.Format(_configuration.GetValue<string>("PKDispenserKeyTokenKeyFormat"), applicationId);
            var cacheResponse = await _cachingService.GetCache<string>(key, transId).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(cacheResponse))
            {
                var obj = JsonConvert.DeserializeObject<PKDispenserKey>(cacheResponse);

                if (!string.IsNullOrEmpty(obj.PublicKey))
                {
                    return obj.PublicKey;
                }
            }

            var response = await _pKDispenserService.GetPkDispenserPublicKey<Service.Presentation.SecurityResponseModel.PKDispenserResponse>(authToken, transactionId, string.Empty).ConfigureAwait(false);
            return GetPublicKeyFromtheResponse(response, transactionId, applicationId);
            #endregion
        }

        private string GetPublicKeyFromtheResponse(Service.Presentation.SecurityResponseModel.PKDispenserResponse response, string transactionId, int applicationId)
        {
            string pkDispenserPublicKey = string.Empty;

            if (response != null && response.Keys != null && response.Keys.Count > 0)
            {
                var obj = (from st in response.Keys
                           where st.CryptoTypeID.Trim().Equals("2")
                           select st).ToList();
                obj[0].PublicKey = obj[0].PublicKey.Replace("\r", "").Replace("\n", "").Replace("-----BEGIN PUBLIC KEY-----", "").Replace("-----END PUBLIC KEY-----", "").Trim();
                pkDispenserPublicKey = obj[0].PublicKey;

                string transId = string.IsNullOrEmpty(transactionId) ? "trans0" : transactionId;
                string key = string.Format(_configuration.GetValue<string>("PKDispenserKeyTokenKeyFormat"), applicationId);
                _cachingService.SaveCache<United.Service.Presentation.SecurityResponseModel.PKDispenserKey>(key, obj[0], transId, new TimeSpan(1, 30, 0));

                string tokenKey = string.Format(_configuration.GetValue<string>("PKDispenserKeyTokenKeyFormat"), "Token::" + applicationId);
                _cachingService.SaveCache<string>(tokenKey, pkDispenserPublicKey, transId, new TimeSpan(1, 30, 0));
            }
            else
            {
                string exceptionMessage = _configuration.GetValue<string>("Booking2OGenericExceptionMessage");
                if (!String.IsNullOrEmpty(_configuration.GetValue<string>("UnableToGetPkDispenserPublicKeyErrorMessage")))
                {
                    exceptionMessage = _configuration.GetValue<string>("UnableToGetPkDispenserPublicKeyErrorMessage");
                }
                throw new MOBUnitedException(exceptionMessage);
            }
            return pkDispenserPublicKey;
        }

        private string GetCSSPublicKeyPersistSessionStaticGUID(int applicationId)
        {
            #region Get Aplication and Profile Ids
            string[] cSSPublicKeyPersistSessionStaticGUIDs = _configuration.GetValue<string>("CSSPublicKeyPersistSessionStaticGUID").Split('|');
            List<string> applicationDeviceTokenSessionIDList = new List<string>();
            foreach (string applicationSessionGUID in cSSPublicKeyPersistSessionStaticGUIDs)
            {
                #region
                if (Convert.ToInt32(applicationSessionGUID.Split('~')[0].ToString().ToUpper().Trim()) == applicationId)
                {
                    return applicationSessionGUID.Split('~')[1].ToString().Trim();
                }
                #endregion
            }
            return "1CSSPublicKeyPersistStatSesion4IphoneApp";
            #endregion
        }

        public string GetNewPublicKeyPersistSessionStaticGUID(int applicationId)
        {
            #region Get Aplication and Profile Ids
            string[] cSSPublicKeyPersistSessionStaticGUIDs = _configuration.GetValue<string>("NewPublicKeyPersistSessionStaticGUID").Split('|');
            List<string> applicationDeviceTokenSessionIDList = new List<string>();
            foreach (string applicationSessionGUID in cSSPublicKeyPersistSessionStaticGUIDs)
            {
                #region
                if (Convert.ToInt32(applicationSessionGUID.Split('~')[0].ToString().ToUpper().Trim()) == applicationId)
                {
                    return applicationSessionGUID.Split('~')[1].ToString().Trim();
                }
                #endregion
            }
            return "1NewPublicKeyPersistStatSesion4IphoneApp";
            #endregion
        }
    }
}
