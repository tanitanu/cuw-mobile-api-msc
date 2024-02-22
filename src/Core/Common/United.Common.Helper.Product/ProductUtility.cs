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
using United.Definition;
using United.Definition.FormofPayment;
using United.Definition.FormofPayment.TravelCredit;
using United.Definition.Shopping;
using United.Definition.Shopping.PriceBreakDown;
using United.Mobile.DataAccess.Common;
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
using ProfileResponse = United.Persist.Definition.Shopping.ProfileResponse;
using WorkFlowType = United.Service.Presentation.CommonEnumModel.WorkFlowType;


namespace United.Common.Helper.Product
{
    public class ProductUtility : IProductUtility
    {
        private readonly IConfiguration _configuration;
        private readonly ICacheLog<ProductUtility> _logger;
        private readonly ISessionHelperService _sessionHelperService;
        private readonly ICachingService _cachingService;

        private readonly IShoppingCartService _shoppingCartService;
        private readonly IPaymentService _paymentService;
        private readonly ICMSContentService _CMSContentService;

        private static readonly string _MSG1 = "MSG1";
        private static readonly string _ERR2 = "ERR2";
        private static readonly string _ERR3 = "ERR3";
        private static readonly string _ERR4 = "Err4";
        private static readonly string _ERR5 = "ERR5";

        public ProductUtility(IConfiguration configuration
            , ICacheLog<ProductUtility> logger
            , ISessionHelperService sessionHelperService
            , IShoppingCartService shoppingCartService
            , IPaymentService paymentService
            , ICMSContentService CMSContentService
            , ICachingService cachingService)
        {
            _configuration = configuration;
            _logger = logger;
            _sessionHelperService = sessionHelperService;
            _cachingService = cachingService;
            _shoppingCartService = shoppingCartService;
            _paymentService = paymentService;
            _CMSContentService = CMSContentService;
        }

        public bool IsBuyMilesFeatureEnabled(int appId, string version)
        {
            if (!_configuration.GetValue<bool>("EnableBuyMilesFeature")) return false;
            return GeneralHelper.IsApplicationVersionGreaterorEqual(appId, version, _configuration.GetValue<string>("Android_BuyMilesFeatureSupported_AppVersion"), _configuration.GetValue<string>("IPhone_BuyMilesFeatureSupported_AppVersion"));
        }

