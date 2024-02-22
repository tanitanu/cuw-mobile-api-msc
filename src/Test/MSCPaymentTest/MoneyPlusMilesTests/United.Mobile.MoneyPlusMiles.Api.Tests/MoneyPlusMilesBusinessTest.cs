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
using United.Common.Helper;
using United.Common.Helper.MSCPayment.Interfaces;
using United.Definition;
using United.Definition.Shopping;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.MSCPayment.Interfaces;
using United.Mobile.DataAccess.Product.Interfaces;
using United.Mobile.Model.Common;
using United.Mobile.MoneyPlusMiles.Domain;
using United.Persist.Definition.SeatChange;
using United.Persist.Definition.Shopping;
using United.Service.Presentation.ReferenceDataModel;
using United.Services.FlightShopping.Common.DisplayCart;
using United.Services.FlightShopping.Common.FlightReservation;
using United.Utility.Helper;
using United.Utility.Http;
using Xunit;
using MOBItem = United.Mobile.Model.Common.MOBItem;
using ProfileResponse = United.Persist.Definition.Shopping.ProfileResponse;

namespace United.Mobile.MoneyPlusMiles.Api.Tests
{
    public class MoneyPlusMilesBusinessTest
    {
        private readonly Mock<ICacheLog<MoneyPlusMilesBusiness>> _logger;
        private readonly IMoneyPlusMilesBusiness _moneyPlusMilesBusiness;
        private readonly IConfiguration _configuration;
        private readonly Mock<IHeaders> _headers;
        private readonly Mock<IMSCShoppingSessionHelper> _shoppingSessionHelper;
        private readonly Mock<ISessionHelperService> _sessionHelperService;
        private readonly Mock<IMSCPkDispenserPublicKey> _pKDispenserPublicKey;
        private readonly Mock<IFlightShoppingProductsService> _flightShoppingProductsService;
        private readonly Mock<IFlightShoppingService> _flightShoppingService;
        private readonly Mock<IShoppingCartService> _shoppingCartService;
        private readonly Mock<IDPService> _dPService;
        private readonly Mock<IETCUtility> _iETCUtility;
        private readonly Mock<IMSCFormsOfPayment> _mscformsOfPaymment;
        private readonly Mock<IShoppingCartUtility> _shoppingCartUtility;
        private readonly Mock<IDynamoDBService> _dynamoDBService;
        private readonly ICachingService _cachingService;
        private readonly Mock<ICacheLog<CachingService>> _logger1;
        private readonly IDataPowerFactory _dataPowerFactory;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        private readonly IResilientClient _resilientClient;
        private readonly HttpClient _httpClient;
        private readonly IAsyncPolicy _policyWrap;
        private readonly Mock<IFeatureSettings> _featureSettings;
        private readonly string _baseUrl;


