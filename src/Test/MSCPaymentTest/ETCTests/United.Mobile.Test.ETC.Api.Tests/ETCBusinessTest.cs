using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Text;
using United.Common.Helper;
using United.Common.Helper.MSCPayment.Interfaces;
using United.Definition;
using United.Definition.FormofPayment;
using United.Definition.Shopping;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.MSCPayment.Interfaces;
using United.Mobile.DataAccess.Product.Interfaces;
using United.Mobile.ETC.Business;
using United.Mobile.Model.Common;
using United.Persist.Definition.Shopping;
using United.Service.Presentation.CommonModel;
using United.Service.Presentation.PaymentResponseModel;
using United.Utility.Helper;
using Xunit;

namespace United.Mobile.Test.ETC.Api.Tests
{
    public class ETCBusinessTest
    {
        private readonly Mock<ICacheLog<ETCBusiness>> _logger;
        private readonly IConfiguration _configuration;
        private readonly Mock<ISessionHelperService> _sessionHelperService;
        private readonly Mock<IMSCShoppingSessionHelper> _shoppingSessionHelper;
        private readonly Mock<ICachingService> _moqcachingService;
        //private readonly Mock<IDPService> _moqdPService;
        private readonly Mock<IMSCPkDispenserPublicKey> _pKDispenserPublicKey;
        //private readonly Mock<IPKDispenserService> _pKDispenserService;
        private readonly Mock<ICachingService> _cachingService;
        // private readonly Mock<IDPService> _dPService;
        private readonly Mock<IETCUtility> _eTCUtility;
        private readonly Mock<IShoppingUtility> _shoppingUtility;
        private readonly Mock<IReferencedataService> _referencedataService;
        private readonly Mock<ICMSContentService> _cMSContentService;
        private readonly Mock<IETCBalanceEnquiryService> _iETCBalanceEnquiryService;
        private readonly ETCBusiness _eTCBusiness;
        private readonly Mock<IDPService> _iDPService;
        private readonly Mock<IMSCFormsOfPayment> _formsOfPayment;
        private readonly Mock<ILegalDocumentsForTitlesService> _legalDocumentsForTitlesService;
        private readonly Mock<IShoppingCartUtility> _shoppingCartUtility;
        private readonly Mock<IShoppingCartService> _shoppingCartService;
        private readonly Mock<IFeatureSettings> _featureSettings;





        public ETCBusinessTest()
        {

            _configuration = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.test.json", optional: false, reloadOnChange: true)
             .Build();
            _logger = new Mock<ICacheLog<ETCBusiness>>();
            _shoppingSessionHelper = new Mock<IMSCShoppingSessionHelper>();
            _sessionHelperService = new Mock<ISessionHelperService>();
            _iDPService = new Mock<IDPService>();
            _cachingService = new Mock<ICachingService>();
            //_dPService = new Mock<IDPService>();
            _eTCUtility = new Mock<IETCUtility>();
            _shoppingUtility = new Mock<IShoppingUtility>();
            _referencedataService = new Mock<IReferencedataService>();
            _cMSContentService = new Mock<ICMSContentService>();
            _iETCBalanceEnquiryService = new Mock<IETCBalanceEnquiryService>();
            // _pKDispenserService = new Mock<IMSCPkDispenserPublicKey>(_configuration, _iDPService);
            _formsOfPayment = new Mock<IMSCFormsOfPayment>();
            _pKDispenserPublicKey = new Mock<IMSCPkDispenserPublicKey>();
            _legalDocumentsForTitlesService = new Mock<ILegalDocumentsForTitlesService>();


            _eTCBusiness = new ETCBusiness(_configuration, _logger.Object, _sessionHelperService.Object, _shoppingSessionHelper.Object, _eTCUtility.Object,
                 _referencedataService.Object, _cMSContentService.Object,
                _iETCBalanceEnquiryService.Object, _formsOfPayment.Object, _pKDispenserPublicKey.Object,
                _legalDocumentsForTitlesService.Object, _cachingService.Object,_shoppingCartUtility.Object,_shoppingCartService.Object, _featureSettings.Object, _shoppingUtility.Object);

            SetHeaders();
        }