        public async System.Threading.Tasks.Task PreLoadTravelCredit(string sessionId, MOBShoppingCart shoppingCart, MOBRequest request, bool isLoadFromCSL = false)
        {
            try
            {
                Session session = await _sessionHelperService.GetSession<Session>(sessionId, new Session().ObjectName, new List<string> { sessionId, new Session().ObjectName }).ConfigureAwait(false);
                Reservation bookingPathReservation =new Reservation ();                
                var basicFopResult = await LoadBasicFOPResponse(session, bookingPathReservation);

                var response = basicFopResult.MobFopResponse;
                bookingPathReservation = basicFopResult.BookingPathReservation;


                if (sessionId == null)
                {
                    throw new Exception("empty session");
                }
                if (response?.ShoppingCart?.FormofPaymentDetails == null)
                {
                    response.ShoppingCart.FormofPaymentDetails = new MOBFormofPaymentDetails();
                }

                var travelCreditDetails = new MOBFOPTravelCreditDetails();

                TravelCredit travelCredit = new TravelCredit();
                List<CMSContentMessage> lstMessages = await GetSDLContentByGroupName(request, sessionId, session.Token, _configuration.GetValue<string>("CMSContentMessages_GroupName_BookingRTI_Messages"), "BookingPathRTI_CMSContentMessagesCached_StaticGUID");
                travelCreditDetails.LookUpMessages = GetSDLContentMessages(lstMessages, "RTI.TravelCertificate.LookUpTravelCredits");
                TCLookupByFreqFlyerNumWithEligibleResponse cslres = new TCLookupByFreqFlyerNumWithEligibleResponse();
                cslres = await GetCSLTravelCredits(sessionId, request, response, isLoadFromCSL);
                travelCreditDetails.ReviewMessages = GetSDLContentMessages(lstMessages, "RTI.TravelCertificate.ReviewTravelCredits");
                travelCreditDetails.ReviewMessages.AddRange(GetSDLContentMessages(lstMessages, "RTI.TravelCertificate.AlertTravelCredits"));
                SwapTitleAndLocation(travelCreditDetails.ReviewMessages);
                SwapTitleAndLocation(travelCreditDetails.LookUpMessages);
                travelCreditDetails.ReviewMessages.AddRange(travelCreditDetails.LookUpMessages);
                travelCreditDetails.TravelCredits = LoadCSLResponse(cslres, response, travelCreditDetails.LookUpMessages, sessionId);
                if (response.ShoppingCart.FormofPaymentDetails.TravelCreditDetails?.TravelCredits?.Count > 0)
                {
                    travelCreditDetails.TravelCredits.ForEach(tc => tc.IsApplied = (response.ShoppingCart.FormofPaymentDetails.TravelCreditDetails.TravelCredits.Exists(existingTC => existingTC.IsApplied && existingTC.PinCode == tc.PinCode)));
                }
                travelCreditDetails.TravelCredits = travelCreditDetails.TravelCredits.OrderBy(x => DateTime.Parse(x.ExpiryDate)).ToList();
                var nameWaiverMatchMessage = GetSDLMessageFromList(lstMessages, "RTI.TravelCertificate.LookUpTravelCredits.Alert.NameMatchWaiver");
                travelCreditDetails.NameWaiverMatchMessage = nameWaiverMatchMessage.Count() > 0 ? nameWaiverMatchMessage?[0].ContentFull.ToString() : null;

                shoppingCart.FormofPaymentDetails.TravelCreditDetails = travelCreditDetails;
            }
            catch (Exception ex)
            {
                // _logEntries.Add(United.Logger.LogEntry.GetLogEntry<string>(sessionId, "PreLoadTravelCredits", "Exception", request.Application.Id, request.Application.Version.Major, request.DeviceId, ex.Message, true, false));
                _logger.LogError("PreLoadTravelCredits Error {@Exception}", JsonConvert.SerializeObject(ex));
            }
        }

