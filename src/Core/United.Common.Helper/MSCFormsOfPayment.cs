using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using United.Definition;
using United.Definition.Booking;
using United.Definition.FormofPayment;
using United.Definition.Shopping;
using United.Definition.Uplift;
using United.Json.Serialization;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.DynamoDB;
using United.Mobile.DataAccess.Product.Interfaces;
using United.Mobile.Model.Common;
using United.Mobile.Model.MSC.FormofPayment;
using United.Persist.Definition.SeatChange;
using United.Persist.Definition.Shopping;
using United.Service.Presentation.CommonModel;
using United.Service.Presentation.InteractionModel;
using United.Service.Presentation.ProductModel;
using United.Service.Presentation.ReservationResponseModel;
using United.Service.Presentation.SegmentModel;
using United.Services.FlightShopping.Common.DisplayCart;
using United.Services.FlightShopping.Common.Extensions;
using United.Services.FlightShopping.Common.FlightReservation;
using United.Utility.AppVersion;
using United.Utility.Helper;
using static System.Net.Mime.MediaTypeNames;
using Genre = United.Service.Presentation.CommonModel.Genre;
using ProfileResponse = United.Persist.Definition.Shopping.ProfileResponse;
using Reservation = United.Persist.Definition.Shopping.Reservation;

namespace United.Common.Helper
{
    public class MSCFormsOfPayment : IMSCFormsOfPayment
    {
        private readonly ICacheLog<MSCFormsOfPayment> _logger;
        private readonly ICachingService _cachingService;
        private readonly IConfiguration _configuration;
        private readonly IHeaders _headers;
        private readonly ISessionHelperService _sessionHelperService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IPaymentService _paymentService;
        private readonly IDynamoDBService _dynamoDBService;
        private readonly IShoppingUtility _shoppingUtility;
        private readonly IShoppingCartUtility _shoppingCartUtility;
        private readonly IFeatureSettings _featureSettings;

        public MSCFormsOfPayment(ICacheLog<MSCFormsOfPayment> logger
            , IConfiguration configuration
            , ICachingService cachingService
            , IHeaders headers
            , ISessionHelperService sessionHelperService
            , IShoppingCartService shoppingCartService
            , IPaymentService paymentService
            , IDynamoDBService dynamoDBService
            , IShoppingUtility shoppingUtility
            , IShoppingCartUtility shoppingCartUtility
            , IFeatureSettings featureSettings)
        {
            _logger = logger;
            _sessionHelperService = sessionHelperService;
            _configuration = configuration;
            _cachingService = cachingService;
            _headers = headers;
            _shoppingCartService = shoppingCartService;
            _paymentService = paymentService;
            _dynamoDBService = dynamoDBService;
            _shoppingUtility = shoppingUtility;
            _shoppingCartUtility = shoppingCartUtility;
            _featureSettings = featureSettings;
            ConfigUtility.UtilityInitialize(_configuration);
        }

