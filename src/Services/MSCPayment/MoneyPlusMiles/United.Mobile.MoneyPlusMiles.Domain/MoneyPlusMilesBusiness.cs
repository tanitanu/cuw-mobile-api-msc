using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
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
using United.Services.FlightShopping.Common.DisplayCart;
using United.Services.FlightShopping.Common.Extensions;
using United.Services.FlightShopping.Common.FlightReservation;
using United.Utility.Helper;
using ProfileResponse = United.Persist.Definition.Shopping.ProfileResponse;

namespace United.Mobile.MoneyPlusMiles.Domain
{
    public class MoneyPlusMilesBusiness : IMoneyPlusMilesBusiness
    {
        private readonly ICacheLog<MoneyPlusMilesBusiness> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHeaders _headers;
        private readonly IMSCShoppingSessionHelper _shoppingSessionHelper;
        private readonly ISessionHelperService _sessionHelperService;
        private readonly IMSCPkDispenserPublicKey _pKDispenserPublicKey;
        private readonly IFlightShoppingProductsService _flightShoppingProductsService;
        private readonly IFlightShoppingService _flightShoppingService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IDPService _dPService;
        private readonly IETCUtility _iETCUtility;
        private readonly IMSCFormsOfPayment _mscformsOfPaymment;
        private readonly IShoppingCartUtility _shoppingCartUtility;
        private readonly ICachingService _cachingService;
        private readonly IDynamoDBService _dynamoDBService;
        private readonly IFeatureSettings _featureSettings;
        private string milesMoneyErrorMessage = string.Empty;
        private string milesMoneyValueDesc = string.Empty;

        public MoneyPlusMilesBusiness(ICacheLog<MoneyPlusMilesBusiness> logger
            , IConfiguration configuration
            , IHeaders headers
            , IMSCShoppingSessionHelper shoppingSessionHelper
            , ISessionHelperService sessionHelperService
            , IFlightShoppingProductsService flightShoppingProductsService
            , IDPService dPService
            , IETCUtility iETCUtility
            , IMSCFormsOfPayment mscformsOfPaymment
            , IMSCPkDispenserPublicKey pkDispenserPublicKey
            , IShoppingCartUtility shoppingCartUtility
            , IFlightShoppingService flightShoppingService
            , IShoppingCartService shoppingCartService
            , ICachingService cachingService
            , IDynamoDBService dynamoDBService
            , IFeatureSettings featureSettings)
        {
            _logger = logger;
            _configuration = configuration;
            _headers = headers;
            _shoppingSessionHelper = shoppingSessionHelper;
            _sessionHelperService = sessionHelperService;
            _pKDispenserPublicKey = pkDispenserPublicKey;
            _flightShoppingProductsService = flightShoppingProductsService;
            _dPService = dPService;
            _iETCUtility = iETCUtility;
            _mscformsOfPaymment = mscformsOfPaymment;
            _shoppingCartUtility = shoppingCartUtility;
            _flightShoppingService = flightShoppingService;
            _shoppingCartService = shoppingCartService;
            _cachingService = cachingService;
            _dynamoDBService = dynamoDBService;
            _featureSettings = featureSettings;
        }

        public async Task<MOBFOPResponse> GetMoneyPlusMiles(GetMoneyPlusMilesRequest request)
        {
            var response = new MOBFOPResponse();


            _logger.LogInformation("GetMoneyPlusMiles Request:{request} with SessionId:{sessionId}", JsonConvert.SerializeObject(request), request.SessionId);

            Session session = await _shoppingSessionHelper.GetBookingFlowSession(request.SessionId);

            if (session == null)
            {
                throw new MOBUnitedException("Could not find your booking session.");
            }

            response = await GetMilesPlusMoneyOptions(session, request);
            response.SessionId = request.SessionId;
            response.Flow = request.Flow;
            bool isDefault = false;
            var tupleRespEligibleFormofPayments = await _mscformsOfPaymment.GetEligibleFormofPayments(request, session, response.ShoppingCart, request.CartId, request.Flow, response.Reservation);
            response.EligibleFormofPayments = tupleRespEligibleFormofPayments.EligibleFormofPayments;
            isDefault = tupleRespEligibleFormofPayments.isDefault;
            await response.Reservation.UpdateRewards(_configuration, _cachingService).ConfigureAwait(false);
            await _sessionHelperService.SaveSession<MOBFOPResponse>(response, request.SessionId, new List<string> { request.SessionId, response.ObjectName }, response.ObjectName);

            return await Task.FromResult(response);
        }

        public async Task<MOBFOPResponse> ApplyMoneyPlusMiles(ApplyMoneyPlusMilesOptionRequest request)
        {
            var response = new MOBFOPResponse();

            Session session = await _shoppingSessionHelper.GetBookingFlowSession(request.SessionId);
            if (session == null)
            {
                throw new MOBUnitedException("Could not find your booking session.");
            }

            response = await ApplyMilesPlusMoneyOption(session, request);
            response.SessionId = request.SessionId;
            response.Flow = request.Flow;
            bool isDefault = false;
            var tupleRespEligibleFormofPayments = await _mscformsOfPaymment.GetEligibleFormofPayments(request, session, response.ShoppingCart, request.CartId, request.Flow, response.Reservation);
            response.EligibleFormofPayments = tupleRespEligibleFormofPayments.EligibleFormofPayments;
            isDefault = tupleRespEligibleFormofPayments.isDefault;
            await response.Reservation.UpdateRewards(_configuration, _cachingService).ConfigureAwait(false);

            await _sessionHelperService.SaveSession<MOBFOPResponse>(response, request.SessionId, new List<string> { request.SessionId, response.ObjectName }, response.ObjectName);

            return await Task.FromResult(response);
        }

