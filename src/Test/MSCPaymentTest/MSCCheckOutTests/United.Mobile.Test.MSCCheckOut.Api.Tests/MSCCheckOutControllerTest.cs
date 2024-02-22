using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using United.Common.Helper;
using United.Common.Helper.MSCPayment.Interfaces;
using United.Definition;
using United.Mobile.DataAccess.Common;
using United.Mobile.Model.Common;
using United.Mobile.MSCCheckOut.Api.Controllers;
using United.Mobile.MSCCheckOut.Domain;
using United.Utility.Helper;
using United.Utility.Http;
using Xunit;
using Constants = United.Mobile.Model.Constants;

namespace United.Mobile.Test.MSCCheckOut.Api.Tests
{
    public class MSCCheckOutControllerTest
    {
        private readonly Mock<ICacheLog<MSCCheckOutController>> _logger;
        private readonly IConfiguration _configuration;
        private readonly Mock<IHeaders> _headers;
        private readonly Mock<IMSCCheckOutBusiness> _mSCCheckOutBusiness;
        private readonly MSCCheckOutController _mSCCheckOutController;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        private readonly ISessionHelperService _sessionHelperService;
        private readonly ICachingService _cachingService;
        private readonly Mock<IResilientClient> _resilientClient;
        private readonly ICacheLog<CachingService> _logger7;
        private readonly Mock<IFeatureSettings> _featureSettings;
        public MSCCheckOutControllerTest()
        {
            _logger = new Mock<ICacheLog<MSCCheckOutController>>();
            _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.test.json", optional: false, reloadOnChange: true)
            .Build();
            _headers = new Mock<IHeaders>();
            _mSCCheckOutBusiness = new Mock<IMSCCheckOutBusiness>();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _resilientClient = new Mock<IResilientClient>();
            _cachingService = new CachingService(_resilientClient.Object, _logger7, _configuration);
            _mSCCheckOutController = new MSCCheckOutController(_logger.Object, _configuration, _headers.Object, _mSCCheckOutBusiness.Object, _sessionHelperService,_cachingService, _featureSettings.Object);
            SetupHttpContextAccessor();
            SetHeaders();

        }
        private void SetupHttpContextAccessor()
        {
            var guid = Guid.NewGuid().ToString();
            var context = new DefaultHttpContext();
            context.Request.Headers[Constants.HeaderAppIdText] = "1";
            context.Request.Headers[Constants.HeaderAppMajorText] = "1";
            context.Request.Headers[Constants.HeaderAppMinorText] = "0";
            context.Request.Headers[Constants.HeaderDeviceIdText] = guid;
            context.Request.Headers[Constants.HeaderLangCodeText] = "en-us";
            context.Request.Headers[Constants.HeaderRequestTimeUtcText] = DateTime.UtcNow.ToString();
            context.Request.Headers[Constants.HeaderTransactionIdText] = guid;
            _httpContextAccessor.Setup(_ => _.HttpContext).Returns(context);
        }

        private void SetHeaders(string deviceId = "589d7852-14e7-44a9-b23b-a6db36657579"
       , string applicationId = "2"
       , string appVersion = "4.1.29"
       , string transactionId = "589d7852-14e7-44a9-b23b-a6db36657579|8f46e040-a200-495c-83ca-4fca2d7175fb"
       , string languageCode = "en-US"
       , string sessionId = "17C979E184CC495EA083D45F4DD9D19D")
        {
            _headers.Setup(_ => _.ContextValues).Returns(
           new HttpContextValues
           {
               Application = new Application()
               {
                   Id = Convert.ToInt32(applicationId),
                   Version = new Mobile.Model.Version
                   {
                       Major = string.IsNullOrEmpty(appVersion) ? 0 : int.Parse(appVersion.ToString().Substring(0, 1)),
                       Minor = string.IsNullOrEmpty(appVersion) ? 0 : int.Parse(appVersion.ToString().Substring(2, 1)),
                       Build = string.IsNullOrEmpty(appVersion) ? 0 : int.Parse(appVersion.ToString().Substring(4, 2))
                   }
               },
               DeviceId = deviceId,
               LangCode = languageCode,
               TransactionId = transactionId,
               SessionId = sessionId
           });
        }

        [Fact]
        public void HealthCheck_Test()
        {
            string result = _mSCCheckOutController.HealthCheck();
            Assert.True(result == "Healthy");
        }
        [Fact]
        public void CheckOut_Test()
        {
            var response = new MOBCheckOutResponse() { TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44", CallDuration = 0 };
            //var request = new Request<MOBCheckOutRequest>()
            var request = new MOBCheckOutRequest()
            {

                MileagePlusNumber = "AW791957",
                PaymentAmount = "",
                Application = new MOBApplication()
                {
                    Id = 1,
                    Version = new MOBVersion()
                    {
                        Major = "4.1.25",
                        Minor = "4.1.25"
                    }
                },
                TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44"

            };
            _mSCCheckOutController.ControllerContext = new ControllerContext();
            _mSCCheckOutController.ControllerContext.HttpContext = new DefaultHttpContext();
            _mSCCheckOutController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _mSCCheckOutBusiness.Setup(p => p.CheckOut(request)).Returns(Task.FromResult(response));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _mSCCheckOutController.CheckOut(request);
            Assert.True(result.Result.TransactionId != null);
            Assert.True(result.Result.Exception == null);
        }

        [Fact]
        public void CheckOut_SystemException_Test()
        {
            //var request = new Request<CheckOutRequest>()
            var request = new MOBCheckOutRequest()
            {

                MileagePlusNumber = "AW791957",
                PaymentAmount = "",
                Application = new MOBApplication()
                {
                    Id = 1,
                    Version = new MOBVersion()
                    {
                        Major = "4.1.25",
                        Minor = "4.1.25"
                    }
                }

            };
            _mSCCheckOutController.ControllerContext = new ControllerContext();
            _mSCCheckOutController.ControllerContext.HttpContext = new DefaultHttpContext();
            _mSCCheckOutController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _mSCCheckOutBusiness.Setup(p => p.CheckOut(request)).ThrowsAsync(new Exception("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _mSCCheckOutController.CheckOut(request);
            Assert.True(result.Result.Exception != null);
        }


        [Fact]
        public void CheckOut_UnitedException_Test()
        {
            // var request = new Request<CheckOutRequest>()
            var request = new MOBCheckOutRequest()
            {

                MileagePlusNumber = "AW791957",
                PaymentAmount = "",
                Application = new MOBApplication()
                {
                    Id = 1,
                    Version = new MOBVersion()
                    {
                        Major = "4.1.25",
                        Minor = "4.1.25"
                    }
                },

            };

            _mSCCheckOutController.ControllerContext = new ControllerContext();
            _mSCCheckOutController.ControllerContext.HttpContext = new DefaultHttpContext();
            _mSCCheckOutController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

           _mSCCheckOutBusiness.Setup(p => p.CheckOut(request)).ThrowsAsync(new MOBUnitedException("Error Message"));
        //    _mSCCheckOutBusiness.Setup(p => p.CheckOut(request)).ThrowsAsync(new MOBUnitedException("90518"));
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();

           
            var result = _mSCCheckOutController.CheckOut(request);

            Assert.True(result.Result.Exception.Message != null);

        }

        
    }
}