        private async Task<TCLookupByFreqFlyerNumWithEligibleResponse> GetCSLTravelCredits(string sessionId, MOBRequest mobRequest, MOBFOPResponse response, bool isLoadFromCSL, FlightReservationResponse flightReservationResponse = null)
        {
            TCLookupByFreqFlyerNumWithEligibleResponse lookupResponse = new TCLookupByFreqFlyerNumWithEligibleResponse();
            string url = "/ECD/TCLookupByFreqFlyerNumWithEligible";
            TCLookupByFreqFlyerNumWithEligibleRequest cslRequest = new TCLookupByFreqFlyerNumWithEligibleRequest();
            cslRequest.FreqFlyerNum = response?.Profiles?[0].Travelers.Find(item => item.IsProfileOwner).MileagePlus.MileagePlusId;
            if (String.IsNullOrEmpty(cslRequest.FreqFlyerNum))
                cslRequest.FreqFlyerNum = response.Reservation?.TravelersCSL?.FirstOrDefault(v => v.IsProfileOwner)?.MileagePlus?.MileagePlusId;
            // In the guest flow we will be able to show the travel credits based on the lastname/DOB 
            if (!_configuration.GetValue<bool>("EnablePreLoadForTCNonMember"))
            {
                if (String.IsNullOrEmpty(cslRequest.FreqFlyerNum))
                {
                    return lookupResponse;
                }
            }
            else
            {
                cslRequest.IsLoadFFCRWithCustomSearch = true;
                cslRequest.IsLoadFFCWithCustomSearch = true;
            }

            cslRequest.IsLoadETC = true;
            cslRequest.IsLoadFFC = true;
            cslRequest.IsLoadFFCR = true;

            Session session = await _sessionHelperService.GetSession<Session>(sessionId, new Session().ObjectName, new List<string> { sessionId, new Session().ObjectName }).ConfigureAwait(false);
            var reservation = new Service.Presentation.ReservationModel.Reservation();
            reservation = await _sessionHelperService.GetSession<Service.Presentation.ReservationModel.Reservation>(sessionId, reservation.GetType().FullName, new List<string> { sessionId, reservation.GetType().FullName }).ConfigureAwait(false);
            if (reservation == null || (!_configuration.GetValue<bool>("DisableMMOptionsReloadInBackButtonFixToggle") && isLoadFromCSL))
            {
                if (flightReservationResponse == null)
                {
                    var cartInfo = await GetCartInformation(sessionId, mobRequest.Application, mobRequest.DeviceId, session.CartId, session.Token);
                    reservation = cartInfo.Reservation;
                }
                else
                {
                    reservation = flightReservationResponse.Reservation;
                }

               await  _sessionHelperService.SaveSession<Service.Presentation.ReservationModel.Reservation>(reservation, session.SessionId, new List<string> { session.SessionId, reservation.GetType().FullName }, reservation.GetType().FullName).ConfigureAwait(false);
            }
            cslRequest.Reservation = reservation;
            cslRequest.CallingService = new ServiceClient();
            cslRequest.CallingService.Requestor = new Requestor();
            cslRequest.CallingService.AccessCode = _configuration.GetValue<string>("TravelCreditAccessCode").ToString();
            cslRequest.CallingService.Requestor.AgentAAA = "HQS";
            cslRequest.CallingService.Requestor.AgentSine = "UA";
            cslRequest.CallingService.Requestor.ApplicationSource = "Mobile";
            cslRequest.CallingService.Requestor.Device = new Service.Presentation.CommonModel.Device();
            cslRequest.CallingService.Requestor.Device.LNIATA = "Mobile";
            cslRequest.CallingService.Requestor.DutyCode = "SU";
            string jsonRequest = JsonConvert.SerializeObject(cslRequest);
            string jsonResponse = await PostAndLog(sessionId, url, jsonRequest, mobRequest, "GetCSLTravelCredits", "TCLookupByFreqFlyerNumWithEligible");

            lookupResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<TCLookupByFreqFlyerNumWithEligibleResponse>(jsonResponse);
            await _sessionHelperService.SaveSession(lookupResponse, session.SessionId, new List<string> { session.SessionId, lookupResponse.GetType().FullName }, lookupResponse.GetType().FullName).ConfigureAwait(false);

            return lookupResponse;
        }

        private async Task <LoadReservationAndDisplayCartResponse> GetCartInformation(string sessionId, MOBApplication application, string device, string cartId, string token, WorkFlowType workFlowType = WorkFlowType.InitialBooking)
        {
            LoadReservationAndDisplayCartRequest loadReservationAndDisplayCartRequest = new LoadReservationAndDisplayCartRequest();
            LoadReservationAndDisplayCartResponse loadReservationAndDisplayResponse = new LoadReservationAndDisplayCartResponse();
            loadReservationAndDisplayCartRequest.CartId = cartId;
            loadReservationAndDisplayCartRequest.WorkFlowType = (Services.FlightShopping.Common.FlightReservation.WorkFlowType)workFlowType;
            string jsonRequest = JsonConvert.SerializeObject(loadReservationAndDisplayCartRequest);
            loadReservationAndDisplayResponse = await _shoppingCartService.GetCartInformation<LoadReservationAndDisplayCartResponse>(token, "LoadReservationAndDisplayCart", jsonRequest, sessionId);
            return loadReservationAndDisplayResponse;
        }

