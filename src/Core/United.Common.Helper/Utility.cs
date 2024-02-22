using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using United.Definition;
using United.Definition.FormofPayment;
using United.Definition.Shopping;
using United.Definition.Shopping.PriceBreakDown;
using United.Mobile.Model.Common;
using United.Persist.Definition.SeatChange;
using United.Persist.Definition.Shopping;
using United.Service.Presentation.CommonModel;
using United.Service.Presentation.CustomerResponseModel;
using United.Service.Presentation.ReservationResponseModel;
//using United.Service.Presentation.RefundModel;
using United.Service.Presentation.SegmentModel;
using United.Services.FlightShopping.Common;
using United.Services.FlightShopping.Common.DisplayCart;
using United.Services.FlightShopping.Common.Extensions;
using United.Services.FlightShopping.Common.FlightReservation;
using United.Utility.Helper;

namespace United.Common.Helper
{
    public static class UtilityHelper
    {
        public static string FormatAwardAmountForDisplay(string amt, bool truncate = true)
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
                                {
                                    newAmt = string.Format("{0:n1}", amount) + "K miles";
                                }
                                else
                                {
                                    newAmt = string.Format("{0:n0}", amount) + "K miles";
                                }
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

        public static string RemoveString(string text, string textToRemove)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(textToRemove))
            {
                return text;
            }

