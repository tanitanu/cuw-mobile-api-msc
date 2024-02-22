using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using United.Common.Helper;
using United.Definition;
using United.Mobile.Model.Common;
using United.Mobile.MoneyPlusMiles.Domain;
using United.Utility.Helper;
using United.Utility.Serilog;

namespace United.Mobile.MoneyPlusMiles.Api.Controllers
{
    [Route("moneyplusmilesservice/api")]
    [ApiController]
    public class MoneyPlusMilesController : ControllerBase
    {
        private readonly ICacheLog<MoneyPlusMilesController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHeaders _headers;
        private readonly IMoneyPlusMilesBusiness _moneyPlusMilesBusiness;
        private readonly IFeatureSettings _featureSettings;
        public MoneyPlusMilesController(ICacheLog<MoneyPlusMilesController> logger
            , IConfiguration configuration
            , IHeaders headers
            , IMoneyPlusMilesBusiness moneyPlusMilesBusiness
            , IFeatureSettings featureSettings)
        {
            _logger = logger;
            _configuration = configuration;
            _headers = headers;
            _moneyPlusMilesBusiness = moneyPlusMilesBusiness;
            _featureSettings = featureSettings;
        }

        [HttpGet]
        [Route("HealthCheck")]
        public string HealthCheck()
        {
            return "Healthy";
        }

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
            }
            finally
            {
                serviceVersionNumber = (null == serviceVersionNumber) ? "0.0.0" : serviceVersionNumber;
            }

            return serviceVersionNumber;
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
        [Route("Payment/GetMoneyPlusMiles")]
        public async Task<MOBFOPResponse> GetMoneyPlusMiles(GetMoneyPlusMilesRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId);

            var response = new MOBFOPResponse();
            IDisposable timer = null;
            try
            {
                _logger.LogInformation("GetMoneyPlusMiles {@ClientRequest}", JsonConvert.SerializeObject(request));

                using (timer = _logger.BeginTimedOperation("Total time taken for GetMoneyPlusMiles business call", request.SessionId))
                {
                    response = await _moneyPlusMilesBusiness.GetMoneyPlusMiles(request);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("GetMoneyPlusMiles UnitedException {@UnitedException}", JsonConvert.SerializeObject(uaex));
                response.Exception = new MOBException();
                response.Exception.Message = uaex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError("GetMoneyPlusMiles Error {@Exception}", JsonConvert.SerializeObject(ex));
                response.Exception = new MOBException("9999", _configuration.GetValue<string>("GenericExceptionMessage"));
            }

            response.CallDuration = (timer != null) ? ((TimedOperation)timer).GetElapseTime() : 0;
            _logger.LogInformation("GetMoneyPlusMiles {@ClientResponse}", JsonConvert.SerializeObject(response));

            return response;
        }

        [HttpPost]
        [Route("Payment/ApplyMoneyPlusMiles")]
        public async Task<MOBFOPResponse> ApplyMoneyPlusMiles(ApplyMoneyPlusMilesOptionRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId);

            var response = new MOBFOPResponse();
            IDisposable timer = null;
            try
            {
                _logger.LogInformation("ApplyMoneyPlusMiles {@ClientRequest}", JsonConvert.SerializeObject(request));
                using (timer = _logger.BeginTimedOperation("Total time taken for ApplyMoneyPlusMiles business call", request.SessionId))
                {
                    response = await _moneyPlusMilesBusiness.ApplyMoneyPlusMiles(request);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("ApplyMoneyPlusMiles UnitedException {@UnitedException}", JsonConvert.SerializeObject(uaex));
                response.Exception = new MOBException();
                response.Exception.Message = uaex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError("ApplyMoneyPlusMiles Error {@Exception}", JsonConvert.SerializeObject(ex));
                response.Exception = new MOBException("9999", _configuration.GetValue<string>("GenericExceptionMessage"));
            }

            response.CallDuration = (timer != null) ? ((TimedOperation)timer).GetElapseTime() : 0;
            _logger.LogInformation("ApplyMoneyPlusMiles {@ClientResponse}", JsonConvert.SerializeObject(response));

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
                request.ServiceName = ServiceNames.MONEYPLUSMILES.ToString();
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
