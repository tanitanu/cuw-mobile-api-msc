using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Common.Helper;
using United.Common.Helper.MSCPayment.Interfaces;
using United.Definition;
using United.Definition.FFC;
using United.Definition.FormofPayment;
using United.Definition.FormofPayment.TravelCredit;
using United.Definition.Shopping;
using United.Definition.Shopping.PriceBreakDown;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.MSCPayment.Interfaces;
using United.Mobile.DataAccess.Product.Interfaces;
using United.Mobile.Model.Common;
using United.Persist.Definition.FOP;
using United.Persist.Definition.Shopping;
using United.Service.Presentation.CommonModel;
using United.Service.Presentation.PaymentModel;
using United.Service.Presentation.PaymentRequestModel;
using United.Service.Presentation.PaymentResponseModel;
using United.Services.FlightShopping.Common.DisplayCart;
using United.Services.FlightShopping.Common.Extensions;
using United.Services.FlightShopping.Common.FlightReservation;
using United.Utility.Helper;
using ETCCertificates = United.Service.Presentation.PaymentModel.ETCCertificates;
using FFCRCertificate = United.Service.Presentation.PaymentModel.FFCRCertificate;
using FFCRCertificates = United.Service.Presentation.PaymentModel.FFCRCertificates;
using LookupByEmailRequest = United.Definition.FFC.LookupByEmailRequest;
using OTFConversion = United.Refunds.Common.Models;
using PaxDetail = United.Definition.FFC.PaxDetail;
using ProfileResponse = United.Persist.Definition.Shopping.ProfileResponse;
using Task = System.Threading.Tasks.Task;
using TravelCreditDetail = United.Definition.FFC.TravelCreditDetail;
using United.ShoppingCart.Model.ManagePayment;

namespace United.Mobile.TravelCredit.Domain
{
    public class TravelCreditBusiness : ITravelCreditBusiness
    {
        private readonly ICacheLog<TravelCreditBusiness> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHeaders _headers;
        private readonly ISessionHelperService _sessionHelperService;
        private readonly IETCUtility _ETCUtility;
        private readonly IDynamoDBService _dynamoDBService;
        private readonly IDPService _dPService;
        private readonly IPaymentService _paymentService;
        private readonly IOTFConversionService _oTFConversionService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IMSCFormsOfPayment _mscFormsOfPayment;
        private readonly IMSCPkDispenserPublicKey _mSCPkDispenserPublicKey;
        private readonly ICMSContentService _cMSContentService;
        private readonly IMSCShoppingSessionHelper _mSCShoppingSessionHelper;
        private readonly ICachingService _cachingService;
        private readonly IFeatureSettings _featureSettings;
        private readonly IShoppingCartUtility _shoppingCartUtility;

        private readonly string _MSG1 = "MSG1";
        private readonly string _ERR2 = "ERR2";
        private readonly string _ERR3 = "ERR3";
        private readonly string _ERR4 = "Err4";
        private readonly string _ERR5 = "ERR5";

        public TravelCreditBusiness(ICacheLog<TravelCreditBusiness> logger
            , IConfiguration configuration
            , IHeaders headers
            , ISessionHelperService sessionHelperService
            , IDynamoDBService dynamoDBService
            , IETCUtility ETCUtility
            , IDPService dPService
            , IPaymentService paymentService
            , IOTFConversionService oTFConversionService
            , IShoppingCartService shoppingCartService
            , IMSCFormsOfPayment mscFormsOfPayment
            , IMSCPkDispenserPublicKey mSCPkDispenserPublicKey
            , IMSCShoppingSessionHelper mSCShoppingSessionHelper
            , ICachingService cachingService
            , ICMSContentService cMSContentService
            , IFeatureSettings featureSettings
           , IShoppingCartUtility shoppingCartUtility)
        {
            _logger = logger;
            _configuration = configuration;
            _headers = headers;
            _sessionHelperService = sessionHelperService;
            _dynamoDBService = dynamoDBService;
            _ETCUtility = ETCUtility;
            _dPService = dPService;
            _paymentService = paymentService;
            _oTFConversionService = oTFConversionService;
            _shoppingCartService = shoppingCartService;
            _mscFormsOfPayment = mscFormsOfPayment;
            _mSCPkDispenserPublicKey = mSCPkDispenserPublicKey;
            _mSCShoppingSessionHelper = mSCShoppingSessionHelper;
            _cachingService = cachingService;
            _cMSContentService = cMSContentService;
            _featureSettings = featureSettings;
            _shoppingCartUtility = shoppingCartUtility;
        }

        public async Task<MOBFutureFlightCreditResponse> FutureFlightCredit(MOBFutureFlightCreditRequest request)
        {

            var response = new MOBFutureFlightCreditResponse();

            _logger.LogInformation("FutureFlightCredit Request{request} with SessionId{sessoinId}", JsonConvert.SerializeObject(request), request.SessionId);

            Session session = await _mSCShoppingSessionHelper.GetBookingFlowSession(request.SessionId);

            if (session == null)
            {
                throw new MOBUnitedException("Could not find your booking session.");
            }

            if (!string.IsNullOrEmpty(request.RecordLocator) && !string.IsNullOrEmpty(request.LastName))
            {
                request.RecordLocator = request.RecordLocator.Trim();
                request.LastName = request.LastName.Trim();
            }

            response = await ManageFutureFlightCredit(request, session);

            if (!string.IsNullOrEmpty(request?.MileagePlusNumber) && !string.IsNullOrEmpty(request?.HashPinCode))
            {
                if (response?.Captions != null && response.Captions.Any
                    (x => string.Equals(x.Key, "90102_ResDetail_RedirectUrl", StringComparison.OrdinalIgnoreCase)))
                {
                    response.WebSessionShareUrl = _configuration.GetValue<string>("DotcomSSOUrl");
                    response.WebShareToken = await GetSSOToken(request.MileagePlusNumber, request.HashPinCode,
                        request.SessionId, (MOBRequest)request).ConfigureAwait(false);

                    if (await _featureSettings.GetFeatureSettingValue("EnableRedirectURLUpdate").ConfigureAwait(false))
                    {
                        response.Captions.Where(x => string.Equals(x.Key, "90102_ResDetail_RedirectUrl", StringComparison.OrdinalIgnoreCase))
                            .ForEach(x => x.Value = $"{_configuration.GetValue<string>("NewDotcomSSOUrl")}?type=sso&token={response.WebShareToken}&landingUrl={x.Value}");
                        response.WebSessionShareUrl = response.WebShareToken = string.Empty;
                    }
                }
            }

            bool isDefault = false;
            var tupleRespEligibleFormofPayments
                = await _mscFormsOfPayment.GetEligibleFormofPayments(request, session, response.ShoppingCart,
                response.ShoppingCart.CartId, request.Flow, response.Reservation);
            response.EligibleFormofPayments = tupleRespEligibleFormofPayments.EligibleFormofPayments;
            isDefault = tupleRespEligibleFormofPayments.isDefault;

            await _sessionHelperService.SaveSession<MOBFutureFlightCreditResponse>(response, session.SessionId, new List<string> { session.SessionId, response.ObjectName }, response.ObjectName);

            return await Task.FromResult(response);
        }

        public async Task<MOBFutureFlightCreditResponse> RemoveFutureFlightCredit(MOBFOPFutureFlightCreditRequest request)
        {
            var response = new MOBFutureFlightCreditResponse();

            _logger.LogInformation("RemoveFutureFlightCredit Request{request} with SessionId{sessoinId}", JsonConvert.SerializeObject(request), request.SessionId);

            Session session = await _mSCShoppingSessionHelper.GetBookingFlowSession(request.SessionId);

            if (session == null)
            {
                throw new MOBUnitedException("Could not find your booking session.");
            }

            response = await RemoveFutureFlightCredit(request, session);
            response.SessionId = request.SessionId;
            response.Flow = request.Flow;
            bool isDefault = false;
            var tupleRespEligibleFormofPayments = await _mscFormsOfPayment.GetEligibleFormofPayments(request, session, response.ShoppingCart, request.CartId, request.Flow, response.Reservation);
            response.EligibleFormofPayments = tupleRespEligibleFormofPayments.EligibleFormofPayments;
            isDefault = tupleRespEligibleFormofPayments.isDefault;
            await _sessionHelperService.SaveSession(response, session.SessionId, new List<string> { session.SessionId, response.ObjectName }, response.ObjectName);

            return await Task.FromResult(response);
        }

        public async Task<MOBFOPResponse> LookUpTravelCredit(MOBFOPLookUpTravelCreditRequest request)
        {
            var response = new MOBFOPResponse();

            #region
            Session session = await _mSCShoppingSessionHelper.GetBookingFlowSession(request.SessionId);
            if (session == null)
            {
                throw new MOBUnitedException("Could not find your booking session.");
            }

            response = await LookUpTravelCredit(session, request);
            bool isDefault = false;

            var tupleRespEligibleFormofPayments = await _mscFormsOfPayment.GetEligibleFormofPayments(request, session, response.ShoppingCart, request.CartId, request.Flow, response.Reservation);
            response.EligibleFormofPayments = tupleRespEligibleFormofPayments.EligibleFormofPayments;
            isDefault = tupleRespEligibleFormofPayments.isDefault;
            response.SessionId = request.SessionId;
            response.Flow = request.Flow;
            response.PkDispenserPublicKey = await _mSCPkDispenserPublicKey.GetCachedOrNewpkDispenserPublicKey(request.Application.Id, request.Application.Version.Major, request.DeviceId, session.SessionId, session.Token, session?.CatalogItems).ConfigureAwait(false);

            await _sessionHelperService.SaveSession<MOBFOPResponse>(response, session.SessionId, new List<string> { session.SessionId, response.ObjectName }, response.ObjectName);

            return await Task.FromResult(response);
            #endregion
        }

        public async Task<MOBFOPResponse> ManageTravelCredit(MOBFOPManageTravelCreditRequest request)
        {
            var response = new MOBFOPResponse();

            #region
            Session session = await _mSCShoppingSessionHelper.GetBookingFlowSession(request.SessionId);
            if (session == null)
            {
                throw new MOBUnitedException("Could not find your booking session.");
            }

            response = await ManageTravelCredit(session, request);
            bool isDefault = false;

            //request doesn't have cartID
            var tupleRespEligibleFormofPayments = await _mscFormsOfPayment.GetEligibleFormofPayments(request, session, response.ShoppingCart, response.ShoppingCart.CartId, request.Flow, response.Reservation);
            response.EligibleFormofPayments = tupleRespEligibleFormofPayments.EligibleFormofPayments;
            isDefault = tupleRespEligibleFormofPayments.isDefault;
            response.PkDispenserPublicKey = await _mSCPkDispenserPublicKey.GetCachedOrNewpkDispenserPublicKey(request.Application.Id, request.Application.Version.Major, request.DeviceId, session.SessionId, session.Token, session?.CatalogItems).ConfigureAwait(false);
            response.SessionId = request.SessionId;
            response.Flow = request.Flow;


            await _sessionHelperService.SaveSession<MOBFOPResponse>(response, session.SessionId, new List<string> { session.SessionId, response.ObjectName }, response.ObjectName);

            return await Task.FromResult(response);
            #endregion
        }

        private async Task<MOBFOPResponse> ManageTravelCredit(Session session, MOBFOPManageTravelCreditRequest request)
        {
            if (ConfigUtility.EnableMFOP(request.Application.Id, request.Application.Version.Major))
                return await ApplyOrRemoveMFOP(session, request);
            else
                return await ApplyOrRemove(session, request);
        }

        private async Task<MOBFOPResponse> ApplyOrRemoveMFOP(Session session, MOBFOPManageTravelCreditRequest request)
        {
            Reservation bookingPathReservation = new Reservation();

            var travelCreditBusinessResult = await LoadBasicMOBFOPResponse(session, bookingPathReservation);
            var response = travelCreditBusinessResult.MobFopResponse;

            bookingPathReservation = travelCreditBusinessResult.BookingPathReservation;
            request.TravelCredit.IsApplied = !request.TravelCredit.IsRemove;

            var paymentRequest = new ManagePaymentRequest();
            if (request.TravelCredit.TravelCreditType == MOBTravelCreditFOP.ETC)
                paymentRequest = GetETCPaymentRequest(request, response);
            else
                paymentRequest = GetFFCFFCRPaymentRequest(request, response);

            var _manageResponse = new ManagePaymentResponse();
            if (!request.TravelCredit.IsApplied)
            {
                if ((request?.TravelCredit?.OperationID != null))
                    _manageResponse = await DeletePaymentResponse(paymentRequest, session, $"/api/payments?cartid={session.CartId}&PaymentId={request?.TravelCredit?.OperationID}");
                else
                    throw new MOBUnitedException("OperationID is empty");
            }
            else
                _manageResponse = await GetPaymentResponse(paymentRequest, session, "/api/payments");

            return await UpdateReservationAndShoppingcart(session, request, bookingPathReservation, response, request.TravelCredit, _manageResponse, paymentRequest);

        }

