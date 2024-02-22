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
using United.Common.Helper;
using United.Common.Helper.MSCPayment.Interfaces;
using United.Definition;
using United.Definition.Shopping;
using United.Ebs.Logging.Enrichers;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.DynamoDB;
using United.Mobile.DataAccess.MSCPayment.Interfaces;
using United.Mobile.DataAccess.Product.Interfaces;
using United.Mobile.Model.Common;
using United.Mobile.Payment.Domain;
using United.Persist.Definition.FOP;
using United.Persist.Definition.SeatChange;
using United.Persist.Definition.Shopping;
using United.Service.Presentation.CommonModel;
using United.Service.Presentation.PaymentModel;
using United.Service.Presentation.PaymentResponseModel;
using United.Service.Presentation.RefundModel;
using United.Service.Presentation.SecurityResponseModel;
using United.Utility.Helper;
using United.Utility.Http;
using Xunit;
using ProfileResponse = United.Persist.Definition.Shopping.ProfileResponse;

namespace United.Mobile.Test.Payment.Api.Tests
{
    public class PaymentBusinessTest
    {
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        private readonly Mock<ICacheLog<PaymentBusiness>> _logger;
        private readonly Mock<IHeaders> _headers;
        private readonly IConfiguration _configuration;
        private readonly Mock<IMSCShoppingSessionHelper> _shoppingSessionHelper;
        private readonly Mock<ISessionHelperService> _sessionHelperService;
        private readonly Mock<ISessionOnCloudService> _sessionOnCloudService;
        private readonly IResilientClient _resilientClient;
        private readonly Mock<IApplicationEnricher> _applicationEnricher;
        private readonly Mock<IMSCShoppingSessionHelper> _mSCShoppingSessionHelper;
        private readonly Mock<ILogger> _logger1;
        private readonly Mock<IDynamoDBService> _dynamoDBService;
        private readonly Mock<ILogger> _logger2;
        private readonly Mock<IDPService> _dPService;
        private readonly Mock<ICacheLog<CachingService>> _logger11;
        private readonly Mock<ICachingService> _cachingService;
        private readonly ICachingService _cachingService1;
        private readonly Mock<IShoppingUtility> _shoppingUtility;
        private readonly Mock<IMSCPageProductInfoHelper> _mSCPageProductInfoHelper;
        private readonly Mock<ICMSContentService> _cMSContentService;
        private PaymentBusiness _paymentBusiness;
        private readonly Mock<IPKDispenserService> _pKDispenserService;
        private readonly Mock<IFlightShoppingProductsService> _flightShoppingProductsServices;
        private readonly Mock<ILogger> _logger3;
        private readonly Mock<IETCUtility> _eTCUtility;
        private readonly Mock<ILogger> _logger4;
        private readonly Mock<IShoppingCartUtility> _shoppingCartUtility;
        private readonly Mock<IFlightShoppingService> _flightShoppingService;
        private readonly Mock<ILogger> _logger5;
        private readonly Mock<IPaymentService> _paymentService;
        private readonly Mock<ILogger> _logger6;
        private readonly Mock<ICheckoutUtiliy> _checkoutUtility;
        private readonly Mock<IDataVaultService> _dataVaultService;
        private readonly Mock<ILogger> _logger7;
        private readonly Mock<IReferencedataService> _referencedataService;
        private readonly Mock<IMSCPkDispenserPublicKey> _mSCPkDispenserPublicKey;
        private readonly Mock<IMSCFormsOfPayment> _mSCFormsOfPayment;
        private readonly Mock<ILogger> _logger8;
        private readonly Mock<DocumentLibraryDynamoDB> _documentLibraryDynamoDB;
        private readonly Mock<ILogger> _logger9;
        private readonly Mock<ICustomerPreferencesService> _customerPreferencesService;
        private readonly Mock<ILogger> _logger10;
        private readonly Mock<IMSCBaggageInfo> _mscBaggageInfoProvider;
        private readonly Mock<IPassDetailService> _passDetailService;
        private readonly Mock<ILegalDocumentsForTitlesService> _legalDocumentsForTitlesService;
        private readonly Mock<IShoppingCartService> _shoppingCartService;
        private readonly HttpClient _httpClient;
        private readonly IAsyncPolicy _policyWrap;
        private readonly string _baseUrl;
        private readonly IDataPowerFactory _dataPowerFactory;
        private readonly Mock<IProvisionService> _provisionService;
        private readonly Mock<IFeatureSettings> _featureSettings;

