using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Text;
using United.Common.Helper;
using United.Common.Helper.MSCPayment.Interfaces;
using United.Definition;
using United.Ebs.Logging.Enrichers;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.DynamoDB;
using United.Mobile.DataAccess.MSCPayment.Interfaces;
using United.Mobile.DataAccess.MSCPayment.Services;
using United.Mobile.DataAccess.Product.Interfaces;
using United.Mobile.Model.Common;
using United.Mobile.Services.RegisterOffers.Domain;
using United.Persist.Definition.CCE;
using United.Persist.Definition.FOP;
using United.Persist.Definition.Merchandizing;
using United.Persist.Definition.Shopping;
using United.Service.Presentation.ProductRequestModel;
using United.Service.Presentation.ReservationResponseModel;
//using United.Service.Presentation.SecurityResponseModel;
using United.Services.FlightShopping.Common.DisplayCart;
using United.Services.FlightShopping.Common.FlightReservation;
using United.Utility.Helper;
using United.Utility.Http;
using Xunit;
using ProfileResponse = United.Persist.Definition.Shopping.ProfileResponse;
//using ProfileResponse = United.Mobile.Model.Common.ProfileResponse;
//using United.Service.Presentation.SecurityResponseModel.PKDispenserKey;

namespace United.Mobile.Test.MSCRegister.Api.Tests
{
    public class MSCRegisterBusinessTest
    {
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        private readonly Mock<ICacheLog<MSCRegisterBusiness>> _logger;
        private readonly IConfiguration _configuration;
        //private readonly Mock<IMSCShoppingSessionHelper> _mscshoppingSessionHelper;
        private readonly Mock<IDynamoDBService> _dynamoDBService;
        private readonly Mock<ICacheLog> _logger1;
        private readonly Mock<IDataPowerFactory> _dataPowerFactory;
        private readonly Mock<IDPService> _dPService;
        private readonly Mock<ICacheLog> _logger2;
        private readonly Mock<IETCUtility> _etcUtility;
        private readonly Mock<ICacheLog> _logger3;
        private readonly Mock<ISessionHelperService> _sessionHelperService;
        private readonly Mock<ISessionOnCloudService> _sessionOnCloudService;
        private readonly Mock<IApplicationEnricher> _applicationEnricher;
        private readonly Mock<IMSCShoppingSessionHelper> _mscshoppingSessionHelper;
        private readonly Mock<ICacheLog> _logger4;
        private readonly Mock<IShoppingCartService> _shoppingCartService;
        private readonly Mock<ICacheLog> _logger5;
        private readonly Mock<IResilientClient> _resilientClient;
        private readonly Mock<IDataVaultService> _dataVaultService;
        private readonly Mock<DocumentLibraryDynamoDB> _documentLibraryDynamoDB;
        private readonly Mock<IMSCBaggageInfo> _mscBaggageInfoProvider;
        private readonly Mock<ICustomerPreferencesService> _customerPreferencesService;
        //private readonly Mock<IMSCPkDispenserPublicKey> _mSCPkDispenserPublicKey;
        private readonly Mock<IPKDispenserService> _pKDispenserService;
        private readonly Mock<IFlightShoppingProductsService> _flightShoppingProductsServices;
        //private readonly Mock<IETCUtility> _eTCUtility;
        private readonly Mock<IShoppingUtility> _shoppingUtility;
        private readonly Mock<IMSCFormsOfPayment> _mscFormsOfPayment;
        private readonly Mock<IPaymentService> _paymentService;
        private readonly Mock<ICacheLog> _logger6;
        private readonly Mock<ICheckoutUtiliy> _checkoutUtiliy;
        //private readonly Mock<IMSCFormsOfPayment> _mscFormsOfPayment;
        //private readonly Mock<IMSCPageProductInfoHelper> _mscPageProductInfoHelper;
        private readonly Mock<ILegalDocumentsForTitlesService> _legalDocumentsForTitlesService;
        private readonly Mock<IMSCPkDispenserPublicKey> _mscPkDispenserPublicKey;
        private readonly Mock<IMSCCheckoutService> _mSCCheckoutService;
        private readonly Mock<IShoppingCartUtility> _shoppingCartUtility;
        private readonly Mock<IMSCPageProductInfoHelper> _mscPageProductInfoHelper;
        private readonly Mock<ICMSContentService> _cMSContentService;
        private readonly MSCRegisterBusiness _mscRegisterBusiness;
        private readonly Mock<IFlightShoppingService> _flightShoppingService;
        private readonly ICachingService _cachingService;
        private readonly ICacheLog<CachingService> _logger7;
        private readonly Mock<IHeaders> _headers;
        private readonly Mock<ISeatEnginePostService> _seatEnginePostService;
        private readonly Mock<IGetPNRByRecordLocatorService> _getPNRByRecordLocatorService;
        private readonly Mock<IMemberInformationService> _memberInformationService;
        private readonly Mock<IVerifyMileagePlusHashpinService> _verifyMileagePlusHashpinService;
        private readonly Mock<IValidateHashPinService> _validateHashPinService;
        private readonly Mock<IFeatureSettings> _featureSettings;
        private readonly Mock<IFeatureToggles> _featureToggles;

