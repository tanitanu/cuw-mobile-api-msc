using CrystalDecisions.CrystalReports.Engine;
using EmployeeRes.Common;
using IDAutomation.NetStandard.PDF417.FontEncoder;
using MerchandizingServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using United.Common.Helper.MSCPayment.Interfaces;
using United.CorporateDirect.Models.CustomerProfile;
using United.Definition;
using United.Definition.Booking;
using United.Definition.CSLModels.CustomerProfile;
using United.Definition.FormofPayment;
using United.Definition.Pcu;
using United.Definition.Shopping;
using United.Definition.Shopping.TripInsurance;
using United.Definition.Shopping.UnfinishedBooking;
using United.Definition.SSR;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.DynamoDB;
using United.Mobile.DataAccess.MSCPayment.Interfaces;
using United.Mobile.DataAccess.MSCPayment.Services;
using United.Mobile.DataAccess.Product.Interfaces;
using United.Mobile.Model.Common;
using United.Mobile.Model.DynamoDb.Common;
using United.Mobile.Model.MSC;
using United.Persist.Definition.FOP;
using United.Persist.Definition.Merchandizing;
using United.Persist.Definition.PriorityBoarding;
using United.Persist.Definition.SeatChange;
using United.Persist.Definition.Shopping;
using United.Refunds.Common.Models.Response;
using United.Service.Presentation.CommonModel;
using United.Service.Presentation.CustomerResponseModel;
using United.Service.Presentation.FlightResponseModel;
using United.Service.Presentation.PaymentModel;
using United.Service.Presentation.PaymentResponseModel;
using United.Service.Presentation.ProductModel;
using United.Service.Presentation.ProductRequestModel;
using United.Service.Presentation.ProductResponseModel;
using United.Service.Presentation.SegmentModel;
using United.Service.Presentation.ValueDocumentModel;
using United.Services.Customer.Common;
using United.Services.Customer.Preferences.Common;
using United.Services.FlightShopping.Common;
using United.Services.FlightShopping.Common.Cart;
using United.Services.FlightShopping.Common.DisplayCart;
using United.Services.FlightShopping.Common.Extensions;
using United.Services.FlightShopping.Common.FlightReservation;
using United.ShoppingCart.Model.ManagePayment;
using United.TravelBank.Model.BalancesDataModel;
using United.Utility.Helper;
using Address = United.Service.Presentation.CommonModel.Address;
using Characteristic = United.Service.Presentation.CommonModel.Characteristic;
using Country = United.Service.Presentation.CommonModel.Country;
using CslDataVaultRequest = United.Service.Presentation.PaymentRequestModel.DataVault<United.Service.Presentation.PaymentModel.Payment>;
using CslDataVaultResponse = United.Service.Presentation.PaymentResponseModel.DataVault<United.Service.Presentation.PaymentModel.Payment>;
using CslFutureFlightCredit = United.Service.Presentation.PaymentModel.FutureFlightCredit;
using FlightReservationResponse = United.Services.FlightShopping.Common.FlightReservation.FlightReservationResponse;
using FormOfPayment = United.Services.FlightShopping.Common.FlightReservation.FormOfPayment;
using Genre = United.Service.Presentation.CommonModel.Genre;
using MilesMoney = United.Service.Presentation.PaymentModel.MilesMoney;
using Task = System.Threading.Tasks.Task;

namespace United.Common.Helper.MSCPayment.Services
{
    public class CheckoutUtiliy : ICheckoutUtiliy
    {
        private readonly ICacheLog<CheckoutUtiliy> _logger;
        private readonly IConfiguration _configuration;
        private readonly IDataVaultService _dataVaultService;
        private readonly ISessionHelperService _sessionHelperService;
        private readonly IDynamoDBService _dynamoDBService;
        private readonly DocumentLibraryDynamoDB _documentLibraryDynamoDB;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IMSCBaggageInfo _mscBaggageInfoProvider;
        private readonly IDPService _dPService;
        private readonly ICustomerPreferencesService _customerPreferencesService;
        private readonly IMSCPkDispenserPublicKey _mSCPkDispenserPublicKey;
        private readonly IShoppingCartUtility _shoppingCartUtility;
        private readonly IMSCCheckoutService _mSCCheckoutService;
        private readonly IFlightShoppingService _flightShoppingService;
        private readonly ICachingService _cachingService;
        private readonly IPassDetailService _passDetailService;
        private readonly IGetBarcodeService _getBarcodeService;
        private readonly ILegalDocumentsForTitlesService _legalDocumentsForTitlesService;
        private readonly IFlightShoppingProductsService _flightShoppingProductsService;
        private readonly IETCUtility _eTCUtility;
        private readonly IMSCPageProductInfoHelper _mSCPageProductInfoHelper;
        private readonly IMerchandizingCSLService _merchandizingCSLService;
        private MOBApplication _application = new MOBApplication() { Version = new MOBVersion() };
        private string _token = string.Empty;
        private bool IsCorpBookingPath = false;
        private const string DOTBaggageInfoDBTitle1 = "DOTBaggageInfoText1";
        private const string DOTBaggageInfoDBTitle1ELF = "DOTBaggageInfoText1 - ELF";
        private const string DOTBaggageInfoDBTitle1IBE = "DOTBaggageInfoText1 - IBE";
        private const string DOTBaggageInfoDBTitle2 = "DOTBaggageInfoText2";
        private const string DOTBaggageInfoDBTitle3 = "DOTBaggageInfoText3";
        private const string DOTBaggageInfoDBTitle3IBE = "DOTBaggageInfoText3IBE";
        private const string DOTBaggageInfoDBTitle4 = "DOTBaggageInfoText4";
        private readonly string _UPGRADEMALL = "UPGRADEMALL";
        private readonly string _strSUCCESS = "SUCCESS";
        private readonly string _strUGC = "UGC";
        private readonly string _strMUA = "MUA";
        private readonly string _strPCU = "PCU";
        private readonly string _strPARTIALFAILURE = "PARTIAL";
        private string savedUnfinishedBookingActionName = "SavedItinerary";
        private string savedUnfinishedBookingAugumentName = "CustomerId";
        private readonly IHeaders _headers;
        private readonly ICustomerProfileTravelerService _customerProfileTravelerService;
        private readonly ILoyaltyUCBService _loyaltyUCBService;
        private bool IsArrangerBooking = false;
        private readonly IAuroraMySqlService _auroraMySqlService;
        private readonly IFeatureSettings _featureSettings;
        private readonly IFeatureToggles _featureToggles;
        public CheckoutUtiliy(ICacheLog<CheckoutUtiliy> logger
            , IConfiguration configuration
            , ISessionHelperService sessionHelperService
            , IDynamoDBService dynamoDBService
            , IShoppingCartService shoppingCartService
            , IDataVaultService dataVaultService
            , IDPService dPService
            , ICustomerPreferencesService customerPreferencesService
            , IMSCBaggageInfo mscBaggageInfoProvider
            , IMSCPkDispenserPublicKey mSCPkDispenserPublicKey
            , IMSCCheckoutService mSCCheckoutService
            , IFlightShoppingService flightShoppingService
            , IGetBarcodeService getBarcodeService
            , ILegalDocumentsForTitlesService legalDocumentsForTitlesService
            , ICachingService cachingService
            , IPassDetailService passDetailService
            , IFlightShoppingProductsService flightShoppingProductsService
            , IShoppingCartUtility shoppingCartUtility
            , IHeaders headers
            , IMSCPageProductInfoHelper mSCPageProductInfoHelper
            , IMerchandizingCSLService merchandizingCSLService
            , IETCUtility eTCUtility
            , ICustomerProfileTravelerService customerProfileTravelerService
            , ILoyaltyUCBService loyaltyUCBService
            , IAuroraMySqlService auroraMySqlService
            , IFeatureSettings featureSettings
            , IFeatureToggles featureToggles)

        {
            _logger = logger;
            _configuration = configuration;
            _sessionHelperService = sessionHelperService;
            _dynamoDBService = dynamoDBService;
            _documentLibraryDynamoDB = new DocumentLibraryDynamoDB(_configuration, _dynamoDBService);
            _shoppingCartService = shoppingCartService;
            _dataVaultService = dataVaultService;
            _dPService = dPService;
            _customerPreferencesService = customerPreferencesService;
            _mscBaggageInfoProvider = mscBaggageInfoProvider;
            _mSCPkDispenserPublicKey = mSCPkDispenserPublicKey;
            _mSCCheckoutService = mSCCheckoutService;
            _flightShoppingService = flightShoppingService;
            _getBarcodeService = getBarcodeService;
            _legalDocumentsForTitlesService = legalDocumentsForTitlesService;
            _cachingService = cachingService;
            _passDetailService = passDetailService;
            _flightShoppingProductsService = flightShoppingProductsService;
            _shoppingCartUtility = shoppingCartUtility;
            _headers = headers;
            _mSCPageProductInfoHelper = mSCPageProductInfoHelper;
            _merchandizingCSLService = merchandizingCSLService;
            _eTCUtility = eTCUtility;
            _customerProfileTravelerService = customerProfileTravelerService;
            _loyaltyUCBService = loyaltyUCBService;
            ConfigUtility.UtilityInitialize(_configuration);
            _auroraMySqlService = auroraMySqlService;
            _featureSettings = featureSettings;
            _featureToggles = featureToggles;
        }

        public bool IsEnableBookingConfirmationWheelchairAlert(Session session)
        {
            return _configuration.GetValue<bool>("EnableBookingConfirmationWheelchairAlert") &&
                   session.CatalogItems != null &&
                   session.CatalogItems.Count > 0 &&
                   session.CatalogItems.FirstOrDefault(a => a.Id == ((int)IOSCatalogEnum.EnableWheelchairLinkUpdate).ToString() || a.Id == ((int)AndroidCatalogEnum.EnableWheelchairLinkUpdate).ToString())?.CurrentValue == "1";
        }

        public async Task<MOBCheckOutResponse> CheckOut(MOBCheckOutRequest request)
        {
            MOBCheckOutResponse response = new MOBCheckOutResponse();
            FlightReservationResponse flightReservationResponse = new FlightReservationResponse();
            bool isSeatAssignmentFailed = false;
            bool isSeatAssignmentFailureOnly = false;
            bool isCOGBundleFailed = false;
            MOBSection taskTainedDogAlertMessage = null;
            MOBSection bringWheelchairAlertMessage = null;
            bool isEnableBookingConfirmationWheelchairAlert = false;

            _logger.LogInformation("CheckOut {request} with {sessionId}", JsonConvert.SerializeObject(request), request.SessionId);

            MOBCheckOutRequest logRequest = request.Clone<MOBCheckOutRequest>();

            if (logRequest.FormofPaymentDetails.CreditCard != null && request.FormofPaymentDetails.FormOfPaymentType.ToUpper() == MOBFormofPayment.CreditCard.ToString().ToUpper())
            {
                logRequest.FormofPaymentDetails.CreditCard.UnencryptedCardNumber = MaskCreditCardUnencryptedNumber(logRequest.FormofPaymentDetails.CreditCard.UnencryptedCardNumber);
            }

            if (logRequest.FormofPaymentDetails.Uplift != null && request.FormofPaymentDetails.FormOfPaymentType.ToUpper() == MOBFormofPayment.Uplift.ToString().ToUpper())
            {
                logRequest.FormofPaymentDetails.Uplift.UnencryptedCardNumber = MaskCreditCardUnencryptedNumber(logRequest.FormofPaymentDetails.Uplift.UnencryptedCardNumber);
            }

            try
            {
                if (request.FormofPaymentDetails.FormOfPaymentType?.ToUpper() == MOBFormofPayment.ApplePay.ToString().ToUpper() && !IsUSACountryAddress(request.FormofPaymentDetails.ApplePayInfo?.BillingAddress.Country))
                {
                    throw new MOBUnitedException(_configuration.GetValue<string>("ApplePayBillingCountryNotUSAMessage"));
                }

                Session session = new Session();
                session = await _sessionHelperService.GetSession<Session>(request.SessionId, session.ObjectName, new List<string> { request.SessionId, session.ObjectName }).ConfigureAwait(false);
                MOBShoppingCart persistShoppingCart = new MOBShoppingCart();
                persistShoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(request.SessionId, persistShoppingCart.ObjectName, new List<string> { request.SessionId, persistShoppingCart.ObjectName }).ConfigureAwait(false);
                Reservation persistedReservation = new Reservation();
                persistedReservation = await _sessionHelperService.GetSession<Reservation>(request.SessionId, persistedReservation.ObjectName, new List<string> { request.SessionId, persistedReservation.ObjectName }).ConfigureAwait(false);

                flightReservationResponse = await RegisterFormsOfPayments_CFOP(request, session);

                //Fix for MOBILE-9749 - Checkout NullRefException BookingFlow -CSL Error- booking not allowed for flights depart less than 60 min
                if (!_configuration.GetValue<bool>("DisableBookingFlightsDepartLessThan60Min") && _configuration.GetValue<bool>("EnableInhibitBooking") && !ConfigUtility.IsViewResFlowCheckOut(request.Flow))
                {
                    if (flightReservationResponse.Errors != null && flightReservationResponse.Errors.Count > 0)
                    {
                        foreach (var error in flightReservationResponse.Errors)
                        {
                            if (error != null && !string.IsNullOrEmpty(error.MinorCode) && error.MinorCode.Trim().Equals("10050"))
                            {
                                throw new MOBUnitedException(error.Message, new Exception(error.MinorCode));
                            }
                        }
                    }
                }
                //Fix for MOBILE-18483 - Checkout NullRefException BookingFlow-CSL Error-shoppingcart object is invalid, it could be session changed for cartid - ServiceErrorSessionNotFound
                if (!_configuration.GetValue<bool>("DisableCheckoutFixSessionMoreThan60Min_MOBILE18483") && !ConfigUtility.IsViewResFlowCheckOut(request.Flow))
                {
                    if (flightReservationResponse.Errors != null && flightReservationResponse.Errors.Count > 0)
                    {
                        foreach (var error in flightReservationResponse.Errors)
                        {
                            if (error != null && !string.IsNullOrEmpty(error.MinorCode) && error.MinorCode.Trim().Equals("10046"))
                            {
                                throw new MOBUnitedException(_configuration.GetValue<string>("BookingExceptionMessage_ServiceErrorSessionNotFound"));
                            }
                        }
                    }
                }
                if (request.Flow == FlowType.RESHOP.ToString())
                {
                    //Make Offers as empty that got saved in ViewRes Flow..In Reshop we will call FS to get those PB/PCU offers
                    GetOffers productOffer = new GetOffers();
                    await _sessionHelperService.SaveSession<GetOffers>(productOffer, request.SessionId, new List<string> { request.SessionId, typeof(GetOffers).FullName }, typeof(GetOffers).FullName).ConfigureAwait(false);

                }

                #region Build Reservation object
                if (request.Flow == FlowType.BOOKING.ToString() || request.Flow == FlowType.RESHOP.ToString())
                {
                    response.Reservation = await BuildCheckOutReservation(request, flightReservationResponse, session);
                    if (ConfigUtility.IsETCCombinabilityEnabled(request.Application.Id, request.Application.Version.Major))
                    {
                        double? etcBalanceAttentionAmount = persistShoppingCart.FormofPaymentDetails?.TravelCertificate?.Certificates?.Sum(c => c.NewValueAfterRedeem);
                        if (etcBalanceAttentionAmount > 0 && persistShoppingCart.FormofPaymentDetails?.TravelCertificate?.Certificates?.Count > 1 &&
                            response?.Reservation?.ShopReservationInfo2 != null)
                        {
                            if (IsEnableEnableCombineConfirmationAlerts(request.Application.Id, request.Application.Version.Major))
                            {
                                AddConfirmationScreenAlertMessage(response.Reservation.ShopReservationInfo2, "Attention", String.Format(_configuration.GetValue<string>("ETCBalanceConfirmationMessage"), String.Format("{0:0.00}", etcBalanceAttentionAmount)), "", MOBCONFIRMATIONALERTMESSAGEORDER.TRAVELCERTIFICATEBALANCE.ToString());
                            }
                            else
                            {
                                response.Reservation.ShopReservationInfo2.ConfirmationPageAlertMessages = getEtcBalanceAttentionConfirmationMessages(etcBalanceAttentionAmount);
                            }

                        }
                    }
                    if (request.IsSecondaryPayment == true && response.Reservation != null && response.Reservation.Prices != null &&
                        response.Reservation.Prices.Any(a => a.DisplayType.Trim().ToUpper() == "TRAVEL INSURANCE" && a.Value == Convert.ToDouble(request.PaymentAmount)))
                    {
                        response = await _sessionHelperService.GetSession<MOBCheckOutResponse>(request.SessionId, response.ObjectName, new List<string> { request.SessionId, response.ObjectName }).ConfigureAwait(false);
                        response.Reservation.Prices = UpdatePricesForTPI(response.Reservation.Prices, response.Reservation.TripInsuranceInfoBookingPath);
                        response.Reservation.IsRedirectToSecondaryPayment = false;
                        response.Reservation.Prices.FirstOrDefault(a => a.DisplayType.ToUpper().Trim() == "TRAVEL INSURANCE").BilledSeperateText = _configuration.GetValue<string>("TPIinfo_ConfirmationPage_BilledSeperateText");
                        if (persistShoppingCart.FormofPaymentDetails.CreditCard == null)
                            persistShoppingCart.FormofPaymentDetails.CreditCard = new MOBCreditCard();
                        persistShoppingCart.FormofPaymentDetails.CreditCard.BilledSeperateText = _configuration.GetValue<string>("TPIinfo_ConfirmationPage_BilledSeperateText_TicketCreditCard");
                        persistShoppingCart.FormofPaymentDetails.SecondaryCreditCard = logRequest.FormofPaymentDetails.CreditCard;
                        persistShoppingCart.FormofPaymentDetails.SecondaryCreditCard.BilledSeperateText = _configuration.GetValue<string>("TPIinfo_ConfirmationPage_BilledSeperateText_TPICreditCard");

                        response.ConfirmationMsgForTPI = persistedReservation.TripInsuranceFile.TripInsuranceBookingInfo.ConfirmationMsg;
                    }
                    if (ConfigUtility.EnableSpecialNeeds(request.Application.Id, request.Application.Version.Major))
                    {
                        if (response.Reservation != null && response.Reservation != null
                            && response.Reservation.TravelersCSL != null && response.Reservation.TravelersCSL.Any()
                            && response.Reservation.ShopReservationInfo2 != null)
                        {
                            List<MOBTravelSpecialNeed> selectedSSRCodes = response.Reservation.TravelersCSL.SelectMany(t => t.SelectedSpecialNeeds).ToList();
                            var specialNeedMsgResult = await GetSpecialNeedsMessageOnConfirmPurchase(request, session, response.Reservation.ShopReservationInfo2.PurchaseToTravelTimeIsWithinSevenDays, response.Reservation.Trips, selectedSSRCodes, response.Reservation.PointOfSale, request.Application.Id, request.Application.Version.Major, taskTainedDogAlertMessage);
                            string specialNeedsMessage = specialNeedMsgResult.SpecialNeedMessage;
                            taskTainedDogAlertMessage = specialNeedMsgResult.MobSection;

                            if (IsEnableEnableCombineConfirmationAlerts(request.Application.Id, request.Application.Version.Major) && !string.IsNullOrEmpty(specialNeedsMessage))
                            {
                                AddConfirmationScreenAlertMessage(response.Reservation.ShopReservationInfo2, "Attention", specialNeedsMessage, "", MOBCONFIRMATIONALERTMESSAGEORDER.SPECIALNEEDS.ToString());
                            }
                            else
                            {
                                response.SpecialNeedsMessage = specialNeedsMessage;
                            }

                            // MOBILE-23844
                            isEnableBookingConfirmationWheelchairAlert = IsEnableBookingConfirmationWheelchairAlert(session);
                            if (isEnableBookingConfirmationWheelchairAlert)
                            {
                                bringWheelchairAlertMessage = await GetSpecialNeedsMessageOnConfirmPurchaseForBringWheelchair(request, session,
                                    response.Reservation.ShopReservationInfo2.PurchaseToTravelTimeIsWithinSevenDays,
                                    response.Reservation.Trips,
                                    selectedSSRCodes,
                                    response.Reservation.PointOfSale);
                            }
                        }
                    }

                    logRequest.FormofPaymentDetails.FormOfPaymentType = request.FormofPaymentDetails.FormOfPaymentType;

                    if (response.Reservation != null && !string.IsNullOrEmpty(response.Reservation.RecordLocator))
                    {
                        #region
                        try
                        {
                            if (_configuration.GetValue<bool>("CallMEForBaggageInfoInMakeReservation"))
                            {
                                response.DOTBaggageInfo = await GetDOTBaggageInfo(request.SessionId, request.LanguageCode, request.Application.Id, response.Reservation);

                            }

                            bool enablePayPalOTPPayment = GetPayPalOTPEnableFlag(ParseEnumFromStringToObject<MOBFormofPayment>(request.FormofPaymentDetails.FormOfPaymentType.ToString()));
                            if (persistedReservation != null && persistedReservation.ClubPassPurchaseRequest != null && enablePayPalOTPPayment)
                            {
                                #region
                                string msg = string.Empty;
                                int numOfPasses = persistedReservation.ClubPassPurchaseRequest.NumberOfPasses;
                                double amountPaid = GetOTPPriceForApplication(1, request.SessionId);
                                string firstName = string.Empty; string lastName = string.Empty;
                                if (request.FormofPaymentDetails.FormOfPaymentType.ToUpper() == MOBFormofPayment.CreditCard.ToString().ToUpper() && !string.IsNullOrEmpty(logRequest.FormofPaymentDetails.CreditCard.UnencryptedCardNumber))
                                {

                                    string[] names = request.FormofPaymentDetails.CreditCard.CCName.Split(' ');

                                    switch (names.Length)
                                    {
                                        case 1:
                                            firstName = names[0];
                                            break;
                                        case 2:
                                            firstName = names[0];
                                            lastName = names[1];
                                            break;
                                        case 3:
                                            firstName = names[0];
                                            lastName = names[2];
                                            break;
                                        default:
                                            firstName = names[0];
                                            lastName = names[names.Length - 1];
                                            break;

                                    }
                                }

                                response.Passes = new List<MOBClubDayPass>();
                                United.Service.Presentation.ProductResponseModel.ProductPurchaseResponse offers = null;
                                try
                                {
                                    var otps = flightReservationResponse.CheckoutResponse.ShoppingCartResponse.Items.Where(item => item.Item.Product.Exists(product => product.Code == "OTP")).ToList();
                                    if (otps != null && otps.Count() > 0 && otps[0].Item != null && otps[0].Item.ProductContext != null && otps[0].Item.ProductContext.Count > 1)
                                    {
                                        string merchCall = otps[0].Item.ProductContext[1];
                                        if (!_configuration.GetValue<bool>("DisableFixforReshopOTPPurchase_MOBILE15282") && request.Flow == FlowType.RESHOP.ToString())
                                            offers = JsonConvert.DeserializeObject<United.Service.Presentation.ProductResponseModel.ProductPurchaseResponse>(merchCall);
                                        else
                                            offers = JsonConvert.DeserializeObject<United.Service.Presentation.ProductResponseModel.ProductPurchaseResponse>(merchCall);
                                    }
                                }
                                catch { }

                                bool getClubPassFromLoyaltyOTP = _configuration.GetValue<string>("LoyaltyOTPServiceSwitchONOFF") != null && Convert.ToBoolean(_configuration.GetValue<string>("LoyaltyOTPServiceSwitchONOFF").ToString()) ? Convert.ToBoolean(_configuration.GetValue<string>("LoyaltyOTPServiceSwitchONOFF").ToString()) : false;
                                if (offers == null || offers.Confirmations == null || offers.Confirmations.Count == 0)
                                {
                                    msg = "One-Time pass purchase failed.";
                                }
                                else
                                {
                                    foreach (var confirmation in offers.Confirmations)
                                    {
                                        #region
                                        try
                                        {
                                            string eddId = confirmation.EDDInternalID;

                                            MOBClubDayPass pass = new MOBClubDayPass();

                                            if (getClubPassFromLoyaltyOTP)
                                            {
                                                pass = await GetLoyaltyUnitedClubIssuePass(request.MileagePlusNumber, request.SessionId, request.Application.Id, request.Application.Version.Major, request.DeviceId, eddId, firstName, lastName, request.FormofPaymentDetails.CreditCard.CCName, request.FormofPaymentDetails.EmailAddress, request.MileagePlusNumber, "", "IOS", amountPaid.ToString(), response.Reservation.RecordLocator);
                                            }
                                            else
                                            {
                                                pass = await GetClubPassCode(request.SessionId, request.Application.Id, request.Application.Version.Major, request.DeviceId, eddId, firstName, lastName, request.FormofPaymentDetails.CreditCard.CCName, request.FormofPaymentDetails.EmailAddress, request.MileagePlusNumber, "", "IOS", amountPaid.ToString(), response.Reservation.RecordLocator);
                                            }

                                            if (pass != null)
                                            {
                                                response.Passes.Add(pass);
                                            }
                                        }
                                        catch (MOBUnitedException uae)
                                        {
                                            msg = uae.Message;
                                        }
                                        catch (Exception ex)
                                        {
                                            msg = "One-Time pass purchase failed.";
                                        }
                                        #endregion
                                    }
                                }

                                if (response.Passes.Count > 0)
                                {
                                    if (!getClubPassFromLoyaltyOTP)
                                    {
                                        SendClubPassReceipt(response.Passes, request.FormofPaymentDetails.CreditCard.DisplayCardNumber);
                                    }
                                }
                                response.OTPMessage = msg;

                                string xmlRemark = XmlSerializerHelper.Serialize<MOBSHOPReservation>(response.Reservation);
                                string formOfPayment = request.FormofPaymentDetails.FormOfPaymentType.ToString();
                                if (!string.IsNullOrEmpty(response.OTPMessage))
                                {
                                    #region
                                    MOBSHOPPrice otpPrice = response.Reservation.Prices.Find(item => item.DisplayType.ToUpper() == "ONE-TIME PASS");
                                    MOBSHOPPrice total = response.Reservation.Prices.Find(item => item.DisplayType.ToUpper() == "GRAND TOTAL");

                                    if (response.Passes.Count == 0)
                                    {

                                        total.DisplayValue = string.Format("{0:#,0.00}", total.Value - otpPrice.Value);

                                        total.FormattedDisplayValue = (total.Value - otpPrice.Value).ToString("C2", CultureInfo.CurrentCulture);
                                        total.Value = total.Value - otpPrice.Value;
                                        response.Reservation.Prices.Remove(response.Reservation.Prices.Find(item => item.DisplayType.ToUpper() == "GRAND TOTAL"));
                                        response.Reservation.Prices.Remove(response.Reservation.Prices.Find(item => item.DisplayType.ToUpper() == "ONE-TIME PASS"));
                                        response.Reservation.Prices.Add(total);
                                    }
                                    else if (response.Passes.Count > 0 && response.Passes.Count < numOfPasses)
                                    {
                                        double otpCharged = otpPrice.Value * response.Passes.Count / numOfPasses;

                                        total.DisplayValue = string.Format("{0:#,0.00}", total.Value - otpPrice.Value + otpCharged);
                                        total.FormattedDisplayValue = (total.Value - otpPrice.Value + otpCharged).ToString("C2", CultureInfo.CurrentCulture);
                                        total.Value = total.Value - otpPrice.Value + otpCharged;

                                        otpPrice.DisplayValue = string.Format("{0:#,0.00}", otpCharged);
                                        otpPrice.FormattedDisplayValue = (otpCharged).ToString("C2", CultureInfo.CurrentCulture);
                                        otpPrice.Value = otpCharged;

                                        response.Reservation.Prices.Remove(response.Reservation.Prices.Find(item => item.DisplayType.ToUpper() == "GRAND TOTAL"));
                                        response.Reservation.Prices.Remove(response.Reservation.Prices.Find(item => item.DisplayType.ToUpper() == "ONE-TIME PASS"));
                                        response.Reservation.Prices.Add(total);
                                        response.Reservation.Prices.Add(otpPrice);

                                        await AddPaymentNew(request.TransactionId, request.Application.Id, request.Application.Version.Major, (session.IsReshopChange ? "RESHOP" : "BOOKING") + "-MobileShoppingCart- Travel Options - OTP", otpCharged, "USD", 0, xmlRemark, request.Application.Id.ToString(), Convert.ToBoolean(_configuration.GetValue<string>("IsBookingTest")), request.SessionId, request.DeviceId, response.Reservation.RecordLocator, request.MileagePlusNumber, formOfPayment).ConfigureAwait(false);
                                    }
                                    #endregion
                                }
                                else
                                {
                                    await AddPaymentNew(request.TransactionId, request.Application.Id, request.Application.Version.Major, (session.IsReshopChange ? "RESHOP" : "BOOKING") + "-MobileShoppingCart- Travel Options - OTP", response.Reservation.Prices.Find(item => item.DisplayType.ToUpper() == "ONE-TIME PASS").Value, "USD", 0, xmlRemark, request.Application.Id.ToString(), Convert.ToBoolean(_configuration.GetValue<string>("IsBookingTest")), request.SessionId, request.DeviceId, response.Reservation.RecordLocator, request.MileagePlusNumber, formOfPayment).ConfigureAwait(false);

                                }
                                #endregion
                            }
                            else if (!enablePayPalOTPPayment)
                            {
                                RemoveOTPFromReservationPricesList(response);
                            }
                        }
                        catch { }

                        if (EnableEPlusAncillary(request.Application.Id, request.Application.Version.Major, session.IsReshopChange) && response.Reservation.Prices != null && response.Reservation.Prices.Count > 0)
                        {
                            persistedReservation.Prices = response.Reservation.Prices;
                        }
                        #endregion
                    }
                    response.DOTBagRules = GetDOTBagRules();
                    bool isCFOPVersionCheck = GeneralHelper.IsApplicationVersionGreaterorEqual(request.Application.Id, request.Application.Version.Major, _configuration.GetValue<string>("AndriodCFOP_Booking_Reshop_PostbookingAppVersion"), _configuration.GetValue<string>("IphoneCFOP_Booking_Reshop_PostbookingAppVersion"));
                    if (isCFOPVersionCheck)
                    {

                        if (!string.IsNullOrEmpty(_configuration.GetValue<string>("IsPostBookingCommonFOPEnabled")))
                        {
                            persistedReservation.IsPostBookingCommonFOPEnabled = Convert.ToBoolean(_configuration.GetValue<string>("IsPostbookingCommonFOPEnabled"));
                            response.Reservation.IsPostBookingCommonFOPEnabled = persistedReservation.IsPostBookingCommonFOPEnabled;
                        }


                    }

                    await _sessionHelperService.SaveSession<Reservation>(persistedReservation, request.SessionId, new List<string> { request.SessionId, persistedReservation.ObjectName }, persistedReservation.ObjectName).ConfigureAwait(false);
                    response.ShoppingCart = persistShoppingCart;

                    if (IsTaskTrainedServiceDogSupportedAppVersion(request.Application.Id, request.Application.Version.Major) &&
                        taskTainedDogAlertMessage != null &&
                        response.ShoppingCart != null)
                    {
                        if (response.ShoppingCart.AlertMessages == null)
                            response.ShoppingCart.AlertMessages = new List<MOBSection>();
                        if (IsEnableEnableCombineConfirmationAlerts(request.Application.Id, request.Application.Version.Major))
                        {
                            List<MOBTravelSpecialNeed> selectedSSRCodes = response.Reservation.TravelersCSL.SelectMany(t => t.SelectedSpecialNeeds).ToList();
                            bool isServiceAnimal = selectedSSRCodes.Where(_ => _.Code == "SVAN" || (_.Code == "ESAN" && _.Value == "5")).Any();

                            AddConfirmationScreenAlertMessage(response.Reservation.ShopReservationInfo2, taskTainedDogAlertMessage.Text1, taskTainedDogAlertMessage.Text2, taskTainedDogAlertMessage.Text3, MOBCONFIRMATIONALERTMESSAGEORDER.TRAINEDDOG.ToString(),
                                isServiceAnimal, response?.Reservation?.RecordLocator, response?.ShoppingCart?.SCTravelers?.FirstOrDefault()?.LastName);
                        }
                        else
                        {
                            response.ShoppingCart.AlertMessages.Add(taskTainedDogAlertMessage);
                        }
                    }

                    // MOBILE-23844
                    if (isEnableBookingConfirmationWheelchairAlert && bringWheelchairAlertMessage != null && response.ShoppingCart != null)
                    {
                        if (response.ShoppingCart.AlertMessages == null)
                            response.ShoppingCart.AlertMessages = new List<MOBSection>();
                        if (IsEnableEnableCombineConfirmationAlerts(request.Application.Id, request.Application.Version.Major))
                        {
                            AddConfirmationScreenAlertMessage(response.Reservation.ShopReservationInfo2,
                                                              bringWheelchairAlertMessage.Text1,
                                                              bringWheelchairAlertMessage.Text2,
                                                              bringWheelchairAlertMessage.Text3,
                                                              MOBCONFIRMATIONALERTMESSAGEORDER.SPECIALNEEDS.ToString(),
                                                              false,
                                                              response?.Reservation?.RecordLocator,
                                                              response?.ShoppingCart?.SCTravelers?.FirstOrDefault()?.LastName);

                        }
                        else
                        {
                            response.ShoppingCart.AlertMessages.Add(bringWheelchairAlertMessage);
                        }

                    }

                    // MOBILE-25395: SAF
                    var safCode = _configuration.GetValue<string>("SAFCode");
                    if (ConfigUtility.IsEnableSAFFeature(session) &&
                        response.ShoppingCart.Products.Any(product => string.Equals(product.Code, safCode, StringComparison.OrdinalIgnoreCase)))
                    {
                        if (response.ShoppingCart.AlertMessages == null)
                            response.ShoppingCart.AlertMessages = new List<MOBSection>();
                        response.ShoppingCart.AlertMessages.Add(await GetSAFAlertMessageOnConfirmPurchase(request, session));
                    }

                    if (!request.IsSecondaryPayment)
                    {
                        try
                        {
                            await GenerateTPISecondaryPaymentInfo(request, response, session.Token, persistedReservation, session);
                        }
                        catch { }
                        response.ShareFlightTitle = _configuration.GetValue<string>("ShareFlightTitle");
                        if (response.Reservation.Trips != null && response.Reservation.Trips.Count > 0)
                        {
                            response.ShareFlightMessage = string.Format(_configuration.GetValue<string>("ShareFlightMessage"), response.Reservation.Trips[0].Origin, response.Reservation.Trips[0].Destination);
                        }
                        if (response.Reservation != null)
                        {
                            response.Reservation.ELFMessagesForRTI = await GetELFShopMessagesForRestrictions(response.Reservation, request.Application.Id);
                        }

                        persistedReservation.IsRedirectToSecondaryPayment = response.Reservation.IsRedirectToSecondaryPayment;

                        await _sessionHelperService.SaveSession<MOBCheckOutResponse>(response, request.SessionId, new List<string> { request.SessionId, typeof(MOBCheckOutResponse).FullName }, typeof(MOBCheckOutResponse).FullName).ConfigureAwait(false);
                        // change persist reservation same as in make reservation resposne. 
                        // persisted TPI changed in GenerateTPISecondaryPaymentInfo for secondary payment flow. 
                        if (response.Reservation.IsRedirectToSecondaryPayment)
                        {
                            try
                            {
                                Reservation reservationToPersist = new Reservation();
                                MOBSHOPAvailability availabilityForMappingReservation = new MOBSHOPAvailability() { Reservation = response.Reservation };
                                reservationToPersist = ReservationToPersistReservation(availabilityForMappingReservation);
                                if (ConfigUtility.IsETCchangesEnabled(request.Application.Id, request.Application.Version.Major) && request.Flow == FlowType.BOOKING.ToString())
                                    reservationToPersist.CSLReservationJSONFormat = persistedReservation.CSLReservationJSONFormat;

                                await _sessionHelperService.SaveSession<Reservation>(reservationToPersist, request.SessionId, new List<string> { request.SessionId, reservationToPersist.ObjectName }, reservationToPersist.ObjectName).ConfigureAwait(false);
                            }
                            catch
                            {
                                MOBUnitedException uaex = new MOBUnitedException("Persist Reservation for secondayPayment failed");
                                MOBExceptionWrapper uaexWrapper = new MOBExceptionWrapper(uaex);
                                //If exceptions occurs persisting the reservation to save the IsRedirectToSecondaryPayment flag.                                
                                await _sessionHelperService.SaveSession<Reservation>(persistedReservation, request.SessionId, new List<string> { request.SessionId, persistedReservation.ObjectName }, persistedReservation.ObjectName).ConfigureAwait(false);
                                _logger.LogError("Checkout Error {message} {StackTrace} and {session}", uaex.Message, uaex.StackTrace, request.SessionId);
                            }
                        }
                    }

                }
                #endregion

                if (persistShoppingCart == null)
                    throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));

                #region ViewResFlow 
                if (ConfigUtility.IsViewResFlowCheckOut(request.Flow))
                {
                    if (IsSuccessOrValidReponse(flightReservationResponse))
                    {

                        SeatChangeState state = new SeatChangeState();
                        state = await _sessionHelperService.GetSession<SeatChangeState>(session.SessionId, state.ObjectName, new List<string> { session.SessionId, state.ObjectName }).ConfigureAwait(false);
                        var merchproductVendorOffer = new United.Persist.Definition.Merchandizing.GetVendorOffers();
                        merchproductVendorOffer = await _sessionHelperService.GetSession<United.Persist.Definition.Merchandizing.GetVendorOffers>(request.SessionId, merchproductVendorOffer.ObjectName, new List<string> { request.SessionId, merchproductVendorOffer.ObjectName });

                        var isMiles = (persistShoppingCart?.FormofPaymentDetails?.FormOfPaymentType == MOBFormofPayment.MilesFOP.ToString());
                        persistShoppingCart.Products = await _shoppingCartUtility.ConfirmationPageProductInfo(flightReservationResponse, true, false, request.Flow.ToString(), request.Application, state, sessionId: request?.SessionId, isMiles: isMiles);
                        persistShoppingCart.AlertMessages = await GetErrorMessagesForConfirmationScreen(flightReservationResponse, request.Flow, persistShoppingCart.Products);
                        var grandtotal = persistShoppingCart.Products != null && persistShoppingCart.Products.Any() ? persistShoppingCart.Products.Sum(p => string.IsNullOrEmpty(p.ProdTotalPrice) ? 0 : Convert.ToDecimal(p.ProdTotalPrice)) : 0;
                        persistShoppingCart.TotalPrice = String.Format("{0:0.00}", grandtotal);
                        persistShoppingCart.DisplayTotalPrice = grandtotal > 0 || _mSCPageProductInfoHelper.isAFSCouponApplied(flightReservationResponse?.DisplayCart) ? Decimal.Parse(grandtotal.ToString()).ToString("c") : string.Empty;
                        persistShoppingCart.TermsAndConditions = await _shoppingCartUtility.GetProductBasedTermAndConditions(merchproductVendorOffer, flightReservationResponse, true, request.SessionId, request.Flow);
                        persistShoppingCart.DisplayMessage = await GetConfirmationMessageForWLPNRManageRes(flightReservationResponse, persistShoppingCart.AlertMessages, request.Flow);
                        bool isCompleteFarelockPurchase = IsCheckFareLockUsingProductCode(persistShoppingCart);
                        var emailAddress = string.Empty;
                        if (isCompleteFarelockPurchase)
                        {
                            emailAddress = (!string.IsNullOrEmpty(request.FormofPaymentDetails.EmailAddress)) ? request.FormofPaymentDetails.EmailAddress.ToString() : (flightReservationResponse.Reservation.EmailAddress.Count() > 0) ? flightReservationResponse.Reservation.EmailAddress.Where(x => x.Address != null).Select(x => x.Address).FirstOrDefault().ToString() : null;

                            persistShoppingCart.Captions = await GetFareLockCaptions(flightReservationResponse.Reservation, emailAddress);
                            persistShoppingCart.FlightShareMessage = GetFlightShareMessageViewRes(flightReservationResponse.Reservation);
                        }

                        if(await _featureToggles.IsEnableAutoRefundMessageOnViewResCheckOut(request.Application.Id, request.Application?.Version?.Major))
                        {
                            string autoRefundMessage = await GetAutoRefundMessage(flightReservationResponse, request, session.SessionId, session.Token);
                            if(!string.IsNullOrEmpty(autoRefundMessage))
                            {
                                if (persistShoppingCart?.Captions == null)
                                    persistShoppingCart.Captions = new List<MOBItem>();

                                persistShoppingCart.Captions.Add(GetFareLockViewResPaymentCaptions("AutoRefund_MRConfirmationPage", autoRefundMessage));
                            }
                        }

                        if (grandtotal > 0 || persistShoppingCart.Products != null && persistShoppingCart.Products.Any() && persistShoppingCart.Products.Any(p => !string.IsNullOrEmpty(p.ProdDisplayOtherPrice)))
                        {
                            if (persistShoppingCart.FormofPaymentDetails == null)
                                persistShoppingCart.FormofPaymentDetails = request.FormofPaymentDetails;

                            persistShoppingCart.FormofPaymentDetails.EmailAddress = IsFareLockApplePay(isCompleteFarelockPurchase, request.FormofPaymentDetails) && !string.IsNullOrEmpty(emailAddress) ? emailAddress : GetCheckOutEmail(request, flightReservationResponse);
                        }
                        else
                        {
                            persistShoppingCart.FormofPaymentDetails = null;
                        }
                        if (ConfigUtility.IsManageResETCEnabled(request.Application.Id, request.Application.Version.Major) && persistShoppingCart.FormofPaymentDetails?.TravelCertificate?.Certificates?.Count > 0)
                        {
                            double etcBalanceAttentionAmount = persistShoppingCart.FormofPaymentDetails.TravelCertificate.Certificates.Sum(c => c.NewValueAfterRedeem);
                            if (Math.Round(etcBalanceAttentionAmount, 2) > 0 && persistShoppingCart.FormofPaymentDetails?.TravelCertificate?.Certificates?.Count > 1)
                            {
                                persistShoppingCart.ConfirmationPageAlertMessages = getEtcBalanceAttentionConfirmationMessages(etcBalanceAttentionAmount);
                            }
                            persistShoppingCart.FormofPaymentDetails.TravelCertificate.AllowedETCAmount = _eTCUtility.GetAlowedETCAmount(persistShoppingCart.Products, persistShoppingCart.Flow);
                            persistShoppingCart?.FormofPaymentDetails?.TravelCertificate?.Certificates?.ForEach(c => c.RedeemAmount = 0);
                            _eTCUtility.AddRequestedCertificatesToFOPTravelerCertificates(persistShoppingCart?.FormofPaymentDetails?.TravelCertificate?.Certificates, persistShoppingCart.ProfileTravelerCertificates, persistShoppingCart.FormofPaymentDetails.TravelCertificate);
                            var certificatePrice = persistShoppingCart.Prices.FirstOrDefault(p => p.DisplayType.ToUpper() == "CERTIFICATE");
                            _eTCUtility.UpdateCertificatePrice(certificatePrice, persistShoppingCart.FormofPaymentDetails.TravelCertificate.TotalRedeemAmount);
                            _eTCUtility.AssignTotalAndCertificateItemsToPrices(persistShoppingCart);

                        }
                        await _sessionHelperService.SaveSession<MOBShoppingCart>(persistShoppingCart, request.SessionId, new List<string> { request.SessionId, persistShoppingCart.GetType().ToString() }, persistShoppingCart.GetType().ToString()).ConfigureAwait(false);

                        response.ShoppingCart = persistShoppingCart;
                        response.PostPurchasePage = PostPurchasePage.Confirmation.ToString();
                        response.RecordLocator = flightReservationResponse.Reservation.ConfirmationID;
                        response.LastName = flightReservationResponse.Reservation.Travelers[0].Person.Surname;
                        await PutPaymentInfoToPaymentTable(request, response, flightReservationResponse, persistShoppingCart).ConfigureAwait(false);
                        if (persistShoppingCart.AlertMessages != null && persistShoppingCart.AlertMessages.Any())
                        {
                            _logger.LogWarning("RegisterFormsOfPayments_CFOP ShoppingCart_AlertMessage {UnitedException}", persistShoppingCart.AlertMessages);
                        }

                        if (string.Equals(request.Flow, _UPGRADEMALL, StringComparison.OrdinalIgnoreCase))
                        {
                            response.ShoppingCart.UpgradeCabinProducts
                                = await GetConfirmedUpgradeProducts(request.SessionId, flightReservationResponse);
                        }

                        if (_configuration.GetValue<bool>("EnablePCUWaitListPNRManageRes") && persistShoppingCart.DisplayMessage != null && !GeneralHelper.IsApplicationVersionGreater(request.Application.Id, request.Application.Version.Major, "PCURefundMessageForIOSOldVersion", "PCURefundMessageForAndroidOldVersion", "", "", true, _configuration))
                        {
                            persistShoppingCart.AlertMessages = persistShoppingCart.AlertMessages != null && persistShoppingCart.AlertMessages.Any() ? null : persistShoppingCart.DisplayMessage;
                        }
                        return response;
                    }
                    //Merch proccessing failed
                    if (flightReservationResponse?.Errors?.Any(e => e?.MinorCode == "90585") ?? false)
                    {
                        response.ShoppingCart = new MOBShoppingCart();
                        response.ShoppingCart.AlertMessages = await GetErrorMessagesForConfirmationScreen(flightReservationResponse, request.Flow);
                        response.PostPurchasePage = PostPurchasePage.Confirmation.ToString();
                        response.RecordLocator = flightReservationResponse.Reservation.ConfirmationID;
                        response.LastName = flightReservationResponse.Reservation.Travelers[0].Person.Surname;
                        if (response?.ShoppingCart?.AlertMessages != null && response.ShoppingCart.AlertMessages.Any())
                        {
                            _logger.LogWarning("RegisterFormsOfPayments_CFOP ShoppingCart_AlertMessage {UnitedException}", response.ShoppingCart.AlertMessages);
                        }
                        if (response.ShoppingCart.AlertMessages == null)
                        {
                            throw new MOBUnitedException("There was a problem completing your purchase");
                        }
                        return response;
                    }

                    //Credit card authorization failed
                    if (flightReservationResponse?.Errors?.Any(e => e?.MinorCode == "90546") ?? false)
                    {
                        throw new MOBUnitedException(await GetTextFromDatabase("CreditCardAuthorizationFailure"));
                    }

                    //Any other errors
                    if (flightReservationResponse?.Errors?.Any() ?? false)
                    {
                        response.IsUpgradePartialSuccess = IsUpgradePartialSuccessUPGRADEMALL(request.Flow, flightReservationResponse.Warnings);

                        if (flightReservationResponse.Errors.Any(x => (x.MinorDescription?.Contains("FltResRegisterFormsOfPayment") ?? false) || (x.MinorDescription?.Contains("ServiceErrorSessionNotFound") ?? false)))
                            throw new MOBUnitedException("There was a problem completing your purchase");

                        try
                        {
                            await GenerateTPISecondaryPaymentInfoFOP(request, response, flightReservationResponse, persistShoppingCart, session);

                            if (response.IsTPIFailed)
                            {
                                return response;
                            }
                        }
                        catch { }


                        if (session.IsReshopChange && flightReservationResponse.Errors.Exists(error => error.MajorCode == "30006.14"))
                        {
                            if (flightReservationResponse.Errors.Exists(error => error.MinorCode == "90518" || error.MinorCode == "90510"))
                            {
                                var errorCsl = flightReservationResponse.Errors.FirstOrDefault(error => error.MinorCode == "90518" || error.MinorCode == "90510");
                                throw new MOBUnitedException(errorCsl.MinorCode);
                            }
                        }

                        string errorMessage = string.Empty;

                        foreach (var error in flightReservationResponse.Errors)
                        {
                            errorMessage = errorMessage + " " + error.Message;
                        }


                        if (errorMessage.ToUpper().Contains("CREDIT"))
                        {
                            throw new MOBUnitedException("We were unable to charge your card as the authorization has been denied. Please contact your financial provider or use a different card.");
                        }
                        else
                        {
                            throw new MOBUnitedException("There was a problem completing your purchase");
                        }
                    }
                }
                #endregion
                else
                {
                    try
                    {
                        if (flightReservationResponse != null && (flightReservationResponse.CheckoutResponse.ShoppingCartResponse.ConfirmationID != null || (flightReservationResponse.Reservation != null && flightReservationResponse.Reservation.ConfirmationID != null)))
                        {
                            var isInsertPaymentTbl = _configuration.GetValue<bool>("DisableFixPaymentTblInsertPostBookingCheckIn_MOBILE19771") == false
                                ? (flightReservationResponse.Status.Equals(United.Services.FlightShopping.Common.StatusType.Success) && (flightReservationResponse.Errors == null || flightReservationResponse.Errors.Count() == 0))
                                : true;

                            if (!(request.FormofPaymentDetails.CreditCard == null && request.FormofPaymentDetails.PayPal == null && request.FormofPaymentDetails.PayPalPayor == null && request.FormofPaymentDetails.Masterpass == null && request.FormofPaymentDetails.MasterPassSessionDetails == null && request.FormofPaymentDetails.ApplePayInfo == null && request.FormofPaymentDetails.FormOfPaymentType == null)
                                && isInsertPaymentTbl)
                            {

                                if (request.Flow == FlowType.POSTBOOKING.ToString())
                                {
                                    try
                                    {
                                        string xmlRemark = XmlSerializerHelper.Serialize<MOBCheckOutResponse>(response);
                                        await AddPaymentNew(request.SessionId, request.Application.Id, request.Application.Version.Major, (session.IsReshopChange ? "RESHOP-" : "BOOKING-") + string.Join(",", persistShoppingCart.Products.Select(x => x.Code).ToList()), Convert.ToDouble(persistShoppingCart.TotalPrice),
                                              "USD",
                                              0,
                                              xmlRemark,
                                              request.Application.Id.ToString(),
                                              Convert.ToBoolean(_configuration.GetValue<string>("IsBookingTest")),
                                              request.SessionId,
                                              request.DeviceId,
                                              flightReservationResponse.Reservation.ConfirmationID,
                                              null,
                                              request.FormofPaymentDetails.FormOfPaymentType,
                                              GetRestAPIVersionBasedonFlowType(request.Flow)).ConfigureAwait(false);
                                        await InsertCouponDetails(persistShoppingCart, request, request.SessionId, xmlRemark, flightReservationResponse.Reservation.ConfirmationID).ConfigureAwait(false);
                                    }
                                    catch { }

                                }
                                else if (!(request.Flow == FlowType.BOOKING.ToString() || request.Flow == FlowType.RESHOP.ToString()))
                                {
                                    try
                                    {
                                        string xmlRemark = XmlSerializerHelper.Serialize<MOBCheckOutResponse>(response);
                                        await AddPaymentNew(
                                             request.SessionId,
                                             request.Application.Id,
                                             request.Application.Version.Major,
                                             string.Join(",", persistShoppingCart.Products.Select(x => x.Code).ToList()),
                                             Convert.ToDouble(flightReservationResponse.DisplayCart.TravelOptions.Select(x => x.Amount).Sum()),
                                             string.IsNullOrEmpty(persistShoppingCart.CurrencyCode) ? "USD" : persistShoppingCart.CurrencyCode,
                                             0,
                                             xmlRemark,
                                             request.Application.Id.ToString(),
                                             Convert.ToBoolean(_configuration.GetValue<string>("IsBookingTest")),
                                             request.SessionId,
                                             request.DeviceId,
                                             flightReservationResponse.Reservation.ConfirmationID,
                                             null,
                                             request.FormofPaymentDetails.FormOfPaymentType,
                                             GetRestAPIVersionBasedonFlowType(request.Flow)).ConfigureAwait(false);
                                    }
                                    catch { }
                                }
                            }
                        }
                    }
                    catch { }

                    bool isEFSFailure = false;
                    isEFSFailure = HandleAncillaryOptionsForEFS(request, response, flightReservationResponse, persistShoppingCart);

                    if (flightReservationResponse != null
                        && flightReservationResponse.CheckoutResponse.ShoppingCart.Items.Any(x => x.Product[0].Code == "SEATASSIGNMENTS")
                        && _shoppingCartUtility.CheckSeatAssignMessage(flightReservationResponse, request.Flow) && !isEFSFailure)
                    {
                        await HandleSeatAssignmentFailure(persistShoppingCart, flightReservationResponse, request, response);
                        isSeatAssignmentFailed = true;
                    }

                    if (flightReservationResponse != null && flightReservationResponse.Status.Equals(United.Services.FlightShopping.Common.StatusType.Success) &&
                        (flightReservationResponse.Errors == null || flightReservationResponse.Errors.Count() == 0))
                    {
                        var productVendorOffer = new GetVendorOffers();
                        productVendorOffer = await _sessionHelperService.GetSession<GetVendorOffers>(session.SessionId, productVendorOffer.ObjectName, new List<string> { session.SessionId, productVendorOffer.ObjectName }).ConfigureAwait(false);

                        //v788383: StrikeOff price - Mobile-1044
                        SeatChangeState state = new SeatChangeState();
                        state = await _sessionHelperService.GetSession<SeatChangeState>(session.SessionId, state.ObjectName, new List<string> { session.SessionId, state.ObjectName }).ConfigureAwait(false);
                        //End

                        //for bags we need the line items that have been persisted
                        List<MOBProdDetail> prodDetails = await _shoppingCartUtility.ConfirmationPageProductInfo(flightReservationResponse, true, false, request.Flow.ToString(), request.Application, state, sessionId: request?.SessionId).ConfigureAwait(false);
                        if (flightReservationResponse.CheckoutResponse.ShoppingCart.Items.Any(x => x.Product[0].Code == "BAG"))
                            prodDetails.First(x => x.Code == "BAG").LineItems = persistShoppingCart.Products.First(x => x.Code == "BAG").LineItems;
                        persistShoppingCart.Products = prodDetails;

                        if (persistShoppingCart.Products.Where(x => x.Code == "SEATASSIGNMENTS").Any(x => (x.Segments == null || x.Segments.Count() == 0) && (x.ProdTotalPrice != "0.00" || Convert.ToInt32(Convert.ToDecimal(x.ProdTotalPrice)) != 0)))
                        {
                            _logger.LogInformation("EmptyDisplaySeatsFromShoppingCart, Shopping Cart Returned Empty Display Seats for Paid Seats. We just logged at Exception to get it inserted into uatb_log table but the response for this is Success. {request} {sessionId}", JsonConvert.SerializeObject(request), request.SessionId);
                        }
                        if (request.Flow == FlowType.POSTBOOKING.ToString())
                        {
                            persistShoppingCart.TermsAndConditions = null;
                        }
                        else
                        {
                            persistShoppingCart.TermsAndConditions = await _shoppingCartUtility.GetProductBasedTermAndConditions(productVendorOffer, flightReservationResponse, true, request.SessionId, request.Flow);
                        }
                        if (persistShoppingCart.FormofPaymentDetails == null)
                            persistShoppingCart.FormofPaymentDetails = request.FormofPaymentDetails;
                            persistShoppingCart.FormofPaymentDetails.EmailAddress = (!string.IsNullOrEmpty(request.FormofPaymentDetails.EmailAddress)) ? request.FormofPaymentDetails.EmailAddress.ToString() : (flightReservationResponse.Reservation.EmailAddress?.Count() > 0) ? flightReservationResponse.Reservation.EmailAddress?.Where(x => x.Address != null).Select(x => x.Address).FirstOrDefault().ToString() : null;
                        persistShoppingCart.TotalPrice = String.Format("{0:0.00}", persistShoppingCart.Products.Select(x => Convert.ToDouble(x.ProdTotalPrice)).ToList().Sum());
                        if (!UtilityHelper.IsCheckinFlow(request.Flow))
                            persistShoppingCart.DisplayTotalPrice = Decimal.Parse(persistShoppingCart.Products.Select(x => Convert.ToDouble(x.ProdTotalPrice)).ToList().Sum().ToString()).ToString("c", new CultureInfo("en-us"));

                        await _sessionHelperService.SaveSession<MOBShoppingCart>(persistShoppingCart, request.SessionId, new List<string> { request.SessionId, persistShoppingCart.ObjectName }, persistShoppingCart.ObjectName).ConfigureAwait(false);

                        response.ShoppingCart = persistShoppingCart;

                        if (flightReservationResponse.CheckoutResponse.ShoppingCart.Items.Any(x => x.Product[0].Code == "RES" || x.Product[0].Code == "PBS" || x.Product[0].Code == "TPI" || x.Product[0].Code == "PCU" || x.Product[0].Code == "PAS" || x.Product[0].Code == "SEATASSIGNMENTS" || x.Product[0].Code == "AAC" || x.Product[0].Code == "PAC") == true)
                        {
                            response.PostPurchasePage = PostPurchasePage.Confirmation.ToString();
                        }
                        else
                        {
                            response.PostPurchasePage = PostPurchasePage.PNRRetrival.ToString();
                        }
                        response.RecordLocator = flightReservationResponse.Reservation.ConfirmationID;
                        response.PnrCreateDate = flightReservationResponse.Reservation.CreateDate;
                        response.LastName = flightReservationResponse.Reservation.Travelers[0].Person.Surname;


                    }
                    else
                    {
                        if (flightReservationResponse.Errors != null && flightReservationResponse.Errors.Count > 0)
                        {
                            string errorMessage = string.Empty;

                            if (flightReservationResponse.Errors.Any(x => (x.MinorDescription?.Contains("FltResRegisterFormsOfPayment") ?? false) || (x.MinorDescription?.Contains("ServiceErrorSessionNotFound") ?? false)))
                                throw new MOBUnitedException("There was a problem completing your purchase");

                            try
                            {
                                await GenerateTPISecondaryPaymentInfoFOP(request, response, flightReservationResponse, persistShoppingCart, session);

                                if (response.EnabledSecondaryFormofPayment)
                                {
                                    await _sessionHelperService.SaveSession<MOBShoppingCart>(response.ShoppingCart, session.SessionId, new List<string> { request.SessionId, persistShoppingCart.ObjectName }, persistShoppingCart.ObjectName).ConfigureAwait(false);
                                    GetFaceCoveringMessage(request, response, persistedReservation);
                                    return response;
                                }
                            }
                            catch { }

                            if (isEFSFailure || (isSeatAssignmentFailed == false && flightReservationResponse.Errors.Any(x => x.MinorCode.Equals("90584"))))
                            {
                                await HandleSeatAssignmentFailure(persistShoppingCart, flightReservationResponse, request, response, isEFSFailure);
                            }

                            if (flightReservationResponse.Errors.Count() == flightReservationResponse.Errors.Where(x => x.MinorCode.Equals("90584") || x.MajorCode.Equals("30006.14")).Count())
                                isSeatAssignmentFailureOnly = true;

                            //Mobie-6398: ByPassing the exception and navigate to Confirmation Page if PNR generated but Bundle purchase failed for COG flight segments.
                            if (flightReservationResponse.Errors.Any(e => e.MinorCode == "90585") && flightReservationResponse.DisplayCart.TravelOptions.Any(x => x.Type == "BE") && flightReservationResponse.Reservation.FlightSegments.Where(x => x.Legs != null).SelectMany(x => x.Legs).Any(e => Convert.ToBoolean(e.IsChangeOfGauge) == true))
                            {
                                isCOGBundleFailed = true;
                            }

                            bool pcuStatusAndHandleErrors = await CheckPCUStatusAndHandleErrors(flightReservationResponse, request, response);

                            if (pcuStatusAndHandleErrors == true)
                            {
                                return response;
                            }


                            foreach (var error in flightReservationResponse.Errors)
                            {
                                errorMessage = errorMessage + " " + error.Message;
                            }

                            if (UtilityHelper.IsCheckinFlow(request.Flow) && flightReservationResponse.Errors.Any(x => x.MinorCode == "900987"))
                            {
                                List<string> errCodes = new List<string>() { "CHANGE_SEAT_FAILED", "SEAT_NO_LONGER_AVAILABLE", "CHANGE_FLIGHT_ERROR", "CHANGE_CABIN_FAILED" };
                                flightReservationResponse.Errors.Where(x => errCodes.Contains(x.MajorCode))
                                                                .ForEach(x => CheckinSpecialErrors(x.MajorCode));
                            }

                            if (session.IsReshopChange && flightReservationResponse.Errors.Exists(error => error.MajorCode == "30006.14"))
                            {
                                if (flightReservationResponse.Errors.Exists(error => error.MinorCode == "90518" || error.MinorCode == "90510"))
                                {
                                    var errorCsl = flightReservationResponse.Errors.FirstOrDefault(error => error.MinorCode == "90518" || error.MinorCode == "90510");
                                    throw new MOBUnitedException(errorCsl.MinorCode);
                                }
                            }
                            //FFCR Combinability failure
                            if (ConfigUtility.IncludeFFCResidual(request.Application.Id, request.Application.Version.Major) && flightReservationResponse.Errors.Any(x => x.MinorCode == "90543"))
                            {
                                List<CMSContentMessage> lstMessages = await _shoppingCartUtility.GetSDLContentByGroupName(request, session.SessionId, session.Token, _configuration.GetValue<string>("CMSContentMessages_GroupName_BookingRTI_Messages"), "BookingPathRTI_CMSContentMessagesCached_StaticGUID");
                                var ffcrCombinabilityError = GetSDLMessageFromList(lstMessages, "RTI.FutureFlightCredits.CombineFailure");
                                throw new MOBUnitedException(ffcrCombinabilityError?.FirstOrDefault()?.ContentFull);
                            }
                            if (!(isSeatAssignmentFailureOnly) && !response.IsTPIFailed && !isCOGBundleFailed && !isEFSFailure)
                            {
                                if (errorMessage.ToUpper().Contains("CREDIT"))
                                {
                                    throw new MOBUnitedException("We were unable to charge your card as the authorization has been denied. Please contact your financial provider or use a different card.");
                                }
                                else
                                {
                                    throw new MOBUnitedException("There was a problem completing your purchase");
                                }
                            }
                        }
                    }
                    GetFaceCoveringMessage(request, response, persistedReservation);
                }
            }
            catch (System.Net.WebException wex)
            {
                if (wex.Response != null)
                {

                    _logger.LogError("Checkout Error {message} {StackTrace} and {session}", wex.Message, wex.StackTrace, request.SessionId);

                }
                throw;
            }
            catch (MOBUnitedException uaex)
            {
                if (!_configuration.GetValue<bool>("DisableExceptionRethrow_MOBILE14682"))
                    System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(uaex.InnerException ?? uaex).Throw();
                else
                    throw uaex;
            }
            catch (System.Exception ex)
            {
                if (!_configuration.GetValue<bool>("DisableExceptionRethrow_MOBILE14682"))
                    System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException ?? ex).Throw();
                else
                    throw ex;
            }
            finally
            {
                _logger.LogInformation("Checkout {response} with {sessionId}", JsonConvert.SerializeObject(response), response.SessionId);
            }

            return response;
        }

        private async Task<string> GetAutoRefundMessage(FlightReservationResponse flightReservationResponse, MOBCheckOutRequest request, string sessionId, string token)
        {
                string refundMessage = string.Empty;
                var autoRefundProductCodes = _configuration.GetValue<string>("AutoRefundProductCodesInViewResCheckout").Split('|');
                var isAutoRefundProduct = flightReservationResponse.CheckoutResponse.ShoppingCartResponse?.Items?.Select(x => x?.Item?.Product?.FirstOrDefault(p => p != null && ((!string.IsNullOrEmpty(p.Code) && autoRefundProductCodes.Contains(p.Code))))).ToList();

                bool hasRefundCode = false;
                if (isAutoRefundProduct !=null && isAutoRefundProduct.Count > 0)
                    {
                        foreach (var product in isAutoRefundProduct)
                        {
                            if(product?.Characteristics !=null && !hasRefundCode)
                                hasRefundCode = product.Characteristics.Any(x => x != null && !string.IsNullOrEmpty(x.Code) && x.Code.Equals("ANCILLARY_RULES_ENGINE"));
                        }
                    }

            if (hasRefundCode)
            {
                List<CMSContentMessage> lstMessages = await _shoppingCartUtility.GetSDLContentByGroupName(request, sessionId, token, _configuration.GetValue<string>("CMSContentMessages_GroupName_BookingRTI_Messages"), "BookingPathRTI_CMSContentMessagesCached_StaticGUID");
                var autoRefundMessage = GetSDLMessageFromList(lstMessages, "MRAutoRefund.ConfirmationPage.Receipt")?.FirstOrDefault();

                refundMessage = autoRefundMessage?.ContentFull;
            }

            return refundMessage;
        }

        public async Task<MOBCheckOutResponse> ViewResRFOPResponse(FlightReservationResponse flightReservationResponse, MOBCheckOutRequest request, MOBShoppingCart persistShoppingCart, Session session)
        {
            MOBCheckOutResponse response = new MOBCheckOutResponse();
            if (IsSuccessOrValidReponse(flightReservationResponse))
            {

                SeatChangeState state = new SeatChangeState();
                state = await _sessionHelperService.GetSession<SeatChangeState>(session.SessionId, state.ObjectName, new List<string> { session.SessionId, state.ObjectName }).ConfigureAwait(false);
                var merchproductVendorOffer = new United.Persist.Definition.Merchandizing.GetVendorOffers();
                merchproductVendorOffer = await _sessionHelperService.GetSession<United.Persist.Definition.Merchandizing.GetVendorOffers>(request.SessionId, merchproductVendorOffer.ObjectName, new List<string> { request.SessionId, merchproductVendorOffer.ObjectName });

                persistShoppingCart.Products = await _shoppingCartUtility.ConfirmationPageProductInfo(flightReservationResponse, true, false, request.Flow.ToString(), request.Application, state, sessionId: request?.SessionId);
                persistShoppingCart.AlertMessages = await GetErrorMessagesForConfirmationScreen(flightReservationResponse, request.Flow, persistShoppingCart.Products);
                var grandtotal = persistShoppingCart.Products != null && persistShoppingCart.Products.Any() ? persistShoppingCart.Products.Sum(p => string.IsNullOrEmpty(p.ProdTotalPrice) ? 0 : Convert.ToDecimal(p.ProdTotalPrice)) : 0;
                persistShoppingCart.TotalPrice = String.Format("{0:0.00}", grandtotal);
                persistShoppingCart.DisplayTotalPrice = grandtotal > 0 || _mSCPageProductInfoHelper.isAFSCouponApplied(flightReservationResponse?.DisplayCart) ? Decimal.Parse(grandtotal.ToString()).ToString("c") : string.Empty;
                persistShoppingCart.TermsAndConditions = await _shoppingCartUtility.GetProductBasedTermAndConditions(merchproductVendorOffer, flightReservationResponse, true, request.SessionId, request.Flow);
                persistShoppingCart.DisplayMessage = await GetConfirmationMessageForWLPNRManageRes(flightReservationResponse, persistShoppingCart.AlertMessages, request.Flow);
                bool isCompleteFarelockPurchase = IsCheckFareLockUsingProductCode(persistShoppingCart);
                var emailAddress = string.Empty;
                if (isCompleteFarelockPurchase)
                {
                    emailAddress = (!string.IsNullOrEmpty(request.FormofPaymentDetails.EmailAddress)) ? request.FormofPaymentDetails.EmailAddress.ToString() : (flightReservationResponse.Reservation.EmailAddress.Count() > 0) ? flightReservationResponse.Reservation.EmailAddress.Where(x => x.Address != null).Select(x => x.Address).FirstOrDefault().ToString() : null;

                    persistShoppingCart.Captions = await GetFareLockCaptions(flightReservationResponse.Reservation, emailAddress);
                    persistShoppingCart.FlightShareMessage = GetFlightShareMessageViewRes(flightReservationResponse.Reservation);
                }

                if (grandtotal > 0 || persistShoppingCart.Products != null && persistShoppingCart.Products.Any() && persistShoppingCart.Products.Any(p => !string.IsNullOrEmpty(p.ProdDisplayOtherPrice)))
                {
                    if (persistShoppingCart.FormofPaymentDetails == null)
                        persistShoppingCart.FormofPaymentDetails = request.FormofPaymentDetails;

                    persistShoppingCart.FormofPaymentDetails.EmailAddress = IsFareLockApplePay(isCompleteFarelockPurchase, request.FormofPaymentDetails) && !string.IsNullOrEmpty(emailAddress) ? emailAddress : GetCheckOutEmail(request, flightReservationResponse);
                }
                else
                {
                    persistShoppingCart.FormofPaymentDetails = null;
                }
                if (ConfigUtility.IsManageResETCEnabled(request.Application.Id, request.Application.Version.Major) && persistShoppingCart.FormofPaymentDetails?.TravelCertificate?.Certificates?.Count > 0)
                {
                    double etcBalanceAttentionAmount = persistShoppingCart.FormofPaymentDetails.TravelCertificate.Certificates.Sum(c => c.NewValueAfterRedeem);
                    if (Math.Round(etcBalanceAttentionAmount, 2) > 0 && persistShoppingCart.FormofPaymentDetails?.TravelCertificate?.Certificates?.Count > 1)
                    {
                        persistShoppingCart.ConfirmationPageAlertMessages = getEtcBalanceAttentionConfirmationMessages(etcBalanceAttentionAmount);
                    }
                    persistShoppingCart.FormofPaymentDetails.TravelCertificate.AllowedETCAmount = _eTCUtility.GetAlowedETCAmount(persistShoppingCart.Products, persistShoppingCart.Flow);
                    persistShoppingCart?.FormofPaymentDetails?.TravelCertificate?.Certificates?.ForEach(c => c.RedeemAmount = 0);
                    _eTCUtility.AddRequestedCertificatesToFOPTravelerCertificates(persistShoppingCart?.FormofPaymentDetails?.TravelCertificate?.Certificates, persistShoppingCart.ProfileTravelerCertificates, persistShoppingCart.FormofPaymentDetails.TravelCertificate);
                    var certificatePrice = persistShoppingCart.Prices.FirstOrDefault(p => p.DisplayType.ToUpper() == "CERTIFICATE");
                    _eTCUtility.UpdateCertificatePrice(certificatePrice, persistShoppingCart.FormofPaymentDetails.TravelCertificate.TotalRedeemAmount);
                    _eTCUtility.AssignTotalAndCertificateItemsToPrices(persistShoppingCart);

                }
                await _sessionHelperService.SaveSession<MOBShoppingCart>(persistShoppingCart, request.SessionId, new List<string> { request.SessionId, persistShoppingCart.GetType().ToString() }, persistShoppingCart.GetType().ToString()).ConfigureAwait(false);

                response.ShoppingCart = persistShoppingCart;
                response.PostPurchasePage = PostPurchasePage.Confirmation.ToString();
                response.RecordLocator = flightReservationResponse.Reservation.ConfirmationID;
                response.LastName = flightReservationResponse.Reservation.Travelers[0].Person.Surname;
                await PutPaymentInfoToPaymentTable(request, response, flightReservationResponse, persistShoppingCart).ConfigureAwait(false);
                if (persistShoppingCart.AlertMessages != null && persistShoppingCart.AlertMessages.Any())
                {
                    _logger.LogWarning("RegisterFormsOfPayments_CFOP ShoppingCart_AlertMessage {UnitedException}", persistShoppingCart.AlertMessages);
                }

                if (string.Equals(request.Flow, _UPGRADEMALL, StringComparison.OrdinalIgnoreCase))
                {
                    response.ShoppingCart.UpgradeCabinProducts
                        = await GetConfirmedUpgradeProducts(request.SessionId, flightReservationResponse);
                }

                if (_configuration.GetValue<bool>("EnablePCUWaitListPNRManageRes") && persistShoppingCart.DisplayMessage != null && !GeneralHelper.IsApplicationVersionGreater(request.Application.Id, request.Application.Version.Major, "PCURefundMessageForIOSOldVersion", "PCURefundMessageForAndroidOldVersion", "", "", true, _configuration))
                {
                    persistShoppingCart.AlertMessages = persistShoppingCart.AlertMessages != null && persistShoppingCart.AlertMessages.Any() ? null : persistShoppingCart.DisplayMessage;
                }
                return response;
            }
            //Merch proccessing failed
            if (flightReservationResponse?.Errors?.Any(e => e?.MinorCode == "90585") ?? false)
            {
                response.ShoppingCart = new MOBShoppingCart();
                response.ShoppingCart.AlertMessages = await GetErrorMessagesForConfirmationScreen(flightReservationResponse, request.Flow);
                response.PostPurchasePage = PostPurchasePage.Confirmation.ToString();
                response.RecordLocator = flightReservationResponse.Reservation.ConfirmationID;
                response.LastName = flightReservationResponse.Reservation.Travelers[0].Person.Surname;
                if (response?.ShoppingCart?.AlertMessages != null && response.ShoppingCart.AlertMessages.Any())
                {
                    _logger.LogWarning("RegisterFormsOfPayments_CFOP ShoppingCart_AlertMessage {UnitedException}", response.ShoppingCart.AlertMessages);
                }
                if (response.ShoppingCart.AlertMessages == null)
                {
                    throw new MOBUnitedException("There was a problem completing your purchase");
                }
                return response;
            }

            //Credit card authorization failed
            if (flightReservationResponse?.Errors?.Any(e => e?.MinorCode == "90546") ?? false)
            {
                throw new MOBUnitedException(await GetTextFromDatabase("CreditCardAuthorizationFailure"));
            }

            //Any other errors
            if (flightReservationResponse?.Errors?.Any() ?? false)
            {
                response.IsUpgradePartialSuccess = IsUpgradePartialSuccessUPGRADEMALL(request.Flow, flightReservationResponse.Warnings);

                if (flightReservationResponse.Errors.Any(x => (x.MinorDescription?.Contains("FltResRegisterFormsOfPayment") ?? false) || (x.MinorDescription?.Contains("ServiceErrorSessionNotFound") ?? false)))
                    throw new MOBUnitedException("There was a problem completing your purchase");

                try
                {
                    await GenerateTPISecondaryPaymentInfoFOP(request, response, flightReservationResponse, persistShoppingCart, session);

                    if (response.IsTPIFailed)
                    {
                        return response;
                    }
                }
                catch { }


                if (session.IsReshopChange && flightReservationResponse.Errors.Exists(error => error.MajorCode == "30006.14"))
                {
                    if (flightReservationResponse.Errors.Exists(error => error.MinorCode == "90518" || error.MinorCode == "90510"))
                    {
                        var errorCsl = flightReservationResponse.Errors.FirstOrDefault(error => error.MinorCode == "90518" || error.MinorCode == "90510");
                        throw new MOBUnitedException(errorCsl.MinorCode);
                    }
                }

                string errorMessage = string.Empty;

                foreach (var error in flightReservationResponse.Errors)
                {
                    errorMessage = errorMessage + " " + error.Message;
                }


                if (errorMessage.ToUpper().Contains("CREDIT"))
                {
                    throw new MOBUnitedException("We were unable to charge your card as the authorization has been denied. Please contact your financial provider or use a different card.");
                }
                else
                {
                    throw new MOBUnitedException("There was a problem completing your purchase");
                }
            }
            return response;
        }

        // MOBILE-25395: SAF
        private async Task<MOBSection> GetSAFAlertMessageOnConfirmPurchase(MOBCheckOutRequest request, Session session)
        {
            List<CMSContentMessage> lstMessages = await _shoppingCartUtility.GetSDLContentByGroupName(request, session.SessionId, session.Token, _configuration.GetValue<string>("CMSContentMessages_GroupName_BookingRTI_Messages"), "BookingPathRTI_CMSContentMessagesCached_StaticGUID");
            string alertText1 = string.Empty;
            string alertText2 = string.Empty;
            if (lstMessages != null && lstMessages.Any())
            {

                alertText1 = GetSDLMessageFromList(lstMessages, _configuration.GetValue<string>("BookingConfirmationAlertMessagesSAFMsgTitleKey"))?.FirstOrDefault()?.ContentFull;
                alertText2 = GetSDLMessageFromList(lstMessages, _configuration.GetValue<string>("BookingConfirmationAlertMessagesSAFMsgTextKey"))?.FirstOrDefault()?.ContentFull;
            }
            return new MOBSection
            {
                MessageType = MOBMESSAGETYPES.ECO_ALERT.ToString(),
                Text1 = alertText1,
                Text2 = alertText2,
                Order = "100",
                IsDefaultOpen = false
            };
        }

        private async Task<MOBClubDayPass> GetClubPassCode(string sessionId, int applicationId, string appVersion, string deviceId, string edocId, string firstName, string lastName, string ccName, string email, string onePassNumber, string location, string deviceType, string amountPaid, string recordLocator)
        {
            MOBClubDayPass pass = null;
            United.UClub.Service.Contracts.Models.OneTimePassRequest request = new UClub.Service.Contracts.Models.OneTimePassRequest();
            request.PassRequest.AmountPaid = amountPaid;
            request.PassRequest.DeviceType = deviceType;
            request.PassRequest.EdocId = edocId;
            if (firstName != "failure")
            {
                request.PassRequest.FirstName = firstName;
                request.PassRequest.LastName = lastName;
            }
            request.PassRequest.CardholderName = ccName;
            request.PassRequest.Location = location;
            request.PassRequest.OnePassNumber = onePassNumber;

            string jsonRequest = JsonConvert.SerializeObject(request);

            string token = await _dPService.GetAnonymousToken(_headers.ContextValues.Application.Id, _headers.ContextValues.DeviceId, _configuration);

            //string jsonResponse = HttpHelper.Post(_configuration.GetValue<string>("ServiceEndPointBaseUrl - OTPGetBarcodeService"), "application/json; charset=utf-8", "", jsonRequest);
            string jsonResponse = await _getBarcodeService.GetClubPassCode(string.Empty, jsonRequest, sessionId, token);

            if (!string.IsNullOrEmpty(jsonResponse))
            {
                United.UClub.Service.Contracts.Models.OneTimePassResponse response = JsonConvert.DeserializeObject<United.UClub.Service.Contracts.Models.OneTimePassResponse>(jsonResponse);

                if (response.Exception != null && response.Exception.Message.ToUpper() == "SUCCESS")
                {
                    if (response.UnitedClubPass != null && !string.IsNullOrEmpty(response.UnitedClubPass.Status) && response.UnitedClubPass.Status.Equals("SUCCESS"))
                    {
                        pass = new MOBClubDayPass();
                        pass.ClubPassCode = response.UnitedClubPass.UnitedClubPassCode;

                        string[] barCodeStrings = pass.ClubPassCode.Split('|');
                        if (barCodeStrings.Length == 5)
                        {
                            pass.PassCode = barCodeStrings[1];
                        }

                        pass.MileagePlusNumber = onePassNumber;
                        pass.FirstName = firstName;
                        pass.LastName = lastName;
                        pass.Email = email;
                        pass.PurchaseDate = DateTime.Today.ToString("MMMM dd, yyyy"); ;
                        pass.PaymentAmount = Convert.ToDouble(amountPaid);
                        pass.ExpirationDate = Convert.ToDateTime(response.UnitedClubPass.ExpirationDate).ToString("MMMM dd, yyyy");
                        pass.BarCode = GetBarCode(pass.ClubPassCode);

                        await InsertUnitedClubPassToDB(pass.PassCode, onePassNumber, firstName, lastName, email, pass.ClubPassCode, pass.PaymentAmount, Convert.ToDateTime(pass.ExpirationDate), deviceType, false, recordLocator);
                    }
                }
                else
                {
                    if (response.UnitedClubPass != null && !string.IsNullOrEmpty(response.UnitedClubPass.Status) && response.UnitedClubPass.Status.IndexOf("CC charge failure") > 0)
                    {
                        throw new MOBUnitedException("We are unable to charge your credit card.");
                    }
                    else
                    {
                        throw new MOBUnitedException("Club Day Pass request failed.");
                    }
                }
            }

            return pass;
        }

        public async Task<FlightReservationResponse> RegisterFormsOfPayments_CFOP(MOBCheckOutRequest checkOutRequest, Session session)
        {
            FlightReservationResponse response = new FlightReservationResponse();

            RegisterFormsOfPaymentRequest request = await GetRegisterFormsOfPaymentsRequest_CFOP(checkOutRequest);

            RegisterFormsOfPaymentRequest clonedRequest = null;
            if (!_configuration.GetValue<bool>("EnableRemoveTaxIdInformation") && request?.SpecialServiceRequest != null)
            {
                clonedRequest = request.Clone();
                #region Remove ID Number from tax Id Information for security purposes
                ConfigUtility.RemoveDescriptionInServiceRequest(clonedRequest?.SpecialServiceRequest);
                #endregion
            }

            string jsonRequest = JsonConvert.SerializeObject(request);
            string actionName = "RegisterFormsOfPayment";

            var additionalHeaders = _shoppingCartUtility.GetAdditionalHeadersForMosaic(checkOutRequest.Flow);
            var jsonResponse = await _shoppingCartService.RegisterFormsOfPayments_CFOP(session.Token, actionName, jsonRequest, checkOutRequest.SessionId, additionalHeaders, clonedRequest == null ? null : JsonConvert.SerializeObject(clonedRequest));

            if (!string.IsNullOrEmpty(jsonResponse))
            {
                response = JsonConvert.DeserializeObject<FlightReservationResponse>(jsonResponse);

                if (_configuration.GetValue<bool>("EnableInflightContactlessPayment") && _configuration.GetValue<bool>("EnablePaxWalletSaveErrorByPass") && !ConfigUtility.IsViewResFlowCheckOut(checkOutRequest.Flow))
                {
                    if (response != null && response.Status.Equals(United.Services.FlightShopping.Common.StatusType.Success) && (response?.Errors?.Any(e => e?.MinorCode?.Equals("90604") ?? false) ?? false))
                    {
                        response.Errors?.RemoveAll(e => e?.MinorCode?.Equals("90604") ?? false);
                    }
                }

                if (response != null && response.Status.Equals(United.Services.FlightShopping.Common.StatusType.Success) && !(response.Errors != null && response.Errors.Count > 0) && _configuration.GetValue<bool>("EnableEmailConfirmation") && !session.IsReshopChange && !ConfigUtility.IsViewResFlowCheckOut(checkOutRequest.Flow))
                {
                    try
                    {
                        await GetCartInformation(response, checkOutRequest, session.Token);
                    }
                    catch (Exception exec)
                    {
                        _logger.LogInformation("RegisterFormsOfPayments_CFOP Exception:{exec} with {sessionId}", JsonConvert.SerializeObject(exec), session.SessionId);
                    }

                }

                await _sessionHelperService.SaveSession<FlightReservationResponse>(response, session.SessionId, new List<string> { session.SessionId, typeof(FlightReservationResponse).FullName }, typeof(FlightReservationResponse).FullName).ConfigureAwait(false);

            }
            else
            {
                if (await _featureSettings.GetFeatureSettingValue("EnableMfopForBags").ConfigureAwait(false)
                    && GeneralHelper.IsApplicationVersionGreaterorEqual(checkOutRequest.Application.Id, checkOutRequest.Application.Version.Major, _configuration.GetValue<string>("AndroidMilesFopBagsVersion"), _configuration.GetValue<string>("iPhoneMilesFopBagsVersion"))
                    && (request.FormsOfPayment?.Any(p => p.PaymentTarget == "BAG") ?? false)
                    && (checkOutRequest.Flow?.ToUpper() == FlowType.MANAGERES.ToString().ToUpper()
                        || checkOutRequest.Flow?.ToUpper() == FlowType.POSTBOOKING.ToString().ToUpper()
                        || checkOutRequest.Flow?.ToUpper() == FlowType.VIEWRES.ToString().ToUpper()
                        || checkOutRequest.Flow?.ToUpper() == FlowType.BAGGAGECALCULATOR.ToString().ToUpper()))
                    throw new MOBUnitedException("9999", _configuration.GetValue<string>("GenericExceptionMessage"));
            }

            return response;
        }

        private bool IsUpgradePartialSuccessUPGRADEMALL(string flow, List<United.Services.FlightShopping.Common.ErrorInfo> warnings)
        {
            return string.Equals(flow, _UPGRADEMALL, StringComparison.OrdinalIgnoreCase) &&
                   (warnings?.Any(x => x?.Message?.IndexOf(_strPARTIALFAILURE, StringComparison.OrdinalIgnoreCase) > -1) ?? false);
        }

        private async Task<string> GetTextFromDatabase(string key)
        {
            var docs = await _documentLibraryDynamoDB.GetNewLegalDocumentsForTitles(new List<string> { key }, _headers.ContextValues.TransactionId);

            if (docs != null && docs.Any())
            {
                var doc = docs.FirstOrDefault();
                return doc.LegalDocument;
            }
            return null;
        }
        private async Task<List<MOBUpgradeOption>> GetConfirmedUpgradeProducts
         (string sessionid, FlightReservationResponse cslresponse)
        {
            try
            {
                var requestedupgradeitem
                    = await _sessionHelperService.GetSession<List<MOBUpgradeOption>>
                    (sessionid, (new List<MOBUpgradeOption>()).GetType().FullName, new List<string> { sessionid, (new List<MOBUpgradeOption>()).GetType().FullName }).ConfigureAwait(false);

                var confimedupgradeitems = new List<MOBUpgradeOption>();

                if (cslresponse.CheckoutResponse != null
                    && cslresponse.CheckoutResponse.ShoppingCartResponse != null
                    && cslresponse.CheckoutResponse.ShoppingCartResponse.UpgradeDetails != null
                    && cslresponse.CheckoutResponse.ShoppingCartResponse.UpgradeDetails.Any())
                {
                    requestedupgradeitem.ForEach(item =>
                    {
                        bool isSuccess = false;

                        if (string.Equals(item.UpgradeType, _strMUA, StringComparison.OrdinalIgnoreCase))
                        {
                            isSuccess = cslresponse.CheckoutResponse.ShoppingCartResponse.UpgradeDetails
                            .Any(x => string.Equals(Convert.ToString(x.Number), item.TripRefId, StringComparison.OrdinalIgnoreCase)
                            && string.Equals(x.Type, _strSUCCESS, StringComparison.OrdinalIgnoreCase));
                        }
                        else if (string.Equals(item.UpgradeType, _strPCU, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(item.UpgradeType, _strUGC, StringComparison.OrdinalIgnoreCase))
                        {
                            isSuccess = cslresponse.CheckoutResponse.ShoppingCartResponse.UpgradeDetails
                            .Any(x => string.Equals(Convert.ToString(x.Number), item.TripRefId, StringComparison.OrdinalIgnoreCase)
                            && string.Equals(Convert.ToString(x.Flight?.SegmentNumber), item.SegmentRefId, StringComparison.OrdinalIgnoreCase)
                            && string.Equals(x.Type, _strSUCCESS, StringComparison.OrdinalIgnoreCase));
                        }
                        if (isSuccess)
                            confimedupgradeitems.Add(item);
                    });
                }

                return confimedupgradeitems.Any() ? confimedupgradeitems : null;
            }
            catch { return null; }
        }

        private string GetCheckOutEmail(MOBCheckOutRequest checkOutRequest, FlightReservationResponse flightReservationResponse)
        {
            return (!checkOutRequest.FormofPaymentDetails.IsNullOrEmpty() && !string.IsNullOrEmpty(checkOutRequest.FormofPaymentDetails.EmailAddress)) ? checkOutRequest.FormofPaymentDetails.EmailAddress.ToString()
                       : (!flightReservationResponse.Reservation.IsNullOrEmpty() && !flightReservationResponse.Reservation.EmailAddress.IsNullOrEmpty() && flightReservationResponse.Reservation.EmailAddress.Count() > 0) ? flightReservationResponse.Reservation.EmailAddress.Where(x => x.Address != null).Select(x => x.Address).FirstOrDefault().ToString() : null;
        }
        private async Task PutPaymentInfoToPaymentTable(MOBCheckOutRequest checkOutRequest, MOBCheckOutResponse checkOutResponse, FlightReservationResponse flightReservationResponse, MOBShoppingCart persistShoppingCart)
        {
            if (!(checkOutRequest.FormofPaymentDetails.CreditCard == null && checkOutRequest.FormofPaymentDetails.PayPal == null && checkOutRequest.FormofPaymentDetails.PayPalPayor == null &&
                                checkOutRequest.FormofPaymentDetails.Masterpass == null && checkOutRequest.FormofPaymentDetails.MasterPassSessionDetails == null && checkOutRequest.FormofPaymentDetails.ApplePayInfo == null
                                && checkOutRequest.FormofPaymentDetails.FormOfPaymentType == null))
            {
                try
                {
                    bool isCompleteFarelockPurchase = persistShoppingCart != null && persistShoppingCart.Products != null ? IsCheckFareLockUsingProductCode(persistShoppingCart) : false;

                    if (flightReservationResponse.DisplayCart.TravelOptions != null && flightReservationResponse.DisplayCart.TravelOptions.Any())
                    {

                        var productCodes = flightReservationResponse.DisplayCart.TravelOptions.Select(t => t.Type != "SEATASSIGNMENTS" ? t.Key : t.Type).Distinct();

                        if (_configuration.GetValue<bool>("EnableMobileCheckoutChanges"))
                        {
                            productCodes = productCodes.Select(p => $"{checkOutRequest.Flow}-{p}").ToList();
                        }
                        string xmlRemark = XmlSerializerHelper.Serialize<MOBCheckOutResponse>(checkOutResponse);
                        if (ConfigUtility.IsManageResETCEnabled(checkOutRequest.Application.Id, checkOutRequest.Application.Version.Major) ? checkOutRequest.FormofPaymentDetails.FormOfPaymentType != "ETC" : true)
                        {
                            await AddPaymentNew(
                       checkOutRequest.SessionId,
                       checkOutRequest.Application.Id,
                       checkOutRequest.Application.Version.Major,
                       string.Join(",", productCodes),
                       Convert.ToDouble(persistShoppingCart.TotalPrice),
                      "USD",
                       0,
                       xmlRemark,
                       checkOutRequest.Application.Id.ToString(),
                       Convert.ToBoolean(_configuration.GetValue<string>("IsBookingTest")),
                       checkOutRequest.SessionId,
                       checkOutRequest.DeviceId,
                       flightReservationResponse.Reservation.ConfirmationID,
                       null,
                       checkOutRequest.FormofPaymentDetails.FormOfPaymentType,
                       GetRestAPIVersionBasedonFlowType(checkOutRequest.Flow)).ConfigureAwait(false);

                        }

                        if (ConfigUtility.IsManageResETCEnabled(checkOutRequest.Application.Id, checkOutRequest.Application.Version.Major) && persistShoppingCart.FormofPaymentDetails?.TravelCertificate?.Certificates?.Count > 0)
                        {
                            await AddPaymentNew(
                              checkOutRequest.SessionId,
                              checkOutRequest.Application.Id,
                              checkOutRequest.Application.Version.Major,
                              string.Join(",", productCodes),
                              Convert.ToDouble(persistShoppingCart.Prices.FirstOrDefault(p => p.DisplayType.ToUpper() == "CERTIFICATE")?.Value),
                             "USD",
                              0,
                              xmlRemark,
                              checkOutRequest.Application.Id.ToString(),
                              _configuration.GetValue<bool>("IsBookingTest"),
                              checkOutRequest.SessionId,
                              checkOutRequest.DeviceId,
                              flightReservationResponse.Reservation.ConfirmationID,
                              null,
                              "ETC",
                              GetRestAPIVersionBasedonFlowType(checkOutRequest.Flow)).ConfigureAwait(false);
                        }
                    }
                    else if (isCompleteFarelockPurchase)
                    {
                        var productCodes = "FLK";
                        string xmlRemark = XmlSerializerHelper.Serialize<MOBCheckOutResponse>(checkOutResponse);
                        await AddPaymentNew(
                            checkOutRequest.SessionId,
                            checkOutRequest.Application.Id,
                            checkOutRequest.Application.Version.Major,
                            string.Join(",", productCodes),
                            Convert.ToDouble(persistShoppingCart.TotalPrice),
                           "USD",
                            0,
                            xmlRemark,
                            checkOutRequest.Application.Id.ToString(),
                            _configuration.GetValue<bool>("IsBookingTest"),
                            checkOutRequest.SessionId,
                            checkOutRequest.DeviceId,
                            flightReservationResponse.Reservation.ConfirmationID,
                            null,
                            checkOutRequest.FormofPaymentDetails.FormOfPaymentType,
                            GetRestAPIVersionBasedonFlowType(checkOutRequest.Flow)).ConfigureAwait(false);
                    }
                }
                catch { }
            }
        }

        private bool IsFareLockApplePay(bool isCompleteFareLockPurchase, MOBFormofPaymentDetails formofPaymentDetails)
        {
            if (isCompleteFareLockPurchase && formofPaymentDetails != null && !formofPaymentDetails.FormOfPaymentType.IsNullOrEmpty())
            {
                if (formofPaymentDetails.FormOfPaymentType.ToUpper() == MOBFormofPayment.ApplePay.ToString().ToUpper())
                {
                    return true;
                }
            }
            return false;
        }
        private string GetFlightShareMessageViewRes(Service.Presentation.ReservationModel.Reservation reservation)
        {
            string reservationShareMessage = string.Empty;

            if (!reservation.IsNullOrEmpty() && !reservation.FlightSegments.IsNullOrEmpty())
            {
                #region Build Reservation Share Message 
                var originSegment = reservation.FlightSegments.FirstOrDefault(k => !k.IsNullOrEmpty() && !k.FlightSegment.IsNullOrEmpty()).FlightSegment;
                var arrivalSegment = reservation.FlightSegments.LastOrDefault(k => !k.IsNullOrEmpty() && !k.FlightSegment.IsNullOrEmpty()).FlightSegment;
                if (!originSegment.IsNullOrEmpty() && !arrivalSegment.IsNullOrEmpty())
                {
                    var initialOrigin = originSegment.DepartureAirport.IATACode;
                    var finalDestination = arrivalSegment.ArrivalAirport.IATACode;
                    var travelersText = !reservation.Travelers.IsNullOrEmpty() ? (reservation.Travelers.Count.ToString() + " " + (reservation.Travelers.Count > 1 ? "travelers" : "traveler")) : string.Empty;
                    var flightDatesText = string.Empty;
                    if (!originSegment.DepartureDateTime.IsNullOrEmpty() && !arrivalSegment.ArrivalDateTime.IsNullOrEmpty())
                    {
                        flightDatesText = DateTime.Parse(originSegment.DepartureDateTime.Replace("\\", ""), CultureInfo.InvariantCulture).ToString("MMM dd") + (reservation.FlightSegments.Count > 1 ? " - " + DateTime.Parse(arrivalSegment.ArrivalDateTime.Replace("\\", ""), CultureInfo.InvariantCulture).ToString("MMM dd") : "");
                    }
                    var searchType = GetJourneyTypeDescription(reservation);

                    string flightNumbers = string.Empty, viaAirports = string.Empty, cabinType = string.Empty;
                    var currenttripnum = string.Empty;

                    foreach (var cslsegment in reservation.FlightSegments)
                    {
                        if (!cslsegment.TripNumber.IsNullOrEmpty() && !currenttripnum.Equals(cslsegment.TripNumber))
                        {
                            currenttripnum = cslsegment.TripNumber;
                            var tripAllSegments = reservation.FlightSegments.Where(p => p != null && p.TripNumber != null && p.TripNumber == cslsegment.TripNumber).ToList();
                            if (string.IsNullOrEmpty(cabinType))
                            {
                                var bookingClass = tripAllSegments.FirstOrDefault(k => !k.IsNullOrEmpty() && !k.BookingClass.IsNullOrEmpty()).BookingClass;
                                cabinType = !bookingClass.IsNullOrEmpty() && !bookingClass.Cabin.IsNullOrEmpty() && !bookingClass.Cabin.Name.IsNullOrEmpty()
                                            ? bookingClass.Cabin.Name.ToUpper().Equals("COACH") ? "Economy" : bookingClass.Cabin.Name : string.Empty;
                            }
                            tripAllSegments.ForEach(k =>
                            {
                                if (!k.IsNullOrEmpty() && !k.FlightSegment.IsNullOrEmpty() && !k.FlightSegment.FlightNumber.IsNullOrEmpty()
                                && !k.FlightSegment.DepartureAirport.IsNullOrEmpty() && !k.FlightSegment.ArrivalAirport.IsNullOrEmpty())
                                {
                                    flightNumbers = flightNumbers + "," + k.FlightSegment.FlightNumber;
                                    if (k.FlightSegment.DepartureAirport.IATACode != initialOrigin && k.FlightSegment.ArrivalAirport.IATACode != finalDestination)
                                    {
                                        if (string.IsNullOrEmpty(viaAirports))
                                        {
                                            viaAirports = " via ";
                                        }
                                        viaAirports = viaAirports + k.FlightSegment.ArrivalAirport.IATACode + ",";
                                    }
                                }
                            });
                        }
                    }

                    if (flightNumbers.Trim(',').Split(',').Count() > 1)
                    {
                        flightNumbers = "Flights " + flightNumbers.Trim(',');
                    }
                    else
                    {
                        flightNumbers = "Flight " + flightNumbers.Trim(',');
                    }
                    reservationShareMessage = string.Format(_configuration.GetValue<string>("Booking20ShareMessage"), flightDatesText, travelersText, searchType, cabinType, flightNumbers.Trim(','), initialOrigin, finalDestination, viaAirports.Trim(','));
                    #endregion
                }
            }
            return reservationShareMessage;
        }

        private async Task<List<MOBItem>> GetFareLockCaptions(United.Service.Presentation.ReservationModel.Reservation reservation, string email)
        {
            if (reservation.IsNullOrEmpty() || email.IsNullOrEmpty())
                return null;

            var attention = string.Format(_configuration.GetValue<string>("EmailConfirmationMessage"), email);
            var subAttention = await GetFareLockAttentionMessageViewRes(reservation);

            List<MOBItem> captions = new List<MOBItem>();
            if (!attention.IsNullOrEmpty())
            {
                captions.Add(GetFareLockViewResPaymentCaptions("ConfirmationPage_Attention", attention));
                if (!subAttention.IsNullOrEmpty())
                {
                    captions.Add(GetFareLockViewResPaymentCaptions("ConfirmationPage_SubAttention", subAttention));
                }
                var confirmationPageCaptions = await _eTCUtility.GetCaptions("ConfirmationPage_ViewRes_Captions");
                confirmationPageCaptions.ForEach(p =>
                {
                    if (!p.IsNullOrEmpty())
                    {
                        captions.Add(p);
                    }
                });
                captions.Add(GetFareLockViewResPaymentCaptions("ConfirmationPage_Email", email));
                captions.Add(GetFareLockViewResPaymentCaptions("PaymentPage_ProductCode", "FLK_ViewRes"));
            }

            // To find if PNR is corporate travel
            bool isCorporateFareLock = IsCorporateTraveler(reservation.Characteristic);
            if (isCorporateFareLock)
            {
                var priceText = _configuration.GetValue<string>("CorporateRateText");
                string vendorName = GetCharactersticValue(reservation.Characteristic, "CorporateTravelVendorName");
                if (!priceText.IsNullOrEmpty() && !vendorName.IsNullOrEmpty())
                {
                    captions.Add(GetFareLockViewResPaymentCaptions("Corporate_PriceBreakDownText", priceText));
                    vendorName = await _featureSettings.GetFeatureSettingValue("EnableSuppressingCompanyNameForBusiness").ConfigureAwait(false) ? string.Empty : string.Format(_configuration.GetValue<string>("CorporateBookingConfirmationMessage"), vendorName);
                    var companyName = string.Empty;
                    if (!string.IsNullOrEmpty(vendorName)) // Added code to camel case company name as PNR service will return all caps.
                    {
                        var splitCamel = vendorName.TrimEnd().Split(' ');
                        if (!splitCamel.IsNullOrEmpty() && splitCamel.Any())
                        {
                            foreach (var camel in splitCamel)
                            {
                                companyName = companyName.IsNullOrEmpty() ? FirstLetterToUpperCase(camel) : companyName + " " + FirstLetterToUpperCase(camel);
                            }
                        }
                    }
                    captions.Add(GetFareLockViewResPaymentCaptions("Corporate_PNRHeaderText", companyName));
                }
            }

            return captions;
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
        private string FirstLetterToUpperCase(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            if (value.Length == 1)
                return value[0].ToString().ToUpper();

            return value[0].ToString().ToUpper() + value.Substring(1).ToLower();
        }
        private string GetCharactersticValue(Collection<Characteristic> characteristics, string code)
        {
            if (characteristics == null || characteristics.Count <= 0) return string.Empty;
            var characteristic = characteristics.FirstOrDefault(c => c != null && c.Code != null
            && !string.IsNullOrEmpty(c.Code) && c.Code.Trim().Equals(code, StringComparison.InvariantCultureIgnoreCase));
            return characteristic == null ? string.Empty : characteristic.Value;
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
        private async Task<string> GetFareLockAttentionMessageViewRes(Service.Presentation.ReservationModel.Reservation reservation)
        {
            if (reservation.IsNullOrEmpty())
                return null;
            string foreignCountry = string.Empty; bool caExists = false; bool meExists = false; bool internationPNR = false;
            var tripnum = string.Empty;
            reservation.FlightSegments.ForEach(p =>
            {
                if (!p.IsNullOrEmpty() && !p.FlightSegment.IsNullOrEmpty() && !p.FlightSegment.IsInternational.IsNullOrEmpty() && p.FlightSegment.IsInternational.ToUpper().Equals("TRUE"))
                {
                    if (tripnum.IsNullOrEmpty())
                    {
                        tripnum = p.TripNumber;
                    }
                    meExists = p.FlightSegment.ArrivalAirport.IATACountryCode.CountryCode.Equals("MX") ? true : false;
                    caExists = p.FlightSegment.ArrivalAirport.IATACountryCode.CountryCode.Equals("CA") ? true : false;
                    internationPNR = true;
                }
            });

            if (internationPNR)
            {
                var JourneyType = GetJourneyTypeDescription(reservation);
                var flightSegment = new FlightSegment();
                if (JourneyType.Equals("Multicity") && !tripnum.IsNullOrEmpty())
                {
                    flightSegment = reservation.FlightSegments.FirstOrDefault(p => (!p.IsNullOrEmpty() && !p.FlightSegment.IsNullOrEmpty() && p.TripNumber.Equals(tripnum))).FlightSegment;
                }
                else if (JourneyType.Equals("Roundtrip"))
                {
                    flightSegment = reservation.FlightSegments.FirstOrDefault(p => (!p.IsNullOrEmpty() && !p.FlightSegment.IsNullOrEmpty())).FlightSegment;
                }
                else
                {
                    flightSegment = reservation.FlightSegments.LastOrDefault(p => (!p.IsNullOrEmpty() && !p.FlightSegment.IsNullOrEmpty())).FlightSegment;
                }
                foreignCountry = !flightSegment.IsNullOrEmpty() && !flightSegment.ArrivalAirport.IsNullOrEmpty() && !flightSegment.ArrivalAirport.IATACode.IsNullOrEmpty()
                                 ? await GetAirportName(flightSegment.ArrivalAirport.IATACode) : string.Empty;
            }

            if (!string.IsNullOrEmpty(foreignCountry))
            {
                string docTitles = "'TripAdvisoryForSelectedOriginDestination','TripAdvisoryForCanada','TripAdvisoryForMexico'";

                List<United.Definition.MOBLegalDocument> docs = await _legalDocumentsForTitlesService.GetNewLegalDocumentsForTitles(docTitles, _headers.ContextValues.TransactionId, true);

                if (docs != null && docs.Count > 0)
                {
                    if (caExists && docs.Find(item => item.Title == "TripAdvisoryForCanada") != null)
                    {
                        return docs.Find(item => item.Title == "TripAdvisoryForCanada").Document;
                    }
                    else if (meExists && docs.Find(item => item.Title == "TripAdvisoryForMexico") != null)
                    {
                        return docs.Find(item => item.Title == "TripAdvisoryForMexico").Document;
                    }
                    else if (docs.Find(item => item.Title == "TripAdvisoryForSelectedOriginDestination") != null)
                    {
                        return string.Format(docs.Find(item => item.Title == "TripAdvisoryForSelectedOriginDestination").Document, foreignCountry);
                    }
                }
            }

            return string.Empty;
        }
        private string GetJourneyTypeDescription(United.Service.Presentation.ReservationModel.Reservation reservation)
        {
            if (reservation != null)
            {
                var JourneyType = reservation.Type.FirstOrDefault(o => (o.Description != null && o.Key != null && o.Description.Equals("JOURNEY_TYPE", StringComparison.OrdinalIgnoreCase)));

                return JourneyType.IsNullOrEmpty() ? UtilityHelper.GetTravelType(reservation.FlightSegments) : UtilityHelper.GetTravelType(JourneyType.Key);
            }
            return string.Empty;
        }
        public async System.Threading.Tasks.Task<string> GetAirportName(string airportCode)
        {
            AirportDynamoDB airportDynamoDB = new AirportDynamoDB(_configuration, _dynamoDBService);
            return await airportDynamoDB.GetAirportName(airportCode, "trans0");
        }
        private bool IsCheckFareLockUsingProductCode(MOBShoppingCart shoppingCart)
        {
            return _configuration.GetValue<bool>("EnableFareLockPurchaseViewRes") && shoppingCart.Products.Any(o => (o != null && o.Code != null && o.Code.Equals("FLK_VIEWRES", StringComparison.OrdinalIgnoreCase)));
        }
        private async Task<List<MOBSection>> GetConfirmationMessageForWLPNRManageRes(FlightReservationResponse flightReservationResponse, List<MOBSection> AlertMessages, string Flow)
        {
            var message = new List<MOBSection>();
            if (!flightReservationResponse.Errors.Any() || !AlertMessages.Any())
            {
                bool isPCUPurchase = flightReservationResponse.CheckoutResponse.ShoppingCart.Items.Where(x => x.Product.FirstOrDefault().Code != "RES").Select(x => x.Product.FirstOrDefault().Code).Distinct().Any(x => x.Equals("PCU"));
                // PCU Waitlist Confirmation Message
                if (_configuration.GetValue<bool>("EnablePCUWaitListPNRManageRes") && isPCUPurchase && IsTherePCUWaitListSegment(flightReservationResponse))
                {
                    var getPCUSegmentFromTravelOptions = getPCUSegments(flightReservationResponse, Flow);
                    var isUPP = getPCUSegmentFromTravelOptions != null ? isPCUUPPWaitListSegment(getPCUSegmentFromTravelOptions, flightReservationResponse.Reservation.FlightSegments) : false;
                    var refundMessages = isUPP ? (await _eTCUtility.GetCaptions("PC_PCUWaitList_RefundMessage_UPPMessage")) : (await _eTCUtility.GetCaptions("PC_PCUWaitList_RefundMessage_GenericMessage"));
                    message = AssignRefundMessage(refundMessages);
                }
            }
            return message.Any() ? message : null;
        }
        private bool IsTherePCUWaitListSegment(FlightReservationResponse flightReservationResponse)
        {
            return flightReservationResponse.CheckoutResponse.ShoppingCartResponse.UpgradeDetails.Any(p => p != null && p.Flight != null && !p.Flight.FlightStatus.IsNullOrEmpty() && p.Flight.FlightStatus.Equals("WAITLISTUPGRADEREQUESTED", StringComparison.OrdinalIgnoreCase));
        }
        private bool isPCUUPPWaitListSegment(List<SubItem> PCUTravelOptions, Collection<ReservationFlightSegment> FlightSegments)
        {
            var hasPCUUPPSegment = PCUTravelOptions.Where(t => t != null && t.Description != null && t.Description.ToUpper().Equals("UNITED PREMIUM PLUS")).Select(t => t.SegmentNumber).Distinct().ToCollection();
            bool isBusiness = false;
            if (!hasPCUUPPSegment.IsNull() && hasPCUUPPSegment.Any())
            {
                foreach (var segment in hasPCUUPPSegment)
                {
                    var selectedSegment = FlightSegments.Where(y => y != null && y.FlightSegment != null && y.FlightSegment.SegmentNumber == Convert.ToInt32(segment)).FirstOrDefault().TripNumber;
                    var flightSegmentCharacteristics = selectedSegment != null ? FlightSegments.Where(x => !x.TripNumber.IsNullOrEmpty() && !selectedSegment.IsNullOrEmpty() && x.TripNumber == selectedSegment && x.BookingClass != null && x.BookingClass.Code != null && (x.BookingClass.Code.Equals("PZ") || x.BookingClass.Code.Equals("PN"))).Select(t => t.Characteristic).FirstOrDefault() : null;
                    isBusiness = flightSegmentCharacteristics != null && flightSegmentCharacteristics.Any() ? flightSegmentCharacteristics.Any(p => p != null && !p.Code.IsNullOrEmpty() && !p.Value.IsNullOrEmpty() && p.Code.Equals("Waitlisted", StringComparison.OrdinalIgnoreCase) && p.Value.Equals("True", StringComparison.OrdinalIgnoreCase)) : false;
                    if (isBusiness)
                        return isBusiness;
                }
            }
            return isBusiness;
        }

        private List<SubItem> getPCUSegments(FlightReservationResponse flightReservationResponse, string Flow)
        {
            var segments = flightReservationResponse.DisplayCart.TravelOptions.Where(d => d != null && d.Key == "PCU").SelectMany(x => x.SubItems).Where(x => _shoppingCartUtility.ShouldIgnoreAmount(x) ? true : x.Amount != 0).ToList();
            if (segments != null && segments.Any())
                segments.OrderBy(x => x.SegmentNumber).GroupBy(x => x.SegmentNumber);
            return segments.Any() ? segments : null;
        }

        public List<MOBSection> AssignRefundMessage(List<MOBItem> refundMessages)
        {
            List<MOBSection> pcuWaitListManageResMessage = new List<MOBSection>();
            if (refundMessages != null && refundMessages.Any())
            {
                var PCUWaitListMessage = new MOBSection()
                {
                    Text1 = refundMessages.Where(x => x.Id == "HEADER").Select(x => x.CurrentValue).FirstOrDefault().ToString(),
                    Text2 = refundMessages.Where(x => x.Id == "BODY").Select(x => x.CurrentValue).FirstOrDefault().ToString()
                };
                pcuWaitListManageResMessage.Add(PCUWaitListMessage);
            }
            return pcuWaitListManageResMessage;
        }

        private bool IsSuccessOrValidReponse(FlightReservationResponse flightReservationResponse)
        {
            if (flightReservationResponse != null && flightReservationResponse.Status.Equals(United.Services.FlightShopping.Common.StatusType.Success) && (flightReservationResponse.Errors == null || flightReservationResponse.Errors.Count() == 0))
                return true;

            if (flightReservationResponse.Errors != null && flightReservationResponse.Errors.Any(e => e != null && (e.MinorCode == "90506" || e.MinorCode == "90584")))
                return true;

            return false;
        }
        private async Task<List<MOBSection>> GetErrorMessagesForConfirmationScreen(FlightReservationResponse flightReservationResponse, string flow, List<MOBProdDetail> prodDetails = null)
        {
            var alertMessages = new List<MOBSection>();
            if (flightReservationResponse.Errors != null && flightReservationResponse.Errors.Any(e => e != null && (e.MinorCode == "90506" || e.MinorCode == "90585")))
            {
                List<string> refundedSegmentNums = null;
                var isRefundSuccess = false;
                var isPartialSuccess = false;
                var isMerchProcessFailed = false;
                MOBSection pcuAlertMessages = null;
                bool isEnableBEBuyOut = _configuration.GetValue<bool>("EnableBasicEconomyBuyOutInViewRes");
                if (flightReservationResponse.Errors.Any(e => e != null && (e.MinorCode == "90585") && flightReservationResponse.DisplayCart.TravelOptions != null && flightReservationResponse.DisplayCart.TravelOptions.Any(x => x != null && (x.Key == "PCU" || (isEnableBEBuyOut && x.Key == "BEB")))))
                {
                    isMerchProcessFailed = true;
                    pcuAlertMessages = await GetAlertMessage(flightReservationResponse, isRefundSuccess, isPartialSuccess, refundedSegmentNums, isMerchProcessFailed);
                }
                else if (flightReservationResponse.Errors.Any(e => e != null && (e.MinorCode == "90506")))
                {
                    if (isEnableBEBuyOut && flightReservationResponse.DisplayCart.TravelOptions != null && flightReservationResponse.DisplayCart.TravelOptions.Any(t => t != null && t.Key == "BEB"))
                    {
                        isRefundSuccess = IsEMDRefundSuccess(flightReservationResponse.CheckoutResponse.ShoppingCartResponse.Items);
                    }
                    else
                    {
                        isRefundSuccess = _shoppingCartUtility.IsRefundSuccess(flightReservationResponse.CheckoutResponse.ShoppingCartResponse.Items, out refundedSegmentNums, flow);
                        isPartialSuccess = isRefundSuccess ? flightReservationResponse.DisplayCart.TravelOptions.Where(t => t.Key == "PCU").SelectMany(x => x.SubItems).Where(x => x.Amount != 0).SelectMany(s => s.SegmentNumber).Distinct().Count() != refundedSegmentNums.Count() : false;
                        if (!isPartialSuccess && isRefundSuccess)
                        {
                            if (_configuration.GetValue<bool>("EnableCSL30ManageResSelectSeatMap"))
                            {
                                if (flightReservationResponse.DisplayCart.DisplaySeats != null && flightReservationResponse.DisplayCart.DisplaySeats.Any(s => (s.Seat == "---" || string.IsNullOrEmpty(s.SeatAssignMessage) || !s.SeatAssignMessage.Equals("SEATS ASSIGNED", StringComparison.OrdinalIgnoreCase))))
                                {
                                    isPartialSuccess = true;
                                }
                            }
                            else
                            {
                                if (flightReservationResponse.DisplayCart.DisplaySeats != null && flightReservationResponse.DisplayCart.DisplaySeats.Any(s => (s.Seat == "---" || !string.IsNullOrEmpty(s.SeatAssignMessage))))
                                {
                                    isPartialSuccess = true;
                                }
                            }

                            if (!isPartialSuccess)
                            {
                                isPartialSuccess = HasAnySuccessfullProduct(prodDetails);
                            }
                        }
                    }

                    pcuAlertMessages = await GetAlertMessage(flightReservationResponse, isRefundSuccess, isPartialSuccess, refundedSegmentNums, isMerchProcessFailed);
                }

                if (pcuAlertMessages != null)
                {
                    alertMessages.Add(pcuAlertMessages);
                }
            }

            if (IsThereSeatAssigmentFailure(flightReservationResponse))
            {
                var seatFailureAlertMessage = new MOBSection()
                {
                    Text1 = "Seat assignment failed",
                    Text2 = _configuration.GetValue<string>("SeatsUnAssignedMessage").Trim()
                };

                alertMessages.Add(seatFailureAlertMessage);
            }
            return alertMessages.Any() ? alertMessages : null;

        }
        private bool IsThereSeatAssigmentFailure(FlightReservationResponse flightReservationResponse)
        {
            if (_configuration.GetValue<bool>("EnableCSL30ManageResSelectSeatMap"))
            {
                if (flightReservationResponse.DisplayCart.DisplaySeats != null && flightReservationResponse.DisplayCart.DisplaySeats.Any(s => s.Seat == "---" || string.IsNullOrEmpty(s.SeatAssignMessage) || !s.SeatAssignMessage.Equals("SEATS ASSIGNED", StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }
            else
            {
                if (flightReservationResponse.Errors != null && flightReservationResponse.Errors.Any(e => e != null && (e.MinorCode == "90584")))
                {
                    if (flightReservationResponse.Errors.Any(e => e != null && (e.Message == "SeatAssignmentFailed . ")))
                    {
                        return true;
                    }
                    if (_configuration.GetValue<bool>("EnableCSL30ManageResSelectSeatMap"))
                    {
                        if (flightReservationResponse.DisplayCart.DisplaySeats != null && flightReservationResponse.DisplayCart.DisplaySeats.Any(s => s.Seat == "---" || string.IsNullOrEmpty(s.SeatAssignMessage) || !s.SeatAssignMessage.Equals("SEATS ASSIGNED", StringComparison.OrdinalIgnoreCase)))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (flightReservationResponse.DisplayCart.DisplaySeats != null && flightReservationResponse.DisplayCart.DisplaySeats.Any(s => s.Seat == "---" || !string.IsNullOrEmpty(s.SeatAssignMessage)))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool HasAnySuccessfullProduct(List<MOBProdDetail> prodDetails)
        {
            return prodDetails != null && prodDetails.Any() && prodDetails.Any(p => p != null && p.Segments != null && p.Segments.Any(s => s != null && s.SubSegmentDetails != null && s.SubSegmentDetails.Any(subSeg => subSeg != null && !subSeg.IsPurchaseFailure)));
        }

        private bool IsEMDRefundSuccess(Collection<ShoppingCartItemResponse> items)
        {
            var item = items?.FirstOrDefault(i => i != null && i.Item != null && !string.IsNullOrEmpty(i.Item.Category) && i.Item.Category.Equals("Reservation.Merchandise.BEB"));
            if (item == null) return false;

            var couponStatus = item?.Item?.Product?.FirstOrDefault()?.EmdDetails?.FirstOrDefault(p => p != null && !string.IsNullOrEmpty(p.CouponStatus)).CouponStatus;
            if (!string.IsNullOrEmpty(couponStatus) && couponStatus.Equals("REFUND:SUCCESS", StringComparison.OrdinalIgnoreCase)) return true;

            return false;
        }
        private async Task<RegisterFormsOfPaymentRequest> GetRegisterFormsOfPaymentsRequest_CFOP(MOBCheckOutRequest checkOutRequest)
        {
            _logger.LogInformation("GetRegisterFormsOfPaymentsRequest_CFOP {request} with {sessionId}", JsonConvert.SerializeObject(checkOutRequest), checkOutRequest.SessionId);

            bool isTPIcall = false;
            isTPIcall = Convert.ToBoolean(_configuration.GetValue<string>("ShowTripInsuranceSwitch") ?? "false") && checkOutRequest.IsTPI;
            if (isTPIcall)// ==>> Venkat and Elise chagne code to offer TPI postbooking when inline TPI is off for new clients 2.1.36 and above
            {
                isTPIcall = EnableTPI(checkOutRequest.Application.Id, checkOutRequest.Application.Version.Major, 1);
            }
            bool isSecondaryPaymentForTPI = EnableTPI(checkOutRequest.Application.Id, checkOutRequest.Application.Version.Major, 3) && checkOutRequest.IsTPI && checkOutRequest.IsSecondaryPayment;

            var persistedShoppingCart = new MOBShoppingCart();
            persistedShoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(checkOutRequest.SessionId, persistedShoppingCart.ObjectName, new List<string> { checkOutRequest.SessionId, persistedShoppingCart.ObjectName }).ConfigureAwait(false);
            Reservation persistedReservation = new Reservation();
            persistedReservation = await _sessionHelperService.GetSession<Reservation>(checkOutRequest.SessionId, persistedReservation.ObjectName, new List<string> { checkOutRequest.SessionId, persistedReservation.ObjectName }).ConfigureAwait(false);
            if (persistedShoppingCart == null)
                throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));

            bool isCompleteFarelockPurchase = _configuration.GetValue<bool>("EnableFareLockPurchaseViewRes") && persistedShoppingCart.Products.Any(o => (o != null && o.Code != null && o.Code.Equals("FLK_VIEWRES", StringComparison.OrdinalIgnoreCase)));

            //Fix for iOS Blue App issue sending CheckOut request with FarelockAutoTicket=true even when its not FareLock Purchase -- MOBILE-8940 -- Shashank
            if (!_configuration.GetValue<bool>("DisableFarelockAutoTicketCheck_MOBILE8940") && checkOutRequest.Flow.Equals("BOOKING", StringComparison.OrdinalIgnoreCase) && !persistedShoppingCart.Products.IsNullOrEmpty())
            {
                bool isFarelockAutoTicket = persistedShoppingCart.Products.Any(o => (o != null && o.Code != null && o.Code.Equals("FareLock", StringComparison.OrdinalIgnoreCase))) && checkOutRequest.FareLockAutoTicket;
                if (!(await _featureSettings.GetFeatureSettingValue("DisableFarelockAutoTicketFix_MOBILE-26671")))
                {
                    isFarelockAutoTicket = persistedShoppingCart.Products.Any(o =>
                                                                                    (o != null && o.Code != null &&
                                                                                    (
                                                                                    o.Code.Equals("FareLock", StringComparison.OrdinalIgnoreCase)
                                                                                    || o.Code.Equals("FLK", StringComparison.OrdinalIgnoreCase)
                                                                                    )))
                                            && checkOutRequest.FareLockAutoTicket;
                }

                if (!isFarelockAutoTicket)
                {
                    checkOutRequest.FareLockAutoTicket = false;
                }
            }
            var session = new Session();
            session = await _sessionHelperService.GetSession<Session>(checkOutRequest.SessionId, session.ObjectName, new List<string> { checkOutRequest.SessionId, session.ObjectName }).ConfigureAwait(false);
            RegisterFormsOfPaymentRequest registerFormsOfPaymentRequest = new RegisterFormsOfPaymentRequest();

            if (string.Equals(checkOutRequest.Flow, "UPGRADEMALL", StringComparison.OrdinalIgnoreCase))
            { registerFormsOfPaymentRequest.WorkFlowType = WorkFlowType.UpgradesPurchase; }

            registerFormsOfPaymentRequest.Reservation = null;
            registerFormsOfPaymentRequest.CartId = checkOutRequest.CartId;
            registerFormsOfPaymentRequest.Channel = _configuration.GetValue<string>("Shopping - ChannelType");
            registerFormsOfPaymentRequest.CheckInProductPurchase = UtilityHelper.IsCheckinFlow(checkOutRequest.Flow);
            registerFormsOfPaymentRequest.Emails = new Collection<Service.Presentation.CommonModel.EmailAddress>();
            AddEmailToRegisterFormsOfPaymentRequest(checkOutRequest.FormofPaymentDetails.EmailAddress, registerFormsOfPaymentRequest);
            registerFormsOfPaymentRequest.AdditionalData = string.IsNullOrEmpty(checkOutRequest.AdditionalData) ? checkOutRequest.DeviceId : checkOutRequest.AdditionalData;
            registerFormsOfPaymentRequest.FareLockAutoTicket = !ConfigUtility.IsViewResFlowCheckOut(checkOutRequest.Flow) ? checkOutRequest.FareLockAutoTicket : false;
            registerFormsOfPaymentRequest.DontInvokeCheckout = false;
            if (isCompleteFarelockPurchase)
            {
                registerFormsOfPaymentRequest.FareLockPurchase = true;
                registerFormsOfPaymentRequest.PostPurchase = false;
            }
            else if (checkOutRequest.Flow == FlowType.BOOKING.ToString())
                registerFormsOfPaymentRequest.PostPurchase = false;
            else if (checkOutRequest.Flow == FlowType.RESHOP.ToString())
                registerFormsOfPaymentRequest.PostPurchase = false;
            else if (ConfigUtility.IsViewResFlowCheckOut(checkOutRequest.Flow))
                registerFormsOfPaymentRequest.PostPurchase = true;
            else
            {
                registerFormsOfPaymentRequest.PostPurchase = !registerFormsOfPaymentRequest.CheckInProductPurchase;
            }

            if (IsBagPurchaseFromCheckinFlow(checkOutRequest.Flow, persistedShoppingCart.PaymentTarget))
            {
                registerFormsOfPaymentRequest.CountryCode = string.IsNullOrEmpty(persistedShoppingCart.PointofSale) ? "US" : persistedShoppingCart.PointofSale;
            }
            // Added as part of Bug 128996 - Android: Unable to add PA, OTP, E+ (Ancillary Products) on RTI screen when we take GUAM as billing country : Issuf
            else if (checkOutRequest != null && checkOutRequest.FormofPaymentDetails.BillingAddress != null && checkOutRequest.FormofPaymentDetails.BillingAddress.Country != null && !string.IsNullOrEmpty(checkOutRequest.FormofPaymentDetails.BillingAddress.Country.Code))
            {
                registerFormsOfPaymentRequest.CountryCode = checkOutRequest.FormofPaymentDetails.BillingAddress.Country.Code;
            }
            else
                registerFormsOfPaymentRequest.CountryCode = "US";

            GetCheckoutRequestPhones(checkOutRequest, registerFormsOfPaymentRequest);

            registerFormsOfPaymentRequest.RemoveFareLock = false;
            registerFormsOfPaymentRequest.FormsOfPayment = new List<FormOfPayment>();

            switch (checkOutRequest.Application.Id)
            {
                case 1:
                    registerFormsOfPaymentRequest.DeviceType = "iOS";
                    break;
                case 2:
                    registerFormsOfPaymentRequest.DeviceType = "Android";
                    break;
            }

            string[] productCodes = persistedShoppingCart.Products.Select(x => x.Code).ToArray();
            int intProductCodesCount = 0;

            ///For RegisterSeats-Free Seats
            if (ConfigUtility.IsViewResFlowPaymentSvcEnabled(checkOutRequest.Flow) && registerFormsOfPaymentRequest.PostPurchase && productCodes.IsNullOrEmpty())
            {
                // Below code is not in sysnc with OnPrem - Anshika to check this code
                FormOfPayment formOfPayment = new FormOfPayment();
                formOfPayment.PaymentTarget = persistedShoppingCart.PaymentTarget;
                registerFormsOfPaymentRequest.FormsOfPayment.Add(formOfPayment);
            }
            Service.Presentation.PaymentModel.CreditCard applepayCreditCard = null;
            if (!_configuration.GetValue<bool>("DisableDoubleAuthorizationFixForApplePay") && checkOutRequest?.FormofPaymentDetails?.FormOfPaymentType?.ToUpper() == MOBFormofPayment.ApplePay.ToString().ToUpper())
            {
                MOBApplePayLoad applePayLoad = JsonConvert.DeserializeObject<MOBApplePayLoad>(checkOutRequest.FormofPaymentDetails.ApplePayInfo.ApplePayLoadJSON);
                applepayCreditCard = await GenerateApplepayTokenWithDataVault(applePayLoad, session.SessionId, session.Token, checkOutRequest.Application, checkOutRequest.DeviceId);
            }
            MOBApplyPromoCodeResponse mOBApplyPromoCodeResponse = new MOBApplyPromoCodeResponse();
            mOBApplyPromoCodeResponse = await _sessionHelperService.GetSession<MOBApplyPromoCodeResponse>(session.SessionId, new MOBApplyPromoCodeResponse().ObjectName, new List<string> { session.SessionId, new MOBApplyPromoCodeResponse().ObjectName }).ConfigureAwait(false);
            foreach (string productCode in productCodes)
            {
                intProductCodesCount++;
                if (checkOutRequest.Flow == FlowType.RESHOP.ToString() && productCode == "RES")
                {
                    await ReshopPaymentAmount(checkOutRequest);
                }
                else if (ConfigUtility.IsViewResFlowCheckOut(checkOutRequest.Flow))
                {
                    checkOutRequest.PaymentAmount = persistedShoppingCart.TotalPrice;
                }
                else if (await _featureSettings.GetFeatureSettingValue("PromoCode_MM") && checkOutRequest.Flow == FlowType.BOOKING.ToString()
                    && mOBApplyPromoCodeResponse?.ShoppingCart?.PromoCodeDetails?.PromoCodes != null
                    && (mOBApplyPromoCodeResponse.ShoppingCart.PromoCodeDetails.PromoCodes.Any(a => a.IsSuccess == false) ||
                    (await _featureSettings.GetFeatureSettingValue("EnableInvalidPromoCodeIssue").ConfigureAwait(false) && mOBApplyPromoCodeResponse.ShoppingCart.PromoCodeDetails.PromoCodes.Count == 0)))
                {
                    checkOutRequest.PaymentAmount = persistedShoppingCart.TotalPrice;
                }
                else
                {
                    checkOutRequest.PaymentAmount = persistedShoppingCart.Products.Where(x => x.Code == productCode).FirstOrDefault().ProdTotalPrice;
                    checkOutRequest.PaymentAmount = checkOutRequest.PaymentAmount == "0.00" ? "0" : checkOutRequest.PaymentAmount;
                }
                FormOfPayment formOfPayment = new FormOfPayment();
                formOfPayment.Payment = new Service.Presentation.PaymentModel.FormOfPayment();
                Address billingAddress = null;
                #region - Fix by Nizam for #2169 - Temporary fix on client behalf

                if (!ConfigUtility.IsViewResFlowCheckOut(checkOutRequest.Flow) && checkOutRequest.PaymentAmount != "0" &&
                    checkOutRequest.FormofPaymentDetails.CreditCard == null &&
                    checkOutRequest.FormofPaymentDetails.PayPal == null &&
                    checkOutRequest.FormofPaymentDetails.PayPalPayor == null &&
                    checkOutRequest.FormofPaymentDetails.Masterpass == null &&
                    checkOutRequest.FormofPaymentDetails.MasterPassSessionDetails == null &&
                    checkOutRequest.FormofPaymentDetails.ApplePayInfo == null &&
                    checkOutRequest.FormofPaymentDetails.Uplift == null &&
                    checkOutRequest.FormofPaymentDetails.TravelBankDetails?.TBApplied == 0
                    && (ConfigUtility.IncludeFFCResidual(checkOutRequest.Application.Id, checkOutRequest.Application.Version.Major) ?
                        (checkOutRequest.FormofPaymentDetails.TravelFutureFlightCredit?.FutureFlightCredits?.Count == 0) : true)
                    && (ConfigUtility.IsETCCombinabilityEnabled(checkOutRequest.Application.Id, checkOutRequest.Application.Version.Major) &&
                        _configuration.GetValue<bool>("NoFormOfPaymentErrorToggle") ?
                                    (checkOutRequest.FormofPaymentDetails.TravelCertificate == null ||
                                    checkOutRequest.FormofPaymentDetails.TravelCertificate?.Certificates == null ||
                                    checkOutRequest.FormofPaymentDetails.TravelCertificate?.Certificates?.Count == 0) : true)
                    && checkOutRequest.FormofPaymentDetails.FormOfPaymentType != null)

                    throw new MOBUnitedException(_configuration.GetValue<string>("NoFormOfPaymentErrorMessage"));

                else if (!ConfigUtility.IsViewResFlowCheckOut(checkOutRequest.Flow) && checkOutRequest.PaymentAmount != "0" &&
                        checkOutRequest.FormofPaymentDetails.CreditCard == null &&
                        checkOutRequest.FormofPaymentDetails.PayPal == null &&
                        checkOutRequest.FormofPaymentDetails.PayPalPayor == null &&
                        checkOutRequest.FormofPaymentDetails.Masterpass == null &&
                        checkOutRequest.FormofPaymentDetails.MasterPassSessionDetails == null &&
                        checkOutRequest.FormofPaymentDetails.ApplePayInfo == null &&
                        checkOutRequest.FormofPaymentDetails.Uplift == null &&
                        checkOutRequest.FormofPaymentDetails.TravelBankDetails?.TBApplied == 0
                        && (ConfigUtility.IncludeFFCResidual(checkOutRequest.Application.Id, checkOutRequest.Application.Version.Major) ?
                        (checkOutRequest.FormofPaymentDetails.TravelFutureFlightCredit?.FutureFlightCredits?.Count == 0) : true)
                        && (ConfigUtility.IsETCCombinabilityEnabled(checkOutRequest.Application.Id, checkOutRequest.Application.Version.Major) &&
                           _configuration.GetValue<bool>("NoFormOfPaymentErrorToggle") ?
                                        (checkOutRequest.FormofPaymentDetails.TravelCertificate == null ||
                                        checkOutRequest.FormofPaymentDetails.TravelCertificate?.Certificates == null ||
                                        checkOutRequest.FormofPaymentDetails.TravelCertificate?.Certificates?.Count == 0) : true)
                        && (persistedShoppingCart.Products.Any(x => x.Code == "SEATASSIGNMENTS") &&
                            checkOutRequest.FormofPaymentDetails.FormOfPaymentType != null)
                        )
                    throw new MOBUnitedException(_configuration.GetValue<string>("NoFormOfPaymentErrorMessage"));

                else if (ConfigUtility.IsViewResFlowCheckOut(checkOutRequest.Flow) && checkOutRequest.PaymentAmount != "0" &&
                    checkOutRequest.FormofPaymentDetails.CreditCard == null &&
                    checkOutRequest.FormofPaymentDetails.PayPal == null &&
                    checkOutRequest.FormofPaymentDetails.PayPalPayor == null &&
                    checkOutRequest.FormofPaymentDetails.Masterpass == null &&
                    checkOutRequest.FormofPaymentDetails.MasterPassSessionDetails == null &&
                    checkOutRequest.FormofPaymentDetails.ApplePayInfo == null &&
                    checkOutRequest.FormofPaymentDetails.MilesFOP == null
               && checkOutRequest.FormofPaymentDetails.Uplift == null
               && (ConfigUtility.IsManageResETCEnabled(checkOutRequest.Application.Id, checkOutRequest.Application.Version.Major) ? (checkOutRequest.FormofPaymentDetails.TravelCertificate == null || checkOutRequest.FormofPaymentDetails.TravelCertificate?.Certificates == null || checkOutRequest.FormofPaymentDetails.TravelCertificate?.Certificates?.Count == 0) : true)
               && !checkOutRequest.FormofPaymentDetails.FormOfPaymentType.IsNullOrEmpty())
                    throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));

                else if (ConfigUtility.IsViewResFlowCheckOut(checkOutRequest.Flow) && checkOutRequest.PaymentAmount != "0" &&
                    checkOutRequest.FormofPaymentDetails.CreditCard == null &&
                    checkOutRequest.FormofPaymentDetails.PayPal == null &&
                    checkOutRequest.FormofPaymentDetails.PayPalPayor == null &&
                    checkOutRequest.FormofPaymentDetails.Masterpass == null &&
                    checkOutRequest.FormofPaymentDetails.MasterPassSessionDetails == null &&
                    checkOutRequest.FormofPaymentDetails.ApplePayInfo == null &&
                    checkOutRequest.FormofPaymentDetails.MilesFOP == null &&
                    checkOutRequest.FormofPaymentDetails.Uplift == null &&
                    (persistedShoppingCart.Products.Any(x => x.Code == "SEATASSIGNMENTS") &&
                    (ConfigUtility.IsManageResETCEnabled(checkOutRequest.Application.Id, checkOutRequest.Application.Version.Major) ? (checkOutRequest.FormofPaymentDetails.TravelCertificate == null || checkOutRequest.FormofPaymentDetails.TravelCertificate?.Certificates == null || checkOutRequest.FormofPaymentDetails.TravelCertificate?.Certificates?.Count == 0) : true) &&
                    !checkOutRequest.FormofPaymentDetails.FormOfPaymentType.IsNullOrEmpty()))
                    throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                #endregion

                //Fix for CheckOut ArgNullException - AwardTravel full Miles purchase having $0 tax for some markets - MOBILE-9833 - Shashank
                else if (!_configuration.GetValue<bool>("DisableCheckforZeroTaxMilesPurchase") && (checkOutRequest.PaymentAmount == "0" || checkOutRequest.PaymentAmount.IsNullOrEmpty()) && checkOutRequest.Flow.Equals("BOOKING", StringComparison.OrdinalIgnoreCase)
                    && persistedReservation != null && persistedReservation.AwardTravel && !(isTPIcall || isSecondaryPaymentForTPI) && productCode == "RES")
                {
                    //Adding OPI Certificate
                    formOfPayment.Payment.Certificate = new Service.Presentation.PaymentModel.Certificate();
                    formOfPayment.Payment.Certificate.Type = new Genre();
                    formOfPayment.Payment.Certificate.Type.Key = "OPI";
                }
                //Fix for iOS only issue when MPAccount has no saved cards added in ViewRes Payment screen (No FormOfPaymentType) -- MOBILE-8915 -- Shashank
                else if (ConfigUtility.IsViewResFlowCheckOut(checkOutRequest.Flow) && _configuration.GetValue<bool>("NoFormOfPaymentErrorToggle") &&
                    checkOutRequest.FormofPaymentDetails.FormOfPaymentType.IsNullOrEmpty() &&
                    (checkOutRequest.PaymentAmount != "0" && checkOutRequest.PaymentAmount != "0.00") &&
                    checkOutRequest.FormofPaymentDetails.CreditCard == null &&
                    checkOutRequest.FormofPaymentDetails.PayPal == null &&
                    checkOutRequest.FormofPaymentDetails.PayPalPayor == null &&
                    checkOutRequest.FormofPaymentDetails.Masterpass == null &&
                    checkOutRequest.FormofPaymentDetails.MasterPassSessionDetails == null &&
                    checkOutRequest.FormofPaymentDetails.ApplePayInfo == null &&
                    checkOutRequest.FormofPaymentDetails.Uplift == null
                    && (ConfigUtility.IsManageResETCEnabled(checkOutRequest.Application.Id, checkOutRequest.Application.Version.Major) ? (checkOutRequest.FormofPaymentDetails.TravelCertificate == null || checkOutRequest.FormofPaymentDetails.TravelCertificate?.Certificates == null || checkOutRequest.FormofPaymentDetails.TravelCertificate?.Certificates?.Count == 0) : true))
                {
                    throw new MOBUnitedException(_configuration.GetValue<string>("NoFormOfPaymentErrorMessage"));
                }
                else if ((!_configuration.GetValue<bool>("DisableCheckforPaymentAmount") && checkOutRequest.PaymentAmount != "0") &&
                    !(checkOutRequest.FormofPaymentDetails.CreditCard == null &&
                    checkOutRequest.FormofPaymentDetails.PayPal == null &&
                    checkOutRequest.FormofPaymentDetails.PayPalPayor == null &&
                    checkOutRequest.FormofPaymentDetails.Masterpass == null &&
                    checkOutRequest.FormofPaymentDetails.MasterPassSessionDetails == null &&
                    checkOutRequest.FormofPaymentDetails.ApplePayInfo == null &&
                    checkOutRequest.FormofPaymentDetails.Uplift == null
                    && checkOutRequest.FormofPaymentDetails.FormOfPaymentType.IsNullOrEmpty()))
                {

                    if ((checkOutRequest.FormofPaymentDetails.FormOfPaymentType.ToUpper() == MOBFormofPayment.CreditCard.ToString().ToUpper() ||
                        checkOutRequest.FormofPaymentDetails.FormOfPaymentType.ToUpper() == MOBFormofPayment.VisaCheckout.ToString().ToUpper()) && checkOutRequest.FormofPaymentDetails.CreditCard != null ||
                        checkOutRequest.FormofPaymentDetails.FormOfPaymentType.ToUpper() == MOBFormofPayment.Uplift.ToString().ToUpper() && checkOutRequest.FormofPaymentDetails.Uplift != null)
                    {
                        formOfPayment.Payment.CreditCard = await MapToCslCreditCard(checkOutRequest, persistedShoppingCart.CurrencyCode, session, persistedShoppingCart.PaymentTarget);
                    }
                    else if ((checkOutRequest.FormofPaymentDetails.FormOfPaymentType.ToUpper() == MOBFormofPayment.PayPal.ToString().ToUpper()) || (checkOutRequest.FormofPaymentDetails.FormOfPaymentType.ToUpper() == MOBFormofPayment.PayPalCredit.ToString().ToUpper()))
                    {
                        #region //**TBD-paypal**//
                        _logger.LogInformation(session.SessionId, "CheckOut", "PayPal_ClientJSONRequest", checkOutRequest.Application.Id, checkOutRequest.Application.Version.Major, checkOutRequest.DeviceId, checkOutRequest, true, true);

                        formOfPayment.Payment.PayPal = PayPalPayLoad(persistedShoppingCart.FormofPaymentDetails.PayPalPayor, Convert.ToDouble(checkOutRequest.PaymentAmount), checkOutRequest.FormofPaymentDetails.PayPal);
                        billingAddress = formOfPayment.Payment.PayPal.BillingAddress;
                        AddEmailToRegisterFormsOfPaymentRequest(persistedShoppingCart.FormofPaymentDetails.PayPalPayor.PayPalContactEmailAddress, registerFormsOfPaymentRequest);
                        //formOfPayment.PaymentTarget = string.Join(",", persistedShoppingCart.Products.Select(x => x.Code).ToList());
                        //AddPhoneToRegisterFormsOfPaymentRequest(persistedShoppingCart.FormofPaymentDetails.PayPalPayor.PayPalContactPhoneNumber, registerFormsOfPaymentRequest, checkOutRequest.FormofPaymentType);
                        #endregion //**TBD-paypal**//
                    }
                    else if (checkOutRequest.FormofPaymentDetails.FormOfPaymentType.ToUpper() == MOBFormofPayment.Masterpass.ToString().ToUpper())
                    {
                        #region //**TBD-Masterpass**//
                        _logger.LogInformation(session.SessionId, "CheckOut", "Masterpass_ClientJSONRequest", checkOutRequest.Application.Id, checkOutRequest.Application.Version.Major, checkOutRequest.DeviceId, checkOutRequest, true, false);
                        //Reservation reservationForMasterpass = new Reservation();
                        //reservationForMasterpass = Persist.FilePersist.Load<Persist.Definition.Shopping.Reservation>(session.SessionId, reservationForMasterpass.ObjectName);
                        formOfPayment.Payment.CreditCard = await MasterpassPayLoad_CFOP(persistedShoppingCart.FormofPaymentDetails.MasterPassSessionDetails, Convert.ToDouble(checkOutRequest.PaymentAmount), Guid.NewGuid().ToString().ToUpper().Replace("-", ""), checkOutRequest.Application, checkOutRequest.DeviceId);
                        billingAddress = formOfPayment.Payment.CreditCard.BillingAddress;
                        AddEmailToRegisterFormsOfPaymentRequest(persistedShoppingCart.FormofPaymentDetails.MasterPassSessionDetails.ContactEmailAddress, registerFormsOfPaymentRequest);
                        //AddPhoneToRegisterFormsOfPaymentRequest(reservationForMasterpass.ReservationPhone, registerFormsOfPaymentRequest, checkOutRequest.FormofPaymentType);
                        //formOfPayment.PaymentTarget = string.Join(",", persistedShoppingCart.Products.Select(x => x.Code).ToList());
                        #endregion
                    }
                    else if (checkOutRequest.FormofPaymentDetails.FormOfPaymentType.ToUpper() == MOBFormofPayment.ApplePay.ToString().ToUpper())
                    {
                        #region //**Apple Pay**//
                        bool applePayDatavaultTokenToggle = _configuration.GetValue<bool>("ApplePayDatavaultTokenToggle");
                        _logger.LogInformation(session.SessionId, "CheckOut", "ApplePay_ClientJSONRequest", checkOutRequest.Application.Id, checkOutRequest.Application.Version.Major, checkOutRequest.DeviceId, checkOutRequest, true, false);
                        if (applePayDatavaultTokenToggle)
                        {
                            formOfPayment.Payment.CreditCard = await ApplePayLoadDataVault_CFOP(checkOutRequest.FormofPaymentDetails.ApplePayInfo, Convert.ToDouble(checkOutRequest.PaymentAmount), session, checkOutRequest.Application, checkOutRequest.DeviceId, applepayCreditCard);
                        }
                        //else
                        //{
                        //    formOfPayment.Payment.CreditCard = ApplePayLoad_CFOP(checkOutRequest.FormofPaymentDetails.ApplePayInfo, Convert.ToDouble(checkOutRequest.PaymentAmount), session, checkOutRequest.Application, checkOutRequest.DeviceId);
                        //}
                        billingAddress = formOfPayment.Payment.CreditCard.BillingAddress;
                        await _sessionHelperService.SaveSession<United.Service.Presentation.PaymentModel.CreditCard>(formOfPayment.Payment.CreditCard, session.SessionId, new List<string> { "United.Service.Presentation.PaymentModel.CreditCard" });
                        AddEmailToRegisterFormsOfPaymentRequest(checkOutRequest.FormofPaymentDetails.ApplePayInfo.EmailAddress.EmailAddress, registerFormsOfPaymentRequest);
                        //formOfPayment.PaymentTarget = string.Join(",", persistedShoppingCart.Products.Select(x => x.Code).ToList());
                        #endregion //**Apple Pay**//
                    }
                }
                if (string.IsNullOrEmpty(persistedShoppingCart.PaymentTarget))
                {
                    formOfPayment.PaymentTarget = isCompleteFarelockPurchase ? "RES" : string.Join(",", persistedShoppingCart.Products.Select(x => x.Code).ToList());
                }
                else if (ConfigUtility.IsViewResFlowCheckOut(checkOutRequest.Flow))
                {
                    formOfPayment.PaymentTarget = persistedShoppingCart.PaymentTarget;
                }
                else
                    formOfPayment.PaymentTarget = isCompleteFarelockPurchase ? "RES" : productCode;

                if (!ConfigUtility.IsViewResFlowCheckOut(checkOutRequest.Flow) && (isTPIcall || isSecondaryPaymentForTPI))
                {
                    //Fix for iOS when MPAccount has no saved cards added in TPI Purchase SecondaryPayment screen & primary purchase done through nonAIG supported fop -- MOBILE-8951 -- Shashank
                    if (_configuration.GetValue<bool>("NoTPISecPurchaseFOPToggle") && isSecondaryPaymentForTPI && checkOutRequest.PaymentAmount != "0" && checkOutRequest.FormofPaymentDetails.FormOfPaymentType != null
                        && (checkOutRequest.FormofPaymentDetails.CreditCard == null || formOfPayment.Payment.CreditCard == null ||
                        (formOfPayment.Payment.CreditCard != null && !string.IsNullOrEmpty(formOfPayment.Payment.CreditCard.CardType) && !IsValidFOPForTPIpayment(formOfPayment.Payment.CreditCard.CardType))))
                    {
                        throw new MOBUnitedException(_configuration.GetValue<string>("NoFormOfPaymentErrorMessage"));
                    }

                    // in post confirmation page, buy TPI only
                    formOfPayment.PaymentTarget = "TPI";
                    registerFormsOfPaymentRequest.PostPurchase = true;
                    // save account number token 
                    Reservation bookingPathReservation = new Reservation();
                    bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(checkOutRequest.SessionId, bookingPathReservation.ObjectName, new List<string> { checkOutRequest.SessionId, bookingPathReservation.ObjectName }).ConfigureAwait(false);
                    if (bookingPathReservation.TripInsuranceFile == null)
                    {
                        bookingPathReservation.TripInsuranceFile = new MOBTripInsuranceFile() { };

                    }
                    bookingPathReservation.SessionId = checkOutRequest.SessionId;
                    bookingPathReservation.TripInsuranceFile.AccountNumberToken = formOfPayment.Payment.CreditCard.AccountNumberToken;

                    await _sessionHelperService.SaveSession<Reservation>(bookingPathReservation, bookingPathReservation.SessionId, new List<string> { bookingPathReservation.SessionId, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName).ConfigureAwait(false);
                }
                #region OfferCode Expansion changes to handle ECD 0$ Changes 
                AddOffer(persistedShoppingCart, formOfPayment, productCode);
                #endregion
                //Setting the PostPurchase to False for the PreOrderMeals Purchase
                if (_configuration.GetValue<bool>("EnableInflightMealsRefreshment") && formOfPayment.PaymentTarget == "POM")
                    registerFormsOfPaymentRequest.PostPurchase = false;

                if (!ConfigUtility.IsViewResFlowCheckOut(checkOutRequest.Flow)
                    && (!ConfigUtility.IsETCCombinabilityEnabled(checkOutRequest.Application.Id, checkOutRequest.Application.Version.Major) ||
                    (ConfigUtility.IsETCCombinabilityEnabled(checkOutRequest.Application.Id, checkOutRequest.Application.Version.Major) && !formOfPayment.Payment.IsNullOrEmpty())))
                {
                    registerFormsOfPaymentRequest.FormsOfPayment.Add(formOfPayment);
                }

                if (intProductCodesCount == productCodes.Count() && session.IsReshopChange && (checkOutRequest.Flow != null && checkOutRequest.Flow == FlowType.RESHOP.ToString()))
                {
                    formOfPayment = await GerRefundFormOfPayment(session.SessionId, billingAddress);
                    if (formOfPayment != null)
                    {
                        registerFormsOfPaymentRequest.FormsOfPayment.Add(formOfPayment);
                    }
                }
                if (!persistedShoppingCart.FormofPaymentDetails.IsNullOrEmpty()
                    && persistedShoppingCart.FormofPaymentDetails.FormOfPaymentType == "MilesFOP")
                {
                    formOfPayment.PaymentTarget = productCode;
                    formOfPayment.Payment = new Service.Presentation.PaymentModel.FormOfPayment();
                    formOfPayment.Payment.Miles = new Service.Presentation.PaymentModel.Miles();
                    formOfPayment.Payment.Miles.Amount = persistedShoppingCart.FormofPaymentDetails.MilesFOP.RequiredMiles;
                    formOfPayment.Payment.Miles.LoyaltyProgramCarrierCode = "UA";
                    formOfPayment.Payment.Miles.LoyaltyProgramMemberID = persistedShoppingCart.FormofPaymentDetails.MilesFOP.ProfileOwnerMPAccountNumber;
                }


                if (ConfigUtility.IsViewResFlowCheckOut(checkOutRequest.Flow))
                {
                    registerFormsOfPaymentRequest.FormsOfPayment.Add(formOfPayment);
                    break;
                }

            }
            string paymentTarget = string.Empty;
            if (ConfigUtility.IsViewResFlowCheckOut(checkOutRequest.Flow) && registerFormsOfPaymentRequest?.FormsOfPayment != null && registerFormsOfPaymentRequest.FormsOfPayment.Any())
            {
                paymentTarget = registerFormsOfPaymentRequest?.FormsOfPayment.FirstOrDefault().PaymentTarget;
            }
            registerFormsOfPaymentRequest.WorkFlowType = ConfigUtility.GetWorkFlowType(checkOutRequest.Flow, paymentTarget);
            #region Adding certificate, if exist in FOP object
            if (!ConfigUtility.EnableMFOP(checkOutRequest.Application.Id, checkOutRequest.Application.Version.Major))
            {
                if ((ConfigUtility.IsETCCombinabilityEnabled(checkOutRequest.Application.Id, checkOutRequest.Application.Version.Major) && checkOutRequest.Flow == FlowType.BOOKING.ToString() && !checkOutRequest.IsSecondaryPayment)
                    || (ConfigUtility.IsManageResETCEnabled(checkOutRequest.Application.Id, checkOutRequest.Application.Version.Major) && (checkOutRequest.Flow == FlowType.VIEWRES.ToString() || checkOutRequest.Flow == FlowType.VIEWRES_SEATMAP.ToString())))
                {

                    AddCertificateFOP1(registerFormsOfPaymentRequest.FormsOfPayment, persistedShoppingCart, checkOutRequest.Application, checkOutRequest.Flow);
                }
                else if (ConfigUtility.IsETCchangesEnabled(checkOutRequest.Application.Id, checkOutRequest.Application.Version.Major) && checkOutRequest.Flow == FlowType.BOOKING.ToString() && !checkOutRequest.IsSecondaryPayment)
                {
                    AddCertificateFOP(registerFormsOfPaymentRequest.FormsOfPayment, persistedShoppingCart.FormofPaymentDetails, persistedShoppingCart.SCTravelers);
                }

                if (ConfigUtility.IncludeFFCResidual(checkOutRequest.Application.Id, checkOutRequest.Application.Version.Major) && checkOutRequest.Flow == FlowType.BOOKING.ToString() && !checkOutRequest.IsSecondaryPayment)
                {
                    FutureFlightCredit ffc = new FutureFlightCredit();
                    await AddFFCCertificateToFOP(registerFormsOfPaymentRequest.FormsOfPayment, persistedShoppingCart, session, checkOutRequest.Application, persistedReservation.Prices);
                }
            }
            else //MFOP
            { //Check if MFOP applied or not in the shoppingcart formsofpayment
                if (checkOutRequest.Flow == FlowType.BOOKING.ToString() &&
                   (checkOutRequest.FormofPaymentDetails.FormOfPaymentType.ToUpper() == MOBFormofPayment.CreditCard.ToString().ToUpper() || checkOutRequest.FormofPaymentDetails.FormOfPaymentType.ToUpper() == "TC") &&
                   ((checkOutRequest.FormofPaymentDetails?.TravelCertificate?.Certificates != null && checkOutRequest.FormofPaymentDetails?.TravelCertificate?.Certificates?.Count > 0) ||
                    (checkOutRequest.FormofPaymentDetails?.TravelFutureFlightCredit?.FutureFlightCredits != null && checkOutRequest.FormofPaymentDetails?.TravelFutureFlightCredit?.FutureFlightCredits?.Count > 0)
                    ))
                {
                    await UpdateCreditCardPayment(registerFormsOfPaymentRequest, checkOutRequest, session).ConfigureAwait(false);
                }
            }

            if (ConfigUtility.IncludeMoneyPlusMiles(checkOutRequest.Application.Id, checkOutRequest.Application.Version.Major) && checkOutRequest.Flow == FlowType.BOOKING.ToString() && !checkOutRequest.IsSecondaryPayment)
            {
                AddMoneyPlusMilesToFOP(registerFormsOfPaymentRequest.FormsOfPayment, persistedShoppingCart, persistedReservation.Prices, persistedReservation.ShopReservationInfo2.AllEligibleTravelersCSL.FirstOrDefault(t => t.IsProfileOwner));
            }

            AddTravelBankToFOP(registerFormsOfPaymentRequest.FormsOfPayment, persistedShoppingCart.FormofPaymentDetails, checkOutRequest.Application, checkOutRequest.Flow, checkOutRequest.IsSecondaryPayment);
            #endregion

            bool postBookingFixForMiles = !(_configuration.GetValue<bool>("NegationPostBookingFixForMiles")) ? (checkOutRequest.Flow != FlowType.POSTBOOKING.ToString()) : true;
            if ((persistedReservation != null && persistedReservation.AwardTravel) && !(isTPIcall || isSecondaryPaymentForTPI) && postBookingFixForMiles)
            {
                FormOfPayment mileagePayment = new FormOfPayment();
                mileagePayment.Payment = new Service.Presentation.PaymentModel.FormOfPayment();
                mileagePayment.Payment.Miles = new Service.Presentation.PaymentModel.Miles();
                mileagePayment.Payment.Miles.Amount = persistedReservation.Prices.Find(item => item.DisplayType.ToUpper() == "MILES").Value;
                mileagePayment.Payment.Miles.LoyaltyProgramCarrierCode = "UA";
                mileagePayment.Payment.Miles.LoyaltyProgramMemberID = checkOutRequest.MileagePlusNumber;
                registerFormsOfPaymentRequest.FormsOfPayment.Add(mileagePayment);
            }
            if (checkOutRequest.Flow == FlowType.BOOKING.ToString() && _configuration.GetValue<bool>("EnableOmniChannelCartMVP1"))
            {
                registerFormsOfPaymentRequest.DeviceID = checkOutRequest.DeviceId;
            }

            if (IsBuyMilesFeatureEnabled(checkOutRequest.Application.Id, checkOutRequest.Application.Version.Major)
                && persistedReservation != null && persistedReservation.AwardTravel)
            {
                var buyMilesValue = persistedReservation.Prices.Find(item => item.DisplayType.ToUpper() == "MPF")?.Value;
                if (buyMilesValue > 0)
                {
                    FormOfPayment mileagePayment = new FormOfPayment();
                    mileagePayment.Payment = new Service.Presentation.PaymentModel.FormOfPayment();
                    bool enableFixForMPFCheckoutIssue = await _featureSettings.GetFeatureSettingValue("EnableFixForMPFCheckoutIssue").ConfigureAwait(false);
                    if (checkOutRequest.FormofPaymentDetails.FormOfPaymentType.ToUpper() == MOBFormofPayment.ApplePay.ToString().ToUpper() && !_configuration.GetValue<bool>("DisableMPFCheckoutFix"))
                    {
                        mileagePayment.Payment.CreditCard = registerFormsOfPaymentRequest.FormsOfPayment.First(fop => fop.PaymentTarget == "RES").Payment.CreditCard.Clone();
                        mileagePayment.Payment.CreditCard.Amount = Convert.ToDouble(buyMilesValue);
                    }
                    else if (((checkOutRequest.FormofPaymentDetails.FormOfPaymentType.ToUpper() == MOBFormofPayment.PayPal.ToString().ToUpper()) || (checkOutRequest.FormofPaymentDetails.FormOfPaymentType.ToUpper() == MOBFormofPayment.PayPalCredit.ToString().ToUpper())) && !_configuration.GetValue<bool>("DisableMPFCheckoutFix"))
                    {
                        mileagePayment.Payment.PayPal = registerFormsOfPaymentRequest.FormsOfPayment.First(fop => fop.PaymentTarget == "RES").Payment.PayPal.Clone();
                        mileagePayment.Payment.PayPal.Amount = Convert.ToDouble(buyMilesValue);
                    }
                    else
                    {
                        if (enableFixForMPFCheckoutIssue)
                        {
                            checkOutRequest.PaymentAmount = buyMilesValue.ToString();
                        }
                        mileagePayment.Payment.CreditCard = await MapToCslCreditCard(checkOutRequest, persistedShoppingCart.CurrencyCode, session, persistedShoppingCart.PaymentTarget);
                    }
                    mileagePayment.Amount = (persistedReservation.Prices.Find(item => item.DisplayType.ToUpper() == "MPF")?.Value > 0)
                                               ? Convert.ToDecimal(persistedReservation.Prices.Find(item => item.DisplayType.ToUpper() == "MPF")?.Value) : 0;
                    mileagePayment.PaymentTarget = "MPF";
                    registerFormsOfPaymentRequest.FormsOfPayment.Add(mileagePayment);
                }
            }

            //Adding each eligible PAX to FormsOfPayment when FFCR is available
            if (checkOutRequest.Flow == Convert.ToString(FlowType.RESHOP))
            {
                var paxfopLst = ReshopAddPaxToIssueFFCR(checkOutRequest, persistedShoppingCart, persistedReservation);
                if (paxfopLst != null && paxfopLst.Any())
                {
                    registerFormsOfPaymentRequest.FormsOfPayment
                        = (registerFormsOfPaymentRequest.FormsOfPayment == null)
                        ? new List<FormOfPayment>() : registerFormsOfPaymentRequest.FormsOfPayment;
                    registerFormsOfPaymentRequest.FormsOfPayment.AddRange(paxfopLst);
                }
            }

            if (!ConfigUtility.IsViewResFlowCheckOut(checkOutRequest.Flow) && ConfigUtility.EnableInflightContactlessPayment(checkOutRequest.Application.Id, checkOutRequest.Application.Version.Major, checkOutRequest?.Flow?.ToUpper() != FlowType.BOOKING.ToString()))
            {
                if (persistedShoppingCart?.InFlightContactlessPaymentEligibility?.IsCCSelectedForContactless ?? false)
                {
                    if (checkOutRequest?.FormofPaymentDetails?.FormOfPaymentType?.ToUpper()?.Equals(MOBFormofPayment.CreditCard.ToString().ToUpper()) ?? false)
                    {
                        string cardDisplayNumber = checkOutRequest?.FormofPaymentDetails?.CreditCard?.DisplayCardNumber;

                        if (!string.IsNullOrEmpty(cardDisplayNumber) && cardDisplayNumber.Length > 3)
                        {
                            if (registerFormsOfPaymentRequest?.FormsOfPayment?.Any(f => f != null && !string.IsNullOrEmpty(f.PaymentTarget) && f.PaymentTarget.ToUpper().Equals("RES") && f.Payment?.CreditCard != null) ?? false)
                            {

                                registerFormsOfPaymentRequest.FormsOfPayment.First(f => f.PaymentTarget.ToUpper().Equals("RES") && f.Payment?.CreditCard != null).Payment.CreditCard.AccountNumberLastFourDigits = cardDisplayNumber.Substring(cardDisplayNumber.Length - 4);
                            }

                            registerFormsOfPaymentRequest.IsSaveForContactless = true;
                        }
                    }
                }
            }

            if (checkOutRequest.Flow == FlowType.MOBILECHECKOUT.ToString())
            {
                registerFormsOfPaymentRequest.WorkFlowType = (WorkFlowType)persistedShoppingCart.CslWorkFlowType;
            }

            #region Tax Id Information

            if (checkOutRequest.Flow == FlowType.BOOKING.ToString()
                 && checkOutRequest.TaxIdInformation != null
                 && persistedShoppingCart != null
                 && persistedShoppingCart.SCTravelers != null
                 && persistedShoppingCart.SCTravelers.Count > 0)
            {
                registerFormsOfPaymentRequest.SpecialServiceRequest = InsertTaxIdSSRValues(checkOutRequest, persistedShoppingCart);
            }
            #endregion

            return registerFormsOfPaymentRequest;

        }

        #region TaxId Information
        // Insert taxId SSR Values if applicable
        private List<Service.Presentation.CommonModel.Service> InsertTaxIdSSRValues(MOBCheckOutRequest checkOutRequest, MOBShoppingCart persistedShoppingCart)
        {
            List<Service.Presentation.CommonModel.Service> SpecialServiceRequests = null;
            string providedTaxIdType = null;
            string providedTaxIdValue = null;
            string providedIdHolderName = null; string providedEmail = null;
            try
            {
                if (checkOutRequest.TaxIdInformation.SelectedValues != null && checkOutRequest.TaxIdInformation?.SelectedValues.Count > 0)
                {
                    SpecialServiceRequests = new List<Service.Presentation.CommonModel.Service>();
                    if (checkOutRequest.TaxIdInformation.IsTravelerOrPurchaserCountry != null 
                        && (checkOutRequest.TaxIdInformation.IsTravelerOrPurchaserCountry.Equals(TaxIdCountryType.PURCHASER.ToString())
                            || (checkOutRequest.TaxIdInformation.IsTravelerOrPurchaserCountry.Equals(TaxIdCountryType.TRAVELER.ToString()) 
                                && checkOutRequest.TaxIdInformation.SelectedValues.Count == 1)))
                    {
                        foreach(var traveler in persistedShoppingCart.SCTravelers) 
                        {
                            // Reason why we skip infant validation
                            // For purchaser countries does not take into account Infants because no point to have duplicate data, since infant on lap does not have an index
                            // For traveler countries,infants can not travel alone
                            if (traveler.TravelerTypeCode != null && !traveler.TravelerTypeCode.ToUpper().Equals("INF")) 
                            {
                                SpecialServiceRequests.AddRange(ListSSRValuesPerTraveler(checkOutRequest.TaxIdInformation.SelectedValues[0], traveler.TravelerNameIndex));
                            }
                        }
                    }
                    else if (checkOutRequest.TaxIdInformation.IsTravelerOrPurchaserCountry != null && checkOutRequest.TaxIdInformation.IsTravelerOrPurchaserCountry.Equals(TaxIdCountryType.TRAVELER.ToString())) 
                    {
                        foreach (var traveler in persistedShoppingCart.SCTravelers) 
                        {
                            List<MOBItem> infoSelectedValuesPerTraveler = checkOutRequest.TaxIdInformation.SelectedValues.Where(x => x.Any(y => y.Id.Equals("Traveler") && y.CurrentValue.Equals(traveler.TravelerNameIndex))).FirstOrDefault();

                            if (infoSelectedValuesPerTraveler != null)
                            {
                                if (traveler.TravelerTypeCode != null && traveler.TravelerTypeCode.ToUpper().Equals("INF"))
                                {
                                    // If information traveler is comming and traveler is an infant we assume IncludeInfantOnLap property is true.                                     
                                    var travelerNameIndexAdult = persistedShoppingCart.SCTravelers.Where(x => x.TravelerTypeCode != null && x.TravelerTypeCode.Equals("ADT")).FirstOrDefault();
                                    if (travelerNameIndexAdult != null) 
                                    {
                                        // For Infant on Lap information use the adult traveler name index
                                        SpecialServiceRequests.AddRange(ListSSRValuesPerTraveler(infoSelectedValuesPerTraveler, travelerNameIndexAdult.TravelerNameIndex));
                                    }
                                }
                                else 
                                {
                                    // For regular travelers add the traveler and index informaton 
                                    SpecialServiceRequests.AddRange(ListSSRValuesPerTraveler(infoSelectedValuesPerTraveler, traveler.TravelerNameIndex));
                                }
                            }
                            else 
                            {
                                if (traveler.IsExtraSeat && traveler.ExtraSeatData != null) 
                                {
                                    var travelerExtraSeatFor = persistedShoppingCart.SCTravelers.Where(x => x.PaxID == traveler.ExtraSeatData.SelectedPaxId).FirstOrDefault();
                                    if (travelerExtraSeatFor != null) 
                                    {
                                        var infoSelectedValuesTravelerExtraSeatFor = checkOutRequest.TaxIdInformation.SelectedValues.Where(x => x.Any(y => y.Id.Equals("Traveler") && y.CurrentValue.Equals(travelerExtraSeatFor.TravelerNameIndex))).FirstOrDefault();
                                        if (infoSelectedValuesTravelerExtraSeatFor != null) 
                                        {
                                            // For extraseats add the traveler extra seat for information and use extraseat index. 
                                            SpecialServiceRequests.AddRange(ListSSRValuesPerTraveler(infoSelectedValuesTravelerExtraSeatFor, traveler.TravelerNameIndex));
                                        }
                                    }
                                }
                                // We ignore the INF since no point to have duplicates. 
                            }
                        }
                    }
                }
                else
                {
                    if (checkOutRequest.TaxIdInformation?.Components != null && checkOutRequest.TaxIdInformation?.Components.Count > 0)
                    {
                        providedTaxIdType = checkOutRequest.TaxIdInformation?.Components?.FirstOrDefault(c => c.Title.ToUpper().Trim().Equals("ID TYPE"))?.SelectedValue;
                        providedTaxIdValue = checkOutRequest.TaxIdInformation?.Components?.FirstOrDefault(c => c.Title.ToUpper().Trim().Contains("ID NUMBER"))?.SelectedValue;
                        providedIdHolderName = checkOutRequest.TaxIdInformation?.Components?.FirstOrDefault(c => c.Title.ToUpper().Trim().Contains("ID HOLDER"))?.SelectedValue;
                        providedEmail = checkOutRequest.TaxIdInformation?.Components?.FirstOrDefault(c => c.Title.ToUpper().Trim().Contains("EMAIL"))?.SelectedValue;
                    }
                    if ((!string.IsNullOrEmpty(checkOutRequest.TaxIdInformation.SelectedTaxIdType) && !string.IsNullOrEmpty(checkOutRequest.TaxIdInformation.SelectedTaxIdValue))
                        || (!string.IsNullOrEmpty(providedTaxIdType) && !string.IsNullOrEmpty(providedTaxIdValue)))
                    {
                        SpecialServiceRequests = new List<Service.Presentation.CommonModel.Service>();
                        foreach (var traveler in persistedShoppingCart.SCTravelers)
                        {
                            if (traveler.TravelerTypeCode != null && !traveler.TravelerTypeCode.ToUpper().Equals("INF"))
                            {
                                string DescriptionforId = null;
                                if (!string.IsNullOrEmpty(checkOutRequest.TaxIdInformation.SelectedTaxIdType) && !string.IsNullOrEmpty(checkOutRequest.TaxIdInformation.SelectedTaxIdValue))
                                {
                                    DescriptionforId = string.Format("{0}{1}", checkOutRequest?.TaxIdInformation?.SelectedTaxIdType, checkOutRequest?.TaxIdInformation?.SelectedTaxIdValue);
                                }
                                else
                                {
                                    DescriptionforId = string.Format("{0}{1}", providedTaxIdType, providedTaxIdValue);
                                }
                                var specialServiceRequestforId = GetTaxIdSSRObject(_configuration.GetValue<string>("TaxIdSSRCode"), DescriptionforId, traveler.TravelerNameIndex);
                                if (specialServiceRequestforId != null)
                                    SpecialServiceRequests.Add(specialServiceRequestforId);

                                if (!string.IsNullOrEmpty(providedIdHolderName))
                                {
                                    var specialServiceRequestForName = GetTaxIdSSRObject(_configuration.GetValue<string>("TaxIdSSRCode"), providedIdHolderName, traveler.TravelerNameIndex);
                                    if (specialServiceRequestForName != null)
                                        SpecialServiceRequests.Add(specialServiceRequestForName);
                                }

                                if (!string.IsNullOrEmpty(providedEmail))
                                {
                                    var specialServiceRequestForEmail = GetTaxIdSSRObject(_configuration.GetValue<string>("TaxIdSSRCodeForEmail"), providedEmail, traveler.TravelerNameIndex);
                                    if (specialServiceRequestForEmail != null)
                                        SpecialServiceRequests.Add(specialServiceRequestForEmail);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.ILoggerError("TaxId InsertTaxIdSSRValues Error {message} {StackTrace} and {session}", ex.Message, ex.StackTrace, checkOutRequest.SessionId);
                SpecialServiceRequests = null;
            }
            return SpecialServiceRequests;
        }

        private List<Service.Presentation.CommonModel.Service> ListSSRValuesPerTraveler(List<MOBItem> travelerSelectedValues, string travelerNameIndex) 
        {
            string SSRCode = _configuration.GetValue<string>("TaxIdSSRCode");
            List<Service.Presentation.CommonModel.Service> specialServiceRequests = new List<Service.Presentation.CommonModel.Service>();

            var selectedIdType = travelerSelectedValues.Where(x => x.Id.ToUpper().Trim().Equals("ID type".ToUpper().Trim())).FirstOrDefault()?.CurrentValue;
            var selectedIdNumber = travelerSelectedValues.Where(x => x.Id.ToUpper().Trim().Contains("ID Number".ToUpper().Trim())).FirstOrDefault()?.CurrentValue;
            var selectedHolderName = travelerSelectedValues.Where(x => x.Id.ToUpper().Trim().Equals("Tax ID holder".ToUpper().Trim())).FirstOrDefault()?.CurrentValue;
            var selectedEmail = travelerSelectedValues.Where(x => x.Id.ToUpper().Trim().Equals("Email address".ToUpper().Trim())).FirstOrDefault()?.CurrentValue;

            // Description
            if (!string.IsNullOrEmpty(selectedIdType) && !string.IsNullOrEmpty(selectedIdNumber))
            {
                var descriptionForId = string.Format("{0}{1}", selectedIdType, selectedIdNumber);
                var specialServiceRequestForId = GetTaxIdSSRObject(SSRCode, descriptionForId, travelerNameIndex);
                if (specialServiceRequestForId != null)
                {
                    specialServiceRequests.Add(specialServiceRequestForId);
                }
            }
            // Holder name
            if (!string.IsNullOrEmpty(selectedHolderName))
            {
                var specialServiceRequestForHolderName = GetTaxIdSSRObject(SSRCode, selectedHolderName, travelerNameIndex);
                if (specialServiceRequestForHolderName != null)
                {
                    specialServiceRequests.Add(specialServiceRequestForHolderName);
                }
            }
            // Email
            if (!string.IsNullOrEmpty(selectedEmail))
            {
                var specialServiceRequestForEmail = GetTaxIdSSRObject(SSRCode, selectedEmail, travelerNameIndex);
                if (specialServiceRequestForEmail != null)
                {
                    specialServiceRequests.Add(specialServiceRequestForEmail);
                }
            }

            return specialServiceRequests;
        }
        #endregion

        private Service.Presentation.CommonModel.Service GetTaxIdSSRObject(string code, string description, string travelerNameIndex)
        {
            if (!string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(description) && !string.IsNullOrEmpty(travelerNameIndex))
            {
                Service.Presentation.CommonModel.Service specialServiceRequest = new Service.Presentation.CommonModel.Service()
                {
                    CarrierCode = new Service.Presentation.CommonModel.VendorModel.Vendor() { Name = _configuration.GetValue<string>("TaxIdSSRCarrierCode") },
                    Code = code,
                    Key = _configuration.GetValue<string>("TaxIdSSRKey"),
                    NumberInParty = 1,
                    Description = description,
                    SegmentNumber = new Collection<int>(),
                    Status = new Status() { Description = _configuration.GetValue<string>("TaxIdSSRStatus") },
                    TravelerNameIndex = travelerNameIndex
                };
                return specialServiceRequest;
            }
            return null;
        }

        #region MFOP CreditCard Add payment         
        private async Task<RegisterFormsOfPaymentRequest> UpdateCreditCardPayment(RegisterFormsOfPaymentRequest registerFormsOfPaymentRequest, MOBCheckOutRequest checkOutRequest, Session session)
        {
            try
            {
                var payments = await GetPayments(session, $"/api/payments?cartid={checkOutRequest.CartId}").ConfigureAwait(false);

                var remainingDue = payments?.Products?.Sum(d => d.RemainingDue);

                if (remainingDue > 0 && checkOutRequest.FormofPaymentDetails.FormOfPaymentType.ToUpper() == MOBFormofPayment.CreditCard.ToString().ToUpper())
                    await AddCreditCardPayment(registerFormsOfPaymentRequest, checkOutRequest, session, "/api/payments", 0).ConfigureAwait(false);

                if (payments?.PaymentsApplied?.Count > 0)
                {
                    registerFormsOfPaymentRequest.BillingAddress = new Address()
                    {
                        AddressLines = new Collection<string> { checkOutRequest.FormofPaymentDetails.BillingAddress?.Line1.ToString() },
                        City = checkOutRequest.FormofPaymentDetails.BillingAddress?.City,
                        Country = new Country() { CountryCode = checkOutRequest.FormofPaymentDetails.BillingAddress?.Country?.Code },
                        PostalCode = checkOutRequest.FormofPaymentDetails.BillingAddress?.PostalCode,
                        StateProvince = new StateProvince() { ShortName = checkOutRequest.FormofPaymentDetails.BillingAddress?.State?.Code, StateProvinceCode = checkOutRequest.FormofPaymentDetails.BillingAddress?.State?.Code }
                    };
                    registerFormsOfPaymentRequest.PaymentApplied = true;
                    registerFormsOfPaymentRequest.FormsOfPayment = new List<FormOfPayment>();
                }
            }
            catch (Exception exe)
            {
                _logger.LogError("MultipeFormsOfPayment Error {message} {StackTrace} and {session}", exe.Message, exe.StackTrace, session.SessionId);
                throw exe;
            }
            return registerFormsOfPaymentRequest;
        }
        private async Task<ManagePaymentRequest> GetAddPaymentRequest(RegisterFormsOfPaymentRequest registerFormsOfPaymentRequest, MOBCheckOutRequest checkOutRequest, Session session, double amount)
        {
            ManagePaymentRequest _managerequest = new ManagePaymentRequest();
            //Add CC details from registerFormsOfPaymentRequest FormofPayments
            var cc = new United.Service.Presentation.PaymentModel.CreditCard();
            cc = await MapToCslCreditCard(checkOutRequest, checkOutRequest?.PointOfSale == "US" ? "USD" : "", session);
            var creditCardDetails = checkOutRequest?.FormofPaymentDetails?.CreditCard;
            if (creditCardDetails == null)
            {
                return new ManagePaymentRequest();
            }
            _managerequest.FormOfPayment = new United.Service.Presentation.PaymentModel.FormOfPayment();
            _managerequest.FormOfPayment.CreditCard = cc;
            _managerequest.CartId = registerFormsOfPaymentRequest.CartId;
            _managerequest.WorkFlowType = registerFormsOfPaymentRequest.WorkFlowType;
            return _managerequest;
        }
        private async Task<RegisterFormsOfPaymentRequest> AddCreditCardPayment(RegisterFormsOfPaymentRequest registerFormsOfPaymentRequest, MOBCheckOutRequest checkOutRequest, Session session, string path, double amount)
        {
            var paymentRequest = await GetAddPaymentRequest(registerFormsOfPaymentRequest, checkOutRequest, session, amount).ConfigureAwait(false);
            _logger.LogInformation("MultipeFormsOfPayment {path}  {request} with {sessionId}", path, JsonConvert.SerializeObject(paymentRequest), session.SessionId);

            ManagePaymentResponse response = new ManagePaymentResponse();
            var jsonResponse = await _shoppingCartService.ShoppingCartServiceCall(session.Token, path, JsonConvert.SerializeObject(paymentRequest), session.SessionId);
            if (!string.IsNullOrEmpty(jsonResponse))
            {
                response = JsonConvert.DeserializeObject<ManagePaymentResponse>(jsonResponse);
            }
            _logger.LogInformation("MultipeFormsOfPayment {path}  {response} with {sessionId}", path, JsonConvert.SerializeObject(response), session.SessionId);

            return registerFormsOfPaymentRequest;
        }

        private async Task<ManagePaymentResponse> GetPayments(Session session, string path)
        {
            _logger.LogInformation("MultipeFormsOfPayment GetPayments {path} with {sessionId}", path, session.SessionId);

            ManagePaymentResponse response = new ManagePaymentResponse();
            var jsonResponse = await _shoppingCartService.GetAsync<ManagePaymentResponse>(path, session.Token, session.SessionId);
            _logger.LogInformation("MultipeFormsOfPayment {path}  {response} with {sessionId}", path, JsonConvert.SerializeObject(jsonResponse), session.SessionId);

            return jsonResponse;
        }

        #endregion
        private bool IsBuyMilesFeatureEnabled(int appId, string version)
        {
            if (!_configuration.GetValue<bool>("EnableBuyMilesFeature")) return false;
            return GeneralHelper.IsApplicationVersionGreaterorEqual(appId, version, _configuration.GetValue<string>("Android_BuyMilesFeatureSupported_AppVersion"), _configuration.GetValue<string>("IPhone_BuyMilesFeatureSupported_AppVersion"));
        }

        private void AddOffer(MOBShoppingCart shoppingCart, FormOfPayment formOfPayment, string productCode)
        {
            //Add this only when ECD 0$ offer is applied as entire amount is covered with Offer we need pass the Certificate object with ECD type.
            if (shoppingCart?.Offers != null
                && !string.IsNullOrEmpty(shoppingCart.Offers.OfferCode)
                && productCode == "RES"
                )
            {
                var resProduct = shoppingCart.Products.FirstOrDefault(prd => prd.Code == "RES");
                if (resProduct != null && !string.IsNullOrEmpty(resProduct.ProdTotalPrice) && Convert.ToDecimal(resProduct.ProdTotalPrice) == 0)
                {
                    formOfPayment.Payment = new Service.Presentation.PaymentModel.FormOfPayment
                    {
                        Certificate = new Certificate
                        {
                            Type = new Genre
                            {
                                Key = "ECD"
                            },
                            BillingAddress = GetCSLBillingAddress(shoppingCart.FormofPaymentDetails?.BillingAddress)

                        }
                    };
                }
            }
        }


        private PayPal PayPalPayLoad(MOBPayPalPayor payPalPayor, double paymentAmount, MOBPayPal mobPayPal)
        {
            PayPal paypal = new PayPal();
            paypal.Amount = paymentAmount;

            paypal.BillingAddress = AssignCreditCardBillingAddress(payPalPayor.PayPalBillingAddress);
            paypal.BillingAddress.Characteristic = new Collection<Service.Presentation.CommonModel.Characteristic>();
            paypal.BillingAddress.Status = new Service.Presentation.CommonModel.Status();
            paypal.BillingAddress.Status.Description = "NONE"; //**TBD-paypal**//
            paypal.Currency = new Service.Presentation.CommonModel.Currency();
            paypal.Currency.Code = mobPayPal.CurrencyCode;
            paypal.PayerID = mobPayPal.PayerID;
            paypal.TokenID = mobPayPal.PayPalTokenID;
            paypal.Payor = new Service.Presentation.PersonModel.Person();
            paypal.Payor.Contact = new Service.Presentation.PersonModel.Contact();
            paypal.Payor.Contact.Emails = new Collection<Service.Presentation.CommonModel.EmailAddress>();
            Service.Presentation.CommonModel.EmailAddress email = new Service.Presentation.CommonModel.EmailAddress();
            email.Address = payPalPayor.PayPalContactEmailAddress;
            paypal.Payor.Contact.Emails.Add(email);
            paypal.Payor.CustomerID = payPalPayor.PayPallCustomerID;
            paypal.Payor.GivenName = payPalPayor.PayPalGivenName;
            paypal.Payor.Surname = payPalPayor.PayPalSurName;
            paypal.Payor.Status = new Service.Presentation.CommonModel.Status();
            paypal.Payor.Status.Description = payPalPayor.PayPalStatus;
            paypal.Type = new Service.Presentation.CommonModel.Genre();
            paypal.Type.Key = "PP"; // check if we need to move this to web config.
            if (!_configuration.GetValue<bool>("EnableWalletCategory"))
                paypal.WalletCategory = Service.Presentation.CommonEnumModel.WalletCategory.PayPal;

            return paypal;
        }

        private async Task ReshopPaymentAmount(MOBCheckOutRequest makeReservationRequest)
        {
            var reservationPaymentAmount = await _sessionHelperService.GetSession<Reservation>(makeReservationRequest.SessionId, new Reservation().ObjectName, new List<string> { makeReservationRequest.SessionId, new Reservation().ObjectName }).ConfigureAwait(false);

            if (reservationPaymentAmount.IsReshopChange)
            {
                double changeFee = 0.0;
                double grandTotal = 0.0;
                double total = 0.0;

                if (reservationPaymentAmount.Prices.Exists
                    (price => string.Equals(price.DisplayType, "CHANGEFEE", StringComparison.OrdinalIgnoreCase)))
                {
                    changeFee = reservationPaymentAmount.Prices.FirstOrDefault
                        (price => string.Equals(price.DisplayType, "CHANGEFEE", StringComparison.OrdinalIgnoreCase)).Value;
                }

                if (reservationPaymentAmount.Prices.Exists
                    (price => string.Equals(price.DisplayType, "GRAND TOTAL", StringComparison.OrdinalIgnoreCase)))
                {
                    grandTotal = reservationPaymentAmount.Prices.FirstOrDefault
                        (price => string.Equals(price.DisplayType, "GRAND TOTAL", StringComparison.OrdinalIgnoreCase)).Value;
                    bool enableDoubleAuthFixForReshopAwardOTP = await _featureSettings.GetFeatureSettingValue("EnableDoubleAuthFixForReshopAwardOTP").ConfigureAwait(false);
                    if (enableDoubleAuthFixForReshopAwardOTP && reservationPaymentAmount.AwardTravel && reservationPaymentAmount.Prices.Exists
                             (price => string.Equals(price.DisplayType, "TAXDIFFERENCE", StringComparison.OrdinalIgnoreCase)))
                    {
                        double awardtotal = reservationPaymentAmount.Prices.FirstOrDefault
                                                (price => string.Equals(price.DisplayType, "TAXDIFFERENCE", StringComparison.OrdinalIgnoreCase)).Value;
                        grandTotal = awardtotal;
                    }
                    if (!reservationPaymentAmount.AwardTravel)
                    {
                        if (reservationPaymentAmount.Prices.Exists
                            (price => string.Equals(price.DisplayType, "TOTAL", StringComparison.OrdinalIgnoreCase)))
                        {
                            total = reservationPaymentAmount.Prices.FirstOrDefault
                                (price => string.Equals(price.DisplayType, "TOTAL", StringComparison.OrdinalIgnoreCase)).Value;
                        }

                        if (_configuration.GetValue<bool>("DisableFix_MOBILE16608"))
                        {
                            if (grandTotal == 0.0)
                            {
                                grandTotal = total;
                            }
                        }
                        else { grandTotal = total; }
                    }
                }
                else
                {
                    if (reservationPaymentAmount.Prices.Exists
                        (price => string.Equals(price.DisplayType, "TOTAL", StringComparison.OrdinalIgnoreCase)))
                    {
                        grandTotal = reservationPaymentAmount.Prices.FirstOrDefault
                            (price => string.Equals(price.DisplayType, "TOTAL", StringComparison.OrdinalIgnoreCase)).Value;
                    }
                }

                makeReservationRequest.PaymentAmount = (grandTotal > changeFee ? (grandTotal - changeFee) : 0).ToString();
            }
        }

        private void GetCheckoutRequestPhones(MOBCheckOutRequest checkOutRequest, RegisterFormsOfPaymentRequest registerFormsOfPaymentRequest)
        {
            if (!_configuration.GetValue<bool>("Disable_Setting_PhonesInRegisterFormOfPaymentDetails_Request") &&
                checkOutRequest.Flow == FlowType.BOOKING.ToString() &&
                checkOutRequest.FormofPaymentDetails != null &&
                checkOutRequest.FormofPaymentDetails.Phone != null)
            {
                if (registerFormsOfPaymentRequest.Phones == null)
                    registerFormsOfPaymentRequest.Phones = new Collection<United.Service.Presentation.CommonModel.Telephone>();

                United.Service.Presentation.CommonModel.Telephone tlp = new United.Service.Presentation.CommonModel.Telephone
                {
                    AreaCityCode = checkOutRequest.FormofPaymentDetails.Phone.AreaNumber,
                    CountryAccessCode = checkOutRequest.FormofPaymentDetails.Phone.CountryCode,
                    Description = "H",
                    PhoneLocation = new Genre { Description = "MOB" },
                    PhoneNumber = checkOutRequest.FormofPaymentDetails.Phone.PhoneNumber
                };
                registerFormsOfPaymentRequest.Phones.Add(tlp);
            }
        }

        private async Task<FormOfPayment> GerRefundFormOfPayment(string sessionId, Address billingAddress)
        {
            FormOfPayment formOfPayment = null;
            Reservation reservationForMasterpass = new Reservation();
            reservationForMasterpass = await _sessionHelperService.GetSession<Reservation>(sessionId, reservationForMasterpass.ObjectName, new List<string> { sessionId, reservationForMasterpass.ObjectName }).ConfigureAwait(false);
            var price = reservationForMasterpass.Prices.FirstOrDefault(p => p.Status == "PP" || p.Status == "AP" || p.Status == "ETC");
            if (price != null)
            {
                formOfPayment = new FormOfPayment();
                formOfPayment.Payment = new Service.Presentation.PaymentModel.FormOfPayment();
                formOfPayment.PaymentTarget = "RES";
                if (price.Status == "ETC")
                {
                    formOfPayment.Payment.Certificate = new Service.Presentation.PaymentModel.Certificate();
                    if (reservationForMasterpass.Reshop.RefundAddress != null)
                    {
                        formOfPayment.Payment.Certificate.BillingAddress = AssignCreditCardBillingAddress(reservationForMasterpass.Reshop.RefundAddress);
                    }
                    else if (billingAddress != null)
                    {
                        formOfPayment.Payment.Certificate.BillingAddress = billingAddress;
                    }
                }
                else
                {
                    formOfPayment.Payment.PayPal = new Service.Presentation.PaymentModel.PayPal();
                    if (reservationForMasterpass.Reshop.RefundAddress != null)
                    {
                        formOfPayment.Payment.PayPal.BillingAddress = AssignCreditCardBillingAddress(reservationForMasterpass.Reshop.RefundAddress);
                    }
                    else if (billingAddress != null)
                    {
                        formOfPayment.Payment.PayPal.BillingAddress = billingAddress;
                    }

                }

            }

            return formOfPayment;
        }

        private void AddCertificateFOP1(List<FormOfPayment> formOfPayment, MOBShoppingCart shoppingCart, MOBApplication application, string flow)
        {
            if (shoppingCart.FormofPaymentDetails?.TravelCertificate?.Certificates?.Count > 0)
            {
                bool istc = !ConfigUtility.IsViewResFlowCheckOut(flow) ? IncludeTravelCredit(application.Id, application.Version.Major) : false;
                if (istc)
                {
                    foreach (var cert in shoppingCart.FormofPaymentDetails.TravelCertificate.Certificates)
                    {
                        var travelCredit = shoppingCart.FormofPaymentDetails?.TravelCreditDetails?.TravelCredits?.FirstOrDefault(c => c.PinCode == cert.PinCode);
                        if (travelCredit != null)
                        {
                            cert.PromoCode = travelCredit.PromoCode;
                        }
                    }
                }

                MOBFOPCertificate[] usedCertificates = new MOBFOPCertificate[shoppingCart.FormofPaymentDetails.TravelCertificate.Certificates.Count];
                shoppingCart.FormofPaymentDetails.TravelCertificate.Certificates.CopyTo(usedCertificates);
                var usedCertificatesList = usedCertificates.ToList();
                AssignRESAmountToCSLFop(formOfPayment, shoppingCart, usedCertificatesList, istc);

                string[] CombinebilityETCAppliedAncillaryCodes;

                bool? isBundleProduct = false;
                if (_configuration.GetValue<bool>("EnableCertificateFOPforBundlesFlowInViewRes"))
                    isBundleProduct = shoppingCart?.Captions?.Exists(b => b != null && b.Id == "IsBundleProduct");

                if (_configuration.GetValue<bool>("EnableEtcforSeats_PCU_Viewres") && (shoppingCart.Flow == FlowType.VIEWRES.ToString() || shoppingCart.Flow == FlowType.VIEWRES_SEATMAP.ToString()))
                {
                    string configViewResETCEligibleProducts = _configuration.GetValue<string>("VIewResETCEligibleProducts");

                    if (isBundleProduct.Value)
                    {
                        var bundleProducts = string.Join("|", shoppingCart?.Products?.Select(s => s?.Code));
                        bundleProducts = bundleProducts.Trim('|');
                        configViewResETCEligibleProducts += "|" + bundleProducts;
                    }

                    CombinebilityETCAppliedAncillaryCodes = configViewResETCEligibleProducts.Split('|');
                }
                else
                {
                    string configCombinebilityETCAppliedAncillaryCodes = _configuration.GetValue<string>("CombinebilityETCAppliedAncillaryCodes");
                    if (_configuration.GetValue<bool>("ETCForAllProductsToggle"))
                    {
                        string nonBundleProductCode = _configuration.GetValue<string>("NonBundleProductCode");

                        List<MOBProdDetail> bundleProducts = shoppingCart.Products.FindAll(p => (nonBundleProductCode.IndexOf(p.Code) == -1) && !string.IsNullOrEmpty(p.ProdTotalPrice));
                        if (bundleProducts != null && bundleProducts.Count > 0)
                        {
                            string bundleProductCodes = string.Join("|", bundleProducts.Select(p => p.Code));
                            bundleProductCodes = bundleProductCodes.Trim('|');
                            configCombinebilityETCAppliedAncillaryCodes += "|" + bundleProductCodes;
                        }
                    }
                    CombinebilityETCAppliedAncillaryCodes = configCombinebilityETCAppliedAncillaryCodes.Split('|');
                }

                double? allowedETCAncillaryAmount = GetAlowedETCAncillaryAmount(shoppingCart.Products, shoppingCart.Flow);
                if (allowedETCAncillaryAmount > 0 && usedCertificatesList.Count > 0)
                {
                    AssignAncillaryAmountToCSLFOP(formOfPayment, shoppingCart, allowedETCAncillaryAmount, usedCertificatesList, CombinebilityETCAppliedAncillaryCodes, flow, isBundleProduct.Value);
                }
                if (istc)
                {
                    RemoveEmptyResAndSeatObject(formOfPayment);
                }
            }
        }

        private double? GetAlowedETCAncillaryAmount(List<MOBProdDetail> products, string flow)
        {
            string allowedETCAncillaryProducts = string.Empty;
            if (_configuration.GetValue<bool>("EnableEtcforSeats_PCU_Viewres") && (flow == FlowType.VIEWRES.ToString() || flow == FlowType.VIEWRES_SEATMAP.ToString()))
            {
                allowedETCAncillaryProducts = _configuration.GetValue<string>("VIewResETCEligibleProducts");
            }
            else
            {
                allowedETCAncillaryProducts = _configuration.GetValue<string>("CombinebilityETCAppliedAncillaryCodes");
            }
            double? totalAncillaryAmount = products == null ? 0 : products.Where(p => (allowedETCAncillaryProducts.IndexOf(p.Code) > -1) && !string.IsNullOrEmpty(p.ProdTotalPrice))?.Sum(a => Convert.ToDouble(a.ProdTotalPrice));
            if (_configuration.GetValue<bool>("ETCForAllProductsToggle"))
            {
                totalAncillaryAmount += GetBundlesAmount(products, flow);
            }

            return totalAncillaryAmount;
        }

        private double GetBundlesAmount(List<MOBProdDetail> products, string flow)
        {
            string nonBundleProductCode = _configuration.GetValue<string>("NonBundleProductCode");
            double bundleAmount = products == null ? 0 : products.Where(p => (nonBundleProductCode.IndexOf(p.Code) == -1) && !string.IsNullOrEmpty(p.ProdTotalPrice)).Sum(a => Convert.ToDouble(a.ProdTotalPrice));

            return bundleAmount;
        }

        private double GetBundlesAmount(List<MOBProdDetail> products)
        {
            string nonBundleProductCode = _configuration.GetValue<string>("NonBundleProductCode");
            double bundleAmount = products == null ? 0 : products.Where(p => (nonBundleProductCode.IndexOf(p.Code) == -1) && !string.IsNullOrEmpty(p.ProdTotalPrice)).Sum(a => Convert.ToDouble(a.ProdTotalPrice));

            return bundleAmount;
        }
        private bool IsPCUProduct(string code)
        {
            switch (code.ToUpper().Trim())
            {
                case "UNITED FIRST":
                case "UNITED BUSINESS":
                case "UNITED POLARIS FIRST":
                case "UNITED POLARIS BUSINESS":
                case "UNITED PREMIUM PLUS":
                    return true;

                default: return false;
            }
        }

        private double getPCUProductPrice(MOBShoppingCart shoppingcart)
        {
            double pcuPrice = 0;
            if (shoppingcart.Products.Where(x => x.Code == "SEATASSIGNMENTS").Any())
            {
                foreach (MOBProductSegmentDetail segment in shoppingcart.Products.Where(x => x.Code == "SEATASSIGNMENTS").FirstOrDefault().Segments)
                {
                    foreach (MOBProductSubSegmentDetail subsegment in segment.SubSegmentDetails)
                    {
                        if (IsPCUProduct(subsegment.SeatCode))
                        {
                            pcuPrice += Convert.ToDouble(subsegment.Price);
                        }
                    }
                }
            }
            return pcuPrice;
        }


        private void AssignAncillaryAmountToCSLFOP(List<FormOfPayment> formOfPayment, MOBShoppingCart shoppingCart, double? allowedETCAncillaryAmount, List<MOBFOPCertificate> usedCertificatesList, string[] CombinebilityETCAppliedAncillaryCodes, string flow, bool isBundleProduct)
        {
            double pcuTotalPrice = 0;
            bool isPCUSeat = false;
            #region logic for finding the PCUSeat.(There is no separate for product for PCU in shoppingcart when purchase through seats flow in manageres.
            if (ConfigUtility.IsViewResFlowCheckOut(flow))
            {
                pcuTotalPrice = getPCUProductPrice(shoppingCart);
                isPCUSeat = pcuTotalPrice > 0 && shoppingCart.Products.Exists(p => p.Code == "SEATASSIGNMENTS");
            }
            #endregion logic for finding the PCUSeat

            foreach (var CombinebilityETCAppliedAncillaryCode in CombinebilityETCAppliedAncillaryCodes)
            {
                var product = shoppingCart.Products.Find(p => p.Code == CombinebilityETCAppliedAncillaryCode);
                if (product != null || (isPCUSeat && CombinebilityETCAppliedAncillaryCode == "PCU"))
                {
                    var productAmount = product != null ? (isPCUSeat ? Convert.ToDouble(product.ProdTotalPrice) - pcuTotalPrice : Convert.ToDouble(product.ProdTotalPrice)) : pcuTotalPrice;
                    foreach (var certificate in usedCertificatesList)
                    {
                        if (certificate.RedeemAmount > 0 && productAmount > 0)
                        {

                            double certificateAmount = AssignRedeemAmountWhichIsGreater(certificate.RedeemAmount, productAmount);

                            certificate.RedeemAmount -= certificateAmount;
                            productAmount -= certificateAmount;
                            allowedETCAncillaryAmount -= certificateAmount;

                            certificate.CertificateTraveler = null;
                            certificate.IsForAllTravelers = true;
                            int i = 0;
                            if (certificateAmount > 0)
                            {
                                FormOfPayment fop = BuildAncillaryCslFOP(shoppingCart, CombinebilityETCAppliedAncillaryCode, certificate, certificateAmount);

                                if (isBundleProduct && !string.IsNullOrEmpty(shoppingCart.PaymentTarget) && shoppingCart.PaymentTarget.Contains("SEATASSIGNMENTS"))
                                {
                                    fop.PaymentTarget = shoppingCart.PaymentTarget;
                                }

                                if (_configuration.GetValue<bool>("EnableEtcforSeats_PCU_Viewres") ? shoppingCart.Flow != null && (shoppingCart.Flow != FlowType.VIEWRES.ToString() && shoppingCart.Flow != FlowType.VIEWRES_SEATMAP.ToString()) : true)
                                {
                                    var ccCombinebilityETCAppliedAncillaryCodeFop = formOfPayment.Find(p => p.PaymentTarget == CombinebilityETCAppliedAncillaryCode && p.Payment.CreditCard != null);
                                    if (ccCombinebilityETCAppliedAncillaryCodeFop != null)
                                    {
                                        ccCombinebilityETCAppliedAncillaryCodeFop.Payment.CreditCard.Amount -= certificateAmount;
                                        ccCombinebilityETCAppliedAncillaryCodeFop.Payment.CreditCard.Amount = Math.Round(ccCombinebilityETCAppliedAncillaryCodeFop.Payment.CreditCard.Amount, 2, MidpointRounding.AwayFromZero);
                                        if (ccCombinebilityETCAppliedAncillaryCodeFop.Payment.CreditCard.Amount == 0)
                                        {
                                            formOfPayment.Remove(ccCombinebilityETCAppliedAncillaryCodeFop);
                                        }
                                    }
                                }
                                //[MOBILE-8683]:  mapp: ETC + CC Seats checkout is failing and we getting Seats unable to assign message on the confirmation screen
                                //Making fix on behalf of csl service team to send the certificate object at the beginning of the list.
                                if (_configuration.GetValue<bool>("EnableEtcforSeats_PCU_Viewres") && (shoppingCart.Flow == FlowType.VIEWRES.ToString() || shoppingCart.Flow == FlowType.VIEWRES_SEATMAP.ToString()))
                                {
                                    formOfPayment.Insert(i, fop);
                                    i++;
                                }
                                else
                                {
                                    formOfPayment.Add(fop);
                                }
                            }

                        }
                    }
                    if ((_configuration.GetValue<bool>("EnableEtcforSeats_PCU_Viewres") && shoppingCart.Flow != null && shoppingCart.Flow == FlowType.VIEWRES.ToString()) ||
                        _configuration.GetValue<bool>("ETCForAllProductsToggle"))
                        formOfPayment.RemoveAll(x => x.Payment?.CreditCard == null && x.Payment?.Certificate == null);
                }

            }

            #region Building FormofPayment for PCUSeat -- Only for ViewResFlow
            if (isPCUSeat)
            {
                var creditCardFop = formOfPayment.Find(p => p.Payment != null && p.Payment.CreditCard != null);
                #region Building CreditCard fop for Seats not amount covered with certificate
                var seatProduct = shoppingCart.Products.Find(p => p.Code == "SEATASSIGNMENTS");
                if (seatProduct != null && creditCardFop != null)
                {
                    var seatPrice = Convert.ToDouble(seatProduct.ProdTotalPrice) - pcuTotalPrice;
                    var seatPricefop = formOfPayment.Where(p => p.PaymentTarget == "SEATASSIGNMENTS" && p.Payment.Certificate != null);
                    if (seatPricefop != null)
                    {
                        var seatsPriceCoveredwithcert = seatPricefop.Sum(f => f.Amount);
                        if (seatPrice - (double)seatsPriceCoveredwithcert > 0)
                        {
                            FormOfPayment seatFop = creditCardFop.Clone();
                            seatFop.Payment.CreditCard.Amount = seatPrice - (double)seatsPriceCoveredwithcert;
                            seatFop.PaymentTarget = "SEATASSIGNMENTS";
                            formOfPayment.Add(seatFop);
                        }
                    }
                }
                #endregion Building CreditCard fop for Seats not amount covered with certificate

                #region Building CreditCard fop for pcu Seats not amount covered with certificate

                var pcuSeatPricefop = formOfPayment.Where(p => p.PaymentTarget == "PCU" && p.Payment.Certificate != null);
                if (creditCardFop != null && pcuSeatPricefop != null)
                {
                    var pcuSeatPriceCoveredwithcert = pcuSeatPricefop.Sum(f => f.Amount);
                    if (pcuTotalPrice - (double)pcuSeatPriceCoveredwithcert > 0)
                    {
                        FormOfPayment pcuSeatFop = creditCardFop.Clone();
                        pcuSeatFop.Payment.CreditCard.Amount = pcuTotalPrice - (double)pcuSeatPriceCoveredwithcert;
                        pcuSeatFop.PaymentTarget = "PCU";
                        formOfPayment.Add(pcuSeatFop);
                    }

                }
                #endregion Building CreditCard fop for pcu Seats not amount covered with certificate
                formOfPayment.Remove(formOfPayment.Find(p => p.PaymentTarget == "SEATASSIGNMENTS,PCU"));
            }
            #endregion Building FormofPayment for PCUSeat
        }

        private FormOfPayment BuildAncillaryCslFOP(MOBShoppingCart shoppingCart, string CombinebilityETCAppliedAncillaryCode, MOBFOPCertificate certificate, double certificateAmount)
        {
            var fop = GetCSL(shoppingCart.FormofPaymentDetails, shoppingCart.SCTravelers, certificate);
            fop.Amount = (Math.Round((decimal)certificateAmount, 2));
            fop.Payment.Certificate.Amount = Math.Round(certificateAmount, 2);
            fop.PaymentTarget = CombinebilityETCAppliedAncillaryCode;
            fop.Payment.Certificate.Payor.Surname = certificate.RecipientsLastName;
            fop.Payment.Certificate.Payor.FirstName = certificate.RecipientsFirstName;
            fop.Payment.Certificate.Payor.GivenName = certificate.RecipientsFirstName;

            return fop;
        }

        private void AssignRESAmountToCSLFop(List<FormOfPayment> formOfPayment, MOBShoppingCart shoppingCart, List<MOBFOPCertificate> usedCertificatesList, bool istc)
        {
            double allowedETCRESAmount = shoppingCart.Products.Exists(c => c.Code.Equals("RES")) ? Convert.ToDouble(shoppingCart.Products.Find(c => c.Code.Equals("RES")).ProdTotalPrice) : 0;
            foreach (var certificate in usedCertificatesList)
            {
                if (allowedETCRESAmount == 0)
                    break;

                foreach (var sctraveler in shoppingCart.SCTravelers)
                {
                    if (certificate.RedeemAmount == 0 || allowedETCRESAmount == 0)
                        break;

                    IEnumerable<FormOfPayment> UserAssignedCSLFOPs = formOfPayment.Where(fp => fp?.Payment?.Certificate?.Payor.Key == sctraveler.TravelerNameIndex);
                    decimal etcAmountAddedForCustomer = 0;
                    if (UserAssignedCSLFOPs != null && UserAssignedCSLFOPs.Count() > 0)
                    {
                        etcAmountAddedForCustomer = UserAssignedCSLFOPs.Sum(fpAmt => fpAmt.Amount);
                    }
                    if (etcAmountAddedForCustomer < (decimal)sctraveler.IndividualTotalAmount)
                    {
                        double certificateAmount = AssignRedeemAmountWhichIsGreater(certificate.RedeemAmount, (sctraveler.IndividualTotalAmount - (double)etcAmountAddedForCustomer));

                        allowedETCRESAmount -= certificateAmount;
                        certificate.RedeemAmount -= certificateAmount;

                        if (certificateAmount > 0)
                        {
                            FormOfPayment fop = BuildsRESCslFOP1(shoppingCart, certificate, sctraveler, certificateAmount);
                            var ccRESFop = formOfPayment.Find(p => p.PaymentTarget == "RES" && p.Payment.CreditCard != null);
                            if (ccRESFop != null)
                            {
                                ccRESFop.Payment.CreditCard.Amount -= certificateAmount;
                                ccRESFop.Payment.CreditCard.Amount = Math.Round(ccRESFop.Payment.CreditCard.Amount, 2, MidpointRounding.AwayFromZero);
                                if (ccRESFop.Payment.CreditCard.Amount == 0)
                                {
                                    formOfPayment.Remove(ccRESFop);
                                }
                            }
                            if (istc)
                            {
                                var travelCredit = shoppingCart.FormofPaymentDetails?.TravelCreditDetails?.TravelCredits?.FirstOrDefault(c => c.PinCode == certificate.PinCode);
                                if (travelCredit != null)
                                {
                                    fop.Payment.Certificate.PromoCode = travelCredit.PromoCode;
                                }
                                formOfPayment.Insert(0, fop);
                            }
                            else
                            {
                                formOfPayment.Add(fop);
                            }
                        }
                    }
                }
            }
        }

        private double AssignRedeemAmountWhichIsGreater(double certificateRedeemAmount, double redeemAmountFromCertificate)
        {
            return certificateRedeemAmount > redeemAmountFromCertificate ? redeemAmountFromCertificate : certificateRedeemAmount;
        }

        private FormOfPayment BuildsRESCslFOP1(MOBShoppingCart shoppingCart, MOBFOPCertificate certificate, MOBCPTraveler sctraveler, double certificateAmount)
        {
            var fop = GetCSL(shoppingCart.FormofPaymentDetails, shoppingCart.SCTravelers, certificate);
            fop.Amount = (Math.Round((decimal)certificateAmount, 2));
            fop.Payment.Certificate.Amount = Math.Round(certificateAmount, 2);
            if (sctraveler.TravelerTypeCode.ToUpper().Equals("INF"))
            {
                MOBCPTraveler mOBCPTraveler = shoppingCart.SCTravelers.FirstOrDefault(st => !st.TravelerTypeCode.ToUpper().Equals("INF"));
                fop.Payment.Certificate.Payor.Key = mOBCPTraveler != null ? mOBCPTraveler.TravelerNameIndex : sctraveler.TravelerNameIndex;
            }
            else
            {
                fop.Payment.Certificate.Payor.Key = sctraveler.TravelerNameIndex;
            }
            fop.Payment.Certificate.Payor.Type = string.IsNullOrEmpty(sctraveler.CslReservationPaxTypeCode) ? sctraveler.TravelerTypeCode : sctraveler.CslReservationPaxTypeCode;
            fop.PaymentTarget = "RES";

            return fop;
        }

        private void AddCertificateFOP(List<FormOfPayment> formOfPayment, MOBFormofPaymentDetails fopDtl, List<MOBCPTraveler> scTravelers)
        {
            if (fopDtl?.TravelCertificate?.Certificates?.Count > 0)
            {
                foreach (var certificate in fopDtl.TravelCertificate.Certificates)
                {
                    if (_configuration.GetValue<bool>("MTETCToggle") && certificate.CertificateTraveler != null && certificate.CertificateTraveler?.TravelerNameIndex == "0")
                    {
                        ApplyToAllTravelerSplittongFOPAmountToEachTraveler(formOfPayment, fopDtl, scTravelers, certificate);
                    }
                    else
                    {
                        var fop = GetCSL(fopDtl, scTravelers, certificate);
                        formOfPayment.Add(fop);
                    }
                }
            }
        }

        private void ApplyToAllTravelerSplittongFOPAmountToEachTraveler(List<FormOfPayment> formOfPayment, MOBFormofPaymentDetails fopDtl, List<MOBCPTraveler> scTravelers, MOBFOPCertificate certificate)
        {
            var totalCertificateRedeemAmount = certificate.RedeemAmount;
            foreach (var sctraveler in scTravelers)
            {
                double certificateAmount = 0;
                if (totalCertificateRedeemAmount > sctraveler.IndividualTotalAmount)
                {
                    certificateAmount = sctraveler.IndividualTotalAmount;
                    totalCertificateRedeemAmount -= sctraveler.IndividualTotalAmount;
                }
                else
                {
                    certificateAmount = totalCertificateRedeemAmount;
                    totalCertificateRedeemAmount = 0;
                }
                if (certificateAmount > 0)
                {
                    var fop = GetCSL(fopDtl, scTravelers, certificate);
                    fop.Amount = (decimal)certificateAmount;
                    fop.Payment.Certificate.Amount = certificateAmount;
                    if (sctraveler.TravelerTypeCode.ToUpper().Equals("INF"))
                    {
                        MOBCPTraveler mOBCPTraveler = scTravelers.FirstOrDefault(st => !st.TravelerTypeCode.ToUpper().Equals("INF"));
                        fop.Payment.Certificate.Payor.Key = mOBCPTraveler != null ? mOBCPTraveler.TravelerNameIndex : sctraveler.TravelerNameIndex;
                    }
                    else
                    {
                        fop.Payment.Certificate.Payor.Key = sctraveler.TravelerNameIndex;
                    }
                    fop.Payment.Certificate.Payor.Type = string.IsNullOrEmpty(sctraveler.CslReservationPaxTypeCode) ? sctraveler.TravelerTypeCode : sctraveler.CslReservationPaxTypeCode;
                    formOfPayment.Add(fop);
                }
            }
        }

        private bool EnableTPI(int appId, string appVersion, int path)
        {
            // path 1 means confirmation flow, path 2 means view reservation flow, path 3 means booking flow 
            if (path == 1)
            {
                // ==>> Venkat and Elise chagne code to offer TPI postbooking when inline TPI is off for new clients 2.1.36 and above
                // App Version 2.1.36 && ShowTripInsuranceSwitch = true
                bool offerTPIAtPostBooking = _configuration.GetValue<bool>("ShowTripInsuranceSwitch") &&
                    GeneralHelper.IsApplicationVersionGreater(appId, appVersion, "AndroidTPIConfirmationVersion", "iPhoneTPIConfirmationVersion", "", "", true, _configuration);
                if (offerTPIAtPostBooking)
                {
                    offerTPIAtPostBooking = !GeneralHelper.IsApplicationVersionGreater(appId, appVersion, "AndroidTPIBookingVersion", "iPhoneTPIBookingVersion", "", "", true, _configuration);                    // When the Flag is true, we offer for old versions, when the flag is off, we offer for all versions. 
                    if (!offerTPIAtPostBooking && _configuration.GetValue<bool>("ShowTPIatPostBooking_ForAppVer_2.1.36_UpperVersions"))
                    {
                        offerTPIAtPostBooking = true;
                    }
                }
                return offerTPIAtPostBooking;
            }
            else if (path == 2)
            {
                return _configuration.GetValue<bool>("ShowTripInsuranceViewResSwitch") &&
                    GeneralHelper.IsApplicationVersionGreater(appId, appVersion, "AndroidTPIViewResVersion", "iPhoneTPIViewResVersion", "", "", true,
                    _configuration);
            }
            else if (path == 3)
            {
                return _configuration.GetValue<bool>("ShowTripInsuranceBookingSwitch") &&
                    GeneralHelper.IsApplicationVersionGreater(appId, appVersion, "AndroidTPIBookingVersion", "iPhoneTPIBookingVersion", "", "", true, _configuration);
            }
            else
            {
                return false;
            }
        }

        private List<FormOfPayment> ReshopAddPaxToIssueFFCR
           (MOBCheckOutRequest checkOutRequest, MOBShoppingCart persistedShoppingCart, Reservation persistedReservation)
        {
            //OLD Client
            if (!ConfigUtility.IncludeReshopFFCResidual
                (checkOutRequest.Application.Id, checkOutRequest.Application.Version.Major)) return null;
            //PNR Address NULL
            if (persistedReservation?.Reshop?.FFCRAddress == null) return null;

            //No Residual FFCR available
            if (!persistedReservation.Reshop.IsResidualFFCRAvailable) return null;

            string paymentTarget = "RES";
            string issueReasonCode = "RS";
            string key = "FFCR";
            List<FormOfPayment> formOfPaymentsLst = null;

            if (persistedShoppingCart?.SCTravelers != null && persistedShoppingCart.SCTravelers.Any())
            {
                formOfPaymentsLst = new List<FormOfPayment>();
                FormOfPayment formOfPayment = null;
                persistedShoppingCart?.SCTravelers.ForEach(pax =>
                {

                    var paxFFCRClaim = pax?.FutureFlightCredits?.FirstOrDefault(x => x.NewValueAfterRedeem > 0);
                    if (paxFFCRClaim != null)
                    {
                        formOfPayment = new FormOfPayment
                        {
                            PaymentTarget = paymentTarget,
                        };

                        formOfPayment.Payment
                        = new Service.Presentation.PaymentModel.FormOfPayment
                        {
                            Certificate = new Certificate
                            {
                                Amount = paxFFCRClaim.NewValueAfterRedeem,
                                IssueReasonCode = issueReasonCode,
                                Type = new Genre { Key = key },
                                Payor = new Service.Presentation.PersonModel.Person
                                {
                                    CustomerID = pax.PNRCustomerID,
                                    Key = pax.Key,
                                    GivenName = pax.LastName,
                                    FirstName = pax.FirstName,
                                    Type = pax.TravelerTypeCode,
                                },
                            }
                        };

                        formOfPayment.Payment.Certificate.BillingAddress
                        = MapResidualFFCRAddress(persistedReservation.Reshop.FFCRAddress);
                        formOfPaymentsLst.Add(formOfPayment);
                    }
                });
            }

            return formOfPaymentsLst;
        }

        private Address MapResidualFFCRAddress(MOBAddress mobaddress)
        {
            var addressLines = new Collection<string>();
            if (!string.IsNullOrEmpty(mobaddress.Line1))
            {
                addressLines.Add(mobaddress.Line1);
            }
            if (!string.IsNullOrEmpty(mobaddress.Line2))
            {
                addressLines.Add(mobaddress.Line2);
            }
            if (!string.IsNullOrEmpty(mobaddress.Line3))
            {
                addressLines.Add(mobaddress.Line3);
            }

            var csladdress = new Address
            {
                AddressLines = addressLines,
                City = mobaddress.City,
                StateProvince = new StateProvince { StateProvinceCode = mobaddress.State?.Code },
                Country = new United.Service.Presentation.CommonModel.Country { CountryCode = mobaddress.Country?.Code },
                PostalCode = mobaddress.PostalCode
            };

            return csladdress;
        }

        private string MaskCreditCardUnencryptedNumber(string unencryptedCardNumber)
        {
            if (unencryptedCardNumber.IsNullOrEmpty())
                return unencryptedCardNumber;

            return (unencryptedCardNumber.Length == 15 ? "XXXXXXXXXXX" : "XXXXXXXXXXXX") + unencryptedCardNumber.Substring(unencryptedCardNumber.Length - 4, 4);
        }

        private void AddEmailToRegisterFormsOfPaymentRequest(string email, RegisterFormsOfPaymentRequest registerFormsOfPaymentRequest)
        {
            if (!string.IsNullOrEmpty(email))
            {
                EmailAddress emailAddress = new EmailAddress();
                emailAddress.Address = email;
                registerFormsOfPaymentRequest.Emails.Add(emailAddress);
            }
        }

        private async Task<string> GetCartInformation(FlightReservationResponse flightReservationResponse, MOBCheckOutRequest checkoutRequest, string token)
        {
            LoadReservationAndDisplayCartRequest loadReservationAndDisplayCartRequest = new LoadReservationAndDisplayCartRequest();
            LoadReservationAndDisplayCartResponse loadReservationAndDisplayResponse = new LoadReservationAndDisplayCartResponse();
            loadReservationAndDisplayCartRequest.CartId = flightReservationResponse.CartId;
            loadReservationAndDisplayCartRequest.Reservation = flightReservationResponse.Reservation;
            string jsonRequest = JsonConvert.SerializeObject(loadReservationAndDisplayCartRequest);

            string actionName = "LoadReservationAndDisplayCart";

            string jsonResponse = await _flightShoppingService.GetCartInformation(token, actionName, jsonRequest, checkoutRequest.SessionId);

            if (!string.IsNullOrEmpty(jsonResponse))
            {
                LoadReservationAndDisplayCartResponse response = JsonConvert.DeserializeObject<LoadReservationAndDisplayCartResponse>(jsonResponse);
                if (response != null && response.Status.Equals(United.Services.FlightShopping.Common.StatusType.Success) && !(response.Errors != null && response.Errors.Count > 0))
                {
                    if (_configuration.GetValue<bool>("EnableCognitivedisabilityForEmailConfirmation"))
                    {
                        jsonResponse = jsonResponse.Replace("DPNA_1", "DPNA");
                    }

                    GetHTMLFromDotCom(jsonResponse, checkoutRequest);
                }
            }

            return jsonResponse;
        }

        private string GetHTMLFromDotCom(string htmlRequest, MOBCheckOutRequest checkoutRequest)
        {
            string url = string.Format("{0}/rev", _configuration.GetValue<string>("DotComGetHTMLResponse"));

            //todo chan
            //string jsonResponse = HttpHelper.Post(url, "application/json; charset=utf-8", "", htmlRequest);
            string jsonResponse = string.Empty;

            var jsonresponsedata = Newtonsoft.Json.Linq.JObject.Parse(jsonResponse);

            if (jsonresponsedata != null)
            {
                string status = (string)jsonresponsedata["status"];
                string error = (string)jsonresponsedata["error"];

                if (error == null && status.ToLower() == "success")
                {
                    string EmailSubject = (string)jsonresponsedata["data"]["EmailSubject"];
                    string EmailBody = (string)jsonresponsedata["data"]["EmailBody"];
                    string Errors = (string)jsonresponsedata["data"]["errors"];

                    SendEmail(EmailSubject, EmailBody, checkoutRequest.FormofPaymentDetails.EmailAddress,
                        checkoutRequest.Application.Id, checkoutRequest.SessionId, checkoutRequest.Application.Version.Major, checkoutRequest.DeviceId);
                }
                else
                {
                    _logger.LogInformation("GETHTMLFromDotCom - Response for GetConfirmationMessage {jsonResponse} with {sessionId}", jsonResponse, checkoutRequest.SessionId);
                }
            }

            return jsonResponse;
        }

        public void SendEmail(string subject, string message, string emailAddress, int appId, string sessionId, string major, string deviceId)
        {
            try
            {
                using (SmtpClient smtp = new SmtpClient(_configuration.GetValue<string>("EmailServer")))
                {
                    MailMessage mail = new MailMessage();
                    mail.From = new MailAddress(_configuration.GetValue<string>("SendEmailConfirmationFrom"));
                    mail.IsBodyHtml = true;
                    mail.Subject = subject;
                    mail.Body = message;
                    mail.To.Add(emailAddress != null ? emailAddress : "");
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                    //if (traceSwitch.TraceWarning)
                    //{
                    //    logEntries.Add(LogEntry.GetLogEntry<string>(sessionId, "SendEmail", "EmailNotification", appId, major, deviceId, "EmailSentSuccessfully"));
                    _logger.LogInformation("SendEmail - EmailNotification {message} with {sessionId}", "EmailSentSuccessfully", sessionId);
                    //}
                }
            }
            catch (SmtpFailedRecipientException exe)
            {
                //if (traceSwitch.TraceError)
                //{
                //    logEntries.Add(LogEntry.GetLogEntry<string>(sessionId, "SendEmail", "EmailNotification", appId, major, deviceId, exe.FailedRecipient));
                //}
                //_logger.LogInformation("SendEmail - EmailNotification {message} with {sessionId}", "EmailSentSuccessfully", sessionId);
                _logger.LogError("SendEmail - EmailNotification Error {message} {StackTrace} and {session}", exe.Message, exe.StackTrace, sessionId);
            }
        }


        private bool IsBagPurchaseFromCheckinFlow(string flowName, string paymentTarget)
        {
            return UtilityHelper.IsCheckinFlow(flowName) && paymentTarget == "BAG";
        }

        private async Task<Service.Presentation.PaymentModel.CreditCard> MasterpassPayLoad_CFOP(MOBMasterpassSessionDetails masterpassSession, double paymentAmount, string sessionId, MOBApplication application, string deviceId)
        {
            Service.Presentation.PaymentModel.CreditCard creditCard = new Service.Presentation.PaymentModel.CreditCard();
            creditCard.Payor = new Service.Presentation.PersonModel.Person();

            creditCard.Amount = paymentAmount;
            creditCard.OperationID = sessionId;
            creditCard.AccountNumberToken = masterpassSession.AccountNumberToken;
            creditCard.ExpirationDate = masterpassSession.ExpirationDate;
            creditCard.Code = masterpassSession.Code;
            creditCard.Name = masterpassSession.Name;
            creditCard.Payor.GivenName = masterpassSession.GivenName;
            creditCard.Payor.Surname = masterpassSession.SurName;
            creditCard.Currency = new Service.Presentation.CommonModel.Currency();
            creditCard.Currency.Code = "USD";
            creditCard.Type = new Genre();
            creditCard.Type.DefaultIndicator = masterpassSession.MasterpassType.DefaultIndicator;
            creditCard.Type.Description = masterpassSession.MasterpassType.Description;
            creditCard.Type.Key = masterpassSession.MasterpassType.Key;
            creditCard.Type.Value = masterpassSession.MasterpassType.Val;
            creditCard.IsBinRangeValidation = "FALSE";
            creditCard.BillingAddress = AssignCreditCardBillingAddress(masterpassSession.BillingAddress);
            creditCard.CreditCardTypeCode = (United.Service.Presentation.CommonEnumModel.CreditCardTypeCode)masterpassSession.CreditCardTypeCode;
            creditCard.Description = (United.Service.Presentation.CommonEnumModel.CreditCardType)masterpassSession.Description;
            creditCard.Payor.Contact = new Service.Presentation.PersonModel.Contact();
            creditCard.Payor.Contact.Emails = new Collection<EmailAddress> { new EmailAddress { Address = masterpassSession.ContactEmailAddress } };
            //MOBILE-1683/MOBILE-1669/MOBILE-1671: PA,PB,PCU- Masterpass : Richa
            MOBVormetricKeys vormetricKeys = await AssignPersistentTokenToCC(masterpassSession.AccountNumberToken, masterpassSession.PersistentToken, string.Empty, masterpassSession.Code, sessionId, "MasterpassPayLoad_CFOP", application.Id, deviceId);
            if (!string.IsNullOrEmpty(vormetricKeys.PersistentToken))
            {
                creditCard.PersistentToken = vormetricKeys.PersistentToken;
                if (!string.IsNullOrEmpty(vormetricKeys.SecurityCodeToken))
                {
                    creditCard.SecurityCodeToken = vormetricKeys.SecurityCodeToken;
                    creditCard.SecurityCode = vormetricKeys.SecurityCodeToken;
                }

                if (!string.IsNullOrEmpty(vormetricKeys.CardType) && string.IsNullOrEmpty(creditCard.Code))
                {
                    creditCard.Code = vormetricKeys.CardType;
                }
            }

            return creditCard;
        }

        public async Task<MOBVormetricKeys> AssignPersistentTokenToCC(string accountNumberToken, string persistentToken, string securityCodeToken, string cardType, string sessionId, string action, int appId, string deviceID)
        {
            MOBVormetricKeys vormetricKeys = new MOBVormetricKeys();
            if (_configuration.GetValue<bool>("VormetricTokenMigration"))
            {
                if ((string.IsNullOrEmpty(persistentToken) || string.IsNullOrEmpty(cardType)) && !string.IsNullOrEmpty(accountNumberToken) && !string.IsNullOrEmpty(sessionId) && !string.IsNullOrEmpty(accountNumberToken))
                {
                    vormetricKeys = await GetPersistentTokenUsingAccountNumberToken(accountNumberToken, sessionId, accountNumberToken);
                    persistentToken = vormetricKeys.PersistentToken;
                }

                if (!string.IsNullOrEmpty(persistentToken))
                {
                    vormetricKeys.PersistentToken = persistentToken;
                    vormetricKeys.SecurityCodeToken = securityCodeToken;
                    vormetricKeys.CardType = cardType;
                }
                else
                {
                    LogNoPersistentTokenInCSLResponseForVormetricPayment(sessionId);
                }
            }
            else
            {
                persistentToken = string.Empty;
            }

            return vormetricKeys;
        }

        public async Task<MOBVormetricKeys> GetPersistentTokenUsingAccountNumberToken(string accountNumberToke, string sessionId, string token)
        {
            //string url = string.Format("{0}/{1}/RSA", _configuration.GetValue<string>("ServiceEndPointBaseUrl - CSLDataVault"), accountNumberToke);
            string url = string.Format("/{0}/RSA", accountNumberToke);
            var cslResponse = await MakeHTTPCallAndLogIt(
                                        sessionId,
                                        _headers.ContextValues.DeviceId,
                                        "CSL-ChangeEligibleCheck",
                                        _application,
                                        token,
                                        url,
                                        string.Empty,
                                        true,
                                        false);

            return GetPersistentTokenFromCSLDatavaultResponse(cslResponse, sessionId);
        }

        private MOBVormetricKeys GetPersistentTokenFromCSLDatavaultResponse(string jsonResponseFromCSL, string sessionID)
        {
            MOBVormetricKeys vormetricKeys = new MOBVormetricKeys();
            if (!string.IsNullOrEmpty(jsonResponseFromCSL))
            {
                CslDataVaultResponse response = DataContextJsonSerializer.DeserializeJsonDataContract<CslDataVaultResponse>(jsonResponseFromCSL);
                if (response != null && response.Responses != null && response.Responses[0].Error == null && response.Responses[0].Message != null && response.Responses[0].Message.Count > 0 && response.Responses[0].Message[0].Code.Trim() == "0")
                {
                    var creditCard = response.Items[0] as Service.Presentation.PaymentModel.CreditCard;
                    vormetricKeys.PersistentToken = creditCard.PersistentToken;
                    vormetricKeys.SecurityCodeToken = creditCard.SecurityCodeToken;
                    vormetricKeys.CardType = creditCard.Code;
                }
                else
                {
                    if (response.Responses[0].Error != null && response.Responses[0].Error.Count > 0)
                    {
                        string errorMessage = string.Empty;
                        foreach (var error in response.Responses[0].Error)
                        {
                            errorMessage = errorMessage + " " + error.Text;
                        }
                        throw new MOBUnitedException(errorMessage);
                    }
                    else
                    {
                        string exceptionMessage = _configuration.GetValue<string>("UnableToInsertCreditCardToProfileErrorMessage");
                        if (_configuration.GetValue<string>("ReturnActualExceptionMessageBackForTesting") != null && _configuration.GetValue<bool>("ReturnActualExceptionMessageBackForTesting"))
                        {
                            exceptionMessage = exceptionMessage + " response.Status not success and response.Errors.Count == 0 - at DAL InsertTravelerCreditCard(MOBUpdateTravelerRequest request)";
                        }
                        throw new MOBUnitedException(exceptionMessage);
                    }
                }
            }

            return vormetricKeys;
        }

        private async Task<string> MakeHTTPCallAndLogIt(string sessionId, string deviceId, string action, MOBApplication application, string token, string url, string jsonRequest, bool isGetCall, bool isXMLRequest = false)
        {
            string jsonResponse = string.Empty;
            if (isGetCall)
            {
                jsonResponse = await _dataVaultService.GetPersistentToken(token, jsonRequest, url, sessionId);
            }
            else
            {
                jsonResponse = await _flightShoppingService.GetProducts(token, jsonRequest, sessionId);
            }

            return jsonResponse;
        }


        private void LogNoPersistentTokenInCSLResponseForVormetricPayment(string sessionId, string Message = "Unable to retieve PersistentToken")
        {
            _logger.LogWarning("LogNoPersistentTokenInCSLResponseForVormetricPayment-PERSISTENTTOKENNOTFOUND Error {exception} {sessionid}", Message, sessionId);
            //No need to block the flow as we are calling DataVault for Persistent Token during the final payment
        }

        private Address AssignCreditCardBillingAddress(MOBAddress billingAddress)
        {
            Service.Presentation.CommonModel.Address address = null;
            if (billingAddress != null)
            {
                address = new Service.Presentation.CommonModel.Address();
                var addressLines = new Collection<string>();
                AddAddressLinesToCslBillingAddress(billingAddress.Line1, ref addressLines);
                AddAddressLinesToCslBillingAddress(billingAddress.Line2, ref addressLines);
                AddAddressLinesToCslBillingAddress(billingAddress.Line3, ref addressLines);
                address.AddressLines = addressLines;
                address.City = billingAddress.City;
                if (billingAddress.State != null)
                {
                    address.StateProvince = new Service.Presentation.CommonModel.StateProvince();
                    address.StateProvince.StateProvinceCode = !string.IsNullOrEmpty(billingAddress.State.Code) ? billingAddress.State.Code : billingAddress.State.Name;
                    address.StateProvince.Name = billingAddress.State.Name;
                    address.StateProvince.ShortName = billingAddress.State.Name;
                }
                if (billingAddress.Country != null)
                {
                    address.Country = new Country();
                    address.Country.CountryCode = billingAddress.Country.Code.ToUpper();
                }
                address.PostalCode = billingAddress.PostalCode;
            }

            return address;
        }

        private Collection<string> AddAddressLinesToCslBillingAddress(string line, ref Collection<string> lines)
        {
            if (!string.IsNullOrEmpty(line))
            {
                var linesAfterSplit = line.Replace("\r\n", "|").Replace("\n", "|").Split('|').ToCollection();
                lines = lines.Concat(linesAfterSplit).ToCollection();
            }

            return lines;
        }

        private async Task<Service.Presentation.PaymentModel.CreditCard> ApplePayLoadDataVault_CFOP(MOBApplePay applePayInfo,
            double paymentAmount, Session session, MOBApplication application, string deviceId, Service.Presentation.PaymentModel.CreditCard applepayCreditCard)
        {
            _logger.LogInformation("MakeReservation {ApplePayLoadJSON} and {SessionId}", applePayInfo.ApplePayLoadJSON, session.SessionId);
            MOBApplePayLoad applePayLoad = JsonConvert.DeserializeObject<MOBApplePayLoad>(applePayInfo.ApplePayLoadJSON);

            Service.Presentation.PaymentModel.CreditCard creditCard = null;

            if (!_configuration.GetValue<bool>("DisableDoubleAuthorizationFixForApplePay"))
            {
                creditCard = applepayCreditCard.Clone();
            }
            else
            {
                creditCard = await GenerateApplepayTokenWithDataVault(applePayLoad, session.SessionId, session.Token, application, deviceId);
            }

            creditCard.Amount = paymentAmount;
            if (_configuration.GetValue<bool>("EDDtoEMDToggle"))
            {
                creditCard.OperationID = Guid.NewGuid().ToString(); // This one we can pass the session id which we using in bookign path.
            }
            else
            {
                creditCard.OperationID = Guid.NewGuid().ToString().ToUpper().Replace("-", "");
            }
            creditCard.WalletCategory = Service.Presentation.CommonEnumModel.WalletCategory.ApplePay;

            creditCard.Payor = new Service.Presentation.PersonModel.Person();
            creditCard.Name = GetCardHolderFullname(applePayInfo.CardHolderName);
            creditCard.Payor.GivenName = applePayInfo.CardHolderName.First;
            creditCard.Payor.Surname = applePayInfo.CardHolderName.Last;
            creditCard.AccountNumberLastFourDigits = applePayInfo.LastFourDigits;
            creditCard.AccountNumberMasked = "*****" + applePayInfo.LastFourDigits;
            creditCard.BillingAddress = AssignCreditCardBillingAddress(applePayInfo.BillingAddress);
            AssignCSLCreditCardCode(applePayInfo, creditCard);

            creditCard.Currency = new Service.Presentation.CommonModel.Currency();
            creditCard.Currency.Code = applePayInfo.CurrencyCode;  // " PaymentCryptogram ": "AnQeed0ACbTkwQRZF0hUMAACAAA=",

            return creditCard;
        }

        private string GetCardHolderFullname(MOBName cardHolderName)
        {
            return string.Format("{0} {1} {2}", cardHolderName.First, cardHolderName.Middle, cardHolderName.Last).Replace("  ", " ").Trim();
        }

        private void AssignCSLCreditCardCode(MOBApplePay mobApplePay, Service.Presentation.PaymentModel.CreditCard creditCard)
        {
            Dictionary<string, string> dictPaymentTypes = new Dictionary<string, string>();
            dictPaymentTypes.Add("VISA", "VI");
            dictPaymentTypes.Add("AMEX", "AX");
            dictPaymentTypes.Add("DISCOVER", "DS");
            dictPaymentTypes.Add("MASTERCARD", "MC");
            dictPaymentTypes.Add("AMERICANEXPRESS", "AX");
            dictPaymentTypes.Add("UNIONPAY", "UP");

            creditCard.Code = dictPaymentTypes[mobApplePay.CardName.ToUpper()];
        }

        private async Task<Service.Presentation.PaymentModel.CreditCard> GenerateApplepayTokenWithDataVault(MOBApplePayLoad applePayLoad, string sessionID, string token,
           MOBApplication applicationDetails, string deviceID)
        {

            #region
            Service.Presentation.PaymentModel.CreditCard cc = null;
            CslDataVaultRequest dataVaultRequest = GetDataValutRequest(applePayLoad, sessionID);
            string jsonRequest = DataContextJsonSerializer.Serialize(dataVaultRequest);
            //string actionName = "AddPayment";
            var jsonResponse = await _dataVaultService.GetCCTokenWithDataVault(token, jsonRequest, sessionID);

            if (!string.IsNullOrEmpty(jsonResponse))
            {
                var payment = DeserializeDatavaultResponse(sessionID, applicationDetails, deviceID, jsonResponse);
                cc = (Service.Presentation.PaymentModel.CreditCard)payment;
            }
            else
            {
                string exceptionMessage = _configuration.GetValue<string>("UnableToInsertCreditCardToProfileErrorMessage");
                if (_configuration.GetValue<string>("ReturnActualExceptionMessageBackForTesting") != null && _configuration.GetValue<bool>("ReturnActualExceptionMessageBackForTesting"))
                {
                    exceptionMessage = exceptionMessage + " - due to jsonResponse is empty at DAL  GenerateCCTokenWithDataVault(MOBUpdateTravelerRequest request, ref string ccDataVaultToken)";
                }
                throw new MOBUnitedException(exceptionMessage);
            }

            return cc;
            #endregion
        }

        private Payment DeserializeDatavaultResponse(string sessionID, MOBApplication applicationDetails, string deviceID, string jsonResponse)
        {
            Payment payment = null;
            CslDataVaultResponse response = DataContextJsonSerializer.DeserializeUseContract<CslDataVaultResponse>(jsonResponse);

            _logger.LogInformation("DeserializeDatavaultResponse - DeSerialized Response{DeSerialized Response} with {SessionId}", JsonConvert.SerializeObject(jsonResponse), sessionID);

            if (response != null && response.Responses != null && response.Responses[0].Error == null && response.Responses[0].Message != null && response.Responses[0].Message.Count > 0 && response.Responses[0].Message[0].Code.Trim() == "0")
            {
                if (response.Items != null && response.Items.Count > 0)
                {
                    payment = response.Items[0];

                }
            }
            else
            {
                if (response.Responses[0].Error != null && response.Responses[0].Error.Count > 0)
                {
                    string errorMessage = string.Empty;
                    foreach (var error in response.Responses[0].Error)
                    {
                        errorMessage = errorMessage + " " + error.Text;
                    }
                    throw new MOBUnitedException(errorMessage);
                }
                else
                {
                    string exceptionMessage = _configuration.GetValue<string>("UnableToInsertCreditCardToProfileErrorMessage");
                    if (_configuration.GetValue<string>("ReturnActualExceptionMessageBackForTesting") != null && _configuration.GetValue<bool>("ReturnActualExceptionMessageBackForTesting"))
                    {
                        exceptionMessage = exceptionMessage + " response.Status not success and response.Errors.Count == 0 - at DAL InsertTravelerCreditCard(MOBUpdateTravelerRequest request)";
                    }
                    throw new MOBUnitedException(exceptionMessage);
                }
            }

            return payment;
        }

        private CslDataVaultRequest GetDataValutRequest(MOBApplePayLoad applePayLoad, string sessionID)
        {
            #region
            var dataVaultRequest = new CslDataVaultRequest
            {
                Items = new Collection<Payment>(),
                Types = new Collection<Service.Presentation.CommonModel.Characteristic>(),
                CallingService = new ServiceClient { Requestor = new Requestor { AgentAAA = "WEB", ApplicationSource = "mobile services" } }
            };
            //InsertCreditCardRequest creditCardInsertRequest = new InsertCreditCardRequest();
            if (applePayLoad != null)
            {
                var cc = new Service.Presentation.PaymentModel.CreditCard();
                cc.AccountNumberEncrypted = applePayLoad.Data;
                if (_configuration.GetValue<bool>("PassMobileSessionIDInsteadOfDifferntGuidEveryTime"))
                {
                    cc.OperationID = sessionID; // This one we can pass the session id which we using in bookign path.
                }
                else if (_configuration.GetValue<bool>("EDDtoEMDToggle"))
                {
                    cc.OperationID = Guid.NewGuid().ToString(); // This one we can pass the session id which we using in bookign path.
                }
                else
                {
                    cc.OperationID = Guid.NewGuid().ToString().ToUpper().Replace("-", "");
                }
                dataVaultRequest.Items.Add(cc);

                dataVaultRequest.Types = new Collection<Service.Presentation.CommonModel.Characteristic>();
                dataVaultRequest.Types.Add(new Service.Presentation.CommonModel.Characteristic { Code = "ENCRYPTION", Value = "APPLEPAY" });
                dataVaultRequest.Types.Add(new Service.Presentation.CommonModel.Characteristic { Code = "APPLEPAY_PUBLIC_KEY", Value = applePayLoad.Header.EphemeralPublicKey });
            }

            return dataVaultRequest;
            #endregion
        }

        private async Task<CslDataVaultRequest> GetMOBDataValutRequest(MOBCreditCard creditCardDetails, string sessionID, MOBApplication application)
        {
            #region
            var dataVaultRequest = new CslDataVaultRequest
            {
                Items = new System.Collections.ObjectModel.Collection<United.Service.Presentation.PaymentModel.Payment>(),
                Types = new System.Collections.ObjectModel.Collection<Characteristic>(),
                CallingService = new United.Service.Presentation.CommonModel.ServiceClient { Requestor = new Requestor { AgentAAA = "WEB", ApplicationSource = "mobile services" } }
            };
            InsertCreditCardRequest creditCardInsertRequest = new InsertCreditCardRequest();
            Session session = new Session();
            session = await _sessionHelperService.GetSession<Session>(sessionID, session.ObjectName, new List<string> { sessionID, session.ObjectName }).ConfigureAwait(false);
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
                    dataVaultRequest.Types = new System.Collections.ObjectModel.Collection<Characteristic>();
                    dataVaultRequest.Types.Add(new Characteristic { Code = "ENCRYPTION", Value = "PKI" });
                    cc.AccountNumberEncrypted = creditCardDetails.EncryptedCardNumber;

                    if (_configuration.GetValue<bool>("EnablePKDispenserKeyRotationAndOAEPPadding") && creditCardDetails.IsOAEPPaddingCatalogEnabled)
                    {
                        if (ConfigUtility.IsSuppressPkDispenserKey(application.Id, application.Version.Major, session?.CatalogItems))
                        {
                            if (string.IsNullOrEmpty(creditCardDetails?.Kid))
                            {
                                _logger.LogError("GetDataValutRequest Validation failed -{Exception} and {sessionId}", "Kid is empty in the request", sessionID);
                                throw new Exception(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                            }
                            else
                            {
                                dataVaultRequest.Types.Add(new Characteristic { Code = "KID", Value = creditCardDetails.Kid });
                            }
                        }
                        else
                        {
                            string key = string.Format(_configuration.GetValue<string>("PKDispenserKeyTokenKeyFormat"), application.Id);
                            var pKDispenserKey = await _cachingService.GetCache<string>(key, "TID1");//same name as couchbase
                            var obj = JsonConvert.DeserializeObject<United.Service.Presentation.SecurityResponseModel.PKDispenserKey>(pKDispenserKey);

                            dataVaultRequest.Types.Add(new Characteristic { Code = "KID", Value = obj.Kid });
                        }
                        dataVaultRequest.Types.Add(new Characteristic { Code = "OAEP", Value = "TRUE" });
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
                    dataVaultRequest.Types.Add(new Characteristic { Code = "DOLLAR_DING", Value = "TRUE" });
                }
                dataVaultRequest.Items.Add(cc);
            }

            return dataVaultRequest;
            #endregion
        }


        private void SendClubPassReceipt(List<MOBClubDayPass> passes, string maskedCCNumber)
        {
            try
            {
                using (SmtpClient smtp = new SmtpClient(_configuration.GetSection("EmailServer").Value))
                {
                    MailMessage mail = new MailMessage();
                    mail.From = new MailAddress(_configuration.GetSection("EmailFrom").Value);
                    mail.To.Add(passes[0].Email);

                    if (passes.Count == 1)
                    {
                        mail.Subject = "Your United Club one-time pass";
                    }
                    else
                    {
                        mail.Subject = "Your United Club one-time passes";
                    }

                    mail.IsBodyHtml = true;

                    string html = @"

        <!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>
        <html xmlns='http://www.w3.org/1999/xhtml'>
        <head id='ctl00_Head1'><title>United Airlines</title>
        <style type='text/css'>
        * {padding:0;margin:0}
        table {border-collapse:collapse;border-spacing:0}
        img, table {border:0}
        .co-message-content p {margin:0 0 12px}
        .co-message-content, .co-message-content td, .co-message-content th {font-family:Arial,Helvetica,sans-serif,Tahoma;font-size:12px;color:#333}
        .co-message-content ul, .co-message-content ol {margin:12px 0}
        .co-message-content li {margin:3px 0 3px 24px}
        .co-message-content h4 {color:#039;margin:0 0 10px;font-size:16px}
        .co-message-content h5 {color:#000;margin:0;font-size:100%}
        </style>
        </head>
        <body link='#3366cc' vlink='#c3b487' style='margin:0;color:#000;background-color:#dbdbdb'>
        <table  border='0' cellspacing='0' cellpadding='0' style='color:#000;background-color:#dbdbdb;width:100%'>
        <tr>
        <td style='padding:10px'>
        <table border='0' cellspacing='0' cellpadding='0' style='width:650px;margin:10px'>
        <tr>
        <td style='padding-top:10px'>
        <table border='0' cellspacing='0' cellpadding='0' style='width:100%'>
        <tr id='ctl00_tableDeliveryMsg'>
        <td style='color:#666;font-family:Arial,Helvetica,sans-serif,Tahoma;font-size:10px;padding-bottom:5px'>Add <strong>unitedairlines@united.com</strong> to your address book. <a style='color:#666' href='http://www.united.com/safelist' target='_blank'>See instructions.</a></td>
        </tr>
        </table>
        <table border='0' cellspacing='0' cellpadding='0' style='width:100%'>
        <tr>
        <td style='border:solid 1px #b8b8b8;border-bottom:none;background-color:#36c;height:8px;font-size:6px'>&nbsp;</td>
        </tr>
        <tr>
        <td style='border:solid 1px #b8b8b8;border-top:none;border-bottom:none;background-color:#fff;padding:12px 10px'>
        <table cellspacing='0' cellpadding='0' border='0' style='width:100%'>
        <tr>
        <td><a target='_blank' href='http://www.united.com/'><img src='http://www.united.com/web/format/img/email/template/united-logo.gif' alt='United Airlines' width='191' height='33' /></a></td>
        <td style='font-family:Arial,Helvetica,sans-serif,Tahoma;color:#333;font-size:10px;text-align:right;vertical-align:bottom'><div id='ctl00_divCurrentDate'>TODAYS_DATE</div></td>
        </tr>
        </table>
        </td>
        </tr>
        <tr>
        <td style='border:solid 1px #b8b8b8;border-top:none;border-bottom:none;background-color:#fff;padding:10px 10px'>
        <table cellspacing='0' cellpadding='0' border='0' style='background-color:#1d3c98;background-image:url(http://www.united.com/web/format/img/email/menu/center.gif);width:100%;height:29px'>
        <tr style='font-family:Arial,Helvetica,sans-serif,Tahoma;font-size:12px;color:#fff;font-weight:bold;vertical-align:middle;text-align:center'>
        <td style='width:10px;height:29px;font-size:1px'><img src='http://www.united.com/web/format/img/email/menu/left.gif' alt='' width='10' height='29' border='0' /></td>
        <td><a href='http://www.united.com/web/en-US/Default.aspx?sender=TECH&camp=&campyear=2012&Language=en-US' target='_blank' style='color:#fff;text-decoration:none'><span style='color:#fff'>united.com</span></a></td>
        <td style='width:5px'>|</td>
        <td><a href='http://www.united.com/web/en-US/content/deals/default.aspx?sender=TECH&camp=&campyear=2012&Language=en-US' target='_blank' style='color:#fff;text-decoration:none'><span style='color:#fff'>Deals &amp; Offers</span></a></td>
        <td style='width:5px'>|</td>
        <td><a href='http://www.united.com/web/en-US/content/reservations/default.aspx?sender=TECH&camp=&campyear=2012&Language=en-US' target='_blank' style='color:#fff;text-decoration:none'><span style='color:#fff'>Reservations</span></a></td>
        <td style='width:5px'>|</td>
        <td><a href='http://www.united.com/web/en-US/content/mileageplus/earn/default.aspx?sender=TECH&camp=&campyear=2012&Language=en-US' target='_blank' style='color:#fff;text-decoration:none'><span style='color:#fff'>Earn MileagePlus<sup>&reg;</sup> Miles</span></a></td>
        <td style='width:5px'>|</td>
        <td><a href='http://www.united.com/web/en-US/apps/account/account.aspx?sender=TECH&camp=&campyear=2012&Language=en-US' target='_blank' style='color:#fff;text-decoration:none'><span style='color:#fff'>My Account</span></a></td>
        <td style='text-align:right;width:10px;font-size:1px'><img src='http://www.united.com/web/format/img/email/menu/right.gif' width='10' height='29' border='0' alt=''/></td>
        </tr>
        </table>
        </td>
        </tr>
        <tr>
        <td style='border:solid 1px #b8b8b8;border-top:none;border-bottom:none;background-color:#fff;padding:5px 10px 0'>
        <table cellspacing='0' cellpadding='0' border='0' style='width:100%'>
        <tr>
        <td style='font-family:Arial,Helvetica,sans-serif,Tahoma;color:#006;font-size:18px;line-height:130%;font-weight:bold;padding:0 5px'></td>
        </tr>
        </table>
        </td>
        </tr>
        <tr>
        <td style='border:solid 1px #b8b8b8;border-top:none;border-bottom:none;background-color:#fff;padding:0 10px'>
        <table cellspacing='0' cellpadding='0' border='0' style='width:100%'>
        <tr>
        <td style='font-family:Arial,Helvetica,sans-serif,Tahoma;font-size:12px;line-height:130%;padding-left:5px;color:#333' class='co-message-content'>
        <table border='0' cellspacing='0' cellpadding='0' style='width:100%'>
        <tr>
        <td>
        <div style='margin-bottom:2em'>You purchase is complete. YOUR_PASSES_ARE_ATTACHED_IN_THE_EMAIL.</div>
        <table border='0' cellspacing='0' cellpadding='0' style='width:100%'>

        <tr>
        <td style='border:1px solid #002244'>
        <table border='0' cellspacing='0' cellpadding='0' style='border-bottom:1px solid #002244;width:100%'>
        <tr style='vertical-align:middle'>
        <td style='background-color:#002244;padding-left:.35em'><h1 style='color:#FFFFFF;font-size:16px;font-weight:bold;margin:0;background:none;padding:0'>YOUR_PASSES</h1></td>
        <td style='background-color:#002244;text-align:center;width:66px;height:30px:align:right'><img src='cid:UnitedClubLogo' width='64' height='28' border='0' alt='ALT_YOUR_PASSES'/></td>
        </tr>
        </table>
        </td>
        </tr>

        <tr>
        <td style='border:1px solid #002244'>
        <table border='0' cellspacing='0' cellpadding='0' style='width:100%'>
        <tr>
        <td>&nbsp;</td>
        <tr>
        <tr>
        <td>PASSES_DETAILS</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
        <tr>
        </table>
        </td>
        </tr>

        <tr>
            <td>&nbsp;</td>
        <tr>

        <tr>
        <td style='border:1px solid #666'>
        <table border='0' cellspacing='0' cellpadding='0' style='border-bottom:1px solid #666;width:100%'>
        <tr style='vertical-align:middle'>
        <td style='text-align:center;width:44px;height:30px'><img src='http://www.united.com/web/format/img/icon/dollarCircle.gif' width='26' height='25' border='0' alt='Purchase Summary'/></td>
        <td style='background-color:#ccc;padding-left:.35em'><h1 style='color:#039;font-size:14px;font-weight:bold;margin:0;background:none;padding:0'>Purchase Summary</h1></td>
        </tr>
        </table>

        </td>
        </tr>
        <tr>
        <td>

        <table border='0' cellspacing='0' cellpadding='0' style='width:100%'>
        <tr>
        <td style='border:2px solid #69c'>
        <table border='0' cellspacing='0' cellpadding='0' style='width:100%'>
        <tr><td style='font-size:1px;line-height:100%'><img src='http://www.united.com/web/format/img/email/gradient/bgGradLtBlue630x20Top.gif' width='630' height='20' alt='' style='width:100%' /></td></tr>
        <tr>
        <td style='padding:12px'>
        <table cellpadding='0' cellspacing='0' border='0' style='width:100%'>
        <tbody>
        <tr>
        <td style='text-align:left;padding:0 .25em 0 0'>PURCHASE_SUMMARY</td>
        <td style='text-align:right;padding:0 0 0 .25em' colspan='5'>PURCHASE_PRICE</td>
        </tr>
        <tr><td style='padding:.5em'>&nbsp;</td></tr>
        </tbody>
        <tbody>
        <tr>
        <td colspan='5' style='text-align:right;padding:2em 0 2em 1em;border-top:solid 1px #666;font-size:13px'>
        <b>Total Price:&nbsp;&nbsp;</b>
        </td>
        <td style='text-align: right; padding: 2em 0 2em 1em; border-top: solid 1px #666;font-size:13px'>
        <b><span id='ctl00_ContentEmail_spanTotalPrice' style='font-size: 1.175em; color: #008800'>TOTAL_PRICE</span></b>
        </td>
        </tr>
        </tbody>
        </table>
        <p style='margin-bottom:0;padding-top:1em;border-top:solid 2px #69c'><strong>Billing Information</strong></p>
        <style type='text/css'>
        .paymentDtl th {text-align:left;font-weight:normal;padding-right:1em}
        .paymentDtl td {font-weight:bold}
        </style>
        <table class='paymentDtl'>
        <tr id='ctl00_ContentEmail_ucPaymentInfo_trCardName'>
        <th>Name of Cardholder:</th>
        <td><span id='ctl00_ContentEmail_ucPaymentInfo_spCardName'>CARD_HOLDER_NAME</span></td>
        </tr>
        <tr id='ctl00_ContentEmail_ucPaymentInfo_trCardType'>
        <!--th>Card Type:</th>
        <td><span id='ctl00_ContentEmail_ucPaymentInfo_spCardType'>CARD_TYPE</span></td-->
        </tr>
        <tr id='ctl00_ContentEmail_ucPaymentInfo_trCardNumber'>
        <th>Card Number:</th>
        <td><span id='ctl00_ContentEmail_ucPaymentInfo_spCardNumber'>CARD_NUMBER</span></td>
        </tr>
        </table>
        </td>
        </tr>
        <tr>
        <td style='font-size:1px;line-height:100%'><img src='http://www.united.com/web/format/img/email/gradient/bgGradLtBlue630x20Btm.gif' width='630' height='20' alt='' style='width:100%' />
        </td>
        </tr>
        </table>
        </td>
        </tr>
        </table>
        </td>
        </tr>
        </table>
        </td>
        </tr>
        <tr>
        <td>&nbsp;</td>
        </tr>
        </table>
        </td>
        </tr>
        </table>
        </td>
        </tr>
        <tr>
        <td style='padding-top:17px'>
        <table border='0' cellspacing='0' cellpadding='0' style='width:100%'>
        <tr style='vertical-align:top'>
        <td style='font-family:Arial,Helvetica,sans-serif,Tahoma;font-size:10px;width:350px;color:#666'>View our <a href='http://www.united.com/web/en-US/content/privacy.aspx?sender=TECH&camp=&campyear=2012&Language=en-US' target='_blank' style='color:#666'>Privacy Policy</a>.</td>
        <td style='width:300px;text-align:right'><a href='http://www.united.com/web/en-US/content/company/alliance/star.aspx?sender=TECH&camp=&campyear=2012&Language=en-US' target='_blank'><img src='http://www.united.com/web/format/img/email/template/staralliance.gif' width='190' height='20' border='0' alt='Star Alliance' /></a></td>
        </tr>
        </table>
        </td>
        </tr>
        <tr>
        <td style='padding-top:17px;'> 
        <table style='font-family:Arial,Helvetica,sans-serif;font-size:10px;color:#333;margin:0 width=100%'>
        <tr>
        <td style='padding:0 3px'>Find United on:</td>
        <td style='padding:0 3px'><a href='http://www.united.com/web/en-US/apps/vendors/out.aspx?sender=TECH&camp=&campyear=2012&Language=en-US&i=emlfacebook' target='_blank'><img src='http://www.united.com/web/format/img/email/template/icon-facebook.png' width='16' height='16' border='0' alt='Facebook' /></a></td>
        <td style='padding:0 3px'><a href='http://www.united.com/web/en-US/apps/vendors/out.aspx?sender=TECH&camp=&campyear=2012&Language=en-US&i=emltwitter' target='_blank'><img src='http://www.united.com/web/format/img/email/template/icon-twitter.png' width='16' height='16' border='0' alt='Twitter' /></a></td>
        </tr>
        </table>
        </td>
        </tr>
        <tr>
        <td>&nbsp;</td>
        </tr>
        <tr>
        <td style='padding:2px'>
        <table border='0' cellspacing='0' cellpadding='0' style='width:100%'>
        <tr>
        <td style='font-family:Arial,Helvetica,sans-serif,Tahoma;color:#666;font-size:12px;line-height:130%'>
        <div><strong>E-mail Information</strong></div>
        <div><strong>Please do not reply to this message using the &#34reply&#34 address.</strong></div><br />

        <div>If you are experiencing technical issues, please contact the Electronic Support Desk by <a href='http://www.united.com/web/en-US/content/Contact/technical/support.aspx?sender=TECH&camp=&campyear=2012&Language=en-US' target='_blank'>e-mail</a> or <a href='http://www.united.com/web/en-US/content/Contact/technical/default.aspx?sender=TECH&camp=&campyear=2012&Language=en-US' target='_blank'>telephone</a>. For issue related to your MileagePlus account, please contact the MileagePlus Center by <a href='http://www.united.com/web/en-US/content/Contact/technical/support.aspx?sender=TECH&camp=&campyear=2012&Language=en-US' target='_blank'>e-mail</a> or <a href='http://www.united.com/web/en-US/content/Contact/technical/default.aspx?sender=TECH&camp=&campyear=2012&Language=en-US' target='_blank'>telephone</a>.</div><br />

        <div>The information contained in this e-mail is intended for the original recipient only.</div><br />

        </td>
        </tr>
        </table>

        </td>
        </tr>
        <tr>
        <td style='padding:5px 0'>

        <table cellspacing='0' cellpadding='0' border='0' style='width:100%'>
        <tr>
        <td style='font-family:Arial,Helvetica,sans-serif,Tahoma;color:#666;font-size:10px'>
        Copyright &copy; 2012 United Air Lines, Inc.
        </td>
        </tr>
        </table>
        </td>
        </tr>
        </table>
        </td>
        </tr>
        </table>

        </body>
        </html>
                            ";

                    html = html.Replace("TODAYS_DATE", DateTime.Today.ToString("dddd, MMMM dd, yyyy"));
                    if (passes.Count == 1)
                    {
                        html = html.Replace("YOUR_PASSES_ARE_ATTACHED_IN_THE_EMAIL", "Your pass is attached to this email");
                        html = html.Replace("YOUR_PASSES", "Your Pass");
                        html = html.Replace("ALT_YOUR_PASSES", "Your Pass");
                    }
                    else
                    {
                        html = html.Replace("YOUR_PASSES_ARE_ATTACHED_IN_THE_EMAIL", "Your passes are attached to this email");
                        html = html.Replace("YOUR_PASSES", "Your Passes");
                        html = html.Replace("ALT_YOUR_PASSES", "Your Passes");
                    }

                    StringBuilder passDetails = new StringBuilder();
                    foreach (var pass in passes)
                    {
                        passDetails.Append("<tr>");
                        passDetails.Append("<td style='text-align:left;padding:0 .25em 0 .5em'><p style='color:#333333;font-size:12px;margin:3;background:none;padding:0'>");
                        passDetails.Append("United Club one-time pass number");
                        passDetails.Append("</p></td>");
                        passDetails.Append("<td><p style='color:#333333;font-size:12px;margin:5;background:none;padding:0'>");
                        passDetails.Append(pass.PassCode);
                        passDetails.Append("</p></td>");
                        passDetails.Append("<td><p style='color:#333333;font-size:12px;margin:5;background:none;padding:0'>");
                        passDetails.Append("Valid through &nbsp;");
                        passDetails.Append(pass.ExpirationDate);
                        passDetails.Append("</p></td>");
                        passDetails.Append("</tr>");
                    }
                    html = html.Replace("PASSES_DETAILS", passDetails.ToString());

                    string purchaseSummary = string.Empty;
                    string totalPrice = string.Empty;
                    if (passes.Count == 1)
                    {
                        purchaseSummary = ConvertNumberToNumberName(passes.Count) + " United Club one-time pass at $" + passes[0].PaymentAmount + " per pass";
                        totalPrice = String.Format("{0:C}", passes[0].PaymentAmount * passes.Count);
                    }
                    else
                    {
                        purchaseSummary = ConvertNumberToNumberName(passes.Count) + " United Club one-time passes at $" + passes[0].PaymentAmount + " per pass";
                        totalPrice = String.Format("{0:C}", passes[0].PaymentAmount * passes.Count);
                    }

                    html = html.Replace("PURCHASE_SUMMARY", purchaseSummary);
                    html = html.Replace("PURCHASE_PRICE", totalPrice);
                    html = html.Replace("TOTAL_PRICE", totalPrice);

                    html = html.Replace("CARD_HOLDER_NAME", passes[0].FirstName + " " + passes[0].LastName);
                    html = html.Replace("CARD_NUMBER", maskedCCNumber);

                    AlternateView htmlView = AlternateView.CreateAlternateViewFromString(html, null, "text/html");

                    LinkedResource unitedClubLogo = new LinkedResource(string.Empty);
                    unitedClubLogo.ContentId = "UnitedClubLogo";
                    htmlView.LinkedResources.Add(unitedClubLogo);

                    mail.AlternateViews.Add(htmlView);

                    for (int i = 0; i < passes.Count; ++i)
                    {
                        mail.Attachments.Add(new Attachment(GetClubPassPDF(passes, i)));
                    }
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
            }
            catch (System.Exception ex)
            {

            }
        }

        private string GetClubPassPDF(List<MOBClubDayPass> passes, int passIndex)
        {
            string fullFileName = string.Empty;

            DataSet dataSet = new DataSet();
            dataSet.Tables.Add();

            DataTable dataTable = new DataTable("Pass");

            dataTable.Columns.Add("PassCode", typeof(string));
            dataTable.Columns.Add("MPAccountNumber", typeof(string));
            dataTable.Columns.Add("FirstName", typeof(string));
            dataTable.Columns.Add("LastName", typeof(string));
            dataTable.Columns.Add("Email", typeof(string));
            dataTable.Columns.Add("PaymentAmount", typeof(double));
            dataTable.Columns.Add("PurchaseDate", typeof(string));
            dataTable.Columns.Add("ExpirationDate", typeof(string));
            dataTable.Columns.Add("BarCode", typeof(byte[]));

            int index = 0;
            foreach (var pass in passes)
            {
                if (index == passIndex)
                {
                    DataRow newRow = dataTable.NewRow();
                    newRow["PassCode"] = pass.PassCode;
                    newRow["MPAccountNumber"] = pass.MileagePlusNumber;
                    newRow["FirstName"] = pass.FirstName;
                    newRow["LastName"] = pass.LastName;
                    newRow["Email"] = pass.Email;
                    newRow["PaymentAmount"] = pass.PaymentAmount;
                    newRow["PurchaseDate"] = pass.PurchaseDate;
                    newRow["ExpirationDate"] = pass.ExpirationDate;
                    newRow["BarCode"] = pass.BarCode;
                    dataTable.Rows.Add(newRow);
                }
                ++index;
            }

            dataSet.Tables.Add(dataTable);

            ReportDocument reportDocument = null;
            try
            {
                reportDocument.Export();
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                try
                {
                    if (reportDocument != null)
                    {
                        reportDocument.Close();
                    }
                }
                catch (System.Exception) { }
                try
                {
                    if (reportDocument != null)
                    {
                        reportDocument.Dispose();
                    }
                }
                catch (System.Exception) { }

                reportDocument = null;
            }

            return fullFileName;
        }

        private string ConvertNumberToNumberName(int number)
        {
            string numberName = number.ToString();

            switch (number)
            {
                case 1:
                    numberName = "One";
                    break;
                case 2:
                    numberName = "Two";
                    break;
                case 3:
                    numberName = "Three";
                    break;
                case 4:
                    numberName = "Four";
                    break;
                case 5:
                    numberName = "Five";
                    break;
                case 6:
                    numberName = "Six";
                    break;
                case 7:
                    numberName = "Seven";
                    break;
                case 8:
                    numberName = "Eight";
                    break;
                case 9:
                    numberName = "Nine";
                    break;
                case 10:
                    numberName = "Ten";
                    break;
                case 11:
                    numberName = "Eleven";
                    break;
                case 12:
                    numberName = "Twelve";
                    break;
                case 13:
                    numberName = "Thirteen";
                    break;
                case 14:
                    numberName = "Fourteen";
                    break;
                case 15:
                    numberName = "Fifteen";
                    break;
                case 16:
                    numberName = "Sixteen";
                    break;
                case 17:
                    numberName = "Seventeen";
                    break;
                case 18:
                    numberName = "Eighteen";
                    break;
                case 19:
                    numberName = "Nineteen";
                    break;
                case 20:
                    numberName = "Twenty";
                    break;
            }

            return numberName;
        }

        private bool IsEnableEnableCombineConfirmationAlerts(int applicationId, string appVersion)
        {
            if (_configuration.GetValue<bool>("EnableCombineConfirmationAlerts") && GeneralHelper.IsApplicationVersionGreaterorEqual(applicationId, appVersion, _configuration.GetValue<string>("Android_EnableCombineConfirmationAlerts_AppVersion"), _configuration.GetValue<string>("iPhone_EnableCombineConfirmationAlerts_AppVersion")))
            {
                return true;
            }

            return false;
        }

        private bool IsAlertMessageExists(string body, string order, List<MOBSection> alertMessages)
        {
            return alertMessages.Exists(alertmessage => alertmessage.Text2 == body && alertmessage.Order == order);
        }

        private List<MOBSection> SortAlertmessage(List<MOBSection> alertMessages)
        {
            if (alertMessages != null)
                return alertMessages.OrderBy(c => (int)(MOBCONFIRMATIONALERTMESSAGEORDER)System.Enum.Parse(typeof(MOBCONFIRMATIONALERTMESSAGEORDER), c.Order)).ToList();

            return alertMessages;
        }

        public async Task<Session> GetValidateSession(string sessionId, bool isBookingFlow, bool isViewRes_CFOPFlow)
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                sessionId = Guid.NewGuid().ToString().ToUpper().Replace("-", "");
            }

            Session session = new Session();
            session = await _sessionHelperService.GetSession<Session>(sessionId, session.ObjectName, new List<string> { sessionId, session.ObjectName }).ConfigureAwait(false);
            if (session == null)
            {
                if (isBookingFlow)
                    throw new MOBUnitedException(_configuration.GetValue<string>("BookingSessionExpiryMessage"));
                else if (isViewRes_CFOPFlow)
                    throw new MOBUnitedException(((int)MOBErrorCodes.ViewResCFOPSessionExpire).ToString(), _configuration.GetValue<string>("ViewResSessionExpiredMessage"));
                else
                    throw new MOBUnitedException(_configuration.GetValue<string>("GeneralSessionExpiryMessage"));
            }
            //if (session.TokenExpireDateTime <= DateTime.Now && session.TokenExpireDateTime != DateTime.MinValue)
            //{
            //    session.IsTokenExpired = true;
            //    if (isViewRes_CFOPFlow)
            //        throw new MOBUnitedException(((int)MOBErrorCodes.ViewResCFOPSessionExpire).ToString(), _configuration.GetValue<string>("ViewResSessionExpiredMessage"));
            //    else
            //        throw new MOBUnitedException(_configuration.GetValue<string>("BookingSessionExpiryMessage"));
            //}
            if (GetTokenExpireDateTimeUTC(session))
            {
                session.IsTokenExpired = true;
                if (isViewRes_CFOPFlow)
                    throw new MOBUnitedException(((int)MOBErrorCodes.ViewResCFOPSessionExpire).ToString(), _configuration.GetValue<string>("ViewResSessionExpiredMessage"));

                else
                    throw new MOBUnitedException(_configuration.GetValue<string>("BookingSessionExpiryMessage"));
            }

            session.LastSavedTime = DateTime.Now;
            await _sessionHelperService.SaveSession<Session>(session, sessionId, new List<string> { sessionId, session.ObjectName }, session.ObjectName).ConfigureAwait(false);

            return session;
        }

        private bool IsUSACountryAddress(MOBCountry country)
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
            }

            return isUSAAddress;
        }

        private void AddTravelBankToFOP(List<FormOfPayment> formOfPayment, MOBFormofPaymentDetails fopDetails, MOBApplication application, string flow, bool isSFOP)
        {
            if (flow == FlowType.BOOKING.ToString() && IncludeTravelBankFOP(application.Id, application.Version.Major) && fopDetails.TravelBankDetails?.TBApplied > 0 && !isSFOP)
            {
                FormOfPayment fop = new FormOfPayment();
                fop.Payment = new Service.Presentation.PaymentModel.FormOfPayment();
                fop.Payment.TravelBank = new Service.Presentation.PaymentModel.TravelBank();
                fop.Payment.TravelBank.Amount = fopDetails.TravelBankDetails.TBApplied;
                fop.Payment.TravelBank.BillingAddress = GetCSLBillingAddress(fopDetails.BillingAddress);
                fop.Payment.TravelBank.OperationID = Guid.NewGuid().ToString();
                fop.Payment.TravelBank.Payor = new Service.Presentation.PersonModel.Person();
                fop.Payment.TravelBank.Payor.GivenName = fopDetails.TravelBankDetails.PayorFirstName;
                fop.Payment.TravelBank.Payor.Surname = fopDetails.TravelBankDetails.PayorLastName;
                fop.Payment.TravelBank.GroupPaymentType = Service.Presentation.CommonEnumModel.GroupPaymentType.None;
                fop.Payment.TravelBank.LoyaltyProgramMemberID = fopDetails.TravelBankDetails.MPNumber;
                fop.PaymentTarget = "RES";

                if (!_configuration.GetValue<bool>("DisableFixforFullTBPurchaseWithTPI_MOBILE16666"))
                    formOfPayment.Insert(0, fop);
                else
                    formOfPayment.Add(fop);

                RemoveRESFOPIfAmountZeroAfterApplyCredit(formOfPayment, fopDetails.TravelBankDetails.TBApplied);
                var ccNullRESFop = formOfPayment.Find(p => p.PaymentTarget == "RES" && p.Payment.CreditCard == null && p.Payment.TravelBank == null);
                if (ccNullRESFop != null)
                    formOfPayment.Remove(ccNullRESFop);
                ccNullRESFop = formOfPayment.Find(p => p.PaymentTarget == "SEATASSIGNMENTS" && p.Payment.CreditCard == null && p.Payment.TravelBank == null);
                if (ccNullRESFop != null)
                    formOfPayment.Remove(ccNullRESFop);
            }
        }

        private bool IncludeTravelBankFOP(int appId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnableTravelBankFOP")
                && GeneralHelper.isApplicationVersionGreater
                (appId, appVersion, "AndroidTravelBankFOPVersion", "iPhoneTravelBankFOPVersion", "", "", true, _configuration);
        }

        private Address GetCSLBillingAddress(MOBAddress billingAddress)
        {
            Address cslAddress = new Address();
            cslAddress.AddressLines = new System.Collections.ObjectModel.Collection<string>();
            cslAddress.AddressLines.Add(billingAddress.Line1);
            cslAddress.AddressLines.Add(billingAddress.Line2);
            cslAddress.AddressLines.Add(billingAddress.Line3);
            cslAddress.Country = new Country();
            if (billingAddress.Country != null)
            {
                cslAddress.Country.CountryCode = billingAddress.Country.Code;
            }
            cslAddress.City = billingAddress.City;
            cslAddress.PostalCode = billingAddress.PostalCode;
            cslAddress.StateProvince = new StateProvince();
            cslAddress.StateProvince.StateProvinceCode = billingAddress.State.Code;
            cslAddress.StateProvince.ShortName = billingAddress.State.Code;

            return cslAddress;
        }

        private void RemoveRESFOPIfAmountZeroAfterApplyCredit(List<FormOfPayment> formOfPayment, double fopAmount)
        {
            var ccRESFop = formOfPayment.Find(p => p.PaymentTarget == "RES" && p.Payment.CreditCard != null);
            if (ccRESFop != null)
            {
                ccRESFop.Payment.CreditCard.Amount -= fopAmount;
                ccRESFop.Payment.CreditCard.Amount
                    = Math.Round(ccRESFop.Payment.CreditCard.Amount, 2, MidpointRounding.AwayFromZero);

                ccRESFop.Amount = (decimal)ccRESFop.Payment.CreditCard.Amount;
                if (ccRESFop.Payment.CreditCard.Amount <= 0)
                {
                    formOfPayment.Remove(ccRESFop);
                }
            }
        }

        private void AddMoneyPlusMilesToFOP(List<FormOfPayment> formOfPayment, MOBShoppingCart shoppingCart, List<MOBSHOPPrice> prices, MOBCPTraveler profileOwner)
        {
            if (_configuration.GetValue<bool>("EnableMilesPlusMoney"))
            {
                MOBFOPMoneyPlusMiles selectedMoneyPlusMiles = shoppingCart.FormofPaymentDetails?.MoneyPlusMilesCredit?.SelectedMoneyPlusMiles;
                if (selectedMoneyPlusMiles != null)
                {
                    var ccRESFop = formOfPayment.Find(p => p.PaymentTarget == "RES" && p.Payment.CreditCard != null);
                    if (ccRESFop != null)
                    {
                        ccRESFop.Payment.CreditCard.GroupPaymentType = Service.Presentation.CommonEnumModel.GroupPaymentType.MilesMoney;
                    }

                    FormOfPayment fop = new FormOfPayment
                    {
                        Payment = new Service.Presentation.PaymentModel.FormOfPayment(),
                        PaymentTarget = "RES",
                        Amount = (decimal)selectedMoneyPlusMiles.MilesOwed
                    };
                    fop.Payment.MilesMoney = new MilesMoney
                    {
                        Amount = selectedMoneyPlusMiles.MilesOwed,
                        Currency = new Currency() { Code = "MILES" },
                        EquivalentAmount = selectedMoneyPlusMiles.MilesMoneyValue,
                        OperationID = Guid.NewGuid().ToString(),
                        GroupID = shoppingCart.CartId,
                        LoyaltyProgramMemberID = profileOwner.MileagePlus.MileagePlusId,
                        GroupPaymentType = Service.Presentation.CommonEnumModel.GroupPaymentType.MilesMoney,
                        Payor = new Service.Presentation.PersonModel.Person()
                        {
                            FirstName = profileOwner.FirstName,
                            Surname = profileOwner.LastName
                        }
                    };
                    formOfPayment.Add(fop);
                }
            }
        }

        private async Task AddFFCCertificateToFOP(List<FormOfPayment> formOfPayment, MOBShoppingCart shoppingCart, Session session, MOBApplication application, List<MOBSHOPPrice> prices)
        {
            if (shoppingCart.FormofPaymentDetails?.TravelFutureFlightCredit?.FutureFlightCredits?.Count > 0)
            {
                bool isOnTheFlyConversionEnabled = OnTheFlyConversionEnabled(application.Id, application.Version.Major);
                if (isOnTheFlyConversionEnabled)
                {
                    bool isTCOn = IncludeTravelCredit(application.Id, application.Version.Major);
                    bool isAncillaryFFCEnable = IsInclueWithThisToggle(application.Id, application.Version.Major, "EnableTravelCreditAncillary", "AndroidTravelCreditVersionAncillary", "iPhoneTravelCreditVersionAncillary");

                    List<MOBFOPFutureFlightCredit> futureFlightCredits = shoppingCart.FormofPaymentDetails?.TravelFutureFlightCredit?.FutureFlightCredits;
                    if (isTCOn)
                    {
                        formOfPayment.AddRange(await GetCSLFFCCertificate(futureFlightCredits, shoppingCart.FormofPaymentDetails, shoppingCart.SCTravelers, session, shoppingCart.Products, isAncillaryFFCEnable, prices));
                    }
                    else
                    {
                        var distinctPnrs = futureFlightCredits.Select(ffc => ffc.RecordLocator).Distinct().ToList();
                        foreach (var pnr in distinctPnrs)
                        {
                            FormOfPayment fop = new FormOfPayment();
                            fop.Payment = new Service.Presentation.PaymentModel.FormOfPayment();
                            fop.PaymentTarget = "RES";
                            var pnrFFCs = futureFlightCredits.Where(ffc => ffc.RecordLocator == pnr).ToList();
                            fop.Amount = (decimal)pnrFFCs.Sum(ffc => ffc.RedeemAmount);
                            {
                                fop.Payment.FutureFlightCredit = GetCSLFutureFlightCredit(pnr, pnrFFCs, shoppingCart.FormofPaymentDetails.BillingAddress, shoppingCart.SCTravelers, session.SessionId);
                            }

                            formOfPayment.Add(fop);
                        }
                    }

                    var ccRESFop = formOfPayment.Find(p => p.PaymentTarget == "RES" && p.Payment.CreditCard != null);
                    if (ccRESFop != null)
                    {
                        if (isAncillaryFFCEnable)
                            ccRESFop.Payment.CreditCard.Amount -= futureFlightCredits.Where(ffc => ffc.TravelerNameIndex != "ANCILLARY").Sum(ffc => ffc.RedeemAmount);
                        else
                            ccRESFop.Payment.CreditCard.Amount -= futureFlightCredits.Sum(ffc => ffc.RedeemAmount);

                        ccRESFop.Payment.CreditCard.Amount
                            = Math.Round(ccRESFop.Payment.CreditCard.Amount, 2, MidpointRounding.AwayFromZero);

                        ccRESFop.Amount = (decimal)ccRESFop.Payment.CreditCard.Amount;
                        if (ccRESFop.Payment.CreditCard.Amount <= 0)
                        {
                            formOfPayment.Remove(ccRESFop);
                        }
                    }

                    if (isTCOn)
                    {
                        UpdateCCProductPriceAfterApplyFFC(formOfPayment, isAncillaryFFCEnable, shoppingCart.Products);
                        RemoveEmptyResAndSeatObject(formOfPayment);
                    }
                }
                else
                {
                    foreach (var futureFlightCredit in shoppingCart.FormofPaymentDetails?.TravelFutureFlightCredit?.FutureFlightCredits)
                    {
                        var sctraveler = shoppingCart.SCTravelers.FirstOrDefault(t => t.TravelerNameIndex == futureFlightCredit.TravelerNameIndex);
                        if (futureFlightCredit.RedeemAmount > 0)
                        {
                            FormOfPayment fop = BuildsRESCslFOP(shoppingCart, futureFlightCredit, sctraveler);
                            var ccRESFop = formOfPayment.Find(p => p.PaymentTarget == "RES" && p.Payment.CreditCard != null);
                            if (ccRESFop != null)
                            {
                                ccRESFop.Payment.CreditCard.Amount -= futureFlightCredit.RedeemAmount;
                                ccRESFop.Payment.CreditCard.Amount = Math.Round(ccRESFop.Payment.CreditCard.Amount, 2, MidpointRounding.AwayFromZero);
                                if (ccRESFop.Payment.CreditCard.Amount == 0)
                                {
                                    formOfPayment.Remove(ccRESFop);
                                }
                            }
                            formOfPayment.Add(fop);
                        }
                    }
                }
            }
        }

        private FormOfPayment BuildsRESCslFOP(MOBShoppingCart shoppingCart, MOBFOPFutureFlightCredit futureFlightCredit, MOBCPTraveler sctraveler)
        {
            var fop = GetCSLFOP(shoppingCart.FormofPaymentDetails, sctraveler, futureFlightCredit);

            fop.Amount = (Math.Round((decimal)futureFlightCredit.RedeemAmount, 2));

            if (fop.Payment.Certificate != null)
            {
                fop.Payment.Certificate.Amount = Math.Round(futureFlightCredit.RedeemAmount, 2);
                if (sctraveler.TravelerTypeCode.ToUpper().Equals("INF"))
                {
                    MOBCPTraveler mOBCPTraveler = shoppingCart.SCTravelers.FirstOrDefault(st => !st.TravelerTypeCode.ToUpper().Equals("INF"));
                    fop.Payment.Certificate.Payor.Key = mOBCPTraveler != null ? mOBCPTraveler.TravelerNameIndex : sctraveler.TravelerNameIndex;
                }
                else
                {
                    fop.Payment.Certificate.Payor.Key = sctraveler.TravelerNameIndex;
                }
            }

            return fop;
        }

        private FormOfPayment GetCSLFOP(MOBFormofPaymentDetails fopDtl, MOBCPTraveler scTraveler, MOBFOPFutureFlightCredit futureFlightCredit)
        {
            FormOfPayment fop = new FormOfPayment();
            fop.Payment = new Service.Presentation.PaymentModel.FormOfPayment();
            fop.PaymentTarget = "RES";
            fop.Amount = (decimal)futureFlightCredit.RedeemAmount;

            fop.Payment.Certificate = new Certificate();
            fop.Payment.Certificate = GetCSLCertificate(futureFlightCredit, fopDtl.BillingAddress, fopDtl.Email, fopDtl.Phone, scTraveler);

            return fop;
        }

        private FormOfPayment GetCSL(MOBFormofPaymentDetails fopDtl, List<MOBCPTraveler> scTravelers, MOBFOPCertificate certificate)
        {
            FormOfPayment fop = new FormOfPayment();
            fop.Payment = new Service.Presentation.PaymentModel.FormOfPayment();
            fop.PaymentTarget = "RES";
            fop.Amount = (decimal)certificate.RedeemAmount;
            fop.Payment.Certificate = new Certificate();
            fop.Payment.Certificate = GetCSLCertificate1(certificate, fopDtl.BillingAddress, fopDtl.Email, fopDtl.Phone, scTravelers);

            return fop;
        }

        private Certificate GetCSLCertificate1(MOBFOPCertificate certificate, MOBAddress billingAddress, MOBEmail email, MOBCPPhone phone, List<MOBCPTraveler> scTravelers)
        {
            Certificate cslCertificate = new Certificate();
            cslCertificate.Amount = certificate.RedeemAmount;
            cslCertificate.BillingAddress = GetCSLBillingAddress(billingAddress);
            cslCertificate.OperationID = Guid.NewGuid().ToString();
            cslCertificate.PinCode = certificate.PinCode;
            cslCertificate.PromoCode = string.IsNullOrEmpty(certificate.PromoCode) ? certificate.YearIssued.Substring(2, 2) + "TCVA" : certificate.PromoCode;
            cslCertificate.Payor = new Service.Presentation.PersonModel.Person();
            cslCertificate.Payor.GivenName = certificate.RecipientsLastName;
            if (_configuration.GetValue<bool>("CombinebilityETCToggle"))
            {
                cslCertificate.Currency = new Currency();
                cslCertificate.Currency.Code = "USD";
            }
            if (_configuration.GetValue<bool>("MTETCToggle") && certificate.CertificateTraveler != null && certificate.CertificateTraveler?.TravelerNameIndex != "0")
            {
                MOBCPTraveler mOBCPTraveler1 = scTravelers.Find(t => t.TravelerNameIndex == certificate.CertificateTraveler.TravelerNameIndex);
                cslCertificate.Payor.Type = string.IsNullOrEmpty(mOBCPTraveler1.CslReservationPaxTypeCode) ? mOBCPTraveler1.TravelerTypeCode : mOBCPTraveler1.CslReservationPaxTypeCode;
                if (cslCertificate.Payor.Type.ToUpper().Equals("INF"))
                {
                    MOBCPTraveler mOBCPTraveler = scTravelers.FirstOrDefault(st => !st.TravelerTypeCode.ToUpper().Equals("INF"));
                    cslCertificate.Payor.Key = mOBCPTraveler != null ? mOBCPTraveler.TravelerNameIndex : certificate.CertificateTraveler.TravelerNameIndex;
                }
                else
                {
                    cslCertificate.Payor.Key = certificate.CertificateTraveler.TravelerNameIndex;
                }
            }
            else
            {
                if (!certificate.IsForAllTravelers)
                {
                    cslCertificate.Payor.Key = certificate.TravelerNameIndex;
                    cslCertificate.Payor.Type = scTravelers.Find(t => t.TravelerNameIndex == certificate.TravelerNameIndex)?.TravelerTypeCode;
                }
                else if (scTravelers.Count > 0)
                {
                    cslCertificate.Payor.Key = scTravelers[0].TravelerNameIndex;
                    cslCertificate.Payor.Type = scTravelers[0].TravelerTypeCode;
                }
            }
            if (email != null || phone != null)
            {
                cslCertificate.Payor.Contact = new Service.Presentation.PersonModel.Contact();
                if (email != null)
                {
                    cslCertificate.Payor.Contact.Emails = new System.Collections.ObjectModel.Collection<EmailAddress>();
                    EmailAddress cslEmail = new EmailAddress();
                    cslEmail.Address = email.EmailAddress;
                    cslCertificate.Payor.Contact.Emails.Add(cslEmail);
                }
                if (phone != null)
                {
                    cslCertificate.Payor.Contact.PhoneNumbers = new System.Collections.ObjectModel.Collection<United.Service.Presentation.CommonModel.Telephone>();
                    United.Service.Presentation.CommonModel.Telephone cslPhone = new United.Service.Presentation.CommonModel.Telephone();
                    cslPhone.PhoneNumber = phone.AreaNumber + phone.PhoneNumber;
                    cslPhone.CountryAccessCode = phone.CountryCode;
                }
            }

            return cslCertificate;
        }

        private Certificate GetCSLCertificate(MOBFOPFutureFlightCredit futureFlightCredit, MOBAddress billingAddress, MOBEmail email, MOBCPPhone phone, MOBCPTraveler scTraveler)
        {
            Certificate cslCertificate = new Certificate();
            cslCertificate.Amount = futureFlightCredit.RedeemAmount;
            cslCertificate.BillingAddress = GetCSLBillingAddress(billingAddress);
            cslCertificate.OperationID = Guid.NewGuid().ToString();
            cslCertificate.PinCode = futureFlightCredit.PinCode;
            cslCertificate.PromoCode = futureFlightCredit.PromoCode; //"21FFCR"; //futureFlightCredit.YearIssued.Substring(2, 2) + "TCVA";
            cslCertificate.Payor = new United.Service.Presentation.PersonModel.Person();
            cslCertificate.Payor.GivenName = futureFlightCredit.RecipientsFirstName;
            cslCertificate.Payor.Surname = futureFlightCredit.RecipientsLastName;
            cslCertificate.Type = new Genre();
            cslCertificate.Type.Key = "FFCT";
            if (_configuration.GetValue<bool>("CombinebilityETCToggle"))
            {
                cslCertificate.Currency = new Currency();
                cslCertificate.Currency.Code = "USD";
            }
            cslCertificate.Payor.Type = string.IsNullOrEmpty(scTraveler.CslReservationPaxTypeCode) ? scTraveler.TravelerTypeCode : scTraveler.CslReservationPaxTypeCode;
            cslCertificate.Payor.Key = scTraveler.TravelerNameIndex;

            if (email != null || phone != null)
            {
                cslCertificate.Payor.Contact = new United.Service.Presentation.PersonModel.Contact();
                if (email != null)
                {
                    cslCertificate.Payor.Contact.Emails = new System.Collections.ObjectModel.Collection<EmailAddress>();
                    EmailAddress cslEmail = new EmailAddress();
                    cslEmail.Address = email.EmailAddress;
                    cslCertificate.Payor.Contact.Emails.Add(cslEmail);
                }
                if (phone != null)
                {
                    cslCertificate.Payor.Contact.PhoneNumbers = new System.Collections.ObjectModel.Collection<United.Service.Presentation.CommonModel.Telephone>();
                    United.Service.Presentation.CommonModel.Telephone cslPhone = new United.Service.Presentation.CommonModel.Telephone();
                    cslPhone.PhoneNumber = phone.AreaNumber + phone.PhoneNumber;
                    cslPhone.CountryAccessCode = phone.CountryCode;
                }
            }

            return cslCertificate;
        }

        private void RemoveEmptyResAndSeatObject(List<FormOfPayment> formOfPayment)
        {
            var ccNullRESFop = formOfPayment.Find(p => p.PaymentTarget == "RES" && p.Payment.CreditCard == null && p.Payment.Certificate == null);
            if (ccNullRESFop != null)
                formOfPayment.Remove(ccNullRESFop);
            ccNullRESFop = formOfPayment.Find(p => p.PaymentTarget == "SEATASSIGNMENTS" && p.Payment.CreditCard == null && p.Payment.Certificate == null);
            if (ccNullRESFop != null)
                formOfPayment.Remove(ccNullRESFop);
        }

        private void UpdateCCProductPriceAfterApplyFFC(List<FormOfPayment> formOfPayment, bool isAncillaryFFCEnable, List<MOBProdDetail> products)
        {
            if (isAncillaryFFCEnable)
            {
                foreach (var productCode in GetAncillaryProductCode(products))
                {
                    var ffcFOP = formOfPayment.Where(f => f.PaymentTarget == productCode && f.Payment.Certificate != null);
                    if (ffcFOP != null)
                    {
                        var ffcProductFop = formOfPayment.Find(p => p.PaymentTarget == productCode && p.Payment.CreditCard != null);
                        if (ffcProductFop != null)
                        {
                            ffcProductFop.Payment.CreditCard.Amount -= ffcFOP.Sum(f => f.Payment.Certificate.Amount);
                            ffcProductFop.Payment.CreditCard.Amount
                                = Math.Round(ffcProductFop.Payment.CreditCard.Amount, 2, MidpointRounding.AwayFromZero);

                            ffcProductFop.Amount = (decimal)ffcProductFop.Payment.CreditCard.Amount;
                            if (ffcProductFop.Payment.CreditCard.Amount <= 0)
                            {
                                formOfPayment.Remove(ffcProductFop);
                            }
                        }
                        else
                        {
                            ffcProductFop = formOfPayment.Find(p => p.PaymentTarget == productCode && p.Payment.CreditCard == null && p.Payment.Certificate == null);
                            if (ffcProductFop != null)
                            {
                                formOfPayment.Remove(ffcProductFop);
                            }

                        }
                    }
                }
            }
        }

        private CslFutureFlightCredit GetCSLFutureFlightCredit(string recordLocator, List<MOBFOPFutureFlightCredit> futureFlightCredit, MOBAddress billingAddress, List<MOBCPTraveler> scTravelers, string sessionid)
        {

            var OnTheFlyOfferEligibilityResponsestr
                = _sessionHelperService.GetSession<string>(sessionid, typeof(United.Refunds.Common.Models.Response.OnTheFlyOfferEligibilityResponse).FullName + recordLocator, new List<string> { sessionid, typeof(United.Refunds.Common.Models.Response.OnTheFlyOfferEligibilityResponse).FullName + recordLocator }).ConfigureAwait(false);

            var OnTheFlyOfferEligibilityResponse
                = JsonConvert.DeserializeObject<United.Refunds.Common.Models.Response.OnTheFlyOfferEligibilityResponse>(OnTheFlyOfferEligibilityResponsestr.ToString());

            #region Remove un used travelers and traveler credits
            foreach (var cslTraveler in OnTheFlyOfferEligibilityResponse.OfferDetails.Travelers)
            {
                cslTraveler.TravelerCredits.RemoveWhere(tc => !futureFlightCredit.Exists(ffc => ffc.PinCode == tc.OfferKey));
            }
            OnTheFlyOfferEligibilityResponse.OfferDetails.Travelers.RemoveWhere(traveler => traveler.TravelerCredits == null || traveler.TravelerCredits.Count == 0);
            #endregion

            CslFutureFlightCredit cslFutureFlightCredit = new CslFutureFlightCredit { Travelers = new Collection<FutureFlightTraveler>() };

            foreach (var cslTraveler in OnTheFlyOfferEligibilityResponse.OfferDetails.Travelers)
            {
                var travelerFFCs = futureFlightCredit.Where(fc => fc.TravelerNameIndex == cslTraveler.Key);
                cslTraveler.Amount = (double)travelerFFCs.Sum(ffc => ffc.RedeemAmount);

                foreach (var cslTc in cslTraveler.TravelerCredits)
                {
                    var ffc = travelerFFCs.FirstOrDefault(fc => fc.PinCode == cslTc.OfferKey);
                    cslTc.CreditAmount.Amount = ffc.RedeemAmount;
                }
                cslTraveler.Currency = cslTraveler.TravelerCredits.FirstOrDefault().CreditAmount.Currency;
                cslFutureFlightCredit.Travelers.Add(cslTraveler);
                cslFutureFlightCredit.RecordLocator = OnTheFlyOfferEligibilityResponse.OfferDetails.RecordLocator;
            }

            cslFutureFlightCredit.BillingAddress = GetCSLPaymentBillingAddress(billingAddress);
            return cslFutureFlightCredit;
        }

        private PaymentAddress GetCSLPaymentBillingAddress(MOBAddress billingAddress)
        {
            if (billingAddress == null) return null;
            var cslAddress = new PaymentAddress
            {
                Address1 = billingAddress.Line1,
                CountryCode = billingAddress.Country?.Code,
                City = billingAddress.City,
                PostalCode = billingAddress.PostalCode
            };

            return cslAddress;
        }

        private bool OnTheFlyConversionEnabled(int applicationId, string applicationVersion)
        {
            return GeneralHelper.IsApplicationVersionGreaterorEqual
                    (applicationId, applicationVersion,
                   _configuration.GetValue<string>("Android_EnableOTFConversionAppVersion"), _configuration.GetValue<string>("iPhone_EnableOTFConversionAppVersion"));

        }

        private bool IncludeTravelCredit(int appId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnableTravelCredit") &&
                   GeneralHelper.isApplicationVersionGreater(appId, appVersion, "AndroidTravelCreditVersion", "iPhoneTravelCreditVersion", "", "", true, _configuration);
        }

        public bool IsInclueWithThisToggle(int appId, string appVersion, string configToggleKey, string androidVersion, string iosVersion)
        {
            return _configuration.GetValue<bool>(configToggleKey) &&
                   GeneralHelper.isApplicationVersionGreater(appId, appVersion, androidVersion, iosVersion, "", "", true, _configuration);
        }

        private async Task<List<FormOfPayment>> GetCSLFFCCertificate(List<MOBFOPFutureFlightCredit> pnrFFCs, MOBFormofPaymentDetails formofPaymentDetails,
            List<MOBCPTraveler> sCTravelers, Session session, List<MOBProdDetail> products, bool isAncillaryFFCEnable,
            List<MOBSHOPPrice> prices, MOBApplication application = null)
        {
            List<FormOfPayment> formOfPayment = new List<FormOfPayment>();
            if (pnrFFCs?.Count > 0)
            {
                foreach (var ffc in pnrFFCs.Where(ffc => ffc.TravelerNameIndex != "ANCILLARY"))
                {
                    FormOfPayment fop = new FormOfPayment();
                    Certificate certificate = new Certificate();

                    fop.Payment = new Service.Presentation.PaymentModel.FormOfPayment();
                    fop.PaymentTarget = "RES";


                    var sctraver = sCTravelers.FirstOrDefault(t => t.TravelerNameIndex == ffc.TravelerNameIndex);
                    var requestedCredit = formofPaymentDetails.TravelCreditDetails.TravelCredits
                                                            .FirstOrDefault(tc => tc.EligibleTravelerNameIndex.Contains(ffc.TravelerNameIndex) && tc.PinCode == ffc.PinCode && tc.RecordLocator == ffc.RecordLocator);
                    Collection<FFCRCertificate> cslReavelCredit = null;
                    if (requestedCredit.IsLookupCredit)
                    {
                        TCLookupByPinOrPNRResponse lookupByPinOrPNRResponse = await GetLookUpCSLResponse(pnrFFCs, session.SessionId, ffc, requestedCredit.LastName);
                        cslReavelCredit = requestedCredit.IsOTFFFC ? lookupByPinOrPNRResponse.FFCCertificates?.CertificateList : lookupByPinOrPNRResponse.FFCRCertificates?.CertificateList;
                    }
                    else
                    {
                        var cslPreloadResponse = await _sessionHelperService.GetSession<TCLookupByFreqFlyerNumWithEligibleResponse>(session.SessionId, typeof(TCLookupByFreqFlyerNumWithEligibleResponse).FullName, new List<string> { session.SessionId, typeof(TCLookupByFreqFlyerNumWithEligibleResponse).FullName }).ConfigureAwait(false);
                        cslReavelCredit = requestedCredit.IsOTFFFC ? cslPreloadResponse.FFCCertificates?.CertificateList : cslPreloadResponse.FFCRCertificates?.CertificateList;
                    }

                    var ffcrCertificate = cslReavelCredit.Where(c => c.TravelCreditList.Exists(t => (t.CertPin == ffc.PinCode || t.OrigTicketNumber == ffc.PinCode) &&
                                                                 t.Travellers.Exists(traveler => traveler.PaxIndex == ffc.TravelerNameIndex))).ToList();

                    var cslTravelCredit = ffcrCertificate.FirstOrDefault().TravelCreditList.FirstOrDefault(t => t.CertPin == ffc.PinCode || t.OrigTicketNumber == ffc.PinCode);
                    var cslTraveler = cslTravelCredit.Travellers.FirstOrDefault(tra => tra.PaxIndex == ffc.TravelerNameIndex);

                    certificate.Amount = ffc.RedeemAmount;
                    certificate.BillingAddress = GetCSLBillingAddress(formofPaymentDetails.BillingAddress);
                    certificate.GroupID = session.CartId;
                    certificate.OperationID = Guid.NewGuid().ToString();
                    certificate.Type = new Genre();
                    certificate.Type.Key = "FFCT";
                    certificate.UsedValue = ffc.RedeemAmount;
                    certificate.CurrentValue = ffc.CurrentValue;
                    certificate.IsNameMatchOverride = ffc.IsNameMatchWaiverApplied;
                    certificate.Payor = new United.Service.Presentation.PersonModel.Person();
                    certificate.Payor.GivenName = ffc.RecipientsFirstName;
                    certificate.Payor.Surname = ffc.RecipientsLastName;
                    certificate.Payor.Key = ffc.TravelerNameIndex;
                    certificate.Payor.Type = string.IsNullOrEmpty(sctraver.CslReservationPaxTypeCode) ? sctraver.TravelerTypeCode : sctraver.CslReservationPaxTypeCode;
                    certificate.PinCode = ffc.PinCode;
                    certificate.PromoCode = ffc.PromoCode;
                    certificate.ConfirmationId = ffc.RecordLocator;
                    if (!_configuration.GetValue<bool>("EnableIsNamematchWavier"))
                        certificate.IsNameMatchOverride = Convert.ToBoolean(cslTraveler.IsNameMatchWaiverApplied) && Convert.ToBoolean(cslTraveler.IsEligibleToRedeem);
                    else
                        certificate.IsNameMatchOverride = Convert.ToBoolean(cslTraveler.IsEligibleToRedeem);
                    if (requestedCredit.IsOTFFFC)
                    {
                        certificate.FutureFlightTraveler = new FutureFlightTraveler();
                        certificate.FutureFlightTraveler.TravelerCredits = new Collection<FutureFlightTravelerCredit>();
                        certificate.FutureFlightTraveler.FirstName = ffcrCertificate.FirstOrDefault().FirstName;
                        certificate.FutureFlightTraveler.LastName = ffcrCertificate.FirstOrDefault().LastName;
                        certificate.FutureFlightTraveler.DateOfBirth = ffcrCertificate.FirstOrDefault().DateOfBirth;
                        certificate.FutureFlightTraveler.Key = cslTraveler.PaxIndex;
                        var futureFlightTravelerCredit = new FutureFlightTravelerCredit();
                        futureFlightTravelerCredit.CreditAmount = new Charge();
                        futureFlightTravelerCredit.CreditAmount.Amount = ffc.CurrentValue;
                        certificate.Currency = new Currency() { Code = cslTravelCredit.CertificateNumber.Substring(cslTravelCredit.CertificateNumber.LastIndexOf('_') + 1) };
                        futureFlightTravelerCredit.IsEligibleToRedeem = cslTraveler.IsEligibleToRedeem;
                        futureFlightTravelerCredit.OfferKey = cslTravelCredit.CertificateNumber;
                        futureFlightTravelerCredit.ExpirationDate = cslTravelCredit.CertExpDate;
                        futureFlightTravelerCredit.TicketNumber = cslTravelCredit.OrigTicketNumber;
                        futureFlightTravelerCredit.IsNameMatch = cslTraveler.IsNameMatch;
                        futureFlightTravelerCredit.IsNameMatchWaiverApplied = cslTraveler.IsNameMatchWaiverApplied;
                        futureFlightTravelerCredit.IsTravelDateBeginsBeforeCertExpiry = cslTraveler.IsTravelDateBeginsBeforeCertExpiry;
                        certificate.FutureFlightTraveler.TravelerCredits.Add(futureFlightTravelerCredit);
                    }
                    fop.Payment.Certificate = certificate;
                    formOfPayment.Insert(0, fop);
                }

                if (isAncillaryFFCEnable)
                {
                    await BuildAncillaryFOPS(formOfPayment, formofPaymentDetails, session, products, prices, application);
                }
            }

            return formOfPayment;
        }

        private async Task<TCLookupByPinOrPNRResponse> GetLookUpCSLResponse(List<MOBFOPFutureFlightCredit> pnrFFCs, string sessionid, MOBFOPFutureFlightCredit ffc, string requestedCreditLastName)
        {
            var lookupByPinOrPNRResponse = await _sessionHelperService.GetSession<TCLookupByPinOrPNRResponse>(sessionid, typeof(TCLookupByPinOrPNRResponse).FullName + ffc.RecordLocator.ToUpper() + requestedCreditLastName.ToUpper(), new List<string> { sessionid, typeof(TCLookupByPinOrPNRResponse).FullName + ffc.RecordLocator.ToUpper() + requestedCreditLastName.ToUpper() }).ConfigureAwait(false);

            if (lookupByPinOrPNRResponse == null)
            {
                foreach (var lastname in pnrFFCs.Where(p => p.RecordLocator == ffc.RecordLocator).Select(l => l.RecipientsLastName))
                {
                    if (!string.IsNullOrEmpty(lastname))
                    {
                        lookupByPinOrPNRResponse = await _sessionHelperService.GetSession<TCLookupByPinOrPNRResponse>(sessionid, typeof(TCLookupByPinOrPNRResponse).FullName + ffc.RecordLocator.ToUpper() + lastname.ToUpper()).ConfigureAwait(false);

                        if (lookupByPinOrPNRResponse != null && lookupByPinOrPNRResponse.FFCCertificates.CertificateList.Exists(c => c.TravelCreditList.Exists(tc => tc.OrigTicketNumber == ffc.PinCode)))
                            break;
                    }
                }
            }

            return lookupByPinOrPNRResponse;
        }

        private async Task BuildAncillaryFOPS(List<FormOfPayment> formOfPayment, MOBFormofPaymentDetails formofPaymentDetails, Session session, List<MOBProdDetail> products, List<MOBSHOPPrice> prices, MOBApplication application = null)
        {
            ApplyFFCToAncillary(products, application, formofPaymentDetails, prices, true);
            var ancillaryFFCs = formofPaymentDetails?.TravelFutureFlightCredit?.FutureFlightCredits.Where(ffc => ffc.TravelerNameIndex == "ANCILLARY").ToList();
            if (ancillaryFFCs?.Count > 0)
            {
                var cslPreloadResponse = await _sessionHelperService.GetSession<TCLookupByFreqFlyerNumWithEligibleResponse>(session.SessionId, typeof(TCLookupByFreqFlyerNumWithEligibleResponse).FullName, new List<string> { session.SessionId, typeof(TCLookupByFreqFlyerNumWithEligibleResponse).FullName }).ConfigureAwait(false);

                Collection<FFCRCertificate> cslReavelCredit = null;
                foreach (var ffc in ancillaryFFCs)
                {
                    var requestedCredit = formofPaymentDetails.TravelCreditDetails.TravelCredits
                                                           .FirstOrDefault(tc => tc.PinCode == ffc.PinCode && tc.RecordLocator == ffc.RecordLocator);
                    if (requestedCredit.IsLookupCredit)
                    {
                        var lookupByPinOrPNRResponse = await GetLookUpCSLResponse(formofPaymentDetails.TravelFutureFlightCredit.FutureFlightCredits, session.SessionId, ffc, requestedCredit.LastName);
                        cslReavelCredit = requestedCredit.IsOTFFFC ? lookupByPinOrPNRResponse.FFCCertificates?.CertificateList : lookupByPinOrPNRResponse.FFCRCertificates?.CertificateList;
                    }
                    else
                    {
                        cslReavelCredit = requestedCredit.IsOTFFFC ? cslPreloadResponse.FFCCertificates?.CertificateList : cslPreloadResponse.FFCRCertificates?.CertificateList;
                    }

                    var ffcrCertificate = cslReavelCredit.FirstOrDefault(c => c.TravelCreditList.Exists(t => (t.CertPin == ffc.PinCode || t.OrigTicketNumber == ffc.PinCode)));


                    var ffcFOPS = GetAncillaryFOP(products, ffc,
                                                            formofPaymentDetails,
                                                            session.CartId,
                                                            requestedCredit.IsOTFFFC,
                                                            ffcrCertificate,
                                                            requestedCredit.EligibleTravelerNameIndex[0]);
                    if (ffcFOPS.Any())
                    {
                        formOfPayment.AddRange(ffcFOPS);
                    }
                }
            }
        }

        private List<FormOfPayment> GetAncillaryFOP(List<MOBProdDetail> products, MOBFOPFutureFlightCredit ffc, MOBFormofPaymentDetails formofPaymentDetails, string cartid, bool isFFC, FFCRCertificate ffcrCertificate, string paxIndex)
        {
            List<FormOfPayment> ancillaryFops = null;
            var ancillaryElgibleAmount = ffc.RedeemAmount;
            foreach (var productCode in GetAncillaryProductCode(products))
            {
                var prod = products.FirstOrDefault(p => p.Code == productCode);
                if (prod != null)
                {
                    if (ancillaryFops == null)
                        ancillaryFops = new List<FormOfPayment>();

                    var appliedCreditToProduct = ancillaryFops.Where(p => p.PaymentTarget == prod.Code && p.Payment.Certificate != null).ToList();
                    double appliedCreditToProductAmount = 0;
                    if (appliedCreditToProduct != null && appliedCreditToProduct.Count > 0)
                    {
                        appliedCreditToProductAmount = appliedCreditToProduct.Sum(p => p.Payment.Certificate.Amount);
                    }

                    if (ancillaryElgibleAmount > 0 && appliedCreditToProductAmount < Convert.ToDouble(prod.ProdTotalPrice))
                    {
                        var fop = new FormOfPayment();
                        fop.Payment = new Service.Presentation.PaymentModel.FormOfPayment();
                        var ffcApplyingAmount = ancillaryElgibleAmount > (Convert.ToDouble(prod.ProdTotalPrice) - appliedCreditToProductAmount) ? (Convert.ToDouble(prod.ProdTotalPrice) - appliedCreditToProductAmount) : ancillaryElgibleAmount;
                        fop.Payment.Certificate = BuildFopCertificateForAncillary(formofPaymentDetails, cartid, ffc, isFFC, ffcrCertificate, paxIndex, ffcApplyingAmount);
                        fop.PaymentTarget = productCode;
                        fop.Amount = (decimal)ffcApplyingAmount;
                        ancillaryFops.Add(fop);
                        ancillaryElgibleAmount = ancillaryElgibleAmount - ffcApplyingAmount;
                    }
                }
            }

            return ancillaryFops;
        }

        private Certificate BuildFopCertificateForAncillary(MOBFormofPaymentDetails formofPaymentDetails, string cartId, MOBFOPFutureFlightCredit ffc, bool isFFC, FFCRCertificate ffcrCertificate, string paxIndex, double amount)
        {
            var cslTravelCredit = ffcrCertificate.TravelCreditList.FirstOrDefault(t => t.CertPin == ffc.PinCode || t.OrigTicketNumber == ffc.PinCode);

            Certificate certificate = new Certificate();
            certificate.Amount = amount;
            certificate.BillingAddress = GetCSLBillingAddress(formofPaymentDetails.BillingAddress);
            certificate.GroupID = cartId;
            certificate.OperationID = Guid.NewGuid().ToString();
            certificate.Type = new Genre();
            certificate.Type.Key = "FFCT";
            certificate.UsedValue = amount;
            certificate.CurrentValue = ffc.CurrentValue;
            certificate.IsNameMatchOverride = ffc.IsNameMatchWaiverApplied;

            certificate.Payor = new United.Service.Presentation.PersonModel.Person();
            certificate.Payor.GivenName = ffcrCertificate.FirstName;
            certificate.Payor.Surname = ffcrCertificate.LastName;
            certificate.Payor.Key = paxIndex;

            if (formofPaymentDetails.Email != null || formofPaymentDetails.Phone != null)
            {
                certificate.Payor.Contact = new United.Service.Presentation.PersonModel.Contact();
                if (formofPaymentDetails.Email != null)
                {
                    certificate.Payor.Contact.Emails = new Collection<EmailAddress>();
                    EmailAddress cslEmail = new EmailAddress();
                    cslEmail.Address = formofPaymentDetails.Email.EmailAddress;
                    certificate.Payor.Contact.Emails.Add(cslEmail);
                }
                if (formofPaymentDetails.Phone != null)
                {
                    certificate.Payor.Contact.PhoneNumbers = new Collection<United.Service.Presentation.CommonModel.Telephone>();
                    United.Service.Presentation.CommonModel.Telephone cslPhone = new United.Service.Presentation.CommonModel.Telephone();
                    cslPhone.PhoneNumber = formofPaymentDetails.Phone.AreaNumber + formofPaymentDetails.Phone.PhoneNumber;
                    cslPhone.CountryAccessCode = formofPaymentDetails.Phone.CountryCode;
                    certificate.Payor.Contact.PhoneNumbers.Add(cslPhone);
                }
            }

            certificate.ConfirmationId = ffc.RecordLocator;
            if (!string.IsNullOrEmpty(cslTravelCredit.CertificateNumber))
                certificate.Currency = new Currency() { Code = cslTravelCredit.CertificateNumber.Substring(cslTravelCredit.CertificateNumber.LastIndexOf('_') + 1) };
            else
                certificate.Currency = new Currency() { Code = "USD" };
            if (isFFC)
            {
                certificate.FutureFlightTraveler = new FutureFlightTraveler();
                certificate.FutureFlightTraveler.TravelerCredits = new Collection<FutureFlightTravelerCredit>();
                certificate.FutureFlightTraveler.FirstName = ffcrCertificate.FirstName;
                certificate.FutureFlightTraveler.LastName = ffcrCertificate.LastName;
                certificate.FutureFlightTraveler.DateOfBirth = ffcrCertificate.DateOfBirth;
                certificate.FutureFlightTraveler.Key = paxIndex;

                var futureFlightTravelerCredit = new FutureFlightTravelerCredit();
                futureFlightTravelerCredit.CreditAmount = new Charge();
                futureFlightTravelerCredit.CreditAmount.Amount = ffc.CurrentValue;
                futureFlightTravelerCredit.OfferKey = cslTravelCredit.CertificateNumber;
                futureFlightTravelerCredit.ExpirationDate = cslTravelCredit.CertExpDate;
                futureFlightTravelerCredit.TicketNumber = cslTravelCredit.OrigTicketNumber;
                certificate.FutureFlightTraveler.TravelerCredits.Add(futureFlightTravelerCredit);
            }
            else
            {
                certificate.PinCode = ffc.PinCode;
                certificate.PromoCode = ffc.PromoCode;
            }

            return certificate;
        }

        private List<string> GetAncillaryProductCode(List<MOBProdDetail> products)
        {
            string configCombinebilityETCAppliedAncillaryCodes = _configuration.GetValue<string>("TravelCreditEligibleProducts");

            string nonBundleProductCode = _configuration.GetValue<string>("NonBundleProductCode");
            List<MOBProdDetail> bundleProducts = products.FindAll(p => (nonBundleProductCode.IndexOf(p.Code) == -1) && !string.IsNullOrEmpty(p.ProdTotalPrice));
            if (bundleProducts != null && bundleProducts.Count > 0)
            {
                string bundleProductCodes = string.Join("|", bundleProducts.Select(p => p.Code));
                bundleProductCodes = bundleProductCodes.Trim('|');
                configCombinebilityETCAppliedAncillaryCodes += "|" + bundleProductCodes;
            }

            return configCombinebilityETCAppliedAncillaryCodes.Split('|').ToList();
        }

        private void ApplyFFCToAncillary(List<MOBProdDetail> products, MOBApplication application, MOBFormofPaymentDetails mobFormofPaymentDetails, List<MOBSHOPPrice> prices, bool isAncillaryON = false)
        {
            bool isAncillaryFFCEnable = (application == null ? isAncillaryON : IsInclueWithThisToggle(application.Id, application.Version.Major, "EnableTravelCreditAncillary", "AndroidTravelCreditVersionAncillary", "iPhoneTravelCreditVersionAncillary"));
            var futureFlightCredits = mobFormofPaymentDetails.TravelFutureFlightCredit?.FutureFlightCredits;
            if (isAncillaryFFCEnable && futureFlightCredits?.Count > 0)
            {
                mobFormofPaymentDetails.TravelFutureFlightCredit.AllowedFFCAmount = GetAllowedFFCAmount(products, isAncillaryFFCEnable);
                mobFormofPaymentDetails.TravelFutureFlightCredit.AllowedAncillaryAmount = GetAncillaryAmount(products, isAncillaryFFCEnable);

                var travelCredits = mobFormofPaymentDetails.TravelCreditDetails.TravelCredits.Where(tc => futureFlightCredits.Exists(ffc => ffc.PinCode == tc.PinCode)).ToList();
                int index = 0;
                foreach (var travelCredit in travelCredits)
                {
                    double ffcAppliedToAncillary = 0;
                    ffcAppliedToAncillary = futureFlightCredits.Where(ffc => ffc.TravelerNameIndex == "ANCILLARY").Sum(t => t.RedeemAmount);
                    ffcAppliedToAncillary = Math.Round(ffcAppliedToAncillary, 2, MidpointRounding.AwayFromZero);
                    var existedFFC = futureFlightCredits.FirstOrDefault(f => f.TravelerNameIndex == "ANCILLARY" && f.PinCode == travelCredit.PinCode);
                    double alreadyAppliedAmount = futureFlightCredits.Where(f => f.PinCode == travelCredit.PinCode).Sum(p => p.RedeemAmount);
                    var balanceAfterAppliedToRESAndAncillary = travelCredit.CurrentValue - alreadyAppliedAmount;

                    if (balanceAfterAppliedToRESAndAncillary > 0 &&
                        ffcAppliedToAncillary < mobFormofPaymentDetails.TravelFutureFlightCredit?.AllowedAncillaryAmount &&
                        existedFFC == null)
                    {
                        index++;
                        var mobFFC = new MOBFOPFutureFlightCredit();
                        mobFFC.CreditAmount = travelCredit.CreditAmount;
                        mobFFC.ExpiryDate = Convert.ToDateTime(travelCredit.ExpiryDate).ToString("MMMMM dd, yyyy");
                        mobFFC.IsCertificateApplied = true;
                        mobFFC.InitialValue = travelCredit.InitialValue;
                        mobFFC.Index = index;
                        mobFFC.PinCode = travelCredit.PinCode;
                        mobFFC.PromoCode = travelCredit.PromoCode;
                        mobFFC.RecordLocator = travelCredit.RecordLocator;
                        mobFFC.TravelerNameIndex = "ANCILLARY";
                        double remainingBalanceAfterAppliedFFC = mobFormofPaymentDetails.TravelFutureFlightCredit.AllowedAncillaryAmount - ffcAppliedToAncillary;
                        mobFFC.RedeemAmount = remainingBalanceAfterAppliedFFC > balanceAfterAppliedToRESAndAncillary ? balanceAfterAppliedToRESAndAncillary : remainingBalanceAfterAppliedFFC;
                        mobFFC.RedeemAmount = Math.Round(mobFFC.RedeemAmount, 2, MidpointRounding.AwayFromZero);
                        mobFFC.DisplayRedeemAmount = (mobFFC.RedeemAmount).ToString("C2", CultureInfo.CurrentCulture);
                        mobFFC.NewValueAfterRedeem = travelCredit.CurrentValue - (mobFFC.RedeemAmount + alreadyAppliedAmount);
                        mobFFC.NewValueAfterRedeem = Math.Round(mobFFC.NewValueAfterRedeem, 2, MidpointRounding.AwayFromZero);
                        mobFFC.DisplayNewValueAfterRedeem = (mobFFC.NewValueAfterRedeem).ToString("C2", CultureInfo.CurrentCulture);
                        mobFFC.IsCertificateApplied = true;
                        mobFFC.CurrentValue = travelCredit.CurrentValue;
                        futureFlightCredits.Add(mobFFC);
                    }
                    else if (existedFFC != null)
                    {
                        double remainingBalanceAfterAppliedFFC = (mobFormofPaymentDetails.TravelFutureFlightCredit.AllowedAncillaryAmount - ffcAppliedToAncillary) + existedFFC.RedeemAmount;
                        existedFFC.NewValueAfterRedeem += existedFFC.RedeemAmount;
                        existedFFC.RedeemAmount = 0;
                        existedFFC.RedeemAmount = remainingBalanceAfterAppliedFFC > existedFFC.NewValueAfterRedeem ? existedFFC.NewValueAfterRedeem : remainingBalanceAfterAppliedFFC;
                        existedFFC.RedeemAmount = Math.Round(existedFFC.RedeemAmount, 2, MidpointRounding.AwayFromZero);
                        existedFFC.DisplayRedeemAmount = (existedFFC.RedeemAmount).ToString("C2", CultureInfo.CurrentCulture);
                        existedFFC.NewValueAfterRedeem -= existedFFC.RedeemAmount;
                        existedFFC.NewValueAfterRedeem = Math.Round(existedFFC.NewValueAfterRedeem, 2, MidpointRounding.AwayFromZero);
                        existedFFC.DisplayNewValueAfterRedeem = (existedFFC.NewValueAfterRedeem).ToString("C2", CultureInfo.CurrentCulture);
                    }

                    futureFlightCredits.RemoveAll(f => f.RedeemAmount <= 0);
                    UpdatePricesInReservation(mobFormofPaymentDetails.TravelFutureFlightCredit, prices);
                    AssignIsOtherFOPRequired(mobFormofPaymentDetails, prices);
                    AssignFormOfPaymentType(mobFormofPaymentDetails, prices, false);
                }
            }
        }

        public void AssignFormOfPaymentType(MOBFormofPaymentDetails formofPaymentDetails, List<MOBSHOPPrice> prices, bool IsSecondaryFOP = false, bool isRemoveAll = false)
        {
            //need to update only when TravelFutureFlightCredit is added as formofpayment.          
            if (formofPaymentDetails?.TravelFutureFlightCredit?.FutureFlightCredits?.Count() > 0 || isRemoveAll)
            {
                if (!formofPaymentDetails.IsOtherFOPRequired && !IsSecondaryFOP)
                {
                    formofPaymentDetails.FormOfPaymentType = MOBFormofPayment.FFC.ToString();
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
        }

        private void AssignIsOtherFOPRequired(MOBFormofPaymentDetails formofPaymentDetails, List<MOBSHOPPrice> prices)
        {
            var grandTotalPrice = prices.FirstOrDefault(p => p.DisplayType.ToUpper().Equals("GRAND TOTAL"));
            if (grandTotalPrice != null)
                formofPaymentDetails.IsOtherFOPRequired = (grandTotalPrice.Value > 0);
        }

        private void UpdatePricesInReservation(MOBFOPTravelFutureFlightCredit travelFutureFlightCredit, List<MOBSHOPPrice> prices)
        {

            var ffcPrice = prices.FirstOrDefault(p => p.DisplayType.ToUpper() == "FFC");
            var totalCreditFFC = prices.FirstOrDefault(p => p.DisplayType.ToUpper() == "REFUNDPRICE");
            var grandtotal = prices.FirstOrDefault(p => p.DisplayType.ToUpper() == "GRAND TOTAL");

            if (ffcPrice == null && travelFutureFlightCredit.TotalRedeemAmount > 0)
            {
                ffcPrice = new MOBSHOPPrice();
                prices.Add(ffcPrice);
            }
            else if (ffcPrice != null)
            {
                UpdateCertificateRedeemAmountFromTotalInReserationPrices(grandtotal, ffcPrice.Value, false);
            }

            if (totalCreditFFC != null)
                prices.Remove(totalCreditFFC);

            if (travelFutureFlightCredit.TotalRedeemAmount > 0)
            {
                UpdateCertificatePrice(ffcPrice, travelFutureFlightCredit.TotalRedeemAmount, "FFC", "Future Flight Credit", isAddNegative: true);
                //Build Total Credit item
                double totalCreditValue = travelFutureFlightCredit.FutureFlightCredits.Sum(ffc => ffc.NewValueAfterRedeem);
                if (totalCreditValue > 0)
                {
                    totalCreditFFC = new MOBSHOPPrice();
                    prices.Add(totalCreditFFC);
                    UpdateCertificatePrice(totalCreditFFC, totalCreditValue, "REFUNDPRICE", "Total Credit", "RESIDUALCREDIT");
                }
                UpdateCertificateRedeemAmountFromTotalInReserationPrices(grandtotal, travelFutureFlightCredit.TotalRedeemAmount);
            }
            else
            {
                prices.Remove(ffcPrice);
            }
        }

        public void UpdateCertificateRedeemAmountFromTotalInReserationPrices(MOBSHOPPrice price, double value, bool isRemove = true)
        {
            if (price != null)
            {
                if (isRemove)
                {
                    price.Value -= value;
                }
                else
                {
                    price.Value += value;
                }
                price.Value = Math.Round(price.Value, 2, MidpointRounding.AwayFromZero);
                price.FormattedDisplayValue = (price.Value).ToString("C2", CultureInfo.CurrentCulture);
                price.DisplayValue = string.Format("{0:#,0.00}", price.Value);
            }
        }

        public MOBSHOPPrice UpdateCertificatePrice(MOBSHOPPrice ffc, double totalAmount, string priceType, string priceTypeDescription, string status = "", bool isAddNegative = false)
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

        private double GetAllowedFFCAmount(List<MOBProdDetail> products, bool isAncillaryFFCEnable = false)
        {
            string allowedFFCProducts = string.Empty;
            allowedFFCProducts = _configuration.GetValue<string>("FFCEligibleProductCodes");
            double allowedFFCAmount = products == null ? 0 : products.Where(p => (allowedFFCProducts.IndexOf(p.Code) > -1) && !string.IsNullOrEmpty(p.ProdTotalPrice)).Sum(a => Convert.ToDouble(a.ProdTotalPrice));
            allowedFFCAmount += GetAncillaryAmount(products, isAncillaryFFCEnable);
            allowedFFCAmount = Math.Round(allowedFFCAmount, 2, MidpointRounding.AwayFromZero);

            return allowedFFCAmount;
        }

        private double GetAncillaryAmount(List<MOBProdDetail> products, bool isAncillaryFFCEnable = false)
        {
            double allowedFFCAmount = 0;
            if (isAncillaryFFCEnable)
            {
                string allowedFFCProducts = _configuration.GetValue<string>("TravelCreditEligibleProducts");
                allowedFFCAmount += products == null ? 0 : products.Where(p => (allowedFFCProducts.IndexOf(p.Code) > -1) && !string.IsNullOrEmpty(p.ProdTotalPrice)).Sum(a => Convert.ToDouble(a.ProdTotalPrice));
                allowedFFCAmount += GetBundlesAmount(products);
                allowedFFCAmount = Math.Round(allowedFFCAmount, 2, MidpointRounding.AwayFromZero);
            }

            return allowedFFCAmount;
        }

        public async Task<Service.Presentation.PaymentModel.CreditCard> MapToCslCreditCard(MOBCheckOutRequest checkOutRequest, string currencyCode, Session session, string paymentTarget = "")
        {
            var cslCreditCard = new Service.Presentation.PaymentModel.CreditCard();
            cslCreditCard.Amount = Convert.ToDouble(checkOutRequest.PaymentAmount);
            cslCreditCard.Payor = new Service.Presentation.PersonModel.Person();


            var creditCard = checkOutRequest.FormofPaymentDetails?.FormOfPaymentType == MOBFormofPayment.Uplift.ToString() ? checkOutRequest.FormofPaymentDetails.Uplift : checkOutRequest.FormofPaymentDetails.CreditCard;

            if (!string.IsNullOrEmpty(creditCard.EncryptedCardNumber) && string.IsNullOrEmpty(creditCard.Key) && string.IsNullOrEmpty(creditCard.AccountNumberToken))
            {
                _logger.LogInformation("Checkout entered into block which is not expected  {sessionId}", checkOutRequest.SessionId);
                string ccDataVaultToken = string.Empty;
                var generateccTokenResult = await GenerateCCTokenWithDataVault(creditCard, checkOutRequest.SessionId, session.Token, checkOutRequest.Application, checkOutRequest.DeviceId, ccDataVaultToken);
                ccDataVaultToken = generateccTokenResult.CcDataVaultToken;
                if (generateccTokenResult.GeneratedCCTokenWithDataVault)
                {
                    cslCreditCard.ExpirationDate = string.Format("{0}/{1}", Convert.ToInt16(creditCard.ExpireMonth).ToString(), creditCard.ExpireYear.Substring(creditCard.ExpireYear.Length - 2));
                    cslCreditCard.Code = creditCard.CardType;
                    cslCreditCard.Name = creditCard.CCName;
                    cslCreditCard.Payor.GivenName = creditCard.CCName;
                    cslCreditCard.AccountNumberToken = ccDataVaultToken;
                    cslCreditCard.SecurityCode = creditCard.CIDCVV2;
                    cslCreditCard.Payor.Contact = new Service.Presentation.PersonModel.Contact();
                    cslCreditCard.Payor.Contact.Emails = new Collection<EmailAddress> { new EmailAddress { Address = checkOutRequest.FormofPaymentDetails.EmailAddress } };
                }
            }
            else
            {
                MOBCreditCard cc = new MOBCreditCard();
                if (_configuration.GetValue<bool>("EnableMOBILE25926Toggle") ? (string.IsNullOrEmpty(creditCard.AccountNumberToken) && string.IsNullOrEmpty(creditCard.PersistentToken)) : string.IsNullOrEmpty(creditCard.AccountNumberToken))
                {
                    MOBSHOPMakeReservationRequest makeReservationRequest = new MOBSHOPMakeReservationRequest();
                    if (makeReservationRequest.FormOfPayment == null)
                        makeReservationRequest.FormOfPayment = new MOBSHOPFormOfPayment();
                    makeReservationRequest.FormOfPayment.CreditCard = creditCard;
                    cc = await GetUnencryptedCardNumber(session.CartId, session.Token, makeReservationRequest, checkOutRequest.Flow);
                }
                else
                {
                    cc = creditCard;
                }
                cslCreditCard.AccountNumberToken = cc.AccountNumberToken; // CC Token fix - Venkat , Dec 15,2014

                MOBVormetricKeys vormetricKeys = null;
                if (checkOutRequest.Flow == FlowType.BOOKING.ToString() || checkOutRequest.Flow == FlowType.RESHOP.ToString() || UtilityHelper.IsCheckinFlow(checkOutRequest.Flow))
                    vormetricKeys = await GetVormetricPersistentTokenForBooking(creditCard, session.SessionId, session.Token);
                else if (checkOutRequest.Flow == FlowType.POSTBOOKING.ToString() || checkOutRequest.Flow == FlowType.VIEWRES.ToString() ||
                    (_configuration.GetValue<bool>("LoadVIewResVormetricForVIEWRES_SEATMAPFlowToggle") /*&& checkOutRequest.Flow == FlowType.MOBILECHECKOUT.ToString()*/))
                    vormetricKeys = await GetVormetricPersistentTokenForViewRes(creditCard, session.SessionId, session.Token, checkOutRequest.Flow);


                if (!string.IsNullOrEmpty(vormetricKeys.PersistentToken))
                {
                    cslCreditCard.PersistentToken = vormetricKeys.PersistentToken;
                    if (!string.IsNullOrEmpty(vormetricKeys.SecurityCodeToken))
                    {
                        cslCreditCard.SecurityCodeToken = vormetricKeys.SecurityCodeToken;
                        cslCreditCard.SecurityCode = vormetricKeys.SecurityCodeToken;
                    }
                    if (!string.IsNullOrEmpty(vormetricKeys.CardType) && string.IsNullOrEmpty(cslCreditCard.CardType))
                    {
                        cslCreditCard.CardType = vormetricKeys.CardType;
                    }
                    //Mobile AppMOBILE-1813 Booking – BE purchase – Choose OTP – CC payment
                    creditCard.PersistentToken = vormetricKeys.PersistentToken;
                    creditCard.SecurityCodeToken = vormetricKeys.SecurityCodeToken;
                    creditCard.CardType = vormetricKeys.CardType;
                }//cslCreditCard.AccountNumber = cc.UnencryptedCardNumber;// CC Token fix - Venkat , Dec 15,2014
                cslCreditCard.ExpirationDate = string.Format("{0}/{1}", Convert.ToInt16(cc.ExpireMonth).ToString(), cc.ExpireYear.Substring(cc.ExpireYear.Length - 2));
                cslCreditCard.Code = cc.CardType;
                cslCreditCard.Name = !string.IsNullOrEmpty(creditCard.CCName) ? creditCard.CCName : cc.CCName;
                cslCreditCard.Payor.GivenName = !string.IsNullOrEmpty(creditCard.CCName) ? creditCard.CCName : cc.CCName;
                cslCreditCard.Payor.Contact = new Service.Presentation.PersonModel.Contact();
                cslCreditCard.Payor.Contact.Emails = new Collection<EmailAddress> { new EmailAddress { Address = checkOutRequest.FormofPaymentDetails.EmailAddress } };
            }
            cslCreditCard.Currency = new Service.Presentation.CommonModel.Currency();
            cslCreditCard.Currency.Code = string.IsNullOrEmpty(currencyCode) ? "USD" : currencyCode;

            if (_configuration.GetValue<bool>("EDDtoEMDToggle"))
            {
                cslCreditCard.OperationID = Guid.NewGuid().ToString(); // This one we can pass the session id which we using in bookign path.
            }
            else
            {
                cslCreditCard.OperationID = Guid.NewGuid().ToString().ToUpper().Replace("-", ""); //As Per Aruna operation ID is a unique GUID generated per transactions so I assgined booking session ID as its unique per booking session - Venakt Dec 19 2014
            }

            if (checkOutRequest.FormofPaymentDetails.FormOfPaymentType == MOBFormofPayment.Uplift.ToString())
            {
                cslCreditCard.PaymentCharacteristics = new Collection<Characteristic> { new Characteristic { Code = "IsUplift", Value = "true" } };
            }

            if (checkOutRequest.FormofPaymentDetails.BillingAddress != null)
            {
                cslCreditCard.BillingAddress = new Service.Presentation.CommonModel.Address();
                cslCreditCard.BillingAddress.AddressLines = new Collection<string>();
                cslCreditCard.BillingAddress.AddressLines.Add(checkOutRequest.FormofPaymentDetails.BillingAddress.Line1.Length > 35 ? checkOutRequest.FormofPaymentDetails.BillingAddress.Line1.Substring(0, 35) : checkOutRequest.FormofPaymentDetails.BillingAddress.Line1);
                cslCreditCard.BillingAddress.City = checkOutRequest.FormofPaymentDetails.BillingAddress.City;
                cslCreditCard.BillingAddress.StateProvince = new Service.Presentation.CommonModel.StateProvince();
                cslCreditCard.BillingAddress.StateProvince.ShortName = checkOutRequest.FormofPaymentDetails.BillingAddress.State.Code;
                // Added as part of Bug 87614:Booking: PNR TAE field does not contain the State : Issuf
                cslCreditCard.BillingAddress.StateProvince.StateProvinceCode = checkOutRequest.FormofPaymentDetails.BillingAddress.State.Code;
                cslCreditCard.BillingAddress.PostalCode = checkOutRequest.FormofPaymentDetails.BillingAddress.PostalCode;
                cslCreditCard.BillingAddress.Country = new Service.Presentation.CommonModel.Country();
                cslCreditCard.BillingAddress.Country.CountryCode = checkOutRequest.FormofPaymentDetails.BillingAddress.Country.Code;
            }
            cslCreditCard.AccountNumberLastFourDigits = ConfigUtility.IsPOMOffer(paymentTarget) ? GetLast4DigitsOfCreditCard(creditCard) : null;
            return cslCreditCard;
        }
        private static string GetLast4DigitsOfCreditCard(MOBCreditCard creditCard)
        {
            string cardDisplayNumber = creditCard?.DisplayCardNumber;
            if (!string.IsNullOrEmpty(cardDisplayNumber) && cardDisplayNumber.Length > 3)
                return cardDisplayNumber.Substring(cardDisplayNumber.Length - 4);
            return string.Empty;
        }
        private async Task<MOBCreditCard> GetUnencryptedCardNumber(string cartId, string token, MOBSHOPMakeReservationRequest makeReservationRequest, string flow)
        {
            MOBCPProfileRequest request = new MOBCPProfileRequest();
            request.AccessCode = makeReservationRequest.AccessCode;
            request.Application = makeReservationRequest.Application;
            request.TransactionId = makeReservationRequest.TransactionId;
            request.LanguageCode = makeReservationRequest.LanguageCode;
            request.DeviceId = makeReservationRequest.DeviceId;
            request.ProfileOwnerOnly = true;
            request.IncludeCreditCards = true;
            request.SessionId = makeReservationRequest.SessionId;
            request.MileagePlusNumber = makeReservationRequest.MileagePlusNumber;
            request.CartId = cartId;
            request.Token = token;

            MOBCreditCard cc = await GetCreditCardWithKey(request, makeReservationRequest.FormOfPayment.CreditCard.Key, flow);
            if (cc == null)
            {
                throw new MOBUnitedException("The credit card with specified key was not found.");
            }
            else
                return cc;

        }

        private async Task<MOBCreditCard> GetCreditCardWithKey(MOBCPProfileRequest request, string creditCardKey, string flow)
        {
            if (ConfigUtility.IsViewResFlowCheckOut(flow) && _configuration.GetValue<bool>("EnableUCBPhase1_MobilePhase3Changes"))
            {
                _logger.LogWarning("Viewres -Checkout Account Number Token Empty {sessionId}", request.SessionId);
                await GetCreditCardWithKeyV2(request, creditCardKey);
            }
            MOBCreditCard creditCardDetails = new MOBCreditCard();
            if (request == null)
            {
                throw new MOBUnitedException("Profile request cannot be null.");
            }

            United.Services.Customer.Common.ProfileRequest profileRequest = GetProfileRequest(request);
            string jsonRequest = JsonConvert.SerializeObject(profileRequest);

            var jsonResponse = await _mSCCheckoutService.GetCustomerData<United.Services.Customer.Common.ProfileResponse>(request.Token, request.SessionId, jsonRequest);

            if (jsonResponse.response != null)
            {
                if (jsonResponse.response != null && jsonResponse.response.Status.Equals(United.Services.Customer.Common.Constants.StatusType.Success) && jsonResponse.response.Profiles != null)
                {
                    creditCardDetails = null;
                    // creditCardDetails = PopulateCreditCardDetails(request.SessionId, request.MileagePlusNumber, request.CustomerId, response.Profiles, creditCardKey, request);

                }
                else
                {
                    if (jsonResponse.response.Errors != null && jsonResponse.response.Errors.Count > 0)
                    {
                        string errorMessage = string.Empty;
                        foreach (var error in jsonResponse.response.Errors)
                        {
                            errorMessage = errorMessage + " " + error.UserFriendlyMessage;
                        }

                        throw new MOBUnitedException(errorMessage);
                    }
                    else
                    {
                        throw new MOBUnitedException("Unable to get Credit Card from profile.");//**// Get approved error message for this message.
                    }
                }
            }
            else
            {
                throw new MOBUnitedException("Unable to get Credit Card details from profile.");//**// Get approved error message for this message.
            }

            return creditCardDetails;
        }
        #region PopulateCreditCardDetails
        //private async Task<MOBCreditCard> PopulateCreditCardDetails(string sessionId, string mileagePlusNumber, int customerId, List<United.Services.Customer.Common.Profile> profiles, string creditCardKey, MOBCPProfileRequest request)
        //{
        //    MOBCreditCard creditCard = null;
        //    if (profiles != null && profiles.Count > 0)
        //    {
        //        foreach (var profile in profiles)
        //        {
        //            MOBCPProfile mobProfile = new MOBCPProfile();
        //            bool isProfileOwnerTSAFlagOn = false;
        //            List<MOBKVP> mpList = new List<MOBKVP>();
        //            var tupleRes = await PopulateTravelers(profile.Travelers, mileagePlusNumber, isProfileOwnerTSAFlagOn, true, request, sessionId);
        //            mobProfile.Travelers = tupleRes.mobTravelersOwnerFirstInList;
        //            isProfileOwnerTSAFlagOn = tupleRes.isProfileOwnerTSAFlagOn;
        //            mpList = tupleRes.savedTravelersMPList;
        //            foreach (MOBCPTraveler traveler in mobProfile.Travelers)
        //            {
        //                if (traveler.CreditCards != null && traveler.CreditCards.Count > 0)
        //                {
        //                    foreach (MOBCreditCard cc in traveler.CreditCards)
        //                    {
        //                        if (cc.Key.Trim() == creditCardKey.Trim())
        //                        {
        //                            creditCard = cc;
        //                            return cc;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return creditCard;
        //}

        //public async Task<(List<MOBCPTraveler> mobTravelersOwnerFirstInList, bool isProfileOwnerTSAFlagOn, List<MOBKVP> savedTravelersMPList)> PopulateTravelers(List<United.Services.Customer.Common.Traveler> travelers, string mileagePluNumber, bool isProfileOwnerTSAFlagOn, bool isGetCreditCardDetailsCall, MOBCPProfileRequest request, string sessionid, bool getMPSecurityDetails = false, string path = "")
        //{
        //    var savedTravelersMPList = new List<MOBKVP>();
        //    List<MOBCPTraveler> mobTravelers = null;
        //    List<MOBCPTraveler> mobTravelersOwnerFirstInList = null;
        //    MOBCPTraveler profileOwnerDetails = new MOBCPTraveler();
        //    United.Persist.Definition.Shopping.ShoppingResponse shop = new Persist.Definition.Shopping.ShoppingResponse();
        //    shop = await _sessionHelperService.GetSession<United.Persist.Definition.Shopping.ShoppingResponse>(request.SessionId, shop.ObjectName, new List<string> { request.SessionId, shop.ObjectName });
        //    var travlerersCountinBookingMain = shop.Request.TravelerTypes.Sum(a => a.Count);

        //    if (travelers != null && travelers.Count > 0)
        //    {
        //        mobTravelers = new List<MOBCPTraveler>();
        //        int i = 0;
        //        var persistedReservation = await PersistedReservation(request);

        //        foreach (United.Services.Customer.Common.Traveler traveler in travelers)
        //        {
        //            #region
        //            MOBCPTraveler mobTraveler = new MOBCPTraveler();
        //            mobTraveler.PaxIndex = i; i++;
        //            mobTraveler.CustomerId = traveler.CustomerId;
        //            if (_configuration.GetValue<bool>("NGRPAwardCalendarMP2017Switch"))
        //            {
        //                mobTraveler.CustomerMetrics = PopulateCustomerMetrics(traveler.CustomerMetrics);
        //            }
        //            if (traveler.BirthDate != null)
        //            {
        //                mobTraveler.BirthDate = traveler.BirthDate.GetValueOrDefault().ToString("MM/dd/yyyy");
        //            }
        //            if (_configuration.GetValue<bool>("EnableNationalityAndCountryOfResidence"))
        //            {
        //                if (persistedReservation != null && persistedReservation.ShopReservationInfo2 != null && persistedReservation.ShopReservationInfo2.InfoNationalityAndResidence != null
        //                    && persistedReservation.ShopReservationInfo2.InfoNationalityAndResidence.IsRequireNationalityAndResidence)
        //                {
        //                    if (string.IsNullOrEmpty(traveler.CountryOfResidence) || string.IsNullOrEmpty(traveler.Nationality))
        //                    {
        //                        mobTraveler.Message = _configuration.GetValue<string>("SavedTravelerInformationNeededMessage");
        //                    }
        //                }
        //                mobTraveler.Nationality = traveler.Nationality;
        //                mobTraveler.CountryOfResidence = traveler.CountryOfResidence;
        //            }
        //            if (travlerersCountinBookingMain > 1)
        //                mobTraveler.FirstName = traveler.FirstName;
        //            mobTraveler.GenderCode = traveler.GenderCode;
        //            mobTraveler.IsDeceased = traveler.IsDeceased;
        //            mobTraveler.IsExecutive = traveler.IsExecutive;
        //            mobTraveler.IsProfileOwner = traveler.IsProfileOwner;
        //            mobTraveler.Key = traveler.Key;
        //            mobTraveler.LastName = traveler.LastName;
        //            mobTraveler.MiddleName = traveler.MiddleName;
        //            mobTraveler.MileagePlus = PopulateMileagePlus(traveler.MileagePlus);
        //            if (mobTraveler.MileagePlus != null)
        //            {
        //                mobTraveler.MileagePlus.MpCustomerId = traveler.CustomerId;

        //                if (request != null && IncludeTravelBankFOP(request.Application.Id, request.Application.Version.Major))
        //                {
        //                    Session session = new Session();
        //                    string cslLoyaltryBalanceServiceResponse = await _loyaltyUCBService.GetLoyaltyBalance(request.Token, request.MileagePlusNumber, request.SessionId);
        //                    if (!string.IsNullOrEmpty(cslLoyaltryBalanceServiceResponse))
        //                    {
        //                        United.TravelBank.Model.BalancesDataModel.BalanceResponse PlusPointResponse = JsonConvert.DeserializeObject<United.TravelBank.Model.BalancesDataModel.BalanceResponse>(cslLoyaltryBalanceServiceResponse);
        //                        United.TravelBank.Model.BalancesDataModel.Balance tbbalance = PlusPointResponse.Balances.FirstOrDefault(tb => tb.ProgramCurrencyType == United.TravelBank.Model.TravelBankConstants.ProgramCurrencyType.UBC);
        //                        if (tbbalance != null && tbbalance.TotalBalance > 0)
        //                        {
        //                            mobTraveler.MileagePlus.TravelBankBalance = (double)tbbalance.TotalBalance;
        //                        }
        //                    }
        //                }
        //            }
        //            mobTraveler.OwnerFirstName = traveler.OwnerFirstName;
        //            mobTraveler.OwnerLastName = traveler.OwnerLastName;
        //            mobTraveler.OwnerMiddleName = traveler.OwnerMiddleName;
        //            mobTraveler.OwnerSuffix = traveler.OwnerSuffix;
        //            mobTraveler.OwnerTitle = traveler.OwnerTitle;
        //            mobTraveler.ProfileId = traveler.ProfileId;
        //            mobTraveler.ProfileOwnerId = traveler.ProfileOwnerId;
        //            bool isTSAFlagOn = false;
        //            if (traveler.SecureTravelers != null && traveler.SecureTravelers.Count > 0)
        //            {
        //                if (request == null)
        //                {
        //                    request = new MOBCPProfileRequest();
        //                    request.SessionId = string.Empty;
        //                    request.DeviceId = string.Empty;
        //                    request.Application = new MOBApplication() { Id = 0 };
        //                }
        //                else if (request.Application == null)
        //                {
        //                    request.Application = new MOBApplication() { Id = 0 };
        //                }
        //                mobTraveler.SecureTravelers = PopulatorSecureTravelers(traveler.SecureTravelers, ref isTSAFlagOn, i >= 2, request.SessionId, request.Application.Id, request.DeviceId);
        //                if (mobTraveler.SecureTravelers != null && mobTraveler.SecureTravelers.Count > 0)
        //                {
        //                    mobTraveler.RedressNumber = mobTraveler.SecureTravelers[0].RedressNumber;
        //                    mobTraveler.KnownTravelerNumber = mobTraveler.SecureTravelers[0].KnownTravelerNumber;
        //                }
        //            }
        //            mobTraveler.IsTSAFlagON = isTSAFlagOn;
        //            if (mobTraveler.IsProfileOwner)
        //            {
        //                isProfileOwnerTSAFlagOn = isTSAFlagOn;
        //            }
        //            mobTraveler.Suffix = traveler.Suffix;
        //            mobTraveler.Title = traveler.Title;
        //            mobTraveler.TravelerTypeCode = traveler.TravelerTypeCode;
        //            mobTraveler.TravelerTypeDescription = traveler.TravelerTypeDescription;
        //            //mobTraveler.PTCDescription = Utility.GetPaxDescription(traveler.TravelerTypeCode);
        //            if (persistedReservation != null && persistedReservation.ShopReservationInfo2 != null && persistedReservation.ShopReservationInfo2.TravelerTypes != null
        //                && persistedReservation.ShopReservationInfo2.TravelerTypes.Count > 0)
        //            {
        //                if (traveler.BirthDate != null)
        //                {
        //                    if (ConfigUtility.EnableYADesc() && persistedReservation.ShopReservationInfo2.IsYATravel)
        //                    {
        //                        mobTraveler.PTCDescription = GetYAPaxDescByDOB();
        //                    }
        //                    else
        //                    {
        //                        mobTraveler.PTCDescription = GeneralHelper.GetPaxDescriptionByDOB(traveler.BirthDate.ToString(), persistedReservation.Trips[0].FlattenedFlights[0].Flights[0].DepartDate);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                if (ConfigUtility.EnableYADesc() && persistedReservation != null && persistedReservation.ShopReservationInfo2 != null && persistedReservation.ShopReservationInfo2.IsYATravel)
        //                {
        //                    mobTraveler.PTCDescription = GetYAPaxDescByDOB();
        //                }
        //            }
        //            mobTraveler.TravelProgramMemberId = traveler.TravProgramMemberId;
        //            if (traveler != null)
        //            {
        //                if (traveler.MileagePlus != null)
        //                {
        //                    mobTraveler.CurrentEliteLevel = traveler.MileagePlus.CurrentEliteLevel;
        //                    //mobTraveler.AirRewardPrograms = GetTravelerLoyaltyProfile(traveler.AirPreferences, traveler.MileagePlus.CurrentEliteLevel);
        //                }
        //            }
        //            else if (_configuration.GetValue<bool>("BugFixToggleFor17M") && request != null && !string.IsNullOrEmpty(request.SessionId))
        //            {
        //                //    mobTraveler.CurrentEliteLevel = GetCurrentEliteLevel(mileagePluNumber);//**// Need to work on this with a test scenario with a Saved Traveler added MP Account with a Elite Status. Try to Add a saved traveler(with MP WX664656) to MP Account VW344781
        //                /// 195113 : Booking - Travel Options -mAPP: Booking: PA tile is displayed for purchase in Customize screen for Elite Premier member travelling and Login with General member
        //                /// Srini - 12/04/2017
        //                /// Calling getprofile for each traveler to get elite level for a traveler, who hav mp#
        //                mobTraveler.MileagePlus = await GetCurrentEliteLevelFromAirPreferences(traveler.AirPreferences, request.SessionId);
        //                if (mobTraveler != null)
        //                {
        //                    if (mobTraveler.MileagePlus != null)
        //                    {
        //                        mobTraveler.CurrentEliteLevel = mobTraveler.MileagePlus.CurrentEliteLevel;
        //                    }
        //                }
        //            }
        //            mobTraveler.AirRewardPrograms = GetTravelerLoyaltyProfile(traveler.AirPreferences, mobTraveler.CurrentEliteLevel);
        //            mobTraveler.Phones = PopulatePhones(traveler.Phones, true);

        //            if (mobTraveler.IsProfileOwner)
        //            {
        //                // These Phone and Email details for Makre Reseravation Phone and Email reason is mobTraveler.Phones = PopulatePhones(traveler.Phones,true) will get only day of travel contacts to register traveler & edit traveler.
        //                mobTraveler.ReservationPhones = PopulatePhones(traveler.Phones, false);
        //                mobTraveler.ReservationEmailAddresses = PopulateEmailAddresses(traveler.EmailAddresses, false);

        //                // Added by Hasnan - #53484. 10/04/2017
        //                // As per the Bug 53484:PINPWD: iOS and Android - Phone number is blank in RTI screen after booking from newly created account.
        //                // If mobTraveler.Phones is empty. Then it newly created account. Thus returning mobTraveler.ReservationPhones as mobTraveler.Phones.
        //                if (!_configuration.GetValue<bool>("EnableDayOfTravelEmail") || string.IsNullOrEmpty(path) || !path.ToUpper().Equals("BOOKING"))
        //                {
        //                    if (mobTraveler.Phones.Count == 0)
        //                    {
        //                        mobTraveler.Phones = mobTraveler.ReservationPhones;
        //                    }
        //                }
        //                #region Corporate Leisure(ProfileOwner must travel)//Client will use the IsMustRideTraveler flag to auto select the travel and not allow to uncheck the profileowner on the SelectTraveler Screen.
        //                if (_configuration.GetValue<bool>("EnableCorporateLeisure"))
        //                {
        //                    if (persistedReservation?.ShopReservationInfo2 != null && persistedReservation.ShopReservationInfo2.TravelType == TravelType.CLB.ToString() && IsCorporateLeisureFareSelected(persistedReservation.Trips))
        //                    {
        //                        mobTraveler.IsMustRideTraveler = true;
        //                    }
        //                }
        //                #endregion Corporate Leisure
        //            }
        //            if (mobTraveler.IsProfileOwner && request == null) //**PINPWD//mobTraveler.IsProfileOwner && request == null Means GetProfile and Populate is for MP PIN PWD Path
        //            {
        //                mobTraveler.ReservationEmailAddresses = PopulateAllEmailAddresses(traveler.EmailAddresses);
        //            }
        //            mobTraveler.AirPreferences = PopulateAirPrefrences(traveler.AirPreferences);
        //            if (request?.Application?.Version != null && string.IsNullOrEmpty(request?.Flow) && IsInternationalBillingAddress_CheckinFlowEnabled(request.Application))
        //            {
        //                try
        //                {
        //                    MOBShoppingCart mobShopCart = new MOBShoppingCart();
        //                    mobShopCart = await _sessionHelperService.GetSession<MOBShoppingCart>(request.SessionId, mobShopCart.ObjectName, new List<string> { request.SessionId, mobShopCart.ObjectName }).ConfigureAwait(false);
        //                    if (mobShopCart != null && !string.IsNullOrEmpty(mobShopCart.Flow) && mobShopCart.Flow == FlowType.CHECKIN.ToString())
        //                    {
        //                        request.Flow = mobShopCart.Flow;
        //                    }
        //                }
        //                catch { }
        //            }
        //            mobTraveler.Addresses = PopulateTravelerAddresses(traveler.Addresses, request?.Application, request?.Flow);

        //            if (_configuration.GetValue<bool>("EnableDayOfTravelEmail") && !string.IsNullOrEmpty(path) && path.ToUpper().Equals("BOOKING"))
        //            {
        //                mobTraveler.EmailAddresses = PopulateEmailAddresses(traveler.EmailAddresses, true);
        //            }
        //            else
        //            if (!getMPSecurityDetails)
        //            {
        //                mobTraveler.EmailAddresses = PopulateEmailAddresses(traveler.EmailAddresses, false);
        //            }
        //            else
        //            {
        //                mobTraveler.EmailAddresses = PopulateMPSecurityEmailAddresses(traveler.EmailAddresses);
        //            }
        //            mobTraveler.CreditCards = IsCorpBookingPath ? await PopulateCorporateCreditCards(traveler.CreditCards, isGetCreditCardDetailsCall, mobTraveler.Addresses, persistedReservation, sessionid) : await PopulateCreditCards(traveler.CreditCards, isGetCreditCardDetailsCall, mobTraveler.Addresses, sessionid);

        //            //if ((mobTraveler.IsTSAFlagON && string.IsNullOrEmpty(mobTraveler.Title)) || string.IsNullOrEmpty(mobTraveler.FirstName) || string.IsNullOrEmpty(mobTraveler.LastName) || string.IsNullOrEmpty(mobTraveler.GenderCode) || string.IsNullOrEmpty(mobTraveler.BirthDate)) //|| mobTraveler.Phones == null || (mobTraveler.Phones != null && mobTraveler.Phones.Count == 0)
        //            if (mobTraveler.IsTSAFlagON || string.IsNullOrEmpty(mobTraveler.FirstName) || string.IsNullOrEmpty(mobTraveler.LastName) || string.IsNullOrEmpty(mobTraveler.GenderCode) || string.IsNullOrEmpty(mobTraveler.BirthDate)) //|| mobTraveler.Phones == null || (mobTraveler.Phones != null && mobTraveler.Phones.Count == 0)
        //            {
        //                mobTraveler.Message = _configuration.GetValue<string>("SavedTravelerInformationNeededMessage");
        //            }
        //            if (mobTraveler.IsProfileOwner)
        //            {
        //                profileOwnerDetails = mobTraveler;
        //            }
        //            else
        //            {
        //                #region
        //                if (mobTraveler.AirRewardPrograms != null && mobTraveler.AirRewardPrograms.Count > 0)
        //                {
        //                    var airRewardProgramList = (from program in mobTraveler.AirRewardPrograms
        //                                                where program.CarrierCode.ToUpper().Trim() == "UA"
        //                                                select program).ToList();

        //                    if (airRewardProgramList != null && airRewardProgramList.Count > 0)
        //                    {
        //                        savedTravelersMPList.Add(new MOBKVP() { Key = mobTraveler.CustomerId.ToString(), Value = airRewardProgramList[0].MemberId });
        //                    }
        //                }
        //                #endregion
        //                mobTravelers.Add(mobTraveler);
        //            }
        //            #endregion
        //        }
        //    }
        //    mobTravelersOwnerFirstInList = new List<MOBCPTraveler>();
        //    mobTravelersOwnerFirstInList.Add(profileOwnerDetails);
        //    if (!IsCorpBookingPath || IsArrangerBooking)
        //    {
        //        mobTravelersOwnerFirstInList.AddRange(mobTravelers);
        //    }

        //    return (mobTravelersOwnerFirstInList, isGetCreditCardDetailsCall, savedTravelersMPList);
        //}
        //private List<MOBPrefAirPreference> PopulateAirPrefrences(List<United.Services.Customer.Common.AirPreference> airPreferences)
        //{
        //    List<MOBPrefAirPreference> mobAirPrefs = new List<MOBPrefAirPreference>();
        //    if (airPreferences != null && airPreferences.Count > 0)
        //    {
        //        foreach (United.Services.Customer.Common.AirPreference pref in airPreferences)
        //        {
        //            MOBPrefAirPreference mobAirPref = new MOBPrefAirPreference();
        //            mobAirPref.AirportCode = pref.AirportCode;
        //            mobAirPref.AirportCode = pref.AirportNameLong;
        //            mobAirPref.AirportNameShort = pref.AirportNameShort;
        //            mobAirPref.AirPreferenceId = pref.AirPreferenceId;
        //            mobAirPref.ClassDescription = pref.ClassDescription;
        //            mobAirPref.ClassId = pref.ClassId;
        //            mobAirPref.CustomerId = pref.CustomerId;
        //            mobAirPref.EquipmentCode = pref.EquipmentCode;
        //            mobAirPref.EquipmentDesc = pref.EquipmentDesc;
        //            mobAirPref.EquipmentId = pref.EquipmentId;
        //            mobAirPref.IsActive = pref.IsActive;
        //            mobAirPref.IsSelected = pref.IsSelected;
        //            mobAirPref.IsNew = pref.IsNew;
        //            mobAirPref.Key = pref.Key;
        //            mobAirPref.LanguageCode = pref.LanguageCode;
        //            mobAirPref.MealCode = pref.MealCode;
        //            mobAirPref.MealDescription = pref.MealDescription;
        //            mobAirPref.MealId = pref.MealId;
        //            mobAirPref.NumOfFlightsDisplay = pref.NumOfFlightsDisplay;
        //            mobAirPref.ProfileId = pref.ProfileId;
        //            mobAirPref.SearchPreferenceDescription = pref.SearchPreferenceDescription;
        //            mobAirPref.SearchPreferenceId = pref.SearchPreferenceId;
        //            mobAirPref.SeatFrontBack = pref.SeatFrontBack;
        //            mobAirPref.SeatSide = pref.SeatSide;
        //            mobAirPref.SeatSideDescription = pref.SeatSideDescription;
        //            mobAirPref.VendorCode = pref.VendorCode;
        //            mobAirPref.VendorDescription = pref.VendorDescription;
        //            mobAirPref.VendorId = pref.VendorId;
        //            mobAirPref.AirRewardPrograms = GetAirRewardPrograms(pref.AirRewardPrograms);
        //            mobAirPref.SpecialRequests = GetTravelerSpecialRequests(pref.SpecialRequests);
        //            mobAirPref.ServiceAnimals = GetTravelerServiceAnimals(pref.ServiceAnimals);

        //            mobAirPrefs.Add(mobAirPref);
        //        }
        //    }
        //    return mobAirPrefs;
        //}
        //private List<United.Definition.MOBPrefRewardProgram> GetAirRewardPrograms(List<United.Services.Customer.Common.RewardProgram> programs)
        //{
        //    List<MOBPrefRewardProgram> mobAirRewardsProgs = new List<MOBPrefRewardProgram>();
        //    if (programs != null && programs.Count > 0)
        //    {
        //        foreach (United.Services.Customer.Common.RewardProgram pref in programs)
        //        {
        //            MOBPrefRewardProgram mobAirRewardsProg = new MOBPrefRewardProgram();
        //            mobAirRewardsProg.CustomerId = Convert.ToInt32(pref.CustomerId);
        //            mobAirRewardsProg.ProfileId = Convert.ToInt32(pref.ProfileId);
        //            //mobAirRewardsProg.ProgramCode = pref.ProgramCode;
        //            //mobAirRewardsProg.ProgramDescription = pref.ProgramDescription;
        //            mobAirRewardsProg.ProgramMemberId = pref.ProgramMemberId;
        //            mobAirRewardsProg.VendorCode = pref.VendorCode;
        //            mobAirRewardsProg.VendorDescription = pref.VendorDescription;
        //            mobAirRewardsProgs.Add(mobAirRewardsProg);
        //        }
        //    }
        //    return mobAirRewardsProgs;
        //}
        //public List<MOBAddress> PopulateTravelerAddresses(List<United.Services.Customer.Common.Address> addresses, MOBApplication application = null, string flow = null)
        //{
        //    #region

        //    List<MOBAddress> mobAddresses = new List<MOBAddress>();
        //    if (addresses != null && addresses.Count > 0)
        //    {
        //        bool isCorpAddressPresent = false;
        //        if (_configuration.GetValue<bool>("CorporateConcurBooking") && IsCorpBookingPath)
        //        {
        //            //As per Business / DotCom Kalpen; we are removing the condition for checking the Effectivedate and Discontinued date
        //            var corpIndex = addresses.FindIndex(x => x.ChannelTypeDescription != null && x.ChannelTypeDescription.ToLower() == "corporate" && x.AddressLine1 != null && x.AddressLine1.Trim() != "");
        //            if (corpIndex >= 0)
        //                isCorpAddressPresent = true;

        //        }
        //        foreach (United.Services.Customer.Common.Address address in addresses)
        //        {
        //            if (_configuration.GetValue<bool>("CorporateConcurBooking"))
        //            {
        //                if (isCorpAddressPresent && address.ChannelTypeDescription.ToLower() == "corporate" &&
        //                    (_configuration.GetValue<bool>("USPOSCountryCodes_ByPass") || IsInternationalBilling(application, address.CountryCode, flow)))
        //                {
        //                    MOBAddress a = new MOBAddress();
        //                    a.Key = address.Key;
        //                    a.Channel = new MOBChannel();
        //                    a.Channel.ChannelCode = address.ChannelCode;
        //                    a.Channel.ChannelDescription = address.ChannelCodeDescription;
        //                    a.Channel.ChannelTypeCode = address.ChannelTypeCode.ToString();
        //                    a.Channel.ChannelTypeDescription = address.ChannelTypeDescription;
        //                    a.ApartmentNumber = address.AptNum;
        //                    a.Channel = new MOBChannel();
        //                    a.Channel.ChannelCode = address.ChannelCode;
        //                    a.Channel.ChannelDescription = address.ChannelCodeDescription;
        //                    a.Channel.ChannelTypeCode = address.ChannelTypeCode.ToString();
        //                    a.Channel.ChannelTypeDescription = address.ChannelTypeDescription;
        //                    a.City = address.City;
        //                    a.CompanyName = address.CompanyName;
        //                    a.Country = new MOBCountry();
        //                    a.Country.Code = address.CountryCode;
        //                    a.Country.Name = address.CountryName;
        //                    a.JobTitle = address.JobTitle;
        //                    a.Line1 = address.AddressLine1;
        //                    a.Line2 = address.AddressLine2;
        //                    a.Line3 = address.AddressLine3;
        //                    a.State = new MOBState();
        //                    a.State.Code = address.StateCode;
        //                    a.IsDefault = address.IsDefault;
        //                    a.IsPrivate = address.IsPrivate;
        //                    a.PostalCode = address.PostalCode;
        //                    if (address.ChannelTypeDescription.ToLower().Trim() == "corporate")
        //                    {
        //                        a.IsPrimary = true;
        //                        a.IsCorporate = true; // MakeIsCorporate true inorder to disable the edit on client
        //                    }
        //                    // Make IsPrimary true inorder to select the corpaddress by default

        //                    if (_configuration.GetValue<bool>("ShowTripInsuranceBookingSwitch"))
        //                    {
        //                        a.IsValidForTPIPurchase = IsValidAddressForTPIpayment(address.CountryCode);

        //                        if (a.IsValidForTPIPurchase)
        //                        {
        //                            a.IsValidForTPIPurchase = IsValidSateForTPIpayment(address.StateCode);
        //                        }
        //                    }
        //                    mobAddresses.Add(a);
        //                }
        //            }


        //            if (address.EffectiveDate <= DateTime.UtcNow && address.DiscontinuedDate >= DateTime.UtcNow)
        //            {
        //                if (_configuration.GetValue<bool>("USPOSCountryCodes_ByPass") || IsInternationalBilling(application, address.CountryCode, flow)) //##Kirti - allow only US addresses 
        //                {
        //                    MOBAddress a = new MOBAddress();
        //                    a.Key = address.Key;
        //                    a.Channel = new MOBChannel();
        //                    a.Channel.ChannelCode = address.ChannelCode;
        //                    a.Channel.ChannelDescription = address.ChannelCodeDescription;
        //                    a.Channel.ChannelTypeCode = address.ChannelTypeCode.ToString();
        //                    a.Channel.ChannelTypeDescription = address.ChannelTypeDescription;
        //                    a.ApartmentNumber = address.AptNum;
        //                    a.Channel = new MOBChannel();
        //                    a.Channel.ChannelCode = address.ChannelCode;
        //                    a.Channel.ChannelDescription = address.ChannelCodeDescription;
        //                    a.Channel.ChannelTypeCode = address.ChannelTypeCode.ToString();
        //                    a.Channel.ChannelTypeDescription = address.ChannelTypeDescription;
        //                    a.City = address.City;
        //                    a.CompanyName = address.CompanyName;
        //                    a.Country = new MOBCountry();
        //                    a.Country.Code = address.CountryCode;
        //                    a.Country.Name = address.CountryName;
        //                    a.JobTitle = address.JobTitle;
        //                    a.Line1 = address.AddressLine1;
        //                    a.Line2 = address.AddressLine2;
        //                    a.Line3 = address.AddressLine3;
        //                    a.State = new MOBState();
        //                    a.State.Code = address.StateCode;
        //                    //a.State.Name = address.StateName;
        //                    a.IsDefault = address.IsDefault;
        //                    a.IsPrimary = address.IsPrimary;
        //                    a.IsPrivate = address.IsPrivate;
        //                    a.PostalCode = address.PostalCode;
        //                    //Adding this check for corporate addresses to gray out the Edit button on the client
        //                    //if (address.ChannelTypeDescription.ToLower().Trim() == "corporate")
        //                    //{
        //                    //    a.IsCorporate = true;
        //                    //}
        //                    if (_configuration.GetValue<bool>("ShowTripInsuranceBookingSwitch"))
        //                    {
        //                        a.IsValidForTPIPurchase = IsValidAddressForTPIpayment(address.CountryCode);

        //                        if (a.IsValidForTPIPurchase)
        //                        {
        //                            a.IsValidForTPIPurchase = IsValidSateForTPIpayment(address.StateCode);
        //                        }
        //                    }
        //                    mobAddresses.Add(a);
        //                }
        //            }
        //        }
        //    }
        //    return mobAddresses;
        //    #endregion
        //}
        //private async Task<MOBCPMileagePlus> GetCurrentEliteLevelFromAirPreferences(List<United.Services.Customer.Common.AirPreference> airPreferences, string sessionid)
        //{
        //    MOBCPMileagePlus mobCPMileagePlus = null;
        //    if (_configuration.GetValue<bool>("BugFixToggleFor17M") &&
        //        airPreferences != null &&
        //        airPreferences.Count > 0 &&
        //        airPreferences[0].AirRewardPrograms != null &&
        //        airPreferences[0].AirRewardPrograms.Count > 0)
        //    {
        //        mobCPMileagePlus = await GetCurrentEliteLevelFromAirRewardProgram(airPreferences, sessionid);
        //    }

        //    return mobCPMileagePlus;
        //}
        //private async Task<MOBCPMileagePlus> GetCurrentEliteLevelFromAirRewardProgram(List<United.Services.Customer.Common.AirPreference> airPreferences, string sessionid)
        //{
        //    MOBCPMileagePlus mobCPMileagePlus = null;
        //    var airRewardProgram = airPreferences[0].AirRewardPrograms[0];
        //    if (!string.IsNullOrEmpty(airRewardProgram.ProgramMemberId))
        //    {
        //        Session session = new Session();
        //        session = await _sessionHelperService.GetSession<Session>(_headers.ContextValues.SessionId, session.ObjectName, new List<string>() { _headers.ContextValues.SessionId, session.ObjectName }).ConfigureAwait(false);

        //        MOBCPProfileRequest request = new MOBCPProfileRequest();
        //        request.CustomerId = 0;
        //        request.MileagePlusNumber = airRewardProgram.ProgramMemberId;
        //        United.Services.Customer.Common.ProfileRequest profileRequest = GetProfileRequest(request);
        //        string jsonRequest = JsonConvert.SerializeObject(profileRequest);
        //        string url = string.Format("/GetProfile");

        //        var jsonresponse = await MakeHTTPPostAndLogIt(session.SessionId, session.DeviceID, "GetProfileForTravelerToGetEliteLevel", session.AppID, string.Empty, session.Token, url, jsonRequest);
        //        mobCPMileagePlus = GetOwnerEliteLevelFromCslResponse(jsonresponse);
        //    }
        //    return mobCPMileagePlus;
        //}
        //private MOBCPMileagePlus GetOwnerEliteLevelFromCslResponse(string jsonresponse)
        //{
        //    MOBCPMileagePlus mobCPMileagePlus = null;
        //    if (!string.IsNullOrEmpty(jsonresponse))
        //    {
        //        United.Services.Customer.Common.ProfileResponse response = JsonConvert.DeserializeObject<United.Services.Customer.Common.ProfileResponse>(jsonresponse);
        //        if (response != null && response.Status.Equals(United.Services.Customer.Common.Constants.StatusType.Success) &&
        //            response.Profiles != null &&
        //            response.Profiles.Count > 0 &&
        //            response.Profiles[0].Travelers != null &&
        //            response.Profiles[0].Travelers.Exists(p => p.IsProfileOwner))
        //        {
        //            var owner = response.Profiles[0].Travelers.First(p => p.IsProfileOwner);
        //            if (owner != null & owner.MileagePlus != null)
        //            {
        //                mobCPMileagePlus = PopulateMileagePlus(owner.MileagePlus);
        //            }
        //        }
        //    }

        //    return mobCPMileagePlus;
        //}
        //private async Task<string> MakeHTTPPostAndLogIt(string sessionId, string deviceId, string action, int applicationId, string appVersion, string token, string url, string jsonRequest, bool isXMLRequest = false)
        //{
        //    ////logEntries.Add(LogEntry.GetLogEntry<string>(sessionId, action, "URL", applicationId, appVersion, deviceId, url, true, true));
        //    ////logEntries.Add(LogEntry.GetLogEntry<string>(sessionId, action, "Request", applicationId, appVersion, deviceId, jsonRequest, true, true));
        //    string jsonResponse = string.Empty;

        //    string paypalCSLCallDurations = string.Empty;
        //    string callTime4Tuning = string.Empty;

        //    #region//****Get Call Duration Code*******
        //    Stopwatch cslCallDurationstopwatch1;
        //    cslCallDurationstopwatch1 = new Stopwatch();
        //    cslCallDurationstopwatch1.Reset();
        //    cslCallDurationstopwatch1.Start();
        //    #endregion

        //    string applicationRequestType = isXMLRequest ? "xml" : "json";
        //    //jsonResponse = HttpHelper.Post(url, "application/" + applicationRequestType + "; charset=utf-8", token, jsonRequest, httpPostTimeOut, httpPostNumberOfRetry);
        //    var response = await _mSCCheckoutService.GetCustomerData<United.Services.Customer.Common.ProfileRequest>(token, sessionId, jsonRequest);
        //    #region
        //    if (cslCallDurationstopwatch1.IsRunning)
        //    {
        //        cslCallDurationstopwatch1.Stop();
        //    }
        //    paypalCSLCallDurations = paypalCSLCallDurations + "|2=" + cslCallDurationstopwatch1.ElapsedMilliseconds.ToString() + "|"; // 2 = shopCSLCallDurationstopwatch1
        //    callTime4Tuning = "|CSL =" + (cslCallDurationstopwatch1.ElapsedMilliseconds / (double)1000).ToString();
        //    #endregion
        //    //check
        //    return response.ToString();
        //}
        #endregion
        public async Task<MOBCreditCard> GetCreditCardWithKeyV2(MOBCPProfileRequest request, string creditCardKey)
        {
            MOBCreditCard creditCardDetails = new MOBCreditCard();
            if (request == null)
            {
                throw new MOBUnitedException("Profile request cannot be null.");
            }
            var response = await MakeProfileTravelerServiceCall(request);
            if (response != null && response.Data?.Travelers != null)
            {
                bool isProfileOwnerTSAFlagOn = false;
                List<MOBKVP> mpList = new List<MOBKVP>();
                var tupleRes = await PopulateTravelersV2(response.Data.Travelers, request.MileagePlusNumber, isProfileOwnerTSAFlagOn, false, request, request.SessionId);
                var travelers = tupleRes.mobTravelersOwnerFirstInList;
                isProfileOwnerTSAFlagOn = tupleRes.isProfileOwnerTSAFlagOn;
                mpList = tupleRes.savedTravelersMPList;
                if (travelers != null)
                {
                    return travelers[0].CreditCards.FirstOrDefault(creditCard => creditCard.Key.Trim() == creditCardKey.Trim());
                }
            }
            else
            {
                throw new MOBUnitedException("Unable to get Credit Card from profile.");
            }
            return null;
        }
        public async Task<CslResponse<TravelersProfileResponse>> MakeProfileTravelerServiceCall(MOBCPProfileRequest request)
        {
            var jsonResponse = await _customerProfileTravelerService.GetProfileTravelerInfo(request.Token, request.SessionId, request.MileagePlusNumber).ConfigureAwait(false);
            if (jsonResponse != null)
            {
                CslResponse<TravelersProfileResponse> travelersProfileResponse = JsonConvert.DeserializeObject<CslResponse<TravelersProfileResponse>>(jsonResponse);
                return travelersProfileResponse;
            }
            else
            {
                throw new Exception(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }
        }

        public async Task<(List<MOBCPTraveler> mobTravelersOwnerFirstInList, bool isProfileOwnerTSAFlagOn, List<MOBKVP> savedTravelersMPList)> PopulateTravelersV2(List<TravelerProfileResponse> travelers, string mileagePluNumber, bool isProfileOwnerTSAFlagOn, bool isGetCreditCardDetailsCall, MOBCPProfileRequest request, string sessionid, bool getMPSecurityDetails = false, string path = "")
        {
            var savedTravelersMPList = new List<MOBKVP>();
            List<MOBCPTraveler> mobTravelers = null;
            List<MOBCPTraveler> mobTravelersOwnerFirstInList = null;
            MOBCPTraveler profileOwnerDetails = new MOBCPTraveler();
            OwnerResponseModel profileOwnerResponse = new OwnerResponseModel();
            CorpProfileResponse corpProfileResponse = new CorpProfileResponse();
            if (travelers != null && travelers.Count > 0)
            {
                mobTravelers = new List<MOBCPTraveler>();
                int i = 0;
                var persistedReservation = !getMPSecurityDetails ? await PersistedReservation(request) : new Reservation();

                foreach (TravelerProfileResponse traveler in travelers)
                {
                    #region
                    MOBCPTraveler mobTraveler = new MOBCPTraveler();
                    mobTraveler.PaxIndex = i; i++;
                    mobTraveler.CustomerId = Convert.ToInt32(traveler.Profile?.CustomerId);
                    if (traveler.Profile?.ProfileOwnerIndicator == true)
                    {
                        profileOwnerResponse = await _sessionHelperService.GetSession<OwnerResponseModel>(request.SessionId, ObjectNames.CSLGetProfileOwnerResponse, new List<string> { request.SessionId, ObjectNames.CSLGetProfileOwnerResponse }).ConfigureAwait(false);
                        mobTraveler.CustomerMetrics = PopulateCustomerMetrics(profileOwnerResponse);
                        mobTraveler.MileagePlus = PopulateMileagePlusV2(profileOwnerResponse, request.MileagePlusNumber);
                        mobTraveler.IsDeceased = profileOwnerResponse?.MileagePlus?.Data?.IsDeceased == true;
                        mobTraveler.EmployeeId = traveler.Profile?.EmployeeId;

                    }
                    if (traveler.Profile?.BirthDate != null)
                    {
                        mobTraveler.BirthDate = traveler.Profile?.BirthDate.ToString("MM/dd/yyyy");
                        if (mobTraveler.BirthDate == "01/01/0001")
                            mobTraveler.BirthDate = null;
                    }
                    if (_configuration.GetValue<bool>("EnableNationalityAndCountryOfResidence"))
                    {
                        if (persistedReservation != null && persistedReservation.ShopReservationInfo2 != null && persistedReservation.ShopReservationInfo2.InfoNationalityAndResidence != null
                            && persistedReservation.ShopReservationInfo2.InfoNationalityAndResidence.IsRequireNationalityAndResidence)
                        {
                            if (string.IsNullOrEmpty(traveler.CustomerAttributes?.CountryofResidence) || string.IsNullOrEmpty(traveler.CustomerAttributes?.Nationality))
                            {
                                mobTraveler.Message = _configuration.GetValue<string>("SavedTravelerInformationNeededMessage");
                            }
                        }
                        mobTraveler.Nationality = traveler.CustomerAttributes?.Nationality;
                        mobTraveler.CountryOfResidence = traveler.CustomerAttributes?.CountryofResidence;
                    }

                    mobTraveler.FirstName = traveler.Profile.FirstName;
                    mobTraveler.GenderCode = traveler.Profile?.Gender.ToString() == "Undefined" ? "" : traveler.Profile.Gender.ToString();
                    mobTraveler.IsProfileOwner = traveler.Profile.ProfileOwnerIndicator;
                    mobTraveler.Key = traveler.Profile.TravelerKey;
                    mobTraveler.LastName = traveler.Profile.LastName;
                    mobTraveler.MiddleName = traveler.Profile.MiddleName;

                    if (mobTraveler.MileagePlus != null)
                    {
                        mobTraveler.MileagePlus.MpCustomerId = Convert.ToInt32(traveler.Profile.CustomerId);

                        if (!getMPSecurityDetails && request != null && IncludeTravelBankFOP(request.Application.Id, request.Application.Version.Major))
                        {
                            United.Persist.Definition.Shopping.Session session = await _sessionHelperService.GetSession<Session>(request.SessionId, new Session().ObjectName, new List<string> { request.SessionId, new Session().ObjectName }).ConfigureAwait(false);
                            string cslLoyaltryBalanceServiceResponse = await _loyaltyUCBService.GetLoyaltyBalance(session.Token, mobTraveler.MileagePlus.MileagePlusId, session.SessionId);
                            if (!string.IsNullOrEmpty(cslLoyaltryBalanceServiceResponse))
                            {
                                United.TravelBank.Model.BalancesDataModel.BalanceResponse PlusPointResponse = JsonConvert.DeserializeObject<BalanceResponse>(cslLoyaltryBalanceServiceResponse);
                                United.TravelBank.Model.BalancesDataModel.Balance tbbalance = PlusPointResponse.Balances.FirstOrDefault(tb => tb.ProgramCurrencyType == United.TravelBank.Model.TravelBankConstants.ProgramCurrencyType.UBC);
                                if (tbbalance != null && tbbalance.TotalBalance > 0)
                                {
                                    mobTraveler.MileagePlus.TravelBankBalance = (double)tbbalance.TotalBalance;
                                }
                            }
                        }
                    }

                    mobTraveler.ProfileId = Convert.ToInt32(traveler.Profile.ProfileId);
                    mobTraveler.ProfileOwnerId = Convert.ToInt32(traveler.Profile.ProfileOwnerId);
                    bool isTSAFlagOn = false;
                    if (traveler.SecureTravelers != null)
                    {
                        if (request == null)
                        {
                            request = new MOBCPProfileRequest();
                            request.SessionId = string.Empty;
                            request.DeviceId = string.Empty;
                            request.Application = new MOBApplication() { Id = 0 };
                        }
                        else if (request.Application == null)
                        {
                            request.Application = new MOBApplication() { Id = 0 };
                        }
                        mobTraveler.SecureTravelers = PopulatorSecureTravelersV2(traveler.SecureTravelers, ref isTSAFlagOn, i >= 2, request.SessionId, request.Application.Id, request.DeviceId);
                        if (mobTraveler.SecureTravelers != null && mobTraveler.SecureTravelers.Count > 0)
                        {
                            mobTraveler.RedressNumber = mobTraveler.SecureTravelers[0].RedressNumber;
                            mobTraveler.KnownTravelerNumber = mobTraveler.SecureTravelers[0].KnownTravelerNumber;
                        }
                    }
                    mobTraveler.IsTSAFlagON = isTSAFlagOn;
                    if (mobTraveler.IsProfileOwner)
                    {
                        isProfileOwnerTSAFlagOn = isTSAFlagOn;
                    }
                    mobTraveler.Suffix = traveler.Profile.Suffix;
                    mobTraveler.Title = traveler.Profile.Title;
                    mobTraveler.TravelerTypeCode = traveler.Profile?.TravelerTypeCode;
                    mobTraveler.TravelerTypeDescription = traveler.Profile?.TravelerTypeDescription;
                    if (persistedReservation != null && persistedReservation.ShopReservationInfo2 != null && persistedReservation.ShopReservationInfo2.TravelerTypes != null
                        && persistedReservation.ShopReservationInfo2.TravelerTypes.Count > 0)
                    {
                        if (traveler.Profile?.BirthDate != null)
                        {
                            if (ConfigUtility.EnableYADesc() && persistedReservation.ShopReservationInfo2.IsYATravel)
                            {
                                mobTraveler.PTCDescription = GetYAPaxDescByDOB();
                            }
                            else
                            {
                                mobTraveler.PTCDescription = GeneralHelper.GetPaxDescriptionByDOB(traveler.Profile.BirthDate.ToString(), persistedReservation.Trips[0].FlattenedFlights[0].Flights[0].DepartDate);
                            }
                        }
                    }
                    else
                    {
                        if (ConfigUtility.EnableYADesc() && persistedReservation != null && persistedReservation.ShopReservationInfo2 != null && persistedReservation.ShopReservationInfo2.IsYATravel)
                        {
                            mobTraveler.PTCDescription = GetYAPaxDescByDOB();
                        }
                    }
                    //  mobTraveler.TravelProgramMemberId = traveler.Profile.TravProgramMemberId; No longer needed confirmed from service
                    if (traveler != null)
                    {
                        if (mobTraveler.MileagePlus != null)
                        {
                            mobTraveler.CurrentEliteLevel = mobTraveler.MileagePlus.CurrentEliteLevel;
                            //mobTraveler.AirRewardPrograms = GetTravelerLoyaltyProfile(traveler.AirPreferences, traveler.MileagePlus.CurrentEliteLevel);
                        }
                    }

                    mobTraveler.AirRewardPrograms = GetTravelerRewardPgrograms(traveler.RewardPrograms, mobTraveler.CurrentEliteLevel);
                    mobTraveler.Phones = PopulatePhonesV2(traveler, true);
                    if (mobTraveler.IsProfileOwner)
                    {
                        // These Phone and Email details for Makre Reseravation Phone and Email reason is mobTraveler.Phones = PopulatePhones(traveler.Phones,true) will get only day of travel contacts to register traveler & edit traveler.
                        mobTraveler.ReservationPhones = PopulatePhonesV2(traveler, false);
                        mobTraveler.ReservationEmailAddresses = PopulateEmailAddressesV2(traveler.Emails, false);

                        // Added by Hasnan - #53484. 10/04/2017
                        // As per the Bug 53484:PINPWD: iOS and Android - Phone number is blank in RTI screen after booking from newly created account.
                        // If mobTraveler.Phones is empty. Then it newly created account. Thus returning mobTraveler.ReservationPhones as mobTraveler.Phones.
                        if (_configuration.GetValue<bool>("EnableDayOfTravelEmail") || string.IsNullOrEmpty(path) || !path.ToUpper().Equals("BOOKING"))
                        {
                            if (mobTraveler.Phones.Count == 0)
                            {
                                mobTraveler.Phones = mobTraveler.ReservationPhones;
                            }
                        }
                        #region Corporate Leisure(ProfileOwner must travel)//Client will use the IsMustRideTraveler flag to auto select the travel and not allow to uncheck the profileowner on the SelectTraveler Screen.
                        if (_configuration.GetValue<bool>("EnableCorporateLeisure"))
                        {
                            if (persistedReservation?.ShopReservationInfo2 != null && persistedReservation.ShopReservationInfo2.TravelType == TravelType.CLB.ToString() && IsCorporateLeisureFareSelected(persistedReservation.Trips))
                            {
                                mobTraveler.IsMustRideTraveler = true;
                            }
                        }
                        #endregion Corporate Leisure
                    }
                    if (mobTraveler.IsProfileOwner && getMPSecurityDetails) //**PINPWD//mobTraveler.IsProfileOwner && request == null Means GetProfile and Populate is for MP PIN PWD Path
                    {
                        mobTraveler.ReservationEmailAddresses = PopulateAllEmailAddressesV2(traveler.Emails);
                    }
                    mobTraveler.AirPreferences = PopulateAirPrefrencesV2(traveler);
                    if (!getMPSecurityDetails && request?.Application?.Version != null && string.IsNullOrEmpty(request?.Flow) && IsInternationalBillingAddress_CheckinFlowEnabled(request.Application))
                    {
                        try
                        {
                            var mobShopCart = await _sessionHelperService.GetSession<United.Definition.MOBShoppingCart>(request.SessionId, typeof(United.Definition.MOBShoppingCart).FullName, new List<string> { request.SessionId, typeof(United.Definition.MOBShoppingCart).FullName }).ConfigureAwait(false);
                            if (mobShopCart != null && !string.IsNullOrEmpty(mobShopCart.Flow) && mobShopCart.Flow == FlowType.CHECKIN.ToString())
                            {
                                request.Flow = mobShopCart.Flow;
                            }
                        }
                        catch { }
                    }
                    mobTraveler.Addresses = PopulateTravelerAddressesV2(traveler.Addresses, request?.Application, request?.Flow);

                    if (_configuration.GetValue<bool>("EnableDayOfTravelEmail") && !string.IsNullOrEmpty(path) && path.ToUpper().Equals("BOOKING"))
                    {
                        mobTraveler.EmailAddresses = PopulateEmailAddressesV2(traveler.Emails, true);
                    }
                    else
                    if (!getMPSecurityDetails)
                    {
                        mobTraveler.EmailAddresses = PopulateEmailAddressesV2(traveler.Emails, false);
                    }
                    else
                    {
                        mobTraveler.EmailAddresses = PopulateMPSecurityEmailAddressesV2(traveler.Emails);
                    }
                    if (mobTraveler.IsProfileOwner == true)
                    {
                        if (!getMPSecurityDetails)
                        {
                            bool isCardMandatory = false;
                            var corpCreditCards = new List<MOBCreditCard>();
                            if (mobTraveler.CreditCards == null)
                            {
                                mobTraveler.CreditCards = new List<MOBCreditCard>();
                            }

                            if (IsCorpBookingPath)
                            {
                                bool isEnableU4BCorporateBooking = request != null && request.Application != null ? ConfigUtility.IsEnableU4BCorporateBooking(request.Application.Id, request.Application.Version?.Major) : false;
                                string sessionId = isEnableU4BCorporateBooking ? request.DeviceId + request.MileagePlusNumber : request.SessionId;

                                corpProfileResponse = await _sessionHelperService.GetSession<United.CorporateDirect.Models.CustomerProfile.CorpProfileResponse>(request.SessionId, ObjectNames.CSLCorpProfileResponse, new List<string> { request.SessionId, ObjectNames.CSLCorpProfileResponse }).ConfigureAwait(false);
                                corpCreditCards = await PopulateCorporateCreditCards(isGetCreditCardDetailsCall, mobTraveler.Addresses, persistedReservation, request);
                                if (corpCreditCards != null && corpCreditCards.Any(s => s.IsMandatory == true))
                                {
                                    isCardMandatory = true;
                                    mobTraveler.CreditCards = corpCreditCards;
                                }
                            }
                            if (!isCardMandatory)
                            {
                                mobTraveler.CreditCards = await PopulateCreditCards(isGetCreditCardDetailsCall, mobTraveler.Addresses, request);
                                if (corpCreditCards != null && corpCreditCards.Count() > 0)
                                {
                                    mobTraveler.CreditCards.AddRange(corpCreditCards);
                                }
                            }
                        }
                        if (IsCorpBookingPath && corpProfileResponse?.Profiles != null && corpProfileResponse.Profiles.Count() > 0)
                        {
                            var corporateTraveler = corpProfileResponse?.Profiles[0].Travelers.FirstOrDefault();
                            if (corporateTraveler != null)
                            {
                                if (corporateTraveler.Addresses != null)
                                {
                                    var corporateAddress = PopulateCorporateTravelerAddresses(corporateTraveler.Addresses, request.Application, request.Flow);
                                    if (mobTraveler.Addresses == null)
                                        mobTraveler.Addresses = new List<MOBAddress>();
                                    mobTraveler.Addresses.AddRange(corporateAddress);
                                }
                                if (corporateTraveler.EmailAddresses != null)
                                {
                                    var corporateEmailAddresses = PopulateCorporateEmailAddresses(corporateTraveler.EmailAddresses, false);
                                    mobTraveler.ReservationEmailAddresses = new List<MOBEmail>();
                                    mobTraveler.ReservationEmailAddresses.AddRange(corporateEmailAddresses);
                                }
                                if (corporateTraveler.Phones != null)
                                {
                                    var corporatePhones = PopulateCorporatePhones(corporateTraveler.Phones, false);
                                    mobTraveler.ReservationPhones = new List<MOBCPPhone>();
                                    mobTraveler.ReservationPhones.AddRange(corporatePhones);
                                }
                                if (corporateTraveler.AirPreferences != null)
                                {
                                    var corporateAirpreferences = PopulateCorporateAirPrefrences(corporateTraveler.AirPreferences);
                                    if (mobTraveler.AirPreferences == null)
                                        mobTraveler.AirPreferences = new List<MOBPrefAirPreference>();
                                    mobTraveler.AirPreferences.AddRange(corporateAirpreferences);
                                }
                            }

                        }

                    }
                    if (mobTraveler.IsTSAFlagON || string.IsNullOrEmpty(mobTraveler.FirstName) || string.IsNullOrEmpty(mobTraveler.LastName) || string.IsNullOrEmpty(mobTraveler.GenderCode) || string.IsNullOrEmpty(mobTraveler.BirthDate)) //|| mobTraveler.Phones == null || (mobTraveler.Phones != null && mobTraveler.Phones.Count == 0)
                    {
                        mobTraveler.Message = _configuration.GetValue<string>("SavedTravelerInformationNeededMessage");
                    }
                    if (mobTraveler.IsProfileOwner)
                    {
                        profileOwnerDetails = mobTraveler;
                    }
                    else
                    {
                        #region
                        if (mobTraveler.AirRewardPrograms != null && mobTraveler.AirRewardPrograms.Count > 0)
                        {
                            var airRewardProgramList = (from program in mobTraveler.AirRewardPrograms
                                                        where program.CarrierCode.ToUpper().Trim() == "UA"
                                                        select program).ToList();

                            if (airRewardProgramList != null && airRewardProgramList.Count > 0)
                            {
                                savedTravelersMPList.Add(new MOBKVP() { Key = mobTraveler.CustomerId.ToString(), Value = airRewardProgramList[0].MemberId });
                            }
                        }
                        #endregion
                        mobTravelers.Add(mobTraveler);
                    }
                    #endregion
                }
            }
            mobTravelersOwnerFirstInList = new List<MOBCPTraveler>();
            mobTravelersOwnerFirstInList.Add(profileOwnerDetails);
            if (!IsCorpBookingPath || IsArrangerBooking)
            {
                mobTravelersOwnerFirstInList.AddRange(mobTravelers);
            }

            return (mobTravelersOwnerFirstInList, isProfileOwnerTSAFlagOn, savedTravelersMPList);
        }

        private List<MOBPrefAirPreference> PopulateCorporateAirPrefrences(List<United.CorporateDirect.Models.CustomerProfile.AirPreference> airPreferences)
        {
            List<MOBPrefAirPreference> mobAirPrefs = new List<MOBPrefAirPreference>();
            if (airPreferences != null && airPreferences.Count > 0)
            {
                foreach (United.CorporateDirect.Models.CustomerProfile.AirPreference pref in airPreferences)
                {
                    MOBPrefAirPreference mobAirPref = new MOBPrefAirPreference();
                    mobAirPref.MealCode = pref.MealCode;
                    mobAirPrefs.Add(mobAirPref);
                }
            }
            return mobAirPrefs;
        }
        public bool IsCorporateLeisureFareSelected(List<MOBSHOPTrip> trips)
        {
            string corporateFareText = _configuration.GetValue<string>("FSRLabelForCorporateLeisure") ?? string.Empty;
            if (trips != null)
            {
                return trips.Any(
                   x =>
                       x.FlattenedFlights.Any(
                           f =>
                               f.Flights.Any(
                                   fl =>
                                       fl.CorporateFareIndicator ==
                                       corporateFareText.ToString())));
            }

            return false;
        }
        private List<MOBCPPhone> PopulateCorporatePhones(List<United.CorporateDirect.Models.CustomerProfile.Phone> phones, bool onlyDayOfTravelContact)
        {
            List<MOBCPPhone> mobCPPhones = new List<MOBCPPhone>();
            bool isCorpPhonePresent = false;


            if (_configuration.GetValue<bool>("CorporateConcurBooking") && IsCorpBookingPath)
            {
                var corpIndex = phones.FindIndex(x => x.ChannelTypeDescription != null && x.ChannelTypeDescription.ToLower() == "corporate" && x.PhoneNumber != null && x.PhoneNumber != "");
                if (corpIndex >= 0)
                    isCorpPhonePresent = true;
            }


            if (phones != null && phones.Count > 0)
            {
                MOBCPPhone primaryMobCPPhone = null;
                int co = 0;
                foreach (United.CorporateDirect.Models.CustomerProfile.Phone phone in phones)
                {
                    #region As per Wade Change want to filter out to return only Primary Phone to client if not primary phone exist return the first one from the list.So lot of work at client side will save time. Sep 15th 2014
                    MOBCPPhone mobCPPhone = new MOBCPPhone();
                    co = co + 1;
                    mobCPPhone.PhoneNumber = phone.PhoneNumber;
                    mobCPPhone.ChannelCode = phone.ChannelCode;
                    mobCPPhone.ChannelCodeDescription = phone.ChannelCodeDescription;
                    mobCPPhone.ChannelTypeCode = Convert.ToString(phone.ChannelTypeCode);
                    mobCPPhone.ChannelTypeDescription = phone.ChannelTypeDescription;
                    mobCPPhone.ChannelTypeDescription = phone.ChannelTypeDescription;
                    mobCPPhone.ChannelTypeSeqNumber = 0;
                    mobCPPhone.CountryCode = phone.CountryCode;
                    mobCPPhone.IsProfileOwner = phone.IsProfileOwner;
                    if (phone.PhoneDevices != null && phone.PhoneDevices.Count > 0)
                    {
                        mobCPPhone.DeviceTypeCode = phone.PhoneDevices[0].CommDeviceTypeCode;
                    }


                    if (_configuration.GetValue<bool>("CorporateConcurBooking"))
                    {
                        #region
                        if (IsCorpBookingPath && isCorpPhonePresent && !onlyDayOfTravelContact && phone.ChannelTypeDescription.ToLower() == "corporate")
                        {
                            //return the corporate phone number
                            primaryMobCPPhone = new MOBCPPhone();
                            mobCPPhone.IsPrimary = true;
                            primaryMobCPPhone = mobCPPhone;
                            break;

                        }
                        if (IsCorpBookingPath && isCorpPhonePresent && !onlyDayOfTravelContact && phone.ChannelTypeDescription.ToLower() != "corporate")
                        {
                            //There is corporate phone number present, continue till corporate phone number is found
                            continue;
                        }
                        #endregion
                    }

                    if (!onlyDayOfTravelContact)
                    {
                        if (co == 1)
                        {
                            primaryMobCPPhone = new MOBCPPhone();
                            primaryMobCPPhone = mobCPPhone;
                        }
                    }
                    #endregion
                }
                if (primaryMobCPPhone != null)
                {
                    mobCPPhones.Add(primaryMobCPPhone);
                }
            }
            return mobCPPhones;
        }

        private List<MOBEmail> PopulateCorporateEmailAddresses(List<United.CorporateDirect.Models.CustomerProfile.Email> emailAddresses, bool onlyDayOfTravelContact)
        {
            #region
            List<MOBEmail> mobEmailAddresses = new List<MOBEmail>();
            bool isCorpEmailPresent = false;

            if (_configuration.GetValue<bool>("CorporateConcurBooking") && IsCorpBookingPath)
            {
                //As per Business / DotCom Kalpen; we are removing the condition for checking the Effectivedate and Discontinued date
                var corpIndex = emailAddresses.FindIndex(x => x.ChannelTypeDescription != null && x.ChannelTypeDescription.ToLower() == "corporate" && x.EmailAddress != null && x.EmailAddress.Trim() != "");
                if (corpIndex >= 0)
                    isCorpEmailPresent = true;

            }

            if (emailAddresses != null && emailAddresses.Count > 0)
            {
                MOBEmail primaryEmailAddress = null;
                int co = 0;
                foreach (United.CorporateDirect.Models.CustomerProfile.Email email in emailAddresses)
                {
                    if (_configuration.GetValue<bool>("CorporateConcurBooking"))
                    {
                        if (isCorpEmailPresent && !onlyDayOfTravelContact && email.ChannelTypeDescription.ToLower() == "corporate")
                        {
                            primaryEmailAddress = new MOBEmail();
                            primaryEmailAddress.Channel = new MOBChannel();
                            primaryEmailAddress.EmailAddress = email.EmailAddress;
                            primaryEmailAddress.Channel.ChannelCode = email.ChannelCode;
                            primaryEmailAddress.Channel.ChannelDescription = email.ChannelCodeDescription;
                            primaryEmailAddress.Channel.ChannelTypeCode = email.ChannelTypeCode.ToString();
                            primaryEmailAddress.Channel.ChannelTypeDescription = email.ChannelTypeDescription;
                            primaryEmailAddress.IsPrimary = true;
                            break;
                        }
                        else if (isCorpEmailPresent && !onlyDayOfTravelContact && email.ChannelTypeDescription.ToLower() != "corporate")
                        {
                            continue;
                        }
                    }
                    //Fix for CheckOut ArgNull Exception - Empty EmailAddress with null EffectiveDate & DiscontinuedDate for Corp Account Revenue Booking (MOBILE-9873) - Shashank : Added OR condition to allow CorporateAccount ProfileOwner.
                    if ((!_configuration.GetValue<bool>("DisableCheckforCorpAccEmail")
                            && email.IsProfileOwner == true && primaryEmailAddress.IsNullOrEmpty()))
                    {
                        #region As per Wade Change want to filter out to return only Primary email to client if not primary phone exist return the first one from the list.So lot of work at client side will save time. Sep 15th 2014
                        co = co + 1;
                        MOBEmail e = new MOBEmail();
                        e.Channel = new MOBChannel();
                        e.EmailAddress = email.EmailAddress;
                        e.Channel.ChannelCode = email.ChannelCode;
                        e.Channel.ChannelDescription = email.ChannelCodeDescription;
                        e.Channel.ChannelTypeCode = email.ChannelTypeCode.ToString();
                        e.Channel.ChannelTypeDescription = email.ChannelTypeDescription;

                        if (!onlyDayOfTravelContact)
                        {
                            if (co == 1)
                            {
                                primaryEmailAddress = new MOBEmail();
                                primaryEmailAddress = e;
                            }
                        }
                        #endregion
                    }
                }
                if (primaryEmailAddress != null)
                {
                    mobEmailAddresses.Add(primaryEmailAddress);
                }
            }
            return mobEmailAddresses;
            #endregion
        }
        public List<MOBAddress> PopulateCorporateTravelerAddresses(List<United.CorporateDirect.Models.CustomerProfile.Address> addresses, MOBApplication application = null, string flow = null)
        {
            #region
            List<MOBAddress> mobAddresses = new List<MOBAddress>();
            if (addresses != null && addresses.Count > 0)
            {

                foreach (United.CorporateDirect.Models.CustomerProfile.Address address in addresses)
                {
                    if ((_configuration.GetValue<bool>("USPOSCountryCodes_ByPass") || IsInternationalBilling(application, address.CountryCode, flow)))
                    {
                        MOBAddress a = new MOBAddress();
                        a.Key = address.Key;
                        a.Channel = new MOBChannel();
                        a.Channel.ChannelCode = address.ChannelCode;
                        a.Channel.ChannelDescription = address.ChannelCodeDescription;
                        a.Channel.ChannelTypeCode = address.ChannelTypeCode.ToString();
                        a.Channel.ChannelTypeDescription = address.ChannelTypeDescription;
                        a.City = address.City;
                        a.Country = new MOBCountry();
                        a.Country.Code = address.CountryCode;
                        a.Line1 = address.AddressLine1;
                        a.State = new MOBState();
                        a.State.Code = address.StateCode;
                        a.PostalCode = address.PostalCode;
                        a.IsPrimary = true;
                        a.IsCorporate = true;
                        if (_configuration.GetValue<bool>("ShowTripInsuranceBookingSwitch"))
                        {
                            a.IsValidForTPIPurchase = IsValidAddressForTPIpayment(address.CountryCode);

                            if (a.IsValidForTPIPurchase)
                            {
                                a.IsValidForTPIPurchase = IsValidSateForTPIpayment(address.StateCode);
                            }
                        }
                        mobAddresses.Add(a);
                    }
                }
            }
            return mobAddresses;
            #endregion
        }
        public async Task<List<MOBCreditCard>> PopulateCreditCards(bool isGetCreditCardDetailsCall, List<MOBAddress> addresses, MOBCPProfileRequest request)
        {
            var response = await _sessionHelperService.GetSession<CreditCardDataReponseModel>(request.SessionId, ObjectNames.CSLProfileCreditCardsResponse, new List<string> { request.SessionId, ObjectNames.CSLProfileCreditCardsResponse }).ConfigureAwait(false);

            if (response != null)
            {
                var creditCards = response.CreditCards;
                List<MOBCreditCard> mobCreditCards = new List<MOBCreditCard>();
                if (creditCards != null && creditCards.Count > 0)
                {
                    #region
                    foreach (ProfileCreditCardItem creditCard in creditCards)
                    {
                        //if(!IsValidCreditCard(creditCard))
                        //{
                        //    continue;
                        //}
                        //if (creditCard.IsCorporate) //Service wont send corporate credit cards now ..So this condition is no longer needed
                        //    continue;

                        MOBCreditCard cc = new MOBCreditCard();
                        cc.Message = IsValidCreditCardMessage(creditCard.AddressKey, creditCard.ExpYear, creditCard.ExpMonth);
                        cc.AddressKey = creditCard.AddressKey;
                        cc.Key = creditCard.Key;
                        if (_configuration.GetValue<bool>("WorkAround4DataVaultUntilClientChange"))
                        {
                            cc.Key = creditCard.Key + "~" + creditCard.AccountNumberToken;
                        }
                        cc.CardType = creditCard.Code;

                        cc.CardTypeDescription = creditCard.CCTypeDescription;
                        cc.Description = creditCard.CustomDescription;
                        cc.ExpireMonth = creditCard.ExpMonth.ToString();
                        cc.ExpireYear = creditCard.ExpYear.ToString();
                        cc.IsPrimary = creditCard.IsPrimary;

                        //updated due to CSL no longer providing the account number.
                        //Wade 11/03/2014
                        cc.UnencryptedCardNumber = "XXXXXXXXXXXX" + creditCard.AccountNumberLastFourDigits;
                        //**NOTE**: IF "XXXXXXXXXXXX" is updated to any other format need to fix this same at GetUpdateCreditCardRequest() as we trying to check if CC updated by checking "XXXXXXXXXXXX" exists in the UnencryptedCardNumber if exists CC Number not updated.

                        cc.DisplayCardNumber = cc.UnencryptedCardNumber;
                        // cc.EncryptedCardNumber = creditCard.AccountNumberEncrypted;Service usually never send this property confirmed with them
                        // cc.cIDCVV2 = creditCard.SecurityCode;Service usually never send this property confirmed with them
                        //cc.CCName = creditCard.Name;
                        if (creditCard.Payor != null)
                        {
                            cc.CCName = creditCard.Payor.GivenName;
                        }
                        //if (isGetCreditCardDetailsCall)
                        //{
                        //    cc.UnencryptedCardNumber = creditCard.AccountNumber;
                        //}
                        cc.AccountNumberToken = creditCard.AccountNumberToken;
                        var vormetricKeys = await AssignPersistentTokenToCC(creditCard.AccountNumberToken, creditCard.PersistentToken, /*creditCard.SecurityCodeToken*/"", creditCard.Code, "", "PopulateCreditCards", 0, "");//SecurityCodeToken will never be sent by service confirmed with service team
                        if (!string.IsNullOrEmpty(vormetricKeys.PersistentToken))
                        {
                            cc.PersistentToken = vormetricKeys.PersistentToken;
                            //cc.SecurityCodeToken = vormetricKeys.SecurityCodeToken;
                        }

                        if (!string.IsNullOrEmpty(vormetricKeys.CardType) && string.IsNullOrEmpty(cc.CardType))
                        {
                            cc.CardType = vormetricKeys.CardType;
                        }

                        //if (_configuration.GetValue<bool>("CFOPViewRes_ExcludeCorporateCard")) //No longer needed to check as new service wont return the corporate credit cards
                        //    cc.IsCorporate = creditCard.IsCorporate;

                        if (_configuration.GetValue<bool>("ShowTripInsuranceBookingSwitch"))
                        {
                            cc.IsValidForTPIPurchase = IsValidFOPForTPIpayment(creditCard.Code);
                        }
                        if (addresses != null)
                        {
                            foreach (var address in addresses)
                            {
                                if (address.Key.ToUpper().Trim() == cc.AddressKey.ToUpper().Trim())
                                {
                                    mobCreditCards.Add(cc);
                                }
                            }
                        }
                    }
                    #endregion
                }
                return mobCreditCards;
            }
            else
            {
                throw new Exception(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }
        }

        private async Task<List<MOBCreditCard>> PopulateCorporateCreditCards(bool isGetCreditCardDetailsCall, List<MOBAddress> addresses, Reservation persistedReservation, MOBCPProfileRequest request)
        {
            var response = await _sessionHelperService.GetSession<CorpFopResponse>(request.SessionId, ObjectNames.CSLCorFopResponse, new List<string> { request.SessionId, ObjectNames.CSLCorFopResponse }).ConfigureAwait(false);

            #region     
            if (response != null)
            {
                List<MOBCreditCard> mobCreditCards = new List<MOBCreditCard>();
                var mobGhostCardFirstInList = new List<MOBCreditCard>();
                MOBCreditCard ghostCreditCard = null;
                bool isGhostCard = false;
                bool isValidForTPI = false;
                var creditCards = response.CreditCards;
                if (creditCards != null && creditCards.Count > 0)
                {
                    #region
                    foreach (United.CorporateDirect.Models.CustomerProfile.CorporateCreditCard creditCard in creditCards)
                    {
                        string addressKey = addresses != null ? addresses[0].Key : string.Empty;
                        MOBCreditCard cc = new MOBCreditCard
                        {
                            Message = IsValidCreditCardMessage(addressKey, creditCard.ExpYear, creditCard.ExpMonth),
                            AddressKey = addresses != null ? addresses[0].Key : string.Empty,//Getprofile service was assigning the first address(Confirmed with service team) ..Corporate direct service wont have addresskey..So,implemented how getprofile is doing
                            Key = !_configuration.GetValue<bool>("DisablePassingCreditcardKeyForCorporateCreditcards") ? (new Guid()).ToString() : string.Empty
                        };
                        if (_configuration.GetValue<bool>("WorkAround4DataVaultUntilClientChange"))
                        {
                            cc.Key = "~" + creditCard.AccountNumberToken;
                        }
                        cc.CardType = creditCard.Code;
                        cc.CardTypeDescription = creditCard.CCTypeDescription;
                        cc.Description = creditCard.CustomDescription;
                        cc.ExpireMonth = creditCard.ExpMonth.ToString();
                        cc.ExpireYear = creditCard.ExpYear.ToString();
                        //cc.IsPrimary = creditCard.IsPrimary;

                        //Wade 11/03/2014
                        cc.UnencryptedCardNumber = "XXXXXXXXXXXX" + creditCard.AccountNumberLastFourDigits;
                        //**NOTE**: IF "XXXXXXXXXXXX" is updated to any other format need to fix this same at GetUpdateCreditCardRequest() as we trying to check if CC updated by checking "XXXXXXXXXXXX" exists in the UnencryptedCardNumber if exists CC Number not updated.

                        cc.DisplayCardNumber = cc.UnencryptedCardNumber;

                        // cc.cIDCVV2 = creditCard.SecurityCode; Service wil never send this confirmed with service team

                        if (creditCard.Payor != null)
                        {
                            cc.CCName = creditCard.Payor.GivenName;
                        }
                        //if (isGetCreditCardDetailsCall)
                        //{
                        //    cc.UnencryptedCardNumber = creditCard.AccountNumber;
                        //}
                        cc.AccountNumberToken = creditCard.AccountNumberToken;
                        var vormetricKeys = await AssignPersistentTokenToCC(creditCard.AccountNumberToken, creditCard.PersistentToken, /*creditCard.SecurityCodeToken*/"", creditCard.Code, "", "PopulateCorporateCreditCards", 0, "");
                        if (!string.IsNullOrEmpty(vormetricKeys.PersistentToken))
                        {
                            cc.PersistentToken = vormetricKeys.PersistentToken;
                            cc.SecurityCodeToken = vormetricKeys.SecurityCodeToken;
                        }
                        if (!string.IsNullOrEmpty(vormetricKeys.CardType) && string.IsNullOrEmpty(cc.CardType))
                        {
                            cc.CardType = vormetricKeys.CardType;
                        }
                        cc.IsCorporate = creditCard.IsCorporate;
                        cc.IsMandatory = creditCard.IsMandatory;
                        if (_configuration.GetValue<bool>("ShowTripInsuranceBookingSwitch"))
                        {
                            cc.IsValidForTPIPurchase = IsValidFOPForTPIpayment(creditCard.Code);
                        }
                        //Not assigning the cc.EncryptedCardNumber = creditCard.AccountNumberEncrypted; because client will send back to us and while updating we will call DataVault and it fails with AppId
                        if (addresses != null)
                        {
                            foreach (var address in addresses)
                            {
                                if (address.Key.ToUpper().Trim() == cc.AddressKey.ToUpper().Trim() && !cc.IsCorporate && !cc.IsMandatory)
                                {
                                    mobCreditCards.Add(cc);
                                }
                            }
                        }
                        //Mandatory Ghost Cards - If Present then only one card should be displayed to the client and no option to add CC / select other FOPs
                        if (creditCard.IsCorporate && creditCard.IsMandatory)
                        {
                            ghostCreditCard = cc;
                            isGhostCard = true;
                            if (_configuration.GetValue<bool>("ShowTripInsuranceBookingSwitch"))
                            {
                                isValidForTPI = cc.IsValidForTPIPurchase;
                            }
                            break;
                        }
                        //Non Mandatory Ghost cards - If Present client can select/Add/Edit other cards and will be first in the list
                        if (cc.IsCorporate && !cc.IsMandatory)
                        {
                            mobGhostCardFirstInList.Add(cc);
                            isGhostCard = true;
                            if (_configuration.GetValue<bool>("ShowTripInsuranceBookingSwitch"))
                            {
                                isValidForTPI = cc.IsValidForTPIPurchase;
                            }
                        }
                    }
                    #endregion
                }
                if (ghostCreditCard != null)
                {
                    //In this case only Ghost card will be in the list
                    mobGhostCardFirstInList.Add(ghostCreditCard);
                }
                else
                {
                    mobGhostCardFirstInList.AddRange(mobCreditCards);
                }
                await GhostCardValidationForTPI(persistedReservation, ghostCreditCard, isGhostCard, isValidForTPI);
                return mobGhostCardFirstInList;
            }
            #endregion
            return null;

        }
        private async Task GhostCardValidationForTPI(Reservation persistedReservation, MOBCreditCard ghostCreditCard, bool isGhostCard, bool isValidForTPI)
        {
            if (isGhostCard)
            {
                if (persistedReservation != null && persistedReservation.ShopReservationInfo != null)
                {
                    //If ghost card has invalid FOP for TPI purchase, we should not show TPI 
                    if (_configuration.GetValue<bool>("ShowTripInsuranceBookingSwitch") && !isValidForTPI)
                    {
                        persistedReservation.ShopReservationInfo.IsGhostCardValidForTPIPurchase = false;
                    }

                    if (ghostCreditCard != null)
                    {
                        persistedReservation.ShopReservationInfo.CanHideSelectFOPOptionsAndAddCreditCard = true;
                    }
                    await SavePersistedReservation(persistedReservation);
                }
            }
        }
        private async Task SavePersistedReservation(Reservation persistedReservation)
        {
            await _sessionHelperService.SaveSession<Reservation>(persistedReservation, persistedReservation.SessionId, new List<string> { persistedReservation.SessionId, persistedReservation.ObjectName }, persistedReservation.ObjectName);
        }
        private string IsValidCreditCardMessage(string addressKey, int expYear, int expMonth)
        {
            string message = string.Empty;
            if (string.IsNullOrEmpty(addressKey))
            {
                message = _configuration.GetValue<string>("NoAddressAssociatedWithTheSavedCreditCardMessage");
            }
            if (expYear < DateTime.Today.Year)
            {
                message = message + _configuration.GetValue<string>("CreditCardDateExpiredMessage");
            }
            else if (expYear == DateTime.Today.Year)
            {
                if (expMonth < DateTime.Today.Month)
                {
                    message = message + _configuration.GetValue<string>("CreditCardDateExpiredMessage");
                }
            }
            return message;
        }
        private List<MOBEmail> PopulateMPSecurityEmailAddressesV2(List<Definition.CSLModels.CustomerProfile.Email> emailAddresses)
        {
            #region
            List<MOBEmail> mobEmailAddresses = new List<MOBEmail>();
            if (emailAddresses != null && emailAddresses.Count > 0)
            {
                MOBEmail primaryEmailAddress = null;
                int co = 0;
                foreach (Definition.CSLModels.CustomerProfile.Email email in emailAddresses)
                {
                    if (email.EffectiveDate <= DateTime.UtcNow && email.DiscontinuedDate >= DateTime.UtcNow)
                    {
                        #region As per Wade Change want to filter out to return only Primary email to client if not primary phone exist return the first one from the list.So lot of work at client side will save time. Sep 15th 2014
                        co = co + 1;
                        MOBEmail e = new MOBEmail();
                        e.Key = email.Key;
                        e.Channel = new MOBChannel();
                        e.Channel.ChannelCode = "E";
                        e.Channel.ChannelDescription = "Email";
                        e.Channel.ChannelTypeCode = email.Type.ToString();
                        e.Channel.ChannelTypeDescription = email.TypeDescription;
                        e.EmailAddress = email.Address;
                        e.IsDefault = email.PrimaryIndicator;
                        e.IsPrimary = email.PrimaryIndicator;
                        e.IsPrivate = email.IsPrivate;
                        e.IsDayOfTravel = email.DayOfTravelNotification;
                        if (email.PrimaryIndicator)
                        {
                            primaryEmailAddress = new MOBEmail();
                            primaryEmailAddress = e;
                            break;
                        }
                        #endregion
                    }
                }
                if (primaryEmailAddress != null)
                {
                    mobEmailAddresses.Add(primaryEmailAddress);
                }
            }
            return mobEmailAddresses;
            #endregion
        }
        public List<MOBAddress> PopulateTravelerAddressesV2(List<Definition.CSLModels.CustomerProfile.Address> addresses, MOBApplication application = null, string flow = null)
        {
            #region

            var mobAddresses = new List<MOBAddress>();
            if (addresses != null && addresses.Count > 0)
            {
                bool isCorpAddressPresent = false;
                //As per Business / DotCom Kalpen; we are removing the condition for checking the Effectivedate and Discontinued date
                var corpIndex = addresses.FindIndex(x => x.TypeDescription != null && x.TypeDescription.ToLower() == "corporate" && x.Line1 != null && x.Line1.Trim() != "");
                if (corpIndex >= 0)
                    isCorpAddressPresent = true;
                foreach (Definition.CSLModels.CustomerProfile.Address address in addresses)
                {

                    if (isCorpAddressPresent && address.TypeDescription.ToLower() == "corporate" &&
                              (_configuration.GetValue<bool>("USPOSCountryCodes_ByPass") || IsInternationalBilling(application, address.CountryCode, flow)))
                    {
                        var a = new MOBAddress();
                        a.Key = address.Key;
                        a.Channel = new MOBChannel();
                        a.Channel.ChannelCode = "A";
                        a.Channel.ChannelDescription = "Address";
                        a.Channel.ChannelTypeCode = address.Type.ToString();
                        a.Channel.ChannelTypeDescription = address.TypeDescription;
                        //a.ApartmentNumber = address.AptNum; No longer needed confirmed from service
                        a.City = address.City;
                        // a.CompanyName = address.CompanyName;No longer needed confirmed from service
                        a.Country = new MOBCountry();
                        a.Country.Code = address.CountryCode;
                        a.Country.Name = address.CountryName;
                        // a.JobTitle = address.JobTitle;No longer needed confirmed from service
                        a.Line1 = address.Line1;
                        a.Line2 = address.Line2;
                        a.Line3 = address.Line3;
                        a.State = new MOBState();
                        a.State.Code = address.StateCode;
                        a.IsDefault = address.PrimaryIndicator;
                        a.IsPrivate = address.IsPrivate;
                        a.PostalCode = address.PostalCode;
                        if (address.TypeDescription.ToLower().Trim() == "corporate")
                        {
                            a.IsPrimary = true;
                            a.IsCorporate = true; // MakeIsCorporate true inorder to disable the edit on client
                        }
                        // Make IsPrimary true inorder to select the corpaddress by default

                        if (_configuration.GetValue<bool>("ShowTripInsuranceBookingSwitch"))
                        {
                            a.IsValidForTPIPurchase = IsValidAddressForTPIpayment(address.CountryCode);

                            if (a.IsValidForTPIPurchase)
                            {
                                a.IsValidForTPIPurchase = IsValidSateForTPIpayment(address.StateCode);
                            }
                        }
                        mobAddresses.Add(a);
                    }
                    if (address.EffectiveDate <= DateTime.UtcNow && address.DiscontinuedDate >= DateTime.UtcNow)
                    {
                        if (_configuration.GetValue<bool>("USPOSCountryCodes_ByPass") || IsInternationalBilling(application, address.CountryCode, flow)) //##Kirti - allow only US addresses 
                        {
                            var a = new MOBAddress();
                            a.Key = address.Key;
                            a.Channel = new MOBChannel();
                            a.Channel.ChannelCode = "A";
                            a.Channel.ChannelDescription = "Address";
                            a.Channel.ChannelTypeCode = address.Type.ToString();
                            a.Channel.ChannelTypeDescription = address.TypeDescription;
                            a.City = address.City;
                            a.Country = new MOBCountry();
                            a.Country.Code = address.CountryCode;
                            a.Country.Name = address.CountryName;
                            a.Line1 = address.Line1;
                            a.Line2 = address.Line2;
                            a.Line3 = address.Line3;
                            a.State = new MOBState();
                            a.State.Code = address.StateCode;
                            //a.State.Name = address.StateName;
                            a.IsDefault = address.PrimaryIndicator;
                            a.IsPrimary = address.PrimaryIndicator;
                            a.IsPrivate = address.IsPrivate;
                            a.PostalCode = address.PostalCode;
                            //Adding this check for corporate addresses to gray out the Edit button on the client
                            //if (address.ChannelTypeDescription.ToLower().Trim() == "corporate")
                            //{
                            //    a.IsCorporate = true;
                            //}
                            if (_configuration.GetValue<bool>("ShowTripInsuranceBookingSwitch"))
                            {
                                a.IsValidForTPIPurchase = IsValidAddressForTPIpayment(address.CountryCode);

                                if (a.IsValidForTPIPurchase)
                                {
                                    a.IsValidForTPIPurchase = IsValidSateForTPIpayment(address.StateCode);
                                }
                            }
                            mobAddresses.Add(a);
                        }
                    }
                }
            }
            return mobAddresses;
            #endregion
        }
        private bool IsValidSateForTPIpayment(string stateCode)
        {
            return !string.IsNullOrEmpty(stateCode) && !string.IsNullOrEmpty(_configuration.GetValue<string>("ExcludeUSStateCodesForTripInsurance")) && !_configuration.GetValue<string>("ExcludeUSStateCodesForTripInsurance").Contains(stateCode.ToUpper().Trim());
        }
        private bool IsValidAddressForTPIpayment(string countryCode)
        {
            return !string.IsNullOrEmpty(countryCode) && countryCode.ToUpper().Trim() == "US";
        }
        public bool IsInternationalBillingAddress_CheckinFlowEnabled(MOBApplication application)
        {
            if (_configuration.GetValue<bool>("EnableInternationalBillingAddress_CheckinFlow"))
            {
                if (application != null && GeneralHelper.isApplicationVersionGreater(application.Id, application.Version.Major, "IntBillingCheckinFlowAndroidversion", "IntBillingCheckinFlowiOSversion", "", "", true, _configuration))
                {
                    return true;
                }
            }
            return false;
        }
        private bool IsInternationalBilling(MOBApplication application, string countryCode, string flow)
        {
            if (_configuration.GetValue<bool>("EnableUCBPhase1_MobilePhase3Changes") && String.IsNullOrEmpty(countryCode))
                return false;

            bool _isIntBilling = IsInternationalBillingAddress_CheckinFlowEnabled(application);
            if (_isIntBilling && flow?.ToLower() == FlowType.CHECKIN.ToString().ToLower()) // need to enable Int Billing address only in Checkin flow
            {
                //check for multiple countries
                return _isIntBilling;
            }
            else
            {
                //Normal Code as usual
                return _configuration.GetValue<string>("USPOSCountryCodes").Contains(countryCode);
            }
        }
        private List<MOBPrefAirPreference> PopulateAirPrefrencesV2(TravelerProfileResponse traveler)
        {
            var airPreferences = traveler.AirPreferences;
            List<MOBPrefAirPreference> mobAirPrefs = new List<MOBPrefAirPreference>();
            if (airPreferences != null && airPreferences.Count > 0)
            {
                foreach (Definition.CSLModels.CustomerProfile.AirPreferenceDataModel pref in airPreferences)
                {
                    MOBPrefAirPreference mobAirPref = new MOBPrefAirPreference();
                    mobAirPref.AirportCode = pref.AirportCode;
                    mobAirPref.AirportCode = pref.AirportNameLong;
                    mobAirPref.AirportNameShort = pref.AirportNameShort;
                    mobAirPref.AirPreferenceId = pref.AirPreferenceId;
                    mobAirPref.ClassDescription = pref.ClassDescription;
                    mobAirPref.ClassId = pref.ClassID;
                    mobAirPref.CustomerId = traveler.Profile.CustomerId;
                    mobAirPref.EquipmentCode = pref.EquipmentCode;
                    mobAirPref.EquipmentDesc = pref.EquipmentDescription;
                    mobAirPref.EquipmentId = pref.EquipmentID;
                    mobAirPref.IsActive = true;//By default if it is returned it is active confirmed with service team
                    mobAirPref.IsSelected = true;// By default if it is returned it is active confirmed with service team
                    mobAirPref.IsNew = false;// By default if it is returned it is false confirmed with service team
                    mobAirPref.Key = pref.Key;
                    //mobAirPref.LanguageCode = pref.LanguageCode;No longer sent from service confirmed with them
                    mobAirPref.MealCode = pref.MealCode;
                    mobAirPref.MealDescription = pref.MealDescription;
                    mobAirPref.MealId = pref.MealId;
                    // mobAirPref.NumOfFlightsDisplay = pref.NumOfFlightsDisplay;No longer sent from service confirmed with them
                    mobAirPref.ProfileId = traveler.Profile.ProfileId;
                    mobAirPref.SearchPreferenceDescription = pref.SearchPreferenceDescription;
                    mobAirPref.SearchPreferenceId = pref.SearchPreferenceID;
                    //mobAirPref.SeatFrontBack = pref.SeatFrontBack;No longer sent from service confirmed with them
                    mobAirPref.SeatSide = pref.SeatSide;
                    mobAirPref.SeatSideDescription = pref.SeatSideDescription;
                    mobAirPref.VendorCode = pref.VendorCode;//Service confirmed we can hard code this as we dont have any other vendor it is always United airlines
                    mobAirPref.VendorDescription = pref.VendorDescription;//Service confirmed we can hard code this as we dont have any other vendor it is always United airlines
                    mobAirPref.VendorId = pref.VendorId;
                    mobAirPref.AirRewardPrograms = GetAirRewardPrograms(traveler);
                    // mobAirPref.SpecialRequests = GetTravelerSpecialRequests(pref.SpecialRequests);Client is not using this even we send this ..
                    // mobAirPref.ServiceAnimals = GetTravelerServiceAnimals(pref.ServiceAnimals);Client is not using this even we send this ..
                    mobAirPrefs.Add(mobAirPref);
                }
            }
            return mobAirPrefs;
        }
        private List<MOBPrefRewardProgram> GetAirRewardPrograms(TravelerProfileResponse traveler)
        {
            List<MOBPrefRewardProgram> mobAirRewardsProgs = new List<MOBPrefRewardProgram>();
            if (traveler?.RewardPrograms != null && traveler?.RewardPrograms.Count > 0)
            {
                foreach (RewardProgramData pref in traveler?.RewardPrograms)
                {
                    MOBPrefRewardProgram mobAirRewardsProg = new MOBPrefRewardProgram();
                    if (traveler?.Profile != null)
                    {
                        mobAirRewardsProg.CustomerId = traveler.Profile.CustomerId;
                        mobAirRewardsProg.ProfileId = traveler.Profile.ProfileId;
                    }
                    mobAirRewardsProg.ProgramMemberId = pref.ProgramMemberId;
                    mobAirRewardsProg.VendorCode = pref.VendorCode;
                    mobAirRewardsProg.VendorDescription = pref.VendorDescription;
                    mobAirRewardsProgs.Add(mobAirRewardsProg);
                }
            }
            return mobAirRewardsProgs;
        }
        private List<MOBEmail> PopulateAllEmailAddressesV2(List<Definition.CSLModels.CustomerProfile.Email> emailAddresses)
        {
            #region
            List<MOBEmail> mobEmailAddresses = new List<MOBEmail>();
            if (emailAddresses != null && emailAddresses.Count > 0)
            {
                int co = 0;
                foreach (Definition.CSLModels.CustomerProfile.Email email in emailAddresses)
                {
                    if (email.EffectiveDate <= DateTime.UtcNow && email.DiscontinuedDate >= DateTime.UtcNow)
                    {
                        #region As per Wade Change want to filter out to return only Primary email to client if not primary phone exist return the first one from the list.So lot of work at client side will save time. Sep 15th 2014
                        co = co + 1;
                        MOBEmail e = new MOBEmail();
                        e.Key = email.Key;
                        e.Channel = new MOBChannel();
                        e.Channel.ChannelCode = "E";
                        e.Channel.ChannelDescription = "Email";
                        e.Channel.ChannelTypeCode = email.Type.ToString();
                        e.Channel.ChannelTypeDescription = email.TypeDescription;
                        e.EmailAddress = email.Address;
                        e.IsDefault = email.PrimaryIndicator;
                        e.IsPrimary = email.PrimaryIndicator;
                        e.IsPrivate = email.IsPrivate;
                        e.IsDayOfTravel = email.DayOfTravelNotification;
                        mobEmailAddresses.Add(e);
                        #endregion
                    }
                }
            }
            return mobEmailAddresses;
            #endregion
        }
        private List<MOBEmail> PopulateEmailAddressesV2(List<Definition.CSLModels.CustomerProfile.Email> emailAddresses, bool onlyDayOfTravelContact)
        {
            #region
            List<MOBEmail> mobEmailAddresses = new List<MOBEmail>();
            bool isCorpEmailPresent = false;

            if (emailAddresses != null && emailAddresses.Count > 0)
            {
                if (IsCorpBookingPath)
                {
                    //As per Business / DotCom Kalpen; we are removing the condition for checking the Effectivedate and Discontinued date
                    var corpIndex = emailAddresses.FindIndex(x => x.TypeDescription != null && x.TypeDescription.ToLower() == "corporate" && x.Address != null && x.Address.Trim() != "");
                    if (corpIndex >= 0)
                        isCorpEmailPresent = true;
                }

                MOBEmail primaryEmailAddress = null;
                int co = 0;
                foreach (Definition.CSLModels.CustomerProfile.Email email in emailAddresses)
                {

                    if (isCorpEmailPresent && !onlyDayOfTravelContact && email.TypeDescription.ToLower() == "corporate")
                    {
                        primaryEmailAddress = new MOBEmail();
                        email.PrimaryIndicator = true;
                        primaryEmailAddress.Key = email.Key;
                        primaryEmailAddress.Channel = new MOBChannel();
                        primaryEmailAddress.EmailAddress = email.Address;
                        primaryEmailAddress.Channel.ChannelCode = "E";
                        primaryEmailAddress.Channel.ChannelDescription = "Email";
                        primaryEmailAddress.Channel.ChannelTypeCode = email.Type.ToString();
                        primaryEmailAddress.Channel.ChannelTypeDescription = email.TypeDescription;
                        primaryEmailAddress.IsDefault = email.PrimaryIndicator;
                        primaryEmailAddress.IsPrimary = email.PrimaryIndicator;
                        primaryEmailAddress.IsPrivate = email.IsPrivate;
                        primaryEmailAddress.IsDayOfTravel = email.DayOfTravelNotification;
                        if (!email.DayOfTravelNotification)
                        {
                            break;
                        }

                    }
                    else if (isCorpEmailPresent && !onlyDayOfTravelContact && email.TypeDescription.ToLower() != "corporate")
                    {
                        continue;
                    }

                    //Fix for CheckOut ArgNull Exception - Empty EmailAddress with null EffectiveDate & DiscontinuedDate for Corp Account Revenue Booking (MOBILE-9873) - Shashank : Added OR condition to allow CorporateAccount ProfileOwner.
                    if ((email.EffectiveDate <= DateTime.UtcNow && email.DiscontinuedDate >= DateTime.UtcNow) ||
                            (email.TypeDescription.ToLower() == "corporate"
                            && email.PrimaryIndicator == true && primaryEmailAddress.IsNullOrEmpty()))
                    {
                        #region As per Wade Change want to filter out to return only Primary email to client if not primary phone exist return the first one from the list.So lot of work at client side will save time. Sep 15th 2014
                        co = co + 1;
                        MOBEmail e = new MOBEmail();
                        e.Key = email.Key;
                        e.Channel = new MOBChannel();
                        e.EmailAddress = email.Address;
                        e.Channel.ChannelCode = "E";
                        e.Channel.ChannelDescription = "Email";
                        e.Channel.ChannelTypeCode = email.Type.ToString();
                        e.Channel.ChannelTypeDescription = email.TypeDescription;
                        e.IsDefault = email.PrimaryIndicator;
                        e.IsPrimary = email.PrimaryIndicator;
                        e.IsPrivate = email.IsPrivate;
                        e.IsDayOfTravel = email.DayOfTravelNotification;
                        if (email.DayOfTravelNotification)
                        {
                            primaryEmailAddress = new MOBEmail();
                            primaryEmailAddress = e;
                            if (onlyDayOfTravelContact)
                            {
                                break;
                            }
                        }
                        if (!onlyDayOfTravelContact)
                        {
                            if (email.PrimaryIndicator)
                            {
                                primaryEmailAddress = new MOBEmail();
                                primaryEmailAddress = e;
                                break;
                            }
                            else if (co == 1)
                            {
                                primaryEmailAddress = new MOBEmail();
                                primaryEmailAddress = e;
                            }
                        }
                        #endregion
                    }
                }
                if (primaryEmailAddress != null)
                {
                    mobEmailAddresses.Add(primaryEmailAddress);
                }
            }
            return mobEmailAddresses;
            #endregion
        }
        public List<MOBBKLoyaltyProgramProfile> GetTravelerRewardPgrograms(List<RewardProgramData> rewardPrograms, int currentEliteLevel)
        {
            List<MOBBKLoyaltyProgramProfile> programs = new List<MOBBKLoyaltyProgramProfile>();

            if (rewardPrograms != null && rewardPrograms.Count > 0)
            {
                foreach (RewardProgramData rewardProgram in rewardPrograms)
                {
                    MOBBKLoyaltyProgramProfile airRewardProgram = new MOBBKLoyaltyProgramProfile();
                    airRewardProgram.ProgramId = rewardProgram.ProgramId.ToString();
                    airRewardProgram.ProgramName = rewardProgram.Description;
                    airRewardProgram.MemberId = rewardProgram.ProgramMemberId;
                    airRewardProgram.CarrierCode = rewardProgram.VendorCode;
                    if (airRewardProgram.CarrierCode.Trim().Equals("UA"))
                    {
                        airRewardProgram.MPEliteLevel = currentEliteLevel;
                    }
                    airRewardProgram.RewardProgramKey = rewardProgram.Key;
                    programs.Add(airRewardProgram);
                }
            }
            return programs;
        }

        public List<MOBCPPhone> PopulatePhonesV2(TravelerProfileResponse traveler, bool onlyDayOfTravelContact)
        {
            List<MOBCPPhone> mobCPPhones = new List<MOBCPPhone>();
            var phones = traveler.Phones;
            if (phones != null && phones.Count > 0)
            {
                MOBCPPhone primaryMobCPPhone = null;
                int co = 0;
                foreach (Definition.CSLModels.CustomerProfile.Phone phone in phones)
                {
                    #region As per Wade Change want to filter out to return only Primary Phone to client if not primary phone exist return the first one from the list.So lot of work at client side will save time. Sep 15th 2014
                    MOBCPPhone mobCPPhone = new MOBCPPhone();
                    co = co + 1;
                    mobCPPhone.AreaNumber = phone.AreaCode;
                    mobCPPhone.PhoneNumber = phone.Number;
                    //mobCPPhone.Attention = phone.Attention;No longer needed confirmed from service
                    mobCPPhone.ChannelCode = "P";
                    mobCPPhone.ChannelCodeDescription = "Phone";
                    mobCPPhone.ChannelTypeCode = phone.Type.ToString();
                    mobCPPhone.ChannelTypeDescription = phone.TypeDescription;
                    mobCPPhone.ChannelTypeSeqNumber = phone.SequenceNumber;
                    mobCPPhone.CountryCode = phone.CountryCode;
                    mobCPPhone.CountryPhoneNumber = phone.CountryPhoneNumber;
                    mobCPPhone.CustomerId = Convert.ToInt32(traveler.Profile.CustomerId);
                    mobCPPhone.Description = phone.Remark;
                    mobCPPhone.DiscontinuedDate = Convert.ToString(phone.DiscontinuedDate);
                    mobCPPhone.EffectiveDate = Convert.ToString(phone.EffectiveDate);
                    mobCPPhone.ExtensionNumber = phone.ExtensionNumber;
                    mobCPPhone.IsPrimary = phone.PrimaryIndicator;
                    mobCPPhone.IsPrivate = phone.IsPrivate;
                    mobCPPhone.IsProfileOwner = traveler.Profile.ProfileOwnerIndicator;
                    mobCPPhone.Key = phone.Key;
                    mobCPPhone.LanguageCode = phone.LanguageCode;
                    mobCPPhone.WrongPhoneDate = Convert.ToString(phone.WrongPhoneDate);
                    mobCPPhone.DeviceTypeCode = phone.DeviceType.ToString();
                    mobCPPhone.DeviceTypeDescription = phone.TypeDescription;

                    mobCPPhone.IsDayOfTravel = phone.DayOfTravelNotification;

                    if (phone.DayOfTravelNotification)
                    {
                        primaryMobCPPhone = new MOBCPPhone();
                        primaryMobCPPhone = mobCPPhone;// Only day of travel contact should be returned to use at Edit Traveler
                        if (onlyDayOfTravelContact)
                        {
                            break;
                        }
                    }
                    if (!onlyDayOfTravelContact)
                    {
                        if (phone.DayOfTravelNotification)
                        {
                            primaryMobCPPhone = new MOBCPPhone();
                            primaryMobCPPhone = mobCPPhone;
                            break;
                        }
                        else if (co == 1)
                        {
                            primaryMobCPPhone = new MOBCPPhone();
                            primaryMobCPPhone = mobCPPhone;
                        }
                    }
                    #endregion
                }
                if (primaryMobCPPhone != null)
                {
                    mobCPPhones.Add(primaryMobCPPhone);
                }
            }
            return mobCPPhones;
        }

        private string GetYAPaxDescByDOB()
        {
            return "Young adult (18-23)";
        }
        private List<MOBCPSecureTraveler> PopulatorSecureTravelersV2(SecureTravelerResponseData secureTravelerResponseData, ref bool isTSAFlag, bool correctDate, string sessionID, int appID, string deviceID)
        {
            List<MOBCPSecureTraveler> mobSecureTravelers = null;

            if (secureTravelerResponseData?.SecureTraveler != null)
            {
                mobSecureTravelers = new List<MOBCPSecureTraveler>();
                var secureTraveler = secureTravelerResponseData.SecureTraveler;
                if (!_configuration.GetValue<bool>("DisableUCBKTNFix") && secureTraveler.DocumentType == null) //MOBILE-26294 : Before UCB Migration documentype used to be empty .But after UCB Migration we are getting it as Null Due to that we are not building the KTN number.Looks for the bug number for more details.
                {
                    secureTraveler.DocumentType = "";
                }

                if (secureTraveler.DocumentType != null && secureTraveler.DocumentType.Trim().ToUpper() != "X")
                {
                    #region
                    MOBCPSecureTraveler mobSecureTraveler = new MOBCPSecureTraveler();
                    if (correctDate)
                    {
                        DateTime tempBirthDate = secureTraveler.BirthDate.GetValueOrDefault().AddHours(1);
                        mobSecureTraveler.BirthDate = tempBirthDate.ToString("MM/dd/yyyy", CultureInfo.CurrentCulture);
                    }
                    else
                    {
                        mobSecureTraveler.BirthDate = secureTraveler.BirthDate.GetValueOrDefault().ToString("MM/dd/yyyy", CultureInfo.CurrentCulture);
                    }
                    mobSecureTraveler.CustomerId = Convert.ToInt32(secureTraveler.CustomerId);
                    mobSecureTraveler.DecumentType = secureTraveler.DocumentType;
                    mobSecureTraveler.Description = secureTraveler.Description;
                    mobSecureTraveler.FirstName = secureTraveler.FirstName;
                    mobSecureTraveler.Gender = secureTraveler.Gender;
                    // mobSecureTraveler.Key = secureTraveler.Key;No longer needed confirmed from service
                    mobSecureTraveler.LastName = secureTraveler.LastName;
                    mobSecureTraveler.MiddleName = secureTraveler.MiddleName;
                    mobSecureTraveler.SequenceNumber = (int)secureTraveler.SequenceNumber;
                    mobSecureTraveler.Suffix = secureTraveler.Suffix;
                    if (secureTravelerResponseData.SupplementaryTravelInfos != null)
                    {
                        foreach (SupplementaryTravelDocsDataMembers supplementaryTraveler in secureTravelerResponseData.SupplementaryTravelInfos)
                        {
                            if (supplementaryTraveler.Type == "K")
                            {
                                mobSecureTraveler.KnownTravelerNumber = supplementaryTraveler.Number;
                            }
                            if (supplementaryTraveler.Type == "R")
                            {
                                mobSecureTraveler.RedressNumber = supplementaryTraveler.Number;
                            }
                        }
                    }
                    if (!isTSAFlag && secureTraveler.DocumentType.Trim().ToUpper() == "U")
                    {
                        isTSAFlag = true;
                    }
                    if (secureTraveler.DocumentType.Trim().ToUpper() == "C" || secureTraveler.DocumentType.Trim() == "") // This is to get only Customer Cleared Secure Traveler records
                    {
                        mobSecureTravelers = new List<MOBCPSecureTraveler>();
                        mobSecureTravelers.Add(mobSecureTraveler);
                    }
                    else
                    {
                        mobSecureTravelers.Add(mobSecureTraveler);
                    }
                    #endregion
                }

            }
            return mobSecureTravelers;
        }

        private MOBCPMileagePlus PopulateMileagePlusV2(OwnerResponseModel profileOwnerResponse, string mileageplusId)
        {
            if (profileOwnerResponse?.MileagePlus?.Data != null)
            {
                MOBCPMileagePlus mileagePlus = null;
                var mileagePlusData = profileOwnerResponse.MileagePlus.Data;

                mileagePlus = new MOBCPMileagePlus();
                var balance = profileOwnerResponse.MileagePlus.Data.Balances?.FirstOrDefault(balnc => (int)balnc.Currency == 5);
                if (balance != null)
                    mileagePlus.AccountBalance = Convert.ToInt32(balance.Amount);
                mileagePlus.ActiveStatusCode = mileagePlusData.AccountStatus;
                mileagePlus.ActiveStatusDescription = mileagePlusData.AccountStatusDescription;
                mileagePlus.AllianceEliteLevel = mileagePlusData.StarAllianceTierLevel;
                mileagePlus.ClosedStatusCode = mileagePlusData.OpenClosedStatusCode;
                mileagePlus.ClosedStatusDescription = mileagePlusData.OpenClosedStatusDescription;
                mileagePlus.CurrentEliteLevel = mileagePlusData.MPTierLevel;
                if (mileagePlus.CurrentEliteLevelDescription != null)
                {
                    mileagePlus.CurrentEliteLevelDescription = mileagePlusData.MPTierLevelDescription.ToString().ToUpper() == "NON-ELITE" ? "General member" : mileagePlusData.MPTierLevelDescription;
                }
                mileagePlus.CurrentYearMoneySpent = mileagePlusData.CurrentYearMoneySpent;
                mileagePlus.EliteMileageBalance = Convert.ToInt32(mileagePlusData.EliteMileageBalance);
                mileagePlus.EnrollDate = mileagePlusData.EnrollDate.GetValueOrDefault().ToString("MM/dd/yyyy");
                mileagePlus.EnrollSourceCode = mileagePlusData.EnrollSourceCode;
                mileagePlus.EnrollSourceDescription = mileagePlusData.EnrollSourceDescription;
                // mileagePlus.FlexPqmBalance = onePass.FlexPQMBalance;
                mileagePlus.FutureEliteDescription = mileagePlusData.NextStatusLevelDescription;
                mileagePlus.FutureEliteLevel = mileagePlusData.NextStatusLevel;
                mileagePlus.InstantEliteExpirationDate = mileagePlusData.NextStatusLevelDescription;
                mileagePlus.IsCEO = mileagePlusData.CEO;
                mileagePlus.IsClosedPermanently = mileagePlusData.IsClosedPermanently;
                mileagePlus.IsClosedTemporarily = mileagePlusData.IsClosedTemporarily;
                mileagePlus.IsLockedOut = mileagePlusData.IsLockedOut;
                mileagePlus.IsUnitedClubMember = mileagePlusData.IsPClubMember;
                mileagePlus.LastActivityDate = mileagePlusData.LastActivityDate.GetValueOrDefault().ToString("MM/dd/yyyy");
                // mileagePlus.LastExpiredMile = Convert.ToInt32(mileagePlusData.LastExpiredMile);
                mileagePlus.LastFlightDate = mileagePlusData.LastFlightDate.GetValueOrDefault().ToString("MM/dd/yyyy");
                //mileagePlus.LastStatementBalance = Convert.ToInt32(mileagePlusData.LastStatementBalance);//This property is deprecated confirmed with service team 
                mileagePlus.LastStatementDate = mileagePlusData.LastStatementDate.GetValueOrDefault().ToString("MM/dd/yyyy");
                mileagePlus.LifetimeEliteMileageBalance = Convert.ToInt32(mileagePlusData.LifetimeMiles);
                // mileagePlus.MileageExpirationDate = mileagePlusData.MileageExpirationDate.GetValueOrDefault().ToString("MM/dd/yyyy");//This property is deprecated confirmed with service team
                mileagePlus.MileagePlusId = mileageplusId;
                return mileagePlus;
            }
            else
            {
                throw new Exception(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }
        }

        private MOBCPCustomerMetrics PopulateCustomerMetrics(OwnerResponseModel profileOwnerResponse)
        {
            if (profileOwnerResponse?.CustomerMetrics?.Data != null)
            {
                MOBCPCustomerMetrics travelerCustomerMetrics = new MOBCPCustomerMetrics();
                if (!String.IsNullOrEmpty(profileOwnerResponse.CustomerMetrics.Data.PTCCode))
                {
                    travelerCustomerMetrics.PTCCode = profileOwnerResponse.CustomerMetrics.Data.PTCCode;
                }
                return travelerCustomerMetrics;
            }
            return null;
        }
        private async Task<Reservation> PersistedReservation(MOBCPProfileRequest request)
        {
            Reservation persistedReservation =
                new Reservation();
            if (request != null)
                persistedReservation = await _sessionHelperService.GetSession<Reservation>(request.SessionId, persistedReservation.ObjectName, new List<string> { request.SessionId, persistedReservation.ObjectName }).ConfigureAwait(false);

            if (_configuration.GetValue<bool>("CorporateConcurBooking"))
            {
                if (persistedReservation != null && persistedReservation.ShopReservationInfo != null &&
                    persistedReservation.ShopReservationInfo.IsCorporateBooking)
                {
                    this.IsCorpBookingPath = true;
                }

                if (persistedReservation != null && persistedReservation.ShopReservationInfo2 != null &&
                    persistedReservation.ShopReservationInfo2.IsArrangerBooking)
                {
                    this.IsArrangerBooking = true;
                }
            }
            return persistedReservation;
        }

        public United.Services.Customer.Common.ProfileRequest GetProfileRequest(MOBCPProfileRequest mobCPProfileRequest, bool getEmployeeIdFromCSLCustomerData = false)
        {
            United.Services.Customer.Common.ProfileRequest request = new United.Services.Customer.Common.ProfileRequest();
            request.DataSetting = _configuration.GetValue<string>("CustomerDBDataSettingForCSLServices").ToString();
            List<string> requestStringList = new List<string>();
            if (mobCPProfileRequest.ProfileOwnerOnly)
            {
                requestStringList.Add("ProfileOwnerOnly");
                if (mobCPProfileRequest.IncludeCreditCards)
                {
                    requestStringList.Add("CreditCards");
                }
            }
            else
            {
                if (mobCPProfileRequest.CustomerId != 0)
                {
                    request.CustomerId = mobCPProfileRequest.CustomerId;
                }
                if (mobCPProfileRequest.IncludeAllTravelerData)
                {
                    requestStringList.Add("AllTravelerData");
                    requestStringList.Add("TravelerData");
                }
                else
                {
                    #region
                    requestStringList.Add("TravelerData");
                    if (mobCPProfileRequest.IncludeAddresses)
                    {
                        requestStringList.Add("Addresses");
                    }
                    if (mobCPProfileRequest.IncludeEmailAddresses)
                    {
                        requestStringList.Add("EmailAddresses");
                    }
                    if (mobCPProfileRequest.IncludePhones)
                    {
                        requestStringList.Add("Phones");
                    }
                    if (mobCPProfileRequest.IncludeSubscriptions)
                    {
                        requestStringList.Add("Subscriptions");
                    }
                    if (mobCPProfileRequest.IncludeTravelMarkets)
                    {
                        requestStringList.Add("TravelMarkets");
                    }
                    if (mobCPProfileRequest.IncludeCustomerProfitScore)
                    {
                        requestStringList.Add("CustomerProfitScore");
                    }
                    if (mobCPProfileRequest.IncludePets)
                    {
                        requestStringList.Add("Pets");
                    }
                    if (mobCPProfileRequest.IncludeCarPreferences)
                    {
                        requestStringList.Add("CarPreferences");
                    }
                    if (mobCPProfileRequest.IncludeDisplayPreferences)
                    {
                        requestStringList.Add("DisplayPreferences");
                    }
                    if (mobCPProfileRequest.IncludeHotelPreferences)
                    {
                        requestStringList.Add("HotelPreferences");
                    }
                    if (mobCPProfileRequest.IncludeAirPreferences)
                    {
                        requestStringList.Add("AirPreferences");
                    }
                    if (mobCPProfileRequest.IncludeContacts)
                    {
                        requestStringList.Add("Contacts");
                    }
                    if (mobCPProfileRequest.IncludePassports)
                    {
                        requestStringList.Add("Passports");
                    }
                    if (mobCPProfileRequest.IncludeSecureTravelers)
                    {
                        requestStringList.Add("SecureTravelers");
                    }
                    if (mobCPProfileRequest.IncludeFlexEQM)
                    {
                        requestStringList.Add("FlexEQM");
                    }
                    if (mobCPProfileRequest.IncludeCreditCards)
                    {
                        requestStringList.Add("CreditCards");
                    }
                    if (mobCPProfileRequest.IncludeServiceAnimals)
                    {
                        requestStringList.Add("ServiceAnimals");
                    }
                    if (mobCPProfileRequest.IncludeSpecialRequests)
                    {
                        requestStringList.Add("SpecialRequests");
                    }
                    if (mobCPProfileRequest.IncludePosCountyCode)
                    {
                        requestStringList.Add("PosCountyCode");
                    }
                    #endregion
                }
            }
            if (requestStringList.Count == 0)
            {
                requestStringList.Add("AllTravelerData"); // This option means return all the data
            }

            if (getEmployeeIdFromCSLCustomerData)
            {
                requestStringList.Add("EmployeeLinkage");
            }
            if (Convert.ToBoolean(_configuration.GetValue<string>("CorporateConcurBooking") ?? "false") && IsCorpBookingPath)
            {
                requestStringList.Add("CorporateCC");
            }
            request.DataToLoad = requestStringList;

            if (mobCPProfileRequest.ReturnAllSavedTravelers || mobCPProfileRequest.CustomerId == 0)
            {
                if (!string.IsNullOrEmpty(mobCPProfileRequest.MileagePlusNumber))
                {
                    request.LoyaltyId = mobCPProfileRequest.MileagePlusNumber;
                }
                else
                {
                    throw new MOBUnitedException("Profile Owner MileagePlus number is required.");
                }
            }
            else
            {
                request.MemberCustomerIdsToLoad = new List<int>();
                request.MemberCustomerIdsToLoad.Add(mobCPProfileRequest.CustomerId);
            }

            return request;
        }

        public async Task<(bool GeneratedCCTokenWithDataVault, string CcDataVaultToken)> GenerateCCTokenWithDataVault(MOBCreditCard creditCardDetails, string sessionID, string token, MOBApplication applicationDetails, string deviceID, string ccDataVaultToken)
        {
            bool generatedCCTokenWithDataVault = false;

            if (!string.IsNullOrEmpty(creditCardDetails.EncryptedCardNumber)) // expecting Client will send only Encrypted Card Number only if the user input is a clear text CC number either for insert CC or update CC not the CC details like CVV number update or expiration date upate no need to call data vault for this type of updates only data vault will be called for CC number update to get the CC token back
            {
                #region
                CslDataVaultRequest dataVaultRequest = await GetMOBDataValutRequest(creditCardDetails, sessionID, applicationDetails);
                string jsonRequest = JsonConvert.SerializeObject(dataVaultRequest);

                string jsonResponse = await _dataVaultService.GetCCTokenWithDataVault(token, jsonRequest, sessionID);

                if (!string.IsNullOrEmpty(jsonResponse))
                {
                    CslDataVaultResponse response = JsonConvert.DeserializeObject<CslDataVaultResponse>(jsonResponse);

                    if (response != null && response.Responses != null && response.Responses[0].Error == null && response.Responses[0].Message != null && response.Responses[0].Message.Count > 0 && response.Responses[0].Message[0].Code.Trim() == "0")
                    {
                        generatedCCTokenWithDataVault = true;
                        if (_configuration.GetValue<bool>("VormetricTokenMigration") && String.IsNullOrEmpty(creditCardDetails.CardType))
                        {
                            CslDataVaultResponse response1 = JsonConvert.DeserializeObject<CslDataVaultResponse>(jsonResponse);
                            var creditCard = response1.Items[0] as Service.Presentation.PaymentModel.CreditCard;
                            creditCardDetails.CardType = creditCard.Code;
                        }
                        else if (!_configuration.GetValue<bool>("DisableCheckForUnionPayFOP_MOBILE13762") && !string.IsNullOrEmpty(creditCardDetails?.CardType))
                        {
                            CslDataVaultResponse response1 = JsonConvert.DeserializeObject<CslDataVaultResponse>(jsonResponse);
                            var creditCard = response1.Items[0] as Service.Presentation.PaymentModel.CreditCard;
                            string[] checkForUnionPayFOP = _configuration.GetValue<string>("CheckForUnionPayFOP")?.Split('|');
                            if (creditCard?.Code == checkForUnionPayFOP?[0])
                            {
                                creditCardDetails.CardType = creditCard.Code;
                                creditCardDetails.CardTypeDescription = checkForUnionPayFOP?[1];
                            }
                        }
                        foreach (var item in response.Items)
                        {
                            ccDataVaultToken = item.AccountNumberToken;
                            item.AccountNumberEncrypted = _configuration.GetValue<bool>("RemoveEncryptedCardNumberForLogs") ? String.Empty : item.AccountNumberEncrypted;
                            break;
                        }
                        _logger.LogInformation("GenerateCCTokenWithDataVault - - DeSerialized Response {response} with {sessionId}", JsonConvert.SerializeObject(response), sessionID);
                    }
                    else
                    {
                        if (response.Responses[0].Error != null && response.Responses[0].Error.Count > 0)
                        {
                            string errorMessage = string.Empty;
                            string errorCode = string.Empty;

                            if (_configuration.GetValue<bool>("EnableBacklogIssueFixes"))
                            {
                                //Datavault returns REST (Dollar Ding) Error - Show appropriate error message //Richa
                                string[] dvErrorCodeAndMessages = _configuration.GetValue<string>("HandleDataVaultErrorCodeAndMessages").Split('|');
                                foreach (var error in response.Responses[0].Error)
                                {
                                    if (dvErrorCodeAndMessages != null)
                                    {
                                        string errorTextFromConfig = dvErrorCodeAndMessages.Any(row => row.Contains(error.Code)) ?
                                                dvErrorCodeAndMessages[Array.FindIndex(dvErrorCodeAndMessages, row => row.Contains(error.Code))].Split('=')[1] :
                                                error.Text;
                                        errorMessage = errorMessage + " " + errorTextFromConfig;
                                        errorCode = error.Code;
                                    }
                                    else
                                    {
                                        errorMessage = errorMessage + " " + error.Text;
                                        errorCode = error.Code;
                                    }
                                }

                            }
                            else
                            {
                                foreach (var error in response.Responses[0].Error)
                                {
                                    errorMessage = errorMessage + " " + error.Text;
                                }
                            }
                            throw new MOBUnitedException(errorCode, errorMessage);
                        }
                        else
                        {
                            string exceptionMessage = _configuration.GetValue<string>("UnableToInsertCreditCardToProfileErrorMessage");
                            if (_configuration.GetValue<string>("ReturnActualExceptionMessageBackForTesting") != null && Convert.ToBoolean(_configuration.GetValue<string>("ReturnActualExceptionMessageBackForTesting")))
                            {
                                exceptionMessage = exceptionMessage + " response.Status not success and response.Errors.Count == 0 - at DAL InsertTravelerCreditCard(MOBUpdateTravelerRequest request)";
                            }
                            throw new MOBUnitedException(exceptionMessage);
                        }
                    }
                }
                else
                {
                    string exceptionMessage = _configuration.GetValue<string>("UnableToInsertCreditCardToProfileErrorMessage").ToString();
                    if (_configuration.GetValue<string>("ReturnActualExceptionMessageBackForTesting") != null && _configuration.GetValue<bool>("ReturnActualExceptionMessageBackForTesting"))
                    {
                        exceptionMessage = exceptionMessage + " - due to jsonResponse is empty at DAL  GenerateCCTokenWithDataVault(MOBUpdateTravelerRequest request, ref string ccDataVaultToken)";
                    }
                    throw new MOBUnitedException(exceptionMessage);
                }
                #endregion
            }
            else if (!string.IsNullOrEmpty(creditCardDetails.AccountNumberToken.Trim()))
            {
                ccDataVaultToken = creditCardDetails.AccountNumberToken;
                generatedCCTokenWithDataVault = true;
            }
            if (_configuration.GetValue<bool>("EnableRemoveEncryptedCardnumber"))
            {
                ConfigUtility.RemoveEncyptedCreditcardNumber(creditCardDetails);
            }
            return (generatedCCTokenWithDataVault, ccDataVaultToken);
        }

        private async Task<MOBVormetricKeys> GetVormetricPersistentTokenForBooking(MOBCreditCard requestCreditCard, string sessionId, string token)
        {
            MOBVormetricKeys vormetricKeys = new MOBVormetricKeys();
            if (_configuration.GetValue<bool>("VormetricTokenMigration"))
            {
                Reservation persistReservation = new Reservation();
                persistReservation = await _sessionHelperService.GetSession<Reservation>(sessionId, persistReservation.ObjectName, new List<string> { sessionId, persistReservation.ObjectName }).ConfigureAwait(false);

                if (persistReservation == null || persistReservation.CreditCards == null)
                {
                    ProfileFOPCreditCardResponse profilePersist = new ProfileFOPCreditCardResponse();
                    profilePersist = await _sessionHelperService.GetSession<ProfileFOPCreditCardResponse>(sessionId, profilePersist.ObjectName, new List<string> { sessionId, profilePersist.ObjectName }).ConfigureAwait(false);

                    if (profilePersist != null && profilePersist.Response != null && profilePersist.Response.SelectedCreditCard != null && !string.IsNullOrEmpty(profilePersist.Response.SelectedCreditCard.PersistentToken))
                    {
                        vormetricKeys.PersistentToken = profilePersist.Response.SelectedCreditCard.PersistentToken;
                        vormetricKeys.SecurityCodeToken = profilePersist.Response.SelectedCreditCard.SecurityCodeToken;
                        vormetricKeys.CardType = profilePersist.Response.SelectedCreditCard.CardType;
                    }

                    MOBPersistFormofPaymentResponse mobPersistFormofPaymentResponse = new MOBPersistFormofPaymentResponse();
                    mobPersistFormofPaymentResponse = await _sessionHelperService.GetSession<MOBPersistFormofPaymentResponse>(sessionId, mobPersistFormofPaymentResponse.ObjectName, new List<string> { sessionId, mobPersistFormofPaymentResponse.ObjectName }).ConfigureAwait(false);

                    if (string.IsNullOrEmpty(vormetricKeys.PersistentToken) &&
                       mobPersistFormofPaymentResponse != null &&
                       mobPersistFormofPaymentResponse.ShoppingCart != null &&
                       mobPersistFormofPaymentResponse.ShoppingCart.FormofPaymentDetails != null &&
                       mobPersistFormofPaymentResponse.ShoppingCart.FormofPaymentDetails.CreditCard != null &&
                       !string.IsNullOrEmpty(mobPersistFormofPaymentResponse.ShoppingCart.FormofPaymentDetails.CreditCard.PersistentToken))
                    {
                        vormetricKeys.PersistentToken = mobPersistFormofPaymentResponse.ShoppingCart.FormofPaymentDetails.CreditCard.PersistentToken;
                        vormetricKeys.SecurityCodeToken = mobPersistFormofPaymentResponse.ShoppingCart.FormofPaymentDetails.CreditCard.SecurityCodeToken;
                        vormetricKeys.CardType = mobPersistFormofPaymentResponse.ShoppingCart.FormofPaymentDetails.CreditCard.CardType;
                    }
                }
                else if (persistReservation.CreditCards != null && persistReservation.CreditCards.Count > 0)   //MOBILE-1193 Booking - Credit Card Checkout with MP Sign IN -Saved FOP
                {
                    if (persistReservation.IsSignedInWithMP) //For MP Sign in
                    {
                        foreach (MOBCreditCard creditCard in persistReservation.CreditCards)
                        {
                            if (requestCreditCard != null && creditCard.Key == requestCreditCard.Key)
                            {
                                vormetricKeys.PersistentToken = creditCard.PersistentToken;
                                vormetricKeys.SecurityCodeToken = creditCard.SecurityCodeToken;
                                vormetricKeys.CardType = creditCard.CardType;
                                break;
                            }
                        }

                        //MOBILE-2490 
                        if (String.IsNullOrEmpty(vormetricKeys.PersistentToken) && !String.IsNullOrEmpty(requestCreditCard.Key))
                        {
                            vormetricKeys = await GetVormetricPersistentTokenFromProfile(requestCreditCard.Key, sessionId, token);
                        }
                    }
                    else if (persistReservation.CreditCards.Exists(cc => cc.AccountNumberToken == requestCreditCard.AccountNumberToken))//MOBILE-1192 Booking - Credit Card Checkout as Guest
                    {
                        var cc = persistReservation.CreditCards.First(c => c.AccountNumberToken == requestCreditCard.AccountNumberToken);
                        vormetricKeys.PersistentToken = cc.PersistentToken;
                        vormetricKeys.SecurityCodeToken = cc.SecurityCodeToken;
                        vormetricKeys.CardType = cc.CardType;
                    }
                }

                if (String.IsNullOrEmpty(vormetricKeys.PersistentToken) && (!string.IsNullOrEmpty(token) || !string.IsNullOrEmpty(_token)) && !string.IsNullOrEmpty(requestCreditCard.AccountNumberToken))
                {
                    if (string.IsNullOrEmpty(token))
                    {
                        token = _token;
                    }
                    vormetricKeys = await GetPersistentTokenUsingAccountNumberToken(requestCreditCard.AccountNumberToken, sessionId, token);
                }

                if (String.IsNullOrEmpty(vormetricKeys.PersistentToken))
                {
                    LogNoPersistentTokenInCSLResponseForVormetricPayment(sessionId);
                }
            }

            return vormetricKeys;
        }

        private async Task<MOBVormetricKeys> GetVormetricPersistentTokenFromProfile(string request_CreditCard_Key, string sessionId, string token)
        {
            MOBVormetricKeys vormetricKeys = new MOBVormetricKeys();
            Mobile.Model.Common.ProfileResponse profilePersistCopy = new Mobile.Model.Common.ProfileResponse();
            profilePersistCopy = await _sessionHelperService.GetSession<Mobile.Model.Common.ProfileResponse>(sessionId, profilePersistCopy.ObjectName, new List<string> { sessionId, profilePersistCopy.ObjectName }).ConfigureAwait(false);
            if (profilePersistCopy != null && profilePersistCopy.Response != null
                && profilePersistCopy.Response.Profiles != null && profilePersistCopy.Response.Profiles.Count > 0
                && profilePersistCopy.Response.Profiles[0].Travelers != null && profilePersistCopy.Response.Profiles[0].Travelers.Count > 0
                && profilePersistCopy.Response.Profiles[0].Travelers[0].CreditCards != null && profilePersistCopy.Response.Profiles[0].Travelers[0].CreditCards.Count > 0)
            {
                foreach (MOBCreditCard creditCard in profilePersistCopy.Response.Profiles[0].Travelers[0].CreditCards)
                {
                    if (creditCard.Key == request_CreditCard_Key)
                    {
                        vormetricKeys.PersistentToken = creditCard.PersistentToken;
                        vormetricKeys.SecurityCodeToken = creditCard.SecurityCodeToken;
                        vormetricKeys.AccountNumberToken = creditCard.AccountNumberToken;
                        vormetricKeys.CardType = creditCard.CardType;
                        break;
                    }
                }
            }

            if (String.IsNullOrEmpty(vormetricKeys.PersistentToken) && !string.IsNullOrEmpty(token))
            {
                vormetricKeys = await GetPersistentTokenUsingAccountNumberToken(vormetricKeys.AccountNumberToken, sessionId, token);
            }

            if (String.IsNullOrEmpty(vormetricKeys.PersistentToken))
            {
                LogNoPersistentTokenInCSLResponseForVormetricPayment(sessionId);
            }

            return vormetricKeys;
        }

        private async Task<MOBVormetricKeys> GetVormetricPersistentTokenForViewRes(MOBCreditCard persistedCreditCard, string sessionId, string token, string flow)
        {
            MOBVormetricKeys vormetricKeys = new MOBVormetricKeys();
            if (_configuration.GetValue<bool>("VormetricTokenMigration"))
            {
                //MOBILE-1243 CFOP MP Sign IN - ViewRes - TPI - CC saved
                ProfileFOPCreditCardResponse profilePersist = new ProfileFOPCreditCardResponse();
                profilePersist = await _sessionHelperService.GetSession<ProfileFOPCreditCardResponse>(sessionId, profilePersist.ObjectName, new List<string> { sessionId, profilePersist.ObjectName }).ConfigureAwait(false);
                if (persistedCreditCard != null && !string.IsNullOrEmpty(persistedCreditCard.Key) &&
                    profilePersist != null &&
                    profilePersist.Response != null &&
                    profilePersist.Response.Profiles != null &&
                    profilePersist.Response.Profiles.Count > 0 &&
                    profilePersist.Response.Profiles[0].Travelers != null &&
                    profilePersist.Response.Profiles[0].Travelers.Count > 0 &&
                    profilePersist.Response.Profiles[0].Travelers[0].CreditCards != null &&
                    profilePersist.Response.Profiles[0].Travelers[0].CreditCards.Count > 0 &&
                        (profilePersist.Response.Profiles[0].Travelers[0].CreditCards.Exists(p => p.Key == persistedCreditCard.Key) ||
                        profilePersist.Response.Profiles[0].Travelers[0].CreditCards.Exists(p => p.AccountNumberToken == persistedCreditCard.AccountNumberToken))

                    )
                {
                    var cc = profilePersist.Response.Profiles[0].Travelers[0].CreditCards.FirstOrDefault(p => p.Key == persistedCreditCard.Key);
                    if (cc == null)
                    {
                        cc = profilePersist.Response.Profiles[0].Travelers[0].CreditCards.FirstOrDefault(p => p.AccountNumberToken == persistedCreditCard.AccountNumberToken);
                    }
                    vormetricKeys.PersistentToken = cc.PersistentToken;
                    vormetricKeys.AccountNumberToken = cc.AccountNumberToken;
                    vormetricKeys.SecurityCodeToken = cc.SecurityCodeToken;
                    vormetricKeys.CardType = cc.CardType;
                }
                else if (profilePersist != null
                    && profilePersist.Response != null
                    && profilePersist.Response.SelectedCreditCard != null
                    && !string.IsNullOrEmpty(profilePersist.Response.SelectedCreditCard.PersistentToken)
                    && (ConfigUtility.IsViewResFlowCheckOut(flow) ? profilePersist?.Response?.SelectedCreditCard?.AccountNumberToken == persistedCreditCard.AccountNumberToken : true))
                {
                    vormetricKeys.PersistentToken = profilePersist.Response.SelectedCreditCard.PersistentToken;
                    vormetricKeys.AccountNumberToken = profilePersist.Response.SelectedCreditCard.AccountNumberToken;
                    vormetricKeys.SecurityCodeToken = profilePersist.Response.SelectedCreditCard.SecurityCodeToken;
                    vormetricKeys.CardType = profilePersist.Response.SelectedCreditCard.CardType;
                }
                //MOBILE-1681 CFOP ON MP Sign IN - ViewRes - PCU - MasterPass
                else if (profilePersist != null
                          && profilePersist.customerProfileResponse != null
                          && profilePersist.customerProfileResponse.SelectedCreditCard != null
                          && !string.IsNullOrEmpty(profilePersist.customerProfileResponse.SelectedCreditCard.PersistentToken)
                          && (ConfigUtility.IsViewResFlowCheckOut(flow) ? profilePersist?.Response?.SelectedCreditCard?.AccountNumberToken == persistedCreditCard.AccountNumberToken : true))
                {
                    vormetricKeys.PersistentToken = profilePersist.customerProfileResponse.SelectedCreditCard.PersistentToken;
                    vormetricKeys.AccountNumberToken = profilePersist.customerProfileResponse.SelectedCreditCard.AccountNumberToken;
                    vormetricKeys.SecurityCodeToken = profilePersist.customerProfileResponse.SelectedCreditCard.SecurityCodeToken;
                    vormetricKeys.CardType = profilePersist.customerProfileResponse.SelectedCreditCard.CardType;
                }
                else if (persistedCreditCard != null && (!string.IsNullOrEmpty(persistedCreditCard.AccountNumberToken) || !string.IsNullOrEmpty(persistedCreditCard.PersistentToken)))
                {
                    vormetricKeys.PersistentToken = persistedCreditCard.PersistentToken;
                    vormetricKeys.AccountNumberToken = persistedCreditCard.AccountNumberToken;
                    vormetricKeys.SecurityCodeToken = persistedCreditCard.SecurityCodeToken;
                    vormetricKeys.CardType = persistedCreditCard.CardType;
                }
                //MOBILE-1238 CFOP Guest Flow - ViewRes - TPI - CC


                if (String.IsNullOrEmpty(vormetricKeys.PersistentToken) && !string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(vormetricKeys.AccountNumberToken))
                {
                    vormetricKeys = await GetPersistentTokenUsingAccountNumberToken(vormetricKeys.AccountNumberToken, sessionId, token);
                }

                if (String.IsNullOrEmpty(vormetricKeys.PersistentToken))
                {
                    LogNoPersistentTokenInCSLResponseForVormetricPayment(sessionId);
                }
            }

            return vormetricKeys;
        }

        private async Task<MOBSHOPReservation> BuildCheckOutReservation(MOBCheckOutRequest checkOutRequest, FlightReservationResponse response, Session session)
        {
            MOBSHOPReservation reservation = new MOBSHOPReservation(_configuration, _cachingService);

            bool isTPIcall = false; // this flag will be true for post booking TPI Purchase. next line of code will determine True or False. 
            isTPIcall = Convert.ToBoolean(_configuration.GetValue<string>("ShowTripInsuranceSwitch") ?? "false")
                && checkOutRequest.IsTPI;
            if (isTPIcall) // ==>> Venkat and Elise chagne code to offer TPI postbooking when inline TPI is off for new clients 2.1.36 and above
            {
                isTPIcall = EnableTPI(checkOutRequest.Application.Id, checkOutRequest.Application.Version.Major, 1);
            }
            bool isSecondaryPaymentForTPI = EnableTPI(checkOutRequest.Application.Id, checkOutRequest.Application.Version.Major, 3) && checkOutRequest.IsTPI && checkOutRequest.IsSecondaryPayment;
            Reservation persistedReservation = new Reservation();
            persistedReservation = await _sessionHelperService.GetSession<Reservation>(checkOutRequest.SessionId, persistedReservation.ObjectName, new List<string> { checkOutRequest.SessionId, persistedReservation.ObjectName }).ConfigureAwait(false);
            List<CMSContentMessage> lstMessages = await _shoppingCartUtility.GetSDLContentByGroupName(checkOutRequest, checkOutRequest.SessionId, session.Token, _configuration.GetValue<string>("CMSContentMessages_GroupName_BookingRTI_Messages"), "BookingPathRTI_CMSContentMessagesCached_StaticGUID");
            if (response != null && (_configuration.GetValue<bool>("CFOP19HBugFixToggle") ?
                         (response.CheckoutResponse != null && (response.CheckoutResponse.Status.Equals(United.Services.FlightShopping.Common.StatusType.Success) && !(isTPIcall || isSecondaryPaymentForTPI))
                               ||
                             (isSecondaryPaymentForTPI))
                         : true))
            {
                reservation.FormOfPaymentType = persistedReservation.FormOfPaymentType;
                if ((persistedReservation.FormOfPaymentType == MOBFormofPayment.PayPal || persistedReservation.FormOfPaymentType == MOBFormofPayment.PayPalCredit) && persistedReservation.PayPal != null)
                {
                    reservation.PayPal = persistedReservation.PayPal;
                    reservation.PayPalPayor = persistedReservation.PayPalPayor;
                }
                if (persistedReservation.ReservationEmail == null)
                    persistedReservation.ReservationEmail = new MOBEmail();
                persistedReservation.ReservationEmail.EmailAddress = (!string.IsNullOrEmpty(checkOutRequest.FormofPaymentDetails.EmailAddress)) ? checkOutRequest.FormofPaymentDetails.EmailAddress.ToString() : (response.Reservation.EmailAddress.Count() > 0) ? response.Reservation.EmailAddress.Where(x => x.Address != null).Select(x => x.Address).FirstOrDefault().ToString() : null;

                MapPersistedReservationToMOBSHOPReservation(reservation, persistedReservation);
                if (!IsEnableEnableCombineConfirmationAlerts(checkOutRequest.Application.Id, checkOutRequest.Application.Version.Major))
                {
                    reservation.Messages = new List<string>();
                    reservation.Messages.Add(string.Format(_configuration.GetValue<string>("EmailConfirmationMessage"), (!string.IsNullOrEmpty(checkOutRequest.FormofPaymentDetails.EmailAddress)) ? checkOutRequest.FormofPaymentDetails.EmailAddress.ToString() : (response.Reservation.EmailAddress.Count() > 0) ? response.Reservation.EmailAddress.Where(x => x.Address != null).Select(x => x.Address).FirstOrDefault().ToString() : null));

                    //Need a empty list in onprem 22I 
                    //reservation.Messages.Add(string.Format(_configuration.GetValue<string>("EmailConfirmationMessage"), (!string.IsNullOrEmpty(checkOutRequest.FormofPaymentDetails.EmailAddress)) ? checkOutRequest.FormofPaymentDetails.EmailAddress.ToString() : (response.Reservation.EmailAddress.Count() > 0) ? response.Reservation.EmailAddress.Where(x => x.Address != null).Select(x => x.Address).FirstOrDefault().ToString() : null));
                }
                if (ConfigUtility.EnableInflightContactlessPayment(checkOutRequest.Application.Id, checkOutRequest.Application.Version.Major, checkOutRequest?.Flow?.ToUpper() != FlowType.BOOKING.ToString()))
                {
                    if ((response?.CheckoutResponse?.ShoppingCartResponse?.Status?.ToUpper()?.Contains("SAVE PAX WALLET FAILED") ?? false) || _configuration.GetValue<bool>("InflightContactlessCCSaveFailed"))
                    {
                        if (IsEnableEnableCombineConfirmationAlerts(checkOutRequest.Application.Id, checkOutRequest.Application.Version.Major))
                        {
                            AddConfirmationScreenAlertMessage(reservation.ShopReservationInfo2, "Attention", _configuration.GetValue<string>("InflightContactlessPaymentSaveFiledMessage"), "", MOBCONFIRMATIONALERTMESSAGEORDER.SAVEWALLETFAILURE.ToString());
                        }
                        else
                        {
                            reservation.Messages.Add(_configuration.GetValue<string>("InflightContactlessPaymentSaveFiledMessage"));
                        }
                    }
                }


                #region Corporate booking confirmation message
                if (Convert.ToBoolean(_configuration.GetValue<string>("CorporateConcurBooking") ?? "false"))
                {
                    ShoppingResponse shopResponse = new ShoppingResponse();
                    shopResponse = await _sessionHelperService.GetSession<ShoppingResponse>(checkOutRequest.SessionId, shopResponse.ObjectName, new List<string> { checkOutRequest.SessionId, shopResponse.ObjectName }).ConfigureAwait(false);
                    if (shopResponse != null)
                    {
                        if (shopResponse.Request.IsCorporateBooking)
                        {
                            string corpCompany = shopResponse.Request.MOBCPCorporateDetails.CorporateTravelProvider;
                            string CorporateBookingConfirmationMessage = await _featureSettings.GetFeatureSettingValue("EnableSuppressingCompanyNameForBusiness").ConfigureAwait(false) ? string.Empty : string.Format(_configuration.GetValue<string>("CorporateBookingConfirmationMessage"), corpCompany);
                            reservation.ShopReservationInfo.CorporateBookingConfirmationMessage = CorporateBookingConfirmationMessage;
                        }
                    }
                }
                #endregion

                if (reservation.FareLock != null)
                {

                }
                bool caExists = false; bool meExists = false;
                string foreignCountry = string.Empty;
                if (response.DisplayCart != null && response.DisplayCart.DisplayTrips != null)
                {
                    for (int i = 0; i < response.DisplayCart.DisplayTrips.Count; i++)
                    {
                        if (response.DisplayCart.DisplayTrips[i].InternationalCitiesExist)
                        {
                            foreignCountry = reservation.Trips[i].DestinationDecoded;
                            foreach (United.Services.FlightShopping.Common.Flight seg in response.DisplayCart.DisplayTrips[i].Flights)
                            {
                                if (seg.International)
                                {
                                    if (seg.DestinationCountryCode == "CA")
                                    {
                                        caExists = true;
                                    }
                                    if (seg.DestinationCountryCode == "MX")
                                    {
                                        meExists = true;
                                    }
                                }

                            }
                        }
                        break;
                    }

                }
                string operatingAirline = string.Empty;

                var listOfCodeShareCarriers = (from flightseg in response.Reservation.FlightSegments
                                               where flightseg.FlightSegment.Message.Any(x => x.Code.Contains("CSH"))
                                               from message in flightseg.FlightSegment.Message
                                               where message.Code == "OPERATINGCARRIERDESC"
                                               select message.Text).Distinct().ToList();
                if (listOfCodeShareCarriers != null && listOfCodeShareCarriers.Count > 0)
                {
                    if (listOfCodeShareCarriers.Count > 1)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (var carrier in listOfCodeShareCarriers)
                        {
                            sb.Append(carrier);
                            sb.Append(",");
                        }
                        operatingAirline = sb.ToString().Substring(0, sb.Length - 1);
                    }
                    else
                    {
                        operatingAirline = listOfCodeShareCarriers[0];
                    }
                }
                if (!string.IsNullOrEmpty(operatingAirline))
                {
                    if (IsEnableEnableCombineConfirmationAlerts(checkOutRequest.Application.Id, checkOutRequest.Application.Version.Major))
                    {
                        if (_configuration.GetValue<bool>("EnableAirlinesFareComparison"))
                        {
                            var airline = reservation?.Trips?.SelectMany(t => t.FlattenedFlights)?.SelectMany(t2 => t2.Flights).FirstOrDefault(t1 => _configuration.GetValue<string>("SupportedAirlinesFareComparison").Contains(t1.OperatingCarrier))?.OperatingCarrier;
                            if (!string.IsNullOrEmpty(airline))
                            {
                                string operatingCarrier = airline.ToUpper() == "XE" ? "JSX" : operatingAirline;


                                var sdlMessage = GetSDLMessageFromList(lstMessages, _configuration.GetValue<string>("PartnerCarrierPurchaseConfirmationInfoSDL"))?.FirstOrDefault()?.ContentFull;
                                if (!string.IsNullOrEmpty(sdlMessage))
                                {
                                    sdlMessage = string.Format(sdlMessage, operatingCarrier);
                                }
                                else
                                {
                                    sdlMessage = string.Format(_configuration.GetValue<string>("PartnerCarrierBookingConfirmationInfo"), operatingCarrier);
                                }
                                AddConfirmationScreenAlertMessage(reservation.ShopReservationInfo2, "Attention", sdlMessage, "", MOBCONFIRMATIONALERTMESSAGEORDER.RESERVATIONON24HOURHOLD.ToString());
                            }
                            else
                            {
                                AddConfirmationScreenAlertMessage(reservation.ShopReservationInfo2, "Attention", string.Format(_configuration.GetValue<string>("ReservationOn24HourHoldMessageText"), operatingAirline, operatingAirline), "", MOBCONFIRMATIONALERTMESSAGEORDER.RESERVATIONON24HOURHOLD.ToString());
                            }
                        }
                        else
                        {
                            AddConfirmationScreenAlertMessage(reservation.ShopReservationInfo2, "Attention", string.Format(_configuration.GetValue<string>("ReservationOn24HourHoldMessageText"), operatingAirline, operatingAirline), "", MOBCONFIRMATIONALERTMESSAGEORDER.RESERVATIONON24HOURHOLD.ToString());
                        }

                    }
                    else
                    {
                        reservation.Messages.Add(string.Format(_configuration.GetValue<string>("ReservationOn24HourHoldMessageText"), operatingAirline, operatingAirline));
                    }
                }

                if (_configuration.GetValue<bool>("IsEnableTravelAdvisoryForInternationalMarket") && !string.IsNullOrEmpty(foreignCountry))
                {
                    string docTitles = "'TripAdvisoryForSelectedOriginDestination','TripAdvisoryForCanada','TripAdvisoryForMexico'";
                    List<United.Definition.MOBLegalDocument> docs = await _legalDocumentsForTitlesService.GetNewLegalDocumentsForTitles(docTitles, _headers.ContextValues.TransactionId, false);

                    if (docs != null && docs.Count > 0)
                    {

                        if (caExists && docs.Find(item => item.Title == "TripAdvisoryForCanada") != null)
                        {
                            if (IsEnableEnableCombineConfirmationAlerts(checkOutRequest.Application.Id, checkOutRequest.Application.Version.Major))
                            {
                                AddConfirmationScreenAlertMessage(reservation.ShopReservationInfo2, "Attention", docs.Find(item => item.Title == "TripAdvisoryForCanada").LegalDocument, "", MOBCONFIRMATIONALERTMESSAGEORDER.ADVISORY.ToString());
                            }
                            else
                            {
                                reservation.Messages.Add(docs.Find(item => item.Title == "TripAdvisoryForCanada").LegalDocument);
                            }
                        }
                        else if (meExists && docs.Find(item => item.Title == "TripAdvisoryForMexico") != null)
                        {
                            if (IsEnableEnableCombineConfirmationAlerts(checkOutRequest.Application.Id, checkOutRequest.Application.Version.Major))
                            {
                                AddConfirmationScreenAlertMessage(reservation.ShopReservationInfo2, "Attention", docs.Find(item => item.Title == "TripAdvisoryForMexico").LegalDocument, "", MOBCONFIRMATIONALERTMESSAGEORDER.ADVISORY.ToString());
                            }
                            else
                            {
                                reservation.Messages.Add(docs.Find(item => item.Title == "TripAdvisoryForMexico").LegalDocument);
                            }
                        }

                        else if (docs.Find(item => item.Title == "TripAdvisoryForSelectedOriginDestination") != null)
                        {
                            if (IsEnableEnableCombineConfirmationAlerts(checkOutRequest.Application.Id, checkOutRequest.Application.Version.Major))
                            {
                                AddConfirmationScreenAlertMessage(reservation.ShopReservationInfo2, "Attention", string.Format(docs.Find(item => item.Title == "TripAdvisoryForSelectedOriginDestination").LegalDocument, foreignCountry), "", MOBCONFIRMATIONALERTMESSAGEORDER.ADVISORY.ToString());
                            }
                            else
                            {
                                reservation.Messages.Add(string.Format(docs.Find(item => item.Title == "TripAdvisoryForSelectedOriginDestination").LegalDocument, foreignCountry));
                            }
                        }
                    }
                }

                bool totalExist = false;
                decimal flightTotal = 0;

                foreach (MOBSHOPPrice price in reservation.Prices)
                {
                    if (price.DisplayType.ToUpper() == "GRAND TOTAL")
                    {
                        totalExist = true;
                        break;
                    }
                    if (price.DisplayType.ToUpper() == "TOTAL")
                    {
                        flightTotal = Convert.ToDecimal(price.DisplayValue);
                    }
                }

                if (!totalExist)
                {
                    MOBSHOPPrice totalPrice = new MOBSHOPPrice();
                    totalPrice.CurrencyCode = reservation.Prices[0].CurrencyCode;
                    totalPrice.DisplayType = "Grand Total";
                    totalPrice.DisplayValue = string.Format("{0:#,0.00}", flightTotal);
                    totalPrice.FormattedDisplayValue = (flightTotal).ToString("C2", CultureInfo.CurrentCulture);
                    totalPrice.PriceType = "Grand Total";
                    double tempDouble = 0;
                    double.TryParse(flightTotal.ToString(), out tempDouble);
                    totalPrice.Value = Math.Round(tempDouble, 2, MidpointRounding.AwayFromZero);
                    reservation.Prices.Add(totalPrice);
                }
                reservation.Taxes = persistedReservation.Taxes;

                if (response.CheckoutResponse != null && response.CheckoutResponse.ShoppingCartResponse != null)
                {
                    reservation.RecordLocator = string.IsNullOrEmpty(response.Reservation.ConfirmationID) ? response.CheckoutResponse.ShoppingCartResponse.ConfirmationID : response.Reservation.ConfirmationID;

                    if (string.IsNullOrEmpty(reservation.RecordLocator))
                    {

                        MOBExceptionWrapper exceptionWrapper = new MOBExceptionWrapper(new System.Exception("No PNR was found in the response."));
                        _logger.LogInformation("BuildCheckOutReservation - Exception {exceptionWrapper} with {sessionId}", exceptionWrapper, checkOutRequest.SessionId);
                    }

                    reservation = await PopulateProducts(reservation, response, checkOutRequest.SessionId, checkOutRequest.Application.Id, checkOutRequest.Application.Version.Major);

                }

                if (_configuration.GetValue<string>("AddFareLockEmail") != null && _configuration.GetValue<string>("AddFareLockEmail").ToString().ToUpper().Trim() == "TRUE" && reservation.TravelOptions != null)
                {
                    var farelock = reservation.TravelOptions.Find(item => item.Key.ToUpper() == "FARELOCK");
                    string pnrcreatedatetime = string.Empty;
                    string farelockoffertype = string.Empty;
                    if (farelock != null)
                    {
                        if (response.CheckoutResponse.ShoppingCartResponse.TimeStamps != null)
                        {
                            var timeStamp = response.CheckoutResponse.ShoppingCartResponse.TimeStamps.Where(x => x.Type.ToUpper() == "GMTDATETIMECREATED");
                            if (timeStamp != null && timeStamp.Count() > 0)
                            {
                                pnrcreatedatetime = timeStamp.First().Time;
                            }
                        }
                        farelockoffertype = farelock.SubItems.Find(item1 => item1.Key.ToUpper() == "FARELOCKHOLDDAYS").Amount + "D";
                        AddFareLockEmail(farelockoffertype, checkOutRequest.FormofPaymentDetails.EmailAddress, reservation.RecordLocator, pnrcreatedatetime);
                    }
                }

                if (_configuration.GetValue<bool>("TPIPostBookingCountryCodeCorrection"))
                {
                    reservation.PointOfSale = (checkOutRequest.FormofPaymentDetails.BillingAddress != null) ? checkOutRequest.FormofPaymentDetails.BillingAddress.Country.Code : "US";
                }
                if (reservation.ShopReservationInfo2 == null)
                {
                    reservation.ShopReservationInfo2 = new MOBSHOPReservationInfo2();
                }

                await FireToGetPremiumCabinUpgrade(checkOutRequest, reservation, session);
                //[sandeep] WI:AO-878 PB Tile Should Not Display When Farelock is Selected
                if (!TravelOptionsContainsFareLock(reservation.TravelOptions))
                {
                    string countryCode = (checkOutRequest.FormofPaymentDetails.BillingAddress != null) ? checkOutRequest.FormofPaymentDetails.BillingAddress.Country.Code : null;
                    reservation.ShopReservationInfo2.PriorityBoarding = IsShowPBSOffer(persistedReservation.IsReshopChange)
                                                                         ? await AssignPBToReservation(checkOutRequest, session.Token, checkOutRequest.CartId, checkOutRequest.LanguageCode, countryCode)
                                                                         : null;
                }
                try
                {
                    #region CTO Tile Change
                    if (await _featureToggles.IsEnableConfirmationCTOTile(checkOutRequest.Application.Id, checkOutRequest.Application.Version.Major))
                    {
                        reservation.ShopReservationInfo2.SafCTOTileInfo = AddSAFCTOTile(lstMessages);
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    var exceptionWrapper = new MOBExceptionWrapper(ex);
                    _logger.ILoggerError("Checkout CTO Tile Softfailure exception {message} {StackTrace}", ex.Message, ex.StackTrace);
                }

            }

            if (isSecondaryPaymentForTPI)
            {
                if (response != null && !(response.Errors != null && response.Errors.Count > 0))
                {
                    reservation.Prices = new List<MOBSHOPPrice>() { new MOBSHOPPrice() { DisplayType = "Travel Insurance", PriceTypeDescription = "secondary payment", Value = Convert.ToDouble(checkOutRequest.PaymentAmount) } };
                    // When payment successful, the payment log will include the correct sessionId. 
                    reservation.SessionId = session.SessionId;
                    reservation.RecordLocator = response.Reservation.ConfirmationID;
                    reservation.IsBookingCommonFOPEnabled = persistedReservation.IsBookingCommonFOPEnabled;
                }
            }
            else if (response != null && !(response.Errors != null && response.Errors.Count > 0) && isTPIcall)
            {
                // Buy TPI success
                reservation.Prices = new List<MOBSHOPPrice>() { new MOBSHOPPrice() { DisplayType = "Trip Insurance", Value = Convert.ToDouble(checkOutRequest.PaymentAmount) } };
                // When payment successful, the payment log will include the correct sessionId. 
                reservation.SessionId = session.SessionId;
            }

            #region Special Needs

            if (ConfigUtility.EnableSpecialNeeds(checkOutRequest.Application.Id, checkOutRequest.Application.Version.Major))
            {
                if (response != null && response.Status.Equals(United.Services.FlightShopping.Common.StatusType.Success) && response.DisplayCart != null)
                {
                    if (reservation.ShopReservationInfo2 == null)
                        reservation.ShopReservationInfo2 = new MOBSHOPReservationInfo2();

                    reservation.ShopReservationInfo2.PurchaseToTravelTimeIsWithinSevenDays = response.DisplayCart.SSRServiceFlag;
                }
            }

            #endregion
            try
            {
                reservation.FormOfPaymentType = (MOBFormofPayment)System.Enum.Parse(typeof(MOBFormofPayment), checkOutRequest.FormofPaymentDetails.FormOfPaymentType, true);
                var visaCheckOutCallID = string.Empty;
                if (response.CheckoutResponse != null && response.CheckoutResponse.Status.Equals(United.Services.FlightShopping.Common.StatusType.Success))
                {
                    await InsertPayment(reservation, checkOutRequest.TransactionId, checkOutRequest.Application.Id, checkOutRequest.Application.Version.Major, checkOutRequest.DeviceId, checkOutRequest.MileagePlusNumber, checkOutRequest.FareLockAutoTicket, checkOutRequest.FormofPaymentDetails.FormOfPaymentType, session, checkOutRequest.Flow).ConfigureAwait(false);
                }
            }
            catch (System.Exception) { }

            return reservation;
        }

        private MOBOfferTile AddSAFCTOTile(List<CMSContentMessage> messages)
        {
            var imageUrl = GetSDLMessageFromList(messages, "MOB_Confirmation_CTOTileImage");
            var buttonText = GetSDLMessageFromList(messages, "MOB_Confirmation_CTOTileButtonText");
            if (imageUrl != null && buttonText != null && imageUrl.Count > 0 && buttonText.Count > 0)
            {
                MOBOfferTile tile = new MOBOfferTile();
                tile.Title = imageUrl.FirstOrDefault().ContentFull;
                tile.Text1 = imageUrl.FirstOrDefault().ContentShort;
                tile.Text2 = buttonText.FirstOrDefault().ContentFull;
                tile.Text3 = buttonText.FirstOrDefault().ContentShort;
                return tile;
            }
            return null;
        }
        private async Task InsertPayment(MOBSHOPReservation reservation, string transactionId, int applicationId, string applicationVersion, string deviceId, string mileagePlusNumber, bool fareLockAutoTicket, string formOfPayment, Session session, string flow)
        {
            if (reservation != null)
            {
                string xmlRemark = XmlSerializerHelper.Serialize<MOBSHOPReservation>(reservation);
                int mileage = 0;
                double totalAmount = 0;
                bool farelockExist = false;
                bool isTPIcall = false;
                bool isEplusAncillary = EnableEPlusAncillary(applicationId, applicationVersion, reservation.IsReshopChange) &&
                         reservation.TravelOptions != null &&
                         reservation.TravelOptions.Exists(t => t?.Key?.Trim()?.ToUpper() == "EFS");

                foreach (MOBSHOPPrice price in reservation.Prices)
                {
                    switch (price.DisplayType.ToUpper())
                    {
                        case "TOTAL":
                            totalAmount = price.Value;
                            break;
                        case "MILES":
                            mileage = Convert.ToInt32(price.Value);
                            break;
                        case "ECONOMYPLUS SEATS":
                            await AddPaymentNew(transactionId, applicationId, applicationVersion, flow + "-MobileShoppingCart- Travel Options - Seats", price.Value, "USD", 0, xmlRemark, applicationId.ToString(), Convert.ToBoolean(_configuration.GetValue<string>("IsBookingTest")), reservation.SessionId, deviceId, reservation.RecordLocator, mileagePlusNumber, formOfPayment).ConfigureAwait(false);
                            break;
                        case "ADVANCE SEAT ASSIGNMENT":
                            await AddPaymentNew(transactionId, applicationId, applicationVersion, flow + "-MobileShoppingCart- Travel Options - Advance Seat Assignments", price.Value, "USD", 0, xmlRemark, applicationId.ToString(), Convert.ToBoolean(_configuration.GetValue<string>("IsBookingTest")), reservation.SessionId, deviceId, reservation.RecordLocator, mileagePlusNumber, formOfPayment).ConfigureAwait(false);
                            break;
                        case "TRAVEL OPTIONS":
                            // The below code is modified by prasad to change the payment log text in uatb_payment for bundles
                            if (isEplusAncillary)
                            {
                                await AddPaymentNew(transactionId, applicationId, applicationVersion, flow + "-MobileShoppingCart- Travel Options - EFS", price.Value, "USD", 0, xmlRemark, applicationId.ToString(), Convert.ToBoolean(_configuration.GetValue<string>("IsBookingTest")), reservation.SessionId, deviceId, reservation.RecordLocator, mileagePlusNumber, formOfPayment).ConfigureAwait(false);
                            }
                            else if (_configuration.GetValue<bool>("EnableBundlePaymentLogText") && _configuration.GetValue<bool>("EnableDynamicBundles"))
                            {
                                bool isSupportedVersion = isApplicationVersionGreaterForBundles(applicationId, applicationVersion, "AndroidBundlePaymentLogtextChange", "IOSBundlePaymentLogtextChange", "", "");
                                if (isSupportedVersion)
                                {
                                    await AddPaymentNew(transactionId, applicationId, applicationVersion, flow + "-MobileShoppingCart- Travel Options - Bundles", price.Value, "USD", 0, xmlRemark, applicationId.ToString(), Convert.ToBoolean(_configuration.GetValue<string>("IsBookingTest")), reservation.SessionId, deviceId, reservation.RecordLocator, mileagePlusNumber, formOfPayment).ConfigureAwait(false);
                                }
                                else
                                {
                                    await AddPaymentNew(transactionId, applicationId, applicationVersion, flow + "-MobileShoppingCart- Travel Options - Premier Access", price.Value, "USD", 0, xmlRemark, applicationId.ToString(), Convert.ToBoolean(_configuration.GetValue<string>("IsBookingTest")), reservation.SessionId, deviceId, reservation.RecordLocator, mileagePlusNumber, formOfPayment).ConfigureAwait(false);
                                }
                            }
                            else
                            {
                                await AddPaymentNew(transactionId, applicationId, applicationVersion, flow + "-MobileShoppingCart- Travel Options - Premier Access", price.Value, "USD", 0, xmlRemark, applicationId.ToString(), Convert.ToBoolean(_configuration.GetValue<string>("IsBookingTest")), reservation.SessionId, deviceId, reservation.RecordLocator, mileagePlusNumber, formOfPayment).ConfigureAwait(false);
                            }
                            break;
                        case "FARELOCK":
                            farelockExist = true;
                            await AddPaymentNew(transactionId, applicationId, applicationVersion, flow + "-MobileShoppingCart- Travel Options - Farelock", price.Value, "USD", 0, xmlRemark, applicationId.ToString(), Convert.ToBoolean(_configuration.GetValue<string>("IsBookingTest")), reservation.SessionId, deviceId, reservation.RecordLocator, mileagePlusNumber, formOfPayment).ConfigureAwait(false);
                            break;
                        case "TRIP INSURANCE":
                            await AddPaymentNew(transactionId, applicationId, applicationVersion, flow + "-MobileShoppingCart- Travel Options - Trip Insurance", price.Value, "USD", 0, xmlRemark, applicationId.ToString(), Convert.ToBoolean(_configuration.GetValue<string>("IsBookingTest")), reservation.SessionId, deviceId, reservation.RecordLocator, mileagePlusNumber, formOfPayment).ConfigureAwait(false);
                            isTPIcall = true;
                            break;
                        case "TRAVEL INSURANCE":
                            if (!string.IsNullOrEmpty(price.PriceTypeDescription) && price.PriceTypeDescription.ToUpper().Trim() == "SECONDARY PAYMENT")
                            {
                                await AddPaymentNew(transactionId, applicationId, applicationVersion, flow + "-MobileShoppingCart- Travel Options - Trip Insurance in post booking flow", price.Value, "USD", 0, xmlRemark, applicationId.ToString(), Convert.ToBoolean(_configuration.GetValue<string>("IsBookingTest")), reservation.SessionId, deviceId, reservation.RecordLocator, mileagePlusNumber, formOfPayment).ConfigureAwait(false);
                                isTPIcall = true;
                            }
                            else
                            {
                                await AddPaymentNew(transactionId, applicationId, applicationVersion, flow + "-MobileShoppingCart- Travel Options - Trip Insurance in booking flow", price.Value, "USD", 0, xmlRemark, applicationId.ToString(), Convert.ToBoolean(_configuration.GetValue<string>("IsBookingTest")), reservation.SessionId, deviceId, reservation.RecordLocator, mileagePlusNumber, formOfPayment).ConfigureAwait(false);
                            }
                            break;
                        case "CERTIFICATE":
                            if (ConfigUtility.IsETCCombinabilityEnabled(applicationId, applicationVersion))
                            {
                                var seatPrice = reservation.Prices.Find(p => p.DisplayType.ToUpper().Contains("SEAT"));
                                totalAmount = totalAmount - (price.Value - (seatPrice != null ? seatPrice.Value : 0));
                            }
                            else
                            {
                                totalAmount -= price.Value;
                            }
                            await AddPaymentNew(transactionId, applicationId, applicationVersion, flow + "-MobileShoppingCart- ETC", price.Value, "USD", 0, xmlRemark, applicationId.ToString(), Convert.ToBoolean(_configuration.GetValue<string>("IsBookingTest")), reservation.SessionId, deviceId, reservation.RecordLocator, mileagePlusNumber, "ETC").ConfigureAwait(false);
                            break;
                        case "FFC":
                            totalAmount -= price.Value;
                            await AddPaymentNew(transactionId, applicationId, applicationVersion, flow + "-MobileShoppingCart- FFC", price.Value, "USD", 0, xmlRemark, applicationId.ToString(), Convert.ToBoolean(_configuration.GetValue<string>("IsBookingTest")), reservation.SessionId, deviceId, reservation.RecordLocator, mileagePlusNumber, "FFC").ConfigureAwait(false);
                            break;
                        case "MILESANDMONEY":
                            await AddPaymentNew(transactionId, applicationId, applicationVersion, flow + "-MobileShoppingCart- MM", price.Value, "USD", 0, xmlRemark, applicationId.ToString(), Convert.ToBoolean(_configuration.GetValue<string>("IsBookingTest")), reservation.SessionId, deviceId, reservation.RecordLocator, mileagePlusNumber, "MONEYPLUSMILES").ConfigureAwait(false);
                            break;
                        case "TB":
                            await AddPaymentNew(transactionId, applicationId, applicationVersion, flow + "-MobileShoppingCart- " + price.DisplayType.ToUpper(), price.Value, "USD", 0, xmlRemark, applicationId.ToString(), Convert.ToBoolean(_configuration.GetValue<string>("IsBookingTest")), reservation.SessionId, deviceId, reservation.RecordLocator, mileagePlusNumber, price.DisplayType.ToUpper()).ConfigureAwait(false);
                            if (!_configuration.GetValue<bool>("DisableDuplicateTBFullPaymentInsert_MOBILE16759"))
                                totalAmount -= price.Value;
                            break;
                    }
                    if (_configuration.GetValue<bool>("EnableCouponsforBooking"))
                    {
                        if (price != null && price.PromoDetails != null && !string.IsNullOrEmpty(price.DisplayType) && !(price.DisplayType.ToUpper() == "GRAND TOTAL" || price.DisplayType.ToUpper() == "TRAVELERPRICE"))
                        {
                            await AddPaymentNew(transactionId, applicationId, applicationVersion, flow + "-MobileShoppingCart-PromoCode-" + price.DisplayType, price.PromoDetails.PromoValue, "USD", 0, xmlRemark, applicationId.ToString(), Convert.ToBoolean(_configuration.GetValue<string>("IsBookingTest")), reservation.SessionId, deviceId, reservation.RecordLocator, mileagePlusNumber, formOfPayment).ConfigureAwait(false);
                        }
                    }
                }

                if (!isTPIcall)
                {
                    string tpBooking = "";
                    if (_configuration.GetValue<bool>("EnablePaymentLogTripPlan"))
                    {
                        tpBooking = (session?.TravelType == TravelType.TPBooking.ToString() ? "-TP" : "");
                    }
                    if (!farelockExist)
                    {
                        if (!_configuration.GetValue<bool>("DisableDuplicateTBFullPaymentInsert_MOBILE16759") && formOfPayment.ToUpper() == "TB")
                        { }
                        else
                            await AddPaymentNew(transactionId, applicationId, applicationVersion, flow + "-MobileShoppingCart" + tpBooking, totalAmount, "USD", mileage, xmlRemark, applicationId.ToString(), Convert.ToBoolean(_configuration.GetValue<string>("IsBookingTest")), reservation.SessionId, deviceId, reservation.RecordLocator, mileagePlusNumber, formOfPayment).ConfigureAwait(false);
                    }
                    else
                    {
                        if (fareLockAutoTicket)
                        {
                            await AddPaymentNew(transactionId, applicationId, applicationVersion, flow + "-MobileShoppingCart- FarelockAutoTicket" + tpBooking, totalAmount, "USD", mileage, xmlRemark, applicationId.ToString(), Convert.ToBoolean(_configuration.GetValue<string>("IsBookingTest")), reservation.SessionId, deviceId, reservation.RecordLocator, mileagePlusNumber, formOfPayment).ConfigureAwait(false);
                        }
                        else
                        {
                            await AddPaymentNew(transactionId, applicationId, applicationVersion, flow + "-MobileShoppingCart- Farelock" + tpBooking, totalAmount, "USD", mileage, xmlRemark, applicationId.ToString(), Convert.ToBoolean(_configuration.GetValue<string>("IsBookingTest")), reservation.SessionId, deviceId, reservation.RecordLocator, mileagePlusNumber, formOfPayment).ConfigureAwait(false);
                        }
                    }
                }
            }
        }

        private bool isApplicationVersionGreaterForBundles(int applicationID, string appVersion, string androidnontfaversion,
                    string iphonenontfaversion, string windowsnontfaversion, string mWebNonELFVersion)
        {
            #region Prasad Code for version check
            bool ValidTFAVersion = false;
            if (!string.IsNullOrEmpty(appVersion))
            {
                string AndroidNonTFAVersion = _configuration.GetValue<string>(androidnontfaversion) ?? "";
                string iPhoneNonTFAVersion = _configuration.GetValue<string>(iphonenontfaversion) ?? "";
                string WindowsNonTFAVersion = _configuration.GetValue<string>(windowsnontfaversion) ?? "";
                string MWebNonTFAVersion = _configuration.GetValue<string>(mWebNonELFVersion) ?? "";

                Regex regex = new Regex("[0-9.]");
                appVersion = string.Join("",
                    regex.Matches(appVersion).Cast<Match>().Select(match => match.Value).ToArray());
                if (applicationID == 1 && appVersion != iPhoneNonTFAVersion)
                {
                    ValidTFAVersion = GeneralHelper.IsVersion1Greater(appVersion, iPhoneNonTFAVersion);
                }
                else if (applicationID == 2 && appVersion != AndroidNonTFAVersion)
                {
                    ValidTFAVersion = GeneralHelper.IsVersion1Greater(appVersion, AndroidNonTFAVersion);
                }
                else if (applicationID == 6 && appVersion != WindowsNonTFAVersion)
                {
                    ValidTFAVersion = GeneralHelper.IsVersion1Greater(appVersion, WindowsNonTFAVersion);
                }
                else if (applicationID == 16 && appVersion != MWebNonTFAVersion)
                {
                    ValidTFAVersion = GeneralHelper.IsVersion1Greater(appVersion, MWebNonTFAVersion);
                }
            }
            #endregion

            return ValidTFAVersion;
        }

        private MOBSHOPUnfinishedBooking MapUnfinishedBookingFromMOBSHOPReservation(MOBSHOPReservation reservation)
        {
            var result = new MOBSHOPUnfinishedBooking
            {
                IsELF = reservation.IsELF,
                CountryCode = reservation.PointOfSale,
                SearchType = reservation.SearchType,
                NumberOfAdults = reservation.NumberOfTravelers,
                TravelerTypes = reservation.ShopReservationInfo2.TravelerTypes,
                SearchExecutionDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time")).ToString("G")
            };
            result.Trips = reservation.Trips.Select(MapToUnfinishedBookingTripFromMOBSHOPTrip).ToList();

            return result;
        }

        private MOBSHOPUnfinishedBookingTrip MapToUnfinishedBookingTripFromMOBSHOPTrip(MOBSHOPTrip trip)
        {
            var ubTrip = new MOBSHOPUnfinishedBookingTrip
            {
                DepartDate = trip.FlattenedFlights.First().Flights.First().DepartDate,
                DepartTime = trip.FlattenedFlights.First().Flights.First().DepartTime,
                ArrivalDate = trip.FlattenedFlights.First().Flights.First().DestinationDate,
                ArrivalTime = trip.FlattenedFlights.First().Flights.First().DestinationTime,
                Destination = trip.Destination,
                Origin = trip.Origin,
                Flights = trip.FlattenedFlights.First().Flights.Select(MapToUnfinishedBookingFlightFromMOBSHOPFlight).ToList(),
            };

            return ubTrip;
        }

        private MOBSHOPUnfinishedBookingFlight MapToUnfinishedBookingFlightFromMOBSHOPFlight(MOBSHOPFlight shopFlight)
        {
            var ubFlight = new MOBSHOPUnfinishedBookingFlight
            {
                BookingCode = shopFlight.ServiceClass,
                DepartDateTime = shopFlight.DepartureDateTime,
                Origin = shopFlight.Origin,
                Destination = shopFlight.Destination,
                FlightNumber = shopFlight.FlightNumber,
                MarketingCarrier = shopFlight.MarketingCarrier,
                ProductType = shopFlight.ShoppingProducts.First().Type,
            };

            if (shopFlight.Connections != null)
                ubFlight.Connections = shopFlight.Connections.Select(MapToUnfinishedBookingFlightFromMOBSHOPFlight).ToList();

            return ubFlight;
        }

        private void MapPersistedReservationToMOBSHOPReservation(MOBSHOPReservation reservation, Reservation persistedReservation)
        {
            reservation.TravelersCSL = new List<MOBCPTraveler>();

            if (persistedReservation == null) return;

            if (persistedReservation?.TravelerKeys != null && persistedReservation.TravelerKeys.Any())
            {
                foreach (var travelerKey in persistedReservation.TravelerKeys)
                {
                    var traveler = persistedReservation.TravelersCSL[travelerKey];
                    reservation.TravelersCSL.Add(traveler);
                }
            }
            reservation.NumberOfTravelers = persistedReservation.NumberOfTravelers;
            reservation.IsSignedInWithMP = persistedReservation.IsSignedInWithMP;
            reservation.SessionId = persistedReservation.SessionId;
            reservation.SearchType = persistedReservation.SearchType;
            reservation.TravelersRegistered = persistedReservation.TravelersRegistered;
            reservation.Trips = persistedReservation.Trips;
            reservation.Prices = persistedReservation.Prices;
            reservation.CartId = persistedReservation.CartId;
            reservation.CreditCards = persistedReservation.CreditCards.Clone();
            reservation.ReservationEmail = persistedReservation.ReservationEmail.Clone();
            reservation.ReservationPhone = persistedReservation.ReservationPhone.Clone();
            reservation.CreditCardsAddress = persistedReservation.CreditCardsAddress.Clone();
            reservation.PointOfSale = persistedReservation.PointOfSale;
            reservation.ClubPassPurchaseRequest = persistedReservation.ClubPassPurchaseRequest;
            reservation.FareLock = persistedReservation.FareLock;
            reservation.FareRules = persistedReservation.FareRules;
            reservation.SeatPrices = persistedReservation.SeatPrices;
            reservation.Taxes = persistedReservation.Taxes;
            reservation.UnregisterFareLock = persistedReservation.UnregisterFareLock;
            reservation.AwardTravel = persistedReservation.AwardTravel;
            reservation.FlightShareMessage = persistedReservation.FlightShareMessage;
            reservation.PKDispenserPublicKey = persistedReservation.PKDispenserPublicKey;
            reservation.TCDAdvisoryMessages = persistedReservation.TCDAdvisoryMessages;
            reservation.IsRefundable = persistedReservation.IsRefundable;
            reservation.ISInternational = persistedReservation.ISInternational;
            reservation.ISFlexibleSegmentExist = persistedReservation.ISFlexibleSegmentExist;
            reservation.SeatAssignmentMessage = persistedReservation.SeatAssignmentMessage;
            reservation.IsELF = persistedReservation.IsELF;
            reservation.IsSSA = persistedReservation.IsSSA;
            if (persistedReservation.TravelOptions != null && persistedReservation.TravelOptions.Count > 0)
            {
                reservation.TravelOptions = persistedReservation.TravelOptions;
            }
            //FOP Options Fix Venkat 12/08
            reservation.FormOfPaymentType = persistedReservation.FormOfPaymentType;
            if (persistedReservation.FormOfPaymentType == MOBFormofPayment.PayPal || persistedReservation.FormOfPaymentType == MOBFormofPayment.PayPalCredit)
            {
                if (persistedReservation.PayPal != null)
                {
                    reservation.PayPal = persistedReservation.PayPal;
                }
                if (persistedReservation.PayPalPayor != null)
                {
                    reservation.PayPalPayor = persistedReservation.PayPalPayor;
                }
            }
            if (persistedReservation.FormOfPaymentType == MOBFormofPayment.Masterpass)
            {
                if (persistedReservation.MasterpassSessionDetails != null)
                    reservation.MasterpassSessionDetails = persistedReservation.MasterpassSessionDetails;
                if (persistedReservation.Masterpass != null)
                    reservation.Masterpass = persistedReservation.Masterpass;
            }
            //FOP Options Fix Venkat 12/08
            if (persistedReservation.FOPOptions != null && persistedReservation.FOPOptions.Count > 0)
            {
                reservation.FOPOptions = persistedReservation.FOPOptions;
            }

            if (persistedReservation.IsReshopChange)
            {
                reservation.ReshopTrips = persistedReservation.ReshopTrips;
                reservation.IsReshopChange = true;
                reservation.Reshop = persistedReservation.Reshop;
            }
            if (persistedReservation.ShopReservationInfo != null)
            {
                reservation.ShopReservationInfo = persistedReservation.ShopReservationInfo;
            }
            if (persistedReservation.ShopReservationInfo2 != null)
            {
                reservation.ShopReservationInfo2 = persistedReservation.ShopReservationInfo2;
            }
            if (persistedReservation.TripInsuranceFile != null && persistedReservation.TripInsuranceFile.TripInsuranceBookingInfo != null)
            {
                reservation.TripInsuranceInfoBookingPath = persistedReservation.TripInsuranceFile.TripInsuranceBookingInfo;
            }
            else
            {
                reservation.TripInsuranceInfoBookingPath = null;
            }

            reservation.AlertMessages = persistedReservation.AlertMessages;
            reservation.IsRedirectToSecondaryPayment = persistedReservation.IsRedirectToSecondaryPayment;
            reservation.RecordLocator = persistedReservation.RecordLocator;
            reservation.Messages = persistedReservation.Messages;
            reservation.CheckedbagChargebutton = persistedReservation.CheckedbagChargebutton;
            reservation.IsBookingCommonFOPEnabled = persistedReservation.IsBookingCommonFOPEnabled;
            reservation.IsReshopCommonFOPEnabled = persistedReservation.IsReshopCommonFOPEnabled;
            reservation.IsPostBookingCommonFOPEnabled = persistedReservation.IsPostBookingCommonFOPEnabled;
        }

        private void AddConfirmationScreenAlertMessage(MOBSHOPReservationInfo2 shopreservartionInfo2, string headerMessage, string body, string text3, string order, bool isWarning = false, string recordLocator = "", string lastName = "")
        {
            if (shopreservartionInfo2 == null)
            {
                shopreservartionInfo2 = new MOBSHOPReservationInfo2();
            }
            if (shopreservartionInfo2.ConfirmationScreenAlertMessages == null)
            {
                shopreservartionInfo2.ConfirmationScreenAlertMessages = new MOBAlertMessages();
                if (isWarning && _configuration.GetValue<bool>("EnableServiceAnimalEnhancements"))
                {
                    body = SetUpDotComDeeplinkMessageForTaskTrainedServiceAnimal(shopreservartionInfo2, body, recordLocator, lastName);
                }
                else
                {
                    shopreservartionInfo2.ConfirmationScreenAlertMessages.HeaderMessage = _configuration.GetValue<string>("ConfirmationAlertMessageHeaderText");
                    shopreservartionInfo2.ConfirmationScreenAlertMessages.MessageType = MOBMESSAGETYPES.INFORMATION.ToString();
                    // By Default IsDefaultOption value false that's why adding the DisabledSpecialRequestCheckThroughFlight toggle
                    shopreservartionInfo2.ConfirmationScreenAlertMessages.IsDefaultOption = !_configuration.GetValue<bool>("DisabledSpecialRequestCheckThroughFlight");

                }
            }

            if (shopreservartionInfo2.ConfirmationScreenAlertMessages.AlertMessages == null)
            {
                shopreservartionInfo2.ConfirmationScreenAlertMessages.AlertMessages = new List<MOBSection>();
            }
            if (!IsAlertMessageExists(body, order, shopreservartionInfo2.ConfirmationScreenAlertMessages.AlertMessages))
            {
                shopreservartionInfo2.ConfirmationScreenAlertMessages.AlertMessages.Add(new MOBSection
                {
                    Text1 = headerMessage,
                    Text2 = body,
                    Text3 = text3,
                    Order = order
                });
            }
            shopreservartionInfo2.ConfirmationScreenAlertMessages.AlertMessages = SortAlertmessage(shopreservartionInfo2.ConfirmationScreenAlertMessages.AlertMessages);
        }
        private string SetUpDotComDeeplinkMessageForTaskTrainedServiceAnimal(MOBSHOPReservationInfo2 shopreservartionInfo2, string body, string recordLocator, string lastName)
        {
            shopreservartionInfo2.ConfirmationScreenAlertMessages.MessageType = MOBMESSAGETYPES.CAUTION.ToString();
            shopreservartionInfo2.ConfirmationScreenAlertMessages.HeaderMessage = _configuration.GetValue<string>("ConfirmationAlertMessageHeaderText");
            shopreservartionInfo2.ConfirmationScreenAlertMessages.IsDefaultOption = true;
            string encryptedString = GetTripDetailRedirect3dot0Url(recordLocator, lastName) + "?SVAN=true";
            body = string.Format(body, encryptedString);
            return body;
        }

        private string GetTripDetailRedirect3dot0Url(string cn, string ln, string ac = "",
           string languagecode = "en/US", string channel = "mobile", int timestampvalidity = 0)
        {
            //REF:{0}/{1}/manageres/tripdetails/{2}/{3}?{4}
            //{env}/{en/US}/manageres/tripdetails/{encryptedStuff}/mobile?changepath=true
            var baseUrl = _configuration.GetValue<string>("TripDetailRedirect3dot0BaseUrl");
            var urlPattern = _configuration.GetValue<string>("TripDetailRedirect3dot0UrlPattern");
            DateTime timestamp
                = (timestampvalidity > 0) ? DateTime.Now.ToUniversalTime().AddMinutes(timestampvalidity) : DateTime.Now.ToUniversalTime();

            string encryptedstring = EncryptString
                (string.Format("RecordLocator={0};LastName={1};TimeStamp={2};", cn, ln, timestamp)).Replace("/", "~~");

            var encodedstring = System.Web.HttpUtility.UrlEncode(encryptedstring);
            string encodedpnr = System.Web.HttpUtility.UrlEncode(EncryptString(cn));

            if (string.Equals(ac, "EX", StringComparison.OrdinalIgnoreCase))
            {
                return string.Format
                    (urlPattern, baseUrl, languagecode, encodedstring, channel, "changepath=true");
            }
            else if (string.Equals(ac, "CA", StringComparison.OrdinalIgnoreCase))
            {
                return string.Format
                    (urlPattern, baseUrl, languagecode, encodedstring, channel, "cancelpath=true");
            }
            else
            {
                return string.Format
                    (urlPattern, baseUrl, languagecode, encodedstring, channel, string.Empty).TrimEnd('?');
            }
        }
        public string EncryptString(string data)
        {
            return United.ECommerce.Framework.Utilities.SecureData.EncryptString(data);
        }
        private List<MOBMobileCMSContentMessages> getEtcBalanceAttentionConfirmationMessages(double? balanceAmount)
        {
            MOBMobileCMSContentMessages message = new MOBMobileCMSContentMessages();
            message.ContentFull = String.Format(_configuration.GetValue<string>("ETCBalanceConfirmationMessage"), String.Format("{0:0.00}", balanceAmount));
            message.HeadLine = "Attention";

            return new List<MOBMobileCMSContentMessages> { message };
        }

        private List<MOBSHOPPrice> UpdatePricesForTPI(List<MOBSHOPPrice> prices, MOBTPIInfoInBookingPath tPIInfo)
        {
            if (tPIInfo != null)
            {
                List<MOBSHOPPrice> totalPrices = new List<MOBSHOPPrice>();
                bool totalExist = false;
                double flightTotal = 0;

                CultureInfo ci = null;

                for (int i = 0; i < prices.Count; ++i)
                {
                    if (ci == null)
                        ci = TopHelper.GetCultureInfo(prices[i].CurrencyCode);

                    if (prices[i].DisplayType.ToUpper() == "GRAND TOTAL")
                    {
                        totalExist = true;
                        prices[i].DisplayValue = string.Format("{0:#,0.00}", (Convert.ToDouble(prices[i].DisplayValue) + tPIInfo.Amount));
                        prices[i].FormattedDisplayValue = TopHelper.FormatAmountForDisplay(prices[i].DisplayValue, ci, false); // string.Format("{0:c}", prices[i].DisplayValue);
                        double tempDouble1 = 0;
                        double.TryParse(prices[i].DisplayValue.ToString(), out tempDouble1);
                        prices[i].Value = Math.Round(tempDouble1, 2, MidpointRounding.AwayFromZero);
                    }
                    if (prices[i].DisplayType.ToUpper() == "TOTAL")
                    {
                        flightTotal = Convert.ToDouble(prices[i].DisplayValue);
                    }
                }
                MOBSHOPPrice travelInsurancePrice = new MOBSHOPPrice();
                travelInsurancePrice.CurrencyCode = "USD";
                travelInsurancePrice.DisplayType = "Travel Insurance";
                travelInsurancePrice.DisplayValue = string.Format("{0:#,0.00}", tPIInfo.Amount.ToString());
                travelInsurancePrice.FormattedDisplayValue = TopHelper.FormatAmountForDisplay(travelInsurancePrice.DisplayValue, ci, false);
                travelInsurancePrice.PriceType = "Travel Insurance";
                travelInsurancePrice.PriceTypeDescription = "Travel Insurance";

                double tmpDouble1 = 0;
                double.TryParse(travelInsurancePrice.DisplayValue.ToString(), out tmpDouble1);
                travelInsurancePrice.Value = Math.Round(tmpDouble1, 2, MidpointRounding.AwayFromZero);

                if (totalExist)
                {
                    prices.Insert(prices.Count - 1, travelInsurancePrice);
                }
                else
                {
                    prices.Add(travelInsurancePrice);
                    MOBSHOPPrice totalPrice = new MOBSHOPPrice();
                    totalPrice.CurrencyCode = "USD";
                    totalPrice.DisplayType = "Grand Total";
                    totalPrice.DisplayValue = (flightTotal + tPIInfo.Amount).ToString("N2", CultureInfo.InvariantCulture);
                    totalPrice.FormattedDisplayValue = TopHelper.FormatAmountForDisplay(totalPrice.DisplayValue, ci, false);
                    double tempDouble1 = 0;
                    double.TryParse(totalPrice.DisplayValue.ToString(), out tempDouble1);
                    totalPrice.Value = Math.Round(tempDouble1, 2, MidpointRounding.AwayFromZero);
                    totalPrice.PriceType = "Grand Total";
                    prices.Add(totalPrice);
                }
            }

            return prices;
        }

        private bool IsUSorPRPointOfSale(string PointOfSale)
        {
            return (!string.IsNullOrEmpty(PointOfSale) && PointOfSale.Trim().ToUpper() == "US" || PointOfSale.Trim().ToUpper() == "PR");
        }

        private async Task<MOBSection> GetSpecialNeedsMessageOnConfirmPurchaseForBringWheelchair(MOBCheckOutRequest request, Session session, bool departWithinSevenDays, List<MOBSHOPTrip> trips, List<MOBTravelSpecialNeed> selectedSSRCodes, string PointOfSale)
        {
            MOBSection section = null;

            bool isUSorPRPointOfSale = IsUSorPRPointOfSale(PointOfSale);
            string bringWheelchairCode = _configuration.GetValue<string>("SSRWheelChairDescription");
            bool isForBringWheelchair = !string.IsNullOrEmpty(bringWheelchairCode) ? selectedSSRCodes?.Any(ssrCode => ssrCode.Code == bringWheelchairCode) ?? false : false;

            if (trips != null && trips.Any() && isUSorPRPointOfSale && isForBringWheelchair)
            {
                List<CMSContentMessage> lstMessages = await _shoppingCartUtility.GetSDLContentByGroupName(request, session.SessionId, session.Token, _configuration.GetValue<string>("CMSContentMessages_GroupName_BookingRTI_Messages"), "BookingPathRTI_CMSContentMessagesCached_StaticGUID");
                var sdlMessage = GetSDLMessageFromList(lstMessages, _configuration.GetValue<string>("BookingConfirmationSpecialTravelNeedsWheelchairMsgKey"))?.FirstOrDefault()?.ContentFull;
                if (!string.IsNullOrEmpty(sdlMessage))
                {
                    section = new MOBSection
                    {
                        IsDefaultOpen = false,
                        MessageType = "INFORMATION",
                        Text1 = "",
                        Text2 = sdlMessage
                    };
                }
            }

            return section;
        }


        private async Task<(string SpecialNeedMessage, MOBSection MobSection)> GetSpecialNeedsMessageOnConfirmPurchase(MOBCheckOutRequest request, Session session, bool departWithinSevenDays, List<MOBSHOPTrip> trips, List<MOBTravelSpecialNeed> selectedSSRCodes, string PointOfSale, int Application_Id, string Application_Version_Major, MOBSection section)
        {
            // Reservation has special need requests 
            if (trips != null && trips.Any() && selectedSSRCodes != null && selectedSSRCodes.Any())
            {
                var oaCarriers = trips.SelectMany(t => t.FlattenedFlights.SelectMany(ff => ff.Flights.Where(f => !string.IsNullOrEmpty(f.OperatingCarrier) && !f.OperatingCarrier.ToUpper().Equals("UA")).Select(f => f.OperatingCarrierDescription)));

                if (_configuration.GetValue<bool>("DisabledSpecialRequestCheckThroughFlight") && oaCarriers != null && oaCarriers.Any())
                {
                    bool moreThanOneOA = oaCarriers.Count() > 1;

                    //You must contact {0} to complete your request for assistance with your special travel needs.
                    return (string.Format(_configuration.GetValue<string>("SSR_OA_MessageNew"), ConvertListToString(oaCarriers.Distinct().ToList())), section);
                }

                // If not showing the message for OA's, then showing the 168 hour message
                var listOfHighTouchRequestCodes = _configuration.GetValue<string>("SSR_HighTouchCodes").Split('|');

                if (listOfHighTouchRequestCodes != null && selectedSSRCodes.Select(_ => _.Code).Intersect(listOfHighTouchRequestCodes).Any())
                {
                    string correctEntry = string.Empty;
                    bool isUSorPRPointOfSale = IsUSorPRPointOfSale(PointOfSale);
                    bool isEmotinalServiceAnimalRequest = selectedSSRCodes.Where(_ => _.Code == "ESAN" && _.Value == "6").Any();
                    bool isCongnitiveAndOtherDisabilityRequest = selectedSSRCodes.Where(_ => _.Code == "DPNA_1" || _.Code == "DPNA_2" || _.Code == "WCHC").Any();
                    bool isServiceAnimal = selectedSSRCodes.Where(_ => _.Code == "SVAN" || (_.Code == "ESAN" && _.Value == "5")).Any();
                    string strPlaceHolder = string.Empty;
                    if (isUSorPRPointOfSale)
                    {
                        if (departWithinSevenDays)
                        {
                            strPlaceHolder = GetHighTouchSpecialNeedItems(selectedSSRCodes, true);
                            if (EnableEmotionalSupportAnimal(Application_Id, Application_Version_Major))
                            {
                                correctEntry = string.Format(_configuration.GetValue<string>("SSR_DepartInWithInSevenDaysCongnitiveAndOtherDisabilityMessageServiceAnimalRequestMessage"), strPlaceHolder, strPlaceHolder.ToLower());
                            }
                            else
                            {

                                correctEntry = _configuration.GetValue<string>("SSR_DepartInWithInSevenDaysCongnitiveAndOtherDisabilityMessageServiceAnimalRequestMessageOldVersion");
                            }

                        }
                        else
                        {
                            strPlaceHolder = GetHighTouchSpecialNeedItems(selectedSSRCodes, !isEmotinalServiceAnimalRequest);
                            if (EnableEmotionalSupportAnimal(Application_Id, Application_Version_Major) && isEmotinalServiceAnimalRequest)
                            {
                                if (isCongnitiveAndOtherDisabilityRequest)
                                {
                                    correctEntry = string.Concat(_configuration.GetValue<string>("SSR_EmotionalSupportAnimalRequestMessage"), "<br/>", string.Format(_configuration.GetValue<string>("SSR_DepartInMoreThanSevenDaysCongnitiveAndOtherDisabilityMessageServiceAnimalRequestMessage"), strPlaceHolder, strPlaceHolder.ToLower()));
                                }
                                else
                                {

                                    correctEntry = _configuration.GetValue<string>("SSR_EmotionalSupportAnimalRequestMessage");
                                }
                            }
                            else
                            {
                                if (EnableEmotionalSupportAnimal(Application_Id, Application_Version_Major) && isServiceAnimal)
                                {
                                    if (IsTaskTrainedServiceDogSupportedAppVersion(Application_Id, Application_Version_Major))
                                    {
                                        correctEntry = "";
                                        List<CMSContentMessage> lstMessages = await _shoppingCartUtility.GetSDLContentByGroupName(request, session.SessionId, session.Token, _configuration.GetValue<string>("CMSContentMessages_GroupName_BookingRTI_Messages"), "BookingPathRTI_CMSContentMessagesCached_StaticGUID");
                                        if (lstMessages != null && lstMessages.Any())
                                        {
                                            string serviceAnimalContent = (_configuration.GetValue<bool>("EnableServiceAnimalEnhancements") && isServiceAnimal
                                               ? ((GetSDLMessageFromList(lstMessages, "TravelNeeds_TaskTrainedDog_Confirmation_Screen_Content_MOB")?.FirstOrDefault()?.ContentFull) ?? "")
                                              : ((GetSDLMessageFromList(lstMessages, "Booking_Confirmation_Page_Task_Trained_Dog_Alert_Content_MOB")?.FirstOrDefault()?.ContentFull) ?? ""));
                                            ;
                                            string serviceAnimalContentHeadline = (GetSDLMessageFromList(lstMessages, "Booking_Confirmation_Page_Task_Trained_Dog_Alert_Content_MOB")?.FirstOrDefault()?.HeadLine) ?? "";
                                            if (_configuration.GetValue<bool>("EnableServiceAnimalEnhancements") && isServiceAnimal)
                                            {
                                                section = new MOBSection
                                                {
                                                    IsDefaultOpen = false,
                                                    MessageType = "WARNING",
                                                    Text2 = serviceAnimalContent,
                                                    Text1 = "Attention"
                                                };
                                            }
                                            else
                                            {
                                                section = new MOBSection
                                                {
                                                    IsDefaultOpen = false,
                                                    MessageType = "INFORMATION",
                                                    Text1 = serviceAnimalContentHeadline,
                                                    Text2 = serviceAnimalContent
                                                };
                                            }
                                        }
                                    }
                                    else
                                    {
                                        correctEntry = string.Format(_configuration.GetValue<string>("SSR_DepartInMoreThanSevenDaysCongnitiveAndOtherDisabilityMessageServiceAnimalRequestMessage"), strPlaceHolder, strPlaceHolder.ToLower());
                                    }
                                }
                                else
                                {
                                    correctEntry = _configuration.GetValue<string>("SSR_DepartInWithInSevenDaysCongnitiveAndOtherDisabilityMessageServiceAnimalRequestMessageOldVersion");
                                }
                            }

                        }
                    }
                    else
                    {
                        correctEntry = departWithinSevenDays ? _configuration.GetValue<string>("SSR_DepartWithinSevenDaysMessage") : _configuration.GetValue<string>("SSR_DepartInMoreThanSevenDaysMessage");
                    }
                    return (correctEntry, section);
                }
            }

            return (null, section);
        }

        private bool EnableEmotionalSupportAnimal(int appId, string appVersion)
        {
            return GeneralHelper.IsApplicationVersionGreater2(appId, appVersion, "AndroidEnableAnimalInCabinPortalMessage", "iPhoneEnableAnimalInCabinPortalMessage", "", "", _configuration);
        }

        private string ConvertListToString(List<string> inputList)
        {
            try
            {
                if (inputList != null && inputList.Any())
                    inputList.RemoveAll(x => (string.IsNullOrWhiteSpace(x) || string.IsNullOrEmpty(x)));

                if (inputList != null && inputList.Any())
                {
                    if (inputList.Count == 1) return inputList[0];
                    else if (inputList.Count == 2)
                        return string.Join(", ", inputList.Take(inputList.Count() - 1)) + " and " + inputList.Last();
                    else
                        return string.Join(", ", inputList.Take(inputList.Count() - 1)) + ", and " + inputList.Last();
                }
                else
                    return string.Empty;
            }
            catch (Exception ex) { return string.Empty; }
        }

        private string GetHighTouchSpecialNeedItems(List<MOBTravelSpecialNeed> selectedSSRCodes, bool includEmotinalServiceAnimal)
        {
            if (selectedSSRCodes.IsNullOrEmpty())
                return string.Empty;
            string HighTouchSpecialNeedItem = string.Empty;
            var listOfHighTouchRequestCodes = _configuration.GetValue<string>("SSR_HighTouchCodes").Split('|');
            foreach (var selectedSSRCode in selectedSSRCodes)
            {
                if (listOfHighTouchRequestCodes.Contains(selectedSSRCode.Code))
                {
                    if (!includEmotinalServiceAnimal && selectedSSRCode.Code == "ESAN" && selectedSSRCode.Value == "6")
                    {
                        //skip 
                    }
                    else
                    {
                        HighTouchSpecialNeedItem += ", " + selectedSSRCode.DisplayDescription.ToLower();
                    }
                }
            }
            return !string.IsNullOrEmpty(HighTouchSpecialNeedItem) ? char.ToUpper(HighTouchSpecialNeedItem[2]) + HighTouchSpecialNeedItem.Remove(0, 3) : string.Empty;
        }

        private bool IsTaskTrainedServiceDogSupportedAppVersion(int appId, string appVersion)
        {
            return GeneralHelper.IsApplicationVersionGreaterorEqual(appId, appVersion, _configuration.GetValue<string>("TravelSpecialNeedInfo_TaskTrainedServiceDog_Supported_AppVestion_Android"), _configuration.GetValue<string>("TravelSpecialNeedInfo_TaskTrainedServiceDog_Supported_AppVestion_iOS"));
        }

        private async Task GenerateTPISecondaryPaymentInfo(MOBCheckOutRequest request, MOBCheckOutResponse response, string token, Reservation persistedReservation, Session session = null)
        {
            if (EnableTPI(request.Application.Id, request.Application.Version.Major, 3) && !response.Reservation.IsReshopChange &&
                persistedReservation != null && persistedReservation.TripInsuranceFile != null && persistedReservation.TripInsuranceFile.TripInsuranceBookingInfo != null && persistedReservation.TripInsuranceFile.TripInsuranceBookingInfo.IsRegistered)
            {
                // TPI payment succeed 
                if (response.Reservation.TripInsuranceInfoBookingPath != null && !string.IsNullOrEmpty(response.Reservation.TripInsuranceInfoBookingPath.Img) && response.Reservation.TripInsuranceInfoBookingPath.IsTPIIncludedInCart)
                {
                    response.Reservation.IsRedirectToSecondaryPayment = false;
                    response.Reservation.Prices.FirstOrDefault(a => a.PriceType.ToUpper().Trim() == "TRAVEL INSURANCE").BilledSeperateText = _configuration.GetValue<string>("TPIinfo_ConfirmationPage_BilledSeperateText");
                }
                // TPI payment fail with a valid card, confirmation page with Alert msg
                else if (ValidateFormOfPaymentType(response.ShoppingCart.FormofPaymentDetails) || (response.Reservation.TripInsuranceInfoBookingPath != null && !response.Reservation.TripInsuranceInfoBookingPath.IsTPIIncludedInCart) || (!string.IsNullOrEmpty(response.Reservation.PointOfSale) && response.Reservation.PointOfSale != "US"))
                {
                    response.Reservation.IsRedirectToSecondaryPayment = false;
                    MOBSection alertMsg = await GetTPIAlertMessage(true, session, request.Flow);
                    // build Alert msg
                    if (response.Reservation.AlertMessages == null || (response.Reservation.AlertMessages != null && response.Reservation.AlertMessages.Count == 0))
                    {
                        response.Reservation.AlertMessages = new List<MOBSection>() { alertMsg };
                    }
                    else
                    {
                        response.Reservation.AlertMessages[0].Text3 = string.Empty;
                        response.Reservation.AlertMessages.Add(alertMsg);
                    }

                    if (response.Reservation.TripInsuranceInfoBookingPath != null && !response.Reservation.TripInsuranceInfoBookingPath.IsTPIIncludedInCart)
                    {
                        response.Reservation.Prices = UpdatePriceForUnRegisterTPI(response.Reservation.Prices);
                        MOBUnitedException uaex = new MOBUnitedException("Trip insurance is registered, but not added to cart");
                        MOBExceptionWrapper uaexWrapper = new MOBExceptionWrapper(uaex);
                        _logger.LogInformation("GenerateTPISecondaryPaymentInfo - UnitedException {uaexWrapper} with {sessionId}", JsonConvert.SerializeObject(uaexWrapper), request.SessionId);
                    }
                }
                // secondary payment  
                else
                {
                    response.Reservation.TripInsuranceInfoBookingPath = persistedReservation.TripInsuranceFile.TripInsuranceBookingInfo;
                    response.Reservation.IsRedirectToSecondaryPayment = true;
                    MOBSection alertMsg = await GetTPIAlertMessage(false);
                    response.Reservation.TripInsuranceInfoBookingPath.PaymentContentBody = alertMsg.Text2;
                    response.Reservation.TripInsuranceInfoBookingPath.PaymentContentHeader = alertMsg.Text1;
                    response.Reservation.Prices = UpdatePriceForUnRegisterTPI(response.Reservation.Prices);
                    response.Reservation.TripInsuranceInfoBookingPath.PaymentContent = _configuration.GetValue<string>("TPIinfo_ConfirmationPage_ProductName");

                    if (!_configuration.GetValue<bool>("DisableShowingPolicyOfInsuranceAndTNCChanges"))
                    {
                        response.Reservation.TripInsuranceInfoBookingPath.PolicyOfInsuranceTextAndUrl = await ShowPolicyOfInsurance(session).ConfigureAwait(false);
                        response.Reservation.TripInsuranceInfoBookingPath.TermsAndConditionsTextAndUrl = ShowTermsAndConditions();
                    }
                }
            }
            // TPI not registered
            else
            {
                response.Reservation.IsRedirectToSecondaryPayment = false;
                #region Trip Insurance confirmation flow
                if (request != null && request.Application != null && request.Application.Version != null)
                {
                    if (EnableTPI(request.Application.Id, request.Application.Version.Major, 1) && !response.Reservation.IsReshopChange)
                    {
                        if (response.Reservation != null && !string.IsNullOrEmpty(response.Reservation.RecordLocator))
                        {
                            response.Reservation.TripInsuranceInfo = new MOBTPIInfo() { };
                            try
                            {
                                // we don't show TPI for farelock path 
                                // we don't show TPI if already choose TPI in booking path.
                                bool isShowTPI = false;
                                isShowTPI = ShowTPIQuoteInfo(response.Reservation.TravelOptions);
                                if (isShowTPI)
                                {
                                    MOBSHOPProductSearchRequest getTripInsuranceRequest = new MOBSHOPProductSearchRequest();
                                    getTripInsuranceRequest.SessionId = request.SessionId;
                                    getTripInsuranceRequest.LanguageCode = request.LanguageCode;
                                    getTripInsuranceRequest.Application = request.Application;
                                    getTripInsuranceRequest.DeviceId = request.DeviceId;
                                    getTripInsuranceRequest.CartId = response.Reservation.CartId;
                                    getTripInsuranceRequest.PointOfSale = "US";
                                    if (_configuration.GetValue<bool>("TPIPostBookingCountryCodeCorrection"))
                                    {
                                        getTripInsuranceRequest.PointOfSale = response.Reservation.PointOfSale;
                                    }
                                    getTripInsuranceRequest.ProductCodes = new List<string>() { "TPI" };
                                    response.Reservation.TripInsuranceInfo = await GetTripInsuranceInfo(getTripInsuranceRequest, token);
                                }
                            }
                            catch
                            {
                                response.Reservation.TripInsuranceInfo = null;
                            }
                        }
                    }
                }
                #endregion
            }
        }

        private async Task<MOBItem> ShowPolicyOfInsurance(Session session)
        {
            if (session != null &&
                await _featureToggles.IsEnabledTripInsuranceV2(session.AppID, session.VersionID, session.CatalogItems).ConfigureAwait(false))
            {
                return new MOBItem
                {
                    Id = _configuration.GetValue<string>("TPI_SecondaryPayment_PolicyOfInsurance_Lable"),
                    CurrentValue = _configuration.GetValue<string>("TPIV2_SecondaryPayment_PolicyOfInsurance_URL"),
                    SaveToPersist = false
                };
            }
            else
            {
                return new MOBItem
                {
                    Id = _configuration.GetValue<string>("TPI_SecondaryPayment_PolicyOfInsurance_Lable"),
                    CurrentValue = _configuration.GetValue<string>("TPI_SecondaryPayment_PolicyOfInsurance_URL"),
                    SaveToPersist = false
                };
            }
        }

        private MOBItem ShowTermsAndConditions()
        {
            return new MOBItem
            {
                Id = _configuration.GetValue<string>("TPI_SecondaryPayment_TermsAndConditions_Lable"),
                CurrentValue = _configuration.GetValue<string>("TPI_SecondaryPayment_TermsAndConditions_URL"),
                SaveToPersist = false
            };
        }

        private async Task<MOBTPIInfo> GetTripInsuranceInfo(MOBSHOPProductSearchRequest request, string token, bool isBookingPath = false)
        {
            MOBTPIInfo tripInsuranceInfo = new MOBTPIInfo();
            bool isPostBookingCall = Convert.ToBoolean(_configuration.GetValue<string>("ShowTripInsuranceSwitch") ?? "false");
            // ==>> Venkat and Elise change only one below line of code to offer TPI postbooking when inline TPI is off for new clients 2.1.36 and above
            isPostBookingCall = EnableTPI(request.Application.Id, request.Application.Version.Major, 1);
            if (isPostBookingCall ||
                (Convert.ToBoolean(_configuration.GetValue<string>("ShowTripInsuranceBookingSwitch") ?? "false")
                && isBookingPath)
                )
            {
                string logAction = isBookingPath ? "GetTripInsuranceInfoInBookingPath" : "GetTripInsuranceInfo";
                try
                {
                    DisplayCartRequest displayCartRequest = GetDisplayCartRequest(request);
                    string jsonRequest = JsonConvert.SerializeObject(displayCartRequest);
                    string jsonResponse = await _flightShoppingService.GetProducts(token, _headers.ContextValues.SessionId, jsonRequest);

                    if (!string.IsNullOrEmpty(jsonResponse))
                    {
                        DisplayCartResponse response = JsonConvert.DeserializeObject<DisplayCartResponse>(jsonResponse);
                        var productVendorOffer = new GetVendorOffers();
                        if (!_configuration.GetValue<bool>("DisableMerchProductOfferNullCheck"))
                            productVendorOffer = response.MerchProductOffer != null ? ObjectToObjectCasting<GetVendorOffers, United.Service.Presentation.ProductResponseModel.ProductOffer>(response.MerchProductOffer) : productVendorOffer;
                        else
                            productVendorOffer = ObjectToObjectCasting<GetVendorOffers, United.Service.Presentation.ProductResponseModel.ProductOffer>(response.MerchProductOffer);

                        await _sessionHelperService.SaveSession<GetVendorOffers>(productVendorOffer, request.SessionId, new List<string> { request.SessionId, typeof(GetVendorOffers).FullName }, typeof(GetVendorOffers).FullName).ConfigureAwait(false);

                        if (response != null && (response.Errors == null || response.Errors.Count == 0) && response.Status.Equals(United.Services.FlightShopping.Common.StatusType.Success) && response.MerchProductOffer != null)
                        {
                            tripInsuranceInfo = await GetTripInsuranceDetails(response.MerchProductOffer, request.Application.Id, request.Application.Version.Major, request.DeviceId, request.SessionId, isBookingPath);
                        }
                        else
                        {
                            tripInsuranceInfo = null;
                        }
                    }

                    if (!isBookingPath)
                    {
                        _logger.LogInformation("GetTripInsuranceInfo - ClientResponse for GetTripInsuranceInfo {Trace} and {sessionId}", tripInsuranceInfo, request.SessionId);

                        Reservation bookingPathReservation = new Reservation();
                        bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(request.SessionId, bookingPathReservation.ObjectName, new List<string> { request.SessionId, bookingPathReservation.ObjectName }).ConfigureAwait(false);
                        if (bookingPathReservation.TripInsuranceFile == null)
                        {
                            bookingPathReservation.TripInsuranceFile = new MOBTripInsuranceFile() { };
                        }
                        bookingPathReservation.TripInsuranceFile.TripInsuranceInfo = tripInsuranceInfo;

                        await _sessionHelperService.SaveSession<Reservation>(bookingPathReservation, request.SessionId, new List<string> { request.SessionId, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName).ConfigureAwait(false);
                    }
                }
                catch (System.Net.WebException wex)
                {
                    var errorResponse = new StreamReader(wex.Response.GetResponseStream()).ReadToEnd();
                    //_logger.LogError("GetTripInsuranceInfo - UnitedException {error} and SessionId {sessionId}", wex.Message.ToString(), request.SessionId);
                }
                catch (System.Exception ex)
                {
                    tripInsuranceInfo = null;
                    MOBExceptionWrapper exceptionWrapper = new MOBExceptionWrapper(ex);
                }
            }
            return tripInsuranceInfo;
        }

        private DisplayCartRequest GetDisplayCartRequest(MOBSHOPProductSearchRequest request)
        {
            DisplayCartRequest displayCartRequest = null;
            if (request != null && !string.IsNullOrEmpty(request.CartId) && request.ProductCodes != null && request.ProductCodes.Count > 0)
            {
                displayCartRequest = new DisplayCartRequest();
                displayCartRequest.CartId = request.CartId;
                displayCartRequest.CountryCode = request.PointOfSale;
                displayCartRequest.LangCode = request.LanguageCode;
                displayCartRequest.ProductCodes = new List<string>();
                displayCartRequest.CartKey = request.CartKey;
                foreach (var productCode in request.ProductCodes)
                {
                    displayCartRequest.ProductCodes.Add(productCode);
                }
            }

            return displayCartRequest;
        }

        private T ObjectToObjectCasting<T, R>(R request)
        {
            var typeInstance = Activator.CreateInstance(typeof(T));

            foreach (var propReq in request.GetType().GetProperties())
            {
                var propRes = typeInstance.GetType().GetProperty(propReq.Name);
                if (propRes != null)
                {
                    propRes.SetValue(typeInstance, propReq.GetValue(request));
                }
            }

            return (T)typeInstance;
        }

        private async Task<MOBTPIInfo> GetTripInsuranceDetails(Service.Presentation.ProductResponseModel.ProductOffer offer, int applicationId, string applicationVersion, string deviceID, string sessionId, bool isBookingPath = false)
        {
            MOBTPIInfo tripInsuranceInfo = new MOBTPIInfo();
            try
            {
                tripInsuranceInfo = await GetTPIDetails(offer, sessionId, true, isBookingPath, applicationId);
            }
            catch (System.Exception ex)
            {
                tripInsuranceInfo = null;
                MOBExceptionWrapper exceptionWrapper = new MOBExceptionWrapper(ex);
                _logger.LogInformation("GetTripInsuranceDetails - Exception {exceptionWrapper} with {sessionId}", exceptionWrapper, sessionId);
            }
            if (tripInsuranceInfo == null && Convert.ToBoolean(_configuration.GetValue<string>("Log_TI_Offer_If_AIG_NotOffered_At_BookingPath") ?? "false"))
            {
                _logger.LogInformation("GetTripInsuranceDetails - UnitedException {message} with {sessionId}", "AIG Not Offered Trip Insuracne in Booking Path", sessionId);
            }
            return tripInsuranceInfo;
        }

        private async Task<MOBTPIInfo> GetTPIDetails(Service.Presentation.ProductResponseModel.ProductOffer productOffer, string sessionId, bool isShoppingCall, bool isBookingPath = false, int appId = -1)
        {
            MOBTPIInfo tripInsuranceInfo = new MOBTPIInfo();
            if (productOffer.Offers == null)
            {
                tripInsuranceInfo = null;
                return tripInsuranceInfo;
            }
            var product = productOffer.Offers.FirstOrDefault(a => a.ProductInformation.ProductDetails.Where(b => b.Product != null && b.Product.Code.ToUpper().Trim() == "TPI").ToList().Count > 0).
                ProductInformation.ProductDetails.FirstOrDefault(c => c.Product != null && c.Product.Code.ToUpper().Trim() == "TPI").Product;
            #region // sample code If AIG Dont Offer TPI, the contents and Prices should be null. 
            if (product.SubProducts[0].Prices == null || product.Presentation.Contents == null)
            {
                tripInsuranceInfo = null;
                return tripInsuranceInfo;
            }
            #endregion
            #region mapping content
            tripInsuranceInfo = await GetTPIInfoFromContent(product.Presentation.Contents, tripInsuranceInfo, sessionId, isShoppingCall, isBookingPath, appId);
            #endregion

            if (tripInsuranceInfo != null)
            {
                tripInsuranceInfo.ProductId = GetTPIQuoteId(product.Characteristics);
                // if quoteId is null, we should keep reponse null
                if (!string.IsNullOrEmpty(tripInsuranceInfo.ProductId))
                {
                    tripInsuranceInfo = GetTPIAmountAndFormattedAmount(tripInsuranceInfo, product.SubProducts);
                }
                tripInsuranceInfo.ProductCode = product.Code;
                tripInsuranceInfo.ProductName = product.DisplayName;
            }

            return tripInsuranceInfo;
        }

        private async Task<MOBTPIInfo> GetTPIInfoFromContent(System.Collections.ObjectModel.Collection<United.Service.Presentation.ProductModel.PresentationContent> contents, MOBTPIInfo tripInsuranceInfo, string sessionId, bool isShoppingCall, bool isBookingPath = false, int appId = -1)
        {
            string tncPaymentText1 = string.Empty;
            string tncPaymentText2 = string.Empty;
            string tncPaymentText3 = string.Empty;
            string tncPaymentLinkMessage = string.Empty;
            string tncProductPageText1 = string.Empty;
            string tncProductPageText2 = string.Empty;
            string tncProductPageLinkMessage = string.Empty;
            string confirmationResponseDetailMessage1 = string.Empty;
            string confirmationResponseDetailMessage2 = string.Empty;
            string contentInBooking1 = string.Empty;
            string contentInBooking2 = string.Empty;
            string contentInBooking3 = string.Empty;
            string header1 = string.Empty;
            string header2 = string.Empty;
            string legalInfo = string.Empty;
            string legalInfoText = string.Empty;
            string bookingImg = string.Empty;
            string bookingTncContentMsg = string.Empty;
            string bookingTncLinkMsg = string.Empty;
            string bookingLegalInfoContentMsg = string.Empty;
            string mobTgiLimitationMessage = string.Empty;
            string mobTgiReadMessage = string.Empty;
            string mobTgiAndMessage = string.Empty;
            // Covid 19 Emergency WHO TPI content
            string mobTIMBemergencyMessage = string.Empty;
            string mobTIMBemergencyMessageUrltext = string.Empty;
            string mobTIMBemergencyMessagelinkUrl = string.Empty;
            string mobTIMREmergencyMessage = string.Empty;
            string mobTIMREmergencyMessageUrltext = string.Empty;
            string mobTIMREmergencyMessagelinkUrl = string.Empty;
            string mobTIMBWashingtonMessage = string.Empty;


            foreach (var content in contents)
            {
                switch (content.Header.ToUpper().Trim())
                {
                    case "MOBOFFERHEADERMESSAGE":
                        tripInsuranceInfo.Title1 = content.Body.Trim();
                        break;
                    case "MOBOFFERTITLEMESSAGE":
                        tripInsuranceInfo.QuoteTitle = content.Body.Trim();
                        break;
                    case "MOBTRIPCOVEREDPRICEMESSAGE":
                        tripInsuranceInfo.CoverCost = content.Body.Trim();
                        break;
                    case "MOBOFFERFROMTMESSAGE":
                        tripInsuranceInfo.Title2 = content.Body.Trim();
                        break;
                    case "MOBOFFERIMAGE":
                        tripInsuranceInfo.Image = content.Body.Trim();
                        tripInsuranceInfo.TileImage = tripInsuranceInfo.Image;
                        break;
                    case "PREBOOKINGMOBOFFERHEADERMESSAGE":
                        tripInsuranceInfo.TileTitle1 = content.Body.Trim();
                        break;
                    case "PREBOOKINGMOBOFFERTITLEMESSAGE":
                        tripInsuranceInfo.TileQuoteTitle = content.Body.Trim();
                        break;
                    case "PREBOOKINGMOBOFFERFROMTMESSAGE":
                        tripInsuranceInfo.TileTitle2 = content.Body.Trim();
                        break;
                    case "PREBOOKINGMOBOFFERCTAMESSAGE":
                        tripInsuranceInfo.TileLinkText = content.Body.Trim();
                        break;
                    case "MOBTGIVIEWHEADERMESSAGE":
                        tripInsuranceInfo.Headline1 = content.Body.Trim();
                        break;
                    case "MOBTIDETAILSTOTALCOVERCOSTMESSAGE":
                        tripInsuranceInfo.LineItemText = content.Body.Trim();
                        break;
                    case "MOBPAYMENTTANDCHEADER1MESSAGE": //By clicking on purchase I acknowledge that I have read and understand the
                        tncPaymentText1 = content.Body.Trim();
                        break;
                    case "MOBPAYMENTTANDCURLHEADERMESSAGE": //Certificate of Insurance
                        tncPaymentText2 = content.Body.Trim();
                        break;
                    case "MOBPAYMENTTANDCURLHEADER2MESSAGE": //, and agree to the terms and conditions of the insurance coverage provided.
                        tncPaymentText3 = content.Body.Trim();
                        break;
                    case "MOBPAYMENTTANDCBODYURLMESSAGE":
                        tncPaymentLinkMessage = content.Body.Trim();
                        break;
                    case "MOBTIDETAILSTANDCHEADER1MESSAGE": // Coverage is offered by Travel Guard Group, Inc. and limitations will apply;
                        tncProductPageText1 = content.Body.Trim();
                        break;
                    case "MOBTIDETAILSTANDCURLHEADERMESSAGE": // view details.
                        tncProductPageText2 = content.Body.Trim();
                        break;
                    case "MOBTIDETAILSTANDCURLMESSAGE":
                        tncProductPageLinkMessage = content.Body.Trim();
                        break;
                    case "MOBTGIVIEWBODY1MESSAGE": // Are you prepared?
                        tripInsuranceInfo.Body1 = content.Body.Trim();
                        break;
                    case "MOBTGIVIEWBODY2MESSAGE": // For millions of travelers every year...
                        tripInsuranceInfo.Body2 = content.Body.Trim();
                        break;
                    // used in payment confirmation page 
                    case "MOBTICONFIRMATIONBODY1MESSAGE":
                        confirmationResponseDetailMessage1 = content.Body.Trim();
                        break;
                    case "MOBTICONFIRMATIONBODY2MESSAGE":
                        confirmationResponseDetailMessage2 = content.Body.Trim();
                        break;
                    // used in booking path 
                    case "PREBOOKINGMOBOFFERIMAGE":
                        bookingImg = content.Body.Trim();
                        tripInsuranceInfo.TileImage = bookingImg;
                        break;
                    case "PREBOOKINGMOBTIDETAILSTANDCURLHEADERMESSAGE":
                        bookingTncContentMsg = content.Body.Trim();
                        break;
                    case "PREBOOKINGMOBTIDETAILSTANDCURLMESSAGE":
                        bookingTncLinkMsg = content.Body.Trim();
                        break;
                    case "PREBOOKINGMOBPAYMENTTANDCHEADER1MESSAGE":
                        bookingLegalInfoContentMsg = content.Body.Trim();
                        break;
                    case "PREBOOKINGMOBBOOKINGPRODPAGEHEADLINE1":
                        header1 = content.Body.Trim();
                        break;
                    case "PREBOOKINGMOBBOOKINGPRODPAGEHEADLINE2":
                        header2 = content.Body.Trim();
                        break;
                    case "PREBOOKINGMOBBOOKINGPRODPAGECONTENT1":
                        contentInBooking1 = content.Body.Trim();
                        break;
                    case "PREBOOKINGMOBBOOKINGPRODPAGECONTENT2":
                        contentInBooking2 = content.Body.Trim();
                        break;
                    case "PREBOOKINGMOBBOOKINGPRODPAGECONTENT3":
                        contentInBooking3 = content.Body.Trim();
                        break;
                    case "PREBOOKINGMOBBOOKINGPRODPAGEBOTTOMLINE":
                        legalInfo = content.Body.Trim();
                        break;
                    case "MOBTGILIMITATIONMESSAGE":
                        mobTgiLimitationMessage = content.Body.Trim();
                        break;
                    case "MOBTGIREADMESSAGE":
                        mobTgiReadMessage = content.Body.Trim();
                        break;
                    case "MOBTGIANDMESSAGE":
                        mobTgiAndMessage = content.Body.Trim();
                        break;
                    // Covid 19 Emergency WHO TPI content
                    case "MOBTIMBEMERGENCYMESSAGETEXT":
                        mobTIMBemergencyMessage = content != null ? content.Body.Trim() : string.Empty;
                        break;
                    case "MOBTIMBEMERGENCYMESSAGELINKTEXT":
                        mobTIMBemergencyMessageUrltext = content != null ? content.Body.Trim() : string.Empty;
                        break;
                    case "MOBTIMBEMERGENCYMESSAGELINKURL":
                        mobTIMBemergencyMessagelinkUrl = content != null ? content.Body.Trim() : string.Empty;
                        break;
                    case "MOBTIMREMERGENCYMESSAGETEXT":
                        mobTIMREmergencyMessage = content != null ? content.Body.Trim() : string.Empty;
                        break;
                    case "MOBTIMREMERGENCYMESSAGELINKTEXT":
                        mobTIMREmergencyMessageUrltext = content != null ? content.Body.Trim() : string.Empty;
                        break;
                    case "MOBTIMREMERGENCYMESSAGELINKURL":
                        mobTIMREmergencyMessagelinkUrl = content != null ? content.Body.Trim() : string.Empty;
                        break;
                    case "MOBTIMBWASHINGTONMESSAGE":
                        mobTIMBWashingtonMessage = content != null && !string.IsNullOrEmpty(content.Body) ? content.Body.Trim() : string.Empty;
                        break;
                    default:
                        break;
                }
            }
            //Covid 19 Emergency WHO TPI content
            if (_configuration.GetValue<bool>("ToggleCovidEmergencytextTPI") == true)
            {
                if (tripInsuranceInfo != null &&
                    !string.IsNullOrEmpty(mobTIMBemergencyMessage) && !string.IsNullOrEmpty(mobTIMBemergencyMessageUrltext)
                    && !string.IsNullOrEmpty(mobTIMBemergencyMessagelinkUrl))
                {
                    MOBItem tpiContentMessage = new MOBItem();
                    tpiContentMessage.Id = "COVID19EmergencyAlert";
                    tpiContentMessage.CurrentValue = mobTIMBemergencyMessage +
                        " <a href =\"" + mobTIMBemergencyMessagelinkUrl + "\" target=\"_blank\">" + mobTIMBemergencyMessageUrltext + "</a> ";
                    tripInsuranceInfo.TPIAIGReturnedMessageContentList = new List<MOBItem>();
                    tripInsuranceInfo.TPIAIGReturnedMessageContentList.Add(tpiContentMessage);
                }
                if (tripInsuranceInfo != null &&
                    !string.IsNullOrEmpty(mobTIMREmergencyMessage) && !string.IsNullOrEmpty(mobTIMREmergencyMessageUrltext)
                    && !string.IsNullOrEmpty(mobTIMREmergencyMessagelinkUrl))
                {
                    MOBItem tpiContentMessage = new MOBItem();
                    tpiContentMessage.Id = "COVID19EmergencyAlertManageRes";
                    tpiContentMessage.CurrentValue = mobTIMREmergencyMessage +
                        " <a href =\"" + mobTIMREmergencyMessagelinkUrl + "\" target=\"_blank\">" + mobTIMREmergencyMessageUrltext + "</a> ";
                    tripInsuranceInfo.TPIAIGReturnedMessageContentList = new List<MOBItem>();
                    tripInsuranceInfo.TPIAIGReturnedMessageContentList.Add(tpiContentMessage);
                }
            }

            string isNewTPIMessageHTML = appId == 2 ? "<br/><br/>" : "<br/>";
            string specialCharacter = _configuration.GetValue<string>("TPIinfo-SpecialCharacter") ?? "";
            if (tripInsuranceInfo != null && !string.IsNullOrEmpty(tripInsuranceInfo.Image) && !string.IsNullOrEmpty(tncProductPageLinkMessage) &&
                !string.IsNullOrEmpty(tncPaymentLinkMessage) &&
                tripInsuranceInfo.QuoteTitle != null && tripInsuranceInfo.QuoteTitle.Contains("(R)") && !isBookingPath)
            {

                tripInsuranceInfo.Body3 = (mobTgiLimitationMessage.IsNullOrEmpty() && mobTgiReadMessage.IsNullOrEmpty()) ?
                                          (tncProductPageText1 + " <a href =\"" + tncProductPageLinkMessage + "\" target=\"_blank\">" + tncProductPageText2 + "</a>")
                                          : (tncProductPageText1 + " <a href =\"" + tncPaymentLinkMessage + "\" target=\"_blank\">" + mobTgiLimitationMessage + "</a> " + mobTgiReadMessage + " <a href =\"" + tncProductPageLinkMessage + "\" target=\"_blank\">" + tncProductPageText2 + "</a>");
                tripInsuranceInfo.Body3 = !_configuration.GetValue<bool>("IsDisableTPIForWashington") && !string.IsNullOrEmpty(tripInsuranceInfo.Body3) && !string.IsNullOrEmpty(mobTIMBWashingtonMessage) ? tripInsuranceInfo.Body3 + isNewTPIMessageHTML + mobTIMBWashingtonMessage : tripInsuranceInfo.Body3;
                tripInsuranceInfo.TNC = tncPaymentText1 + tncPaymentText3 + " <a href =\"" + tncPaymentLinkMessage + "\" target=\"_blank\">" + tncPaymentText2 + "</a> " + mobTgiAndMessage +
                    " <a href =\"" + tncProductPageLinkMessage + "\" target=\"_blank\">" + tncProductPageText2 + "</a> ";
                tripInsuranceInfo.QuoteTitle = @tripInsuranceInfo.QuoteTitle.Replace("(R)", specialCharacter);
                tripInsuranceInfo.PageTitle = _configuration.GetValue<string>("TPIinfo-PageTitle") ?? "";
                tripInsuranceInfo.Headline2 = _configuration.GetValue<string>("TPIinfo-Headline2") ?? "";
                tripInsuranceInfo.PaymentContent = _configuration.GetValue<string>("TPIinfo-PaymentContent") ?? "";
                // confirmation page use
                if (isShoppingCall)
                {
                    Reservation bookingPathReservation = new Reservation();
                    bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(sessionId, bookingPathReservation.ObjectName, new List<string> { sessionId, bookingPathReservation.ObjectName }).ConfigureAwait(false);
                    if (bookingPathReservation == null)
                    {
                        bookingPathReservation = new Reservation();
                    }
                    if (bookingPathReservation.TripInsuranceFile == null)
                    {
                        bookingPathReservation.TripInsuranceFile = new MOBTripInsuranceFile() { };
                    }
                    bookingPathReservation.TripInsuranceFile.ConfirmationResponseDetailMessage1 = @confirmationResponseDetailMessage1.Replace("(R)", specialCharacter);
                    bookingPathReservation.TripInsuranceFile.ConfirmationResponseDetailMessage2 = confirmationResponseDetailMessage2;

                    await _sessionHelperService.SaveSession<Reservation>(bookingPathReservation, sessionId, new List<string> { sessionId, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName).ConfigureAwait(false);
                }
                else
                {
                    tripInsuranceInfo.Confirmation1 = @confirmationResponseDetailMessage1.Replace("(R)", specialCharacter);
                    tripInsuranceInfo.Confirmation2 = confirmationResponseDetailMessage2;
                }
            }
            else if (isShoppingCall && isBookingPath && !string.IsNullOrEmpty(contentInBooking1) && !string.IsNullOrEmpty(contentInBooking2) && !string.IsNullOrEmpty(contentInBooking3)
                        && !string.IsNullOrEmpty(header1) && !string.IsNullOrEmpty(header2) && !string.IsNullOrEmpty(tncProductPageText1)
                        && !string.IsNullOrEmpty(bookingTncLinkMsg) && !string.IsNullOrEmpty(bookingTncContentMsg) && !string.IsNullOrEmpty(bookingLegalInfoContentMsg)
                        && !string.IsNullOrEmpty(tncPaymentLinkMessage) && !string.IsNullOrEmpty(tncPaymentText2) && !string.IsNullOrEmpty(tncPaymentText3) && !string.IsNullOrEmpty(bookingImg)
                        && !string.IsNullOrEmpty(tripInsuranceInfo.CoverCost) && !string.IsNullOrEmpty(tncPaymentText1)
                        && !string.IsNullOrEmpty(confirmationResponseDetailMessage1)
                        && !string.IsNullOrEmpty(confirmationResponseDetailMessage2))
            {
                tripInsuranceInfo.Body3 = (mobTgiLimitationMessage.IsNullOrEmpty() && mobTgiReadMessage.IsNullOrEmpty()) ?
                                          tncProductPageText1 + " <a href =\"" + bookingTncLinkMsg + "\" target=\"_blank\">" + bookingTncContentMsg + "</a>"
                                        : (tncProductPageText1 + " <a href =\"" + tncPaymentLinkMessage + "\" target=\"_blank\">" + mobTgiLimitationMessage + "</a> " + mobTgiReadMessage + " <a href =\"" + bookingTncLinkMsg + "\" target=\"_blank\">" + bookingTncContentMsg + "</a>");
                tripInsuranceInfo.Body3 = !_configuration.GetValue<bool>("IsDisableTPIForWashington") && !string.IsNullOrEmpty(tripInsuranceInfo.Body3) && !string.IsNullOrEmpty(mobTIMBWashingtonMessage) ? tripInsuranceInfo.Body3 + isNewTPIMessageHTML + mobTIMBWashingtonMessage : tripInsuranceInfo.Body3;
                tripInsuranceInfo.TNC = (mobTgiLimitationMessage.IsNullOrEmpty() && mobTgiReadMessage.IsNullOrEmpty()) ? bookingLegalInfoContentMsg + " <a href =\"" + tncPaymentLinkMessage + "\" target=\"_blank\">" + tncPaymentText2 + "</a> " + tncPaymentText3
                                        : bookingLegalInfoContentMsg + " " + tncPaymentText3 + " <a href =\"" + tncPaymentLinkMessage + "\" target=\"_blank\">" + tncPaymentText2 + "</a> " + mobTgiAndMessage + " <a href =\"" + bookingTncLinkMsg + "\" target=\"_blank\">" + bookingTncContentMsg + "</a>";

                tripInsuranceInfo.PageTitle = _configuration.GetValue<string>("TPIinfo-PageTitle") ?? "";
                tripInsuranceInfo.Image = bookingImg;
                Reservation bookingPathReservation = new Reservation();

                bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(sessionId, bookingPathReservation.ObjectName, new List<string> { sessionId, bookingPathReservation.ObjectName }).ConfigureAwait(false);

                if (bookingPathReservation == null)
                {
                    bookingPathReservation = new Reservation();
                }
                if (bookingPathReservation.TripInsuranceFile == null)
                {
                    bookingPathReservation.TripInsuranceFile = new MOBTripInsuranceFile() { };
                }
                if (bookingPathReservation.TripInsuranceFile.TripInsuranceBookingInfo == null)
                {
                    bookingPathReservation.TripInsuranceFile.TripInsuranceBookingInfo = new MOBTPIInfoInBookingPath() { };
                }
                List<string> contentInBooking = new List<string>();
                contentInBooking.Add(contentInBooking1);
                contentInBooking.Add(contentInBooking2);
                contentInBooking.Add(contentInBooking3);
                bookingPathReservation.TripInsuranceFile.TripInsuranceBookingInfo.Content = contentInBooking;
                bookingPathReservation.TripInsuranceFile.TripInsuranceBookingInfo.Header = header1 + " <b>" + header2 + "</b>";
                bookingPathReservation.TripInsuranceFile.TripInsuranceBookingInfo.LegalInformation = legalInfo;
                bookingPathReservation.TripInsuranceFile.TripInsuranceBookingInfo.TncSecondaryFOPPage = (mobTgiLimitationMessage.IsNullOrEmpty() && mobTgiReadMessage.IsNullOrEmpty()) ? tncPaymentText1 + " <a href =\"" + tncPaymentLinkMessage + "\" target=\"_blank\">" + tncPaymentText2 + "</a> " + tncPaymentText3
                : tncPaymentText1 + " " + tncPaymentText3 + " <a href =\"" + tncPaymentLinkMessage + "\" target=\"_blank\">" + tncPaymentText2 + "</a> " + mobTgiAndMessage + " <a href =\"" + tncProductPageLinkMessage + "\" target=\"_blank\">" + tncProductPageText2 + "</a> ";
                bookingPathReservation.TripInsuranceFile.TripInsuranceBookingInfo.ConfirmationMsg = @confirmationResponseDetailMessage1.Replace("(R)", specialCharacter) + "\n\n" + confirmationResponseDetailMessage2;

                await _sessionHelperService.SaveSession<Reservation>(bookingPathReservation, sessionId, new List<string> { sessionId, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName).ConfigureAwait(false);
            }
            else
            {
                tripInsuranceInfo = null;
            }

            return tripInsuranceInfo;
        }

        private string GetTPIQuoteId(System.Collections.ObjectModel.Collection<Service.Presentation.CommonModel.Characteristic> characteristics)
        {
            string productId = string.Empty;
            productId = characteristics.FirstOrDefault(a => !string.IsNullOrEmpty(a.Code) && a.Code.ToUpper().Trim() == "QUOTEPACKID").Value.Trim();

            return productId;
        }

        private MOBTPIInfo GetTPIAmountAndFormattedAmount(MOBTPIInfo tripInsuranceInfo, System.Collections.ObjectModel.Collection<Service.Presentation.ProductModel.SubProduct> subProducts)
        {
            string currencyCode = string.Empty;
            var prices = subProducts.Where(a => a.Prices != null && a.Prices.Count > 0).FirstOrDefault().Prices;
            foreach (var price in prices)
            {
                if (price != null && price.PaymentOptions != null && price.PaymentOptions.Count > 0)
                {
                    foreach (var option in price.PaymentOptions)
                    {
                        if (option != null && option.Type != null && option.Type.ToUpper().Contains("TOTALPRICE"))
                        {
                            foreach (var PriceComponent in option.PriceComponents)
                            {
                                foreach (var total in PriceComponent.Price.Totals)
                                {
                                    tripInsuranceInfo.Amount = total.Amount;
                                    currencyCode = total.Currency.Code.ToUpper().Trim();
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            // concate currency sign and round up amount //removed
            // real amount concat with currency sign
            tripInsuranceInfo.DisplayAmount = AttachCurrencySymbol(tripInsuranceInfo.Amount, currencyCode, false);
            if (tripInsuranceInfo.Amount <= 0)
            {
                tripInsuranceInfo = null;
            }

            return tripInsuranceInfo;
        }

        private string AttachCurrencySymbol(double amount, string currencyCode, bool isRoundUp)
        {
            string formattedDisplayAmount = string.Empty;
            CultureInfo ci = TopHelper.GetCultureInfo(currencyCode);
            formattedDisplayAmount = TopHelper.FormatAmountForDisplay(string.Format("{0:#,0.00}", amount), ci, isRoundUp);

            return formattedDisplayAmount;
        }

        private bool ValidateFormOfPaymentType(MOBFormofPaymentDetails formOfPayment)
        {
            return formOfPayment != null && formOfPayment.FormOfPaymentType == MOBFormofPayment.CreditCard.ToString() && formOfPayment.CreditCard != null && !string.IsNullOrEmpty(formOfPayment.CreditCard.CardType) &&
                                    IsValidFOPForTPIpayment(formOfPayment.CreditCard.CardType);
        }

        private bool IsValidFOPForTPIpayment(string cardType)
        {
            return !string.IsNullOrEmpty(cardType) &&
                (cardType.ToUpper().Trim() == "VI" || cardType.ToUpper().Trim() == "MC" || cardType.ToUpper().Trim() == "AX" || cardType.ToUpper().Trim() == "DS");
        }

        private async Task<MOBSection> GetTPIAlertMessage(bool isPurchaseMessage, Session session = null, string flow = "")
        {
            bool isEnabledTripInsuranceV2 = false;
            if (session != null)
            {
                isEnabledTripInsuranceV2 = await _featureToggles.IsEnabledTripInsuranceV2(session.AppID, session.VersionID, session.CatalogItems).ConfigureAwait(false);
            }

            var tPIMessageKey = isPurchaseMessage ?
                                (isEnabledTripInsuranceV2 && !string.IsNullOrEmpty(flow) && flow.Equals(FlowType.BOOKING.ToString()) ?
                                    "TPI_V2_PURCHASE_FAILED_MESSAGE"
                                    : "TPI_PURCHASE_FAILED_MESSAGE")
                                : "TPI_BILLED_SEPARATE_TEXT";

            List<MOBItem> alertMsgDB = await GetMessagesFromDb(tPIMessageKey);

            return AssignAlertMessage(alertMsgDB);
        }

        private async Task<List<MOBItem>> GetMessagesFromDb(string seatMessageKey)
        {
            return seatMessageKey.IsNullOrEmpty()
                    ? null
                    : await GetMPPINPWDTitleMessages(seatMessageKey);
        }

        private async Task<List<MOBItem>> GetMPPINPWDTitleMessages(string titleList)
        {
            List<MOBItem> messages = new List<MOBItem>();
            List<United.Definition.MOBLegalDocument> docs = await _legalDocumentsForTitlesService.GetNewLegalDocumentsForTitles(titleList, _headers.ContextValues.TransactionId, true);

            if (docs != null && docs.Count > 0)
            {
                foreach (var doc in docs)
                {
                    MOBItem item = new MOBItem();
                    item.Id = doc.Title;
                    item.CurrentValue = doc.LegalDocument;
                    messages.Add(item);
                }
            }

            return messages;
        }

        private MOBSection AssignAlertMessage(List<MOBItem> seatAssignmentMessage)
        {
            MOBSection alertMsg = new MOBSection() { IsDefaultOpen = _configuration.GetValue<bool>("EnableCombineConfirmationAlerts") ? false : true };
            if (seatAssignmentMessage != null && seatAssignmentMessage.Count > 0)
            {
                foreach (var msg in seatAssignmentMessage)
                {
                    if (msg != null)
                    {
                        switch (msg.Id.ToUpper())
                        {
                            case "HEADER":
                                alertMsg.Text1 = msg.CurrentValue.Trim();
                                break;
                            case "BODY":
                                alertMsg.Text2 = msg.CurrentValue.Trim();
                                break;
                            case "FOOTER":
                                alertMsg.Text3 = msg.CurrentValue.Trim();
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            return alertMsg;
        }

        private List<MOBSHOPPrice> UpdatePriceForUnRegisterTPI(List<MOBSHOPPrice> persistedPrices)
        {
            List<MOBSHOPPrice> prices = null;
            if (persistedPrices != null && persistedPrices.Count > 0)
            {
                double travelOptionSubTotal = 0.0;
                foreach (var price in persistedPrices)
                {
                    // Add TPI here 
                    if (price.PriceType.ToUpper().Equals("TRAVEL INSURANCE"))
                    {
                        travelOptionSubTotal = travelOptionSubTotal + Convert.ToDouble(price.DisplayValue);
                    }
                    else if (!price.PriceType.ToUpper().Equals("GRAND TOTAL"))
                    {
                        if (prices == null)
                        {
                            prices = new List<MOBSHOPPrice>();
                        }
                        prices.Add(price);
                    }
                }

                foreach (var price in persistedPrices)
                {
                    if (price.PriceType.ToUpper().Equals("GRAND TOTAL"))
                    {
                        double grandTotal = Convert.ToDouble(price.DisplayValue);
                        price.DisplayValue = string.Format("{0:#,0.00}", grandTotal - travelOptionSubTotal);
                        price.FormattedDisplayValue = string.Format("${0:c}", price.DisplayValue);
                        double tempDouble1 = 0;
                        double.TryParse(price.DisplayValue.ToString(), out tempDouble1);
                        price.Value = Math.Round(tempDouble1, 2, MidpointRounding.AwayFromZero);
                        if (prices == null)
                        {
                            prices = new List<MOBSHOPPrice>();
                        }
                        prices.Add(price);
                    }
                }
            }

            return prices;
        }

        private bool ShowTPIQuoteInfo(List<MOBSHOPTravelOption> travelOptions)
        {
            bool isShowTPIQuote = true;
            if (travelOptions != null && travelOptions.Count() > 0)
            {
                foreach (var travelOption in travelOptions)
                {
                    if (travelOption != null && !string.IsNullOrEmpty(travelOption.Key) && (travelOption.Key.ToUpper().Trim() == "TPI" || travelOption.Key.ToUpper().Trim() == "FARELOCK"))
                    {
                        isShowTPIQuote = false;
                        break;
                    }
                }
            }

            return isShowTPIQuote;
        }

        private async Task<MOBDOTBaggageInfo> GetDOTBaggageInfo(string transactionId, string languageCode, int applicationId, MOBSHOPReservation reservation)
        {
            MOBDOTBaggageInfo dotBaggageInfo = null;

            string channelId = string.Empty;
            string channelName = string.Empty;
            if (_configuration.GetValue<bool>("EnabledMERCHChannels"))
            {
                string merchChannel = "MOBBE";
                SetMerchandizeChannelValues(merchChannel, ref channelId, ref channelName);

            }
            else
            {
                channelId = _configuration.GetValue<string>("MerchandizeOffersServiceChannelID").ToString().Trim();// "401";  //Changed per Praveen Vemulapalli email
                channelName = _configuration.GetValue<string>("MerchandizeOffersServiceChannelName").ToString().Trim();//"MBE";  //Changed per Praveen Vemulapalli email
            }

            var dotBaggageInfoResponse = await GetDOTBaggageInfoWithPNR("ACCESSCODE", transactionId, languageCode, "XML", applicationId,
                reservation.RecordLocator, "01/01/0001", channelId, channelName, reservation);
            if (dotBaggageInfoResponse != null)
            {
                dotBaggageInfo = dotBaggageInfoResponse.DotBaggageInfo;
            }

            return dotBaggageInfo;
        }

        private void SetMerchandizeChannelValues(string merchChannel, ref string channelId, ref string channelName)
        {
            channelId = string.Empty;
            channelName = string.Empty;

            if (merchChannel != null)
            {
                switch (merchChannel)
                {
                    case "MOBBE":
                        channelId = _configuration.GetValue<string>("MerchandizeOffersServiceMOBBEChannelID").ToString().Trim();
                        channelName = _configuration.GetValue<string>("MerchandizeOffersServiceMOBBEChannelName").ToString().Trim();
                        break;
                    case "MOBMYRES":
                        channelId = _configuration.GetValue<string>("MerchandizeOffersServiceMOBMYRESChannelID").ToString().Trim();
                        channelName = _configuration.GetValue<string>("MerchandizeOffersServiceMOBMYRESChannelName").ToString().Trim();
                        break;
                    case "MOBWLT":
                        channelId = _configuration.GetValue<string>("MerchandizeOffersServiceMOBWLTChannelID").ToString().Trim();
                        channelName = _configuration.GetValue<string>("MerchandizeOffersServiceMOBWLTChannelName").ToString().Trim();
                        break;
                    default:
                        break;
                }
            }
        }

        public async Task<MOBDOTBaggageInfoResponse> GetDOTBaggageInfoWithPNR(string accessCode, string transactionId, string languageCode, string messageFormat, int applicationId, string recordLocator, string ticketingDate, string channelId, string channelName, MOBSHOPReservation reservation = null, United.Service.Presentation.ReservationModel.Reservation cslReservation = null)
        {
            MOBDOTBaggageInfoRequest request = new MOBDOTBaggageInfoRequest();
            request.TransactionId = "Mobile_Request_11";
            request.AccessCode = accessCode;
            request.TransactionId = transactionId;
            request.LanguageCode = languageCode;
            request.Application = new MOBApplication();
            request.Application.Id = applicationId;
            request.RecordLocator = recordLocator;
            request.TraverlerINfo = new List<MOBDOTBaggageTravelerInfo>();
            MOBDOTBaggageTravelerInfo travelInfo = new MOBDOTBaggageTravelerInfo();
            travelInfo.Id = "1";
            if (ticketingDate.Trim() == "" || ticketingDate.Trim() == "01/01/0001")
            {
                ticketingDate = DateTime.Now.ToString("MM/dd/yyyy");
            }
            else if (DateTime.ParseExact(ticketingDate, "MM/dd/yyyy", CultureInfo.InvariantCulture) < DateTime.Now.AddYears(-2) || DateTime.ParseExact(ticketingDate, "MM/dd/yyyy", CultureInfo.InvariantCulture) < DateTime.Now.AddDays(1)) // If passed any default date which is not accurate
            {
                ticketingDate = DateTime.Now.ToString("MM/dd/yyyy");
            }
            travelInfo.TicketingDate = DateTime.ParseExact(ticketingDate, "MM/dd/yyyy", CultureInfo.InvariantCulture);
            travelInfo.TicketingDateString = ticketingDate;
            request.TraverlerINfo.Add(travelInfo);

            _logger.LogInformation("GetDOTBaggageInfoWithPNR Request {request} with {sessionId}", JsonConvert.SerializeObject(request), _headers.ContextValues.SessionId);


            MOBDOTBaggageInfoResponse response = new MOBDOTBaggageInfoResponse();
            try
            {
                response = await GetDOTBaggageInfoWithPNR(request, channelId, channelName, reservation, cslReservation);
                response.Request = request;
            }
            catch (MOBUnitedException uex)
            {
                response.DotBaggageInfo = null;
                response.DotBaggageInfo = new MOBDOTBaggageInfo();
                response.DotBaggageInfo.ErrorMessage = _configuration.GetValue<string>("DOTBaggageGenericExceptionMessage");
                response.Exception = new MOBException();
                response.Exception.Message = uex.Message;
            }
            catch (System.Exception ex)
            {
                response.DotBaggageInfo = null;
                response.DotBaggageInfo = new MOBDOTBaggageInfo();
                response.DotBaggageInfo.ErrorMessage = _configuration.GetValue<string>("DOTBaggageGenericExceptionMessage");

                response.Exception = new MOBException("99999", _configuration.GetValue<string>("GenericExceptionMessage"));
            }

            _logger.LogInformation("GetDOTBaggageInfoWithPNR Response {response} with {sessionId}", JsonConvert.SerializeObject(response), _headers.ContextValues.SessionId);

            return response;
        }

        public async Task<MOBDOTBaggageInfoResponse> GetDOTBaggageInfoWithPNR(MOBDOTBaggageInfoRequest request, string channelId, string channelName, MOBSHOPReservation reservation, United.Service.Presentation.ReservationModel.Reservation cslReservation)
        {
            MOBDOTBaggageInfoResponse response = new MOBDOTBaggageInfoResponse();
            try
            {
                MerchandizingServicesClient merchClient = null;
                if (_configuration.GetValue<string>("AssignTimeOutForMerchandizeDOTBaggageServiceCall") != null && Convert.ToBoolean(_configuration.GetValue<string>("AssignTimeOutForMerchandizeDOTBaggageServiceCall")))
                {
                    #region Assigne Time Out Value for Merchantize Engine Call
                    MerchandizingServicesClient merchClient1 = new MerchandizingServicesClient();
                    int timeOutSeconds = _configuration.GetValue<string>("TimeOutSecondsForMerchandizeDOTBaggage") != null ? Convert.ToInt32(_configuration.GetValue<string>("TimeOutSecondsForMerchandizeDOTBaggage").Trim()) : 7;
                    BasicHttpBinding binding = new BasicHttpBinding();
                    TimeSpan timeout = new TimeSpan(0, 0, timeOutSeconds);
                    binding.CloseTimeout = timeout;
                    binding.SendTimeout = timeout;
                    binding.ReceiveTimeout = timeout;
                    binding.OpenTimeout = timeout;
                    var merchEndpoint = merchClient1.Endpoint.Address.ToString();
                    EndpointAddress endPoint = new EndpointAddress(merchEndpoint);
                    merchClient = new MerchandizingServicesClient(binding, endPoint);
                    #endregion
                }
                else
                {
                    merchClient = new MerchandizingServicesClient();
                }
                var offer = new GetMerchandizingOffersInput();
                offer.MerchOfferRequest = new MerchOfferRequest();
                offer.MerchOfferRequest = ConfigUtility.EnableIBEFull() && cslReservation != null
                                   ? BuildBagRequestFromCslreservationDetail(cslReservation, channelId, channelName)
                                   : MerchOfferRequestWithPNR(request, channelId, channelName, reservation);

                _logger.LogInformation("GetDOTBaggageInfoWithPNR Request {request} with {sessionId}", JsonConvert.SerializeObject(request), _headers.ContextValues.SessionId);

                try
                {
                    GetMerchandizingOffersOutput offers = await merchClient.GetMerchandizingOffersAsync(offer.MerchOfferRequest);

                    _logger.LogInformation("GetDOTBaggageInfoWithPNR Response from backend {resposne} with {sessionId}", offers, _headers.ContextValues.SessionId);

                    response.DotBaggageInfo = await MapDOTBagInfoFromMerchandizingResponse(offers.MerchOffers, reservation, _mscBaggageInfoProvider);
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
            catch (System.Exception ex) { throw ex; }

            _logger.LogInformation("GetDOTBaggageInfoWithPNR Response {resposne} with {sessionId}", JsonConvert.SerializeObject(response), _headers.ContextValues.SessionId);

            return response;
        }

        private async Task<MOBDOTBaggageInfo> MapDOTBagInfoFromMerchandizingResponse(MerchandizingServices.MerchOffers offers, MOBSHOPReservation reservation, IMSCBaggageInfo mscBaggageInfoProvider)
        {
            #region
            var baggageInfo = await mscBaggageInfoProvider.GetBaggageInfo(reservation);
            List<MOBBagFeesPerSegment> baggageFeesPerSegment = new List<MOBBagFeesPerSegment>();
            MOBBagFeesPerSegment obj1 = new MOBBagFeesPerSegment();
            bool isFirstBagFree = true;
            decimal chaseOfferAmount = 0; // Added new variable for getting the highest baggage fee
            var chaseOfferCurrency = "USD"; // Added new variable for getting the highest baggage fee
            string chaseFOPMessageText = string.Empty;
            if (offers != null && offers.Itinerary != null)
            {
                #region
                foreach (ItineraryType itinerary in offers.Itinerary)
                {
                    #region
                    if (itinerary.Items != null && itinerary.Items.Count() > 0)
                    {
                        foreach (PricedItineraryType item in itinerary.Items)
                        {
                            if (item.AirItinerary != null && item.AirItinerary.OriginDestinationOptions != null)
                            {
                                foreach (OriginDestinationOptionType orgdestType in item.AirItinerary.OriginDestinationOptions)
                                {
                                    #region
                                    obj1 = GetFirstAndSecondBagFeePerTrip(offers, orgdestType.FlightSegment[0].Id.Trim(), ref chaseFOPMessageText, ref chaseOfferAmount, ref chaseOfferCurrency);
                                    int i = orgdestType.FlightSegment.Count();
                                    MOBBagFeesPerSegment obj2 = new MOBBagFeesPerSegment();
                                    obj2.FirstBagFee = obj1.FirstBagFee;
                                    obj2.SecondBagFee = obj1.SecondBagFee;
                                    obj2.WeightPerBag = obj1.WeightPerBag;
                                    obj2.IsFirstBagFree = obj1.IsFirstBagFree;
                                    obj2.RegularFirstBagFee = obj1.RegularFirstBagFee;
                                    obj2.RegularSecondBagFee = obj1.RegularSecondBagFee;
                                    obj2.FlightTravelDate = orgdestType.FlightSegment[0].DepartureDateTime.ToString("ddd., MMM dd, yyyy");
                                    obj2.OriginAirportCode = orgdestType.FlightSegment[0].DepartureAirport.LocationCode;
                                    string airportName = string.Empty;
                                    string cityName = string.Empty;
                                    var tupleResponse = await GetAirportCityName(obj2.OriginAirportCode);
                                    airportName = tupleResponse.airportName;
                                    cityName = tupleResponse.cityName;
                                    obj2.OriginAirportName = airportName;
                                    obj2.DestinationAirportCode = orgdestType.FlightSegment[i - 1].ArrivalAirport.LocationCode;//To get the destination
                                    airportName = string.Empty;
                                    cityName = string.Empty;
                                    var tupleResponse2 = await GetAirportCityName(obj2.DestinationAirportCode);
                                    airportName = tupleResponse.airportName;
                                    cityName = tupleResponse.cityName;
                                    obj2.DestinationAirportName = airportName;
                                    if (!obj2.IsFirstBagFree)
                                    {
                                        isFirstBagFree = false;
                                    }
                                    baggageFeesPerSegment.Add(obj2);
                                    #endregion
                                }
                            }
                        }
                    }
                    #endregion
                }
                #endregion
            }
            bool showChaseCardMessage = CheckShowChaseBagFeeMessage(offers);
            if (isFirstBagFree && !showChaseCardMessage)
                if (isFirstBagFree)
                {
                    baggageInfo.Title3 = "";
                    baggageInfo.Description3 = "";
                }
                else
                {
                    if (!_configuration.GetValue<bool>("SupressFixForCheckedBagsChaseOfferDynamicPrice") && !isFirstBagFree && chaseOfferAmount > 0) //Added this to get the highest first baggage price of IBE,IBELite
                    {
                        var chaseFormatedOfferAmount = FormatedBagFee(chaseOfferAmount * 2, chaseOfferCurrency);
                        baggageInfo.Description3 = string.Format(baggageInfo.Description3, chaseFormatedOfferAmount);
                    }
                }
            baggageInfo.Title3 = ""; // According ot curtis do not show the chase free bag header for My Flights (view reservation also comes as my flights)
            baggageInfo.BaggageFeesPerSegment = baggageFeesPerSegment;
            if (!String.IsNullOrEmpty(chaseFOPMessageText))
            {
                baggageInfo.Description3 = baggageInfo.Description3 + " " + chaseFOPMessageText;
            }

            return baggageInfo;
            #endregion
        }

        private string FormatedBagFee(decimal amount, string currencyCode)
        {
            if (currencyCode.Equals("USD"))
            {
                return "$" + amount.ToString().Replace(".00", "");
            }
            else
            {
                return amount.ToString().Replace(".00", "") + " " + currencyCode;
            }
        }

        private bool CheckShowChaseBagFeeMessage(MerchandizingServices.MerchOffers offers)
        {
            #region
            foreach (ItineraryType itinerary in offers.Itinerary)
            {
                #region
                if (itinerary.TravelerInfo != null)
                {
                    if (itinerary.TravelerInfo != null && itinerary.TravelerInfo.Traveler != null)
                    {
                        foreach (TravelerType traveler in itinerary.TravelerInfo.Traveler)
                        {
                            if (traveler.Loyalty != null)
                            {
                                foreach (CustomerLoyaltyType loyalty in traveler.Loyalty)
                                {
                                    if (loyalty.CardType != null && (loyalty.CardType == CustomerLoyaltyTypeCardType.PPC || loyalty.CardType == CustomerLoyaltyTypeCardType.MEC || loyalty.CardType == CustomerLoyaltyTypeCardType.OPC || loyalty.CardType == CustomerLoyaltyTypeCardType.CCC))
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion
            }
            return true;
            #endregion
        }

        private async Task<(string airportName, string cityName)> GetAirportCityName(string airportCode)
        {
            #region
            try
            {
                AirportDynamoDB airportDynamoDB = new AirportDynamoDB(_configuration, _dynamoDBService);
                return await airportDynamoDB.GetAirportCityName(airportCode, _headers.ContextValues.SessionId);

            }
            catch (System.Exception) { }
            return default;
            #endregion
        }

        private MOBBagFeesPerSegment GetFirstAndSecondBagFeePerTrip(MerchandizingServices.MerchOffers offers, string flightSegmentIndex, ref string chaseFOPMessageText, ref decimal chaseOfferAmount, ref string chaseOfferCurrency)
        {
            MOBBagFeesPerSegment obj1 = new MOBBagFeesPerSegment();
            string first1BagFee = "", second2BagFee = ""; string chaseCurreny = "USD"; // Chase FOP Fix
            if (offers != null && offers.Itinerary != null)
            {
                #region
                foreach (ItineraryType itinerary in offers.Itinerary)
                {
                    #region
                    if (itinerary.MerchandisingInfo != null && itinerary.MerchandisingInfo.Services != null)
                    {
                        foreach (MerchandizingServices.ServiceType service in itinerary.MerchandisingInfo.Services)
                        {
                            if (service != null && service.Tiers != null)
                            {
                                foreach (TierType tier in service.Tiers)
                                {
                                    if (tier.TierInfo != null && tier.TierInfo.Group == TierInfoTypeGroup.BG && tier.TierInfo.SubGroup == TierInfoTypeSubGroup.BG && tier.TierExtension != null && tier.TierExtension.Bags != null && tier.TierExtension.Bags.Bag != null && tier.Pricing != null && tier.Pricing[0] != null && tier.Pricing[0].Association != null)
                                    {
                                        foreach (AssociationTypeSegmentMapping item in tier.Pricing[0].Association.Items)
                                        {
                                            if (item.SegmentReference.Trim() == flightSegmentIndex)
                                            {
                                                #region

                                                string currencyCode = "USD";

                                                if (tier.TierExtension.Bags.Bag[0] != null && Convert.ToInt32(tier.TierExtension.Bags.Bag[0].MinQuantity.Trim()) <= 1 && Convert.ToInt32(tier.TierExtension.Bags.Bag[0].MaxQuantity.Trim()) >= 1)
                                                {
                                                    #region

                                                    if (!string.IsNullOrEmpty(tier.Pricing[0].PaymentOptions[0].Price[0].TotalPrice.currencycode) && tier.Pricing[0].PaymentOptions[0].Price[0].TotalPrice.currencycode.Trim().ToUpper().Equals("USD"))
                                                    {
                                                        currencyCode = "USD";
                                                    }
                                                    else
                                                    {
                                                        currencyCode = tier.Pricing[0].PaymentOptions[0].Price[0].TotalPrice.currencycode;
                                                    }

                                                    if (currencyCode.Equals("USD"))
                                                    {
                                                        obj1.FirstBagFee = "$" + tier.Pricing[0].PaymentOptions[0].Price[0].TotalPrice.amount.ToString().Replace(".00", "");
                                                    }
                                                    else
                                                    {
                                                        obj1.FirstBagFee = tier.Pricing[0].PaymentOptions[0].Price[0].TotalPrice.amount.ToString().Replace(".00", "") + " " + currencyCode;
                                                    }

                                                    first1BagFee = tier.Pricing[0].PaymentOptions[0].Price[0].TotalPrice.amount.ToString().Replace(".00", ""); // Chase FOP Fix

                                                    obj1.WeightPerBag = tier.TierExtension.Bags.Bag[0].Weight[1].Max + " " + tier.TierExtension.Bags.Bag[0].Weight[1].Unit + " (" + tier.TierExtension.Bags.Bag[0].Weight[0].Max + " " + tier.TierExtension.Bags.Bag[0].Weight[0].Unit + ")";//."50 lbs (23 kg)";
                                                    if (tier.TierInfo.Type == TierInfoTypeType.Allowance)
                                                    {
                                                        obj1.IsFirstBagFree = true;
                                                        if (currencyCode.Equals("USD"))
                                                        {
                                                            obj1.FirstBagFee = "$0";
                                                        }
                                                        else
                                                        {
                                                            obj1.FirstBagFee = "0 " + currencyCode;
                                                        }

                                                        first1BagFee = "0"; // Chase FOP Fix

                                                        if (tier.TierExtension.Bags.Bag[0].RegularPrice != null && tier.TierExtension.Bags.Bag[0].RegularPrice[0].Price != null)
                                                        {
                                                            if (currencyCode.Equals("USD"))
                                                            {
                                                                obj1.RegularFirstBagFee = "$" + tier.TierExtension.Bags.Bag[0].RegularPrice[0].Price.BasePrice.amount.ToString().Replace(".00", "");
                                                            }
                                                            else
                                                            {
                                                                obj1.RegularFirstBagFee = tier.TierExtension.Bags.Bag[0].RegularPrice[0].Price.BasePrice.amount.ToString().Replace(".00", "") + " " + currencyCode;
                                                            }

                                                        }
                                                    }
                                                    #endregion
                                                    chaseCurreny = currencyCode;
                                                }
                                                if (tier.TierExtension.Bags.Bag[0] != null && Convert.ToInt32(tier.TierExtension.Bags.Bag[0].MinQuantity.Trim()) <= 2 && Convert.ToInt32(tier.TierExtension.Bags.Bag[0].MaxQuantity.Trim()) >= 2)
                                                {
                                                    if (!string.IsNullOrEmpty(tier.Pricing[0].PaymentOptions[0].Price[0].TotalPrice.currencycode) && tier.Pricing[0].PaymentOptions[0].Price[0].TotalPrice.currencycode.Trim().ToUpper().Equals("USD"))
                                                    {
                                                        currencyCode = "USD";
                                                    }
                                                    else
                                                    {
                                                        currencyCode = tier.Pricing[0].PaymentOptions[0].Price[0].TotalPrice.currencycode;
                                                    }

                                                    if (currencyCode.Equals("USD"))
                                                    {
                                                        obj1.SecondBagFee = "$" + tier.Pricing[0].PaymentOptions[0].Price[0].TotalPrice.amount.ToString().Replace(".00", "");
                                                    }
                                                    else
                                                    {
                                                        obj1.SecondBagFee = tier.Pricing[0].PaymentOptions[0].Price[0].TotalPrice.amount.ToString().Replace(".00", "") + " " + currencyCode;
                                                    }

                                                    second2BagFee = tier.Pricing[0].PaymentOptions[0].Price[0].TotalPrice.amount.ToString().Replace(".00", "");// Chase FOP Fix

                                                    if (tier.TierInfo.Type == TierInfoTypeType.Allowance)
                                                    {
                                                        if (currencyCode.Equals("USD"))
                                                        {
                                                            obj1.SecondBagFee = "$0";
                                                        }
                                                        else
                                                        {
                                                            obj1.SecondBagFee = "0 " + currencyCode;
                                                        }

                                                        second2BagFee = "0"; // Chase FOP Fix

                                                        if (tier.TierExtension.Bags.Bag[0].RegularPrice != null && tier.TierExtension.Bags.Bag[0].RegularPrice.Length > 0 && tier.TierExtension.Bags.Bag[0].RegularPrice[tier.TierExtension.Bags.Bag[0].RegularPrice.Length - 1].Price != null)
                                                        {
                                                            if (currencyCode.Equals("USD"))
                                                            {
                                                                obj1.RegularSecondBagFee = "$" + tier.TierExtension.Bags.Bag[0].RegularPrice[tier.TierExtension.Bags.Bag[0].RegularPrice.Length - 1].Price.BasePrice.amount.ToString().Replace(".00", "");
                                                            }
                                                            else
                                                            {
                                                                obj1.RegularSecondBagFee = tier.TierExtension.Bags.Bag[0].RegularPrice[tier.TierExtension.Bags.Bag[0].RegularPrice.Length - 1].Price.BasePrice.amount.ToString().Replace(".00", "") + " " + currencyCode;
                                                            }

                                                        }
                                                    }
                                                    chaseCurreny = currencyCode;
                                                }
                                                #endregion                                                
                                            }
                                        }
                                    }
                                }

                            }
                        }
                    }
                    else
                    {
                        throw new MOBUnitedException(_configuration.GetValue<string>("DOTBaggageGenericExceptionMessage"));
                    }
                    #endregion
                }
                #endregion
            }
            #region // Chase FOP Fix
            if (first1BagFee != "0")
            {
                obj1.IsFirstBagFree = false;
                obj1.RegularFirstBagFee = "";
                chaseFOPMessageText = (_configuration.GetValue<string>("ChaseFOPTextMessage") == null ? string.Empty : _configuration.GetValue<string>("ChaseFOPTextMessage").ToString());
            }
            if (second2BagFee != "0")
            {
                obj1.RegularSecondBagFee = "";
                chaseFOPMessageText = (_configuration.GetValue<string>("ChaseFOPTextMessage") == null ? string.Empty : _configuration.GetValue<string>("ChaseFOPTextMessage").ToString());
            }

            if (!_configuration.GetValue<bool>("SupressFixForCheckedBagsChaseOfferDynamicPrice") && !string.IsNullOrEmpty(first1BagFee) && (Convert.ToDecimal(first1BagFee) > chaseOfferAmount)) //Added this to get the highest first baggage price of IBE,IBELite
            {
                chaseOfferAmount = Convert.ToDecimal(first1BagFee);
                chaseOfferCurrency = chaseCurreny;
            }
            #endregion

            return obj1;
        }

        private MerchOfferRequest MerchOfferRequestWithPNR(MOBDOTBaggageInfoRequest request, string channelId, string channelName, MOBSHOPReservation reservation)
        {
            MerchOfferRequest offerRequest = new MerchOfferRequest();
            RequestCriterionType requestCriterionType = new RequestCriterionType();
            RequestCriterionTypeBookingReferenceIds[] pnrs = new RequestCriterionTypeBookingReferenceIds[1];
            RequestCriterionTypeBookingReferenceIds pnr = new RequestCriterionTypeBookingReferenceIds();
            pnr.BookingReferenceId = request.RecordLocator;
            pnrs[0] = pnr;
            requestCriterionType.BookingReferenceIds = pnrs;

            #region

            ArrayOfIncludeExcludeOffersTypeServiceFilterCodeServiceFilterCode[] includeExcludeOffersList =
                new ArrayOfIncludeExcludeOffersTypeServiceFilterCodeServiceFilterCode[1];
            ArrayOfIncludeExcludeOffersTypeServiceFilterCodeServiceFilterCode includeExcludeOffer =
                new ArrayOfIncludeExcludeOffersTypeServiceFilterCodeServiceFilterCode();
            includeExcludeOffer.ServiceCode = ServiceFilterGroupTypeServiceCode.BAG;
            includeExcludeOffer.ResultAction = ServiceFilterGroupTypeResultAction.Include;
            includeExcludeOffersList[0] = new ArrayOfIncludeExcludeOffersTypeServiceFilterCodeServiceFilterCode();
            includeExcludeOffersList[0] = includeExcludeOffer;
            requestCriterionType.IncludeExcludeOffers = includeExcludeOffersList;

            #endregion

            #region Traveler

            if (ConfigUtility.EnableIBEFull() && reservation != null)
            {
                if (reservation.TravelersCSL != null && reservation.TravelersCSL.Count > 0)
                {
                    TravelerInfoType travelerInfoType1 = new TravelerInfoType();
                    List<TravelerType> lst = new List<TravelerType>();


                    foreach (var traveler in reservation.TravelersCSL)
                    {
                        TravelerType tType = new TravelerType();
                        tType.Id = traveler.PaxIndex + 1.ToString();

                        tType.GivenName = traveler.FirstName;
                        tType.Surname = traveler.LastName;
                        if (!string.IsNullOrEmpty(traveler.TravelerTypeCode))
                        {
                            tType.PassengerTypeCode =
                                (TravelerTypePassengerTypeCode)
                                    System.Enum.Parse(typeof(TravelerTypePassengerTypeCode), traveler.TravelerTypeCode);
                        }
                        if (!string.IsNullOrEmpty(traveler.GenderCode))
                        {
                            tType.Gender = (GenderType)System.Enum.Parse(typeof(GenderType), (GetGenderCode(traveler.GenderCode)));
                        }
                        tType.TicketingDate = request.TraverlerINfo[0].TicketingDate;
                        tType.TicketingDateSpecified = true;
                        if (traveler.AirRewardPrograms != null && traveler.AirRewardPrograms.Count > 0)
                        {
                            CustomerLoyaltyType[] loyaltyType = new CustomerLoyaltyType[traveler.AirRewardPrograms.Count];
                            for (int l = 0; l < loyaltyType.Count(); l++)
                            {
                                CustomerLoyaltyType clt = new CustomerLoyaltyType()
                                {
                                    ProgramId = traveler.AirRewardPrograms[l].ProgramId,
                                    MemberShipId = traveler.AirRewardPrograms[l].MemberId,
                                    LoyalLevel =
                                        (CustomerLoyaltyTypeLoyalLevel)
                                            System.Enum.Parse(typeof(CustomerLoyaltyTypeLoyalLevel),
                                                (traveler.AirRewardPrograms[l].MPEliteLevel + 1).ToString())

                                };

                                loyaltyType[l] = clt;
                            }
                            tType.Loyalty = loyaltyType;
                        }
                        lst.Add(tType);
                    }

                    travelerInfoType1.Traveler = lst.ToArray<TravelerType>();
                    requestCriterionType.TravelerInfo = travelerInfoType1;
                }

                #endregion Traveler 

                #region OriginDestinationOptions 

                List<OriginDestinationOptionType> lstOriginDestinationOptions = new List<OriginDestinationOptionType>();

                int i = 0;
                int segmentNumber = 0;
                foreach (var trip in reservation.Trips)
                {
                    OriginDestinationOptionType type = new OriginDestinationOptionType();
                    type.Id = "OD" + ++i;
                    FlightSegmentType[] lstFlightSegments = new FlightSegmentType[trip.FlattenedFlights[0].Flights.Count];
                    int j = 0;
                    foreach (var seg in trip.FlattenedFlights[0].Flights)
                    {
                        ++segmentNumber;
                        FlightSegmentType fs = new FlightSegmentType();

                        fs.DepartureAirport = new Location()
                        {
                            LocationCode = seg.Origin
                        };
                        fs.ArrivalAirport = new Location() { LocationCode = seg.Destination };
                        fs.DepartureDateTime = Convert.ToDateTime(seg.DepartureDateTime);
                        fs.DepartureDateTimeSpecified = true;
                        fs.ArrivalDateTime = Convert.ToDateTime(seg.ArrivalDateTime);
                        fs.ArrivalDateTimeSpecified = true;
                        fs.FlightNumber = seg.FlightNumber;
                        fs.OperatingAirline = seg.OperatingCarrier;
                        fs.MarketingAirline = seg.MarketingCarrier;
                        fs.SegmentNumber = segmentNumber.ToString();
                        fs.ClassOfService = seg.ServiceClass;
                        if ((reservation.IsELF ||
                             (reservation.ShopReservationInfo2 != null &&
                              (reservation.ShopReservationInfo2.IsIBE || reservation.ShopReservationInfo2.IsIBELite))))
                        {
                            ArrayOfAdditionalInfoTypeInfoInfo[] additionalInfo = new ArrayOfAdditionalInfoTypeInfoInfo[1];
                            additionalInfo[0] = new ArrayOfAdditionalInfoTypeInfoInfo()
                            {
                                Name = "BEProductCode",
                                Value = seg.ShoppingProducts[0].ProductCode
                            };
                            fs.AdditionalInfo = additionalInfo;
                        }
                        fs.ActiveInd = FlightDetailsActiveInd.Y;
                        fs.Id = segmentNumber.ToString();

                        lstFlightSegments[j] = fs;
                        j++;
                    }
                    type.FlightSegment = lstFlightSegments;

                    lstOriginDestinationOptions.Add(type);
                }
                PricedItineraryType[] piType = new PricedItineraryType[1];
                piType[0] = new PricedItineraryType();
                piType[0].AirItinerary = new AirItineraryType();
                piType[0].AirItinerary.OriginDestinationOptions = lstOriginDestinationOptions.ToArray();
                requestCriterionType.Items = piType;

                #endregion OriginDestinationOptions
            }

            #region

            RequestorType requestorType = new RequestorType();
            if (_configuration.GetValue<bool>("EnabledMERCHChannels"))
            {
                requestorType.ChannelId = channelId;
                requestorType.ChannelName = channelName;
            }
            else
            {
                requestorType.ChannelId = _configuration.GetValue<string>("MerchandizeOffersServiceChannelID").ToString().Trim();// "401";  //Changed per Praveen Vemulapalli email
                requestorType.ChannelName = _configuration.GetValue<string>("MerchandizeOffersServiceChannelName").ToString().Trim();//"MBE";  //Changed per Praveen Vemulapalli email
            }

            #endregion

            offerRequest.RequestCriterion = requestCriterionType;
            offerRequest.Requestor = requestorType;
            offerRequest.TransactionIdentifier = "Mobile_Request_With_RecordLocator_" + request.RecordLocator;

            return offerRequest;
        }

        private string GetGenderCode(string gender)
        {
            switch (gender.ToUpper())
            {
                case "M": return "Male";
                case "F": return "Female";
                default: return "Unknown";
            }
        }

        private MerchOfferRequest BuildBagRequestFromCslreservationDetail(United.Service.Presentation.ReservationModel.Reservation reservation, string channelId, string channelName)
        {
            if (!_configuration.GetValue<bool>("EnabledMERCHChannels"))
            {
                channelId = _configuration.GetValue<string>("MerchandizeOffersServiceChannelID").ToString().Trim();// "401";  //Changed per Praveen Vemulapalli email
                channelName = _configuration.GetValue<string>("MerchandizeOffersServiceChannelName").ToString().Trim();//"MBE";  //Changed per Praveen Vemulapalli email
            }
            var offerRequest = new MerchOfferRequest
            {
                RequestCriterion = new RequestCriterionType
                {
                    IncludeExcludeOffers = IncludeExcludeOffersBag(),
                    BookingReferenceIds = BookingReferenceIds(reservation.ConfirmationID),
                    TravelerInfo = TravelerInfo(reservation.Travelers),
                    Items = PricedItinerary(reservation.FlightSegments)
                },
                Requestor = new RequestorType()
                {
                    ChannelId = channelId,
                    ChannelName = channelName
                },
                TransactionIdentifier = "Mobile_Request_With_RecordLocator_" + reservation.ConfirmationID
            };

            return offerRequest;
        }

        private PricedItineraryType[] PricedItinerary(Collection<ReservationFlightSegment> flightSegments)
        {
            var segmentsWithOd = flightSegments.GroupBy(f => f.TripNumber);

            return new PricedItineraryType[]
            {
                new PricedItineraryType
                {
                    AirItinerary = new AirItineraryType
                    {
                        OriginDestinationOptions = segmentsWithOd.Select(f => new OriginDestinationOptionType
                        {
                            Id = "OD" + f.Key,
                            FlightSegment = f.Select(q => BuildFlightSegmentType(q.FlightSegment)).ToArray()
                        }).ToArray()
                    }
                }
            };
        }

        private FlightSegmentType BuildFlightSegmentType(FlightSegment segment)
        {
            return new FlightSegmentType()
            {
                ActiveInd = FlightDetailsActiveInd.Y,
                ActiveIndSpecified = true,
                Id = segment.SegmentNumber.ToString(),
                DepartureAirport = new Location { LocationCode = segment.DepartureAirport.IATACode },
                ArrivalAirport = new Location { LocationCode = segment.ArrivalAirport.IATACode },
                DepartureDateTime = Convert.ToDateTime(segment.DepartureDateTime),
                DepartureDateTimeSpecified = true,
                ArrivalDateTime = Convert.ToDateTime(segment.ArrivalDateTime),
                ArrivalDateTimeSpecified = true,
                FlightNumber = segment.FlightNumber,
                OperatingAirline = segment.OperatingAirlineCode,
                MarketingAirline = segment.MarketedFlightSegment[0].MarketingAirlineCode,
                SegmentNumber = segment.SegmentNumber.ToString(),
                ClassOfService = segment.BookingClasses[0].Code,
                SegmentStatus = segment.FlightSegmentType,
                AdditionalInfo = AdditionalInfo(segment.Characteristic)
            };
        }

        private ArrayOfAdditionalInfoTypeInfoInfo[] AdditionalInfo(Collection<Characteristic> characteristic)
        {
            var productCode = UtilityHelper.GetCharactersticValue(characteristic, "ProductCode");
            return string.IsNullOrEmpty(productCode) ? null :
                    new ArrayOfAdditionalInfoTypeInfoInfo[] { new ArrayOfAdditionalInfoTypeInfoInfo() { Name = "BEProductCode", Value = productCode } };
        }

        private TravelerInfoType TravelerInfo(Collection<United.Service.Presentation.ReservationModel.Traveler> travelers)
        {
            var i = 0;
            return new TravelerInfoType
            {
                Traveler = travelers.Select(t => new TravelerType
                {
                    Id = (++i).ToString(),
                    GivenName = t.Person.GivenName,
                    Surname = t.Person.Surname,
                    PassengerTypeCode = CheckIsPassengerMilatryOnDuty(t.Person.Type),
                    PassengerTypeCodeSpecified = true,
                    Gender = Gender(t.Person.Sex),
                    GenderSpecified = true,
                    Loyalty = Loyalty(t.LoyaltyProgramProfile),
                    TicketNumber = TicketedNumber(t.Tickets),
                    TicketingDate = Convert.ToDateTime(TicketingDate(t.Tickets)),
                    TicketingDateSpecified = true
                }).ToArray()
            };
        }

        private string TicketingDate(Collection<ValueDocument> tickets)
        {
            if (tickets.IsNullOrEmpty()) return DefaultDate;

            var eTicket = tickets.FirstOrDefault(t => t.Type == "ETicketNumber");

            return eTicket == null ? DefaultDate : eTicket.IssueDate;
        }

        private string DefaultDate
        {
            get { return new DateTime(0001, 01, 01).ToString(CultureInfo.InvariantCulture); }
        }

        private string TicketedNumber(Collection<ValueDocument> tickets)
        {
            if (tickets.IsNullOrEmpty()) return null;

            var eTicket = tickets.FirstOrDefault(t => t.Type == "ETicketNumber");

            return eTicket == null ? null : eTicket.DocumentID;
        }

        private CustomerLoyaltyType[] Loyalty(LoyaltyProgramProfile profile)
        {
            return new CustomerLoyaltyType[]
            {
                new CustomerLoyaltyType()
                {
                    ProgramId = profile.LoyaltyProgramCarrierCode,
                    MemberShipId = profile.LoyaltyProgramMemberID,
                    LoyalLevel = GetLoyaltyLevel(profile.LoyaltyProgramMemberTierDescription),
                    Subscriptions = new CustomerLoyaltyTypeSubscriptions() {IsAvailable = false}
                }
            };
        }

        private CustomerLoyaltyTypeLoyalLevel GetLoyaltyLevel(United.Service.Presentation.CommonEnumModel.LoyaltyProgramMemberTierLevel loyaltyProgramMemberTierDescription)
        {
            switch (loyaltyProgramMemberTierDescription)
            {
                case Service.Presentation.CommonEnumModel.LoyaltyProgramMemberTierLevel.GeneralMember:
                    return CustomerLoyaltyTypeLoyalLevel.GeneralMember;
                case Service.Presentation.CommonEnumModel.LoyaltyProgramMemberTierLevel.PremierSilver:
                    return CustomerLoyaltyTypeLoyalLevel.PremierSilver;
                case Service.Presentation.CommonEnumModel.LoyaltyProgramMemberTierLevel.PremierGold:
                    return CustomerLoyaltyTypeLoyalLevel.PremierGold;
                case Service.Presentation.CommonEnumModel.LoyaltyProgramMemberTierLevel.Premier1K:
                    return CustomerLoyaltyTypeLoyalLevel.Premier1K;
                case Service.Presentation.CommonEnumModel.LoyaltyProgramMemberTierLevel.PremierPlatinum:
                    return CustomerLoyaltyTypeLoyalLevel.PremierPlatinum;
                case Service.Presentation.CommonEnumModel.LoyaltyProgramMemberTierLevel.GlobalServices:
                    return CustomerLoyaltyTypeLoyalLevel.GlobalServices;
                case Service.Presentation.CommonEnumModel.LoyaltyProgramMemberTierLevel.StarSilver:
                    return CustomerLoyaltyTypeLoyalLevel.StarAllianceSilver;
                case Service.Presentation.CommonEnumModel.LoyaltyProgramMemberTierLevel.StarGold:
                    return CustomerLoyaltyTypeLoyalLevel.StarAllianceGold;
                default:
                    return CustomerLoyaltyTypeLoyalLevel.Unknown;
            }
        }

        private GenderType Gender(string sex)
        {
            sex = sex ?? string.Empty;
            switch (sex.ToUpper().Trim())
            {
                case "M":
                    return GenderType.Male;
                case "F":
                    return GenderType.Female;
                default:
                    return GenderType.Unknown;
            }
        }

        private TravelerTypePassengerTypeCode CheckIsPassengerMilatryOnDuty(string passengerType)
        {
            if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.ACC.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.ACC; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.ADD.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.ADD; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.ADT.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.ADT; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.AGT.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.AGT; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.ANN.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.ANN; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.ASB.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.ASB; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.AST.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.AST; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.BAG.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.BAG; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.BLD.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.BLD; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.BNN.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.BNN; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.BRV.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.BRV; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.CMA.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.ANN; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.CMP.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.CMP; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.CNN.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.CNN; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.CPN.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.CPN; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.DAT.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.DAT; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.DIS.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.DIS; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.DOD.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.DOD; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.ECH.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.ECH; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.EDT.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.EDT; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.FFP.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.FFP; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.FFY.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.FFY; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.GNN.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.GNN; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.GRP.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.GRP; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.INF.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.INF; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.INS.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.INS; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.MBT.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.MBT; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.MCR.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.MCR; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.MDP.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.MDP; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.MIL.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.MIL; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.MIR.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.MIR; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.MNF.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.MNF; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.MNN.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.MNN; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.MNS.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.MNS; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.MPA.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.MPA; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.MRE.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.MRE; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.MSB.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.MSB; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.MUS.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.MUS; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.MXS.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.MXS; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.NAT.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.NAT; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.REC.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.REC; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.REF.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.REF; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.SPA.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.SPA; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.SRC.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.SRC; }
            else if (passengerType.ToString().ToUpper().Trim() == TravelerTypePassengerTypeCode.ZED.ToString().ToUpper().Trim()) { return TravelerTypePassengerTypeCode.ZED; }

            return TravelerTypePassengerTypeCode.ADT;
        }

        private RequestCriterionTypeBookingReferenceIds[] BookingReferenceIds(string recordLocator)
        {
            return new[]
            {
                new RequestCriterionTypeBookingReferenceIds
                {
                    BookingReferenceId = recordLocator
                }
            };
        }

        private ArrayOfIncludeExcludeOffersTypeServiceFilterCodeServiceFilterCode[] IncludeExcludeOffersBag()
        {
            return new[]
            {
                new ArrayOfIncludeExcludeOffersTypeServiceFilterCodeServiceFilterCode()
                {
                    ServiceCode = ServiceFilterGroupTypeServiceCode.BAG,
                    ResultAction = ServiceFilterGroupTypeResultAction.Include,
                    ServiceSubType = ServiceFilterGroupTypeServiceSubType.BAGPOLICYCALC,
                    ServiceSubTypeSpecified = true
                }
            };
        }


        private readonly List<string> Titles = new List<string>
        {
            DOTBaggageInfoDBTitle1,
            DOTBaggageInfoDBTitle1ELF,
            DOTBaggageInfoDBTitle2,
            DOTBaggageInfoDBTitle3,
            DOTBaggageInfoDBTitle4,
            DOTBaggageInfoDBTitle1IBE,
            DOTBaggageInfoDBTitle3IBE
        };


        private T ParseEnumFromStringToObject<T>(string enumName)
        {
            var typeInstance = Activator.CreateInstance(typeof(T));

            return (T)System.Enum.Parse(typeof(T), enumName, true);
        }

        private bool GetPayPalOTPEnableFlag(MOBFormofPayment fop)
        {
            bool isPayPalOTPPurchaseEnabled = (
                                                  (!Convert.ToBoolean(_configuration.GetValue<string>("DisablePayPalOTPPayment") ?? "false") &&    //if DisablePayPalOTPPayment is null or false
                                                  (fop == MOBFormofPayment.PayPal ||
                                                   fop == MOBFormofPayment.PayPalCredit ||
                                                   fop == MOBFormofPayment.Masterpass))                                                             //   And FOP is Paypal or Masterpass
                                                  ||                                                                                                //OR
                                                  (fop != MOBFormofPayment.PayPal &&
                                                  fop != MOBFormofPayment.PayPalCredit &&
                                                  fop != MOBFormofPayment.Masterpass)                                                               //   FOP is NOT Paypal & Masterpass
                                               );
            return isPayPalOTPPurchaseEnabled;
        }

        private double GetOTPPriceForApplication(int appID, string sessionId)
        {
            var catalogDynamoDB = new CatalogDynamoDB(_configuration, _dynamoDBService);
            double otpPrice = 59.0;
            try
            {
                var catalogItems = catalogDynamoDB.GetCatalogItems<MOBCatalogItem>("10010", sessionId).Result; // Get OTP Price of iOS app as the OTP price will be the same on all Apps
                if (catalogItems != null)
                {
                    double otpPrice1 = Convert.ToDouble(catalogItems.CurrentValue.Split('|')[0].ToString()); // 50|50
                    double otpPrice2 = Convert.ToDouble(catalogItems.CurrentValue.Split('|')[1].ToString()); // The price from Catalog will be like 50|50 we have to pick the least price for otp.
                    otpPrice = (otpPrice1 < otpPrice2) ? otpPrice1 : otpPrice2;
                    return otpPrice;
                }
                otpPrice = appID == 1 ? _configuration.GetValue<double>("iOSOTPPrice") : _configuration.GetValue<double>("AndroidOTPPrice");

            }
            catch (Exception ex)
            {
                string enterlog = string.IsNullOrEmpty(ex.StackTrace) ? ex.Message : ex.StackTrace;
                _logger.LogError("PurchaseOTPPasses-GetOTPPriceForApplication {@Exception}", enterlog);
            }
            return otpPrice;
        }

        private async Task<MOBClubDayPass> GetLoyaltyUnitedClubIssuePass(string mileagePlusNumber, string sessionId, int applicationId, string appVersion, string deviceId, string edocId, string firstName, string lastName, string ccName, string email, string onePassNumber, string location, string deviceType, string amountPaid, string recordLocator)
        {
            //Make the request
            MOBClubDayPass mobClubDayPass = new MOBClubDayPass();

            United.Service.Presentation.LoyaltyRequestModel.OneTimePassRequest oneTimePassRequest = new United.Service.Presentation.LoyaltyRequestModel.OneTimePassRequest();
            oneTimePassRequest.PassRequest = new United.Service.Presentation.LoyaltyRequestModel.OneTimePassProfile();

            oneTimePassRequest.PassRequest.AcquisitionDate = DateTime.Now; //AcquisitionDate
            oneTimePassRequest.PassRequest.AmountPaid = amountPaid;
            oneTimePassRequest.PassRequest.CardholderName = ccName;
            oneTimePassRequest.PassRequest.Carrier = "";
            oneTimePassRequest.PassRequest.Compensation = "";
            oneTimePassRequest.PassRequest.CustomerMP = ""; //Customer MP is populated if purchasing on behalf of that customer 
            oneTimePassRequest.PassRequest.DateAdmitted = "";
            oneTimePassRequest.PassRequest.DateCreated = DateTime.Now;
            oneTimePassRequest.PassRequest.DeviceType = deviceType;
            oneTimePassRequest.PassRequest.EdocId = edocId;
            oneTimePassRequest.PassRequest.EffectiveDate = DateTime.Now;
            oneTimePassRequest.PassRequest.EmailAddress = email;
            oneTimePassRequest.PassRequest.ExpirationDate = DateTime.Now.AddYears(1);
            oneTimePassRequest.PassRequest.FirstName = firstName;
            oneTimePassRequest.PassRequest.FlightNumber = "";
            oneTimePassRequest.PassRequest.IsAgent = false;
            oneTimePassRequest.PassRequest.IsSendEmail = true;
            oneTimePassRequest.PassRequest.IsTest = false;
            oneTimePassRequest.PassRequest.LastName = lastName;
            oneTimePassRequest.PassRequest.Location = "";
            oneTimePassRequest.PassRequest.MPAccount = mileagePlusNumber;
            oneTimePassRequest.PassRequest.Notes = "";


            oneTimePassRequest.PassRequest.PassReason = Service.Presentation.CommonEnumModel.Reason.Purchased;
            oneTimePassRequest.PassRequest.PassSource = Service.Presentation.CommonEnumModel.PassType.Mobile;

            oneTimePassRequest.PassRequest.ProgramId = 0;
            oneTimePassRequest.PassRequest.PromoCode = "";
            oneTimePassRequest.PassRequest.RecordLocator = recordLocator;
            oneTimePassRequest.PassRequest.RequestInfo = new Service.Presentation.LoyaltyModel.PassRequestInfo(); //right now passing null object as per OTP Service sample request

            oneTimePassRequest.PassRequest.Visitors = 1;
            oneTimePassRequest.PassRequest.WhoCreated = "";

            string jsonRequest = Newtonsoft.Json.JsonConvert.SerializeObject(oneTimePassRequest);

            string actionName = string.Format("IssuePass");
            string token = await _dPService.GetAnonymousToken(_headers.ContextValues.Application.Id, _headers.ContextValues.DeviceId, _configuration);
            string jsonResponse = await _passDetailService.GetLoyaltyUnitedClubIssuePass(token, actionName, jsonRequest, sessionId);

            if (!string.IsNullOrEmpty(jsonResponse))
            {
                var oneTimePassResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<United.Service.Presentation.LoyaltyResponseModel.OneTimePassResponse>(jsonResponse);
                if (oneTimePassResponse != null && oneTimePassResponse.UnitedClubPass != null) //(_response.ServiceStatus.StatusType == "Success") 
                {
                    //_response = oneTimePassResponse;
                    string passCode = string.Empty;
                    foreach (United.Service.Presentation.LoyaltyModel.UnitedClubPass unitedClubPass in oneTimePassResponse.UnitedClubPass)
                    {

                        mobClubDayPass.ClubPassCode = unitedClubPass.UnitedClubPassCode;
                        mobClubDayPass.ElectronicClubPassesType = MOBEnElectronicClubPassesType.PurchasedOTP;
                        mobClubDayPass.Email = unitedClubPass.EmailAddress;
                        mobClubDayPass.ExpirationDate = unitedClubPass.ExpirationDate.ToString("MMMM dd, yyyy");
                        mobClubDayPass.FirstName = unitedClubPass.FirstName;
                        mobClubDayPass.LastName = unitedClubPass.LastName;
                        mobClubDayPass.MileagePlusNumber = unitedClubPass.MPAccount;
                        mobClubDayPass.PassCode = unitedClubPass.UnitedClubPassCode;
                        string[] result = unitedClubPass.UnitedClubPassCode.Split('|');
                        mobClubDayPass.PassCode = result[1];
                        mobClubDayPass.PaymentAmount = unitedClubPass.AmountPaid;
                        mobClubDayPass.PurchaseDate = unitedClubPass.AcquiredDate.ToString("MMMM dd, yyyy");
                        mobClubDayPass.BarCode = GetBarCode(unitedClubPass.UnitedClubPassCode); //@TODO check the output

                        if (!_configuration.GetValue<bool>("disableSendingOnePassExpirationDateTime")) //Set disableSendingOnePassExpirationDateTime key in appsettings to turn off this feature
                            mobClubDayPass.ExpirationDateTime = unitedClubPass.ExpirationDate.Date.AddDays(1).AddSeconds(-1).ToString("MMMM dd, yyyy hh:mm:ss tt");

                        bool getClubPassFromLoyaltyOTP = _configuration.GetValue<string>("INSERTLoyaltyOTPServiceIssuePassToDBONOFF") != null ? Convert.ToBoolean(_configuration.GetValue<string>("INSERTLoyaltyOTPServiceIssuePassToDBONOFF").ToString()) : false;
                        if (getClubPassFromLoyaltyOTP)
                        {
                            await InsertUnitedClubPassToDB(mobClubDayPass.PassCode, onePassNumber, firstName, lastName, email, mobClubDayPass.ClubPassCode, mobClubDayPass.PaymentAmount, Convert.ToDateTime(mobClubDayPass.ExpirationDate), deviceType, false, recordLocator);
                        }
                    }
                }
            }

            return mobClubDayPass;
        }

        private async Task<bool> InsertUnitedClubPassToDB(string clubPassCode, string mpAccountNumber, string firstName, string lastName, string eMail, string barCodeString, double paymentAmount, DateTime expirationDate, string deviceType, bool isTest, string recordLocator)
        {
            try
            {
                InsertClubPassData data = new InsertClubPassData
                {
                    BarCodeString = barCodeString,
                    ClubPassCode = clubPassCode,
                    MpAccountNumber = mpAccountNumber,
                    FirstName = firstName,
                    LastName = lastName,
                    EMail = eMail,
                    PaymentAmount = paymentAmount,
                    ExpirationDate = expirationDate,
                    DeviceType = deviceType,
                    IsTest = isTest,
                    RecordLocator = recordLocator
                };

                InsertClubPassDynamoDB insertClubPassDynamoDB = new InsertClubPassDynamoDB(_configuration, _dynamoDBService);
                return await insertClubPassDynamoDB.InsertUnitedClubPassToDB(data, clubPassCode, _headers.ContextValues.SessionId);
            }
            catch { }

            return false;
        }

        private byte[] GetBarCode(string data)
        {
            string dataToEncode = data;
            bool applyTilde = true;
            bool truncate = true;
            int ModuleSize = 5;
            int QuietZone = 3;
            int TotalRows = 3;
            int TotalCols = 5;
            int ECLevel = 8;
            PDF417 obj = new PDF417();
            byte[] bmpstream = obj.EncodePDF417(dataToEncode, applyTilde, ECLevel, EncodingModes.Binary, TotalCols, TotalRows, truncate, QuietZone, ModuleSize);
            return bmpstream;
        }

        private List<MOBSHOPPrice> RemoveOTPFromReservationPricesList(MOBCheckOutResponse response)
        {
            List<MOBSHOPPrice> prices = null;
            if (response != null && response.Reservation != null && response.Reservation.Prices != null && response.Reservation.Prices.Count > 0)
            {
                var otps = response.Reservation.Prices.Find(item => item.DisplayType.ToUpper() == "ONE-TIME PASS");
                var grandTotal = response.Reservation.Prices.Find(item => item.DisplayType.ToUpper() == "GRAND TOTAL");
                if (grandTotal != null && otps != null)
                {
                    grandTotal.Value = grandTotal.Value - otps.Value;
                    grandTotal.DisplayValue = string.Format("{0:#,0.00}", grandTotal.Value);
                    grandTotal.FormattedDisplayValue = (grandTotal.Value).ToString("C2", CultureInfo.CurrentCulture);
                }
                response.Reservation.Prices.Remove(otps);
                prices = response.Reservation.Prices;
            }
            return prices;
        }

        private bool EnableEPlusAncillary(int appID, string appVersion, bool isReshop = false)
        {
            return _configuration.GetValue<bool>("EnableEPlusAncillaryChanges") && !isReshop && GeneralHelper.IsApplicationVersionGreaterorEqual(appID, appVersion, _configuration.GetValue<string>("EplusAncillaryAndroidversion"), _configuration.GetValue<string>("EplusAncillaryiOSversion"));
        }

        private List<string> GetDOTBagRules()
        {
            List<string> dotBagRules = null;

            string ruleText = HttpUtility.HtmlDecode(_configuration.GetValue<string>("DOTBagRules"));
            if (!string.IsNullOrEmpty(ruleText))
            {
                string[] rules = ruleText.Split('|');
                if (rules != null && rules.Length > 0)
                {
                    dotBagRules = new List<string>();
                    foreach (string s in rules)
                    {
                        dotBagRules.Add(s);
                    }
                }
            }

            return dotBagRules;
        }

        private async Task<List<MOBItem>> GetELFShopMessagesForRestrictions(MOBSHOPReservation reservation, int appId = 0)
        {
            if (reservation == null) return null;

            var databaseKey = string.Empty;

            if (reservation.IsELF)
            {
                databaseKey = reservation.IsSSA ?
                              "SSA_ELF_RESTRICTIONS_APPLY_MESSAGES" :
                              "ELF_RESTRICTIONS_APPLY_MESSAGES";
            }
            else if (reservation.ShopReservationInfo2 != null && reservation.ShopReservationInfo2.IsIBELite)
            {
                databaseKey = "IBELite_RESTRICTIONS_APPLY_MESSAGES";
            }
            else if (reservation.ShopReservationInfo2 != null && reservation.ShopReservationInfo2.IsIBE)
            {
                if (_configuration.GetValue<bool>("EnablePBE"))
                {
                    string productCode = reservation?.Trips[0]?.FlattenedFlights?[0]?.Flights?[0].ShoppingProducts.First(p => p != null && p.IsIBE).ProductCode;
                    databaseKey = productCode + "_RESTRICTIONS_APPLY_MESSAGES";
                }
                else
                {
                    databaseKey = "IBE_RESTRICTIONS_APPLY_MESSAGES";
                }
            }

            var messages = !string.IsNullOrEmpty(databaseKey) ?
                   await GetMPPINPWDTitleMessages(databaseKey) : null;

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

        private Reservation ReservationToPersistReservation(MOBSHOPAvailability availability)
        {
            var reservation = new Reservation
            {
                IsSSA = availability.Reservation.IsSSA,
                IsELF = availability.Reservation.IsELF,
                IsMetaSearch = availability.Reservation.IsMetaSearch,
                AwardTravel = availability.Reservation.AwardTravel,
                CartId = availability.Reservation.CartId,
                ClubPassPurchaseRequest = availability.Reservation.ClubPassPurchaseRequest,
                CreditCards = availability.Reservation.CreditCards,
                CreditCardsAddress = availability.Reservation.CreditCardsAddress,
                FareLock = availability.Reservation.FareLock,
                FareRules = availability.Reservation.FareRules,
                FlightShareMessage = availability.Reservation.FlightShareMessage,
                GetALLSavedTravelers = availability.Reservation.GetALLSavedTravelers,
                IneligibleToEarnCreditMessage = availability.Reservation.IneligibleToEarnCreditMessage,
                ISFlexibleSegmentExist = availability.Reservation.ISFlexibleSegmentExist,
                ISInternational = availability.Reservation.ISInternational,
                IsRefundable = availability.Reservation.IsRefundable,
                IsSignedInWithMP = availability.Reservation.IsSignedInWithMP,
                LMXFlights = availability.Reservation.LMXFlights,
                LMXTravelers = availability.Reservation.lmxtravelers,
                NumberOfTravelers = availability.Reservation.NumberOfTravelers,
                PKDispenserPublicKey = availability.Reservation.PKDispenserPublicKey,
                PointOfSale = availability.Reservation.PointOfSale,
                Prices = availability.Reservation.Prices,
                ReservationEmail = availability.Reservation.ReservationEmail,
                ReservationPhone = availability.Reservation.ReservationPhone,
                SearchType = availability.Reservation.SearchType,
                SeatPrices = availability.Reservation.SeatPrices,
                SessionId = availability.Reservation.SessionId,
                MetaSessionId = availability.Reservation.MetaSessionId,
                ELFMessagesForRTI = availability.Reservation.ELFMessagesForRTI,
                Taxes = availability.Reservation.Taxes,
                TCDAdvisoryMessages = availability.Reservation.TCDAdvisoryMessages,
                SeatAssignmentMessage = availability.Reservation.SeatAssignmentMessage,
                AlertMessages = availability.Reservation.AlertMessages,
                ShopReservationInfo = availability.Reservation.ShopReservationInfo,
                ShopReservationInfo2 = availability.Reservation.ShopReservationInfo2,
                CheckedbagChargebutton = availability.Reservation.CheckedbagChargebutton,
                IsBookingCommonFOPEnabled = availability.Reservation.IsBookingCommonFOPEnabled,
                IsReshopCommonFOPEnabled = availability.Reservation.IsReshopCommonFOPEnabled,
                IsCubaTravel = availability.Reservation.IsCubaTravel,
                CubaTravelInfo = availability.Reservation.CubaTravelInfo
            };

            if (availability.Reservation.Travelers != null && availability.Reservation.Travelers.Count > 0)
            {
                reservation.Travelers = new SerializableDictionary<string, MOBSHOPTraveler>();
                foreach (var traveler in availability.Reservation.Travelers)
                {
                    reservation.Travelers.Add(traveler.Key, traveler);
                }
            }
            if (availability.Reservation.TravelersCSL != null && availability.Reservation.TravelersCSL.Count > 0)
            {
                reservation.TravelersCSL = new SerializableDictionary<string, MOBCPTraveler>();
                foreach (var travelersCSL in availability.Reservation.TravelersCSL)
                {
                    reservation.TravelersCSL.Add(travelersCSL.Key, travelersCSL);
                }
            }
            reservation.TravelersRegistered = availability.Reservation.TravelersRegistered;
            reservation.TravelOptions = availability.Reservation.TravelOptions;
            reservation.Trips = availability.Reservation.Trips;
            reservation.UnregisterFareLock = availability.Reservation.UnregisterFareLock;
            if (!string.IsNullOrEmpty(availability.Reservation.RecordLocator))
            {
                if (availability.Reservation.TravelersCSL != null && availability.Reservation.TravelersCSL.Count > 0)
                {
                    reservation.TravelerKeys = new List<string>() { };
                    foreach (var travelersCSL in availability.Reservation.TravelersCSL)
                    {
                        reservation.TravelerKeys.Add(travelersCSL.Key);
                    }
                }
                reservation.IsRedirectToSecondaryPayment = availability.Reservation.IsRedirectToSecondaryPayment;
                reservation.RecordLocator = availability.Reservation.RecordLocator;
                reservation.Messages = availability.Reservation.Messages;
            }

            reservation.TripInsuranceFile = new MOBTripInsuranceFile() { TripInsuranceBookingInfo = availability.Reservation.TripInsuranceInfoBookingPath, TripInsuranceInfo = availability.Reservation.TripInsuranceInfo };
            return reservation;
        }

        private async Task AddPaymentNew(string transactionId, int applicationId, string applicationVersion, string paymentType, double amount, string currencyCode, int mileage, string remark, string insertBy, bool isTest, string sessionId, string deviceId, string recordLocator, string mileagePlusNumber, string formOfPayment, string restAPIVersion = "")
        {

            if (await _featureSettings.GetFeatureSettingValue("EnableTrackPartnerProvision_Cards").ConfigureAwait(false) && !string.IsNullOrEmpty(formOfPayment) && formOfPayment.ToUpper() == MOBFormofPayment.CreditCard.ToString().ToUpper())
            {
                PartnerProvisionDetails partnerProvisionDetails = new PartnerProvisionDetails();
                partnerProvisionDetails = await _sessionHelperService.GetSession<PartnerProvisionDetails>(sessionId, partnerProvisionDetails.ObjectName, new List<string> { sessionId, partnerProvisionDetails.ObjectName }).ConfigureAwait(false);
                if (partnerProvisionDetails != null && partnerProvisionDetails.IsItChaseProvisionCard)
                    formOfPayment = "ChaseProvisionCard";
            }

            if (_configuration.GetValue<string>("SendPayment") != null && _configuration.GetValue<string>("SendPayment").ToUpper().Trim() == "TRUE")
            {
                var seatMapDynamoDB = new SeatMapDynamoDB(_configuration, _dynamoDBService);
                var savePaymentNew = new PaymentDB()
                {
                    TransactionId = transactionId,
                    ApplicationId = applicationId,
                    ApplicationVersion = applicationVersion,
                    PaymentType = paymentType,
                    Amount = amount,
                    CurrencyCode = "USD",
                    Mileage = mileage,
                    Remark = remark,
                    InsertBy = insertBy,
                    IsTest = _configuration.GetValue<bool>("IsBookingTest") ? "Y" : "N",
                    SessionId = sessionId,
                    DeviceId = deviceId,
                    RecordLocator = recordLocator,
                    MileagePlusNumber = mileagePlusNumber,
                    FormOfPayment = !_configuration.GetValue<bool>("BlockDBInsertPaymentFOPPerameter") ? formOfPayment : string.Empty,// "CreditCard",
                    RestAPIVersion = (string.IsNullOrEmpty(restAPIVersion) ? (!string.IsNullOrEmpty(_configuration?.GetValue<string>("LogExceptionOnly")) ? _configuration?.GetValue<string>("RESTWEBAPIVersion").ToString() : null) : restAPIVersion)
                };

                if (_configuration.GetValue<bool>("EnableFeatureSettingsChanges") ? await _featureSettings.GetFeatureSettingValue("EnableMySqlPaymentTable").ConfigureAwait(false) : _configuration.GetValue<bool>("EnableMySqlPaymentTable"))
                {
                    await _auroraMySqlService.InsertpaymentRecord(savePaymentNew).ConfigureAwait(false);
                }
                else
                {
                    var key = transactionId + "::" + applicationId;
                    var returnValue = await _dynamoDBService.SaveRecords<PaymentDB>(_configuration?.GetValue<string>("DynamoDBTables:uatb-Payment"), "trans0", key, savePaymentNew, sessionId).ConfigureAwait(false);
                }

            }
        }

        private string GetRestAPIVersionBasedonFlowType(string flowType)
        {
            string flow = string.Empty;

            if (!string.IsNullOrEmpty(flowType))
            {
                switch (flowType.ToUpper())
                {
                    case "VIEWRES":
                    case "VIEWRES_SEATMAP":
                        flow = "ViewRES_CFOP";
                        break;
                }
            }
            return flow;
        }

        private async Task InsertCouponDetails(MOBShoppingCart shoppingCart, MOBRequest request, string sessionId, string xmlRemark, string recordLocator)
        {
            if (GetTotalDiscountPrice(shoppingCart) > 0)
            {
                await AddPaymentNew(sessionId, request.Application.Id, request.Application.Version.Major, shoppingCart.Flow + " " + string.Join(",", shoppingCart.Products.Select(x => x.Code).ToList()) + "-Promocode", GetTotalDiscountPrice(shoppingCart),
                           "USD",
                           0,
                           xmlRemark,
                           request.Application.Id.ToString(),
                           Convert.ToBoolean(_configuration.GetValue<string>("IsBookingTest")),
                           sessionId,
                           request.DeviceId,
                           recordLocator,
                           null,
                           "PromoCode",
                           GetRestAPIVersionBasedonFlowType(shoppingCart.Flow)).ConfigureAwait(false);
            }
        }

        private double GetTotalDiscountPrice(MOBShoppingCart shoppingCart)
        {
            Double totalDiscountPrice = 0;
            if (shoppingCart?.PromoCodeDetails?.PromoCodes != null
                && shoppingCart.PromoCodeDetails.PromoCodes.Count > 0
                && shoppingCart?.Products != null)
            {
                shoppingCart.Products.ForEach(product =>
                {
                    product.Segments.ForEach(segment =>
                    {
                        segment.SubSegmentDetails.ForEach(subSegment =>
                        {
                            totalDiscountPrice += subSegment.PromoDetails != null ? subSegment.PromoDetails.PromoValue : 0;
                        });
                    });
                });
            }
            return totalDiscountPrice;
        }

        private bool HandleAncillaryOptionsForEFS(MOBCheckOutRequest request, MOBCheckOutResponse response, FlightReservationResponse flightReservationResponse, MOBShoppingCart persistShoppingCart)
        {
            if (EnableEPlusAncillary(request.Application.Id, request.Application.Version.Major))
            {
                try
                {
                    if (request?.Flow?.Trim().ToUpper() == FlowType.BOOKING.ToString() && persistShoppingCart != null
                        && flightReservationResponse?.CheckoutResponse?.ShoppingCartResponse?.Items?.Any(x => x?.Item?.Product[0]?.Code?.Trim().ToUpper() == "EFS") == true
                        && flightReservationResponse.CheckoutResponse.ShoppingCartResponse.Items.Where(x => x.Item.Product[0].Code == "EFS").Select(x => x.Item).Any(y => y.Status.Contains("FAILED"))
                        && flightReservationResponse.Errors.Any(e => e.MinorCode == "90585"))
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("HandleAncillaryOptionsForEFS Error {message} {StackTrace} and {session}", ex.Message, ex.StackTrace, request.SessionId);

                }
            }
            return false;
        }


        private async Task HandleSeatAssignmentFailure(MOBShoppingCart persistShoppingCart, FlightReservationResponse flightReservationResponse, MOBCheckOutRequest request, MOBCheckOutResponse response, bool isEFSFail = false)
        {
            #region - Fix by Nizam for WI #2188
            if (!string.IsNullOrEmpty(_configuration.GetValue<string>("CFOPViewRes_AndroidVer_ThrowExceptionWhenSeatUnAssigned")) && request.Application.Id == 2 && request.Application.Version.Major == _configuration.GetValue<string>("CFOPViewRes_AndroidVer_ThrowExceptionWhenSeatUnAssigned").ToString().Trim()
               && request.PaymentAmount == null && persistShoppingCart.Products.Select(x => x.Code == "SEATASSIGNMENTS").FirstOrDefault())
                throw new MOBUnitedException(((int)MOBErrorCodes.ViewResCFOPSessionExpire).ToString(), _configuration.GetValue<string>("SeatsUnAssignedMessage").ToString());
            #endregion
            SeatChangeState state = new SeatChangeState();
            state = await _sessionHelperService.GetSession<SeatChangeState>(request.SessionId, state.ObjectName, new List<string> { request.SessionId, state.ObjectName }).ConfigureAwait(false);

            bool isSeatAssignmentFailedCSL30
                = _configuration.GetValue<bool>("EnableCSL30BookingReshopSelectSeatMap")
                        && (request.Flow == FlowType.BOOKING.ToString() || request.Flow == FlowType.RESHOP.ToString())
                        && flightReservationResponse.DisplayCart.DisplaySeats != null
                        && flightReservationResponse.DisplayCart.DisplaySeats.Any
                        (s => (!string.IsNullOrEmpty(s.SeatAssignMessage)
                        && !string.Equals(s.SeatAssignMessage, "SEATS ASSIGNED", StringComparison.OrdinalIgnoreCase)));
            //Eplus: Fixing issue as CSL is assigning seat even after seat purchase failure
            if (isSeatAssignmentFailedCSL30 == false && isEFSFail)
            {
                isSeatAssignmentFailedCSL30 = isEFSFail;
            }
            if ((flightReservationResponse.Errors.Any(x => x.MajorCode.Equals("30006.14")
            && x.MajorDescription.Equals("SeatAssignmentFailed . "))))
                response.SeatAssignMessages.Add(_configuration.GetValue<string>("SeatsUnAssignedMessage").ToString().Trim());
            else if (isSeatAssignmentFailedCSL30)
                response.SeatAssignMessages.Add(_configuration.GetValue<string>("SeatsUnAssignedMessage").ToString().Trim());

            if (persistShoppingCart.Products == null)
                persistShoppingCart.Products = new List<MOBProdDetail>();

            if ((flightReservationResponse.Errors.Any(x => x.MajorCode.Equals("30006.14")
            && x.MajorDescription.Equals("SeatAssignmentFailed . "))))
            {
                bool isSeatPurchaseFailed;
                bool isSeatAssignmentFailed;
                bool areAllSeatsPending;
                List<MOBItem> seatAssignmentMessage = new List<MOBItem>();
                List<MOBSeat> unassignedSeats = new List<MOBSeat>();

                IdentifySeatAssignmentAndPurchaseFail
                    (flightReservationResponse.CheckoutResponse.ShoppingCartResponse.Items.Where
                    (x => x.Item.Product[0].Code.Equals("SEATASSIGNMENTS")).FirstOrDefault(),
                    out isSeatAssignmentFailed, out isSeatPurchaseFailed, out areAllSeatsPending,
                    out unassignedSeats);

                seatAssignmentMessage = await SetSeatAssignmentMessage
                    (isSeatPurchaseFailed, isSeatAssignmentFailed, areAllSeatsPending);

                if (seatAssignmentMessage != null)
                {
                    MOBSection alertMsg = new MOBSection() { };
                    alertMsg = AssignAlertMessage(seatAssignmentMessage);
                    persistShoppingCart.AlertMessages.Add(alertMsg);
                }
                if (persistShoppingCart.AlertMessages.Count > 1)
                {
                    persistShoppingCart.AlertMessages[0].Text3 = string.Empty;
                }
            }
            else if (isSeatAssignmentFailedCSL30)
            {
                bool isSeatPurchaseFailed;
                bool isSeatAssignmentFailed;
                bool areAllSeatsPending;
                List<MOBItem> seatAssignmentMessage = new List<MOBItem>();
                List<MOBSeat> unassignedSeats = new List<MOBSeat>();

                IdentifySeatAssignmentAndPurchaseFail
                    (flightReservationResponse.CheckoutResponse.ShoppingCartResponse.Items.Where
                    (x => x.Item.Product[0].Code.Equals("SEATASSIGNMENTS")).FirstOrDefault(),
                    out isSeatAssignmentFailed, out isSeatPurchaseFailed, out areAllSeatsPending,
                    out unassignedSeats, isSeatAssignmentFailedCSL30, isEFSFail);

                bool isPromoCodeAppliedForSeats
                                 = _configuration.GetValue<bool>("EnableCouponsforBooking")
                                   && (request.Flow == FlowType.BOOKING.ToString() || request.Flow == FlowType.RESHOP.ToString())
                                   && flightReservationResponse.DisplayCart.DisplaySeats != null
                                   && flightReservationResponse.DisplayCart.DisplaySeats.Any
                                      (s => (!string.IsNullOrEmpty(s.SeatAssignMessage)
                                   && !string.Equals(s.SeatAssignMessage, "SEATS ASSIGNED", StringComparison.OrdinalIgnoreCase)) && !String.IsNullOrEmpty(s.PromotionalCouponCode));
                seatAssignmentMessage = await SetSeatAssignmentMessage
                    (isSeatPurchaseFailed, isSeatAssignmentFailed, areAllSeatsPending, isPromoCodeAppliedForSeats);

                if (seatAssignmentMessage != null)
                {
                    MOBSection alertMsg = new MOBSection() { };
                    alertMsg = AssignAlertMessage(seatAssignmentMessage);
                    persistShoppingCart.AlertMessages.Add(alertMsg);
                }

                if (persistShoppingCart.AlertMessages.Count > 1)
                {
                    persistShoppingCart.AlertMessages[0].Text3 = string.Empty;
                }
            }
            persistShoppingCart.Products = await _shoppingCartUtility.ConfirmationPageProductInfo(flightReservationResponse, true, false, request.Flow.ToString(), request.Application, state);
            persistShoppingCart.TotalPrice = String.Format("{0:0.00}", persistShoppingCart.Products.Select(x => Convert.ToDouble(x.ProdTotalPrice)).ToList().Sum());
            persistShoppingCart.DisplayTotalPrice = Decimal.Parse(persistShoppingCart.Products.Select(x => Convert.ToDouble(x.ProdTotalPrice)).ToList().Sum().ToString()).ToString("c");
            persistShoppingCart.TermsAndConditions = null;
            if (persistShoppingCart.FormofPaymentDetails == null)
                persistShoppingCart.FormofPaymentDetails = request.FormofPaymentDetails;

            persistShoppingCart.FormofPaymentDetails.EmailAddress = (!string.IsNullOrEmpty(request.FormofPaymentDetails.EmailAddress)) ? request.FormofPaymentDetails.EmailAddress.ToString() : (flightReservationResponse.Reservation.EmailAddress.Count() > 0) ? flightReservationResponse.Reservation.EmailAddress.Where(x => x.Address != null).Select(x => x.Address).FirstOrDefault().ToString() : null;

            await _sessionHelperService.SaveSession<MOBShoppingCart>(persistShoppingCart, request.SessionId, new List<string> { request.SessionId, persistShoppingCart.ObjectName }, persistShoppingCart.ObjectName).ConfigureAwait(false);

            response.ShoppingCart = persistShoppingCart;
            response.PostPurchasePage = PostPurchasePage.Confirmation.ToString();
            response.RecordLocator = flightReservationResponse.Reservation.ConfirmationID;
            response.PnrCreateDate = flightReservationResponse.Reservation.CreateDate;
            response.LastName = flightReservationResponse.Reservation.Travelers[0].Person.Surname;

        }

        private void IdentifySeatAssignmentAndPurchaseFail
           (ShoppingCartItemResponse item, out bool isSeatAssignmentFailed, out bool isSeatPurchaseFailed,
           out bool areAllSeatsPending, out List<MOBSeat> unassignedSeats, bool isSeatAssignmentFailedCSL30 = false, bool isEFSFail = false)
        {
            isSeatAssignmentFailed = false;
            isSeatPurchaseFailed = false;
            areAllSeatsPending = false;
            unassignedSeats = null;

            if (!item.IsNullOrEmpty() && !item.Error.IsNullOrEmpty() && !item.Error[0].Text.IsNullOrEmpty()
                && item.Error[0].Text.ToUpper().Contains("SEATASSIGNMENTFAILED"))
            {
                isSeatAssignmentFailed = true;
                if (!item.Item.IsNullOrEmpty())
                {
                    unassignedSeats = GetUnAssignedSeats(item.Item.Product.FirstOrDefault(), out areAllSeatsPending, false);

                    if (!unassignedSeats.IsNullOrEmpty())
                        isSeatPurchaseFailed = unassignedSeats.Any(s => !s.IsNullOrEmpty() && s.Price > 0);
                }
            }
            else if (isSeatAssignmentFailedCSL30)
            {
                if (!item.IsNullOrEmpty() && !item.Item.IsNullOrEmpty())
                {
                    isSeatAssignmentFailed = true;

                    unassignedSeats = GetUnAssignedSeats
                        (item.Item.Product.FirstOrDefault(), out areAllSeatsPending, isSeatAssignmentFailedCSL30);

                    if (!unassignedSeats.IsNullOrEmpty())
                    {
                        isSeatPurchaseFailed = unassignedSeats.Any(s => !s.IsNullOrEmpty() && s.Price > 0);
                    }
                }
            }

            if (isEFSFail)
            {
                isSeatPurchaseFailed = true;
            }

        }

        private List<MOBSeat> GetUnAssignedSeats(Service.Presentation.ProductModel.Product product, out bool areAllSeatsPending, bool isSeatAssignmentFailedCSL30)
        {
            areAllSeatsPending = false;
            if (product.IsNullOrEmpty() || product.Code.IsNullOrEmpty()
                || product.Status.IsNullOrEmpty() || product.Status.Description.IsNullOrEmpty()
                || !product.Code.ToUpper().Trim().Equals("SEATASSIGNMENTS"))
                return null;

            var assignedSeats = product.Status.Description.TrimStart().StartsWith("{")
                ? JsonConvert.DeserializeObject<AssignTravelerSeat>(product.Status.Description)
                : XmlSerializerHelper.Deserialize<AssignTravelerSeat>(product.Status.Description);

            if (isSeatAssignmentFailedCSL30)
            {
                if (assignedSeats.IsNullOrEmpty() || assignedSeats.Travelers.IsNullOrEmpty()) return null;

                var lstAssignMessage = assignedSeats.Travelers.Where(x => !x.Seats.IsNullOrEmpty()).Select(x => new { x.Seats.FirstOrDefault().AssignMessage }).ToList();
                var allPending = lstAssignMessage.FindAll(t => t.AssignMessage != null && t.AssignMessage.Trim().ToUpper().Contains("SEAT ASSIGNMENT PENDING"));
                areAllSeatsPending = !allPending.IsNullOrEmpty() && allPending.Count == lstAssignMessage.Count();
            }
            else
            {
                if (assignedSeats.IsNullOrEmpty() || assignedSeats.Travelers.IsNullOrEmpty() || assignedSeats.Error.IsNullOrEmpty()) return null;

                var errors = assignedSeats.Error.ToList();
                var allPendingErrors = errors.FindAll(t => t.Text != null && t.Text.Trim().ToUpper().Contains("SEAT ASSIGNMENT PENDING"));
                areAllSeatsPending = !allPendingErrors.IsNullOrEmpty() && allPendingErrors.Count == errors.Count;
            }

            if (areAllSeatsPending)
                return null;

            List<SeatDetail> unassignedSeats = null;
            if (isSeatAssignmentFailedCSL30)
            {
                unassignedSeats = assignedSeats.Travelers.Where(t => !t.Seats.IsNullOrEmpty())
                    .SelectMany(q => q.Seats.Where(s => !s.IsNullOrEmpty()
                                        && !string.Equals(s.AssignMessage, "SEATS ASSIGNED", StringComparison.OrdinalIgnoreCase)
                                        && !s.DepartureAirport.IsNullOrEmpty()
                                        && !s.DepartureAirport.IATACode.IsNullOrEmpty()
                                        && !s.ArrivalAirport.IsNullOrEmpty()
                                        && !s.ArrivalAirport.IATACode.IsNullOrEmpty()
                                        && !s.Seat.IsNullOrEmpty()
                                        && !s.Seat.Identifier.IsNullOrEmpty())).ToList();
            }
            else
            {
                unassignedSeats = assignedSeats.Travelers.Where(t => !t.Seats.IsNullOrEmpty())
                    .SelectMany(q => q.Seats.Where(s => !s.IsNullOrEmpty()
                                                        && !Convert.ToBoolean(s.Status)
                                                        && !s.DepartureAirport.IsNullOrEmpty()
                                                        && !s.DepartureAirport.IATACode.IsNullOrEmpty()
                                                        && !s.ArrivalAirport.IsNullOrEmpty()
                                                        && !s.ArrivalAirport.IATACode.IsNullOrEmpty()
                                                        && !s.Seat.IsNullOrEmpty()
                                                        && !s.Seat.Identifier.IsNullOrEmpty())).ToList();
            }


            List<MOBSeat> failedSeats = new List<MOBSeat>();
            foreach (var seatDetail in unassignedSeats)
            {
                MOBSeat seat = new MOBSeat();
                seat.Origin = seatDetail.DepartureAirport.IATACode;
                seat.Destination = seatDetail.ArrivalAirport.IATACode;
                seat.SeatAssignment = seatDetail.Seat.Identifier;
                if (!seatDetail.Seat.Price.IsNullOrEmpty() && !seatDetail.Seat.Price.Totals.IsNullOrEmpty())
                    seat.Price = Convert.ToDecimal(seatDetail.Seat.Price.Totals[0].Amount);

                if (ConfigUtility.IsMilesFOPEnabled())
                {
                    seat.OldSeatMiles = Convert.ToInt32(_configuration.GetValue<string>("milesFOP"));
                    seat.DisplayOldSeatMiles = UtilityHelper.FormatAwardAmountForDisplay(_configuration.GetValue<string>("milesFOP"), false);
                }
                failedSeats.Add(seat);
            }

            return failedSeats;
        }


        private async Task<MOBSHOPReservation> PopulateProducts(MOBSHOPReservation reservation, FlightReservationResponse response, string sessionId, int appID, string appVersion)
        {
            List<MOBCPTraveler> bookedTravelers = reservation.TravelersCSL;

            List<MOBSeat> unassignedSeats = new List<MOBSeat>();
            foreach (Service.Presentation.CustomerResponseModel.ShoppingCartItemResponse item in response.CheckoutResponse.ShoppingCartResponse.Items)
            {
                bool isEFSFailed = false;
                if (EnableEPlusAncillary(appID, appVersion, reservation.IsReshopChange)
                    && item.Item != null && item.Item.Product[0].Code.Equals("EFS"))
                {
                    if (item.ServiceStatus != null
                        && !string.IsNullOrEmpty(item.ServiceStatus.StatusType)
                        && item.ServiceStatus.StatusType.ToUpper().Trim() != "SUCCESS")
                    {
                        reservation.TravelOptions?.RemoveAll(t => t?.Key == "EFS");
                        // This should be changed when bundles are allowed with EFS in booking path.
                        reservation.Prices = UpdatePriceForUnRegisterEFS(reservation.Prices);
                        isEFSFailed = true;
                    }
                }
                if ((item.Error == null || item.Error != null && item.Error.Count == 0) && item.Item != null)
                {
                    if (item.Item.Product[0].Code.Equals("SEATASSIGNMENTS"))
                    {
                        if (item.Item.Product[0].Status != null)
                        {
                            if (item.Item.Product[0].Status.Description.TrimStart().StartsWith("{"))
                            {
                                United.Service.Presentation.FlightResponseModel.AssignTravelerSeat assignedSeats = JsonConvert.DeserializeObject<United.Service.Presentation.FlightResponseModel.AssignTravelerSeat>(item.Item.Product[0].Status.Description);
                                if (assignedSeats != null)
                                {
                                    PopulateSeats(ref bookedTravelers, assignedSeats);
                                }
                            }
                            else
                            {
                                United.Service.Presentation.FlightResponseModel.AssignTravelerSeat assignedSeats = XmlSerializerHelper.Deserialize<United.Service.Presentation.FlightResponseModel.AssignTravelerSeat>(item.Item.Product[0].Status.Description);
                                if (assignedSeats != null)
                                {
                                    PopulateSeats(ref bookedTravelers, assignedSeats);
                                }
                            }
                        }
                        reservation.TravelersCSL = bookedTravelers;
                    }

                    if (item.Item.Product[0].Code.Equals("PAS"))
                    {
                        if (item.Item.Payment != null && item.Item.Payment.Count > 0 && item.Item.Payment[0].CreditCard != null)
                        {
                            Reservation persistedReservation = new Reservation();
                            persistedReservation = await _sessionHelperService.GetSession<Reservation>(sessionId, persistedReservation.ObjectName, new List<string> { sessionId, persistedReservation.ObjectName }).ConfigureAwait(false);

                            reservation.TravelOptions = persistedReservation.TravelOptions;
                        }
                    }

                    if (item.Item.Product[0].Code.Equals("FLK") && item.Item.Status.ToUpper().Contains("ADD FOP SUCCESS"))
                    {
                        if (item.Item.Payment != null && item.Item.Payment.Count > 0 && item.Item.Payment[0].CreditCard != null)
                        {
                            for (int i = 0; i < reservation.Prices.Count; i++)
                            {
                                if (reservation.Prices[i].DisplayType.ToUpper() == "GRAND TOTAL")
                                {
                                    // AB-2213, AB-2214 Price displaying is inconsistent for Farelock booking (Grand total is displaying incorrect value)
                                    reservation.Prices[i].DisplayValue = string.Format("{0:#,0.00}", Convert.ToDouble(reservation.Prices[i].DisplayValue)); //+item.Item.Payment[0].CreditCard.Amount
                                    reservation.Prices[i].FormattedDisplayValue = Convert.ToDouble(reservation.Prices[i].DisplayValue).ToString("C2", CultureInfo.CurrentCulture);

                                    double tempDouble = 0;
                                    double.TryParse(reservation.Prices[i].DisplayValue.ToString(), out tempDouble);
                                    reservation.Prices[i].Value = Math.Round(tempDouble, 2, MidpointRounding.AwayFromZero);
                                }
                            }
                            MOBSHOPPrice travelOptionPrice = new MOBSHOPPrice();
                            travelOptionPrice.CurrencyCode = item.Item.Payment[0].CreditCard.Currency.Code;
                            travelOptionPrice.DisplayType = "FareLock";
                            travelOptionPrice.DisplayValue = string.Format("{0:#,0.00}", item.Item.Payment[0].CreditCard.Amount.ToString());
                            travelOptionPrice.FormattedDisplayValue = item.Item.Payment[0].CreditCard.Amount.ToString("C2", CultureInfo.CurrentCulture);
                            travelOptionPrice.PriceType = "FareLock";

                            double tempDouble1 = 0;
                            double.TryParse(travelOptionPrice.DisplayValue.ToString(), out tempDouble1);
                            travelOptionPrice.Value = Math.Round(tempDouble1, 2, MidpointRounding.AwayFromZero);

                            reservation.Prices.Add(travelOptionPrice);

                            reservation.TravelOptions = GetTravelOptions(response.DisplayCart, appID, appVersion, reservation.IsReshopChange);

                        }
                    }
                }
                else
                {
                    if (IsAnySeatChoosen(reservation))
                    {
                        bool isSeatPurchaseFailed;
                        bool isSeatAssignmentFailed;
                        bool areAllSeatsPending;
                        IdentifySeatAssignmentAndPurchaseFail(item, out isSeatAssignmentFailed, out isSeatPurchaseFailed, out areAllSeatsPending, out unassignedSeats);

                        if (isEFSFailed)
                        {
                            reservation.SeatAssignmentMessage = await SetSeatAssignmentMessage(true, isSeatAssignmentFailed, areAllSeatsPending);
                        }
                        else
                        {
                            reservation.SeatAssignmentMessage = await SetSeatAssignmentMessage(isSeatPurchaseFailed, isSeatAssignmentFailed, areAllSeatsPending);
                        }

                        if (reservation.SeatAssignmentMessage != null)
                        {
                            MOBSection alertMsg = new MOBSection() { };
                            alertMsg = AssignAlertMessage(reservation.SeatAssignmentMessage);
                            reservation.AlertMessages = new List<MOBSection>() { alertMsg };
                        }
                        reservation.SeatPrices = RevisedSeatPricesAfterAssignmentFailure(reservation.TravelersCSL, reservation.SeatPrices, unassignedSeats, isSeatAssignmentFailed);
                        reservation.Prices = UpdatePricesWithNewSeatPrices(reservation.Prices, reservation.SeatPrices, isSeatPurchaseFailed);
                    }
                }
                if (item.Item != null && item.Item.Product[0].Code.Equals("TPI"))
                {
                    Reservation persistedReservation = new Reservation();
                    persistedReservation = await _sessionHelperService.GetSession<Reservation>(sessionId, persistedReservation.ObjectName, new List<string> { sessionId, persistedReservation.ObjectName }).ConfigureAwait(false);
                    if (item.Item.Payment != null && item.Item.Payment.Count > 0 && item.Item.Payment[0].CreditCard != null && item.ServiceStatus != null && !string.IsNullOrEmpty(item.ServiceStatus.StatusType) && item.ServiceStatus.StatusType.ToUpper().Trim() == "SUCCESS")
                    {
                        if (persistedReservation != null && persistedReservation.TripInsuranceFile != null && persistedReservation.TripInsuranceFile.TripInsuranceBookingInfo != null)
                        {
                            reservation.TripInsuranceInfoBookingPath = persistedReservation.TripInsuranceFile.TripInsuranceBookingInfo;
                        }
                    }
                    else
                    {
                        reservation.TripInsuranceInfoBookingPath = new MOBTPIInfoInBookingPath() { };
                        reservation.Prices = UpdatePriceForUnRegisterTPI(reservation.Prices);
                    }

                    reservation.TripInsuranceInfoBookingPath.IsTPIIncludedInCart = true;
                }
            }
            return reservation;
        }

        private List<MOBSHOPPrice> UpdatePriceForUnRegisterEFS(List<MOBSHOPPrice> persistedPrices)
        {
            List<MOBSHOPPrice> prices = null;
            if (persistedPrices != null && persistedPrices.Count > 0)
            {
                double travelOptionSubTotal = 0.0;
                foreach (var price in persistedPrices)
                {
                    if (price.PriceType.ToUpper().Equals("TRAVEL OPTIONS"))
                    {
                        travelOptionSubTotal = travelOptionSubTotal + Convert.ToDouble(price.DisplayValue);
                    }
                    else if (!price.PriceType.ToUpper().Equals("GRAND TOTAL"))
                    {
                        if (prices == null)
                        {
                            prices = new List<MOBSHOPPrice>();
                        }
                        prices.Add(price);
                    }
                }

                foreach (var price in persistedPrices)
                {
                    if (price.PriceType.ToUpper().Equals("GRAND TOTAL"))
                    {
                        double grandTotal = Convert.ToDouble(price.DisplayValue);
                        price.DisplayValue = string.Format("{0:#,0.00}", grandTotal - travelOptionSubTotal);
                        price.FormattedDisplayValue = string.Format("${0:c}", price.DisplayValue);
                        double tempDouble1 = 0;
                        double.TryParse(price.DisplayValue.ToString(), out tempDouble1);
                        price.Value = Math.Round(tempDouble1, 2, MidpointRounding.AwayFromZero);
                        if (prices == null)
                        {
                            prices = new List<MOBSHOPPrice>();
                        }
                        prices.Add(price);
                    }
                }
            }

            return prices;
        }

        private void PopulateSeats(ref List<MOBCPTraveler> bookedTravelers, AssignTravelerSeat assignedSeats)
        {
            foreach (MOBCPTraveler t in bookedTravelers)
            {
                t.Seats = new List<MOBSeat>();
            }
            if (assignedSeats.Travelers != null && assignedSeats.Travelers.Count > 0)
            {
                foreach (Service.Presentation.ProductModel.ProductTraveler traveler in assignedSeats.Travelers)
                {
                    foreach (MOBCPTraveler t in bookedTravelers)
                    {
                        if (traveler.Seats != null && traveler.Seats.Count > 0 && traveler.GivenName.ToUpper() == t.FirstName.ToUpper() && traveler.Surname.ToUpper() == t.LastName.ToUpper())
                        {
                            foreach (Service.Presentation.SegmentModel.SeatDetail seat in traveler.Seats)
                            {
                                if (Convert.ToBoolean(seat.Status))
                                {
                                    MOBSeat assignedSeat = new MOBSeat();
                                    assignedSeat.SeatAssignment = seat.Seat.Identifier;
                                    assignedSeat.TravelerSharesIndex = traveler.TravelerNameIndex;
                                    assignedSeat.Origin = seat.DepartureAirport.IATACode;
                                    assignedSeat.Destination = seat.ArrivalAirport.IATACode;
                                    if (seat.Seat.Price != null)
                                    {
                                        assignedSeat.Price = Convert.ToDecimal(seat.Seat.Price.Totals[0].Amount);
                                        assignedSeat.Currency = seat.Seat.Price.Totals[0].Currency.Code;
                                        if (ConfigUtility.IsMilesFOPEnabled())
                                        {
                                            assignedSeat.OldSeatMiles = Convert.ToInt32(_configuration.GetValue<string>("milesFOP"));
                                            assignedSeat.DisplayOldSeatMiles = UtilityHelper.FormatAwardAmountForDisplay(_configuration.GetValue<string>("milesFOP"), false);
                                        }
                                    }

                                    assignedSeat.ProgramCode = seat.ProgramCode;
                                    assignedSeat.SeatType = seat.Seat.SeatType;

                                    t.Seats.Add(assignedSeat);
                                }
                            }
                        }
                    }
                }
            }
        }

        private List<MOBSHOPTravelOption> GetTravelOptions(DisplayCart displayCart, int appID, string appVersion, bool isReshop)
        {
            List<MOBSHOPTravelOption> travelOptions = null;
            if (displayCart != null && displayCart.TravelOptions != null && displayCart.TravelOptions.Count > 0)
            {
                CultureInfo ci = null;
                travelOptions = new List<MOBSHOPTravelOption>();
                bool addTripInsuranceInTravelOption =
                    !_configuration.GetValue<bool>("ShowTripInsuranceBookingSwitch")
                    && Convert.ToBoolean(_configuration.GetValue<string>("ShowTripInsuranceSwitch") ?? "false");
                foreach (var anOption in displayCart.TravelOptions)
                {
                    //wade - added check for farelock as we were bypassing it
                    if (!anOption.Type.Equals("Premium Access") && !anOption.Key.Trim().ToUpper().Contains("FARELOCK") && !anOption.Key.Trim().ToUpper().Contains("OTP") && !(addTripInsuranceInTravelOption && anOption.Key.Trim().ToUpper().Contains("TPI"))
                        && !(EnableEPlusAncillary(appID, appVersion, isReshop) && anOption.Key.Trim().ToUpper().Contains("EFS")))
                    {
                        continue;
                    }
                    if (ci == null)
                    {
                        ci = TopHelper.GetCultureInfo(anOption.Currency);
                    }

                    MOBSHOPTravelOption travelOption = new MOBSHOPTravelOption();

                    travelOption.Amount = (double)anOption.Amount;

                    travelOption.DisplayAmount = TopHelper.FormatAmountForDisplay(anOption.Amount, ci, false);

                    //??
                    if (anOption.Key.Trim().ToUpper().Contains("FARELOCK") || (addTripInsuranceInTravelOption && anOption.Key.Trim().ToUpper().Contains("TPI")))
                        travelOption.DisplayButtonAmount = TopHelper.FormatAmountForDisplay(anOption.Amount, ci, false);
                    else
                        travelOption.DisplayButtonAmount = TopHelper.FormatAmountForDisplay(anOption.Amount, ci, true);

                    travelOption.CurrencyCode = anOption.Currency;
                    travelOption.Deleted = anOption.Deleted;
                    travelOption.Description = anOption.Description;
                    travelOption.Key = anOption.Key;
                    travelOption.ProductId = anOption.ProductId;
                    travelOption.SubItems = GetTravelOptionSubItems(anOption.SubItems);
                    if (EnableEPlusAncillary(appID, appVersion, isReshop) && anOption.SubItems != null && anOption.SubItems.Count > 0)
                    {
                        travelOption.BundleCode = GetTravelOptionEplusAncillary(anOption.SubItems, travelOption.BundleCode);
                        GetTravelOptionAncillaryDescription(anOption.SubItems, travelOption, displayCart);
                    }
                    if (!string.IsNullOrEmpty(anOption.Type))
                    {
                        travelOption.Type = anOption.Type.Equals("Premium Access") ? "Premier Access" : anOption.Type;
                    }
                    travelOptions.Add(travelOption);
                }
            }

            return travelOptions;
        }

        private bool IsAnySeatChoosen(MOBSHOPReservation reservation)
        {
            if (_configuration.GetValue<bool>("DisableSSA_SeatAssignmentFailure"))
                return false;

            if (reservation.IsNullOrEmpty() || reservation.TravelersCSL.IsNullOrEmpty())
                return false;

            return reservation.TravelersCSL.Where(t => !t.Seats.IsNullOrEmpty())
                                            .SelectMany(t => t.Seats)
                                            .Any(s => !s.SeatAssignment.IsNullOrEmpty());
        }

        private List<MOBSHOPTravelOptionSubItem> GetTravelOptionSubItems(SubitemsCollection subitemsCollection)
        {

            List<MOBSHOPTravelOptionSubItem> subItems = null;

            if (subitemsCollection != null && subitemsCollection.Count > 0)
            {
                CultureInfo ci = null;
                subItems = new List<MOBSHOPTravelOptionSubItem>();

                foreach (var item in subitemsCollection)
                {
                    if (ci == null)
                    {
                        ci = TopHelper.GetCultureInfo(item.Currency);
                    }

                    MOBSHOPTravelOptionSubItem subItem = new MOBSHOPTravelOptionSubItem();
                    subItem.Amount = (double)item.Amount;
                    subItem.DisplayAmount = TopHelper.FormatAmountForDisplay(item.Amount, ci, false);
                    subItem.CurrencyCode = item.Currency;
                    subItem.Description = item.Description;
                    subItem.Key = item.Key;
                    subItem.ProductId = item.Type;
                    subItem.Value = item.Value;
                    subItems.Add(subItem);
                }

            }

            return subItems;
        }

        private List<MobShopBundleEplus> GetTravelOptionEplusAncillary(SubitemsCollection subitemsCollection, List<MobShopBundleEplus> bundlecode)
        {
            if (bundlecode == null || bundlecode.Count == 0)
            {
                bundlecode = new List<MobShopBundleEplus>();
            }

            foreach (var item in subitemsCollection)
            {
                if (item?.Type?.Trim().ToUpper() == "EFS")
                {
                    MobShopBundleEplus objeplus = new MobShopBundleEplus();
                    objeplus.ProductKey = item.Type;
                    objeplus.AssociatedTripIndex = Convert.ToInt32(item.TripIndex);
                    bundlecode.Add(objeplus);
                }
            }

            return bundlecode;
        }

        private void GetTravelOptionAncillaryDescription(SubitemsCollection subitemsCollection, MOBSHOPTravelOption travelOption, DisplayCart displayCart)
        {
            List<MOBAncillaryDescriptionItem> ancillaryDesciptionItems = new List<MOBAncillaryDescriptionItem>();
            CultureInfo ci = null;

            if (subitemsCollection.Any(t => t?.Type?.Trim().ToUpper() == "EFS"))
            {
                var trips = subitemsCollection.GroupBy(x => x.TripIndex);
                foreach (var trip in trips)
                {
                    if (trip != null)
                    {
                        decimal ancillaryAmount = 0;
                        foreach (var item in trip)
                        {
                            ancillaryAmount += item.Amount;
                            if (ci == null)
                            {
                                ci = TopHelper.GetCultureInfo(item.Currency);
                            }
                        }

                        MOBAncillaryDescriptionItem objeplus = new MOBAncillaryDescriptionItem();
                        objeplus.DisplayValue = TopHelper.FormatAmountForDisplay(ancillaryAmount, ci, false);
                        objeplus.SubTitle = displayCart.DisplayTravelers?.Count.ToString() + (displayCart.DisplayTravelers?.Count > 1 ? " travelers" : " traveler");
                        var displayTrip = displayCart.DisplayTrips?.FirstOrDefault(s => s.Index == Convert.ToInt32(trip.FirstOrDefault().TripIndex));
                        if (displayTrip != null)
                        {
                            objeplus.Title = displayTrip.Origin + " - " + displayTrip.Destination;
                        }
                        ancillaryDesciptionItems.Add(objeplus);
                    }
                }

                travelOption.BundleOfferTitle = "Economy Plus®";
                travelOption.BundleOfferSubtitle = "Included with your fare";
                travelOption.AncillaryDescriptionItems = ancillaryDesciptionItems;
            }
        }

        private async Task<List<MOBItem>> SetSeatAssignmentMessage(bool isSeatPurchaseFailed, bool isSeatAssignmentFailed, bool areAllSeatsPending, bool isPromoCodeAppliedForSeats = false)
        {
            if (!isSeatAssignmentFailed && !isSeatPurchaseFailed && !areAllSeatsPending) return null;
            string seatMessageKey = String.Empty;
            if (_configuration.GetValue<bool>("EnableCouponsforBooking"))
            {
                seatMessageKey = areAllSeatsPending ? "OA_SEAT_ASSIGNMENT_PENDING_MESSAGE" :
                         isSeatPurchaseFailed ? isPromoCodeAppliedForSeats ? "SEAT_PURCHASE_FAILED_MESSAGE_PromoCode" : "EPLUS_ANCILLARY_PURCHASE_FAIL"
                                              : "SSA_SEAT_ASSIGNMENT_FAILED_MESSAGE";
            }
            else
            {
                seatMessageKey = areAllSeatsPending ? "OA_SEAT_ASSIGNMENT_PENDING_MESSAGE" :
                        isSeatPurchaseFailed ? "SSA_SEAT_PURCHASE_FAILED_MESSAGE"
                                             : "SSA_SEAT_ASSIGNMENT_FAILED_MESSAGE";
            }


            return await GetMessagesFromDb(seatMessageKey);
        }

        private List<MOBSeatPrice> RevisedSeatPricesAfterAssignmentFailure(List<MOBCPTraveler> travelers, List<MOBSeatPrice> currentSeatPrices, List<MOBSeat> unassignedSeats, bool isSeatAssignmentFailed)
        {
            if (!isSeatAssignmentFailed || currentSeatPrices.IsNullOrEmpty() || unassignedSeats.IsNullOrEmpty())
                return currentSeatPrices;

            List<MOBSeat> seats = travelers.SelectMany(traveler => traveler.Seats).ToList();

            foreach (var unassignedSeat in unassignedSeats)
            {
                seats.RemoveWhere(s => s.Origin.Equals(unassignedSeat.Origin)
                                        && s.Destination.Equals(unassignedSeat.Destination)
                                        && s.SeatAssignment.Equals(unassignedSeat.SeatAssignment));
            }

            List<MOBSeatPrice> seatPrices = BuildSeatPricesForPriceBreakDown(seats);
            seatPrices = SortAndOrderSeatPrices(seatPrices);
            return seatPrices;
        }

        private List<MOBSeatPrice> SortAndOrderSeatPrices(List<MOBSeatPrice> seatPrices)
        {
            List<MOBSeatPrice> economySeatPrices, preferedSeatPrices = null;
            if (seatPrices.IsNullOrEmpty())
                return seatPrices;

            var preferedSeatTitle = _configuration.GetValue<string>("PreferedSeat_PriceBreakdownTitle") ?? "";
            var ci = TopHelper.GetCultureInfo(seatPrices[0].CurrencyCode == null ? seatPrices[0].CurrencyCode : "USD");
            if (_configuration.GetValue<bool>("EnableLimitedReclineAllProducts"))
            {
                economySeatPrices = seatPrices.FindAll(sp => sp.SeatMessage.ToUpper().Contains("ADVANCE SEAT ASSIGNMENT")).Clone();
                preferedSeatPrices = seatPrices.FindAll(sp => sp.SeatMessage.ToUpper().Contains(preferedSeatTitle.ToUpper())).Clone();
                seatPrices.RemoveAll(sp => sp.SeatMessage.ToUpper().Contains("ADVANCE SEAT ASSIGNMENT") || sp.SeatMessage.ToUpper().Contains(preferedSeatTitle.ToUpper()));
            }
            else
            {
                economySeatPrices = seatPrices.FindAll(sp => sp.SeatMessage.Equals("Advance Seat Assignment")).Clone();
                preferedSeatPrices = seatPrices.FindAll(sp => sp.SeatMessage.Equals(preferedSeatTitle, StringComparison.InvariantCultureIgnoreCase)).Clone();
                seatPrices.RemoveAll(sp => sp.SeatMessage.Equals("Advance Seat Assignment") || sp.SeatMessage.Equals(preferedSeatTitle, StringComparison.InvariantCultureIgnoreCase));
            }

            if (!seatPrices.IsNullOrEmpty() && !seatPrices[0].IsNullOrEmpty() && seatPrices[0].NumberOftravelers > 0)
            {
                decimal total = seatPrices.Aggregate<MOBSeatPrice, decimal>(0, (current, sp) => current + sp.DiscountedTotalPrice);
                int noOfTotalSeats = seatPrices.Aggregate(0, (current, sp) => current + sp.NumberOftravelers);
                seatPrices[0].SeatPricesHeaderandTotal = new MOBItem
                {
                    Id = noOfTotalSeats > 1 ? "Economy Plus® Seats" : "Economy Plus® Seat",
                    CurrentValue = TopHelper.FormatAmountForDisplay(total, ci, false)
                };
            }

            if (!preferedSeatPrices.IsNullOrEmpty() && !preferedSeatPrices[0].IsNullOrEmpty() && preferedSeatPrices[0].NumberOftravelers > 0)
            {
                decimal total = preferedSeatPrices.Aggregate<MOBSeatPrice, decimal>(0, (current, sp) => current + sp.DiscountedTotalPrice);
                int noOfTotalSeats = preferedSeatPrices.Aggregate(0, (current, sp) => current + sp.NumberOftravelers);
                preferedSeatPrices[0].SeatPricesHeaderandTotal = new MOBItem
                {
                    Id = noOfTotalSeats > 1 ? "Preferred seats" : "Preferred seat",
                    CurrentValue = TopHelper.FormatAmountForDisplay(total, ci, false)
                };
                seatPrices.AddRange(preferedSeatPrices);
            }

            if (!economySeatPrices.IsNullOrEmpty() && !economySeatPrices[0].IsNullOrEmpty() && economySeatPrices[0].NumberOftravelers > 0)
            {
                decimal total = economySeatPrices.Aggregate<MOBSeatPrice, decimal>(0, (current, sp) => current + sp.DiscountedTotalPrice);
                int noOfTotalSeats = economySeatPrices.Aggregate(0, (current, sp) => current + sp.NumberOftravelers);
                economySeatPrices[0].SeatPricesHeaderandTotal = new MOBItem
                {
                    Id = noOfTotalSeats > 1 ? "Advance Seat Assignments" : "Advance Seat Assignment",
                    CurrentValue = TopHelper.FormatAmountForDisplay(total, ci, false)
                };
                seatPrices.AddRange(economySeatPrices);
            }

            return seatPrices;
        }

        private List<MOBSeatPrice> BuildSeatPricesForPriceBreakDown(List<MOBSeat> seats)
        {
            if (seats.IsNullOrEmpty()) return null;

            seats.RemoveWhere(s => s.Price == 0 && s.PriceAfterTravelerCompanionRules == 0);
            if (seats.IsNullOrEmpty()) return null;

            seats = seats.GroupBy(s => s.Origin + s.Destination).SelectMany(s => s).ToList();
            List<MOBSeatPrice> seatPrices = new List<MOBSeatPrice>();
            var origin = "";
            var destination = "";
            var preferredSeat = _configuration.GetValue<string>("PreferedSeat_PriceBreakdownTitle") ?? string.Empty;
            var limitedReclineText = _configuration.GetValue<string>("NoOrLimitedReclineMessage").ToUpper() ?? string.Empty;
            foreach (var seat in seats)
            {
                if (seat.Origin == origin && seat.Destination == destination)
                {
                    MOBSeatPrice item = null;
                    if (_configuration.GetValue<bool>("EnableLimitedReclineAllProducts"))
                    {

                        item = seatPrices.Find(s => s.Origin == seat.Origin
                                                        && s.Destination == seat.Destination
                                                        && s.SeatMessage.ToUpper().Contains(limitedReclineText) == seat.LimitedRecline
                                                        && s.SeatMessage.ToUpper().Contains("ADVANCE SEAT ASSIGNMENT") == IsEMinusSeat(seat.ProgramCode)
                                                        && s.SeatMessage.ToUpper().Contains(preferredSeat.ToUpper()) == IsPreferredSeat(seat.SeatType, seat.ProgramCode));
                    }
                    else
                    {

                        item = seatPrices.Find(s => s.Origin == seat.Origin
                                                       && s.Destination == seat.Destination
                                                       && s.SeatMessage.ToUpper().Contains("LIMITED RECLINE") == seat.LimitedRecline
                                                       && s.SeatMessage.ToUpper().Contains("ADVANCE SEAT ASSIGNMENT") == IsEMinusSeat(seat.ProgramCode)
                                                       && s.SeatMessage.ToUpper().Contains(preferredSeat.ToUpper()) == IsPreferredSeat(seat.SeatType, seat.ProgramCode));
                    }

                    if (item != null)
                    {
                        UpdateSeatPrices(ref item, seat);
                    }
                    else
                    {
                        MOBSeatPrice sp = BuildSeatPrice(seat);
                        if (!sp.IsNullOrEmpty())
                            seatPrices.Add(sp);
                    }
                }
                else
                {
                    origin = seat.Origin;
                    destination = seat.Destination;
                    MOBSeatPrice sp = BuildSeatPrice(seat);
                    if (!sp.IsNullOrEmpty())
                        seatPrices.Add(sp);
                }
            }

            return seatPrices;
        }

        private bool IsEMinusSeat(string programCode)
        {
            if (!_configuration.GetValue<bool>("EnableSSA") || programCode.IsNullOrEmpty())
                return false;

            programCode = programCode.ToUpper().Trim();
            return programCode.Equals("ASA") || programCode.Equals("BSA");
        }

        private bool IsPreferredSeat(string DisplaySeatType, string program)
        {
            return IsPreferredSeatProgramCode(program) && IsPreferredSeatDisplayType(DisplaySeatType);
        }

        private bool IsPreferredSeatProgramCode(string program)
        {
            string preferredProgramCodes = _configuration.GetValue<string>("PreferredSeatProgramCodes");

            if (!string.IsNullOrEmpty(preferredProgramCodes) && !string.IsNullOrEmpty(program))
            {
                string[] codes = preferredProgramCodes.Split('|');

                foreach (string code in codes)
                {
                    if (code.Equals(program))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool IsPreferredSeatDisplayType(string displaySeatType)
        {
            string seatTypes = _configuration.GetValue<string>("PreferredSeatSharesSeatTypes") ?? string.Empty;
            var seatTypesList = seatTypes.Split('|');
            if (!string.IsNullOrEmpty(seatTypes) && !string.IsNullOrEmpty(displaySeatType))
            {
                if (seatTypesList.Any(s => s.Trim().Equals(displaySeatType, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }
            return false;
        }

        private void UpdateSeatPrices(ref MOBSeatPrice item, MOBSeat seat)
        {
            if (seat.IsNullOrEmpty())
                return;

            var ci = TopHelper.GetCultureInfo(item.CurrencyCode);

            item.NumberOftravelers = item.NumberOftravelers + 1;
            item.TotalPrice = item.TotalPrice + seat.Price;
            item.DiscountedTotalPrice = item.DiscountedTotalPrice + seat.PriceAfterTravelerCompanionRules;
            item.TotalPriceDisplayValue = TopHelper.FormatAmountForDisplay(item.TotalPrice, ci, false);
            item.DiscountedTotalPriceDisplayValue = TopHelper.FormatAmountForDisplay(item.DiscountedTotalPrice, ci, false);
            if (item.SeatNumbers.IsNullOrEmpty())
                item.SeatNumbers = new List<string>();
            if (!seat.SeatAssignment.IsNullOrEmpty())
                item.SeatNumbers.Add(seat.SeatAssignment);
        }

        public MOBSeatPrice BuildSeatPrice(MOBSeat seat)
        {
            if (seat.IsNullOrEmpty())
                return null;
            var ci = TopHelper.GetCultureInfo(seat.Currency);
            var seatMessage = GetSeatMessage(seat.SeatType, seat.SeatFeature, seat.ProgramCode, seat.LimitedRecline);
            return new MOBSeatPrice
            {
                Origin = seat.Origin,
                Destination = seat.Destination,
                SeatMessage = seatMessage,
                NumberOftravelers = 1,
                TotalPrice = seat.Price,
                TotalPriceDisplayValue = TopHelper.FormatAmountForDisplay(seat.Price, ci, false),
                DiscountedTotalPrice = seat.PriceAfterTravelerCompanionRules,
                DiscountedTotalPriceDisplayValue = TopHelper.FormatAmountForDisplay(seat.PriceAfterTravelerCompanionRules, ci, false),
                CurrencyCode = seat.Currency,
                SeatNumbers = new List<string> { seat.SeatAssignment }
            };
        }

        private string GetSeatMessage(string seatType, string seatFeature, string programCode, bool limitedRecline)
        {
            if (_configuration.GetValue<bool>("EnableLimitedReclineAllProducts"))
                return GetSeatMessageWithLimitRecline(seatType, seatFeature, programCode, limitedRecline);

            if (IsEMinusSeat(programCode))
                return "Advance Seat Assignment";

            if (_configuration.GetValue<bool>("isEnablePreferredZone") && (IsPreferredSeat(seatType, programCode) || IsPreferredSeat(seatFeature, programCode)))
                return _configuration.GetValue<string>("PreferedSeat_PriceBreakdownTitle");

            return limitedRecline ? "Economy Plus Seat (limited recline)"
                                  : "Economy Plus Seat";
        }

        private string GetSeatMessageWithLimitRecline(string seatType, string seatFeature, string programCode, bool limitedRecline)
        {
            string seatMsg = "Economy Plus Seat";

            if (IsEMinusSeat(programCode))
                seatMsg = "Advance Seat Assignment";

            if (_configuration.GetValue<bool>("isEnablePreferredZone") && (IsPreferredSeat(seatType, programCode) || IsPreferredSeat(seatFeature, programCode)))
                seatMsg = _configuration.GetValue<string>("PreferedSeat_PriceBreakdownTitle");

            if (limitedRecline)
                seatMsg = seatMsg + _configuration.GetValue<bool>("NoOrLimitedReclineMessage");
            return seatMsg;
        }

        private List<MOBSHOPPrice> UpdatePricesWithNewSeatPrices(List<MOBSHOPPrice> prices, List<MOBSeatPrice> seatPrices, bool isSeatPurchaseFailed)
        {
            if (!isSeatPurchaseFailed || prices.IsNullOrEmpty()) return prices;

            if (seatPrices == null) seatPrices = new List<MOBSeatPrice>();

            int countNoOfEPlusSeatPriceRows = 0;
            int countNoOfEminusPurchased = 0;
            int countNoOfEplusPurchased = 0;
            int countNoOfPreferredSeatPurchased = 0;

            double totalEplusSeatPrice = 0;
            double totalEminusSeatPrice = 0;
            double totalPreferredSeatPrice = 0;

            double currentEminusSeatPrice = 0;
            double currentEplusSeatPrice = 0;
            double currentPreferredSeatPrice = 0;
            bool showLimitedRecline = false;

            var preferredSeat = _configuration.GetValue<string>("PreferedSeat_PriceBreakdownTitle") ?? string.Empty;

            foreach (var seatPrice in seatPrices)
            {
                if (seatPrice.SeatMessage.ToUpper().Contains("ECONOMY PLUS"))
                {
                    if (!seatPrice.SeatNumbers.IsNullOrEmpty())
                        countNoOfEplusPurchased = countNoOfEplusPurchased + seatPrice.SeatNumbers.Count;
                    countNoOfEPlusSeatPriceRows = countNoOfEPlusSeatPriceRows + 1;
                    if (countNoOfEPlusSeatPriceRows == 1)
                        showLimitedRecline = seatPrice.SeatMessage.ToUpper().Contains("LIMITED RECLINE");
                    totalEplusSeatPrice += Convert.ToDouble(seatPrice.DiscountedTotalPrice);
                }

                if (seatPrice.SeatMessage.ToUpper().Contains(preferredSeat.ToUpper()))
                {
                    if (!seatPrice.SeatNumbers.IsNullOrEmpty())
                        countNoOfPreferredSeatPurchased = countNoOfPreferredSeatPurchased + seatPrice.SeatNumbers.Count;
                    totalPreferredSeatPrice += Convert.ToDouble(seatPrice.DiscountedTotalPrice);
                }

                if (seatPrice.SeatMessage.ToUpper().Contains("ADVANCE SEAT ASSIGNMENT"))
                {
                    if (!seatPrice.SeatNumbers.IsNullOrEmpty())
                        countNoOfEminusPurchased = countNoOfEminusPurchased + seatPrice.SeatNumbers.Count;
                    totalEminusSeatPrice += Convert.ToDouble(seatPrice.DiscountedTotalPrice);
                }
            }

            //Get and Update Price Items
            var eplusPrice = prices.FirstOrDefault(p => !p.DisplayType.IsNullOrEmpty() && p.DisplayType.ToUpper() == "ECONOMYPLUS SEATS");
            if (eplusPrice != null)
            {
                currentEplusSeatPrice = eplusPrice.Value;
                if (totalEplusSeatPrice > 0)
                {
                    UpdatePriceItem(eplusPrice, totalEplusSeatPrice, "ECONOMYPLUS SEATS");
                    eplusPrice.PriceTypeDescription = showLimitedRecline ? (countNoOfEplusPurchased > 1 ? "Economy Plus Seats (limited recline)" : "Economy Plus Seat (limited recline)")
                                                                         : (countNoOfEplusPurchased > 1 ? "Economy Plus Seats" : "Economy Plus Seat");
                }
                else
                {
                    prices.Remove(eplusPrice);
                }
            }

            var preferredSeatPrice = prices.FirstOrDefault(p => !p.DisplayType.IsNullOrEmpty() && p.DisplayType.ToUpper() == preferredSeat.ToUpper());
            if (preferredSeatPrice != null)
            {
                currentPreferredSeatPrice = preferredSeatPrice.Value;

                if (totalPreferredSeatPrice > 0)
                {
                    UpdatePriceItem(preferredSeatPrice, totalPreferredSeatPrice, preferredSeat.ToUpper());
                    preferredSeatPrice.PriceTypeDescription = countNoOfPreferredSeatPurchased > 1 ? "Preferred seats" : "Preferred seat";
                }
                else
                {
                    prices.Remove(preferredSeatPrice);
                }
            }

            var economyPrice = prices.FirstOrDefault(p => !p.DisplayType.IsNullOrEmpty() && p.DisplayType.ToUpper() == "ADVANCE SEAT ASSIGNMENT");
            if (economyPrice != null)
            {
                currentEminusSeatPrice = economyPrice.Value;

                if (totalEminusSeatPrice > 0)
                {
                    UpdatePriceItem(economyPrice, totalEminusSeatPrice, "ADVANCE SEAT ASSIGNMENT");
                    economyPrice.PriceTypeDescription = countNoOfEminusPurchased > 1 ? "Advance Seat Assignments"
                                                                                     : "Advance Seat Assignment";
                }
                else
                {
                    prices.Remove(economyPrice);
                }
            }

            var grandTotal = prices.FirstOrDefault(p => !p.DisplayType.IsNullOrEmpty() && p.DisplayType.ToUpper() == "GRAND TOTAL");
            if (grandTotal != null)
            {
                var actualGrandtotal = grandTotal.Value
                                       - (currentEplusSeatPrice + currentPreferredSeatPrice + currentEminusSeatPrice)
                                       + (totalEplusSeatPrice + totalPreferredSeatPrice + totalEminusSeatPrice);

                if (grandTotal.Value > actualGrandtotal)
                    UpdatePriceItem(grandTotal, actualGrandtotal, "GRAND TOTAL");
            }

            return prices;
        }

        private void UpdatePriceItem(MOBSHOPPrice mobshopPrice, double updatedPrice, string displayPrice)
        {
            if (mobshopPrice.DisplayType.Trim().ToUpper().Equals(displayPrice))
            {
                mobshopPrice.Value = updatedPrice;
                mobshopPrice.DisplayValue = string.Format("{0:#,0.00}", mobshopPrice.Value);
                mobshopPrice.FormattedDisplayValue = mobshopPrice.Value.ToString("C2", CultureInfo.CurrentCulture);
            }
        }

        private async Task FireToGetPremiumCabinUpgrade(MOBRequest mobRequest, MOBSHOPReservation reservation, Session session)
        {
            await Task.Factory.StartNew(async () =>
            {
                if ((_configuration.GetValue<bool>("EnablePCUatReshop") || !reservation.IsReshopChange) && !reservation.IsELF && !reservation.ShopReservationInfo2.IsIBELite && !reservation.ShopReservationInfo2.IsIBE && !TravelOptionsContainsFareLock(reservation.TravelOptions))
                {
                    await GetPremiumCabinUpgrade(reservation.RecordLocator, reservation.SessionId, reservation.CartId, mobRequest, null, session?.CatalogItems);
                }
            });
        }

        public async Task<MOBPremiumCabinUpgrade> GetPremiumCabinUpgrade(string recordLocator, string sessionId, string cartId, MOBRequest mobRequest, United.Service.Presentation.ReservationModel.Reservation cslReservation, List<United.Mobile.Model.Common.MOBItem> catalogItems = null)
        {
            var isBookingPath = !string.IsNullOrEmpty(cartId);
            var logAction = isBookingPath ? "PCUPostBookingOffer" : "PremiumCabinUpgrade Offer";
            try
            {
                if (!EnablePCU(mobRequest.Application.Id, mobRequest.Application.Version.Major, isBookingPath))
                    return null;

                var premiumCabinUpgrade = new MSCPremiumCabinUpgrade(_configuration, _legalDocumentsForTitlesService, _mSCPkDispenserPublicKey, _sessionHelperService, _flightShoppingProductsService);
                premiumCabinUpgrade.Initialization(recordLocator, sessionId, cartId, mobRequest, cslReservation);

                await (await (await (await ((await (await (premiumCabinUpgrade.BuildOfferRequest()
                                 .GetOffer())).ValidateOfferResponse())
                                 .BuildPremiumCabinUpgrade()))
                                 .GetTokenFromSession(sessionId))
                                 .GetpkDispenserPublicKey(catalogItems))
                                 .SaveState();


                return premiumCabinUpgrade.CabinUpgradeOffer;
            }
            catch (MOBUnitedException uaex)
            {
                var uaexWrapper = new MOBExceptionWrapper(uaex);
                _logger.LogInformation("GetPremiumCabinUpgrade - UnitedException {uaexWrapper} with {sessionId}", uaexWrapper, sessionId);

            }
            catch (Exception ex)
            {
                var exceptionWrapper = new MOBExceptionWrapper(ex);
                _logger.LogError("GetPremiumCabinUpgrade Exception {message} {StackTrace} and {session}", ex.Message, ex.StackTrace, sessionId);

            }

            return null;
        }

        private bool EnablePCU(int appId, string appVersion, bool isBookingPath)
        {
            return _configuration.GetValue<bool>("EnablePCU") &&
                   isBookingPath ? GeneralHelper.isApplicationVersionGreater(appId, appVersion, "AndroidPCUInBookingVersion", "iPhonePCUInBookingVersion", "", "", true, _configuration)
                                 : GeneralHelper.isApplicationVersionGreater(appId, appVersion, "AndroidPCUVersion", "iPhonePCUVersion", "", "", true, _configuration);
        }

        private void AddFareLockEmail(string farelockoffertype, string emaiaddr, string pnr, string pnrcreatedatetime)
        {
            //todo-OnPrem db service implemented, need to incorporate in onCloud code
            try
            {
                Microsoft.Practices.EnterpriseLibrary.Data.Database database = DatabaseFactory.CreateDatabase("ConnectionString - Fullfillment");
                DbCommand dbCommand = (DbCommand)database.GetStoredProcCommand("up_FareLockEmail_ins");
                database.AddInParameter(dbCommand, "@EmailType", DbType.String, "CONFIRM");
                database.AddInParameter(dbCommand, "@FareLockOfferType", DbType.String, farelockoffertype);
                database.AddInParameter(dbCommand, "@EmailAddress", DbType.String, emaiaddr);
                database.AddInParameter(dbCommand, "@ConfNum", DbType.String, pnr);
                database.AddInParameter(dbCommand, "@ConfNumCreateDateTime", DbType.DateTime, Convert.ToDateTime(pnrcreatedatetime));

                database.ExecuteNonQuery(dbCommand);
            }
            catch { }
        }

        private bool TravelOptionsContainsFareLock(List<MOBSHOPTravelOption> options)
        {
            bool containsFareLock = false;

            if (options != null && options.Count > 0)
            {
                foreach (MOBSHOPTravelOption option in options)
                {
                    if (option != null && !string.IsNullOrEmpty(option.Key) && option.Key.ToUpper() == "FARELOCK")
                    {
                        containsFareLock = true;
                        break;
                    }
                }
            }

            return containsFareLock;
        }

        private bool IsShowPBSOffer(bool isReshop)
        {
            if (!_configuration.GetValue<bool>("DontDisplayPBSOfferinReshopPostBooking"))
            {
                return !isReshop;
            }
            return true;
        }

        private async Task<MOBPriorityBoarding> AssignPBToReservation(MOBCheckOutRequest checkoutRequest, string token, string cartId, string langCode, string countryCode)
        {
            if (!GeneralHelper.FeatureVersionCheck(checkoutRequest.Application.Id, checkoutRequest.Application.Version.Major, "EnablePBSInPostBookingPath", "AndroidPBSPostBookingVersion", "iPhonePBSPostBookingVersion", _configuration))
                return null;

            MOBPriorityBoarding priorityBoarding = null;
            try
            {
                if (_configuration.GetValue<bool>("IsPBSLazyLoadingEnabled"))
                {
                    //fire and forget
                    var task = GetPBDetailInBookingFlow(checkoutRequest.LanguageCode, cartId, checkoutRequest.SessionId, token, checkoutRequest.DeviceId, checkoutRequest.Application, langCode, countryCode);
                }
                else
                {
                    priorityBoarding = await GetPBDetailInBookingFlow(checkoutRequest.LanguageCode, cartId, checkoutRequest.SessionId, token, checkoutRequest.DeviceId, checkoutRequest.Application, langCode, countryCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("AssignPBToReservation Error {message} {StackTrace} and {session}", ex.Message, ex.StackTrace, checkoutRequest.SessionId);
            }

            return priorityBoarding;
        }

        private async Task<MOBPriorityBoarding> GetPBDetailInBookingFlow(string languageCode, string cartId, string sessionId, string token, string deviceId, MOBApplication application, string langCode, string countryCode)
        {
            var priorityBoarding = new MOBPriorityBoarding();
            string logAction = "GetPBInfoInPostBookingFlow";
            try
            {
                DisplayCartRequest getPBOfferRequest = new DisplayCartRequest();
                getPBOfferRequest.LangCode = languageCode;
                getPBOfferRequest.CartId = cartId;
                getPBOfferRequest.CountryCode = "US";
                getPBOfferRequest.CartKey = "PBS";
                getPBOfferRequest.ProductCodes = new List<string>() { "PBS" };
                string jsonRequest = JsonConvert.SerializeObject(getPBOfferRequest);
                string url = string.Format("/GetProducts", _configuration.GetValue<string>("ServiceEndPointBaseUrl - CSLShopping"));
                var jsonResponse = await MakeHTTPCallAndLogIt(sessionId,
                                        deviceId,
                                        logAction,
                                        application,
                                        token,
                                        url,
                                        jsonRequest,
                                        false,
                                        false);
                var offerResponse = JsonConvert.DeserializeObject<DisplayCartResponse>(jsonResponse);
                var productOffer = new GetOffers();
                if (offerResponse?.MerchProductOffer != null)
                {
                    productOffer = ObjectToObjectCasting<GetOffers, United.Service.Presentation.ProductResponseModel.ProductOffer>(offerResponse.MerchProductOffer);
                    await PersistAncillaryProducts(sessionId, productOffer);
                    int custCount = 0;
                    priorityBoarding = BuildPBDetailBaseOnSegment(offerResponse.MerchProductOffer, ref custCount, true);
                    var pBFile = new PriorityBoardingFile();
                    pBFile.NumberOfTraveler = custCount;
                    pBFile.PriorityBoarding = priorityBoarding;
                    pBFile.LangCode = langCode;
                    pBFile.CartID = cartId;
                    pBFile.CountryCode = countryCode;

                    await _sessionHelperService.SaveSession(pBFile, sessionId, new List<string> { sessionId, pBFile.ObjectName }, pBFile.ObjectName).ConfigureAwait(false);

                }
                else
                {
                    string errorMessage = string.Empty;
                    if (offerResponse?.Errors != null && offerResponse?.Errors.Count > 0)
                    {
                        foreach (var error in offerResponse.Errors)
                        {
                            errorMessage = errorMessage + " " + error?.Message;
                        }
                    }
                    throw new MOBUnitedException(errorMessage);
                }
            }
            catch (MOBUnitedException uaex)
            {
                MOBExceptionWrapper uaexWrapper = new MOBExceptionWrapper(uaex);
                _logger.LogWarning("GetPBDetailInBookingFlow - UnitedException {uaexWrapper} with {sessionId}", uaexWrapper, sessionId);
                priorityBoarding = null;
            }
            catch (Exception Ex)
            {
                _logger.LogError("GetPBDetailInBookingFlow Exception {message} {StackTrace} and {session}", Ex.Message, Ex.StackTrace, sessionId);
                priorityBoarding = null;
            }

            return priorityBoarding != null && priorityBoarding.PbOfferTileInfo != null ? priorityBoarding : null;
        }

        private MOBPriorityBoarding BuildPBDetailBaseOnSegment(Service.Presentation.ProductResponseModel.ProductOffer offerResponse, ref int custCount, bool isPostBooking)
        {
            var priorityBoarding = new MOBPriorityBoarding();
            double lowestPrice = 0;
            priorityBoarding.Segments = BuildPbOfferBaseOnSegment(offerResponse, ref lowestPrice, isPostBooking);

            if (priorityBoarding.Segments.IsNullOrEmpty())
                return null;

            BuildPBPurchasedTextBaseOnSegment(priorityBoarding);

            // client need segment and customer info for building purchase confirmation message. When lowest price is Zero, send segments only.
            if (lowestPrice == 0)
            {
                priorityBoarding.PbOfferTileInfo = null;
                priorityBoarding.PbOfferDetails = null;
                priorityBoarding.TAndC = null;
                return priorityBoarding;
            }

            priorityBoarding.PbOfferTileInfo = new MOBOfferTile()
            {
                CurrencyCode = "$",
                Price = Convert.ToDecimal(lowestPrice),
                Text1 = "Travel made easier with",
                Text2 = "From",
                Text3 = "per person",
                Title = "Priority Boarding"
            };
            priorityBoarding.TAndC = _shoppingCartUtility.GetPBContentList("PriorityBoardingTermsAndConditionsList");
            priorityBoarding.PbOfferDetails = _shoppingCartUtility.GetPBContentList("PriorityBoardingOfferDetailsList");
            priorityBoarding.ProductCode = (offerResponse.Offers[0] != null) ? offerResponse.Offers[0].ProductInformation.ProductDetails[0].Product.Code : null;
            priorityBoarding.ProductName = (offerResponse.Offers[0] != null) ? offerResponse.Offers[0].ProductInformation.ProductDetails[0].Product.Description : null;
            if (!offerResponse.IsNullOrEmpty() && offerResponse.Travelers.IsNotNullNorEmpty() && offerResponse.Travelers.Any())
            {
                custCount = offerResponse.Travelers.Count();
            }
            return priorityBoarding;
        }

        private List<MOBPBSegment> BuildPbOfferBaseOnSegment(Service.Presentation.ProductResponseModel.ProductOffer offerResponse, ref double lowestPrice, bool isPostBooking)
        {
            List<MOBPBSegment> segments = new List<MOBPBSegment>();
            var productCode = "PBS";
            var productDetail = GetProductDetails(offerResponse, productCode);
            if (productDetail == null) return null;
            if (isPostBooking && (offerResponse.FlightSegments == null || !offerResponse.FlightSegments.Any()))
                return null;
            // all segments with basic info as for date, origination and destinaton 
            segments = isPostBooking
                ? BuildSegmentsBasicInfoForPostBookingPath(offerResponse.FlightSegments)
                : BuildSegmentBasicInfoBaseOnSegment(offerResponse.Solutions[0].ODOptions);
            if (segments.IsNullOrEmpty())
                return null;

            segments = GetAllPricesInfoBaseOnSegment(productDetail, segments, ref lowestPrice);

            if (segments.IsNullOrEmpty())
                return null;
            return segments;
        }

        private List<MOBPBSegment> GetAllPricesInfoBaseOnSegment(Service.Presentation.ProductResponseModel.ProductDetail produtDetail, List<MOBPBSegment> segments, ref double lowestPrice)
        {
            if (produtDetail.IsNullOrEmpty())
                return null;

            if (produtDetail.InEligibleSegments.IsNotNullNorEmpty())
            {
                foreach (var ineligibleSegment in produtDetail.InEligibleSegments.Where(i => !i.Assocatiation.IsNullOrEmpty() && i.Assocatiation.SegmentRefIDs.IsNotNullNorEmpty()))
                {
                    foreach (var segmentID in ineligibleSegment.Assocatiation.SegmentRefIDs)
                    {
                        // if we found out PA is purchased or PA is offered, then we jump out from the loop. 
                        segments = BuildSegmentDetailInfoWithNoPBOfferedBaseOnSegment(segments, ineligibleSegment.Reason, segmentID);
                        if (segments == null)
                        {
                            return null;
                        }
                    }
                }
                // if all segments not offered with PBS or already included in Premier/ first calss, send back null segments 
                if (!segments.Any(s => s.PbSegmentType == MOBPBSegmentType.Regular || s.PbSegmentType == MOBPBSegmentType.AlreadyPurchased))
                {
                    return null;
                }
            }

            if (!produtDetail.Product.IsNullOrEmpty() && !produtDetail.Product.SubProducts.IsListNullOrEmpty())
            {
                var prices = produtDetail.Product.SubProducts.Where(s => s.InEligibleReason.IsNullOrEmpty() && s.Prices.IsNotNullNorEmpty()).SelectMany(p => p.Prices.Where(a => a.Association != null && a.PaymentOptions != null));
                BuildSegmentDetailPriceInfo(segments, prices, ref lowestPrice);
            }
            return segments;
        }

        private void BuildSegmentDetailPriceInfo(List<MOBPBSegment> segmentsWithBasics, IEnumerable<ProductPriceOption> prices, ref double lowestPrice)
        {
            if (prices.IsNull()) return;
            foreach (var segment in segmentsWithBasics)
            {
                segment.Fee = 0;
                foreach (var price in prices.
                    Where(p => p.PaymentOptions.IsNotNullNorEmpty()
                               && !p.PaymentOptions[0].IsNullOrEmpty()
                               && p.PaymentOptions[0].PriceComponents.IsNotNullNorEmpty()
                               && !p.PaymentOptions[0].PriceComponents[0].IsNullOrEmpty()
                               && !p.PaymentOptions[0].PriceComponents[0].Price.IsNullOrEmpty()
                               && p.PaymentOptions[0].PriceComponents[0].Price.Totals.IsNotNullNorEmpty()
                               && !p.PaymentOptions[0].PriceComponents[0].Price.Totals[0].IsNullOrEmpty()
                               && p.PaymentOptions[0].PriceComponents[0].Price.Totals[0].Amount > 0))
                {
                    if (segment.SegmentId == price.Association.SegmentRefIDs[0] && segment.PbSegmentType == MOBPBSegmentType.Regular)
                    {
                        int fee = Convert.ToInt32(price.PaymentOptions[0].PriceComponents[0].Price.Totals[0].Amount);
                        segment.Message = string.Format("{0}{1}{2}", "$", fee, "/traveler");
                        lowestPrice = GetLowest(lowestPrice, fee);
                        segment.CustPrice = fee;
                        segment.Fee = segment.Fee + fee;
                        if (segment.OfferIds.IsListNullOrEmpty())
                        {
                            segment.OfferIds = new List<string>();
                        }
                        segment.OfferIds.Add(price.ID);
                    }
                }
            }
        }

        private double GetLowest(double lowestPrice, double fee)
        {
            if (lowestPrice <= 0 && fee >= 0)
            {
                lowestPrice = fee;
            }
            else if (lowestPrice > fee && fee > 0)
            {
                lowestPrice = fee;
            }

            return lowestPrice;
        }

        private List<MOBPBSegment> BuildSegmentDetailInfoWithNoPBOfferedBaseOnSegment(List<MOBPBSegment> segments, InEligibleReason inEligibleReason, string segId)
        {
            if (!inEligibleReason.IsNullOrEmpty() && !segments.IsNullOrEmpty())
            {
                foreach (var segment in segments.Where(s => s.SegmentId == segId))
                {
                    BuildSegmentsNotOfferMessageWithReasonBaseOnSegment(segment, inEligibleReason);
                    if (segment.IsNullOrEmpty() || string.IsNullOrEmpty(segment.SegmentId))
                    {
                        segments = null;
                        break;
                    }
                }
            }
            return segments;
        }

        private void BuildSegmentsNotOfferMessageWithReasonBaseOnSegment(MOBPBSegment segment, InEligibleReason inEligibleReason)
        {
            if (segment.IsNullOrEmpty() || inEligibleReason.IsNullOrEmpty())
            {
                segment.SegmentId = string.Empty;
            }
            else if (segment.PbSegmentType == MOBPBSegmentType.Regular)
            {
                switch (string.Format("{0},{1}", inEligibleReason.MajorCode.Trim(), inEligibleReason.MinorCode.Trim()))
                {
                    // PBS Already purchased
                    case "010,3":
                        segment.PbSegmentType = MOBPBSegmentType.AlreadyPurchased;
                        segment.Fee = 0;
                        segment.Message = _configuration.GetValue<string>("PriorityBoardingAlreadyPurchasedMessage");
                        break;
                    // PBS  offered only for Economy
                    case "010,4":
                    case "010,7":
                        segment.PbSegmentType = MOBPBSegmentType.Included;
                        segment.Message = _configuration.GetValue<string>("PriorityBoardingIncludedMessage");
                        segment.Fee = 0;
                        break;
                    //PBS not Offered 
                    case "010,1":
                    case "010,2":
                    case "010,5":
                    case "010,10":
                    case "010,18":
                    case "010,19":
                    case "010,22":
                    case "002,3":
                        segment.SegmentId = string.Empty;
                        break;
                    default:
                        segment.PbSegmentType = MOBPBSegmentType.InEligible;
                        segment.Message = _configuration.GetValue<string>("PriorityBoardingNotAvailableMessage");
                        segment.Fee = 0;
                        break;
                }
            }
        }


        private List<MOBPBSegment> BuildSegmentsBasicInfoForPostBookingPath(Collection<ProductFlightSegment> flightSegments)
        {
            var segmentsWithBasicInfo = new List<MOBPBSegment>();
            if (flightSegments.IsListNullOrEmpty())
            {
                return null;
            }
            foreach (var flightSegment in flightSegments.
                Where(fs => fs != null && !string.IsNullOrEmpty(fs.ID)
                && fs.DepartureAirport != null && !string.IsNullOrEmpty(fs.DepartureAirport.IATACode)
                && fs.ArrivalAirport != null && !string.IsNullOrEmpty(fs.ArrivalAirport.IATACode)))
            {
                MOBPBSegment segment = new MOBPBSegment() { };
                segment.SegmentId = flightSegment.ID;
                segment.Origin = flightSegment.DepartureAirport.IATACode.ToString();
                segment.Destination = flightSegment.ArrivalAirport.IATACode.ToString();
                segment.FlightDate = string.Empty;
                segment.Customers = null;
                segment.PbSegmentType = MOBPBSegmentType.Regular;
                segment.OfferIds = new List<string>();
                segmentsWithBasicInfo.Add(segment);
            }
            return segmentsWithBasicInfo;
        }

        private List<MOBPBSegment> BuildSegmentBasicInfoBaseOnSegment(Collection<ProductOriginDestinationOption> oDOptions)
        {
            List<MOBPBSegment> segmentsWithBasicsInfo = new List<MOBPBSegment>() { };

            if (oDOptions == null || !oDOptions.Any())
                return null;

            foreach (var flightSegment in oDOptions.Where(o => o.FlightSegments != null && o.FlightSegments.Count > 0).SelectMany(f => f.FlightSegments.Where(s => s != null && !string.IsNullOrEmpty(s.IsActive) && s.IsActive.ToUpper().Trim() == "Y")))
            {
                MOBPBSegment segment = new MOBPBSegment() { };
                segment.SegmentId = flightSegment.ID;
                segment.Origin = flightSegment.DepartureAirport.IATACode.ToString();
                segment.Destination = flightSegment.ArrivalAirport.IATACode.ToString();
                segment.FlightDate = string.Empty;
                segment.Customers = null;
                segment.OfferIds = new List<string>();
                segment.PbSegmentType = MOBPBSegmentType.Regular;
                segmentsWithBasicsInfo.Add(segment);
            }
            return segmentsWithBasicsInfo;
        }

        private ProductDetail GetProductDetails(Service.Presentation.ProductResponseModel.ProductOffer offersResponse, string productCode)
        {
            var productDetail = new ProductDetail();
            if (offersResponse != null && !offersResponse.Offers.IsListNullOrEmpty() && !offersResponse.Offers[0].IsNullOrEmpty()
                && !offersResponse.Offers[0].ProductInformation.IsNullOrEmpty() && !offersResponse.Offers[0].ProductInformation.ProductDetails.IsListNullOrEmpty() && !string.IsNullOrEmpty(productCode))
            {
                productDetail = offersResponse.Offers[0].ProductInformation.ProductDetails.FirstOrDefault(p => p != null && p.Product != null && p.Product.Code.ToUpper().Trim() == productCode);
            }
            else
            {
                productDetail = null;
            }
            return productDetail;
        }

        private void BuildPBPurchasedTextBaseOnSegment(MOBPriorityBoarding priorityBoarding)
        {
            if (!priorityBoarding.IsNullOrEmpty() && string.IsNullOrEmpty(priorityBoarding.PbDetailsConfirmationTxt) && !priorityBoarding.Segments.IsNullOrEmpty())
            {
                if (priorityBoarding.Segments.Any(segment => segment.PbSegmentType == MOBPBSegmentType.AlreadyPurchased))
                {
                    priorityBoarding.PbDetailsConfirmationTxt = _configuration.GetValue<string>("PriorityBoardingConfirmationTxtBaseOnSegment");
                    priorityBoarding.PbAddedTravelerTxt = _configuration.GetValue<string>("PriorityBoardingAddedSegmentTxt");
                }
            }
        }

        private async Task PersistAncillaryProducts(string sessionId, GetOffers productOffer, bool IsCCEDynamicOffer = false, String product = "")
        {
            //Task.Run(async() =>
            //{
            var persistedProductOffers = new GetOffers();
            persistedProductOffers = await _sessionHelperService.GetSession<GetOffers>(sessionId, persistedProductOffers.ObjectName, new List<string> { sessionId, persistedProductOffers.ObjectName }).ConfigureAwait(false);
            if (_configuration.GetValue<bool>("EnableOmniCartReleaseCandidateTwoChanges_Bundles") && !string.IsNullOrEmpty(product))
            {
                //Remove the Existing offer if we are making the dynamicOffer call multiple times with the same session
                RemoveProductOfferIfAlreadyExists(persistedProductOffers, product);
            }

            if (persistedProductOffers != null && persistedProductOffers.Offers.Count > 0)
            {
                if (!_configuration.GetValue<bool>("DisablePostBookingPurchaseFailureFix"))//Flightsegments need to be updated when ever we get an offer for the product.
                {
                    persistedProductOffers.FlightSegments = productOffer.FlightSegments;
                    persistedProductOffers.Travelers = productOffer.Travelers;
                    persistedProductOffers.Solutions = productOffer.Solutions;
                    persistedProductOffers.Response = productOffer.Response;
                    persistedProductOffers.Requester = productOffer.Requester;
                }

                if (_configuration.GetValue<bool>("EnableOmniCartReleaseCandidateTwoChanges_Bundles") && !string.IsNullOrEmpty(product) && productOffer.Offers.FirstOrDefault().ProductInformation.ProductDetails != null && productOffer.Offers.FirstOrDefault().ProductInformation.ProductDetails.Count > 0)
                {
                    foreach (var productDetails in productOffer.Offers.FirstOrDefault().ProductInformation.ProductDetails)
                    {
                        persistedProductOffers.Offers.FirstOrDefault().ProductInformation.ProductDetails.Add(productDetails);
                    }
                }
                else
                {
                    persistedProductOffers.Offers.FirstOrDefault().ProductInformation.ProductDetails.Add(productOffer.Offers.FirstOrDefault().ProductInformation.ProductDetails.FirstOrDefault());
                }
            }
            else
            {
                persistedProductOffers = productOffer;
            }
            if (_configuration.GetValue<bool>("EnableOmniCartReleaseCandidateTwoChanges_Bundles"))
            {
                if (IsCCEDynamicOffer)
                {
                    if (productOffer.Characteristics == null)
                    {
                        productOffer.Characteristics = new Collection<Characteristic>();
                    }
                    if (persistedProductOffers != null && !persistedProductOffers.Characteristics.Any(characteristic => characteristic.Code == "IsEnabledThroughCCE"))
                    {
                        productOffer.Characteristics.Add(new Characteristic { Code = "IsEnabledThroughCCE", Value = "True" });
                    }
                }
                else// Need to remove this characteristics when IsCCEDynamicOffer==false ,As this is the same method we use for saving the postbooking products (PBS and PCU) at that we shouldnt send this characteristis as we are going to flightshoppig.
                {
                    if (persistedProductOffers != null && persistedProductOffers.Characteristics?.Any(characteristic => characteristic.Code == "IsEnabledThroughCCE") == true)
                    {
                        persistedProductOffers.Characteristics.Remove(persistedProductOffers.Characteristics.First(characteristic => characteristic.Code == "IsEnabledThroughCCE"));
                    }
                }
            }

            await _sessionHelperService.SaveSession<GetOffers>(persistedProductOffers, sessionId, new List<string> { sessionId, typeof(GetOffers).FullName }, typeof(GetOffers).FullName).ConfigureAwait(false);
            //  });
        }

        private void RemoveProductOfferIfAlreadyExists(GetOffers productOffers, string product)
        {
            if (product == "Bundle")
            {
                //If it is bundles remove all the bundle products in the offers.
                if (productOffers?.Offers != null)
                {
                    var products = productOffers.Offers.First()
                          .ProductInformation
                          .ProductDetails
                          .Where(productDetail =>
                                     productDetail?.Product?.SubProducts != null
                                     && productDetail.Product.SubProducts.Any(subProduct => subProduct.GroupCode == "BE"));//Get all products with groupCode as BE (This would be Be only for bundle products)
                    if (products != null)
                    {
                        foreach (var bundleProduct in products.ToList())
                        {
                            productOffers.Offers.First().ProductInformation.ProductDetails.Remove(bundleProduct);
                        }

                    }
                }
            }
        }




        private void GetTravelTypesFromShop(SerializableSavedItinerary ub, MOBSHOPUnfinishedBooking mobEntry)
        {
            foreach (var t in ub.PaxInfoList.GroupBy(p => p.PaxType))
            {
                switch ((int)t.Key)
                {
                    case (int)PaxType.Adult:
                        mobEntry.TravelerTypes.Add(new MOBTravelerType() { Count = t.Count(), TravelerType = MOBPAXTYPE.Adult.ToString() });
                        break;

                    case (int)PaxType.Senior:
                        mobEntry.TravelerTypes.Add(new MOBTravelerType() { Count = t.Count(), TravelerType = MOBPAXTYPE.Senior.ToString() });
                        break;

                    case (int)PaxType.Child01:
                        mobEntry.TravelerTypes.Add(new MOBTravelerType() { Count = t.Count(), TravelerType = MOBPAXTYPE.Child2To4.ToString() });
                        break;

                    case (int)PaxType.Child02:
                        mobEntry.TravelerTypes.Add(new MOBTravelerType() { Count = t.Count(), TravelerType = MOBPAXTYPE.Child5To11.ToString() });
                        break;

                    case (int)PaxType.Child03:
                        mobEntry.TravelerTypes.Add(new MOBTravelerType() { Count = t.Count(), TravelerType = MOBPAXTYPE.Child12To17.ToString() });
                        break;

                    case (int)PaxType.Child04:
                        mobEntry.TravelerTypes.Add(new MOBTravelerType() { Count = t.Count(), TravelerType = MOBPAXTYPE.Child12To14.ToString() });
                        break;

                    case (int)PaxType.Child05:
                        mobEntry.TravelerTypes.Add(new MOBTravelerType() { Count = t.Count(), TravelerType = MOBPAXTYPE.Child15To17.ToString() });
                        break;

                    case (int)PaxType.InfantSeat:
                        mobEntry.TravelerTypes.Add(new MOBTravelerType() { Count = t.Count(), TravelerType = MOBPAXTYPE.InfantSeat.ToString() });
                        break;

                    case (int)PaxType.InfantLap:
                        mobEntry.TravelerTypes.Add(new MOBTravelerType() { Count = t.Count(), TravelerType = MOBPAXTYPE.InfantLap.ToString() });
                        break;
                    default:
                        mobEntry.TravelerTypes.Add(new MOBTravelerType() { Count = t.Count(), TravelerType = MOBPAXTYPE.Adult.ToString() });
                        break;
                }
            }
        }



        private void GetFaceCoveringMessage(MOBCheckOutRequest request, MOBCheckOutResponse response, Reservation reservation)
        {
            if (_configuration.GetValue<bool>("IsEnableIndiaRepatriationFlightMessaging"))
            {
                if (request != null && !string.IsNullOrEmpty(request.Flow) && (request.Flow == FlowType.BOOKING.ToString() || request.Flow == FlowType.RESHOP.ToString()) && response != null && response.ShoppingCart != null)
                {

                    if (IsEnableEnableCombineConfirmationAlerts(request.Application.Id, request.Application.Version.Major))
                    {
                        if (reservation?.ShopReservationInfo2?.AlertMessages != null)
                        {
                            reservation?.ShopReservationInfo2?.AlertMessages.ForEach(am =>
                            {
                                AddConfirmationScreenAlertMessage(response.Reservation.ShopReservationInfo2, am.Text1, am.Text2, am.Text3, am.Order);
                            });
                        }
                    }
                    else
                    {
                        if (reservation?.ShopReservationInfo2?.AlertMessages != null && reservation.ShopReservationInfo2.AlertMessages.Any())
                        {
                            if (response.ShoppingCart.AlertMessages == null)
                                response.ShoppingCart.AlertMessages = new List<MOBSection>();

                            if (response.ShoppingCart.AlertMessages.Any())
                            {
                                response.ShoppingCart.AlertMessages.InsertRange(0, reservation.ShopReservationInfo2.AlertMessages);
                            }
                            else
                                response.ShoppingCart.AlertMessages.AddRange(reservation.ShopReservationInfo2.AlertMessages);

                            if (response.ShoppingCart.AlertMessages.Count > 1)
                                response.ShoppingCart.AlertMessages = response.ShoppingCart.AlertMessages.GroupBy(m => m.Text2).Select(x => x.First()).ToList();
                        }
                    }
                }
            }
            else
            {
                if (_configuration.GetValue<bool>("IsEnableFaceCoveringMessage") && request != null && !string.IsNullOrEmpty(request.Flow) && (request.Flow == FlowType.BOOKING.ToString() || request.Flow == FlowType.RESHOP.ToString()) && response != null && response.ShoppingCart != null)
                {
                    if (IsEnableEnableCombineConfirmationAlerts(request.Application.Id, request.Application.Version.Major))
                    {

                        AddConfirmationScreenAlertMessage(response.Reservation.ShopReservationInfo2, _configuration.GetValue<string>("FaceCoverMessageTextHeader"), _configuration.GetValue<string>("FaceCoverMessageTextBody"), "", MOBCONFIRMATIONALERTMESSAGEORDER.FACECOVERING.ToString());
                    }
                    else
                    {
                        if (response.ShoppingCart.AlertMessages != null && response.ShoppingCart.AlertMessages.Any())
                        {
                            List<MOBSection> sortAlertMessages = new List<MOBSection>();
                            MOBSection alert = new MOBSection()
                            {
                                Text1 = _configuration.GetValue<string>("FaceCoverMessageTextHeader"),
                                Text2 = _configuration.GetValue<string>("FaceCoverMessageTextBody")
                            };
                            sortAlertMessages.Add(alert);
                            foreach (var message in response.ShoppingCart.AlertMessages)
                            {
                                if (!string.IsNullOrEmpty(message.Text1) && !message.Text1.ToUpper().Equals(_configuration.GetValue<string>("FaceCoverMessageTextHeader").ToUpper(), StringComparison.OrdinalIgnoreCase))
                                {
                                    sortAlertMessages.Add(message);
                                }
                            }
                            response.ShoppingCart.AlertMessages = sortAlertMessages;
                        }
                        else
                        {
                            response.ShoppingCart.AlertMessages = new List<MOBSection>();
                            MOBSection alert = new MOBSection()
                            {
                                Text1 = _configuration.GetValue<string>("FaceCoverMessageTextHeader"),
                                Text2 = _configuration.GetValue<string>("FaceCoverMessageTextBody")
                            };
                            response.ShoppingCart.AlertMessages.Add(alert);
                        }
                    }
                }
            }
        }

        private async Task GenerateTPISecondaryPaymentInfoFOP(MOBCheckOutRequest checkOutRequest, MOBCheckOutResponse checkOutResponse, FlightReservationResponse flightReservationResponse, MOBShoppingCart persistedShoppingCart, Session session = null)
        {
            if (persistedShoppingCart != null && /*Convert.ToBoolean(persistedShoppingCart.Products.Any(x => x.Code == "TPI")) &&*/ flightReservationResponse.CheckoutResponse.ShoppingCartResponse.Items.Any(x => x.Item.Product[0].Code == "TPI") &&
                flightReservationResponse.CheckoutResponse.ShoppingCartResponse.Items.Where(x => x.Item.Product[0].Code == "TPI").Select(x => x.Item).Any(y => y.Status.Contains("FAILED"))
                && !ValidateFormOfPaymentTypeCFOP(checkOutRequest.FormofPaymentDetails))
            {
                checkOutResponse.IsTPIFailed = true;

                if (((checkOutRequest.Flow == FlowType.VIEWRES.ToString() || UtilityHelper.IsCheckinFlow(checkOutRequest.Flow)) && flightReservationResponse.CheckoutResponse.ShoppingCart.Items.Where(x => x.Product[0].Code != "RES").Count() == 1))
                {
                    checkOutResponse.Exception = new MOBException("9999", _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                }
                else
                {
                    if (checkOutResponse.ShoppingCart == null)
                        checkOutResponse.ShoppingCart = new MOBShoppingCart();
                    if (ConfigUtility.IsViewResFlowCheckOut(checkOutRequest.Flow))
                    {
                        MOBSection alertMsg = await GetTPIAlertMessage(false);
                        checkOutResponse.ShoppingCart.Products = new List<MOBProdDetail> { new MOBProdDetail {
                        Code = "TPI",
                        ProdDescription = "Travel Insurance",
                        Segments = null,
                        TermsAndCondition = new MOBMobileCMSContentMessages { Title = alertMsg.Text1, ContentFull = alertMsg.Text2  }} };
                    }
                    else
                    {
                        MOBProductSubSegmentDetail subSegmentDetails = new MOBProductSubSegmentDetail();
                        checkOutResponse.ShoppingCart.Products = new List<MOBProdDetail> { new MOBProdDetail{
                    Code = "TPI",
                    ProdDescription = string.Empty,
                    ProdTotalPrice = string.Format("{0:0.00}", flightReservationResponse.CheckoutResponse.ShoppingCart.Items.Where(x => x.Product[0].Code == "TPI" && x.Product[0].Price != null && x.Product[0].Price.Totals != null).Select(x => x.Product[0].Price.Totals[0].Amount).FirstOrDefault().ToString()),
                    ProdDisplayTotalPrice = Decimal.Parse(flightReservationResponse.CheckoutResponse.ShoppingCart.Items.Where(x => x.Product[0].Code == "TPI" && x.Product[0].Price != null && x.Product[0].Price.Totals != null).Select(x => x.Product[0].Price.Totals[0].Amount).FirstOrDefault().ToString()).ToString("c"),
                    Segments = new List<MOBProductSegmentDetail > {
                                     new MOBProductSegmentDetail{
                                         ProductIds = new List<string> { flightReservationResponse.DisplayCart.TravelOptions.Where(x=>x.Key=="TPI").SelectMany(x=>x.SubItems).Where(x=>x.Amount!=0).FirstOrDefault().Key },
                                        SegmentInfo = "",
                                        SubSegmentDetails = new List<MOBProductSubSegmentDetail>
                                                            {
                                                                new MOBProductSubSegmentDetail
                                                                {
                                                                    Price = String.Format("{0:0.00}", flightReservationResponse.CheckoutResponse.ShoppingCart.Items.Where(x => x.Product[0].Code == "TPI" && x.Product[0].Price != null && x.Product[0].Price.Totals != null).Select(x => x.Product[0].Price.Totals[0].Amount).FirstOrDefault().ToString()),
                                                                    DisplayPrice = Decimal.Parse(flightReservationResponse.CheckoutResponse.ShoppingCart.Items.Where(x => x.Product[0].Code == "TPI" && x.Product[0].Price != null && x.Product[0].Price.Totals != null).Select(x => x.Product[0].Price.Totals[0].Amount).FirstOrDefault().ToString()).ToString("c"),
                                                                    Passenger = "",
                                                                    SegmentDescription = "Travel Insurance"
                                                                }
                                                            }
                                                    }
                                                    },
                    }
                    };
                        var productVendorOffer = new GetVendorOffers();
                        productVendorOffer = await _sessionHelperService.GetSession<GetVendorOffers>(checkOutRequest.SessionId, productVendorOffer.ObjectName, new List<string> { checkOutRequest.SessionId, productVendorOffer.ObjectName }).ConfigureAwait(false);
                        checkOutResponse.ShoppingCart.PaymentAlerts = new List<MOBSection> { await GetTPIAlertMessage(false) };
                        checkOutResponse.ShoppingCart.TermsAndConditions = await _shoppingCartUtility.GetProductBasedTermAndConditions(productVendorOffer, flightReservationResponse, true, checkOutRequest.SessionId, checkOutRequest.Flow);
                        checkOutResponse.ShoppingCart.PaymentTarget = "TPI";
                        if (!_configuration.GetValue<bool>("DisableFixForSendingPnrToUpliftWhenSecondaryFOP"))
                        {
                            checkOutResponse.RecordLocator = flightReservationResponse?.Reservation?.ConfirmationID;
                            checkOutResponse.PnrCreateDate = flightReservationResponse?.Reservation?.CreateDate;
                            checkOutResponse.LastName = flightReservationResponse?.Reservation?.Travelers?.FirstOrDefault()?.Person?.Surname;
                        }
                    }
                    checkOutResponse.ShoppingCart.TotalPrice = string.Format("{0:0.00}", flightReservationResponse.CheckoutResponse.ShoppingCart.Items.Where(x => x.Product[0].Code == "TPI" && x.Product[0].Price != null && x.Product[0].Price.Totals != null).Select(x => x.Product[0].Price.Totals[0].Amount).FirstOrDefault().ToString());
                    checkOutResponse.ShoppingCart.DisplayTotalPrice = Decimal.Parse(flightReservationResponse.CheckoutResponse.ShoppingCart.Items.Where(x => x.Product[0].Code == "TPI" && x.Product[0].Price != null && x.Product[0].Price.Totals != null).Select(x => x.Product[0].Price.Totals[0].Amount).FirstOrDefault().ToString()).ToString("c");
                    checkOutResponse.PostPurchasePage = PostPurchasePage.SecondaryFormOfPayment.ToString();
                    checkOutResponse.EnabledSecondaryFormofPayment = true;
                    if (checkOutResponse?.ShoppingCart?.Offers != null && !string.IsNullOrEmpty(checkOutResponse?.ShoppingCart?.Offers.OfferCode) && checkOutResponse?.ShoppingCart?.Offers.IsPassPlussOffer == true)
                    {
                        checkOutResponse.ShoppingCart.Offers.IsPassPlussOffer = false;// making this flag to false if it is secondarfop as client is using this flag to support only uatp card on CC screen
                    }
                }
            }
            else if (!ConfigUtility.IsViewResFlowCheckOut(checkOutRequest.Flow) && persistedShoppingCart != null /*&& Convert.ToBoolean(persistedShoppingCart.Products.Any(x => x.Code == "TPI"))*/ && flightReservationResponse.CheckoutResponse.ShoppingCartResponse.Items.Any(x => x.Item.Product[0].Code == "TPI") &&
                flightReservationResponse.CheckoutResponse.ShoppingCartResponse.Items.Where(x => x.Item.Product[0].Code == "TPI").Select(x => x.Item).Any(y => y.Status.Contains("FAILED"))
                && ValidateFormOfPaymentTypeCFOP(checkOutRequest.FormofPaymentDetails))
            {
                MOBShoppingCart shoppingCart = new MOBShoppingCart();
                SeatChangeState state = new SeatChangeState();
                state = await _sessionHelperService.GetSession<SeatChangeState>(checkOutRequest.SessionId, state.ObjectName, new List<string> { checkOutRequest.SessionId, state.ObjectName }).ConfigureAwait(false);

                shoppingCart.Products = await _shoppingCartUtility.ConfirmationPageProductInfo(flightReservationResponse, true, false, checkOutRequest.Flow.ToString(), checkOutRequest.Application, state, sessionId: checkOutRequest.SessionId);
                shoppingCart.TotalPrice = string.Format("{0:0.00}", shoppingCart.Products.Select(x => Convert.ToDouble(x.ProdTotalPrice)).ToList().Sum());
                shoppingCart.DisplayTotalPrice = Decimal.Parse(shoppingCart.Products.Select(x => Convert.ToDouble(x.ProdTotalPrice)).ToList().Sum().ToString()).ToString("c");
                shoppingCart.FormofPaymentDetails = persistedShoppingCart.FormofPaymentDetails;
                shoppingCart.FormofPaymentDetails.EmailAddress = (!string.IsNullOrEmpty(checkOutRequest.FormofPaymentDetails.EmailAddress)) ? checkOutRequest.FormofPaymentDetails.EmailAddress.ToString() : (flightReservationResponse.Reservation.EmailAddress.Count() > 0) ? flightReservationResponse.Reservation.EmailAddress.Where(x => x.Address != null).Select(x => x.Address).FirstOrDefault().ToString() : null;
                shoppingCart.CartId = checkOutRequest.CartId;
                shoppingCart.AlertMessages = new List<MOBSection> { await GetTPIAlertMessage(true, session, checkOutRequest.Flow) };
                if (shoppingCart.AlertMessages.Count > 1)
                {
                    shoppingCart.AlertMessages[0].Text3 = string.Empty;
                }
                shoppingCart.Flow = checkOutRequest.Flow;
                await _sessionHelperService.SaveSession<MOBShoppingCart>(shoppingCart, checkOutRequest.SessionId, new List<string> { checkOutRequest.SessionId, shoppingCart.ObjectName }, shoppingCart.ObjectName).ConfigureAwait(false);

                checkOutResponse.ShoppingCart = shoppingCart;
                checkOutResponse.PostPurchasePage = PostPurchasePage.Confirmation.ToString();
                checkOutResponse.RecordLocator = flightReservationResponse.Reservation.ConfirmationID;
                checkOutResponse.PnrCreateDate = flightReservationResponse.Reservation.CreateDate;
                checkOutResponse.LastName = flightReservationResponse.Reservation.Travelers[0].Person.Surname;
                checkOutResponse.Flow = checkOutRequest.Flow;
                checkOutResponse.SessionId = checkOutRequest.SessionId;

                checkOutResponse.IsTPIFailed = true;
                checkOutResponse.Reservation.IsRedirectToSecondaryPayment = false;

            }
        }

        private bool ValidateFormOfPaymentTypeCFOP(MOBFormofPaymentDetails formOfPayment)
        {
            return formOfPayment != null && formOfPayment.FormOfPaymentType.ToUpper() == MOBFormofPayment.CreditCard.ToString().ToUpper() && formOfPayment.CreditCard != null && !string.IsNullOrEmpty(formOfPayment.CreditCard.CardType) &&
                                    IsValidFOPForTPIpayment(formOfPayment.CreditCard.CardType);
        }

        private void CheckinSpecialErrors(string err)
        {
            switch (err)
            {
                case "CHANGE_SEAT_FAILED":
                case "SEAT_NO_LONGER_AVAILABLE":
                    throw new MOBUnitedException("SeatChangeError", "SeatChangeError");
                case "CHANGE_FLIGHT_ERROR":
                    throw new MOBUnitedException("SDCChangeError", "SDCChangeError");
                case "CHANGE_CABIN_FAILED":
                    throw new MOBUnitedException("DoDChangeError", "DoDChangeError");
            }
        }

        private async Task<bool> CheckPCUStatusAndHandleErrors(FlightReservationResponse flightReservationResponse, MOBCheckOutRequest checkOutRequest, MOBCheckOutResponse checkOutResponse)
        {
            if (flightReservationResponse.CheckoutResponse.ShoppingCartResponse.Items.Any(x => x.Item.Product[0].Code == "PCU"))
            {
                if (flightReservationResponse.Errors.Any(e => e.MinorCode == "90506"))
                {
                    checkOutResponse.ShoppingCart = new MOBShoppingCart();
                    checkOutResponse.ShoppingCart.AlertMessages = new List<MOBSection>();
                    if (checkOutResponse.ShoppingCart == null)
                        checkOutResponse.ShoppingCart = new MOBShoppingCart();

                    List<SCProductContext> unSuccessfulSegmentDetails = new List<SCProductContext>();
                    List<string> refundedSegmentNums;
                    if (_configuration.GetValue<bool>("EnableFixCheckOutDeserilizeIssue"))
                    {
                        var lstProductContext = flightReservationResponse.CheckoutResponse.ShoppingCartResponse.Items.Where(u => u.Item != null && string.Equals(u.Item.Category, "Reservation.Merchandise.PCU", StringComparison.OrdinalIgnoreCase))?
                            .Select(u => u.Item)?.FirstOrDefault()?.ProductContext;

                        if (lstProductContext != null && lstProductContext.Any())
                        {
                            foreach (var productContext in lstProductContext.Distinct().ToList())
                            {
                                var sCProductContexts = DataContextJsonSerializer.NewtonSoftDeserializeIgnoreErrorAndReturnNull<List<SCProductContext>>(productContext);
                                if (sCProductContexts != null && sCProductContexts.Count > 0)
                                {
                                    unSuccessfulSegmentDetails.AddRange(sCProductContexts);
                                }
                            }
                        }
                    }
                    else
                    {
                        var lstProductContext = flightReservationResponse.CheckoutResponse.ShoppingCartResponse.Items.Where(u => u.Item != null).Select(u => u.Item).
                            Where(u => u.Category == "Reservation.Merchandise.PCU").Select(u => u.ProductContext).Select(x => x.Where(p => !p.StartsWith("<"))).ToList().SelectMany(x => x).ToList();
                        foreach (var productContext in lstProductContext.Distinct().ToList())
                            unSuccessfulSegmentDetails.AddRange(JsonConvert.DeserializeObject<List<SCProductContext>>(productContext));
                    }

                    var isRefundSuccess = _shoppingCartUtility.IsRefundSuccess(flightReservationResponse.CheckoutResponse.ShoppingCartResponse.Items, out refundedSegmentNums, checkOutRequest.Flow);
                    var isPartialSuccess = (isRefundSuccess == true && (unSuccessfulSegmentDetails.Count() != 0 || unSuccessfulSegmentDetails != null)) ? flightReservationResponse.DisplayCart.TravelOptions.SelectMany(x => x.SubItems).Where(x => x.Amount != 0).Count() != unSuccessfulSegmentDetails.Count() : false;

                    if (isRefundSuccess == true)
                    {
                        checkOutResponse.ShoppingCart.Products = await _shoppingCartUtility.ConfirmationPageProductInfo(flightReservationResponse, true, true,
                           checkOutRequest.Flow.ToString(), checkOutRequest.Application, null, isRefundSuccess, isPartialSuccess,
                           unSuccessfulSegmentDetails, refundedSegmentNums, sessionId: checkOutRequest.SessionId);
                        double price = ConfigUtility.GetGrandTotalPriceForShoppingCart(false, flightReservationResponse, true, checkOutRequest.Flow);
                        checkOutResponse.ShoppingCart.TotalPrice = String.Format("{0:0.00}", price);
                        checkOutResponse.ShoppingCart.DisplayTotalPrice = Decimal.Parse(price.ToString()).ToString("c");
                        checkOutResponse.ShoppingCart.FormofPaymentDetails = checkOutRequest.FormofPaymentDetails;
                    }
                    string failureMessage = (isRefundSuccess == true) ? ((isPartialSuccess == true) ? "PCUPartialFailure" : "PCUFullFailure") : "PCURefundFailure";

                    if (!_configuration.GetValue<bool>("DisablePCUCheckOutUnitedExc_MOBILE19735"))
                        _logger.LogInformation("CheckPCUStatusAndHandleErrors - UnitedException {failureMessage} with {sessionId}", failureMessage, checkOutRequest.SessionId);
                    else
                        _logger.LogInformation("CheckPCUStatusAndHandleErrors - UnitedException {failureMessage} with {sessionId}", failureMessage, checkOutRequest.SessionId);

                    checkOutResponse.ShoppingCart.Flow = checkOutRequest.Flow;
                    checkOutResponse.ShoppingCart.CartId = checkOutRequest.CartId;
                    checkOutResponse.ShoppingCart.TermsAndConditions = null;
                    checkOutResponse.ShoppingCart.AlertMessages.Add(await GetAlertMessage(flightReservationResponse, isRefundSuccess, isPartialSuccess, refundedSegmentNums, false));
                    checkOutResponse.RecordLocator = flightReservationResponse.Reservation.ConfirmationID;
                    checkOutResponse.LastName = flightReservationResponse.Reservation.Travelers[0].Person.Surname;
                    return true;
                }
                else if (flightReservationResponse.Errors.Any(e => e.MinorCode == "90585"))
                {
                    checkOutResponse.ShoppingCart = new MOBShoppingCart();
                    checkOutResponse.ShoppingCart.AlertMessages = new List<MOBSection>();
                    if (!_configuration.GetValue<bool>("DisablePCUCheckOutUnitedExc_MOBILE19735"))
                        _logger.LogInformation("CheckPCUStatusAndHandleErrors - UnitedException {message} with {sessionId}", "PCUMerchandizingServiceFailure", checkOutRequest.SessionId);
                    else
                        _logger.LogInformation("CheckPCUStatusAndHandleErrors - UnitedException {message} with {sessionId}", "PCUMerchandizingServiceFailure", checkOutRequest.SessionId);

                    checkOutResponse.ShoppingCart.Flow = checkOutRequest.Flow;
                    checkOutResponse.ShoppingCart.CartId = checkOutRequest.CartId;
                    checkOutResponse.ShoppingCart.TermsAndConditions = null;
                    checkOutResponse.ShoppingCart.AlertMessages.Add(await GetAlertMessage(flightReservationResponse, false, false, null, true));
                    checkOutResponse.RecordLocator = flightReservationResponse.Reservation.ConfirmationID;
                    checkOutResponse.LastName = flightReservationResponse.Reservation.Travelers[0].Person.Surname;
                    return true;
                }
            }
            return false;
        }

        private async Task<MOBSection> GetAlertMessage(FlightReservationResponse flightReservationResponse, bool isRefundSuccess, bool isPartialSuccess, List<string> refundedSegmentNums, bool isGenericError)
        {
            MOBSection alertMessage = new MOBSection();

            if (isGenericError)
            {
                var errorMessages = await _eTCUtility.GetCaptions("PCU_UpgradeFailed_GenericError");
                alertMessage.Text1 = errorMessages.Where(x => x.Id == "HEADER").Select(x => x.CurrentValue).FirstOrDefault().ToString();
                alertMessage.Text2 = errorMessages.Where(x => x.Id == "BODY").Select(x => x.CurrentValue).FirstOrDefault().ToString();
            }
            else if (isRefundSuccess && isPartialSuccess)
            {
                List<string> segmentDetails = new List<string>();
                foreach (var refundedSegmentNum in refundedSegmentNums)
                    segmentDetails.Add(flightReservationResponse.Reservation.FlightSegments.Where(y => y.FlightSegment.SegmentNumber == Convert.ToInt32(refundedSegmentNum)).Select(y => y.FlightSegment.DepartureAirport.IATACode + " - " + y.FlightSegment.ArrivalAirport.IATACode).FirstOrDefault().ToString()
                        + "~" + flightReservationResponse.DisplayCart.TravelOptions.Where(x => x.Key == "PCU").SelectMany(x => x.SubItems).Where(x => x.Amount != 0 && x.SegmentNumber == refundedSegmentNum).Select(x => x.Amount).Sum());

                var errorMessages = await _eTCUtility.GetCaptions("PCU_UpgradePartialFailure_Refunded");
                alertMessage.Text1 = errorMessages.Where(x => x.Id == "HEADER").Select(x => x.CurrentValue).FirstOrDefault().ToString();
                string subcontent = errorMessages.Where(x => x.Id == "OPTIONALSUBCONTENT").Select(x => x.CurrentValue).FirstOrDefault().ToString();
                alertMessage.Text2 = errorMessages.Where(x => x.Id == "BODY").Select(x => x.CurrentValue).FirstOrDefault().ToString() + string.Join("<br>", segmentDetails.Select(x => string.Format(subcontent, x.Split('~')[0], Decimal.Parse(x.Split('~')[1]).ToString("c"))).ToList());
            }
            else if (!isPartialSuccess && isRefundSuccess)
            {
                var errorMessages = await _eTCUtility.GetCaptions("PCU_UpgradeFailed_Refunded");
                alertMessage.Text1 = errorMessages.Where(x => x.Id == "HEADER").Select(x => x.CurrentValue).FirstOrDefault().ToString();
                alertMessage.Text2 = errorMessages.Where(x => x.Id == "BODY").Select(x => x.CurrentValue).FirstOrDefault().ToString();
            }
            else if (!isRefundSuccess)
            {
                var errorMessages = await _eTCUtility.GetCaptions("PCU_UpgradeFailed");
                alertMessage.Text1 = errorMessages.Where(x => x.Id == "HEADER").Select(x => x.CurrentValue).FirstOrDefault().ToString();
                alertMessage.Text2 = errorMessages.Where(x => x.Id == "BODY").Select(x => x.CurrentValue).FirstOrDefault().ToString();
            }

            return alertMessage;
        }

        private List<MOBMobileCMSContentMessages> GetSDLMessageFromList(List<CMSContentMessage> list, string title)
        {
            List<MOBMobileCMSContentMessages> listOfMessages = new List<MOBMobileCMSContentMessages>();
            list?.Where(l => l.Title.ToUpper().Equals(title.ToUpper()))?.ForEach(i => listOfMessages.Add(new MOBMobileCMSContentMessages()
            {
                Title = i.Title,
                ContentFull = HttpUtility.HtmlDecode(i.ContentFull),
                HeadLine = i.Headline,
                ContentShort = HttpUtility.HtmlDecode(i.ContentShort),
                LocationCode = i.LocationCode
            }));

            return listOfMessages;
        }

        public async Task<MOBPNRAdvisory> PopulateSeatConfirmarionAlertMessage(Session session, MOBRequest request)
        {
            List<CMSContentMessage> lstMessages = await _shoppingCartUtility.GetSDLContentByGroupName(request, session.SessionId, session.Token, _configuration.GetValue<string>("CMSContentMessages_GroupName_MANAGERESOffers_Messages"), "ManageReservation_Offers_CMSContentMessagesCached_StaticGUID");

            MOBPNRAdvisory changeSeatConfirmationMessage = new MOBPNRAdvisory();

            if (lstMessages != null)
            {
                var changeSeatMessage = GetSDLMessageFromList(lstMessages, "SeatMap.FreeSeatConfirmationMessage");
                if (changeSeatMessage != null)
                {
                    changeSeatConfirmationMessage.ContentType = ContentType.CHANGESEATCONFIRMATIONMESSAGE;
                    changeSeatConfirmationMessage.AdvisoryType = AdvisoryType.ECO_ALERT;
                    changeSeatConfirmationMessage.Header = changeSeatMessage.FirstOrDefault()?.ContentShort;
                    changeSeatConfirmationMessage.Body = changeSeatMessage.FirstOrDefault()?.ContentFull;
                    changeSeatConfirmationMessage.IsDefaultOpen = true;
                }

            }
            return changeSeatConfirmationMessage;
        }

            private bool GetTokenExpireDateTimeUTC(Session session)
        {
            var returnVal = false;
            try
            {
                DateTime localDateTime;

                var result = DateTime.TryParse(session.CreationTime.ToString(), out localDateTime);
                if (result)
                {
                    var expDatetime = localDateTime.AddSeconds(session.TokenExpirationValueInSeconds);
                    returnVal = (expDatetime.ToUniversalTime() <= DateTime.UtcNow && expDatetime != DateTime.MinValue);
                }


            }
            catch (FormatException)
            {
            }
            return returnVal;
        }

        // Notify the declination offer
        public async void NotifyTPIOfferDeclined(string sessionId)
        {
            var session = new Session();
            session = await _sessionHelperService.GetSession<Session>(sessionId, session.ObjectName, new List<string> { sessionId, session.ObjectName }).ConfigureAwait(false);
            Reservation persistedReservation = new Reservation();
            persistedReservation = await _sessionHelperService.GetSession<Reservation>(sessionId, persistedReservation.ObjectName, new List<string> { sessionId, persistedReservation.ObjectName }).ConfigureAwait(false);

            var requestObject = new United.Service.Presentation.ProductRequestModel.ProductOffer()
            {
                Requester = new ServiceClient()
                {
                    Requestor = new Requestor() { ChannelID = "401", ChannelName = "MBE" }
                },
                Filters = new Collection<ProductFilter>()
                {
                    new ProductFilter(){ ProductCode = "TPI", IsIncluded = true.ToString() }
                },
                Characteristics = new Collection<Characteristic>()
                {
                    new Characteristic(){ Code = "QuotePackId", Value = persistedReservation.TripInsuranceFile.TripInsuranceBookingInfo.QuoteId},
                    new Characteristic(){ Code = "DECLINE_OFFER", Value = true.ToString() }
                }
            };

            string jsonRequest = JsonConvert.SerializeObject(requestObject);
            string jsonResponse = await _merchandizingCSLService.DeclineTPIOffer(session.Token, jsonRequest);

            if (jsonResponse.Contains("SUCCESS"))
            {
                _logger.LogInformation("Checkout Merch Trip Insurance Offer Declined Successfully for QuoteId {QuoteId}", persistedReservation.TripInsuranceFile.TripInsuranceBookingInfo.QuoteId);
            }
            else
            {
                _logger.LogWarning("CheckOut Merch Trip Insurance Offer Declined Issue {response}", jsonResponse);
            }
        }
    }
}
