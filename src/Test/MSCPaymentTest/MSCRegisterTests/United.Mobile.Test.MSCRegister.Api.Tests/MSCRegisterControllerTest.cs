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
using United.Mobile.Services.RegisterOffers.Api.Controllers;
using United.Mobile.Services.RegisterOffers.Domain;
using United.Persist.Definition.Shopping;
using United.Utility.Helper;
using Xunit;
using Constants = United.Mobile.Model.Constants;

namespace United.Mobile.Test.MSCRegister.Api.Tests
{
    public class MSCRegisterControllerTest
    {
        private readonly Mock<ICacheLog<MSCRegisterController>> _logger;
        private IConfiguration _configuration;
        private readonly Mock<IHeaders> _headers;
        private readonly Mock<IMSCRegisterBusiness> _mscRegisterBusiness;
        private readonly MSCRegisterController _mSCRegisterController;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        private readonly Mock<IFeatureSettings> _featureSettings;
        public IConfiguration Configuration
        {
            get
            {
                _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.test.json", optional: false, reloadOnChange: true)
                .Build();
                return _configuration;
            }
        }
        public MSCRegisterControllerTest()
        {
            _logger = new Mock<ICacheLog<MSCRegisterController>>();
            _headers = new Mock<IHeaders>();
            _mscRegisterBusiness = new Mock<IMSCRegisterBusiness>();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _mSCRegisterController = new MSCRegisterController(_logger.Object, _configuration, _headers.Object, _mscRegisterBusiness.Object,_featureSettings.Object);
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
            string result = _mSCRegisterController.HealthCheck();
            Assert.True(result == "Healthy");
        }

