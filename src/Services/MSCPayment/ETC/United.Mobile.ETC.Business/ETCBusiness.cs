using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using United.Common.Helper;
using United.Common.Helper.MSCPayment.Interfaces;
using United.Definition;
using United.Definition.FormofPayment;
using United.Definition.Shopping;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.MSCPayment.Interfaces;
using United.Mobile.DataAccess.Product.Interfaces;
using United.Mobile.Model.Common;
using United.Persist.Definition.Shopping;
using United.Service.Presentation.CommonModel;
using United.Service.Presentation.PaymentModel;
using United.Service.Presentation.PaymentResponseModel;
using United.Service.Presentation.PersonModel;
using United.Services.FlightShopping.Common.Extensions;
using Task = System.Threading.Tasks.Task;
using United.Utility.Helper;
using United.Mobile.Model.MSC.FormofPayment;
using United.Services.FlightShopping.Common;
using System.Globalization;
using United.Services.FlightShopping.Common.FlightReservation;
using Reservation = United.Persist.Definition.Shopping.Reservation;

namespace United.Mobile.ETC.Business
{
    public class ETCBusiness : IETCBusiness
    {
        private readonly IConfiguration _configuration;
        private readonly ICacheLog<ETCBusiness> _logger;
        private readonly ISessionHelperService _sessionHelperService;
        private readonly IMSCShoppingSessionHelper _shoppingSessionHelper;
        private readonly IMSCPkDispenserPublicKey _pKDispenserPublicKey;
        private readonly IETCUtility _eTCUtility;
        private readonly IReferencedataService _referencedataService;
        private readonly ICMSContentService _cMSContentService;
        private readonly IETCBalanceEnquiryService _iETCBalanceEnquiryService;
        private readonly IMSCFormsOfPayment _mSCFormsOfPayment;
        private readonly ILegalDocumentsForTitlesService _legalDocumentsForTitlesService;
        private readonly ICachingService _cachingService;
        private readonly IShoppingCartUtility _shoppingCartUtility; 
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IFeatureSettings _featureSettings;
        private readonly IShoppingUtility _shoppingUtility; 

        public ETCBusiness(IConfiguration configuration
            , ICacheLog<ETCBusiness> logger
            , ISessionHelperService sessionHelperService
            , IMSCShoppingSessionHelper shoppingSessionHelper
            , IETCUtility eTCUtility
            , IReferencedataService referencedataService
            , ICMSContentService cMSContentService
            , IETCBalanceEnquiryService iETCBalanceEnquiryService
            , IMSCFormsOfPayment mSCFormsOfPayment
            , IMSCPkDispenserPublicKey pKDispenserPublicKey
            , ILegalDocumentsForTitlesService legalDocumentsForTitlesService
            , ICachingService cachingService
            , IShoppingCartUtility shoppingCartUtility
            ,IShoppingCartService shoppingCartService
            , IFeatureSettings featureSettings
            ,IShoppingUtility shoppingUtility)
        {
            _configuration = configuration;
            _logger = logger;
            _sessionHelperService = sessionHelperService;
            _shoppingSessionHelper = shoppingSessionHelper;
            _pKDispenserPublicKey = pKDispenserPublicKey;
            _eTCUtility = eTCUtility;
            _referencedataService = referencedataService;
            _cMSContentService = cMSContentService;
            _iETCBalanceEnquiryService = iETCBalanceEnquiryService;
            _mSCFormsOfPayment = mSCFormsOfPayment;
            _legalDocumentsForTitlesService = legalDocumentsForTitlesService;
            _cachingService = cachingService;
            _shoppingCartUtility = shoppingCartUtility;
            _shoppingCartService = shoppingCartService;
            ConfigUtility.UtilityInitialize(_configuration);
            _featureSettings = featureSettings;
            _shoppingUtility = shoppingUtility;
        }

        public async Task<MOBFOPTravelerCertificateResponse> TravelerCertificate(MOBFOPTravelerCertificateRequest request)
        {
            
            var response = new MOBFOPTravelerCertificateResponse();
            if (request.Certificate != null)
                request.Certificate.PinCode = request.Certificate.PinCode.ToUpper();

            if (request.Certificate != null && request.Certificate.YearIssued.Length != 4)
            {
                throw new MOBUnitedException(_configuration.GetValue<string>("ETCInvalidCertificateMessage"));
            }

            if (request.CombineCertificates != null && request.CombineCertificates.Exists(C => C.YearIssued.Length != 4))
            {
                throw new MOBUnitedException(_configuration.GetValue<string>("ETCInvalidCertificateMessage"));
            }

            Session session = await _shoppingSessionHelper.GetBookingFlowSession(request.SessionId);
            if (session == null)
            {
                throw new MOBUnitedException("Could not find your booking session.");
            }

            response = await TravelerCertificate(request, session);

            if (ConfigUtility.IsManageResETCEnabled(request.Application.Id, request.Application.Version.Major))
            {
                response.Flow = request.Flow;
            }

            bool isDefault = false;
            var tupleRespEligibleFormofPayments = await _mSCFormsOfPayment.GetEligibleFormofPayments(request, session, response.ShoppingCart, request.CartId, request.Flow, response.Reservation);
            response.EligibleFormofPayments = tupleRespEligibleFormofPayments.EligibleFormofPayments;
            isDefault = tupleRespEligibleFormofPayments.isDefault;
            await _sessionHelperService.SaveSession<MOBFOPTravelerCertificateResponse>(response, session.SessionId, new List<string> { session.SessionId, response.ObjectName }, response.ObjectName);

            response.SessionId = request.SessionId;

            return await Task.FromResult(response);
        }

