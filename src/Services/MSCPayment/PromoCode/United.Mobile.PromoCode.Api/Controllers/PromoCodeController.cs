using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using United.Common.Helper;
using United.Definition;
using United.Definition.FormofPayment;
using United.Mobile.Model.Common;
using United.Mobile.PromoCode.Domain;
using United.Utility.Helper;
using United.Utility.Serilog;

namespace United.Mobile.PromoCode.Api.Controllers
{
    [Route("promocodeservice/api")]
    [ApiController]

    public class PromoCodeController : ControllerBase
    {
        private readonly ICacheLog<PromoCodeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHeaders _headers;
        private readonly IPromoCodeBusiness _promoCodeBusiness;
        private readonly IFeatureSettings _featureSettings;
        public PromoCodeController(ICacheLog<PromoCodeController> logger
            , IConfiguration configuration
            , IHeaders headers
            , IPromoCodeBusiness promoCodeBusiness
            , IFeatureSettings featureSettings)
        {
            _logger = logger;
            _configuration = configuration;
            _headers = headers;
            _promoCodeBusiness = promoCodeBusiness;
            _featureSettings = featureSettings;
        }

        /// <summary>
        /// HTTP GET
        /// Returns a version number of the service.
        /// </summary>
        /// <returns>
        /// Version Number of the Service.
        /// </returns>
        [HttpGet]
        [Route("version")]
        public virtual string Version()
        {
            string serviceVersionNumber = null;

            try
            {
                serviceVersionNumber = Environment.GetEnvironmentVariable("SERVICE_VERSION_NUMBER");
            }
            catch
            {
                // Suppress any exceptions
            }
            finally
            {
                serviceVersionNumber = (null == serviceVersionNumber) ? "0.0.0" : serviceVersionNumber;
            }

            return serviceVersionNumber;
        }

        [HttpGet]
        [Route("HealthCheck")]
        public string HealthCheck()
        {
            return "Healthy";
        }
        [HttpGet]
        [Route("environment")]
        public virtual string ApiEnvironment()
        {
            try
            {
                return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            }
            catch
            {
            }
            return "Unknown";
        }

        [HttpPost]
        [Route("Payment/ApplyPromoCode")]

        public async Task<MOBApplyPromoCodeResponse> ApplyPromoCode(MOBApplyPromoCodeRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId);
            var response = new MOBApplyPromoCodeResponse();
            IDisposable timer = null;
            try
            {
                _logger.LogInformation("ApplyPromoCode {@ClientRequest}", JsonConvert.SerializeObject(request));
                using (timer = _logger.BeginTimedOperation("Total time taken for ApplyPromoCode business call", request.SessionId))
                {

                    response = await _promoCodeBusiness.ApplyPromoCode(request);
                }

            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("ApplyPromoCode UnitedException {@UnitedException}", JsonConvert.SerializeObject(uaex));
                response.Exception = new MOBException();
                response.Exception.Message = uaex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError("ApplyPromoCode Error {@Exception}", JsonConvert.SerializeObject(ex));
                response.Exception = new MOBException("9999", _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }

            response.CallDuration = 0;

            if (timer != null)
            {
                response.CallDuration = ((TimedOperation)timer).GetElapseTime();
                timer.Dispose();
            }

            _logger.LogInformation("ApplyPromoCode {@ClientResponse}", JsonConvert.SerializeObject(response));

            return response;
        }

        [HttpPost]
        [Route("Payment/GetTermsandConditionsByPromoCode")]
        public async Task<MOBPromoCodeTermsandConditionsResponse> GetTermsandConditionsByPromoCode(MOBApplyPromoCodeRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId);
            var response = new MOBPromoCodeTermsandConditionsResponse();
            IDisposable timer = null;
            try
            {
                _logger.LogInformation("GetTermsandConditionsByPromoCode {@ClientRequest}", JsonConvert.SerializeObject(request));
                using (timer = _logger.BeginTimedOperation("Total time taken for GetTermsandConditionsByPromoCode business call", request.SessionId))
                {

                    response = await _promoCodeBusiness.GetTermsandConditionsByPromoCode(request);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("GetTermsandConditionsByPromoCode  UnitedException {@UnitedException}", JsonConvert.SerializeObject(uaex));
                response.Exception = new MOBException();
                response.Exception.Message = uaex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError("GetTermsandConditionsByPromoCode Error {@Exception}", JsonConvert.SerializeObject(ex));
                response.Exception = new MOBException("9999", _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }

            response.CallDuration = 0;

            if (timer != null)
            {
                response.CallDuration = ((TimedOperation)timer).GetElapseTime();
                timer.Dispose();
            }

            _logger.LogInformation("GetTermsandConditionsByPromoCode {@ClientResponse}", JsonConvert.SerializeObject(response));

            return response;
        }

        [HttpPost]
        [Route("Payment/RemovePromoCode")]
        public async Task<MOBApplyPromoCodeResponse> RemovePromoCode(MOBApplyPromoCodeRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId);
            var response = new MOBApplyPromoCodeResponse();
            IDisposable timer = null;
            try
            {
                _logger.LogInformation("RemovePromoCode {@ClientRequest}", JsonConvert.SerializeObject(request));
                using (timer = _logger.BeginTimedOperation("Total time taken for RemovePromoCode business call", request.SessionId))
                {
                    response = await _promoCodeBusiness.RemovePromoCode(request);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("RemovePromoCode UnitedException {@UnitedException}", JsonConvert.SerializeObject(uaex));
                response.Exception = new MOBException();
                response.Exception.Message = uaex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError("RemovePromoCode Error {@Exception}", JsonConvert.SerializeObject(ex));
                response.Exception = new MOBException("9999", _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }

            response.CallDuration = 0;

            if (timer != null)
            {
                response.CallDuration = ((TimedOperation)timer).GetElapseTime();
                timer.Dispose();
            }

            _logger.LogInformation("RemovePromoCode {@ClientResponse}", JsonConvert.SerializeObject(response));

            return response;
        }
        [HttpGet("GetFeatureSettings")]
        public GetFeatureSettingsResponse GetFeatureSettings()
        {
            GetFeatureSettingsResponse response = new GetFeatureSettingsResponse();
            try
            {
                response = _featureSettings.GetFeatureSettings();
            }
            catch (Exception ex)
            {
                _logger.LogError("GetFeatureSettings Error {exceptionstack} and {transactionId}", JsonConvert.SerializeObject(ex), "GetFeatureSettings_TransId");
                response.Exception = new MOBException("9999", JsonConvert.SerializeObject(ex));
            }
            return response;
        }
        [HttpPost("RefreshFeatureSettingCache")]
        public async Task<MOBResponse> RefreshFeatureSettingCache(MOBFeatureSettingsCacheRequest request)
        {
            MOBResponse response = new MOBResponse();
            try
            {
                request.ServiceName = ServiceNames.PROMOCODE.ToString();
                await _featureSettings.RefreshFeatureSettingCache(request);
            }
            catch (Exception ex)
            {
                _logger.LogError("RefreshFeatureSettingCache Error {exceptionstack} and {transactionId}", JsonConvert.SerializeObject(ex), "RefreshRetrieveAllFeatureSettings_TransId");
                response.Exception = new MOBException("9999", JsonConvert.SerializeObject(ex));
            }
            return response;
        }
    }
}