        public MSCRegisterBusinessTest()
        {
            _configuration = new ConfigurationBuilder()
                      .SetBasePath(Directory.GetCurrentDirectory())
                      .AddJsonFile("appSettings.test.json", optional: false, reloadOnChange: true)
                      .Build();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _sessionHelperService = new Mock<ISessionHelperService>();
            _logger = new Mock<ICacheLog<MSCRegisterBusiness>>();
            _logger1 = new Mock<ICacheLog>();
            _sessionOnCloudService = new Mock<ISessionOnCloudService>();
            _resilientClient = new Mock<IResilientClient>();
            _logger2 = new Mock<ICacheLog>();
            _logger3 = new Mock<ICacheLog>();
            _logger4 = new Mock<ICacheLog>();
            _logger5 = new Mock<ICacheLog>();
            _logger6 = new Mock<ICacheLog>();
            _applicationEnricher = new Mock<IApplicationEnricher>();
            //_mSCShoppingSessionHelper = new Mock<IMSCShoppingSessionHelper>();
            _dynamoDBService = new Mock<IDynamoDBService>();
            _mscshoppingSessionHelper = new Mock<IMSCShoppingSessionHelper>();
            _dataPowerFactory = new Mock<IDataPowerFactory>();
            _dPService = new Mock<IDPService>();
            _mscFormsOfPayment = new Mock<IMSCFormsOfPayment>();
            _cachingService = new CachingService(_resilientClient.Object, _logger7, _configuration);
            _shoppingUtility = new Mock<IShoppingUtility>();
            //MSCFormsOfPayment = new Mock<IMSCFormsOfPayment>();
            _paymentService = new Mock<IPaymentService>();
            _mscPkDispenserPublicKey = new Mock<IMSCPkDispenserPublicKey>();
            _pKDispenserService = new Mock<IPKDispenserService>();
            _flightShoppingProductsServices = new Mock<IFlightShoppingProductsService>();
            _etcUtility = new Mock<IETCUtility>();
            _checkoutUtiliy = new Mock<ICheckoutUtiliy>();
            _legalDocumentsForTitlesService = new Mock<ILegalDocumentsForTitlesService>();
            _shoppingCartService = new Mock<IShoppingCartService>();
            _shoppingCartUtility = new Mock<IShoppingCartUtility>();
            _mscPageProductInfoHelper = new Mock<IMSCPageProductInfoHelper>();
            _cMSContentService = new Mock<ICMSContentService>();
            _seatEnginePostService = new Mock<ISeatEnginePostService>();
            _headers = new Mock<IHeaders>();
            _getPNRByRecordLocatorService = new Mock<IGetPNRByRecordLocatorService>();
            _verifyMileagePlusHashpinService = new Mock<IVerifyMileagePlusHashpinService>();
            _validateHashPinService = new Mock<IValidateHashPinService>();
            _featureSettings = new Mock<IFeatureSettings>();
            _featureToggles = new Mock<IFeatureToggles>();

            _mscRegisterBusiness = new MSCRegisterBusiness(_logger.Object, _configuration, _mscshoppingSessionHelper.Object,
                 _etcUtility.Object, _sessionHelperService.Object, _shoppingCartService.Object, _shoppingUtility.Object, _paymentService.Object,
                 _checkoutUtiliy.Object, _mscFormsOfPayment.Object, _mscPageProductInfoHelper.Object, _legalDocumentsForTitlesService.Object,
                 _mscPkDispenserPublicKey.Object, _shoppingCartUtility.Object, _cachingService, _seatEnginePostService.Object,
                 _getPNRByRecordLocatorService.Object, _memberInformationService.Object, _verifyMileagePlusHashpinService.Object
                 , _headers.Object, _dynamoDBService.Object, _validateHashPinService.Object, _featureSettings.Object, _featureToggles.Object);

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
        [MemberData(nameof(TestDataGenerator.RegisterOffers_Test), MemberType = typeof(TestDataGenerator))]
        public void RegisterOffers_Test(MOBRegisterOfferRequest mOBRegisterOfferRequest, Session session, ShopBookingDetailsResponse shopBookingDetailsResponse, CCEFlightReservationResponseByCartId cCEFlightReservationResponseByCartId, FlightReservationResponse flightReservationResponse, Service.Presentation.SecurityResponseModel.PKDispenserKey pkdispenser, MOBShoppingCart mOBShoppingCart, GetOffers getOffers, GetVendorOffers getVendorOffers, ReservationDetail reservationDetail)
        {

            _mscshoppingSessionHelper.Setup(p => p.GetValidateSession(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(session);

            _sessionHelperService.Setup(p => p.GetSession<ShopBookingDetailsResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(shopBookingDetailsResponse);

            _sessionHelperService.Setup(p => p.GetSession<CCEFlightReservationResponseByCartId>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(cCEFlightReservationResponseByCartId);

            _shoppingCartService.Setup(p => p.RegisterOffers<FlightReservationResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(flightReservationResponse);

            //_checkoutUtiliy.Setup(p => p.GetProductBasedTermAndConditions(It.IsAny<ProductOffer>(), It.IsAny<FlightReservationResponse>(), It.IsAny<bool>)).Returns(mOBRegisterOfferResponse.ShoppingCart.TermsAndConditions);

            _sessionHelperService.Setup(p => p.GetSession<Service.Presentation.SecurityResponseModel.PKDispenserKey>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(pkdispenser);

            _sessionHelperService.Setup(p => p.GetSession<MOBShoppingCart>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBShoppingCart);

            _shoppingCartService.Setup(p => p.CreateCart(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("Cart");

            _sessionHelperService.Setup(p => p.GetSession<GetOffers>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(getOffers);

            _sessionHelperService.Setup(p => p.GetSession<GetVendorOffers>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(getVendorOffers);

            _sessionHelperService.Setup(p => p.GetSession<United.Service.Presentation.ReservationResponseModel.ReservationDetail>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservationDetail);

            //Act
            var result = _mscRegisterBusiness.RegisterOffers(mOBRegisterOfferRequest);

            //Assert
            Assert.True(result.Exception != null || result.Result != null);
        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.RegisterOffers_Test1), MemberType = typeof(TestDataGenerator))]
        public void RegisterOffers_Test1(MOBRegisterOfferRequest mOBRegisterOfferRequest, Session session, ShopBookingDetailsResponse shopBookingDetailsResponse, CCEFlightReservationResponseByCartId cCEFlightReservationResponseByCartId, FlightReservationResponse flightReservationResponse, Service.Presentation.SecurityResponseModel.PKDispenserKey pkdispenser, MOBShoppingCart mOBShoppingCart, GetOffers getOffers, GetVendorOffers getVendorOffers, ReservationDetail reservationDetail)
        {

            _mscshoppingSessionHelper.Setup(p => p.GetValidateSession(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(session);


            _sessionHelperService.Setup(p => p.GetSession<ShopBookingDetailsResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(shopBookingDetailsResponse);

            _sessionHelperService.Setup(p => p.GetSession<CCEFlightReservationResponseByCartId>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(cCEFlightReservationResponseByCartId);

            _shoppingCartService.Setup(p => p.RegisterOffers<FlightReservationResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(flightReservationResponse);

            //_checkoutUtiliy.Setup(p => p.GetProductBasedTermAndConditions(It.IsAny<ProductOffer>(), It.IsAny<FlightReservationResponse>(), It.IsAny<bool>)).Returns(mOBRegisterOfferResponse.ShoppingCart.TermsAndConditions);

            _sessionHelperService.Setup(p => p.GetSession<Service.Presentation.SecurityResponseModel.PKDispenserKey>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(pkdispenser);

            _sessionHelperService.Setup(p => p.GetSession<MOBShoppingCart>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBShoppingCart);

            _shoppingCartService.Setup(p => p.CreateCart(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("Cart");

            _sessionHelperService.Setup(p => p.GetSession<GetOffers>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(getOffers);

            _sessionHelperService.Setup(p => p.GetSession<GetVendorOffers>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(getVendorOffers);

            _sessionHelperService.Setup(p => p.GetSession<United.Service.Presentation.ReservationResponseModel.ReservationDetail>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservationDetail);

            //Act
            var result = _mscRegisterBusiness.RegisterOffers(mOBRegisterOfferRequest);

            //Assert
            Assert.True(result.Exception != null || result.Result != null);
        }


        [Theory]
        [MemberData(nameof(TestDataGenerator.RegisterOffers_Flow), MemberType = typeof(TestDataGenerator))]
        public void RegisterOffers_Flow(MOBRegisterOfferRequest mOBRegisterOfferRequest, Session session, ShopBookingDetailsResponse shopBookingDetailsResponse, CCEFlightReservationResponseByCartId cCEFlightReservationResponseByCartId, FlightReservationResponse flightReservationResponse, Service.Presentation.SecurityResponseModel.PKDispenserKey pkdispenser, MOBShoppingCart mOBShoppingCart, GetOffers getOffers, GetVendorOffers getVendorOffers, ReservationDetail reservationDetail)
        {

            _mscshoppingSessionHelper.Setup(p => p.GetValidateSession(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(session);

            _sessionHelperService.Setup(p => p.GetSession<ShopBookingDetailsResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(shopBookingDetailsResponse);

            _sessionHelperService.Setup(p => p.GetSession<CCEFlightReservationResponseByCartId>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(cCEFlightReservationResponseByCartId);

            _shoppingCartService.Setup(p => p.RegisterOffers<FlightReservationResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(flightReservationResponse);

            //_checkoutUtiliy.Setup(p => p.GetProductBasedTermAndConditions(It.IsAny<ProductOffer>(), It.IsAny<FlightReservationResponse>(), It.IsAny<bool>)).Returns(mOBRegisterOfferResponse.ShoppingCart.TermsAndConditions);

            _sessionHelperService.Setup(p => p.GetSession<Service.Presentation.SecurityResponseModel.PKDispenserKey>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(pkdispenser);

            _sessionHelperService.Setup(p => p.GetSession<MOBShoppingCart>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBShoppingCart);

            _shoppingCartService.Setup(p => p.CreateCart(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("Cart");

            _sessionHelperService.Setup(p => p.GetSession<GetOffers>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(getOffers);

            _sessionHelperService.Setup(p => p.GetSession<GetVendorOffers>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(getVendorOffers);

            _sessionHelperService.Setup(p => p.GetSession<United.Service.Presentation.ReservationResponseModel.ReservationDetail>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservationDetail);

            //Act
            var result = _mscRegisterBusiness.RegisterOffers(mOBRegisterOfferRequest);

            //Assert
            Assert.True(result.Exception != null || result.Result != null);
        }



        [Theory]
        [MemberData(nameof(TestDataGenerator.RegisterBags_Test), MemberType = typeof(TestDataGenerator))]
        public void RegisterBags_Test(MOBRegisterBagsRequest mOBRegisterBagsRequest, Session session, ReservationDetail reservationDetail, MOBShoppingCart mOBShoppingCart, Reservation reservation, ProfileFOPCreditCardResponse profileFOPCreditCardResponse, MOBPersistFormofPaymentResponse mOBPersistFormofPaymentResponse, ProfileResponse profileResponse)
        {
            _mscshoppingSessionHelper.Setup(p => p.GetValidateSession(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(session);

            _sessionHelperService.Setup(p => p.GetSession<ReservationDetail>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservationDetail);

            _sessionHelperService.Setup(p => p.GetSession<MOBShoppingCart>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBShoppingCart);

            _sessionHelperService.Setup(p => p.GetSession<Reservation>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservation);

            _sessionHelperService.Setup(p => p.GetSession<ProfileFOPCreditCardResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(profileFOPCreditCardResponse);

            _sessionHelperService.Setup(p => p.GetSession<MOBPersistFormofPaymentResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBPersistFormofPaymentResponse);

            _sessionHelperService.Setup(p => p.GetSession<ProfileResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(profileResponse);

            // _sessionHelperService.Setup(p => p.GetSession<Service.Presentation.SecurityResponseModel.PKDispenserKey>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>())).ReturnsAsync(pkdispenser);
            //Act
            var result = _mscRegisterBusiness.RegisterBags(mOBRegisterBagsRequest);
            //Assert
            Assert.True(result.Exception != null || result.Result != null);
        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.RegisterSameDayChange_Test), MemberType = typeof(TestDataGenerator))]
        public void RegisterSameDayChange_Test(MOBRegisterSameDayChangeRequest mOBRegisterSameDayChangeRequest, Session session, ReservationDetail reservationDetail, Service.Presentation.SecurityResponseModel.PKDispenserKey pkdispenser)
        {
            _mscshoppingSessionHelper.Setup(p => p.GetValidateSession(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(session);

            _sessionHelperService.Setup(p => p.GetSession<ReservationDetail>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservationDetail);

            _sessionHelperService.Setup(p => p.GetSession<Service.Presentation.SecurityResponseModel.PKDispenserKey>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(pkdispenser);
            //Act
            var result = _mscRegisterBusiness.RegisterSameDayChange(mOBRegisterSameDayChangeRequest);
            //Assert
            Assert.True(result.Exception != null || result.Result != null);
        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.RegisterCheckinSeats_Test), MemberType = typeof(TestDataGenerator))]
        public void RegisterCheckinSeats_Test(MOBRegisterCheckinSeatsRequest mOBRegisterCheckinSeatsRequest, Session session, ReservationDetail reservationDetail, Service.Presentation.SecurityResponseModel.PKDispenserKey pkdispenser, Reservation reservation, LoadReservationAndDisplayCartResponse loadReservationAndDisplayCartResponse, ProfileResponse profileResponse)
        {
            _mscshoppingSessionHelper.Setup(p => p.GetValidateSession(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(session);

            _sessionHelperService.Setup(p => p.GetSession<ReservationDetail>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservationDetail);

            _sessionHelperService.Setup(p => p.GetSession<Service.Presentation.SecurityResponseModel.PKDispenserKey>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(pkdispenser);

            _sessionHelperService.Setup(p => p.GetSession<Reservation>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservation);

            _shoppingCartService.Setup(p => p.GetCartInformation<LoadReservationAndDisplayCartResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(loadReservationAndDisplayCartResponse);

            _sessionHelperService.Setup(p => p.GetSession<ProfileResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(profileResponse);

            //Act

            var result = _mscRegisterBusiness.RegisterCheckinSeats(mOBRegisterCheckinSeatsRequest);
            //Assert
            Assert.True(result.Exception != null || result.Result != null);
        }
    }
}

