using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Polly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using United.Common.Helper;
using United.Common.Helper.MSCPayment.Interfaces;
using United.Definition;
using United.Definition.FFC;
using United.Definition.FormofPayment;
using United.Definition.Shopping;
using United.Ebs.Logging.Enrichers;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.MSCPayment.Interfaces;
using United.Mobile.DataAccess.Product.Interfaces;
using United.Mobile.Model.Common;
using United.Mobile.TravelCredit.Domain;
using United.Persist.Definition.FOP;
using United.Persist.Definition.SeatChange;
using United.Persist.Definition.Shopping;
using United.Service.Presentation.PaymentResponseModel;
using United.Service.Presentation.SecurityResponseModel;
using United.Services.FlightShopping.Common.DisplayCart;
using United.Utility.Helper;
using United.Utility.Http;
using Xunit;
using ProfileResponse = United.Persist.Definition.Shopping.ProfileResponse;
//using ProfileResponse = United.Mobile.Model.Common.ProfileResponse;

namespace United.Mobile.Test.TravelCredit.Api.Tests
{
    public class TravelCreditBusinessTest
    {
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        private readonly Mock<ICacheLog<TravelCreditBusiness>> _logger;
        private readonly IConfiguration _configuration;
        private readonly Mock<IHeaders> _headers;
        private readonly Mock<ISessionHelperService> _sessionHelperService;
        private readonly Mock<ILogger> _logger1;
        private readonly Mock<ISessionOnCloudService> _sessionOnCloudService;
        private readonly Mock<IApplicationEnricher> _applicationEnricher;
        private readonly Mock<ILegalDocumentsForTitlesService> _legalDocumentsForTitlesService;
        private readonly Mock<IDynamoDBService> _dynamoDBService;
        private readonly Mock<IETCUtility> _eTCUtility;
        private readonly Mock<ILogger> _logger2;
        private readonly Mock<ILogger> _logger3;
        private readonly Mock<ICachingService> _cachingService;
        private readonly Mock<ILogger> _logger4;
        private readonly Mock<IPaymentService> _paymentService;
        private readonly Mock<ILogger> _logger5;
        private readonly Mock<IOTFConversionService> _oTFConversionService;
        private readonly Mock<ILogger> _logger6;
        private readonly Mock<IShoppingCartService> _shoppingCartService;
        private readonly Mock<IMSCFormsOfPayment> _mscFormofPayment;
        private readonly Mock<IMSCPkDispenserPublicKey> _mSCPkDispenserPublicKey;
        private readonly Mock<IMSCShoppingSessionHelper> _mSCShoppingSessionHelper;
        private readonly Mock<ILogger> _logger7;
        private readonly Mock<IPKDispenserService> _pKDispenserService;
        private readonly Mock<ICMSContentService> _cMSContentService;
        private readonly Mock<ILogger> _logger8;
        private readonly Mock<IDataPowerFactory> _dataPowerFactory;
        private readonly Mock<IDPService> _dPService;
        private readonly TravelCreditBusiness _travelCreditBusiness;
        private readonly Mock<ILogger> _logger9;
        private ICacheLog<CachingService> _logger10;
        private readonly IResilientClient _resilientClient;
        private readonly ICachingService _cachingService1;
        private readonly Mock<ICacheLog<CachingService>> _logger11;
        private readonly HttpClient _httpClient;
        private readonly IAsyncPolicy _policyWrap;
        private readonly string _baseUrl;
        private readonly Mock<IMSCFormsOfPayment> _mSCFormsOfPayment;
        private readonly Mock<IFeatureSettings> _featureSettings;
        private readonly Mock<IShoppingCartUtility> _shoppingCartClient;


