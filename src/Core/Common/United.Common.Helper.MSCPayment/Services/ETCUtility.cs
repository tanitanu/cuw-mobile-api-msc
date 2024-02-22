using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Common.Helper.MSCPayment.Interfaces;
using United.Definition;
using United.Definition.FormofPayment;
using United.Definition.Shopping;
using United.Definition.Shopping.PriceBreakDown;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.Product.Interfaces;
using United.Mobile.Model.Common;
using United.Persist.Definition.FOP;
using United.Persist.Definition.Shopping;
using United.Utility.Helper;

namespace United.Common.Helper.MSCPayment.Services
{
    public class ETCUtility : IETCUtility
    {
        private readonly IConfiguration _configuration;
        private readonly ISessionHelperService _sessionHelperService;
        private readonly ILegalDocumentsForTitlesService _legalDocumentsForTitlesService;
        private readonly ICachingService _cachingService;
        private readonly IShoppingCartUtility _shoppingCartUtility;
        private readonly IFeatureSettings _featureSettings;

        public ETCUtility(IConfiguration configuration
            , ISessionHelperService sessionHelperService
            , ILegalDocumentsForTitlesService legalDocumentsForTitlesService
            , ICachingService cachingService
            , IShoppingCartUtility shoppingCartUtility
            , IFeatureSettings featureSettings)
        {
            _configuration = configuration;
            _sessionHelperService = sessionHelperService;
            _legalDocumentsForTitlesService = legalDocumentsForTitlesService;
            _cachingService = cachingService;
            _shoppingCartUtility = shoppingCartUtility;
            _featureSettings = featureSettings;
        }