        public async Task<MOBFOPTravelerCertificateResponse> PersistFOPBillingContactInfo_ETC(MOBFOPBillingContactInfoRequest request)
        {
            
            var response = new MOBFOPTravelerCertificateResponse();

            Session session = await _shoppingSessionHelper.GetBookingFlowSession(request.SessionId);
            if (session == null)
            {
                throw new MOBUnitedException("Could not find your booking session.");
            }

            response = await PersistFOPBillingContactInfo(request, session);

            List<FormofPaymentOption> persistedEligibleFormofPayments = new List<FormofPaymentOption>();
            persistedEligibleFormofPayments = await _sessionHelperService.GetSession<List<FormofPaymentOption>>(session.SessionId, persistedEligibleFormofPayments.GetType().ToString(), new List<string> { session.SessionId, persistedEligibleFormofPayments.GetType().ToString() }).ConfigureAwait(false);

            response.EligibleFormofPayments = persistedEligibleFormofPayments;
            response.SessionId = request.SessionId;

            await _sessionHelperService.SaveSession<MOBFOPTravelerCertificateResponse>(response, session.SessionId, new List<string> { session.SessionId, response.ObjectName }, response.ObjectName);

            return await Task.FromResult(response);
        }
        public async Task<MOBFOPTravelerCertificateResponse> ApplyMileagePricing(MOBRTIMileagePricingRequest request)
        {
            MOBFOPTravelerCertificateResponse response = new MOBFOPTravelerCertificateResponse();

            #region
            Session session = await _shoppingSessionHelper.GetBookingFlowSession(request.SessionId);
            if (session == null)
            {
                throw new MOBUnitedException("Could not find your booking session.");
            }
            try
            {
                string cslActionName = "/api/ApplyTravelCreditsRTI";
                CultureInfo ci = TopHelper.GetCultureInfo("en-US");
                var applyTravelCreditsRequest = new TravelCreditsRequest
                {
                    CartId = request.CartId,
                    TravelCreditDetails = new TravelCreditDetails
                    {
                        LoyaltyId = request.MileagePlusAccountNumber
                    }
                };
                TravelCreditsResponse travelCreditsResponse;
                if (string.IsNullOrEmpty(request.PricingType) || request.PricingType == PricingType.MONEY.ToString() || request.PricingType == PricingType.MONEYPLUSMILES.ToString())
                {
                    cslActionName = "/api/RemoveTravelCredits/CartId=" + request.CartId;
                    travelCreditsResponse = await GetTravelCreditsResponse(null, session, cslActionName);

                }
                else
                {
                    travelCreditsResponse = await GetTravelCreditsResponse(applyTravelCreditsRequest, session, cslActionName);
                }
                if (travelCreditsResponse != null && travelCreditsResponse.Status.Equals(StatusType.Success) && travelCreditsResponse.DisplayCart != null && travelCreditsResponse.Errors.Count() == 0)
                {
                    string token = session.Token;
                    List<MOBFOPCertificate> mOBFOPCertificates = new List<MOBFOPCertificate>();
                    if (travelCreditsResponse.DisplayCart.SpecialPricingInfo?.TravelCreditDetails != null)
                    {
                        foreach (var certificate in travelCreditsResponse.DisplayCart.SpecialPricingInfo.TravelCreditDetails.TravelCertificates)
                        {
                            mOBFOPCertificates.Add(new MOBFOPCertificate
                            {
                                CertificateTraveler = null,
                                PinCode = certificate.CertPin.ToUpper()
                            });
                        }
                    }
                    //_ = await _eTCUtility.AddCertificatesToFOP(mOBFOPCertificates, session);

                    var persistShoppingCart = new MOBShoppingCart();
                    response.ShoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(session.SessionId, persistShoppingCart.ObjectName, new List<string> { session.SessionId, persistShoppingCart.ObjectName }).ConfigureAwait(false);

                    FlightReservationResponse loadFlightReservationResponse = await _sessionHelperService.GetSession<FlightReservationResponse>(session.SessionId, new FlightReservationResponse().GetType().FullName, new List<string> { session.SessionId, new FlightReservationResponse().GetType().FullName }).ConfigureAwait(false);
                    if (loadFlightReservationResponse != null)
                    {
                        loadFlightReservationResponse.DisplayCart = travelCreditsResponse.DisplayCart;
                    }
                    await _sessionHelperService.SaveSession<FlightReservationResponse>(loadFlightReservationResponse, session.SessionId, new List<string> { session.SessionId, typeof(FlightReservationResponse).FullName }, typeof(FlightReservationResponse).FullName).ConfigureAwait(false);

                    Reservation reservation = await _sessionHelperService.GetSession<Reservation>(request.SessionId, new Reservation().ObjectName, new List<string> { request.SessionId, new United.Persist.Definition.Shopping.Reservation().ObjectName }).ConfigureAwait(false);

                    //check for travel credits from FOP and set eligibleforetcpricingtype = true                    
                    //response.ShoppingCart = await _shoppingCartUtility.BuildShoppingCart(request, loadFlightReservationResponse, FlowType.BOOKING.ToString(), request.CartId, request.SessionId, session);
                    if (request.PricingType == PricingType.ETC.ToString())
                    {
                        session.PricingType = PricingType.ETC.ToString();
                        var travelCredits = travelCreditsResponse.DisplayCart?.DisplayPrices?.FirstOrDefault(x => x.Type.Equals("TravelCredits", StringComparison.OrdinalIgnoreCase));
                        if (travelCredits != null)
                        {
                            if (ci == null)
                            {
                                ci = _shoppingUtility.GetCultureInfo(travelCredits.Currency);
                            }
                            session.CreditsAmount = travelCredits.Amount.ToString();
                        }
                    }
                    else
                    {
                        session.PricingType = response.Reservation?.PricingType ?? PricingType.MONEY.ToString();

                    }
                    await _sessionHelperService.SaveSession<Session>(session, request.SessionId, new List<string> { request.SessionId, session.ObjectName }, session.ObjectName);

                    // _shoppingCartUtility.ApplyETCCreditsOnRTIAction(response.ShoppingCart, travelCreditsResponse.DisplayCart);

                    response.Reservation = _eTCUtility.MakeReservationFromPersistReservation(response.Reservation, reservation, session);
                    var tupleRespEligibleFormofPayments = await _mSCFormsOfPayment.GetEligibleFormofPayments(request, session, response.ShoppingCart, request.CartId, "BOOKING", response.Reservation);
                    response.EligibleFormofPayments = tupleRespEligibleFormofPayments.EligibleFormofPayments;
                    _shoppingUtility.SetETCTravelCreditsEligible(response.EligibleFormofPayments, response.Reservation);
                    response.Reservation.PricingType = response.Reservation.PricingType ?? PricingType.MONEY.ToString();

                    response.Reservation.Prices = await GetPrices(travelCreditsResponse.DisplayCart.DisplayPrices, false, session.SessionId, false, FlowType.BOOKING.ToString(), session: session);
                    _eTCUtility.AddGrandTotalIfNotExistInPrices(response.Reservation.Prices);
                    _shoppingCartUtility.BuildOmniCart(response.ShoppingCart, response.Reservation, request.Application);
                    if (_configuration.GetValue<bool>("EnableTravelCreditSummary"))
                    {
                        var travelCreditCount = response.ShoppingCart?.FormofPaymentDetails?.TravelCreditDetails?.TravelCredits?.Count ?? 0;

                        if (travelCreditCount > 0)
                        {
                            var travelCreditSummary = _configuration.GetValue<string>("TravelCreditSummary");
                            var pluralChar = travelCreditCount > 1 ? "s" : string.Empty;
                            response.ShoppingCart.FormofPaymentDetails.TravelCreditDetails.TravelCreditSummary = response.ShoppingCart?.Products?.FirstOrDefault().Code != "FLK" ? string.Format(travelCreditSummary, travelCreditCount, pluralChar) : string.Empty;
                            if (await _featureSettings.GetFeatureSettingValue("EnableFSRETCTravelCreditsFeature").ConfigureAwait(false))
                            {
                                travelCreditSummary = _configuration.GetValue<string>("PricingTypeTravelCreditSummary");
                                response.ShoppingCart.FormofPaymentDetails.TravelCreditDetails.TravelCreditSummary = response.ShoppingCart?.Products?.FirstOrDefault().Code != "FLK" ? string.Format(travelCreditSummary, travelCreditCount, pluralChar) : string.Empty;
                            }
                        }
                    }
                    if (request.PricingType == PricingType.MONEY.ToString() && response.ShoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit != null && _shoppingCartUtility.IsIncludeMoneyMilesInRTI(response.EligibleFormofPayments))
                        response.Reservation.IsEligibleForMoneyPlusMiles = true;


                    await _sessionHelperService.SaveSession<MOBShoppingCart>(response.ShoppingCart, request.SessionId, new List<string> { request.SessionId, persistShoppingCart.GetType().ToString() }, persistShoppingCart.GetType().ToString()).ConfigureAwait(false);

                }
                else
                {
                _logger.ILoggerInfo("ApplyMileagePricing : There is problem Apppling MileagePricing ApplyMileagePricing exception , {@Request} " + JsonConvert.SerializeObject(travelCreditsResponse), request);
                    throw new MOBUnitedException("Unable to apply Travel credits");
                }
                return await Task.FromResult(response);
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("ApplyMileagePricing UnitedException:{Exception} with SessionId:{sessionId}", JsonConvert.SerializeObject(uaex), session.SessionId);
                throw uaex;
            }
            catch (Exception ex)
            {
                _logger.LogError("ApplyMileagePricing Exception:{Exception} with SessionId:{sessionId}", JsonConvert.SerializeObject(ex), session.SessionId);
                throw;
            }
            #endregion
        }        

