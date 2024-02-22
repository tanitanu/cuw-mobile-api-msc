using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using United.Definition;
using United.Definition.FormofPayment;
using United.Definition.Shopping;
using United.Mobile.Model.Common;
using United.Persist.Definition.Shopping;
using United.Service.Presentation.PaymentModel;
using United.Services.FlightShopping.Common.FlightReservation;
using United.Utility.Helper;
using MOBItem = United.Mobile.Model.Common.MOBItem;
using WorkFlowType = United.Services.FlightShopping.Common.FlightReservation.WorkFlowType;
using CslDataVaultRequest = United.Service.Presentation.PaymentRequestModel.DataVault<United.Service.Presentation.PaymentModel.Payment>;
using CslDataVaultResponse = United.Service.Presentation.PaymentResponseModel.DataVault<United.Service.Presentation.PaymentModel.Payment>;
using Newtonsoft.Json;
using System.Net;
using United.Service.Presentation.CommonModel;
using System.Collections.ObjectModel;

namespace United.Common.Helper
{
    public class ConfigUtility : IConfigUtility
    {
        private static IConfiguration _configuration { get; set; }

        public ConfigUtility(IConfiguration configuration)
        {
            ConfigUtility._configuration = configuration;
        }
        public static void UtilityInitialize(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static bool IsSeatMapSupportedOa(string operatingCarrier, string MarketingCarrier)
        {
            if (string.IsNullOrEmpty(operatingCarrier)) return false;
            var seatMapSupportedOa = _configuration.GetValue<string>("SeatMapSupportedOtherAirlines");
            if (string.IsNullOrEmpty(seatMapSupportedOa)) return false;

            var seatMapEnabledOa = seatMapSupportedOa.Split(',');
            if (seatMapEnabledOa.Any(s => s == operatingCarrier.ToUpper().Trim()))
                return true;
            else if (_configuration.GetValue<string>("SeatMapSupportedOtherAirlinesMarketedBy") != null)
            {
                return _configuration.GetValue<string>("SeatMapSupportedOtherAirlinesMarketedBy").Split(',').ToList().Any(m => m == MarketingCarrier + "-" + operatingCarrier);
            }

            return false;
        }

        public static bool IsETCEnabledforMultiTraveler(int applicationId, string appVersion)
        {
            if (_configuration.GetValue<bool>("MTETCToggle") && GeneralHelper.IsApplicationVersionGreaterorEqual(applicationId, appVersion, _configuration.GetValue<string>("iPhone_EnableETCForMultiTraveler_AppVersion"), _configuration.GetValue<string>("Android_EnableETCForMultiTraveler_AppVersion")))
            {
                return true;
            }
            return false;
        }

        public static bool IncludeFFCResidual(int appId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnableFFCResidual")
                && GeneralHelper.IsApplicationVersionGreater
                (appId, appVersion, "AndroidFFCResidualVersion", "iPhoneFFCResidualVersion", "", "", true, _configuration);
        }

        public static bool EnablePreferredZone(int appId, string appVersion)
        {
            if (!string.IsNullOrEmpty(appVersion) && appId != -1)
            {
                return _configuration.GetValue<bool>("isEnablePreferredZone")
               && GeneralHelper.IsApplicationVersionGreater(appId, appVersion, "AndroidPreferredSeatVersion", "iOSPreferredSeatVersion", "", "", true, _configuration);
            }
            return false;
        }

        public static bool IsUPPSeatMapSupportedVersion(int appId, string appVersion)
        {
            if (!string.IsNullOrEmpty(appVersion) && appId != -1)
            {
                return _configuration.GetValue<bool>("EnableUPPSeatmap")
                    && GeneralHelper.IsApplicationVersionGreater(appId, appVersion, "AndroidUPPSeatmapVersion", "iPhoneUPPSeatmapVersion", "", "", true, _configuration);
            }

            return false;
        }

        public static bool OaSeatMapExceptionVersion(int applicationId, string appVersion)
        {
            return GeneralHelper.IsApplicationVersionGreater(applicationId, appVersion, "AndroidOaSeatMapExceptionVersion", "iPhoneOaSeatMapExceptionVersion", "", "", true, _configuration);
        }

        public static bool IsIBE(Reservation persistedReservation)
        {
            if (_configuration.GetValue<bool>("EnablePBE") && (persistedReservation.ShopReservationInfo2 != null))
            {
                return persistedReservation.ShopReservationInfo2.IsIBE;
            }
            return false;
        }


        public static double GetGrandTotalPriceForShoppingCart(bool isCompleteFarelockPurchase, Services.FlightShopping.Common.FlightReservation.FlightReservationResponse flightReservationResponse, bool isPost, string flow = "VIEWRES")
        {
            //Added Null check for price.Since,CSL is not sending the price when we select seat of price zero.
            //Added condition to check whether Total in the price exists(This is for scenario when we register the bundle after registering the seat).
            //return isCompleteFarelockPurchase ? Convert.ToDouble(flightReservationResponse.DisplayCart.DisplayPrices.FirstOrDefault(o => (o.Description != null && o.Description.Equals("GrandTotal", StringComparison.OrdinalIgnoreCase))).Amount)
            //                                  : (Utility.IsCheckinFlow(flow) || flow == FlowType.VIEWRES.ToString()) ? flightReservationResponse.ShoppingCart.Items.Where(x => x.Product.FirstOrDefault().Code != "RES" && (x.Product.FirstOrDefault().Price != null ? (x.Product.FirstOrDefault().Price.Totals.Any()) : false)).Select(x => x.Product.FirstOrDefault().Price.Totals.FirstOrDefault().Amount).ToList().Sum() : flightReservationResponse.ShoppingCart.Items.SelectMany(x => x.Product).Where(x => x.Price != null).SelectMany(x => x.Price.Totals).Where(x => (x.Name != null ? x.Name.ToUpper() == "GrandTotalForCurrency".ToUpper() : true)).Select(x => x.Amount).ToList().Sum();
            double shoppingCartTotalPrice = 0.0;
            double closeBookingFee = 0.0;
            if (isCompleteFarelockPurchase)
                shoppingCartTotalPrice = Convert.ToDouble(flightReservationResponse.DisplayCart.DisplayPrices.FirstOrDefault(o => (o.Description != null && o.Description.Equals("GrandTotal", StringComparison.OrdinalIgnoreCase))).Amount);
            else
            {
                if (isPost ? flightReservationResponse.CheckoutResponse.ShoppingCartResponse.Items.Select(x => x.Item).Any(x => x.Product.FirstOrDefault().Code == "FLK")
                        : flightReservationResponse.ShoppingCart.Items.Any(x => x.Product.FirstOrDefault().Code == "FLK"))
                    flow = FlowType.FARELOCK.ToString();
                //[MB-6519]:Getting Sorry Something went wrong for Award Booking for with Reward booking fee and reservation price is zero
                if (_configuration.GetValue<bool>("CFOP19HBugFixToggle") && (isPost ? flightReservationResponse.CheckoutResponse.ShoppingCartResponse.Items.Select(x => x.Item).Any(x => x.Product.FirstOrDefault().Code == "RBF")
                                                                            : flightReservationResponse.ShoppingCart.Items.Any(x => x.Product.FirstOrDefault().Code == "RBF")))

                {
                    United.Service.Presentation.InteractionModel.ShoppingCart flightReservationResponseShoppingCart = new United.Service.Presentation.InteractionModel.ShoppingCart();
                    flightReservationResponseShoppingCart = isPost ? flightReservationResponse.CheckoutResponse.ShoppingCart : flightReservationResponse.ShoppingCart;
                    closeBookingFee = UtilityHelper.GetCloseBookingFee(isPost, flightReservationResponseShoppingCart, flow);

                }

                switch (flow)
                {
                    case "BOOKING":
                    case "RESHOP":
                        shoppingCartTotalPrice = isPost ? flightReservationResponse.CheckoutResponse.ShoppingCart.Items.Where(x => !UtilityHelper.CheckFailedShoppingCartItem(flightReservationResponse, x)).SelectMany(x => x.Product).Where(x => x.Price != null).SelectMany(x => x.Price.Totals).Where(x => (x.Name != null ? x.Name.ToUpper() == "GrandTotalForCurrency".ToUpper() /*|| x.Name.ToUpper() == "Close-In Booking Fee".ToUpper()*/ : true)).Select(x => x.Amount).ToList().Sum()
                            : flightReservationResponse.ShoppingCart.Items.SelectMany(x => x.Product).Where(x => x.Price != null).SelectMany(x => x.Price.Totals).Where(x => (x.Name != null ? x.Name.ToUpper() == "GrandTotalForCurrency".ToUpper() /*|| x.Name.ToUpper() == "Close-In Booking Fee".ToUpper()*/ : true)).Select(x => x.Amount).ToList().Sum();
                        break;

                    case "POSTBOOKING":
                        //productCodes = flightReservationResponseShoppingCart.Items.SelectMany(x => x.Product).Where(x => x.Characteristics != null && (x.Characteristics.Any(y => y.Description.ToUpper() == "POSTPURCHASE" && Convert.ToBoolean(y.Value) == true))).Select(x => x.Code).ToList();
                        shoppingCartTotalPrice = isPost ? flightReservationResponse.CheckoutResponse.ShoppingCart.Items.Where(x => !UtilityHelper.CheckFailedShoppingCartItem(flightReservationResponse, x)).Where(x => (x.Product.FirstOrDefault().Price != null ? (x.Product.FirstOrDefault().Price.Totals.Any()) : false) && x.Product.FirstOrDefault().Characteristics != null && (x.Product.FirstOrDefault().Characteristics.Any(y => y.Description == "PostPurchase" && Convert.ToBoolean(y.Value) == true))).Select(x => x.Product.FirstOrDefault().Price.Totals.FirstOrDefault().Amount).ToList().Sum()
                            : flightReservationResponse.ShoppingCart.Items.Where(x => (x.Product.FirstOrDefault().Price != null ? (x.Product.FirstOrDefault().Price.Totals.Any()) : false) && x.Product.FirstOrDefault().Characteristics != null && (x.Product.FirstOrDefault().Characteristics.Any(y => y.Description == "PostPurchase" && Convert.ToBoolean(y.Value) == true))).Select(x => x.Product.FirstOrDefault().Price.Totals.FirstOrDefault().Amount).ToList().Sum(); ;
                        break;

                    case "FARELOCK":
                        shoppingCartTotalPrice = isPost ? flightReservationResponse.CheckoutResponse.ShoppingCart.Items.Where(x => !UtilityHelper.CheckFailedShoppingCartItem(flightReservationResponse, x)).Where(x => x.Product.FirstOrDefault().Code == "FLK" && (x.Product.FirstOrDefault().Price != null ? (x.Product.FirstOrDefault().Price.Totals.Any()) : false)).Select(x => x.Product.FirstOrDefault().Price.Totals.FirstOrDefault().Amount).ToList().Sum()
                            : flightReservationResponse.ShoppingCart.Items.Where(x => x.Product.FirstOrDefault().Code == "FLK" && (x.Product.FirstOrDefault().Price != null ? (x.Product.FirstOrDefault().Price.Totals.Any()) : false)).Select(x => x.Product.FirstOrDefault().Price.Totals.FirstOrDefault().Amount).ToList().Sum();
                        break;

                    case "VIEWRES":
                    case "CHECKIN":
                        shoppingCartTotalPrice = isPost ? flightReservationResponse.CheckoutResponse.ShoppingCart.Items.Where(x => !UtilityHelper.CheckFailedShoppingCartItem(flightReservationResponse, x)).Where(x => x.Product.FirstOrDefault().Code != "RES" && (x.Product.FirstOrDefault().Price != null ? (x.Product.FirstOrDefault().Price.Totals.Any()) : false)).Select(x => x.Product.FirstOrDefault().Price.Totals.FirstOrDefault().Amount).ToList().Sum()
                            : flightReservationResponse.ShoppingCart.Items.Where(x => x.Product.FirstOrDefault().Code != "RES" && (x.Product.FirstOrDefault().Price != null ? (x.Product.FirstOrDefault().Price.Totals.Any()) : false)).Select(x => x.Product.FirstOrDefault().Price.Totals.FirstOrDefault().Amount).ToList().Sum();
                        break;
                }
            }
            if (_configuration.GetValue<bool>("CFOP19HBugFixToggle"))
            {
                shoppingCartTotalPrice = shoppingCartTotalPrice + closeBookingFee;
            }
            return shoppingCartTotalPrice;
        }

        public static bool OaSeatMapSupportedVersion(int applicationId, string appVersion, string carrierCode, string MarketingCarrier = "")
        {
            var supportedOA = false;
            if (IsSeatMapSupportedOa(carrierCode, MarketingCarrier))
            {
                switch (carrierCode)
                {
                    case "AC":
                        {
                            supportedOA = EnableAirCanada(applicationId, appVersion);
                            break;
                        }
                    default:
                        {
                            supportedOA = GeneralHelper.IsApplicationVersionGreater(applicationId, appVersion, "AndroidOaSeatMapVersion", "iPhoneOaSeatMapVersion", "", "", true, _configuration);
                            break;
                        }
                }
            }
            return supportedOA;
        }

        public static bool EnableAirCanada(int appId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnableAirCanada")
                    && GeneralHelper.IsApplicationVersionGreater(appId, appVersion, "AndroidAirCanadaVersion", "iPhoneAirCanadaVersion", "", "", true, _configuration);
        }

        public static bool EnableTravelerTypes(int appId, string appVersion, bool reshop = false)
        {
            if (!string.IsNullOrEmpty(appVersion) && appId != -1)
            {
                return _configuration.GetValue<bool>("EnableTravelerTypes") && !reshop
               && GeneralHelper.IsApplicationVersionGreater(appId, appVersion, "AndroidTravelerTypesVersion", "iPhoneTravelerTypesVersion", "", "", true, _configuration);
            }
            return false;
        }

        public static bool IsETCchangesEnabled(int applicationId, string appVersion)
        {
            if (_configuration.GetValue<bool>("ETCToggle") && GeneralHelper.IsApplicationVersionGreaterorEqual(applicationId, appVersion, _configuration.GetValue<string>("iPhone_ETC_AppVersion"), _configuration.GetValue<string>("Android_ETC_AppVersion")))
            {
                return true;
            }
            return false;
        }

        public static bool EnableIBEFull()
        {
            return _configuration.GetValue<bool>("EnableIBE");
        }

        public static bool IsEnableOmniCartMVP2Changes(int applicationId, string appVersion, bool isDisplayCart)
        {
            if (_configuration.GetValue<bool>("EnableOmniCartMVP2Changes") && GeneralHelper.IsApplicationVersionGreaterorEqual(applicationId, appVersion, _configuration.GetValue<string>("Android_EnableOmniCartMVP2Changes_AppVersion"), _configuration.GetValue<string>("iPhone_EnableOmniCartMVP2Changes_AppVersion")) && isDisplayCart)
            {
                return true;
            }
            return false;
        }

        public static bool EnableSpecialNeeds(int appId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnableSpecialNeeds")
                    && GeneralHelper.IsApplicationVersionGreater(appId, appVersion, "AndroidEnableSpecialNeedsVersion", "iPhoneEnableSpecialNeedsVersion", "", "", true, _configuration);
        }

        public static bool EnableInflightContactlessPayment(int appID, string appVersion, bool isReshop = false)
        {
            return _configuration.GetValue<bool>("EnableInflightContactlessPayment") && !isReshop && GeneralHelper.IsApplicationVersionGreaterorEqual(appID, appVersion, _configuration.GetValue<string>("InflightContactlessPaymentAndroidVersion"), _configuration.GetValue<string>("InflightContactlessPaymentiOSVersion"));
        }

        public static bool EnableUnfinishedBookings(MOBRequest request)
        {
            return _configuration.GetValue<bool>("EnableUnfinishedBookings")
                    && GeneralHelper.IsApplicationVersionGreater(request.Application.Id, request.Application.Version.Major, "AndroidEnableUnfinishedBookingsVersion", "iPhoneEnableUnfinishedBookingsVersion", "", "", true, _configuration);
        }

        public static bool EnableSavedTripShowChannelTypes(int appId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnableUnfinishedBookings") // feature toggle
                    && GeneralHelper.IsApplicationVersionGreater(appId, appVersion, "AndroidEnableUnfinishedBookingsVersion", "iPhoneEnableUnfinishedBookingsVersion", "", "", true, _configuration)

                    && _configuration.GetValue<bool>("EnableSavedTripShowChannelTypes") // story toggle
                    && GeneralHelper.IsApplicationVersionGreater(appId, appVersion, "AndroidEnableSavedTripShowChannelTypesVersion", "iPhoneEnableSavedTripShowChannelTypesVersion", "", "", true, _configuration);
        }

        public static bool EnableYADesc(bool isReshop = false)
        {
            return _configuration.GetValue<bool>("EnableYoungAdultBooking") && _configuration.GetValue<bool>("EnableYADesc") && !isReshop;
        }

        public static bool IsETCCombinabilityEnabled(int applicationId, string appVersion)
        {
            if (_configuration.GetValue<bool>("CombinebilityETCToggle") && GeneralHelper.IsApplicationVersionGreaterorEqual(applicationId, appVersion, _configuration.GetValue<string>("Android_EnableETCCombinability_AppVersion"), _configuration.GetValue<string>("iPhone_EnableETCCombinability_AppVersion")))
            {
                return true;
            }

            return false;
        }

        public static void AssignIsOtherFOPRequired(MOBFormofPaymentDetails formofPaymentDetails, List<MOBSHOPPrice> prices, bool IsSecondaryFOP = false, bool isRemoveAll = false)
        {
            var grandTotalPrice = prices.FirstOrDefault(p => p.DisplayType.ToUpper().Equals("GRAND TOTAL"));
            //if(grandTotalPrice == null)
            //{
            //    var totalPrice = prices.FirstOrDefault(p => p.DisplayType.ToUpper().Equals("TOTAL"));
            //    grandTotalPrice = BuildGrandTotalPriceForReservation(totalPrice.Value);
            //    prices.Add(grandTotalPrice);
            //}
            formofPaymentDetails.IsOtherFOPRequired = (grandTotalPrice.Value > 0);

            //need to update only when travelcertificate is added as formofpayment.
            //Need to update formofpaymentype only when travel certificate is not added as other fop or all the certficates are removed
            if (formofPaymentDetails?.TravelCertificate?.Certificates?.Count > 0 || isRemoveAll)
            {
                if (!formofPaymentDetails.IsOtherFOPRequired && !IsSecondaryFOP)
                {
                    formofPaymentDetails.FormOfPaymentType = MOBFormofPayment.ETC.ToString();
                    if (!_configuration.GetValue<bool>("DisableBugMOBILE9122Toggle") &&
                        !string.IsNullOrEmpty(formofPaymentDetails.CreditCard?.Message) &&
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

        public static MOBSHOPPrice UpdateCertificatePrice(MOBSHOPPrice certificatePrice, double totalAmount)
        {
            certificatePrice.CurrencyCode = "USD";
            certificatePrice.DisplayType = "Certificate";
            certificatePrice.PriceType = "Certificate";
            certificatePrice.PriceTypeDescription = "Electronic travel certificate";
            if (_configuration.GetValue<bool>("MTETCToggle"))
            {
                certificatePrice.Value = totalAmount;
            }
            else
            {
                certificatePrice.Value += totalAmount;
            }
            certificatePrice.Value = Math.Round(certificatePrice.Value, 2, MidpointRounding.AwayFromZero);
            certificatePrice.FormattedDisplayValue = "-" + (certificatePrice.Value).ToString("C2", new CultureInfo("en-US"));
            certificatePrice.DisplayValue = string.Format("{0:#,0.00}", certificatePrice.Value);

            return certificatePrice;
        }

        public static bool IsMilesFOPEnabled()
        {
            Boolean isMilesFOP;
            Boolean.TryParse(_configuration.GetValue<string>("EnableMilesAsPayment"), out isMilesFOP);
            return isMilesFOP;
        }

        public static bool IsMFOPCatalogEnabled(List<MOBItem> catalogValues)
        {
            return catalogValues != null
                && catalogValues.Count > 0
                && catalogValues.FirstOrDefault(a => a.Id == ((int)IOSCatalogEnum.EnableMilesFOPForPaidSeats).ToString() || a.Id == ((int)AndroidCatalogEnum.EnableMilesFOPForPaidSeats).ToString())?.CurrentValue == "1";
        }
        public static bool IncludeReshopFFCResidual(int appId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnableReshopFFCResidual")
                && GeneralHelper.IsApplicationVersionGreater
                (appId, appVersion, "AndroidFFCResidualVersion", "iPhoneFFCResidualVersion", "", "", true, _configuration);
        }
        public static bool IsEnableU4BCorporateBooking(int applicationId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnableU4BCorporateBooking") && GeneralHelper.IsApplicationVersionGreaterorEqual(applicationId, appVersion, _configuration.GetValue<string>("Android_EnableU4BCorporateBooking_AppVersion"), _configuration.GetValue<string>("IPhone_EnableU4BCorporateBooking_AppVersion"));
        }
        public static WorkFlowType GetWorkFlowType(string flow, string productCode = "")
        {
            switch (flow)
            {
                case "CHECKIN":
                    return WorkFlowType.CheckInProductsPurchase;

                case "BOOKING":
                    return WorkFlowType.InitialBooking;

                case "VIEWRES":
                case "POSTBOOKING":                    
                case "VIEWRES_SEATMAP":
                case "BAGGAGECALCULATOR":
                    if (productCode == "RES")
                        return WorkFlowType.FareLockPurchase;
                    else if (IsPOMOffer(productCode))
                        return WorkFlowType.PreOrderMeals;
                    else
                        return WorkFlowType.PostPurchase;

                case "RESHOP":
                    return WorkFlowType.Reshop;

                case "FARELOCK":
                    return WorkFlowType.FareLockPurchase;
                case "UPGRADEMALL":
                    return WorkFlowType.UpgradesPurchase;
            }
            return WorkFlowType.UnKnown;
        }

        public static bool IsPOMOffer(string productCode)
        {
            if (!_configuration.GetValue<bool>("EnableInflightMealsRefreshment")) return false;
            if (string.IsNullOrEmpty(productCode)) return false;
            return (productCode == _configuration.GetValue<string>("InflightMealProductCode"));
        }

        public static bool IncludeMoneyPlusMiles(int appId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnableMilesPlusMoney")
                && GeneralHelper.IsApplicationVersionGreater
                (appId, appVersion, "AndroidMilesPlusMoneyVersion", "iPhoneMilesPlusMoneyVersion", "", "", true, _configuration);
        }

        public static double GetAlowedETCAmount(List<MOBProdDetail> products, string flow)
        {
            string allowedETCAncillaryProducts = string.Empty;
            if (_configuration.GetValue<bool>("EnableEtcforSeats_PCU_Viewres") && flow == FlowType.VIEWRES.ToString())
            {
                allowedETCAncillaryProducts = _configuration.GetValue<string>("VIewResETCEligibleProducts");
            }
            else
            {
                allowedETCAncillaryProducts = _configuration.GetValue<string>("CombinebilityETCAppliedAncillaryCodes");
            }
            double maximumAllowedETCAmount = Convert.ToDouble(_configuration.GetValue<string>("CombinebilityMaxAmountOfETCsAllowed"));
            double allowedETCAmount = products == null ? 0 : products.Where(p => (p.Code == "RES" || allowedETCAncillaryProducts.IndexOf(p.Code) > -1) && !string.IsNullOrEmpty(p.ProdTotalPrice)).Sum(a => Convert.ToDouble(a.ProdTotalPrice));
            if (_configuration.GetValue<bool>("ETCForAllProductsToggle"))
            {
                allowedETCAmount += GetBundlesAmount(products, flow);
            }
            if (allowedETCAmount > maximumAllowedETCAmount)
            {
                allowedETCAmount = maximumAllowedETCAmount;
            }
            return allowedETCAmount;
        }

        public static void UpdateCertificateAmountInTotalPrices(List<MOBSHOPPrice> prices, double certificateTotalAmount)
        {
            var certificatePrice = prices.FirstOrDefault(p => p.DisplayType.ToUpper() == "CERTIFICATE");
            var grandtotal = prices.FirstOrDefault(p => p.DisplayType.ToUpper() == "GRAND TOTAL");

            if (certificatePrice == null && certificateTotalAmount > 0)
            {
                certificatePrice = new MOBSHOPPrice();
                UpdateCertificatePrice(certificatePrice, certificateTotalAmount);
                prices.Add(certificatePrice);
            }
            else if (certificatePrice != null)
            {
                UtilityHelper.UpdateCertificateRedeemAmountFromTotalInReserationPrices(grandtotal, certificatePrice.Value, false);
                UpdateCertificatePrice(certificatePrice, certificateTotalAmount);
            }

            if (certificateTotalAmount == 0 && certificatePrice != null)
            {
                prices.Remove(certificatePrice);
            }

            UtilityHelper.UpdateCertificateRedeemAmountFromTotalInReserationPrices(grandtotal, certificateTotalAmount);
        }

        public static double GetBundlesAmount(List<MOBProdDetail> products, string flow)
        {
            string nonBundleProductCode = _configuration.GetValue<string>("NonBundleProductCode");
            double bundleAmount = products == null ? 0 : products.Where(p => (nonBundleProductCode.IndexOf(p.Code) == -1) && !string.IsNullOrEmpty(p.ProdTotalPrice)).Sum(a => Convert.ToDouble(a.ProdTotalPrice));
            return bundleAmount;
        }

        public static bool IncludeMOBILE12570ResidualFix(int appId, string appVersion)
        {
            bool isApplicationGreater = GeneralHelper.IsApplicationVersionGreater(appId, appVersion, "AndroidMOBILE12570ResidualVersion", "iPhoneMOBILE12570ResidualVersion", "", "", true, _configuration);
            return (_configuration.GetValue<bool>("eableMOBILE12570Toggle") && isApplicationGreater);
        }

        public static bool IsManageResETCEnabled(int applicationId, string appVersion)
        {
            if (_configuration.GetValue<bool>("EnableEtcforSeats_PCU_Viewres") && GeneralHelper.IsApplicationVersionGreaterorEqual(applicationId, appVersion, _configuration.GetValue<string>("Android_EnableETCManageRes_AppVersion"), _configuration.GetValue<string>("iPhone_EnableETCManageRes_AppVersion")))
            {
                return true;
            }
            return false;
        }

        public static void UpdateSavedCertificate(MOBShoppingCart shoppingcart)
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

        public static void AddPromoDetailsInSegments(MOBProdDetail prodDetail)
        {
            if (prodDetail?.Segments != null)
            {
                double promoValue;
                prodDetail?.Segments.ForEach(p =>
                {
                    p.SubSegmentDetails.ForEach(subSegment =>
                    {
                        if (!string.IsNullOrEmpty(subSegment.OrginalPrice) && !string.IsNullOrEmpty(subSegment.Price))
                        {
                            promoValue = Convert.ToDouble(subSegment.OrginalPrice) - Convert.ToDouble(subSegment.Price);
                            subSegment.Price = subSegment.OrginalPrice;
                            subSegment.DisplayPrice = Decimal.Parse(subSegment.Price).ToString("c");
                            if (promoValue > 0)
                            {
                                subSegment.PromoDetails = new MOBPromoCode
                                {
                                    PriceTypeDescription = _configuration.GetValue<string>("PromoCodeAppliedText"),
                                    PromoValue = Math.Round(promoValue, 2, MidpointRounding.AwayFromZero),
                                    FormattedPromoDisplayValue = "-" + promoValue.ToString("C2", CultureInfo.CurrentCulture)
                                };
                            }
                        }
                    });

                });
            }
        }

        public static void AddCouponDetails(List<MOBProdDetail> prodDetails, FlightReservationResponse cslFlightReservationResponse, bool isPost, string flow, MOBApplication application)
        {
            United.Service.Presentation.InteractionModel.ShoppingCart flightReservationResponseShoppingCart = new United.Service.Presentation.InteractionModel.ShoppingCart();
            flightReservationResponseShoppingCart = isPost ? cslFlightReservationResponse.CheckoutResponse.ShoppingCart : cslFlightReservationResponse.ShoppingCart;
            foreach (var prodDetail in prodDetails)
            {
                var product = flightReservationResponseShoppingCart.Items.SelectMany(I => I.Product).Where(p => p.Code == prodDetail.Code).FirstOrDefault();
                if (product != null && product.CouponDetails != null && product.CouponDetails.Any(c => c != null) && product.CouponDetails.Count() > 0)
                {
                    prodDetail.CouponDetails = new List<CouponDetails>();
                    foreach (var coupon in product.CouponDetails)
                    {
                        if (coupon != null)
                        {
                            prodDetail.CouponDetails.Add(new CouponDetails
                            {
                                PromoCode = coupon.PromoCode,
                                Product = coupon.Product,
                                IsCouponEligible = coupon.IsCouponEligible,
                                Description = coupon.Description,
                                DiscountType = coupon.DiscountType
                            });
                        }
                    }
                }
                if (flow == FlowType.POSTBOOKING.ToString() && prodDetail.CouponDetails != null && prodDetail.CouponDetails.Count > 0
                      || (flow == FlowType.BOOKING.ToString() && prodDetail.CouponDetails != null && IsEnableOmniCartMVP2Changes(application.Id, application.Version.Major, true)) || (_configuration.GetValue<bool>("IsEnableManageResCoupon") && (flow == FlowType.VIEWRES.ToString() || flow == FlowType.VIEWRES_SEATMAP.ToString()) && prodDetail.CouponDetails != null))
                {
                    AddPromoDetailsInSegments(prodDetail);
                }
            }
        }

        public static bool IsOriginalPriceExists(MOBProdDetail prodDetail)
        {
            return !_configuration.GetValue<bool>("DisableFreeCouponFix")
                   && !string.IsNullOrEmpty(prodDetail.ProdOriginalPrice)
                   && Decimal.TryParse(prodDetail.ProdOriginalPrice, out decimal originalPrice)
                   && originalPrice > 0;
        }

        public static string GetFormattedCabinName(string cabinName)
        {
            if (!_configuration.GetValue<bool>("EnablePcuMultipleUpgradeOptions"))
            {
                return cabinName;
            }

            if (string.IsNullOrWhiteSpace(cabinName))
                return string.Empty;

            switch (cabinName.ToUpper().Trim())
            {
                case "UNITED FIRST":
                    return "United First®";
                case "UNITED BUSINESS":
                    return "United Business®";
                case "UNITED POLARIS FIRST":
                    return "United Polaris℠ first";
                case "UNITED POLARIS BUSINESS":
                    return "United Polaris℠ business";
                case "UNITED PREMIUM PLUS":
                    return "United® Premium Plus";
                default:
                    return string.Empty;
            }
        }

        public static string GetSeatTypeBasedonCode(string seatCode, int travelerCount, bool isCheckinPath = false)
        {
            string seatType = string.Empty;

            switch (seatCode.ToUpper().Trim())
            {
                case "SXZ": //StandardPreferredExitPlus
                case "SZX": //StandardPreferredExit
                case "SBZ": //StandardPreferredBlukheadPlus
                case "SZB": //StandardPreferredBlukhead
                case "SPZ": //StandardPreferredZone
                case "PZA":
                    seatType = (travelerCount > 1) ? "Preferred seats" : "Preferred seat";
                    break;
                case "SXP": //StandardPrimeExitPlus
                case "SPX": //StandardPrimeExit
                case "SBP": //StandardPrimeBlukheadPlus
                case "SPB": //StandardPrimeBlukhead
                case "SPP": //StandardPrimePlus
                case "PPE": //StandardPrime
                case "BSA":
                case "ASA":
                    if (isCheckinPath)
                        seatType = (travelerCount > 1) ? "Seat assignments" : "Seat assignment";
                    else
                        seatType = (travelerCount > 1) ? "Advance seat assignments" : "Advance seat assignment";
                    break;
                case "EPL": //EplusPrime
                case "EPU": //EplusPrimePlus
                case "BHS": //BulkheadPrime
                case "BHP": //BulkheadPrimePlus  
                case "PSF": //PrimePlus  
                    seatType = (travelerCount > 1) ? "Economy Plus Seats" : "Economy Plus Seat";
                    break;
                case "PSL": //Prime                            
                    seatType = (travelerCount > 1) ? "Economy Plus Seats (limited recline)" : "Economy Plus Seat (limited recline)";
                    break;
                default:
                    var pcuCabinName = GetFormattedCabinName(seatCode);
                    if (!string.IsNullOrEmpty(pcuCabinName))
                    {
                        return pcuCabinName + ((travelerCount > 1) ? " Seats" : " Seat");
                    }
                    return string.Empty;
            }
            return seatType;
        }

        public static bool VersionCheck_NullSession_AfterAppUpgradation(MOBRequest request)
        {
            bool isVersionGreaterorEqual = GeneralHelper.IsApplicationVersionGreater2(request.Application.Id, request.Application.Version.Major, "Android_NullSession_AfterUpgradation_AppVersion", "iPhone_NullSession_AfterUpgradation_AppVersion", null, null, _configuration);
            return isVersionGreaterorEqual;
        }

        public static bool IsFFCSummaryUpdated(int appId, string version)
        {
            if (!_configuration.GetValue<bool>("EnableFFCROnSummaryFeature")) return false;
            return GeneralHelper.IsApplicationVersionGreaterorEqual(appId, version, _configuration.GetValue<string>("Android_EnableFFCROnSummaryFeature_AppVersion"), _configuration.GetValue<string>("IPhone_EnableFFCROnSummaryFeature_AppVersion"));
        }

        public static bool IsEnableU4BCorporateBookingFeature(int appId, string appVersion)
        {
            if (!_configuration.GetSection("EnableU4BCorporateBookingFFC").GetValue<bool>("isEnable")) return false;
            return GeneralHelper.IsApplicationVersionGreaterorEqual(appId, appVersion, _configuration.GetSection("EnableU4BCorporateBookingFFC").GetValue<string>("android_EnableU4BCorporateBookingFFC_AppVersion"), _configuration.GetSection("EnableU4BCorporateBookingFFC").GetValue<string>("iPhone_EnableU4BCorporateBookingFFC_AppVersion"));
        }
        public static bool IsSuppressPkDispenserKey(int appId, string appVersion, List<MOBItem> catalogItems = null)
        {
            return (_configuration.GetValue<bool>("EnableSuppressPkDispenserKey") &&
                   GeneralHelper.IsApplicationVersionGreaterorEqual(appId, appVersion, _configuration.GetValue<string>("EnableSuppressPkDispenserKey_Android_Version"), _configuration.GetValue<string>("EnableSuppressPkDispenserKey_iPhone_Version"))
                   && ((catalogItems != null && catalogItems.Count > 0 &&
                   catalogItems.FirstOrDefault(a => a.Id == ((int)IOSCatalogEnum.DisablePKDispenserKeyFromCSL).ToString() || a.Id == ((int)AndroidCatalogEnum.DisablePKDispenserKeyFromCSL).ToString())?.CurrentValue == "1")
                  ));
        }

        public static bool IsSuppressPkDispenserKey(int appId, string appVersion, List<MOBItem> catalogItems = null, string flow = "")
        {
            return (_configuration.GetValue<bool>("EnableSuppressPkDispenserKey") &&
                   GeneralHelper.IsApplicationVersionGreaterorEqual(appId, appVersion, _configuration.GetValue<string>("EnableSuppressPkDispenserKey_Android_Version"), _configuration.GetValue<string>("EnableSuppressPkDispenserKey_iPhone_Version"))
                   && ((catalogItems != null && catalogItems.Count > 0 &&
                   catalogItems.FirstOrDefault(a => a.Id == ((int)IOSCatalogEnum.DisablePKDispenserKeyFromCSL).ToString() || a.Id == ((int)AndroidCatalogEnum.DisablePKDispenserKeyFromCSL).ToString())?.CurrentValue == "1")
                   || IsViewResFlowPaymentSvcEnabled(flow)));
        }

        // MOBILE-25395: SAF
        public static bool IsEnableSAFFeature(Session session)
        {
            return IsEnabledFeatureWithCatalogCheck(session,
                                                    "EnableSAFFeature",
                                                    IOSCatalogEnum.EnableSAFFeature,
                                                    AndroidCatalogEnum.EnableSAFFeature);
        }

        // MOBILE-25395: SAF
        public static bool IsEnabledFeatureWithCatalogCheck(Session session, string settingsKey, IOSCatalogEnum iosCatalogItem, AndroidCatalogEnum androidCatalogItem)
        {
            return _configuration.GetValue<bool>(settingsKey) &&
                   session.CatalogItems != null &&
                   session.CatalogItems.Count > 0 &&
                   session.CatalogItems.FirstOrDefault(a => a.Id == ((int)iosCatalogItem).ToString() || a.Id == ((int)androidCatalogItem).ToString())?.CurrentValue == "1";
        }
        public static void RemoveEncyptedCreditcardNumber(MOBCreditCard creditCard)
        {
            if (creditCard != null && !string.IsNullOrEmpty(creditCard.EncryptedCardNumber))
            {
                creditCard.EncryptedCardNumber = String.Empty;
            }
        }

        public static void RemoveIDNumberInTaxIdInformation(MOBTaxIdInformation taxIdInformation) 
        {
            if (taxIdInformation != null && taxIdInformation.SelectedValues != null) 
            {
                foreach (var items in taxIdInformation.SelectedValues) 
                {
                    foreach (var item in items) 
                    {
                        item.CurrentValue = string.Empty;
                    }
                }
            }
        }

        public static void RemoveDescriptionInServiceRequest(List<Service.Presentation.CommonModel.Service> specialService) 
        {
            if (specialService != null && specialService.Count > 0) 
            {
                foreach (var item in specialService) 
                {
                    item.Description = string.Empty;
                }
            }
        }
       
        public static string RemoveEncryptedCardNumberFromDatavaultCSLResponse((string response, HttpStatusCode statusCode, string url) responseData)
        {
            if (_configuration.GetValue<bool>("RemoveEncryptedCardNumberForLogs"))
            {
                CslDataVaultResponse cslDatavaultResponse = DataContextJsonSerializer.DeserializeJsonDataContract<CslDataVaultResponse>(responseData.response);
                if (cslDatavaultResponse?.Items != null && cslDatavaultResponse?.Items.Count() > 0)
                    cslDatavaultResponse.Items[0].AccountNumberEncrypted = String.Empty;
                return JsonConvert.SerializeObject(cslDatavaultResponse);
            }
            return responseData.response;
        }
        public static string RemoveEncryptedCardNumberFromDatavaultCSLRequest(string requestData)
        {
            if (_configuration.GetValue<bool>("RemoveEncryptedCardNumberForLogs"))
            {
                CslDataVaultRequest cslDatavalutrequest = DataContextJsonSerializer.DeserializeJsonDataContract<CslDataVaultRequest>(requestData);
                cslDatavalutrequest.Items[0].AccountNumberEncrypted = String.Empty;
                return JsonConvert.SerializeObject(cslDatavalutrequest);
            }
            return requestData;
        }
        public static bool IsViewResFlowCheckOut(string flow)
        {
            return _configuration.GetValue<bool>("EnableViewResFlowCheckOutInCloud") &&
                    (flow == FlowType.VIEWRES.ToString() ||
                    flow == FlowType.MANAGERES.ToString() ||
                    flow == FlowType.VIEWRES_BUNDLES_SEATMAP.ToString() ||
                    flow == FlowType.VIEWRES_SEATMAP.ToString() ||
                    flow == FlowType.BAGGAGECALCULATOR.ToString());
        }

        public static bool IsViewResFlowPaymentSvcEnabled(string flow)
        {
            return !string.IsNullOrEmpty(flow) && _configuration.GetValue<bool>("EnableViewResFlowPaymentService") &&
                    (flow == FlowType.VIEWRES.ToString() ||
                    flow == FlowType.MANAGERES.ToString() ||
                    flow == FlowType.VIEWRES_BUNDLES_SEATMAP.ToString() ||
                     flow == FlowType.VIEWRES_SEATMAP.ToString() ||
                    flow == FlowType.BAGGAGECALCULATOR.ToString());
        }
        public static bool IsIbeProductCode(Collection<Characteristic> characteristics)
        {
            var IBEFullProductCodes = _configuration.GetValue<string>("IBEFullShoppingProductCodes").Split(',');
            return _configuration.GetValue<bool>("EnableIBE") &&
                    characteristics != null &&
                    characteristics.Any(c => c != null &&
                                            "PRODUCTCODE".Equals(c.Code, StringComparison.OrdinalIgnoreCase) &&
                                            IBEFullProductCodes.Contains(c.Value));
        }
        public static bool EnablePBE()
        {
            return _configuration.GetValue<bool>("EnablePBE");
        }
        public static bool EnableMFOP(int appId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnableMFOP") &&
                   GeneralHelper.IsApplicationVersionGreater(appId, appVersion, "AndroidMFOPVersion", "iPhoneMFOPVersion", "", "", true, _configuration);
        }

        public static bool IsEnableMFOPCheckinBags(int applicationId, string appVersion)
        {
            if (GeneralHelper.IsApplicationVersionGreaterorEqual(applicationId, appVersion, _configuration.GetValue<string>("AndroidMilesFopCheckinBagsVersion"), _configuration.GetValue<string>("iPhoneMilesFopCheckinBagsVersion")))
            {
                return true;
            }
            return false;
        }
    }
}
