using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using United.Common.Helper;
using United.Definition;
using United.Definition.FormofPayment;
//using United.Definition.FormofPayment;
using United.Definition.SDL;
//using United.Common.Helper.TravelCredit;
using United.Ebs.Logging.Enrichers;
using United.Mobile.DataAccess.Common;
//using United.Mobile.DataAccess.InFlightPurchase;
//using United.Mobile.DataAccess.MemberSignIn;
using United.Mobile.DataAccess.MSCPayment.Interfaces;
//using United.Mobile.DataAccess.Profile;
using United.Mobile.InFlightPurchase.Domain;
using United.Mobile.Model.Common;
using United.Persist.Definition.InflightPurchase;
//using United.Mobile.Model.MOBKVP;
//using United.Mobile.Model.InFlightPurchase;
//using United.Mobile.Services.InFlightPurchase.Domain;
using United.Service.Presentation.CommonModel;
using United.Utility.Helper;
//using United.Definition.FormofPayment.MOBSavedCCInflightPurchaseRequest;
using Xunit;
using IPurchaseMerchandizingService = United.Mobile.DataAccess.MSCPayment.Interfaces.IPurchaseMerchandizingService;

namespace United.Mobile.Test.InFlightPurchase.Api.Tests
{
    public class InFlightPurchaseBusinessTests
    {
        private readonly Mock<ICacheLog<InFlightPurchaseBusiness>> _logger;
        private readonly IConfiguration _configuration;
        private readonly Mock<IHeaders> _headers;
        private readonly Mock<ISessionHelperService> _sessionHelperService;
        private readonly Mock<ISessionOnCloudService> _sessionOnCloudService;
        private readonly Mock<IApplicationEnricher> _applicationEnricher;
        private readonly Mock<IReferencedataService> _referencedataService;
        private readonly Mock<IDataVaultService> _dataVaultService;
        private readonly Mock<IDPService> _dPService;
        private readonly Mock<IPurchaseMerchandizingService> _purchaseMerchandizingService;
        private readonly Mock<IGetTravelersService> _getTravelersService;
        private readonly Mock<IGetMPNumberService> _getMPNumberService;
        private readonly Mock<IGetSDLKeyValuePairContentService> _getSDLKeyValuePairContentService;
        private readonly InFlightPurchaseBusiness _inFlightPurchaseBusiness;
        private readonly Mock<IDynamoDBService> _dynamodbService;
        public InFlightPurchaseBusinessTests()
        {

            _logger = new Mock<ICacheLog<InFlightPurchaseBusiness>>();
            _configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.test.json", optional: false, reloadOnChange: true)
               .Build();
            _headers = new Mock<IHeaders>();
            _sessionHelperService = new Mock<ISessionHelperService>();
            _sessionOnCloudService = new Mock<ISessionOnCloudService>();
            _applicationEnricher = new Mock<IApplicationEnricher>();
            _referencedataService = new Mock<IReferencedataService>();
            _dataVaultService = new Mock<IDataVaultService>();
            _dPService = new Mock<IDPService>();
            _purchaseMerchandizingService = new Mock<IPurchaseMerchandizingService>();
            _getTravelersService = new Mock<IGetTravelersService>();
            _getMPNumberService = new Mock<IGetMPNumberService>();
            _getSDLKeyValuePairContentService = new Mock<IGetSDLKeyValuePairContentService>();
            _dynamodbService = new Mock<IDynamoDBService>();

            _inFlightPurchaseBusiness = new InFlightPurchaseBusiness(_logger.Object, _configuration, _headers.Object, _sessionHelperService.Object, _referencedataService.Object,
                _dPService.Object, _dataVaultService.Object, _purchaseMerchandizingService.Object, _getTravelersService.Object, _getMPNumberService.Object, _getSDLKeyValuePairContentService.Object,
                 _dynamodbService.Object);
            SetHeaders();
        }
        private void SetHeaders(string deviceId = "D873298F-F27D-4AEC-BE6C-DE79C4259626"
             , string applicationId = "2"
             , string appVersion = "4.1.26"
             , string transactionId = "3f575588-bb12-41fe-8be7-f57c55fe7762|afc1db10-5c39-4ef4-9d35-df137d56a23e"
             , string languageCode = "en-US"
             , string sessionId = "D58E298C35274F6F873A133386A42916")
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

        [Theory]
        [MemberData(nameof(TestDataGenerator.InFlightPurchase_Test), MemberType = typeof(TestDataGenerator))]