        public async Task<MOBFOPTravelerCertificateResponse> AddCertificatesToFOP(List<MOBFOPCertificate> requestedCertificates, Session session)
        {
            MOBFOPTravelerCertificateResponse response = new MOBFOPTravelerCertificateResponse();
            //below two lines are loading reservation and shoppingcart from persist
            Reservation bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(session.SessionId, new Reservation().ObjectName, new List<string> { session.SessionId, new Reservation().ObjectName }).ConfigureAwait(false);

            MOBFOPTravelCertificate travelerCertificate = await GetShoppingCartTravelCertificateFromPersist(session, response);

            if (!(_configuration.GetValue<bool>("EnableEtcforSeats_PCU_Viewres") && bookingPathReservation == null))
            {
                UpdateCertificateAmountInTotalPrices(bookingPathReservation.Prices, 0);
            }

            //Throwing exception, if any ETC already added and trying to add again
            IsRequestCertifiateAlreadyApplied(travelerCertificate.Certificates, requestedCertificates);

            //Clear IsRemove certificates from persisted TravelerCertificate object
            ClearIsRemoveCertificates(requestedCertificates, travelerCertificate.Certificates);

            //Add requested certificaates to TravelerCertificate object in FOP
            travelerCertificate.AllowedETCAmount = GetAlowedETCAmount(response.ShoppingCart.Products, response.ShoppingCart.Flow);
            requestedCertificates.ForEach(c => c.RedeemAmount = 0);
            AddRequestedCertificatesToFOPTravelerCertificates(requestedCertificates, response.ShoppingCart.ProfileTravelerCertificates, travelerCertificate);
            if (_configuration.GetValue<bool>("EnableSelectDifferentFOPAtRTI") && bookingPathReservation != null)
            {
                if (travelerCertificate.Certificates.Count == 0 && response.ShoppingCart.FormofPaymentDetails.CreditCard != null)
                    bookingPathReservation.ShopReservationInfo2.ShowSelectDifferentFOPAtRTI = true;
                else
                    bookingPathReservation.ShopReservationInfo2.ShowSelectDifferentFOPAtRTI = false;
            }

            if (_configuration.GetValue<bool>("EnableEtcforSeats_PCU_Viewres")
                && bookingPathReservation == null && response.ShoppingCart != null)
            {

                response.ShoppingCart.FormofPaymentDetails.TravelCertificate = travelerCertificate;
                AssignTotalAndCertificateItemsToPrices(response.ShoppingCart);
                AssignIsOtherFOPRequired(response.ShoppingCart.FormofPaymentDetails, response.ShoppingCart.Prices, false, travelerCertificate?.Certificates?.Count == 0);
            }
            else
            {
                await UpdateReservationWithCertificatePrices(session, response, travelerCertificate.TotalRedeemAmount, bookingPathReservation);
                AssignIsOtherFOPRequired(response.ShoppingCart.FormofPaymentDetails, bookingPathReservation.Prices, false, travelerCertificate?.Certificates?.Count == 0);
            }

            UpdateSavedCertificate(response.ShoppingCart);

            //Get Learmore Details
            await AssignLearmoreInformationDetails(travelerCertificate);
            travelerCertificate.ReviewETCMessages = await UpdateReviewETCAlertmessages(response.ShoppingCart);

           await _sessionHelperService.SaveSession<MOBShoppingCart>(response.ShoppingCart, session.SessionId, new List<string> { session.SessionId, response.ShoppingCart.ObjectName }, response.ShoppingCart.ObjectName).ConfigureAwait(false);

            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestedCertificates"></param>
        /// <param name="session"></param>
        /// <param name="certificate">Management response certificate</param>
        /// <returns></returns>
        public async Task<MOBFOPTravelerCertificateResponse> AddTravelCertificatesToFOP(List<MOBFOPCertificate> requestedCertificates, Session session, MOBFOPCertificate certificate)
        {
            // instead of requested certificates send requested certificate after management response 
            MOBFOPTravelerCertificateResponse response = new MOBFOPTravelerCertificateResponse();
            //below two lines are loading reservation and shoppingcart from persist
            Reservation bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(session.SessionId, new Reservation().ObjectName, new List<string> { session.SessionId, new Reservation().ObjectName }).ConfigureAwait(false);

            //Loads list of Travel Certificates -- which is not requried for MFOP
            MOBFOPTravelCertificate travelerCertificate = await GetShoppingCartTravelCertificateFromPersist(session, response);

            if (!(_configuration.GetValue<bool>("EnableEtcforSeats_PCU_Viewres") && bookingPathReservation == null))
            {
                UpdateCertificateAmountInTotalPrices(bookingPathReservation.Prices, 0);
            }

            //Clear IsRemove certificates from persisted TravelerCertificate object
            ClearIsRemoveCertificates(requestedCertificates, travelerCertificate.Certificates);

            //Add requested certificaates to TravelerCertificate object in FOP
            travelerCertificate.AllowedETCAmount = GetAlowedETCAmount(response.ShoppingCart.Products, response.ShoppingCart.Flow); // Not required for MFOP
            requestedCertificates.ForEach(c => c.RedeemAmount = 0);
            AddRequestedCertificatesToFOPTravelerCertificates(requestedCertificates, response.ShoppingCart.ProfileTravelerCertificates, travelerCertificate); // Not required 
            // instead use below directly to add certificate 
            //travelerCertificate.Certificates.Add(certificate);
            if (_configuration.GetValue<bool>("EnableSelectDifferentFOPAtRTI") && bookingPathReservation != null)
            {
                if (travelerCertificate.Certificates.Count == 0 && response.ShoppingCart.FormofPaymentDetails.CreditCard != null)
                    bookingPathReservation.ShopReservationInfo2.ShowSelectDifferentFOPAtRTI = true;
                else
                    bookingPathReservation.ShopReservationInfo2.ShowSelectDifferentFOPAtRTI = false;
            }

            if (_configuration.GetValue<bool>("EnableEtcforSeats_PCU_Viewres")
                && bookingPathReservation == null && response.ShoppingCart != null)
            {

                response.ShoppingCart.FormofPaymentDetails.TravelCertificate = travelerCertificate;
                AssignTotalAndCertificateItemsToPrices(response.ShoppingCart);
                AssignIsOtherFOPRequired(response.ShoppingCart.FormofPaymentDetails, response.ShoppingCart.Prices, false, travelerCertificate?.Certificates?.Count == 0);
            }
            else
            {
                await UpdateReservationWithCertificatePrices(session, response, travelerCertificate.TotalRedeemAmount, bookingPathReservation);
                AssignIsOtherFOPRequired(response.ShoppingCart.FormofPaymentDetails, bookingPathReservation.Prices, false, travelerCertificate?.Certificates?.Count == 0);
            }

            UpdateSavedCertificate(response.ShoppingCart);

            //Get Learmore Details
            await AssignLearmoreInformationDetails(travelerCertificate);
            travelerCertificate.ReviewETCMessages = await UpdateReviewETCAlertmessages(response.ShoppingCart);

            await _sessionHelperService.SaveSession<MOBShoppingCart>(response.ShoppingCart, session.SessionId, new List<string> { session.SessionId, response.ShoppingCart.ObjectName }, response.ShoppingCart.ObjectName).ConfigureAwait(false);

            return response;
        }

        public async Task<List<MOBMobileCMSContentMessages>> UpdateReviewETCAlertmessages(MOBShoppingCart shoppingCart)
        {
            List<MOBMobileCMSContentMessages> alertMessages = new List<MOBMobileCMSContentMessages>();
            alertMessages = await AssignAlertMessages("TravelCertificate_Combinability_ReviewETCAlertMsg");
            //Show other fop required message only when isOtherFop is required
            if (shoppingCart?.FormofPaymentDetails?.IsOtherFOPRequired == false)
            {
                alertMessages.Remove(alertMessages.Find(x => x.HeadLine == "TravelCertificate_Combinability_ReviewETCAlertMsgs_OtherFopRequiredMessage"));
            }
            //Update the total price
            if (shoppingCart?.FormofPaymentDetails?.TravelCertificate?.Certificates != null &&
                shoppingCart?.FormofPaymentDetails?.TravelCertificate?.Certificates.Count > 0
                )
            {
                double balanceETCAmount = Math.Round(shoppingCart.FormofPaymentDetails.TravelCertificate.Certificates.Sum(x => x.NewValueAfterRedeem), 2);
                if (balanceETCAmount > 0 && shoppingCart.FormofPaymentDetails?.TravelCertificate?.Certificates?.Count > 1)
                {
                    alertMessages.Find(x => x.HeadLine == "TravelCertificate_Combinability_ReviewETCAlertMsgs_ETCBalanceAttentionmessage").ContentFull = string.Format(alertMessages.Find(x => x.HeadLine == "TravelCertificate_Combinability_ReviewETCAlertMsgs_ETCBalanceAttentionmessage").ContentFull, String.Format("{0:0.00}", balanceETCAmount));
                }
                else
                {
                    alertMessages.Remove(alertMessages.Find(x => x.HeadLine == "TravelCertificate_Combinability_ReviewETCAlertMsgs_ETCBalanceAttentionmessage"));
                }
                await SetETCTraveCreditsReviewAlertMessage(alertMessages);
            }
            return alertMessages;
        }

        public async Task SetETCTraveCreditsReviewAlertMessage(List<MOBMobileCMSContentMessages> alertMessages)
        {
            if (await _featureSettings.GetFeatureSettingValue("EnableFSRETCTravelCreditsFeature"))
            {
                if (!alertMessages.Any(c => c.LocationCode == _configuration.GetValue<string>("RTI.TravelCredits.ReviewETCAlertMsgs.AboutTravelCredits.LocationCode")))
                {
                    alertMessages.Add(
                        new MOBMobileCMSContentMessages
                        {
                            ContentFull = _configuration.GetValue<string>("RTI.TravelCredits.ReviewETCAlertMsgs.AboutTravelCredits.ContentFull"),
                            ContentShort = _configuration.GetValue<string>("RTI.TravelCredits.ReviewETCAlertMsgs.AboutTravelCredits.ContentShort"),
                            HeadLine = _configuration.GetValue<string>("RTI.TravelCredits.ReviewETCAlertMsgs.AboutTravelCredits.HeadLine"),
                            LocationCode = _configuration.GetValue<string>("RTI.TravelCredits.ReviewETCAlertMsgs.AboutTravelCredits.LocationCode"),
                            Title = _configuration.GetValue<string>("RTI.TravelCredits.ReviewETCAlertMsgs.AboutTravelCredits.Title"),
                            ContentKey = ""
                        });
                }
                if (!alertMessages.Any(c => c.LocationCode == _configuration.GetValue<string>("RTI.TravelCredits.ReviewETCAlertMsgs.TravelCreditsMsg.LocationCode")))
                {
                    alertMessages.Add(
                      new MOBMobileCMSContentMessages
                      {
                          ContentFull = _configuration.GetValue<string>("RTI.TravelCredits.ReviewETCAlertMsgs.TravelCreditsMsg.ContentFull"),
                          ContentShort = _configuration.GetValue<string>("RTI.TravelCredits.ReviewETCAlertMsgs.TravelCreditsMsg.ContentShort"),
                          HeadLine = _configuration.GetValue<string>("RTI.TravelCredits.ReviewETCAlertMsgs.TravelCreditsMsg.HeadLine"),
                          LocationCode = _configuration.GetValue<string>("RTI.TravelCredits.ReviewETCAlertMsgs.TravelCreditsMsg.LocationCode"),
                          Title = _configuration.GetValue<string>("ETCLearnmoreTooltRTI.TravelCredits.ReviewETCAlertMsgs.TravelCreditsMsg.Title"),
                          ContentKey = ""
                      });
                }
            }
        }

        public async Task AssignLearmoreInformationDetails(MOBFOPTravelCertificate travelCertificate)
        {
            if (travelCertificate.LearnmoreTermsandConditions == null || travelCertificate.LearnmoreTermsandConditions?.Count == 0)
            {
                var docs = await GetCaptions("TravelCertificate_LearnmoreTermsandconditions");

                if (docs == null || !docs.Any())
                {
                    travelCertificate.LearnmoreTermsandConditions = null;
                }
                else
                {
                    travelCertificate.LearnmoreTermsandConditions = new List<MOBMobileCMSContentMessages>();
                    foreach (var doc in docs)
                    {
                        var tnc = new MOBMobileCMSContentMessages
                        {
                            Title = _configuration.GetValue<string>("ETCLearnmoreTooltipText"),
                            ContentFull = doc.CurrentValue,
                            HeadLine = doc.Id
                        };
                        travelCertificate.LearnmoreTermsandConditions.Add(tnc);
                    }
                }
            }
        }

        public void UpdateSavedCertificate(MOBShoppingCart shoppingcart)
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

        public async Task UpdateReservationWithCertificatePrices(Session session, MOBFOPTravelerCertificateResponse response, double totalRedeemAmount, Reservation bookingPathReservation)
        {
            AddGrandTotalIfNotExistInPrices(bookingPathReservation.Prices);

            UpdateCertificateAmountInTotalPrices(bookingPathReservation.Prices, totalRedeemAmount);
            if (await _featureSettings.GetFeatureSettingValue("EnableChaseStemtentFixForMoneyPlusMiles-MOBILE-35118"))
                _shoppingCartUtility.UpdateChaseCreditStatement(bookingPathReservation);
            await _sessionHelperService.SaveSession<Reservation>(bookingPathReservation, session.SessionId, new List<string> { session.SessionId, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName).ConfigureAwait(false);
            response.Reservation = new MOBSHOPReservation(_configuration,_cachingService);
            //Reservation object Load
            response.Reservation = await GetReservationFromPersist(response.Reservation, session);
        }

        public async Task<MOBSHOPReservation> GetReservationFromPersist(MOBSHOPReservation reservation, Session session)
        {
            #region
            Reservation bookingPathReservation = new Reservation();
            bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(session.SessionId, bookingPathReservation.ObjectName, new List<string> { session.SessionId, bookingPathReservation.ObjectName }).ConfigureAwait(false);
            return MakeReservationFromPersistReservation(reservation, bookingPathReservation, session);

            #endregion
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
                    new MOBSHOPPriceBreakDown4Items()
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

        public void AddGrandTotalIfNotExistInPrices(List<MOBSHOPPrice> prices)
        {
            var grandTotalPrice = prices.FirstOrDefault(p => p.DisplayType.ToUpper().Equals("GRAND TOTAL"));
            if (grandTotalPrice == null)
            {
                var totalPrice = prices.FirstOrDefault(p => p.DisplayType.ToUpper().Equals("TOTAL"));
                grandTotalPrice = BuildGrandTotalPriceForReservation(totalPrice.Value);
                prices.Add(grandTotalPrice);
            }
        }

        public void AssignIsOtherFOPRequired(MOBFormofPaymentDetails formofPaymentDetails, List<MOBSHOPPrice> prices, bool IsSecondaryFOP = false, bool isRemoveAll = false)
        {
            var grandTotalPrice = prices.FirstOrDefault(p => p.DisplayType.ToUpper().Equals("GRAND TOTAL"));

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

        public void AssignTotalAndCertificateItemsToPrices(MOBShoppingCart shoppingcart)
        {
            if (_configuration.GetValue<bool>("EnableEtcforSeats_PCU_Viewres"))
            {
                double total = 0;
                //rebuilding Grandtotal
                total = shoppingcart.Products.Sum(p => Convert.ToDouble(p.ProdTotalPrice));
                shoppingcart.Prices = new List<MOBSHOPPrice>();
                if (shoppingcart.TotalPrice != "")
                {
                    shoppingcart.Prices.Add(BuildGrandTotalPriceForReservation(total));
                }

                if (shoppingcart?.FormofPaymentDetails?.TravelCertificate?.Certificates != null)
                {
                    MOBSHOPPrice certificatePrice = new MOBSHOPPrice();
                    UpdateCertificateAmountInTotalPrices(shoppingcart.Prices, shoppingcart.FormofPaymentDetails.TravelCertificate.TotalRedeemAmount);
                }
                double grandTotal = GetGrandTotalFromPrices(shoppingcart.Prices);
                shoppingcart.TotalPrice = string.Format("{0:0.00}", grandTotal);
                shoppingcart.DisplayTotalPrice = Decimal.Parse(grandTotal.ToString()).ToString("c");
            }

        }

        private MOBSHOPPrice BuildGrandTotalPriceForReservation(double grandtotal)
        {
            grandtotal = Math.Round(grandtotal, 2, MidpointRounding.AwayFromZero);
            MOBSHOPPrice totalPrice = new MOBSHOPPrice();
            totalPrice.CurrencyCode = "USD";
            totalPrice.DisplayType = "Grand Total";
            totalPrice.DisplayValue = grandtotal.ToString("N2", CultureInfo.InvariantCulture);
            totalPrice.FormattedDisplayValue = TopHelper.FormatAmountForDisplay(totalPrice.DisplayValue, GetCultureInfo(totalPrice.CurrencyCode), false); //string.Format("${0:c}", totalPrice.DisplayValue);
            double tempDouble1 = 0;
            double.TryParse(totalPrice.DisplayValue.ToString(), out tempDouble1);
            totalPrice.Value = Math.Round(tempDouble1, 2, MidpointRounding.AwayFromZero);
            totalPrice.PriceType = "Grand Total";
            return totalPrice;
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

        private double GetGrandTotalFromPrices(List<MOBSHOPPrice> prices)
        {
            var grandTotal = prices.Find(p => p.DisplayType == "GRAND TOTAL");
            return grandTotal == null ? 0 : grandTotal.Value;
        }

        public void AddRequestedCertificatesToFOPTravelerCertificates(List<MOBFOPCertificate> requestedCertificates, List<MOBFOPCertificate> profileTravelerCertificates, MOBFOPTravelCertificate travelerCertificate)
        {
            List<MOBFOPCertificate> requestedCertificatesCopy = new List<MOBFOPCertificate>();
            if (requestedCertificates != null && requestedCertificates.Count > 0)
            {
                requestedCertificatesCopy = requestedCertificates.Clone();
                int recentAssignedIndex = 0;
                foreach (var certificate in requestedCertificates.ToList())
                {
                    if (!certificate.IsRemove && (travelerCertificate.AllowedETCAmount - travelerCertificate.TotalRedeemAmount > 0 || travelerCertificate.Certificates.Exists(x => x.Index == certificate.Index)))
                    {
                        if (certificate.Index == 0)
                        {
                            int profileMaxId = ((profileTravelerCertificates != null && profileTravelerCertificates.Count > 0) ? profileTravelerCertificates.Max(c => c.Index) + 1 : 1);
                            int addedCerrtMaxId = ((travelerCertificate.Certificates != null && travelerCertificate.Certificates.Count > 0) ? travelerCertificate.Certificates.Max(c => c.Index) + 1 : 1);
                            certificate.Index = profileMaxId > addedCerrtMaxId ? profileMaxId : addedCerrtMaxId;
                            certificate.Index = certificate.Index > recentAssignedIndex ? certificate.Index : recentAssignedIndex + 1;
                            recentAssignedIndex = certificate.Index;
                        }
                        else
                        {
                            travelerCertificate.Certificates.RemoveAll(c => c.Index == certificate.Index);
                        }

                        AssignCertificateRedeemAmount(certificate, travelerCertificate.AllowedETCAmount - travelerCertificate.TotalRedeemAmount);
                        travelerCertificate.Certificates.Add(certificate);
                    }
                }
            }

            if (travelerCertificate.Certificates != null && !requestedCertificatesCopy.Any(x => x.Index == 0))
            {
                var existCertificatesInTraveletCertificateObject = travelerCertificate.Certificates.Where(tcc => !requestedCertificates.All(rc => rc.Index == tcc.Index));
                if (existCertificatesInTraveletCertificateObject != null && existCertificatesInTraveletCertificateObject.Count() > 0)
                {
                    foreach (var tCertificate in existCertificatesInTraveletCertificateObject)
                    {
                        if (travelerCertificate.AllowedETCAmount - travelerCertificate.TotalRedeemAmount > 0 && tCertificate.NewValueAfterRedeem > 0)
                        {
                            AssignCertificateRedeemAmount(tCertificate, travelerCertificate.AllowedETCAmount - (travelerCertificate.TotalRedeemAmount - tCertificate.RedeemAmount));
                        }
                    }
                }
            }
        }

        public void AssignCertificateRedeemAmount(MOBFOPCertificate requestCertificate, List<MOBSHOPPrice> prices)
        {
            if (requestCertificate.CurrentValue >= prices.First(p => p.DisplayType.ToUpper().Equals("TOTAL")).Value)
            {
                requestCertificate.RedeemAmount = prices.First(p => p.DisplayType.ToUpper().Equals("TOTAL")).Value;
                requestCertificate.NewValueAfterRedeem = requestCertificate.CurrentValue - requestCertificate.RedeemAmount;
            }
            else
            {
                requestCertificate.RedeemAmount = requestCertificate.CurrentValue;
                requestCertificate.NewValueAfterRedeem = 0;
            }
            requestCertificate.DisplayRedeemAmount = (requestCertificate.RedeemAmount).ToString("C2", CultureInfo.CurrentCulture);
            requestCertificate.DisplayNewValueAfterRedeem = (requestCertificate.NewValueAfterRedeem).ToString("C2", CultureInfo.CurrentCulture);
        }

        public void AssignCertificateRedeemAmount(MOBFOPCertificate requestCertificate, double amount)
        {
            if (requestCertificate.CurrentValue >= amount)
            {
                requestCertificate.RedeemAmount = amount;
                requestCertificate.NewValueAfterRedeem = requestCertificate.CurrentValue - requestCertificate.RedeemAmount;
            }
            else
            {
                requestCertificate.RedeemAmount = requestCertificate.CurrentValue;
                requestCertificate.NewValueAfterRedeem = 0;
            }
            requestCertificate.DisplayRedeemAmount = (requestCertificate.RedeemAmount).ToString("C2", CultureInfo.CurrentCulture);
            requestCertificate.DisplayNewValueAfterRedeem = (requestCertificate.NewValueAfterRedeem).ToString("C2", CultureInfo.CurrentCulture);
        }

        private void ClearIsRemoveCertificates(List<MOBFOPCertificate> requestedCertificates, List<MOBFOPCertificate> persistedCertificates)
        {
            foreach (var certificate in requestedCertificates)
            {
                if (certificate.IsRemove && persistedCertificates.Exists(c => c.Index == certificate.Index))
                {
                    persistedCertificates.RemoveAll(c => c.Index == certificate.Index);
                }
            }
        }

        private void IsRequestCertifiateAlreadyApplied(List<MOBFOPCertificate> persistedCertificates, List<MOBFOPCertificate> requestedCertificates)
        {
            foreach (var certificate in requestedCertificates)
            {
                if (!certificate.IsRemove)
                {
                    if (persistedCertificates.Exists(c => c.PinCode == certificate.PinCode && c.Index != certificate.Index))
                    {
                        throw new MOBUnitedException(_configuration.GetValue<string>("ETCAlreadyAppliedToOtherTravelerMessage"));
                    }
                }
            }
        }

        public void UpdateCertificateAmountInTotalPrices(List<MOBSHOPPrice> prices, double certificateTotalAmount)
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
                UpdateCertificateRedeemAmountFromTotalInReserationPrices(grandtotal, certificatePrice.Value, false);
                UpdateCertificatePrice(certificatePrice, certificateTotalAmount);
            }

            if (certificateTotalAmount == 0 && certificatePrice != null)
            {
                prices.Remove(certificatePrice);
            }

            UpdateCertificateRedeemAmountFromTotalInReserationPrices(grandtotal, certificateTotalAmount);
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


        public MOBSHOPPrice UpdateCertificatePrice(MOBSHOPPrice certificatePrice, double totalAmount)
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
            certificatePrice.FormattedDisplayValue = "-" + (certificatePrice.Value).ToString("C2", CultureInfo.CurrentCulture);
            certificatePrice.DisplayValue = string.Format("{0:#,0.00}", certificatePrice.Value);
            return certificatePrice;
        }

        public async Task<MOBFOPTravelCertificate> GetShoppingCartTravelCertificateFromPersist(Session session, MOBFOPTravelerCertificateResponse response)
        {
            //Shopping Cart bject Load
            var persistShoppingCart = new MOBShoppingCart();
            persistShoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(session.SessionId, persistShoppingCart.ObjectName, new List<string> { session.SessionId, persistShoppingCart.ObjectName }).ConfigureAwait(false);

            response.ShoppingCart = persistShoppingCart;
            response.Profiles = await LoadPersistedProfile(session.SessionId, response.ShoppingCart.Flow);
            //Prices in reservation
            if (response.ShoppingCart == null)
            {
                MOBShoppingCart shoppingCart = new MOBShoppingCart();
            }
            if (response.ShoppingCart.FormofPaymentDetails == null)
            {
                response.ShoppingCart.FormofPaymentDetails = new MOBFormofPaymentDetails();
            }
            if (response.ShoppingCart.FormofPaymentDetails.TravelCertificate == null)
            {
                if (_configuration.GetValue<bool>("EnableEtcforSeats_PCU_Viewres") && (response.ShoppingCart.Flow == FlowType.VIEWRES.ToString() || response.ShoppingCart.Flow == FlowType.VIEWRES_SEATMAP.ToString()))
                {
                    await InitialiseShoppingCartAndDevfaultValuesForETC(response.ShoppingCart, response.ShoppingCart.Products, response.ShoppingCart.Flow);
                }
                else
                {
                    response.ShoppingCart.FormofPaymentDetails.TravelCertificate = new MOBFOPTravelCertificate();
                }
            }
            if (response.ShoppingCart.DisplayMessage != null)
            {
                response.ShoppingCart.DisplayMessage = response.ShoppingCart.DisplayMessage.Where(x => x.Text1 != "Not enough miles").ToList();
            }
            var travelerCertificate = response.ShoppingCart.FormofPaymentDetails.TravelCertificate;
            if (travelerCertificate.Certificates == null)
            {
                travelerCertificate.Certificates = new List<MOBFOPCertificate>();
            }
            return travelerCertificate;

        }

        public async Task<List<MOBCPProfile>> LoadPersistedProfile(string sessionId, string flow)
        {
            switch (flow)
            {
                case "BOOKING":   //profile Object load
                    United.Persist.Definition.Shopping.ProfileResponse persistedProfileResponse = new United.Persist.Definition.Shopping.ProfileResponse();
                    persistedProfileResponse = await _sessionHelperService.GetSession<United.Persist.Definition.Shopping.ProfileResponse>(sessionId, persistedProfileResponse.ObjectName, new List<string> { sessionId, persistedProfileResponse.ObjectName }).ConfigureAwait(false);

                    return persistedProfileResponse != null ? persistedProfileResponse.Response.Profiles : null;
                case "VIEWRES":
                    ProfileFOPCreditCardResponse profilePersist = new ProfileFOPCreditCardResponse();
                    profilePersist = await _sessionHelperService.GetSession<ProfileFOPCreditCardResponse>(sessionId, profilePersist.ObjectName, new List<string> { sessionId, profilePersist.ObjectName }).ConfigureAwait(false);

                    return profilePersist != null ? profilePersist.Response.Profiles : null;

                default: return null;
            }
        }

        public async Task<MOBShoppingCart> InitialiseShoppingCartAndDevfaultValuesForETC(MOBShoppingCart shoppingcart, List<MOBProdDetail> products, string flow)
        {
            if (shoppingcart == null)
            {
                shoppingcart = new MOBShoppingCart();
            }
            if (shoppingcart.FormofPaymentDetails == null)
            {
                shoppingcart.FormofPaymentDetails = new MOBFormofPaymentDetails();
            }
            if (shoppingcart.FormofPaymentDetails.TravelCertificate == null)
            {
                shoppingcart.FormofPaymentDetails.TravelCertificate = new MOBFOPTravelCertificate();
                shoppingcart.FormofPaymentDetails.TravelCertificate.AllowedETCAmount = GetAlowedETCAmount(shoppingcart.Products ?? products, (string.IsNullOrEmpty(shoppingcart.Flow) ? flow : shoppingcart.Flow));
                shoppingcart.FormofPaymentDetails.TravelCertificate.NotAllowedETCAmount = GetNotAlowedETCAmount(products, (string.IsNullOrEmpty(shoppingcart.Flow) ? flow : shoppingcart.Flow));
                shoppingcart.FormofPaymentDetails.TravelCertificate.MaxAmountOfETCsAllowed = Convert.ToDouble(_configuration.GetValue<string>("CombinebilityMaxAmountOfETCsAllowed"));
                shoppingcart.FormofPaymentDetails.TravelCertificate.MaxNumberOfETCsAllowed = _configuration.GetValue<int>("CombinebilityMaxNumberOfETCsAllowed");
                shoppingcart.FormofPaymentDetails.TravelCertificate.ReviewETCMessages = await AssignAlertMessages("TravelCertificate_Combinability_ReviewETCAlertMsg");
                shoppingcart.FormofPaymentDetails.TravelCertificate.SavedETCMessages = await AssignAlertMessages("TravelCertificate_Combinability_SavedETCListAlertMsg");
                string removeAllCertificatesAlertMessage = _configuration.GetValue<string>("RemoveAllTravelCertificatesAlertMessage");
                shoppingcart.FormofPaymentDetails.TravelCertificate.RemoveAllCertificateAlertMessage = new MOBSection { Text1 = removeAllCertificatesAlertMessage, Text2 = "Cancel", Text3 = "Continue" };
                await SetETCTraveCreditsReviewAlertMessage(shoppingcart.FormofPaymentDetails.TravelCertificate.ReviewETCMessages);
            }
            return shoppingcart;
        }

        private double GetNotAlowedETCAmount(List<MOBProdDetail> products, string flow)
        {
            return products.Sum(a => Convert.ToDouble(a.ProdTotalPrice)) - GetAlowedETCAmount(products, flow);
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

        public async Task<List<MOBItem>> GetCaptions(string key)
        {
            return !string.IsNullOrEmpty(key) ? await GetCaptions(key, true) : null;
        }

        private async Task<List<MOBItem>> GetCaptions(string keyList, bool isTnC)
        {
            var docs = await _legalDocumentsForTitlesService.GetNewLegalDocumentsForTitles(keyList, "trans0", isTnC);
            if (docs == null || !docs.Any()) return null;

            var captions = new List<MOBItem>();

            captions.AddRange(
                docs.Select(doc => new MOBItem
                {
                    Id = doc.Title,
                    CurrentValue = doc.LegalDocument
                }));
            return captions;
        }

        public double GetAlowedETCAmount(List<MOBProdDetail> products, string flow)
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

        private double GetBundlesAmount(List<MOBProdDetail> products, string flow)
        {
            string nonBundleProductCode = _configuration.GetValue<string>("NonBundleProductCode");
            double bundleAmount = products == null ? 0 : products.Where(p => (nonBundleProductCode.IndexOf(p.Code) == -1) && !string.IsNullOrEmpty(p.ProdTotalPrice)).Sum(a => Convert.ToDouble(a.ProdTotalPrice));
            return bundleAmount;
        }
        public void SetEligibilityforETCTravelCredit(MOBSHOPReservation reservation, Session session, Reservation bookingPathReservation)
        {
            if (_shoppingCartUtility.IsEnableETCCreditsInBookingFeature(session.AppID, session.VersionID, session?.CatalogItems))             
            {
                reservation.EligibleForETCPricingType = false;
                if (session.PricingType == Mobile.Model.MSC.FormofPayment.PricingType.ETC.ToString())
                {
                    reservation.EligibleForETCPricingType = true;
                }
                else
                {
                    if (bookingPathReservation.FormOfPaymentType == MOBFormofPayment.ETC || bookingPathReservation.FormOfPaymentType == MOBFormofPayment.TC)
                    {
                        reservation.EligibleForETCPricingType = true;
                    }
                }
                reservation.PricingType = session.PricingType;
            }
        }
    }
}