        private async Task<MOBFOPTravelerCertificateResponse> TravelerCertificate(MOBFOPTravelerCertificateRequest request, Session session)
        {

            MOBFOPTravelerCertificateResponse response = new MOBFOPTravelerCertificateResponse();
            string token = session.Token;
            string jsonResponse = "";

            if (ConfigUtility.IsETCCombinabilityEnabled(request.Application.Id, request.Application.Version.Major) && request.CombineCertificates != null && request.CombineCertificates.Count > 0)
            {
                foreach (MOBFOPCertificate certificate in request.CombineCertificates.Where(c => !c.IsRemove))
                {
                    certificate.CertificateTraveler = null;
                    certificate.PinCode = certificate.PinCode.ToUpper();
                    jsonResponse = await GetETCBalaceInquiry(request, token, certificate);

                    ValidateBalanceEnquiryResponse(jsonResponse, certificate);

                }

                response = await _eTCUtility.AddCertificatesToFOP(request.CombineCertificates, session);

                _logger.LogInformation("TravelerCertificate - AddCertificatesToFOP {response} with {sessionId}", JsonConvert.SerializeObject(response), request.SessionId);

                response.PkDispenserPublicKey = await _pKDispenserPublicKey.GetCachedOrNewpkDispenserPublicKey(request.Application.Id, request.Application.Version.Major, request.DeviceId, session.SessionId, session.Token , session?.CatalogItems).ConfigureAwait(false);
            }
            else
            {
                if (!request.IsRemove)
                {
                    jsonResponse =await GetETCBalaceInquiry(request, token);

                    response = await DeserialiseAndBuildMOBFOPTravelerCertificateResponse(jsonResponse, session, request);
                }
                else
                {
                    response = await RemoveCertificateFromPersist(session, request.Certificate);
                }
            }

            return response;
        }

        private async Task<MOBFOPTravelerCertificateResponse> DeserialiseAndBuildMOBFOPTravelerCertificateResponse(string jsonResponse, Session session, MOBFOPTravelerCertificateRequest request)
        {
            MOBFOPTravelerCertificateResponse response = new MOBFOPTravelerCertificateResponse();

            if (!string.IsNullOrEmpty(jsonResponse))
            {
                var balanceInquiryResponse = JsonConvert.DeserializeObject<BalanceInquiry>(jsonResponse);
                if (balanceInquiryResponse != null &&
                    balanceInquiryResponse.FormOfPayment != null &&
                    balanceInquiryResponse.Response != null &&
                    balanceInquiryResponse.Response.Message != null &&
                    balanceInquiryResponse.Response.Message.Count > 0 &&
                    balanceInquiryResponse.Response.Message.Exists(p => p.Text.ToUpper().Equals("SUCCESS")))
                {
                    Reservation bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(session.SessionId, new Reservation().ObjectName, new List<string> { session.SessionId, new Reservation().ObjectName }).ConfigureAwait(false);

                    MOBFOPTravelCertificate travelerCertificate = await _eTCUtility.GetShoppingCartTravelCertificateFromPersist(session, response);

                    var certificate = balanceInquiryResponse.FormOfPayment as Certificate;
                    request.Certificate.InitialValue = certificate.InitialValue;
                    request.Certificate.CurrentValue = certificate.CurrentValue;
                    if (ConfigUtility.IsETCCombinabilityEnabled(request.Application.Id, request.Application.Version.Major))
                    {
                        request.Certificate.RecipientsFirstName = certificate.Payor.FirstName ?? certificate.Payor.GivenName;
                        request.Certificate.RecipientsLastName = certificate.Payor.Surname;
                    }
                    request.Certificate.ExpiryDate = Convert.ToDateTime(certificate.ExpirationDate).ToString("MMMM dd, yyyy");
                    UpdateCertificateAmountInTotalPrices(bookingPathReservation.Prices, response.ShoppingCart.Products, 0);
                    RemoveEditingIndexAndUpdateCertificatesWithRequestValues(request.Certificate, response.ShoppingCart.CertificateTravelers, response.ShoppingCart.ProfileTravelerCertificates, travelerCertificate);
                    if (ConfigUtility.IsETCEnabledforMultiTraveler(request.Application.Id, request.Application.Version.Major.ToString()) &&
                        request.Certificate.CertificateTraveler != null && response.ShoppingCart.SCTravelers.Count > 1 &&
                        !string.IsNullOrEmpty(request.Certificate.CertificateTraveler.TravelerNameIndex))
                    {
                        UpdateMultiTravelerCertificate(travelerCertificate.Certificates, response.ShoppingCart.CertificateTravelers, request.Certificate, response.ShoppingCart.ProfileTravelerCertificates);
                    }
                    else
                    {
                        _eTCUtility.AssignCertificateRedeemAmount(request.Certificate, bookingPathReservation.Prices);
                        if (request.Certificate.IsForAllTravelers)
                        {
                            travelerCertificate.Certificates.Clear();
                        }

                        if (travelerCertificate.Certificates == null || !travelerCertificate.Certificates.Exists(c => c.PinCode == request.Certificate.PinCode))
                        {
                            travelerCertificate.Certificates.Add(request.Certificate);
                        }
                        else
                        {
                            var certificateInPersist = travelerCertificate.Certificates.Find(c => c.PinCode == request.Certificate.PinCode);
                            travelerCertificate.Certificates.Remove(certificateInPersist);
                            travelerCertificate.Certificates.Add(request.Certificate);
                        }
                    }
                    if (_configuration.GetValue<bool>("EnableSelectDifferentFOPAtRTI"))
                    {
                        if (bookingPathReservation.ShopReservationInfo2 == null)
                        {
                            bookingPathReservation.ShopReservationInfo2 = new MOBSHOPReservationInfo2();
                        }

                        if ((travelerCertificate.Certificates == null || travelerCertificate.Certificates.Count == 0) && response.ShoppingCart?.FormofPaymentDetails?.CreditCard != null)
                            bookingPathReservation.ShopReservationInfo2.ShowSelectDifferentFOPAtRTI = true;
                        else
                            bookingPathReservation.ShopReservationInfo2.ShowSelectDifferentFOPAtRTI = false;
                    }
                    #region ETC Balance Attention Message
                    if (_configuration.GetValue<bool>("EnableETCBalanceAttentionMessageOnRTI"))
                    {
                        await AssignBalanceAttentionInfoWarningMessage (bookingPathReservation.ShopReservationInfo2, travelerCertificate);
                    }
                    #endregion
                    await _eTCUtility.UpdateReservationWithCertificatePrices(session, response, travelerCertificate.TotalRedeemAmount, bookingPathReservation);
                    _eTCUtility.AssignIsOtherFOPRequired(response.ShoppingCart.FormofPaymentDetails, bookingPathReservation.Prices);

                    //Get Learmore Details
                    await _eTCUtility.AssignLearmoreInformationDetails(travelerCertificate);

                    _eTCUtility.UpdateSavedCertificate(response.ShoppingCart);

                    await _sessionHelperService.SaveSession<MOBShoppingCart>(response.ShoppingCart, session.SessionId, new List<string> { session.SessionId, response.ShoppingCart.ObjectName }, response.ShoppingCart.ObjectName).ConfigureAwait(false);

                }
                else
                {
                    throw new MOBUnitedException(_configuration.GetValue<string>("ETCInvalidCertificateMessage"));
                }
            }
            else
            {
                throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }

            _logger.LogInformation("DeserialiseAndBuildMOBFOPTravelerCertificateResponse response:{response} with {sessioniD}", JsonConvert.SerializeObject(response), request.SessionId);

            return response;
        }