        private async Task<string> PostAndLog(string sessionId, string path, string jsonRequest, MOBRequest mobRequest, string logAction, string cslAction)
        {
            cslAction = cslAction ?? string.Empty;
            logAction = logAction ?? string.Empty;
            logAction = cslAction + " - " + logAction;
            var session = new Session();
            session = await _sessionHelperService.GetSession<Session>(sessionId, session.ObjectName, new List<string> { sessionId, session.ObjectName }).ConfigureAwait(false);
            var token = session.Token;

            var jsonResponse = await _paymentService.GetEligibleFormOfPayments(token, path, jsonRequest, sessionId);

            if (string.IsNullOrEmpty(jsonResponse))
                throw new Exception("Service did not return any reponse");

            return jsonResponse;
        }

        public List<MOBFOPTravelCredit> LoadCSLResponse
           (TCLookupByFreqFlyerNumWithEligibleResponse cslResponse, MOBFOPResponse response, List<MOBMobileCMSContentMessages> lookUpMessages,string sessionId)
        {
            List<MOBFOPTravelCredit> travelCredits = new List<MOBFOPTravelCredit>();

            if (cslResponse == null)
            {
                return travelCredits;
            }
            var etc = cslResponse.ETCCertificates;
            var ffc = cslResponse.FFCCertificates;
            var ffcr = cslResponse.FFCRCertificates;
            //ETC
            if (etc?.CertificateList != null)
            {
                foreach (var item in etc?.CertificateList)
                {
                    travelCredits.Add(new MOBFOPTravelCredit()
                    {
                        IsLookupCredit = false,
                        CreditAmount = $"${item.InitialValue}",
                        CurrentValue = Convert.ToDouble(item.CurrentValue),
                        DisplayNewValueAfterRedeem = item.CurrentValue,
                        DisplayRedeemAmount = "0.00",
                        ExpiryDate = Convert.ToDateTime(item.CertExpDate).ToString("MMMM dd, yyyy"),
                        InitialValue = Convert.ToDouble(item.InitialValue),
                        IsApplied = false,
                        IsRemove = false,
                        IsEligibleToRedeem = true,
                        NewValueAfterRedeem = Convert.ToDouble(item.CurrentValue),
                        PinCode = item.CertPin,
                        PromoCode = item.PromoID,
                        RecordLocator = item.PNR,
                        RedeemAmount = 0,
                        TravelCreditType = (MOBTravelCreditFOP)MOBTravelCreditFOP.ETC,
                        YearIssued = Convert.ToDateTime(item.OrigIssueDate).ToString("MMMM dd, yyyy"),
                        Recipient = $"{item.FirstName} {item.LastName}",
                        IsHideShowDetails = false,
                        LastName = _configuration.GetValue<bool>("EnableMFOP") ? item.LastName : string.Empty,
                        FirstName = _configuration.GetValue<bool>("EnableMFOP") ? item.FirstName : string.Empty
                    });

                }
            }


            AddFFCandFFCR(response.Reservation.TravelersCSL, travelCredits, ffc, lookUpMessages, true, false,sessionId);
            if (ffcr != null)
                AddFFCandFFCR(response.Reservation.TravelersCSL, travelCredits, ffcr, lookUpMessages, false, false,sessionId);
            return travelCredits;
        }