        public void InFlightPurchase_Test(MOBInFlightPurchaseRequest mOBInFlightPurchaseRequest,
            SDLKeyValuePairContentResponse sDLKeyValuePairContentResponse
          )
        {
            _dPService.Setup(p => p.GetAnonymousToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IConfiguration>())).ReturnsAsync("Bearer Token");

           
            //_purchaseMerchandizingService.Setup(p => p.GetInflightPurchaseEligibility<ProductEligibilityResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(productEligibilityResponse, callDuration);

            _sessionHelperService.Setup(p => p.GetSession<SDLKeyValuePairContentResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(sDLKeyValuePairContentResponse);

            SDLKeyValuePairContentResponse sDLKeyValuePairContentResponse1 = new SDLKeyValuePairContentResponse
            {
                content=new List<KeyValuePairContent>
                {
                    new KeyValuePairContent
                    {
                        content=new KeyValuePairSubContent
                        {
                            labels=new List<KeyValuePairLabelContent>
                            {
                                new KeyValuePairLabelContent
                                {
                                    content=  new Model.Common.KeyValuePair
                                    {
                                        Key="Testkey",
                                        Value="Testvalue"
                                    }
                                }
                            }
                        }

                    }
                },
                ErrorList=new List<SDLError>
                {
                    new SDLError
                    {
                        Code="Testcode",
                        FieldName="name",
                        Message="Testmessage"
                    }
                }
            };
            string stringjson = JsonConvert.SerializeObject(sDLKeyValuePairContentResponse1);
            
            _getSDLKeyValuePairContentService.Setup(p => p.GetSDLKeyValuePairContentByPageName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(stringjson);

            _getTravelersService.Setup(p => p.GetTravelersSavedCCDetails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("Bearer Token");

            _getMPNumberService.Setup(p => p.GetMPNumberByEmployeeId(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            _getMPNumberService.Setup(p => p.GetSavedCCForMileaguePlusMember(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            _getMPNumberService.Setup(p => p.GetProfileAddressByKey(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            _getTravelersService.Setup(p => p.LookupAndSaveProfileCard(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            var result = _inFlightPurchaseBusiness.EiligibilityCheckToAddNewCCForInFlightPurchase(mOBInFlightPurchaseRequest);

            if (result.Exception == null)
                Assert.True(result.Result != null && result.Result.TransactionId != null);
            else
                Assert.True(result.Exception != null && result.Exception.InnerException != null);
        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.InFlightPurchase_Test1), MemberType = typeof(TestDataGenerator))]

        public void InFlightPurchase_Test1(MOBSavedCCInflightPurchaseRequest mOBSavedCCInflightPurchaseRequest)
        {
            _dPService.Setup(p => p.GetAnonymousToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IConfiguration>())).ReturnsAsync("Bearer Token");

            _dataVaultService.Setup(p => p.GetCCTokenWithDataVault(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("Bearer Token");

            _referencedataService.Setup(p => p.GetDataGetAsync<List<StateProvince>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            _getTravelersService.Setup(p => p.GetCslApiResponse(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("Bearer Token");


            var result = _inFlightPurchaseBusiness.SaveCCPNROnlyForInflightPurchase(mOBSavedCCInflightPurchaseRequest);

            if (result.Exception == null)
                Assert.True(result.Result != null && result.Result.TransactionId != null);
            else
                Assert.True(result.Exception != null && result.Exception.InnerException != null);
        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.InFlightPurchase_Test2), MemberType = typeof(TestDataGenerator))]

        public void InFlightPurchase_Test2(MOBSavedCCInflightPurchaseRequest mOBSavedCCInflightPurchaseRequest, MOBInFlightPurchaseResponse response)
        {
            _dPService.Setup(p => p.GetAnonymousToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IConfiguration>())).ReturnsAsync("Bearer Token");

            _getTravelersService.Setup(p => p.GetCslApiResponse(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("Bearer Token");

            _sessionHelperService.Setup(p => p.GetSession<SDLKeyValuePairContentResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>()));

            _getSDLKeyValuePairContentService.Setup(p => p.GetSDLKeyValuePairContentByPageName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("Bearer Token");

            _sessionHelperService.Setup(p => p.GetSession<ProfileCreditCardResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>()));

            _getTravelersService.Setup(p => p.OptOutMPCardInflightPurchase(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("Bearer Token");

            _getTravelersService.Setup(p => p.OptOutBookingCardInflightPurchase(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("Bearer Token");


            var result = _inFlightPurchaseBusiness.VerifySavedCCForInflightPurchase(mOBSavedCCInflightPurchaseRequest);

            if (result.Exception == null)
                Assert.True(result.Result != null && result.Result.TransactionId != null);
            else
                Assert.True(result.Exception != null && result.Exception.InnerException != null);

        }
    }
}