        public async Task<(List<FormofPaymentOption> EligibleFormofPayments, bool isDefault)> GetEligibleFormofPayments(MOBRequest request, Session session, MOBShoppingCart shoppingCart, string cartId, string flow, MOBSHOPReservation reservation = null, bool IsMilesFOPEnabled = false, SeatChangeState persistedState = null)
        {
            List<FormofPaymentOption> response = new List<FormofPaymentOption>();
            bool isDefault = false;
            SeatChangeState state = persistedState;
            if (!_configuration.GetValue<bool>("EnableFlowCheck") && flow == null)
                flow = string.Empty;
            var flowTemp = flow;
            flow = flow.Equals("MOBILECHECKOUT") ? "VIEWRES" : flow;
            if (state == null && (flow == FlowType.VIEWRES.ToString() || _configuration.GetValue<bool>("IsEnableManageResCoupon") && flow == FlowType.VIEWRES_SEATMAP.ToString()) && ConfigUtility.IsManageResETCEnabled(request.Application.Id, request.Application.Version.Major))
            {
                state = await _sessionHelperService.GetSession<SeatChangeState>(session.SessionId, "United.Persist.Definition.SeatChange.SeatChangeState", new List<string> { session.SessionId, "United.Persist.Definition.SeatChange.SeatChangeState" }).ConfigureAwait(false);
            }
            if (_configuration.GetValue<bool>("EnableTravelOptionsInViewRes") && flow == FlowType.VIEWRES_BUNDLES_SEATMAP.ToString())
            {
                flow = FlowType.VIEWRES.ToString();
            }
            if (_configuration.GetValue<bool>("GetFoPOptionsAlongRegisterOffers") && shoppingCart.Products != null && shoppingCart.Products.Count > 0)
            {
                MOBFOPEligibilityRequest eligiblefopRequest = new MOBFOPEligibilityRequest()
                {
                    TransactionId = request.TransactionId,
                    DeviceId = request.DeviceId,
                    AccessCode = request.AccessCode,
                    LanguageCode = request.LanguageCode,
                    Application = request.Application,
                    CartId = cartId,
                    SessionId = session.SessionId,
                    Flow = flow,
                    Products = GetProductsForEligibleFopRequest(shoppingCart, state)
                };
                if (shoppingCart.FormofPaymentDetails?.MilesFOP?.IsMilesFOPEligible != null)
                {
                    IsMilesFOPEnabled = shoppingCart.FormofPaymentDetails.MilesFOP.IsMilesFOPEligible;
                }
                var tupleRespEligibleFormofPayments = await EligibleFormOfPayments(eligiblefopRequest, session, IsMilesFOPEnabled);
                response = tupleRespEligibleFormofPayments.EligibleFormofPayments;
                isDefault = tupleRespEligibleFormofPayments.isDefault;
                if ((reservation?.IsMetaSearch ?? false) && _configuration.GetValue<bool>("CreditCardFOPOnly_MetaSearch"))
                {
                    if (!_configuration.GetValue<bool>("EnableETCFopforMetaSearch"))
                    {
                        response = response.Where(x => x.Category == "CC").ToList();
                    }
                    else
                    {
                        response = response.Where(x => x.Category == "CC" || x.Category == "CERT").ToList();
                    }

                }
                var upliftFop = UpliftAsFormOfPayment(reservation, shoppingCart);
                if (upliftFop != null && response != null)
                {
                    response.Add(upliftFop);
                }

                // Added as part of Money + Miles changes: For MM user have to pay money only using CC - MOBILE-14735 -- MOBILE-14833 // MM is only for Booking
                if (IncludeMoneyPlusMiles(request.Application.Id, request.Application.Version.Major) && flow == FlowType.BOOKING.ToString())
                {
                    if (response.Exists(x => x.Category == "MILES") && !_configuration.GetValue<bool>("DisableMMFixForSemiLogin_MOBILE17070")
                        && !reservation.IsSignedInWithMP)
                    {
                        response = response.Where(x => x.Category != "MILES").ToList();
                    }
                    if (shoppingCart.FormofPaymentDetails?.MoneyPlusMilesCredit?.SelectedMoneyPlusMiles != null) // selected miles will not be empty only after Applied Miles
                    {
                        response = response.Where(x => x.Category == "CC").ToList();
                    }
                }

                response = TravelBankEFOPFilter(request, shoppingCart?.FormofPaymentDetails?.TravelBankDetails, flow, response);


                bool isPoolMileesEnabled = (await _featureSettings.GetFeatureSettingValue("EnableFSRMilesPoolingFeature").ConfigureAwait(false)
                     && GeneralHelper.IsApplicationVersionGreaterorEqual(request.Application.Id, request.Application.Version.Major,
                     _configuration.GetValue<string>("Android_EnableFSRMilesPoolingFeature_AppVersion"),
                    _configuration.GetValue<string>("iPhone_EnableFSRMilesPoolingFeature_AppVersion"))
                     && reservation.ShopReservationInfo2.AwardRedemptionDetail != null
                     && reservation.ShopReservationInfo2.AwardRedemptionDetail.CanRedeemPoolBalance == true);





                //Added a part of enabiling ETC changes
                if (ConfigUtility.IsETCchangesEnabled(request.Application.Id, request.Application.Version.Major) && flow == FlowType.BOOKING.ToString())
                    response = BuildEligibleFormofPaymentsResponse(response, shoppingCart, session, request, reservation?.IsMetaSearch ?? false, isPoolMileesEnabled);
                else if ((flow == FlowType.VIEWRES.ToString() || _configuration.GetValue<bool>("IsEnableManageResCoupon") && flow == FlowType.VIEWRES_SEATMAP.ToString()) && ConfigUtility.IsManageResETCEnabled(request.Application.Id, request.Application.Version.Major))
                    response = BuildEligibleFormofPaymentsResponse(response, shoppingCart, request);
                if (flowTemp.Equals("MOBILECHECKOUT"))
                {
                    response = response.Where(x => x.Category == "CC").ToList();
                }
                if (IsInternationalBillingAddress_CheckinFlowEnabled(request.Application.Id, request.Application.Version.Major)
                    && shoppingCart.FormofPaymentDetails?.InternationalBilling?.BillingAddressProperties == null && flow?.ToLower() == FlowType.CHECKIN.ToString().ToLower())
                {
                    if (shoppingCart.FormofPaymentDetails == null)
                    {
                        shoppingCart.FormofPaymentDetails = new MOBFormofPaymentDetails();
                    }
                    shoppingCart.FormofPaymentDetails.InternationalBilling = new MOBInternationalBilling();
                    var billingCountries = await GetCachedBillingAddressCountries().ConfigureAwait(false);

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

                    shoppingCart.FormofPaymentDetails.InternationalBilling.BillingAddressProperties = (billingCountries == null || !billingCountries.Any()) ? null : billingCountries;
                }

                //Travel Credit summary Banner should not displaying on RTI when Money&Miles||TravelBank FOP is Applied - MOBILE-23748
                if (!_configuration.GetValue<bool>("EnableTravelCreditBanner"))
                {
                    var isMandatory = !_configuration.GetValue<bool>("DisableTravelCreditBannerCheckForUpdateContact") ? (shoppingCart?.FormofPaymentDetails?.CreditCard != null ? shoppingCart.FormofPaymentDetails.CreditCard.IsMandatory : false) : false;
                    if ((shoppingCart.FormofPaymentDetails?.MoneyPlusMilesCredit?.SelectedMoneyPlusMiles != null ||
                         shoppingCart.FormofPaymentDetails?.TravelBankDetails?.TBApplied > 0 ||
                         shoppingCart?.FormofPaymentDetails?.FormOfPaymentType == MOBFormofPayment.ApplePay.ToString() ||
                         shoppingCart?.FormofPaymentDetails?.FormOfPaymentType == MOBFormofPayment.PayPal.ToString() ||
                         shoppingCart?.FormofPaymentDetails?.FormOfPaymentType == MOBFormofPayment.PayPalCredit.ToString() ||
                         shoppingCart?.FormofPaymentDetails?.FormOfPaymentType == MOBFormofPayment.Masterpass.ToString() ||
                         shoppingCart?.FormofPaymentDetails?.FormOfPaymentType == MOBFormofPayment.Uplift.ToString() ||
                         isMandatory ||
                         (!_configuration.GetValue<bool>("DisableTravelCreditBannerCheckForUpdateContact") && !response.Exists(x => x.Category == "TRAVELCREDIT")))
                         && !string.IsNullOrEmpty(shoppingCart?.FormofPaymentDetails?.TravelCreditDetails?.TravelCreditSummary)
                         && flow?.ToLower() == FlowType.BOOKING.ToString()?.ToLower())
                    {
                        shoppingCart.FormofPaymentDetails.TravelCreditDetails.TravelCreditSummary = string.Empty;
                    }
                    //Apple Pay to remove
                    if (isMandatory)
                    {
                        response = response.Where(x => x.Category.ToUpper() != "APP").ToList();
                    }
                }
                try
                {
                    if (_shoppingCartUtility.IsEnableMoneyPlusMilesFeature(request.Application.Id, request.Application.Version.Major, session?.CatalogItems))
                    {
                        reservation.IsEligibleForMoneyPlusMiles = (shoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit != null
                            && (session.IsMoneyPlusMilesSelected || _shoppingCartUtility.IsIncludeMoneyMilesInRTI(response))); // Indicator for Display the M+M on RTI screen 
                        reservation.IsMoneyPlusMilesSelected = session.IsMoneyPlusMilesSelected;// Indiactor for UI display the M+M Option with checkbox selected
                        if (await _featureSettings.GetFeatureSettingValue("EnableFSRETCTravelCreditsFeature").ConfigureAwait(false) && reservation.IsMoneyPlusMilesSelected)
                        {
                            reservation.EligibleForETCPricingType = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.ILoggerWarning("GetMoneyPlusMilesOptionsForFinalRTIScreen : There is problem getting setting IsEligibleMoneyMiles , {@Request} " + ex.Message, request);
                }
                try
                {
                    if (await _featureSettings.GetFeatureSettingValue("EnableFSRETCTravelCreditsFeature").ConfigureAwait(false) && response?.Any(e => e.Code == MOBFormofPayment.ETC.ToString() || e.Code == MOBFormofPayment.TC.ToString()) == true)
                    {
                        reservation.EligibleForETCPricingType = true;
                        reservation.PricingType = session.PricingType;
                        if (session.PricingType != null && session.PricingType == PricingType.ETC.ToString())
                        {
                            reservation.IsEligibleForMoneyPlusMiles = false;

                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.ILoggerWarning("GetETCTravelCreditsRTIScreen : There is problem getting setting IsMileagePricingTypeSelected , {@Request} " + ex.Message, request);
                }
                await _sessionHelperService.SaveSession<List<FormofPaymentOption>>(response, session.SessionId, new List<string> { session.SessionId, response.GetType().ToString() }, response.GetType().ToString()).ConfigureAwait(false); //Change Session
            }
            return (response, isDefault);
            //return response;
        }

        public List<FormofPaymentOption> BuildEligibleFormofPaymentsResponse(List<FormofPaymentOption> response, MOBShoppingCart shoppingCart, Session session, MOBRequest request, bool isMetaSearch = false, bool isPoolMiles = false)
        {
            //Metasearch
            if (!_configuration.GetValue<bool>("EnableETCFopforMetaSearch") && isMetaSearch && _configuration.GetValue<bool>("CreditCardFOPOnly_MetaSearch"))
            {
                return response;
            }
            if (_configuration.GetValue<bool>("EnableFFCinBookingforPreprodTesting"))
            {
                if (!response.Exists(x => x.Category == "CERT" && x.Code == "FF"))
                {
                    FormofPaymentOption elgibileOption = new FormofPaymentOption();
                    elgibileOption.Category = "CERT";
                    elgibileOption.Code = "FF";
                    elgibileOption.FoPDescription = "Future flight credit";
                    elgibileOption.FullName = "Future flight credit";
                    response.Add(elgibileOption);
                }
            }
            bool isMultiTraveler = shoppingCart.SCTravelers?.Count > 1;
            bool isTravelCertificateAdded = shoppingCart.FormofPaymentDetails.TravelCertificate != null && shoppingCart.FormofPaymentDetails.TravelCertificate.Certificates != null && shoppingCart.FormofPaymentDetails.TravelCertificate.Certificates.Count > 0;
            bool isETCCombinabilityChangeEnabled = IsETCCombinabilityEnabled(request.Application.Id, request.Application.Version.Major);
            bool isFFCAdded = shoppingCart.FormofPaymentDetails.TravelFutureFlightCredit != null && shoppingCart.FormofPaymentDetails.TravelFutureFlightCredit.FutureFlightCredits != null && shoppingCart.FormofPaymentDetails.TravelFutureFlightCredit.FutureFlightCredits.Count > 0;
            if (IsETCEligibleTravelType(session) && !isMultiTraveler && !isETCCombinabilityChangeEnabled)//Check whether ETC Eligible booking Type
            {
                //If travel certificate is added only creditcard should be allowed
                if (isTravelCertificateAdded)
                {
                    response = response.Where(x => x.Category == "CC").ToList();
                }
            }
            else if (IsETCEligibleTravelType(session) && isMultiTraveler && !isETCCombinabilityChangeEnabled)
            {
                if (IsETCEnabledforMultiTraveler(request.Application.Id, request.Application.Version.Major.ToString()) && isTravelCertificateAdded && shoppingCart.IsMultipleTravelerEtcFeatureClientToggleEnabled)
                {
                    //Entire reservation price is covered with ETC..it doesnt matter whether Ancillary is added or not we need to show only credit card
                    if (shoppingCart.Products != null && shoppingCart.Products.Exists(x => x.Code == "RES") && Convert.ToDecimal(shoppingCart.Products?.Where(x => x.Code == "RES").FirstOrDefault().ProdTotalPrice) == 0)
                    {
                        response = response.Where(x => x.Category == "CC").ToList();
                    }//There is residual amount left on reservation and Ancillary products amount
                    else if (shoppingCart.Products != null && shoppingCart.Products.Exists(x => x.Code == "RES") && Convert.ToDecimal(shoppingCart.Products?.Where(x => x.Code == "RES").FirstOrDefault().ProdTotalPrice) > 0)
                    {
                        //If there is residual amount left on reservation but apply for all traveler option is selected it doesnt matter whether we added ancillary or not we need to show only credit card
                        if (shoppingCart.FormofPaymentDetails?.TravelCertificate?.Certificates[0]?.IsForAllTravelers == true)
                        {
                            response = response.Where(x => x.Category == "CC").ToList();
                        }
                        else
                        {
                            if (IsCertificatesAppliedforAllIndividualTravelers(shoppingCart))
                            {
                                response = response.Where(x => x.Category == "CC").ToList();
                            }
                            else
                            {
                                response = response.Where(x => x.Category == "CC" || x.Category == "CERT").ToList();
                            }
                        }
                    }
                }
            }
            else if (IsETCEligibleTravelType(session) && isETCCombinabilityChangeEnabled && isTravelCertificateAdded)
            {
                if (shoppingCart?.FormofPaymentDetails?.TravelCertificate?.AllowedETCAmount > shoppingCart?.FormofPaymentDetails?.TravelCertificate?.TotalRedeemAmount &&
                    (_configuration.GetValue<bool>("ETCMaxCountCheckToggle") ? shoppingCart?.FormofPaymentDetails?.TravelCertificate?.Certificates?.Count < shoppingCart?.FormofPaymentDetails.TravelCertificate?.MaxNumberOfETCsAllowed : true))
                {
                    response = response.Where(x => x.Category == "CC" || (x.Category == "CERT" && x.Code == "ETC")).ToList();
                }
                else
                {
                    response = response.Where(x => x.Category == "CC").ToList();
                }
            }
            else if (IsETCEligibleTravelType(session, "FFCEligibleTravelTypes") && isFFCAdded && IncludeFFCResidual(request.Application.Id, request.Application.Version.Major))
            {
                if (shoppingCart?.FormofPaymentDetails?.TravelFutureFlightCredit?.AllowedFFCAmount > shoppingCart?.FormofPaymentDetails?.TravelFutureFlightCredit?.TotalRedeemAmount)
                {
                    response = response.Where(x => x.Category == "CC" || (x.Category == "CERT" && x.Code == "FF")).ToList();
                }
                else
                {
                    response = response.Where(x => x.Category == "CC").ToList();
                }
            }
            else if ((!IsETCEligibleTravelType(session) || !IsETCEligibleTravelType(session, "FFCEligibleTravelTypes"))) //ETC Shouldn't be allowed for ineligible travel types
            {
                if (!IncludeFFCResidual(request.Application.Id, request.Application.Version.Major))
                {
                    response = response.Where(x => x.Category != "CERT").ToList();
                }
                else
                {
                    if (!IsETCEligibleTravelType(session))
                    {
                        var etcFOP = response.Where(x => x.Category == "CERT" && x.Code == "ETC").FirstOrDefault();
                        if (etcFOP != null)
                            response.Remove(etcFOP);
                    }
                    if (!IsETCEligibleTravelType(session, "FFCEligibleTravelTypes"))
                    {
                        var ffcFOP = response.Where(x => x.Category == "CERT" && x.Code == "FF").FirstOrDefault();
                        if (ffcFOP != null)
                            response.Remove(ffcFOP);
                    }
                }
            }
            if ((!_configuration.GetValue<bool>("EnableETCFopforMetaSearch") ? !isMetaSearch : true)//to enable ETC for metasearch               
                && /*IsShoppingCarthasOnlyFareLockProduct(shoppingCart)*/
                shoppingCart?.FormofPaymentDetails?.FormOfPaymentType == MOBFormofPayment.ApplePay.ToString() ||
                shoppingCart?.FormofPaymentDetails?.FormOfPaymentType == MOBFormofPayment.PayPal.ToString() ||
                shoppingCart?.FormofPaymentDetails?.FormOfPaymentType == MOBFormofPayment.PayPalCredit.ToString() ||
                shoppingCart?.FormofPaymentDetails?.FormOfPaymentType == MOBFormofPayment.Masterpass.ToString() ||
                shoppingCart?.FormofPaymentDetails?.FormOfPaymentType == MOBFormofPayment.Uplift.ToString() ||
                (!(IsETCEnabledforMultiTraveler(request.Application.Id, request.Application.Version.Major.ToString()) && shoppingCart.IsMultipleTravelerEtcFeatureClientToggleEnabled) ? (isMultiTraveler) : false))
            {
                response = response.Where(x => x.Category != "CERT").ToList();
                if (IncludeMoneyPlusMiles(request.Application.Id, request.Application.Version.Major))
                {
                    response = response.Where(x => x.Category != "MILES").ToList();
                }
                if (IncludeTravelBankFOP(request.Application.Id, request.Application.Version.Major))
                {
                    response = response.Where(x => x.Category != "TRAVELBANK").ToList();
                }
                if (IncludeTravelCredit(request.Application.Id, request.Application.Version.Major))
                {
                    response = response.Where(x => x.Category != "TRAVELCREDIT").ToList();
                    if (_configuration.GetValue<bool>("EnableTravelCreditSummary") && !string.IsNullOrEmpty(shoppingCart?.FormofPaymentDetails?.TravelCreditDetails?.TravelCreditSummary))
                    {
                        shoppingCart.FormofPaymentDetails.TravelCreditDetails.TravelCreditSummary = string.Empty;
                    }
                }
            }

            if (response.Exists(x => x.Category == "CERT" && x.Code == "ETC"))
            {
                response.Where(x => x.Category == "CERT" && x.Code == "ETC").FirstOrDefault().FullName = _configuration.GetValue<string>("ETCFopFullName");
            }
            if (response.Exists(x => x.Category == "CERT" && x.Code == "FF"))
            {
                response.Where(x => x.Category == "CERT" && x.Code == "FF").FirstOrDefault().FullName = _configuration.GetValue<string>("FFCFopFullName");
            }
            if (isPoolMiles)
            {
                response.Add(new FormofPaymentOption()
                {
                    Category = "POOLEDMILES",
                    FullName = "Miles",
                    Code = "Miles",
                    DeleteOrder = false,
                    FoPDescription = "Miles"
                });
                response.Add(new FormofPaymentOption()
                {
                    Category = "POOLEDMILES",
                    FullName = "Pooled miles",
                    Code = "PooledMiles",
                    DeleteOrder = false,
                    FoPDescription = "Pooled Miles"
                });
            }
            return response;
        }

        private bool IsETCCombinabilityEnabled(int applicationId, string appVersion)
        {
            if (_configuration.GetValue<bool>("CombinebilityETCToggle") && GeneralHelper.IsApplicationVersionGreaterorEqual(applicationId, appVersion, _configuration.GetValue<string>("Android_EnableETCCombinability_AppVersion"), _configuration.GetValue<string>("iPhone_EnableETCCombinability_AppVersion")))
            {
                return true;
            }
            return false;
        }

        private bool IsCertificatesAppliedforAllIndividualTravelers(MOBShoppingCart shoppingCart)
        {
            int certificateAppliedTravelerCount = 0, travelersCount = 0;
            certificateAppliedTravelerCount = shoppingCart.CertificateTravelers.Where(x => x.TravelerNameIndex != "0" && x.IsCertificateApplied).Count();
            travelersCount = shoppingCart?.SCTravelers != null ? shoppingCart.SCTravelers.Where(sct => sct.IndividualTotalAmount > 0).Count() : 0;
            if (certificateAppliedTravelerCount == travelersCount)
            {
                return true;
            }

            return false;
        }

        private bool IncludeTravelCredit(int appId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnableTravelCredit") &&
                   GeneralHelper.IsApplicationVersionGreater(appId, appVersion, "AndroidTravelCreditVersion", "iPhoneTravelCreditVersion", "", "", true, _configuration);
        }

        private bool IncludeFFCResidual(int appId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnableFFCResidual") && GeneralHelper.IsApplicationVersionGreater(appId, appVersion, "AndroidFFCResidualVersion", "iPhoneFFCResidualVersion", "", "", true, _configuration);
        }

        private bool IsETCEligibleTravelType(Session session, string travelTypeConfigKey = "ETCEligibleTravelTypes")
        {
            string[] travelTypes = _configuration.GetValue<string>(travelTypeConfigKey).Split('|');//"Revenue|YoungAdult"
            bool isEligible = false;
            if (session.IsAward && travelTypes.Contains("Award"))
            {
                isEligible = true;
            }
            else if (!string.IsNullOrEmpty(session.EmployeeId) && travelTypes.Contains("UADiscount"))
            {
                isEligible = true;
            }
            else if (session.IsYoungAdult && travelTypes.Contains("YoungAdult"))
            {
                isEligible = true;
            }
            else if (session.IsCorporateBooking && travelTypes.Contains("Corporate"))
            {
                isEligible = true;
            }
            else if (session.TravelType == TravelType.CLB.ToString() && travelTypes.Contains("CorporateLeisure"))
            {
                isEligible = true;
            }
            else if (!session.IsAward && string.IsNullOrEmpty(session.EmployeeId) && !session.IsYoungAdult && !session.IsCorporateBooking && session.TravelType != TravelType.CLB.ToString() && travelTypes.Contains("Revenue"))
            {
                isEligible = true;
            }
            return isEligible;
        }

        private bool IsETCEnabledforMultiTraveler(int applicationId, string appVersion)
        {
            if (_configuration.GetValue<bool>("MTETCToggle") && GeneralHelper.IsApplicationVersionGreaterorEqual(applicationId, appVersion, _configuration.GetValue<string>("iPhone_EnableETCForMultiTraveler_AppVersion"), _configuration.GetValue<string>("Android_EnableETCForMultiTraveler_AppVersion")))
            {
                return true;
            }
            return false;
        }

        private bool IncludeMoneyPlusMiles(int appId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnableMilesPlusMoney")
                && GeneralHelper.IsApplicationVersionGreater
                (appId, appVersion, "AndroidMilesPlusMoneyVersion", "iPhoneMilesPlusMoneyVersion", "", "", true, _configuration);
        }

        public async Task<(List<FormofPaymentOption> EligibleFormofPayments, bool isDefault)> EligibleFormOfPayments(MOBFOPEligibilityRequest request, Session session, bool IsMilesFOPEnabled = false)
        {
            var response = new List<FormofPaymentOption>();
            bool isDefault = false;
            try
            {
                if (!string.IsNullOrEmpty(request.Flow) && request.Flow == "RESHOP")
                {
                    request.Flow = "EXCHANGE";
                }
                if (await _featureSettings.GetFeatureSettingValue("EnableFixForMMFOPForViewResSeatMapFlow") && request.Flow == FlowType.VIEWRES_SEATMAP.ToString())
                {
                    request.Flow = FlowType.VIEWRES.ToString();
                }
                string requestXml = string.Empty;
                United.Service.Presentation.InteractionModel.ShoppingCart shoppingCart = await BuildEligibleFormOfPaymentsRequest(request, session);
                bool isMigrateToJSONService = _configuration.GetValue<bool>("EligibleFopMigrateToJSonService");
                var xmlRequest = isMigrateToJSONService
                                 ? JsonConvert.SerializeObject(shoppingCart)
                                 : XmlSerializerHelper.Serialize(shoppingCart);

                string url = "/FormOfPayment/EligibleFormOfPaymentByShoppingCart";

                _logger.LogInformation("EligibleFormOfPayments - EligibleFormOfPaymentByShoppingCart Request {@shoppingCart} Path:{@url}", JsonConvert.SerializeObject(shoppingCart), url);

                if (ConfigUtility.IsEnableU4BCorporateBookingFeature(request.Application.Id, request.Application.Version.Major) && session.IsCorporateBooking)
                {
                    var buildEligibleFOPRequest = BuildEligibleFOPRequest(request.CartId, session.IsCorporateBooking.ToString(), shoppingCart.PointOfSale.Country.CountryCode, shoppingCart);
                    url = "/FormOfPayment/EligibleFOP";
                    xmlRequest = JsonConvert.SerializeObject(buildEligibleFOPRequest);
                }
                string xmlResponse = await _paymentService.GetEligibleFormOfPayments(session.Token, url, xmlRequest, request.SessionId).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(xmlResponse))
                {

                    Service.Presentation.PaymentResponseModel.EligibleFormOfPayment eligibleFormOfPayment = isMigrateToJSONService
                                                                                                            ? JsonConvert.DeserializeObject<Service.Presentation.PaymentResponseModel.EligibleFormOfPayment>(xmlResponse)
                                                                                                            : XmlSerializerHelper.Deserialize<Service.Presentation.PaymentResponseModel.EligibleFormOfPayment>(xmlResponse);

                    if ((request.Products != null) && (request.Products.Count == 1) && (IsETCchangesEnabled(request.Application.Id, request.Application.Version.Major) ? request.Flow != FlowType.BOOKING.ToString() : true))
                    {
                        response = eligibleFormOfPayment.ProductFormOfPayment[0].FormsOfPayment.GroupBy(p => p.Payment.Type.Key).Select(x => x.FirstOrDefault()).Select(x => new FormofPaymentOption { Category = x.Payment.Category, FullName = (x.Payment.Category == "CC") ? "Credit Card" : x.Payment.Type.Description, Code = x.Payment.Type.Key, FoPDescription = x.Payment.Type.Description }).ToList();
                    }
                    else
                    {
                        response = eligibleFormOfPayment.FormsOfPayment.GroupBy(p => p.Payment.Type.Key).Select(x => x.FirstOrDefault()).Select(x => new FormofPaymentOption { Category = x.Payment.Category, FullName = (x.Payment.Category == "CC") ? "Credit Card" : x.Payment.Type.Description, Code = x.Payment.Type.Key, FoPDescription = x.Payment.Type.Description }).ToList();
                    }
                    isDefault = false;

                    if (IsMilesFOPEnabled && ConfigUtility.IsMilesFOPEnabled())
                    {
                        response.Insert(1, new FormofPaymentOption { Category = "MILES", FullName = "Use miles", Code = "Miles", FoPDescription = "Use miles" });
                    }

                    if (IsMilesFOPEnabled)
                    {
                        var MilesFOP = response.FirstOrDefault(fop => fop.Code == "MILE");
                        if (MilesFOP != null)
                        {
                            MilesFOP.Category = "USEMILES";
                            MilesFOP.FoPDescription = "Use miles";
                            MilesFOP.FullName = "Use miles";
                        }
                    }
                    else
                    {
                        response = response.Where(x => x.Code != "MILE").ToList();
                    }

                    //If Payment service enables ETC fop for SEATS and PCU it will automatically shows ETC as FOP .So,disabling etc for lower versions .
                    if ((request.Flow == FlowType.VIEWRES.ToString() || _configuration.GetValue<bool>("IsEnableManageResCoupon") && request.Flow == FlowType.VIEWRES_SEATMAP.ToString()) && !GeneralHelper.IsApplicationVersionGreaterorEqual(request.Application.Id, request.Application.Version.Major, _configuration.GetValue<string>("Android_EnableETCManageRes_AppVersion"), _configuration.GetValue<string>("iPhone_EnableETCManageRes_AppVersion")))
                    {
                        if (response.Exists(x => x.Category == "CERT"))
                        {
                            response = response.Where(x => x.Category != "CERT").ToList();
                        }
                    }

                    if ((request.Flow == FlowType.BOOKING.ToString() && !IncludeMoneyPlusMiles(request.Application.Id, request.Application.Version.Major)) || session.IsAward || session.IsCorporateBooking)
                    {
                        if (response.Exists(x => x.Category == "MILES"))
                        {
                            response = response.Where(x => x.Category != "MILES").ToList();
                        }
                    }

                    if (!IncludeTravelBankFOP(request.Application.Id, request.Application.Version.Major) ||
                        request.Flow != FlowType.BOOKING.ToString() ||
                        session.IsAward ||
                        await GetTravelBankBalance(session.SessionId).ConfigureAwait(false) == 0.00)
                    {
                        if (response.Exists(x => x.Category == "TRAVELBANK"))
                        {
                            response = response.Where(x => x.Category != "TRAVELBANK").ToList();
                        }
                    }

                    if (request.Flow == FlowType.BOOKING.ToString()
                        && IncludeTravelCredit(request.Application.Id, request.Application.Version.Major)
                        && !session.IsAward
                        && (ConfigUtility.IsEnableU4BCorporateBookingFeature(request.Application.Id, request.Application.Version.Major) ? true : !session.IsCorporateBooking)
                        && response.Exists(x => x.Category == "CERT"))
                    {
                        response.Add(
                            new FormofPaymentOption
                            {
                                Category = "TRAVELCREDIT",
                                Code = "TC",
                                DeleteOrder = false,
                                FoPDescription = "Travel Credit",
                                FullName = "Travel Credit"
                            });
                        response.RemoveAll(x => x.Category == "CERT");
                    }

                    if (request.Flow == FlowType.BOOKING.ToString()
                         && IncludeTravelCredit(request.Application.Id, request.Application.Version.Major)
                         && !session.IsAward
                         && !ConfigUtility.IsEnableU4BCorporateBookingFeature(request.Application.Id, request.Application.Version.Major)
                         && session.IsCorporateBooking)

                    {
                        response.RemoveAll(x => x.Category == "CERT");
                    }
                }
                _logger.LogInformation("EligibleFormOfPayments - Response {@response}  ", JsonConvert.SerializeObject(response));
                //logEntries.Add(LogEntry.GetLogEntry<List<FormofPaymentOption>>(request.SessionId, "EligibleFormOfPayments - CSL Response ", "Trace", request.Application.Id, request.Application.Version.Major, request.DeviceId, response, true, false));

            }
            catch (System.Net.WebException wex)
            {
                if (wex.Response != null)
                {
                    var errorResponse = new StreamReader(wex.Response.GetResponseStream()).ReadToEnd();

                    //logEntries.Add(United.Logger.LogEntry.GetLogEntry<string>(request.SessionId, "EligibleFormOfPayments - Exception", "ErrorMessageResponse", request.Application.Id, request.Application.Version.Major, request.DeviceId, errorResponse, true, false));
                    _logger.LogError("EligibleFormOfPayments-Exception, ErrorMessageResponse:{exception} with sessionId:{sessionId}", JsonConvert.SerializeObject(wex), request.SessionId);
                    response.Add(new FormofPaymentOption { Category = "CC", FullName = "Credit Card", Code = null, FoPDescription = null });
                    response.Add(new FormofPaymentOption { Category = "PP", FullName = "PayPal", Code = null, FoPDescription = null });
                    response.Add(new FormofPaymentOption { Category = "PPC", FullName = "PayPal Credit", Code = null, FoPDescription = null });
                    response.Add(new FormofPaymentOption { Category = "MPS", FullName = "Masterpass", Code = null, FoPDescription = null });
                    response.Add(new FormofPaymentOption { Category = "APP", FullName = "Apple Pay", Code = null, FoPDescription = null });

                    isDefault = true;
                }
            }
            catch (MOBUnitedException coex)
            {
                response = null;
                _logger.LogError("EligibleFormOfPayments- UnitedException:{exception} with sessionId:{sessionId}", JsonConvert.SerializeObject(coex), request.SessionId);
            }
            catch (System.Exception ex)
            {
                response = null;
                MOBExceptionWrapper exceptionWrapper = new MOBExceptionWrapper(ex);
                _logger.LogError("EligibleFormOfPayments- Exception:{exception} with sessionId:{sessionId}", exceptionWrapper, request.SessionId);
            }
            return (response, isDefault);
            //return response;
        }

        private EligibleFOPRequest BuildEligibleFOPRequest(string cartId, string isCorporateBooking, string countryCode, United.Service.Presentation.InteractionModel.ShoppingCart shoppingCart)
        {
            EligibleFOPRequest eligibleFOPRequest = new EligibleFOPRequest();
            eligibleFOPRequest.CartID = cartId;
            eligibleFOPRequest.IsCorporateBooking = isCorporateBooking.ToString();
            eligibleFOPRequest.PointOfSale = shoppingCart.PointOfSale.Country.CountryCode;
            eligibleFOPRequest.ShoppingCart = shoppingCart;
            return eligibleFOPRequest;
        }
        public async Task<United.Service.Presentation.InteractionModel.ShoppingCart> BuildEligibleFormOfPaymentsRequest(MOBFOPEligibilityRequest request, Session session)
        {
            string channel = (request.Application.Id == 1 ? "MOBILE-IOS" : "MOBILE-Android");
            string productContext = string.Format(@" <Reservation xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://schemas.datacontract.org/2004/07/United.Service.Presentation.ReservationModel"">            <Channel>{0}</Channel>            <Type xmlns:d2p1=""http://schemas.datacontract.org/2004/07/United.Service.Presentation.CommonModel"">            <d2p1:Genre>            <d2p1:Key>{1}</d2p1:Key>            </d2p1:Genre>            </Type>            </Reservation> ", channel, request.Flow);
            United.Service.Presentation.InteractionModel.ShoppingCart shoppingCart = new United.Service.Presentation.InteractionModel.ShoppingCart();
            shoppingCart.ID = new UniqueIdentifier();
            shoppingCart.ID.ID = (!string.IsNullOrEmpty(request.CartId) ? request.CartId : Guid.NewGuid().ToString());
            shoppingCart.Items = new Collection<ShoppingCartItem>();
            var shoppingCartItem = new ShoppingCartItem();
            shoppingCartItem.Product = new Collection<Product>();

            foreach (var product in request.Products)
            {
                Product productObj = new Product();
                productObj.Code = (_configuration.GetValue<bool>("EnableFareLockPurchaseViewRes") && product.Code != null && product.Code.Equals("FLK_VIEWRES", StringComparison.OrdinalIgnoreCase)) ? "FLK" : product.Code;
                productObj.Description = product.ProductDescription;
                shoppingCartItem.Product.Add(productObj);
            }

            if (IsETCchangesEnabled(request.Application.Id, request.Application.Version.Major) && request.Flow == FlowType.BOOKING.ToString()
                || (request.Flow == FlowType.VIEWRES.ToString() || request.Flow == FlowType.BAGGAGECALCULATOR.ToString()) && ConfigUtility.IsManageResETCEnabled(request.Application.Id, request.Application.Version.Major))
            {
                channel = (request.Application.Id == 1 ? _configuration.GetValue<string>("eligibleFopMobileioschannelname") : _configuration.GetValue<string>("eligibleFopMobileandriodchannelname"));
                string newProductContext = string.Empty;
                newProductContext = await BuildProductContextForEligibleFoprequest(request, channel, session);
                productContext = string.IsNullOrEmpty(newProductContext) ? productContext : newProductContext;
                //FareLock code should be FLK Instead of FareLock
                if (shoppingCartItem.Product.Exists(x => x.Code == "FareLock"))
                {
                    shoppingCartItem.Product.Where(x => x.Code == "FareLock").FirstOrDefault().Code = "FLK";
                }
            }
            shoppingCartItem.ProductContext = new Collection<string>();
            shoppingCartItem.ProductContext.Add(productContext);
            shoppingCart.Items.Add(shoppingCartItem);
            shoppingCart.PointOfSale = new Service.Presentation.CommonModel.PointOfSale();
            shoppingCart.PointOfSale.Country = new Service.Presentation.CommonModel.Country();
            shoppingCart.PointOfSale.Country.CountryCode = await _shoppingCartUtility.IsCheckInFlow(request.Flow) && await _featureSettings.GetFeatureSettingValue("EnableCheckInCloudMigrationMSC_23X") && !string.IsNullOrEmpty(request.PointOfSale) ? request.PointOfSale : "US";
            return shoppingCart;
        }

        private string GetMobilePathforEligibleFop(string flow)
        {
            string mobilePath = string.Empty;
            switch (flow)
            {
                case "BOOKING":
                    mobilePath = "INITIAL";
                    break;
                case "RESHOP":
                    mobilePath = "EXCHANGE";
                    break;
                case "BAGGAGECALCULATOR":
                    mobilePath = "VIEWRES";
                    break;
                default:
                    return flow;
            }
            return mobilePath;
        }

        private async Task<string> BuildProductContextForEligibleFoprequest(MOBFOPEligibilityRequest request, string channel, Session session)
        {
            var reservation = new Service.Presentation.ReservationModel.Reservation();
            string mobilePath = GetMobilePathforEligibleFop(request.Flow);
            reservation.Channel = channel;
            reservation.Type = new Collection<Genre>();
            reservation.Type.Add(new Genre { Key = mobilePath });
            switch (request.Flow)
            {
                case "BOOKING":
                    Reservation persistedReservation = new Reservation();

                    persistedReservation = await _sessionHelperService.GetSession<Reservation>(request.SessionId, persistedReservation.ObjectName, new List<string> { request.SessionId, persistedReservation.ObjectName }).ConfigureAwait(false);
                    if (persistedReservation == null)
                    {
                        _logger.LogError("BuildProductContextForEligibleFoprequest Reservation is null with {sessionID}", session.SessionId);
                    }

                    List<ReservationFlightSegment> segments = DataContextJsonSerializer.DeserializeUseContract<List<ReservationFlightSegment>>(persistedReservation.CSLReservationJSONFormat);
                    if (persistedReservation != null)
                    {
                        reservation.FlightSegments = segments.ToCollection();//ETC is offered for only united operated flights.Need to send the segment details with operating carrier code.
                        if (IncludeMoneyPlusMiles(request.Application.Id, request.Application.Version.Major))
                        {
                            reservation = await _sessionHelperService.GetSession<Service.Presentation.ReservationModel.Reservation>(request.SessionId, reservation.GetType().FullName, new List<string> { request.SessionId, reservation.GetType().FullName }).ConfigureAwait(false);

                            if (reservation == null)
                            {
                                var cartInfo = await GetCartInformation(request.SessionId, request.Application, request.DeviceId, request.CartId, session.Token).ConfigureAwait(false);
                                reservation = cartInfo.Reservation;
                                await _sessionHelperService.SaveSession<Service.Presentation.ReservationModel.Reservation>(cartInfo.Reservation, session.SessionId, new List<string> { session.SessionId, reservation.GetType().FullName }, reservation.GetType().FullName).ConfigureAwait(false);
                            }

                            reservation.Channel = channel;
                            reservation.Type = new Collection<Genre>();
                            reservation.Type.Add(new Genre { Key = mobilePath });
                        }
                    }

                    //return XmlSerializerHelper.Serialize<Service.Presentation.ReservationModel.Reservation>(reservation);
                    return ProviderHelper.SerializeXml(reservation);
                case "VIEWRES":
                case "BAGGAGECALCULATOR":
                    ReservationDetail reservationDetail = new ReservationDetail();
                    reservationDetail = await _sessionHelperService.GetSession<ReservationDetail>(request.SessionId, reservationDetail.GetType().FullName, new List<string> { request.SessionId, reservationDetail.GetType().FullName }).ConfigureAwait(false);

                    if (reservationDetail != null)
                    {

                        reservation.FlightSegments = reservationDetail.Detail.FlightSegments;//ETC is offered for only united operated flights.Need to send the segment details with operating carrier code.
                    }

                    //return XmlSerializerHelper.Serialize<Service.Presentation.ReservationModel.Reservation>(reservation);
                    return ProviderHelper.SerializeXml(reservation);
                default: return string.Empty;
            }
        }

        public async Task<LoadReservationAndDisplayCartResponse> GetCartInformation(string sessionId, MOBApplication application, string device, string cartId, string token, WorkFlowType workFlowType = WorkFlowType.InitialBooking)
        {
            LoadReservationAndDisplayCartRequest loadReservationAndDisplayCartRequest = new LoadReservationAndDisplayCartRequest();
            LoadReservationAndDisplayCartResponse loadReservationAndDisplayResponse = new LoadReservationAndDisplayCartResponse();
            loadReservationAndDisplayCartRequest.CartId = cartId;
            loadReservationAndDisplayCartRequest.WorkFlowType = workFlowType;
            string jsonRequest = JsonConvert.SerializeObject(loadReservationAndDisplayCartRequest);

            loadReservationAndDisplayResponse = await _shoppingCartService.GetCartInformation<LoadReservationAndDisplayCartResponse>(token, "LoadReservationAndDisplayCart", jsonRequest, _headers.ContextValues.SessionId);

            return loadReservationAndDisplayResponse;
        }

        private bool IsETCchangesEnabled(int applicationId, string appVersion)
        {
            if (_configuration.GetValue<bool>("ETCToggle") && GeneralHelper.IsApplicationVersionGreaterorEqual(applicationId, appVersion, _configuration.GetValue<string>("iPhone_ETC_AppVersion"), _configuration.GetValue<string>("Android_ETC_AppVersion")))
            {
                return true;
            }
            return false;
        }

        public async Task<double> GetTravelBankBalance(string sessionId)
        {
            MOBCPTraveler mobCPTraveler = await GetProfileOwnerTravelerCSL(sessionId).ConfigureAwait(false);

            return mobCPTraveler?.MileagePlus?.TravelBankBalance > 0.00 ? mobCPTraveler.MileagePlus.TravelBankBalance : 0.00;
        }

        public async Task<MOBCPTraveler> GetProfileOwnerTravelerCSL(string sessionID)
        {
            ProfileResponse profilePersist = new ProfileResponse();
            profilePersist = await GetCSLProfileResponseInSession(sessionID).ConfigureAwait(false);
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

        public List<FormofPaymentOption> BuildEligibleFormofPaymentsResponse(List<FormofPaymentOption> response, MOBShoppingCart shoppingCart, MOBRequest request)
        {
            bool isTravelCertificateAdded = shoppingCart.FormofPaymentDetails?.TravelCertificate != null && shoppingCart.FormofPaymentDetails.TravelCertificate.Certificates != null && shoppingCart.FormofPaymentDetails.TravelCertificate.Certificates.Count > 0;
            if (isTravelCertificateAdded)
            {

                if (shoppingCart?.FormofPaymentDetails?.TravelCertificate?.AllowedETCAmount > shoppingCart?.FormofPaymentDetails?.TravelCertificate?.TotalRedeemAmount
                    && shoppingCart?.FormofPaymentDetails?.TravelCertificate?.Certificates?.Count < shoppingCart?.FormofPaymentDetails.TravelCertificate?.MaxNumberOfETCsAllowed)
                {
                    response = response.Where(x => x.Category == "CC" || x.Category == "CERT").ToList();
                }
                else
                {
                    response = response.Where(x => x.Category == "CC").ToList();
                }
            }

            if (shoppingCart?.FormofPaymentDetails?.FormOfPaymentType == MOBFormofPayment.ApplePay.ToString() ||
                shoppingCart?.FormofPaymentDetails?.FormOfPaymentType == MOBFormofPayment.PayPal.ToString() ||
                shoppingCart?.FormofPaymentDetails?.FormOfPaymentType == MOBFormofPayment.PayPalCredit.ToString() ||
                shoppingCart?.FormofPaymentDetails?.FormOfPaymentType == MOBFormofPayment.Masterpass.ToString() ||
                shoppingCart?.FormofPaymentDetails?.FormOfPaymentType == MOBFormofPayment.Uplift.ToString())
            {
                response = response.Where(x => x.Category != "CERT").ToList();
            }

            if (response.Exists(x => x.Category == "CERT"))
            {
                response.Where(x => x.Category == "CERT").FirstOrDefault().FullName = _configuration.GetValue<string>("ETCFopFullName");
            }
            return response;
        }

        public List<FormofPaymentOption> TravelBankEFOPFilter(MOBRequest request, MOBFOPTravelBankDetails travelBankDet, string flow, List<FormofPaymentOption> response)
        {
            if (travelBankDet?.TBApplied > 0 && IncludeTravelBankFOP(request.Application.Id, request.Application.Version.Major) && flow == FlowType.BOOKING.ToString())
            {
                response = response.Where(x => x.Category == "CC").ToList();
            }

            return response;
        }

        private bool IncludeTravelBankFOP(int appId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnableTravelBankFOP")
                && GeneralHelper.IsApplicationVersionGreater
                (appId, appVersion, "AndroidTravelBankFOPVersion", "iPhoneTravelBankFOPVersion", "", "", true, _configuration);
        }


        public FormofPaymentOption UpliftAsFormOfPayment(MOBSHOPReservation reservation, MOBShoppingCart shoppingCart)
        {
            if (_configuration.GetValue<bool>("EnableUpliftPayment"))
            {
                if (IsEligibileForUplift(reservation, shoppingCart))
                {
                    return new FormofPaymentOption
                    {
                        Category = "UPLIFT",
                        FoPDescription = "Pay Monthly",
                        Code = "UPLIFT",
                        FullName = "Pay Monthly",
                        DeleteOrder = shoppingCart?.FormofPaymentDetails?.FormOfPaymentType?.ToUpper() != MOBFormofPayment.Uplift.ToString().ToUpper()
                    };
                }
            }
            return null;
        }

        public bool IsEligibileForUplift(MOBSHOPReservation reservation, MOBShoppingCart shoppingCart)
        {
            if (shoppingCart?.Flow?.ToUpper() == FlowType.VIEWRES.ToString().ToUpper())
            {
                return HasEligibleProductsForUplift(shoppingCart.TotalPrice, shoppingCart.Products);
            }

            if (!_configuration.GetValue<bool>("EnableUpliftPayment"))
                return false;
            if (shoppingCart.Offers != null
                && !_shoppingCartUtility.IsUpliftEligbleOffer(shoppingCart.Offers))
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

        public bool HasEligibleProductsForUplift(string totalPrice, List<MOBProdDetail> products)
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

        public async Task<List<MOBCPBillingCountry>> GetCachedBillingAddressCountries()
        {
            var seatMapDynamoDB = new SeatMapDynamoDB(_configuration, _dynamoDBService);
            try
            {
                return await seatMapDynamoDB?.GetBillingAddressCountries<List<MOBCPBillingCountry>>("inflight-billingcountries", _headers.ContextValues.SessionId);
            }
            catch (MOBUnitedException uex)
            {
                _logger.LogError("Unable to retrieve data for billing countries {KEY}", "inflight-billingcountries");
                throw uex;
            }
        }

        private bool IsInternationalBillingAddress_CheckinFlowEnabled(int appId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnableInternationalBillingAddress_CheckinFlow")
                && GeneralHelper.IsApplicationVersionGreater
                (appId, appVersion, "IntBillingCheckinFlowAndroidversion", "IntBillingCheckinFlowiOSversion", "", "", true, _configuration);
        }

        public Collection<FOPProduct> GetProductsForEligibleFopRequest(MOBShoppingCart shoppingCart, SeatChangeState state = null)
        {
            if (shoppingCart == null || shoppingCart.Products == null || !shoppingCart.Products.Any())
                return null;

            var products = shoppingCart.Products.GroupBy(k => new { k.Code, k.ProdDescription }).Select(x => new FOPProduct { Code = x.Key.Code, ProductDescription = x.Key.ProdDescription }).ToCollection();
            AddPCUToRequestWhenPCUSeatIsSelected(state, ref products);
            return products;
        }

        private void AddPCUToRequestWhenPCUSeatIsSelected(SeatChangeState state, ref Collection<FOPProduct> products)
        {
            if (state != null && state.BookingTravelerInfo.Any(t => t.Seats.Any(s => !string.IsNullOrEmpty(s.PcuOfferOptionId) && s.PriceAfterTravelerCompanionRules > 0)))
            {
                if (!products.Any(p => p.Code == "PCU"))
                {
                    products.Add(new FOPProduct { Code = "PCU", ProductDescription = "Premium Cabin Upsell" });
                }
            }
        }

        public async Task<List<MOBSHOPTrip>> GetTrips(Collection<United.Service.Presentation.SegmentModel.ReservationFlightSegment> FlightSegments, string flow)
        {
            if (FlightSegments == null || !FlightSegments.Any())
                return null;

            List<MOBSHOPTrip> trips = new List<MOBSHOPTrip>();
            var currenttripnum = string.Empty;
            foreach (var cslsegment in FlightSegments)
            {
                if (!cslsegment.TripNumber.IsNullOrEmpty() && !currenttripnum.Equals(cslsegment.TripNumber))
                {
                    currenttripnum = cslsegment.TripNumber;
                    var tripAllSegments = FlightSegments.Where(p => p != null && p.TripNumber != null && p.TripNumber == cslsegment.TripNumber).ToList();
                    var tripsegment = await ConvertPNRSegmentToShopTripWithTripNumber(tripAllSegments, flow);
                    trips.Add(tripsegment);
                }
            }
            return trips;
        }
        private async Task<MOBSHOPTrip> ConvertPNRSegmentToShopTripWithTripNumber(List<ReservationFlightSegment> pnrFlightSegment, string flow)
        {
            MOBSHOPTrip trip = null;
            if (pnrFlightSegment != null && pnrFlightSegment.Count > 0)
            {
                var pnrLastFlightSegment = pnrFlightSegment.Where(p => p.TripNumber == pnrFlightSegment[0].TripNumber).OrderByDescending(r => Convert.ToInt32(r.SegmentNumber)).FirstOrDefault();
                trip = new MOBSHOPTrip();
                trip.Origin = pnrFlightSegment[0].FlightSegment.DepartureAirport.IATACode;
                trip.OriginDecoded = await _shoppingUtility.GetAirportName(trip.Origin); // pnrFlightSegment[0].FlightSegment.DepartureAirport.Name;
                trip.Destination = pnrLastFlightSegment.FlightSegment.ArrivalAirport.IATACode;
                trip.DestinationDecoded = await _shoppingUtility.GetAirportName(trip.Destination); //pnrLastFlightSegment.FlightSegment.ArrivalAirport.Name;
                trip.DepartDate = GeneralHelper.FormatDateFromDetails(pnrFlightSegment[0].FlightSegment.DepartureDateTime);


                trip.FlattenedFlights = new List<MOBSHOPFlattenedFlight>();
                MOBSHOPFlattenedFlight mobShopFlattenedFlight = new MOBSHOPFlattenedFlight();
                mobShopFlattenedFlight.Flights = new List<MOBSHOPFlight>();
                int currentFlightIndex = 0;
                foreach (var flightSegment in pnrFlightSegment)
                {
                    if (isCOGorConnectionFlight(flightSegment))
                    {
                        foreach (United.Service.Presentation.SegmentModel.PersonFlightSegment reservationFlightSegment in flightSegment.Legs)
                        {
                            MOBSHOPFlight flight = new MOBSHOPFlight();
                            flight.Origin = reservationFlightSegment.DepartureAirport.IATACode;
                            flight.Destination = reservationFlightSegment.ArrivalAirport.IATACode;
                            flight.OriginDescription = await _shoppingUtility.GetAirportName(flight.Origin);
                            flight.DestinationDescription = await _shoppingUtility.GetAirportName(flight.Destination);

                            if (_configuration.GetValue<bool>("EnableShareTripInSoftRTI") && ConfigUtility.IsViewResFlowPaymentSvcEnabled(flow))
                            {
                                //with share trip flag OriginDescription=Chicago, IL, US (ORD)
                                flight.OriginDecodedWithCountry = reservationFlightSegment.DepartureAirport.Name;
                                flight.DestinationDecodedWithCountry = reservationFlightSegment.ArrivalAirport.Name;
                            }

                            flight.DepartureDateTimeGMT = reservationFlightSegment.DepartureUTCDateTime;
                            flight.ArrivalDateTimeGMT = reservationFlightSegment.ArrivalUTCDateTime;

                            bool flightDateChanged = false;
                            flight.FlightArrivalDays = GeneralHelper.GetDayDifference(pnrFlightSegment[0].FlightSegment.DepartureDateTime, reservationFlightSegment.ArrivalDateTime);
                            flight.NextDayFlightArrDate = GetRedEyeFlightArrDate(pnrFlightSegment[0].FlightSegment.DepartureDateTime, reservationFlightSegment.ArrivalDateTime, ref flightDateChanged);
                            flight.RedEyeFlightDepDate = GetRedEyeDepartureDate(pnrFlightSegment[0].FlightSegment.DepartureDateTime, reservationFlightSegment.DepartureDateTime, ref flightDateChanged);
                            flight.FlightDateChanged = flightDateChanged;
                            if (flightSegment.BookingClass != null && flightSegment.BookingClass.Cabin != null)
                                flight.Cabin = flightSegment.BookingClass.Cabin.Name;
                            else
                                flight.Cabin = string.Empty;

                            flight.DepartDate = GeneralHelper.FormatDate(reservationFlightSegment.DepartureDateTime);
                            flight.DepartTime = GeneralHelper.FormatTime(reservationFlightSegment.DepartureDateTime);
                            flight.DestinationDate = GeneralHelper.FormatDate(reservationFlightSegment.ArrivalDateTime);
                            flight.DestinationTime = GeneralHelper.FormatTime(reservationFlightSegment.ArrivalDateTime);
                            flight.ArrivalDateTime = reservationFlightSegment.ArrivalDateTime;
                            flight.DepartureDateTime = reservationFlightSegment.DepartureDateTime;
                            flight.FlightNumber = reservationFlightSegment.FlightNumber;

                            DateTime depatureDateTime = Convert.ToDateTime(reservationFlightSegment.DepartureUTCDateTime);
                            DateTime arrivalDateTime = Convert.ToDateTime(reservationFlightSegment.ArrivalUTCDateTime);
                            TimeSpan timeSpan = (arrivalDateTime - depatureDateTime);
                            if (((timeSpan.Days * 24) + timeSpan.Hours) > 0)
                            {
                                flight.TravelTime = string.Format("{0}h {1}m", ((timeSpan.Days * 24) + timeSpan.Hours), timeSpan.Minutes);
                            }
                            else
                            {
                                flight.TravelTime = string.Format("{0}m", timeSpan.Minutes);
                            }

                            if (currentFlightIndex > 0)
                            {
                                DateTime previousFlightArrivalTime = Convert.ToDateTime(pnrFlightSegment[currentFlightIndex - 1].FlightSegment.ArrivalUTCDateTime);
                                timeSpan = (depatureDateTime - previousFlightArrivalTime);
                                if (((timeSpan.Days * 24) + timeSpan.Hours) > 0)
                                {
                                    flight.ConnectTimeMinutes = string.Format("{0}h {1}m", ((timeSpan.Days * 24) + timeSpan.Hours), timeSpan.Minutes);
                                }
                                else
                                {
                                    flight.ConnectTimeMinutes = string.Format("{0}m", timeSpan.Minutes);
                                }
                            }

                            if (flightSegment.FlightSegment.MarketedFlightSegment != null && flightSegment.FlightSegment.MarketedFlightSegment.Count > 0)
                            {
                                flight.MarketingCarrier = flightSegment.FlightSegment.MarketedFlightSegment[0].MarketingAirlineCode;
                                flight.MarketingCarrierDescription = flightSegment.FlightSegment.MarketedFlightSegment[0].MarketingAirlineCode;
                            }
                            flight.OperatingCarrier = reservationFlightSegment.OperatingAirlineCode;
                            flight.ServiceClassDescription = GetServiceClassDescriptionFromCslReservationFlightBookingClasses(reservationFlightSegment.BookingClasses);
                            flight.EquipmentDisclosures = ConvertPNRSegmentEquipmentToMobShopEquipmentDisclousures(reservationFlightSegment.Equipment);
                            flight.Meal = GetCharacteristicValue(flightSegment.Characteristic.ToList(), "MealDescription");
                            flight.Miles = flightSegment.FlightSegment.Distance.ToString();
                            flight.Messages = new List<MOBSHOPMessage>();

                            MOBSHOPMessage msg = new MOBSHOPMessage();
                            msg.MessageCode = flight.ServiceClassDescription;
                            flight.Messages.Add(msg);
                            msg = new MOBSHOPMessage();
                            msg.MessageCode = flight.Meal;
                            flight.Messages.Add(msg);
                            msg = new MOBSHOPMessage();
                            msg.MessageCode = "None";
                            flight.Messages.Add(msg);
                            if (reservationFlightSegment.OperatingAirlineCode != "UA")
                            {
                                flight.OperatingCarrierDescription = reservationFlightSegment.OperatingAirlineName;
                            }

                            mobShopFlattenedFlight.Flights.Add(flight);
                            if (flightSegment != null && flightSegment.FlightSegment != null && flightSegment.FlightSegment.Characteristic != null)
                            {
                                if (ConfigUtility.EnablePBE() && ConfigUtility.IsViewResFlowPaymentSvcEnabled(flow))
                                {
                                    mobShopFlattenedFlight.IsIBE =
                                        ConfigUtility.IsIbeProductCode(flightSegment.FlightSegment.Characteristic);
                                }
                                else
                                {
                                    mobShopFlattenedFlight.IsIBE = IsIbe(flightSegment.FlightSegment.Characteristic);
                                }
                                mobShopFlattenedFlight.IsElf = IsElf(flightSegment.FlightSegment.Characteristic);
                            }
                            currentFlightIndex++;
                        }
                    }
                    else
                    {
                        MOBSHOPFlight flight = new MOBSHOPFlight();
                        flight.Origin = flightSegment.FlightSegment.DepartureAirport.IATACode;
                        flight.Destination = flightSegment.FlightSegment.ArrivalAirport.IATACode;
                        flight.OriginDescription = await _shoppingUtility.GetAirportName(flight.Origin);
                        flight.DestinationDescription = await _shoppingUtility.GetAirportName(flight.Destination);

                        if (_configuration.GetValue<bool>("EnableShareTripInSoftRTI") && ConfigUtility.IsViewResFlowPaymentSvcEnabled(flow))
                        {
                            //with share trip flag OriginDescription=Chicago, IL, US (ORD)
                            flight.OriginDecodedWithCountry = flightSegment.FlightSegment.DepartureAirport.Name;
                            flight.DestinationDecodedWithCountry = flightSegment.FlightSegment.ArrivalAirport.Name;
                        }

                        flight.DepartureDateTimeGMT = flightSegment.FlightSegment.DepartureUTCDateTime;
                        flight.ArrivalDateTimeGMT = flightSegment.FlightSegment.ArrivalUTCDateTime;

                        bool flightDateChanged = false;
                        flight.FlightArrivalDays = GeneralHelper.GetDayDifference(pnrFlightSegment[0].FlightSegment.DepartureDateTime, flightSegment.FlightSegment.ArrivalDateTime);
                        flight.NextDayFlightArrDate = GetRedEyeFlightArrDate(pnrFlightSegment[0].FlightSegment.DepartureDateTime, flightSegment.FlightSegment.ArrivalDateTime, ref flightDateChanged);
                        flight.RedEyeFlightDepDate = GetRedEyeDepartureDate(pnrFlightSegment[0].FlightSegment.DepartureDateTime, flightSegment.FlightSegment.DepartureDateTime, ref flightDateChanged);
                        flight.FlightDateChanged = flightDateChanged;
                        if (flightSegment.BookingClass != null && flightSegment.BookingClass.Cabin != null)
                            flight.Cabin = flightSegment.BookingClass.Cabin.Name;
                        else
                            flight.Cabin = string.Empty;

                        flight.DepartDate = GeneralHelper.FormatDate(flightSegment.FlightSegment.DepartureDateTime);
                        flight.DepartTime = GeneralHelper.FormatTime(flightSegment.FlightSegment.DepartureDateTime);
                        flight.DestinationDate = GeneralHelper.FormatDate(flightSegment.FlightSegment.ArrivalDateTime);
                        flight.DestinationTime = GeneralHelper.FormatTime(flightSegment.FlightSegment.ArrivalDateTime);
                        flight.ArrivalDateTime = flightSegment.FlightSegment.ArrivalDateTime;
                        flight.DepartureDateTime = flightSegment.FlightSegment.DepartureDateTime;
                        flight.FlightNumber = flightSegment.FlightSegment.FlightNumber;

                        DateTime depatureDateTime = Convert.ToDateTime(flightSegment.FlightSegment.DepartureUTCDateTime);
                        DateTime arrivalDateTime = Convert.ToDateTime(flightSegment.FlightSegment.ArrivalUTCDateTime);
                        TimeSpan timeSpan = (arrivalDateTime - depatureDateTime);
                        if (((timeSpan.Days * 24) + timeSpan.Hours) > 0)
                        {
                            flight.TravelTime = string.Format("{0}h {1}m", ((timeSpan.Days * 24) + timeSpan.Hours), timeSpan.Minutes);
                        }
                        else
                        {
                            flight.TravelTime = string.Format("{0}m", timeSpan.Minutes);
                        }

                        if (currentFlightIndex > 0)
                        {
                            DateTime previousFlightArrivalTime = Convert.ToDateTime(pnrFlightSegment[currentFlightIndex - 1].FlightSegment.ArrivalUTCDateTime);
                            timeSpan = (depatureDateTime - previousFlightArrivalTime);
                            if (((timeSpan.Days * 24) + timeSpan.Hours) > 0)
                            {
                                flight.ConnectTimeMinutes = string.Format("{0}h {1}m", ((timeSpan.Days * 24) + timeSpan.Hours), timeSpan.Minutes);
                            }
                            else
                            {
                                flight.ConnectTimeMinutes = string.Format("{0}m", timeSpan.Minutes);
                            }
                        }

                        if (flightSegment.FlightSegment.MarketedFlightSegment != null && flightSegment.FlightSegment.MarketedFlightSegment.Count > 0)
                        {
                            flight.MarketingCarrier = flightSegment.FlightSegment.MarketedFlightSegment[0].MarketingAirlineCode;
                            flight.MarketingCarrierDescription = flightSegment.FlightSegment.MarketedFlightSegment[0].MarketingAirlineCode;
                        }
                        flight.OperatingCarrier = flightSegment.FlightSegment.OperatingAirlineCode;
                        flight.ServiceClassDescription = GetServiceClassDescriptionFromCslReservationFlightBookingClasses(flightSegment.FlightSegment.BookingClasses);
                        flight.EquipmentDisclosures = ConvertPNRSegmentEquipmentToMobShopEquipmentDisclousures(flightSegment.FlightSegment.Equipment);
                        flight.Meal = GetCharacteristicValue(flightSegment.Characteristic.ToList(), "MealDescription");
                        flight.Miles = flightSegment.FlightSegment.Distance.ToString();
                        flight.Messages = new List<MOBSHOPMessage>();

                        MOBSHOPMessage msg = new MOBSHOPMessage();
                        msg.MessageCode = flight.ServiceClassDescription;
                        flight.Messages.Add(msg);
                        msg = new MOBSHOPMessage();
                        msg.MessageCode = flight.Meal;
                        flight.Messages.Add(msg);
                        msg = new MOBSHOPMessage();
                        msg.MessageCode = "None";
                        flight.Messages.Add(msg);
                        if (flightSegment.FlightSegment.OperatingAirlineCode != "UA")
                        {
                            flight.OperatingCarrierDescription = flightSegment.FlightSegment.OperatingAirlineName;
                        }

                        mobShopFlattenedFlight.Flights.Add(flight);
                        if (flightSegment != null && flightSegment.FlightSegment != null && flightSegment.FlightSegment.Characteristic != null)
                        {
                            if (ConfigUtility.EnablePBE() && ConfigUtility.IsViewResFlowPaymentSvcEnabled(flow))
                            {
                                mobShopFlattenedFlight.IsIBE =
                                    ConfigUtility.IsIbeProductCode(flightSegment.FlightSegment.Characteristic);
                            }
                            else
                            {
                                mobShopFlattenedFlight.IsIBE = IsIbe(flightSegment.FlightSegment.Characteristic);
                            }
                            mobShopFlattenedFlight.IsElf = IsElf(flightSegment.FlightSegment.Characteristic);
                        }
                        currentFlightIndex++;
                    }
                }
                trip.FlattenedFlights.Add(mobShopFlattenedFlight);
            }
            return trip;
        }
        private bool isCOGorConnectionFlight(ReservationFlightSegment flightSegment)
        {
            if (flightSegment.IsNullOrEmpty())
            {
                return false;
            }
            bool isGaugeChange = string.IsNullOrEmpty(flightSegment.FlightSegment.IsChangeOfGauge) ? false : Convert.ToBoolean(flightSegment.FlightSegment.IsChangeOfGauge);
            bool isConnection = string.IsNullOrEmpty(flightSegment.IsConnection) ? false : Convert.ToBoolean(flightSegment.IsConnection);
            if ((isGaugeChange || isConnection) && flightSegment.FlightSegment.NumberofStops > 0 && flightSegment.Legs != null && flightSegment.Legs.Any())
            {
                return true;
            }
            return false;
        }

        private string GetRedEyeFlightArrDate(String flightDepart, String flightArrive, ref bool flightDateChanged)
        {
            try
            {

                DateTime depart = DateTime.MinValue;
                DateTime arrive = DateTime.MinValue;

                DateTime.TryParse(flightDepart, out depart);
                DateTime.TryParse(flightArrive, out arrive);

                int days = (arrive.Date - depart.Date).Days;

                if (days == 0)
                {
                    return string.Empty;
                }
                else if (days > 0)
                {
                    return arrive.ToString("ddd. MMM dd"); // Wed. May 20
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }

        private string GetRedEyeDepartureDate(String tripDate, String flightDepartureDate, ref bool flightDateChanged)
        {
            try
            {
                DateTime trip = DateTime.MinValue;
                DateTime departure = DateTime.MinValue;

                DateTime.TryParse(tripDate, out trip);
                DateTime.TryParse(flightDepartureDate, out departure);

                int days = (departure.Date - trip.Date).Days;

                if (days > 0)
                {
                    flightDateChanged = true; // Venkat - Showing Flight Date Change message is only for Departure date is different than Flight Search Date.
                    return departure.ToString("ddd. MMM dd"); // Wed. May 20                    
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }

        private string GetServiceClassDescriptionFromCslReservationFlightBookingClasses(Collection<BookingClass> bookingClasses)
        {
            string serviceClassDescription = string.Empty;
            if (bookingClasses != null && bookingClasses.Count > 0 && bookingClasses[0].Cabin != null)
            {
                serviceClassDescription = string.Format("{0} ({1})", bookingClasses[0].Cabin.Name, bookingClasses[0].Code);
            }
            return serviceClassDescription;
        }

        private MOBSHOPEquipmentDisclosure ConvertPNRSegmentEquipmentToMobShopEquipmentDisclousures(Service.Presentation.CommonModel.AircraftModel.Aircraft airCraft)
        {
            MOBSHOPEquipmentDisclosure mobShopEquipmentDisclosure = null;
            if (airCraft != null && airCraft.Model != null)
            {
                mobShopEquipmentDisclosure = new MOBSHOPEquipmentDisclosure();
                mobShopEquipmentDisclosure.EquipmentType = airCraft.Model.Fleet;
                mobShopEquipmentDisclosure.EquipmentDescription = airCraft.Model.Description;
                mobShopEquipmentDisclosure.WheelchairsNotAllowed = !string.IsNullOrEmpty(airCraft.Model.IsWheelchairAllowed);
                mobShopEquipmentDisclosure.NonJetEquipment = !string.IsNullOrEmpty(airCraft.Model.IsJetEquipment);
                mobShopEquipmentDisclosure.NoBoardingAssistance = !string.IsNullOrEmpty(airCraft.Model.HasBoardingAssistance);
                mobShopEquipmentDisclosure.IsSingleCabin = !string.IsNullOrEmpty(airCraft.Model.IsSingleCabin);
            }
            return mobShopEquipmentDisclosure;
        }

        private string GetCharacteristicValue(List<Characteristic> characteristics, string code)
        {
            string keyValue = string.Empty;
            if (characteristics.Exists(p => p.Code == code))
            {
                keyValue = characteristics.First(p => p.Code == code).Value;
            }
            return keyValue;
        }

        private bool IsElf(Collection<Characteristic> characteristics)
        {
            return _configuration.GetValue<bool>("EnableIBE") &&
                    characteristics != null &&
                    characteristics.Any(c => c != null &&
                                            "PRODUCTCODE".Equals(c.Code, StringComparison.OrdinalIgnoreCase) &&
                                            "ELF".Equals(c.Value, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsIbe(Collection<Characteristic> characteristics)
        {
            return _configuration.GetValue<bool>("EnableIBE") &&
                    characteristics != null &&
                    characteristics.Any(c => c != null &&
                                            "PRODUCTCODE".Equals(c.Code, StringComparison.OrdinalIgnoreCase) &&
                                            "IBE".Equals(c.Value, StringComparison.OrdinalIgnoreCase));
        }

        public List<MOBCPTraveler> GetTravelerCSLDetails(United.Service.Presentation.ReservationModel.Reservation reservation, List<MOBSHOPTrip> shoptrips, string sessionId, string flow)
        {
            if (reservation == null || shoptrips == null || !reservation.Travelers.Any() || !reservation.FlightSegments.Any() || !shoptrips.Any())
                return null;

            var trips = GetTripBase(shoptrips);
            if (trips == null || !trips.Any())
                return null;

            int paxIndex = 0;
            string departDate = trips.FirstOrDefault(k => !k.IsNullOrEmpty()).DepartDate;

            List<MOBCPTraveler> travelerCSL = new List<MOBCPTraveler>();
            reservation.Travelers.ForEach(p =>
            {
                if (p != null && p.Person != null)
                {

                    MOBCPTraveler mobCPTraveler = MapCslPersonToMOBCPTravel(p, paxIndex, reservation.FlightSegments, trips, flow);
                    mobCPTraveler.Phones = GetMobCpPhones(p.Person.Contact);
                    mobCPTraveler.FirstName = FirstLetterToUpperCase(mobCPTraveler.FirstName);
                    mobCPTraveler.LastName = FirstLetterToUpperCase(mobCPTraveler.LastName);
                    if (!mobCPTraveler.BirthDate.IsNullOrEmpty() && !departDate.IsNullOrEmpty())
                    {
                        mobCPTraveler.PTCDescription = GeneralHelper.GetPaxDescriptionByDOB(mobCPTraveler.BirthDate.ToString(), departDate);
                    }
                    travelerCSL.Add(mobCPTraveler);
                    paxIndex++;
                }
            });

            return travelerCSL;
        }

        public MOBCPTraveler MapCslPersonToMOBCPTravel(Service.Presentation.ReservationModel.Traveler cslTraveler, int paxIndex, Collection<ReservationFlightSegment> flightSegments, List<MOBSHOPTripBase> trips, string flow)
        {
            var mobCPTraveler = new MOBCPTraveler();
            Service.Presentation.PersonModel.Person person = cslTraveler.Person;
            mobCPTraveler.Key = person.Key;
            mobCPTraveler.PaxIndex = paxIndex;
            mobCPTraveler.BirthDate = Convert.ToDateTime(person.DateOfBirth).ToShortDateString();
            mobCPTraveler.GenderCode = person.Sex;
            mobCPTraveler.FirstName = person.GivenName;
            mobCPTraveler.MiddleName = person.MiddleName;
            mobCPTraveler.LastName = person.Surname;
            mobCPTraveler.KnownTravelerNumber = string.Empty;
            mobCPTraveler.Message = string.Empty;
            mobCPTraveler.MileagePlus = null;
            mobCPTraveler.TravelerNameIndex = person.Key;

            if (ConfigUtility.IsViewResFlowPaymentSvcEnabled(flow))
            {
                mobCPTraveler.CslReservationPaxTypeCode = person.PricingPaxType;
                mobCPTraveler.TravelerTypeCode = person.Type;
                mobCPTraveler.PNRCustomerID = person.CustomerID;
            }
            if (trips != null && trips.Any())
            {
                string DeptDateOfFLOF = trips[0].DepartDate;

                if (!string.IsNullOrEmpty(cslTraveler.Person.Type)
                    && cslTraveler.Person.Type.ToUpper().Equals("INF")
                    && !string.IsNullOrEmpty(cslTraveler.Person.DateOfBirth)
                    && TopHelper.GetAgeByDOB(cslTraveler.Person.DateOfBirth, DeptDateOfFLOF) < 2)
                    mobCPTraveler.IsEligibleForSeatSelection = false;
                else
                    mobCPTraveler.IsEligibleForSeatSelection = true;
            }

            mobCPTraveler.Seats = new List<MOBSeat>();

            if (cslTraveler.LoyaltyProgramProfile != null && !string.IsNullOrEmpty(cslTraveler.LoyaltyProgramProfile.LoyaltyProgramMemberID))
            {
                mobCPTraveler.MileagePlus = new MOBCPMileagePlus();
                int currentLevel = 0;
                mobCPTraveler.MileagePlus.MileagePlusId = cslTraveler.LoyaltyProgramProfile.LoyaltyProgramMemberID;
                mobCPTraveler.MileagePlus.CurrentEliteLevelDescription = GetLoyalityStatus(Convert.ToString(cslTraveler.LoyaltyProgramProfile.LoyaltyProgramMemberTierDescription), out currentLevel);
                mobCPTraveler.MileagePlus.CurrentEliteLevel = currentLevel;

                //For integration testing added these 3 lines to match with Onprem response
                mobCPTraveler.MileagePlus.TravelBankExpirationDate = mobCPTraveler.MileagePlus.TravelBankExpirationDate == string.Empty ? null : mobCPTraveler.MileagePlus.TravelBankExpirationDate;
                mobCPTraveler.MileagePlus.VendorCode = mobCPTraveler.MileagePlus.VendorCode == string.Empty ? null : mobCPTraveler.MileagePlus.VendorCode;
                mobCPTraveler.MileagePlus.TravelBankCurrencyCode = mobCPTraveler.MileagePlus.TravelBankCurrencyCode == string.Empty ? null : mobCPTraveler.MileagePlus.TravelBankCurrencyCode;

                mobCPTraveler.AirRewardPrograms = new List<MOBBKLoyaltyProgramProfile>();

                MOBBKLoyaltyProgramProfile airRewardProgram = new MOBBKLoyaltyProgramProfile();
                airRewardProgram.MemberId = cslTraveler.LoyaltyProgramProfile.LoyaltyProgramMemberID;
                airRewardProgram.CarrierCode = cslTraveler.LoyaltyProgramProfile.LoyaltyProgramCarrierCode;
                if (airRewardProgram.CarrierCode.Trim().Equals("UA"))
                {
                    airRewardProgram.MPEliteLevel = currentLevel;
                }
                mobCPTraveler.AirRewardPrograms.Add(airRewardProgram);
            }

            foreach (var fs in flightSegments)
            {
                foreach (var cs in fs.CurrentSeats)
                {
                    if (cs.ReservationNameIndex == person.Key)
                    {
                        int tripno = Convert.ToInt32(fs.TripNumber);
                        if (trips.Count() < tripno)
                            tripno--;
                        MOBSeat mobseat = new MOBSeat();
                        if (trips[tripno - 1].ChangeType != MOBSHOPTripChangeType.ChangeFlight)
                        {
                            if (mobCPTraveler.IsEligibleForSeatSelection)
                            {
                                mobseat.SeatAssignment = cs.Seat.Identifier.Replace("*", "").Trim('0');
                                mobseat.SeatType = cs.Seat.SeatType;
                            }
                            mobseat.TravelerSharesIndex = person.Key;
                            mobseat.Destination = fs.FlightSegment.ArrivalAirport.IATACode;
                            mobseat.Origin = fs.FlightSegment.DepartureAirport.IATACode;
                            mobseat.ProgramCode = cs.ProgramCode;
                            mobseat.FlightNumber = fs.FlightSegment.FlightNumber;
                            mobCPTraveler.Seats.Add(mobseat);
                        }
                    }
                }
            }

            if (person.Contact != null && person.Contact.Emails != null && person.Contact.Emails.Count > 0)
            {
                mobCPTraveler.EmailAddresses = GetMobEmails(person.Contact.Emails);
                mobCPTraveler.ReservationEmailAddresses = mobCPTraveler.EmailAddresses;
            }

            return mobCPTraveler;
        }

        private string GetLoyalityStatus(string memberType, out int level)
        {
            switch (memberType)
            {
                case "PremierGold":
                    level = 2;
                    return "Premier Gold";
                case "PremierSilver":
                    level = 1;
                    return "Premier Silver";
                case "PremierPlatinum":
                    level = 3;
                    return "Premier Platium";
                case "Premier1K":
                    level = 4;
                    return "Premier 1K";
                case "GlobalServices":
                    level = 5;
                    return "Global Services";
            }
            level = 0;
            return "General Member";
        }

        private List<MOBEmail> GetMobEmails(Collection<EmailAddress> pnrEmailAddress)
        {
            var mobEmails = new List<MOBEmail>();
            if (pnrEmailAddress != null)
            {
                foreach (var pnremail in pnrEmailAddress)
                {
                    var mobEmail = new MOBEmail();
                    mobEmail.EmailAddress = pnremail.Address.ToLower();
                    mobEmails.Add(mobEmail);
                }
            }
            return mobEmails;
        }

        public string FirstLetterToUpperCase(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            if (value.Length == 1)
                return value[0].ToString().ToUpper();

            return value[0].ToString().ToUpper() + value.Substring(1).ToLower();
        }

        public List<MOBCPPhone> GetMobCpPhones(United.Service.Presentation.PersonModel.Contact contact)
        {
            if (contact.IsNullOrEmpty() || contact.PhoneNumbers.IsListNullOrEmpty())
                return null;

            var mobCpPhones = new List<MOBCPPhone>();

            foreach (var pnrPhone in contact.PhoneNumbers)
            {
                if (!pnrPhone.IsNullOrEmpty() && pnrPhone.CountryAccessCode.Equals("US"))
                {
                    var mobPhone = new MOBCPPhone();
                    var phoneNum = !pnrPhone.PhoneNumber.IsNullOrEmpty() ? pnrPhone.PhoneNumber : string.Empty;
                    if (phoneNum.Length == 11)
                    {
                        mobPhone.AreaNumber = phoneNum.Substring(1, 3);
                        mobPhone.PhoneNumber = Regex.Replace(phoneNum.Substring(4), @"[^0-9]+", "");
                        mobPhone.CountryCode = pnrPhone.CountryAccessCode;
                        mobPhone.ChannelCodeDescription = pnrPhone.CountryAccessCode;
                        mobCpPhones.Add(mobPhone);
                    }
                }
            }

            return mobCpPhones;
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

        public async Task<MOBULTripInfo> GetUpliftTripInfo(United.Service.Presentation.ReservationModel.Reservation reservation, string totalPrice, List<MOBProdDetail> products, bool isGetCartInfo = false)
        {
            if (!HasEligibleProductsForUplift(totalPrice, products) && !isGetCartInfo)
                return null;

            double.TryParse(totalPrice, out double price);
            new Dictionary<string, string>() { { "ONEWAY", "OW" }, { "ROUNDTRIP", "RT" }, { "MULTICITY", "MD" } }.TryGetValue(UtilityHelper.GetJourneyTypeDescription(reservation)?.ToUpper(), out string tripType);

            bool isEnableFilterExtraSeat = await _shoppingCartUtility.IsExtraSeatCheckEnableForUpliftMR();
            var extraSeatPassengerIndex = _shoppingCartUtility.GetTravelerNameIndexForExtraSeat(isEnableFilterExtraSeat, reservation?.Services);

            return new MOBULTripInfo
            {
                AirReservations = new List<MOBULAirReservation>
                {
                    new MOBULAirReservation
                    {
                        Itineraries = reservation.FlightSegments.Select( f =>
                        new MOBULItinerary{
                            ArrivalTime = DateTime.Parse( f.FlightSegment.ArrivalDateTime).ToString("MM/dd/yyyy h:mm tt"),
                            DepartureTime = DateTime.Parse(  f.FlightSegment.DepartureDateTime).ToString("MM/dd/yyyy h:mm tt"),
                            CarrierCode = f.FlightSegment.OperatingAirlineCode,
                            Origin = f.FlightSegment.DepartureAirport.IATACode,
                            OriginDescription = f.FlightSegment.DepartureAirport.Name,
                            Destination = f.FlightSegment.ArrivalAirport.IATACode,
                            DestinationDescription = f.FlightSegment.ArrivalAirport.Name,
                            FareClass = f.FlightSegment.BookingClasses?.FirstOrDefault().Cabin.Name
                        }).ToList(),
                        Pnr =reservation.ConfirmationID,
                        Price = Convert.ToInt32(price * 100),
                        ReservationType = "standard",
                        TripType = tripType,
                        Origin = reservation.FlightSegments?.FirstOrDefault()?.FlightSegment?.DepartureAirport?.IATACode,
                        Destination = reservation.FlightSegments?.LastOrDefault(f => f?.TripNumber == reservation.FlightSegments?.FirstOrDefault()?.TripNumber)?.FlightSegment?.ArrivalAirport?.IATACode
                    }
                },
                OrderAmount = Convert.ToInt32(price * 100),
                Travelers = GetUpLiftTravelerDetails(reservation, extraSeatPassengerIndex, isEnableFilterExtraSeat),
                OrderLines = products.Select(p =>
                new MOBULOrderLine
                {
                    Name = p.ProdDescription,
                    Price = Convert.ToInt32(p.ProdTotalPrice.ToDecimal() * 100)
                }).ToList(),
                Email = reservation.Travelers.FirstOrDefault()?.Person?.Contact?.Emails?.FirstOrDefault()?.Address,
                PhoneNumber = _shoppingCartUtility.ExcludeCountryCodeFrmPhoneNumber(reservation.Travelers.FirstOrDefault()?.Person?.Contact?.PhoneNumbers?.FirstOrDefault()?.PhoneNumber, await _shoppingCartUtility.GetCountryCode(reservation.Travelers.FirstOrDefault()?.Person?.Contact?.PhoneNumbers?.FirstOrDefault()?.CountryAccessCode))
            };
        }

        private List<MOBULTraveler> GetUpLiftTravelerDetails(Service.Presentation.ReservationModel.Reservation reservation, List<string> extraSeatPassengerIndex, bool isEnableFilterExtraSeat)
        {
            if (isEnableFilterExtraSeat && extraSeatPassengerIndex != null && extraSeatPassengerIndex.Count > 0)
            {
                return reservation.Travelers.Where(x => !string.IsNullOrEmpty(x?.Person?.Key) && !extraSeatPassengerIndex.Contains(x?.Person?.Key)).Select((t, index) =>
                               new MOBULTraveler
                               {
                                   Index = index,
                                   FirstName = t.Person.GivenName,
                                   LastName = t.Person.Surname,
                                   DateOfBirth = DateTime.Parse(t.Person.DateOfBirth).ToString("MM/dd/yyyy")
                               }).ToList();
            }
            else
            {
                return reservation.Travelers.Select((t, index) =>
                               new MOBULTraveler
                               {
                                   Index = index,
                                   FirstName = t.Person.GivenName,
                                   LastName = t.Person.Surname,
                                   DateOfBirth = DateTime.Parse(t.Person.DateOfBirth).ToString("MM/dd/yyyy")
                               }).ToList();
            }
        }

        public async Task<MOBULTripInfo> GetUpliftTripInfo(FlightReservationResponse flightReservationResponse, string totalPrice, List<MOBProdDetail> products, bool isGetCartInfo = false)
        {
            if (!HasEligibleProductsForUplift(totalPrice, products) && !isGetCartInfo)
                return null;

            double.TryParse(totalPrice, out double price);
            new Dictionary<string, string>() { { "ONEWAY", "OW" }, { "ROUNDTRIP", "RT" }, { "MULTICITY", "MD" } }.TryGetValue(UtilityHelper.GetJourneyTypeDescription(flightReservationResponse.Reservation)?.ToUpper(), out string tripType);
            return new MOBULTripInfo
            {
                AirReservations = new List<MOBULAirReservation>
                {
                    new MOBULAirReservation
                    {
                        Itineraries = flightReservationResponse.Reservation.FlightSegments.Select( f =>
                        new MOBULItinerary{
                            ArrivalTime = DateTime.Parse( f.FlightSegment.ArrivalDateTime).ToString("MM/dd/yyyy h:mm tt"),
                            DepartureTime = DateTime.Parse(  f.FlightSegment.DepartureDateTime).ToString("MM/dd/yyyy h:mm tt"),
                            CarrierCode = f.FlightSegment.OperatingAirlineCode,
                            Origin = f.FlightSegment.DepartureAirport.IATACode,
                            OriginDescription = f.FlightSegment.DepartureAirport.Name,
                            Destination = f.FlightSegment.ArrivalAirport.IATACode,
                            DestinationDescription = f.FlightSegment.ArrivalAirport.Name,
                            FareClass = f.FlightSegment.BookingClasses?.FirstOrDefault().Cabin.Name
                        }).ToList(),
                        Pnr = flightReservationResponse.Reservation.ConfirmationID,
                        Price = Convert.ToInt32(price * 100),
                        ReservationType = "standard",
                        TripType = tripType,
                        Origin = flightReservationResponse.Reservation.FlightSegments?.FirstOrDefault()?.FlightSegment?.DepartureAirport?.IATACode,
                        Destination = flightReservationResponse.Reservation.FlightSegments?.LastOrDefault(f => f?.TripNumber == flightReservationResponse.Reservation.FlightSegments?.FirstOrDefault()?.TripNumber)?.FlightSegment?.ArrivalAirport?.IATACode
                    }
                },
                OrderAmount = Convert.ToInt32(price * 100),
                Travelers = flightReservationResponse.Reservation.Travelers.Select((t, index) =>
               new MOBULTraveler
               {
                   Index = index,
                   FirstName = t.Person.GivenName,
                   LastName = t.Person.Surname,
                   DateOfBirth = DateTime.Parse(t.Person.DateOfBirth).ToString("MM/dd/yyyy")
               }).ToList(),
                OrderLines = products.Select(p =>
                new MOBULOrderLine
                {
                    Name = p.ProdDescription,
                    Price = Convert.ToInt32(p.ProdTotalPrice.ToDecimal() * 100)
                }).ToList(),
                Email = flightReservationResponse.Reservation.Travelers.FirstOrDefault()?.Person?.Contact?.Emails?.FirstOrDefault()?.Address,
                PhoneNumber = _shoppingCartUtility.ExcludeCountryCodeFrmPhoneNumber(flightReservationResponse.Reservation.Travelers.FirstOrDefault()?.Person?.Contact?.PhoneNumbers?.FirstOrDefault()?.PhoneNumber, await _shoppingCartUtility.GetCountryCode(flightReservationResponse.Reservation.Travelers.FirstOrDefault()?.Person?.Contact?.PhoneNumbers?.FirstOrDefault()?.CountryAccessCode))
            };
        }
    }
}