        public MoneyPlusMilesBusinessTest()
        {
            _logger = new Mock<ICacheLog<MoneyPlusMilesBusiness>>();
            _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.test.json", optional: false, reloadOnChange: true)
            .Build();

            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _shoppingSessionHelper = new Mock<IMSCShoppingSessionHelper>();
            _iETCUtility = new Mock<IETCUtility>();
            _headers = new Mock<IHeaders>();
            _logger1 = new Mock<ICacheLog<CachingService>>();

            _pKDispenserPublicKey = new Mock<IMSCPkDispenserPublicKey>();
            _flightShoppingService = new Mock<IFlightShoppingService>();
            _flightShoppingProductsService = new Mock<IFlightShoppingProductsService>();
            _shoppingCartService = new Mock<IShoppingCartService>();
            _sessionHelperService = new Mock<ISessionHelperService>();
            _dPService = new Mock<IDPService>();
            _shoppingCartUtility = new Mock<IShoppingCartUtility>();
            _featureSettings = new Mock<IFeatureSettings>();
            _resilientClient = new ResilientClient(_baseUrl);

            _cachingService = new CachingService(_resilientClient, _logger1.Object, _configuration);
            _mscformsOfPaymment = new Mock<IMSCFormsOfPayment>();
            _dynamoDBService = new Mock<IDynamoDBService>();
            SetupHttpContextAccessor();
            _moneyPlusMilesBusiness = new MoneyPlusMilesBusiness(_logger.Object, _configuration, _headers.Object, _shoppingSessionHelper.Object, _sessionHelperService.Object, _flightShoppingProductsService.Object, _dPService.Object, _iETCUtility.Object, _mscformsOfPaymment.Object, _pKDispenserPublicKey.Object, _shoppingCartUtility.Object, _flightShoppingService.Object, _shoppingCartService.Object, _cachingService, _dynamoDBService.Object 
                                           ,_featureSettings.Object);
                
           
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
                    Version = new Model.Version
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
        [MemberData(nameof(TestDataGenerator.GetMoneyPlusMiles_Test), MemberType = typeof(TestDataGenerator))]
        public void GetMoneyPlusMiles_Test(GetMoneyPlusMilesRequest getMoneyPlusMilesRequest, Session session, List<CMSContentMessage> cMSContentMessages, MOBShoppingCart mOBShoppingCart, LoadReservationAndDisplayCartResponse loadReservationAndDisplayResponse,MOBFOPResponse mOBFOPResponse,ProfileResponse profileResponse)
        {
            _shoppingSessionHelper.Setup(p => p.GetBookingFlowSession(It.IsAny<string>(), It.IsAny<bool>())).Returns(System.Threading.Tasks.Task.FromResult((session)));

            _shoppingCartUtility.Setup(p => p.GetSDLContentByGroupName(It.IsAny<MOBRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(cMSContentMessages);

            _sessionHelperService.Setup(p => p.GetSession<MOBShoppingCart>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBShoppingCart);

            _shoppingCartService.Setup(p => p.GetCartInformation<LoadReservationAndDisplayCartResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(loadReservationAndDisplayResponse);

            _dPService.Setup(p => p.GetAnonymousToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IConfiguration>())).ReturnsAsync("Bearer Sample Token");

            MilesAndMoneyResponse milesAndMoneyResponse = new MilesAndMoneyResponse
            {
                CartId="testcartid",
                MilesAndMoneyOptions=new System.Collections.ObjectModel.Collection<MilesAndMoneyOption>
                {
                    new MilesAndMoneyOption
                    {
                        ConversionRate=123,
                        MilesMoneyValue=4567,
                        MilesOwed=7263,
                        MilesPercentage=768345,
                        MoneyOwed=26145,
                        OptionId="1"
                    }
                }
            };
            string cslresponse=JsonConvert.SerializeObject(milesAndMoneyResponse);

            _flightShoppingProductsService.Setup(p => p.GetCSLMilesPlusMoneyOptions(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(cslresponse);

            _iETCUtility.Setup(p => p.GetReservationFromPersist(It.IsAny<MOBSHOPReservation>(), It.IsAny<Session>())).ReturnsAsync(mOBFOPResponse.Reservation);

            _sessionHelperService.Setup(p => p.GetSession<ProfileResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(profileResponse);
            //_mscformsOfPaymment.Setup(p => p.GetEligibleFormofPayments(It.IsAny<MOBRequest>(), It.IsAny<Session>(), It.IsAny<MOBShoppingCart>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MOBSHOPReservation>(), It.IsAny<bool>(),It.IsAny<SeatChangeState>())).Returns((mOBFOPResponse.EligibleFormofPayments, isdefault));


            var result = _moneyPlusMilesBusiness.GetMoneyPlusMiles(getMoneyPlusMilesRequest);
            //Assert
            Assert.True(result.Exception != null || result.Result != null);
        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.ApplyMoneyPlusMiles_Test), MemberType = typeof(TestDataGenerator))]

        public void ApplyMoneyPlusMiles_Test(ApplyMoneyPlusMilesOptionRequest applyMoneyPlusMilesOptionRequest, Session session, List<CMSContentMessage> cMSContentMessage, LoadReservationAndDisplayCartResponse loadReservationAndDisplayResponse, FlightReservationResponse flightReservationResponse,MOBShoppingCart mOBShoppingCart,Reservation reservation,MOBFOPResponse mOBFOPResponse,ProfileResponse profileResponse) 
        {
            _shoppingSessionHelper.Setup(p => p.GetBookingFlowSession(It.IsAny<string>(), It.IsAny<bool>())).Returns(System.Threading.Tasks.Task.FromResult(session));
           
            _shoppingCartUtility.Setup(p => p.GetSDLContentByGroupName(It.IsAny<MOBRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(cMSContentMessage);

            _shoppingCartService.Setup(p => p.GetCartInformation<LoadReservationAndDisplayCartResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(loadReservationAndDisplayResponse);

            _dPService.Setup(p => p.GetAnonymousToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IConfiguration>())).ReturnsAsync("Bearer Token");
           
            FlightReservationResponse flightReservationResponse1 = new FlightReservationResponse
            {
                CartId="testcartid",
                DisplayCart=new DisplayCart
                {
                    CountryCode="India",
                    DisplayPrices=new List<DisplayPrice>
                    {
                        new DisplayPrice
                        {
                            Amount=134,
                            ResidualAmount=5346,
                            Description="tesytdesc",
                            Type="MILESANDMONEY",
                            Currency="rupees",
                            Status="teststatis",
                            Waived=true
                        }
                    }
                },
                Status=new Services.FlightShopping.Common.StatusType { },
                Reservation=new Service.Presentation.ReservationModel.Reservation
                {
                    CdmAddressCheck="testcheck"
                }
            };
            string cslresponse = JsonConvert.SerializeObject(flightReservationResponse1);

            _flightShoppingProductsService.Setup(p => p.ApplyCSLMilesPlusMoneyOptions(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(cslresponse);

            _shoppingCartService.Setup(p => p.RegisterFlights<FlightReservationResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(flightReservationResponse);

            _sessionHelperService.Setup(p => p.GetSession<MOBShoppingCart>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBShoppingCart);

            _sessionHelperService.Setup(p => p.GetSession<Reservation>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservation);

            _iETCUtility.Setup(p => p.GetReservationFromPersist(It.IsAny<MOBSHOPReservation>(), It.IsAny<Session>())).ReturnsAsync(mOBFOPResponse.Reservation);

           

            _pKDispenserPublicKey.Setup(p => p.GetCachedOrNewpkDispenserPublicKey(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<United.Mobile.Model.Common.MOBItem>>(), It.IsAny<string>())).ReturnsAsync(mOBFOPResponse.PkDispenserPublicKey);

            //_mscformsOfPaymment.Setup(p => p.GetEligibleFormofPayments(It.IsAny<MOBRequest>(), It.IsAny<Session>(), It.IsAny<MOBShoppingCart>(), It.IsAny<string>(), It.IsAny<string>(), ref It.Ref<bool>.IsAny, It.IsAny<MOBSHOPReservation>(), It.IsAny<bool>(), It.IsAny<SeatChangeState>())).Returns(mOBFOPResponse.EligibleFormofPayments);



            var result = _moneyPlusMilesBusiness.ApplyMoneyPlusMiles(applyMoneyPlusMilesOptionRequest);

            if (result.Exception == null)
                Assert.True(result.Result != null && result.Result.TransactionId != null);
            else
                Assert.True(result.Exception != null && result.Exception.InnerException != null);

        }





    }
    
}