        private async Task<MOBFOPResponse> ApplyOrRemove(Session session, MOBFOPManageTravelCreditRequest request)
        {
            Reservation bookingPathReservation = new Reservation();

            var travelCreditBusinessResult = await LoadBasicMOBFOPResponse(session, bookingPathReservation);
            var response = travelCreditBusinessResult.MobFopResponse;

            bookingPathReservation = travelCreditBusinessResult.BookingPathReservation;
            request.TravelCredit.IsApplied = !request.TravelCredit.IsRemove;
            bool isAncillaryFFCEnable = IsInclueWithThisToggle(request.Application.Id, request.Application.Version.Major, "EnableTravelCreditAncillary", "AndroidTravelCreditVersionAncillary", "iPhoneTravelCreditVersionAncillary");
            if (isAncillaryFFCEnable)
            {
                response.ShoppingCart.FormofPaymentDetails.TravelFutureFlightCredit.AllowedFFCAmount = GetAllowedFFCAmount(response.ShoppingCart.Products, isAncillaryFFCEnable);
                response.ShoppingCart.FormofPaymentDetails.TravelFutureFlightCredit.AllowedAncillaryAmount = GetAncillaryAmount(response.ShoppingCart.Products, isAncillaryFFCEnable);
            }

            await AddTCToETCOrFFC(session, request, bookingPathReservation, response, request.TravelCredit, isAncillaryFFCEnable);

            var travelCredits = response.ShoppingCart.FormofPaymentDetails.TravelCreditDetails.TravelCredits;

            await UpdateTCPriceAndFOPType(response.Reservation.Prices, response.ShoppingCart.FormofPaymentDetails, request.Application, response.ShoppingCart.Products, response.ShoppingCart.SCTravelers, isManageTravelCreditApi: true);

            if (IsEnableBundleLiveUpdateChanges(request.Application.Id, request.Application.Version.Major, response.Reservation?.ShopReservationInfo2?.IsDisplayCart == true))
            {
                BuildOmniCart(response.ShoppingCart, response.Reservation, request.Application);
            }

            var lstMessages = await GetSDLContentByGroupName(request, session.SessionId, session.Token, _configuration.GetValue<string>("CMSContentMessages_GroupName_BookingRTI_Messages"), "BookingPathRTI_CMSContentMessagesCached_StaticGUID");
            response.ShoppingCart.FormofPaymentDetails.TravelCreditDetails.AlertMessages = BuildReviewFFCHeaderMessage(response.ShoppingCart.FormofPaymentDetails?.TravelFutureFlightCredit, response.Reservation.TravelersCSL, lstMessages);

            bookingPathReservation.Prices = response.Reservation.Prices;

            await _sessionHelperService.SaveSession<Reservation>(bookingPathReservation, session.SessionId, new List<string> { session.SessionId, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName).ConfigureAwait(false);

            await _sessionHelperService.SaveSession<MOBShoppingCart>(response.ShoppingCart, session.SessionId, new List<string> { session.SessionId, response.ShoppingCart.ObjectName }, response.ShoppingCart.ObjectName).ConfigureAwait(false);

            return response;
        }

        #region MFOP
        private ManagePaymentRequest GetFFCFFCRPaymentRequest(MOBFOPManageTravelCreditRequest request, MOBFOPResponse response)
        {
            // Use FFC and FFCR under one method and do a seperate request for ETC
            ManagePaymentRequest _managerequest = new ManagePaymentRequest();
            if (request?.TravelCredit == null)
            {
                return _managerequest;
            }
            MOBFOPTravelCredit _tc = request.TravelCredit;
            var dob = "01/01/0001";
            try
            {
                var _travelers = response?.Reservation?.TravelersCSL;
                var _dob = _travelers.Where(x => ($"{x.FirstName.ToUpper() + x.MiddleName.ToUpper()} {x.LastName.ToUpper()}").Replace(" ", "") == request.TravelCredit.Recipient.ToUpper().Replace(" ", ""))?.FirstOrDefault();
                if (_dob != null)
                    dob = _dob.BirthDate;
                else
                    dob = _travelers.Where(x => ($"{x.FirstName.ToUpper()} {x.LastName.ToUpper()}").Replace(" ", "") == request.TravelCredit.Recipient.ToUpper().Replace(" ", ""))?.FirstOrDefault().BirthDate;
            }
            catch (Exception ex) { }
            United.Service.Presentation.PaymentModel.Certificate certificate = new Certificate()
            {
                ConfirmationId = _tc.RecordLocator,
                InitialValue = _tc.CurrentValue, // We have to send the current credit amount not the intial 
                CurrentValue = _tc.CurrentValue,
                ExpirationDate = Convert.ToDateTime(_tc.ExpiryDate).ToString("MM/dd/yyyy"),
                OperationID = Guid.NewGuid().ToString(),
                Type = new Genre()
                {
                    Key = "FFCT"// _tc.TravelCreditType.ToString()
                },
                Payor = new Service.Presentation.PersonModel.Person()
                {
                    DateOfBirth = Convert.ToDateTime(dob).ToString("MM/dd/yyyy"),
                    ReservationIndex = _tc.TravelerNameIndex.Replace(",", "")
                },
                WalletCategory = Service.Presentation.CommonEnumModel.WalletCategory.None,
                GroupPaymentType = Service.Presentation.CommonEnumModel.GroupPaymentType.None,
                IsNameMatchOverride = _tc.IsNameMatchWaiverApplied && _tc.IsEligibleToRedeem             
            };

            if (request.TravelCredit.TravelCreditType == MOBTravelCreditFOP.FFC && ( _tc.PromoCode == null))
            {
                certificate.FutureFlightTraveler = new FutureFlightTraveler()
                {
                    DateOfBirth = Convert.ToDateTime(dob).ToString("MM/dd/yyyy"),
                    Key = _tc.TravelerNameIndex.Replace(",", ""),
                    LastName = _tc.LastName,
                    FirstName = _tc.FirstName,
                    TravelerCredits = new Collection<FutureFlightTravelerCredit>()
                    {
                        new FutureFlightTravelerCredit(){
                            CreditAmount= new Charge()
                            {
                                Amount= _tc.InitialValue,
                                Currency=new Currency(){ Code="USD"}
                            },
                            ExpirationDate=Convert.ToDateTime(_tc.ExpiryDate).ToString("MM/dd/yyyy"),
                            IsEligibleToRedeem=_tc.IsEligibleToRedeem.ToString(),
                            IsNameMatch=_tc.IsNameMatch.ToString(),
                            IsNameMatchWaiverApplied = request.TravelCredit.IsNameMatchWaiverApplied.ToString(),
                            IsTravelDateBeginsBeforeCertExpiry=_tc.IsTravelDateBeginsBeforeCertExpiry.ToString(),
                            TicketNumber=_tc.PinCode,
                            OfferKey= _tc.CertificateNumber,
                            TravelCreditType=_tc.CsltravelCreditType,
                        }
                    }
                };
            }

            if (request.TravelCredit.TravelCreditType == MOBTravelCreditFOP.FFCR || _tc.PromoCode != null)
            {
                certificate.PinCode = _tc.PinCode;
                certificate.PromoCode = _tc.PromoCode;
            }

            // isNameMatchWaiverApplied true remove the payor details 
            if (_tc.IsNameMatchWaiverApplied && _tc.IsEligibleToRedeem)
            {
                certificate.Payor = null;
            }
            _managerequest.FormOfPayment = new United.Service.Presentation.PaymentModel.FormOfPayment();
            _managerequest.FormOfPayment.Certificate = certificate;
            _managerequest.CartId = response.ShoppingCart.CartId;
            _managerequest.WorkFlowType = WorkFlowType.InitialBooking; // testing for booking have to update to actual flow if (response.Flow == "BOOKING") 
            return _managerequest;
        }

        private ManagePaymentRequest GetETCPaymentRequest(MOBFOPManageTravelCreditRequest request, MOBFOPResponse response)
        {
            ManagePaymentRequest _managerequest = new ManagePaymentRequest();
            MOBFOPTravelCredit _tc = request.TravelCredit;
            var certificate = new Certificate()
            {
                PromoCode = _tc.PromoCode,
                PinCode = _tc.PinCode,
                InitialValue = _tc.CurrentValue,
                CurrentValue = _tc.CurrentValue,
                ExpirationDate = Convert.ToDateTime(_tc.ExpiryDate).ToString("MM/dd/yyyy"),
                OperationID = Guid.NewGuid().ToString(),
                Type = new Genre()
                {
                    Key = _tc.TravelCreditType.ToString()
                },
                WalletCategory = Service.Presentation.CommonEnumModel.WalletCategory.None,
                GroupPaymentType = Service.Presentation.CommonEnumModel.GroupPaymentType.None,
                Payor = new Service.Presentation.PersonModel.Person() { PreferredName = _tc.LastName }
            };
            _managerequest.FormOfPayment = new United.Service.Presentation.PaymentModel.FormOfPayment();
            _managerequest.FormOfPayment.Certificate = certificate;
            _managerequest.CartId = response.ShoppingCart.CartId;
            _managerequest.WorkFlowType = WorkFlowType.InitialBooking; // testing for booking have to update to actual flow if (response.Flow == "BOOKING") 
            return _managerequest;
        }

        private async Task<ManagePaymentResponse> GetPaymentResponse(ManagePaymentRequest request, Session session, string path)
        {
            ManagePaymentResponse response = new ManagePaymentResponse();
            var jsonResponse = await _shoppingCartService.ShoppingCartServiceCall(session.Token, path, JsonConvert.SerializeObject(request), session.SessionId);
            if (!string.IsNullOrEmpty(jsonResponse))
            {
                response = JsonConvert.DeserializeObject<ManagePaymentResponse>(jsonResponse);
            }
            return response;
        }
        private async Task<ManagePaymentResponse> DeletePaymentResponse(ManagePaymentRequest request, Session session, string path)
        {
            ManagePaymentResponse response = new ManagePaymentResponse();
            var jsonResponse = await _shoppingCartService.DeletePayment(session.Token, path, session.SessionId);
            if (!string.IsNullOrEmpty(jsonResponse))
            {
                response = JsonConvert.DeserializeObject<ManagePaymentResponse>(jsonResponse);
            }
            return response;
        }

        private async Task<MOBFOPResponse> UpdateReservationAndShoppingcart(Session session, MOBRequest request, Reservation bookingPathReservation, MOBFOPResponse response, MOBFOPTravelCredit selectedTC, ManagePaymentResponse manageResponse, ManagePaymentRequest manageRequest)
        {
            var lstMessages = await GetSDLContentByGroupName(request, session.SessionId, session.Token, _configuration.GetValue<string>("CMSContentMessages_GroupName_BookingRTI_Messages"), "BookingPathRTI_CMSContentMessagesCached_StaticGUID");

            var bookingPathReservationV2 = bookingPathReservation;

            var grandTotal = Convert.ToDouble(manageResponse?.Products.Sum(x => x.RemainingDue));
            var totalAmount = Convert.ToDouble(manageResponse?.Products.Sum(x => x.Totals?.TotalAmount));

            var certificateAmount = Math.Round(totalAmount - grandTotal, 2, MidpointRounding.AwayFromZero);
            //var certificateRemainingAmount = 0.00;
            var _etcAmount = 0.00;
            var _ffcAmount = 0.00;
            var _etcRemainingAmount = 0.00;
            var _ffcRemainingAmount = 0.00;

            //if (manageResponse?.CreditDetails?.AppliedPaymentInfos != null)
            //    certificateRemainingAmount = Math.Round(Convert.ToDouble(manageResponse?.CreditDetails?.AppliedPaymentInfos.Sum(x => x.CurrentValue)), 2, MidpointRounding.AwayFromZero);

            if (certificateAmount > 0 && manageResponse?.PaymentsApplied.Count() > 0)
            {
                _etcAmount = (from data in (manageResponse?.CreditDetails.AppliedPaymentInfos) where (manageResponse?.PaymentsApplied.Any(x => data.PaymentId.Contains(x.Id) && x.Type == MOBTravelCreditFOP.ETC.ToString()) == true) && (data.InitialValue != data.CurrentValue) select data.InitialValue - data.CurrentValue).Sum();
                _ffcAmount = (from data in (manageResponse?.CreditDetails.AppliedPaymentInfos) where (manageResponse?.PaymentsApplied.Any(x => data.PaymentId.Contains(x.Id) && x.Type != MOBTravelCreditFOP.ETC.ToString()) == true) && (data.InitialValue != data.CurrentValue) select data.InitialValue - data.CurrentValue).Sum();
                _etcRemainingAmount = (from data in (manageResponse?.CreditDetails.AppliedPaymentInfos) where (manageResponse?.PaymentsApplied.Any(x => data.PaymentId.Contains(x.Id) && x.Type == MOBTravelCreditFOP.ETC.ToString()) == true) && (data.CurrentValue != 0) select data.CurrentValue).Sum();
                _ffcRemainingAmount = (from data in (manageResponse?.CreditDetails.AppliedPaymentInfos) where (manageResponse?.PaymentsApplied.Any(x => data.PaymentId.Contains(x.Id) && x.Type != MOBTravelCreditFOP.ETC.ToString()) == true) && (data.CurrentValue != 0) select data.CurrentValue).Sum();
            }

            CultureInfo ci = TopHelper.GetCultureInfo(manageResponse.Products.FirstOrDefault().Totals.AmountCurrencyCode);

            #region Reservation Update

            var totalUpdate = new string[] { "GRAND TOTAL" };
            bookingPathReservationV2.Prices.Where(x => totalUpdate.Contains(x.DisplayType.ToUpper())).ToList().
                ForEach(listItem =>
                {
                    listItem.DisplayValue = string.Format("{0:#,0.00}", (grandTotal));
                    listItem.FormattedDisplayValue = TopHelper.FormatAmountForDisplay(Convert.ToDecimal(grandTotal), ci, false);
                    listItem.Value = grandTotal;
                });

            string[] priceTypeArray = { "CERTIFICATE", "FFC" };
            foreach (var _priceType in priceTypeArray)
            {
                var _amount = _priceType == "CERTIFICATE" ? _etcAmount : _ffcAmount;
                bookingPathReservationV2.Prices.RemoveAll(p => p.DisplayType.ToUpper() == _priceType.ToUpper());
                if (_amount > 0)
                {
                    var _title = _priceType == "CERTIFICATE" ? "Travel certificate" : "Future flight credit";
                    UpdateReservationCertificatePrices(bookingPathReservationV2, _title, _amount, _priceType);
                }
            }

            string[] refundType = { //$"REFUNDPRICE|Total credit|{certificateRemainingAmount.ToString()}" , // No Refund Price for MFOP
                                    $"REFUNDCERTIFICATEPRICE|Travel certificate|{_etcRemainingAmount.ToString()}" ,
                                    $"REFUNDFFCPRICE|Future flight credit|{_ffcRemainingAmount.ToString()}"
            };
            foreach (var type in refundType)
            {
                var data = type.Split('|');
                bookingPathReservationV2.Prices.RemoveAll(p => p.PriceType.ToUpper() == data[0]);
                if (Convert.ToDouble(data[2]) > 0)
                {
                    var priceTypeDescription = data[1];
                    var price = new MOBSHOPPrice();
                    bookingPathReservationV2.Prices.Add(price);
                    UpdateCertificatePrice(price, Convert.ToDouble(data[2]), data[0], data[1], "RESIDUALCREDIT");
                }
            }

            response.Reservation.Prices = bookingPathReservationV2.Prices;
            #endregion

            #region ShoppingCart Update
            var _shoppingCart = response.ShoppingCart;
            if (_shoppingCart.FormofPaymentDetails.TravelFutureFlightCredit == null)
                _shoppingCart.FormofPaymentDetails.TravelFutureFlightCredit = new MOBFOPTravelFutureFlightCredit();
            if (_shoppingCart.FormofPaymentDetails.TravelCertificate == null)
                _shoppingCart.FormofPaymentDetails.TravelCertificate = new MOBFOPTravelCertificate();

            var _traveRes = manageResponse?.CreditDetails?.AppliedPaymentInfos.ToList().Where(x => x.PaymentId == manageRequest.FormOfPayment.Certificate.OperationID).ToList();
            _shoppingCart.FormofPaymentDetails.TravelCreditDetails.TravelCredits.Where(s => s.PinCode == selectedTC.PinCode).FirstOrDefault().OperationID = !selectedTC.IsRemove ? manageRequest.FormOfPayment.Certificate.OperationID : null;
            if (!selectedTC.IsApplied && _traveRes.IsListNullOrEmpty())
            {
                selectedTC.RedeemAmount = Math.Round(selectedTC.RedeemAmount, 2, MidpointRounding.AwayFromZero);
                _traveRes = new List<AppliedPaymentInfo> { new AppliedPaymentInfo { CurrentValue = selectedTC.CurrentValue + selectedTC.RedeemAmount, InitialValue = selectedTC.InitialValue, PaymentId = null, Pin = selectedTC.PinCode, Promo = selectedTC.PromoCode } };
                _shoppingCart.FormofPaymentDetails.TravelCreditDetails.TravelCredits.Where(s => s.PinCode == selectedTC.PinCode).FirstOrDefault().CurrentValue = Convert.ToDouble(_traveRes.FirstOrDefault().CurrentValue);
                _shoppingCart.FormofPaymentDetails.TravelCreditDetails.TravelCredits.Where(s => s.PinCode == selectedTC.PinCode).FirstOrDefault().InitialValue = _traveRes.FirstOrDefault().InitialValue;
                _shoppingCart.FormofPaymentDetails.TravelCreditDetails.TravelCredits.Where(s => s.PinCode == selectedTC.PinCode).FirstOrDefault().NewValueAfterRedeem = _traveRes.FirstOrDefault().CurrentValue;
                _shoppingCart.FormofPaymentDetails.TravelCreditDetails.TravelCredits.Where(s => s.PinCode == selectedTC.PinCode).FirstOrDefault().DisplayNewValueAfterRedeem = $"${string.Format("{0:#,0.00}", _traveRes.FirstOrDefault().CurrentValue)}";
                _shoppingCart.FormofPaymentDetails.TravelCreditDetails.TravelCredits.Where(s => s.PinCode == selectedTC.PinCode).FirstOrDefault().DisplayRedeemAmount = $"${string.Format("{0:#,0.00}", (selectedTC.NewValueAfterRedeem + selectedTC.RedeemAmount) - _traveRes.FirstOrDefault().CurrentValue)}";
                _shoppingCart.FormofPaymentDetails.TravelCreditDetails.TravelCredits.Where(s => s.PinCode == selectedTC.PinCode).FirstOrDefault().RedeemAmount = (selectedTC.RedeemAmount + selectedTC.NewValueAfterRedeem) - _traveRes.FirstOrDefault().CurrentValue;
                _shoppingCart.FormofPaymentDetails.TravelCreditDetails.TravelCredits.Where(s => s.PinCode == selectedTC.PinCode).FirstOrDefault().IsApplied = !selectedTC.IsRemove;
                _shoppingCart.FormofPaymentDetails.TravelCreditDetails.TravelCredits.Where(s => s.PinCode == selectedTC.PinCode).FirstOrDefault().OperationID = "";
                await UpdateCertificate(response, _shoppingCart, selectedTC, manageResponse, manageRequest, response.Reservation, session).ConfigureAwait(false);
            }
            //Update Certificates every time as we will not have any control on which certificate is updated.
            if (manageResponse?.CreditDetails?.AppliedPaymentInfos != null)
            {
                foreach (var applied in manageResponse?.CreditDetails?.AppliedPaymentInfos)
                {
                    if (_shoppingCart.FormofPaymentDetails.TravelCreditDetails.TravelCredits.Where(s => s.OperationID == applied.PaymentId).FirstOrDefault() != null)
                    {
                        _shoppingCart.FormofPaymentDetails.TravelCreditDetails.TravelCredits.Where(s => s.OperationID == applied.PaymentId).FirstOrDefault().CurrentValue = Convert.ToDouble(applied.CurrentValue);
                        _shoppingCart.FormofPaymentDetails.TravelCreditDetails.TravelCredits.Where(s => s.OperationID == applied.PaymentId).FirstOrDefault().InitialValue = applied.InitialValue;
                        _shoppingCart.FormofPaymentDetails.TravelCreditDetails.TravelCredits.Where(s => s.OperationID == applied.PaymentId).FirstOrDefault().NewValueAfterRedeem = applied.CurrentValue;
                        _shoppingCart.FormofPaymentDetails.TravelCreditDetails.TravelCredits.Where(s => s.OperationID == applied.PaymentId).FirstOrDefault().DisplayNewValueAfterRedeem = $"${string.Format("{0:#,0.00}", applied.CurrentValue)}";
                        _shoppingCart.FormofPaymentDetails.TravelCreditDetails.TravelCredits.Where(s => s.OperationID == applied.PaymentId).FirstOrDefault().DisplayRedeemAmount = $"${string.Format("{0:#,0.00}", applied.InitialValue - applied.CurrentValue)}";
                        _shoppingCart.FormofPaymentDetails.TravelCreditDetails.TravelCredits.Where(s => s.OperationID == applied.PaymentId).FirstOrDefault().RedeemAmount = applied.InitialValue - applied.CurrentValue;
                        _shoppingCart.FormofPaymentDetails.TravelCreditDetails.TravelCredits.Where(s => s.OperationID == applied.PaymentId).FirstOrDefault().IsApplied = (applied.CurrentValue == applied.InitialValue) ? false : true;
                        await UpdateCertificate(response, _shoppingCart, _shoppingCart.FormofPaymentDetails.TravelCreditDetails.TravelCredits.Where(s => s.OperationID == applied.PaymentId).FirstOrDefault(), manageResponse, manageRequest, response.Reservation, session).ConfigureAwait(false);
                    }
                }
            }

            if (_shoppingCart?.FormofPaymentDetails?.TravelFutureFlightCredit?.FutureFlightCredits != null && _shoppingCart?.FormofPaymentDetails?.TravelFutureFlightCredit?.FutureFlightCredits.Count > 0)
                _shoppingCart.FormofPaymentDetails.TravelFutureFlightCredit.FutureFlightCredits.RemoveAll(f => f.RedeemAmount <= 0);
            else
            {
                response.Reservation.TravelersCSL.ForEach(listItem =>
                {
                    listItem.TotalFFCRedeemAmount = null;
                    listItem.DisplayTotalFFCRedeemAmount = "";
                });
            }

            //Ends
            _shoppingCart.FormofPaymentDetails.IsOtherFOPRequired = (grandTotal > 0);
            _shoppingCart.FormofPaymentDetails.TravelFutureFlightCredit.ReviewFFCMessages = BuildReviewFFCHeaderMessage(response.ShoppingCart.FormofPaymentDetails?.TravelFutureFlightCredit, response.Reservation.TravelersCSL, lstMessages);
            _shoppingCart.FormofPaymentDetails.TravelCertificate.EmailConfirmationTCMessages = _etcAmount > 0 ? AssignEmailMessageForTCRefund(lstMessages, response.Reservation.Prices, response.ShoppingCart.FormofPaymentDetails.EmailAddress, response.ShoppingCart.FormofPaymentDetails.TravelCertificate, request.Application) : null;
            _shoppingCart.FormofPaymentDetails.TravelFutureFlightCredit.EmailConfirmationFFCMessages = AssignEmailMessageForFFCRefund(lstMessages, response.Reservation.Prices, response.ShoppingCart.FormofPaymentDetails.EmailAddress, response.ShoppingCart.FormofPaymentDetails.TravelFutureFlightCredit, request.Application);
            _shoppingCart.SCTravelers = (response.Reservation.TravelersCSL != null && response.Reservation.TravelersCSL.Count() > 0) ? response.Reservation.TravelersCSL : null;
            _shoppingCart.FormofPaymentDetails.TravelCreditDetails.AlertMessages = UpdateAlertMessage(manageResponse, selectedTC, lstMessages, _shoppingCart.SCTravelers);

            if ((selectedTC.TravelCreditType == MOBTravelCreditFOP.ETC || selectedTC.TravelCreditType == MOBTravelCreditFOP.FFC || selectedTC.TravelCreditType == MOBTravelCreditFOP.FFCR) && grandTotal == 0.00)
                _shoppingCart.FormofPaymentDetails.FormOfPaymentType = "TC";
            else
                _shoppingCart.FormofPaymentDetails.FormOfPaymentType = MOBFormofPayment.CreditCard.ToString();

            response.ShoppingCart = _shoppingCart;
            #endregion     

            if (IsEnableBundleLiveUpdateChanges(request.Application.Id, request.Application.Version.Major, response.Reservation?.ShopReservationInfo2?.IsDisplayCart == true))
            {
                BuildOmniCart(response.ShoppingCart, response.Reservation, request.Application);
            }
            bookingPathReservationV2.Prices = response.Reservation.Prices;

            if (_shoppingCart?.FormofPaymentDetails?.TravelCreditDetails?.TravelCredits != null)
            {
                var persistedTravelCertifcateResponse = await _sessionHelperService.GetSession<MOBFOPTravelerCertificateResponse>(response.Reservation.SessionId, new MOBFOPTravelerCertificateResponse().ObjectName, new List<string> { response.Reservation.SessionId, new MOBFOPTravelerCertificateResponse().ObjectName }).ConfigureAwait(false);
                persistedTravelCertifcateResponse.ShoppingCart = _shoppingCart;
                persistedTravelCertifcateResponse.Reservation = response.Reservation;
                await _sessionHelperService.SaveSession<MOBFOPTravelerCertificateResponse>(persistedTravelCertifcateResponse, session.SessionId, new List<string> { session.SessionId, persistedTravelCertifcateResponse.ObjectName }, persistedTravelCertifcateResponse.ObjectName).ConfigureAwait(false);
            }
            await _sessionHelperService.SaveSession<Reservation>(bookingPathReservationV2, session.SessionId, new List<string> { session.SessionId, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName).ConfigureAwait(false);

            await _sessionHelperService.SaveSession<MOBSHOPReservation>(response.Reservation, session.SessionId, new List<string> { session.SessionId, "United.Definition.MOBSHOPReservation" }, "United.Definition.MOBSHOPReservation").ConfigureAwait(false);

            await _sessionHelperService.SaveSession<MOBShoppingCart>(response.ShoppingCart, session.SessionId, new List<string> { session.SessionId, response.ShoppingCart.ObjectName }, response.ShoppingCart.ObjectName).ConfigureAwait(false);

            return response;
        }
        private void UpdateReservationCertificatePrices(Reservation bookingPathReservation, string title, double _certificateAmount, string priceType)
        {
            MOBSHOPPrice certificatePrice = new MOBSHOPPrice();
            certificatePrice.CurrencyCode = "USD";
            certificatePrice.DisplayType = priceType;
            certificatePrice.PriceType = priceType;
            certificatePrice.PriceTypeDescription = title;
            certificatePrice.Value = Convert.ToDouble(_certificateAmount);
            certificatePrice.Value = Math.Round(certificatePrice.Value, 2, MidpointRounding.AwayFromZero);
            certificatePrice.FormattedDisplayValue = $"-{(certificatePrice.Value).ToString("C2", CultureInfo.CurrentCulture)}";
            certificatePrice.DisplayValue = string.Format("{0:#,0.00}", certificatePrice.Value);
            bookingPathReservation.Prices.Add(certificatePrice);
        }
      
        private List<MOBMobileCMSContentMessages> UpdateAlertMessage(ManagePaymentResponse manageResponse, MOBFOPTravelCredit selectedTC, List<CMSContentMessage> lstMessages, List<MOBCPTraveler> SCTravelers)
        {
            if (manageResponse?.CreditDetails?.AppliedPaymentInfos == null)
            {
                return null;
            }
            List<MOBMobileCMSContentMessages> alertmessage = new List<MOBMobileCMSContentMessages>();
            var name = selectedTC.Recipient;// $"{selectedTC.FirstName} {selectedTC.LastName}";
            var expiryDate = Convert.ToDateTime(selectedTC.ExpiryDate).ToString("MMMM dd, yyyy");
            var _etccurrent = (from data in (manageResponse?.CreditDetails.AppliedPaymentInfos) where (manageResponse?.PaymentsApplied.Any(x => data.PaymentId.Contains(x.Id) && x.Type == MOBTravelCreditFOP.ETC.ToString()) == true) && (data.InitialValue != data.CurrentValue) select data.CurrentValue).Sum();
            var _ffccurrent = (from data in (manageResponse?.CreditDetails.AppliedPaymentInfos) where (manageResponse?.PaymentsApplied.Any(x => data.PaymentId.Contains(x.Id) && x.Type != MOBTravelCreditFOP.ETC.ToString()) == true) && (data.InitialValue != data.CurrentValue) select data.CurrentValue).Sum();

            var _appliedffc = from data in (manageResponse?.PaymentsApplied) where (manageResponse.CreditDetails.AppliedPaymentInfos.Any(x => data.Id.Contains(x.PaymentId) && x.InitialValue != x.CurrentValue)) && (data.Type != MOBTravelCreditFOP.ETC.ToString()) select data;
            var _appliedetc = from data in (manageResponse?.PaymentsApplied) where (manageResponse.CreditDetails.AppliedPaymentInfos.Any(x => data.Id.Contains(x.PaymentId) && x.InitialValue != x.CurrentValue)) && (data.Type == MOBTravelCreditFOP.ETC.ToString()) select data;

            if (_ffccurrent > 0 && _appliedffc.Count() > 1)
            {
                var msg = lstMessages.Where(x => x.LocationCode == "RTI.TravelCredit.MFOP.FFC.FFCCreditBalanceMSG").FirstOrDefault();
                string txt1 = msg.ContentFull;
                alertmessage.Add(new MOBMobileCMSContentMessages()
                {
                    ContentFull = string.Format(txt1, _ffccurrent, name, expiryDate),
                    ContentShort = msg?.ContentShort,
                    LocationCode = msg?.LocationCode,
                    Title = msg?.Title
                });
            }

            if (_etccurrent > 0 && _appliedetc.Count() > 1)
            {
                var msg2 = lstMessages.Where(x => x.LocationCode == "RTI.TravelCredit.MFOP.TravelCreditBalanceMSG").FirstOrDefault();
                string txt = msg2.ContentFull;
                alertmessage.Add(new MOBMobileCMSContentMessages()
                {
                    ContentFull = string.Format(txt, _etccurrent),
                    ContentShort = msg2?.ContentShort,
                    LocationCode = msg2?.LocationCode,
                    Title = msg2?.Title
                });
            }

            return alertmessage;
        }

        private async Task<MOBShoppingCart> UpdateCertificate(MOBFOPResponse response, MOBShoppingCart shoppingCart, MOBFOPTravelCredit selectedTC, ManagePaymentResponse manageResponse, ManagePaymentRequest manageRequest, MOBSHOPReservation reservation, Session session)
        {
            if (!selectedTC.IsApplied)
            {
                if (selectedTC.TravelCreditType == MOBTravelCreditFOP.ETC)
                {
                    var certificate = shoppingCart.FormofPaymentDetails.TravelCertificate.Certificates.Where(s => s.OperationID == selectedTC.OperationID).FirstOrDefault();
                    shoppingCart.FormofPaymentDetails.TravelCertificate.Certificates.Remove(certificate);
                }
                else
                {
                    var ffc = shoppingCart.FormofPaymentDetails.TravelFutureFlightCredit.FutureFlightCredits.Where(s => s.OperationID == selectedTC.OperationID).FirstOrDefault();
                    shoppingCart.FormofPaymentDetails.TravelFutureFlightCredit.FutureFlightCredits.Remove(ffc);
                }
            }
            else
            {
                if (selectedTC.TravelCreditType == MOBTravelCreditFOP.ETC)
                    BuildTravelCertificate(selectedTC, shoppingCart.FormofPaymentDetails?.TravelCertificate, manageResponse);
                else                    
                    BuildFutureFlightCredits(manageResponse, selectedTC, manageRequest, shoppingCart, reservation);                
            }
            return shoppingCart;
        }

        private void BuildFutureFlightCredits(ManagePaymentResponse manageResponse, MOBFOPTravelCredit selectedTC, ManagePaymentRequest manageRequest, MOBShoppingCart shoppingCart, MOBSHOPReservation reservation)
        {          
            List<MOBFOPTravelCredit> travelCredits = shoppingCart.FormofPaymentDetails.TravelCreditDetails.TravelCredits;
            MOBFOPTravelCredit travelCredit = new MOBFOPTravelCredit();
            travelCredit = selectedTC;
            var _traveRes = manageResponse.CreditDetails.AppliedPaymentInfos.ToList().Where(x => x.PaymentId == manageRequest.FormOfPayment.Certificate.OperationID).ToList();
            var _paymentInfo = manageResponse.PaymentsApplied.ToList().Where(x => x.Id == manageRequest.FormOfPayment.Certificate.OperationID).ToList();

            var grandTotal = Convert.ToDouble(manageResponse?.Products.Sum(x => x.RemainingDue));
            var totalAmount = Convert.ToDouble(manageResponse?.Products.Sum(x => x.Totals?.TotalAmount));

            var currentFFCAmount = manageResponse.CreditDetails.AppliedPaymentInfos.ToList().Where(x => x.PaymentId == selectedTC.OperationID).FirstOrDefault().CurrentValue;
            var initialFFcAmount = manageResponse.CreditDetails.AppliedPaymentInfos.ToList().Where(x => x.PaymentId == selectedTC.OperationID).FirstOrDefault().InitialValue;

            var appliedCreditAmount = initialFFcAmount - currentFFCAmount;
            var sessionSavedTC = travelCredits.FirstOrDefault(tc => tc.PinCode == selectedTC.PinCode);

            if (shoppingCart.FormofPaymentDetails.TravelFutureFlightCredit == null)
                shoppingCart.FormofPaymentDetails.TravelFutureFlightCredit = new MOBFOPTravelFutureFlightCredit();
            List<MOBFOPFutureFlightCredit> futureFlightCredits = shoppingCart.FormofPaymentDetails?.TravelFutureFlightCredit.FutureFlightCredits;

            if (futureFlightCredits == null)
                futureFlightCredits = new List<MOBFOPFutureFlightCredit>();
            string travelstartDate = reservation?.Trips?.FirstOrDefault()?.DepartDate;
            var travelerCSL = reservation?.TravelersCSL;

            if (travelCredit != null)
            {
                int index = 0;
                int appliedFFCCount = 0;
                double newValueAfterRedeem = currentFFCAmount;// travelCredit.NewValueAfterRedeem;
                foreach (var TravelerNameIndex in travelCredit.EligibleTravelerNameIndex)
                {
                    var mobtraveler = manageResponse.Products.FirstOrDefault().PriceDetails.Where(x => x.TravelerIndex == TravelerNameIndex); // travelerCSL.FirstOrDefault(t => t.TravelerNameIndex == TravelerNameIndex);                   
                    var _mobtraveler = travelerCSL.FirstOrDefault(t => t.TravelerNameIndex == TravelerNameIndex);
                    if (mobtraveler != null)
                    {
                        double ffcAppliedToTraveler = mobtraveler.FirstOrDefault().Amount - mobtraveler.FirstOrDefault().RemainingDue;  //mobtraveler.FutureFlightCredits.Sum(t => t.RedeemAmount);

                        ffcAppliedToTraveler = Math.Round(ffcAppliedToTraveler, 2, MidpointRounding.AwayFromZero);

                        //Clear and apply latest FFC values
                        if (futureFlightCredits.Exists(f => f.OperationID == selectedTC.OperationID))
                            futureFlightCredits.RemoveAll(x => x.OperationID == selectedTC.OperationID);

                        if (Convert.ToDouble(travelCredit.RedeemAmount) > 0 && manageResponse.CreditDetails.AppliedPaymentInfos.Exists(f => f.PaymentId == selectedTC.OperationID))
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
                            mobFFC.TravelerNameIndex = mobtraveler.FirstOrDefault().TravelerIndex;
                            mobFFC.RedeemAmount = travelCredit.InitialValue - travelCredit.CurrentValue;
                            mobFFC.RedeemAmount = Math.Round(mobFFC.RedeemAmount, 2, MidpointRounding.AwayFromZero);
                            mobFFC.DisplayRedeemAmount = (mobFFC.RedeemAmount).ToString("C2", CultureInfo.CurrentCulture);
                            mobFFC.NewValueAfterRedeem = Math.Round(travelCredit.CurrentValue, 2, MidpointRounding.AwayFromZero);
                            mobFFC.DisplayNewValueAfterRedeem = (mobFFC.NewValueAfterRedeem).ToString("C2", CultureInfo.CurrentCulture);
                            mobFFC.RecipientsFirstName = selectedTC.FirstName + travelerCSL.FirstOrDefault().MiddleName;
                            mobFFC.RecipientsLastName = selectedTC.LastName;
                            mobFFC.PaxId = Convert.ToInt32(mobtraveler.FirstOrDefault().TravelerIndex.Split(".")[0]);
                            mobFFC.IsCertificateApplied = true;
                            mobFFC.CurrentValue = travelCredit.CurrentValue;
                            mobFFC.OperationID = !selectedTC.IsRemove ? selectedTC.OperationID : null;
                            futureFlightCredits.Add(mobFFC);
                            appliedFFCCount++;
                        }

                        double ffcAppliedToAncillary = futureFlightCredits.Where(ffc => ffc.TravelerNameIndex == "ANCILLARY").Sum(t => t.RedeemAmount);
                        ffcAppliedToAncillary = Math.Round(ffcAppliedToAncillary, 2, MidpointRounding.AwayFromZero);

                    }//travelCredit
                    _mobtraveler.FutureFlightCredits = futureFlightCredits.Where(x => x.TravelerNameIndex == TravelerNameIndex).ToList();
                    AssignTravelerTotalFFCNewValueAfterReDeem(_mobtraveler);
                    AssignTravelerTotalFFCReDeemAmount(_mobtraveler);
                }
            }

            _logger.LogInformation("GetFutureFlightCredits FutureFlightCredit:{futureFlightCredits} and sessionId:{sessionId}", JsonConvert.SerializeObject(futureFlightCredits), _headers.ContextValues.SessionId);

            shoppingCart.FormofPaymentDetails.TravelFutureFlightCredit.FutureFlightCredits =  futureFlightCredits;
        }

        #endregion
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
                shoppingCart.OmniCart.CostBreakdownFareHeader = GetCostBreakdownFareHeader(reservation?.ShopReservationInfo2?.TravelType,shoppingCart);
            }

            if (_configuration.GetValue<bool>("EnableLivecartForAwardTravel") && reservation.AwardTravel)
            {
                shoppingCart.OmniCart.AdditionalMileDetail = GetAdditionalMileDetail(reservation);
            }

            shoppingCart.OmniCart.FOPDetails = GetFOPDetails(reservation, application);
            if (reservation != null && reservation.ShopReservationInfo2 != null && !string.IsNullOrEmpty(reservation.ShopReservationInfo2.CorporateDisclaimerText))
            {
                shoppingCart.OmniCart.CorporateDisclaimerText = reservation.ShopReservationInfo2.CorporateDisclaimerText;
            }
            AssignUpliftText(shoppingCart, reservation);                //Assign message text and link text to the Uplift
        }

        private MOBSection GetAdditionalMileDetail(MOBSHOPReservation reservation)
        {
            var additionalMilesPrice = reservation?.Prices?.FirstOrDefault(price => string.Equals("MPF", price?.DisplayType, StringComparison.OrdinalIgnoreCase));
            if (additionalMilesPrice != null)
            {
                var returnObject = new MOBSection();
                returnObject.Text1 = !string.IsNullOrEmpty(_configuration.GetValue<string>("AdditionalMilesLabelText")) ? _configuration.GetValue<string>("AdditionalMilesLabelText") : "Additional Miles";
                returnObject.Text2 = additionalMilesPrice.PriceTypeDescription?.Replace("Additional", String.Empty).Trim();
                returnObject.Text3 = additionalMilesPrice.FormattedDisplayValue;

                return returnObject;
            }
            return null;

        }

        private bool HasEligibleProductsForUplift(string totalPrice, List<MOBProdDetail> products)
        {
            decimal.TryParse(totalPrice, out decimal price);
            if (price >= MinimumPriceForUplift && price <= MaxmimumPriceForUplift)
            {
                var eligibleProductsForUplift = _configuration.GetValue<string>("EligibleProductsForUpliftInViewRes").Split(',');
                if (eligibleProductsForUplift.Any())
                {
                    return products.Any(p => eligibleProductsForUplift.Contains(p.Code));
                }
            }

            return false;
        }

        private int MinimumPriceForUplift
        {
            get
            {
                var minimumAmountForUplift = _configuration.GetValue<string>("MinimumPriceForUplift");
                if (string.IsNullOrEmpty(minimumAmountForUplift))
                    return 300;

                int.TryParse(minimumAmountForUplift, out int upliftMinAmount);
                return upliftMinAmount;
            }
        }

        private int MaxmimumPriceForUplift
        {
            get
            {
                var maximumAmountForUplift = _configuration.GetValue<string>("MaximumPriceForUplift");
                if (string.IsNullOrEmpty(maximumAmountForUplift))
                    return 150000;

                int.TryParse(maximumAmountForUplift, out int upliftMaxAmount);
                return upliftMaxAmount;
            }
        }

        private bool IsEligibileForUplift(MOBSHOPReservation reservation, MOBShoppingCart shoppingCart)
        {
            if (shoppingCart?.Flow?.ToUpper() == FlowType.VIEWRES.ToString().ToUpper())
            {
                return HasEligibleProductsForUplift(shoppingCart.TotalPrice, shoppingCart.Products);
            }

            if (!_configuration.GetValue<bool>("EnableUpliftPayment"))
                return false;
            if (shoppingCart.Offers != null
               && !IsUpliftEligbleOffer(shoppingCart.Offers))
            {
                return false;
            }
            if (reservation == null || reservation.Prices == null || shoppingCart == null || shoppingCart?.Flow != FlowType.BOOKING.ToString())
                return false;

            if (reservation.ShopReservationInfo?.IsCorporateBooking ?? false)
                return false;

            if (shoppingCart.Products?.Any(p => p?.Code == "FLK") ?? false)
                return false;

            if (!_configuration.GetValue<bool>("DisableFixForUpliftFareLockDefect"))
            {
                if (shoppingCart.Products?.Any(p => p?.Code?.ToUpper() == "FARELOCK") ?? false)
                    return false;
            }

            if (reservation.Prices.Any(p => "TOTALPRICEFORUPLIFT".Equals(p.DisplayType, StringComparison.CurrentCultureIgnoreCase) && p.Value >= MinimumPriceForUplift && p.Value <= MaxmimumPriceForUplift) &&
               (shoppingCart?.SCTravelers?.Any(t => t?.TravelerTypeCode == "ADT" || t?.TravelerTypeCode == "SNR") ?? false))
            {
                return true;
            }
            return false;
        }

        public bool IsUpliftEligbleOffer(MOBOffer offer)
        {
            if (!string.IsNullOrEmpty(offer.OfferCode)
                && (offer.IsPassPlussOffer || offer.OfferType == OfferType.ECD))
            {
                return false;
            }
            return true;
        }
        private void AssignUpliftText(MOBShoppingCart shoppingCart, MOBSHOPReservation reservation)
        {
            //Get the total price of Uplift

            var upliftTotalPrice = reservation?.Prices?.FirstOrDefault(p => "TOTALPRICEFORUPLIFT".Equals(p.DisplayType, StringComparison.CurrentCultureIgnoreCase));

            if (upliftTotalPrice != null && IsEligibileForUplift(reservation, shoppingCart))                //Check if eligible for Uplift
            {
                shoppingCart.OmniCart.IsUpliftEligible = true;      //Set property to true, if Uplift is eligible
                                                                    //shoppingCart.OmniCart.UpliftMessageText = _configuration.GetValue<string>("UpliftMessageText"); //Get Uplift Message Text from appropriate appsettings file
                                                                    // shoppingCart.OmniCart.UpliftMessageLinkText = $"${upliftTotalPrice.DisplayValue}/month"; //Set the Uplift Message link text to $xxx/month format
            }
            else //Set Uplift properties to false / empty as Uplift isn't eligible
            {
                shoppingCart.OmniCart.IsUpliftEligible = false;
                //shoppingCart.OmniCart.UpliftMessageText = string.Empty;
                //shoppingCart.OmniCart.UpliftMessageLinkText = string.Empty;
            }
        }

        private List<MOBSection> GetFOPDetails(MOBSHOPReservation reservation, MOBApplication application)
        {
            if (ConfigUtility.EnableMFOP(application.Id, application.Version.Major))
            {
                return GetMFOPDetails(reservation);
            }
            else
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
        }

        private List<MOBSection> GetMFOPDetails(MOBSHOPReservation reservation)
        {
            var mobSectionList = new List<MOBSection>();
            var mobSection = default(MOBSection);

            var travelCredit = reservation.Prices.Where(price => new[] { "TB", "CERTIFICATE", "FFC" }.Any(credit => string.Equals(price.PriceType, credit, StringComparison.OrdinalIgnoreCase)));
            mobSection = new MOBSection();
            foreach (var tc in travelCredit)
            {
                if (string.Equals(tc.PriceType, "TB", StringComparison.OrdinalIgnoreCase))
                {
                    mobSection = new MOBSection();
                    mobSection.Text1 = !string.IsNullOrEmpty(_configuration.GetValue<string>("UnitedTravelBankCashLabelText")) ? _configuration.GetValue<string>("UnitedTravelBankCashLabelText") : "United TravelBank cash";
                    mobSection.Text2 = !string.IsNullOrEmpty(_configuration.GetValue<string>("TravelBankCashAppliedLabelText")) ? _configuration.GetValue<string>("TravelBankCashAppliedLabelText") : "TravelBank cash applied";
                    mobSection.Text3 = tc.FormattedDisplayValue;
                }
                else
                {
                    mobSection = new MOBSection();
                    mobSection.Text1 = string.Equals(tc.PriceType, "CERTIFICATE", StringComparison.OrdinalIgnoreCase) ? "Travel certificate" : "Future flight credit";
                    mobSection.Text2 = "Travel credit";
                    mobSection.Text3 = tc.FormattedDisplayValue;
                }
                mobSectionList.Add(mobSection);
            }

            return mobSectionList;
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

        private string GetFareLockPrice(List<MOBProdDetail> products)
        {
            return products.Where(p => p.Code.ToUpper() == "FARELOCK" || p.Code.ToUpper() == "FLK").First().ProdDisplayTotalPrice;
        }

        private int GetCartItemsCount(MOBShoppingCart shoppingcart)
        {
            int itemsCount = 0;
            if (shoppingcart?.Products != null && shoppingcart.Products.Count > 0)
            {
                shoppingcart.Products.ForEach(product =>
                {
                    if (!string.IsNullOrEmpty(product.ProdTotalPrice) && Decimal.TryParse(product.ProdTotalPrice, out decimal totalprice) && (totalprice > 0 
                                || product.Code == "RES" && totalprice == 0 ))
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

        private bool IsEnableBundleLiveUpdateChanges(int applicationId, string appVersion, bool isDisplayCart)
        {
            if (_configuration.GetValue<bool>("EnableBundleLiveUpdateChanges")
                && GeneralHelper.IsApplicationVersionGreaterorEqual(applicationId, appVersion, _configuration.GetValue<string>("Android_EnableBundleLiveUpdateChanges_AppVersion"), _configuration.GetValue<string>("iPhone_EnableBundleLiveUpdateChanges_AppVersion"))
                && isDisplayCart)
            {
                return true;
            }
            return false;
        }

        private async Task<List<CMSContentMessage>> GetSDLContentByGroupName(MOBRequest request, string guid, string token, string groupName, string docNameConfigEntry, bool useCache = false)
        {
            MOBCSLContentMessagesResponse response = null;

            try
            {
                var getSDL = await _cachingService.GetCache<string>(_configuration.GetValue<string>(docNameConfigEntry) + ObjectNames.MOBCSLContentMessagesResponseFullName, request.TransactionId).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(getSDL))
                {
                    response = JsonConvert.DeserializeObject<MOBCSLContentMessagesResponse>(getSDL);
                }

                if (response != null && response.Messages != null) { return response.Messages; }
            }

            catch { }

            MOBCSLContentMessagesRequest sdlReqeust = new MOBCSLContentMessagesRequest
            {
                Lang = "en",
                Pos = "us",
                Channel = "mobileapp",
                Listname = new List<string>(),
                LocationCodes = new List<string>(),
                Groupname = groupName,
                Usecache = useCache
            };

            string jsonRequest = JsonConvert.SerializeObject(sdlReqeust);

            response = await _cMSContentService.GetSDLContentByGroupName<MOBCSLContentMessagesResponse>(token, "message", jsonRequest, guid);

            if (response == null)
            {
                return null;
            }

            if (response.Errors.Count > 0)
            {
                string errorMsg = String.Join(" ", response.Errors.Select(x => x.Message));
                _logger.LogInformation("GetSDLContentByGroupName {CSL-CallError} with Session:{sessionId}", errorMsg, _headers.ContextValues.SessionId);
                return null;
            }

            if (response != null && (Convert.ToBoolean(response.Status) && response.Messages != null))
            {
                if (!_configuration.GetValue<bool>("DisableSDLEmptyTitleFix"))
                {
                    response.Messages = response.Messages.Where(l => l.Title != null)?.ToList();
                }
                var saveSDL = await _cachingService.SaveCache<MOBCSLContentMessagesResponse>(_configuration.GetValue<string>(docNameConfigEntry) + ObjectNames.MOBCSLContentMessagesResponseFullName, response, request.TransactionId, new TimeSpan(1, 30, 0));
                _logger.LogInformation("GetSDLContentByGroupName {SDLResponse} with Session:{sessionId}", JsonConvert.SerializeObject(response), guid);
            }

            _logger.LogInformation("GetSDLContentByGroupName SDLResponse Message:{SDLResponse} with SessionId:{sessionId}", response.Messages, guid);

            return response.Messages;
        }

        private async Task UpdateTCPriceAndFOPType(List<MOBSHOPPrice> prices, MOBFormofPaymentDetails formofPaymentDetails, MOBApplication application, List<MOBProdDetail> products, List<MOBCPTraveler> travelers, bool isManageTravelCreditApi = false)
        {
            if (IncludeTravelCredit(application.Id, application.Version.Major))
            {
                ApplyFFCToAncillary(products, application, formofPaymentDetails, prices, isManageTravelCreditApi: isManageTravelCreditApi);
                var price = prices.FirstOrDefault(p => p.DisplayType.ToUpper() == "CERTIFICATE" || p.DisplayType.ToUpper() == "FFC");
                if (price != null)
                {
                    formofPaymentDetails.TravelCreditDetails.AlertMessages = (formofPaymentDetails.TravelFutureFlightCredit?.FutureFlightCredits?.Count > 0 ?
                                                                              formofPaymentDetails.TravelFutureFlightCredit.ReviewFFCMessages :
                                                                              formofPaymentDetails.TravelCertificate.ReviewETCMessages.Where(m => m.HeadLine != "TravelCertificate_Combinability_ReviewETCAlertMsgs_OtherFopRequiredMessage").ToList());
                }
                else if (formofPaymentDetails.TravelCreditDetails != null)
                {
                    formofPaymentDetails.TravelCreditDetails.AlertMessages = null;
                }

                UpdateTravelCreditAmountWithSelectedETCOrFFC(formofPaymentDetails, prices, travelers, application.Id, application.Version.Major, isManageTravelCreditApi);
                try
                {
                    MOBCSLContentMessagesResponse lstMessages = null;
                    
                    var s = await _cachingService.GetCache<string>(_configuration.GetValue<string>("BookingPathRTI_CMSContentMessagesCached_StaticGUID") + ObjectNames.MOBCSLContentMessagesResponseFullName, _headers.ContextValues.TransactionId).ConfigureAwait(false);

                    if (!string.IsNullOrEmpty(s))
                    {
                        lstMessages = JsonConvert.DeserializeObject<MOBCSLContentMessagesResponse>(s);
                        formofPaymentDetails.TravelCreditDetails = formofPaymentDetails.TravelCreditDetails ?? new MOBFOPTravelCreditDetails();
                        formofPaymentDetails.TravelCreditDetails.AlertMessages = BuildReviewFFCHeaderMessage(formofPaymentDetails?.TravelFutureFlightCredit, travelers, lstMessages.Messages);
                        //FFC.FutureFlightCredit.AssignEmailMessageForFFCRefund(lstMessages.Messages, prices, formofPaymentDetails.EmailAddress, formofPaymentDetails.TravelFutureFlightCredit, application, true) This is for displaying the EmailConfirmation message in the purchase summary after CHECKOUT call, Its based on the NEW PIN CREATED , the above alert message is build for that logic
                        // So we are checking the message and based on the add the email message
                        if (_configuration.GetValue<bool>("EnableEmailConfirmationBasedOnNEWPINCREATED") == false && ConfigUtility.IsFFCSummaryUpdated(application.Id, application.Version.Major) && isManageTravelCreditApi == false && formofPaymentDetails.TravelCreditDetails.AlertMessages != null)
                        {
                            formofPaymentDetails.TravelFutureFlightCredit.EmailConfirmationFFCMessages = GetSDLMessageFromList(lstMessages.Messages, "RTI.FutureFlightCredits.EmailConfirmation");
                            formofPaymentDetails.TravelFutureFlightCredit.EmailConfirmationFFCMessages[0].ContentFull = string.Format(formofPaymentDetails.TravelFutureFlightCredit.EmailConfirmationFFCMessages[0].ContentFull, formofPaymentDetails.EmailAddress);
                            formofPaymentDetails.TravelFutureFlightCredit.EmailConfirmationFFCMessages[0].ContentShort = formofPaymentDetails.TravelFutureFlightCredit.EmailConfirmationFFCMessages[0].ContentFull;
                        }
                    }
                }
                catch { }

                if (formofPaymentDetails?.FormOfPaymentType == "ETC" ||
                   formofPaymentDetails?.FormOfPaymentType == "FFC")
                    formofPaymentDetails.FormOfPaymentType = "TC";

            }
        }

        private void UpdateTravelCreditAmountWithSelectedETCOrFFC(MOBFormofPaymentDetails formofPaymentDetails, List<MOBSHOPPrice> prices, List<MOBCPTraveler> travelers, int appId = 0, string appVersion = "", bool isManageTravelCreditApi = false)
        {
            if (formofPaymentDetails?.TravelCreditDetails?.TravelCredits?.Count > 0)
            {
                bool isETC = (formofPaymentDetails?.TravelCertificate?.Certificates?.Count > 0);
                bool isFFC = (formofPaymentDetails?.TravelFutureFlightCredit?.FutureFlightCredits?.Count > 0);
                bool isTravelerFFC_Check = !_configuration.GetValue<bool>("DisableMPSignedInInsertUpdateTraveler");

                foreach (var travelCredit in formofPaymentDetails.TravelCreditDetails.TravelCredits)
                {
                    double redeemAmount = 0;
                    if (isETC)
                    {
                        var cert = formofPaymentDetails.TravelCertificate.Certificates.Where(c => c.PinCode == travelCredit.PinCode).ToList();
                        if (cert != null)
                        {
                            redeemAmount = cert.Sum(c => c.RedeemAmount);
                        }
                    }

                    if (isFFC)
                    {
                        var ffcs = formofPaymentDetails.TravelFutureFlightCredit.FutureFlightCredits.Where(c => c.PinCode == travelCredit.PinCode).ToList();
                        if (ffcs != null)
                        {
                            redeemAmount = ffcs.Sum(c => c.RedeemAmount);
                        }
                    }
                    UpdateTravelCreditRedeemAmount(travelCredit, redeemAmount);
                }
                if (isFFC)
                {
                    IEnumerable<MOBFOPTravelCredit> ancillaryTCs = formofPaymentDetails.TravelCreditDetails.TravelCredits.Where(tc => tc.IsApplied);
                    foreach (var ancillaryTC in ancillaryTCs)
                    {
                        foreach (var scTraveler in isTravelerFFC_Check ? travelers.Where(trav => trav.FutureFlightCredits != null).Where(tc => tc.FutureFlightCredits.Exists(f => f.PinCode == ancillaryTC.PinCode)) : travelers.Where(trav => trav.FutureFlightCredits.Exists(f => f.PinCode == ancillaryTC.PinCode)))
                        {
                            var travelerFFC = scTraveler.FutureFlightCredits.FirstOrDefault(ffc => ffc.PinCode == ancillaryTC.PinCode);
                            if (travelerFFC != null)
                            {
                                travelerFFC.NewValueAfterRedeem = 0;
                                travelerFFC.NewValueAfterRedeem = Math.Round(travelerFFC.NewValueAfterRedeem, 2, MidpointRounding.AwayFromZero);
                                travelerFFC.DisplayNewValueAfterRedeem = (travelerFFC.NewValueAfterRedeem).ToString("C2", CultureInfo.CurrentCulture);

                                AssignTravelerTotalFFCNewValueAfterReDeem(scTraveler);
                            }
                        }
                        foreach (var tnIndex in ancillaryTC.EligibleTravelers)
                        {
                            MOBCPTraveler traveler = isTravelerFFC_Check ? travelers.Where(tc => tc.FutureFlightCredits != null).FirstOrDefault(trav => trav.TravelerNameIndex == tnIndex) : travelers.FirstOrDefault(trav => trav.TravelerNameIndex == tnIndex);
                            var travelerFFC = traveler?.FutureFlightCredits.FirstOrDefault(ffc => ffc.PinCode == ancillaryTC.PinCode);
                            if (travelerFFC != null)
                            {
                                travelerFFC.NewValueAfterRedeem = ancillaryTC.NewValueAfterRedeem / ancillaryTC.EligibleTravelers.Count;
                                travelerFFC.NewValueAfterRedeem = Math.Round(travelerFFC.NewValueAfterRedeem, 2, MidpointRounding.AwayFromZero);
                                travelerFFC.DisplayNewValueAfterRedeem = (travelerFFC.NewValueAfterRedeem).ToString("C2", CultureInfo.CurrentCulture);

                                AssignTravelerTotalFFCNewValueAfterReDeem(traveler);
                            }
                        }
                    }

                    var newRefundValueAfterReeedeem = ancillaryTCs.Sum(tcAmount => tcAmount.NewValueAfterRedeem);
                    var refundPrice = prices.FirstOrDefault(p => p.PriceType == "REFUNDPRICE");
                    if (refundPrice != null)
                    {
                        if (newRefundValueAfterReeedeem > 0)
                        {
                            string priceTypeDescription = UpdateRefundPricePriceDescription(appId, appVersion, ancillaryTCs, isManageTravelCreditApi);
                            UpdateCertificatePrice(refundPrice, newRefundValueAfterReeedeem, "REFUNDPRICE", priceTypeDescription, "RESIDUALCREDIT");
                        }
                        else
                        {
                            prices.RemoveAll(p => p.PriceType == "REFUNDPRICE");
                        }
                    }
                }
            }
        }

        private string UpdateRefundPricePriceDescription(int appId, string appVersion, IEnumerable<MOBFOPTravelCredit> ancillaryTCs, bool isManageTravelCreditApi = false)
        {
            string priceTypeDescription = "Total credit";
            if (ConfigUtility.IsFFCSummaryUpdated(appId, appVersion) && isManageTravelCreditApi == false)
            {
                priceTypeDescription = _configuration.GetValue<string>("FFC_RemainingText");
            }

            return priceTypeDescription;
        }

        private void UpdateTravelCreditRedeemAmount(MOBFOPTravelCredit travelCredit, double redeemAmount)
        {     
            travelCredit.RedeemAmount = redeemAmount;
            travelCredit.RedeemAmount = Math.Round(travelCredit.RedeemAmount, 2, MidpointRounding.AwayFromZero);
            travelCredit.DisplayRedeemAmount = (redeemAmount).ToString("C2", CultureInfo.CurrentCulture);
            travelCredit.NewValueAfterRedeem = travelCredit.CurrentValue - (!_configuration.GetValue<bool>("FFC_NegativeValue") ? travelCredit.RedeemAmount : redeemAmount);
            travelCredit.NewValueAfterRedeem = Math.Round(travelCredit.NewValueAfterRedeem, 2, MidpointRounding.AwayFromZero);
            travelCredit.DisplayNewValueAfterRedeem = (travelCredit.NewValueAfterRedeem).ToString("C2", CultureInfo.CurrentCulture);
            travelCredit.IsApplied = redeemAmount > 0;
        }

        private void ApplyFFCToAncillary(List<MOBProdDetail> products, MOBApplication application, MOBFormofPaymentDetails mobFormofPaymentDetails, List<MOBSHOPPrice> prices, bool isAncillaryON = false, bool isManageTravelCreditApi = false)
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

        private async Task AddTCToETCOrFFC(Session session, MOBRequest request, Reservation bookingPathReservation, MOBFOPResponse response, MOBFOPTravelCredit selectedTC, bool isAncillaryFFCEnable)
        {
            if (selectedTC.TravelCreditType == MOBTravelCreditFOP.ETC)
            {
                List<MOBFOPCertificate> requestedCertificates = BuildETCCertificate(selectedTC, response.ShoppingCart.FormofPaymentDetails?.TravelCertificate);
                var etcResponse = await _ETCUtility.AddCertificatesToFOP(requestedCertificates, session);

               await  _sessionHelperService.SaveSession<MOBFOPTravelerCertificateResponse>(etcResponse, session.SessionId, new List<string> { session.SessionId, etcResponse.ObjectName }, etcResponse.ObjectName).ConfigureAwait(false);

                response.ShoppingCart = etcResponse.ShoppingCart;
                response.Reservation = etcResponse.Reservation;
                if(await _featureSettings.GetFeatureSettingValue("EnableChaseStatementFixForCreditCardChange"))
                    bookingPathReservation.ShopReservationInfo2 = etcResponse.Reservation.ShopReservationInfo2;
                bookingPathReservation.Prices = etcResponse.Reservation.Prices;

                var appliedEtc = response.ShoppingCart.FormofPaymentDetails.TravelCertificate.Certificates.FirstOrDefault(c => c.PinCode == selectedTC.PinCode);
            }
            else
            {
                if (selectedTC.IsApplied)
                {
                    var travelFutureFlightCredit = response.ShoppingCart.FormofPaymentDetails.TravelFutureFlightCredit;
                    if (travelFutureFlightCredit == null)
                        travelFutureFlightCredit = new MOBFOPTravelFutureFlightCredit();

                    travelFutureFlightCredit.FutureFlightCredits
                               = await AssignOTFTravelerFFCs(selectedTC, travelFutureFlightCredit, response.Reservation, isAncillaryFFCEnable);
                    response.ShoppingCart.FormofPaymentDetails.TravelFutureFlightCredit = travelFutureFlightCredit;
                }
                else
                {
                    int index = 0;
                    List<MOBFOPTravelCredit> travelCredits = response.ShoppingCart.FormofPaymentDetails.TravelCreditDetails.TravelCredits;
                    var sessionSavedTC = travelCredits.FirstOrDefault(tc => tc.PinCode == selectedTC.PinCode);
                    sessionSavedTC.IsApplied = false;
                    foreach (var travelNameIndex in selectedTC.EligibleTravelerNameIndex)
                    {
                        var requestedCredit = response.ShoppingCart.FormofPaymentDetails
                                                              .TravelFutureFlightCredit.FutureFlightCredits
                                                              .FirstOrDefault(ffc => ffc.TravelerNameIndex == travelNameIndex &&
                                                                              ffc.PinCode == selectedTC.PinCode &&
                                                                              ffc.RecordLocator == selectedTC.RecordLocator);

                        List<MOBFOPFutureFlightCredit> futureFlightCredits = response?.ShoppingCart?.FormofPaymentDetails?.TravelFutureFlightCredit?.FutureFlightCredits;
                        if (futureFlightCredits != null && requestedCredit != null)
                        {
                            var removeFFCtraveler = response.Reservation.TravelersCSL.Find(t => t.TravelerNameIndex == requestedCredit.TravelerNameIndex && t.PaxID == requestedCredit.PaxId);
                            removeFFCtraveler.FutureFlightCredits = RemoveRequestedFFCandUpdateNewValueForRemainingFFCs(requestedCredit, removeFFCtraveler.FutureFlightCredits, removeFFCtraveler.IndividualTotalAmount);
                            AssignTravelerTotalFFCNewValueAfterReDeem(removeFFCtraveler);
                            futureFlightCredits.RemoveAll(ffc => ffc.TravelerNameIndex == requestedCredit.TravelerNameIndex && ffc.PaxId == requestedCredit.PaxId);
                            if (removeFFCtraveler.FutureFlightCredits != null)
                            {
                                futureFlightCredits.AddRange(removeFFCtraveler.FutureFlightCredits);
                            }
                        }
                        index++;
                    }
                    if (isAncillaryFFCEnable)
                    {
                        response.ShoppingCart.FormofPaymentDetails.TravelFutureFlightCredit.FutureFlightCredits
                                                              .RemoveAll(ffc => ffc.TravelerNameIndex == "ANCILLARY" && ffc.PinCode == selectedTC.PinCode);
                    }

                    ReloadPricesAfterRemove(travelCredits, response, isAncillaryFFCEnable, bookingPathReservation);

                    if (!_configuration.GetValue<bool>("disable21GFFCToggle"))
                    {
                         response.PkDispenserPublicKey = await _mSCPkDispenserPublicKey.GetCachedOrNewpkDispenserPublicKey(request.Application.Id, request.Application.Version.Major, request.DeviceId, session.SessionId, session.Token, session?.CatalogItems).ConfigureAwait(false);
                    }
                }
                List<CMSContentMessage> lstMessages = await GetSDLContentByGroupName(request, session.SessionId, session.Token,
                                                                                        _configuration.GetValue<string>("CMSContentMessages_GroupName_BookingRTI_Messages"),
                                                                                                                   "BookingPathRTI_CMSContentMessagesCached_StaticGUID");
                await SaveSCAndReservationToPersistWithFFCTraveler(session, response, request, lstMessages, bookingPathReservation);
            }
        }

        private async Task ReloadPricesAfterRemove(List<MOBFOPTravelCredit> travelCredits, MOBFOPResponse response, bool isAncillaryFFCEnable, Reservation bookingPathReservation)
        {

            response.Reservation.TravelersCSL.ForEach(tr => tr.FutureFlightCredits = new List<MOBFOPFutureFlightCredit>());
            response.ShoppingCart.FormofPaymentDetails.TravelFutureFlightCredit.FutureFlightCredits = new List<MOBFOPFutureFlightCredit>();

            foreach (var travelCredt in travelCredits.Where(t => t.IsApplied))
            {
                UpdateTravelCreditRedeemAmount(travelCredt, 0);
                response.ShoppingCart.FormofPaymentDetails.TravelFutureFlightCredit.FutureFlightCredits
                                = await AssignOTFTravelerFFCs(travelCredt, response.ShoppingCart.FormofPaymentDetails.TravelFutureFlightCredit, response.Reservation, isAncillaryFFCEnable);
            }
            foreach (var traveler in bookingPathReservation.TravelersCSL)
            {
                traveler.Value.FutureFlightCredits = response.Reservation.TravelersCSL.FirstOrDefault(t => t.TravelerNameIndex == traveler.Value.TravelerNameIndex).FutureFlightCredits;
            }
        }

        private List<MOBFOPFutureFlightCredit> RemoveRequestedFFCandUpdateNewValueForRemainingFFCs(MOBFOPFutureFlightCredit requestedFFC, List<MOBFOPFutureFlightCredit> ffcs, double totalAmount)
        {
            ffcs.RemoveAll(ffc => ffc.PaxId == requestedFFC.PaxId && ffc.TravelerNameIndex == requestedFFC.TravelerNameIndex && ffc.PinCode == requestedFFC.PinCode);
            if (ffcs.Count == 0)
            {
                ffcs = null;
            }
            else
            {
                double totalAmountCopy = totalAmount;
                double ffcAppliedToTraveler = 0;
                foreach (var ffc in ffcs)
                {
                    if (totalAmountCopy > 0)
                    {
                        double remainingBalanceAfterAppliedFFC = totalAmount - ffcAppliedToTraveler;
                        ffc.RedeemAmount = remainingBalanceAfterAppliedFFC > ffc.CurrentValue ? ffc.CurrentValue : remainingBalanceAfterAppliedFFC;
                        ffc.RedeemAmount = Math.Round(ffc.RedeemAmount, 2, MidpointRounding.AwayFromZero);
                        ffc.DisplayRedeemAmount = (ffc.RedeemAmount).ToString("C2", CultureInfo.CurrentCulture);
                        ffc.NewValueAfterRedeem = ffc.CurrentValue - ffc.RedeemAmount;
                        ffc.NewValueAfterRedeem = Math.Round(ffc.NewValueAfterRedeem, 2, MidpointRounding.AwayFromZero);
                        ffc.DisplayNewValueAfterRedeem = (ffc.NewValueAfterRedeem).ToString("C2", CultureInfo.CurrentCulture);

                        totalAmountCopy -= ffc.RedeemAmount;
                        ffcAppliedToTraveler += ffc.RedeemAmount;
                    }
                }
            }

            _logger.LogInformation("RemoveRequestedFFCandUpdateNewValueForRemainingFFCs FFCList:{ffc}anf sessionId:{sessionId}", JsonConvert.SerializeObject(ffcs), _headers.ContextValues.SessionId);

            return ffcs;
        }

        private async Task<List<MOBFOPFutureFlightCredit>> AssignOTFTravelerFFCs(MOBFOPTravelCredit travelCredit, MOBFOPTravelFutureFlightCredit mOBFOPTravelFutureFlightCredit, MOBSHOPReservation reservation, bool isAncillaryFFCEnable)
        {
            List<MOBFOPFutureFlightCredit> futureFlightCredits = mOBFOPTravelFutureFlightCredit.FutureFlightCredits;
            if (futureFlightCredits == null)
                futureFlightCredits = new List<MOBFOPFutureFlightCredit>();
            string travelstartDate = reservation?.Trips?.FirstOrDefault()?.DepartDate;
            var travelerCSL = reservation?.TravelersCSL;

            if (await _featureSettings.GetFeatureSettingValue("FixedAssignTravelerFFCIssue").ConfigureAwait(false))
            {
                if (travelCredit != null)
                {
                    int index = 0;
                    int appliedFFCCount = 0;
                    double newValueAfterRedeem = travelCredit.NewValueAfterRedeem;
                    foreach (var TravelerNameIndex in travelCredit.EligibleTravelerNameIndex)
                    {
                        var mobtraveler = travelerCSL.FirstOrDefault(t => t.TravelerNameIndex == TravelerNameIndex);
                        if (mobtraveler != null)
                        {
                            if (mobtraveler.FutureFlightCredits == null)
                                mobtraveler.FutureFlightCredits = new List<MOBFOPFutureFlightCredit>();

                            double ffcAppliedToTraveler = 0;
                            if (mobtraveler.FutureFlightCredits != null)
                                ffcAppliedToTraveler = mobtraveler.FutureFlightCredits.Sum(t => t.RedeemAmount);

                            ffcAppliedToTraveler = Math.Round(ffcAppliedToTraveler, 2, MidpointRounding.AwayFromZero);

                            if (Convert.ToDouble(newValueAfterRedeem) > 0
                                && ffcAppliedToTraveler < mobtraveler.IndividualTotalAmount
                                && !mobtraveler.FutureFlightCredits.Exists(f => f.PaxId == mobtraveler.PaxID &&
                                                                                f.TravelerNameIndex == mobtraveler.TravelerNameIndex &&
                                                                                f.PinCode == travelCredit.PinCode))
                            {
                                index++;
                                var mobFFC = new MOBFOPFutureFlightCredit();
                                mobFFC.CreditAmount = (newValueAfterRedeem).ToString("C2", CultureInfo.CurrentCulture);
                                mobFFC.ExpiryDate = Convert.ToDateTime(travelCredit.ExpiryDate).ToString("MMMMM dd, yyyy");
                                mobFFC.IsCertificateApplied = true;
                                mobFFC.InitialValue = travelCredit.InitialValue;
                                mobFFC.Index = index;
                                mobFFC.PinCode = travelCredit.PinCode;
                                mobFFC.PromoCode = travelCredit.PromoCode;
                                mobFFC.RecordLocator = travelCredit.RecordLocator;

                                mobFFC.TravelerNameIndex = mobtraveler.TravelerNameIndex;
                                double remainingBalanceAfterAppliedFFC = mobtraveler.IndividualTotalAmount - ffcAppliedToTraveler;
                                mobFFC.RedeemAmount = remainingBalanceAfterAppliedFFC > newValueAfterRedeem ? newValueAfterRedeem : remainingBalanceAfterAppliedFFC;
                                mobFFC.RedeemAmount = Math.Round(mobFFC.RedeemAmount, 2, MidpointRounding.AwayFromZero);
                                mobFFC.DisplayRedeemAmount = (mobFFC.RedeemAmount).ToString("C2", CultureInfo.CurrentCulture);
                                mobFFC.NewValueAfterRedeem = newValueAfterRedeem - mobFFC.RedeemAmount;
                                mobFFC.NewValueAfterRedeem = Math.Round(mobFFC.NewValueAfterRedeem, 2, MidpointRounding.AwayFromZero);
                                mobFFC.DisplayNewValueAfterRedeem = (mobFFC.NewValueAfterRedeem).ToString("C2", CultureInfo.CurrentCulture);
                                mobFFC.RecipientsFirstName = mobtraveler.FirstName + mobtraveler.MiddleName;
                                mobFFC.RecipientsLastName = mobtraveler.LastName;
                                mobFFC.PaxId = mobtraveler.PaxID;
                                mobFFC.IsCertificateApplied = true;
                                mobFFC.CurrentValue = mobFFC.RedeemAmount;
                                newValueAfterRedeem -= mobFFC.RedeemAmount;
                                newValueAfterRedeem = Math.Round(newValueAfterRedeem, 2, MidpointRounding.AwayFromZero);


                                futureFlightCredits.Add(mobFFC);
                                mobtraveler.FutureFlightCredits.Add(mobFFC);
                                appliedFFCCount++;
                            }
                        }
                        AssignTravelerTotalFFCNewValueAfterReDeem(mobtraveler);
                    }
                    double ffcAppliedToAncillary = futureFlightCredits.Where(ffc => ffc.TravelerNameIndex == "ANCILLARY").Sum(t => t.RedeemAmount);
                    ffcAppliedToAncillary = Math.Round(ffcAppliedToAncillary, 2, MidpointRounding.AwayFromZero);

                    if (isAncillaryFFCEnable && Convert.ToDouble(newValueAfterRedeem) > 0
                        && ffcAppliedToAncillary < mOBFOPTravelFutureFlightCredit.AllowedAncillaryAmount
                        && !futureFlightCredits.Exists(f => f.TravelerNameIndex == "ANCILLARY" && f.PinCode == travelCredit.PinCode))
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
                        double remainingBalanceAfterAppliedFFC = mOBFOPTravelFutureFlightCredit.AllowedAncillaryAmount - ffcAppliedToAncillary;
                        mobFFC.RedeemAmount = remainingBalanceAfterAppliedFFC > newValueAfterRedeem ? newValueAfterRedeem : remainingBalanceAfterAppliedFFC;
                        mobFFC.RedeemAmount = Math.Round(mobFFC.RedeemAmount, 2, MidpointRounding.AwayFromZero);
                        mobFFC.DisplayRedeemAmount = (mobFFC.RedeemAmount).ToString("C2", CultureInfo.CurrentCulture);
                        mobFFC.NewValueAfterRedeem = newValueAfterRedeem - mobFFC.RedeemAmount;
                        mobFFC.NewValueAfterRedeem = Math.Round(mobFFC.NewValueAfterRedeem, 2, MidpointRounding.AwayFromZero);
                        mobFFC.DisplayNewValueAfterRedeem = (mobFFC.NewValueAfterRedeem).ToString("C2", CultureInfo.CurrentCulture);
                        mobFFC.IsCertificateApplied = true;
                        mobFFC.CurrentValue = travelCredit.CurrentValue;
                        futureFlightCredits.Add(mobFFC);
                        newValueAfterRedeem -= mobFFC.RedeemAmount;
                        newValueAfterRedeem = Math.Round(newValueAfterRedeem, 2, MidpointRounding.AwayFromZero);
                    }
                }
            }
            else
            {
                if (travelCredit != null)
                {
                    int index = 0;
                    int appliedFFCCount = 0;
                    double newValueAfterRedeem = travelCredit.NewValueAfterRedeem;
                    foreach (var TravelerNameIndex in travelCredit.EligibleTravelerNameIndex)
                    {

                        var mobtraveler = travelerCSL.FirstOrDefault(t => t.TravelerNameIndex == TravelerNameIndex);

                        if (mobtraveler != null)
                        {
                            if (mobtraveler.FutureFlightCredits == null)
                                mobtraveler.FutureFlightCredits = new List<MOBFOPFutureFlightCredit>();

                            double ffcAppliedToTraveler = 0;
                            if (mobtraveler.FutureFlightCredits != null)
                                ffcAppliedToTraveler = mobtraveler.FutureFlightCredits.Sum(t => t.RedeemAmount);

                            ffcAppliedToTraveler = Math.Round(ffcAppliedToTraveler, 2, MidpointRounding.AwayFromZero);

                            if (Convert.ToDouble(newValueAfterRedeem) > 0
                                && ffcAppliedToTraveler < mobtraveler.IndividualTotalAmount
                                && !mobtraveler.FutureFlightCredits.Exists(f => f.PaxId == mobtraveler.PaxID &&
                                                                                f.TravelerNameIndex == mobtraveler.TravelerNameIndex &&
                                                                                f.PinCode == travelCredit.PinCode))
                            {
                                index++;
                                var mobFFC = new MOBFOPFutureFlightCredit();
                                mobFFC.CreditAmount = (newValueAfterRedeem).ToString("C2", CultureInfo.CurrentCulture);
                                mobFFC.ExpiryDate = Convert.ToDateTime(travelCredit.ExpiryDate).ToString("MMMMM dd, yyyy");
                                mobFFC.IsCertificateApplied = true;
                                mobFFC.InitialValue = travelCredit.InitialValue;
                                mobFFC.Index = index;
                                mobFFC.PinCode = travelCredit.PinCode;
                                mobFFC.PromoCode = travelCredit.PromoCode;
                                mobFFC.RecordLocator = travelCredit.RecordLocator;

                                mobFFC.TravelerNameIndex = mobtraveler.TravelerNameIndex;
                                double remainingBalanceAfterAppliedFFC = mobtraveler.IndividualTotalAmount - ffcAppliedToTraveler;
                                mobFFC.RedeemAmount = remainingBalanceAfterAppliedFFC > newValueAfterRedeem ? newValueAfterRedeem : remainingBalanceAfterAppliedFFC;
                                mobFFC.RedeemAmount = Math.Round(mobFFC.RedeemAmount, 2, MidpointRounding.AwayFromZero);
                                mobFFC.DisplayRedeemAmount = (mobFFC.RedeemAmount).ToString("C2", CultureInfo.CurrentCulture);
                                mobFFC.NewValueAfterRedeem = newValueAfterRedeem - mobFFC.RedeemAmount;
                                mobFFC.NewValueAfterRedeem = Math.Round(mobFFC.NewValueAfterRedeem, 2, MidpointRounding.AwayFromZero);
                                mobFFC.DisplayNewValueAfterRedeem = (mobFFC.NewValueAfterRedeem).ToString("C2", CultureInfo.CurrentCulture);
                                mobFFC.RecipientsFirstName = mobtraveler.FirstName + mobtraveler.MiddleName;
                                mobFFC.RecipientsLastName = mobtraveler.LastName;
                                mobFFC.PaxId = mobtraveler.PaxID;
                                mobFFC.IsCertificateApplied = true;
                                mobFFC.CurrentValue = mobFFC.RedeemAmount;
                                newValueAfterRedeem -= mobFFC.RedeemAmount;
                                newValueAfterRedeem = Math.Round(newValueAfterRedeem, 2, MidpointRounding.AwayFromZero);


                                futureFlightCredits.Add(mobFFC);
                                mobtraveler.FutureFlightCredits.Add(mobFFC);
                                appliedFFCCount++;
                            }

                            double ffcAppliedToAncillary = futureFlightCredits.Where(ffc => ffc.TravelerNameIndex == "ANCILLARY").Sum(t => t.RedeemAmount);
                            ffcAppliedToAncillary = Math.Round(ffcAppliedToAncillary, 2, MidpointRounding.AwayFromZero);

                            if (isAncillaryFFCEnable && Convert.ToDouble(newValueAfterRedeem) > 0
                                && ffcAppliedToAncillary < mOBFOPTravelFutureFlightCredit.AllowedAncillaryAmount
                                && !futureFlightCredits.Exists(f => f.TravelerNameIndex == "ANCILLARY" && f.PinCode == travelCredit.PinCode))
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
                                double remainingBalanceAfterAppliedFFC = mOBFOPTravelFutureFlightCredit.AllowedAncillaryAmount - ffcAppliedToAncillary;
                                mobFFC.RedeemAmount = remainingBalanceAfterAppliedFFC > newValueAfterRedeem ? newValueAfterRedeem : remainingBalanceAfterAppliedFFC;
                                mobFFC.RedeemAmount = Math.Round(mobFFC.RedeemAmount, 2, MidpointRounding.AwayFromZero);
                                mobFFC.DisplayRedeemAmount = (mobFFC.RedeemAmount).ToString("C2", CultureInfo.CurrentCulture);
                                mobFFC.NewValueAfterRedeem = newValueAfterRedeem - mobFFC.RedeemAmount;
                                mobFFC.NewValueAfterRedeem = Math.Round(mobFFC.NewValueAfterRedeem, 2, MidpointRounding.AwayFromZero);
                                mobFFC.DisplayNewValueAfterRedeem = (mobFFC.NewValueAfterRedeem).ToString("C2", CultureInfo.CurrentCulture);
                                mobFFC.IsCertificateApplied = true;
                                mobFFC.CurrentValue = travelCredit.CurrentValue;
                                futureFlightCredits.Add(mobFFC);
                                newValueAfterRedeem -= mobFFC.RedeemAmount;
                                newValueAfterRedeem = Math.Round(newValueAfterRedeem, 2, MidpointRounding.AwayFromZero);
                            }
                        }//travelCredit
                        AssignTravelerTotalFFCNewValueAfterReDeem(mobtraveler);
                    }
                }
            }
            _logger.LogInformation("AssignOTFTravelerFFCs FutureFlightCredit:{futureFlightCredits} and sessionId:{sessionId}", JsonConvert.SerializeObject(futureFlightCredits), _headers.ContextValues.SessionId);

            return futureFlightCredits;
        }

        private List<MOBFOPCertificate> BuildETCCertificate(MOBFOPTravelCredit travelCredit, MOBFOPTravelCertificate travelCertificate)
        {
            List<MOBFOPCertificate> certificates = new List<MOBFOPCertificate>();
            var certificate = new MOBFOPCertificate();
            certificate.PinCode = travelCredit.PinCode;
            certificate.YearIssued = travelCredit.YearIssued;
            certificate.RecipientsLastName = travelCredit.LastName;
            certificate.RecipientsFirstName = travelCredit.FirstName;
            certificate.RedeemAmount = travelCredit.RedeemAmount;
            certificate.InitialValue = travelCredit.InitialValue;
            certificate.CurrentValue = travelCredit.CurrentValue;
            certificate.Index = 0;
            certificate.IsCertificateApplied = false;
            certificate.IsProfileCertificate = true;
            certificate.EditingIndex = 0;
            certificate.IsRemove = travelCredit.IsRemove;
            certificate.ExpiryDate = travelCredit.ExpiryDate;
            certificate.DisplayRedeemAmount = travelCredit.DisplayRedeemAmount;
            certificate.DisplayNewValueAfterRedeem = travelCredit.DisplayNewValueAfterRedeem;
            certificate.NewValueAfterRedeem = travelCredit.NewValueAfterRedeem;

            if (travelCertificate?.Certificates?.Count > 0)
            {
                var cert = travelCertificate.Certificates.FirstOrDefault(c => c.PinCode == travelCredit.PinCode);
                if (cert != null)
                    certificate.Index = cert.Index;
            }

            certificates.Add(certificate);

            _logger.LogInformation("BuildETCCertificate Certificates:{certificates} and sessionId:{sessionId}", JsonConvert.SerializeObject(certificates), _headers.ContextValues.SessionId);

            return certificates;
        }
        private void BuildTravelCertificate(MOBFOPTravelCredit travelCredit, MOBFOPTravelCertificate travelCertificate, ManagePaymentResponse manageResponse)
        {
            //List<MOBFOPCertificate> certificates = travelCertificate.Certificates;

            if (travelCertificate.Certificates.IsListNullOrEmpty())
                travelCertificate.Certificates = new List<MOBFOPCertificate>();

            var _traveRes = manageResponse.CreditDetails.AppliedPaymentInfos.ToList().Where(x => x.PaymentId == travelCredit.OperationID).ToList();
            travelCertificate.Certificates.RemoveAll(c => c.OperationID == travelCredit.OperationID); // Remove and rearrange the Certificates based on the latest response
            var certificate = new MOBFOPCertificate();
            certificate.PinCode = travelCredit.PinCode;
            certificate.YearIssued = travelCredit.YearIssued;
            certificate.RecipientsLastName = travelCredit.LastName;
            certificate.RecipientsFirstName = travelCredit.FirstName;
            certificate.RedeemAmount = _traveRes.FirstOrDefault().InitialValue - _traveRes.FirstOrDefault().CurrentValue; // travelCredit.RedeemAmount;
            certificate.InitialValue = _traveRes.FirstOrDefault().InitialValue; // travelCredit.InitialValue;
            certificate.CurrentValue = Convert.ToDouble(_traveRes.FirstOrDefault().CurrentValue); // travelCredit.CurrentValue;
            certificate.Index = 0;
            certificate.IsCertificateApplied = !travelCredit.IsRemove;
            certificate.IsProfileCertificate = true;
            certificate.EditingIndex = 0;
            certificate.IsRemove = travelCredit.IsRemove;
            certificate.ExpiryDate = travelCredit.ExpiryDate;
            certificate.DisplayRedeemAmount = $"${string.Format("{0:#,0.00}", certificate.RedeemAmount)}"; // travelCredit.DisplayRedeemAmount;
            certificate.NewValueAfterRedeem = _traveRes.FirstOrDefault().CurrentValue;// travelCredit.NewValueAfterRedeem;
            certificate.DisplayNewValueAfterRedeem = $"${string.Format("{0:#,0.00}", certificate.NewValueAfterRedeem)}"; // travelCredit.DisplayNewValueAfterRedeem;
            certificate.OperationID = !travelCredit.IsRemove ? travelCredit.OperationID : null;

            if (travelCertificate?.Certificates?.Count > 0)
            {
                var cert = travelCertificate.Certificates.FirstOrDefault(c => c.PinCode == travelCredit.PinCode);
                if (cert != null)
                    certificate.Index = cert.Index;
                //var count = 0;
                //travelCertificate.Certificates.ForEach(x => { x.Index = count++; });
            }
            travelCertificate.Certificates.Add(certificate);
            _logger.LogInformation("BuildETCCertificate Certificates:{certificates} and sessionId:{sessionId}", JsonConvert.SerializeObject(travelCertificate.Certificates), _headers.ContextValues.SessionId);

            //return travelCertificate;
        }


        private async Task SaveSCAndReservationToPersistWithFFCTraveler(Session session, MOBFOPResponse response, MOBRequest request, List<CMSContentMessage> lstMessages, Reservation bookingPathReservation)
        {
            if (response.Reservation.TravelersCSL != null && response.Reservation.TravelersCSL.Count > 0)
            {
                bookingPathReservation.TravelersCSL = new SerializableDictionary<string, MOBCPTraveler>();
                foreach (var travelersCSL in response.Reservation.TravelersCSL)
                {
                    bookingPathReservation.TravelersCSL.Add(travelersCSL.PaxIndex.ToString(), travelersCSL);
                }
            }

            bookingPathReservation.Prices = response.Reservation.Prices;
            response.ShoppingCart.FormofPaymentDetails.FormOfPaymentType = response.ShoppingCart.FormofPaymentDetails?.TravelFutureFlightCredit?.TotalRedeemAmount > 0 ? MOBFormofPayment.FFC.ToString() : MOBFormofPayment.CreditCard.ToString();
            var scRESProduct = response.ShoppingCart.Products.Find(p => p.Code == "RES");
            double totalRedeemAmount = 0;
            if (response.ShoppingCart.FormofPaymentDetails?.TravelFutureFlightCredit?.TotalRedeemAmount > 0)
            {
                totalRedeemAmount = response.ShoppingCart.FormofPaymentDetails.TravelFutureFlightCredit.TotalRedeemAmount;
            }
            UpdatePricesInReservation(response.ShoppingCart.FormofPaymentDetails.TravelFutureFlightCredit, response.Reservation.Prices, appId: request.Application.Id, appVersion: request.Application.Version.Major, isManageTravelCreditAPI: true);
            AssignIsOtherFOPRequired(response.ShoppingCart.FormofPaymentDetails, response.Reservation.Prices);
            AssignFormOfPaymentType(response.ShoppingCart.FormofPaymentDetails, response.Reservation.Prices, false);  
            if (await _featureSettings.GetFeatureSettingValue("EnableChaseStemtentFixForMoneyPlusMiles-MOBILE-35118"))
                _shoppingCartUtility.UpdateChaseCreditStatement(bookingPathReservation);

            if (lstMessages == null)
            {
                lstMessages = await GetSDLContentByGroupName(request, session.SessionId, session.Token, _configuration.GetValue<string>("CMSContentMessages_GroupName_BookingRTI_Messages"), "BookingPathRTI_CMSContentMessagesCached_StaticGUID");
            }
            response.ShoppingCart.FormofPaymentDetails.TravelFutureFlightCredit.ReviewFFCMessages = BuildReviewFFCHeaderMessage(response.ShoppingCart.FormofPaymentDetails?.TravelFutureFlightCredit, response.Reservation.TravelersCSL, lstMessages);
            response.ShoppingCart.FormofPaymentDetails.TravelFutureFlightCredit.EmailConfirmationFFCMessages = AssignEmailMessageForFFCRefund(lstMessages, response.Reservation.Prices, response.ShoppingCart.FormofPaymentDetails.EmailAddress, response.ShoppingCart.FormofPaymentDetails.TravelFutureFlightCredit, request.Application);

            response.ShoppingCart.SCTravelers = (response.Reservation.TravelersCSL != null && response.Reservation.TravelersCSL.Count() > 0) ? response.Reservation.TravelersCSL : null;
        }

        private bool IsInclueWithThisToggle(int appId, string appVersion, string configToggleKey, string androidVersion, string iosVersion)
        {
            return _configuration.GetValue<bool>(configToggleKey) &&
                   GeneralHelper.IsApplicationVersionGreater(appId, appVersion, androidVersion, iosVersion, "", "", true, _configuration);
        }

        private async Task<MOBFOPResponse> LookUpTravelCredit
        (Session session, MOBFOPLookUpTravelCreditRequest request)
        {
            var response = new MOBFOPResponse();
            Reservation bookingPathReservation = new Reservation();


            if (session == null || request == null)
            {
                throw new Exception("empty session or empty request");
            }

            var persistShoppingCart = new MOBShoppingCart();
            persistShoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(session.SessionId, persistShoppingCart.ObjectName, new List<string> { session.SessionId, persistShoppingCart.ObjectName }).ConfigureAwait(false);

            IsRequestCertifiateAlreadyApplied(persistShoppingCart?.FormofPaymentDetails?.TravelCreditDetails?.TravelCredits, request.PinOrPnr);
            response.ShoppingCart = persistShoppingCart;
            response.Profiles = await LoadPersistedProfile(session.SessionId, response.ShoppingCart.Flow);
            if (response?.ShoppingCart?.FormofPaymentDetails == null)
            {
                response.ShoppingCart.FormofPaymentDetails = new MOBFormofPaymentDetails();
            }
            
            var travelCreditBusinessResult = await LoadBasicMOBFOPResponse(session, bookingPathReservation);
            response = travelCreditBusinessResult.MobFopResponse;
            bookingPathReservation = travelCreditBusinessResult.BookingPathReservation;


            var reservation = await _sessionHelperService.GetSession<Service.Presentation.ReservationModel.Reservation>(session.SessionId, new Service.Presentation.ReservationModel.Reservation().GetType().FullName, new List<string> { session.SessionId, new Service.Presentation.ReservationModel.Reservation().GetType().FullName }).ConfigureAwait(false);
            string url = "/ECD/TCLookupByPinOrPNR";

            string cslRrequest = GetCSLLookupRequest(request, response, session, reservation);

            string cslresponse = await _paymentService.GetLookUpTravelCredit(session.Token, url, cslRrequest, session.SessionId);

            var cSLresponse = JsonConvert.DeserializeObject<TCLookupByPinOrPNRResponse>(cslresponse);

           await _sessionHelperService.SaveSession<TCLookupByPinOrPNRResponse>(cSLresponse, session.SessionId, new List<string> { session.SessionId, typeof(TCLookupByPinOrPNRResponse).FullName + request.PinOrPnr.ToUpper() + request.LastName.ToUpper() }, typeof(TCLookupByPinOrPNRResponse).FullName + request.PinOrPnr.ToUpper() + request.LastName.ToUpper()).ConfigureAwait(false);

            List<CMSContentMessage> lstMessages
                = await GetSDLContentByGroupName(request, session.SessionId, session.Token,
               _configuration.GetValue<string>("CMSContentMessages_GroupName_BookingRTI_Messages"), "BookingPathRTI_CMSContentMessagesCached_StaticGUID");

            var reviewMessages
                = GetSDLContentMessages(lstMessages, "RTI.TravelCertificate.ReviewTravelCredits");

            var nameWaiverMatchMessage
               = GetSDLMessageFromList(lstMessages, "RTI.TravelCertificate.LookUpTravelCredits.Alert.NameMatchWaiver");

            var lookUpMessages = GetSDLContentMessages(lstMessages, "RTI.TravelCertificate.LookUpTravelCredits");

            if (cSLresponse != null && (cSLresponse?.FFCRCertificates?.CertificateList != null
                || cSLresponse?.FFCCertificates?.CertificateList != null
                || cSLresponse?.ETCCertificate?.CertificateList != null))
            {
                LoadLookUpTravelCreditCSLResponse(request, cSLresponse, response, lookUpMessages);
            }
            else
            {
                string OTF_errorCode = "90102";
                if (request.PinOrPnr.Length == 6)
                {
                    response.Exception = new MOBException
                    {
                        Code = OTF_errorCode,
                        Message = lstMessages.FirstOrDefault(x => string.Equals(x.Title,
                        "RTI.FutureFlightCredits.OTFConversion_Generic_Error1", StringComparison.OrdinalIgnoreCase)).ContentFull,
                    };
                }
                else if (request.PinOrPnr.Length == 10)
                {
                    response.Exception = new MOBException
                    {
                        Code = OTF_errorCode,
                        Message = _configuration.GetValue<string>("ETCInvalidCertificateMessage"),
                    };
                }
            }

            response.ShoppingCart.FormofPaymentDetails.TravelCreditDetails.ReviewMessages
                = (response.ShoppingCart.FormofPaymentDetails.TravelCreditDetails.ReviewMessages == null)
                ? new List<MOBMobileCMSContentMessages>()
                : response.ShoppingCart.FormofPaymentDetails.TravelCreditDetails.ReviewMessages;

            if (reviewMessages != null && reviewMessages.Any())
                response.ShoppingCart.FormofPaymentDetails.TravelCreditDetails.ReviewMessages.AddRange(reviewMessages);

            if (lookUpMessages != null && lookUpMessages.Any())
                response.ShoppingCart.FormofPaymentDetails.TravelCreditDetails.ReviewMessages.AddRange(lookUpMessages);

            response.ShoppingCart.FormofPaymentDetails.TravelCreditDetails.NameWaiverMatchMessage
                = nameWaiverMatchMessage.Count() > 0 ? nameWaiverMatchMessage?[0].ContentFull.ToString() : null;

            await _sessionHelperService.SaveSession<MOBShoppingCart>(response.ShoppingCart, session.SessionId, new List<string> { session.SessionId, response.ShoppingCart.ObjectName }, response.ShoppingCart.ObjectName).ConfigureAwait(false);

            _logger.LogInformation("LookUpTravelCredit Response:{response} with sessionId:{sessionId}", JsonConvert.SerializeObject(response), session.SessionId);

            return response;
        }

        private string GetCSLLookupRequest(MOBFOPLookUpTravelCreditRequest request, MOBFOPResponse response, Session session, Service.Presentation.ReservationModel.Reservation reservation)
        {
            TCLookupByPinOrPNRRequest
                cslRequest = new TCLookupByPinOrPNRRequest();

            cslRequest.CallingService = new ServiceClient
            {
                AccessCode = _configuration.GetValue<string>("FFCAccessCode"),
                Requestor = new Requestor
                {
                    AgentAAA = "HQS",
                    AgentSine = "UA",
                    ApplicationSource = "Mobile",
                    Device = new Service.Presentation.CommonModel.Device
                    {
                        LNIATA = "Mobile",
                    },
                    DutyCode = "SU",
                }
            };

            //Data
            cslRequest.CertpinOrPNR = request.PinOrPnr;
            cslRequest.LastName = request.LastName;
            cslRequest.Reservation = (cslRequest.Reservation == null)
                ? new Service.Presentation.ReservationModel.Reservation() : cslRequest.Reservation;
            cslRequest.Reservation = reservation;

            string jsonRequest
                = JsonConvert.SerializeObject(cslRequest);


            _logger.LogInformation("GetCSLLookupRequest request:{request} with sessionId:{sessionId}", JsonConvert.SerializeObject(jsonRequest), session.SessionId);
            return jsonRequest;
        }

        private async Task<(MOBFOPResponse MobFopResponse, Reservation BookingPathReservation)> LoadBasicMOBFOPResponse(Session session, Reservation bookingPathReservation)
        {
            var response = new MOBFOPResponse();
            bookingPathReservation = new Reservation();
            bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(session.SessionId, bookingPathReservation.ObjectName, new List<string> { session.SessionId, bookingPathReservation.ObjectName }).ConfigureAwait(false);
            response.Reservation = new MOBSHOPReservation(_configuration, _cachingService);
            response.Reservation = MakeReservationFromPersistReservation(response.Reservation, bookingPathReservation, session);
            await response.Reservation.UpdateRewards(_configuration, _cachingService);

            var persistShoppingCart = new MOBShoppingCart();
            persistShoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(session.SessionId, persistShoppingCart.ObjectName, new List<string> { session.SessionId, persistShoppingCart.ObjectName }).ConfigureAwait(false);

            response.ShoppingCart = persistShoppingCart;
            response.Profiles = await LoadPersistedProfile(session.SessionId, response.ShoppingCart.Flow);
            if (response?.ShoppingCart?.FormofPaymentDetails == null)
            {
                response.ShoppingCart.FormofPaymentDetails = new MOBFormofPaymentDetails();
            }

            _logger.LogInformation("LoadBasicMOBFOPResponse Response{response} with SessionId{sessoinId}", JsonConvert.SerializeObject(response), session.SessionId);

            return (response, bookingPathReservation);
        }

        private MOBSHOPReservation MakeReservationFromPersistReservation(Reservation reservation)
        {
            return new MOBSHOPReservation(_configuration,_cachingService)
            {
                SessionId = reservation.SessionId,
                IsSignedInWithMP = reservation.IsSignedInWithMP,
                NumberOfTravelers = reservation.NumberOfTravelers,
                SearchType = reservation.SearchType,
                TravelersRegistered = reservation.TravelersRegistered,
                CartId = reservation.CartId,
                CreditCards = reservation.CreditCards.Clone(),
                CreditCardsAddress = reservation.CreditCardsAddress.Clone(),
                Prices = reservation.Prices.Clone(),
                PointOfSale = reservation.PointOfSale,
                ClubPassPurchaseRequest = reservation.ClubPassPurchaseRequest,
                FareLock = reservation.FareLock,
                FareRules = reservation.FareRules,
                UnregisterFareLock = reservation.UnregisterFareLock,
                AwardTravel = reservation.AwardTravel,
                LMXFlights = reservation.LMXFlights,
                lmxtravelers = reservation.LMXTravelers,
                ReservationEmail = reservation.ReservationEmail.Clone(),
                ReservationPhone = reservation.ReservationPhone.Clone(),
                IneligibleToEarnCreditMessage = reservation.IneligibleToEarnCreditMessage,
                OaIneligibleToEarnCreditMessage = reservation.OaIneligibleToEarnCreditMessage,
                IsELF = reservation.IsELF,
                IsSSA = reservation.IsSSA,
                IsMetaSearch = reservation.IsMetaSearch,
                MetaSessionId = reservation.MetaSessionId,
                IsUpgradedFromEntryLevelFare = reservation.IsUpgradedFromEntryLevelFare,
                IsReshopChange = reservation.IsReshopChange,
                ReshopTrips = reservation.ReshopTrips,
                Reshop = reservation.Reshop,
                ShopReservationInfo = reservation.ShopReservationInfo,
                ShopReservationInfo2 = reservation.ShopReservationInfo2,
                SeatAssignmentMessage = reservation.SeatAssignmentMessage,
                TripInsuranceInfoBookingPath = reservation.TripInsuranceFile == null ? null : reservation.TripInsuranceFile.TripInsuranceBookingInfo,
                AlertMessages = reservation.AlertMessages,
                IsRedirectToSecondaryPayment = reservation.IsRedirectToSecondaryPayment,
                RecordLocator = reservation.RecordLocator,
                Messages = reservation.Messages,
                TCDAdvisoryMessages = reservation.TCDAdvisoryMessages,
                CheckedbagChargebutton = reservation.CheckedbagChargebutton,
                TravelOptions = reservation.TravelOptions,
                Trips = reservation.Trips
            };
        }

        private async Task<List<MOBCPProfile>> LoadPersistedProfile(string sessionId, string flow)
        {
            switch (flow)
            {
                case "BOOKING":   //profile Object load
                    ProfileResponse persistedProfileResponse = new ProfileResponse();
                    persistedProfileResponse = await _sessionHelperService.GetSession<ProfileResponse>(sessionId, persistedProfileResponse.ObjectName, new List<string> { sessionId, persistedProfileResponse.ObjectName }).ConfigureAwait(false);

                    return persistedProfileResponse != null ? persistedProfileResponse.Response.Profiles : null;
                case "VIEWRES":
                    United.Persist.Definition.FOP.ProfileFOPCreditCardResponse profilePersist = new ProfileFOPCreditCardResponse();
                    profilePersist = await _sessionHelperService.GetSession<ProfileFOPCreditCardResponse>(sessionId, profilePersist.ObjectName, new List<string> { sessionId, profilePersist.ObjectName }).ConfigureAwait(false);
                    return profilePersist != null ? profilePersist.Response.Profiles : null;
                default: return null;
            }
        }

        private void IsRequestCertifiateAlreadyApplied(List<MOBFOPTravelCredit> persistedCertificates, string pincode)
        {
            if (persistedCertificates != null && persistedCertificates.Exists(c => c.PinCode == pincode && c.TravelCreditType == MOBTravelCreditFOP.ETC))
            {
                throw new MOBUnitedException(_configuration.GetValue<string>("ETCAlreadyAppliedToTravelCredit"));
            }
        }

        private void LoadLookUpTravelCreditCSLResponse
            (MOBFOPLookUpTravelCreditRequest mobRequest, TCLookupByPinOrPNRResponse cslResponse,
            MOBFOPResponse mobResponse, List<MOBMobileCMSContentMessages> lookUpMessages)
        {
            mobResponse.ShoppingCart.FormofPaymentDetails.TravelCreditDetails
                            = (mobResponse.ShoppingCart.FormofPaymentDetails.TravelCreditDetails == null)
                            ? new MOBFOPTravelCreditDetails()
                            : mobResponse.ShoppingCart.FormofPaymentDetails.TravelCreditDetails;

            List<MOBFOPTravelCredit> travelCredits = mobResponse.ShoppingCart.FormofPaymentDetails.TravelCreditDetails.TravelCredits;
            travelCredits = travelCredits ?? new List<MOBFOPTravelCredit>();

            //Swaping title is missed in the guest flow
            if (!_configuration.GetValue<bool>("EnableAwardOTF"))
                SwapTitleAndLocation(lookUpMessages);

            var etc = cslResponse?.ETCCertificate;
            var ffcr = cslResponse?.FFCRCertificates;
            var ffc = cslResponse?.FFCCertificates;

            AddETCToTC(travelCredits, etc, true);

            AddFFCandFFCR(mobResponse.Reservation.TravelersCSL,
                travelCredits, ffc, lookUpMessages, true, true);

            AddFFCandFFCR(mobResponse.Reservation.TravelersCSL,
                travelCredits, ffcr, lookUpMessages, false, true);

            mobResponse.ShoppingCart.FormofPaymentDetails.TravelCreditDetails.TravelCredits = travelCredits.OrderBy(x => DateTime.Parse(x.ExpiryDate)).ToList();

        }

        private void AddFFCandFFCR(List<MOBCPTraveler> travelers, List<MOBFOPTravelCredit> travelCredits, FFCRCertificates ffcr, List<MOBMobileCMSContentMessages> lookUpMessages, bool isOtfFFC, bool islookUp)
        {
            if (ffcr != null)
            {
                Collection<FFCRCertificate> certificates = ffcr.CertificateList;
                if (certificates != null)
                {
                    if (travelCredits == null)
                        travelCredits = new List<MOBFOPTravelCredit>();
                    foreach (var item in certificates)
                    {
                        foreach (var travelCredit in item.TravelCreditList)
                        {
                            var item1 = ConvertToFFC(travelCredit, travelers, lookUpMessages, isOtfFFC, item.FirstName, item.LastName, ffcr.Errors, islookUp);
                            if (item1 != null)
                            {
                                    if (!_configuration.GetValue<bool>("Disable_Ignore_FFCR_PNR_CaseCheck"))
                                    {
                                        if (!travelCredits.Exists(tc => String.Equals(tc.PinCode, item1.PinCode, StringComparison.OrdinalIgnoreCase) && String.Equals(tc.RecordLocator, item1.RecordLocator, StringComparison.OrdinalIgnoreCase)))
                                            travelCredits.Add(item1);
                                    }
                                    else
                                    {
                                        if (!travelCredits.Exists(tc => tc.PinCode == item1.PinCode && tc.RecordLocator == item1.RecordLocator))
                                            travelCredits.Add(item1);
                                    }
                            }
                        }
                    }
                }
            }
        }

        private MOBFOPTravelCredit ConvertToFFC(Service.Presentation.PaymentModel.TravelCredit cslTravelCredit, List<MOBCPTraveler> travelers, List<MOBMobileCMSContentMessages> lookUpMessages, bool isOtfFFC, string firstName, string lastName, Collection<TCError> errors, bool islookUp)
        {
            MOBFOPTravelCredit travelCredit = null;
            try
            {
                travelCredit = new MOBFOPTravelCredit();
                string initialValue = cslTravelCredit.InitialValue;
                string origIssueDate = cslTravelCredit.OrigIssueDate;
                if (string.IsNullOrEmpty(initialValue))
                {
                    initialValue = cslTravelCredit.CurrentValue;
                }
                initialValue = initialValue.Trim('$');
                if (string.IsNullOrEmpty(origIssueDate))
                {
                    origIssueDate = cslTravelCredit.PNRCreateDate;
                }
                travelCredit.IsLookupCredit = islookUp;
                travelCredit.CreditAmount = $"${string.Format("{0:0.00}", Math.Round(Convert.ToDouble(cslTravelCredit.CurrentValue), 2, MidpointRounding.AwayFromZero))}";
                travelCredit.InitialValue = Math.Round(Convert.ToDouble(initialValue), 2, MidpointRounding.AwayFromZero);
                travelCredit.CurrentValue = Math.Round(Convert.ToDouble(cslTravelCredit.CurrentValue), 2, MidpointRounding.AwayFromZero);
                travelCredit.DisplayNewValueAfterRedeem = $"${string.Format("{0:0.00}", Math.Round(Convert.ToDouble(cslTravelCredit.CurrentValue), 2, MidpointRounding.AwayFromZero))}";
                travelCredit.DisplayRedeemAmount = "0.00";
                travelCredit.ExpiryDate = Convert.ToDateTime(cslTravelCredit.CertExpDate).ToString("MMMM dd, yyyy");
                travelCredit.IsApplied = false;
                travelCredit.IsRemove = false;
                travelCredit.NewValueAfterRedeem = Math.Round(Convert.ToDouble(cslTravelCredit.CurrentValue), 2, MidpointRounding.AwayFromZero);
                travelCredit.PinCode = isOtfFFC ? cslTravelCredit.OrigTicketNumber : cslTravelCredit.CertPin;
                travelCredit.PromoCode = cslTravelCredit.PromoID;
                travelCredit.RecordLocator = cslTravelCredit.OrigPNR;
                travelCredit.RedeemAmount = 0;
                travelCredit.TravelCreditType = MOBTravelCreditFOP.FFC;
                travelCredit.YearIssued = Convert.ToDateTime(origIssueDate).ToString("MMMM dd, yyyy");
                travelCredit.Recipient = $"{firstName} {lastName}";
                travelCredit.FirstName = firstName;
                travelCredit.LastName = lastName;
                travelCredit.IsOTFFFC = isOtfFFC;
                travelCredit.IsNameMatch = cslTravelCredit.Travellers.Exists(tcTraveler => Convert.ToBoolean(tcTraveler.IsNameMatch));
                travelCredit.IsNameMatchWaiverApplied = cslTravelCredit.Travellers.Exists(tcTraveler => Convert.ToBoolean(Convert.ToBoolean(tcTraveler.IsNameMatchWaiverApplied))
                                                                                                        && !Convert.ToBoolean(tcTraveler.IsNameMatch));
                travelCredit.IsTravelDateBeginsBeforeCertExpiry = cslTravelCredit.Travellers.Exists(tcTraveler => Convert.ToBoolean(tcTraveler.IsTravelDateBeginsBeforeCertExpiry));
                travelCredit.IsEligibleToRedeem = cslTravelCredit.Travellers.Exists(tcTraveler => Convert.ToBoolean(tcTraveler.IsEligibleToRedeem));

                if (_configuration.GetValue<bool>("EnableU4BCorporateName"))
                {
                    if (_configuration.GetSection("EnableU4BCorporateBookingFFC").GetValue<bool>("isEnable"))
                    {
                        travelCredit.IsCorporateTravelCreditText = String.Equals(cslTravelCredit.IsCorporateBooking, "true", StringComparison.OrdinalIgnoreCase) ? _configuration.GetSection("EnableU4BCorporateBookingFFC").GetValue<string>("u4BCorporateText") : String.Empty;
                    }
                    travelCredit.CorporateName = cslTravelCredit?.CorporateName;
                }
                travelCredit.Captions = TravelCreditErrorMapping
                    (errors, lookUpMessages, cslTravelCredit);

                if (!_configuration.GetValue<bool>("EnableAwardOTF"))
                {
                    //var _errorMSG = new string[] { "Mnr22", "Mnr23" }; // List of error messages for Award OTF
                    travelCredit.IsAwardOTFEligible = errors.IsNullOrEmpty() ? false :
                                            !errors.Where(t => string.Equals(t.RecordLocator, cslTravelCredit.OrigPNR, StringComparison.OrdinalIgnoreCase) && //PNR Check
                                             ((string.Equals(t.MajorCode, "Msg1", StringComparison.OrdinalIgnoreCase) && string.Equals(t.MinorCode, "Mnr22", StringComparison.OrdinalIgnoreCase)) ||
                                             (string.Equals(t.MajorCode, "Err2", StringComparison.OrdinalIgnoreCase) && string.Equals(t.MinorCode, "Mnr23", StringComparison.OrdinalIgnoreCase)))).IsNullOrEmpty();
                }

                if (!travelCredit.IsEligibleToRedeem
                    && (travelCredit.Captions == null || !travelCredit.Captions.Any()))
                {
                    travelCredit.Captions = SetErrorMessage2(lookUpMessages);
                }
                if (!_configuration.GetValue<bool>("EnableAwardOTF"))
                {
                    var _errorMSG = new string[] { "Mnr22", "Mnr23" }; // List of error messages for Award OTF
                    travelCredit.IsAwardOTFEligible = errors.IsNullOrEmpty() ? false :
                                            !errors.Where(t => string.Equals(t.RecordLocator, cslTravelCredit.OrigPNR, StringComparison.OrdinalIgnoreCase) && //PNR Check
                                             ((string.Equals(t.MajorCode, "Msg1", StringComparison.OrdinalIgnoreCase) && string.Equals(t.MinorCode, "Mnr22", StringComparison.OrdinalIgnoreCase)) ||
                                             (string.Equals(t.MajorCode, "Err2", StringComparison.OrdinalIgnoreCase) && string.Equals(t.MinorCode, "Mnr23", StringComparison.OrdinalIgnoreCase)))).IsNullOrEmpty();
                }

                travelCredit.IsEligibleToRedeem
                    = (travelCredit.IsEligibleToRedeem && (travelCredit.Captions == null || !travelCredit.Captions.Any()));

                var csltravelers = cslTravelCredit.Travellers.Where(t => Convert.ToBoolean(t.IsEligibleToRedeem) ||
                                                                  Convert.ToBoolean(t.IsNameMatchWaiverApplied));
                if (csltravelers != null)
                {
                    travelCredit.EligibleTravelerNameIndex = new List<string>();
                    travelCredit.EligibleTravelers = new List<string>();
                    foreach (var csltraveler in csltravelers)
                    {
                        var traveler = travelers.FirstOrDefault(t => t.TravelerNameIndex == csltraveler?.PaxIndex);
                        travelCredit.TravelerNameIndex += traveler.TravelerNameIndex + ",";
                        travelCredit.EligibleTravelerNameIndex.Add(traveler.TravelerNameIndex);
                        if (!Convert.ToBoolean(csltraveler.IsNameMatchWaiverApplied))
                            travelCredit.EligibleTravelers.Add(traveler.TravelerNameIndex);
                    }
                }
                travelCredit.IsHideShowDetails = false;

                if (_configuration.GetValue<bool>("EnableMFOP"))
                {
                    travelCredit.CertificateNumber = cslTravelCredit.CertificateNumber;
                    travelCredit.CsltravelCreditType = cslTravelCredit.TravelCreditType;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError("ConvertToFFC Exception:{exceprion} with sessionId:{sessionId}", JsonConvert.SerializeObject(ex), _headers.ContextValues.SessionId);
            }

            return travelCredit;
        }

        private List<MOBTypeOption> SetErrorMessage2(List<MOBMobileCMSContentMessages> lookUpMessages)
        {
            var errordata = lookUpMessages?.FirstOrDefault
                   (x => string.Equals(x.Title, "RTI.TravelCertificate.LookUpTravelCredits.ErrorMessage2", StringComparison.OrdinalIgnoreCase));

            if (errordata != null)
            {
                return new List<MOBTypeOption> {
                            new MOBTypeOption{ Key = "NAVIGATE", Value = "WEB"},
                            new MOBTypeOption{ Key = "URL", Value = errordata.ContentShort},
                            new MOBTypeOption{ Key = "BUTTONTXT", Value = errordata.HeadLine},
                            new MOBTypeOption{ Key = "HEADERTXT", Value = errordata.ContentFull  },
                };
            }
            return null;
        }

        private List<MOBTypeOption> TravelCreditErrorMapping(Collection<TCError> errors, List<MOBMobileCMSContentMessages> lookUpMessages,
            Service.Presentation.PaymentModel.TravelCredit cslTravelCredit)
        {
            List<MOBTypeOption> mobcaptions = null;

            if (errors == null || !errors.Any()) return null;

            var selectedError = errors.FirstOrDefault
                (x => string.Equals(x.RecordLocator, cslTravelCredit.OrigPNR, StringComparison.OrdinalIgnoreCase));

            if (selectedError == null || string.IsNullOrEmpty(selectedError.MajorCode)) return null;

            mobcaptions = (mobcaptions == null) ? new List<MOBTypeOption>() : mobcaptions;


            if (string.Equals(selectedError.MajorCode, _MSG1, StringComparison.OrdinalIgnoreCase))
            {
                var errordata = lookUpMessages?.FirstOrDefault
                    (x => string.Equals(x.Title, "RTI.TravelCertificate.LookUpTravelCredits.ErrorMessage1", StringComparison.OrdinalIgnoreCase));

                if (errordata != null)
                {
                    mobcaptions = new List<MOBTypeOption> {
                            new MOBTypeOption{ Key = "NAVIGATE", Value = "MOBILE"},
                            new MOBTypeOption{ Key = "BUTTONTXT", Value = errordata.ContentShort },
                            new MOBTypeOption{ Key = "HEADERTXT", Value =  errordata.ContentFull },
                };
                }
            }
            else if (string.Equals(selectedError.MajorCode, _ERR2, StringComparison.OrdinalIgnoreCase))
            {
                //var selectedErr2 = errors.Select
                //(x => x.Characteristics.Where(y => string.Equals(y.Code, "OrigTicketNumber", StringComparison.OrdinalIgnoreCase))).FirstOrDefault();

                mobcaptions = SetErrorMessage2(lookUpMessages);
            }
            else if (string.Equals(selectedError.MajorCode, _ERR3, StringComparison.OrdinalIgnoreCase))
            {
                var errordata = lookUpMessages?.FirstOrDefault
                  (x => string.Equals(x.Title, "RTI.TravelCertificate.LookUpTravelCredits.ErrorMessage3", StringComparison.OrdinalIgnoreCase));

                if (errordata != null)
                {
                    mobcaptions = new List<MOBTypeOption> {
                            new MOBTypeOption{ Key = "NAVIGATE", Value = "NONE"},
                            new MOBTypeOption{ Key = "HEADERTXT", Value = string.Format(errordata.ContentFull,cslTravelCredit.CertExpDate) },
                };
                }
            }
            else if (string.Equals(selectedError.MajorCode, _ERR4, StringComparison.OrdinalIgnoreCase))
            {
                var errordata = lookUpMessages?.FirstOrDefault
                  (x => string.Equals(x.Title, "RTI.TravelCertificate.LookUpTravelCredits.ErrorMessage4", StringComparison.OrdinalIgnoreCase));

                if (errordata != null)
                {
                    mobcaptions = new List<MOBTypeOption> {
                            new MOBTypeOption{ Key = "NAVIGATE", Value = "NONE"},
                            new MOBTypeOption{ Key = "HEADERTXT", Value = errordata.ContentFull },
                };
                }
            }
            else if (string.Equals(selectedError.MajorCode, _ERR5, StringComparison.OrdinalIgnoreCase))
            {
                var errordata = lookUpMessages?.FirstOrDefault
                  (x => string.Equals(x.Title, "RTI.TravelCertificate.LookUpTravelCredits.ErrorMessage5", StringComparison.OrdinalIgnoreCase));

                if (errordata != null)
                {
                    mobcaptions = new List<MOBTypeOption> {
                            new MOBTypeOption{ Key = "NAVIGATE", Value = "WEB"},
                            new MOBTypeOption{ Key = "URL", Value = errordata.ContentShort},
                            new MOBTypeOption{ Key = "BUTTONTXT", Value = errordata.HeadLine},
                            new MOBTypeOption{ Key = "HEADERTXT", Value = errordata.ContentFull },
                };
                }
            }

            return (mobcaptions == null || !mobcaptions.Any()) ? null : mobcaptions;
        }

        private void AddETCToTC(List<MOBFOPTravelCredit> travelCredits, ETCCertificates etc, bool islookUp)
        {
            //Load ETC
            if (etc?.CertificateList != null && etc.CertificateList.Any())
            {
                foreach (var item in etc.CertificateList)
                {
                    travelCredits.Add(new MOBFOPTravelCredit()
                    {
                        IsLookupCredit = islookUp,
                        CreditAmount = $"${string.Format("{0:0.00}", Math.Round(Convert.ToDouble(item.CurrentValue), 2, MidpointRounding.AwayFromZero))}",
                        CurrentValue = Math.Round(Convert.ToDouble(item.CurrentValue), 2, MidpointRounding.AwayFromZero),
                        DisplayNewValueAfterRedeem = $"${string.Format("{0:0.00}", Math.Round(Convert.ToDouble(item.CurrentValue), 2, MidpointRounding.AwayFromZero))}",
                        DisplayRedeemAmount = "0.00",
                        ExpiryDate = Convert.ToDateTime(item.CertExpDate).ToString("MMMM dd, yyyy"),
                        InitialValue = Math.Round(Convert.ToDouble(item.InitialValue), 2, MidpointRounding.AwayFromZero),
                        IsRemove = false,
                        IsEligibleToRedeem = true,
                        NewValueAfterRedeem = Math.Round(Convert.ToDouble(item.CurrentValue), 2, MidpointRounding.AwayFromZero),
                        PinCode = item.CertPin,
                        PromoCode = item.PromoID,
                        RecordLocator = item.PNR,
                        RedeemAmount = 0,
                        TravelCreditType = MOBTravelCreditFOP.ETC,
                        YearIssued = Convert.ToDateTime(item.OrigIssueDate).ToString("MMMM dd, yyyy"),
                        Recipient = $"{item.FirstName} {item.LastName}",
                        IsHideShowDetails = false,
                        LastName = _configuration.GetValue<bool>("EnableMFOP") ? item.LastName : string.Empty,
                        FirstName = _configuration.GetValue<bool>("EnableMFOP") ? item.FirstName : string.Empty
                    });
                }
            }//End ETC
        }

        private List<MOBMobileCMSContentMessages> GetSDLContentMessages(List<CMSContentMessage> lstMessages, string title)
        {
            List<MOBMobileCMSContentMessages> messages = new List<MOBMobileCMSContentMessages>();
            messages.AddRange(GetSDLMessageFromList(lstMessages, title));

            return messages;
        }

        private async Task <MOBFutureFlightCreditResponse> RemoveFutureFlightCredit(MOBFOPFutureFlightCreditRequest request, Session session)
        {
            MOBFutureFlightCreditResponse response = new MOBFutureFlightCreditResponse();
            await LoadSessionValuesToResponse(session, response);

            List<MOBFOPFutureFlightCredit> futureFlightCredits = response?.ShoppingCart?.FormofPaymentDetails?.TravelFutureFlightCredit?.FutureFlightCredits;
            if (futureFlightCredits != null)
            {
                var removeFFCtraveler = response.Reservation.TravelersCSL.Find(t => t.TravelerNameIndex == request.FutureFlightCredit.TravelerNameIndex && t.PaxID == request.FutureFlightCredit.PaxId);
                removeFFCtraveler.FutureFlightCredits = RemoveRequestedFFCandUpdateNewValueForRemainingFFCs(request.FutureFlightCredit, removeFFCtraveler.FutureFlightCredits, removeFFCtraveler.IndividualTotalAmount);
                AssignTravelerTotalFFCNewValueAfterReDeem(removeFFCtraveler);
                futureFlightCredits.RemoveAll(ffc => ffc.TravelerNameIndex == request.FutureFlightCredit.TravelerNameIndex && ffc.PaxId == request.FutureFlightCredit.PaxId);
                if (removeFFCtraveler.FutureFlightCredits != null)
                {
                    futureFlightCredits.AddRange(removeFFCtraveler.FutureFlightCredits);
                }
            }
            if (!_configuration.GetValue<bool>("disable21GFFCToggle"))
            {
                 response.PkDispenserPublicKey = await _mSCPkDispenserPublicKey.GetCachedOrNewpkDispenserPublicKey(request.Application.Id, request.Application.Version.Major, request.DeviceId, session.SessionId, session.Token, session?.CatalogItems).ConfigureAwait(false);
            }

            await SaveSCAndReservationToPersistWithFFCTraveler(session, response, request, null);

            _logger.LogInformation("RemoveFutureFlightCredit Response:{response} with sessionid:{sessionId}", JsonConvert.SerializeObject(response), session.SessionId);

            return response;
        }

        private async Task<MOBFutureFlightCreditResponse> ManageFutureFlightCredit(MOBFutureFlightCreditRequest mOBFutureFlightCredit, Session session)
        {
            MOBFutureFlightCreditResponse response = new MOBFutureFlightCreditResponse();
            await LoadSessionValuesToResponse(session, response);
            List<CMSContentMessage> lstMessages
                = await GetSDLContentByGroupName(mOBFutureFlightCredit, session.SessionId, session.Token,
               _configuration.GetValue<string>("CMSContentMessages_GroupName_BookingRTI_Messages"), "BookingPathRTI_CMSContentMessagesCached_StaticGUID");

            bool isOnTheFlyConversionEnabled
                = OnTheFlyConversionEnabled(mOBFutureFlightCredit.Application.Id, mOBFutureFlightCredit.Application.Version.Major);

            if (!string.IsNullOrEmpty(mOBFutureFlightCredit.LastName) && !string.IsNullOrEmpty(mOBFutureFlightCredit.RecordLocator))
            {
                LookupByPnrResponse cslResponse = null;
                OTFConversion.Response.OnTheFlyOfferEligibilityResponse cslOTFResponse = null;
                string reponseOTFString = string.Empty;
                if (isOnTheFlyConversionEnabled)
                {
                    reponseOTFString = await OTFConversionByPnr(mOBFutureFlightCredit, response, session.Token);
                    cslOTFResponse = JsonConvert.DeserializeObject
                        <OTFConversion.Response.OnTheFlyOfferEligibilityResponse>(reponseOTFString);
                }
                else
                {
                    string reponseString = await GetFFCByPnr((MOBRequest)mOBFutureFlightCredit, mOBFutureFlightCredit.RecordLocator,
                        mOBFutureFlightCredit.LastName, mOBFutureFlightCredit.SessionId, response);
                    cslResponse = JsonConvert.DeserializeObject<LookupByPnrResponse>(reponseString);
                }

                var travelFutureFlightCredit = response.ShoppingCart.FormofPaymentDetails.TravelFutureFlightCredit;
                if (travelFutureFlightCredit == null)
                    travelFutureFlightCredit = new MOBFOPTravelFutureFlightCredit();

                //load FFC object 
                if ((cslResponse != null && cslResponse.IsSuccessful && cslResponse.TravelCreditDetails?.Count > 0)
                    || (cslOTFResponse != null && cslOTFResponse.OfferEligible && (cslOTFResponse.Error == null || !cslOTFResponse.Error.Any())))
                {
                    travelFutureFlightCredit.AllowedFFCAmount = GetAllowedFFCAmount(response.ShoppingCart.Products);

                    if (travelFutureFlightCredit.FutureFlightCredits == null)
                        travelFutureFlightCredit.FutureFlightCredits = new List<MOBFOPFutureFlightCredit>();

                    if (isOnTheFlyConversionEnabled)
                    {
                        travelFutureFlightCredit.FutureFlightCredits
                            = AssignOTFTravelerFFCs(cslOTFResponse, travelFutureFlightCredit.FutureFlightCredits, response.Reservation);

                        await _sessionHelperService.SaveSession<string>(reponseOTFString, session.SessionId, new List<string> { session.SessionId, typeof(OTFConversion.Response.OnTheFlyOfferEligibilityResponse).FullName + mOBFutureFlightCredit.RecordLocator }, typeof(OTFConversion.Response.OnTheFlyOfferEligibilityResponse).FullName + mOBFutureFlightCredit.RecordLocator).ConfigureAwait(false);
                    }
                    else
                    {
                        travelFutureFlightCredit.FutureFlightCredits
                            = AssignTravelerFFCs(cslResponse.TravelCreditDetails, response.Reservation.TravelersCSL,
                           travelFutureFlightCredit.FutureFlightCredits, lstMessages, response.Reservation?.Trips[0]?.DepartDate);
                    }

                    travelFutureFlightCredit.FFCButtonText = "Apply";

                    travelFutureFlightCredit.LearnmoreTermsandConditions = GetSDLMessageFromList(lstMessages, "RTI.FutureFlightCredits.ReviewFFC.LNMore");
                    response.ShoppingCart.FormofPaymentDetails.TravelFutureFlightCredit = travelFutureFlightCredit;
                    response.PkDispenserPublicKey = await _mSCPkDispenserPublicKey.GetCachedOrNewpkDispenserPublicKey(mOBFutureFlightCredit.Application.Id, mOBFutureFlightCredit.Application.Version.Major, mOBFutureFlightCredit.DeviceId, session.SessionId, session.Token, session?.CatalogItems).ConfigureAwait(false);
                    await SaveSCAndReservationToPersistWithFFCTraveler(session, response, mOBFutureFlightCredit, lstMessages);
                }
                else
                {
                    if (isOnTheFlyConversionEnabled)
                    {
                        OTFErrorAndRedirectHandling(cslOTFResponse, mOBFutureFlightCredit, response, lstMessages);
                    }
                    else
                    {
                        throw new MOBUnitedException("90101",
                            _configuration.GetValue<string>("FFC_90101_ErrorPopup_ErrorMessage"));
                    }
                }
            }
            else if (!string.IsNullOrEmpty(mOBFutureFlightCredit.Email))
            {
                string responseString = await GetFFCByEmail((MOBRequest)mOBFutureFlightCredit, mOBFutureFlightCredit.Email, mOBFutureFlightCredit.SessionId, response);
                var cslResponse = JsonConvert.DeserializeObject<LookupByEmailResponse>(responseString);
                List<MOBMobileCMSContentMessages> findFFCConfirmationMessageSuccess = null;
                var travelFutureFlightCredit = new MOBFOPTravelFutureFlightCredit();
                travelFutureFlightCredit.FutureFlightCredits = new List<MOBFOPFutureFlightCredit>();
                string errormessage = string.Empty;
                var isEnableTravelCredit = IncludeTravelCredit(mOBFutureFlightCredit.Application.Id, mOBFutureFlightCredit.Application.Version.Major);
                //load FFC object 
                if (cslResponse != null && cslResponse.IsSuccessful)
                {
                    string title = isEnableTravelCredit ? "RTI.TravelCertificate.LookUpTravelCredits.Alert.FindTCByEmail" : "RTI.FutureFlightCredits.FindFFC.Success";
                    findFFCConfirmationMessageSuccess = GetSDLMessageFromList(lstMessages, title);
                    findFFCConfirmationMessageSuccess[0].ContentFull = string.Format(findFFCConfirmationMessageSuccess[0].ContentFull, mOBFutureFlightCredit.Email?.Trim());
                    travelFutureFlightCredit.FindFFCConfirmationMessage = new MOBSection { Text1 = findFFCConfirmationMessageSuccess[0].ContentFull, Text2 = "Continue" };
                }
                else if (cslResponse != null && !cslResponse.IsSuccessful)
                {
                    if (!isEnableTravelCredit)
                    {
                        errormessage = GetSDLMessageFromList(lstMessages, "RTI.FutureFlightCredits.FindFFC.Error")?.FirstOrDefault()?.ContentFull;
                        throw new MOBUnitedException(errormessage);
                    }
                    else
                    {
                        errormessage = string.Format(GetSDLMessageFromList(lstMessages, "RTI.TravelCertificate.LookUpTravelCredits.Alert.FindTCByEmail")[0].ContentFull, mOBFutureFlightCredit.Email?.Trim());
                    }
                }

                if (isEnableTravelCredit)
                {
                    try
                    {
                        response.ShoppingCart.FormofPaymentDetails.TravelCreditDetails.FindETCConfirmationMessage = await SendETCByEmail(mOBFutureFlightCredit, session);
                    }
                    catch (Exception ex)
                    {
                        errormessage = ex.Message.ToString();
                    }
                }
                // Condition will work for both ETC and FFC flow.
                if (!errormessage.IsNullOrEmpty())
                    throw new MOBUnitedException(errormessage);
                response.ShoppingCart.FormofPaymentDetails.TravelFutureFlightCredit = travelFutureFlightCredit;
            }

            _logger.LogInformation("FutureFlightCredit Response{response} with SessionId{sessoinId}", JsonConvert.SerializeObject(response), mOBFutureFlightCredit.SessionId);

            return response;
        }

        public async Task<MOBFutureFlightCreditResponse> FindFutureFlightCreditByEmail(MOBFutureFlightCreditRequest mOBFutureFlightCredit)
        {
            MOBFutureFlightCreditResponse response = new MOBFutureFlightCreditResponse();

            string token = await _dPService.GetAnonymousToken(mOBFutureFlightCredit.Application.Id, mOBFutureFlightCredit.DeviceId, _configuration);

            var session = new Session { SessionId = mOBFutureFlightCredit.SessionId, Token = token };

            await LoadSessionValuesToResponse(session, response);

            List<CMSContentMessage> lstMessages
                = await GetSDLContentByGroupName(mOBFutureFlightCredit, "", token,
               _configuration.GetValue<string>("CMSContentMessages_GroupName_BookingRTI_Messages"), "BookingPathRTI_CMSContentMessagesCached_StaticGUID");

            string responseString = await GetFFCByEmail((MOBRequest)mOBFutureFlightCredit, mOBFutureFlightCredit.Email, session, response);
            var cslResponse = JsonConvert.DeserializeObject<LookupByEmailResponse>(responseString);
            List<MOBMobileCMSContentMessages> findFFCConfirmationMessageSuccess = null;
            var travelFutureFlightCredit = new MOBFOPTravelFutureFlightCredit();
            travelFutureFlightCredit.FutureFlightCredits = new List<MOBFOPFutureFlightCredit>();
            string errormessage = string.Empty;
            var isEnableTravelCredit = IncludeTravelCredit(mOBFutureFlightCredit.Application.Id, mOBFutureFlightCredit.Application.Version.Major);
            //load FFC object 
            if (cslResponse != null && cslResponse.IsSuccessful)
            {
                string title = isEnableTravelCredit ? "RTI.TravelCertificate.LookUpTravelCredits.Alert.FindTCByEmail" : "RTI.FutureFlightCredits.FindFFC.Success";
                findFFCConfirmationMessageSuccess = GetSDLMessageFromList(lstMessages, title);
                findFFCConfirmationMessageSuccess[0].ContentFull = string.Format(findFFCConfirmationMessageSuccess[0].ContentFull, mOBFutureFlightCredit.Email?.Trim());
                travelFutureFlightCredit.FindFFCConfirmationMessage = new MOBSection { Text1 = findFFCConfirmationMessageSuccess[0].ContentFull, Text2 = "Continue" };
            }
            else if (cslResponse != null && !cslResponse.IsSuccessful)
            {
                if (!isEnableTravelCredit)
                {
                    errormessage = GetSDLMessageFromList(lstMessages, "RTI.FutureFlightCredits.FindFFC.Error")?.FirstOrDefault()?.ContentFull;
                    throw new MOBUnitedException(errormessage);
                }
                else
                {
                    errormessage = string.Format(GetSDLMessageFromList(lstMessages, "RTI.TravelCertificate.LookUpTravelCredits.Alert.FindTCByEmail")[0].ContentFull, mOBFutureFlightCredit.Email?.Trim());
                }
            }

            if (isEnableTravelCredit)
            {
                try
                {
                    response.ShoppingCart.FormofPaymentDetails.TravelCreditDetails.FindETCConfirmationMessage = await SendETCByEmail(mOBFutureFlightCredit, session);
                }
                catch (Exception ex)
                {
                    errormessage = ex.Message.ToString();
                }
            }
            // Condition will work for both ETC and FFC flow.
            if (!errormessage.IsNullOrEmpty())
                throw new MOBUnitedException(errormessage);
            response.ShoppingCart.FormofPaymentDetails.TravelFutureFlightCredit = travelFutureFlightCredit;

            _logger.LogInformation("FindFutureFlightCreditByEmail Response{response} with SessionId{sessoinId}", JsonConvert.SerializeObject(response), mOBFutureFlightCredit.SessionId);

            return response;
        }

        private async Task<MOBSection> SendETCByEmail(MOBFutureFlightCreditRequest mobRequest, Session session)
        {
            MOBSection msg = new MOBSection();
            bool enableGetETCByEmailFix = await _featureSettings.GetFeatureSettingValue("EnableGetETCByEmailFix").ConfigureAwait(false);
            string jsonresponse = await GetETCByEmail(mobRequest, mobRequest.Email, session.SessionId, session.Token, enableGetETCByEmailFix);
            var cslResponse = JsonConvert.DeserializeObject<United.Definition.ETC.LookupByEmailResponse>(jsonresponse);
            List<CMSContentMessage> lstMessages = await GetSDLContentByGroupName(mobRequest, session.SessionId, session.Token, _configuration.GetValue<string>("CMSContentMessages_GroupName_BookingRTI_Messages"), "BookingPathRTI_CMSContentMessagesCached_StaticGUID");
            if (enableGetETCByEmailFix)
            {
                if (cslResponse != null && !string.IsNullOrEmpty(cslResponse.Message))
                {
                    var content = GetSDLMessageFromList(lstMessages, "RTI.TravelCertificate.LookUpTravelCredits.Alert.FindTCByEmail")[0];
                    msg = new MOBSection
                    {
                        Text1 = string.Format(content?.ContentFull, mobRequest.Email),
                        Text2 = "Cancel",
                        Text3 = "Continue"
                    };
                }
            }
            else
            {
                if (cslResponse != null && cslResponse.Success)
                {
                    var content = GetSDLMessageFromList(lstMessages, "RTI.TravelCertificate.LookUpTravelCredits.Alert.FindTCByEmail")[0];
                    msg = new MOBSection
                    {
                        Text1 = string.Format(content?.ContentFull, mobRequest.Email),
                        Text2 = "Cancel",
                        Text3 = "Continue"
                    };
                }
                else if (cslResponse != null && !cslResponse.Success)
                {
                    var errorMessage = GetSDLMessageFromList(lstMessages, "RTI.FutureFlightCredits.FindFFC.Error")?.FirstOrDefault()?.ContentFull;
                    throw new MOBUnitedException(string.Format(errorMessage, mobRequest.Email));
                }
            }
            return msg;
        }

        private async Task<string> GetETCByEmail(MOBFutureFlightCreditRequest mobRequest, string email, string sessionId, string token, bool enableGetETCByEmailFix)
        {
            string path = enableGetETCByEmailFix ? "/ECD/ETCSearchAndEmail" : "/form-utility-data/etcsendemail";
            United.Definition.ETC.LookupByEmailRequest lookupByEmailRequest = BuildLookupByEmailRequest(email, enableGetETCByEmailFix);
            string jsonRequest = JsonConvert.SerializeObject(lookupByEmailRequest);

            return enableGetETCByEmailFix ? await _paymentService.GetETCByEmail(token, path, jsonRequest, sessionId) : await _cMSContentService.GetETCByEmail(path, jsonRequest, sessionId, token);
        }

        private United.Definition.ETC.LookupByEmailRequest BuildLookupByEmailRequest(string email, bool enableGetETCByEmailFix)
        {
            var lookupByEmailRequest = new United.Definition.ETC.LookupByEmailRequest();
            if (enableGetETCByEmailFix)
            {
                lookupByEmailRequest.MailTo = email;
                lookupByEmailRequest.CertPin = null;
                lookupByEmailRequest.PromoID = null;
                lookupByEmailRequest.StationID = "Mobile";
                lookupByEmailRequest.CallingService = new ServiceClient();
                lookupByEmailRequest.CallingService.Requestor = new Requestor();
                lookupByEmailRequest.CallingService.AccessCode = _configuration.GetValue<string>("TravelCreditAccessCode");
                lookupByEmailRequest.CallingService.Requestor.AgentAAA = "IAH";
                lookupByEmailRequest.CallingService.Requestor.AgentSine = "UA";
                lookupByEmailRequest.CallingService.Requestor.ApplicationSource = "Mobile";
                lookupByEmailRequest.CallingService.Requestor.Device = new Service.Presentation.CommonModel.Device();
                lookupByEmailRequest.CallingService.Requestor.Device.LNIATA = "Mobile";
                lookupByEmailRequest.CallingService.Requestor.DutyCode = "SU";
                lookupByEmailRequest.CallingService.Requestor.Lniata = "Mobile";
            }
            else
            {
                lookupByEmailRequest.EmailID = email;
                lookupByEmailRequest.PromoID = null;
                lookupByEmailRequest.CertPin = null;
                lookupByEmailRequest.LastName = null;
                lookupByEmailRequest.FirstName = null;
                lookupByEmailRequest.AgentSine = "UA";
                lookupByEmailRequest.StationID = "Mobile";
                lookupByEmailRequest.LineIATA = "Mobile";
                lookupByEmailRequest.DutyCode = "SU";
                lookupByEmailRequest.AccessCode = _configuration.GetValue<string>("TravelCreditAccessCode");
            }

            _logger.LogInformation("BuildLookupByEmailRequest request:{lookupByEmailRequest} and sessionId:{sessionId}", JsonConvert.SerializeObject(lookupByEmailRequest), _headers.ContextValues.SessionId);

            return lookupByEmailRequest;
        }

        private bool IncludeTravelCredit(int appId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnableTravelCredit") &&
                   GeneralHelper.IsApplicationVersionGreater(appId, appVersion, "AndroidTravelCreditVersion", "iPhoneTravelCreditVersion", "", "", true, _configuration);
        }

        private async Task<string> GetFFCByEmail(MOBRequest mobRequest, string email, string sessionId, MOBFutureFlightCreditResponse ffcResponse)
        {
            string url = "/ECD/TCLookupByEmailAndNotify";
            LookupByEmailRequest lookupByPnrRequest = BuildLookupByEmailRequest(ffcResponse, email);
            string jsonRequest = JsonConvert.SerializeObject(lookupByPnrRequest);
            string token = await _dPService.GetAnonymousToken(mobRequest.Application.Id, mobRequest.DeviceId, _configuration);

            string response = await _paymentService.GetFFCByEmail(token, url, jsonRequest, sessionId);

            return response;
        }

        private async Task<string> GetFFCByEmail(MOBRequest mobRequest, string email, Session session, MOBFutureFlightCreditResponse ffcResponse)
        {
            string url = "/ECD/TCLookupByEmailAndNotify";
            LookupByEmailRequest lookupByPnrRequest = BuildLookupByEmailRequest(ffcResponse, email);
            string jsonRequest = JsonConvert.SerializeObject(lookupByPnrRequest);

            string response = await _paymentService.GetFFCByEmail(session.Token, url, jsonRequest, session.SessionId);

            return response;
        }

        private LookupByEmailRequest BuildLookupByEmailRequest(MOBFutureFlightCreditResponse ffcResponse, string email)
        {
            var lookupByEmailRequest = new LookupByEmailRequest();
            lookupByEmailRequest.EmailID = email;
            lookupByEmailRequest.CallingService = new ServiceClient();
            lookupByEmailRequest.CallingService.Requestor = new Requestor();
            lookupByEmailRequest.CallingService.AccessCode = _configuration.GetValue<string>("FFCAccessCode");
            lookupByEmailRequest.CallingService.Requestor.AgentAAA = "HQS";
            lookupByEmailRequest.CallingService.Requestor.AgentSine = "UA";
            lookupByEmailRequest.CallingService.Requestor.ApplicationSource = "Mobile";
            lookupByEmailRequest.CallingService.Requestor.Device = new Service.Presentation.CommonModel.Device();
            lookupByEmailRequest.CallingService.Requestor.Device.LNIATA = "Mobile";
            lookupByEmailRequest.CallingService.Requestor.DutyCode = "SU";

            return lookupByEmailRequest;
        }

        private void OTFErrorAndRedirectHandling
            (OTFConversion.Response.OnTheFlyOfferEligibilityResponse cslOTFResponse,
            MOBFutureFlightCreditRequest mobrequest, MOBFutureFlightCreditResponse mobresponse, List<CMSContentMessage> lstMessages)
        {
            string OTF_errorCode = "90102";
            string OTF_redirectDotComCodes
                        = _configuration.GetValue<string>("OTFConversion_RedirectDotCom_Codes");
            string OTF_redirectMappCodes
                = _configuration.GetValue<string>("OTFConversion_RedirectMapp_Codes");
            string OTF_CSL_ErrorCodes = string.Empty;

            var errorObject = cslOTFResponse?.Error?.FirstOrDefault
                           (x => !string.IsNullOrEmpty(x.MajorCode) || !string.IsNullOrWhiteSpace(x.MajorCode));

            OTF_CSL_ErrorCodes = string.Format("Major-{0}, Monor-{1}", errorObject?.MajorCode, errorObject?.MinorCode);

            if (string.Equals(errorObject?.MajorCode, "Err1", StringComparison.OrdinalIgnoreCase))
            {
                //OTFConversion_Generic_Error1
                mobresponse.Exception = OTFSetExceptionMessage(OTF_errorCode,
                    lstMessages.FirstOrDefault(x => string.Equals(x.Title,
                    "RTI.FutureFlightCredits.OTFConversion_Generic_Error1", StringComparison.OrdinalIgnoreCase)).ContentFull,
                    OTF_CSL_ErrorCodes);
            }
            else if (string.Equals(errorObject?.MajorCode, "Msg1", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(errorObject?.MinorCode))
                {
                    //OTFConversion_Redirect_Msg1
                    if (OTF_redirectDotComCodes.IndexOf(errorObject?.MinorCode, StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        mobresponse.Captions = OTFPopupCaption
                        (mobrequest.RecordLocator, mobrequest.LastName, includeUrl: true, includebtn: false);
                        mobresponse.Exception = OTFSetExceptionMessage(OTF_errorCode,
                            lstMessages.FirstOrDefault(x => string.Equals(x.Title,
                            "RTI.FutureFlightCredits.OTFConversion_Redirect_Msg1", StringComparison.OrdinalIgnoreCase)).ContentFull,
                            OTF_CSL_ErrorCodes);
                    }
                    else if (OTF_redirectMappCodes.IndexOf(errorObject?.MinorCode, StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        mobresponse.Captions = OTFPopupCaption
                        (mobrequest.RecordLocator, mobrequest.LastName, includeUrl: false, includeFFCoption: true);
                        mobresponse.Exception = OTFSetExceptionMessage(OTF_errorCode,
                            lstMessages.FirstOrDefault(x => string.Equals(x.Title,
                            "RTI.FutureFlightCredits.OTFConversion_Redirect_Msg1", StringComparison.OrdinalIgnoreCase)).ContentFull,
                            OTF_CSL_ErrorCodes);
                    }
                }
            }
            else if (string.Equals(errorObject?.MajorCode, "Err2", StringComparison.OrdinalIgnoreCase))
            {
                //OTFConversion_Redirect_Error2
                if (!string.IsNullOrEmpty(errorObject?.MinorCode))
                {
                    if (OTF_redirectMappCodes.IndexOf(errorObject?.MinorCode, StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        mobresponse.Captions = OTFPopupCaption
                        (mobrequest.RecordLocator, mobrequest.LastName, includeUrl: false, includebtn: true);
                        mobresponse.Exception = OTFSetExceptionMessage(OTF_errorCode,
                            lstMessages.FirstOrDefault(x => string.Equals(x.Title,
                            "RTI.FutureFlightCredits.OTFConversion_Redirect_Error2", StringComparison.OrdinalIgnoreCase)).ContentFull,
                            OTF_CSL_ErrorCodes);
                    }
                }
                else
                {
                    mobresponse.Captions = OTFPopupCaption
                       (mobrequest.RecordLocator, mobrequest.LastName, includeUrl: false, includebtn: true);
                    mobresponse.Exception = OTFSetExceptionMessage(OTF_errorCode,
                        lstMessages.FirstOrDefault(x => string.Equals(x.Title,
                        "RTI.FutureFlightCredits.OTFConversion_Redirect_Error2", StringComparison.OrdinalIgnoreCase)).ContentFull,
                        OTF_CSL_ErrorCodes);

                }
            }
            else if (string.Equals(errorObject?.MajorCode, "Err3", StringComparison.OrdinalIgnoreCase))
            {
                //OTFConversion_Redirect_Error3
                mobresponse.Captions = OTFPopupCaption
                       (mobrequest.RecordLocator, mobrequest.LastName, includeUrl: false, includebtn: false);

                var bookByDateChar = errorObject?.Characteristics?.FirstOrDefault
                    (x => string.Equals(x.Code, "ExpirationDate", StringComparison.OrdinalIgnoreCase));

                string error3 = string.Empty;

                if (!string.IsNullOrEmpty(bookByDateChar?.Value))
                {
                    string bookByDate = lstMessages.FirstOrDefault(x => string.Equals(x.Title,
                           "RTI.FutureFlightCredits.OTFConversion_Redirect_BookByDate_Error3", StringComparison.OrdinalIgnoreCase)).ContentFull;
                    error3 = string.Format(bookByDate, bookByDateChar.Value);
                }
                else
                {
                    error3 = lstMessages.FirstOrDefault(x => string.Equals(x.Title,
                            "RTI.FutureFlightCredits.OTFConversion_Redirect_Error3", StringComparison.OrdinalIgnoreCase)).ContentFull;
                }
                mobresponse.Exception = OTFSetExceptionMessage(OTF_errorCode, error3, OTF_CSL_ErrorCodes);
            }
            else if (string.Equals(errorObject?.MajorCode, "Err4", StringComparison.OrdinalIgnoreCase))
            {
                //OTFConversion_Redirect_Error4
                mobresponse.Captions = OTFPopupCaption
                       (mobrequest.RecordLocator, mobrequest.LastName, includeUrl: false, includebtn: false);
                mobresponse.Exception = OTFSetExceptionMessage(OTF_errorCode,
                    lstMessages.FirstOrDefault(x => string.Equals(x.Title,
                    "RTI.FutureFlightCredits.OTFConversion_Redirect_Error4", StringComparison.OrdinalIgnoreCase)).ContentFull,
                    OTF_CSL_ErrorCodes);
            }
            else if (string.Equals(errorObject?.MajorCode, "Err5", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(errorObject?.MinorCode))
                {
                    //OTFConversion_Generic_Error5
                    if (string.Equals(errorObject?.MinorCode, "Mnr1", StringComparison.OrdinalIgnoreCase))
                    {
                        mobresponse.Captions = OTFPopupCaption
                       (mobrequest.RecordLocator, mobrequest.LastName, includeUrl: false, includebtn: false, includeCustSupport: true, cmsMessages: lstMessages);
                        mobresponse.Exception = OTFSetExceptionMessage(OTF_errorCode,
                            lstMessages.FirstOrDefault(x => string.Equals(x.Title,
                            "RTI.FutureFlightCredits.OTFConversion_Generic_Error5", StringComparison.OrdinalIgnoreCase)).ContentFull,
                            OTF_CSL_ErrorCodes);
                    }
                }
            }

            if (mobresponse.Exception == null)
            {
                //OTFConversion_Generic_Error1
                mobresponse.Exception = OTFSetExceptionMessage(OTF_errorCode,
                    lstMessages.FirstOrDefault(x => string.Equals(x.Title,
                    "RTI.FutureFlightCredits.OTFConversion_Generic_Error1", StringComparison.OrdinalIgnoreCase)).ContentFull,
                    OTF_CSL_ErrorCodes);
                mobresponse.Captions = null;
            }
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

        private List<MOBTypeOption> OTFPopupCaption(string recordlocator, string lastname,
            string action = "", bool includeUrl = false, bool includebtn = false, bool includeCustSupport = false,
            bool includeFFCoption = false, List<CMSContentMessage> cmsMessages = null)
        {
            var captions = new List<MOBTypeOption>();
            if (includebtn)
            {
                captions.Add(new MOBTypeOption { Key = "90102_ResDetail_Btntxt", Value = "Go to Reservation Details" });
            }
            if (includeUrl)
            {
                captions.Add(new MOBTypeOption { Key = "90102_ResDetail_Btntxt", Value = "Go to Trip Details" });
                var url = string.Empty;
                GetTripDetailRedirect3dot0Url(recordlocator, lastname);
                captions.Add(new MOBTypeOption { Key = "90102_ResDetail_RedirectUrl", Value = url });
            }
            if (includeCustSupport)
            {
                var message5 = cmsMessages.FirstOrDefault(x => string.Equals(x.Title,
                            "RTI.FutureFlightCredits.OTFConversion_Generic_Error5", StringComparison.OrdinalIgnoreCase));

                if (message5 != null)
                {
                    captions.Add(new MOBTypeOption { Key = "90102_CustomerSupport_Btntxt", Value = message5.CallToAction1 });
                    captions.Add(new MOBTypeOption
                    {
                        Key = "90102_CustomerSupport_RedirectUrl",
                        Value = message5.CallToActionUrl1
                    });
                }
            }
            if (includeFFCoption)
            {
                captions.Add(new MOBTypeOption { Key = "90102_ResDetail_Btntxt", Value = "Book with credit" });
            }
            captions.Add(new MOBTypeOption { Key = "90102_Next_Btntxt", Value = "Continue without this credit" });
            return captions;
        }

        private string EncryptString(string data)
        {
            //todo
            //return ECommerce.Framework.Utilities.SecureData.EncryptString(data);
            return default;
        }

        private MOBException OTFSetExceptionMessage(string code, string message, string cslerrcode)
        {
            return new MOBException
            {
                Code = code,
                ErrMessage = cslerrcode,
                Message = message
            };
        }

        private async Task SaveSCAndReservationToPersistWithFFCTraveler(Session session, MOBFutureFlightCreditResponse response, MOBRequest request, List<CMSContentMessage> lstMessages)
        {
            Reservation bookingPathReservation = new Reservation();
            bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(session.SessionId, bookingPathReservation.ObjectName, new List<string> { session.SessionId, bookingPathReservation.ObjectName }).ConfigureAwait(false);

            if (response.Reservation.TravelersCSL != null && response.Reservation.TravelersCSL.Count > 0)
            {
                bookingPathReservation.TravelersCSL = new SerializableDictionary<string, MOBCPTraveler>();
                foreach (var travelersCSL in response.Reservation.TravelersCSL)
                {
                    bookingPathReservation.TravelersCSL.Add(travelersCSL.PaxIndex.ToString(), travelersCSL);
                }
            }
            bookingPathReservation.Prices = response.Reservation.Prices;
            response.ShoppingCart.FormofPaymentDetails.FormOfPaymentType = response.ShoppingCart.FormofPaymentDetails?.TravelFutureFlightCredit?.TotalRedeemAmount > 0 ? MOBFormofPayment.FFC.ToString() : MOBFormofPayment.CreditCard.ToString();
            var scRESProduct = response.ShoppingCart.Products.Find(p => p.Code == "RES");
            double totalRedeemAmount = 0;
            if (response.ShoppingCart.FormofPaymentDetails?.TravelFutureFlightCredit?.TotalRedeemAmount > 0)
            {
                totalRedeemAmount = response.ShoppingCart.FormofPaymentDetails.TravelFutureFlightCredit.TotalRedeemAmount;
            }
            UpdatePricesInReservation(response.ShoppingCart.FormofPaymentDetails.TravelFutureFlightCredit, response.Reservation.Prices, request.Application.Id, request.Application.Version.Major, true);
            AssignIsOtherFOPRequired(response.ShoppingCart.FormofPaymentDetails, response.Reservation.Prices);
            AssignFormOfPaymentType(response.ShoppingCart.FormofPaymentDetails, response.Reservation.Prices, false);
            if (lstMessages == null)
            {
                lstMessages = await GetSDLContentByGroupName(request, session.SessionId, session.Token, _configuration.GetValue<string>("CMSContentMessages_GroupName_BookingRTI_Messages"), "BookingPathRTI_CMSContentMessagesCached_StaticGUID");
            }

            response.ShoppingCart.FormofPaymentDetails.TravelFutureFlightCredit.ReviewFFCMessages = BuildReviewFFCHeaderMessage(response.ShoppingCart.FormofPaymentDetails?.TravelFutureFlightCredit, response.Reservation.TravelersCSL, lstMessages);
            response.ShoppingCart.FormofPaymentDetails.TravelFutureFlightCredit.EmailConfirmationFFCMessages = AssignEmailMessageForFFCRefund(lstMessages, response.Reservation.Prices, response.ShoppingCart.FormofPaymentDetails.EmailAddress, response.ShoppingCart.FormofPaymentDetails.TravelFutureFlightCredit, request.Application);

            AssignIsOtherFOPRequired(response.ShoppingCart.FormofPaymentDetails, response.Reservation.Prices);
            AssignFormOfPaymentType(response.ShoppingCart.FormofPaymentDetails, response.Reservation.Prices, false);

            await _sessionHelperService.SaveSession(bookingPathReservation, session.SessionId, new List<string> { session.SessionId, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName).ConfigureAwait(false);

            response.ShoppingCart.SCTravelers = (response.Reservation.TravelersCSL != null && response.Reservation.TravelersCSL.Count() > 0) ? response.Reservation.TravelersCSL : null;

            await _sessionHelperService.SaveSession<MOBShoppingCart>(response.ShoppingCart, session.SessionId, new List<string> { session.SessionId, response.ShoppingCart.ObjectName }, response.ShoppingCart.ObjectName).ConfigureAwait(false);
        }

        private void AssignFormOfPaymentType(MOBFormofPaymentDetails formofPaymentDetails, List<MOBSHOPPrice> prices, bool IsSecondaryFOP = false, bool isRemoveAll = false)
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

        private List<MOBMobileCMSContentMessages> AssignEmailMessageForFFCRefund(List<CMSContentMessage> lstMessages, List<MOBSHOPPrice> prices, string email, MOBFOPTravelFutureFlightCredit futureFlightCredit, MOBApplication application)
        {
            List<MOBMobileCMSContentMessages> ffcHeaderMessage = null;
            if (ConfigUtility.EnableMFOP(application.Id, application.Version.Major))
            {
                if (prices.Exists(p => p.DisplayType.ToUpper() == "REFUNDFFCPRICE"))
                {
                    ffcHeaderMessage = GetSDLMessageFromList(lstMessages, "RTI.FutureFlightCredits.EmailConfirmation");
                    ffcHeaderMessage[0].ContentFull = string.Format(ffcHeaderMessage[0].ContentFull, email);
                    ffcHeaderMessage[0].ContentShort = ffcHeaderMessage[0].ContentFull;
                }
            }
            else if (prices.Exists(p => p.DisplayType.ToUpper() == "REFUNDPRICE"))
            {
                ffcHeaderMessage = GetSDLMessageFromList(lstMessages, "RTI.FutureFlightCredits.EmailConfirmation");
                if (IncludeMOBILE12570ResidualFix(application.Id, application.Version.Major))
                {
                    ffcHeaderMessage[0].ContentFull = "";
                    ffcHeaderMessage[0].ContentShort = "";
                }
                else
                {
                    ffcHeaderMessage[0].ContentFull = string.Format(ffcHeaderMessage[0].ContentFull, email);
                    ffcHeaderMessage[0].ContentShort = ffcHeaderMessage[0].ContentFull;
                }
            }
            return ffcHeaderMessage;
        }
        private List<MOBMobileCMSContentMessages> AssignEmailMessageForTCRefund(List<CMSContentMessage> lstMessages, List<MOBSHOPPrice> prices, string email, MOBFOPTravelCertificate travelCertificate, MOBApplication application)
        {
            List<MOBMobileCMSContentMessages> tcHeaderMessage = null;
            if (prices.Exists(p => p.DisplayType.ToUpper() == "REFUNDCERTIFICATEPRICE"))
            {
                tcHeaderMessage = GetSDLMessageFromList(lstMessages, "RTI.TravelCredits.EmailConfirmation");
                if (tcHeaderMessage.IsListNullOrEmpty())
                {
                    tcHeaderMessage = new List<MOBMobileCMSContentMessages>() {
                        new MOBMobileCMSContentMessages {
                            ContentFull = "Travel certificate details",
                            ContentShort = "Travel certificate details",
                            HeadLine = "Travel certificate",
                            LocationCode = "RTI.TravelCredits.EmailConfirmation",
                            Title = "RTI.TravelCredits.EmailConfirmation" }
                    };
                }
                //tcHeaderMessage[0].ContentFull = string.Format(tcHeaderMessage[0].ContentFull, email);
                //tcHeaderMessage[0].ContentShort = tcHeaderMessage[0].ContentFull;
            }
            return tcHeaderMessage;
        }

        private bool IncludeMOBILE12570ResidualFix(int appId, string appVersion)
        {
            bool isApplicationGreater = GeneralHelper.IsApplicationVersionGreater(appId, appVersion, "AndroidMOBILE12570ResidualVersion", "iPhoneMOBILE12570ResidualVersion", "", "", true, _configuration);
            return (_configuration.GetValue<bool>("eableMOBILE12570Toggle") && isApplicationGreater);
        }

        private List<MOBMobileCMSContentMessages> BuildReviewFFCHeaderMessage(MOBFOPTravelFutureFlightCredit travelFutureFlightCredit, List<MOBCPTraveler> travelers, List<CMSContentMessage> lstMessages)
        {
            List<MOBMobileCMSContentMessages> ffcHeaderMessage = null;
            if (travelFutureFlightCredit?.FutureFlightCredits?.Count() > 1 && travelFutureFlightCredit.FutureFlightCredits.Sum(ffc => ffc.NewValueAfterRedeem) > 0)
            {
                ffcHeaderMessage = GetSDLMessageFromList(lstMessages, "RTI.FutureFlightCredits.ReviewFFC");
                string contentFullMessage = ffcHeaderMessage[0].ContentFull;
                string travlerBalanceFFCMessageTemplete = "{0}:{1}, travel-by date {2}";
                string travlerBalanceFFCMessage = "";
                foreach (var traveler in travelers)
                {
                    if (traveler.FutureFlightCredits != null && traveler.FutureFlightCredits.Count > 1 && traveler.FutureFlightCredits.Sum(ffc => ffc.NewValueAfterRedeem) > 0)
                    {
                        travlerBalanceFFCMessage += (travlerBalanceFFCMessage != "" ? " and " : "") + string.Format(travlerBalanceFFCMessageTemplete,
                                                                                traveler.FirstName + " " + traveler.LastName,
                                                                                traveler.FutureFlightCredits.Sum(ffc => ffc.NewValueAfterRedeem).ToString("C2", CultureInfo.CurrentCulture),
                                                                                traveler.FutureFlightCredits.Min(ffc => Convert.ToDateTime(ffc.ExpiryDate).ToString("MM/dd/yyyy")));
                    }
                }
                if (!string.IsNullOrEmpty(travlerBalanceFFCMessage))
                {
                    ffcHeaderMessage[0].ContentFull = string.Format(ffcHeaderMessage[0].ContentFull, travlerBalanceFFCMessage);
                }
                else
                {
                    ffcHeaderMessage = null;
                }
            }

            return ffcHeaderMessage;
        }

        private void UpdatePricesInReservation(MOBFOPTravelFutureFlightCredit travelFutureFlightCredit, List<MOBSHOPPrice> prices, int appId = 0, string appVersion = "", bool isManageTravelCreditAPI = false)
        {

            var ffcPrice = prices.FirstOrDefault(p => p.DisplayType.ToUpper() == "FFC");
            var totalCreditFFC = prices.FirstOrDefault(p => p.DisplayType.ToUpper() == "REFUNDPRICE");
            var grandtotal = prices.FirstOrDefault(p => p.DisplayType.ToUpper() == "GRAND TOTAL");
            string maskedConfirmation = string.Empty;
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
                string priceTypeDescription = (ConfigUtility.IsFFCSummaryUpdated(appId, appVersion) && isManageTravelCreditAPI == false) ? _configuration.GetValue<string>("FFC_Applied") : "Future Flight Credit";
                UpdateCertificatePrice(ffcPrice, travelFutureFlightCredit.TotalRedeemAmount, "FFC", priceTypeDescription, isAddNegative: true);
                //Build Total Credit item
                double totalCreditValue = travelFutureFlightCredit.FutureFlightCredits.Sum(ffc => ffc.NewValueAfterRedeem);
                if (totalCreditValue > 0)
                {
                    priceTypeDescription = UpdateRefundPricePriceTypeDescription(travelFutureFlightCredit, appId, appVersion, ref maskedConfirmation, isManageTravelCreditAPI);
                    totalCreditFFC = new MOBSHOPPrice();
                    prices.Add(totalCreditFFC);
                    UpdateCertificatePrice(totalCreditFFC, totalCreditValue, "REFUNDPRICE", priceTypeDescription, "RESIDUALCREDIT");
                }
                UpdateCertificateRedeemAmountFromTotalInReserationPrices(grandtotal, travelFutureFlightCredit.TotalRedeemAmount);
            }
            else
            {
                prices.Remove(ffcPrice);
            }
        }

        private string UpdateRefundPricePriceTypeDescription(MOBFOPTravelFutureFlightCredit travelFutureFlightCredit, int appId, string appVersion, ref string maskedConfirmation, bool isManageTravelCreditAPI = false)
        {
            string priceTypeDescription = "Total credit";
            if (ConfigUtility.IsFFCSummaryUpdated(appId, appVersion) && isManageTravelCreditAPI == false)
            {
                priceTypeDescription = _configuration.GetValue<string>("FFC_RemainingText");
            }

            return priceTypeDescription;
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

        private void UpdateCertificateRedeemAmountFromTotalInReserationPrices(MOBSHOPPrice price, double value, bool isRemove = true)
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

        private List<MOBFOPFutureFlightCredit> AssignOTFTravelerFFCs
            (OTFConversion.Response.OnTheFlyOfferEligibilityResponse cslResponse,
            List<MOBFOPFutureFlightCredit> futureFlightCredits, MOBSHOPReservation reservation)
        {
            string travelstartDate = reservation?.Trips?.FirstOrDefault()?.DepartDate;
            var offerdetails = cslResponse.OfferDetails;
            var travelerCSL = reservation?.TravelersCSL;

            if (offerdetails?.Travelers != null && offerdetails.Travelers.Any())
            {
                ValidateOTFFFCExpiration(offerdetails.Travelers, travelstartDate);

                int index = 0;
                int appliedFFCCount = 0;

                foreach (var traveleroffer in offerdetails.Travelers)
                {
                    var mobtraveler = travelerCSL.FirstOrDefault(t => t.TravelerNameIndex == traveleroffer.Key);

                    if (mobtraveler != null)
                    {
                        if (mobtraveler.FutureFlightCredits == null)
                            mobtraveler.FutureFlightCredits = new List<MOBFOPFutureFlightCredit>();

                        foreach (var travelCredit in traveleroffer.TravelerCredits)
                        {
                            double ffcAppliedToTraveler = 0;
                            if (mobtraveler.FutureFlightCredits != null)
                                ffcAppliedToTraveler = mobtraveler.FutureFlightCredits.Sum(t => t.RedeemAmount);

                            ffcAppliedToTraveler = Math.Round(ffcAppliedToTraveler, 2, MidpointRounding.AwayFromZero);

                            if (Convert.ToDouble(travelCredit?.CreditAmount?.Amount) > 0
                                && ffcAppliedToTraveler < mobtraveler.IndividualTotalAmount
                                && !mobtraveler.FutureFlightCredits.Exists(f => f.PinCode == travelCredit.OfferKey)
                                && !IsFFCExpired(travelstartDate, travelCredit.ExpirationDate))
                            {
                                index++;
                                var mobFFC = new MOBFOPFutureFlightCredit();
                                mobFFC.CurrentValue = travelCredit.CreditAmount.Amount;
                                mobFFC.CreditAmount = (mobFFC.CurrentValue).ToString("C2", CultureInfo.CurrentCulture);
                                mobFFC.ExpiryDate = Convert.ToDateTime(travelCredit.ExpirationDate).ToString("MMMMM dd, yyyy");
                                mobFFC.IsCertificateApplied = true;
                                mobFFC.InitialValue = travelCredit.CreditAmount.Amount;
                                mobFFC.Index = index;
                                mobFFC.PinCode = travelCredit.OfferKey;
                                mobFFC.PromoCode = travelCredit.PromoCode;
                                mobFFC.RecordLocator = offerdetails.RecordLocator;

                                ///This section should load from traveler
                                mobFFC.TravelerNameIndex = mobtraveler.TravelerNameIndex;
                                double remainingBalanceAfterAppliedFFC = mobtraveler.IndividualTotalAmount - ffcAppliedToTraveler;
                                mobFFC.RedeemAmount = remainingBalanceAfterAppliedFFC > mobFFC.CurrentValue ? mobFFC.CurrentValue : remainingBalanceAfterAppliedFFC;
                                mobFFC.RedeemAmount = Math.Round(mobFFC.RedeemAmount, 2, MidpointRounding.AwayFromZero);
                                mobFFC.DisplayRedeemAmount = (mobFFC.RedeemAmount).ToString("C2", CultureInfo.CurrentCulture);
                                mobFFC.NewValueAfterRedeem = mobFFC.CurrentValue - mobFFC.RedeemAmount;
                                mobFFC.NewValueAfterRedeem = Math.Round(mobFFC.NewValueAfterRedeem, 2, MidpointRounding.AwayFromZero);
                                mobFFC.DisplayNewValueAfterRedeem = (mobFFC.NewValueAfterRedeem).ToString("C2", CultureInfo.CurrentCulture);
                                mobFFC.RecipientsFirstName = mobtraveler.FirstName + mobtraveler.MiddleName;
                                mobFFC.RecipientsLastName = mobtraveler.LastName;
                                mobFFC.PaxId = mobtraveler.PaxID;
                                mobFFC.IsCertificateApplied = true;
                                futureFlightCredits.Add(mobFFC);
                                mobtraveler.FutureFlightCredits.Add(mobFFC);
                                appliedFFCCount++;
                            }
                        }//travelCredit
                        AssignTravelerTotalFFCNewValueAfterReDeem(mobtraveler);
                    }
                }//traveleroffer
            }

            return futureFlightCredits;
        }

        private void ValidateOTFFFCExpiration
         (Collection<FutureFlightTraveler> travelCreditDetails, string travelStartDate)
        {
            bool isCertNotExpiredExist = travelCreditDetails
                                         .Any(tcd => tcd.TravelerCredits != null
                                                 && tcd.TravelerCredits.Any(tc => Convert.ToDouble(tc.CreditAmount.Amount) > 0
                                                 && !IsFFCExpired(travelStartDate, tc.ExpirationDate)));
            if (!isCertNotExpiredExist)
            {
                throw new MOBUnitedException(_configuration.GetValue<string>("FFC_ExpiryErrorMessage"));
            }
        }

        private List<MOBFOPFutureFlightCredit> AssignTravelerFFCs(List<TravelCreditDetail> travelCreditDetails, List<MOBCPTraveler> travelerCSL, List<MOBFOPFutureFlightCredit> futureFlightCredits, List<CMSContentMessage> list, string travelStartDate)
        {
            if (travelCreditDetails != null && travelCreditDetails.Count > 0)
            {
                ValidateFFCExpiration(travelCreditDetails, travelerCSL, travelStartDate);
                int index = 0;
                int appliedFFCCount = 0;

                foreach (var travelCreditDetail in travelCreditDetails)
                {
                    var traveler = travelerCSL.FirstOrDefault(t => t.PaxID == travelCreditDetail.PassengerIndex);
                    if (traveler != null)
                    {
                        if (traveler.FutureFlightCredits == null)
                            traveler.FutureFlightCredits = new List<MOBFOPFutureFlightCredit>();
                        foreach (var travelCredit in travelCreditDetail.TravelCreditList)
                        {
                            double ffcAppliedToTraveler = 0;
                            if (traveler.FutureFlightCredits != null)
                                ffcAppliedToTraveler = traveler.FutureFlightCredits.Sum(t => t.RedeemAmount);

                            ffcAppliedToTraveler = Math.Round(ffcAppliedToTraveler, 2, MidpointRounding.AwayFromZero);
                            if (Convert.ToDouble(travelCredit.CurrentValue) > 0 && ffcAppliedToTraveler < traveler.IndividualTotalAmount && !traveler.FutureFlightCredits.Exists(f => f.PinCode == travelCredit.CertPin) && !IsFFCExpired(travelStartDate, travelCredit.CertExpDate))
                            {
                                var mobFFC = new MOBFOPFutureFlightCredit();
                                index++;
                                mobFFC.CurrentValue = double.Parse(travelCredit.CurrentValue);
                                mobFFC.CreditAmount = (mobFFC.CurrentValue).ToString("C2", CultureInfo.CurrentCulture);
                                mobFFC.ExpiryDate = Convert.ToDateTime(travelCredit.CertExpDate).ToString("MMMMM dd, yyyy");
                                mobFFC.IsCertificateApplied = true;
                                mobFFC.InitialValue = double.Parse(travelCredit.InitialValue);
                                mobFFC.Index = index;
                                mobFFC.PinCode = travelCredit.CertPin;
                                mobFFC.PromoCode = travelCredit.PromoID;
                                mobFFC.RecordLocator = travelCredit.OrigPNR;
                                mobFFC.YearIssued = !string.IsNullOrEmpty(travelCredit.OrigIssueDate) ? Convert.ToDateTime(travelCredit.OrigIssueDate).Year.ToString() : "";

                                ///This section should load from traveler
                                mobFFC.TravelerNameIndex = traveler.TravelerNameIndex;
                                double remainingBalanceAfterAppliedFFC = traveler.IndividualTotalAmount - ffcAppliedToTraveler;
                                mobFFC.RedeemAmount = remainingBalanceAfterAppliedFFC > mobFFC.CurrentValue ? mobFFC.CurrentValue : remainingBalanceAfterAppliedFFC;
                                mobFFC.RedeemAmount = Math.Round(mobFFC.RedeemAmount, 2, MidpointRounding.AwayFromZero);
                                mobFFC.DisplayRedeemAmount = (mobFFC.RedeemAmount).ToString("C2", CultureInfo.CurrentCulture);
                                mobFFC.NewValueAfterRedeem = mobFFC.CurrentValue - mobFFC.RedeemAmount;
                                mobFFC.NewValueAfterRedeem = Math.Round(mobFFC.NewValueAfterRedeem, 2, MidpointRounding.AwayFromZero);
                                mobFFC.DisplayNewValueAfterRedeem = (mobFFC.NewValueAfterRedeem).ToString("C2", CultureInfo.CurrentCulture);
                                mobFFC.RecipientsFirstName = traveler.FirstName + traveler.MiddleName;
                                mobFFC.RecipientsLastName = traveler.LastName;
                                mobFFC.PaxId = traveler.PaxID;
                                mobFFC.IsCertificateApplied = true;
                                futureFlightCredits.Add(mobFFC);
                                traveler.FutureFlightCredits.Add(mobFFC);
                                appliedFFCCount++;
                            }
                        }
                        AssignTravelerTotalFFCNewValueAfterReDeem(traveler);
                    }
                }
                if (appliedFFCCount == 0)
                {
                    if ((travelCreditDetails?.SelectMany(tcd => tcd?.TravelCreditList?.Select(tc => tc?.CertPin))?.ToList()?.Intersect(travelerCSL?.SelectMany(t => t?.FutureFlightCredits?.Select(ffc => ffc?.PinCode))?.ToList())?.ToList()?.Count ?? 0) != 0)
                    {
                        List<MOBMobileCMSContentMessages> messages = GetSDLMessageFromList(list, "RTI.FutureFlightCredits.FFCDuplicationError");
                        throw new MOBUnitedException((messages != null && messages.Count > 0) ? messages[0]?.ContentFull : _configuration.GetValue<string>("FFCDuplicationError"));
                    }

                    throw new MOBUnitedException("90101", _configuration.GetValue<string>("FFC_90101_ErrorPopup_ErrorMessage"));
                }
            }

            return futureFlightCredits;
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

        private void ValidateFFCExpiration(List<TravelCreditDetail> travelCreditDetails, List<MOBCPTraveler> travelerCSL, string travelStartDate)
        {
            bool isCertNotExpiredExist = travelCreditDetails
                                         .Exists(tcd => tcd.TravelCreditList != null
                                                 && tcd.TravelCreditList.Exists(tc => Convert.ToDouble(tc.CurrentValue) > 0
                                                 && !IsFFCExpired(travelStartDate, tc.CertExpDate)));
            if (!isCertNotExpiredExist)
            {
                throw new MOBUnitedException(_configuration.GetValue<string>("FFC_ExpiryErrorMessage"));
            }
        }

        private void AssignTravelerTotalFFCNewValueAfterReDeem(MOBCPTraveler traveler)
        {
            if (traveler.FutureFlightCredits?.Count > 0)
            {
                var sumOfNewValueAfterRedeem = traveler.FutureFlightCredits.Sum(ffc => ffc.NewValueAfterRedeem);
                if (sumOfNewValueAfterRedeem > 0)
                {
                    sumOfNewValueAfterRedeem = Math.Round(sumOfNewValueAfterRedeem, 2, MidpointRounding.AwayFromZero);
                    traveler.TotalFFCNewValueAfterRedeem = (sumOfNewValueAfterRedeem).ToString("C2", CultureInfo.CurrentCulture);
                }
                else
                {
                    traveler.TotalFFCNewValueAfterRedeem = "";
                }
            }
            else
            {
                traveler.TotalFFCNewValueAfterRedeem = "";
            }
        }

        private void AssignTravelerTotalFFCReDeemAmount(MOBCPTraveler traveler)
        {
            if (traveler.FutureFlightCredits?.Count > 0)
            {
                var sumOfRedeemAmount = traveler.FutureFlightCredits.Sum(ffc => ffc.RedeemAmount);
                if (sumOfRedeemAmount > 0)
                {
                    sumOfRedeemAmount = Math.Round(sumOfRedeemAmount, 2, MidpointRounding.AwayFromZero);
                    traveler.TotalFFCRedeemAmount = sumOfRedeemAmount;
                    traveler.DisplayTotalFFCRedeemAmount = "-" + (sumOfRedeemAmount).ToString("C2", CultureInfo.CurrentCulture);
                }
                else
                {
                    traveler.TotalFFCRedeemAmount = null;
                    traveler.DisplayTotalFFCRedeemAmount = "";
                }
            }
            else
            {
                traveler.TotalFFCRedeemAmount = null;
                traveler.DisplayTotalFFCRedeemAmount = "";
            }
        }

        private bool IsFFCExpired(string travelDate, string certExpiryDate)
        {
            DateTime tdate, cExpiryDate;
            if (DateTime.TryParse(travelDate, out tdate) && DateTime.TryParse(certExpiryDate, out cExpiryDate))
            {
                if (tdate <= cExpiryDate)
                {
                    return false;
                }
            }

            return true;
        }

        #region allowed ffc amount
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

        private double GetBundlesAmount(List<MOBProdDetail> products)
        {
            string nonBundleProductCode = _configuration.GetValue<string>("NonBundleProductCode");
            double bundleAmount = products == null ? 0 : products.Where(p => (nonBundleProductCode.IndexOf(p.Code) == -1) && !string.IsNullOrEmpty(p.ProdTotalPrice)).Sum(a => Convert.ToDouble(a.ProdTotalPrice));
            return bundleAmount;
        }
        #endregion

        private async Task<string> GetFFCByPnr(MOBRequest mobRequest, string recordLocator, string lastname, string sessionId, MOBFutureFlightCreditResponse ffcResponse)
        {
            string url = "/ECD/TCLookupByPnr";
            LookupByPnrRequest lookupByPnrRequest = BuildLookupByPnrRequest(ffcResponse, recordLocator, lastname);
            string jsonRequest = JsonConvert.SerializeObject(lookupByPnrRequest);

            string token = await _dPService.GetAnonymousToken(mobRequest.Application.Id,mobRequest.DeviceId, _configuration);

            return await _paymentService.GetFFCByPnr(token, url, jsonRequest, sessionId);
        }

        private LookupByPnrRequest BuildLookupByPnrRequest(MOBFutureFlightCreditResponse ffcResponse, string pnr, string lastname)
        {
            var lookupByPnrRequest = new LookupByPnrRequest();
            lookupByPnrRequest.OrigPNR = pnr;
            lookupByPnrRequest.LastName = lastname;
            lookupByPnrRequest.CartId = ffcResponse.ShoppingCart.CartId;
            lookupByPnrRequest.PaxDetails = new List<PaxDetail>();
            foreach (var traveler in ffcResponse.Reservation.TravelersCSL)
            {
                var paxDetail = new PaxDetail();
                paxDetail.FirstName = traveler.FirstName;
                paxDetail.LastName = traveler.LastName;

                if (traveler.EmailAddresses != null && traveler.EmailAddresses.Count > 0)
                    paxDetail.Email = traveler.EmailAddresses[0].EmailAddress;
                if (traveler.Phones != null && traveler.Phones.Count > 0)
                    paxDetail.Phone = traveler.Phones[0].AreaNumber + traveler.Phones[0].PhoneNumber;
                if (traveler.Addresses != null && traveler.Addresses.Count > 0)
                    paxDetail.ZipCode = traveler.Addresses[0].PostalCode;
                paxDetail.PassengerIndex = traveler.PaxID;
                paxDetail.DateOfBirth = traveler.BirthDate;
                lookupByPnrRequest.PaxDetails.Add(paxDetail);
            }
            lookupByPnrRequest.AgentSine = "UA";
            lookupByPnrRequest.AccessCode = "SU";

            lookupByPnrRequest.CallingService = new ServiceClient();
            lookupByPnrRequest.CallingService.Requestor = new Requestor();
            lookupByPnrRequest.CallingService.AccessCode = _configuration.GetValue<string>("FFCAccessCode");
            lookupByPnrRequest.CallingService.Requestor.AgentAAA = "HQS";
            lookupByPnrRequest.CallingService.Requestor.AgentSine = "UA";
            lookupByPnrRequest.CallingService.Requestor.ApplicationSource = "Mobile";
            lookupByPnrRequest.CallingService.Requestor.Device = new Service.Presentation.CommonModel.Device();
            lookupByPnrRequest.CallingService.Requestor.Device.LNIATA = "Mobile";
            lookupByPnrRequest.CallingService.Requestor.DutyCode = "SU";

            return lookupByPnrRequest;
        }

        private async  Task<string> OTFConversionByPnr(MOBFutureFlightCreditRequest ffcRequest, MOBFutureFlightCreditResponse ffcResponse, string token)
        {
            string jsonRequest = string.Empty;
            string jsonResponse = string.Empty;
            string url = string.Empty;
            OTFConversion.Request.OnTheFlyOfferEligibilityRequest otfEligibilityRequest;

            url = "/OnTheFly/OfferEligible";
            otfEligibilityRequest = await BuildOTFConversionRequest(ffcRequest, token);
            jsonRequest = JsonConvert.SerializeObject(otfEligibilityRequest);           

            return await _oTFConversionService.OTFConversionByPnr(url, jsonRequest, ffcRequest.SessionId, token); 
        }

        private async Task<OTFConversion.Request.OnTheFlyOfferEligibilityRequest> BuildOTFConversionRequest
          (MOBFutureFlightCreditRequest ffcRequest, string token)
        {
            var otfEligibilityRequest = new OTFConversion.Request.OnTheFlyOfferEligibilityRequest();
            otfEligibilityRequest.RecordLocator = ffcRequest.RecordLocator;
            otfEligibilityRequest.LastName = ffcRequest.LastName;
            otfEligibilityRequest.Channel = ffcRequest.Application.Id;

            var cartInfo = await GetCartInformation(ffcRequest.SessionId,
                ffcRequest.Application, ffcRequest.DeviceId, ffcRequest.CartId, token);

            otfEligibilityRequest.Reservation = cartInfo.Reservation;

            return otfEligibilityRequest;
        }

        private async Task<LoadReservationAndDisplayCartResponse> GetCartInformation(string sessionId, MOBApplication application, string device, string cartId, string token, WorkFlowType workFlowType = WorkFlowType.InitialBooking)
        {
            LoadReservationAndDisplayCartRequest loadReservationAndDisplayCartRequest = new LoadReservationAndDisplayCartRequest();
            
            loadReservationAndDisplayCartRequest.CartId = cartId;
            loadReservationAndDisplayCartRequest.WorkFlowType = (United.Services.FlightShopping.Common.FlightReservation.WorkFlowType)(WorkFlowType)workFlowType;            
            
            return await _shoppingCartService.GetCartInformation<LoadReservationAndDisplayCartResponse>(token, "LoadReservationAndDisplayCart", JsonConvert.SerializeObject(loadReservationAndDisplayCartRequest), sessionId); 
        }

        private async Task LoadSessionValuesToResponse(Session session, MOBFutureFlightCreditResponse response)
        {
            response.ShoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(session.SessionId, new MOBShoppingCart().ObjectName, new List<string> { session.SessionId, new MOBShoppingCart().ObjectName }).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(session.SessionId))
            {
                if (response.ShoppingCart == null)
                {
                    response.ShoppingCart = null;
                }
                if (response.ShoppingCart.FormofPaymentDetails == null)
                {
                    response.ShoppingCart.FormofPaymentDetails = new MOBFormofPaymentDetails();
                }
            }
            else //v871375: This is for FindFutureFlightCreditByEmail to work properly
            {
                response.ShoppingCart = new MOBShoppingCart();
                response.ShoppingCart.FormofPaymentDetails = new MOBFormofPaymentDetails();
                response.ShoppingCart.FormofPaymentDetails.TravelCreditDetails = new MOBFOPTravelCreditDetails();
            }
            
            response.SessionId = session.SessionId;
            response.Flow = string.IsNullOrEmpty(session.Flow) ? response.ShoppingCart.Flow : session.Flow;
            response.Reservation = new MOBSHOPReservation(_configuration, _cachingService);
            response.Reservation = await GetReservationFromPersist(response.Reservation, session);
            await response.Reservation.UpdateRewards(_configuration, _cachingService);

            ProfileResponse persistedProfileResponse = new ProfileResponse();
            persistedProfileResponse = await _sessionHelperService.GetSession<ProfileResponse>(session.SessionId, persistedProfileResponse.ObjectName, new List<string> { session.SessionId, persistedProfileResponse.ObjectName }).ConfigureAwait(false);

            response.Profiles = persistedProfileResponse != null ? persistedProfileResponse.Response.Profiles : null;
        }

        private async Task<MOBSHOPReservation> GetReservationFromPersist(MOBSHOPReservation reservation, Session session)
        {
            #region
            Reservation bookingPathReservation = new Reservation();
            bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(session.SessionId, bookingPathReservation.ObjectName, new List<string> { session.SessionId, bookingPathReservation.ObjectName }).ConfigureAwait(false);
            await reservation.Initialise(_configuration, _cachingService);

            if (bookingPathReservation != null)
            {
                return MakeReservationFromPersistReservation(reservation, bookingPathReservation, session);
            }
            else
            {
                return reservation;
            }

            #endregion
        }

        private MOBSHOPReservation MakeReservationFromPersistReservation(MOBSHOPReservation reservation, Reservation bookingPathReservation,
             Session session)
        {
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
            if (_configuration.GetValue
                <string>("EnableShopPriceBreakDown") != null &&
                _configuration.GetValue
                <bool>("EnableShopPriceBreakDown"))
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
            _ETCUtility.SetEligibilityforETCTravelCredit(reservation, session, bookingPathReservation);
            _logger.LogInformation("MakeReservationFromPersistReservation Reservation:{reservation} with SessionId:{sessionid}", JsonConvert.SerializeObject(reservation), session.SessionId);

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

        private bool OnTheFlyConversionEnabled(int applicationId, string applicationVersion)
        {
            return GeneralHelper.IsApplicationVersionGreaterorEqual
                    (applicationId, applicationVersion,
                    _configuration.GetValue<string>("Android_EnableOTFConversionAppVersion"), _configuration.GetValue<string>("iPhone_EnableOTFConversionAppVersion"));
        }

        private async Task<string> GetSSOToken(string mileagePlusNumber, string hashPinCode, string sessionId, MOBRequest request)
        {
            try
            {
                if (!string.IsNullOrEmpty(mileagePlusNumber) && !string.IsNullOrEmpty(hashPinCode))
                {
                    string authToken = string.Empty;
                    var validWalletRequest = await new HashPin(_logger, _configuration, null, _dynamoDBService, _headers, _featureSettings).ValidateHashPinAndGetAuthToken(mileagePlusNumber, hashPinCode, request.Application.Id, request.DeviceId, request.Application.Version.Major, authToken).ConfigureAwait(false);

                    if (!validWalletRequest)
                        throw new MOBUnitedException(_configuration.GetValue<string>("bugBountySessionExpiredMsg"));

                    if (validWalletRequest)
                    {
                        return GetSSOToken
                            (request.Application.Id, request.DeviceId, request.Application.Version.Major,
                            request.TransactionId, null, sessionId, mileagePlusNumber);
                    }
                }
            }
            catch { return string.Empty; }

            return string.Empty;
        }

        private string GetSSOToken(int applicationId, string deviceId, string appVersion, string transactionId, String webConfigSession, string sessionID, string mileagPlusNumber)
        {
            string ssoToken = string.Empty;

            ssoToken = null;//Authentication.GetSSOToken(applicationId, deviceId, appVersion, transactionId, webConfigSession, sessionID, mileagPlusNumber);
            return ssoToken;
        }

        public List<MOBMobileCMSContentMessages> SwapTitleAndLocation(List<MOBMobileCMSContentMessages> cmsList)
        {
            foreach (var item in cmsList)
            {
                string location = item.LocationCode;
                item.LocationCode = item.Title;
                item.Title = location;
            }

            return cmsList;
        }

        public string GetCostBreakdownFareHeader(string travelType,MOBShoppingCart shoppingCart)
        {
            string fareHeader = "Fare";
            if (!string.IsNullOrEmpty(travelType))
            {
                travelType = travelType.ToUpper();
                if (travelType == TravelType.CB.ToString())
                {
                    fareHeader = "Corporate fare";
                }
                else if (travelType == TravelType.CLB.ToString())
                {
                    fareHeader = "Break from Business fare";
                }
                else if (shoppingCart?.Offers != null && !string.IsNullOrEmpty(shoppingCart?.Offers.OfferCode))
                {
                    fareHeader = _configuration.GetValue<string>("OfferCodeFareText");
                }
            }
            return fareHeader;
        }
    }
}
