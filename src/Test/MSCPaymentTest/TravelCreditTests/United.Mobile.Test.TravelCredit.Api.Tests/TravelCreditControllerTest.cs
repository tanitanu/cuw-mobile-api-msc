using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using United.Common.Helper;
using United.Definition;
using United.Definition.FormofPayment;
using United.Mobile.Model.Common;
using United.Mobile.TravelCredit.Api.Controllers;
using United.Mobile.TravelCredit.Domain;
using United.Utility.Helper;
using Xunit;
using Constants = United.Mobile.Model.Constants;

namespace United.Mobile.Test.TravelCredit.Api.Tests
{
    public class TravelCreditControllerTest
    {
        private readonly Mock<ICacheLog<TravelCreditController>> _logger;
        private readonly IConfiguration _configuration;
        private readonly Mock<IHeaders> _headers;
        private readonly Mock<ITravelCreditBusiness> _travelCreditBusiness;
        private readonly TravelCreditController _travelCreditController;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        private readonly Mock<IFeatureSettings> _featureSettings;

        public TravelCreditControllerTest()
        {
            _logger = new Mock<ICacheLog<TravelCreditController>>();
            _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.test.json", optional: false, reloadOnChange: true)
            .Build();
            _headers = new Mock<IHeaders>();
            _travelCreditBusiness = new Mock<ITravelCreditBusiness>();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _travelCreditController = new TravelCreditController(_logger.Object, _configuration, _headers.Object, _travelCreditBusiness.Object,_featureSettings.Object);
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
            string result = _travelCreditController.HealthCheck();
            Assert.True(result == "Healthy");
        }
        [Fact]
        public void FutureFlightCredit_Test()
        {
            var response = new MOBFutureFlightCreditResponse() { TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44", CallDuration = 0 };
            //var request = new Request<MOBApplyPromoCodeRequest>()
            var request = new MOBFutureFlightCreditRequest()
            {


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
            _travelCreditController.ControllerContext = new ControllerContext();
            _travelCreditController.ControllerContext.HttpContext = new DefaultHttpContext();
            _travelCreditController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _travelCreditBusiness.Setup(p => p.FutureFlightCredit(request)).Returns(Task.FromResult(response));


            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _travelCreditController.FutureFlightCredit(request);
            Assert.True(result.Result.TransactionId != null);
            Assert.True(result.Result.Exception == null);
        }
        [Fact]
        public void FutureFlightCredit_SystemException_Test()
        {
            var request = new MOBFutureFlightCreditRequest()
            {

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
            _travelCreditController.ControllerContext = new ControllerContext();
            _travelCreditController.ControllerContext.HttpContext = new DefaultHttpContext();
            _travelCreditController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _travelCreditBusiness.Setup(p => p.FutureFlightCredit(request)).ThrowsAsync(new Exception("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _travelCreditController.FutureFlightCredit(request);
            Assert.True(result.Result.Exception != null);
        }
        [Fact]
        public void FutureFlightCredit_UnitedException_Test()
        {
            var request = new MOBFutureFlightCreditRequest()
            {

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
            _travelCreditController.ControllerContext = new ControllerContext();
            _travelCreditController.ControllerContext.HttpContext = new DefaultHttpContext();
            _travelCreditController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _travelCreditBusiness.Setup(p => p.FutureFlightCredit(request)).ThrowsAsync(new MOBUnitedException("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _travelCreditController.FutureFlightCredit(request);
            Assert.True(result.Result.Exception.Message != null);
        }
        [Fact]
        public void RemoveFutureFlightCredit_Test()
        {
            var response = new MOBFutureFlightCreditResponse() { TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44", CallDuration = 0 };
            //var request = new Request<MOBApplyPromoCodeRequest>()
            var request = new MOBFOPFutureFlightCreditRequest()
            {


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
            _travelCreditController.ControllerContext = new ControllerContext();
            _travelCreditController.ControllerContext.HttpContext = new DefaultHttpContext();
            _travelCreditController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _travelCreditBusiness.Setup(p => p.RemoveFutureFlightCredit(request)).Returns(Task.FromResult(response));


            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _travelCreditController.RemoveFutureFlightCredit(request);
            Assert.True(result.Result.TransactionId != null);
            Assert.True(result.Result.Exception == null);
        }
        [Fact]
        public void RemoveFutureFlightCredit_SystemException_Test()
        {
            var request = new MOBFOPFutureFlightCreditRequest()
            {

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
            _travelCreditController.ControllerContext = new ControllerContext();
            _travelCreditController.ControllerContext.HttpContext = new DefaultHttpContext();
            _travelCreditController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _travelCreditBusiness.Setup(p => p.RemoveFutureFlightCredit(request)).ThrowsAsync(new Exception("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _travelCreditController.RemoveFutureFlightCredit(request);
            Assert.True(result.Result.Exception != null);
        }
        [Fact]
        public void RemoveFutureFlightCredit_UnitedException_Test()
        {
            var request = new MOBFOPFutureFlightCreditRequest()
            {

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
            _travelCreditController.ControllerContext = new ControllerContext();
            _travelCreditController.ControllerContext.HttpContext = new DefaultHttpContext();
            _travelCreditController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _travelCreditBusiness.Setup(p => p.RemoveFutureFlightCredit(request)).ThrowsAsync(new MOBUnitedException("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _travelCreditController.RemoveFutureFlightCredit(request);
            Assert.True(result.Result.Exception.Message != null);
        }
        [Fact]
        public void LookUpTravelCredit_Test()
        {
            var response = new MOBFOPResponse() { TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44", CallDuration = 0 };
            //var request = new Request<MOBApplyPromoCodeRequest>()
            var request = new MOBFOPLookUpTravelCreditRequest()
            {


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
            _travelCreditController.ControllerContext = new ControllerContext();
            _travelCreditController.ControllerContext.HttpContext = new DefaultHttpContext();
            _travelCreditController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _travelCreditBusiness.Setup(p => p.LookUpTravelCredit(request)).Returns(Task.FromResult(response));


            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _travelCreditController.LookUpTravelCredit(request);
            Assert.True(result.Result.TransactionId != null);
            Assert.True(result.Result.Exception == null);
        }
        [Fact]
        public void LookUpTravelCredit_SystemException_Test()
        {
            var request = new MOBFOPLookUpTravelCreditRequest()
            {

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
            _travelCreditController.ControllerContext = new ControllerContext();
            _travelCreditController.ControllerContext.HttpContext = new DefaultHttpContext();
            _travelCreditController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _travelCreditBusiness.Setup(p => p.LookUpTravelCredit(request)).ThrowsAsync(new Exception("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _travelCreditController.LookUpTravelCredit(request);
            Assert.True(result.Result.Exception != null);
        }
        [Fact]
        public void LookUpTravelCredit_UnitedException_Test()
        {
            var request = new MOBFOPLookUpTravelCreditRequest()
            {

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
            _travelCreditController.ControllerContext = new ControllerContext();
            _travelCreditController.ControllerContext.HttpContext = new DefaultHttpContext();
            _travelCreditController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _travelCreditBusiness.Setup(p => p.LookUpTravelCredit(request)).ThrowsAsync(new MOBUnitedException("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _travelCreditController.LookUpTravelCredit(request);
            Assert.True(result.Result.Exception.Message != null);
        }
        [Fact]
        public void ManageTravelCredit_Test()
        {
            var response = new MOBFOPResponse() { TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44", CallDuration = 0 };
            //var request = new Request<MOBApplyPromoCodeRequest>()
            var request = new MOBFOPManageTravelCreditRequest()
            {


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
            _travelCreditController.ControllerContext = new ControllerContext();
            _travelCreditController.ControllerContext.HttpContext = new DefaultHttpContext();
            _travelCreditController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _travelCreditBusiness.Setup(p => p.ManageTravelCredit(request)).Returns(Task.FromResult(response));


            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _travelCreditController.ManageTravelCredit(request);
            Assert.True(result.Result.TransactionId != null);
            Assert.True(result.Result.Exception == null);
        }
        [Fact]
        public void ManageTravelCredit_SystemException_Test()
        {
            var request = new MOBFOPManageTravelCreditRequest()
            {

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
            _travelCreditController.ControllerContext = new ControllerContext();
            _travelCreditController.ControllerContext.HttpContext = new DefaultHttpContext();
            _travelCreditController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _travelCreditBusiness.Setup(p => p.ManageTravelCredit(request)).ThrowsAsync(new Exception("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _travelCreditController.ManageTravelCredit(request);
            Assert.True(result.Result.Exception != null);
        }
        [Fact]
        public void ManageTravelCredit_UnitedException_Test()
        {
            var request = new MOBFOPManageTravelCreditRequest()
            {

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
            _travelCreditController.ControllerContext = new ControllerContext();
            _travelCreditController.ControllerContext.HttpContext = new DefaultHttpContext();
            _travelCreditController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _travelCreditBusiness.Setup(p => p.ManageTravelCredit(request)).ThrowsAsync(new MOBUnitedException("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _travelCreditController.ManageTravelCredit(request);
            Assert.True(result.Result.Exception.Message != null);
        }
    }
}