        private async Task AssignBalanceAttentionInfoWarningMessage(MOBSHOPReservationInfo2 shopReservationInfo2, MOBFOPTravelCertificate travelCertificate)
        {
            if (shopReservationInfo2 == null)
            {
                shopReservationInfo2 = new MOBSHOPReservationInfo2();
            }
            //To show balance attention message on RTI when Combinability is ON from Shoppingcart service  and OFF from MRest
            if (shopReservationInfo2.InfoWarningMessages == null)
            {
                shopReservationInfo2.InfoWarningMessages = new List<MOBInfoWarningMessages>();
            }
            MOBInfoWarningMessages balanceAttentionMessage = new MOBInfoWarningMessages();
            balanceAttentionMessage = await GetETCBalanceAttentionInfoWarningMessage(travelCertificate);
            if (shopReservationInfo2.InfoWarningMessages.Exists(x => x.Order == MOBINFOWARNINGMESSAGEORDER.RTIETCBALANCEATTENTION.ToString()))
            {
                shopReservationInfo2.InfoWarningMessages.Remove(shopReservationInfo2.InfoWarningMessages.Find(x => x.Order == MOBINFOWARNINGMESSAGEORDER.RTIETCBALANCEATTENTION.ToString()));
            }
            if (balanceAttentionMessage != null)
            {
                shopReservationInfo2.InfoWarningMessages.Add(balanceAttentionMessage);
                shopReservationInfo2.InfoWarningMessages = shopReservationInfo2.InfoWarningMessages.OrderBy(c => (int)((MOBINFOWARNINGMESSAGEORDER)Enum.Parse(typeof(MOBINFOWARNINGMESSAGEORDER), c.Order))).ToList();
            }
        }

        private async Task<List<MOBMobileCMSContentMessages>> AssignAlertMessages(string captionKey)
        {
            List<MOBMobileCMSContentMessages> tncs = null;
            var docs = await GetCaptions(captionKey);
            if (docs != null && docs.Any())
            {
                tncs = new List<MOBMobileCMSContentMessages>();
                foreach (var doc in docs)
                {
                    var tnc = new MOBMobileCMSContentMessages
                    {
                        ContentFull = doc.CurrentValue,
                        HeadLine = doc.Id
                    };
                    tncs.Add(tnc);
                }
            }

            return tncs;
        }

        private async Task<List<MOBItem>> GetCaptions(string key)
        {
            return !string.IsNullOrEmpty(key) ? await GetCaptions(key, true) : null;
        }

        private async Task<List<MOBItem>> GetCaptions(string keyList, bool isTnC)
        {
            var docs = await _legalDocumentsForTitlesService.GetNewLegalDocumentsForTitles(keyList,"trans0", isTnC).ConfigureAwait(false);
            if (docs == null || !docs.Any()) return null;

            var captions = new List<MOBItem>();

            captions.AddRange(
                docs.Select(doc => new MOBItem
                {
                    Id = doc.Title,
                    CurrentValue = doc.Document
                }));

            return captions;
        }

        private async Task<MOBInfoWarningMessages> GetETCBalanceAttentionInfoWarningMessage(MOBFOPTravelCertificate travelCertificate)
        {
            MOBInfoWarningMessages infoMessage = null;
            double? etcBalanceAttentionAmount = travelCertificate?.Certificates?.Sum(c => c.NewValueAfterRedeem);
            if (etcBalanceAttentionAmount > 0 && travelCertificate?.Certificates?.Count > 1)
            {
                List<MOBMobileCMSContentMessages> alertMessages = new List<MOBMobileCMSContentMessages>();
                alertMessages = await AssignAlertMessages("TravelCertificate_Combinability_ReviewETCAlertMsg");
                infoMessage = new MOBInfoWarningMessages();
                infoMessage.Order = MOBINFOWARNINGMESSAGEORDER.RTIETCBALANCEATTENTION.ToString();
                infoMessage.IconType = MOBINFOWARNINGMESSAGEICON.INFORMATION.ToString();
                infoMessage.Messages = new List<string>();
                infoMessage.Messages.Add(string.Format(alertMessages.Find(x => x.HeadLine == "TravelCertificate_Combinability_ReviewETCAlertMsgs_ETCBalanceAttentionmessage").ContentFull, String.Format("{0:0.00}", etcBalanceAttentionAmount)));
            }

            return infoMessage;
        }

        private void IsRequestCertifiateAlreadyAppliedForOtherTraveler(List<MOBFOPCertificate> certificates, MOBFOPCertificate requestCertificate)
        {
            if (requestCertificate.Index == 0 && certificates != null && certificates.Exists(c => c.PinCode == requestCertificate.PinCode && c.CertificateTraveler.TravelerNameIndex != requestCertificate.CertificateTraveler.TravelerNameIndex)
                || (requestCertificate.Index > 0 && certificates != null && certificates.Exists(c => c.PinCode == requestCertificate.PinCode && c.Index != requestCertificate.Index)))
            {
                throw new MOBUnitedException(_configuration.GetValue<string>("ETCAlreadyAppliedToOtherTravelerMessage"));
            }
        }

        private void UpdateMultiTravelerCertificate(List<MOBFOPCertificate> certificates, List<MOBFOPCertificateTraveler> certificateTravelers, MOBFOPCertificate requestCertificate, List<MOBFOPCertificate> profileSacedCertificates)
        {
            IsRequestCertifiateAlreadyAppliedForOtherTraveler(certificates, requestCertificate);
            if (requestCertificate.CertificateTraveler == null && certificateTravelers.Count == 1)
            {
                requestCertificate.CertificateTraveler = certificateTravelers[0];
                requestCertificate.CertificateTraveler.IsCertificateApplied = true;
            }
            if (requestCertificate.CertificateTraveler?.TravelerNameIndex == "0")
            {
                certificates.Clear();
                certificateTravelers = certificateTravelers.Select(c => { c.IsCertificateApplied = false; return c; }).ToList();
            }
            var certificateTraveler = certificateTravelers.Find(ct => ct.TravelerNameIndex == requestCertificate.TravelerNameIndex);

            _eTCUtility.AssignCertificateRedeemAmount(requestCertificate, certificateTraveler.IndividualTotalAmount);
            certificateTraveler.IsCertificateApplied = true;

            if (certificates == null || !certificates.Exists(c => c.Index == requestCertificate.Index))
            {
                if (certificates == null)
                    certificates = new List<MOBFOPCertificate>();
                if (_configuration.GetValue<bool>("SavedETCToggle"))
                {
                    int profileMaxId = ((profileSacedCertificates != null && profileSacedCertificates.Count > 0) ? profileSacedCertificates.Max(c => c.Index) + 1 : 1);
                    int addedCerrtMaxId = ((certificates != null && certificates.Count > 0) ? certificates.Max(c => c.Index) + 1 : 1);
                    if (!requestCertificate.IsProfileCertificate)
                    {
                        requestCertificate.Index = profileMaxId > addedCerrtMaxId ? profileMaxId : addedCerrtMaxId;
                    }
                }
                else
                {
                    requestCertificate.Index = (certificates.Count > 0 ? certificates.Max(c => c.Index) + 1 : 1);
                }
                certificates.Add(requestCertificate);
            }
            else
            {
                var certificateInPersist = certificates.Find(c => c.Index == requestCertificate.Index);
                if (certificateInPersist?.CertificateTraveler?.TravelerNameIndex != requestCertificate.CertificateTraveler.TravelerNameIndex)
                {
                    var traverlerCertificate = certificateTravelers.Find(ct => ct.TravelerNameIndex == certificateInPersist.CertificateTraveler.TravelerNameIndex);
                    if (traverlerCertificate != null)
                    {
                        traverlerCertificate.IsCertificateApplied = false;
                    }
                }
                certificates.Remove(certificateInPersist);
                certificates.Add(requestCertificate);
            }

            SelectAllTravelersAndAssignIsCertificateApplied(certificateTravelers, certificates);
        }