        private async Task< MOBFOPResponse> ApplyMilesPlusMoneyOption(Session session, ApplyMoneyPlusMilesOptionRequest request)
        {
            MOBFOPResponse milesPlusMoneyCreditResponse = new MOBFOPResponse();
            try
            {
                List<CMSContentMessage> lstMessages = await _shoppingCartUtility.GetSDLContentByGroupName(request, session.SessionId, session.Token, _configuration.GetValue<string>("CMSContentMessages_GroupName_BookingRTI_Messages"), _configuration.GetValue<string>("BookingPathRTI_CMSContentMessagesCached_StaticGUID"));
                milesMoneyErrorMessage = GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.ReviewMMCMessage.ErrorMsg").FirstOrDefault()?.ContentFull;
                var response = await ApplyCSLMilesPlusMoneyOptions(session, request, request.OptionId);

                var shoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(session.SessionId, new MOBShoppingCart().ObjectName, new List<string> { session.SessionId, new MOBShoppingCart().ObjectName }).ConfigureAwait(false);

                await AssignUpdatedPricesToReservation(session.SessionId, response, request.OptionId, shoppingCart.Products, request.IsRTI, session, request);

                await LoadSessionValuesToResponse(session, milesPlusMoneyCreditResponse, null, request.OptionId, shoppingCart, lstMessages, request.IsRTI);
                if (_shoppingCartUtility.IsEnableMoneyPlusMilesFeature(request.Application.Id, request.Application.Version.Major, session.CatalogItems)
                    )
                {
                    if (string.IsNullOrEmpty(request.OptionId))
                    {
                        milesPlusMoneyCreditResponse.ShoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit = await _sessionHelperService.GetSession<MOBFOPMoneyPlusMilesCredit>(session.SessionId, new MOBFOPMoneyPlusMilesCredit().ObjectName, new List<string> { session.SessionId, new MOBFOPMoneyPlusMilesCredit().ObjectName }).ConfigureAwait(false);
                        milesPlusMoneyCreditResponse.ShoppingCart.OmniCart.TotalPrice.CurrentValue = _shoppingCartUtility.GetGrandTotalPrice(milesPlusMoneyCreditResponse.Reservation);
                        if (milesPlusMoneyCreditResponse.ShoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit != null)
                            milesPlusMoneyCreditResponse.ShoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit.SelectedMoneyPlusMiles = null; // OnRemoval assign back to null
                        if (milesPlusMoneyCreditResponse.ShoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit?.PromoCodeMoneyMilesAlertMessage != null)
                            milesPlusMoneyCreditResponse.ShoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit.PromoCodeMoneyMilesAlertMessage = null;
                        session.IsMoneyPlusMilesSelected = false;
                        if(await _shoppingCartUtility.IsEnableETCCreditsInBookingFeature(session?.CatalogItems))
                        {
                            if(shoppingCart.FormofPaymentDetails.TravelCertificate.Certificates.Count > 0)
                            {
                                session.IsEligibleForFSRPricingType = true;
                                milesPlusMoneyCreditResponse.Reservation.EligibleForETCPricingType = true;
                            }
                        }
                    }
                    else 
                    {
                        milesPlusMoneyCreditResponse.Reservation.IsMoneyPlusMilesSelected = true; // when checkbox selected either from applyMM RTI or from payment
                        session.IsMoneyPlusMilesSelected = true;
                        if (await _shoppingCartUtility.IsEnableETCCreditsInBookingFeature(session?.CatalogItems))
                        {
                             session.IsEligibleForFSRPricingType = false;
                             milesPlusMoneyCreditResponse.Reservation.EligibleForETCPricingType = false;
                            
                        }
                    }
                    await _sessionHelperService.SaveSession<Session>(session, session.SessionId, new List<string> { session.SessionId, session.ObjectName }, session.ObjectName).ConfigureAwait(false);
                }
                if (!_configuration.GetValue<bool>("OmniCartTotalPriceBugFixForMoneyPlusMiles"))
                {
                    _shoppingCartUtility.BuildOmniCart(milesPlusMoneyCreditResponse?.ShoppingCart, milesPlusMoneyCreditResponse?.Reservation, request.Application);
                }
                #region OfferCode expansion Changes(This properties are added as part of offercode expansion changes but can be later used in general )
                milesPlusMoneyCreditResponse.ShoppingCart.FormofPaymentDetails.IsFOPRequired = _shoppingCartUtility.IsFOPRequired(milesPlusMoneyCreditResponse.ShoppingCart, milesPlusMoneyCreditResponse.Reservation);
                milesPlusMoneyCreditResponse.ShoppingCart.FormofPaymentDetails.IsEnableAgreeandPurchaseButton = _shoppingCartUtility.IsEnableAgreeandPurchaseButton(milesPlusMoneyCreditResponse.ShoppingCart, milesPlusMoneyCreditResponse.Reservation);
                milesPlusMoneyCreditResponse.ShoppingCart.FormofPaymentDetails.MaskedPaymentMethod =await _shoppingCartUtility.AssignMaskedPaymentMethod(milesPlusMoneyCreditResponse.ShoppingCart,request.Application).ConfigureAwait(false);
                #endregion OfferCode expansion Changes
                milesPlusMoneyCreditResponse.PkDispenserPublicKey = await _pKDispenserPublicKey.GetCachedOrNewpkDispenserPublicKey(request.Application.Id, request.Application.Version.Major, request.DeviceId, session.SessionId, session.Token, session?.CatalogItems).ConfigureAwait(false);
            }
            catch
            {
                throw new MOBUnitedException(milesMoneyErrorMessage != null ? milesMoneyErrorMessage : _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }

            return milesPlusMoneyCreditResponse;
        }

        private async Task AssignUpdatedPricesToReservation(string sessionId, FlightReservationResponse cslReservation, string optionId, List<MOBProdDetail> products,
            bool isMoneyPlusMilesFromRTI = false, Session session = null, MOBRequest request = null)
        {
            Reservation bookingPathReservation = new Reservation();
            bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(sessionId, bookingPathReservation.ObjectName, new List<string> { sessionId, bookingPathReservation.ObjectName }).ConfigureAwait(false);

            List<DisplayPrice> displayPrices = cslReservation?.DisplayCart?.DisplayPrices;

            if (displayPrices != null)
            {
                foreach (var price in displayPrices)
                {
                    if (string.IsNullOrEmpty(price.Description))
                    {
                        price.Description = price.Type;
                    }
                }
            }

            BuildMoneyPlusMilesPrice(bookingPathReservation.Prices, displayPrices, string.IsNullOrEmpty(optionId), products, isMoneyPlusMilesFromRTI,cslReservation?.DisplayCart, session);
            if(await _featureSettings.GetFeatureSettingValue("EnableChaseStemtentFixForMoneyPlusMiles-MOBILE-35118"))
                 _shoppingCartUtility.UpdateChaseCreditStatement(bookingPathReservation);
            AddGrandTotalIfNotExistInPrices(bookingPathReservation.Prices);
            await _sessionHelperService.SaveSession(bookingPathReservation, sessionId, new List<string> { sessionId, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName).ConfigureAwait(false);
        }

        private void AddGrandTotalIfNotExistInPrices(List<MOBSHOPPrice> prices)
        {
            var grandTotalPrice = prices.FirstOrDefault(p => p.DisplayType.ToUpper().Equals("GRAND TOTAL"));
            if (grandTotalPrice == null)
            {
                var totalPrice = prices.FirstOrDefault(p => p.DisplayType.ToUpper().Equals("TOTAL"));
                grandTotalPrice = BuildGrandTotalPriceForReservation(totalPrice.Value);
                prices.Add(grandTotalPrice);
            }
        }

        private MOBSHOPPrice BuildGrandTotalPriceForReservation(double grandtotal)
        {
            grandtotal = Math.Round(grandtotal, 2, MidpointRounding.AwayFromZero);
            MOBSHOPPrice totalPrice = new MOBSHOPPrice();
            totalPrice.CurrencyCode = "USD";
            totalPrice.DisplayType = "Grand Total";
            totalPrice.DisplayValue = grandtotal.ToString("N2", CultureInfo.InvariantCulture);
            totalPrice.FormattedDisplayValue = formatAmountForDisplay(totalPrice.DisplayValue, GetCultureInfo(totalPrice.CurrencyCode), false); //string.Format("${0:c}", totalPrice.DisplayValue);
            double tempDouble1 = 0;
            double.TryParse(totalPrice.DisplayValue.ToString(), out tempDouble1);
            totalPrice.Value = Math.Round(tempDouble1, 2, MidpointRounding.AwayFromZero);
            totalPrice.PriceType = "Grand Total";
            return totalPrice;
        }

        private CultureInfo GetCultureInfo(string currencyCode)
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


        private string formatAmountForDisplay(string amt, CultureInfo ci, bool roundup = true, bool isAward = false)
        {
            string newAmt = amt;
            decimal amount = 0;
            decimal.TryParse(amt, out amount);

            try
            {

                string currencySymbol = "";

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
                        //currencySymbol = GetCurrencySymbol(currency.ToUpper());
                        //newAmt = currencySymbol + newAmt;
                        newAmt = GetCurrencySymbol(ci, amount, roundup);
                        break;
                }

            }
            catch { }

            return isAward ? "+ " + newAmt : newAmt;
        }

        private string GetCurrencySymbol(CultureInfo ci, decimal amount, bool roundup)
        {
            string result = string.Empty;

            try
            {
                if (amount > -1)
                {
                    if (roundup)
                    {
                        int newTempAmt = (int)decimal.Ceiling(amount);
                        try
                        {
                            var ri = new RegionInfo(ci.Name);
                            CultureInfo tempCi = Thread.CurrentThread.CurrentCulture;
                            Thread.CurrentThread.CurrentCulture = ci;
                            result = newTempAmt.ToString("c0");
                            Thread.CurrentThread.CurrentCulture = tempCi;
                        }
                        catch { }
                    }
                    else
                    {
                        try
                        {
                            var ri = new RegionInfo(ci.Name);
                            CultureInfo tempCi = Thread.CurrentThread.CurrentCulture;
                            Thread.CurrentThread.CurrentCulture = ci;
                            result = amount.ToString("c");
                            Thread.CurrentThread.CurrentCulture = tempCi;
                        }
                        catch { }
                    }
                }

            }
            catch { }

            return result;
        }

        private bool BuildMoneyPlusMilesPrice(List<MOBSHOPPrice> prices, List<DisplayPrice> displayPrices, bool isRemove, List<MOBProdDetail> products, 
            bool isMoneyPlusMilesFromRTI = false, DisplayCart displayCart = null, Session session= null)
        {
            bool isDirty = false;
            double mmValue = 0;
            double previousMMValue = 0;
            if (_configuration.GetValue<bool>("EnableFSRMoneyPlusMilesFeature") && isMoneyPlusMilesFromRTI && prices.LastOrDefault(p => p.DisplayType == "MILESANDMONEY") != null)
            {
                previousMMValue = prices.LastOrDefault(p => p.DisplayType == "MILESANDMONEY").Value;
                // Currently since the grand total is always updated we have to find a way to update the grand total as the persist is storing the old value at this 
            }
            if (isRemove)
            {
                var mmPrice = prices.FirstOrDefault(p => p.DisplayType == "MILESANDMONEY");
                if (mmPrice != null)
                {
                    mmValue = mmPrice.Value;
                    prices.Remove(mmPrice);
                    isDirty = true;
                }
                if (_shoppingCartUtility.IsEnableMoneyPlusMilesFeature(catalogItems: session?.CatalogItems))
                {
                    var mplusmPrice = prices.FirstOrDefault(p => p.DisplayType == "MONEYPLUSMILES");
                    if (mplusmPrice != null)
                    {
                        prices.Remove(mplusmPrice);
                        isDirty = true;
                    }
                }
            }
            else
            {
                var mmPrice = displayPrices.FirstOrDefault(p => p.Type.Equals("MILESANDMONEY", StringComparison.OrdinalIgnoreCase));
                if (mmPrice != null && mmPrice.Amount > 0)
                {
                    if(_shoppingCartUtility.IsEnableMoneyPlusMilesFeature(catalogItems: session?.CatalogItems))
                    {
                        try
                        {
                            var moneyAndMiles = prices.FirstOrDefault(p => p.DisplayType.Equals("MILESANDMONEY", StringComparison.OrdinalIgnoreCase));
                            if (moneyAndMiles == null)
                            {
                                mmValue = AddMoneyAndMilesToPrices(prices, mmPrice, session);
                            }
                            else
                            {
                                mmValue = UpdateMoneyAndMilesToPrices(mmPrice, moneyAndMiles);
                            }
                        }catch(Exception ex)
                        {
                            _logger.ILoggerWarning("MoneyAndMiles RTI BuildMoneyPlusMilesPrice" + ex.Message);
                        }
                    }
                    else
                    {
                        mmValue = AddMoneyAndMilesToPrices(prices, mmPrice, session);
                    }
                    isDirty = true;
                    
                    if (_shoppingCartUtility.IsEnableMoneyPlusMilesFeature(catalogItems: session?.CatalogItems) && isRemove == false)
                    {
                        try
                        {
                            var mPlusmPrice = prices.FirstOrDefault(p => p.DisplayType.Equals("MONEYPLUSMILES", StringComparison.OrdinalIgnoreCase));
                            if (mPlusmPrice == null)
                            {
                                MOBSHOPPrice moneyPlusMilesRTI = new MOBSHOPPrice();
                                moneyPlusMilesRTI.DisplayType = "MONEYPLUSMILES"; // Used for RTI screen display
                                var miles = mmPrice?.SubItems?.Where(a => a.Type == "Miles").FirstOrDefault().Amount.ToString();
                                moneyPlusMilesRTI.FormattedDisplayValue = " + " + UtilityHelper.FormatAwardAmountForDisplay(miles, false);
                                moneyPlusMilesRTI.PriceTypeDescription = "";
                                prices.Add(moneyPlusMilesRTI);
                            }
                            else
                            {
                                var updatePrice = prices.FirstOrDefault(a => a.DisplayType.Equals("MONEYPLUSMILES", StringComparison.OrdinalIgnoreCase));
                                if (updatePrice != null)
                                {
                                    var miles = mmPrice?.SubItems?.Where(a => a.Type == "Miles").FirstOrDefault().Amount.ToString();
                                    updatePrice.FormattedDisplayValue = " + " + UtilityHelper.FormatAwardAmountForDisplay(miles, false);
                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.ILoggerWarning("MoneyAndMiles RTI BuildMoneyPlusMilesPrice Update MneyPlusMiles" + ex.Message);
                        }
                    }
                }
            }

            if (isDirty)
            {
                if (_configuration.GetValue<bool>("EnableFSRMoneyPlusMilesFeature") && isMoneyPlusMilesFromRTI)
                {
                    DisplayPrice displayPrice = displayPrices.FirstOrDefault(p => p.Type == "Total");
                    var price = prices.FirstOrDefault(p => p.DisplayType == "TOTAL");
                    UpdateMoneyPlusMilesRTIPriceValuesFromCSL(price, displayPrice.Amount);
                    price = prices.FirstOrDefault(p => p.DisplayType.ToUpper().Equals("GRAND TOTAL"));
                    var grandTotalAfterApplyMM =  (price.Value + previousMMValue) ;
                    if (_configuration.GetValue<bool>("EnableMoneyPlusMilesGrandTotalFix"))
                        grandTotalAfterApplyMM = (isRemove == false) ? (grandTotalAfterApplyMM - mmValue) : grandTotalAfterApplyMM;
                    else
                        grandTotalAfterApplyMM = Convert.ToDouble(displayCart?.GrandTotal);
                    UpdateMoneyPlusMilesRTIPriceValuesFromCSL(price, Convert.ToDecimal(grandTotalAfterApplyMM));

                    price = prices.FirstOrDefault(p => p.DisplayType == "RES");
                    UpdateCertificateRedeemAmountFromTotalInReserationPrices(price, mmValue, !isRemove);
                    var scRESProduct = products.Find(p => p.Code == "RES");
                  
                    UpdateCertificateRedeemAmountInSCProductPricesForRTIMoneyMiles(scRESProduct, mmValue, previousMMValue, isRemove);
                }
                else
                {
                    var price = prices.FirstOrDefault(p => p.DisplayType == "RES");
                    UpdateCertificateRedeemAmountFromTotalInReserationPrices(price, mmValue, !isRemove);
                    price = prices.FirstOrDefault(p => p.DisplayType == "TOTAL");
                    UpdateCertificateRedeemAmountFromTotalInReserationPrices(price, mmValue, !isRemove);
                    price = prices.FirstOrDefault(p => p.DisplayType.ToUpper().Equals("GRAND TOTAL"));
                    UpdateCertificateRedeemAmountFromTotalInReserationPrices(price, mmValue, !isRemove);
                    var scRESProduct = products.Find(p => p.Code == "RES");
                    UpdateCertificateRedeemAmountInSCProductPrices(scRESProduct, mmValue, !isRemove);
                }
               
            }

            return isDirty;
        }

        private double UpdateMoneyAndMilesToPrices(DisplayPrice mmPrice, MOBSHOPPrice moneyAndMiles)
        {
            double mmValue;
            double tempDouble = 0;
            decimal? milesDisplay = 0;
            double.TryParse(mmPrice.Amount.ToString(), out tempDouble);
            moneyAndMiles.Value = Math.Round(tempDouble, 2, MidpointRounding.AwayFromZero);
            CultureInfo ci = TopHelper.GetCultureInfo(mmPrice.Currency);
            if (_configuration.GetValue<bool>("EnableFSRMoneyPlusMilesFeature"))
            {
                milesDisplay = mmPrice?.SubItems?.Where(a => a.Type == "Miles").FirstOrDefault()?.Amount;
                moneyAndMiles.FormattedDisplayValue = "-" + TopHelper.FormatAmountForDisplay(mmPrice.Amount, ci, false) + " (" + UtilityHelper.FormatAwardAmountForDisplay(milesDisplay.ToString(), false) + ")";
            }
            else
            {
                moneyAndMiles.FormattedDisplayValue = "-" + TopHelper.FormatAmountForDisplay(mmPrice.Amount, ci, false); // Money and Miles have to be displayed in -ve format as per MOBILE-14807
            }
            
            mmValue = moneyAndMiles.Value;
            return mmValue;
        }

        private double AddMoneyAndMilesToPrices(List<MOBSHOPPrice> prices, DisplayPrice mmPrice, Session session)
        {
            double mmValue;
            decimal? milesDisplay = 0;
            MOBSHOPPrice bookingPrice = new MOBSHOPPrice();
            bookingPrice.CurrencyCode = mmPrice.Currency;
            bookingPrice.DisplayType = mmPrice.Type;
            bookingPrice.Status = mmPrice.Status;
            bookingPrice.Waived = mmPrice.Waived;
            bookingPrice.DisplayValue = string.Format("{0:#,0.00}", mmPrice.Amount);
            bookingPrice.PriceTypeDescription = "Miles applied";

            double tempDouble = 0;
            double.TryParse(mmPrice.Amount.ToString(), out tempDouble);
            bookingPrice.Value = Math.Round(tempDouble, 2, MidpointRounding.AwayFromZero);
            CultureInfo ci = TopHelper.GetCultureInfo(mmPrice.Currency);
            bookingPrice.FormattedDisplayValue = "-" + TopHelper.FormatAmountForDisplay(mmPrice.Amount, ci, false); // Money and Miles have to be displayed in -ve format as per MOBILE-14807
            if (_shoppingCartUtility.IsEnableMoneyPlusMilesFeature(catalogItems: session?.CatalogItems))
            {
                milesDisplay = mmPrice?.SubItems?.Where(a => a.Type == "Miles").FirstOrDefault()?.Amount;
                bookingPrice.FormattedDisplayValue = bookingPrice.FormattedDisplayValue + " (" + UtilityHelper.FormatAwardAmountForDisplay(milesDisplay.ToString(), false) + ")";
            }            
            
            prices.Add(bookingPrice);
            mmValue = bookingPrice.Value;
            return mmValue;
        }

        private static MOBSHOPPrice UpdateMoneyPlusMilesRTIPriceValuesFromCSL(MOBSHOPPrice price, Decimal displayPrice)
        {
            price.Value = Math.Round(Convert.ToDouble(displayPrice), 2, MidpointRounding.AwayFromZero);
            price.FormattedDisplayValue = (displayPrice).ToString("C2", CultureInfo.CurrentCulture);
            price.DisplayValue = string.Format("{0:#,0.00}", displayPrice);
            return price;
        }

        private static MOBProdDetail UpdateCertificateRedeemAmountInSCProductPricesForRTIMoneyMiles(MOBProdDetail scProduct, double mmValue, double previousMMValue, bool isRemove)
        {
            if (scProduct != null)
            {
                double prodValue = Convert.ToDouble(scProduct.ProdTotalPrice);
                var scRESProductPrice = (prodValue + previousMMValue);
                scRESProductPrice = (isRemove == false) ? (scRESProductPrice - mmValue) : scRESProductPrice;
                prodValue = Math.Round(scRESProductPrice, 2, MidpointRounding.AwayFromZero);
                double prodValueAfterUpdate;
                prodValueAfterUpdate = Math.Round(prodValue, 2, MidpointRounding.AwayFromZero);
                scProduct.ProdTotalPrice = (prodValueAfterUpdate).ToString("N2", CultureInfo.CurrentCulture);
                scProduct.ProdDisplayTotalPrice = (prodValueAfterUpdate).ToString("C2", new CultureInfo("en-US"));
            }
            return scProduct;
        }

        private void UpdateCertificateRedeemAmountInSCProductPrices(MOBProdDetail scProduct, double value, bool isRemove = true)
        {
            if (scProduct != null)
            {
                double prodValue = Convert.ToDouble(scProduct.ProdTotalPrice);
                prodValue = Math.Round(prodValue, 2, MidpointRounding.AwayFromZero);
                double prodValueAfterUpdate;
                if (isRemove)
                {
                    prodValueAfterUpdate = prodValue - value;
                }
                else
                {
                    prodValueAfterUpdate = prodValue + value;
                }
                prodValueAfterUpdate = Math.Round(prodValueAfterUpdate, 2, MidpointRounding.AwayFromZero);
                scProduct.ProdTotalPrice = (prodValueAfterUpdate).ToString("N2", CultureInfo.CurrentCulture);
                scProduct.ProdDisplayTotalPrice = (prodValueAfterUpdate).ToString("C2", new CultureInfo("en-US"));
            }
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
                price.FormattedDisplayValue = (price.Value).ToString("C2", new CultureInfo("en-US"));
                price.DisplayValue = string.Format("{0:#,0.00}", price.Value);
            }
        }

        private async Task<FlightReservationResponse> ApplyCSLMilesPlusMoneyOptions(Session session, ApplyMoneyPlusMilesOptionRequest request, string optionId)
        {
            FlightReservationResponse response = new FlightReservationResponse();
            string actionName = string.IsNullOrEmpty(optionId) ? "UndoMilesAndMoneyOption" : "ApplyMilesAndMoneyOption";

            ApplyMilesAndMoneyOptionRequest cslRequest = new ApplyMilesAndMoneyOptionRequest();
            cslRequest.CartId = session.CartId;
            var cartInfo = await GetCartInformation(session.SessionId, request.Application, session.DeviceID, session.CartId, session.Token);
            cslRequest.Reservation = cartInfo.Reservation;
            cslRequest.DisplayCart = cartInfo.DisplayCart;
            cslRequest.OptionId = optionId;

            string jsonRequest = JsonConvert.SerializeObject(cslRequest);            
            string cslresponse = await _flightShoppingProductsService.ApplyCSLMilesPlusMoneyOptions(session.Token, actionName, jsonRequest, session.SessionId).ConfigureAwait(false);
            response = JsonConvert.DeserializeObject<FlightReservationResponse>(cslresponse);
            await RegisterFlights(response, session, request).ConfigureAwait(false);

            return response;
        }

        private async Task<FlightReservationResponse> RegisterFlights(FlightReservationResponse flightReservationResponse, Session session, MOBRequest request)
        {
            string flow = session.Flow;
            var registerFlightRequest = BuildRegisterFlightRequest(flightReservationResponse, flow, request, session.CartId);
            string jsonRequest = JsonConvert.SerializeObject(registerFlightRequest);
            FlightReservationResponse response = new FlightReservationResponse();
            string url = "RegisterFlights";            
            response = await _shoppingCartService.RegisterFlights<FlightReservationResponse>(session.Token, url, jsonRequest, session.SessionId).ConfigureAwait(false);
            if (response != null)
            {
                if (!(response != null && response.Status.Equals(United.Services.FlightShopping.Common.StatusType.Success) && response.DisplayCart != null && response.Reservation != null))
                {
                    if (response.Errors != null && response.Errors.Count > 0)
                    {
                        string errorMessage = string.Empty;
                        foreach (var error in response.Errors)
                        {
                            errorMessage = errorMessage + " " + error.Message;
                        }
                        throw new System.Exception(errorMessage);
                    }
                }
            }
            else
            {
                throw new MOBUnitedException(milesMoneyErrorMessage != null ? milesMoneyErrorMessage : _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }

            return response;
        }

        private RegisterFlightsRequest BuildRegisterFlightRequest(United.Services.FlightShopping.Common.FlightReservation.FlightReservationResponse flightReservationResponse, string flow, MOBRequest mobRequest, string cartid)
        {
            RegisterFlightsRequest request = new RegisterFlightsRequest();
            request.CartId = cartid;
            request.CartInfo = flightReservationResponse.DisplayCart;
            request.CountryCode = flightReservationResponse.DisplayCart.CountryCode;//TODO:Check this is populated all the time.
            request.Reservation = flightReservationResponse.Reservation;
            request.DeviceID = mobRequest.DeviceId;
            request.Upsells = flightReservationResponse.Upsells;
            request.MerchOffers = flightReservationResponse.MerchOffers;
            request.LoyaltyUpgradeOffers = flightReservationResponse.LoyaltyUpgradeOffers;
            request.WorkFlowType = GetWorkFlowType(flow);
            request.AppliedMilesAndMoney = true;

            _logger.LogInformation("BuildRegisterFlightRequest Request{request} with SessionId{sessoinId}", JsonConvert.SerializeObject(request), _headers.ContextValues.SessionId);

            return request;
        }

        private WorkFlowType GetWorkFlowType(string flow)
        {
            switch (flow)
            {
                case "CHECKIN":
                    return WorkFlowType.CheckInProductsPurchase;

                case "BOOKING":
                    return WorkFlowType.InitialBooking;

                case "VIEWRES":
                case "POSTBOOKING":
                    return WorkFlowType.PostPurchase;

                case "RESHOP":
                    return WorkFlowType.Reshop;

                case "FARELOCK":
                    return WorkFlowType.FareLockPurchase;
            }

            return WorkFlowType.UnKnown;
        }


        private async Task<LoadReservationAndDisplayCartResponse> GetCartInformation(
            string sessionId, MOBApplication application, string deviceid, string cartId, string token,
            WorkFlowType workFlowType = WorkFlowType.InitialBooking)
        {
            LoadReservationAndDisplayCartRequest loadReservationAndDisplayCartRequest = new LoadReservationAndDisplayCartRequest();
            LoadReservationAndDisplayCartResponse loadReservationAndDisplayResponse = new LoadReservationAndDisplayCartResponse();
            loadReservationAndDisplayCartRequest.CartId = cartId;
            loadReservationAndDisplayCartRequest.WorkFlowType = workFlowType;
            string jsonRequest = Newtonsoft.Json.JsonConvert.SerializeObject(loadReservationAndDisplayCartRequest);

            string actionName = "LoadReservationAndDisplayCart";

            loadReservationAndDisplayResponse = await _shoppingCartService.GetCartInformation<LoadReservationAndDisplayCartResponse>(token, actionName, jsonRequest, sessionId).ConfigureAwait(false);

            //loadReservationAndDisplayResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<LoadReservationAndDisplayCartResponse>(jsonResponse);
            return loadReservationAndDisplayResponse;
        }

        public async Task< MOBFOPResponse> GetMilesPlusMoneyOptions(Session session, GetMoneyPlusMilesRequest request)
        {
            MOBFOPResponse milesPlusMoneyCreditResponse = new MOBFOPResponse();
            try
            {
                List<CMSContentMessage> lstMessages = await _shoppingCartUtility.GetSDLContentByGroupName(request, session.SessionId, session.Token, _configuration.GetValue<string>("CMSContentMessages_GroupName_BookingRTI_Messages"), _configuration.GetValue<string>("BookingPathRTI_CMSContentMessagesCached_StaticGUID"));

                MOBShoppingCart shoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(session.SessionId, new MOBShoppingCart().ObjectName, new List<string> { session.SessionId, new MOBShoppingCart().ObjectName }).ConfigureAwait(false);

                int totalMilesAvailable = 0;
                string mileagePlusTraveler = string.Empty;
                var cartInfo = await GetCartInformation(session.SessionId, request.Application, session.DeviceID, session.CartId, session.Token);
                if (cartInfo?.Reservation?.Sponsor?.AccountBalances?.Count > 0)
                {
                    totalMilesAvailable = cartInfo.Reservation.Sponsor.AccountBalances[0].Balance;
                    mileagePlusTraveler = $"{cartInfo.Reservation.Sponsor.GivenName} {cartInfo.Reservation.Sponsor.Surname}";
                }

                var milesAndMoneyOptions = await GetCSLMilesPlusMoneyOptions(session, request, cartInfo).ConfigureAwait(false);

                var milesAndMoneyCredit = BuildMilePlusMoneyCredit(milesAndMoneyOptions, lstMessages, totalMilesAvailable);

                milesAndMoneyCredit.MileagePlusTraveler = mileagePlusTraveler;

                await LoadSessionValuesToResponse(session, milesPlusMoneyCreditResponse, milesAndMoneyCredit, "", null, lstMessages);
            }
            catch
            {
                throw new MOBUnitedException(milesMoneyErrorMessage != null ? milesMoneyErrorMessage : _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }

            return milesPlusMoneyCreditResponse;
        }

        private MOBFOPMoneyPlusMilesCredit BuildMilePlusMoneyCredit(MilesAndMoneyResponse milesAndMoneyOptions, List<CMSContentMessage> lstMessages, int totalMilesAvailable)
        {
            var milesPlusMoneyCredit = new MOBFOPMoneyPlusMilesCredit();
            milesPlusMoneyCredit.MMCMessages = GetMMCContentMessages(lstMessages);
            milesPlusMoneyCredit.ReviewMMCMessage = GetReviewContentMessages(lstMessages);
            milesPlusMoneyCredit.MilesPlusMoneyOptions = LoadMilesPlusMoneyOptions(milesAndMoneyOptions, totalMilesAvailable);
            milesPlusMoneyCredit.TotalMilesAvailable = totalMilesAvailable.ToString("#,##0");

            return milesPlusMoneyCredit;
        }

        private List<MOBFOPMoneyPlusMiles> LoadMilesPlusMoneyOptions(MilesAndMoneyResponse milesAndMoneyOptions, int totalMilesAvailable)
        {
            List<MOBFOPMoneyPlusMiles> milesMoneyOptions = new List<MOBFOPMoneyPlusMiles>();
            if (milesAndMoneyOptions?.MilesAndMoneyOptions != null && milesAndMoneyOptions?.MilesAndMoneyOptions.Count > 0)
            {
                foreach (var milesPlusMoneyOption in milesAndMoneyOptions.MilesAndMoneyOptions)
                {
                    if (milesPlusMoneyOption != null)
                    {
                        var milesMoneyOption = new MOBFOPMoneyPlusMiles
                        {
                            OptionId = milesPlusMoneyOption.OptionId,
                            ConversionRate = milesPlusMoneyOption.ConversionRate,
                            MilesMoneyValue = milesPlusMoneyOption.MilesMoneyValue,
                            MilesOwed = milesPlusMoneyOption.MilesOwed,
                            MilesPercentage = milesPlusMoneyOption.MilesPercentage,
                            MoneyOwed = milesPlusMoneyOption.MoneyOwed,
                            MilesApplied = milesPlusMoneyOption.MilesOwed.ToString("#,##0"),
                            MoneyDiscountForMiles = milesMoneyValueDesc != string.Empty ? string.Format(milesMoneyValueDesc, milesPlusMoneyOption.MilesMoneyValue.ToString("#,##0.00")) : milesPlusMoneyOption.MilesMoneyValue.ToString("#,##0.00"),
                            MilesRemaining = (totalMilesAvailable - milesPlusMoneyOption.MilesOwed).ToString("#,##0"),
                            MoneyBalanceDue = "$" + milesPlusMoneyOption.MoneyOwed.ToString("#,##0.00"),
                            ReviewMilesApplied = "-$" + milesPlusMoneyOption.MilesMoneyValue.ToString("#,##0.00"),
                            Fare = "$" + (milesPlusMoneyOption.MilesMoneyValue + milesPlusMoneyOption.MoneyOwed).ToString("#,##0.00")
                        };
                        milesMoneyOptions.Add(milesMoneyOption);
                    }
                }
            }
            else
            {
                throw new System.Exception("No options returned in GetMilesAndMoneyOptions CSL service call");
            }

            return milesMoneyOptions;
        }

        private List<MOBMobileCMSContentMessages> GetReviewContentMessages(List<CMSContentMessage> lstMessages)
        {
            List<MOBMobileCMSContentMessages> reviewMCMMessage = new List<MOBMobileCMSContentMessages>();

            reviewMCMMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.ReviewMMCMessage.Includestaxandfees"));
            reviewMCMMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.ReviewMMCMessage.PriceBreakdown"));
            reviewMCMMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.ReviewMMCMessage.MileagePlusTraveler"));
            reviewMCMMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.ReviewMMCMessage.AvailableMiles"));
            reviewMCMMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.ReviewMMCMessage.RemainingMiles"));
            reviewMCMMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.ReviewMMCMessage.Fare"));
            reviewMCMMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.ReviewMMCMessage.MilesApplied"));
            reviewMCMMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.ReviewMMCMessage.MoneyBalanceDue"));
            reviewMCMMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.ReviewMMCMessage.ErrorMsg"));
            reviewMCMMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.AboutMMCMessage.TransactionFailed"));
            reviewMCMMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.ReviewMMCMessage.BtnRemoveMoneyPlusMiles"));

            return reviewMCMMessage;
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

        private List<MOBMobileCMSContentMessages> GetMMCContentMessages(List<CMSContentMessage> lstMessages)
        {
            List<MOBMobileCMSContentMessages> mmcMessage = new List<MOBMobileCMSContentMessages>();

            milesMoneyValueDesc = GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.MMCMessage.MilesMoneyValue").FirstOrDefault()?.ContentFull; // Assigning MilesMoneyValue generic description string ${0} off on this trip
            mmcMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.MMCMessage.Information"));
            mmcMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.MMCMessage.BtnText"));
            mmcMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.MMCMessage.SelectMiles"));
            mmcMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.MMCMessage.MilesApplied"));
            mmcMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.MMCMessage.MoneyBalanceDue"));
            mmcMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.MMCMessage.AvailableMiles"));
            mmcMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.MMCMessage.MilesRemaining"));
            mmcMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.MMCMessage.TotalPrice"));
            mmcMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.MMCMessage.AvailableMilesTrip"));
            if (!_configuration.GetValue<bool>("isEnableDueInformation"))
            {
                mmcMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.MMCMessage.DueInformation1"));
                mmcMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.MMCMessage.DueInformation2"));
                mmcMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.MMCMessage.DueInformation"));
            }
            var errorMsg = GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.ReviewMMCMessage.ErrorMsg");
            milesMoneyErrorMessage = errorMsg.FirstOrDefault()?.ContentShort;
            mmcMessage.AddRange(errorMsg);
            mmcMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.AboutMMCMessage.AboutMoneyPlusMiles"));
            mmcMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.AboutMMCMessage.TransactionFailed"));
            mmcMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.AboutMMCMessage.MoneyPlusMilesMsg"));
            mmcMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.AboutMMCMessage.BtnBackToMoneyPlusMiles"));

            return mmcMessage;
        }

        private async Task LoadSessionValuesToResponse(Session session, MOBFOPResponse response, MOBFOPMoneyPlusMilesCredit moneyPlusMilesCredit = null, string optionId = "", MOBShoppingCart shoppingCart = null, 
            List<CMSContentMessage> lstMessages = null, bool isMoneyMilesFromRTI  = false)
        {

            response.ShoppingCart = shoppingCart ?? await _sessionHelperService.GetSession<MOBShoppingCart>(session.SessionId, new MOBShoppingCart().ObjectName, new List<string> { session.SessionId, new MOBShoppingCart().ObjectName }).ConfigureAwait(false);

            if (response.ShoppingCart == null)
            {
                response.ShoppingCart = new MOBShoppingCart();
            }
            if (response.ShoppingCart?.FormofPaymentDetails == null)
            {
                response.ShoppingCart.FormofPaymentDetails = new MOBFormofPaymentDetails();
            }

            if (moneyPlusMilesCredit != null)
            {
                response.ShoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit = moneyPlusMilesCredit;
            }
            else
            {
                if (!string.IsNullOrEmpty(optionId))
                {
                    if (_configuration.GetValue<bool>("EnableFSRMoneyPlusMilesFeature") && isMoneyMilesFromRTI  && response.ShoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit == null  )
                    {
                        response.ShoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit = await _sessionHelperService.GetSession<MOBFOPMoneyPlusMilesCredit>(session.SessionId, new MOBFOPMoneyPlusMilesCredit().ObjectName, new List<string> { session.SessionId, new MOBFOPMoneyPlusMilesCredit().ObjectName }).ConfigureAwait(false);
                    }
                        response.ShoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit.SelectedMoneyPlusMiles = response.ShoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit.MilesPlusMoneyOptions.FirstOrDefault(mm => mm.OptionId == optionId);
                        var changeInTravelerMessage = string.Empty;
                        changeInTravelerMessage = GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.MMCMessage.RemoveCoupons")[0]?.ContentFull;
                        response.ShoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit.PromoCodeMoneyMilesAlertMessage = string.IsNullOrWhiteSpace(changeInTravelerMessage) ? null : new MOBSection
                        {
                            Text1 = changeInTravelerMessage,
                            Text2 = "Cancel",
                            Text3 = "Continue"
                        };
                }
                else
                {
                    if (_shoppingCartUtility.IsEnableMoneyPlusMilesFeature(catalogItems: session?.CatalogItems) && session.IsMoneyPlusMilesSelected)
                    {
                        if (response.ShoppingCart?.FormofPaymentDetails?.MoneyPlusMilesCredit != null)
                            response.ShoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit.SelectedMoneyPlusMiles = null;
                        if (response.ShoppingCart?.FormofPaymentDetails?.MoneyPlusMilesCredit?.PromoCodeMoneyMilesAlertMessage != null)
                            response.ShoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit.PromoCodeMoneyMilesAlertMessage = null;
                    }
                    else
                    {
                        response.ShoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit = null;
                    }
                }
            }

            await _sessionHelperService.SaveSession(response.ShoppingCart, session.SessionId, new List<string> { session.SessionId, new MOBShoppingCart().ObjectName }, new MOBShoppingCart().ObjectName).ConfigureAwait(false);

            response.SessionId = session.SessionId;
            response.Flow = string.IsNullOrEmpty(session.Flow) ? response.ShoppingCart.Flow : session.Flow;
            response.Reservation = new MOBSHOPReservation(_configuration, _cachingService);
            response.Reservation = await _iETCUtility.GetReservationFromPersist(response.Reservation, session);
            ProfileResponse persistedProfileResponse = new ProfileResponse();
            persistedProfileResponse = await _sessionHelperService.GetSession<ProfileResponse>(response.SessionId, persistedProfileResponse.ObjectName, new List<string> { response.SessionId, persistedProfileResponse.ObjectName }).ConfigureAwait(false);

            response.Profiles = persistedProfileResponse != null ? persistedProfileResponse.Response.Profiles : null;
        }

        private async Task<MilesAndMoneyResponse> GetCSLMilesPlusMoneyOptions(Session session, MOBRequest mobRequest, LoadReservationAndDisplayCartResponse cartInfo)
        {
            MilesAndMoneyResponse response = new MilesAndMoneyResponse();
            string url = "GetMilesAndMoneyOptions";

            MilesAndMoneyRequest cslRequest = new MilesAndMoneyRequest();
            cslRequest.CartId = session.CartId;
            cslRequest.Reservation = cartInfo.Reservation;
            cslRequest.Characteristics = new System.Collections.ObjectModel.Collection<Service.Presentation.CommonModel.Characteristic>() { new Service.Presentation.CommonModel.Characteristic() { Code = "LOADFROMSESSION", Value = "True" } };
            string jsonRequest = JsonConvert.SerializeObject(cslRequest);
            string cslresponse = await _flightShoppingProductsService.GetCSLMilesPlusMoneyOptions(session.Token, url, jsonRequest, session.SessionId).ConfigureAwait(false);
            response = JsonConvert.DeserializeObject<MilesAndMoneyResponse>(cslresponse);
            return response;
        }

        public async Task GetMoneyPlusMilesOptionsForFinalRTIScreen(MOBRegisterSeatsRequest request, MOBBookingRegisterSeatsResponse response, United.Persist.Definition.Shopping.Session session)
        {
            // For Money Plus Miles feature on RTI there is a need to make GetMoneyPlus miles all in the review booking screen if they are eligible for M+M
            GetMoneyPlusMilesRequest moneyPlusMilesRequest = CreateGetMoneyPlusMilesRequest(request);
            var moneyPlusMilesOptions = await GetMilesPlusMoneyOptions(session, moneyPlusMilesRequest);
            response.ShoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit = moneyPlusMilesOptions.ShoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit;
            response.ShoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit.SelectedMoneyPlusMiles = moneyPlusMilesOptions.ShoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit.MilesPlusMoneyOptions.LastOrDefault();
            if (moneyPlusMilesOptions.ShoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit != null && _shoppingCartUtility.IsIncludeMoneyMilesInRTI(response.EligibleFormofPayments))
                response.Reservation.IsEligibleForMoneyPlusMiles = true; // Indicator for Display the M+M on RTI screen 
            if (session.IsMoneyPlusMilesSelected)
                response.Reservation.IsMoneyPlusMilesSelected = true;// Indiactor for UI display the M+M Option with checkbox selected
            if (await _shoppingCartUtility.IsEnableETCCreditsInBookingFeature(session.CatalogItems))
            {
                response.Reservation.EligibleForETCPricingType = false;

            }
        }

        private static GetMoneyPlusMilesRequest CreateGetMoneyPlusMilesRequest(MOBRegisterSeatsRequest request)
        {
            GetMoneyPlusMilesRequest moneyPlusMilesRequest = new GetMoneyPlusMilesRequest();
            moneyPlusMilesRequest.Application = request.Application;
            moneyPlusMilesRequest.AccessCode = request.AccessCode;
            moneyPlusMilesRequest.Flow = request.Flow;
            moneyPlusMilesRequest.LanguageCode = request.LanguageCode;
            moneyPlusMilesRequest.MileagePlusNumber = request.MileagePlusNumber;
            moneyPlusMilesRequest.SessionId = request.SessionId;
            moneyPlusMilesRequest.TransactionId = request.TransactionId;
            return moneyPlusMilesRequest;
        }

        private static GetMoneyPlusMilesRequest CreateGetMoneyPlusMilesRequest(ApplyMoneyPlusMilesOptionRequest request)
        {
            GetMoneyPlusMilesRequest moneyPlusMilesRequest = new GetMoneyPlusMilesRequest();
            moneyPlusMilesRequest.Application = request.Application;
            moneyPlusMilesRequest.AccessCode = request.AccessCode;
            moneyPlusMilesRequest.Flow = request.Flow;
            moneyPlusMilesRequest.LanguageCode = request.LanguageCode;
            moneyPlusMilesRequest.MileagePlusNumber = request.MileagePlusNumber;
            moneyPlusMilesRequest.SessionId = request.SessionId;
            moneyPlusMilesRequest.TransactionId = request.TransactionId;
            return moneyPlusMilesRequest;
        }
    }
}
