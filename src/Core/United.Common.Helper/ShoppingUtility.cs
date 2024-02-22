using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using United.Definition;
using United.Definition.Shopping;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.DynamoDB;
using United.Mobile.Model;
using United.Mobile.Model.Common;
using United.Mobile.Model.MSC.FormofPayment;
using United.Persist.Definition.Shopping;
using United.Services.FlightShopping.Common.DisplayCart;
using United.Services.FlightShopping.Common.Extensions;
using United.Services.FlightShopping.Common.FlightReservation;
using United.Utility.Helper;

namespace United.Common.Helper
{
    public class ShoppingUtility:IShoppingUtility
    {
        private readonly IConfiguration _configuration;
        private readonly ISessionHelperService _sessionHelperService;
        private readonly IDynamoDBService _dynamoDBService;
        private readonly IFeatureSettings _featureSettings;
        public ShoppingUtility(IConfiguration configuration
            , ISessionHelperService sessionHelperService
            , IDynamoDBService dynamoDBService
            , IFeatureSettings featureSettings)
        {
            _configuration = configuration;
            _sessionHelperService = sessionHelperService;
            _dynamoDBService = dynamoDBService;
            _featureSettings = featureSettings;

        }

        public async Task ValidateAwardMileageBalance(string sessionId, decimal milesNeeded)
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

        public double GetGrandTotalPriceForShoppingCart(bool isCompleteFarelockPurchase, FlightReservationResponse flightReservationResponse, bool isPost, string flow = "VIEWRES")
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
                    closeBookingFee = GetCloseBookingFee(isPost, flightReservationResponseShoppingCart, flow);

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