        private void SelectAllTravelersAndAssignIsCertificateApplied(List<MOBFOPCertificateTraveler> certTravelersCopy, List<MOBFOPCertificate> certificates)
        {
            var allTraveler = certTravelersCopy.Find(ct => ct.TravelerNameIndex == "0");
            if (allTraveler != null)
            {
                allTraveler.IsCertificateApplied = certificates.Count > 0;
            }
        }

        private void RemoveEditingIndexAndUpdateCertificatesWithRequestValues(MOBFOPCertificate requestCertificate, List<MOBFOPCertificateTraveler> certificateTravelers, List<MOBFOPCertificate> profileSacedCertificates, MOBFOPTravelCertificate travelerCertificate)
        {
            if (_configuration.GetValue<bool>("SavedETCToggle") && requestCertificate.EditingIndex > 0 && requestCertificate.EditingIndex != requestCertificate.Index)
            {
                var certificateInPersist = travelerCertificate.Certificates.Find(c => c.Index == requestCertificate.EditingIndex);
                if (certificateInPersist?.CertificateTraveler?.TravelerNameIndex != requestCertificate.CertificateTraveler.TravelerNameIndex)
                {
                    var traverlerCertificate = certificateTravelers.Find(ct => ct.TravelerNameIndex == certificateInPersist.CertificateTraveler.TravelerNameIndex);
                    if (traverlerCertificate != null)
                    {
                        traverlerCertificate.IsCertificateApplied = false;
                    }
                }

                travelerCertificate.Certificates.Remove(certificateInPersist);
                if (profileSacedCertificates.Exists(c => c.Index == requestCertificate.EditingIndex))
                {
                    profileSacedCertificates.Find(c => c.Index == requestCertificate.EditingIndex).IsCertificateApplied = false;
                }
            }
        }

        private void UpdateCertificateAmountInTotalPrices(List<MOBSHOPPrice> prices, List<MOBProdDetail> scProducts, double certificateTotalAmount, bool isShoppingCartProductsGotRefresh = false)
        {
            var certificatePrice = prices.FirstOrDefault(p => p.DisplayType.ToUpper() == "CERTIFICATE");
            var EtccertificatePrice = prices.FirstOrDefault(p => p.DisplayType.ToUpper() == "TravelCredits");
            var scRESProduct = scProducts.Find(p => p.Code == "RES");
            var grandtotal = prices.FirstOrDefault(p => p.DisplayType.ToUpper() == "GRAND TOTAL");
            if (certificatePrice == null && certificateTotalAmount > 0)
            {
                certificatePrice = new MOBSHOPPrice();
                _eTCUtility.UpdateCertificatePrice(certificatePrice, certificateTotalAmount);
                prices.Add(certificatePrice);
            }
            else if (certificatePrice != null)
            {
                //this two lines adding certificate price back to total for removing latest certificate amount in next lines
                if (!isShoppingCartProductsGotRefresh)
                {
                    UtilityHelper.UpdateCertificateRedeemAmountInSCProductPrices(scRESProduct, certificatePrice.Value, false);
                }

                _eTCUtility.UpdateCertificateRedeemAmountFromTotalInReserationPrices(grandtotal, certificatePrice.Value, false);

                if (_configuration.GetValue<bool>("MTETCToggle"))
                {
                    _eTCUtility.UpdateCertificatePrice(certificatePrice, certificateTotalAmount);
                }
            }

            if (certificateTotalAmount == 0 && certificatePrice != null)
            {
                prices.Remove(certificatePrice);
            }

            UtilityHelper.UpdateCertificateRedeemAmountInSCProductPrices(scRESProduct, certificateTotalAmount);

            _eTCUtility.UpdateCertificateRedeemAmountFromTotalInReserationPrices(grandtotal, certificateTotalAmount);
        }

        private async Task<MOBFOPTravelerCertificateResponse> RemoveCertificateFromPersist(Session session, MOBFOPCertificate certificate)
        {
            var response = new MOBFOPTravelerCertificateResponse();
            bool isAllcertificatesRemoved = false;
            //Shopping cart
            MOBFOPTravelCertificate travelerCertificate = await _eTCUtility.GetShoppingCartTravelCertificateFromPersist(session, response);
            Reservation bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(session.SessionId, new Reservation().ObjectName, new List<string> { session.SessionId, new Reservation().ObjectName }).ConfigureAwait(false);

            MOBFOPCertificate removingCertificate = null;
            if (_configuration.GetValue<bool>("MTETCToggle") && bookingPathReservation.ShopReservationInfo2.IsMultipleTravelerEtcFeatureClientToggleEnabled)
            {
                removingCertificate = travelerCertificate.Certificates.Find(c => c.Index == certificate.Index);
            }
            else
            {
                removingCertificate = travelerCertificate.Certificates.Find(c => c.PinCode == certificate.PinCode);
            }
            if (removingCertificate != null)
            {
                if (removingCertificate.CertificateTraveler != null)
                {
                    var removingCertificateTraveler = response.ShoppingCart.CertificateTravelers.Find(ct => ct.TravelerNameIndex == removingCertificate.TravelerNameIndex);
                    if (removingCertificateTraveler != null)
                    {
                        removingCertificateTraveler.IsCertificateApplied = false;
                    }
                }
                if (_configuration.GetValue<bool>("SavedETCToggle") && response.ShoppingCart.ProfileTravelerCertificates != null && response.ShoppingCart.ProfileTravelerCertificates.Exists(c => c.Index == removingCertificate.Index))
                {
                    response.ShoppingCart.ProfileTravelerCertificates.Find(c => c.Index == removingCertificate.Index).IsCertificateApplied = false;
                }
                travelerCertificate.Certificates.Remove(removingCertificate);
                SelectAllTravelersAndAssignIsCertificateApplied(response.ShoppingCart.CertificateTravelers, travelerCertificate.Certificates);
                UpdateSavedCertificate(response.ShoppingCart);
            }

            double totalRedeemAmount = (travelerCertificate == null) ? 0 : travelerCertificate.TotalRedeemAmount;

            if (_configuration.GetValue<bool>("EnableSelectDifferentFOPAtRTI"))
            {
                if (travelerCertificate.Certificates.Count == 0 && response.ShoppingCart.FormofPaymentDetails.CreditCard != null)
                    bookingPathReservation.ShopReservationInfo2.ShowSelectDifferentFOPAtRTI = true;
                else
                    bookingPathReservation.ShopReservationInfo2.ShowSelectDifferentFOPAtRTI = false;
            }

            #region ETC Balance Attention Message
            if (_configuration.GetValue<bool>("EnableETCBalanceAttentionMessageOnRTI"))
            {
                await AssignBalanceAttentionInfoWarningMessage (bookingPathReservation.ShopReservationInfo2, travelerCertificate);
            }
            #endregion
            await _eTCUtility.UpdateReservationWithCertificatePrices(session, response, totalRedeemAmount, bookingPathReservation);

            if (travelerCertificate.Certificates.Count == 0)
            {
                isAllcertificatesRemoved = true;
                response.ShoppingCart.FormofPaymentDetails.TravelCertificate = null;
            }

            _eTCUtility.AssignIsOtherFOPRequired(response.ShoppingCart.FormofPaymentDetails, bookingPathReservation.Prices, false, isAllcertificatesRemoved);

            await _sessionHelperService.SaveSession<MOBShoppingCart>(response.ShoppingCart, session.SessionId, new List<string> { session.SessionId, response.ShoppingCart.ObjectName }, response.ShoppingCart.ObjectName).ConfigureAwait(false);

            _logger.LogInformation("RemoveCertificateFromPersist Response:{response} with sessionId:{sessionId}", JsonConvert.SerializeObject(response), session.SessionId);

            return response;
        }

