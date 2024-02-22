using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using United.Ebs.Logging.Enrichers;
using United.Mobile.Model;
using United.Mobile.Model.Common;
using United.Utility.Helper;

namespace United.Mobile.DataAccess.Common
{
    public class SessionHelperService : ISessionHelperService
    {
        private readonly ICacheLog<SessionHelperService> _logger;
        private readonly IConfiguration _configuration;
        private readonly ISessionOnCloudService _sessionOnCloudService;
        private readonly IApplicationEnricher _applicationEnricher;

        public string SessionID { get; set; }

        public SessionHelperService(ICacheLog<SessionHelperService> logger
            , IConfiguration configuration
            , ISessionOnCloudService sessionOnCloudService
            , IApplicationEnricher applicationEnricher)
        {
            _logger = logger;
            _configuration = configuration;
            _sessionOnCloudService = sessionOnCloudService;
            _applicationEnricher = applicationEnricher;
        }



        public async Task<T> GetSession<T>(string sessionID, string objectName, List<string> vParams = null, bool isReadOnPrem = false)
        {
            if (string.IsNullOrEmpty(sessionID))
            {
                return default;
            }

            string sessionObjectName = (string.IsNullOrEmpty(objectName)) ? typeof(T).FullName : objectName;

            try
            {
                //  getsessionValueOnCloud 
                //isReadOnPrem = isReadOnPrem ? isReadOnPrem : _configuration.GetValue<bool>("loadSessionFromOnPremCouchbase");
                var sessionResponse = await _sessionOnCloudService?.GetSession(sessionID, objectName, vParams, sessionID, isReadOnPrem);
                bool isSessionDataSucceed = false;
                if (!string.IsNullOrEmpty(sessionResponse))
                {
                    var sessionData = JsonConvert.DeserializeObject<SessionResponse>(sessionResponse);

                    if (!string.IsNullOrEmpty(sessionData?.Data) && sessionData.Succeed)
                    {

                        try
                        {
                            return JsonConvert.DeserializeObject<T>(sessionData.Data); ;
                        }
                        catch (Exception)
                        {
                        }

                        try
                        {

                            T typeInstance = default(T);
                            StringReader memoryStream = new StringReader(sessionData.Data);
                            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                            typeInstance = (T)xmlSerializer.Deserialize(memoryStream);
                            return typeInstance;
                        }
                        catch (Exception)
                        {
                        }

                        try
                        {

                            var deserValue = (sessionData.Data.Contains(@"<?xml")) ?
                                           XmlSerializerHelper.GetObjectFromXmlData<T>(sessionData.Data) :
                                           JsonConvert.DeserializeObject<T>(sessionData.Data);

                            return await Task.FromResult(JsonConvert.DeserializeObject<T>(deserValue));
                        }
                        catch (Exception)
                        {
                        }
                        
                    }
                    isSessionDataSucceed = sessionData.Succeed;
                }

                ///This code is removed once we move all API's (Booking, MangRes,ResShop, MPSign)  to cloud
                if (_configuration.GetValue<bool>("EnableGetSessionFromOnPrem") && !isSessionDataSucceed && !isReadOnPrem)
                {
                    ///This code is removed once we move all API's (Booking, MangRes,ResShop, MPSign)  to cloud
                    return await GetSession<T>(sessionID, objectName, vParams, true);
                }
            }
            catch (Exception ex)
            { throw ex; }

            return default;
        }

