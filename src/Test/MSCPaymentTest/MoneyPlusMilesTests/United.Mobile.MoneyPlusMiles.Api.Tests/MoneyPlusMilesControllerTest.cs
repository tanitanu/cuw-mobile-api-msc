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
using United.Mobile.Model.Common;
using United.Mobile.MoneyPlusMiles.Api.Controllers;
using United.Mobile.MoneyPlusMiles.Domain;
using United.Utility.Helper;
using Xunit;
using Constants = United.Mobile.Model.Constants;


namespace United.Mobile.MoneyPlusMiles.Api.Tests
{
    public class MoneyPlusMilesControllerTest
    {
        private readonly Mock<ICacheLog<MoneyPlusMilesController>> _logger;
        private readonly IConfiguration _configuration;
        private readonly Mock<IHeaders> _headers;
        private readonly Mock<IMoneyPlusMilesBusiness> _moneyPlusMilesBusiness;
        private readonly MoneyPlusMilesController _moneyPlusMilesController;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        private readonly Mock<IFeatureSettings> _featureSettings;
        public MoneyPlusMilesControllerTest()
        {
            _logger = new Mock<ICacheLog<MoneyPlusMilesController>>();
            _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.test.json", optional: false, reloadOnChange: true)
            .Build();
            _headers = new Mock<IHeaders>();
            _moneyPlusMilesBusiness = new Mock<IMoneyPlusMilesBusiness>();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            //_sessionHelperService = new Mock<ISessionHelperService>();
            //_applicationEnricher = new Mock<IApplicationEnricher>();
            SetHeaders();
            _moneyPlusMilesController = new MoneyPlusMilesController(_logger.Object, _configuration, _headers.Object, _moneyPlusMilesBusiness.Object,_featureSettings.Object);
            SetupHttpContextAccessor();
           
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
            string result = _moneyPlusMilesController.HealthCheck();
            Assert.True(result == "Healthy");
        }

        [Fact]
        public void GetMoneyPlusMiles_Test()
        {

            var response = new MOBFOPResponse() { TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44", CallDuration = 0 };

            var request = new GetMoneyPlusMilesRequest()
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
            _moneyPlusMilesController.ControllerContext = new ControllerContext();
            _moneyPlusMilesController.ControllerContext.HttpContext = new DefaultHttpContext();
            _moneyPlusMilesController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";


            _moneyPlusMilesBusiness.Setup(p => p.GetMoneyPlusMiles(request)).Returns(Task.FromResult(response));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _moneyPlusMilesController.GetMoneyPlusMiles(request);
            Assert.True(result.Result.TransactionId != null);
            Assert.True(result.Result.Exception == null);
        }

        [Fact]
        public void GetMoneyPlusMiles_SystemException_Test()
        {
            var request = new GetMoneyPlusMilesRequest()
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
            _moneyPlusMilesController.ControllerContext = new ControllerContext();
            _moneyPlusMilesController.ControllerContext.HttpContext = new DefaultHttpContext();
            _moneyPlusMilesController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _moneyPlusMilesBusiness.Setup(p => p.GetMoneyPlusMiles(request)).ThrowsAsync(new Exception("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _moneyPlusMilesController.GetMoneyPlusMiles(request);
            Assert.True(result.Result.Exception != null);
        }
        [Fact]
        public void GetMoneyPlusMiles_UnitedException_Test()
        {
            var request = new GetMoneyPlusMilesRequest()
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
            _moneyPlusMilesController.ControllerContext = new ControllerContext();
            _moneyPlusMilesController.ControllerContext.HttpContext = new DefaultHttpContext();
            _moneyPlusMilesController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _moneyPlusMilesBusiness.Setup(p => p.GetMoneyPlusMiles(request)).ThrowsAsync(new MOBUnitedException("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _moneyPlusMilesController.GetMoneyPlusMiles(request);
            Assert.True(result.Result.Exception.Message != null);
        }

        [Fact]
        public void ApplyMoneyPlusMiles_Test()
        {

            var response = new MOBFOPResponse() { TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44", CallDuration = 0 };

            var request = new ApplyMoneyPlusMilesOptionRequest()
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
            _moneyPlusMilesController.ControllerContext = new ControllerContext();
            _moneyPlusMilesController.ControllerContext.HttpContext = new DefaultHttpContext();
            _moneyPlusMilesController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";


            _moneyPlusMilesBusiness.Setup(p => p.ApplyMoneyPlusMiles(request)).Returns(Task.FromResult(response));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _moneyPlusMilesController.ApplyMoneyPlusMiles(request);
            Assert.True(result.Result.TransactionId != null);
            Assert.True(result.Result.Exception == null);
        }

        [Fact]
        public void ApplyMoneyPlusMiles_SystemException_Test()
        {
            var request = new ApplyMoneyPlusMilesOptionRequest()
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
            _moneyPlusMilesController.ControllerContext = new ControllerContext();
            _moneyPlusMilesController.ControllerContext.HttpContext = new DefaultHttpContext();
            _moneyPlusMilesController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _moneyPlusMilesBusiness.Setup(p => p.ApplyMoneyPlusMiles(request)).ThrowsAsync(new Exception("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _moneyPlusMilesController.ApplyMoneyPlusMiles(request);
            Assert.True(result.Result.Exception != null);
        }
        [Fact]
        public void ApplyMoneyPlusMiles_UnitedException_Test()
        {
            var request = new ApplyMoneyPlusMilesOptionRequest()
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
            _moneyPlusMilesController.ControllerContext = new ControllerContext();
            _moneyPlusMilesController.ControllerContext.HttpContext = new DefaultHttpContext();
            _moneyPlusMilesController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _moneyPlusMilesBusiness.Setup(p => p.ApplyMoneyPlusMiles(request)).ThrowsAsync(new MOBUnitedException("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _moneyPlusMilesController.ApplyMoneyPlusMiles(request);
            Assert.True(result.Result.Exception.Message != null);
        }


    }
}