        private void UpdateSavedCertificate(MOBShoppingCart shoppingcart)
        {
            if (_configuration.GetValue<bool>("SavedETCToggle") && shoppingcart != null)
            {
                var shoppingCaartCertificates = shoppingcart.ProfileTravelerCertificates;
                var appliedCertificates = shoppingcart.FormofPaymentDetails?.TravelCertificate?.Certificates;
                if (shoppingCaartCertificates?.Count > 0 && appliedCertificates != null)
                {
                    foreach (var shoppingCaartCertificate in shoppingCaartCertificates)
                    {
                        var appliedCertificate = appliedCertificates.Exists(c => c.Index == shoppingCaartCertificate.Index);
                        shoppingCaartCertificate.IsCertificateApplied = appliedCertificate;
                        if (appliedCertificate)
                        {
                            appliedCertificates.Find(c => c.Index == shoppingCaartCertificate.Index).IsProfileCertificate = appliedCertificate;
                        }
                    }
                }
            }
        }

        private async Task<string> GetETCBalaceInquiry(MOBFOPTravelerCertificateRequest request, string token, MOBFOPCertificate requestCertificate = null)
        {
            string url = "/PaymentBroker/BalanceInquiry";

            Service.Presentation.PaymentRequestModel.BalanceInquiry balanceInquiryRequest = null;
            if (requestCertificate != null)
            {
                balanceInquiryRequest = BuildBalanceInquiryRequest(requestCertificate);
            }
            else
            {
                balanceInquiryRequest = BuildBalanceInquiryRequest(request.Certificate);
            }
            string jsonRequest = DataContextJsonSerializer.Serialize(balanceInquiryRequest);

            string response = await _iETCBalanceEnquiryService.GetETCBalanceInquiry(url, jsonRequest, request.SessionId, token).ConfigureAwait(false);

            return response;
        }

        private Service.Presentation.PaymentRequestModel.BalanceInquiry BuildBalanceInquiryRequest(MOBFOPCertificate requestCertificate)
        {
            United.Service.Presentation.PaymentRequestModel.BalanceInquiry balanceInquiry = new United.Service.Presentation.PaymentRequestModel.BalanceInquiry();

            balanceInquiry.AccountHolder = new LoyaltyPerson();
            balanceInquiry.AccountHolder.Surname = requestCertificate.RecipientsLastName;

            balanceInquiry.CallingService = new ServiceClient();
            balanceInquiry.CallingService.Requestor = new Requestor();
            balanceInquiry.CallingService.Requestor.AgentAAA = "HQS";
            balanceInquiry.CallingService.Requestor.AgentSine = "UA";
            balanceInquiry.CallingService.Requestor.ApplicationSource = "Mobile";
            balanceInquiry.CallingService.Requestor.Device = new Service.Presentation.CommonModel.Device();
            balanceInquiry.CallingService.Requestor.Device.LNIATA = "Mobile";
            balanceInquiry.CallingService.Requestor.DutyCode = "SU";
            Certificate certificate = new Certificate();
            certificate.OperationID = Guid.NewGuid().ToString();
            certificate.PinCode = requestCertificate.PinCode;
            certificate.PromoCode = requestCertificate.YearIssued.Substring(2, 2) + "TCVA";

            balanceInquiry.FormOfPayment = certificate;

            return balanceInquiry;
        }

        private bool ValidateBalanceEnquiryResponse(string cslBalanceEnquiryResponse, MOBFOPCertificate certificate)
        {
            bool isValidCertificate = false;
            if (!string.IsNullOrEmpty(cslBalanceEnquiryResponse))
            {
                var balanceInquiryResponse = DataContextJsonSerializer.DeserializeJsonDataContract<BalanceInquiry>(cslBalanceEnquiryResponse);
                if (balanceInquiryResponse != null &&
                    balanceInquiryResponse.FormOfPayment != null &&
                    balanceInquiryResponse.Response != null &&
                    balanceInquiryResponse.Response.Message != null &&
                    balanceInquiryResponse.Response.Message.Count > 0 &&
                    balanceInquiryResponse.Response.Message.Exists(p => p.Text.ToUpper().Equals("SUCCESS")))
                {
                    var cslCertificate = balanceInquiryResponse.FormOfPayment as Certificate;
                    certificate.InitialValue = cslCertificate.InitialValue;
                    certificate.CurrentValue = cslCertificate.CurrentValue;
                    certificate.RecipientsFirstName = cslCertificate.Payor.FirstName ?? cslCertificate.Payor.GivenName;
                    certificate.RecipientsLastName = cslCertificate.Payor.Surname;
                    certificate.ExpiryDate = Convert.ToDateTime(cslCertificate.ExpirationDate).ToString("MMMM dd, yyyy");
                    isValidCertificate = true;
                }
                else
                {
                    throw new MOBUnitedException(_configuration.GetValue<string>("ETCInvalidCertificateMessage"));
                }
            }
            else
            {
                throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }

            return isValidCertificate;
        }