        public PaymentBusinessTest()
        {

            _configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appSettings.test.json", optional: false, reloadOnChange: true)
           .Build();

            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _sessionHelperService = new Mock<ISessionHelperService>();
            _logger = new Mock<ICacheLog<PaymentBusiness>>();
            _logger1 = new Mock<ILogger>();
            _sessionOnCloudService = new Mock<ISessionOnCloudService>();
            _logger2 = new Mock<ILogger>();
            _logger3 = new Mock<ILogger>();
            _logger4 = new Mock<ILogger>();
            _logger5 = new Mock<ILogger>();
            _logger6 = new Mock<ILogger>();
            _logger7 = new Mock<ILogger>();
            _logger8 = new Mock<ILogger>();
            _logger9 = new Mock<ILogger>();
            _logger10 = new Mock<ILogger>();
            _logger11 = new Mock<ICacheLog<CachingService>>();
            _headers = new Mock<IHeaders>();
            _applicationEnricher = new Mock<IApplicationEnricher>();
            _mSCShoppingSessionHelper = new Mock<IMSCShoppingSessionHelper>();
            _dynamoDBService = new Mock<IDynamoDBService>();
            _dPService = new Mock<IDPService>();
            _cachingService = new Mock<ICachingService>();
            _shoppingUtility = new Mock<IShoppingUtility>();
            _mSCFormsOfPayment = new Mock<IMSCFormsOfPayment>();
            _paymentService = new Mock<IPaymentService>();
            _pKDispenserService = new Mock<IPKDispenserService>();
            _flightShoppingProductsServices = new Mock<IFlightShoppingProductsService>();
            _eTCUtility = new Mock<IETCUtility>();
            _flightShoppingService = new Mock<IFlightShoppingService>();
            _shoppingSessionHelper = new Mock<IMSCShoppingSessionHelper>();
            _legalDocumentsForTitlesService = new Mock<ILegalDocumentsForTitlesService>();
            _shoppingCartService = new Mock<IShoppingCartService>();
            _referencedataService = new Mock<IReferencedataService>();
            _checkoutUtility = new Mock<ICheckoutUtiliy>();
            _dataVaultService = new Mock<IDataVaultService>();
            _mSCPkDispenserPublicKey = new Mock<IMSCPkDispenserPublicKey>();
            _shoppingCartUtility = new Mock<IShoppingCartUtility>();
            _mSCPageProductInfoHelper = new Mock<IMSCPageProductInfoHelper>();
            _cMSContentService = new Mock<ICMSContentService>();
            _provisionService = new Mock<IProvisionService>();

            _resilientClient = new ResilientClient(_baseUrl);

            _cachingService1 = new CachingService(_resilientClient, _logger11.Object, _configuration);


            _paymentBusiness = new PaymentBusiness(_logger.Object, _configuration, _shoppingSessionHelper.Object, _sessionHelperService.Object,
                 _shoppingUtility.Object, _eTCUtility.Object, _shoppingCartUtility.Object, _flightShoppingService.Object, _dPService.Object,
                _paymentService.Object, _checkoutUtility.Object, _referencedataService.Object, _mSCPkDispenserPublicKey.Object,
                 _mSCFormsOfPayment.Object, _dataVaultService.Object, _dynamoDBService.Object, _legalDocumentsForTitlesService.Object, _cachingService.Object,
                 _shoppingCartService.Object, _headers.Object, _provisionService.Object, _cMSContentService.Object,_featureSettings.Object);

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
        [MemberData(nameof(TestDataGenerator.GetPaymentToken_Test), MemberType = typeof(TestDataGenerator))]
        public void GetPaymentToken_Test(MOBFOPAcquirePaymentTokenRequest mOBFOPAcquirePaymentTokenRequest, Session session)
        {
            _shoppingSessionHelper.Setup(p => p.GetValidateSession(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(session);

            _dPService.Setup(p => p.GetAnonymousToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IConfiguration>())).ReturnsAsync("Bearer Token");
            //Act
            var result = _paymentBusiness.GetPaymentToken(mOBFOPAcquirePaymentTokenRequest);
            //Assert
            Assert.True(result.Exception != null || result.Result != null);

        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.GetPaymentToken_Test1), MemberType = typeof(TestDataGenerator))]
        public void GetPaymentToken_Test1(MOBFOPAcquirePaymentTokenRequest mOBFOPAcquirePaymentTokenRequest, Session session)
        {

            _shoppingSessionHelper.Setup(p => p.GetValidateSession(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(session);

            _dPService.Setup(p => p.GetAnonymousToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IConfiguration>())).ReturnsAsync("Bearer Token");

            PayPal paypal = new PayPal()
            {
                LoyaltyProgramCarrierCode = "123",
                LoyaltyProgramMemberID = "L001",
                TrackingID = "T002",
                PayerID = "P004",
                TokenID = "B406B8FF",
                Payor = new Service.Presentation.PersonModel.Person
                {
                    CustomerID = "ABC12",
                    Contact = new Service.Presentation.PersonModel.Contact
                    {
                        Emails = new System.Collections.ObjectModel.Collection<EmailAddress>
                       {
                          new EmailAddress ()
                          {
                              Address = "ABC@gmail.com"
                          }
                       }

                    },
                    Status = new Status
                    {
                        Description = "Success"
                    }
                },
                BillingAgreementID = "ABCXYZ",

                BillingAddress = new Address
                {
                    Name = "US",
                    City = "NY",
                    PostalCode = "123456",
                    StateProvince = new StateProvince
                    {
                        ShortName = "USA",
                        StateProvinceCode = "1234",
                        Name = "US"
                    },
                    Characteristic = new System.Collections.ObjectModel.Collection<Characteristic>
                    {
                        new Characteristic ()
                        {
                            Code = "3012",
                            Value = "5647",
                            Description = "Success",
                             Status = new Status
                    {
                        Description = "Success"
                    }
                        }
                    },
                    Country = new Country
                    {
                        CountryCode = "1234",
                        PhoneCountryCode = "5432"
                    },
                    AddressLines = new System.Collections.ObjectModel.Collection<string>
                    {

                    }
                }
            };

            var response1 = JsonConvert.SerializeObject(paypal);

            _paymentService.Setup(p => p.GetEligibleFormOfPayments(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(response1);

            //Act
            var result = _paymentBusiness.GetPaymentToken(mOBFOPAcquirePaymentTokenRequest);
            //Assert
            Assert.True(result.Exception != null || result.Result != null);

        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.GetPaymentToken_Test1_1), MemberType = typeof(TestDataGenerator))]
        public void GetPaymentToken_Test2(MOBFOPAcquirePaymentTokenRequest mOBFOPAcquirePaymentTokenRequest, Session session)
        {

            _shoppingSessionHelper.Setup(p => p.GetValidateSession(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(session);

            _dPService.Setup(p => p.GetAnonymousToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IConfiguration>())).ReturnsAsync("Bearer Token");

            PayPal paypal = new PayPal()
            {
                LoyaltyProgramCarrierCode = "123",
                LoyaltyProgramMemberID = "L001",
                TrackingID = "T002",
                PayerID = "P004",
                TokenID = "B406B8FF",
                Payor = new Service.Presentation.PersonModel.Person
                {
                    CustomerID = "ABC12",
                    Contact = new Service.Presentation.PersonModel.Contact
                    {
                        Emails = new System.Collections.ObjectModel.Collection<EmailAddress>
                       {
                          new EmailAddress ()
                          {
                              Address = "ABC@gmail.com"
                          }
                       }

                    },
                    Status = new Status
                    {
                        Description = "Success"
                    }
                },
                BillingAgreementID = "ABCXYZ",
                BillingAddress = new Address
                {
                    Name = "US",
                    City = "NY",
                    PostalCode = "123456",
                    StateProvince = new StateProvince
                    {
                        ShortName = "USA",
                        StateProvinceCode = "1234",
                        Name = "US"
                    },
                    Characteristic = new System.Collections.ObjectModel.Collection<Characteristic>
                    {
                        new Characteristic ()
                        {
                            Code = "3012",
                            Value = "5647",
                            Description = "Success",
                             Status = new Status
                    {
                        Description = "Success"
                    }
                        }
                    },
                    Country = new Country
                    {
                        CountryCode = "1234",
                        PhoneCountryCode = "5432"
                    },
                    AddressLines = new System.Collections.ObjectModel.Collection<string>
                    {

                    }
                }
            };

            OpenWalletSessionResponse openWalletSessionResponse = new OpenWalletSessionResponse()
            {
                ServiceStatus = new Foundations.Practices.Framework.Model.Exception.ServiceStatus
                {
                    ServiceCode = "S0024",
                    ServiceName = "Status",
                    StatusType = "SUCCESS"
                },
                MerchantCheckoutID = "C0009",
                RequestToken = "https://sandbox.masterpass.com/switchui/index.html?allowedCardTypes={0}&amp",
                SessionID = "67945321097C4CF58FFC7DF9565CB276"
            };

            var response1 = JsonConvert.SerializeObject(openWalletSessionResponse);

            _paymentService.Setup(p => p.GetEligibleFormOfPayments(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(response1);

            //Act
            var result = _paymentBusiness.GetPaymentToken(mOBFOPAcquirePaymentTokenRequest);

            //Assert
            Assert.True(result.Exception != null || result.Result != null);

        }


        [Theory]
        [MemberData(nameof(TestDataGenerator.GetPaymentToken_Flow), MemberType = typeof(TestDataGenerator))]
        public void GetPaymentToken_Flow(MOBFOPAcquirePaymentTokenRequest mOBFOPAcquirePaymentTokenRequest, Session session)
        {
            _shoppingSessionHelper.Setup(p => p.GetValidateSession(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(session);

            _dPService.Setup(p => p.GetAnonymousToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IConfiguration>())).ReturnsAsync("Bearer Token");
            //Act
            var result = _paymentBusiness.GetPaymentToken(mOBFOPAcquirePaymentTokenRequest);
            //Assert
            Assert.True(result.Exception != null || result.Result != null);

        }



        [Theory]
        [MemberData(nameof(TestDataGenerator.PersistFormofPaymentDetails_Test), MemberType = typeof(TestDataGenerator))]
        public void PersistFormofPaymentDetails_Test(MOBPersistFormofPaymentRequest mOBPersistFormofPaymentRequest, Session session, Reservation reservation, ProfileResponse profileResponse, MOBShoppingCart mOBShoppingCart, MOBFOPAcquirePaymentTokenResponse mOBFOPAcquirePaymentTokenResponse, ProfileFOPCreditCardResponse profileFOPCreditCardResponse, MOBVormetricKeys mOBVormetricKeys, MOBCCAdStatement mOBCCAdStatement, SeatChangeState seatChangeState)
        {

            var _dataPowerFactory = new DataPowerFactory(_configuration, _sessionHelperService.Object);

            _shoppingSessionHelper.Setup(p => p.GetValidateSession(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(session);

            _sessionHelperService.Setup(p => p.GetSession<Reservation>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservation);

            _sessionHelperService.Setup(p => p.GetSession<ProfileResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(profileResponse);

            _sessionHelperService.Setup(p => p.GetSession<MOBShoppingCart>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBShoppingCart);

            _sessionHelperService.Setup(p => p.GetSession<MOBFOPAcquirePaymentTokenResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBFOPAcquirePaymentTokenResponse);

            _sessionHelperService.Setup(p => p.GetSession<ProfileFOPCreditCardResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(profileFOPCreditCardResponse);

            _checkoutUtility.Setup(p => p.AssignPersistentTokenToCC(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(mOBVormetricKeys);

            _sessionHelperService.Setup(p => p.GetSession<MOBCCAdStatement>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBCCAdStatement);

            _sessionHelperService.Setup(p => p.GetSession<SeatChangeState>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(seatChangeState);

            //_mSCPkDispenserPublicKey.Setup(p => p.GetCachedOrNewpkDispenserPublicKey(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(mOBPersistFormofPaymentRespose.PkDispenserPublicKey);

            PayPal paypal = new PayPal()
            {
                LoyaltyProgramCarrierCode = "123",
                LoyaltyProgramMemberID = "L001",
                TrackingID = "T002",
                PayerID = "P004",
                TokenID = "B406B8FF",
                Payor = new Service.Presentation.PersonModel.Person
                {
                    CustomerID = "ABC12",
                    Contact = new Service.Presentation.PersonModel.Contact
                    {
                        Emails = new System.Collections.ObjectModel.Collection<EmailAddress>
                       {
                          new EmailAddress ()
                          {
                              Address = "ABC@gmail.com"
                          }
                       }

                    },
                    Status = new Status
                    {
                        Description = "Success"
                    }
                },
                BillingAgreementID = "ABCXYZ",

                BillingAddress = new Address
                {
                    Name = "US",
                    City = "NY",
                    PostalCode = "123456",
                    StateProvince = new StateProvince
                    {
                        ShortName = "USA",
                        StateProvinceCode = "1234",
                        Name = "US"
                    },
                    Characteristic = new System.Collections.ObjectModel.Collection<Characteristic>
                    {
                        new Characteristic ()
                        {
                            Code = "3012",
                            Value = "5647",
                            Description = "Success",
                             Status = new Status
                    {
                        Description = "Success"
                    }
                        }
                    },
                    Country = new Country
                    {
                        CountryCode = "1234",
                        PhoneCountryCode = "5432"
                    },
                    AddressLines = new System.Collections.ObjectModel.Collection<string>
                    {

                    }
                }
            };

            var response1 = JsonConvert.SerializeObject(paypal);

            _paymentService.Setup(p => p.GetEligibleFormOfPayments(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(response1);

            _shoppingCartUtility.Setup(p => p.ReservationToShoppingCart_DataMigration(It.IsAny<MOBSHOPReservation>(), It.IsAny<MOBShoppingCart>(), It.IsAny<MOBRequest>(), It.IsAny<bool>(), It.IsAny<Session>())).ReturnsAsync(mOBShoppingCart);

            var paymentBusiness = new PaymentBusiness(_logger.Object, _configuration, _shoppingSessionHelper.Object, _sessionHelperService.Object,
                 _shoppingUtility.Object, _eTCUtility.Object, _shoppingCartUtility.Object, _flightShoppingService.Object, _dPService.Object,
                _paymentService.Object, _checkoutUtility.Object, _referencedataService.Object, _mSCPkDispenserPublicKey.Object,
                 _mSCFormsOfPayment.Object, _dataVaultService.Object, _dynamoDBService.Object, _legalDocumentsForTitlesService.Object, _cachingService1,
                 _shoppingCartService.Object, _headers.Object, _provisionService.Object, _cMSContentService.Object,_featureSettings.Object);

            //Act
            var result = paymentBusiness.PersistFormofPaymentDetails(mOBPersistFormofPaymentRequest);
            //Assert
            Assert.True(result.Exception != null || result.Result != null);

        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.PersistFormofPaymentDetails_Flow), MemberType = typeof(TestDataGenerator))]
        public void PersistFormofPaymentDetails_Flow(MOBPersistFormofPaymentRequest mOBPersistFormofPaymentRequest, Session session, Reservation reservation, ProfileResponse profileResponse, MOBShoppingCart mOBShoppingCart, MOBFOPAcquirePaymentTokenResponse mOBFOPAcquirePaymentTokenResponse, ProfileFOPCreditCardResponse profileFOPCreditCardResponse, MOBVormetricKeys mOBVormetricKeys, MOBCCAdStatement mOBCCAdStatement, SeatChangeState seatChangeState)
        {

            var _dataPowerFactory = new DataPowerFactory(_configuration, _sessionHelperService.Object);

            _shoppingSessionHelper.Setup(p => p.GetValidateSession(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(session);

            _sessionHelperService.Setup(p => p.GetSession<Reservation>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservation);

            _sessionHelperService.Setup(p => p.GetSession<ProfileResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(profileResponse);

            _sessionHelperService.Setup(p => p.GetSession<MOBShoppingCart>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBShoppingCart);

            _sessionHelperService.Setup(p => p.GetSession<MOBFOPAcquirePaymentTokenResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBFOPAcquirePaymentTokenResponse);

            _sessionHelperService.Setup(p => p.GetSession<ProfileFOPCreditCardResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(profileFOPCreditCardResponse);

            _checkoutUtility.Setup(p => p.AssignPersistentTokenToCC(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(mOBVormetricKeys);

            _sessionHelperService.Setup(p => p.GetSession<MOBCCAdStatement>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBCCAdStatement);

            _sessionHelperService.Setup(p => p.GetSession<SeatChangeState>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(seatChangeState);

            //_mSCPkDispenserPublicKey.Setup(p => p.GetCachedOrNewpkDispenserPublicKey(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(mOBPersistFormofPaymentRespose.PkDispenserPublicKey);

            PayPal paypal = new PayPal()
            {
                LoyaltyProgramCarrierCode = "123",
                LoyaltyProgramMemberID = "L001",
                TrackingID = "T002",
                PayerID = "P004",
                TokenID = "B406B8FF",
                Payor = new Service.Presentation.PersonModel.Person
                {
                    CustomerID = "ABC12",
                    Contact = new Service.Presentation.PersonModel.Contact
                    {
                        Emails = new System.Collections.ObjectModel.Collection<EmailAddress>
                       {
                          new EmailAddress ()
                          {
                              Address = "ABC@gmail.com"
                          }
                       }

                    },
                    Status = new Status
                    {
                        Description = "Success"
                    }
                },
                BillingAgreementID = "ABCXYZ",

                BillingAddress = new Address
                {
                    Name = "US",
                    City = "NY",
                    PostalCode = "123456",
                    StateProvince = new StateProvince
                    {
                        ShortName = "USA",
                        StateProvinceCode = "1234",
                        Name = "US"
                    },
                    Characteristic = new System.Collections.ObjectModel.Collection<Characteristic>
                    {
                        new Characteristic ()
                        {
                            Code = "3012",
                            Value = "5647",
                            Description = "Success",
                             Status = new Status
                    {
                        Description = "Success"
                    }
                        }
                    },
                    Country = new Country
                    {
                        CountryCode = "1234",
                        PhoneCountryCode = "5432"
                    },
                    AddressLines = new System.Collections.ObjectModel.Collection<string>
                    {

                    }
                }
            };

            var response1 = JsonConvert.SerializeObject(paypal);

            _paymentService.Setup(p => p.GetEligibleFormOfPayments(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(response1);

            _shoppingCartUtility.Setup(p => p.ReservationToShoppingCart_DataMigration(It.IsAny<MOBSHOPReservation>(), It.IsAny<MOBShoppingCart>(), It.IsAny<MOBRequest>(), It.IsAny<bool>(), It.IsAny<Session>())).ReturnsAsync(mOBShoppingCart);

            var paymentBusiness = new PaymentBusiness(_logger.Object, _configuration, _shoppingSessionHelper.Object, _sessionHelperService.Object,
                 _shoppingUtility.Object, _eTCUtility.Object, _shoppingCartUtility.Object, _flightShoppingService.Object, _dPService.Object,
                _paymentService.Object, _checkoutUtility.Object, _referencedataService.Object, _mSCPkDispenserPublicKey.Object,
                 _mSCFormsOfPayment.Object, _dataVaultService.Object, _dynamoDBService.Object, _legalDocumentsForTitlesService.Object, _cachingService1,
                 _shoppingCartService.Object, _headers.Object, _provisionService.Object, _cMSContentService.Object,_featureSettings.Object);

            //Act
            var result = paymentBusiness.PersistFormofPaymentDetails(mOBPersistFormofPaymentRequest);
            //Assert
            Assert.True(result.Exception != null || result.Result != null);

        }


        [Theory]
        [MemberData(nameof(TestDataGenerator.GetCreditCardToken_Test), MemberType = typeof(TestDataGenerator))]
        public void GetCreditCardToken_Test(MOBPersistFormofPaymentRequest mOBPersistFormofPaymentRequest, Session session, MOBShoppingCart mOBShoppingCart, Reservation reservation, MOBVormetricKeys mOBVormetricKeys, MOBCCAdStatement mOBCCAdStatement, MOBCSLContentMessagesResponse mOBCSLContentMessagesResponse, List<FormofPaymentOption> formofPaymentOptions)
        {

            var _dataPowerFactory = new DataPowerFactory(_configuration, _sessionHelperService.Object);

            _shoppingSessionHelper.Setup(p => p.GetValidateSession(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(session);

            //_sessionHelperService.Setup(p => p.GetSession<List<MSCFormofPaymentOption>>(It.IsAny<HttpContextValues>(), It.IsAny<string>(), It.IsAny<string>())).Returns(mscformofPaymentOption);

            _sessionHelperService.Setup(p => p.GetSession<MOBShoppingCart>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBShoppingCart);

            _sessionHelperService.Setup(p => p.GetSession<Reservation>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservation);

            _checkoutUtility.Setup(p => p.AssignPersistentTokenToCC(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(mOBVormetricKeys);

            _sessionHelperService.Setup(p => p.GetSession<MOBCCAdStatement>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBCCAdStatement);

            _cMSContentService.Setup(p => p.GetSDLContentByGroupName<MOBCSLContentMessagesResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(mOBCSLContentMessagesResponse);

            //_mSCPkDispenserPublicKey.Setup(p => p.GetCachedOrNewpkDispenserPublicKey(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(mOBPersistFormofPaymentRespose.PkDispenserPublicKey);

            _sessionHelperService.Setup(p => p.GetSession<List<FormofPaymentOption>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(formofPaymentOptions);

            _shoppingCartUtility.Setup(p => p.ReservationToShoppingCart_DataMigration(It.IsAny<MOBSHOPReservation>(), It.IsAny<MOBShoppingCart>(), It.IsAny<MOBRequest>(), It.IsAny<bool>(), It.IsAny<Session>())).ReturnsAsync(mOBShoppingCart);

            //_mSCFormsOfPayment.Setup(p => p.GetEligibleFormofPayments(It.IsAny<MOBRequest>(), It.IsAny<Session>(), It.IsAny<MOBShoppingCart>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MOBSHOPReservation>(), It.IsAny<bool>(), It.IsAny<SeatChangeState>())).Returns(Task.FromResult((formofPaymentOptions, true)));


            var paymentBusiness = new PaymentBusiness(_logger.Object, _configuration, _shoppingSessionHelper.Object, _sessionHelperService.Object,
                _shoppingUtility.Object, _eTCUtility.Object, _shoppingCartUtility.Object, _flightShoppingService.Object, _dPService.Object,
               _paymentService.Object, _checkoutUtility.Object, _referencedataService.Object, _mSCPkDispenserPublicKey.Object,
                _mSCFormsOfPayment.Object, _dataVaultService.Object, _dynamoDBService.Object, _legalDocumentsForTitlesService.Object, _cachingService1,
                _shoppingCartService.Object, _headers.Object, _provisionService.Object, _cMSContentService.Object,_featureSettings.Object);

            //Act
            var result = paymentBusiness.GetCreditCardToken(mOBPersistFormofPaymentRequest);
            //Assert
            Assert.True(result.Exception != null || result.Result != null);
        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.GetCreditCardToken_Flow), MemberType = typeof(TestDataGenerator))]
        public void GetCreditCardToken_Flow(MOBPersistFormofPaymentRequest mOBPersistFormofPaymentRequest, Session session, MOBShoppingCart mOBShoppingCart, Reservation reservation, MOBVormetricKeys mOBVormetricKeys, MOBCCAdStatement mOBCCAdStatement, MOBCSLContentMessagesResponse mOBCSLContentMessagesResponse, List<FormofPaymentOption> formofPaymentOptions)
        {

            var _dataPowerFactory = new DataPowerFactory(_configuration, _sessionHelperService.Object);

            _shoppingSessionHelper.Setup(p => p.GetValidateSession(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(session);

            //_sessionHelperService.Setup(p => p.GetSession<List<MSCFormofPaymentOption>>(It.IsAny<HttpContextValues>(), It.IsAny<string>(), It.IsAny<string>())).Returns(mscformofPaymentOption);

            _sessionHelperService.Setup(p => p.GetSession<MOBShoppingCart>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBShoppingCart);

            _sessionHelperService.Setup(p => p.GetSession<Reservation>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservation);

            _checkoutUtility.Setup(p => p.AssignPersistentTokenToCC(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(mOBVormetricKeys);

            _sessionHelperService.Setup(p => p.GetSession<MOBCCAdStatement>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBCCAdStatement);

            _cMSContentService.Setup(p => p.GetSDLContentByGroupName<MOBCSLContentMessagesResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(mOBCSLContentMessagesResponse);

            //_mSCPkDispenserPublicKey.Setup(p => p.GetCachedOrNewpkDispenserPublicKey(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(mOBPersistFormofPaymentRespose.PkDispenserPublicKey);

            _sessionHelperService.Setup(p => p.GetSession<List<FormofPaymentOption>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(formofPaymentOptions);

            _shoppingCartUtility.Setup(p => p.ReservationToShoppingCart_DataMigration(It.IsAny<MOBSHOPReservation>(), It.IsAny<MOBShoppingCart>(), It.IsAny<MOBRequest>(), It.IsAny<bool>(), It.IsAny<Session>())).ReturnsAsync(mOBShoppingCart);

            //_mSCFormsOfPayment.Setup(p => p.GetEligibleFormofPayments(It.IsAny<MOBRequest>(), It.IsAny<Session>(), It.IsAny<MOBShoppingCart>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MOBSHOPReservation>(), It.IsAny<bool>(), It.IsAny<SeatChangeState>())).Returns(Task.FromResult((formofPaymentOptions, true)));


            var paymentBusiness = new PaymentBusiness(_logger.Object, _configuration, _shoppingSessionHelper.Object, _sessionHelperService.Object,
                _shoppingUtility.Object, _eTCUtility.Object, _shoppingCartUtility.Object, _flightShoppingService.Object, _dPService.Object,
               _paymentService.Object, _checkoutUtility.Object, _referencedataService.Object, _mSCPkDispenserPublicKey.Object,
                _mSCFormsOfPayment.Object, _dataVaultService.Object, _dynamoDBService.Object, _legalDocumentsForTitlesService.Object, _cachingService1,
                _shoppingCartService.Object, _headers.Object, _provisionService.Object, _cMSContentService.Object,_featureSettings.Object);

            //Act
            var result = paymentBusiness.GetCreditCardToken(mOBPersistFormofPaymentRequest);
            //Assert
            Assert.True(result.Exception != null || result.Result != null);
        }


        [Theory]
        [MemberData(nameof(TestDataGenerator.GetCartInformation_Test), MemberType = typeof(TestDataGenerator))]
        public void GetCartInformation_Test(MOBSHOPMetaSelectTripRequest mOBSHOPMetaSelectTripRequest, MOBRegisterOfferResponse mOBRegisterOfferResponse, CSLShopRequest cSLShopRequest, PKDispenserKey pKDispenserKey, Session session, MOBShoppingCart mOBShoppingCart)
        {
            //_shoppingSessionHelper.Setup(p => p.CreateShoppingSession(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(response);

            _dPService.Setup(p => p.GetAnonymousToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IConfiguration>())).ReturnsAsync("Bearer Token");

            _mSCPkDispenserPublicKey.Setup(p => p.GetCachedOrNewpkDispenserPublicKey(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<United.Mobile.Model.Common.MOBItem>>(), It.IsAny<string>())).ReturnsAsync(mOBRegisterOfferResponse.PkDispenserPublicKey);




            //Act
            var result = _paymentBusiness.GetCartInformation(mOBSHOPMetaSelectTripRequest);
            //Assert
            Assert.True(result.Exception != null || result.Result != null);
        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.TravelBankCredit_Test), MemberType = typeof(TestDataGenerator))]
        public void TravelBankCredit_Test(MOBFOPTravelerBankRequest mOBFOPTravelerBankRequest, Session session, SeatChangeState seatChangeState, Reservation reservation, ReservationDetail reservationDetail, ProfileResponse profileResponse, MOBFOPResponse mOBFOPResponse, MOBShoppingCart mOBShoppingCart)
        {

            var _dataPowerFactory = new DataPowerFactory(_configuration, _sessionHelperService.Object);

            _shoppingSessionHelper.Setup(p => p.GetBookingFlowSession(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(session);

            // _mSCFormsOfPayment.Setup(p => p.GetEligibleFormofPayments(It.IsAny<MOBRequest>(), It.IsAny<Session>(), It.IsAny<MOBShoppingCart>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MOBSHOPReservation>(), It.IsAny<bool>(), It.IsAny<SeatChangeState>())).Returns(Task.FromResult((formofPaymentOptions, true)));

            _sessionHelperService.Setup(p => p.GetSession<SeatChangeState>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(seatChangeState);

            _sessionHelperService.Setup(p => p.GetSession<Reservation>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservation);

            _sessionHelperService.Setup(p => p.GetSession<ReservationDetail>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservationDetail);

            _sessionHelperService.Setup(p => p.GetSession<ProfileResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(profileResponse);

            _eTCUtility.Setup(p => p.MakeReservationFromPersistReservation(It.IsAny<MOBSHOPReservation>(), It.IsAny<Reservation>(), It.IsAny<Session>())).Returns(mOBFOPResponse.Reservation);

            _sessionHelperService.Setup(p => p.GetSession<MOBShoppingCart>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBShoppingCart);

            _eTCUtility.Setup(p => p.LoadPersistedProfile(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(mOBFOPResponse.Profiles);

            _mSCFormsOfPayment.Setup(p => p.GetTravelBankBalance(It.IsAny<string>())).ReturnsAsync(33.001222221);

            List<CMSContentMessage> cMSContentMessages1 = new List<CMSContentMessage>
            {
                new CMSContentMessage()
                {
                    Title = "RTI.TravelBank.Apply",
                    ContentFull = "To determine first and second checked baggage service charges for your itinerary, use the baggage calculator below. Please note that as required by U.S. Department of Transportation regulations, baggage service charges for your entire itinerary are determined by the marketing carrier for the first segment of your itinerary. Your originating marketing carrier is the airline whose flight number is assigned to the first segment of your itinerary. If this carrier is not United or United Express, different charges may apply.\n<p>Baggage service charges are based on the date of ticketing and on the ticketed cabin as well as the traveler's status or membership on the date of travel. Eligibility for the checked baggage service charge waiver for travelers on award tickets issued or re-issued on or after April 15, 2015, is based on each traveler’s Premier status at check-in, rather than the status of the member whose miles were used to purchase the award ticket.</p><p>For all United Economy® tickets issued or re-issued before February 1, 2015, Premier® Gold members are eligible for three complimentary checked bags at 70 pounds (32 kg) each. For United Economy tickets issued or re-issued on or after February 1, 2015, Premier Gold members are eligible for two complimentary checked bags at 70 pounds (32 kg) each for travel between the U.S., Canada, Puerto Rico and the U.S. Virgin Islands. For travel to and from select international markets, Premier Gold members are eligible for three complimentary checked bags at 70 pounds (32 kg) each. To determine specific checked baggage service charges, please use the baggage calculator below.</p>",
                    ContentShort = "determine first and second checked baggage",
                    Headline = "Checked baggage",
                    LocationCode = "Bags:Checked",
                }
            };

            var response1 = JsonConvert.SerializeObject(cMSContentMessages1);


            _shoppingCartUtility.Setup(p => p.GetSDLContentByGroupName(It.IsAny<MOBRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(cMSContentMessages1);

            var paymentBusiness = new PaymentBusiness(_logger.Object, _configuration, _shoppingSessionHelper.Object, _sessionHelperService.Object,
                 _shoppingUtility.Object, _eTCUtility.Object, _shoppingCartUtility.Object, _flightShoppingService.Object, _dPService.Object,
                _paymentService.Object, _checkoutUtility.Object, _referencedataService.Object, _mSCPkDispenserPublicKey.Object,
                 _mSCFormsOfPayment.Object, _dataVaultService.Object, _dynamoDBService.Object, _legalDocumentsForTitlesService.Object, _cachingService1,
                 _shoppingCartService.Object, _headers.Object, _provisionService.Object, _cMSContentService.Object,_featureSettings.Object);

            //Act
            var result = paymentBusiness.TravelBankCredit(mOBFOPTravelerBankRequest);
            //Assert
            Assert.True(result.Exception != null || result.Result != null);
        }
    }
}