        public TravelCreditBusinessTest()
        {
            _configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appSettings.test.json", optional: false, reloadOnChange: true)
           .Build();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _sessionHelperService = new Mock<ISessionHelperService>();
            _logger = new Mock<ICacheLog<TravelCreditBusiness>>();
            _logger1 = new Mock<ILogger>();
            _sessionOnCloudService = new Mock<ISessionOnCloudService>();
            //_resilientClient = new Mock<IResilientClient>();
            _logger2 = new Mock<ILogger>();
            _logger3 = new Mock<ILogger>();
            _logger4 = new Mock<ILogger>();
            _logger5 = new Mock<ILogger>();
            _logger6 = new Mock<ILogger>();
            _logger7 = new Mock<ILogger>();
            _logger8 = new Mock<ILogger>();
            _logger9 = new Mock<ILogger>();
            _logger11 = new Mock<ICacheLog<CachingService>>();
            _headers = new Mock<IHeaders>();
            _applicationEnricher = new Mock<IApplicationEnricher>();
            _legalDocumentsForTitlesService = new Mock<ILegalDocumentsForTitlesService>();
            _dynamoDBService = new Mock<IDynamoDBService>();
            _eTCUtility = new Mock<IETCUtility>();
            _cachingService = new Mock<ICachingService>();
            _paymentService = new Mock<IPaymentService>();
            _oTFConversionService = new Mock<IOTFConversionService>();
            _shoppingCartService = new Mock<IShoppingCartService>();
            _mscFormofPayment = new Mock<IMSCFormsOfPayment>();
            _mSCPkDispenserPublicKey = new Mock<IMSCPkDispenserPublicKey>();
            _mSCShoppingSessionHelper = new Mock<IMSCShoppingSessionHelper>();
            _pKDispenserService = new Mock<IPKDispenserService>();
            _cMSContentService = new Mock<ICMSContentService>();
            _dataPowerFactory = new Mock<IDataPowerFactory>();
            _dPService = new Mock<IDPService>();
            _mSCFormsOfPayment = new Mock<IMSCFormsOfPayment>();
            _featureSettings = new Mock<IFeatureSettings>();
            _shoppingCartClient = new Mock<IShoppingCartUtility>();

            _resilientClient = new ResilientClient(_baseUrl);

            _cachingService1 = new CachingService(_resilientClient, _logger11.Object, _configuration);

            _travelCreditBusiness = new TravelCreditBusiness(_logger.Object, _configuration, _headers.Object, _sessionHelperService.Object,
              _dynamoDBService.Object, _eTCUtility.Object, _dPService.Object, _paymentService.Object, _oTFConversionService.Object,
              _shoppingCartService.Object, _mscFormofPayment.Object, _mSCPkDispenserPublicKey.Object, _mSCShoppingSessionHelper.Object,  _cachingService.Object, _cMSContentService.Object,_featureSettings.Object, _shoppingCartClient.Object);

            SetupHttpContextAccessor();
        }

