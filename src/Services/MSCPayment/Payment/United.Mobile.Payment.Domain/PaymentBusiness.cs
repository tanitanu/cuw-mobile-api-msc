using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using United.Common.Helper;
using United.Common.Helper.MSCPayment.Interfaces;
using United.Definition;
using United.Definition.CSLModels.CustomerProfile;
using United.Definition.FormofPayment;
using United.Definition.Shopping;
using United.Definition.Shopping.PriceBreakDown;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.DynamoDB;
using United.Mobile.DataAccess.MSCPayment.Interfaces;
using United.Mobile.DataAccess.Product.Interfaces;
using United.Mobile.Model.Common;
using United.Persist.Definition.FOP;
using United.Persist.Definition.Shopping;
using United.Service.Presentation.CommonModel;
using United.Service.Presentation.PaymentModel;
using United.Service.Presentation.PaymentRequestModel;
using United.Service.Presentation.PaymentResponseModel;
using United.Service.Presentation.ReferenceDataModel;
using United.Services.Customer.Profile.Common;
using United.Services.FlightShopping.Common.DisplayCart;
using United.Services.FlightShopping.Common.Extensions;
using United.Services.FlightShopping.Common.FlightReservation;
using United.Utility.Helper;
using Address = United.Service.Presentation.CommonModel.Address;
using Characteristic = United.Service.Presentation.CommonModel.Characteristic;
using CslDataVaultRequest = United.Service.Presentation.PaymentRequestModel.DataVault<United.Service.Presentation.PaymentModel.Payment>;
using CslDataVaultResponse = United.Service.Presentation.PaymentResponseModel.DataVault<United.Service.Presentation.PaymentModel.Payment>;
using Genre = United.Service.Presentation.CommonModel.Genre;
using ProfileResponse = United.Persist.Definition.Shopping.ProfileResponse;
using Task = System.Threading.Tasks.Task;
using TravelOption = United.Services.FlightShopping.Common.DisplayCart.TravelOption;

namespace United.Mobile.Payment.Domain

{
    public class PaymentBusiness : IPaymentBusiness
    {
        private readonly ICacheLog<PaymentBusiness> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMSCShoppingSessionHelper _shoppingSessionHelper;
        private readonly ISessionHelperService _sessionHelperService;
        private readonly IShoppingUtility _shoppingUtility;
        private readonly IETCUtility _eTCUtility;
        private readonly IShoppingCartUtility _shoppingCartUtility;
        private readonly IFlightShoppingService _flightShoppingService;
        private readonly IDPService _dPService;
        private readonly IDynamoDBService _dynamoDBService;
        private readonly IPaymentService _paymentService;
        private readonly ICheckoutUtiliy _checkoutUtility;
        private readonly IReferencedataService _referencedataService;
        private readonly IMSCPkDispenserPublicKey _mSCPkDispenserPublicKey;
        private readonly IMSCFormsOfPayment _mSCFormsOfPayment;
        private readonly IDataVaultService _dataVaultService;
        private readonly ILegalDocumentsForTitlesService _legalDocumentsForTitlesService;
        private readonly ICachingService _cachingService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IHeaders _headers;
        private MOBApplication _application = new MOBApplication() { Version = new MOBVersion() };
        private string _deviceId = string.Empty;
        private string _token = string.Empty;
        private string _sessionId = string.Empty;
        private readonly IProvisionService _provisionService;
        private readonly ICMSContentService _cMSContentService;
        private readonly IFeatureSettings _featureSettings;

        public PaymentBusiness(ICacheLog<PaymentBusiness> logger
            , IConfiguration configuration
            , IMSCShoppingSessionHelper shoppingSessionHelper
            , ISessionHelperService sessionHelperService
            , IShoppingUtility shoppingUtility
            , IETCUtility eTCUtility
            , IShoppingCartUtility shoppingCartUtility
            , IFlightShoppingService flightShoppingSercvice
            , IDPService dPService
            , IPaymentService paymentService
            , ICheckoutUtiliy checkoutUtility
            , IReferencedataService referencedataService
            , IMSCPkDispenserPublicKey mSCPkDispenserPublicKey
            , IMSCFormsOfPayment mSCFormsOfPayment
            , IDataVaultService dataVaultService
            , IDynamoDBService dynamoDBService
            , ILegalDocumentsForTitlesService legalDocumentsForTitlesService
            , ICachingService cachingService
            , IShoppingCartService shoppingCartService
            , IHeaders headers
            , IProvisionService provisionService
            , ICMSContentService cMSContentService
            , IFeatureSettings featureSettings)
        {
            _logger = logger;
            _configuration = configuration;
            _shoppingSessionHelper = shoppingSessionHelper;
            _sessionHelperService = sessionHelperService;
            _shoppingUtility = shoppingUtility;
            _eTCUtility = eTCUtility;
            _shoppingCartUtility = shoppingCartUtility;
            _flightShoppingService = flightShoppingSercvice;
            _dPService = dPService;
            _referencedataService = referencedataService;
            _checkoutUtility = checkoutUtility;
            _mSCFormsOfPayment = mSCFormsOfPayment;
            _paymentService = paymentService;
            _dataVaultService = dataVaultService;
            _mSCPkDispenserPublicKey = mSCPkDispenserPublicKey;
            _dynamoDBService = dynamoDBService;
            _legalDocumentsForTitlesService = legalDocumentsForTitlesService;
            _cachingService = cachingService;
            _shoppingCartService = shoppingCartService;
            _headers = headers;
            _provisionService = provisionService;
            _cMSContentService = cMSContentService;
            _featureSettings = featureSettings;
        }

        public async Task<MOBFOPAcquirePaymentTokenResponse> GetPaymentToken(MOBFOPAcquirePaymentTokenRequest request)
        {
            var response = new MOBFOPAcquirePaymentTokenResponse();
            Session session = null;
            if (!string.IsNullOrEmpty(request.SessionId))
            {
                session = await _shoppingSessionHelper.GetValidateSession(request.SessionId, false, true);
                session.Flow = request.Flow;
            }
            //ADDED .TOString()
            switch (request.FormofPaymentCode.ToUpper().ToString())
            {
                case "PP":
                case "PPC":
                    response = await GetPayPalToken(request, session);
                    break;
                case "MP":
                    response = await GetMasterpassToken(request, session);
                    await _sessionHelperService.SaveSession<MOBFOPAcquirePaymentTokenResponse>(response, response.SessionId, new List<string> { response.SessionId, response.ObjectName }, response.ObjectName);
                    break;
            }
            response.TransactionId = request.TransactionId;
            response.SessionId = request.SessionId;
            response.CheckinSessionId = request.CheckinSessionId;
            response.CartId = request.CartId;
            response.Flow = request.Flow;

            return await Task.FromResult(response);

        }
        public async Task<MOBPersistFormofPaymentResponse> PersistFormofPaymentDetails(MOBPersistFormofPaymentRequest request)
        {
            MOBPersistFormofPaymentResponse response = new MOBPersistFormofPaymentResponse();
            MOBShoppingCart shoppingCart = new MOBShoppingCart();
            Session session = null;

            if (!string.IsNullOrEmpty(request.SessionId))
            {
                session = await _shoppingSessionHelper.GetValidateSession(request.SessionId, false, true);
                session.Flow = request.Flow;
            }
            response = await PersistFormofPaymentDetails(request, session);

            if (response.Reservation != null)
                await response.Reservation.Initialise(_configuration, _cachingService);

            if (IsETCchangesEnabled(request.Application.Id, request.Application.Version.Major) && (request.Flow == FlowType.BOOKING.ToString() || ConfigUtility.IsViewResFlowPaymentSvcEnabled(request.Flow)))
            {
                bool isDefault = false;
                var tupleRespEligibleFormofPayments = await _mSCFormsOfPayment.GetEligibleFormofPayments(request, session, response.ShoppingCart, request.CartId, request.Flow, response.Reservation);
                response.EligibleFormofPayments = tupleRespEligibleFormofPayments.EligibleFormofPayments;
                isDefault = tupleRespEligibleFormofPayments.isDefault;
            }
            else
            {
                var persistedEligibleFormofPayments = new List<FormofPaymentOption>();
                persistedEligibleFormofPayments = await _sessionHelperService.GetSession<List<FormofPaymentOption>>(session.SessionId, persistedEligibleFormofPayments.GetType().ToString(), new List<string> { session.SessionId, persistedEligibleFormofPayments.GetType().ToString() }).ConfigureAwait(false);
                response.EligibleFormofPayments = persistedEligibleFormofPayments;

            }
            response.PkDispenserPublicKey = await _mSCPkDispenserPublicKey.GetCachedOrNewpkDispenserPublicKey(request.Application.Id, request.Application.Version.Major, request.DeviceId, session.SessionId, session.Token, session?.CatalogItems, request.Flow).ConfigureAwait(false);
            shoppingCart = response.ShoppingCart;
            if (response.Reservation != null)
            {
                shoppingCart = await _shoppingCartUtility.ReservationToShoppingCart_DataMigration(response.Reservation, shoppingCart, request);
            }
            shoppingCart.Flow = request.Flow;
            response.ShoppingCart = shoppingCart;
            if (IsInternationalBillingAddress_CheckinFlowEnabled(request.Application)
                    && response.ShoppingCart.FormofPaymentDetails?.InternationalBilling?.BillingAddressProperties == null
                    && request?.Flow.ToLower() == FlowType.CHECKIN.ToString().ToLower())
            {
                response.ShoppingCart.FormofPaymentDetails = await GetBillingAddressProperties(response.ShoppingCart.FormofPaymentDetails).ConfigureAwait(false);
            }

            response.TransactionId = request.TransactionId;
            response.SessionId = request.SessionId;
            response.CheckinSessionId = request.CheckinSessionId;
            response.Flow = request.Flow;

            return await Task.FromResult(response);
        }

        public async Task<MOBFormofPaymentDetails> GetBillingAddressProperties(MOBFormofPaymentDetails formofPaymentDetails)
        {
            if (formofPaymentDetails == null)
            {
                formofPaymentDetails = new MOBFormofPaymentDetails();
            }
            formofPaymentDetails.InternationalBilling = new MOBInternationalBilling();
            var billingCountries = await _mSCFormsOfPayment.GetCachedBillingAddressCountries().ConfigureAwait(false);

            if (billingCountries == null || !billingCountries.Any())
            {
                billingCountries = new List<MOBCPBillingCountry>();

                billingCountries.Add(new MOBCPBillingCountry
                {
                    CountryCode = "US",
                    CountryName = "United States",
                    Id = "1",
                    IsStateRequired = true,
                    IsZipRequired = true
                });
            }
            formofPaymentDetails.InternationalBilling.BillingAddressProperties = (billingCountries == null || !billingCountries.Any()) ? null : billingCountries;
            return formofPaymentDetails;
        }

        private bool IsInternationalBillingAddress_CheckinFlowEnabled(MOBApplication application)
        {
            if (_configuration.GetValue<bool>("EnableInternationalBillingAddress_CheckinFlow"))
            {
                if (application != null && GeneralHelper.IsApplicationVersionGreater(application.Id, application.Version.Major, "IntBillingCheckinFlowAndroidversion", "IntBillingCheckinFlowiOSversion", "", "", true, _configuration))
                {
                    return true;
                }
            }
            return false;
        }
        public async Task<MOBPersistFormofPaymentResponse> GetCreditCardToken(MOBPersistFormofPaymentRequest request)
        {
            MOBPersistFormofPaymentResponse response = new MOBPersistFormofPaymentResponse();
            MOBShoppingCart shoppingCart = new MOBShoppingCart();
            Session session = null;
            session = await _shoppingSessionHelper.GetValidateSession(request.SessionId, false, true);
            session.Flow = request.Flow;
            response = await GetCreditCardToken(request, session);
            shoppingCart = response.ShoppingCart;
            if (response.Reservation != null)
            {
                shoppingCart = await _shoppingCartUtility.ReservationToShoppingCart_DataMigration(response.Reservation, shoppingCart, request);
            }
            shoppingCart.Flow = request.Flow;
            response.ShoppingCart = shoppingCart;

            if (IsETCchangesEnabled(request.Application.Id, request.Application.Version.Major) && request.Flow == FlowType.BOOKING.ToString())
            {
                bool isDefault = false;
                var tupleRespEligibleFormofPayments = await _mSCFormsOfPayment.GetEligibleFormofPayments(request, session, response.ShoppingCart, request.CartId, request.Flow, response.Reservation);
                response.EligibleFormofPayments = tupleRespEligibleFormofPayments.EligibleFormofPayments;
                isDefault = tupleRespEligibleFormofPayments.isDefault;
            }
            if (ConfigUtility.IsManageResETCEnabled(request.Application.Id, request.Application.Version.Major) && ConfigUtility.IsViewResFlowPaymentSvcEnabled(request.Flow))
            {
                bool isDefault = false;
                var tupleRes = await _mSCFormsOfPayment.GetEligibleFormofPayments(request, session, response.ShoppingCart, request.CartId, request.Flow, null, GetIsMilesFOPEnabled(response.ShoppingCart));
                response.EligibleFormofPayments = tupleRes.EligibleFormofPayments;
                isDefault = tupleRes.isDefault;
            }

            response.SessionId = request.SessionId;
            response.CheckinSessionId = request.CheckinSessionId;
            response.TransactionId = request.TransactionId;
            response.Flow = request.Flow;
            if (response.Reservation != null)
                await response.Reservation.Initialise(_configuration, _cachingService);

            if (_configuration.GetValue<bool>("VormetricTokenMigration"))
            {
                await _sessionHelperService.SaveSession<MOBPersistFormofPaymentResponse>(response, response.SessionId, new List<string> { session.SessionId, response.GetType().FullName }, response.GetType().FullName);
            }
            return await Task.FromResult(response);
        }

        private bool IsETCchangesEnabled(int applicationId, string appVersion)
        {
            if (_configuration.GetValue<bool>("ETCToggle") && GeneralHelper.IsApplicationVersionGreaterorEqual(applicationId, appVersion, _configuration.GetValue<string>("iPhone_ETC_AppVersion"), _configuration.GetValue<string>("Android_ETC_AppVersion")))
            {
                return true;
            }
            return false;
        }

        public async Task<MOBRegisterOfferResponse> GetCartInformation(MOBSHOPMetaSelectTripRequest request)
        {
            bool isDefault = false;
            MOBRegisterOfferResponse response = new MOBRegisterOfferResponse();
            string sessionid = request.TransactionId;
            Session session = null;

            session = await _shoppingSessionHelper.CreateShoppingSession(request.Application.Id, request.DeviceId, request.Application.Version.Major, request.TransactionId, request.MileagePlusAccountNumber, string.Empty, false, true);
            session.Flow = request.Flow;
            session.CartId = request.CartId;
            response = await GetCartInformation(request, session);
            response.SessionId = session.SessionId;
            bool IsMilesFOPEnabled = false;
            IsMilesFOPEnabled = GetIsMilesFOPEnabled(response.ShoppingCart);
            var tupleRespEligibleFormofPayments = await _mSCFormsOfPayment.GetEligibleFormofPayments(request, session, response.ShoppingCart, request.CartId, session.Flow);
            response.EligibleFormofPayments = tupleRespEligibleFormofPayments.EligibleFormofPayments;
            isDefault = tupleRespEligibleFormofPayments.isDefault;
            response.IsDefaultPaymentOption = isDefault;

            response.Flow = session.Flow;
            response.LanguageCode = request.LanguageCode;
            response.SessionId = session.SessionId;
            response.CheckinSessionId = session.SessionId;
            response.TransactionId = request.TransactionId;
            return await Task.FromResult(response);
        }

        public async Task<MOBFOPResponse> TravelBankCredit(MOBFOPTravelerBankRequest request)
        {
            MOBFOPResponse response = new MOBFOPResponse();
            Session session = await _shoppingSessionHelper.GetBookingFlowSession(request.SessionId);
            if (session == null)
            {
                throw new MOBUnitedException("Could not find your booking session.");
            }

            response.Flow = request.Flow;
            response = await TravelBankCredit(session, request, true);
            bool isDefault = false;
            var tupleRespEligibleFormofPayments = await _mSCFormsOfPayment.GetEligibleFormofPayments(request, session, response.ShoppingCart, request.CartId, request.Flow, response.Reservation);
            response.EligibleFormofPayments = tupleRespEligibleFormofPayments.EligibleFormofPayments;
            isDefault = tupleRespEligibleFormofPayments.isDefault;
            //FilePersist.Save<MOBFOPResponse>(session.SessionId, typeof(MOBFOPResponse).FullName, response);
            response.SessionId = request.SessionId;
            await _sessionHelperService.SaveSession<MOBFOPResponse>(response, request.SessionId, new List<string> { request.SessionId, response.ObjectName }, response.ObjectName);
            return await System.Threading.Tasks.Task.FromResult(response);
        }