        private void AddFFCandFFCR(List<MOBCPTraveler> travelers, List<MOBFOPTravelCredit> travelCredits,
            FFCRCertificates ffcr, List<MOBMobileCMSContentMessages> lookUpMessages, bool isOtfFFC, bool islookUp,string sessionId)
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
                            var item1 = ConvertToFFC(travelCredit, travelers, lookUpMessages, isOtfFFC, item.FirstName, item.LastName, ffcr.Errors, islookUp,sessionId);
                            if (item1 != null)
                            {
                                if (_configuration.GetValue<bool>("Disable_Ignore_FFCR_PNR_CaseCheck"))
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

        private MOBFOPTravelCredit ConvertToFFC(Service.Presentation.PaymentModel.TravelCredit cslTravelCredit, List<MOBCPTraveler> travelers, 
            List<MOBMobileCMSContentMessages> lookUpMessages, bool isOtfFFC, string firstName, string lastName, Collection<TCError> errors,
            bool islookUp, string sessionId)
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
                travelCredit.TravelCreditType = (MOBTravelCreditFOP)MOBTravelCreditFOP.FFC;
                travelCredit.YearIssued = Convert.ToDateTime(origIssueDate).ToString("MMMM dd, yyyy");
                travelCredit.Recipient = $"{firstName} {lastName}";
                travelCredit.FirstName = firstName;
                travelCredit.LastName = lastName;
                travelCredit.IsOTFFFC = isOtfFFC;
                travelCredit.IsNameMatch = cslTravelCredit.Travellers.Exists(tcTraveler => Convert.ToBoolean(tcTraveler.IsNameMatch));
                travelCredit.IsNameMatchWaiverApplied = cslTravelCredit.Travellers.Exists(tcTraveler => Convert.ToBoolean(Convert.ToBoolean(tcTraveler.IsNameMatchWaiverApplied))
                                                                                                        && !Convert.ToBoolean(tcTraveler.IsNameMatch));
                if (_configuration.GetSection("EnableU4BCorporateBookingFFC").GetValue<bool>("isEnable"))
                {
                    travelCredit.IsCorporateTravelCreditText = String.Equals(cslTravelCredit.IsCorporateBooking, "true", StringComparison.OrdinalIgnoreCase) ? _configuration.GetSection("EnableU4BCorporateBookingFFC").GetValue<string>("u4BCorporateText") : String.Empty;
                }
                travelCredit.IsTravelDateBeginsBeforeCertExpiry = cslTravelCredit.Travellers.Exists(tcTraveler => Convert.ToBoolean(tcTraveler.IsTravelDateBeginsBeforeCertExpiry));
                travelCredit.IsEligibleToRedeem = cslTravelCredit.Travellers.Exists(tcTraveler => Convert.ToBoolean(tcTraveler.IsEligibleToRedeem));

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
                if (_configuration.GetValue<bool>("DisableFireAndForgetTravelCreditCallInGetProfile"))
                {
                    travelCredit.IsHideShowDetails = cslTravelCredit.IsTravelerCustomDataUsedToLookupCert.ToBoolean();
                }
                else
                {
                    travelCredit.IsHideShowDetails = false;
                }

                if (_configuration.GetValue<bool>("EnableMFOP"))
                {
                    travelCredit.CertificateNumber = cslTravelCredit.CertificateNumber;
                    travelCredit.CsltravelCreditType = cslTravelCredit.TravelCreditType;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError("ConvertToFFC Exception:{exceprion} with sessionId:{sessionId}", JsonConvert.SerializeObject(ex), sessionId);
            }

            return travelCredit;
        }