        private async Task<MOBFOPTravelerCertificateResponse> PersistFOPBillingContactInfo(MOBFOPBillingContactInfoRequest request, Session session)
        {
            MOBFOPTravelerCertificateResponse response = new MOBFOPTravelerCertificateResponse();
            if (request != null && request.BillingAddress != null && request.BillingAddress.State != null && !string.IsNullOrEmpty(request.BillingAddress.State.Code.Trim()))
            {
                string stateCode = string.Empty; //MOBAddress
                bool validStateCode = false;
                MOBPersistFormofPaymentRequest mobPersistFormofPaymentRequest = new MOBPersistFormofPaymentRequest();
                mobPersistFormofPaymentRequest.Application = request.Application;
                mobPersistFormofPaymentRequest.FormofPaymentDetails = new MOBFormofPaymentDetails();
                mobPersistFormofPaymentRequest.FormofPaymentDetails.BillingAddress = request.BillingAddress;
                mobPersistFormofPaymentRequest.SessionId = request.SessionId;
                mobPersistFormofPaymentRequest.LanguageCode = request.LanguageCode;
                (validStateCode, stateCode) = await GetAndValidateStateCode_CFOP(mobPersistFormofPaymentRequest, session);
                if (validStateCode)
                {
                    request.BillingAddress.State.Code = stateCode;
                }
            }

            response.ShoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(session.SessionId, response.ShoppingCart.ObjectName, new List<string> { session.SessionId, response.ShoppingCart.ObjectName }).ConfigureAwait(false);

            if (response.ShoppingCart == null)
            {
                response.ShoppingCart = null;
            }
            if (response.ShoppingCart.FormofPaymentDetails == null)
            {
                response.ShoppingCart.FormofPaymentDetails = new MOBFormofPaymentDetails();
            }

            response.ShoppingCart.FormofPaymentDetails.Phone = request.Phone;
            response.ShoppingCart.FormofPaymentDetails.Email = request.Email;
            response.ShoppingCart.FormofPaymentDetails.BillingAddress = request.BillingAddress;
            response.ShoppingCart.FormofPaymentDetails.EmailAddress = request.Email.EmailAddress;

            if (ConfigUtility.IncludeFFCResidual(request.Application.Id, request.Application.Version.Major) && (!_configuration.GetValue<bool>("DisableMOBILE-13592Fix") ? request.Flow == FlowType.BOOKING.ToString() : true))
            {
                Reservation bookingPathReservation = new Reservation();
                bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(session.SessionId, bookingPathReservation.ObjectName, new List<string> { session.SessionId, bookingPathReservation.ObjectName }).ConfigureAwait(false);

                bookingPathReservation.ReservationEmail = request.Email;
                bookingPathReservation.ReservationPhone = request.Phone;
                bookingPathReservation.CreditCardsAddress = new List<MOBAddress>();
                bookingPathReservation.CreditCardsAddress.Add(request.BillingAddress);

                await _sessionHelperService.SaveSession<Reservation>(bookingPathReservation, session.SessionId, new List<string> { session.SessionId, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName).ConfigureAwait(false);
            }

            if (!(ConfigUtility.IsManageResETCEnabled(request.Application.Id, request.Application.Version.Major) && (request.Flow == FlowType.VIEWRES.ToString() || request.Flow == FlowType.VIEWRES_SEATMAP.ToString())))
            {
                response.Reservation = new MOBSHOPReservation(_configuration,_cachingService);
                //Reservation object Load
                response.Reservation = await _eTCUtility.GetReservationFromPersist(response.Reservation, session);
                //response.Reservation.UpdateRewards(_configuration, _cachingService);
            }
            //Update the email address for emailconfiramtion message on RTI if FFCR is added.
            if (ConfigUtility.IncludeFFCResidual(request.Application.Id, request.Application.Version.Major))
            {
                if (response.ShoppingCart?.FormofPaymentDetails?.TravelFutureFlightCredit?.FutureFlightCredits?.Count > 0)
                {
                    List<CMSContentMessage> lstMessages = await GetSDLContentByGroupName(request, session.SessionId, session?.Token, _configuration.GetValue<string>("CMSContentMessages_GroupName_BookingRTI_Messages"), "BookingPathRTI_CMSContentMessagesCached_StaticGUID");
                    response.ShoppingCart.FormofPaymentDetails.TravelFutureFlightCredit.EmailConfirmationFFCMessages = AssignEmailMessageForFFCRefund(lstMessages, response.Reservation.Prices, response.ShoppingCart.FormofPaymentDetails.EmailAddress, response.ShoppingCart.FormofPaymentDetails.TravelFutureFlightCredit, request.Application);
                }
            }
            #region OfferCode expansion Changes(This properties are added as part of offercode expansion changes but can be later used in general )
            response.ShoppingCart.FormofPaymentDetails.IsFOPRequired =_shoppingCartUtility.IsFOPRequired(response.ShoppingCart, response.Reservation);
            response.ShoppingCart.FormofPaymentDetails.IsEnableAgreeandPurchaseButton = _shoppingCartUtility.IsEnableAgreeandPurchaseButton(response.ShoppingCart, response.Reservation);
            response.ShoppingCart.FormofPaymentDetails.MaskedPaymentMethod = await _shoppingCartUtility.AssignMaskedPaymentMethod(response.ShoppingCart,request.Application).ConfigureAwait(false);
            #endregion OfferCode expansion Changes
            await _sessionHelperService.SaveSession<MOBShoppingCart>(response.ShoppingCart, session.SessionId, new List<string> { session.SessionId, response.ShoppingCart.ObjectName }, response.ShoppingCart.ObjectName).ConfigureAwait(false);
            if (!_configuration.GetValue<bool>("DisableTravelCreditBannerCheckForUpdateContact"))
            {
                var isMandatory = response.ShoppingCart?.FormofPaymentDetails?.CreditCard != null ? response.ShoppingCart.FormofPaymentDetails.CreditCard.IsMandatory : false;
                if ((response.ShoppingCart?.FormofPaymentDetails?.MoneyPlusMilesCredit?.SelectedMoneyPlusMiles != null ||
                     response.ShoppingCart?.FormofPaymentDetails?.TravelBankDetails?.TBApplied > 0 ||
                     response.ShoppingCart?.FormofPaymentDetails?.FormOfPaymentType == MOBFormofPayment.ApplePay.ToString() ||
                     response.ShoppingCart?.FormofPaymentDetails?.FormOfPaymentType == MOBFormofPayment.PayPal.ToString() ||
                     response.ShoppingCart?.FormofPaymentDetails?.FormOfPaymentType == MOBFormofPayment.PayPalCredit.ToString() ||
                     response.ShoppingCart?.FormofPaymentDetails?.FormOfPaymentType == MOBFormofPayment.Masterpass.ToString() ||
                     response.ShoppingCart?.FormofPaymentDetails?.FormOfPaymentType == MOBFormofPayment.Uplift.ToString() ||
                     isMandatory)
                     && !string.IsNullOrEmpty(response.ShoppingCart?.FormofPaymentDetails?.TravelCreditDetails?.TravelCreditSummary))
                {
                    response.ShoppingCart.FormofPaymentDetails.TravelCreditDetails.TravelCreditSummary = string.Empty;
                }
                //Apple Pay to remove
                if (isMandatory)
                {
                    response.EligibleFormofPayments = response.EligibleFormofPayments.Where(x => x.Category.ToUpper() != "APP").ToList();
                }
            }
            response.Profiles = await _eTCUtility.LoadPersistedProfile(session.SessionId, request.Flow);

            _logger.LogInformation("PersistFOPBillingContactInfo Response:{response} with sessionId:{sessioId}", JsonConvert.SerializeObject(response), session.SessionId);

            return response;
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

            response = await _cMSContentService.GetSDLContentByGroupName<MOBCSLContentMessagesResponse>(token, "message", jsonRequest, guid).ConfigureAwait(false);

            if (response != null)
            {
                _logger.LogInformation("GetSDLContentByGroupName CSL response is empty or null {response} {sessionId}", response, guid);
            }

            if (response == null)
            {
                _logger.LogInformation("GetSDLContentByGroupName Failed to deserialize CSL response {sessionId}", guid);
                return null;
            }

            if (response.Errors.Count > 0)
            {
                string errorMsg = String.Join(" ", response.Errors.Select(x => x.Message));
                _logger.LogInformation("GetSDLContentByGroupName {errorMsg} {sessionId}", errorMsg, guid);

                return null;
            }

            if (response != null && (Convert.ToBoolean(response.Status) && response.Messages != null))
            {
                if (!_configuration.GetValue<bool>("DisableSDLEmptyTitleFix"))
                {
                    response.Messages = response.Messages.Where(l => l.Title != null)?.ToList();
                }
                var saveSDL = await _cachingService.SaveCache<MOBCSLContentMessagesResponse>(_configuration.GetValue<string>(docNameConfigEntry) + ObjectNames.MOBCSLContentMessagesResponseFullName, response, request.TransactionId, new TimeSpan(1, 30, 0)).ConfigureAwait(false);
            }

            _logger.LogInformation("GetSDLContentByGroupName Response{response} {sessionId}", response.Messages, guid);

            return response.Messages;
        }

        private List<MOBMobileCMSContentMessages> AssignEmailMessageForFFCRefund(List<CMSContentMessage> lstMessages, List<MOBSHOPPrice> prices, string email, MOBFOPTravelFutureFlightCredit futureFlightCredit, MOBApplication application)
        {
            List<MOBMobileCMSContentMessages> ffcHeaderMessage = null;
            if (prices.Exists(p => p.DisplayType.ToUpper() == "REFUNDPRICE"))
            {
                ffcHeaderMessage = GetSDLMessageFromList(lstMessages, "RTI.FutureFlightCredits.EmailConfirmation");
                if (ConfigUtility.IncludeMOBILE12570ResidualFix(application.Id, application.Version.Major))
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

        private async Task<(bool,string)> GetAndValidateStateCode_CFOP(MOBPersistFormofPaymentRequest request, Session session)
        {
            bool validStateCode = false;
            string stateCode = string.Empty;
            #region
            string path = string.Format("/StatesFilter?State={0}&CountryCode={1}&Language={2}", request.FormofPaymentDetails.BillingAddress.State.Code, request.FormofPaymentDetails.BillingAddress.Country.Code, request.LanguageCode);
            _logger.LogInformation("GetAndValidateStateCode - Request Url {Request Url}", path);
            var response = await _referencedataService.GetDataGetAsync<List<StateProvince>>(path, session.Token, session.SessionId).ConfigureAwait(false);
            _logger.LogInformation("GetAndValidateStateCode - Response  {response}", response);
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
                if (_configuration.GetValue<string>("ReturnActualExceptionMessageBackForTesting") != null && _configuration.GetValue<bool>("ReturnActualExceptionMessageBackForTesting"))
                {
                    exceptionMessage = exceptionMessage + " - due to jsonResponse is empty at DAL  GetCommonUsedDataList()";
                }
                throw new MOBUnitedException(exceptionMessage);
            }
            #endregion
            return (validStateCode,stateCode);
        }
        private async Task<TravelCreditsResponse> GetTravelCreditsResponse(TravelCreditsRequest request, Session session, string path)
        {
            TravelCreditsResponse response = new TravelCreditsResponse();
            string jsonResponse;
            if (request != null)
            {
                jsonResponse = await _shoppingCartService.ShoppingCartServiceCall(session.Token, path, JsonConvert.SerializeObject(request), session.SessionId);
            }
            else
            {
                jsonResponse = await _shoppingCartService.DeletePayment(session.Token, path, session.SessionId);
            }
            if (!string.IsNullOrEmpty(jsonResponse))
            {
                response = JsonConvert.DeserializeObject<TravelCreditsResponse>(jsonResponse);
            }
            return response;
        }
        private async Task<List<MOBSHOPPrice>> GetPrices(List<United.Services.FlightShopping.Common.DisplayCart.DisplayPrice> prices, bool isAwardBooking, string sessionId, bool isReshopChange = false, string searchType = null, bool isFareLockViewRes = false, bool isCorporateFare = false, int appId = 0, string appVersion = "", FlightReservationResponse shopBookingDetailsResponse = null, Session session = null)
        {
            List<MOBSHOPPrice> bookingPrices = new List<MOBSHOPPrice>();
            CultureInfo ci = null;
            var isEnableOmniCartMVP2Changes = _configuration.GetValue<bool>("EnableOmniCartMVP2Changes");
            foreach (var price in prices)
            {
                if (ci == null)
                {
                    ci = _shoppingUtility.GetCultureInfo(price.Currency);
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
                            FormattedPromoDisplayValue = "-" + TopHelper.FormatAmountForDisplay(promoValue.ToString(), ci, false)//promoValue.ToString("C2", CultureInfo.CurrentCulture)
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
                        if (!_shoppingUtility.EnableYADesc(isReshopChange) || (price.PricingPaxType != null && !price.PricingPaxType.Equals("UAY")))
                        {
                            desc = _shoppingUtility.GetFareDescription(price);
                        }
                    }
                    if (_shoppingUtility.EnableYADesc(isReshopChange) && !string.IsNullOrEmpty(price.PricingPaxType) && price.PricingPaxType.ToUpper().Equals("UAY"))
                    {
                        bookingPrice.PriceTypeDescription = _shoppingUtility.BuildYAPriceTypeDescription(searchType);
                        bookingPrice.PaxTypeDescription = $"{price?.Count} {"young adult (18-23)"}".ToLower();
                    }
                    else
                    if (price.Description?.ToUpper().Contains("TOTAL") == true)
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
                            bookingPrice.StrikeThroughDisplayValue = UtilityHelper.FormatAwardAmountForDisplay(price.StrikeThroughPricing.ToString(), true);
                            bookingPrice.StrikeThroughDescription = _shoppingUtility.BuildStrikeThroughDescription();
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
                        await ValidateAwardMileageBalance(sessionId, price.Amount);
                    }
                }

                double tempDouble = 0;
                double.TryParse(NonDiscountTravelPrice > 0 ? NonDiscountTravelPrice.ToString() : price.Amount.ToString(), out tempDouble);
                bookingPrice.Value = Math.Round(tempDouble, 2, MidpointRounding.AwayFromZero);

                if (price.Currency.ToUpper() == "MIL")
                {
                    bookingPrice.FormattedDisplayValue = UtilityHelper.FormatAwardAmountForDisplay(price.Amount.ToString(), false);
                }
                else
                {
                    bookingPrice.FormattedDisplayValue = TopHelper.FormatAmountForDisplay(NonDiscountTravelPrice > 0 ? NonDiscountTravelPrice : price.Amount, ci, false);

                    if (_configuration.GetValue<bool>("EnableMilesPlusMoney") && string.Equals("MILESANDMONEY", price.Type, StringComparison.OrdinalIgnoreCase))
                    {
                        bookingPrice.FormattedDisplayValue = "-" + TopHelper.FormatAmountForDisplay(price.Amount, new CultureInfo("en-US"), false); // Money and Miles have to be displayed in -ve format as per MOBILE-14807
                    }
                }

                _shoppingUtility.UpdatePricesForTravelCredits(bookingPrices, price, bookingPrice, session);
                bookingPrices.Add(bookingPrice);
            }
            return bookingPrices;
        }
       
        private async Task ValidateAwardMileageBalance(string sessionId, decimal milesNeeded)
        {
            CSLShopRequest shopRequest = new CSLShopRequest();
            shopRequest = await _sessionHelperService.GetSession<CSLShopRequest>(sessionId, shopRequest.ObjectName, new List<string> { sessionId, shopRequest.ObjectName }).ConfigureAwait(false);
            if (shopRequest != null && shopRequest.ShopRequest != null && shopRequest.ShopRequest.AwardTravel && shopRequest.ShopRequest.LoyaltyPerson != null && shopRequest.ShopRequest.LoyaltyPerson.AccountBalances != null)
            {
                if (shopRequest.ShopRequest.LoyaltyPerson.AccountBalances[0] != null && shopRequest.ShopRequest.LoyaltyPerson.AccountBalances[0].Balance < milesNeeded)
                {
                    throw new MOBUnitedException(_configuration.GetValue<string>("NoEnoughMilesForAwardBooking"));
                }
            }
        }
    }
}