        private async Task<MOBFOPResponse> TravelBankCredit(Session session, United.Definition.MOBFOPTravelerBankRequest request, bool isCustomerCall)
        {
            Reservation bookingPathReservation = new Reservation();
            var bookingPathReservationResult = await LoadBasicFOPResponse(session, bookingPathReservation);
            MOBFOPResponse response = bookingPathReservationResult.MobFopResponse;
            bookingPathReservation = bookingPathReservationResult.BookingPathReservation;
            response.Flow = request.Flow;

            var travelBankDetails = response.ShoppingCart.FormofPaymentDetails.TravelBankDetails;
            if (travelBankDetails == null)
            {
                travelBankDetails = new MOBFOPTravelBankDetails();
            }
            var scRESProduct = response.ShoppingCart.Products.Find(p => p.Code == "RES");
            double prodValue = Convert.ToDouble(scRESProduct.ProdTotalPrice);
            prodValue = Math.Round(prodValue, 2, MidpointRounding.AwayFromZero);

            travelBankDetails.TBBalance = await _mSCFormsOfPayment.GetTravelBankBalance(session.SessionId).ConfigureAwait(false);
            travelBankDetails.DisplayTBBalance = (travelBankDetails.TBBalance).ToString("C2", CultureInfo.CurrentCulture);
            travelBankDetails.TBApplied = request.AppliedAmount > prodValue ? prodValue : request.AppliedAmount;
            travelBankDetails.TBApplied = request.IsRemove ? 0 : travelBankDetails.TBApplied;
            if (isCustomerCall)
                travelBankDetails.TBAppliedByCustomer = travelBankDetails.TBApplied;
            else if (travelBankDetails.TBAppliedByCustomer > travelBankDetails.TBApplied && travelBankDetails.TBAppliedByCustomer <= prodValue)
                travelBankDetails.TBApplied = travelBankDetails.TBAppliedByCustomer;

            travelBankDetails.DisplaytbApplied = (travelBankDetails.TBApplied).ToString("C2", CultureInfo.CurrentCulture);
            travelBankDetails.RemainingBalance = travelBankDetails.TBBalance > 0 ? travelBankDetails.TBBalance - travelBankDetails.TBApplied : 0;
            travelBankDetails.DisplayRemainingBalance = (travelBankDetails.RemainingBalance).ToString("C2", CultureInfo.CurrentCulture);
            travelBankDetails.DisplayAvailableBalanceAsOfDate = $"{"Balance as of "}{ DateTime.Now.ToString("MM/dd/yyyy") }";
            List<CMSContentMessage> lstMessages = await _shoppingCartUtility.GetSDLContentByGroupName(request, session.SessionId, session.Token, _configuration.GetValue<string>("CMSContentMessages_GroupName_BookingRTI_Messages"), "BookingPathRTI_CMSContentMessagesCached_StaticGUID");
            travelBankDetails.ApplyTBContentMessage = GetSDLContentMessages(lstMessages, "RTI.TravelBank.Apply");
            //travelBankDetails.ReviewTBContentMessage = GetSDLContentMessages(lstMessages, "RTI.TravelBank.Review");
            travelBankDetails.LearnmoreTermsandConditions = GetSDLContentMessages(lstMessages, "RTI.TravelBank.Learnmore");
            SwapTitleAndLocation(travelBankDetails.ApplyTBContentMessage);
            //SwapTitleAndLocation(travelBankDetails.ReviewTBContentMessage);
            SwapTitleAndLocation(travelBankDetails.LearnmoreTermsandConditions);
            //TODO
            UpdateCertificateAmountInTotalPrices(bookingPathReservation.Prices, (request.IsRemove ? 0 : travelBankDetails.TBApplied), "TB", "TravelBank cash");
            response.Reservation.Prices = bookingPathReservation.Prices;

            _eTCUtility.AssignIsOtherFOPRequired(response.ShoppingCart.FormofPaymentDetails, response.Reservation.Prices);
            AssignFormOfPaymentType(response.ShoppingCart.FormofPaymentDetails, response.Reservation.Prices, response.ShoppingCart.FormofPaymentDetails.IsOtherFOPRequired, travelBankDetails.TBApplied, MOBFormofPayment.TB);
            response.ShoppingCart.FormofPaymentDetails.TravelBankDetails = travelBankDetails;

            if (IsEnableBundleLiveUpdateChanges(request.Application.Id, request.Application.Version.Major, response.Reservation?.ShopReservationInfo2?.IsDisplayCart == true))
            {
                BuildOmniCart(response.ShoppingCart, response.Reservation, request.Application);
            }

            //response.PkDispenserPublicKey = United.Mobile.DAL.Utility.GetCachedOrNewpkDispenserPublicKey(request.Application.Id, request.Application.Version.Major, request.DeviceId, session.SessionId, session.Token, LogEntries, _traceSwitch);
            response.PkDispenserPublicKey = await _mSCPkDispenserPublicKey.GetCachedOrNewpkDispenserPublicKey(request.Application.Id, request.Application.Version.Major, request.DeviceId, session.SessionId, session.Token, session?.CatalogItems).ConfigureAwait(false);


            await _sessionHelperService.SaveSession<Reservation>(bookingPathReservation, session.SessionId, new List<string> { session.SessionId, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName).ConfigureAwait(false);

            await _sessionHelperService.SaveSession<MOBShoppingCart>(response.ShoppingCart, session.SessionId, new List<string> { session.SessionId, response.ShoppingCart.ObjectName }, response.ShoppingCart.ObjectName).ConfigureAwait(false);


            return response;
        }

        private void BuildOmniCart(MOBShoppingCart shoppingCart, MOBSHOPReservation reservation, MOBApplication application)
        {
            if (shoppingCart.OmniCart == null)
            {
                shoppingCart.OmniCart = new MOBCart();
            }
            shoppingCart.OmniCart.CartItemsCount = GetCartItemsCount(shoppingCart);
            shoppingCart.OmniCart.TotalPrice = GetTotalPrice(shoppingCart?.Products, reservation);
            shoppingCart.OmniCart.PayLaterPrice = GetPayLaterAmount(shoppingCart?.Products, reservation);
            if (_configuration.GetValue<bool>("EnableShoppingCartPhase2Changes"))
            {
                shoppingCart.OmniCart.CostBreakdownFareHeader = _shoppingCartUtility.GetCostBreakdownFareHeader(reservation?.ShopReservationInfo2?.TravelType, shoppingCart);
            }

            if (_configuration.GetValue<bool>("EnableLivecartForAwardTravel") && reservation.AwardTravel)
            {
                shoppingCart.OmniCart.AdditionalMileDetail = _shoppingCartUtility.GetAdditionalMileDetail(reservation);
            }

            //shoppingCart.OmniCart.FOPDetails = shoppingCart.OmniCart.FOPDetails.Count==0? shoppingCart.OmniCart.FOPDetails=null:GetFOPDetails(reservation);
            shoppingCart.OmniCart.FOPDetails = _shoppingCartUtility.GetFOPDetails(reservation, application);
            if (reservation != null && reservation.ShopReservationInfo2 != null && !string.IsNullOrEmpty(reservation.ShopReservationInfo2.CorporateDisclaimerText))
            {
                shoppingCart.OmniCart.CorporateDisclaimerText = reservation.ShopReservationInfo2.CorporateDisclaimerText;
            }
            AssignUpliftText(shoppingCart, reservation);                //Assign message text and link text to the Uplift
        }

        private void AssignUpliftText(MOBShoppingCart shoppingCart, MOBSHOPReservation reservation)
        {

            if (_mSCFormsOfPayment.IsEligibileForUplift(reservation, shoppingCart) && shoppingCart?.FormofPaymentDetails?.MoneyPlusMilesCredit?.SelectedMoneyPlusMiles == null)                //Check if eligible for Uplift
            {
                shoppingCart.OmniCart.IsUpliftEligible = true;      //Set property to true, if Uplift is eligible
            }
            else //Set Uplift properties to false / empty as Uplift isn't eligible
            {
                shoppingCart.OmniCart.IsUpliftEligible = false;
            }
        }

        public List<MOBSection> GetFOPDetails(MOBSHOPReservation reservation)
        {
            var mobSection = default(MOBSection);
            if (reservation?.Prices?.Count > 0)
            {
                var travelCredit = reservation.Prices.FirstOrDefault(price => new[] { "TB", "CERTIFICATE", "FFC" }.Any(credit => string.Equals(price.PriceType, credit, StringComparison.OrdinalIgnoreCase)));
                if (travelCredit != null)
                {
                    if (string.Equals(travelCredit.PriceType, "TB", StringComparison.OrdinalIgnoreCase))
                    {
                        mobSection = new MOBSection();
                        mobSection.Text1 = !string.IsNullOrEmpty(_configuration.GetValue<string>("UnitedTravelBankCashLabelText")) ? _configuration.GetValue<string>("UnitedTravelBankCashLabelText") : "United TravelBank cash";
                        mobSection.Text2 = !string.IsNullOrEmpty(_configuration.GetValue<string>("TravelBankCashAppliedLabelText")) ? _configuration.GetValue<string>("TravelBankCashAppliedLabelText") : "TravelBank cash applied";
                        mobSection.Text3 = travelCredit.FormattedDisplayValue;
                    }
                    else
                    {
                        mobSection = new MOBSection();
                        mobSection.Text1 = !string.IsNullOrEmpty(_configuration.GetValue<string>("TravelCreditsLabelText")) ? _configuration.GetValue<string>("TravelCreditsLabelText") : "Travel credits";
                        mobSection.Text2 = !string.IsNullOrEmpty(_configuration.GetValue<string>("CreditKeyLabelText")) ? _configuration.GetValue<string>("CreditKeyLabelText") : "Credit";
                        mobSection.Text3 = travelCredit.FormattedDisplayValue;

                    }
                }

            }
            return mobSection != null ? new List<MOBSection> { mobSection } : null;
        }

        private MOBItem GetPayLaterAmount(List<MOBProdDetail> products, MOBSHOPReservation reservation)
        {
            if (products != null && reservation != null)
            {
                if (IsFarelock(products))
                {
                    return new MOBItem { Id = _configuration.GetValue<string>("PayDueLaterLabelText"), CurrentValue = GetGrandTotalPrice(reservation) };
                }
            }
            return null;
        }

        private MOBItem GetTotalPrice(List<MOBProdDetail> products, MOBSHOPReservation reservation)
        {
            if (products != null && reservation != null)
            {
                return new MOBItem
                {
                    Id = IsFarelock(products) ? _configuration.GetValue<string>("FarelockTotalPriceLabelText") : _configuration.GetValue<string>("TotalPriceLabelText")
                ,
                    CurrentValue = IsFarelock(products) ? GetFareLockPrice(products) : GetGrandTotalPrice(reservation)
                };
            }
            return null;
        }
        private string GetGrandTotalPrice(MOBSHOPReservation reservation)
        {
            if (reservation?.Prices != null)
            {
                var grandTotalPrice = reservation.Prices.Exists(p => p.DisplayType.ToUpper().Equals("GRAND TOTAL"))
                                ? reservation.Prices.FirstOrDefault(p => p.DisplayType.ToUpper().Equals("GRAND TOTAL"))
                                : reservation.Prices.FirstOrDefault(p => p.DisplayType.ToUpper().Equals("TOTAL"));
                if (_configuration.GetValue<bool>("EnableLivecartForAwardTravel") && reservation.AwardTravel)
                {
                    var totalDue = string.Empty;
                    var awardPrice = reservation.Prices.FirstOrDefault(p => string.Equals("miles", p.DisplayType, StringComparison.OrdinalIgnoreCase));
                    if (awardPrice != null)
                    {
                        totalDue = FormatedMilesValueAndText(awardPrice.Value);
                    }
                    if (grandTotalPrice != null)
                    {
                        totalDue = string.IsNullOrWhiteSpace(totalDue)
                                    ? grandTotalPrice.FormattedDisplayValue
                                    : $"{totalDue} + {grandTotalPrice.FormattedDisplayValue}";
                    }
                    return totalDue;
                }
                else
                {
                    if (grandTotalPrice != null)
                    {
                        return grandTotalPrice.FormattedDisplayValue;
                    }
                }
            }
            return string.Empty;
        }

        private static string FormatedMilesValueAndText(double milesValue)
        {
            if (milesValue >= 1000)
                return (milesValue / 1000D).ToString("0.#" + "K miles");
            else if (milesValue > 0)
                return milesValue.ToString("0,# miles");
            else
                return string.Empty;
        }

        private string GetFareLockPrice(List<MOBProdDetail> products)
        {
            return products.Where(p => p.Code.ToUpper() == "FARELOCK" || p.Code.ToUpper() == "FLK").First().ProdDisplayTotalPrice;
        }

        private bool IsEnableBundleLiveUpdateChanges(int applicationId, string appVersion, bool isDisplayCart)
        {
            if (_configuration.GetValue<bool>("EnableBundleLiveUpdateChanges")
                && GeneralHelper.IsApplicationVersionGreaterorEqual(applicationId, appVersion,
                _configuration.GetValue<string>("Android_EnableBundleLiveUpdateChanges_AppVersion"), _configuration.GetValue<string>("iPhone_EnableBundleLiveUpdateChanges_AppVersion"))
                && isDisplayCart)
            {
                return true;
            }
            return false;
        }

        private void AssignFormOfPaymentType(MOBFormofPaymentDetails formofPaymentDetails, List<MOBSHOPPrice> prices, bool isOtherFOPRequired, double fopAmount, MOBFormofPayment fopPayment)
        {
            //need to update only when TravelFutureFlightCredit is added as formofpayment.          
            if (fopAmount > 0)
            {
                if (!isOtherFOPRequired)
                {
                    formofPaymentDetails.FormOfPaymentType = fopPayment.ToString();
                    if (!string.IsNullOrEmpty(formofPaymentDetails.CreditCard?.Message) &&
                        _configuration.GetValue<string>("CreditCardDateExpiredMessage").IndexOf(formofPaymentDetails.CreditCard?.Message, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        formofPaymentDetails.CreditCard = null;
                    }
                }
                else
                {
                    formofPaymentDetails.FormOfPaymentType = MOBFormofPayment.CreditCard.ToString();
                }
            }
            else
            {
                formofPaymentDetails.FormOfPaymentType = MOBFormofPayment.CreditCard.ToString();
            }
        }

        private void UpdateCertificateAmountInTotalPrices(List<MOBSHOPPrice> prices, double certificateTotalAmount, string fopType, string fopDescription)
        {
            var ffcPrice = prices.FirstOrDefault(p => p.DisplayType.ToUpper() == fopType);
            var grandtotal = prices.FirstOrDefault(p => p.DisplayType.ToUpper() == "GRAND TOTAL");

            if (ffcPrice == null && certificateTotalAmount > 0)
            {
                ffcPrice = new MOBSHOPPrice();
                prices.Add(ffcPrice);
            }
            else if (ffcPrice != null)
            {
                _eTCUtility.UpdateCertificateRedeemAmountFromTotalInReserationPrices(grandtotal, ffcPrice.Value, false);
            }

            if (certificateTotalAmount > 0)
            {
                UpdateCertificatePrice(ffcPrice, certificateTotalAmount, fopType, fopDescription, isAddNegative: true);
                _eTCUtility.UpdateCertificateRedeemAmountFromTotalInReserationPrices(grandtotal, certificateTotalAmount);
            }
            else
            {
                prices.Remove(ffcPrice);
            }
        }

        private MOBSHOPPrice UpdateCertificatePrice(MOBSHOPPrice ffc, double totalAmount, string priceType, string priceTypeDescription, string status = "", bool isAddNegative = false)
        {
            ffc.CurrencyCode = "USD";
            ffc.DisplayType = priceType;
            ffc.PriceType = priceType;
            ffc.Status = status;
            ffc.PriceTypeDescription = priceTypeDescription;
            ffc.Value = totalAmount;
            ffc.Value = Math.Round(ffc.Value, 2, MidpointRounding.AwayFromZero);
            ffc.FormattedDisplayValue = (isAddNegative ? "-" : "") + (ffc.Value).ToString("C2", CultureInfo.CurrentCulture);
            ffc.DisplayValue = string.Format("{0:#,0.00}", ffc.Value);
            return ffc;
        }

        private List<MOBMobileCMSContentMessages> SwapTitleAndLocation(List<MOBMobileCMSContentMessages> cmsList)
        {
            foreach (var item in cmsList)
            {
                string location = item.LocationCode;
                item.LocationCode = item.Title;
                item.Title = location;
            }

            return cmsList;
        }

        private List<MOBMobileCMSContentMessages> GetSDLContentMessages(List<CMSContentMessage> lstMessages, string title)
        {
            List<MOBMobileCMSContentMessages> messages = new List<MOBMobileCMSContentMessages>();
            messages.AddRange(GetSDLMessageFromList(lstMessages, title));

            return messages;
        }

        private List<MOBMobileCMSContentMessages> GetSDLMessageFromList(List<CMSContentMessage> list, string title)
        {
            List<MOBMobileCMSContentMessages> listOfMessages = new List<MOBMobileCMSContentMessages>();
            list?.Where(l => l.Title.ToUpper().Equals(title.ToUpper()))?.ForEach(i => listOfMessages.Add(new MOBMobileCMSContentMessages()
            {
                Title = i.Title,
                ContentFull = i.ContentFull,
                HeadLine = i.Headline,
                ContentShort = i.ContentShort,
                LocationCode = i.LocationCode
            }));

            return listOfMessages;
        }

        private async Task<(MOBFOPResponse MobFopResponse, Reservation BookingPathReservation)> LoadBasicFOPResponse(Session session, Reservation bookingPathReservation)
        {
            var response = new MOBFOPResponse();
            bookingPathReservation = new Reservation();
            bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(session.SessionId, bookingPathReservation.ObjectName, new List<string> { session.SessionId, bookingPathReservation.ObjectName }).ConfigureAwait(false);
            response.Reservation = new MOBSHOPReservation(_configuration, _cachingService);
            response.Reservation = _eTCUtility.MakeReservationFromPersistReservation(response.Reservation, bookingPathReservation, session);
            var persistShoppingCart = new MOBShoppingCart();
            persistShoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(session.SessionId, persistShoppingCart.ObjectName, new List<string> { session.SessionId, persistShoppingCart.ObjectName }).ConfigureAwait(false);
            response.ShoppingCart = persistShoppingCart;
            response.Profiles = await _eTCUtility.LoadPersistedProfile(session.SessionId, response.ShoppingCart.Flow);
            if (response?.ShoppingCart?.FormofPaymentDetails == null)
            {
                response.ShoppingCart.FormofPaymentDetails = new MOBFormofPaymentDetails();
            }
            return (response, bookingPathReservation);
        }

        private bool GetIsMilesFOPEnabled(MOBShoppingCart shoppingCart)
        {
            Int32 j;
            Int32.TryParse(shoppingCart.TotalMiles, out j);
            return j > 0;
        }

        private async Task<MOBPersistFormofPaymentResponse> GetCreditCardToken(MOBPersistFormofPaymentRequest request, Session session)
        {
            MOBPersistFormofPaymentResponse response = new MOBPersistFormofPaymentResponse();

            var persistShoppingCart = new MOBShoppingCart();
            persistShoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(session.SessionId, persistShoppingCart.ObjectName, new List<string> { session.SessionId, persistShoppingCart.ObjectName }).ConfigureAwait(false);
            if (persistShoppingCart == null)
                throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));

            var bookingPathReservation = new Reservation();
            bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(session.SessionId, bookingPathReservation.ObjectName, new List<string> { session.SessionId, bookingPathReservation.ObjectName }).ConfigureAwait(false);


            //Build formofpaymentdetails
            var formofPaymentDetails = new MOBFormofPaymentDetails();
            if (!_configuration.GetValue<bool>("DisableManageResChanges23C"))
            {
                if (ConfigUtility.IsManageResETCEnabled(request.Application.Id, request.Application.Version.Major) && request.Flow == FlowType.VIEWRES.ToString()
                    && persistShoppingCart?.FormofPaymentDetails?.TravelCertificate != null)
                {
                    formofPaymentDetails.TravelCertificate = persistShoppingCart.FormofPaymentDetails.TravelCertificate;
                    formofPaymentDetails.IsOtherFOPRequired = persistShoppingCart.FormofPaymentDetails != null ? persistShoppingCart.FormofPaymentDetails.IsOtherFOPRequired : true;
                }
            }

            if (!(persistShoppingCart.Flow == FlowType.BOOKING.ToString() && bookingPathReservation.IsRedirectToSecondaryPayment))
            {
                formofPaymentDetails.CreditCard = await GetCreditCardWithToken(request.FormofPaymentDetails.CreditCard, session, request);
            }
            else
            {
                formofPaymentDetails.CreditCard = persistShoppingCart.FormofPaymentDetails.CreditCard;
                formofPaymentDetails.SecondaryCreditCard = await GetCreditCardWithToken(request.FormofPaymentDetails.CreditCard, session, request);
            }
            formofPaymentDetails.FormOfPaymentType = MOBFormofPayment.CreditCard.ToString();
            formofPaymentDetails.EmailAddress = request.FormofPaymentDetails.EmailAddress;
            formofPaymentDetails.BillingAddress = await GetBillingAddressWithValidStateCode(request, session);
            formofPaymentDetails.Phone = request.FormofPaymentDetails.Phone;
            if (IncludeFFCResidual(request.Application.Id, request.Application.Version.Major) && persistShoppingCart.FormofPaymentDetails != null)
            {
                formofPaymentDetails.TravelFutureFlightCredit = persistShoppingCart.FormofPaymentDetails.TravelFutureFlightCredit;
                formofPaymentDetails.IsOtherFOPRequired = persistShoppingCart.FormofPaymentDetails.IsOtherFOPRequired;
            }
            if (IncludeTravelCredit(request.Application.Id, request.Application.Version.Major))
            {
                formofPaymentDetails.TravelCreditDetails = persistShoppingCart.FormofPaymentDetails?.TravelCreditDetails;
            }
            if (persistShoppingCart.FormofPaymentDetails?.MilesFOP != null)
            {
                formofPaymentDetails.MilesFOP = persistShoppingCart.FormofPaymentDetails?.MilesFOP;
            }

            persistShoppingCart.FormofPaymentDetails = formofPaymentDetails;
            persistShoppingCart.CartId = request.CartId;
            persistShoppingCart.TotalPrice = String.Format("{0:0.00}", request.Amount);
            if (EnableInflightContactlessPayment(request.Application.Id, request.Application.Version.Major, persistShoppingCart.Flow != FlowType.BOOKING.ToString()))
            {
                if (persistShoppingCart?.InFlightContactlessPaymentEligibility?.IsEligibleInflightCLPayment ?? false)
                    persistShoppingCart.InFlightContactlessPaymentEligibility.IsCCSelectedForContactless = request.IsCCSelectedForContactless;
            }

            await _sessionHelperService.SaveSession<MOBShoppingCart>(persistShoppingCart, session.SessionId, new List<string> { session.SessionId, persistShoppingCart.ObjectName }, persistShoppingCart.ObjectName).ConfigureAwait(false);


            response.ShoppingCart = persistShoppingCart;
            List<FormofPaymentOption> persistedEligibleFormofPayments = new List<FormofPaymentOption>();
            persistedEligibleFormofPayments = await _sessionHelperService.GetSession<List<FormofPaymentOption>>(session.SessionId, persistedEligibleFormofPayments.GetType().ToString(), new List<string> { session.SessionId, persistedEligibleFormofPayments.GetType().ToString() }).ConfigureAwait(false);
            //persistedEligibleFormofPayments = FilePersist.Load<List<FormofPaymentOption>>(request.SessionId, persistedEligibleFormofPayments.GetType().ToString());
            response.EligibleFormofPayments = persistedEligibleFormofPayments;
            if (await _shoppingCartUtility.IsCheckInFlow(request.Flow))
            {
                response.PkDispenserPublicKey = null;
            }
            else
            {
                response.PkDispenserPublicKey = await _mSCPkDispenserPublicKey.GetCachedOrNewpkDispenserPublicKey(request.Application.Id, request.Application.Version.Major, request.DeviceId, session.SessionId, session.Token, session?.CatalogItems).ConfigureAwait(false);
            }
                if (request.Flow == "BOOKING" || request.Flow == FlowType.RESHOP.ToString())
            {
                response.Reservation = new MOBSHOPReservation(_configuration, _cachingService);

                MOBFormofPayment formOfPaymentType;
                Enum.TryParse(request.FormofPaymentDetails.FormOfPaymentType, out formOfPaymentType);
                bookingPathReservation.FormOfPaymentType = formOfPaymentType;
                bookingPathReservation.CreditCards = !bookingPathReservation.IsRedirectToSecondaryPayment ? new List<MOBCreditCard> { formofPaymentDetails.CreditCard } : new List<MOBCreditCard> { formofPaymentDetails.SecondaryCreditCard };
                bookingPathReservation.CreditCardsAddress = new List<MOBAddress> { formofPaymentDetails.BillingAddress };
                bookingPathReservation.ReservationPhone = request.FormofPaymentDetails.Phone;
                bookingPathReservation.ReservationEmail = new MOBEmail { EmailAddress = request.FormofPaymentDetails.EmailAddress };
                bookingPathReservation.PayPal = null;
                bookingPathReservation.PayPalPayor = null;
                bookingPathReservation.Masterpass = null;
                bookingPathReservation.MasterpassSessionDetails = null;
                await HandleAncillaryOptionsForUplift(request, bookingPathReservation);

                await _sessionHelperService.SaveSession<Reservation>(bookingPathReservation, session.SessionId, new List<string> { session.SessionId, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName).ConfigureAwait(false);

                response.Reservation = await GetReservationFromPersist(response.Reservation, session);
                if (request.Flow == "BOOKING")
                {
                    //Covid 19 Emergency WHO TPI content
                    if (_configuration.GetValue<bool>("ToggleCovidEmergencytextTPI") == true)
                    {
                        bool return_TPICOVID_19WHOMessage_For_BackwardBuilds = GeneralHelper.isApplicationVersionGreater(request.Application.Id, request.Application.Version.Major, "Android_Return_TPICOVID_19WHOMessage__For_BackwardBuilds", "iPhone_Return_TPICOVID_19WHOMessage_For_BackwardBuilds", "", "", false, _configuration);
                        if (!return_TPICOVID_19WHOMessage_For_BackwardBuilds && response.Reservation != null
                            && response.Reservation.TripInsuranceInfoBookingPath != null && response.Reservation.TripInsuranceInfoBookingPath.TPIAIGReturnedMessageContentList != null
                            && response.Reservation.TripInsuranceInfoBookingPath.TPIAIGReturnedMessageContentList.Count > 0)
                        {
                            MOBItem tpiCOVID19EmergencyAlert = response.Reservation.TripInsuranceInfoBookingPath.TPIAIGReturnedMessageContentList.Find(p => p.Id.ToUpper().Trim() == "COVID19EmergencyAlert".ToUpper().Trim());
                            if (tpiCOVID19EmergencyAlert != null)
                            {
                                response.Reservation.TripInsuranceInfoBookingPath.Tnc = response.Reservation.TripInsuranceInfoBookingPath.Tnc +
                                    "<br><br>" + tpiCOVID19EmergencyAlert.CurrentValue;
                            }
                        }
                    }
                }
            }


            return response;
        }


        private async Task<MOBPersistFormofPaymentResponse> PersistFormofPaymentDetails(MOBPersistFormofPaymentRequest request, Session session)
        {
            MOBPersistFormofPaymentResponse response = new MOBPersistFormofPaymentResponse();
            response = await BuildPersistFormofPaymentDetailsresponse(request, session);
            if (request.Flow == "BOOKING" || request.Flow == FlowType.RESHOP.ToString())
            {
                response.Reservation = new MOBSHOPReservation(_configuration, _cachingService);
                await response.Reservation.UpdateRewards(_configuration, _cachingService);
                Reservation bookingPathReservation = new Reservation();
                bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(request.SessionId, bookingPathReservation.ObjectName, new List<string> { request.SessionId, bookingPathReservation.ObjectName }).ConfigureAwait(false);
                MOBFormofPayment formOfPaymentType;
                Enum.TryParse(request.FormofPaymentDetails.FormOfPaymentType, out formOfPaymentType);
                bookingPathReservation.FormOfPaymentType = formOfPaymentType;
                bookingPathReservation.CreditCards = null;
                bookingPathReservation.CreditCardsAddress = request.FormofPaymentDetails?.BillingAddress != null ? new List<MOBAddress> { request.FormofPaymentDetails.BillingAddress } : null;
                bookingPathReservation.PayPal = request.FormofPaymentDetails.PayPal;
                bookingPathReservation.PayPalPayor = response.ShoppingCart.FormofPaymentDetails.PayPalPayor;
                bookingPathReservation.Masterpass = request.FormofPaymentDetails.Masterpass;
                bookingPathReservation.MasterpassSessionDetails = response.ShoppingCart.FormofPaymentDetails.MasterPassSessionDetails;

                if (_configuration.GetValue<bool>("EnableUpliftPayment"))
                {
                    if (bookingPathReservation.ShopReservationInfo2 != null)
                    {
                        bookingPathReservation.ShopReservationInfo2.Uplift = response.ShoppingCart.FormofPaymentDetails.Uplift;
                    }
                    bookingPathReservation.ReservationPhone = request.FormofPaymentDetails.Phone ?? bookingPathReservation.ReservationPhone;
                    bookingPathReservation.ReservationEmail = !string.IsNullOrWhiteSpace(request.FormofPaymentDetails.EmailAddress) ? new MOBEmail { EmailAddress = request.FormofPaymentDetails.EmailAddress } : bookingPathReservation.ReservationEmail;
                    await HandleAncillaryOptionsForUplift(request, bookingPathReservation);
                }

                await _sessionHelperService.SaveSession<Reservation>(bookingPathReservation, session.SessionId, new List<string> { session.SessionId, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName).ConfigureAwait(false);
                response.Reservation = await GetReservationFromPersist(response.Reservation, session);

                if (request.Flow == "BOOKING")
                {
                    //Covid 19 Emergency WHO TPI content
                    if (_configuration.GetValue<bool>("ToggleCovidEmergencytextTPI") == true)
                    {
                        bool return_TPICOVID_19WHOMessage_For_BackwardBuilds = GeneralHelper.IsApplicationVersionGreater(request.Application.Id, request.Application.Version.Major, "Android_Return_TPICOVID_19WHOMessage__For_BackwardBuilds", "iPhone_Return_TPICOVID_19WHOMessage_For_BackwardBuilds", "", "", false, _configuration);
                        if (!return_TPICOVID_19WHOMessage_For_BackwardBuilds && response.Reservation != null
                            && response.Reservation.TripInsuranceInfoBookingPath != null && response.Reservation.TripInsuranceInfoBookingPath.TPIAIGReturnedMessageContentList != null
                            && response.Reservation.TripInsuranceInfoBookingPath.TPIAIGReturnedMessageContentList.Count > 0)
                        {
                            MOBItem tpiCOVID19EmergencyAlert = response.Reservation.TripInsuranceInfoBookingPath.TPIAIGReturnedMessageContentList.Find(p => p.Id.ToUpper().Trim() == "COVID19EmergencyAlert".ToUpper().Trim());
                            if (tpiCOVID19EmergencyAlert != null)
                            {
                                response.Reservation.TripInsuranceInfoBookingPath.Tnc = response.Reservation.TripInsuranceInfoBookingPath.Tnc +
                                    "<br><br>" + tpiCOVID19EmergencyAlert.CurrentValue;
                            }
                        }
                    }
                }
            }
            ProfileResponse persistedProfileResponse = new ProfileResponse();
            //persistedProfileResponse = FilePersist.Load<ProfileResponse>(session.SessionId, persistedProfileResponse.ObjectName, false);
            persistedProfileResponse = await _sessionHelperService.GetSession<ProfileResponse>(session.SessionId, persistedProfileResponse.ObjectName, new List<string> { session.SessionId, persistedProfileResponse.ObjectName }).ConfigureAwait(false);
            response.Profiles = persistedProfileResponse != null ? persistedProfileResponse.Response.Profiles : null;
            return response;
        }

        private async Task<MOBPersistFormofPaymentResponse> BuildPersistFormofPaymentDetailsresponse(MOBPersistFormofPaymentRequest request, Session session)
        {
            MOBPersistFormofPaymentResponse response = new MOBPersistFormofPaymentResponse();
            //var shopping = new Shopping();
            var formofPaymentDetails = new MOBFormofPaymentDetails();
            MOBShoppingCart persistShoppingCart = new MOBShoppingCart();
            persistShoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(request.SessionId, persistShoppingCart.ObjectName, new List<string> { request.SessionId, persistShoppingCart.ObjectName }).ConfigureAwait(false);

            if (request.FormofPaymentDetails.FormOfPaymentType.ToUpper() == MOBFormofPayment.PayPal.ToString().ToUpper() || request.FormofPaymentDetails.FormOfPaymentType.ToUpper() == MOBFormofPayment.PayPalCredit.ToString().ToUpper() || request.FormofPaymentDetails.PayPal != null)
            {
                formofPaymentDetails = await PersistPayPalDetails(request, session);
                formofPaymentDetails.FormOfPaymentType = request.FormofPaymentDetails.FormOfPaymentType.ToString();
                if (IncludeTravelBankFOP(request.Application.Id, request.Application.Version.Major))
                {
                    formofPaymentDetails.TravelBankDetails = persistShoppingCart.FormofPaymentDetails?.TravelBankDetails;
                }
            }
            else if (request.FormofPaymentDetails.FormOfPaymentType.ToUpper() == MOBFormofPayment.Masterpass.ToString().ToUpper() || request.FormofPaymentDetails.Masterpass != null)
            {
                //var acquireMasterpassTokenResponse = FilePersist.Load<MOBFOPAcquirePaymentTokenResponse>(request.SessionId, typeof(MOBFOPAcquirePaymentTokenResponse).FullName);
                var acquireMasterpassTokenResponse = await _sessionHelperService.GetSession<MOBFOPAcquirePaymentTokenResponse>(session.SessionId, new MOBFOPAcquirePaymentTokenResponse().ObjectName, new List<string> { session.SessionId, new MOBFOPAcquirePaymentTokenResponse().ObjectName }).ConfigureAwait(false);
                request.FormofPaymentDetails.Masterpass.CslSessionId = acquireMasterpassTokenResponse.CslSessionId;
                formofPaymentDetails = await PersistMasterPassDetails(request, session);
                formofPaymentDetails.FormOfPaymentType = MOBFormofPayment.Masterpass.ToString();
            }
            else if (request.FormofPaymentDetails.FormOfPaymentType.ToUpper() == MOBFormofPayment.ApplePay.ToString().ToUpper())
            {
                formofPaymentDetails.FormOfPaymentType = MOBFormofPayment.ApplePay.ToString();

                if (!_configuration.GetValue<bool>("DisableTravelBankApplePayPayPal") && IncludeTravelBankFOP(request.Application.Id, request.Application.Version.Major))
                {
                    formofPaymentDetails.TravelBankDetails = persistShoppingCart.FormofPaymentDetails?.TravelBankDetails;
                }
            }
            else if (request.FormofPaymentDetails.FormOfPaymentType.ToUpper() == MOBFormofPayment.Uplift.ToString().ToUpper())
            {
                formofPaymentDetails.FormOfPaymentType = MOBFormofPayment.Uplift.ToString();
                formofPaymentDetails.Uplift = await GetCreditCardWithToken(request.FormofPaymentDetails.Uplift, session, request);
                formofPaymentDetails.EmailAddress = request.FormofPaymentDetails.EmailAddress;
                formofPaymentDetails.BillingAddress = await GetBillingAddressWithValidStateCode(request, session);
                formofPaymentDetails.Phone = request.FormofPaymentDetails.Phone;
            }
            else if (request.FormofPaymentDetails.FormOfPaymentType.ToUpper() == MOBFormofPayment.MilesFormOfPayment.ToString().ToUpper() && IsMilesFOPEnabled())
            {
                //var profilePersist = United.Persist.FilePersist.Load<ProfileFOPCreditCardResponse>(request.SessionId, "United.Persist.Definition.FOP.ProfileFOPCreditCardResponse");
                var profilePersist = await _sessionHelperService.GetSession<ProfileFOPCreditCardResponse>(session.SessionId, new ProfileFOPCreditCardResponse().ObjectName, new List<string> { session.SessionId, new ProfileFOPCreditCardResponse().ObjectName }).ConfigureAwait(false);
                if (profilePersist != null && profilePersist.Response != null && profilePersist.Response.Profiles != null && profilePersist.Response.Profiles.Count > 0 && profilePersist.Response.Profiles[0].Travelers != null &&
                    profilePersist.Response.Profiles[0].Travelers.Count > 0 && profilePersist.Response.Profiles[0].Travelers[0].MileagePlus != null)
                {
                    var MilagePlus = profilePersist.Response.Profiles[0].Travelers[0].MileagePlus;
                    var mileFOP = new MOBMilesFOP();
                    mileFOP.Name = new MOBName()
                    {
                        First = profilePersist.Response.Profiles[0].Travelers[0].FirstName,
                        Last = profilePersist.Response.Profiles[0].Travelers[0].LastName,
                        Middle = profilePersist.Response.Profiles[0].Travelers[0].MiddleName,
                        Suffix = profilePersist.Response.Profiles[0].Travelers[0].Suffix,
                        Title = profilePersist.Response.Profiles[0].Travelers[0].Title
                    };
                    mileFOP.ProfileOwnerMPAccountNumber = profilePersist.Response.MileagePlusNumber;
                    mileFOP.CustomerId = profilePersist.Response.Profiles[0].ProfileId;
                    mileFOP.RequiredMiles = Convert.ToInt32(persistShoppingCart.TotalMiles);
                    mileFOP.AvailableMiles = MilagePlus.AccountBalance;
                    mileFOP.HasEnoughMiles = mileFOP.AvailableMiles >= mileFOP.RequiredMiles ? true : false;
                    mileFOP.DisplayRequiredMiles = formatAwardAmountForDisplay(mileFOP.RequiredMiles.ToString(), false);
                    mileFOP.DisplayAvailableMiles = formatAwardAmountForDisplay(mileFOP.AvailableMiles.ToString(), false);
                    if (!mileFOP.HasEnoughMiles)
                    {
                        persistShoppingCart.AlertMessages = new List<MOBSection>();
                        persistShoppingCart.AlertMessages.Add(new MOBSection()
                        {
                            Text1 = "Insufficient miles for purchase",
                            Text2 = "You don't have enough miles to complete this purchase. Please select a different payment method and try again."
                        });
                    }
                    formofPaymentDetails.MilesFOP = mileFOP;
                    formofPaymentDetails.FormOfPaymentType = MOBFormofPayment.MilesFormOfPayment.ToString();
                }
            }
            if (IncludeTravelCredit(request.Application.Id, request.Application.Version.Major))
            {
                formofPaymentDetails.TravelCreditDetails = persistShoppingCart.FormofPaymentDetails?.TravelCreditDetails;
            }
            if (formofPaymentDetails?.FormOfPaymentType != MOBFormofPayment.MilesFOP.ToString()) {
                if (persistShoppingCart.DisplayMessage != null)
                {
                    persistShoppingCart.DisplayMessage = persistShoppingCart.DisplayMessage.Where(x => x.Text1 != "Not enough miles").ToList();
                }
                formofPaymentDetails.MilesFOP = null;
            }
            if (persistShoppingCart?.FormofPaymentDetails?.MilesFOP?.IsMilesFOPEligible != null)
            {
                formofPaymentDetails.MilesFOP = formofPaymentDetails?.MilesFOP == null ?
                    new MOBMilesFOP() { IsMilesFOPEligible = persistShoppingCart.FormofPaymentDetails.MilesFOP.IsMilesFOPEligible } : formofPaymentDetails?.MilesFOP;
            }
            if (persistShoppingCart == null)
                throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            persistShoppingCart.FormofPaymentDetails = formofPaymentDetails;
            persistShoppingCart.CartId = request.CartId;
            persistShoppingCart.TotalPrice = String.Format("{0:0.00}", request.Amount);
            if (!persistShoppingCart.Products.Any(x => x.Code == "BAG"))//needed for bags since we have currency code as well
                persistShoppingCart.DisplayTotalPrice = Decimal.Parse(request.Amount).ToString("c");

            bool isEnableMFOPBags = await _featureSettings.GetFeatureSettingValue("EnableMfopForBags").ConfigureAwait(false)
                                       && GeneralHelper.IsApplicationVersionGreaterorEqual(request.Application.Id, request.Application.Version.Major, _configuration.GetValue<string>("AndroidMilesFopBagsVersion"), _configuration.GetValue<string>("iPhoneMilesFopBagsVersion"));
            if (isEnableMFOPBags
                && (persistShoppingCart.Products?.Any(p => p.Code == "BAG") ?? false)
                && persistShoppingCart.FormofPaymentDetails?.FormOfPaymentType != MOBFormofPayment.MilesFOP.ToString()
                && (persistShoppingCart.TermsAndConditions?.Any(t => t.ContentKey == "MilesFOP_TandC") ?? false))
            {
                persistShoppingCart.TermsAndConditions.Remove(persistShoppingCart.TermsAndConditions.First(t => t.ContentKey == "MilesFOP_TandC"));
            }

            await _sessionHelperService.SaveSession<MOBShoppingCart>(persistShoppingCart, session.SessionId, new List<string> { session.SessionId, persistShoppingCart.ObjectName }, persistShoppingCart.ObjectName).ConfigureAwait(false);

            response.ShoppingCart = persistShoppingCart;
            return response;
        }

        private string formatAwardAmountForDisplay(string amt, bool truncate = true)
        {
            string newAmt = amt;
            decimal amount = 0;
            decimal.TryParse(amt, out amount);

            try
            {
                if (amount > -1)
                {
                    if (truncate)
                    {
                        //int newTempAmt = (int)decimal.Ceiling(amount);
                        try
                        {
                            if (amount > 999)
                            {
                                amount = amount / 1000;
                                if (amount % 1 > 0)
                                    newAmt = string.Format("{0:n1}", amount) + "K miles";
                                else
                                    newAmt = string.Format("{0:n0}", amount) + "K miles";
                            }
                            else
                            {
                                newAmt = string.Format("{0:n0}", amount) + " miles";
                            }
                        }
                        catch { }
                    }
                    else
                    {
                        try
                        {
                            newAmt = string.Format("{0:n0}", amount) + " miles";
                        }
                        catch { }
                    }
                }

            }
            catch { }

            return newAmt;
        }

        private bool IsMilesFOPEnabled()
        {
            Boolean isMilesFOP;
            Boolean.TryParse(_configuration.GetValue<string>("EnableMilesAsPayment"), out isMilesFOP);
            return isMilesFOP;
        }

        private async Task<MOBFormofPaymentDetails> PersistMasterPassDetails(MOBPersistFormofPaymentRequest request, Session session)
        {
            MOBFormofPaymentDetails response = new MOBFormofPaymentDetails();
            string token = session.Token;

            string path = "/MasterPassWallet/GetMasterPassSessionDetails";
            United.Service.Presentation.PaymentModel.Wallet wallet = BuildWalletObject(request.FormofPaymentDetails.Masterpass, request.CartId);

            string jsonRequest = JsonConvert.SerializeObject(wallet);

            var jsonResponse = await _paymentService.GetEligibleFormOfPayments(token, path, jsonRequest, session.SessionId);

            response.MasterPassSessionDetails = await DeserialiseAndBuildMOBMasterpassSessionDetailsFOP(jsonResponse);
            response.Masterpass = request.FormofPaymentDetails.Masterpass;
            return response;
        }

        private async Task<MOBMasterpassSessionDetails> DeserialiseAndBuildMOBMasterpassSessionDetailsFOP(string jsonResponse)
        {
            MOBMasterpassSessionDetails response = new MOBMasterpassSessionDetails();

            if (!string.IsNullOrEmpty(jsonResponse))
            {
                var walletSessionResponse = JsonConvert.DeserializeObject<WalletSessionResponse>(jsonResponse);
                if (walletSessionResponse != null && walletSessionResponse.Response != null && walletSessionResponse.Response.Message != null && walletSessionResponse.Response.Message[0].Text.ToUpper().Equals("SUCCESSFUL"))
                {
                    var payment = walletSessionResponse.Payment;
                    CreditCard cc = (CreditCard)walletSessionResponse.Payment;
                    response.AccountNumber = payment.AccountNumber;
                    response.AccountNumberEncrypted = payment.AccountNumberEncrypted;
                    response.AccountNumberHMAC = payment.AccountNumberHMAC;
                    response.AccountNumberLastFourDigits = payment.AccountNumberLastFourDigits;
                    response.AccountNumberMasked = payment.AccountNumberMasked;
                    response.AccountNumberToken = payment.AccountNumberToken;
                    response.ExpirationDate = cc.ExpirationDate;
                    response.Amount = payment.Amount;
                    response.OperationID = payment.OperationID;
                    response.GivenName = payment.Payor.GivenName;
                    response.SurName = payment.Payor.Surname;
                    response.Code = cc.Code;
                    response.CreditCardTypeCode = Convert.ToInt32(cc.CreditCardTypeCode);
                    response.Description = Convert.ToInt32(cc.Description);
                    response.Name = cc.Name;
                    bool masterPassCheckCountryNameToggle = Convert.ToBoolean(_configuration.GetValue<string>("MasterPassCheckCountryNameToggle") ?? "false");
                    if (masterPassCheckCountryNameToggle &&
                        payment.BillingAddress != null &&
                        payment.BillingAddress.Country != null)
                    {
                        payment.BillingAddress.Country.CountryCode = string.Empty;
                    }
                    response.BillingAddress = ConvertCslBillingAddressToMOBAddress(payment.BillingAddress, MOBFormofPayment.Masterpass);
                    response.ContactPhoneNumber =
                        GetKeyValueFromAddressCharacteristicCollection(payment.BillingAddress, "PHONE");
                    response.ContactEmailAddress =
                        GetKeyValueFromAddressCharacteristicCollection(payment.BillingAddress, "EMAIL");
                    response.MasterpassType = new MOBMasterpassType();
                    response.MasterpassType.DefaultIndicator = payment.Type.DefaultIndicator;
                    response.MasterpassType.Description = payment.Type.Description;
                    response.MasterpassType.Key = payment.Type.Key;
                    response.MasterpassType.Val = payment.Type.Value;
                    //MOBILE-1683/MOBILE-1669/MOBILE-1671: PA,PB,PCU- Masterpass : Richa
                    MOBVormetricKeys vormetricKeys = await _checkoutUtility.AssignPersistentTokenToCC(payment.AccountNumberToken, payment.PersistentToken, string.Empty, response.Code, walletSessionResponse.Response.TransactionID, "DeserialiseAndBuildMOBMasterpassSessionDetailsFOP", 0, "");
                    if (!string.IsNullOrEmpty(vormetricKeys.PersistentToken))
                    {
                        response.PersistentToken = vormetricKeys.PersistentToken;
                    }
                    if (!string.IsNullOrEmpty(vormetricKeys.CardType) && string.IsNullOrEmpty(response.Code))
                    {
                        response.Code = vormetricKeys.CardType;
                    }
                }
                else
                {
                    if (walletSessionResponse != null) ThrowExceptionAsPerErrorResponse(walletSessionResponse.Response.Error);
                }
            }
            else
            {
                throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }

            return response;
        }

        private United.Service.Presentation.PaymentModel.Wallet BuildWalletObject(MOBMasterpass masterpass, string cartId)
        {
            United.Service.Presentation.PaymentModel.Wallet wallet = new Service.Presentation.PaymentModel.Wallet();
            wallet.CartID = cartId;
            wallet.CheckoutURL = masterpass.CheckoutResourceURL;
            wallet.OathToken = masterpass.OauthToken;
            wallet.OathVerifier = masterpass.Oauth_verifier;
            wallet.PointOfSale = _configuration.GetValue<string>("AcquireMasterPassToken-PointOfSale");
            wallet.WalletType = "MPS";
            wallet.SessionID = masterpass.CslSessionId;
            wallet.Version = "1.0";
            return wallet;
        }

        private async Task<MOBFormofPaymentDetails> PersistPayPalDetails(MOBPersistFormofPaymentRequest request, Session session)
        {
            MOBFormofPaymentDetails response = new MOBFormofPaymentDetails();
            var payPalResponse = new Service.Presentation.PaymentModel.PayPal();

            var payPalRequest = new Service.Presentation.PaymentModel.PayPal();
            payPalRequest.Amount = Convert.ToDouble(request.Amount);
            payPalRequest.BillingAddress = new Service.Presentation.CommonModel.Address();
            payPalRequest.BillingAddress.Country = new Service.Presentation.CommonModel.Country() { CountryCode = request.FormofPaymentDetails.PayPal.BillingAddressCountryCode }; // Make sure the country code here for AcquirePayPalCreditCard is the same from Billing address counrty code or some thing differenct.
            payPalRequest.PayerID = request.FormofPaymentDetails.PayPal.PayerID;
            payPalRequest.TokenID = request.FormofPaymentDetails.PayPal.PayPalTokenID;
            #region
            string path = string.Format("{0}{1}", "/PayPal/GetPayPalCustomerDetails/", request.CartId);
            string jsonRequest = JsonConvert.SerializeObject(payPalRequest);
            string jsonResponse = await _paymentService.GetEligibleFormOfPayments(session.Token, path, jsonRequest, session.SessionId);

            if (!string.IsNullOrEmpty(jsonResponse))
            {
                #region
                payPalResponse = JsonConvert.DeserializeObject<PayPal>(jsonResponse);

                if (payPalResponse != null)
                {
                    #region Populate values from paypal creditcard response
                    response.PayPalPayor = new MOBPayPalPayor();
                    response.PayPalPayor.PayPallCustomerID = payPalResponse.Payor.CustomerID;
                    response.PayPalPayor.PayPalGivenName = payPalResponse.Payor.GivenName;
                    response.PayPalPayor.PayPalStatus = payPalResponse.Payor.Status.Description;
                    response.PayPalPayor.PayPalContactPhoneNumber = GetKeyValueFromAddressCharacteristicCollection(payPalResponse.BillingAddress, "PHONE");
                    response.PayPalPayor.PayPalBillingAddress = ConvertCslBillingAddressToMOBAddress(payPalResponse.BillingAddress, MOBFormofPayment.PayPal);
                    response.PayPalPayor.PayPalContactEmailAddress = payPalResponse.Payor.Contact.Emails.Select(x => x.Address).FirstOrDefault();
                    response.PayPal = request.FormofPaymentDetails.PayPal;
                    #endregion 
                }
                else
                {
                    throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                }
                #endregion
            }
            else
            {
                throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }
            #endregion

            return response;
        }

        private MOBAddress ConvertCslBillingAddressToMOBAddress(United.Service.Presentation.CommonModel.Address cslAddress, MOBFormofPayment fop)
        {
            MOBAddress mobAddress = new MOBAddress();
            if (cslAddress != null)
            {
                foreach (string addressLines in cslAddress.AddressLines)
                {
                    if (string.IsNullOrEmpty(mobAddress.Line1))
                    {
                        mobAddress.Line1 = addressLines;
                    }
                    else if (string.IsNullOrEmpty(mobAddress.Line2))
                    {
                        mobAddress.Line2 = addressLines;

                    }
                    else if (string.IsNullOrEmpty(mobAddress.Line3))
                    {
                        mobAddress.Line3 = addressLines;

                    }
                    else
                    {
                        mobAddress.Line3 = mobAddress.Line3 + addressLines;
                    }
                }

                mobAddress.Country = new MOBCountry();
                mobAddress.Country.Code = cslAddress.Country.CountryCode;
                mobAddress.Country.Name = cslAddress.Country.Name;
                mobAddress.City = cslAddress.City;
                mobAddress.PostalCode = cslAddress.PostalCode;
                mobAddress.State = new MOBState();

                if (fop == MOBFormofPayment.Masterpass)
                {
                    mobAddress.State.Code = !string.IsNullOrEmpty(cslAddress.StateProvince.Name) ? cslAddress.StateProvince.Name.ToUpper().Replace("US-", "") : (cslAddress.StateProvince.Name ?? "");
                    mobAddress.State.Name = (cslAddress.StateProvince.Name ?? "").ToUpper();
                }
                else
                {
                    mobAddress.State.Code = cslAddress.StateProvince.ShortName;
                    mobAddress.State.Name = cslAddress.StateProvince.StateProvinceCode;
                }

                bool payPalBillingCountryNotUsaMessageToggle = Convert.ToBoolean(_configuration.GetValue<string>("PayPalBillingCountryNotUSAMessageToggle") ?? "false");
                if (payPalBillingCountryNotUsaMessageToggle && !IsUSACountryAddress(mobAddress.Country))
                {
                    throw new MOBUnitedException(_configuration.GetValue<string>("PayPalBillingCountryNotUSAMessage"));
                }
            }
            else
            {
                throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }
            return mobAddress;
        }

        public bool IsUSACountryAddress(MOBCountry country)
        {
            var billingCountryCodes = _configuration.GetValue<string>("BillingCountryCodes") ?? "";
            bool isUSAAddress = false;
            if (!string.IsNullOrEmpty(billingCountryCodes) && country != null && !string.IsNullOrEmpty(country.Code))
            {
                var countryCodes = billingCountryCodes.Split('|').ToList();
                isUSAAddress = countryCodes.Exists(p => p.Split('~')[0].ToUpper() == country.Code.Trim().ToUpper());
            }
            else if (!string.IsNullOrEmpty(billingCountryCodes) && country != null && !string.IsNullOrEmpty(country.Name))
            {
                var countryCodes = billingCountryCodes.Split('|').ToList();
                foreach (string coutryCode in countryCodes)
                {
                    if (coutryCode.Split('~')[1].ToUpper() == country.Name.Trim().ToUpper())
                    {
                        isUSAAddress = true;
                        country.Code = coutryCode.Split('~')[0].ToUpper();
                    }
                }
                //isUSAAddress = countryCodes.Exists(p => p.Split('~')[1].ToUpper() == country.Name.Trim().ToUpper());
            }
            return isUSAAddress;
        }

        private string GetKeyValueFromAddressCharacteristicCollection(Service.Presentation.CommonModel.Address address, string key)
        {
            string phoneNumber = string.Empty;
            if (address != null &&
                address.Characteristic != null &&
                address.Characteristic.Count > 0)
            {
                foreach (var characterstic in address.Characteristic)
                {
                    if (characterstic != null)
                    {
                        if (((characterstic.Description ?? "").ToUpper() == key || (characterstic.Code ?? "").ToUpper() == key) &&
                             key == "PHONE" &&
                             !string.IsNullOrEmpty(characterstic.Value))
                        {
                            phoneNumber = characterstic.Value.Replace("-", "");
                        }
                        else if ((characterstic.Description ?? "").ToUpper() == key || (characterstic.Code ?? "").ToUpper() == key)
                        {
                            phoneNumber = characterstic.Value;
                        }
                    }
                }
            }
            return phoneNumber;
        }

        private bool IncludeTravelBankFOP(int appId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnableTravelBankFOP")
                && GeneralHelper.isApplicationVersionGreater
                (appId, appVersion, "AndroidTravelBankFOPVersion", "iPhoneTravelBankFOPVersion", "", "", true, _configuration);
        }

        private async Task<MOBSHOPReservation> GetReservationFromPersist(MOBSHOPReservation reservation, Session session)
        {
            #region
            //United.Persist.Definition.Shopping.Session session = GetShoppingSession(sessionID);
            Reservation bookingPathReservation = new Reservation();
            bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(session.SessionId, bookingPathReservation.ObjectName, new List<string> { session.SessionId, bookingPathReservation.ObjectName }).ConfigureAwait(false);
            return MakeReservationFromPersistReservation(reservation, bookingPathReservation, session);

            #endregion
        }

        private MOBSHOPReservation MakeReservationFromPersistReservation(MOBSHOPReservation reservation, Reservation bookingPathReservation,
            Session session)
        {
            reservation = new MOBSHOPReservation(_configuration, _cachingService);
            reservation.CartId = bookingPathReservation.CartId;
            reservation.PointOfSale = bookingPathReservation.PointOfSale;
            reservation.ClubPassPurchaseRequest = bookingPathReservation.ClubPassPurchaseRequest;
            reservation.CreditCards = bookingPathReservation.CreditCards;
            reservation.CreditCardsAddress = bookingPathReservation.CreditCardsAddress;
            reservation.FareLock = bookingPathReservation.FareLock;
            reservation.FareRules = bookingPathReservation.FareRules;
            reservation.IsSignedInWithMP = bookingPathReservation.IsSignedInWithMP;
            reservation.NumberOfTravelers = bookingPathReservation.NumberOfTravelers;
            reservation.Prices = bookingPathReservation.Prices;
            reservation.SearchType = bookingPathReservation.SearchType;
            reservation.SeatPrices = bookingPathReservation.SeatPrices;
            reservation.SessionId = session.SessionId;
            reservation.Taxes = bookingPathReservation.Taxes;
            reservation.UnregisterFareLock = bookingPathReservation.UnregisterFareLock;
            reservation.AwardTravel = bookingPathReservation.AwardTravel;
            reservation.LMXFlights = bookingPathReservation.LMXFlights;
            reservation.IneligibleToEarnCreditMessage = bookingPathReservation.IneligibleToEarnCreditMessage;
            reservation.OaIneligibleToEarnCreditMessage = bookingPathReservation.OaIneligibleToEarnCreditMessage;
            reservation.SeatPrices = bookingPathReservation.SeatPrices;
            reservation.IsBookingCommonFOPEnabled = bookingPathReservation.IsBookingCommonFOPEnabled;
            reservation.IsReshopCommonFOPEnabled = bookingPathReservation.IsReshopCommonFOPEnabled;
            if (bookingPathReservation.TravelersCSL != null && bookingPathReservation.TravelerKeys != null)
            {
                List<MOBCPTraveler> lstTravelers = new List<MOBCPTraveler>();
                foreach (string travelerKey in bookingPathReservation.TravelerKeys)
                {
                    lstTravelers.Add(bookingPathReservation.TravelersCSL[travelerKey]);
                }
                reservation.TravelersCSL = lstTravelers;
            }
            reservation.TravelersRegistered = bookingPathReservation.TravelersRegistered;
            reservation.TravelOptions = bookingPathReservation.TravelOptions;
            reservation.Trips = bookingPathReservation.Trips;
            reservation.ReservationPhone = bookingPathReservation.ReservationPhone;
            reservation.ReservationEmail = bookingPathReservation.ReservationEmail;
            reservation.FlightShareMessage = bookingPathReservation.FlightShareMessage;
            reservation.PKDispenserPublicKey = bookingPathReservation.PKDispenserPublicKey;
            reservation.IsRefundable = bookingPathReservation.IsRefundable;
            reservation.ISInternational = bookingPathReservation.ISInternational;
            reservation.ISFlexibleSegmentExist = bookingPathReservation.ISFlexibleSegmentExist;
            reservation.ClubPassPurchaseRequest = bookingPathReservation.ClubPassPurchaseRequest;
            reservation.GetALLSavedTravelers = bookingPathReservation.GetALLSavedTravelers;
            reservation.IsELF = bookingPathReservation.IsELF;
            reservation.IsSSA = bookingPathReservation.IsSSA;
            reservation.IsMetaSearch = bookingPathReservation.IsMetaSearch;
            reservation.MetaSessionId = bookingPathReservation.MetaSessionId;
            reservation.IsUpgradedFromEntryLevelFare = bookingPathReservation.IsUpgradedFromEntryLevelFare;
            reservation.SeatAssignmentMessage = bookingPathReservation.SeatAssignmentMessage;
            if (bookingPathReservation.TCDAdvisoryMessages != null && bookingPathReservation.TCDAdvisoryMessages.Count > 0)
            {
                reservation.TCDAdvisoryMessages = bookingPathReservation.TCDAdvisoryMessages;
            }
            //##Price Break Down - Kirti
            if (_configuration.GetValue<string>("EnableShopPriceBreakDown") != null &&
                Convert.ToBoolean(_configuration.GetValue<string>("EnableShopPriceBreakDown").ToString()))
            {
                reservation.ShopPriceBreakDown = GetPriceBreakDown(bookingPathReservation);
            }

            if (session != null && !string.IsNullOrEmpty(session.EmployeeId) && reservation != null)
            {
                reservation.IsEmp20 = true;
            }
            if (bookingPathReservation.IsCubaTravel)
            {
                reservation.IsCubaTravel = bookingPathReservation.IsCubaTravel;
                reservation.CubaTravelInfo = bookingPathReservation.CubaTravelInfo;
            }
            reservation.FormOfPaymentType = bookingPathReservation.FormOfPaymentType;
            if (bookingPathReservation.FormOfPaymentType == MOBFormofPayment.PayPal || bookingPathReservation.FormOfPaymentType == MOBFormofPayment.PayPalCredit)
            {
                reservation.PayPal = bookingPathReservation.PayPal;
                reservation.PayPalPayor = bookingPathReservation.PayPalPayor;
            }
            if (bookingPathReservation.FormOfPaymentType == MOBFormofPayment.Masterpass)
            {
                if (bookingPathReservation.MasterpassSessionDetails != null)
                    reservation.MasterpassSessionDetails = bookingPathReservation.MasterpassSessionDetails;
                if (bookingPathReservation.Masterpass != null)
                    reservation.Masterpass = bookingPathReservation.Masterpass;
            }
            if (bookingPathReservation.FOPOptions != null && bookingPathReservation.FOPOptions.Count > 0) //FOP Options Fix Venkat 12/08
            {
                reservation.FOPOptions = bookingPathReservation.FOPOptions;
            }

            if (bookingPathReservation.IsReshopChange)
            {
                reservation.ReshopTrips = bookingPathReservation.ReshopTrips;
                reservation.Reshop = bookingPathReservation.Reshop;
                reservation.IsReshopChange = true;
            }
            reservation.ELFMessagesForRTI = bookingPathReservation.ELFMessagesForRTI;
            if (bookingPathReservation.ShopReservationInfo != null)
            {
                reservation.ShopReservationInfo = bookingPathReservation.ShopReservationInfo;
            }
            if (bookingPathReservation.ShopReservationInfo2 != null)
            {
                reservation.ShopReservationInfo2 = bookingPathReservation.ShopReservationInfo2;
            }

            if (bookingPathReservation.ReservationEmail != null)
            {
                reservation.ReservationEmail = bookingPathReservation.ReservationEmail;
            }

            if (bookingPathReservation.TripInsuranceFile != null && bookingPathReservation.TripInsuranceFile.TripInsuranceBookingInfo != null)
            {
                reservation.TripInsuranceInfoBookingPath = bookingPathReservation.TripInsuranceFile.TripInsuranceBookingInfo;
            }
            else
            {
                reservation.TripInsuranceInfoBookingPath = null;
            }
            reservation.AlertMessages = bookingPathReservation.AlertMessages;
            reservation.IsRedirectToSecondaryPayment = bookingPathReservation.IsRedirectToSecondaryPayment;
            reservation.RecordLocator = bookingPathReservation.RecordLocator;
            reservation.Messages = bookingPathReservation.Messages;
            reservation.CheckedbagChargebutton = bookingPathReservation.CheckedbagChargebutton;
            _eTCUtility.SetEligibilityforETCTravelCredit(reservation, session, bookingPathReservation);
            return reservation;
        }

        private MOBSHOPTripPriceBreakDown GetPriceBreakDown(Reservation reservation)
        {
            //##Price Break Down - Kirti
            var priceBreakDownObj = new MOBSHOPTripPriceBreakDown();
            bool hasAward = false;
            string awardPrice = string.Empty;
            string basePrice = string.Empty;
            string totalPrice = string.Empty;
            bool hasOneTimePass = false;
            string oneTimePassCost = string.Empty;
            bool hasFareLock = false;
            double awardPriceValue = 0;
            double basePriceValue = 0;

            if (reservation != null)
            {
                priceBreakDownObj.PriceBreakDownDetails = new MOBSHOPPriceBreakDownDetails();
                priceBreakDownObj.PriceBreakDownSummary = new MOBSHOPPriceBreakDownSummary();

                foreach (var travelOption in reservation.TravelOptions)
                {
                    if (travelOption.Key.Equals("FareLock"))
                    {
                        hasFareLock = true;

                        priceBreakDownObj.PriceBreakDownDetails.FareLock = new List<MOBSHOPPriceBreakDown2Items>();
                        priceBreakDownObj.PriceBreakDownSummary.FareLock = new MOBSHOPPriceBreakDown2Items();
                        var fareLockAmount = new MOBSHOPPriceBreakDown2Items();
                        foreach (var subItem in travelOption.SubItems)
                        {
                            if (subItem.Key.Equals("FareLockHoldDays"))
                            {
                                fareLockAmount.Text1 = string.Format("{0} {1}", subItem.Amount, "Day FareLock");
                            }
                        }
                        //Row 0 Column 0
                        fareLockAmount.Price1 = travelOption.DisplayAmount;
                        priceBreakDownObj.PriceBreakDownDetails.FareLock.Add(fareLockAmount);
                        priceBreakDownObj.PriceBreakDownSummary.FareLock = fareLockAmount;

                        priceBreakDownObj.PriceBreakDownDetails.FareLock.Add(new MOBSHOPPriceBreakDown2Items() { Text1 = "Total due now" });
                        //Row 1 Column 0
                    }
                }

                StringBuilder tripType = new StringBuilder();
                if (reservation.SearchType.Equals("OW"))
                {
                    tripType.Append("Oneway");
                }
                else if (reservation.SearchType.Equals("RT"))
                {
                    tripType.Append("Roundtrip");
                }
                else
                {
                    tripType.Append("MultipleTrip");
                }
                tripType.Append(" (");
                tripType.Append(reservation.NumberOfTravelers);
                tripType.Append(reservation.NumberOfTravelers > 1 ? " travelers)" : " traveler)");
                //row 2 coulum 0

                foreach (var price in reservation.Prices)
                {
                    switch (price.DisplayType)
                    {
                        case "MILES":
                            hasAward = true;
                            awardPrice = price.FormattedDisplayValue;
                            awardPriceValue = price.Value;
                            break;

                        case "TRAVELERPRICE":
                            basePrice = price.FormattedDisplayValue;
                            basePriceValue = price.Value;
                            break;

                        case "TOTAL":
                            totalPrice = price.FormattedDisplayValue;
                            break;

                        case "ONE-TIME PASS":
                            hasOneTimePass = true;
                            oneTimePassCost = price.FormattedDisplayValue;
                            break;

                        case "GRAND TOTAL":
                            if (!hasFareLock)
                                totalPrice = price.FormattedDisplayValue;
                            break;
                    }
                }

                string travelPrice = string.Empty;
                double travelPriceValue = 0;
                //row 2 column 1
                if (hasAward)
                {
                    travelPrice = awardPrice;
                    travelPriceValue = awardPriceValue;
                }
                else
                {
                    travelPrice = basePrice;
                    travelPriceValue = basePriceValue;
                }

                priceBreakDownObj.PriceBreakDownDetails.Trip = new MOBSHOPPriceBreakDown2Items() { Text1 = tripType.ToString(), Price1 = travelPrice };

                priceBreakDownObj.PriceBreakDownSummary.TravelOptions = new List<MOBSHOPPriceBreakDown2Items>();

                decimal taxNfeesTotal = 0;
                BuildTaxesAndFees(reservation, priceBreakDownObj, out taxNfeesTotal);

                if (((reservation.SeatPrices != null && reservation.SeatPrices.Count > 0) ||
                    reservation.TravelOptions != null && reservation.TravelOptions.Count > 0 || hasOneTimePass) && !hasFareLock)
                {
                    priceBreakDownObj.PriceBreakDownDetails.AdditionalServices = new MOBSHOPPriceBreakDownAddServices();

                    // Row n+ 5 column 0
                    // Row n+ 5 column 1

                    priceBreakDownObj.PriceBreakDownDetails.AdditionalServices.Seats = new List<MOBSHOPPriceBreakDown4Items>();
                    priceBreakDownObj.PriceBreakDownDetails.AdditionalServices.Seats.Add(new MOBSHOPPriceBreakDown4Items() { Text1 = "Additional services:" });

                    BuildSeatPrices(reservation, priceBreakDownObj);

                    BuildTravelOptions(reservation, priceBreakDownObj);
                }

                if (hasOneTimePass)
                {
                    priceBreakDownObj.PriceBreakDownDetails.AdditionalServices.OneTimePass = new List<MOBSHOPPriceBreakDown2Items>();
                    priceBreakDownObj.PriceBreakDownDetails.AdditionalServices.OneTimePass.Add(new MOBSHOPPriceBreakDown2Items() { Text1 = "One-Time Pass", Price1 = oneTimePassCost });

                    priceBreakDownObj.PriceBreakDownSummary.TravelOptions.Add(new MOBSHOPPriceBreakDown2Items() { Text1 = "One-Time Pass", Price1 = oneTimePassCost });
                }

                var finalPriceSummary = new MOBSHOPPriceBreakDown2Items();

                priceBreakDownObj.PriceBreakDownDetails.Total = new List<MOBSHOPPriceBreakDown2Items>();
                priceBreakDownObj.PriceBreakDownSummary.Total = new List<MOBSHOPPriceBreakDown2Items>();
                if (hasFareLock)
                {
                    //column 0
                    finalPriceSummary.Text1 = "Total price (held)";
                }
                else
                {
                    //  buildDottedLine(); column 1
                    finalPriceSummary.Text1 = "Total price";
                }
                if (hasAward)
                {
                    //colum 1
                    finalPriceSummary.Price1 = awardPrice;
                    priceBreakDownObj.PriceBreakDownDetails.Total.Add(finalPriceSummary);

                    priceBreakDownObj.PriceBreakDownSummary.Total.Add(new MOBSHOPPriceBreakDown2Items() { Price1 = string.Format("+{0}", totalPrice) });

                    priceBreakDownObj.PriceBreakDownSummary.Trip = new List<MOBSHOPPriceBreakDown2Items>()
                             {
                                 new MOBSHOPPriceBreakDown2Items()
                                 {
                                    Text1 = tripType.ToString(), Price1 = string.Format("${0}", taxNfeesTotal.ToString("F"))
                                 }
                             };
                }
                else
                {
                    //column 1
                    finalPriceSummary.Price1 = totalPrice;
                    priceBreakDownObj.PriceBreakDownDetails.Total.Add(new MOBSHOPPriceBreakDown2Items() { Text1 = totalPrice });

                    priceBreakDownObj.PriceBreakDownSummary.Trip = new List<MOBSHOPPriceBreakDown2Items>()
                             {
                                new MOBSHOPPriceBreakDown2Items()
                                {
                                  Text1 = tripType.ToString(), Price1 = string.Format("${0}", (travelPriceValue + Convert.ToDouble(taxNfeesTotal)).ToString("F"))
                                }
                             };
                }

                priceBreakDownObj.PriceBreakDownSummary.Total.Add(finalPriceSummary);
            }

            return priceBreakDownObj;
        }

        private void BuildTaxesAndFees(Reservation reservation, MOBSHOPTripPriceBreakDown priceBreakDownObj, out decimal taxNfeesTotal)
        {
            taxNfeesTotal = 0;
            priceBreakDownObj.PriceBreakDownDetails.TaxAndFees = new List<MOBSHOPPriceBreakDown2TextItems>
            {
                new MOBSHOPPriceBreakDown2TextItems(), // blank line

                new MOBSHOPPriceBreakDown2TextItems() { Text1 = "Taxes and fees:", Text2 = (reservation.Taxes != null && reservation.Taxes[0] != null) ? reservation.Taxes[0].TaxCodeDescription : string.Empty }
            };

            foreach (var tax in reservation.Taxes)
            {
                if (!tax.TaxCode.Equals("PERPERSONTAX"))
                {
                    if (!tax.TaxCode.Equals("TOTALTAX"))
                    {
                        // Row n+ 4 column 0
                        // Row n+ 4 column 1
                        priceBreakDownObj.PriceBreakDownDetails.TaxAndFees.Add(new MOBSHOPPriceBreakDown2TextItems() { Text1 = string.Format("{0} {1}", tax.TaxCodeDescription, tax.DisplayAmount) });
                    }
                    if (tax.TaxCode.Equals("TOTALTAX"))
                    {
                        priceBreakDownObj.PriceBreakDownDetails.TaxAndFees.Add(new MOBSHOPPriceBreakDown2TextItems()); // blank line
                        priceBreakDownObj.PriceBreakDownDetails.TaxAndFees.Add(new MOBSHOPPriceBreakDown2TextItems() { Text1 = tax.TaxCodeDescription, Price1 = tax.DisplayAmount });
                        priceBreakDownObj.PriceBreakDownDetails.TaxAndFees.Add(new MOBSHOPPriceBreakDown2TextItems()); // blank line

                        taxNfeesTotal = tax.Amount;
                    }
                }
            }
        }

        private void BuildSeatPrices(Reservation reservation, MOBSHOPTripPriceBreakDown priceBreakDownObj)
        {
            if (reservation.SeatPrices != null && reservation.SeatPrices.Count > 0)
            {
                // double travelTotalPrice = 0.0;
                // Row n+ 6 column 0
                // Row n+ 6 column 1
                var seatPriceSum = reservation.SeatPrices.Sum(a => a.DiscountedTotalPrice);

                priceBreakDownObj.PriceBreakDownDetails.AdditionalServices.Seats.Add(new MOBSHOPPriceBreakDown4Items() { Text1 = "Economy Plus® Seats", Price3 = string.Format("${0}", seatPriceSum.ToString("F")) });

                string economyPlusSeatText = string.Empty;

                foreach (var seat in reservation.SeatPrices)
                {
                    if (seat.SeatMessage.Trim().Contains("limited recline"))
                    {
                        economyPlusSeatText = seat.SeatMessage;
                    }
                    else
                    {
                        economyPlusSeatText = "Economy Plus Seat";
                    }

                    // Row n+m+ 7 column 0
                    // Row n+ 7 column 1

                    priceBreakDownObj.PriceBreakDownDetails.AdditionalServices.Seats.Add(new MOBSHOPPriceBreakDown4Items() { Text1 = string.Format("{0} - {1}", seat.Origin, seat.Destination) });
                    priceBreakDownObj.PriceBreakDownDetails.AdditionalServices.Seats.Add(new MOBSHOPPriceBreakDown4Items());//blank

                    // Row n+m+ 8 column 0
                    // Row n+m+ 8 column 1

                    priceBreakDownObj.PriceBreakDownDetails.AdditionalServices.Seats.Add(new MOBSHOPPriceBreakDown4Items()
                    {
                        Text1 = string.Format("{0} {1}", seat.NumberOftravelers, (seat.NumberOftravelers == 1) ? "traveler" : "traveler"),
                        Price1 = seat.TotalPriceDisplayValue,
                        Price2 = seat.DiscountedTotalPriceDisplayValue
                    });

                    priceBreakDownObj.PriceBreakDownDetails.AdditionalServices.Seats.Add(new MOBSHOPPriceBreakDown4Items() { Text1 = seat.SeatMessage });

                    priceBreakDownObj.PriceBreakDownDetails.AdditionalServices.Seats.Add(new MOBSHOPPriceBreakDown4Items());//blank
                }
                priceBreakDownObj.PriceBreakDownSummary.TravelOptions.Add(
                    new MOBSHOPPriceBreakDown2Items()
                    {
                        Text1 = economyPlusSeatText,
                        Price1 = string.Format("${0}", seatPriceSum)

                    });
            }
        }

        private void BuildTravelOptions(Reservation reservation, MOBSHOPTripPriceBreakDown priceBreakDownObj)
        {
            if (reservation.TravelOptions != null && reservation.TravelOptions.Count > 0)
            {
                priceBreakDownObj.PriceBreakDownDetails.AdditionalServices.PremiumAccess = new List<MOBSHOPPriceBreakDown3Items>();

                foreach (var option in reservation.TravelOptions)
                {
                    if (option.Key.Equals("PAS"))
                    {
                        priceBreakDownObj.PriceBreakDownDetails.AdditionalServices.PremiumAccess.Add(new MOBSHOPPriceBreakDown3Items() { Text1 = option.Type, Price2 = option.DisplayAmount });

                        priceBreakDownObj.PriceBreakDownSummary.TravelOptions.Add(new MOBSHOPPriceBreakDown2Items() { Text1 = option.Type, Price1 = option.DisplayAmount });
                        // traveOptionSummaryList.Add(new Definition.Shopping.PriceBreakDown.MOBSHOPPriceBreakDown2Items() { Text1 = option.Type, Price1 = option.DisplayAmount });

                        if (option.SubItems != null && option.SubItems.Count > 0)
                        {
                            foreach (var subOption in option.SubItems)
                            {
                                if (!string.IsNullOrEmpty(subOption.Description) && (subOption.ProductId.Equals("PremierAccessSegment")))
                                {
                                    foreach (var subItemMap in option.SubItems)
                                    {
                                        if (subOption.Value != null && subItemMap.Key != null && subItemMap.Key.Equals(subOption.Value))
                                        {
                                            //column 1
                                            priceBreakDownObj.PriceBreakDownDetails.AdditionalServices.PremiumAccess.Add(new MOBSHOPPriceBreakDown3Items() { Text1 = subOption.Description, Price2 = subItemMap.DisplayAmount });

                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private async Task HandleAncillaryOptionsForUplift(MOBPersistFormofPaymentRequest request, Reservation bookingPathReservation)
        {
            if (!_configuration.GetValue<bool>("EnableUpliftPayment"))
                return;

            if (bookingPathReservation?.ShopReservationInfo2 == null)
                return;

            if (request.FormofPaymentDetails?.FormOfPaymentType == MOBFormofPayment.Uplift.ToString() && request.FormofPaymentDetails?.Uplift != null)
            {
                //need to save chaseAd before removing, we will need to show chaseAd if user changes the form of payment
                if (bookingPathReservation.ShopReservationInfo2.ChaseCreditStatement != null)
                {
                    bookingPathReservation.ShopReservationInfo2.ChaseCreditStatement = null;
                    await _sessionHelperService.SaveSession<MOBCCAdStatement>(bookingPathReservation.ShopReservationInfo2.ChaseCreditStatement, request.SessionId, new List<string> { request.SessionId, new MOBCCAdStatement().GetType().ToString() }, new MOBCCAdStatement().GetType().ToString()).ConfigureAwait(false);
                    //FilePersist.Save(request.SessionId, new MOBCCAdStatement().GetType().ToString(), bookingPathReservation.ShopReservationInfo2.ChaseCreditStatement);
                }
                bookingPathReservation.ShopReservationInfo2.HideTravelOptionsOnRTI = true;
                bookingPathReservation.ShopReservationInfo2.hideSelectSeatsOnRTI = true;
            }
            else
            {
                //need to load chaseAd from persist, and update the offer prices.
                //this is to handle when user choose Uplift as FOP and then changed the fop we need to show chaseAd again.
                if (bookingPathReservation.ShopReservationInfo2.ChaseCreditStatement == null)
                {
                    var chaseCreditStatement = await _sessionHelperService.GetSession<MOBCCAdStatement>(request.SessionId, new MOBCCAdStatement().GetType().ToString(), new List<string> { request.SessionId, new MOBCCAdStatement().GetType().ToString() });
                    //var chaseCreditStatement = FilePersist.Load<MOBCCAdStatement>(request.SessionId, new MOBCCAdStatement().GetType().ToString(), false);
                    if (chaseCreditStatement != null)
                    {
                        var objPrice = bookingPathReservation.Prices.FirstOrDefault(p => p.PriceType.ToUpper().Equals("GRAND TOTAL"));
                        if (objPrice != null)
                        {
                            decimal price = Convert.ToDecimal(objPrice.Value);
                            if (_configuration.GetValue<bool>("TurnOffChaseBugMOBILE-11134"))
                            {
                                chaseCreditStatement.finalAfterStatementDisplayPrice = GetPriceAfterChaseCredit(price);
                            }
                            else
                            {
                                chaseCreditStatement.finalAfterStatementDisplayPrice = GetPriceAfterChaseCredit(price, chaseCreditStatement.statementCreditDisplayPrice);
                            }

                            chaseCreditStatement.initialDisplayPrice = price.ToString("C2", CultureInfo.CurrentCulture);
                            FormatChaseCreditStatemnet(chaseCreditStatement);
                        }
                        bookingPathReservation.ShopReservationInfo2.ChaseCreditStatement = chaseCreditStatement;
                    }
                }
                bookingPathReservation.ShopReservationInfo2.HideTravelOptionsOnRTI = false;
                //bookingPathReservation.ShopReservationInfo2.HideSelectSeatOnRTI = false;
                bookingPathReservation.ShopReservationInfo2.hideSelectSeatsOnRTI = false;

            }

            //Handle trip insurance and uplift payment message
            var showUpliftTpiMessage = ShowUpliftTpiMessage(bookingPathReservation, request.FormofPaymentDetails?.FormOfPaymentType);
            bookingPathReservation.ShopReservationInfo2.InfoWarningMessages = UpdateInfoWarningForUpliftTpi(bookingPathReservation.ShopReservationInfo2.InfoWarningMessages, showUpliftTpiMessage);

        }

        private List<MOBInfoWarningMessages> UpdateInfoWarningForUpliftTpi(List<MOBInfoWarningMessages> infoWarningMessages, bool showUpliftTpiMessage)
        {
            if (!showUpliftTpiMessage)
            {
                if (infoWarningMessages?.Any() ?? false)
                {
                    infoWarningMessages.RemoveAll(i => i.Order == MOBINFOWARNINGMESSAGEORDER.UPLIFTTPISECONDARYPAYMENT.ToString());
                }
                return infoWarningMessages;
            }

            if (infoWarningMessages?.Any() ?? false)
            {
                if (infoWarningMessages?.Any(i => i.Order == MOBINFOWARNINGMESSAGEORDER.UPLIFTTPISECONDARYPAYMENT.ToString()) ?? false)
                {
                    return infoWarningMessages;
                }

                infoWarningMessages.Add(GetTpiUpliftMessage());
                infoWarningMessages.OrderBy(c => (int)(MOBINFOWARNINGMESSAGEORDER)Enum.Parse(typeof(MOBINFOWARNINGMESSAGEORDER), c.Order));
                return infoWarningMessages;
            }

            return new List<MOBInfoWarningMessages> { GetTpiUpliftMessage() };
        }

        private MOBInfoWarningMessages GetTpiUpliftMessage()
        {
            return new MOBInfoWarningMessages
            {
                IconType = MOBINFOWARNINGMESSAGEICON.INFORMATION.ToString(),
                Messages = new List<string> { _configuration.GetValue<string>("UpliftTpiSecondaryPaymentMessage") },
                Order = MOBINFOWARNINGMESSAGEORDER.UPLIFTTPISECONDARYPAYMENT.ToString()
            };
        }

        private bool ShowUpliftTpiMessage(Reservation reservation, string formOfPaymentType)
        {
            return (reservation?.TripInsuranceFile?.TripInsuranceBookingInfo?.IsRegistered ?? false) &&
                   (formOfPaymentType == MOBFormofPayment.Uplift.ToString());
        }

        private void FormatChaseCreditStatemnet(MOBCCAdStatement chaseCreditStatement)
        {
            if (_configuration.GetValue<bool>("UpdateChaseColor16788"))
            {
                chaseCreditStatement.styledInitialDisplayPrice = string.IsNullOrWhiteSpace(chaseCreditStatement.initialDisplayPrice) ? "" : HttpUtility.HtmlDecode(_configuration.GetValue<string>("StyledTextBeginning")) + chaseCreditStatement.initialDisplayPrice + HttpUtility.HtmlDecode(_configuration.GetValue<string>("StyledTextEnding"));
                chaseCreditStatement.styledInitialDisplayText = HttpUtility.HtmlDecode(_configuration.GetValue<string>("StyledTextBeginning")) + _configuration.GetValue<string>("InitialDisplayText") + HttpUtility.HtmlDecode(_configuration.GetValue<string>("StyledTextEnding"));
                chaseCreditStatement.styledStatementCreditDisplayPrice = string.IsNullOrWhiteSpace(chaseCreditStatement.statementCreditDisplayPrice) ? "" : HttpUtility.HtmlDecode(_configuration.GetValue<string>("StyledTextBeginningWithColor")) + HttpUtility.HtmlDecode(_configuration.GetValue<string>("StyledTextStrongBeginning")) + GetPriceAfterChaseCredit(0, chaseCreditStatement.statementCreditDisplayPrice) + HttpUtility.HtmlDecode(_configuration.GetValue<string>("StyledTextStrongEnding")) + HttpUtility.HtmlDecode(_configuration.GetValue<string>("StyledTextEnding"));
                chaseCreditStatement.styledStatementCreditDisplayText = HttpUtility.HtmlDecode(_configuration.GetValue<string>("StyledTextBeginningWithColor")) + HttpUtility.HtmlDecode(_configuration.GetValue<string>("StyledTextStrongBeginning")) + _configuration.GetValue<string>("StatementCreditDisplayText") + HttpUtility.HtmlDecode(_configuration.GetValue<string>("StyledTextStrongEnding")) + HttpUtility.HtmlDecode(_configuration.GetValue<string>("StyledTextEnding"));
                chaseCreditStatement.styledFinalAfterStatementDisplayPrice = string.IsNullOrWhiteSpace(chaseCreditStatement.finalAfterStatementDisplayPrice) ? "" : HttpUtility.HtmlDecode(_configuration.GetValue<string>("StyledTextBeginning")) + chaseCreditStatement.finalAfterStatementDisplayPrice + HttpUtility.HtmlDecode(_configuration.GetValue<string>("StyledTextEnding"));
                chaseCreditStatement.styledFinalAfterStatementDisplayText = HttpUtility.HtmlDecode(_configuration.GetValue<string>("StyledTextBeginning")) + _configuration.GetValue<string>("FinalAfterStatementDisplayText") + HttpUtility.HtmlDecode(_configuration.GetValue<string>("StyledTextEnding"));
            }
        }

        private string GetPriceAfterChaseCredit(decimal price, string chaseCrediAmount)
        {
            int creditAmt = 0;

            int.TryParse(chaseCrediAmount, System.Globalization.NumberStyles.AllowCurrencySymbol | System.Globalization.NumberStyles.AllowDecimalPoint, null, out creditAmt);

            CultureInfo culture = new System.Globalization.CultureInfo("en-US");
            culture.NumberFormat.CurrencyNegativePattern = 1;
            return String.Format(culture, "{0:C}", price - creditAmt);

            //return (Convert.ToDecimal(price - creditAmt)).ToString("C2", CultureInfo.CurrentCulture);
        }

        private string GetPriceAfterChaseCredit(decimal price)
        {
            int creditAmt = (_configuration.GetValue<int>("ChaseStatementCredit"));

            CultureInfo culture = new CultureInfo("en-US");
            culture.NumberFormat.CurrencyNegativePattern = 1;
            return String.Format(culture, "{0:C}", price - creditAmt);

            //return (Convert.ToDecimal(price - creditAmt)).ToString("C2", CultureInfo.CurrentCulture);
        }

        private bool EnableInflightContactlessPayment(int appID, string appVersion, bool isReshop = false)
        {
            return _configuration.GetValue<bool>("EnableInflightContactlessPayment") && !isReshop && GeneralHelper.IsApplicationVersionGreaterorEqual(appID, appVersion, _configuration.GetValue<string>("InflightContactlessPaymentAndroidVersion"), _configuration.GetValue<string>("InflightContactlessPaymentiOSVersion"));
        }

        private bool IncludeTravelCredit(int appId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnableTravelCredit") &&
                   GeneralHelper.isApplicationVersionGreater(appId, appVersion, "AndroidTravelCreditVersion", "iPhoneTravelCreditVersion", "", "", true, _configuration);
        }

        private bool IncludeFFCResidual(int appId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnableFFCResidual") && GeneralHelper.isApplicationVersionGreater(appId, appVersion, "AndroidFFCResidualVersion", "iPhoneFFCResidualVersion", "", "", true, _configuration);
        }


        private async Task<MOBAddress> GetBillingAddressWithValidStateCode(MOBPersistFormofPaymentRequest request, Session session)
        {
            if (request != null && request.FormofPaymentDetails != null && request.FormofPaymentDetails.BillingAddress != null
                && request.FormofPaymentDetails.BillingAddress != null && request.FormofPaymentDetails.BillingAddress.State != null
                && !string.IsNullOrEmpty(request.FormofPaymentDetails.BillingAddress.State.Code?.Trim())
                && (request.FormofPaymentDetails.BillingAddress.Country.Code?.Trim() == "US" || ConfigUtility.IsViewResFlowPaymentSvcEnabled(request.Flow))) // Validate State Code only if it is United States
            {
                string stateCode = string.Empty; //MOBAddress
                var validStateCodeResult = await GetAndValidateStateCode_CFOP(request, session, stateCode);
                if (validStateCodeResult.IsValidSateCode)
                {
                    request.FormofPaymentDetails.BillingAddress.State.Code = validStateCodeResult.stateCode;
                }

                return request.FormofPaymentDetails.BillingAddress;
            }

            return request?.FormofPaymentDetails?.BillingAddress;
        }

        private async Task<(bool IsValidSateCode, string stateCode)> GetAndValidateStateCode_CFOP(MOBPersistFormofPaymentRequest request, Session session, string stateCode)
        {
            bool validStateCode = false;
            #region

            string path = string.Format("/StatesFilter?State={0}&CountryCode={1}&Language={2}", request.FormofPaymentDetails.BillingAddress.State.Code, request.FormofPaymentDetails.BillingAddress.Country.Code, request.LanguageCode);

            var response = await _referencedataService.GetDataGetAsync<List<StateProvince>>(path, session.Token, session.SessionId);
            if (response != null)
            {
                if (response != null && response.Count == 1 && !string.IsNullOrEmpty(response[0].StateProvinceCode))
                {
                    stateCode = response[0].StateProvinceCode;
                    validStateCode = true;
                }
                else
                {
                    string exceptionMessage = _configuration.GetValue<string>("UnableToGetAndValidateStateCode").ToString();
                    throw new MOBUnitedException(exceptionMessage);
                }
            }
            else
            {
                string exceptionMessage = _configuration.GetValue<string>("UnableToGetAndValidateStateCode");
                if (_configuration.GetValue<string>("ReturnActualExceptionMessageBackForTesting") != null && Convert.ToBoolean(_configuration.GetValue<string>("ReturnActualExceptionMessageBackForTesting").ToString()))
                {
                    exceptionMessage = exceptionMessage + " - due to jsonResponse is empty at DAL  GetCommonUsedDataList()";
                }
                throw new MOBUnitedException(exceptionMessage);
            }
            #endregion
            return (validStateCode, stateCode);
        }


        private async Task<MOBCreditCard> GetCreditCardWithToken(MOBCreditCard creditCard, Session session, MOBRequest mobRequest)
        {
            if (!string.IsNullOrEmpty(creditCard.EncryptedCardNumber))
            {
                #region
                string ccDataVaultToken = string.Empty;

                bool isPersistAssigned = await AssignDataVaultAndPersistTokenToCC(session.SessionId, session.Token, creditCard, mobRequest, session.Flow);

                if (!isPersistAssigned)
                {
                    var generateTokenResult = await _checkoutUtility.GenerateCCTokenWithDataVault(creditCard, session.SessionId, session.Token, mobRequest.Application, mobRequest.DeviceId, ccDataVaultToken);
                    if (generateTokenResult.GeneratedCCTokenWithDataVault)
                    {
                        ccDataVaultToken = generateTokenResult.CcDataVaultToken;
                        creditCard.AccountNumberToken = ccDataVaultToken;
                        if (creditCard.UnencryptedCardNumber != null)
                        {
                            creditCard.UnencryptedCardNumber = "XXXXXXXXXXXX" + creditCard.UnencryptedCardNumber.Substring((creditCard.UnencryptedCardNumber.Length - 4), 4);
                        }
                    }
                }


                #endregion
                if (creditCard != null)
                {
                    if (creditCard.CardTypeDescription != null)
                    {
                        switch (creditCard.CardTypeDescription.ToLower())
                        {
                            case "diners club":
                                creditCard.CardTypeDescription = "Diners Club Card";
                                break;
                            case "uatp (formerly air travel card)":
                                creditCard.CardTypeDescription = "UATP";
                                break;
                        }
                    }
                }
            }
            if (_configuration.GetValue<string>("MakeItEmptyCreditCardInformationNeededMessage") != null && Convert.ToBoolean(_configuration.GetValue<string>("MakeItEmptyCreditCardInformationNeededMessage").ToString()))
            {
                creditCard.Message = string.Empty;
            }
            creditCard.IsPrimary = true;
            //creditCard.IsTemparory = true;
            creditCard.IsValidForTPIPurchase = IsValidFOPForTPIpayment(creditCard.CardType);
            return creditCard;
        }

        private bool IsValidFOPForTPIpayment(string cardType)
        {
            return !string.IsNullOrEmpty(cardType) &&
                (cardType.ToUpper().Trim() == "VI" || cardType.ToUpper().Trim() == "MC" || cardType.ToUpper().Trim() == "AX" || cardType.ToUpper().Trim() == "DS");
        }

        private async Task<bool> AssignDataVaultAndPersistTokenToCC(string sessionId, string sessionToken, MOBCreditCard creditCard, MOBRequest mobRequest, string flow)
        {
            bool isPersistAssigned = _configuration.GetValue<bool>("VormetricTokenMigration");
            _token = sessionToken;
            _sessionId = sessionId;
            _application = mobRequest?.Application;
            _deviceId = mobRequest?.DeviceId;
            if (isPersistAssigned)
            {

                //Profile profile = new Profile();
                if (await GenerateCCTokenWithDataVault(creditCard, sessionId, sessionToken, _application, _deviceId, flow))
                {
                    if (!string.IsNullOrEmpty(creditCard.PersistentToken))
                    {
                        if (creditCard.UnencryptedCardNumber != null && creditCard.UnencryptedCardNumber.Length > 4)
                        {
                            creditCard.UnencryptedCardNumber = "XXXXXXXXXXXX" + creditCard.UnencryptedCardNumber.Substring((creditCard.UnencryptedCardNumber.Length - 4), 4);
                        }
                        else
                        {
                            creditCard.UnencryptedCardNumber = "XXXXXXXXXXXX";
                        }
                    }

                    else if (String.IsNullOrEmpty(creditCard.AccountNumberToken) && !string.IsNullOrEmpty(_token) && !string.IsNullOrEmpty(_sessionId))
                    {
                        MOBVormetricKeys vormetricKeys = await _checkoutUtility.GetPersistentTokenUsingAccountNumberToken(creditCard.AccountNumberToken, _sessionId, _token);
                        creditCard.PersistentToken = vormetricKeys.PersistentToken;
                        creditCard.SecurityCodeToken = vormetricKeys.SecurityCodeToken;
                        creditCard.CardType = vormetricKeys.CardType;
                    }
                    else
                    {
                        LogNoPersistentTokenInCSLResponseForVormetricPayment(sessionId);
                    }
                }
            }
            return isPersistAssigned;
        }

        private async Task<bool> GenerateCCTokenWithDataVault(MOBCreditCard creditCardDetails, string sessionID, string token, MOBApplication applicationDetails, string deviceID, string flow)
        {
            bool generatedCCTokenWithDataVault = false;
            //if (creditCardDetails.UnencryptedCardNumber == null || (creditCardDetails.UnencryptedCardNumber != null && !creditCardDetails.UnencryptedCardNumber.Contains("XXXXXXXXXXXX")))
            if (!string.IsNullOrEmpty(creditCardDetails.EncryptedCardNumber)) // expecting Client will send only Encrypted Card Number only if the user input is a clear text CC number either for insert CC or update CC not the CC details like CVV number update or expiration date upate no need to call data vault for this type of updates only data vault will be called for CC number update to get the CC token back
            {
                #region
                CslDataVaultRequest dataVaultRequest = await GetDataValutRequest(creditCardDetails, sessionID, applicationDetails, flow);
                //United.Services.Customer.Common.InsertCreditCardRequest insertAddress = GetInsertCreditCardRequest(request);

                string jsonRequest = DataContextJsonSerializer.Serialize<CslDataVaultRequest>(dataVaultRequest);


                //string jsonRequest = JsonConvert.SerializeObject(dataVaultRequest);
                //string url = string.Format("{0}/AddPayment", _configuration.GetValue<string>("ServiceEndPointBaseUrl - CSLDataVault"));
                string jsonResponse = await _dataVaultService.GetCCTokenWithDataVault(token, jsonRequest, sessionID);

                #endregion//****Get Call Duration Code - Venkat 03/17/2015*******
                //string jsonResponse = HttpHelper.Post(url, "application/json; charset=utf-8", token, jsonRequest);
                #region// 2 = cslStopWatch//****Get Call Duration Code - Venkat 03/17/2015*******

                if (!string.IsNullOrEmpty(jsonResponse))
                {
                    CslDataVaultResponse response = DataContextJsonSerializer.DeserializeJsonDataContract<CslDataVaultResponse>(jsonResponse);

                    if (response != null && response.Responses != null && response.Responses[0].Error == null && response.Responses[0].Message != null && response.Responses[0].Message.Count > 0 && response.Responses[0].Message[0].Code.Trim() == "0")
                    {
                        generatedCCTokenWithDataVault = true;
                        var creditCard = response.Items[0] as Service.Presentation.PaymentModel.CreditCard;
                        creditCardDetails.AccountNumberToken = creditCard.AccountNumberToken;
                        creditCardDetails.PersistentToken = creditCard.PersistentToken;
                        creditCardDetails.SecurityCodeToken = creditCard.SecurityCodeToken;
                        if (String.IsNullOrEmpty(creditCardDetails.CardType))
                        {
                            creditCardDetails.CardType = creditCard.Code;
                        }
                        else if (!_configuration.GetValue<bool>("DisableCheckForUnionPayFOP_MOBILE13762"))
                        {
                            string[] checkForUnionPayFOP = _configuration.GetValue<string>("CheckForUnionPayFOP")?.Split('|');
                            if (creditCard?.Code == checkForUnionPayFOP?[0])
                            {
                                creditCardDetails.CardType = creditCard.Code;
                                creditCardDetails.CardTypeDescription = checkForUnionPayFOP?[1];
                            }
                        }
                    }
                    else
                    {
                        if (response.Responses[0].Error != null && response.Responses[0].Error.Count > 0)
                        {
                            string errorMessage = string.Empty;
                            string errorCode = string.Empty;
                            foreach (var error in response.Responses[0].Error)
                            {
                                errorMessage = errorMessage + " " + error.Text;
                                errorCode = error.Code;
                            }
                            throw new MOBUnitedException(errorCode, errorMessage);
                        }
                        else
                        {
                            string exceptionMessage = _configuration.GetValue<string>("UnableToInsertCreditCardToProfileErrorMessage").ToString();
                            if (_configuration.GetValue<string>("ReturnActualExceptionMessageBackForTesting") != null && Convert.ToBoolean(_configuration.GetValue<string>("ReturnActualExceptionMessageBackForTesting").ToString()))
                            {
                                exceptionMessage = exceptionMessage + " response.Status not success and response.Errors.Count == 0 - at DAL InsertTravelerCreditCard(MOBUpdateTravelerRequest request)";
                            }
                            throw new MOBUnitedException(exceptionMessage);
                        }
                    }
                }
                else
                {
                    string exceptionMessage = _configuration.GetValue<string>("UnableToInsertCreditCardToProfileErrorMessage");
                    if (_configuration.GetValue<string>("ReturnActualExceptionMessageBackForTesting") != null && Convert.ToBoolean(_configuration.GetValue<string>("ReturnActualExceptionMessageBackForTesting")))
                    {
                        exceptionMessage = exceptionMessage + " - due to jsonResponse is empty at DAL  GenerateCCTokenWithDataVault(MOBUpdateTravelerRequest request, ref string ccDataVaultToken)";
                    }
                    throw new MOBUnitedException(exceptionMessage);
                }
                #endregion
            }
            else if (!string.IsNullOrEmpty(creditCardDetails.AccountNumberToken.Trim()))
            {
                generatedCCTokenWithDataVault = true;
            }
            if (_configuration.GetValue<bool>("EnableRemoveEncryptedCardnumber"))
            {
                ConfigUtility.RemoveEncyptedCreditcardNumber(creditCardDetails);
            }
            return generatedCCTokenWithDataVault;
        }

        private async Task<CslDataVaultRequest> GetDataValutRequest(MOBCreditCard creditCardDetails, string sessionID, MOBApplication applicationDetails, string flow)
        {
            #region
            var dataVaultRequest = new CslDataVaultRequest
            {
                Items = new System.Collections.ObjectModel.Collection<United.Service.Presentation.PaymentModel.Payment>(),
                Types = new System.Collections.ObjectModel.Collection<United.Service.Presentation.CommonModel.Characteristic>(),
                CallingService = new United.Service.Presentation.CommonModel.ServiceClient { Requestor = new Requestor { AgentAAA = "WEB", ApplicationSource = "mobile services" } }
            };
            United.Services.Customer.Common.InsertCreditCardRequest creditCardInsertRequest = new United.Services.Customer.Common.InsertCreditCardRequest();
            if (creditCardDetails != null)
            {
                var cc = new United.Service.Presentation.PaymentModel.CreditCard();
                cc.ExpirationDate = creditCardDetails.ExpireMonth + "/" + (creditCardDetails.ExpireYear.Trim().Length == 2 ? creditCardDetails.ExpireYear.Trim() : creditCardDetails.ExpireYear.Trim().Substring(2, 2).ToString()); //"05/17";
                if (!_configuration.GetValue<bool>("DisableUATBCvvCodeNullCheckAndAssignEmptyString") &&
                   string.IsNullOrEmpty(creditCardDetails.CIDCVV2))
                {
                    cc.SecurityCode = "";
                }
                else
                {
                    cc.SecurityCode = creditCardDetails.CIDCVV2.Trim(); //"1234";
                }

                cc.Code = creditCardDetails.CardType;  //"VI";
                cc.Name = creditCardDetails.CCName; //"Test Testing";
                if (!string.IsNullOrEmpty(creditCardDetails.EncryptedCardNumber))
                {
                    dataVaultRequest.Types = new System.Collections.ObjectModel.Collection<United.Service.Presentation.CommonModel.Characteristic>();
                    dataVaultRequest.Types.Add(new United.Service.Presentation.CommonModel.Characteristic { Code = "ENCRYPTION", Value = "PKI" });
                    cc.AccountNumberEncrypted = creditCardDetails.EncryptedCardNumber;

                    if (_configuration.GetValue<bool>("EnablePKDispenserKeyRotationAndOAEPPadding") && creditCardDetails.IsOAEPPaddingCatalogEnabled)
                    {
                        Session session = new Session();
                        session = await _sessionHelperService.GetSession<Session>(sessionID, session.ObjectName, new List<string> { sessionID, session.ObjectName }).ConfigureAwait(false);
                        if (ConfigUtility.IsSuppressPkDispenserKey(applicationDetails.Id, applicationDetails.Version.Major, session?.CatalogItems, flow) || await _shoppingCartUtility.IsCheckInFlow(flow))
                        {
                            if (string.IsNullOrEmpty(creditCardDetails?.Kid))
                            {
                                _logger.LogError("GetCreditCardToken Exception-kid value in request is empty with {sessionId}", sessionID);
                                throw new Exception(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                            }
                            else
                            {
                                dataVaultRequest.Types.Add(new Characteristic { Code = "KID", Value = creditCardDetails.Kid });
                            }
                        }
                        else
                        {
                            string transId = string.IsNullOrEmpty(_headers.ContextValues.TransactionId) ? "trans0" : _headers.ContextValues.TransactionId;
                            string key = string.Format(_configuration.GetValue<string>("PKDispenserKeyTokenKeyFormat"), applicationDetails.Id);
                            var cacheResponse = await _cachingService.GetCache<United.Service.Presentation.SecurityResponseModel.PKDispenserKey>(key, transId).ConfigureAwait(false);
                            var obj = JsonConvert.DeserializeObject<United.Service.Presentation.SecurityResponseModel.PKDispenserKey>(cacheResponse);
                            dataVaultRequest.Types.Add(new Characteristic { Code = "KID", Value = obj.Kid });
                        }
                        dataVaultRequest.Types.Add(new United.Service.Presentation.CommonModel.Characteristic { Code = "OAEP", Value = "TRUE" });
                    }
                }
                else
                {
                    cc.AccountNumber = creditCardDetails.UnencryptedCardNumber; //"4000000000000002";
                }
                cc.OperationID = Guid.NewGuid().ToString();

                ///93875 - iOS Android UnionPay bookings are not throwing error for bin ranges that are not 16 digit credit cards
                ///Srini - 02/13/2018
                if (_configuration.GetValue<bool>("DataVaultRequestAddDollarDingToggle"))
                {
                    cc.PinEntryCapability = Service.Presentation.CommonEnumModel.PosTerminalPinEntryCapability.PinNotSupported;
                    cc.TerminalAttended = Service.Presentation.CommonEnumModel.PosTerminalAttended.Unattended;
                    dataVaultRequest.PointOfSaleCountryCode = "US";
                    dataVaultRequest.Types.Add(new United.Service.Presentation.CommonModel.Characteristic { Code = "DOLLAR_DING", Value = "TRUE" });
                }
                dataVaultRequest.Items.Add(cc);
            }
            return dataVaultRequest;
            #endregion
        }

        private void LogNoPersistentTokenInCSLResponseForVormetricPayment(string sessionId, string Message = "Unable to retieve PersistentToken")
        {
            _logger.LogError("LogNoPersistentTokenInCSLResponseForVormetricPayment-PERSISTENTTOKENNOTFOUND Error {exception} {sessionid}", Message, sessionId);
            //No need to block the flow as we are calling DataVault for Persistent Token during the final payment
        }

        private async Task<MOBFOPAcquirePaymentTokenResponse> GetPayPalToken(MOBFOPAcquirePaymentTokenRequest request, Session session)
        {
            var response = new MOBFOPAcquirePaymentTokenResponse();

            string path = string.Format("{0}{1}", "/PayPal/GetPayPalUrl/", request.CartId);
            var requestPayPal = new PayPal();
            PayPal responsePayPal = new PayPal(); ;
            requestPayPal.Amount = Convert.ToDouble(request.Amount);
            requestPayPal.BillingAddress = new Address();
            requestPayPal.BillingAddress.Country = new Service.Presentation.CommonModel.Country() { CountryCode = request.CountryCode };
            requestPayPal.ReturnURL = string.Format(_configuration.GetValue<string>("AcquirePayPalToken - ReturnURL"));
            requestPayPal.CancelURL = string.Format(_configuration.GetValue<string>("AcquirePayPalToken - CancelURL"));
            requestPayPal.Type = new Genre() { Key = request.FormofPaymentCode };
            string jsonRequest = JsonConvert.SerializeObject(requestPayPal);

            string response1 = await _paymentService.GetEligibleFormOfPayments(session.Token, path, jsonRequest, session.SessionId);

            if (await _featureSettings.GetFeatureSettingValue("EnablePaypalTokenExceptionHandling").ConfigureAwait(false))
            {
                if (!string.IsNullOrEmpty(response1))
                {
                    responsePayPal = JsonConvert.DeserializeObject<PayPal>(response1);
                    if (!string.IsNullOrEmpty(responsePayPal.TokenID))
                        response.Token = responsePayPal.TokenID;
                    else
                        throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                }
                else
                {
                    throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(response1))
                {
                    responsePayPal = JsonConvert.DeserializeObject<PayPal>(response1);
                    response.Token = responsePayPal.TokenID;
                }
            }
            return response;
        }

        private async Task<MOBFOPAcquirePaymentTokenResponse> GetMasterpassToken(MOBFOPAcquirePaymentTokenRequest request, Session session)
        {

            string path = "/MasterPassWallet/CreateMasterPassOpenWalletSession";
            OpenWalletSessionRequest openWalletRequest = BuildOpenWalletSessionRequest(Convert.ToDouble(request.Amount), session.CartId);

            var jsonResponse = await _paymentService.GetEligibleFormOfPayments(session.Token, path, JsonConvert.SerializeObject(openWalletRequest), session.SessionId);

            return DeserialiseAndBuildMOBFOPAcquirePaymentTokenResponse(jsonResponse);
        }

        private OpenWalletSessionRequest BuildOpenWalletSessionRequest(double amount, string cartId)
        {
            OpenWalletSessionRequest openWalletRequest = new OpenWalletSessionRequest();
            openWalletRequest.Amount = Convert.ToString(amount);
            openWalletRequest.Currency = _configuration.GetValue<string>("AcquireMasterpassToken-CurrencyCode");
            openWalletRequest.OriginCallingURL =
                _configuration.GetValue<string>("AcquireMasterpassToken-OriginCallingURL");
            openWalletRequest.OriginURL =
               _configuration.GetValue<string>("AcquireMasterpassToken-OriginURL");
            openWalletRequest.CartID = cartId;
            openWalletRequest.PointOfSale = _configuration.GetValue<string>("AcquireMasterPassToken-PointOfSale");
            return openWalletRequest;
        }

        private MOBFOPAcquirePaymentTokenResponse DeserialiseAndBuildMOBFOPAcquirePaymentTokenResponse(string jsonResponse)
        {
            MOBFOPAcquirePaymentTokenResponse response = new MOBFOPAcquirePaymentTokenResponse();

            if (!string.IsNullOrEmpty(jsonResponse))
            {
                var openWalletResponse = JsonConvert.DeserializeObject<OpenWalletSessionResponse>(jsonResponse);
                if (openWalletResponse != null && openWalletResponse.ServiceStatus != null && openWalletResponse.ServiceStatus.StatusType.ToUpper().Equals("SUCCESS"))
                {
                    response.Token = string.Format(_configuration.GetValue<string>("MasterpassURL"),
                                                                    openWalletResponse.AllowedCardTypes,
                                                                    _configuration.GetValue<string>("AcquireMasterpassToken-ShippingCountris"),
                                                                    _configuration.GetValue<string>("AcquireMasterpassToken-CallbackURL"),
                                                                    openWalletResponse.MerchantCheckoutID,
                                                                    openWalletResponse.RequestToken);
                    response.CslSessionId = openWalletResponse.SessionID;
                }
                else
                {
                    if (openWalletResponse != null) ThrowExceptionAsPerErrorResponse(openWalletResponse.Error);
                }
            }
            else
            {
                throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }

            return response;
        }

        private void ThrowExceptionAsPerErrorResponse(Collection<Service.Presentation.CommonModel.ExceptionModel.Error> errors)
        {
            if (errors != null && errors.Count > 0)
            {
                string errorMessage = string.Empty;
                foreach (var error in errors)
                {
                    errorMessage = errorMessage + " " + (error.Description ?? error.Text);
                }
                throw new System.Exception(errorMessage.Trim());
            }
            else
            {
                throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }
        }


        private async Task<string> MetaSyncUserSession(Session session, MOBSHOPMetaSelectTripRequest request)
        {

            string url = string.Format("UserSessionSync?Channel=MOBILE&cartDetail=true");

            MetaUserSessionSyncRequest metaUserSessionSync = new MetaUserSessionSyncRequest();
            metaUserSessionSync.AuthTokenId = request.MedaSessionId;
            metaUserSessionSync.CartId = request.CartId;
            string jsonRequest = JsonConvert.SerializeObject(metaUserSessionSync);

            string jsonResponse = await _shoppingCartService.MetaSyncUserSession<string>(session.Token, session.SessionId, url, jsonRequest);
            // jsonResponse = HttpHelper.Post(url, "application/json; charset=utf-8", session.Token, jsonRequest, httpPostTimeOut, httpPostNumberOfRetry);

            if (!string.IsNullOrEmpty(jsonResponse))
            {
                MetaUserSessionSyncResponse response = JsonConvert.DeserializeObject<MetaUserSessionSyncResponse>(jsonResponse);
                if (response.Status.Equals("1"))
                {
                    return jsonResponse;
                }
                else
                {
                    throw new MOBUnitedException(_configuration.GetValue<string>("MetaTripExceptionMessage"));
                }
            }

            return jsonResponse;
        }
        private async Task<MOBRegisterOfferResponse> GetCartInformation(MOBSHOPMetaSelectTripRequest request, Session session)
        {
            var flow = request.Flow;
            MOBRegisterOfferResponse response = new MOBRegisterOfferResponse();
            var cartJsonResponse = await MetaSyncUserSession(session, request);

            var flightReservationResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<FlightReservationResponse>(cartJsonResponse);
            flightReservationResponse.CartId = request.CartId;

            var isCompleteFarelockPurchase = false; //Utility.GetBooleanConfigValue("EnableFareLockPurchaseViewRes") && request.MerchandizingOfferDetails.Any(o => (o.ProductCode != null && o.ProductCode.Equals("FLK_VIEWRES", StringComparison.OrdinalIgnoreCase)));

            response.ShoppingCart = new MOBShoppingCart();
            response.ShoppingCart.Products = await _shoppingCartUtility.ConfirmationPageProductInfo(flightReservationResponse, false, false, flow, request.Application, null, false, false, null, null, true);
            response.ShoppingCart.TermsAndConditions = await _shoppingCartUtility.GetProductBasedTermAndConditions(null, flightReservationResponse, false, session.SessionId, flow, true);
            response.ShoppingCart.PaymentTarget = _shoppingCartUtility.GetPaymentTargetForRegisterFop(flightReservationResponse, flow, isCompleteFarelockPurchase);
            double price = _shoppingUtility.GetGrandTotalPriceForShoppingCart(isCompleteFarelockPurchase, flightReservationResponse, false, flow);
            response.ShoppingCart.TotalPrice = String.Format("{0:0.00}", price);
            response.ShoppingCart.Flow = flow;
            if (flightReservationResponse.DisplayCart.Characteristics != null && flightReservationResponse.DisplayCart.Characteristics.Any(c => c.Code.Equals("WorkFlowType", StringComparison.OrdinalIgnoreCase)))
            {
                response.ShoppingCart.CslWorkFlowType = Convert.ToInt32(flightReservationResponse.DisplayCart.Characteristics.FirstOrDefault(c => c.Code.Equals("WorkFlowType", StringComparison.OrdinalIgnoreCase)).Value);
            }
            response.ShoppingCart.TripInfoForUplift = await _mSCFormsOfPayment.GetUpliftTripInfo(flightReservationResponse.Reservation, response.ShoppingCart.TotalPrice, response.ShoppingCart.Products, true);

            response.ShoppingCart.CartId = flightReservationResponse.CartId.ToString();
            response.PkDispenserPublicKey = await _mSCPkDispenserPublicKey.GetCachedOrNewpkDispenserPublicKey(request.Application.Id, request.Application.Version.Major, request.DeviceId, session.SessionId, session.Token, session?.CatalogItems).ConfigureAwait(false);

            TravelOptionsCollection travelOptions = flightReservationResponse.DisplayCart?.TravelOptions;
            TravelOption bagTravelOption = travelOptions?.FirstOrDefault(x => string.Equals(x.Type, "Bags", StringComparison.OrdinalIgnoreCase));
            response.ShoppingCart.DisplayTotalPrice = $"{travelOptions?[0].SubItems?[0].Currency} { Decimal.Parse(price.ToString()).ToString("c")}";
            if (bagTravelOption != null)
            {
                string[] currency = bagTravelOption.Currency.Split('-');
                string currencyCode = currency[0];
                string currencySymbol = currency[1];
                response.ShoppingCart.DisplayTotalPrice = $"{currencyCode} {Decimal.Parse(price.ToString()).ToString("c")}";
                response.ShoppingCart.DisplaySubTotalPrice = $"{currencySymbol}" + String.Format("{0:0.00}", bagTravelOption.SubItems.Where(key => key.Key.Contains("BAGFEE")).Sum(total => total.Amount));
                response.ShoppingCart.DisplayTaxesAndFees = $"{currencySymbol}" + String.Format("{0:0.00}", bagTravelOption.SubItems.Where(key => key.Key.Contains("BAGTAXFEE")).Sum(tax => tax.Amount));
            }
            if (!response.ShoppingCart.Products.IsNullOrEmpty() && response.ShoppingCart.Products.Any(p => string.Equals(p.Code, "SEATASSIGNMENTS", StringComparison.OrdinalIgnoreCase)))
            {
                // # below method is to check eligible product code for couponing
                //TODO
                response.ShoppingCart.IsCouponEligibleProduct = !_configuration.GetValue<bool>("IsEnableManageResCoupon") && IsCouponEligibleProduct(response.ShoppingCart.Products);
                response.ShoppingCart.PromoCodeDetails = _configuration.GetValue<bool>("IsEnableManageResCoupon") && GeneralHelper.IsApplicationVersionGreaterorEqual(request.Application.Id, request.Application.Version.Major, "Android_IsEnableManageResMerchCoupon_AppVersion", "iPhone_IsEnableManageResMerchCoupon_AppVersion") ? new MOBPromoCodeDetails() : null;

            }
            if (isCompleteFarelockPurchase)
            {
                response.ShoppingCart.Trips = null;/* GetTrips(null)*/
                response.ShoppingCart.SCTravelers = _mSCFormsOfPayment.GetTravelerCSLDetails(flightReservationResponse.Reservation, response.ShoppingCart.Trips, session.SessionId, request.Flow);
                //Prices & Taxes
                if (!flightReservationResponse.DisplayCart.IsNullOrEmpty() && !flightReservationResponse.DisplayCart.DisplayPrices.IsNullOrEmpty() && flightReservationResponse.DisplayCart.DisplayPrices.Any())
                {
                    // Journey Type will be "OW", "RT", "MD"
                    var JourneyType = UtilityHelper.GetJourneyTypeDescription(flightReservationResponse.Reservation);
                    bool isCorporateFare = IsCorporateTraveler(flightReservationResponse.Reservation.Characteristic);
                    response.ShoppingCart.Prices = await GetPrices(flightReservationResponse.DisplayCart.DisplayPrices, false, null, false, JourneyType, isCompleteFarelockPurchase, isCorporateFare, session: session);
                    response.ShoppingCart.Taxes = GetTaxAndFeesAfterPriceChange(flightReservationResponse.DisplayCart.DisplayPrices, false);
                    response.ShoppingCart.Captions = GetFareLockCaptions(flightReservationResponse.Reservation, JourneyType, isCorporateFare);
                    response.ShoppingCart.ELFLimitations = await GetELFLimitationsViewRes(response.ShoppingCart.Trips, request.Application.Id);
                }
            }
            if (response.ShoppingCart.Captions.IsNullOrEmpty())
            {
                response.ShoppingCart.Captions = await _eTCUtility.GetCaptions("PaymentPage_ViewRes_Captions");
            }
            IsHidePromoOption(response.ShoppingCart, flow, request.Application, session.IsReshopChange);
            await AssignProfileSavedETCsFromPersist(session.SessionId, response.ShoppingCart);

            await _sessionHelperService.SaveSession<MOBShoppingCart>(response.ShoppingCart, response.SessionId, new List<string> { response.SessionId, response.ShoppingCart.ObjectName }, response.ShoppingCart.ObjectName).ConfigureAwait(false);

            return response;
        }

        private List<MOBSHOPTripBase> GetTripBase(List<MOBSHOPTrip> ShopTrip)
        {
            if (ShopTrip == null || !ShopTrip.Any())
                return null;

            List<MOBSHOPTripBase> trips = new List<MOBSHOPTripBase>();
            foreach (var trip in ShopTrip)
            {
                var tripBase = new MOBSHOPTripBase();
                tripBase = trip;
                tripBase.DepartDate = trip.DepartDate;
                trips.Add(tripBase);
            }
            return trips;
        }

        private List<MOBTypeOption> GetTCList(string configValue)
        {
            List<MOBTypeOption> contentList = new List<MOBTypeOption>();
            if (_configuration.GetValue<string>(configValue) != null)
            {
                string tcContentList = _configuration.GetValue<string>(configValue);
                foreach (string eachItem in tcContentList.Split('|'))
                {
                    contentList.Add(new MOBTypeOption() { Value = eachItem });
                }
            }
            return contentList;
        }
        private async Task<List<MOBMobileCMSContentMessages>> GetTermsAndConditions(string title)
        {
            var cmsContentMessages = new List<MOBMobileCMSContentMessages>();
            var docKeys = title;
            var docs = await _legalDocumentsForTitlesService.GetNewLegalDocumentsForTitles(docKeys, _headers.ContextValues.SessionId, true);
            if (docs != null && docs.Any())
            {
                foreach (var doc in docs)
                {
                    var cmsContentMessage = new MOBMobileCMSContentMessages();
                    //legaldocument=Document?
                    //cmsContentMessage.ContentFull = doc.LegalDocument;
                    cmsContentMessage.ContentFull = doc.Document;
                    cmsContentMessage.Title = doc.Title;
                    cmsContentMessages.Add(cmsContentMessage);
                }
            }

            return cmsContentMessages;
        }
        private async Task<List<MOBItem>> GetCaptions(string key)
        {
            return !string.IsNullOrEmpty(key) ? await GetCaptions(key, true) : null;
        }
        private async Task<List<MOBItem>> GetCaptions(string keyList, bool isTnC)
        {
            var docs = await _legalDocumentsForTitlesService.GetNewLegalDocumentsForTitles(keyList, _headers.ContextValues.TransactionId, isTnC);
            if (docs == null || !docs.Any()) return null;

            var captions = new List<MOBItem>();

            captions.AddRange(
                docs.Select(doc => new MOBItem
                {
                    Id = doc.Title,
                    //CurrentValue = doc.LegalDocument
                    CurrentValue = doc.Document
                }));
            return captions;
        }
        private string GetCommonSeatCode(string seatCode)
        {
            if (string.IsNullOrEmpty(seatCode))
                return string.Empty;

            string commonSeatCode = string.Empty;

            switch (seatCode.ToUpper().Trim())
            {
                case "SXZ": //StandardPreferredExitPlus
                case "SZX": //StandardPreferredExit
                case "SBZ": //StandardPreferredBlukheadPlus
                case "SZB": //StandardPreferredBlukhead
                case "SPZ": //StandardPreferredZone
                case "PZA":
                    commonSeatCode = "PZA";
                    break;
                case "SXP": //StandardPrimeExitPlus
                case "SPX": //StandardPrimeExit
                case "SBP": //StandardPrimeBlukheadPlus
                case "SPB": //StandardPrimeBlukhead
                case "SPP": //StandardPrimePlus
                case "PPE": //StandardPrime
                case "BSA":
                case "ASA":
                    commonSeatCode = "ASA";
                    break;
                case "EPL": //EplusPrime
                case "EPU": //EplusPrimePlus
                case "BHS": //BulkheadPrime
                case "BHP": //BulkheadPrimePlus  
                case "PSF": //PrimePlus 
                    commonSeatCode = "EPU";
                    break;
                case "PSL": //Prime                           
                    commonSeatCode = "PSL";
                    break;
                default:
                    return seatCode;
            }
            return commonSeatCode;
        }

        private async Task<List<MOBMobileCMSContentMessages>> GetTermsAndConditions(bool hasPremierAccelerator)
        {
            var dbKey = _configuration.GetValue<bool>("EnablePPRChangesForAAPA") ? hasPremierAccelerator ? "PPR_AAPA_TERMS_AND_CONDITIONS_AA_PA_MP"
                                              : "PPR_AAPA_TERMS_AND_CONDITIONS_AA_MP" : hasPremierAccelerator ? "AAPA_TERMS_AND_CONDITIONS_AA_PA_MP"
                                              : "AAPA_TERMS_AND_CONDITIONS_AA_MP";

            var docs = await _legalDocumentsForTitlesService.GetNewLegalDocumentsForTitles(dbKey, _headers.ContextValues.SessionId, true);
            if (docs == null || !docs.Any())
                return null;

            var tncs = new List<MOBMobileCMSContentMessages>();
            foreach (var doc in docs)
            {
                var tnc = new MOBMobileCMSContentMessages
                {
                    Title = "Terms and conditions",
                    //ContentFull = doc.LegalDocument
                    ContentFull = doc.Document,
                    ContentShort = _configuration.GetValue<string>("PaymentTnCMessage"),
                    HeadLine = doc.Title
                };
                tncs.Add(tnc);
            }

            return tncs;
        }

        private List<MOBTypeOption> GetPBContentList(string configValue)
        {
            List<MOBTypeOption> contentList = new List<MOBTypeOption>();
            if (_configuration.GetValue<string>(configValue) != null)
            {
                string pBContentList = _configuration.GetValue<string>(configValue);
                foreach (string eachItem in pBContentList.Split('~'))
                {
                    contentList.Add(new MOBTypeOption(eachItem.Split('|')[0].ToString(), eachItem.Split('|')[1].ToString()));
                }
            }
            return contentList;
        }

        private async Task<List<MOBMobileCMSContentMessages>> GetTermsAndConditions()
        {
            var cmsContentMessages = new List<MOBMobileCMSContentMessages>();
            var docKeys = "PCU_TnC";
            var docs = await _legalDocumentsForTitlesService.GetNewLegalDocumentsForTitles(docKeys, _headers.ContextValues.SessionId, true);
            if (docs != null && docs.Any())
            {
                foreach (var doc in docs)
                {
                    var cmsContentMessage = new MOBMobileCMSContentMessages();
                    //cmsContentMessage.ContentFull = doc.LegalDocument;
                    cmsContentMessage.ContentFull = doc.Document;

                    cmsContentMessage.Title = doc.Title;
                    cmsContentMessages.Add(cmsContentMessage);
                }
            }
            return cmsContentMessages;
        }

        private List<MOBTypeOption> GetPATermsAndConditionsList()
        {
            List<MOBTypeOption> tAndCList = new List<MOBTypeOption>();
            if (_configuration.GetValue<string>("PremierAccessTermsAndConditionsList") != null)
            {
                string premierAccessTermsAndConditionsList = _configuration.GetValue<string>("PremierAccessTermsAndConditionsList");
                foreach (string eachItem in premierAccessTermsAndConditionsList.Split('~'))
                {
                    tAndCList.Add(new MOBTypeOption(eachItem.Split('|')[0].ToString(), eachItem.Split('|')[1].ToString()));
                }
            }
            else
            {
                #region
                tAndCList.Add(new MOBTypeOption("paTandC1", "This Premier Access offer is nonrefundable and non-transferable"));
                tAndCList.Add(new MOBTypeOption("paTandC2", "Voluntary changes to your itinerary may forfeit your Premier Access purchase and \n any associated fees."));
                tAndCList.Add(new MOBTypeOption("paTandC3", "In the event of a flight cancellation or involuntary schedule change, we will refund \n the fees paid for the unused Premier Access product upon request."));
                tAndCList.Add(new MOBTypeOption("paTandC4", "Premier Access is offered only on flights operated by United and United Express."));
                tAndCList.Add(new MOBTypeOption("paTandC5", "This Premier Access offer is processed based on availability at time of purchase."));
                tAndCList.Add(new MOBTypeOption("paTandC6", "Premier Access does not guarantee wait time in airport check-in, boarding, or security lines. Premier Access does not exempt passengers from check-in time limits."));
                tAndCList.Add(new MOBTypeOption("paTandC7", "Premier Access benefits apply only to the customer who purchased Premier Access \n unless purchased for all customers on a reservation. Each travel companion must purchase Premier Access in order to receive benefits."));
                tAndCList.Add(new MOBTypeOption("paTandC8", "“Premier Access” must be printed or displayed on your boarding pass in order to \n receive benefits."));
                tAndCList.Add(new MOBTypeOption("paTandC9", "This offer is made at United's discretion and is subject to change or termination \n at any time with or without notice to the customer."));
                tAndCList.Add(new MOBTypeOption("paTandC10", "By clicking “I agree - Continue to purchase” you agree to all terms and conditions."));
                #endregion
            }
            return tAndCList;
        }

        private async Task<List<MOBItem>> GetELFLimitationsViewRes(List<MOBSHOPTrip> shopTrip, int appId = 0)
        {
            if (shopTrip.IsNullOrEmpty())
            { return null; }

            var databaseKey = string.Empty;
            bool isIBE = false;
            bool isELF = false;
            shopTrip.ForEach(p =>
            {
                if (!p.IsNullOrEmpty() && !p.FlattenedFlights.IsNullOrEmpty())
                {
                    isELF = p.FlattenedFlights.Any(k => k.IsElf);
                    isIBE = p.FlattenedFlights.Any(k => k.IsIBE);
                }
            });

            if (isELF)
            {
                databaseKey = "SSA_ELF_MR_Limitations";
            }
            else if (isIBE)
            {
                databaseKey = "IBE_MR_Limitations";
            }

            var messages = !string.IsNullOrEmpty(databaseKey) ?
                    await GetMPPINPWDTitleMessages(new List<string> { databaseKey }) : null;

            if (!_configuration.GetValue<bool>("DisableRestrictionsForiOS"))
            {
                if (messages != null && appId == 1)
                {
                    try
                    {
                        var footNote = messages.Where(x => x.Id == _configuration.GetValue<string>("RestrictionsLimitationsFootNotes")).FirstOrDefault();
                        if (footNote != null && footNote?.CurrentValue != null)
                        {
                            if (footNote.CurrentValue.StartsWith("<p>"))
                            {
                                footNote.CurrentValue = footNote.CurrentValue.Replace("<p>", "").Replace("</p>", "").Replace("<br/><br/>", "\n\n");
                            }
                        }
                    }
                    catch (Exception ex) { }
                }
            }

            return messages;
        }

        private async Task<List<MOBItem>> GetMPPINPWDTitleMessages(List<string> titleList)
        {
            List<MOBItem> messages = new List<MOBItem>();
            var documentLibrary = new DocumentLibraryDynamoDB(_configuration, _dynamoDBService);
            List<United.Definition.MOBLegalDocument> docs = await documentLibrary.GetNewLegalDocumentsForTitles(titleList, _headers.ContextValues.SessionId);
            if (docs != null && docs.Count > 0)
            {
                foreach (var doc in docs)
                {
                    MOBItem item = new MOBItem();
                    item.Id = doc.Title;
                    //item.CurrentValue = doc.LegalDocument;
                    item.CurrentValue = doc.Document;
                    messages.Add(item);
                }
            }
            return messages;
        }

        private List<MOBItem> GetFareLockCaptions(United.Service.Presentation.ReservationModel.Reservation reservation, string journeyType, bool isCorporateFareLock)
        {
            if (reservation.IsNullOrEmpty() || journeyType.IsNullOrEmpty())
                return null;

            var title = GetPaymentTitleCaptionFareLock(reservation, journeyType);
            var tripType = GetSegmentDescriptionPageSubTitle(reservation);

            List<MOBItem> captions = new List<MOBItem>();
            if (!title.IsNullOrEmpty() && !tripType.IsNullOrEmpty())
            {
                captions.Add(GetFareLockViewResPaymentCaptions("PaymentPage_Title", title));
                captions.Add(GetFareLockViewResPaymentCaptions("PaymentPage_SubTitle", tripType));
                captions.Add(GetFareLockViewResPaymentCaptions("PaymentPage_Book24Hr_Policy", "Book without worry"));
                captions.Add(GetFareLockViewResPaymentCaptions("PaymentPage_ProductCode", "FLK_ViewRes"));
            }

            // To find if PNR is corporate travel
            if (isCorporateFareLock)
            {
                var priceText = _configuration.GetValue<string>("CorporateRateText");
                if (!priceText.IsNullOrEmpty())
                {
                    captions.Add(GetFareLockViewResPaymentCaptions("Corporate_PriceBreakDownText", priceText));
                }
            }

            return captions;
        }

        private string GetSegmentDescriptionPageSubTitle(United.Service.Presentation.ReservationModel.Reservation reservation)
        {
            if (reservation != null)
            {
                var traveler = reservation.Travelers.Count.ToString() + (reservation.Travelers.Count() > 1 ? " travelers" : " traveler");
                var JourneyType = UtilityHelper.GetJourneyTypeDescription(reservation);

                return !JourneyType.IsNullOrEmpty() ? JourneyType + ", " + traveler : string.Empty;
            }
            return string.Empty;
        }

        private MOBItem GetFareLockViewResPaymentCaptions(string id, string currentValue)
        {
            if (id.IsNullOrEmpty() || currentValue.IsNullOrEmpty())
                return null;

            var captions = new MOBItem()
            {
                Id = id,
                CurrentValue = currentValue
            };
            return captions;
        }

        private string GetPaymentTitleCaptionFareLock(Service.Presentation.ReservationModel.Reservation reservation, string journeyType)
        {
            if (reservation.IsNullOrEmpty() || journeyType.IsNullOrEmpty())
                return string.Empty;
            string fareLockHeaderDate = GetFareLockTitleViewResPaymentRTI(reservation, journeyType);
            string fareLockHeaderTitle = GetFareLockTitle(reservation, journeyType);

            if (!fareLockHeaderDate.IsNullOrEmpty() && !fareLockHeaderTitle.IsNullOrEmpty())
            {
                string title = journeyType.Equals("Multicity") ? "Starting" + " " + fareLockHeaderDate + " " + fareLockHeaderTitle
                                                        : fareLockHeaderDate + " " + fareLockHeaderTitle;
                if (journeyType.Equals("Multicity") && title.Length > 26)
                {
                    int endIndex = title.IndexOf("/") + 1;
                    title = title.Substring(0, endIndex) + "....";
                }
                return title;
            }
            return string.Empty;
        }

        private string GetFareLockTitleViewResPaymentRTI(United.Service.Presentation.ReservationModel.Reservation reservation, string journeyType)
        {
            if (reservation.IsNullOrEmpty() || journeyType.IsNullOrEmpty())
                return null;
            string depatureDate = reservation.FlightSegments.FirstOrDefault(k => !k.IsNullOrEmpty() && !k.FlightSegment.IsNullOrEmpty() && !k.FlightSegment.DepartureDateTime.IsNullOrEmpty()).FlightSegment.DepartureDateTime;
            string arrivalDate = reservation.FlightSegments.LastOrDefault(k => !k.IsNullOrEmpty() && !k.FlightSegment.IsNullOrEmpty() && !k.FlightSegment.ArrivalDateTime.IsNullOrEmpty()).FlightSegment.ArrivalDateTime;
            string getTitle = string.Empty;
            if (!depatureDate.IsNullOrEmpty() && !arrivalDate.IsNullOrEmpty())
            {
                DateTime depatureDateObj;
                DateTime arrivalDateObj;
                DateTimeFormatInfo getMonth = new DateTimeFormatInfo();

                DateTime.TryParse(depatureDate, out depatureDateObj);
                string departMonth = !depatureDateObj.IsNullOrEmpty() ? getMonth.GetMonthName(depatureDateObj.Month).Substring(0, 3) : string.Empty;

                if (journeyType.Equals("Roundtrip"))
                {
                    DateTime.TryParse(arrivalDate, out arrivalDateObj);
                    string arrivalMonth = !arrivalDateObj.IsNullOrEmpty() ? getMonth.GetMonthName(arrivalDateObj.Month).Substring(0, 3) : string.Empty;
                    if (departMonth.Equals(arrivalMonth, StringComparison.InvariantCultureIgnoreCase) && depatureDateObj.Day.Equals(arrivalDateObj.Day))
                    {
                        getTitle = departMonth + " " + depatureDateObj.Day.ToString();
                    }
                    else if (departMonth.Equals(arrivalMonth, StringComparison.InvariantCultureIgnoreCase))
                    {
                        getTitle = departMonth + " " + depatureDateObj.Day.ToString() + " - " + arrivalDateObj.Day.ToString();
                    }
                    else
                    {
                        getTitle = departMonth + " " + depatureDateObj.Day.ToString() + " - " + arrivalMonth + " " + arrivalDateObj.Day.ToString();
                    }
                }
                else
                {
                    getTitle = departMonth + " " + depatureDateObj.Day.ToString();
                }

                return getTitle;
            }
            return getTitle;
        }

        private string GetFareLockTitle(United.Service.Presentation.ReservationModel.Reservation reservation, string journeyType)
        {
            if (reservation.IsNullOrEmpty() || journeyType.IsNullOrEmpty())
                return string.Empty;

            string tripBuild = string.Empty;
            var arrivalAirportCode = reservation.FlightSegments.LastOrDefault(k => !k.IsNullOrEmpty() && !k.FlightSegment.IsNullOrEmpty()).FlightSegment;
            reservation.FlightSegments.ForEach(p =>
            {
                if (p != null && !p.FlightSegment.IsNullOrEmpty() && !p.FlightSegment.DepartureAirport.IsNullOrEmpty() && !p.FlightSegment.ArrivalAirport.IsNullOrEmpty())
                {
                    if (journeyType.Equals("Multicity"))
                    {
                        tripBuild = tripBuild.IsNullOrEmpty() ? p.FlightSegment.DepartureAirport.IATACode + " to " + p.FlightSegment.ArrivalAirport.IATACode
                                                              : tripBuild + "/" + p.FlightSegment.DepartureAirport.IATACode + " to " + p.FlightSegment.ArrivalAirport.IATACode;
                    }
                    else if (journeyType.Equals("Roundtrip"))
                    {
                        if (tripBuild.IsNullOrEmpty())
                        {
                            tripBuild = p.FlightSegment.DepartureAirport.IATACode + " - " + p.FlightSegment.ArrivalAirport.IATACode;
                        }
                    }
                    else
                    {
                        if (tripBuild.IsNullOrEmpty())
                        {
                            string arrivalAirport = !arrivalAirportCode.IsNullOrEmpty() && !arrivalAirportCode.ArrivalAirport.IsNullOrEmpty() ? arrivalAirportCode.ArrivalAirport.IATACode : p.FlightSegment.ArrivalAirport.IATACode;
                            tripBuild = p.FlightSegment.DepartureAirport.IATACode + " - " + arrivalAirport;
                        }
                    }
                }
            });
            return tripBuild;
        }

        public List<List<MOBSHOPTax>> GetTaxAndFeesAfterPriceChange(List<United.Services.FlightShopping.Common.DisplayCart.DisplayPrice> prices, bool isReshopChange = false, int appId = 0, string appVersion = "", string travelType = null)
        {
            List<List<MOBSHOPTax>> taxsAndFees = new List<List<MOBSHOPTax>>();
            CultureInfo ci = null;
            decimal taxTotal = 0.0M;
            decimal subTaxTotal = 0.0M;
            bool isTravelerPriceDirty = false;
            bool isEnableOmniCartMVP2Changes = _configuration.GetValue<bool>("EnableOmniCartMVP2Changes");

            foreach (var price in prices)
            {
                List<MOBSHOPTax> tmpTaxsAndFees = new List<MOBSHOPTax>();

                subTaxTotal = 0;

                if (_configuration.GetValue<bool>("EnableAdvanceSearchCouponBooking") && !string.IsNullOrEmpty(price?.Type) && price.Type.ToUpper() == "NONDISCOUNTPRICE-TRAVELERPRICE")
                    continue;

                if (price.SubItems != null && price.SubItems.Count > 0 && (!isReshopChange || (isReshopChange && price.Type.ToUpper() == "TRAVELERPRICE" && !isTravelerPriceDirty))) // Added by Hasnan - # 167553 - 10/04/2017
                {
                    foreach (var subItem in price.SubItems)
                    {
                        if (ci == null)
                        {
                            ci = TopHelper.GetCultureInfo(subItem.Currency);
                        }
                        MOBSHOPTax taxNfee = new MOBSHOPTax();
                        taxNfee = new MOBSHOPTax();
                        taxNfee.CurrencyCode = subItem.Currency;
                        taxNfee.Amount = subItem.Amount;
                        taxNfee.DisplayAmount = formatAmountForDisplay(taxNfee.Amount, ci, false);
                        taxNfee.TaxCode = subItem.Type;
                        taxNfee.TaxCodeDescription = subItem.Description;
                        isTravelerPriceDirty = true;
                        tmpTaxsAndFees.Add(taxNfee);

                        subTaxTotal += taxNfee.Amount;
                    }
                }

                if (tmpTaxsAndFees != null && tmpTaxsAndFees.Count > 0)
                {
                    //add new label as first item for UI
                    MOBSHOPTax tnf = new MOBSHOPTax();
                    tnf.CurrencyCode = tmpTaxsAndFees[0].CurrencyCode;
                    tnf.Amount = subTaxTotal;
                    tnf.DisplayAmount = formatAmountForDisplay(tnf.Amount, ci, false);
                    tnf.TaxCode = "PERPERSONTAX";
                    string description = price?.Description;
                    if (_shoppingUtility.EnableShoppingcartPhase2ChangesWithVersionCheck(appId, appVersion) && !isReshopChange && !string.IsNullOrEmpty(travelType) && (travelType == TravelType.CB.ToString() || travelType == TravelType.CLB.ToString()))
                    {
                        description = _shoppingUtility.BuildPaxTypeDescription(price?.PaxTypeCode, price?.Description, price.Count);
                    }
                    tnf.TaxCodeDescription = string.Format("{0} {1}: {2}{3}", price.Count, description.ToLower(), tnf.DisplayAmount,
                        isEnableOmniCartMVP2Changes ? "/person" : " per person");
                    tmpTaxsAndFees.Insert(0, tnf);
                }
                taxTotal += subTaxTotal * price.Count;
                if (tmpTaxsAndFees.Count > 0)
                {
                    taxsAndFees.Add(tmpTaxsAndFees);
                }

            }
            if (taxsAndFees != null && taxsAndFees.Count > 0)
            {
                //add grand total for all taxes
                List<MOBSHOPTax> lstTnfTotal = new List<MOBSHOPTax>();
                MOBSHOPTax tnfTotal = new MOBSHOPTax();
                tnfTotal.CurrencyCode = taxsAndFees[0][0].CurrencyCode;
                tnfTotal.Amount += taxTotal;
                tnfTotal.DisplayAmount = formatAmountForDisplay(tnfTotal.Amount, ci, false);
                tnfTotal.TaxCode = "TOTALTAX";
                tnfTotal.TaxCodeDescription = "Taxes and fees total";
                lstTnfTotal.Add(tnfTotal);
                taxsAndFees.Add(lstTnfTotal);
            }

            return taxsAndFees;
        }

        private string formatAmountForDisplay(decimal amt, CultureInfo ci, /*string currency,*/ bool roundup = true, bool isAward = false)
        {
            return FormatAmountForDisplay(amt.ToString(), ci, roundup, isAward);
        }


        private async Task<List<MOBSHOPPrice>> GetPrices(List<United.Services.FlightShopping.Common.DisplayCart.DisplayPrice> prices, bool isAwardBooking, string sessionId, bool isReshopChange = false, string searchType = null, bool isFareLockViewRes = false, bool isCorporateFare = false, int appId = 0, string appVersion = "", FlightReservationResponse shopBookingDetailsResponse = null, Session session = null)
        {
            List<MOBSHOPPrice> bookingPrices = new List<MOBSHOPPrice>();
            CultureInfo ci = null;
            bool isEnableOmniCartMVP2Changes = _configuration.GetValue<bool>("EnableOmniCartMVP2Changes");
            foreach (var price in prices)
            {
                if (ci == null)
                {
                    ci = _eTCUtility.GetCultureInfo(price.Currency);
                }

                MOBSHOPPrice bookingPrice = new MOBSHOPPrice();
                decimal NonDiscountTravelPrice = 0;
                double promoValue = 0;
                if (_configuration.GetValue<bool>("EnableAdvanceSearchCouponBooking"))
                {
                    if (price.Type.Equals("NONDISCOUNTPRICE-TRAVELERPRICE", StringComparison.OrdinalIgnoreCase) || price.Type.Equals("NonDiscountPrice-Total", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    if (price.Type.Equals("TRAVELERPRICE", StringComparison.OrdinalIgnoreCase) || (price.Type.Equals("TOTAL", StringComparison.OrdinalIgnoreCase)))
                    {
                        if (price.Type.Equals("TRAVELERPRICE", StringComparison.OrdinalIgnoreCase))
                        {
                            var nonDiscountedPrice = prices.Find(dp => dp.PaxTypeCode == price.PaxTypeCode && dp.Type.ToUpper().Equals("NONDISCOUNTPRICE-TRAVELERPRICE"));
                            var discountedPrice = prices.Find(dp => dp.PaxTypeCode == price.PaxTypeCode && dp.Type.ToUpper().Equals("TRAVELERPRICE"));
                            if (discountedPrice != null && nonDiscountedPrice != null)
                            {
                                promoValue = Math.Round(Convert.ToDouble(nonDiscountedPrice.Amount)
                                             - Convert.ToDouble(discountedPrice.Amount), 2, MidpointRounding.AwayFromZero);
                                NonDiscountTravelPrice = nonDiscountedPrice.Amount;
                            }
                            else
                            {
                                promoValue = 0;
                            }
                        }
                        if (price.Type.Equals("TOTAL", StringComparison.OrdinalIgnoreCase))
                        {
                            var nonDiscountedTotalPrice = prices.Find(dp => dp.PaxTypeCode == price.PaxTypeCode && dp.Type.ToUpper().Equals("NONDISCOUNTPRICE-TOTAL"));
                            var discountedTotalPrice = prices.Find(dp => dp.PaxTypeCode == price.PaxTypeCode && dp.Type.ToUpper().Equals("TOTAL"));
                            if (discountedTotalPrice != null && nonDiscountedTotalPrice != null)
                            {
                                promoValue = Math.Round(Convert.ToDouble(nonDiscountedTotalPrice.Amount)
                                            - Convert.ToDouble(discountedTotalPrice.Amount), 2, MidpointRounding.AwayFromZero);
                                promoValue = _shoppingUtility.UpdatePromoValueForFSRMoneyMiles(prices, session, promoValue);
                            }
                            else
                            {
                                promoValue = 0;
                            }
                        }
                        bookingPrice.PromoDetails = promoValue > 0 ? new MOBPromoCode
                        {
                            PriceTypeDescription = price.Type.Equals("TOTAL", StringComparison.OrdinalIgnoreCase) ? _configuration.GetValue<string>("PromoSavedText") : _configuration.GetValue<string>("PromoCodeAppliedText"),
                            PromoValue = Math.Round(promoValue, 2, MidpointRounding.AwayFromZero),
                            FormattedPromoDisplayValue = "-" + promoValue.ToString("C2", CultureInfo.CurrentCulture)
                        } : null;
                    }
                }

                bookingPrice.CurrencyCode = price.Currency;
                bookingPrice.DisplayType = price.Type;
                bookingPrice.Status = price.Status;
                bookingPrice.Waived = price.Waived;
                bookingPrice.DisplayValue = string.Format("{0:#,0.00}", price.Amount);
                if (_configuration.GetValue<bool>("EnableCouponsforBooking") && !string.IsNullOrEmpty(price.PaxTypeCode))
                {
                    bookingPrice.PaxTypeCode = price.PaxTypeCode;
                }
                if (!string.IsNullOrEmpty(searchType))
                {
                    string desc = string.Empty;
                    if (price.Description != null && price.Description.Length > 0)
                    {
                        if (!ConfigUtility.EnableYADesc(isReshopChange) || (price.PricingPaxType != null && !price.PricingPaxType.Equals("UAY")))
                        {
                            desc = UtilityHelper.GetFareDescription(price);
                        }
                    }
                    if (ConfigUtility.EnableYADesc(isReshopChange) && !string.IsNullOrEmpty(price.PricingPaxType) && price.PricingPaxType.ToUpper().Equals("UAY"))
                    {
                        bookingPrice.PriceTypeDescription = UtilityHelper.BuildYAPriceTypeDescription(searchType);
                        bookingPrice.PaxTypeDescription = $"{price?.Count} {"young adult (18-23)"}".ToLower();
                    }
                    else
                    if (price.Description.ToUpper().Contains("TOTAL"))
                    {
                        bookingPrice.PriceTypeDescription = price?.Description.ToLower(); 
                        if (prices.Any(x => x.Type.Equals("TravelCredits", StringComparison.OrdinalIgnoreCase) && x.Amount > 0)
                            && price.StrikeThroughPricing > 0 && (int)price.StrikeThroughPricing >= (int)price.Amount)
                        {
                            bookingPrice.StrikeThroughDisplayValue = ((int)price.StrikeThroughPricing - (int)price.Amount).ToString("C2", CultureInfo.CurrentCulture);
                        }
                    }
                    else
                    {
                        // bookingPrice.PriceTypeDescription = UtilityHelper.BuildPriceTypeDescription(searchType, price.Description, price.Count, desc, isFareLockViewRes, isCorporateFare);
                        if (_configuration.GetValue<bool>("EnableAwardStrikeThroughPricing") && session.IsAward
                                && session != null && session.CatalogItems != null && session.CatalogItems.Count > 0 && session.CatalogItems.FirstOrDefault(a => a.Id == ((int)IOSCatalogEnum.AwardStrikeThroughPricing).ToString() || a.Id == ((int)AndroidCatalogEnum.AwardStrikeThroughPricing).ToString())?.CurrentValue == "1"
                                && price.StrikeThroughPricing > 0 && (int)price.StrikeThroughPricing != (int)price.Amount
                          )
                        {
                            bookingPrice.StrikeThroughDisplayValue = FormatAwardAmountForDisplay(price.StrikeThroughPricing.ToString(), true);
                            bookingPrice.StrikeThroughDescription = BuildStrikeThroughDescription();
                        }
                        bookingPrice.PriceTypeDescription = UtilityHelper.BuildPriceTypeDescription(searchType, price.Description, price.Count, desc, isFareLockViewRes, isCorporateFare);
                        if (isEnableOmniCartMVP2Changes)
                        {
                            string description = price?.Description;
                            if (_shoppingUtility.EnableShoppingcartPhase2ChangesWithVersionCheck(appId, appVersion) && !isReshopChange && !string.IsNullOrEmpty(session?.TravelType) && (session.TravelType == TravelType.CB.ToString() || session.TravelType == TravelType.CLB.ToString()))
                            {
                                description = _shoppingUtility.BuildPaxTypeDescription(price?.PaxTypeCode, price?.Description, price.Count);
                            }
                            bookingPrice.PaxTypeDescription = $"{price?.Count} {description}".ToLower();
                        }

                    }
                }

                if (!isReshopChange)
                {
                    if (!string.IsNullOrEmpty(bookingPrice.DisplayType) && bookingPrice.DisplayType.Equals("MILES") && isAwardBooking && !string.IsNullOrEmpty(sessionId))
                    {
                        await _shoppingUtility.ValidateAwardMileageBalance(sessionId, price.Amount);
                    }
                }

                double tempDouble = 0;
                double.TryParse(NonDiscountTravelPrice > 0 ? NonDiscountTravelPrice.ToString() : price.Amount.ToString(), out tempDouble);
                bookingPrice.Value = Math.Round(tempDouble, 2, MidpointRounding.AwayFromZero);

                if (price.Currency.ToUpper() == "MIL")
                {
                    bookingPrice.FormattedDisplayValue = FormatAwardAmountForDisplay(price.Amount.ToString(), false);
                }
                else
                {
                    bookingPrice.FormattedDisplayValue = FormatAmountForDisplay((NonDiscountTravelPrice > 0 ? NonDiscountTravelPrice : price.Amount).ToString(), ci, false);

                    if (_configuration.GetValue<bool>("EnableMilesPlusMoney") && string.Equals("MILESANDMONEY", price.Type, StringComparison.OrdinalIgnoreCase))
                    {
                        bookingPrice.FormattedDisplayValue = "-" + FormatAmountForDisplay((price.Amount).ToString(), ci, false); // Money and Miles have to be displayed in -ve format as per MOBILE-14807
                    }
                }
                _shoppingUtility.UpdatePricesForTravelCredits(bookingPrices, price, bookingPrice, session);
                bookingPrices.Add(bookingPrice);
            }

            return bookingPrices;
        }

        private string FormatAwardAmountForDisplay(string amt, bool truncate = true)
        {
            string newAmt = amt;
            decimal amount = 0;
            decimal.TryParse(amt, out amount);

            try
            {
                if (amount > -1)
                {
                    if (truncate)
                    {
                        try
                        {
                            if (amount > 999)
                            {
                                amount = amount / 1000;
                                if (amount % 1 > 0)
                                    newAmt = string.Format("{0:n1}", amount) + "K miles";
                                else
                                    newAmt = string.Format("{0:n0}", amount) + "K miles";
                            }
                            else
                            {
                                newAmt = string.Format("{0:n0}", amount) + " miles";
                            }
                        }
                        catch { }
                    }
                    else
                    {
                        try
                        {
                            newAmt = string.Format("{0:n0}", amount) + " miles";
                        }
                        catch { }
                    }
                }

            }
            catch { }

            return newAmt;
        }

        private string FormatAmountForDisplay(string amt, CultureInfo ci, bool roundup = true, bool isAward = false)
        {
            string newAmt = amt;
            decimal amount = 0;
            decimal.TryParse(amt, out amount);

            try
            {
                RegionInfo ri = new RegionInfo(ci.Name);

                switch (ri.ISOCurrencySymbol.ToUpper())
                {
                    case "JPY":
                    case "EUR":
                    case "CAD":
                    case "GBP":
                    case "CNY":
                    case "USD":
                    case "AUD":
                    default:
                        newAmt = TopHelper.GetCurrencySymbol(ci, amount, roundup);
                        break;
                }

            }
            catch { }

            return isAward ? "+ " + newAmt : newAmt;
        }

        private bool IsCorporateTraveler(Collection<Characteristic> characteristics)
        {
            if (!characteristics.IsNullOrEmpty())
            {
                return characteristics.Any(c => !c.Code.IsNullOrEmpty() && c.Code.Equals("IsValidCorporateTravel", StringComparison.OrdinalIgnoreCase) &&
                                          !c.Value.IsNullOrEmpty() && c.Value.Equals("True"));
            }
            return false;
        }

        private async Task AssignProfileSavedETCsFromPersist(string sessionId, MOBShoppingCart shoppingCart)
        {
            if (_configuration.GetValue<bool>("SavedETCToggle"))
            {
                // var persistShopingCart = //FilePersist.Load<MOBShoppingCart>(sessionId, shoppingCart.GetType().ToString());
                var persistShopingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(sessionId, shoppingCart.ObjectName, new List<string> { sessionId, shoppingCart.ObjectName }).ConfigureAwait(false);
                if (persistShopingCart?.ProfileTravelerCertificates != null)
                {
                    shoppingCart.ProfileTravelerCertificates = persistShopingCart.ProfileTravelerCertificates;
                }
            }
        }

        private bool IsPostBookingPromoCodeEnabled(int applicationId, string appVersion)
        {
            if (_configuration.GetValue<bool>("EnableCouponsInPostBooking") && GeneralHelper.IsApplicationVersionGreaterorEqual(applicationId, appVersion, "Android_EnablePromoCodePostBooking_AppVersion", "iPhone_EnablePromoCodePostBooking_AppVersion"))
            {
                return true;
            }
            return false;
        }

        private void IsHidePromoOption(MOBShoppingCart shoppingCart, string flow, MOBApplication application, bool IsReshopChange)
        {
            string[] productCodes;
            switch (flow)
            {
                case "POSTBOOKING":
                    if (IsPostBookingPromoCodeEnabled(application.Id, application.Version.Major))
                    {
                        productCodes = _configuration.GetValue<string>("PostBookingPromocodeEligibleProducts").Split('|');
                        if (shoppingCart.PromoCodeDetails == null)
                        {
                            shoppingCart.PromoCodeDetails = new MOBPromoCodeDetails();
                        }
                        //To know if it is Postbooking after Reshop                   
                        if (IsReshopChange)
                        {
                            shoppingCart.PromoCodeDetails.IsHidePromoOption = true;
                        }
                        else
                        {
                            if (shoppingCart?.Products != null
                                && !shoppingCart.Products.Any(p => productCodes.Contains(p.Code)))
                            {
                                shoppingCart.PromoCodeDetails.IsHidePromoOption = true;
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private bool IsCouponEligibleProduct(List<MOBProdDetail> proddetail)
        {
            string couponProductCode = _configuration.GetValue<string>("IsCouponEligibleProduct") ?? string.Empty;

            if (!couponProductCode.IsNullOrEmpty())
            {
                var couponProductCodeList = couponProductCode.Split('|');
                foreach (var c in proddetail)
                {

                    if (couponProductCodeList.Any(t => t.Equals(c.Code)))
                    {
                        return true;
                    }

                }
            }

            return false;
        }

        public static int GetCartItemsCount(MOBShoppingCart shoppingcart)
        {
            int itemsCount = 0;
            if (shoppingcart?.Products != null && shoppingcart.Products.Count > 0)
            {
                shoppingcart.Products.ForEach(product =>
                {
                    if (!string.IsNullOrEmpty(product.ProdTotalPrice) && Decimal.TryParse(product.ProdTotalPrice, out decimal totalprice) && (totalprice > 0 
                                   || product.Code == "RES" && totalprice == 0))
                    {
                        if (product?.Segments != null && product.Segments.Count > 0)
                        {
                            product.Segments.ForEach(segment =>
                            {
                                segment.SubSegmentDetails.ForEach(subSegment =>
                                {
                                    if (subSegment != null)
                                    {
                                        if (product.Code == "SEATASSIGNMENTS")
                                        {
                                            itemsCount += subSegment.PaxDetails.Count();
                                        }
                                        else
                                        {
                                            itemsCount += 1;
                                        }
                                    }
                                });

                            });
                            return;
                        }
                        itemsCount += 1;
                    }
                });
            }
            return itemsCount;
        }

        private bool IsFarelock(List<MOBProdDetail> products)
        {
            if (products != null)
            {
                if (products.Any(p => p.Code.ToUpper() == "FARELOCK" || p.Code.ToUpper() == "FLK"))
                {
                    return true;
                }
            }
            return false;
        }

        private string BuildStrikeThroughDescription()
        {
            return _configuration.GetValue<string>("StrikeThroughPriceTypeDescription");
        }
        public async Task<MOBProvisionResponse> CreateProvision(MOBProvisionRequest request)
        {
            string token = "";
            Session session = null;
            if (request.Flow == FlowType.BOOKING.ToString() || request.Flow == FlowType.POSTBOOKING.ToString() || request.Flow == FlowType.CHECKIN.ToString())
                session = await _shoppingSessionHelper.GetValidateSession(request.SessionId, true, false);
            else if (request.Flow == FlowType.RESHOP.ToString() || request.Flow == FlowType.VIEWRES.ToString() || request.Flow == FlowType.VIEWRES_SEATMAP.ToString())
                session = await _shoppingSessionHelper.GetValidateSession(request.SessionId, false, true);
            else
            {
                session = new Session
                {
                    DeviceID = request.DeviceId,
                    AppID = request.Application.Id,
                    SessionId = request.SessionId,
                    CreationTime = DateTime.Now,
                    LastSavedTime = DateTime.Now,
                    MileagPlusNumber = request.MileagePlusNumber,
                };
                await _dPService.GetAndSaveAnonymousToken(request.Application.Id, request.DeviceId, _configuration, "dpTokenRequest", session).ConfigureAwait(false);
                await _sessionHelperService.SaveSession<Session>(session, session.SessionId, new List<string> { session.SessionId, session.ObjectName }, session.ObjectName).ConfigureAwait(false);
            }

            if (session != null && !string.IsNullOrEmpty(session.Token))
                token = session.Token;
            else
                throw new MOBUnitedException("Could not find your booking session.");

            if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(request.MileagePlusNumber))
            {
                var _cslReq = new ProvisionCSLRequest()
                {
                    ChannelName = _configuration.GetValue<string>("Shopping - ChannelType"),
                    FailureRedirectUrl = request.Application?.Id == 1 ? _configuration.GetValue<string>("Chase_IOS - Failure") : _configuration.GetValue<string>("Chase - Failure"),
                    PartnerRequestIdentifier = Guid.NewGuid().ToString(),
                    SuccessRedirectUrl = request.Application?.Id == 1 ? _configuration.GetValue<string>("Chase_IOS - Success") : _configuration.GetValue<string>("Chase - Success"),
                    MPNumber = request.MileagePlusNumber
                };

                string path = "/CreateProvisionRequest";
                var jsonResponse = await _provisionService.CSL_PartnerProvisionCall(token, path, JsonConvert.SerializeObject(_cslReq));

                if (!string.IsNullOrEmpty(jsonResponse))
                {
                    MOBProvisionResponse mobProvisionResponse = JsonConvert.DeserializeObject<MOBProvisionResponse>(jsonResponse);
                    mobProvisionResponse.PartnerRequestIdentifier = _cslReq.PartnerRequestIdentifier;

                    if (mobProvisionResponse != null && mobProvisionResponse.Errors != null && mobProvisionResponse.Errors.Count > 0)
                    {
                        throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                    }
                    else
                    {
                        mobProvisionResponse.SessionId = request.SessionId;
                        mobProvisionResponse.TransactionId = request.TransactionId;
                        return await System.Threading.Tasks.Task.FromResult(mobProvisionResponse);
                    }
                }
                else
                {
                    throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                }
            }
            else
            {
                throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }
        }

        public async Task<MOBProvisionAccountDetails> GetProvisionDetails(MOBProvisionRequest request)
        {
            MOBProvisionAccountDetails response = new MOBProvisionAccountDetails();

            string token = "";
            Session session = null;
            if (request.Flow == FlowType.BOOKING.ToString() || request.Flow == FlowType.POSTBOOKING.ToString() || request.Flow == FlowType.CHECKIN.ToString())
                session = await _shoppingSessionHelper.GetValidateSession(request.SessionId, true, false);
            else if (request.Flow == FlowType.RESHOP.ToString() || request.Flow == FlowType.VIEWRES.ToString() || request.Flow == FlowType.VIEWRES_SEATMAP.ToString())
                session = await _shoppingSessionHelper.GetValidateSession(request.SessionId, false, true);
            else
                session = await _shoppingSessionHelper.GetValidateSession(request.SessionId, false, false);

            if (session != null)
                token = session.Token;
            else
                throw new MOBUnitedException("Could not find your booking session.");

            var _cslreq = new GetProvisionCSLRequest() { ChannelName = _configuration.GetValue<string>("Shopping - ChannelType"), MPNumber = request.MileagePlusNumber, ProvisionRequestIdentifier = request.ProvisionRequestIdentifier, PartnerRequestIdentifier = request.PartnerRequestIdentifier };

            if (!string.IsNullOrEmpty(request.MileagePlusNumber))
            {
                string path = "/GetProvisionDetails";
                var jsonResponse = await _provisionService.CSL_PartnerProvisionCall(token, path, JsonConvert.SerializeObject(_cslreq));
                response = await DeserialiseAndBuildMOBProvisionAccountDetails(session.SessionId, token, jsonResponse, request.Flow);
            }
            else
            {
                throw new MOBUnitedException("MPNumber required for Chase provisioning...");
            }

            //response.Flow = request.Flow;
            response.ProvisionRequestIdentifier = request.ProvisionRequestIdentifier;
            response.PartnerRequestIdentifier = request.PartnerRequestIdentifier;
            return await System.Threading.Tasks.Task.FromResult(response);
        }

        private async Task<MOBProvisionAccountDetails> DeserialiseAndBuildMOBProvisionAccountDetails(string sessionId, string token, string jsonResponse, string flow)
        {
            MOBProvisionAccountDetails response = new MOBProvisionAccountDetails();

            if (!string.IsNullOrEmpty(jsonResponse))
            {
                MOBGetProvisionDetailsResponse getProvisionDetailsResponse = JsonConvert.DeserializeObject<MOBGetProvisionDetailsResponse>(jsonResponse);
                if(getProvisionDetailsResponse != null && getProvisionDetailsResponse.Errors != null && getProvisionDetailsResponse.Errors.Count > 0)
                {
                    string title = "RTI_CHASE_PROVISION_UNABLE_TO_LINK_CARD";
                    response.MOBMobileCMSContentMessages = await GetSDLCMSContentMesseges(token, _headers.ContextValues.SessionId, title);
                }
                else if (getProvisionDetailsResponse != null && getProvisionDetailsResponse.AccountInfo != null && getProvisionDetailsResponse.AddressInfo != null)
                {
                    var accountInfo = getProvisionDetailsResponse.AccountInfo;
                    var addressInfo = getProvisionDetailsResponse.AddressInfo;

                    ProfileCreditCardItem existingCreditCardItem = null;
                    if (flow == FlowType.BOOKING.ToString() || flow == FlowType.RESHOP.ToString() || flow == FlowType.VIEWRES.ToString() || flow == FlowType.VIEWRES_SEATMAP.ToString() || flow == FlowType.CHECKIN.ToString() || flow == FlowType.POSTBOOKING.ToString())
                    {
                        CreditCardDataReponseModel creditCardDataReponseModel = new CreditCardDataReponseModel();
                        creditCardDataReponseModel = await _sessionHelperService.GetSession<CreditCardDataReponseModel>(sessionId, ObjectNames.CSLProfileCreditCardsResponse, new List<string> { sessionId, ObjectNames.CSLProfileCreditCardsResponse }).ConfigureAwait(false);
                        existingCreditCardItem = new ProfileCreditCardItem();
                        existingCreditCardItem = creditCardDataReponseModel?.CreditCards?.Where(x => x.PersistentToken == accountInfo.PersistentToken && x.AccountNumberLastFourDigits == accountInfo.AccountCode && x.ExpYear == Convert.ToInt32(accountInfo.ExpirationYear) && x.ExpMonth == Convert.ToInt32(accountInfo.ExpirationMonth)).FirstOrDefault();
                    }
                    else
                    {
                        string transId = string.IsNullOrEmpty(_headers.ContextValues.TransactionId) ? "trans0" : _headers.ContextValues.TransactionId;
                        var obj = await _cachingService.GetCache<GetCreditCardDataModel>(sessionId + "GetCreditCardDataModel", transId);
                        GetCreditCardDataModel getCreditCardDataModel = JsonConvert.DeserializeObject<GetCreditCardDataModel>(obj);
                        existingCreditCardItem = new ProfileCreditCardItem();
                        existingCreditCardItem = getCreditCardDataModel?.CreditCards?.Where(x => x.PersistentToken == accountInfo.PersistentToken && x.AccountNumberLastFourDigits == accountInfo.AccountCode && x.ExpYear == Convert.ToInt32(accountInfo.ExpirationYear) && x.ExpMonth == Convert.ToInt32(accountInfo.ExpirationMonth)).Select(x => new ProfileCreditCardItem()
                        {
                            AccountNumberToken = x.AccountNumberToken,
                            Code = x.Code,
                            CCTypeDescription = x.CCTypeDescription,
                            ExpMonth = x.ExpMonth,
                            ExpYear = x.ExpYear,
                            Payor = new Person { GivenName = x.Payor.GivenName },
                            AccountNumberLastFourDigits = x.AccountNumberLastFourDigits,
                            PersistentToken = x.PersistentToken,
                            AddressKey = x.AddressKey,
                            Key = x.Key

                        }).FirstOrDefault();
                    }

                    if (existingCreditCardItem != null)
                    {
                        response.AccountReferenceIdentifier = accountInfo.AccountReferenceIdentifier;
                        response.SelectedCreditCard = new MOBCreditCard();
                        response.SelectedCreditCard.AccountNumberToken = accountInfo.AccountNumberToken;
                        response.SelectedCreditCard.CardType = accountInfo.CardType;
                        response.SelectedCreditCard.CardTypeDescription = accountInfo.CardTypeName;
                        response.SelectedCreditCard.ExpireMonth = accountInfo.ExpirationMonth.ToCharArray()[0] == '0' ? accountInfo.ExpirationMonth.Remove(0, 1) : accountInfo.ExpirationMonth;
                        response.SelectedCreditCard.ExpireYear = accountInfo.ExpirationYear;
                        response.SelectedCreditCard.CCName = addressInfo.FullName;
                        response.SelectedCreditCard.DisplayCardNumber = "XXXXXXXXXXXX" + accountInfo.AccountCode;
                        response.SelectedCreditCard.UnencryptedCardNumber = "XXXXXXXXXXXX" + accountInfo.AccountCode;
                        response.SelectedCreditCard.PersistentToken = accountInfo.PersistentToken;
                        response.SelectedCreditCard.Key = existingCreditCardItem.Key;
                        response.SelectedCreditCard.AddressKey = addressInfo.AddressKey;
                        response.IsExistCreditCard = true;

                        string title = string.IsNullOrEmpty(flow) ? "RTI_CHASE_PROVISION_CARD_ALREADY_IN_WALLET" : "RTI_CHASE_PROVISION_CARD_ALREADY_IN_ALLFLOWS";
                        response.MOBMobileCMSContentMessages = await GetSDLCMSContentMesseges(token, _headers.ContextValues.SessionId, title);
                        response.SelectedAddress = new MOBAddress();
                        response.SelectedAddress.Key = addressInfo.AddressKey;
                        response.SelectedAddress.City = addressInfo.AddressCityName;
                        response.SelectedAddress.Country = new MOBCountry();
                        response.SelectedAddress.Country.Code = addressInfo.AddressCountryCode;
                        response.SelectedAddress.PostalCode = addressInfo.AddressPostalCode;
                        response.SelectedAddress.State = new MOBState();
                        response.SelectedAddress.State.Code = addressInfo.AddressStateCode;
                        response.SelectedAddress.Line1 = addressInfo.AddressLine1;
                    }
                    else
                    {
                        response.AccountReferenceIdentifier = accountInfo.AccountReferenceIdentifier;
                        response.SelectedCreditCard = new MOBCreditCard();
                        response.SelectedCreditCard.AccountNumberToken = accountInfo.AccountNumberToken;
                        response.SelectedCreditCard.CardType = accountInfo.CardType;
                        response.SelectedCreditCard.CardTypeDescription = accountInfo.CardTypeName;
                        response.SelectedCreditCard.ExpireMonth = accountInfo.ExpirationMonth.ToCharArray()[0] == '0' ? accountInfo.ExpirationMonth.Remove(0, 1) : accountInfo.ExpirationMonth;
                        response.SelectedCreditCard.ExpireYear = accountInfo.ExpirationYear;
                        response.SelectedCreditCard.CCName = addressInfo.FullName;
                        response.SelectedCreditCard.DisplayCardNumber = "XXXXXXXXXXXX" + accountInfo.AccountCode;
                        response.SelectedCreditCard.PersistentToken = accountInfo.PersistentToken;
                        response.SelectedCreditCard.UnencryptedCardNumber = "XXXXXXXXXXXX" + accountInfo.AccountCode;
                        response.SelectedCreditCard.AddressKey = addressInfo.AddressKey;

                        response.SelectedAddress = new MOBAddress();
                        response.SelectedAddress.Key = addressInfo.AddressKey;
                        response.SelectedAddress.City = addressInfo.AddressCityName;
                        response.SelectedAddress.Country = new MOBCountry();
                        response.SelectedAddress.Country.Code = addressInfo.AddressCountryCode;
                        response.SelectedAddress.PostalCode = addressInfo.AddressPostalCode;
                        response.SelectedAddress.State = new MOBState();
                        response.SelectedAddress.State.Code = addressInfo.AddressStateCode;
                        response.SelectedAddress.Line1 = addressInfo.AddressLine1;

                        response.ProvisionSuccessMessage = _configuration.GetValue<string>("ProvisionSuccessMessage");
                    }
                }
            }
            else
            {
                throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }

            return response;
        }

        private async Task<List<MOBMobileCMSContentMessages>> GetSDLCMSContentMesseges(string token, string sessionId, string title)
        {
            List<MOBMobileCMSContentMessages> mobileCMSContentMessages = new List<MOBMobileCMSContentMessages>();
            
            MOBCSLContentMessagesRequest cslContentReqeust = new MOBCSLContentMessagesRequest();
            cslContentReqeust.Lang = "en";
            cslContentReqeust.Pos = "us";
            cslContentReqeust.Channel = "mobileapp";
            cslContentReqeust.Listname = new List<string>();
            cslContentReqeust.Usecache = false;
            cslContentReqeust.LocationCodes = new List<string>();
            cslContentReqeust.LocationCodes.Add(title);
            cslContentReqeust.Groupname = "Booking:RTI";
            cslContentReqeust.Listname = new List<string>();
            cslContentReqeust.Listname.Add("Messages");

            string jsonRequest = JsonConvert.SerializeObject(cslContentReqeust);

            MOBCSLContentMessagesResponse lstMessages = await _cMSContentService.GetSDLContentByGroupName<MOBCSLContentMessagesResponse>(token, "message", jsonRequest, sessionId).ConfigureAwait(false);

            mobileCMSContentMessages = GetSDLMessageFromList(lstMessages?.Messages, title);

            return mobileCMSContentMessages;
        }

        public async Task<MOBUpdateProvisionLinkStatusResponse> UpdateProvisionLinkStatus(MOBProvisionRequest request)
        {
            string token = "";
            Session session = null;
            if (request.Flow == FlowType.BOOKING.ToString() || request.Flow == FlowType.POSTBOOKING.ToString() || request.Flow == FlowType.CHECKIN.ToString())
                session = await _shoppingSessionHelper.GetValidateSession(request.SessionId, true, false);
            else if (request.Flow == FlowType.RESHOP.ToString() || request.Flow == FlowType.VIEWRES.ToString() || request.Flow == FlowType.VIEWRES_SEATMAP.ToString())
                session = await _shoppingSessionHelper.GetValidateSession(request.SessionId, false, true);
            else
                session = await _shoppingSessionHelper.GetValidateSession(request.SessionId, false, false);

            if (session != null && !string.IsNullOrEmpty(session.Token))
                token = session.Token;
            else
                throw new MOBUnitedException("Could not find your session.");

            if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(request.MileagePlusNumber))
            {
                var _cslReq = new
                {
                    AccountReferenceIdentifier = request.AccountReferenceIdentifier,
                    ChannelName = _configuration.GetValue<string>("Shopping - ChannelType"),
                    PartnerRequestIdentifier = request.PartnerRequestIdentifier,
                    CustomerIdentifier = request.MileagePlusNumber,
                    LinkageStatusCode = request.LinkageStatusCode,
                    MPNumber = request.MileagePlusNumber
                };
                string path = "/UpdateProvisionLinkStatus";
                var jsonResponse = await _provisionService.CSL_PartnerProvisionCall(token, path, JsonConvert.SerializeObject(_cslReq));

                if (!string.IsNullOrEmpty(jsonResponse))
                {
                    MOBUpdateProvisionLinkStatusResponse updateProvisionLinkStatusResponse = JsonConvert.DeserializeObject<MOBUpdateProvisionLinkStatusResponse>(jsonResponse);
                    if (updateProvisionLinkStatusResponse != null && updateProvisionLinkStatusResponse.Errors != null && updateProvisionLinkStatusResponse.Errors.Count > 0)
                    {
                        throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                    }
                    else
                    {
                        updateProvisionLinkStatusResponse.SessionId = request.SessionId;
                        updateProvisionLinkStatusResponse.TransactionId = request.TransactionId;
                        return await System.Threading.Tasks.Task.FromResult(updateProvisionLinkStatusResponse);
                    }
                }
                else
                {
                    throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                }
            }
            else
            {
                throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }
        }

    }
}