        private List<MOBTypeOption> SetErrorMessage2(List<MOBMobileCMSContentMessages> lookUpMessages)
        {
            var errordata = lookUpMessages?.FirstOrDefault
                   (x => string.Equals(x.LocationCode, "RTI.TravelCertificate.LookUpTravelCredits.ErrorMessage2", StringComparison.OrdinalIgnoreCase));

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

        private List<MOBTypeOption> TravelCreditErrorMapping
            (Collection<TCError> errors, List<MOBMobileCMSContentMessages> lookUpMessages,
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
                    (x => string.Equals(x.LocationCode, "RTI.TravelCertificate.LookUpTravelCredits.ErrorMessage1", StringComparison.OrdinalIgnoreCase));

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

        private async Task<List<MOBCPProfile>> LoadPersistedProfile(string sessionId, string flow)
        {
            switch (flow)
            {
                case "BOOKING":   //profile Object load
                    ProfileResponse persistedProfileResponse = new ProfileResponse();
                    persistedProfileResponse = await _sessionHelperService.GetSession<ProfileResponse>(sessionId, persistedProfileResponse.ObjectName, new List<string> { sessionId, persistedProfileResponse.ObjectName }).ConfigureAwait(false);

                    return persistedProfileResponse != null ? persistedProfileResponse.Response.Profiles : null;
                case "VIEWRES":
                    ProfileFOPCreditCardResponse profilePersist = new ProfileFOPCreditCardResponse();
                    profilePersist = await _sessionHelperService.GetSession<ProfileFOPCreditCardResponse>(sessionId, profilePersist.ObjectName, new List<string> { sessionId, profilePersist.ObjectName }).ConfigureAwait(false);

                    return profilePersist != null ? profilePersist.Response.Profiles : null;

                default: return null;
            }
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

        public List<MOBMobileCMSContentMessages> GetSDLContentMessages(List<CMSContentMessage> lstMessages, string title)
        {
            List<MOBMobileCMSContentMessages> messages = new List<MOBMobileCMSContentMessages>();
            messages.AddRange(GetSDLMessageFromList(lstMessages, title));

            return messages;
        }
        public async Task<(MOBFOPResponse MobFopResponse,Reservation BookingPathReservation)> LoadBasicFOPResponse(Session session, Reservation bookingPathReservation)
        {
            var response = new MOBFOPResponse();
            bookingPathReservation = new Reservation();

            bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(session.SessionId, bookingPathReservation.ObjectName, new List<string> { session.SessionId, bookingPathReservation.ObjectName }).ConfigureAwait(false);

            response.Reservation = new MOBSHOPReservation(_configuration, _cachingService);
            response.Reservation = MakeReservationFromPersistReservation(response.Reservation, bookingPathReservation, session);

            var persistShoppingCart = new MOBShoppingCart();

            persistShoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(session.SessionId, persistShoppingCart.ObjectName, new List<string> { session.SessionId, persistShoppingCart.ObjectName }).ConfigureAwait(false);
            response.ShoppingCart = persistShoppingCart;
            response.Profiles = await LoadPersistedProfile(session.SessionId, response.ShoppingCart.Flow);
            if (response?.ShoppingCart?.FormofPaymentDetails == null)
            {
                response.ShoppingCart.FormofPaymentDetails = new MOBFormofPaymentDetails();
            }

            return (response, bookingPathReservation);
        }

        public async Task<MOBFOPTravelBankDetails> PopulateTravelBankData(Session session, MOBSHOPReservation reservation, MOBRequest request)
        {
            MOBFOPTravelBankDetails travelBankDetails = null;
            if (reservation != null && reservation.IsSignedInWithMP)
            {
                double travelBankBalance = await GetTravelBankBalance(session.SessionId);
                if (travelBankBalance > 0)
                {
                    MOBCPTraveler mobCPTraveler = await GetProfileOwnerTravelerCSL(session.SessionId);
                    travelBankDetails = new MOBFOPTravelBankDetails
                    {
                        ApplyTBContentMessage = await GetTBContentMessages(session, request),
                        TBBalance = travelBankBalance,
                        DisplayTBBalance = (travelBankBalance).ToString("C2", CultureInfo.CurrentCulture),
                        TBApplied = 0,
                        DisplaytbApplied = "$0.00",
                        RemainingBalance = travelBankBalance,
                        DisplayRemainingBalance = (travelBankBalance).ToString("C2", CultureInfo.CurrentCulture),
                        DisplayAvailableBalanceAsOfDate = $"{"Balance as of "}{ DateTime.Now.ToString("MM/dd/yyyy") }",
                        PayorFirstName = mobCPTraveler.FirstName,
                        PayorLastName = mobCPTraveler.LastName,
                        MPNumber = mobCPTraveler.MileagePlus.MileagePlusId
                        //TravelBanks = new List<MOBFOPTravelBank> { }
                    };
                }
            }
            return travelBankDetails;
        }

        private async Task<double> GetTravelBankBalance(string sessionId)
        {
            MOBCPTraveler mobCPTraveler = await GetProfileOwnerTravelerCSL(sessionId);
            return mobCPTraveler?.MileagePlus?.TravelBankBalance > 0.00 ? mobCPTraveler.MileagePlus.TravelBankBalance : 0.00;
        }

        public async Task<MOBCPTraveler> GetProfileOwnerTravelerCSL(string sessionID)
        {
            ProfileResponse profilePersist = new ProfileResponse();
            profilePersist = await GetCSLProfileResponseInSession(sessionID);
            if (profilePersist != null && profilePersist.Response != null && profilePersist.Response.Profiles != null)
            {
                foreach (MOBCPTraveler mobCPTraveler in profilePersist.Response.Profiles[0].Travelers)
                {
                    if (mobCPTraveler.IsProfileOwner)
                    {
                        return mobCPTraveler;
                    }
                }
            }
            return null;
        }


        private async Task<ProfileResponse> GetCSLProfileResponseInSession(string sessionId)
        {
            ProfileResponse profile = new ProfileResponse();
            try
            {
                profile = await _sessionHelperService.GetSession<ProfileResponse>(sessionId, profile.ObjectName, new List<string> { sessionId, profile.ObjectName }).ConfigureAwait(false);
            }
            catch (System.Exception)
            {

            }
            return profile;
        }

        private async Task<List<MOBMobileCMSContentMessages>> GetTBContentMessages(Session session, MOBRequest request)
        {
            List<CMSContentMessage> lstMessages = await GetSDLContentByGroupName(request, session.SessionId, session.Token, _configuration.GetValue<string>("CMSContentMessages_GroupName_BookingRTI_Messages"), "BookingPathRTI_CMSContentMessagesCached_StaticGUID");
            return SwapTitleAndLocation(GetSDLContentMessages(lstMessages, "RTI.TravelBank.Apply"));
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

        public async Task<List<CMSContentMessage>> GetSDLContentByGroupName(MOBRequest request, string guid, string token, string groupName, string docNameConfigEntry, bool useCache = false)
        {
            MOBCSLContentMessagesResponse response = null;
            try
            {
                var getSDL = await _cachingService.GetCache<string>(_configuration.GetValue<string>(docNameConfigEntry) + ObjectNames.MOBCSLContentMessagesResponseFullName, request.TransactionId).ConfigureAwait(false);

                if (!response.IsNullOrEmpty())
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

            response = await _CMSContentService.GetSDLContentByGroupName<MOBCSLContentMessagesResponse>(token, "message", jsonRequest, guid);
            if (response == null)
            {
                _logger.LogInformation("GetSDLContentByGroupName Failed to deserialize CSL response SessionId:{sessionId}", guid);

                return null;
            }

            if (response.Errors?.Count > 0)
            {
                string errorMsg = String.Join(" ", response.Errors.Select(x => x.Message));

                _logger.LogError("GetSDLContentByGroupName CSL-CallError{CSL-CallError} SessionId:{sessionId}", errorMsg, guid);

                return null;
            }

            if (response != null && (Convert.ToBoolean(response.Status) && response.Messages != null))
            {
                if (!_configuration.GetValue<bool>("DisableSDLEmptyTitleFix"))
                {
                    response.Messages = response.Messages.Where(l => l.Title != null)?.ToList();
                }
                var saveSDL = await _cachingService.SaveCache<MOBCSLContentMessagesResponse>(_configuration.GetValue<string>(docNameConfigEntry) + ObjectNames.MOBCSLContentMessagesResponseFullName, response, request.TransactionId, new TimeSpan(1, 30, 0));

            }

            _logger.LogInformation("GetSDLContentByGroupName responseMessage {responseMessage} sessionID{sessionId}", response.Messages, guid);

            return response.Messages;
        }

        public async Task<MOBSHOPReservation> GetReservationFromPersist(MOBSHOPReservation reservation, Session session)
        {
            #region
            Reservation bookingPathReservation = new Reservation();
            bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(session.SessionId, bookingPathReservation.ObjectName, new List<string> { session.SessionId, bookingPathReservation.ObjectName }).ConfigureAwait(false);
            await reservation.Initialise(_configuration, _cachingService);
            return MakeReservationFromPersistReservation(reservation, bookingPathReservation, session);

            #endregion
        }

        public MOBSHOPReservation MakeReservationFromPersistReservation(MOBSHOPReservation reservation, Reservation bookingPathReservation,
           Session session)
        {
            if (reservation == null)
            {
                reservation = new MOBSHOPReservation(_configuration, _cachingService);
            }
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
           // reservation.IsCubaTravel = bookingPathReservation.IsCubaTravel;

            if (bookingPathReservation.TravelersCSL != null && bookingPathReservation.TravelerKeys != null)
            {
                List<MOBCPTraveler> lstTravelers = new List<MOBCPTraveler>();
                foreach (string travelerKey in bookingPathReservation.TravelerKeys)
                {
                    lstTravelers.Add(bookingPathReservation.TravelersCSL[travelerKey]);
                }
                reservation.TravelersCSL = lstTravelers;

                //if (session.IsReshopChange)
                //{
                //    if (reservation.IsCubaTravel)
                //    {
                //        reservation.TravelersCSL.ForEach(x => { x.PaxID = x.PaxIndex + 1; x.IsPaxSelected = true; });
                //    }
                //    else
                //    {
                //        reservation.TravelersCSL.ForEach(x => { x.Message = string.Empty; x.CubaTravelReason = null; });
                //    }
                //    bookingPathReservation.ShopReservationInfo2.AllEligibleTravelersCSL = reservation.TravelersCSL;
                //}
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
            //reservation.IsReshopCommonFOPEnabled = bookingPathReservation.IsReshopCommonFOPEnabled;


            if (bookingPathReservation.TCDAdvisoryMessages != null && bookingPathReservation.TCDAdvisoryMessages.Count > 0)
            {
                reservation.TCDAdvisoryMessages = bookingPathReservation.TCDAdvisoryMessages;
            }
            //##Price Break Down - Kirti
            if (_configuration.GetValue<string>("EnableShopPriceBreakDown") != null &&
                Convert.ToBoolean(_configuration.GetValue<string>("EnableShopPriceBreakDown")))
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
                reservation.ShopReservationInfo2.AllowAdditionalTravelers = !session.IsCorporateBooking;
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
            SetEligibilityforETCTravelCredit(reservation, session, bookingPathReservation);
            return reservation;
        }

        public MOBSHOPTripPriceBreakDown GetPriceBreakDown(Reservation reservation)
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

                    //build travel options
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
        private void SetEligibilityforETCTravelCredit(MOBSHOPReservation reservation, Session session, Reservation bookingPathReservation)
        {
            if (IsEnableETCCreditsInBookingFeature(session?.CatalogItems))
            {
                reservation.EligibleForETCPricingType = false;
                if (session.PricingType == Mobile.Model.MSC.FormofPayment.PricingType.ETC.ToString())
                {
                    reservation.EligibleForETCPricingType = true;
                    reservation.PricingType = session.PricingType;
                }
                else
                {
                    if (bookingPathReservation.FormOfPaymentType == MOBFormofPayment.ETC)
                    {
                        reservation.EligibleForETCPricingType = true;
                    }
                    reservation.PricingType = session.PricingType;
                }
            }
        }
        private bool IsEnableETCCreditsInBookingFeature(List<MOBItem> catalogItems = null)
        {
            return _configuration.GetValue<bool>("EnableFSRETCCreditsFeature") && (catalogItems != null && catalogItems.Count > 0 &&
        catalogItems.FirstOrDefault(a => a.Id == ((int)IOSCatalogEnum.EnableETCCreditsInBooking).ToString() || a.Id == ((int)AndroidCatalogEnum.EnableETCCreditsInBooking).ToString())?.CurrentValue == "1");
        }
    }
}