                    case "CHECKIN":
                        shoppingCartTotalPrice = isPost ? flightReservationResponse.CheckoutResponse.ShoppingCart.Items.Where(x => !UtilityHelper.CheckFailedShoppingCartItem(flightReservationResponse, x)).Where(x => x.Product.FirstOrDefault().Code != "RES" && (x.Product.FirstOrDefault().Price != null ? (x.Product.FirstOrDefault().Price.Totals.Any()) : false)).Select(x => x.Product.FirstOrDefault().Price.Totals.FirstOrDefault().Amount).ToList().Sum()
                            : flightReservationResponse.ShoppingCart.Items.Where(x => x.Product.FirstOrDefault().Code != "RES" && (x.Product.FirstOrDefault().Price != null ? (x.Product.FirstOrDefault().Price.Totals.Any()) : false)).Select(x => x.Product.FirstOrDefault().Price.Totals.FirstOrDefault().Amount).ToList().Sum();
                        break;
                    case "VIEWRES":
                    case "VIEWRES_SEATMAP":
                    case "MOBILECHECKOUT":
                        shoppingCartTotalPrice = isPost ? flightReservationResponse.CheckoutResponse.ShoppingCart.Items.Where(x => !UtilityHelper.CheckFailedShoppingCartItem(flightReservationResponse, x)).Where(x => x.Product.FirstOrDefault().Code != "RES" && (x.Product.FirstOrDefault().Price != null ? (x.Product.FirstOrDefault().Price.Totals.Any()) : false)).Select(x => x.Product.FirstOrDefault().Price.Totals.FirstOrDefault().Amount).ToList().Sum()
                            : flightReservationResponse.ShoppingCart.Items.Where(x => x.Product.FirstOrDefault().Code != "RES" && (x.Product.FirstOrDefault().Price != null ? (x.Product.FirstOrDefault().Price.Totals.Any()) : false)).Select(x => x.Product.FirstOrDefault().Price.Totals.Select(t => t != null && t.Amount > 0 ? t.Amount : 0).ToList().Sum()).ToList().Sum();
                        break;
                }
            }
            if (_configuration.GetValue<bool>("CFOP19HBugFixToggle"))
            {
                shoppingCartTotalPrice = shoppingCartTotalPrice + closeBookingFee;
            }
            return shoppingCartTotalPrice;
        }

        public async System.Threading.Tasks.Task<string> GetAirportName(string airportCode)
        {
            AirportDynamoDB airportDynamoDB = new AirportDynamoDB(_configuration, _dynamoDBService);
            return await airportDynamoDB.GetAirportName(airportCode, "trans0");
        }

        private double GetCloseBookingFee(bool isPost, United.Service.Presentation.InteractionModel.ShoppingCart flightReservationResponseShoppingCart, string flow)
        {
            return isPost ? flightReservationResponseShoppingCart.Items.SelectMany(d => d.Product).Where(d => d.Code == "RBF").FirstOrDefault().Price.Totals.FirstOrDefault().Amount :
                                  flightReservationResponseShoppingCart.Items.SelectMany(d => d.Product).Where(d => d.Code == "RBF").SelectMany(x => x.Price.Totals).FirstOrDefault().Amount;
        }

        public bool EnableShoppingcartPhase2ChangesWithVersionCheck(int appId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnableShoppingCartPhase2Changes")
                    && GeneralHelper.IsApplicationVersionGreaterorEqual(appId, appVersion, _configuration.GetValue<string>("Android_EnableShoppingCartPhase2Changes_AppVersion"), _configuration.GetValue<string>("iPhone_EnableShoppingCartPhase2Changes_AppVersion"));
        }

        public double UpdatePromoValueForFSRMoneyMiles(List<DisplayPrice> prices, Session session, double promoValue)
        {
            if (_configuration.GetValue<bool>("EnableFSRMoneyPlusMilesFeature") && prices != null && session?.IsMoneyPlusMilesSelected == true)
            {
                var moneyMilesPrice = prices.Find(dp => dp.Type?.ToUpper()?.Equals("MILESANDMONEY") == true);
                if (moneyMilesPrice != null && promoValue > 0)
                    promoValue = promoValue - Convert.ToDouble(moneyMilesPrice.Amount);
            }

            return promoValue;
        }
        public string BuildPaxTypeDescription(string paxTypeCode, string paxDescription, int paxCount)
        {
            string description = paxDescription;
            if (!string.IsNullOrEmpty(paxTypeCode))
            {
                switch (paxTypeCode.ToUpper())
                {
                    case "ADT":
                        description = $"{((paxCount == 1) ? "adult (18-64)" : "adults (18-64)")} ";
                        break;
                    case "SNR":
                        description = $"{((paxCount == 1) ? "senior (65+)" : "seniors (65+)")} ";
                        break;
                    case "C17":
                        description = $"{((paxCount == 1) ? "child (15-17)" : "children (15-17)")} ";
                        break;
                    case "C14":
                        description = $"{((paxCount == 1) ? "child (12-14)" : "children (12-14)")} ";
                        break;
                    case "C11":
                        description = $"{((paxCount == 1) ? "child (5-11)" : "children (5-11)")} ";
                        break;
                    case "C04":
                        description = $"{((paxCount == 1) ? "child (2-4)" : "children (2-4)")} ";
                        break;
                    case "INS":
                        description = $"{((paxCount == 1) ? "infant(under 2) - seat" : "infants(under 2) - seat")} ";
                        break;
                    case "INF":
                        description = $"{((paxCount == 1) ? "infant (under 2) - lap" : "infants (under 2) - lap")} ";
                        break;
                    default:
                        description = paxDescription;
                        break;
                }

            }
            return description;
        }
        public bool EnableEditForAllCabinPOM(int appId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnableisEditablePOMFeature") && GeneralHelper.IsApplicationVersionGreaterorEqual(appId, appVersion, _configuration.GetValue<string>("Android_isEditablePOMFeatureSupported_AppVersion"), _configuration.GetValue<string>("IPhone_isEditablePOMFeatureSupported_AppVersion"));
        }
        public bool EnablePOMPreArrival(int appId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnablePOMPreArrival") && GeneralHelper.IsApplicationVersionGreaterorEqual(appId, appVersion, _configuration.GetValue<string>("Android_EnablePOMPreArrival_AppVersion"), _configuration.GetValue<string>("IPhone_EnablePOMPreArrival_AppVersion"));
        }
        public bool IsEnablePartnerProvision(List<MOBItem> catalogItems, string flow, int applicationId, string appVersion)
        {
            if (flow == FlowType.BOOKING.ToString() || flow == FlowType.POSTBOOKING.ToString())
            {
                return (catalogItems.FirstOrDefault(a => a.Id == ((int)IOSCatalogEnum.EnablePartnerProvision).ToString() || a.Id == ((int)AndroidCatalogEnum.EnablePartnerProvision).ToString())?.CurrentValue == "1"
                    && GeneralHelper.IsApplicationVersionGreaterorEqual(applicationId, appVersion, _configuration.GetValue<string>("Android_PartnerProvision_AppVersion"), _configuration.GetValue<string>("IPhone_PartnerProvision_AppVersion")));
            }
            else if (flow == FlowType.CHECKIN.ToString())
            {
                return (catalogItems.FirstOrDefault(a => a.Id == ((int)IOSCatalogEnum.EnableProvisionInCHECKIN).ToString() || a.Id == ((int)AndroidCatalogEnum.EnableProvisionInCHECKIN).ToString())?.CurrentValue == "1"
                    && GeneralHelper.IsApplicationVersionGreaterorEqual(applicationId, appVersion, _configuration.GetValue<string>("Android_PartnerProvision_AppVersion"), _configuration.GetValue<string>("IPhone_PartnerProvision_AppVersion")));
            }
            else
            {
                return (catalogItems.FirstOrDefault(a => a.Id == ((int)IOSCatalogEnum.EnableProvisionInVIEWRES).ToString() || a.Id == ((int)AndroidCatalogEnum.EnableProvisionInVIEWRES).ToString())?.CurrentValue == "1"
                    && GeneralHelper.IsApplicationVersionGreaterorEqual(applicationId, appVersion, _configuration.GetValue<string>("Android_PartnerProvision_AppVersion"), _configuration.GetValue<string>("IPhone_PartnerProvision_AppVersion")));
            }
        }

        public string GetFormatPriceSearch(string searchType)
        {
            return !searchType.IsNullOrEmpty() ? "Fare " + searchType + " " : string.Empty;
        }

        public string BuildPriceTypeDescription(string searchType, string priceDescription, int price, string desc, bool isFareLockViewRes, bool isCorporateFareLock)
        {
            if (!searchType.IsNullOrEmpty() && !priceDescription.IsNullOrEmpty() && !price.IsNullOrEmpty())
            {
                if (isFareLockViewRes)
                {
                    var description = isCorporateFareLock ? "traveler)" : desc;
                    return GetFormatPriceSearch(searchType) + "(" + Convert.ToString(price) + " " + description;
                }
                else
                {
                    if (!priceDescription.ToUpper().Equals("CORPORATE RATE"))
                    {
                        return GetSearchTypeDesc(searchType) + "(" + Convert.ToString(price) + " " + desc;
                    }
                }
            }

            return string.Empty;
        }

        public string GetSearchTypeDesc(string searchType)
        {
            if (string.IsNullOrEmpty(searchType))
                return string.Empty;

            string searchTypeDesc = string.Empty;
            if (searchType.ToUpper().Equals("OW"))
            {
                searchTypeDesc = "Oneway";
            }
            else if (searchType.ToUpper().Equals("RT"))
            {
                searchTypeDesc = "Roundtrip";
            }
            else if (searchType.ToUpper().Equals("MD"))
            {
                searchTypeDesc = "Multipletrip";
            }

            return (!string.IsNullOrEmpty(searchTypeDesc)) ? "Fare " + searchTypeDesc + " " : string.Empty; //"Fare Oneway "
        }

        public string BuildYAPriceTypeDescription(string searchType)
        {
            if (searchType.ToUpper().Equals("OW"))
                return "Fare Oneway (1 young adult)";
            else
                if (searchType.ToUpper().Equals("RT"))
                return "Fare Roundtrip (1 young adult)";
            else
                if (searchType.ToUpper().Equals("MD"))
                return "Fare Multipletrip (1 young adult)";
            else
                return "Fare Oneway (1 young adult)";
        }

        public string GetFareDescription(DisplayPrice price)
        {
            return (price.Description.ToUpper().IndexOf("ADULT") == 0 ? "adult)" : (!price.Description.ToUpper().Contains("TOTAL")) ? price.Description.Replace("(", "").ToLower() : string.Empty);
        }

        public bool EnableYADesc(bool isReshop = false)
        {
            return _configuration.GetValue<bool>("EnableYoungAdultBooking") && _configuration.GetValue<bool>("EnableYADesc") && !isReshop;
        }

        public List<MOBSHOPPrice> AdjustTotal(List<MOBSHOPPrice> prices)
        {
            CultureInfo ci = null;

            List<MOBSHOPPrice> newPrices = prices;
            double fee = 0;
            foreach (MOBSHOPPrice p in newPrices)
            {
                if (ci == null)
                {
                    ci = GetCultureInfo(p.CurrencyCode);
                }

                if (fee == 0)
                {
                    foreach (MOBSHOPPrice q in newPrices)
                    {
                        if (q.DisplayType.Trim().ToUpper() == "RBF")
                        {
                            fee = q.Value;
                            break;
                        }
                    }
                }
                if (p.DisplayType.Trim().ToUpper() == "REFUNDPRICE" && p.Value < 0)
                {
                    p.Value *= -1;
                }
                if ((fee > 0 && p.DisplayType.Trim().ToUpper() == "TOTAL") || p.DisplayType.Trim().ToUpper() == "REFUNDPRICE")
                {
                    //update total
                    p.Value += fee;
                    p.DisplayValue = string.Format("{0:#,0.00}", p.Value);
                }
            }

            return newPrices;
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

                if (price.SubItems != null && price.SubItems.Count > 0
                    && (!isReshopChange || (isReshopChange && price.Type.ToUpper() == "TRAVELERPRICE" && !isTravelerPriceDirty))
                    && (_configuration.GetValue<bool>("EnableCouponsforBooking") && price.Type.ToUpper() != "NONDISCOUNTPRICE-TRAVELERPRICE")) // Added by Hasnan - # 167553 - 10/04/2017
                {
                    foreach (var subItem in price.SubItems)
                    {
                        if (ci == null)
                        {
                            ci = GetCultureInfo(subItem.Currency);
                        }
                        MOBSHOPTax taxNfee = new MOBSHOPTax();
                        taxNfee = new MOBSHOPTax();
                        taxNfee.CurrencyCode = subItem.Currency;
                        taxNfee.Amount = subItem.Amount;
                        taxNfee.DisplayAmount = TopHelper.FormatAmountForDisplay(taxNfee.Amount, ci, false);
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
                    tnf.DisplayAmount = TopHelper.FormatAmountForDisplay(tnf.Amount, ci, false);
                    tnf.TaxCode = "PERPERSONTAX";
                    string description = price?.Description;
                    if (EnableShoppingcartPhase2ChangesWithVersionCheck(appId, appVersion) && !isReshopChange && !string.IsNullOrEmpty(travelType) && (travelType == TravelType.CB.ToString() || travelType == TravelType.CLB.ToString()))
                    {
                        description = BuildPaxTypeDescription(price?.PaxTypeCode, price?.Description, price.Count);
                    }
                    tnf.TaxCodeDescription = string.Format("{0} {1}: {2}{3}", price.Count, description.ToLower(), tnf.DisplayAmount
                        , isEnableOmniCartMVP2Changes ? "/person" : " per person");
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
                tnfTotal.DisplayAmount = TopHelper.FormatAmountForDisplay(tnfTotal.Amount, ci, false);
                tnfTotal.TaxCode = "TOTALTAX";
                tnfTotal.TaxCodeDescription = "Taxes and fees total";
                lstTnfTotal.Add(tnfTotal);
                taxsAndFees.Add(lstTnfTotal);
            }

            return taxsAndFees;
        }

        public CultureInfo GetCultureInfo(string currencyCode)
        {
            CultureInfo culture = new CultureInfo("en-US");

            if (!string.IsNullOrEmpty(currencyCode))
            {
                var cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);

                foreach (var ci in cultures)
                {
                    try
                    {
                        var ri = new RegionInfo(ci.Name);
                        if (ri.ISOCurrencySymbol.ToUpper() == currencyCode.ToUpper())
                        {
                            culture = ci;
                            break;
                        }
                    }
                    catch { culture = new CultureInfo("en-US"); }
                }
            }

            return culture;
        }
        public List<List<MOBSHOPTax>> GetTaxAndFeesAfterPriceChange(List<United.Services.FlightShopping.Common.DisplayCart.DisplayPrice> prices, int numPax, bool isReshopChange = false, int appId = 0, string appVersion = "", string travelType = null)
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
                            ci = GetCultureInfo(subItem.Currency);
                        }
                        MOBSHOPTax taxNfee = new MOBSHOPTax();
                        taxNfee = new MOBSHOPTax();
                        taxNfee.CurrencyCode = subItem.Currency;
                        taxNfee.Amount = subItem.Amount;
                        taxNfee.DisplayAmount = TopHelper.FormatAmountForDisplay(taxNfee.Amount, ci, false);
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
                    tnf.DisplayAmount = TopHelper.FormatAmountForDisplay(tnf.Amount, ci, false);
                    tnf.TaxCode = "PERPERSONTAX";
                    string description = price?.Description;
                    if (EnableShoppingcartPhase2ChangesWithVersionCheck(appId, appVersion) && !isReshopChange && !string.IsNullOrEmpty(travelType) && (travelType == TravelType.CB.ToString() || travelType == TravelType.CLB.ToString()))
                    {
                        description = BuildPaxTypeDescription(price?.PaxTypeCode, price?.Description, price.Count);
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
                tnfTotal.DisplayAmount = TopHelper.FormatAmountForDisplay(tnfTotal.Amount, ci, false);
                tnfTotal.TaxCode = "TOTALTAX";
                tnfTotal.TaxCodeDescription = "Taxes and fees total";
                lstTnfTotal.Add(tnfTotal);
                taxsAndFees.Add(lstTnfTotal);
            }

            return taxsAndFees;
        }

        public List<MOBSHOPTax> AddFeesAfterPriceChange(List<United.Services.FlightShopping.Common.DisplayCart.DisplayPrice> prices)
        {

            List<MOBSHOPTax> taxsAndFees = new List<MOBSHOPTax>();
            CultureInfo ci = null;

            foreach (var price in prices)
            {
                if (ci == null)
                {
                    ci = GetCultureInfo(price.Currency);
                }
                MOBSHOPTax taxNfee = new MOBSHOPTax();
                taxNfee.CurrencyCode = price.Currency;
                taxNfee.Amount = price.Amount;
                taxNfee.DisplayAmount = TopHelper.FormatAmountForDisplay(taxNfee.Amount, ci, false);
                taxNfee.TaxCode = price.Type;
                taxNfee.TaxCodeDescription = price.Description;
                if (taxNfee.TaxCode != "MPF")
                    taxsAndFees.Add(taxNfee);
            }
            return taxsAndFees;
        }
        public string BuildStrikeThroughDescription()
        {
            return _configuration.GetValue<string>("StrikeThroughPriceTypeDescription");
        }
        public void UpdatePricesForTravelCredits(List<MOBSHOPPrice> bookingPrices, DisplayPrice price, MOBSHOPPrice bookingPrice, Session session)
        {

            if (IsEnableETCCreditsInBookingFeature(session?.CatalogItems) && !string.IsNullOrEmpty(price.Type) && price.Type.Equals("TravelCredits", StringComparison.OrdinalIgnoreCase)
                && bookingPrices.Any(x => x.DisplayType.Equals("TOTAL", StringComparison.OrdinalIgnoreCase) && x.Value >= 0 && string.IsNullOrEmpty(x.StrikeThroughDisplayValue) == false)
                && session.PricingType == PricingType.ETC.ToString())
            {
                try
                {
                    var total = bookingPrices.FirstOrDefault(x => x.DisplayType.Equals("TOTAL", StringComparison.OrdinalIgnoreCase));
                    if (total != null && !string.IsNullOrEmpty(total.StrikeThroughDisplayValue))
                    {
                        //bookingPrice.PriceTypeDescription = _configuration.GetValue<string>("ETCCreditsStrikeThroughTypeDescription");
                        bookingPrice.PriceType = price.Type.ToUpper();
                        bookingPrice.FormattedDisplayValue = " + " + total.StrikeThroughDisplayValue + " Travel Credits";
                    }
                }
                catch (Exception)
                {
                }
            }
        }
        private bool IsEnableETCCreditsInBookingFeature(List<MOBItem> catalogItems = null)
        {
            return _configuration.GetValue<bool>("EnableFSRETCCreditsFeature") && (catalogItems != null && catalogItems.Count > 0 &&
        catalogItems.FirstOrDefault(a => a.Id == ((int)IOSCatalogEnum.EnableETCCreditsInBooking).ToString() || a.Id == ((int)AndroidCatalogEnum.EnableETCCreditsInBooking).ToString())?.CurrentValue == "1");
        }
        public void SetETCTravelCreditsEligible(List<FormofPaymentOption> EligibleFormofPayments, MOBSHOPReservation reservation)
        {
            if (EligibleFormofPayments.Any(e => e.Code.Equals(MOBFormofPayment.ETC.ToString()) || e.Code.Equals(MOBFormofPayment.TC.ToString())))
            {
                reservation.EligibleForETCPricingType = true;
                reservation.IsEligibleForMoneyPlusMiles = false;
            }
            else
            {
                reservation.EligibleForETCPricingType = false;
            }
        }
    }
}