        [Fact]
        public void RegisterOffers_Test()
        {
            var response = new MOBRegisterOfferResponse() { TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44", CallDuration = 0 };
            //var request = new Request<RegisterOfferRequest>()
            var request = new MOBRegisterOfferRequest()
            {

                SessionId = "0B4D8C69883C46EFB69177D68387BA73",
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
            _mSCRegisterController.ControllerContext = new ControllerContext();
            _mSCRegisterController.ControllerContext.HttpContext = new DefaultHttpContext();
            _mSCRegisterController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _mscRegisterBusiness.Setup(p => p.RegisterOffers(request)).Returns(Task.FromResult(response));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _mSCRegisterController.RegisterOffers(request);
            Assert.True(result.Result.TransactionId != null);
            Assert.True(result.Result.Exception == null);
        }

        [Fact]
        public void RegisterOffers_SystemException_Test()
        {
            var request = new MOBRegisterOfferRequest()
            {
                SessionId = "0B4D8C69883C46EFB69177D68387BA73",
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
            _mSCRegisterController.ControllerContext = new ControllerContext();
            _mSCRegisterController.ControllerContext.HttpContext = new DefaultHttpContext();
            _mSCRegisterController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _mscRegisterBusiness.Setup(p => p.RegisterOffers(request)).ThrowsAsync(new Exception("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _mSCRegisterController.RegisterOffers(request);


            Assert.True(result != null);
            
        }

        [Fact]
        public void RegisterOffers_UnitedException_Test()
        {
            //var request = new Request<RegisterOfferRequest>()
            var request = new MOBRegisterOfferRequest()
            {

                SessionId = "0B4D8C69883C46EFB69177D68387BA73",
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
            _mSCRegisterController.ControllerContext = new ControllerContext();
            _mSCRegisterController.ControllerContext.HttpContext = new DefaultHttpContext();
            _mSCRegisterController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _mscRegisterBusiness.Setup(p => p.RegisterOffers(request)).ThrowsAsync(new MOBUnitedException("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _mSCRegisterController.RegisterOffers(request);
            Assert.True(result.Result.Exception.Message != null);
        }


        [Fact]
        public void RegisterBags_Test()
        {
            var response = new MOBRegisterOfferResponse() { TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44", CallDuration = 0 };
            //var request = new Request<MOBRegisterBagsRequest>()
            var request = new MOBRegisterBagsRequest()
            {

                SessionId = "0B4D8C69883C46EFB69177D68387BA73",
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
            _mSCRegisterController.ControllerContext = new ControllerContext();
            _mSCRegisterController.ControllerContext.HttpContext = new DefaultHttpContext();
            _mSCRegisterController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _mscRegisterBusiness.Setup(p => p.RegisterBags(request)).Returns(Task.FromResult(response));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _mSCRegisterController.RegisterBags(request);
            Assert.True(result.Result.TransactionId != null);
            Assert.True(result.Result.Exception == null);
        }

        [Fact]
        public void RegisterBags_SystemException_Test()
        {
            //var request = new Request<MOBRegisterBagsRequest>()
            var request = new MOBRegisterBagsRequest()
            {
                SessionId = "0B4D8C69883C46EFB69177D68387BA73",
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
            _mSCRegisterController.ControllerContext = new ControllerContext();
            _mSCRegisterController.ControllerContext.HttpContext = new DefaultHttpContext();
            _mSCRegisterController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";
            //todo
            _mscRegisterBusiness.Setup(p => p.RegisterBags(request)).ThrowsAsync(new Exception("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _mSCRegisterController.RegisterBags(request);
            Assert.True(result != null);
        }

        [Fact]
        public void RegisterBags_UnitedException_Test()
        {
            //var request = new Request<MOBRegisterBagsRequest>()
            var request = new MOBRegisterBagsRequest()
            {

                SessionId = "0B4D8C69883C46EFB69177D68387BA73",
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
            _mSCRegisterController.ControllerContext = new ControllerContext();
            _mSCRegisterController.ControllerContext.HttpContext = new DefaultHttpContext();
            _mSCRegisterController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _mscRegisterBusiness.Setup(p => p.RegisterBags(request)).ThrowsAsync(new MOBUnitedException("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _mSCRegisterController.RegisterBags(request);
            Assert.True(result.Result.Exception.Message != null);
        }

        [Fact]
        public void RegisterSameDayChange_Test()
        {
            var response = new MOBRegisterOfferResponse() { TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44", CallDuration = 0 };
            // var request = new Request<MOBRegisterSameDayChangeRequest>()
            var request = new MOBRegisterSameDayChangeRequest()
            {

                SessionId = "0B4D8C69883C46EFB69177D68387BA73",
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
            _mSCRegisterController.ControllerContext = new ControllerContext();
            _mSCRegisterController.ControllerContext.HttpContext = new DefaultHttpContext();
            _mSCRegisterController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _mscRegisterBusiness.Setup(p => p.RegisterSameDayChange(request)).Returns(Task.FromResult(response));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _mSCRegisterController.RegisterSameDayChange(request);
            Assert.True(result.Result.TransactionId != null);
            Assert.True(result.Result.Exception == null);
        }

        [Fact]
        public void RegisterSameDayChange_SystemException_Test()
        {
            // var request = new Request<MOBRegisterSameDayChangeRequest>()
            var request = new MOBRegisterSameDayChangeRequest()
            {
                SessionId = "0B4D8C69883C46EFB69177D68387BA73",
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
            _mSCRegisterController.ControllerContext = new ControllerContext();
            _mSCRegisterController.ControllerContext.HttpContext = new DefaultHttpContext();
            _mSCRegisterController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            //todo
            _mscRegisterBusiness.Setup(p => p.RegisterSameDayChange(request)).ThrowsAsync(new Exception("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _mSCRegisterController.RegisterSameDayChange(request);
            Assert.True(result != null);
        }

        [Fact]
        public void RegisterSameDayChange_UnitedException_Test()
        {
            //var request = new Request<MOBRegisterSameDayChangeRequest>()
            var request = new MOBRegisterSameDayChangeRequest()
            {

                SessionId = "0B4D8C69883C46EFB69177D68387BA73",
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
            _mSCRegisterController.ControllerContext = new ControllerContext();
            _mSCRegisterController.ControllerContext.HttpContext = new DefaultHttpContext();
            _mSCRegisterController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _mscRegisterBusiness.Setup(p => p.RegisterSameDayChange(request)).ThrowsAsync(new MOBUnitedException("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _mSCRegisterController.RegisterSameDayChange(request);
            Assert.True(result.Result.Exception.Message != null);
        }

        [Fact]
        public void RegisterCheckinSeats_Test()
        {
            var response = new MOBRegisterOfferResponse() { TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44", CallDuration = 0 };
            //var request = new Request<MOBRegisterCheckinSeatsRequest>()
            var request = new MOBRegisterCheckinSeatsRequest()
            {

                SessionId = "0B4D8C69883C46EFB69177D68387BA73",
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
            _mSCRegisterController.ControllerContext = new ControllerContext();
            _mSCRegisterController.ControllerContext.HttpContext = new DefaultHttpContext();
            _mSCRegisterController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _mscRegisterBusiness.Setup(p => p.RegisterCheckinSeats(request)).Returns(Task.FromResult(response));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _mSCRegisterController.RegisterCheckinSeats(request);
            Assert.True(result.Result.TransactionId != null);
            Assert.True(result.Result.Exception == null);
        }

        [Fact]
        public void RegisterCheckinSeats_SystemException_Test()
        {
            //var request = new Request<MOBRegisterCheckinSeatsRequest>()
            var request = new MOBRegisterCheckinSeatsRequest()
            {

                SessionId = "0B4D8C69883C46EFB69177D68387BA73",
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
            _mSCRegisterController.ControllerContext = new ControllerContext();
            _mSCRegisterController.ControllerContext.HttpContext = new DefaultHttpContext();
            _mSCRegisterController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            //todo
            _mscRegisterBusiness.Setup(p => p.RegisterCheckinSeats(request)).ThrowsAsync(new Exception("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _mSCRegisterController.RegisterCheckinSeats(request);
            Assert.True(result != null);
        }

        [Fact]
        public void RegisterCheckinSeats_UnitedException_Test()
        {
            //var request = new Request<MOBRegisterCheckinSeatsRequest>()
            var request = new MOBRegisterCheckinSeatsRequest()
            {

                SessionId = "0B4D8C69883C46EFB69177D68387BA73",
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
            _mSCRegisterController.ControllerContext = new ControllerContext();
            _mSCRegisterController.ControllerContext.HttpContext = new DefaultHttpContext();
            _mSCRegisterController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _mscRegisterBusiness.Setup(p => p.RegisterCheckinSeats(request)).ThrowsAsync(new MOBUnitedException("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _mSCRegisterController.RegisterCheckinSeats(request);
            Assert.True(result.Result.Exception.Message != null);
        }
    }
}