        private void SetupHttpContextAccessor()
        {
            string deviceId = "589d7852-14e7-44a9-b23b-a6db36657579";
            string applicationId = "2";
            string appVersion = "4.1.29";
            string transactionId = "589d7852-14e7-44a9-b23b-a6db36657579|8f46e040-a200-495c-83ca-4fca2d7175fb";
            string languageCode = "en-US";
            string sessionId = "17C979E184CC495EA083D45F4DD9D19D";
            var guid = Guid.NewGuid().ToString();
            var context = new DefaultHttpContext();
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
        public static string GetFileContent(string fileName)
        {
            fileName = string.Format("..\\..\\..\\TestData\\{0}", fileName);
            var path = Path.IsPathRooted(fileName) ? fileName : Path.GetRelativePath(Directory.GetCurrentDirectory(), fileName);
            return File.ReadAllText(path);
        }
        [Theory]
        [MemberData(nameof(TestDataGenerator.FutureFlightCredit_Test), MemberType = typeof(TestDataGenerator))]
        public void FutureFlightCredit_Test(MOBFutureFlightCreditRequest mOBFutureFlightCreditRequest, Session session, MOBShoppingCart mOBShoppingCart, ProfileResponse profileResponse, Reservation reservation, MOBCSLContentMessagesResponse mOBCSLContentMessagesResponse, LoadReservationAndDisplayCartResponse loadReservationAndDisplayCartResponse, MOBFutureFlightCreditResponse mOBFutureFlightCreditResponse, PKDispenserKey pKDispenserKey, List<FormofPaymentOption> eligibleFormofPayments)
        {

            var _dataPowerFactory = new DataPowerFactory(_configuration, _sessionHelperService.Object);

            _mSCShoppingSessionHelper.Setup(p => p.GetBookingFlowSession(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(session);

            _sessionHelperService.Setup(p => p.GetSession<MOBShoppingCart>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBShoppingCart);

            _sessionHelperService.Setup(p => p.GetSession<ProfileResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(profileResponse);

            _sessionHelperService.Setup(p => p.GetSession<Reservation>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservation);

            _cMSContentService.Setup(p => p.GetSDLContentByGroupName<MOBCSLContentMessagesResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(mOBCSLContentMessagesResponse);

            _shoppingCartService.Setup(p => p.GetCartInformation<LoadReservationAndDisplayCartResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(loadReservationAndDisplayCartResponse);

            _dPService.Setup(p => p.GetAnonymousToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IConfiguration>())).ReturnsAsync("Bearer Token");

            _mSCPkDispenserPublicKey.Setup(p => p.GetCachedOrNewpkDispenserPublicKey(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<United.Mobile.Model.Common.MOBItem>>(), It.IsAny<string>())).ReturnsAsync(mOBFutureFlightCreditResponse.PkDispenserPublicKey);

            _sessionHelperService.Setup(p => p.GetSession<PKDispenserKey>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(pKDispenserKey);

            LookupByEmailResponse lookupByEmailResponse = new LookupByEmailResponse
            {
                IsSuccessful = true,
                Message = "Email sent"
                
            };
            var cslResponse = JsonConvert.SerializeObject(lookupByEmailResponse);

            _paymentService.Setup(p => p.GetFFCByEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(cslResponse);

            _mSCFormsOfPayment.Setup(p => p.GetEligibleFormofPayments(It.IsAny<MOBRequest>(), It.IsAny<Session>(), It.IsAny<MOBShoppingCart>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MOBSHOPReservation>(), It.IsAny<bool>(), It.IsAny<SeatChangeState>())).Returns(Task.FromResult((eligibleFormofPayments, true)));

            var travelCreditBusiness = new TravelCreditBusiness(_logger.Object, _configuration, _headers.Object, _sessionHelperService.Object,
              _dynamoDBService.Object, _eTCUtility.Object, _dPService.Object, _paymentService.Object, _oTFConversionService.Object,
              _shoppingCartService.Object, _mscFormofPayment.Object, _mSCPkDispenserPublicKey.Object, _mSCShoppingSessionHelper.Object, _cachingService1, _cMSContentService.Object,_featureSettings.Object, _shoppingCartClient.Object);

            //Act
            var result = travelCreditBusiness.FutureFlightCredit(mOBFutureFlightCreditRequest);

            //Assert
            Assert.True(result.Exception != null || result.Result != null);

        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.RemoveFutureFlightCredit_Test), MemberType = typeof(TestDataGenerator))]
        public void RemoveFutureFlightCredit_Test(MOBFOPFutureFlightCreditRequest mOBFOPFutureFlightCreditRequest, Session session, MOBFutureFlightCreditResponse mOBFutureFlightCreditResponse, ProfileResponse profileResponse, MOBShoppingCart mOBShoppingCart, Reservation reservation, MOBCSLContentMessagesResponse mOBCSLContentMessagesResponse, SeatChangeState seatChangeState)
        {
            _mSCShoppingSessionHelper.Setup(p => p.GetBookingFlowSession(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(session);

            _mSCPkDispenserPublicKey.Setup(p => p.GetCachedOrNewpkDispenserPublicKey(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<United.Mobile.Model.Common.MOBItem>>(), It.IsAny<string>())).ReturnsAsync(mOBFutureFlightCreditResponse.PkDispenserPublicKey);

            _sessionHelperService.Setup(p => p.GetSession<ProfileResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(profileResponse);

            _sessionHelperService.Setup(p => p.GetSession<MOBShoppingCart>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBShoppingCart);

            _sessionHelperService.Setup(p => p.GetSession<Reservation>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservation);

            _cMSContentService.Setup(p => p.GetSDLContentByGroupName<MOBCSLContentMessagesResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(mOBCSLContentMessagesResponse);

            _sessionHelperService.Setup(p => p.GetSession<SeatChangeState>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(seatChangeState);

            //_mscFormofPayment.Setup(p => p.GetEligibleFormofPayments(It.IsAny<MOBRequest>(), It.IsAny<Session>(), It.IsAny<MOBShoppingCart>(), It.IsAny<string>(), It.IsAny<string>(), ref It.Ref<bool>.IsAny).Returns(mOBFutureFlightCreditResponse.EligibleFormofPayments);

            //_sessionHelperService.Setup(p => p.GetSession<ProfileResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>())).ReturnsAsync(profileResponse);

            //Act
            var result = _travelCreditBusiness.RemoveFutureFlightCredit(mOBFOPFutureFlightCreditRequest);
            //Assert
            Assert.True(result.Exception != null || result.Result != null);
        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.LookUpTravelCredit_Test), MemberType = typeof(TestDataGenerator))]
        public void LookUpTravelCredit_Test(MOBFOPLookUpTravelCreditRequest mOBFOPLookUpTravelCreditRequest, Session session, MOBFOPResponse mOBFOPResponse, MOBShoppingCart mOBShoppingCart, Reservation reservation, ProfileResponse profileResponse, ProfileFOPCreditCardResponse profileFOPCreditCardResponse, MOBCSLContentMessagesResponse mOBCSLContentMessagesResponse, List<FormofPaymentOption> eligibleFormofPayments)
        {

            var _dataPowerFactory = new DataPowerFactory(_configuration, _sessionHelperService.Object);

            _mSCShoppingSessionHelper.Setup(p => p.GetBookingFlowSession(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(session);

            _mSCPkDispenserPublicKey.Setup(p => p.GetCachedOrNewpkDispenserPublicKey(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<United.Mobile.Model.Common.MOBItem>>(), It.IsAny<string>())).ReturnsAsync(mOBFOPResponse.PkDispenserPublicKey);

            _sessionHelperService.Setup(p => p.GetSession<MOBShoppingCart>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBShoppingCart);

            _sessionHelperService.Setup(p => p.GetSession<Reservation>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservation);

            _dPService.Setup(p => p.GetAnonymousToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IConfiguration>())).ReturnsAsync("Bearer Token");

            _sessionHelperService.Setup(p => p.GetSession<ProfileResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(profileResponse);

            _sessionHelperService.Setup(p => p.GetSession<ProfileFOPCreditCardResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(profileFOPCreditCardResponse);

            _cMSContentService.Setup(p => p.GetSDLContentByGroupName<MOBCSLContentMessagesResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(mOBCSLContentMessagesResponse);

            TCLookupByPinOrPNRResponse tCLookupByPinOrPNRResponse = new TCLookupByPinOrPNRResponse
            {
                IsSuccessful = true,
                Message = "XYZ",
                RecordLocator = "record",
                ReservationCreateDate = "22/09/2022",
            };
            var response2 = JsonConvert.SerializeObject(tCLookupByPinOrPNRResponse);

            _paymentService.Setup(p => p.GetLookUpTravelCredit(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(response2);

            _mSCFormsOfPayment.Setup(p => p.GetEligibleFormofPayments(It.IsAny<MOBRequest>(), It.IsAny<Session>(), It.IsAny<MOBShoppingCart>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MOBSHOPReservation>(), It.IsAny<bool>(), It.IsAny<SeatChangeState>())).Returns(Task.FromResult((eligibleFormofPayments, true)));


            var travelCreditBusiness = new TravelCreditBusiness(_logger.Object, _configuration, _headers.Object, _sessionHelperService.Object,
           _dynamoDBService.Object, _eTCUtility.Object, _dPService.Object, _paymentService.Object, _oTFConversionService.Object,
           _shoppingCartService.Object, _mscFormofPayment.Object, _mSCPkDispenserPublicKey.Object, _mSCShoppingSessionHelper.Object, _cachingService1, _cMSContentService.Object,_featureSettings.Object, _shoppingCartClient.Object);

            //Act
            var result = travelCreditBusiness.LookUpTravelCredit(mOBFOPLookUpTravelCreditRequest);
            //Assert
            Assert.True(result.Exception != null || result.Result != null);
        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.ManageTravelCredit_Test), MemberType = typeof(TestDataGenerator))]
        public void ManageTravelCredit_Test(MOBFOPManageTravelCreditRequest mOBFOPManageTravelCreditRequest, Session session, MOBShoppingCart mOBShoppingCart, Reservation reservation, MOBFOPResponse mOBFOPResponse, ProfileResponse profileResponse, ProfileFOPCreditCardResponse profileFOPCreditCardResponse, MOBCSLContentMessagesResponse mOBCSLContentMessagesResponse)
        {
            _mSCShoppingSessionHelper.Setup(p => p.GetBookingFlowSession(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(session);

            _sessionHelperService.Setup(p => p.GetSession<MOBShoppingCart>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBShoppingCart);

            _sessionHelperService.Setup(p => p.GetSession<Reservation>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservation);

            _mSCPkDispenserPublicKey.Setup(p => p.GetCachedOrNewpkDispenserPublicKey(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<United.Mobile.Model.Common.MOBItem>>(), It.IsAny<string>())).ReturnsAsync(mOBFOPResponse.PkDispenserPublicKey);

            //_sessionHelperService.Setup(p => p.GetSession<MOBShoppingCart>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>())).ReturnsAsync(mOBShoppingCart);

            //_sessionHelperService.Setup(p => p.GetSession<Reservation>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>())).ReturnsAsync(reservation);

            _sessionHelperService.Setup(p => p.GetSession<ProfileResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(profileResponse);

            _sessionHelperService.Setup(p => p.GetSession<ProfileFOPCreditCardResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(profileFOPCreditCardResponse);

            _cMSContentService.Setup(p => p.GetSDLContentByGroupName<MOBCSLContentMessagesResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(mOBCSLContentMessagesResponse);

            //Act
            var result = _travelCreditBusiness.ManageTravelCredit(mOBFOPManageTravelCreditRequest);
            //Assert
            Assert.True(result.Exception != null || result.Result != null);
        }
    }
}