            var length = textToRemove.Length;
            var index = text.IndexOf(textToRemove, StringComparison.InvariantCultureIgnoreCase);
            return index != -1 ? Regex.Replace(text.Remove(index, length).Trim(), @"\s+", " ") : text;
        }

        public static string GetSeachTypeSelection(SearchType searchType)
        {
            var result = string.Empty;
            try
            {
                return new Dictionary<SearchType, string>
                {
                    {SearchType.OneWay, "OW"},
                    {SearchType.RoundTrip, "RT"},
                    {SearchType.MultipleDestination, "MD"},
                    {SearchType.ValueNotSet, string.Empty},
                }[searchType];
            }
            catch { return result; }
        }


        public static MOBInfoWarningMessages BuildInfoWarningMessages(string message)
        {
            var infoWarningMessages = new MOBInfoWarningMessages
            {
                Order = MOBINFOWARNINGMESSAGEORDER.BEFAREINVERSION.ToString(),
                IconType = MOBINFOWARNINGMESSAGEICON.INFORMATION.ToString(),
                Messages = new List<string>
                {
                    message
                }
            };

            return infoWarningMessages;
        }

        public static string GetFareDescription(DisplayPrice price)
        {
            return (price.Description.ToUpper().IndexOf("ADULT") == 0 ? "adult)" : (!price.Description.ToUpper().Contains("TOTAL")) ? price.Description.Replace("(", "").ToLower() : string.Empty);
        }

        public static string BuildYAPriceTypeDescription(string searchType)
        {
            if (searchType.ToUpper().Equals("OW"))
            {
                return "Fare Oneway (1 young adult)";
            }
            else
                if (searchType.ToUpper().Equals("RT"))
            {
                return "Fare Roundtrip (1 young adult)";
            }
            else
                if (searchType.ToUpper().Equals("MD"))
            {
                return "Fare Multipletrip (1 young adult)";
            }
            else
            {
                return "Fare Oneway (1 young adult)";
            }
        }

        public static string BuildPriceTypeDescription(string searchType, string priceDescription, int price, string desc, bool isFareLockViewRes, bool isCorporateFareLock)
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

        private static string GetFormatPriceSearch(string searchType)
        {
            return !searchType.IsNullOrEmpty() ? "Fare " + searchType + " " : string.Empty;
        }

        public static void UpdateCertificateRedeemAmountFromTotalInReserationPrices(MOBSHOPPrice price, double value, bool isRemove = true)
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

        public static void UpdateCertificateRedeemAmountInSCProductPrices(MOBProdDetail scProduct, double value, bool isRemove = true)
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
                scProduct.ProdDisplayTotalPrice = (prodValueAfterUpdate).ToString("C2", CultureInfo.CurrentCulture);
            }
        }

        public static double GetTotalPriceForRESProduct(bool isPost, Service.Presentation.InteractionModel.ShoppingCart flightReservationResponseShoppingCart, string flow)
        {
            double totalPrice = 0;
            switch (flow)
            {

                case "POSTBOOKING":
                case "VIEWRES":
                case "CHECKIN":
                    totalPrice = isPost ? flightReservationResponseShoppingCart.Items.SelectMany(d => d.Product).Where(d => d.Code == "RES").FirstOrDefault().Characteristics.Where(x => (x.Code != null ? x.Code.ToUpper() == "GrandTotal".ToUpper() : true)).Select(x => Convert.ToDouble(x.Value)).ToList().Sum() :
                                   flightReservationResponseShoppingCart.Items.SelectMany(d => d.Product).Where(d => d.Code == "RES").SelectMany(x => x.Characteristics).Where(x => (x.Code != null ? x.Code.ToUpper() == "GrandTotal".ToUpper() : true)).Select(x => Convert.ToDouble(x.Value)).ToList().Sum();
                    break;
                case "BOOKING"://For now booking check ia added here But we need to remove this once CSL issue is fixed.
                case "RESHOP":
                    totalPrice = isPost ? flightReservationResponseShoppingCart.Items.SelectMany(d => d.Product).Where(d => d.Code == "RES").FirstOrDefault().Price.Totals.Where(x => (x.Name != null ? x.Name.ToUpper() == "GrandTotalForCurrency".ToUpper() : true)).Select(x => x.Amount).ToList().Sum() :
                               flightReservationResponseShoppingCart.Items.SelectMany(d => d.Product).Where(d => d.Code == "RES").SelectMany(x => x.Price.Totals).Where(x => (x.Name != null ? x.Name.ToUpper() == "GrandTotalForCurrency".ToUpper() : true)).Select(x => x.Amount).ToList().Sum();
                    break;
            }

            return totalPrice;
        }

        public static void AddRequestedCertificatesToFOPTravelerCertificates(List<MOBFOPCertificate> requestedCertificates, List<MOBFOPCertificate> profileTravelerCertificates, MOBFOPTravelCertificate travelerCertificate)
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

        public static void AssignCertificateRedeemAmount(MOBFOPCertificate requestCertificate, double amount)
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

        private static string GetSearchTypeDesc(string searchType)
        {
            if (string.IsNullOrEmpty(searchType))
            {
                return string.Empty;
            }

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


        public static string GetDisplayOriginalPrice(Decimal price, Decimal originalPrice)
        {
            if (originalPrice > 0)
            {
                return Decimal.Parse(originalPrice.ToString()).ToString("c");
            }

            return Decimal.Parse(price.ToString()).ToString("c");
        }


        public static bool IsCheckinFlow(string flowName)
        {
            FlowType flowType;
            if (!Enum.TryParse(flowName, out flowType))
            {
                return false;
            }

            return flowType == FlowType.CHECKIN;
        }
        public static string ToPascalCase(this string the_string)
        {
            // If there are 0 or 1 characters, just return the string.
            if (the_string == null) return the_string;
            if (the_string.Length < 2) return the_string.ToUpper();

            // Split the string into words.
            string[] words = the_string.Split(
                new char[] { },
                StringSplitOptions.RemoveEmptyEntries);

            // Combine the words.
            string result = "";
            foreach (string word in words)
            {
                result +=
                    word.Substring(0, 1).ToUpper() +
                    word.Substring(1);
            }

            return result;
        }


        public static bool IsPurchaseFailed(bool isPCUProduct, string originalSegmentIndex, List<string> refundedSegmentNums)
        {
            if (!isPCUProduct)
            {
                return false;
            }

            if (refundedSegmentNums == null || !refundedSegmentNums.Any() || string.IsNullOrEmpty(originalSegmentIndex))
            {
                return false;
            }

            return refundedSegmentNums.Any(s => s == originalSegmentIndex);
        }

        private static bool containsKey(Genre g, string key)
        {
            return g != null && !string.IsNullOrEmpty(g.Description) && g.Description.ToUpper().Contains(key);
        }

        public static Dictionary<string, int> GetSeatPriceOrder()
        {
            return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) {
                { "United First® Seats",1 },
                { "United First® Seat",1 },
                { "United Business® Seats",1 },
                { "United Business® Seat",1 },
                { "United Polaris℠ first Seats",1 },
                { "United Polaris℠ first Seat",1 },
                { "United Polaris℠ business Seats",1 },
                { "United Polaris℠ business Seat",1 },
                { "United® Premium Plus Seats",1 },
                { "United® Premium Plus Seat",1 },
                { "Economy Plus Seats", 2 },
                { "Economy Plus Seat", 3 },
                { "Economy Plus Seats (limited recline)", 4 },
                { "Economy Plus Seat (limited recline)", 5 },
                { "Preferred seats", 6 },
                { "Preferred seat", 7 },
                { "Advance Seat Assignments", 8 },
                { "Advance Seat Assignment", 9 },
                { string.Empty, 9 } };
        }

        public static string GetCommonSeatCode(string seatCode)
        {
            if (string.IsNullOrEmpty(seatCode))
            {
                return string.Empty;
            }

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

        public static string GetFormatedDisplayPriceForSeats(string price)
        {
            if (string.IsNullOrEmpty(price))
            {
                return string.Empty;
            }

            return Decimal.Parse(price).ToString("c");
        }


        public static string GetSeatTypeForDisplay(SeatAssignment s, TravelOptionsCollection travelOptions)
        {
            if (s == null)
            {
                return string.Empty;
            }

            if (s.PCUSeat)
            {
                return GetCabinNameForPcuSeat(s.TravelerIndex, s.OriginalSegmentIndex, travelOptions);
            }

            return GetCommonSeatCode(s.SeatPromotionCode);
        }

        public static string GetCabinNameForPcuSeat(int travelerIndex, int originalSegmentIndex, TravelOptionsCollection travelOptions)
        {
            if (travelOptions == null || !travelOptions.Any())
            {
                return string.Empty;
            }

            var pcutravelOption = travelOptions.Where(t => t.Key == "PCU").FirstOrDefault();
            if (pcutravelOption == null)
            {
                return string.Empty;
            }

            var subItem = pcutravelOption.SubItems.Where(s => s != null && s.Amount > 0 && s.SegmentNumber == originalSegmentIndex.ToString() && s.TravelerRefID == (travelerIndex + 1).ToString()).FirstOrDefault();
            if (subItem == null)
            {
                return string.Empty;
            }

            return subItem.Description;
        }

        public static string GetSegmentInfo(Services.FlightShopping.Common.FlightReservation.FlightReservationResponse flightReservationResponse, int SegmentNumber, int LegIndex)
        {
            if (LegIndex == 0)
            {
                return flightReservationResponse.Reservation.FlightSegments.Where(k => k.SegmentNumber == Convert.ToInt32(SegmentNumber)).Select(y => y.FlightSegment.DepartureAirport.IATACode + " - " + y.FlightSegment.ArrivalAirport.IATACode).FirstOrDefault().ToString();
            }
            else
            {
                return flightReservationResponse.Reservation.FlightSegments.Where(k => k.SegmentNumber == Convert.ToInt32(SegmentNumber)).Select(x => x.Legs).Select(x => x[LegIndex - 1]).Select(y => y.DepartureAirport.IATACode + " - " + y.ArrivalAirport.IATACode).FirstOrDefault().ToString();
            }
            //return flightReservationResponse.Reservation.FlightSegments.Where(k => k.SegmentNumber == Convert.ToInt32(SegmentNumber)).Select(k => k.Legs).FirstOrDefault().Select(y => y.DepartureAirport.IATACode + " - " + y.ArrivalAirport.IATACode).FirstOrDefault().ToString();

        }

        public static string GetSeatDescription(string seatCode)
        {
            string seatDescription = string.Empty;

            switch (seatCode.ToUpper().Trim())
            {

                case "PZA"://All Preferred Seats
                    seatDescription = "Preferred seat";
                    break;
                case "ASA"://All Advance Seat assignment Seats                                       
                    seatDescription = "Advance seat assignment";
                    break;
                case "EPU": //EplusPrimePlus           
                    seatDescription = "Economy Plus®";
                    break;
                case "PSL": //Prime                            
                    seatDescription = "Economy Plus® (limited recline)";
                    break;
                default:
                    return string.Empty;
            }
            return seatDescription;
        }


        public static List<string> OrderProducts(List<string> productCodes)
        {
            if (productCodes == null || !productCodes.Any())
            {
                return productCodes;
            }

            return productCodes.OrderBy(p => GetProductOrder()[GetProductKeytoOrder(p)]).ToList();
        }

        public static Dictionary<string, int> GetProductOrder()
        {
            return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) {
                { "AAC", 0 },
                { "PAC", 1 },
                { string.Empty, 2 } };
        }

        public static string GetProductKeytoOrder(string productCode)
        {
            productCode = string.IsNullOrEmpty(productCode) ? string.Empty : productCode.ToUpper().Trim();

            if (productCode == "AAC" || productCode == "PAC")
            {
                return productCode;
            }

            return string.Empty;
        }

        public static string GetFareLockPassengerDescription(United.Service.Presentation.ReservationModel.Reservation reservation)
        {
            if (reservation != null)
            {
                var traveler = reservation.Travelers.Count.ToString() + (reservation.Travelers.Count() > 1 ? " travelers" : " traveler");
                var JourneyType = GetJourneyTypeDescription(reservation);

                return !JourneyType.IsNullOrEmpty() ? "<b>" + JourneyType + "(" + traveler + ")</b>" : string.Empty;
            }
            return string.Empty;
        }

        public static string GetJourneyTypeDescription(United.Service.Presentation.ReservationModel.Reservation reservation)
        {
            if (reservation != null)
            {
                var JourneyType = reservation.Type.FirstOrDefault(o => (o.Description != null && o.Key != null && o.Description.Equals("JOURNEY_TYPE", StringComparison.OrdinalIgnoreCase)));
                return JourneyType.IsNullOrEmpty() ? GetTravelType(reservation.FlightSegments) : GetTravelType(JourneyType.Key);
            }
            return string.Empty;
        }

        public static string GetTravelType(Collection<ReservationFlightSegment> FlightSegments)
        {
            var journeytype = string.Empty;

            if (FlightSegments != null && FlightSegments.Any(p => p != null))
            {

                var maxTripNumber = FlightSegments.Max(tq => tq.TripNumber);
                var minTripNumber = FlightSegments.Min(f => f.TripNumber);

                if (maxTripNumber.ToInteger() == 1)
                {
                    journeytype = "Oneway";
                }

                if (maxTripNumber.ToInteger() == 2)
                {

                    var firstTripDepartureAirportCode = FlightSegments.Where(t => t.TripNumber == minTripNumber.ToString()).Select(t => t.FlightSegment.DepartureAirport.IATACode).FirstOrDefault();
                    var firstTripArrivalAirportCode = FlightSegments.Where(t => t.TripNumber == minTripNumber.ToString()).Select(t => t.FlightSegment.ArrivalAirport.IATACode).LastOrDefault();
                    var lastTripArrivalAirportCode = FlightSegments.Where(f => f.TripNumber == maxTripNumber.ToString()).Select(t => t.FlightSegment.ArrivalAirport.IATACode).LastOrDefault();
                    var lastTripDepartureAirportCode = FlightSegments.Where(f => f.TripNumber == maxTripNumber.ToString()).Select(t => t.FlightSegment.DepartureAirport.IATACode).FirstOrDefault();

                    if (firstTripDepartureAirportCode == lastTripArrivalAirportCode && firstTripArrivalAirportCode == lastTripDepartureAirportCode)
                    {
                        journeytype = "Roundtrip";
                    }
                    else
                    {
                        journeytype = "Multicity";
                    }

                }
                if (maxTripNumber.ToInteger() > 2)
                {
                    journeytype = "Multicity";
                }
            }


            return journeytype;
        }

        public static string GetTravelType(string JourneyType)
        {
            string Type = string.Empty;

            if (!string.IsNullOrEmpty(JourneyType))
            {
                switch (JourneyType.ToLower())
                {
                    case "one_way":
                        Type = "Oneway";
                        break;
                    case "round_trip":
                        Type = "Roundtrip";
                        break;
                    case "multi_city":
                    default:
                        return "Multicity";
                }
            }
            return Type;
        }

        public static double GetCloseBookingFee(bool isPost, United.Service.Presentation.InteractionModel.ShoppingCart flightReservationResponseShoppingCart, string flow)
        {
            return isPost ? flightReservationResponseShoppingCart.Items.SelectMany(d => d.Product).Where(d => d.Code == "RBF").FirstOrDefault().Price.Totals.FirstOrDefault().Amount :
                                  flightReservationResponseShoppingCart.Items.SelectMany(d => d.Product).Where(d => d.Code == "RBF").SelectMany(x => x.Price.Totals).FirstOrDefault().Amount;
        }

        public static bool CheckFailedShoppingCartItem(FlightReservationResponse flightReservationResponse, United.Service.Presentation.InteractionModel.ShoppingCartItem item)
        {
            string productCode = item.Product.Select(z => z.Code).FirstOrDefault().ToString();
            bool isFailed = false;

            switch (productCode)
            {
                case "TPI":
                case "EFS":
                case "SEATASSIGNMENTS":
                    isFailed = flightReservationResponse.CheckoutResponse.ShoppingCartResponse.Items.Select(x => x.Item).Any(x => x.Product.Select(y => y.Code).FirstOrDefault() == productCode.ToString() && (!x.Status.IsNullOrEmpty() && x.Status.Contains("FAILED")));
                    break;

                default:
                    isFailed = flightReservationResponse.CheckoutResponse.ShoppingCartResponse.Items.Any(x => x.Item.Product.Select(y => y.Code).FirstOrDefault() == productCode.ToString() && (x.Error != null && x.Error.Count > 0));
                    break;
            }

            return isFailed;
        }

        #region SeatMap


        #endregion

        public static string GetCharactersticValue(Collection<Service.Presentation.CommonModel.Characteristic> characteristics, string code)
        {
            if (characteristics == null || characteristics.Count <= 0)
            {
                return string.Empty;
            }

            var characteristic = characteristics.FirstOrDefault(c => c != null && c.Code != null
            && !string.IsNullOrEmpty(c.Code) && c.Code.Trim().Equals(code, StringComparison.InvariantCultureIgnoreCase));
            return characteristic == null ? string.Empty : characteristic.Value;
        }
        public static double GetGrandTotalPriceFareLockShoppingCart(Services.FlightShopping.Common.FlightReservation.FlightReservationResponse flightReservationResponse)
        {
            double totalPrice = 0.0;
            if (!flightReservationResponse.IsNullOrEmpty() && !flightReservationResponse.DisplayCart.IsNullOrEmpty() && !flightReservationResponse.DisplayCart.DisplayPrices.IsNullOrEmpty())
            {

                return !flightReservationResponse.DisplayCart.GrandTotal.IsNullOrEmpty() ? Convert.ToDouble(flightReservationResponse.DisplayCart.GrandTotal)
                                                                                   : Convert.ToDouble(flightReservationResponse.DisplayCart.DisplayPrices.FirstOrDefault(o => (o.Description != null && o.Description.Equals("GrandTotal", StringComparison.OrdinalIgnoreCase))).Amount);
            }
            return totalPrice;
        }
        public static string GetFareLockSegmentDescription(United.Service.Presentation.ReservationModel.Reservation reservation)
        {
            if (reservation != null)
            {
                var traveler = reservation.Travelers.Count.ToString() + (reservation.Travelers.Count() > 1 ? " Travelers" : " Traveler");
                var JourneyType = reservation.Type.FirstOrDefault(o => (o.Description != null && o.Key != null && o.Description.Equals("JOURNEY_TYPE", StringComparison.OrdinalIgnoreCase)));

                return JourneyType.IsNullOrEmpty() ? GetTravelType(reservation.FlightSegments) + "(" + traveler + ")" : GetTravelType(JourneyType.Key) + "(" + traveler + ")";
            }
            return string.Empty;
        }
    }
}