        public async Task<T> GetSession<T>(string sessionID, string objectName, int temp, List<string> vParams = null)
        {

            if (string.IsNullOrEmpty(sessionID))
            {
                return default;
            }

            string sessionObjectName = (string.IsNullOrEmpty(objectName)) ? typeof(T).FullName : objectName;

            try
            {
                _logger.LogInformation("GetSession - request  {sessionID} {objectName} {validationParams}", sessionID, objectName, vParams);
                //  getsessionValueOnCloud 
                var sessionResponse = await _sessionOnCloudService?.GetSession(sessionID, objectName, vParams);

                _logger.LogInformation("GetSession - {response} {sessionID}", sessionResponse, sessionID);

                if (!string.IsNullOrEmpty(sessionResponse))
                {
                    var sessionData = JsonConvert.DeserializeObject<SessionResponse>(sessionResponse);

                    if (!string.IsNullOrEmpty(sessionData?.Data) && sessionData.Succeed)
                    {
                        try
                        {
                            T xmlResult = default(T);
                            StringReader memoryStream = new StringReader(sessionData.Data);
                            var xmlSerializer = new XmlSerializer(typeof(T));
                            xmlResult = (T)xmlSerializer.Deserialize(memoryStream);

                            //StringWriter myWriter = new StringWriter(sessionData.Data);
                            //XmlSerializer s = new XmlSerializer(typeof(T));
                            //xmlResult = (T)s.Deserialize(myWriter);
                            //return XmlSerializerHelper.GetObjectFromXmlData<T>(sessionData.Data);
                            return xmlResult;
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }

            }
            catch (Exception ex)
            { }

            return default;
        }


        public async Task<bool> SaveSession<T>(T data, string sessionID, List<string> validateParams, string objectName = "", int sessionTimeSpanInSecs = 5400, bool saveJsonOnCloudXMLOnPrem = false)
        {
            var sessionObjectName = (string.IsNullOrEmpty(objectName)) ? typeof(T).FullName : objectName;
            string saveData = string.Empty;
            try
            {
                saveData = JsonConvert.SerializeObject(data);
            }
            catch (Exception ex)
            {
                //saveData = XmlSerializerHelper.SaveObjectFromXmlData<T>(data);
                throw ex;
            }
            if (!_configuration.GetValue<bool>("SavePersistInXML"))
            {
                if (saveJsonOnCloudXMLOnPrem == true)
                {
                    //SaveSessionValue from OnCloud Service (CSLShop
                    return await _sessionOnCloudService?.SaveSessionONCloudOnPrem(saveData, sessionID, validateParams, sessionObjectName, TimeSpan.FromSeconds(sessionTimeSpanInSecs), sessionID, SaveXMLFormatToOnPrem: false);
                }
                else
                {
                    //SaveSessionValue from OnCloud Service
                    await _sessionOnCloudService?.SaveSessionONCloud(saveData, sessionID, validateParams, sessionObjectName, TimeSpan.FromSeconds(sessionTimeSpanInSecs), sessionID);
                    try
                    {
                        saveData = XmlSerializerHelper.SaveObjectFromXmlData<T>(data);
                        await _sessionOnCloudService.SaveSessionOnPrem(saveData, sessionID, validateParams, sessionObjectName, TimeSpan.FromSeconds(sessionTimeSpanInSecs), sessionID).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Exception - SaveSessionOnPrem {@Exception}, {@ValidateParam} {@SessionId}", JsonConvert.SerializeObject(ex), JsonConvert.SerializeObject(validateParams), sessionID);
                    }

                }
                return true;
            }
            else
            {
                //return await _sessionOnCloudService.SaveSessionONCloud<T>(data, sessionID, new List<string> { sessionID, objectName }, objectName, TimeSpan.FromSeconds(sessionTimeSpanInSecs), sessionID);
                return await _sessionOnCloudService.SaveSessionONCloud(saveData, sessionID, validateParams, sessionObjectName, TimeSpan.FromSeconds(sessionTimeSpanInSecs), sessionID).ConfigureAwait(false);
            }

        }

        public async Task<bool> SaveSessions<T>(T data, string sessionID, List<string> validateParams, string objectName = "")
        {
            var sessionObjectName = (string.IsNullOrEmpty(objectName)) ? typeof(T).FullName : objectName;
            var sessionTimeExpiry = TimeSpan.FromSeconds(_configuration.GetValue<double>("SessionDataExp") == 0 ? 10 : _configuration.GetValue<double>("SessionDataExp"));

            try
            {

                if (!_configuration.GetValue<bool>("SavePersistInXML"))
                {
                    var saveData = XmlSerializerHelper.SaveObjectFromXml<T>(data);
                    //SaveSessionValue from OnCloud Service
                    return await _sessionOnCloudService?.SaveSessionONCloudOnPrem(saveData, sessionID, validateParams, sessionObjectName, sessionTimeExpiry, sessionID);
                }
                else
                {

                    return await _sessionOnCloudService.SaveSessionONCloud<T>(data, sessionID, new List<string> { sessionID, objectName }, objectName, sessionTimeExpiry, sessionID);

                }
            }
            catch (Exception ex)
            { }
            return false;
        }


    }
}