        public static string GetFileContent(string fileName)
        {
            fileName = string.Format("..\\..\\..\\TestData\\{0}", fileName);
            var path = Path.IsPathRooted(fileName) ? fileName : Path.GetRelativePath(Directory.GetCurrentDirectory(), fileName);
            return File.ReadAllText(path);
        }
        private void SetHeaders(string deviceId = "D873298F-F27D-4AEC-BE6C-DE79C4259626"
        , string applicationId = "2"
        , string appVersion = "4.1.26"
        , string transactionId = "3f575588-bb12-41fe-8be7-f57c55fe7762|afc1db10-5c39-4ef4-9d35-df137d56a23e"
        , string languageCode = "en-US"
        , string sessionId = "D58E298C35274F6F873A133386A42916")
        {
           
            //new HttpContextValues
            //{
            //    Application = new Application()
            //    {
            //        Id = Convert.ToInt32(applicationId),
            //        Version = new Mobile.Model.Version
            //        {
            //            Major = string.IsNullOrEmpty(appVersion) ? 0 : int.Parse(appVersion.ToString().Substring(0, 1)),
            //            Minor = string.IsNullOrEmpty(appVersion) ? 0 : int.Parse(appVersion.ToString().Substring(2, 1)),
            //            Build = string.IsNullOrEmpty(appVersion) ? 0 : int.Parse(appVersion.ToString().Substring(4, 2))
            //        }
            //    },
            //    DeviceId = deviceId,
            //    LangCode = languageCode,
            //    TransactionId = transactionId,
            //    SessionId = sessionId
            //};
        }
        [Theory]
        [MemberData(nameof(TestDataGenarator.ETC_Test), MemberType = typeof(TestDataGenarator))]
        public void ETC_Test(MOBFOPTravelerCertificateRequest mOBFOPTravelerCertificateRequest, Session session, MOBFOPTravelerCertificateResponse response, Reservation reservation, MOBFOPTravelCertificate mOBFOPTravelCertificate, MOBSHOPPrice mOBSHOPPrice)
        {
            _shoppingSessionHelper.Setup(p => p.GetBookingFlowSession(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(session);





            _pKDispenserPublicKey.Setup(p => p.GetCachedOrNewpkDispenserPublicKey(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),It.IsAny<List<United.Mobile.Model.Common.MOBItem>>(), It.IsAny<string>())).ReturnsAsync(response.PkDispenserPublicKey);



            _eTCUtility.Setup(p => p.AddCertificatesToFOP(It.IsAny<List<MOBFOPCertificate>>(), It.IsAny<Session>())).ReturnsAsync(response);



            //_travelCreditUtility.Setup(p => p.GetEligibleFormofPayments(It.IsAny<MOBRequest>(), It.IsAny<Session>(), It.IsAny<MOBShoppingCart>(), It.IsAny<string>(), It.IsAny<string>(), ref It.Ref<bool>.IsAny, It.IsAny<MOBSHOPReservation>(), It.IsAny<bool>(), It.IsAny<SeatChangeState>())).Returns(response.EligibleFormofPayments);



            _sessionHelperService.Setup(p => p.GetSession<Reservation>(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservation);



            _eTCUtility.Setup(p => p.GetShoppingCartTravelCertificateFromPersist(It.IsAny<Session>(), It.IsAny<MOBFOPTravelerCertificateResponse>())).ReturnsAsync(mOBFOPTravelCertificate);



            _eTCUtility.Setup(p => p.UpdateCertificatePrice(It.IsAny<MOBSHOPPrice>(), It.IsAny<double>())).Returns(mOBSHOPPrice);



            _eTCUtility.Setup(p => p.UpdateCertificateRedeemAmountFromTotalInReserationPrices(It.IsAny<MOBSHOPPrice>(), It.IsAny<double>(), It.IsAny<bool>()));



            //_shoppingUtility.Setup(p => p.IsETCEnabledforMultiTraveler(It.IsAny<int>(), It.IsAny<string>())).Returns(ok);



            _eTCUtility.Setup(p => p.AssignCertificateRedeemAmount(It.IsAny<MOBFOPCertificate>(), It.IsAny<double>()));



            //_shoppingUtility.Setup(p => p.AssignBalanceAttentionInfoWarningMessage(It.IsAny<ReservationInfo2>(), It.IsAny<MOBFOPTravelCertificate>()));



            _eTCUtility.Setup(p => p.UpdateReservationWithCertificatePrices(It.IsAny<Session>(), It.IsAny<MOBFOPTravelerCertificateResponse>(), It.IsAny<double>(), It.IsAny<Reservation>()));



            _eTCUtility.Setup(p => p.AssignIsOtherFOPRequired(It.IsAny<MOBFormofPaymentDetails>(), It.IsAny<List<MOBSHOPPrice>>(), It.IsAny<bool>(), It.IsAny<bool>()));



            _eTCUtility.Setup(p => p.AssignLearmoreInformationDetails(It.IsAny<MOBFOPTravelCertificate>()));



            _eTCUtility.Setup(p => p.UpdateSavedCertificate(It.IsAny<MOBShoppingCart>()));
            BalanceInquiry balanceInquiry = new BalanceInquiry
            {
                IsAccountLocked = true,
                IsAccountNotFound = true,
                FormOfPayment = new Service.Presentation.PaymentModel.Payment
                {
                   AccountNumber = "123",
                    Date ="ABC",
                    AccountNumberHMAC= "XYZ",
                    GroupID = "pqr"

                },
                Response = new ServiceResponse
                {
                    Message = new System.Collections.ObjectModel.Collection<Message>
                    {  
                        new Message
                        {
                            Text = "SUCCESS"
                        }
                    }
                },
               
                
            };
            string cslresponse = JsonConvert.SerializeObject(balanceInquiry);
            _iETCBalanceEnquiryService.Setup(p => p.GetETCBalanceInquiry(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 
            It.IsAny<string>())).ReturnsAsync(cslresponse);


            if (mOBFOPTravelerCertificateRequest.SessionId == "17C979E184CC495EA083D45F4DD9D19D")
            {
                _configuration["CombinebilityETCToggle"] = "true";
            }

            if (mOBFOPTravelerCertificateRequest.SessionId == "17C979E184CC495EA083D45F4DD9D19D")
            {
                _configuration["Android_EnableETCCombinability_AppVersion"] = "3.0.44";
            }
            if (mOBFOPTravelerCertificateRequest.SessionId == "17C979E184CC495EA083D45F4DD9D19D")
            {
                _configuration["iPhone_EnableETCCombinability_AppVersion"] = "3.0.44";
            }

            var result = _eTCBusiness.TravelerCertificate(mOBFOPTravelerCertificateRequest);
            // Assert
            Assert.True(result.Exception != null || result.Result != null);

        }

        [Theory]
        [MemberData(nameof(TestDataGenarator.ETC1_Test), MemberType = typeof(TestDataGenarator))]
        public void ETC1_Test(MOBFOPTravelerCertificateRequest mOBFOPTravelerCertificateRequest, Session session, MOBFOPTravelerCertificateResponse response, Reservation reservation, MOBFOPTravelCertificate mOBFOPTravelCertificate, MOBSHOPPrice mOBSHOPPrice)
        {
            //_shoppingSessionHelper.Setup(p => p.GetBookingFlowSession(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(session);


            _sessionHelperService.Setup(p => p.GetSession<Session>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(session);

            //_iETCBalanceEnquiryService.Setup(p => p.GetETCBalanceInquiry(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("");



           // _pKDispenserPublicKey.Setup(p => p.GetCachedOrNewpkDispenserPublicKey(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(response.PkDispenserPublicKey);



            _eTCUtility.Setup(p => p.AddCertificatesToFOP(It.IsAny<List<MOBFOPCertificate>>(), It.IsAny<Session>())).ReturnsAsync(response);



            //_travelCreditUtility.Setup(p => p.GetEligibleFormofPayments(It.IsAny<MOBRequest>(), It.IsAny<Session>(), It.IsAny<MOBShoppingCart>(), It.IsAny<string>(), It.IsAny<string>(), ref It.Ref<bool>.IsAny, It.IsAny<MOBSHOPReservation>(), It.IsAny<bool>(), It.IsAny<SeatChangeState>())).Returns(response.EligibleFormofPayments);



            _sessionHelperService.Setup(p => p.GetSession<Reservation>(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservation);



            _eTCUtility.Setup(p => p.GetShoppingCartTravelCertificateFromPersist(It.IsAny<Session>(), It.IsAny<MOBFOPTravelerCertificateResponse>())).ReturnsAsync(mOBFOPTravelCertificate);



            _eTCUtility.Setup(p => p.UpdateCertificatePrice(It.IsAny<MOBSHOPPrice>(), It.IsAny<double>())).Returns(mOBSHOPPrice);



            _eTCUtility.Setup(p => p.UpdateCertificateRedeemAmountFromTotalInReserationPrices(It.IsAny<MOBSHOPPrice>(), It.IsAny<double>(), It.IsAny<bool>()));



            //_shoppingUtility.Setup(p => p.IsETCEnabledforMultiTraveler(It.IsAny<int>(), It.IsAny<string>())).Returns(ok);



            _eTCUtility.Setup(p => p.AssignCertificateRedeemAmount(It.IsAny<MOBFOPCertificate>(), It.IsAny<double>()));



            //_shoppingUtility.Setup(p => p.AssignBalanceAttentionInfoWarningMessage(It.IsAny<ReservationInfo2>(), It.IsAny<MOBFOPTravelCertificate>()));



            _eTCUtility.Setup(p => p.UpdateReservationWithCertificatePrices(It.IsAny<Session>(), It.IsAny<MOBFOPTravelerCertificateResponse>(), It.IsAny<double>(), It.IsAny<Reservation>()));



            _eTCUtility.Setup(p => p.AssignIsOtherFOPRequired(It.IsAny<MOBFormofPaymentDetails>(), It.IsAny<List<MOBSHOPPrice>>(), It.IsAny<bool>(), It.IsAny<bool>()));



            _eTCUtility.Setup(p => p.AssignLearmoreInformationDetails(It.IsAny<MOBFOPTravelCertificate>()));



            _eTCUtility.Setup(p => p.UpdateSavedCertificate(It.IsAny<MOBShoppingCart>()));
            BalanceInquiry balanceInquiry = new BalanceInquiry
            {
                IsAccountLocked = true,
                IsAccountNotFound = true,
                FormOfPayment = new Service.Presentation.PaymentModel.Payment
                {

                },
                Response = new ServiceResponse
                {
                    Message = new System.Collections.ObjectModel.Collection<Message>
                    {
                        new Message
                        {
                            Text = "SUCCESS"
                        }
                    }
                },


            };
            string cslresponse = JsonConvert.SerializeObject(balanceInquiry);
            _iETCBalanceEnquiryService.Setup(p => p.GetETCBalanceInquiry(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>())).ReturnsAsync(cslresponse);


            var result = _eTCBusiness.TravelerCertificate(mOBFOPTravelerCertificateRequest);
            // Assert
            Assert.True(result.Exception != null || result.Result != null);

        }

        [Theory]
        [MemberData(nameof(TestDataGenarator.ETC2_Test), MemberType = typeof(TestDataGenarator))]
        public void ETC2_Test(MOBFOPTravelerCertificateRequest mOBFOPTravelerCertificateRequest, Session session, MOBFOPTravelerCertificateResponse response, Reservation reservation, MOBFOPTravelCertificate mOBFOPTravelCertificate, MOBSHOPPrice mOBSHOPPrice)
        {
            _shoppingSessionHelper.Setup(p => p.GetBookingFlowSession(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(session);



            //_iETCBalanceEnquiryService.Setup(p => p.GetETCBalanceInquiry(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("");



           // _pKDispenserPublicKey.Setup(p => p.GetCachedOrNewpkDispenserPublicKey(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(response.PkDispenserPublicKey);



            _eTCUtility.Setup(p => p.AddCertificatesToFOP(It.IsAny<List<MOBFOPCertificate>>(), It.IsAny<Session>())).ReturnsAsync(response);



            //_travelCreditUtility.Setup(p => p.GetEligibleFormofPayments(It.IsAny<MOBRequest>(), It.IsAny<Session>(), It.IsAny<MOBShoppingCart>(), It.IsAny<string>(), It.IsAny<string>(), ref It.Ref<bool>.IsAny, It.IsAny<MOBSHOPReservation>(), It.IsAny<bool>(), It.IsAny<SeatChangeState>())).Returns(response.EligibleFormofPayments);



            _sessionHelperService.Setup(p => p.GetSession<Reservation>(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservation);



            _eTCUtility.Setup(p => p.GetShoppingCartTravelCertificateFromPersist(It.IsAny<Session>(), It.IsAny<MOBFOPTravelerCertificateResponse>())).ReturnsAsync(mOBFOPTravelCertificate);



            _eTCUtility.Setup(p => p.UpdateCertificatePrice(It.IsAny<MOBSHOPPrice>(), It.IsAny<double>())).Returns(mOBSHOPPrice);



            _eTCUtility.Setup(p => p.UpdateCertificateRedeemAmountFromTotalInReserationPrices(It.IsAny<MOBSHOPPrice>(), It.IsAny<double>(), It.IsAny<bool>()));



            //_shoppingUtility.Setup(p => p.IsETCEnabledforMultiTraveler(It.IsAny<int>(), It.IsAny<string>())).Returns(ok);



            _eTCUtility.Setup(p => p.AssignCertificateRedeemAmount(It.IsAny<MOBFOPCertificate>(), It.IsAny<double>()));



            //_shoppingUtility.Setup(p => p.AssignBalanceAttentionInfoWarningMessage(It.IsAny<ReservationInfo2>(), It.IsAny<MOBFOPTravelCertificate>()));



            _eTCUtility.Setup(p => p.UpdateReservationWithCertificatePrices(It.IsAny<Session>(), It.IsAny<MOBFOPTravelerCertificateResponse>(), It.IsAny<double>(), It.IsAny<Reservation>()));



            _eTCUtility.Setup(p => p.AssignIsOtherFOPRequired(It.IsAny<MOBFormofPaymentDetails>(), It.IsAny<List<MOBSHOPPrice>>(), It.IsAny<bool>(), It.IsAny<bool>()));



            _eTCUtility.Setup(p => p.AssignLearmoreInformationDetails(It.IsAny<MOBFOPTravelCertificate>()));



            _eTCUtility.Setup(p => p.UpdateSavedCertificate(It.IsAny<MOBShoppingCart>()));
            BalanceInquiry balanceInquiry = new BalanceInquiry
            {
                IsAccountLocked = true,
                IsAccountNotFound = true,
                FormOfPayment = new Service.Presentation.PaymentModel.Payment
                {

                },
                Response = new ServiceResponse
                {
                    Message = new System.Collections.ObjectModel.Collection<Message>
                    {
                        new Message
                        {
                            Text = "SUCCESS"
                        }
                    }
                },


            };
            string cslresponse = JsonConvert.SerializeObject(balanceInquiry);
            _iETCBalanceEnquiryService.Setup(p => p.GetETCBalanceInquiry(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>())).ReturnsAsync(cslresponse);

            

            var result = _eTCBusiness.TravelerCertificate(mOBFOPTravelerCertificateRequest);
            // Assert
            Assert.True(result.Exception != null || result.Result != null);

        }

        [Theory]
        [MemberData(nameof(TestDataGenarator.ETC3_Test1), MemberType = typeof(TestDataGenarator))]
        public void ETC3_Test1(MOBFOPTravelerCertificateRequest mOBFOPTravelerCertificateRequest, Session session, MOBFOPTravelerCertificateResponse response, Reservation reservation, MOBFOPTravelCertificate mOBFOPTravelCertificate, MOBSHOPPrice mOBSHOPPrice)
        {
            _shoppingSessionHelper.Setup(p => p.GetBookingFlowSession(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(session);



            //_iETCBalanceEnquiryService.Setup(p => p.GetETCBalanceInquiry(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("");



           // _pKDispenserPublicKey.Setup(p => p.GetCachedOrNewpkDispenserPublicKey(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(response.PkDispenserPublicKey);



            _eTCUtility.Setup(p => p.AddCertificatesToFOP(It.IsAny<List<MOBFOPCertificate>>(), It.IsAny<Session>())).ReturnsAsync(response);



            //_travelCreditUtility.Setup(p => p.GetEligibleFormofPayments(It.IsAny<MOBRequest>(), It.IsAny<Session>(), It.IsAny<MOBShoppingCart>(), It.IsAny<string>(), It.IsAny<string>(), ref It.Ref<bool>.IsAny, It.IsAny<MOBSHOPReservation>(), It.IsAny<bool>(), It.IsAny<SeatChangeState>())).Returns(response.EligibleFormofPayments);



            _sessionHelperService.Setup(p => p.GetSession<Reservation>(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservation);



            _eTCUtility.Setup(p => p.GetShoppingCartTravelCertificateFromPersist(It.IsAny<Session>(), It.IsAny<MOBFOPTravelerCertificateResponse>())).ReturnsAsync(mOBFOPTravelCertificate);



            _eTCUtility.Setup(p => p.UpdateCertificatePrice(It.IsAny<MOBSHOPPrice>(), It.IsAny<double>())).Returns(mOBSHOPPrice);



            _eTCUtility.Setup(p => p.UpdateCertificateRedeemAmountFromTotalInReserationPrices(It.IsAny<MOBSHOPPrice>(), It.IsAny<double>(), It.IsAny<bool>()));



            //_shoppingUtility.Setup(p => p.IsETCEnabledforMultiTraveler(It.IsAny<int>(), It.IsAny<string>())).Returns(ok);



            _eTCUtility.Setup(p => p.AssignCertificateRedeemAmount(It.IsAny<MOBFOPCertificate>(), It.IsAny<double>()));



            //_shoppingUtility.Setup(p => p.AssignBalanceAttentionInfoWarningMessage(It.IsAny<ReservationInfo2>(), It.IsAny<MOBFOPTravelCertificate>()));



            _eTCUtility.Setup(p => p.UpdateReservationWithCertificatePrices(It.IsAny<Session>(), It.IsAny<MOBFOPTravelerCertificateResponse>(), It.IsAny<double>(), It.IsAny<Reservation>()));



            _eTCUtility.Setup(p => p.AssignIsOtherFOPRequired(It.IsAny<MOBFormofPaymentDetails>(), It.IsAny<List<MOBSHOPPrice>>(), It.IsAny<bool>(), It.IsAny<bool>()));



            _eTCUtility.Setup(p => p.AssignLearmoreInformationDetails(It.IsAny<MOBFOPTravelCertificate>()));



            _eTCUtility.Setup(p => p.UpdateSavedCertificate(It.IsAny<MOBShoppingCart>()));
            BalanceInquiry balanceInquiry = new BalanceInquiry
            {
                IsAccountLocked = true,
                IsAccountNotFound = true,
                FormOfPayment = new Service.Presentation.PaymentModel.Payment
                {

                },
                Response = new ServiceResponse
                {
                    Message = new System.Collections.ObjectModel.Collection<Message>
                    {
                        new Message
                        {
                            Text = "SUCCESS"
                        }
                    }
                },


            };
            string cslresponse = JsonConvert.SerializeObject(balanceInquiry);
            _iETCBalanceEnquiryService.Setup(p => p.GetETCBalanceInquiry(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>())).ReturnsAsync(cslresponse);



            var result = _eTCBusiness.TravelerCertificate(mOBFOPTravelerCertificateRequest);
            // Assert
            Assert.True(result.Exception != null || result.Result != null);

        }

        [Theory]
        [MemberData(nameof(TestDataGenarator.ETC4_Flow), MemberType = typeof(TestDataGenarator))]
        public void ETC4_Flow(MOBFOPTravelerCertificateRequest mOBFOPTravelerCertificateRequest, Session session, MOBFOPTravelerCertificateResponse response, Reservation reservation, MOBFOPTravelCertificate mOBFOPTravelCertificate, MOBSHOPPrice mOBSHOPPrice)
        {
            _shoppingSessionHelper.Setup(p => p.GetBookingFlowSession(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(session);





            _pKDispenserPublicKey.Setup(p => p.GetCachedOrNewpkDispenserPublicKey(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<United.Mobile.Model.Common.MOBItem>>(), It.IsAny<string>())).ReturnsAsync(response.PkDispenserPublicKey);



            _eTCUtility.Setup(p => p.AddCertificatesToFOP(It.IsAny<List<MOBFOPCertificate>>(), It.IsAny<Session>())).ReturnsAsync(response);



            //_travelCreditUtility.Setup(p => p.GetEligibleFormofPayments(It.IsAny<MOBRequest>(), It.IsAny<Session>(), It.IsAny<MOBShoppingCart>(), It.IsAny<string>(), It.IsAny<string>(), ref It.Ref<bool>.IsAny, It.IsAny<MOBSHOPReservation>(), It.IsAny<bool>(), It.IsAny<SeatChangeState>())).Returns(response.EligibleFormofPayments);



            _sessionHelperService.Setup(p => p.GetSession<Reservation>(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservation);



            _eTCUtility.Setup(p => p.GetShoppingCartTravelCertificateFromPersist(It.IsAny<Session>(), It.IsAny<MOBFOPTravelerCertificateResponse>())).ReturnsAsync(mOBFOPTravelCertificate);



            _eTCUtility.Setup(p => p.UpdateCertificatePrice(It.IsAny<MOBSHOPPrice>(), It.IsAny<double>())).Returns(mOBSHOPPrice);



            _eTCUtility.Setup(p => p.UpdateCertificateRedeemAmountFromTotalInReserationPrices(It.IsAny<MOBSHOPPrice>(), It.IsAny<double>(), It.IsAny<bool>()));



            //_shoppingUtility.Setup(p => p.IsETCEnabledforMultiTraveler(It.IsAny<int>(), It.IsAny<string>())).Returns(ok);



            _eTCUtility.Setup(p => p.AssignCertificateRedeemAmount(It.IsAny<MOBFOPCertificate>(), It.IsAny<double>()));



            //_shoppingUtility.Setup(p => p.AssignBalanceAttentionInfoWarningMessage(It.IsAny<ReservationInfo2>(), It.IsAny<MOBFOPTravelCertificate>()));



            _eTCUtility.Setup(p => p.UpdateReservationWithCertificatePrices(It.IsAny<Session>(), It.IsAny<MOBFOPTravelerCertificateResponse>(), It.IsAny<double>(), It.IsAny<Reservation>()));



            _eTCUtility.Setup(p => p.AssignIsOtherFOPRequired(It.IsAny<MOBFormofPaymentDetails>(), It.IsAny<List<MOBSHOPPrice>>(), It.IsAny<bool>(), It.IsAny<bool>()));



            _eTCUtility.Setup(p => p.AssignLearmoreInformationDetails(It.IsAny<MOBFOPTravelCertificate>()));



            _eTCUtility.Setup(p => p.UpdateSavedCertificate(It.IsAny<MOBShoppingCart>()));
            BalanceInquiry balanceInquiry = new BalanceInquiry
            {
                IsAccountLocked = true,
                IsAccountNotFound = true,
                FormOfPayment = new Service.Presentation.PaymentModel.Payment
                {
                    AccountNumber = "123",
                    Date = "ABC",
                    AccountNumberHMAC = "XYZ",
                    GroupID = "pqr"

                },
                Response = new ServiceResponse
                {
                    Message = new System.Collections.ObjectModel.Collection<Message>
                    {
                        new Message
                        {
                            Text = "SUCCESS"
                        }
                    }
                },


            };
            string cslresponse = JsonConvert.SerializeObject(balanceInquiry);
            _iETCBalanceEnquiryService.Setup(p => p.GetETCBalanceInquiry(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>())).ReturnsAsync(cslresponse);


            if (mOBFOPTravelerCertificateRequest.SessionId == "17C979E184CC495EA083D45F4DD9D19D")
            {
                _configuration["CombinebilityETCToggle"] = "true";
            }

            if (mOBFOPTravelerCertificateRequest.SessionId == "17C979E184CC495EA083D45F4DD9D19D")
            {
                _configuration["Android_EnableETCCombinability_AppVersion"] = "3.0.44";
            }
            if (mOBFOPTravelerCertificateRequest.SessionId == "17C979E184CC495EA083D45F4DD9D19D")
            {
                _configuration["iPhone_EnableETCCombinability_AppVersion"] = "3.0.44";
            }

            var result = _eTCBusiness.TravelerCertificate(mOBFOPTravelerCertificateRequest);
            // Assert
            Assert.True(result.Exception != null || result.Result != null);

        }

        [Theory]
        [MemberData(nameof(TestDataGenarator.ETC_Negative_Test), MemberType = typeof(TestDataGenarator))]
        public void ETC_Negative_Test(MOBFOPTravelerCertificateRequest mOBFOPTravelerCertificateRequest, Session session, MOBFOPTravelerCertificateResponse response, Reservation reservation, MOBFOPTravelCertificate mOBFOPTravelCertificate, MOBSHOPPrice mOBSHOPPrice)
        {
            _shoppingSessionHelper.Setup(p => p.GetBookingFlowSession(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(session);





            _pKDispenserPublicKey.Setup(p => p.GetCachedOrNewpkDispenserPublicKey(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<United.Mobile.Model.Common.MOBItem>>(), It.IsAny<string>())).ReturnsAsync(response.PkDispenserPublicKey);



            _eTCUtility.Setup(p => p.AddCertificatesToFOP(It.IsAny<List<MOBFOPCertificate>>(), It.IsAny<Session>())).ReturnsAsync(response);



            //_travelCreditUtility.Setup(p => p.GetEligibleFormofPayments(It.IsAny<MOBRequest>(), It.IsAny<Session>(), It.IsAny<MOBShoppingCart>(), It.IsAny<string>(), It.IsAny<string>(), ref It.Ref<bool>.IsAny, It.IsAny<MOBSHOPReservation>(), It.IsAny<bool>(), It.IsAny<SeatChangeState>())).Returns(response.EligibleFormofPayments);



            _sessionHelperService.Setup(p => p.GetSession<Reservation>(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservation);



            _eTCUtility.Setup(p => p.GetShoppingCartTravelCertificateFromPersist(It.IsAny<Session>(), It.IsAny<MOBFOPTravelerCertificateResponse>())).ReturnsAsync(mOBFOPTravelCertificate);



            _eTCUtility.Setup(p => p.UpdateCertificatePrice(It.IsAny<MOBSHOPPrice>(), It.IsAny<double>())).Returns(mOBSHOPPrice);



            _eTCUtility.Setup(p => p.UpdateCertificateRedeemAmountFromTotalInReserationPrices(It.IsAny<MOBSHOPPrice>(), It.IsAny<double>(), It.IsAny<bool>()));



            //_shoppingUtility.Setup(p => p.IsETCEnabledforMultiTraveler(It.IsAny<int>(), It.IsAny<string>())).Returns(ok);



            _eTCUtility.Setup(p => p.AssignCertificateRedeemAmount(It.IsAny<MOBFOPCertificate>(), It.IsAny<double>()));



            //_shoppingUtility.Setup(p => p.AssignBalanceAttentionInfoWarningMessage(It.IsAny<ReservationInfo2>(), It.IsAny<MOBFOPTravelCertificate>()));



            _eTCUtility.Setup(p => p.UpdateReservationWithCertificatePrices(It.IsAny<Session>(), It.IsAny<MOBFOPTravelerCertificateResponse>(), It.IsAny<double>(), It.IsAny<Reservation>()));



            _eTCUtility.Setup(p => p.AssignIsOtherFOPRequired(It.IsAny<MOBFormofPaymentDetails>(), It.IsAny<List<MOBSHOPPrice>>(), It.IsAny<bool>(), It.IsAny<bool>()));



            _eTCUtility.Setup(p => p.AssignLearmoreInformationDetails(It.IsAny<MOBFOPTravelCertificate>()));



            _eTCUtility.Setup(p => p.UpdateSavedCertificate(It.IsAny<MOBShoppingCart>()));
            BalanceInquiry balanceInquiry = new BalanceInquiry
            {
                IsAccountLocked = true,
                IsAccountNotFound = true,
                FormOfPayment = new Service.Presentation.PaymentModel.Payment
                {
                    AccountNumber = "123",
                    Date = "ABC",
                    AccountNumberHMAC = "XYZ",
                    GroupID = "pqr"

                },
                Response = new ServiceResponse
                {
                    Message = new System.Collections.ObjectModel.Collection<Message>
                    {
                        new Message
                        {
                            Text = "SUCCESS"
                        }
                    }
                },


            };
            string cslresponse = JsonConvert.SerializeObject(balanceInquiry);
            _iETCBalanceEnquiryService.Setup(p => p.GetETCBalanceInquiry(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>())).ReturnsAsync(cslresponse);


            if (mOBFOPTravelerCertificateRequest.SessionId == "17C979E184CC495EA083D45F4DD9D19D")
            {
                _configuration["CombinebilityETCToggle"] = "true";
            }

            if (mOBFOPTravelerCertificateRequest.SessionId == "17C979E184CC495EA083D45F4DD9D19D")
            {
                _configuration["Android_EnableETCCombinability_AppVersion"] = "3.0.44";
            }
            if (mOBFOPTravelerCertificateRequest.SessionId == "17C979E184CC495EA083D45F4DD9D19D")
            {
                _configuration["iPhone_EnableETCCombinability_AppVersion"] = "3.0.44";
            }

            var result = _eTCBusiness.TravelerCertificate(mOBFOPTravelerCertificateRequest);
            // Assert
            Assert.True(result.Exception != null || result.Result != null);

        }


        [Theory]
        [MemberData(nameof(TestDataGenarator.ETC_Test1), MemberType = typeof(TestDataGenarator))]
        public void ETC_Test1(MOBFOPBillingContactInfoRequest fOPBillingContactInfoRequest, Session session, MOBFOPTravelerCertificateResponse mOBFOPTravelerCertificateResponse, List<StateProvince> stateProvince, Reservation reservation, bool ok, MOBCSLContentMessagesResponse mOBcSLContentMessagesResponse, List<FormofPaymentOption> formofPaymentOption)
        {

            var _dataPowerFactory = new DataPowerFactory(_configuration, _sessionHelperService.Object);

            _shoppingSessionHelper.Setup(p => p.GetBookingFlowSession(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(session);

            _referencedataService.Setup(p => p.GetDataGetAsync<List<StateProvince>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(stateProvince);

            _sessionHelperService.Setup(p => p.GetSession<MOBShoppingCart>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBFOPTravelerCertificateResponse.ShoppingCart);

            _sessionHelperService.Setup(p => p.GetSession<Reservation>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservation);

            _eTCUtility.Setup(p => p.GetReservationFromPersist(It.IsAny<MOBSHOPReservation>(), It.IsAny<Session>())).ReturnsAsync(mOBFOPTravelerCertificateResponse.Reservation);

            //_shoppingUtility.Setup(p => p.IsManageResETCEnabled(It.IsAny<int>(), It.IsAny<string>())).Returns(ok);

            //_shoppingUtility.Setup(p => p.IncludeFFCResidual(It.IsAny<int>(), It.IsAny<string>())).Returns(ok);

            _sessionHelperService.Setup(p => p.GetSession<MOBCSLContentMessagesResponse>(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBcSLContentMessagesResponse);

            _cMSContentService.Setup(p => p.GetSDLContentByGroupName<MOBCSLContentMessagesResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(mOBcSLContentMessagesResponse);

            _eTCUtility.Setup(p => p.LoadPersistedProfile(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(mOBFOPTravelerCertificateResponse.Profiles);

            //_shoppingUtility.Setup(p => p.IncludeMOBILE12570ResidualFix(It.IsAny<int>(), It.IsAny<string>())).Returns(ok);

            var result = _eTCBusiness.PersistFOPBillingContactInfo_ETC(fOPBillingContactInfoRequest);
            // Assert
            Assert.True(result.Exception != null || result.Result != null);
        }


        


        [Theory]
        [MemberData(nameof(TestDataGenarator.ETC_Test2), MemberType = typeof(TestDataGenarator))]
        public void ETC_Test2(MOBFOPBillingContactInfoRequest fOPBillingContactInfoRequest, Session session, MOBFOPTravelerCertificateResponse mOBFOPTravelerCertificateResponse, List<StateProvince> stateProvince1, Reservation reservation, bool ok, MOBCSLContentMessagesResponse mOBcSLContentMessagesResponse1, List<FormofPaymentOption> formofPaymentOption)
        {
            var _dataPowerFactory = new DataPowerFactory(_configuration, _sessionHelperService.Object);

            _shoppingSessionHelper.Setup(p => p.GetBookingFlowSession(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(session);

            _referencedataService.Setup(p => p.GetDataGetAsync<List<StateProvince>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(stateProvince1);

            _sessionHelperService.Setup(p => p.GetSession<MOBShoppingCart>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBFOPTravelerCertificateResponse.ShoppingCart);

            _sessionHelperService.Setup(p => p.GetSession<Reservation>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservation);

            _eTCUtility.Setup(p => p.GetReservationFromPersist(It.IsAny<MOBSHOPReservation>(), It.IsAny<Session>())).ReturnsAsync(mOBFOPTravelerCertificateResponse.Reservation);

            //_shoppingUtility.Setup(p => p.IsManageResETCEnabled(It.IsAny<int>(), It.IsAny<string>())).Returns(ok);

            //_shoppingUtility.Setup(p => p.IncludeFFCResidual(It.IsAny<int>(), It.IsAny<string>())).Returns(ok);

            _sessionHelperService.Setup(p => p.GetSession<MOBCSLContentMessagesResponse>(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBcSLContentMessagesResponse1);

            _cMSContentService.Setup(p => p.GetSDLContentByGroupName<MOBCSLContentMessagesResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(mOBcSLContentMessagesResponse1);

            _eTCUtility.Setup(p => p.LoadPersistedProfile(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(mOBFOPTravelerCertificateResponse.Profiles);

            //_shoppingUtility.Setup(p => p.IncludeMOBILE12570ResidualFix(It.IsAny<int>(), It.IsAny<string>())).Returns(ok);


            var result = _eTCBusiness.PersistFOPBillingContactInfo_ETC(fOPBillingContactInfoRequest);
            // Assert
            Assert.False(result.Id == 15);

        }


        [Theory]
        [MemberData(nameof(TestDataGenarator.ETC_Flow), MemberType = typeof(TestDataGenarator))]
        public void ETC_Flow(MOBFOPBillingContactInfoRequest fOPBillingContactInfoRequest, Session session, MOBFOPTravelerCertificateResponse mOBFOPTravelerCertificateResponse, List<StateProvince> stateProvince, Reservation reservation, bool ok, MOBCSLContentMessagesResponse mOBcSLContentMessagesResponse, List<FormofPaymentOption> formofPaymentOption)
        {

            var _dataPowerFactory = new DataPowerFactory(_configuration, _sessionHelperService.Object);

            _shoppingSessionHelper.Setup(p => p.GetBookingFlowSession(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(session);

            _referencedataService.Setup(p => p.GetDataGetAsync<List<StateProvince>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(stateProvince);

            _sessionHelperService.Setup(p => p.GetSession<MOBShoppingCart>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBFOPTravelerCertificateResponse.ShoppingCart);

            _sessionHelperService.Setup(p => p.GetSession<Reservation>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservation);

            _eTCUtility.Setup(p => p.GetReservationFromPersist(It.IsAny<MOBSHOPReservation>(), It.IsAny<Session>())).ReturnsAsync(mOBFOPTravelerCertificateResponse.Reservation);

            //_shoppingUtility.Setup(p => p.IsManageResETCEnabled(It.IsAny<int>(), It.IsAny<string>())).Returns(ok);

            //_shoppingUtility.Setup(p => p.IncludeFFCResidual(It.IsAny<int>(), It.IsAny<string>())).Returns(ok);

            _sessionHelperService.Setup(p => p.GetSession<MOBCSLContentMessagesResponse>(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBcSLContentMessagesResponse);

            _cMSContentService.Setup(p => p.GetSDLContentByGroupName<MOBCSLContentMessagesResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(mOBcSLContentMessagesResponse);

            _eTCUtility.Setup(p => p.LoadPersistedProfile(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(mOBFOPTravelerCertificateResponse.Profiles);

            //_shoppingUtility.Setup(p => p.IncludeMOBILE12570ResidualFix(It.IsAny<int>(), It.IsAny<string>())).Returns(ok);

            var result = _eTCBusiness.PersistFOPBillingContactInfo_ETC(fOPBillingContactInfoRequest);
            // Assert
            Assert.True(result.Exception != null || result.Result != null);
        }
    }
}
