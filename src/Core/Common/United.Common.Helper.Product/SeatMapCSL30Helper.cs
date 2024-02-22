using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using United.Definition;
using United.Definition.Booking;
using United.Definition.Pcu;
using United.Definition.SeatCSL30;
using United.Definition.Shopping;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.DynamoDB;
using United.Mobile.DataAccess.MSCPayment.Interfaces;
using United.Mobile.DataAccess.Product.Interfaces;
using United.Mobile.Model.DynamoDb.Common;
using United.Persist.Definition.Shopping;
using United.Service.Presentation.ProductModel;
using United.Service.Presentation.ReferenceDataRequestModel;
using United.Service.Presentation.ReferenceDataResponseModel;
using United.Service.Presentation.SegmentModel;
using United.Services.FlightShopping.Common.Extensions;
using United.Utility.Helper;
using Characteristic = United.Service.Presentation.CommonModel.Characteristic;
using SeatMapRequest = United.Definition.SeatCSL30.SeatMapRequest;
using United.Mobile.Model.Common;
using ProfileResponse = United.Persist.Definition.Shopping.ProfileResponse;

namespace United.Common.Helper.Product
{
    public class SeatMapCSL30Helper : ISeatMapCSL30Helper
    {
        private readonly IConfiguration _configuration;
        private readonly ISessionHelperService _sessionHelperService;
        private readonly ISeatMapCSL30Service _seatMapCSL30service;
        private readonly SeatMapEngine _seatMapEngine;
        private readonly IComplimentaryUpgradeService _complimentaryUpgradeService;
        private readonly ISeatMapService _seatMapService;
        private readonly SeatEngineDynamoDB _seatEngineDynamoDB;
        private readonly ILegalDocumentsForTitlesService _legalDocumentsForTitlesService;
        private string seatMapLegendEntry1;
        private string seatMapLegendEntry2;
        private string pcuOfferAmountForthisCabin;
        private readonly IDPService _dPService;
        private readonly IReferencedataService _referencedataService;
        private readonly IHeaders _headers;
        private readonly ICachingService _cachingService;

        public SeatMapCSL30Helper(IConfiguration configuration
            , ISessionHelperService sessionHelperService
            , ISeatMapCSL30Service seatMapCSL30service
            , IComplimentaryUpgradeService complimentaryUpgradeService
            , ISeatMapService seatMapService
            , ILegalDocumentsForTitlesService legalDocumentsForTitlesService
            , IDPService dPService
            , IReferencedataService referencedataService
            , IHeaders headers
            , ICachingService cachingService)
        {
            _configuration = configuration;
            _sessionHelperService = sessionHelperService;
            _seatMapCSL30service = seatMapCSL30service;
            _seatMapEngine = new SeatMapEngine(configuration, null, null, headers);
            ConfigUtility.UtilityInitialize(_configuration);
            _complimentaryUpgradeService = complimentaryUpgradeService;
            _seatMapService = seatMapService;
            _seatEngineDynamoDB = new SeatEngineDynamoDB(_configuration, null);
            _legalDocumentsForTitlesService = legalDocumentsForTitlesService;
            seatMapLegendEntry1 = _configuration.GetValue<string>("seatMapLegendEntry1");
            seatMapLegendEntry2 = _configuration.GetValue<string>("seatMapLegendEntry2");
            _dPService = dPService;
            _referencedataService = referencedataService;
            _headers = headers;
            _cachingService = cachingService;
        }

        private MOBSHOPFlight GetFlightDetails(List<MOBSHOPTrip> trips, string origin, string destination, ref bool isFirstSegmentInReservation)
        {
            MOBSHOPFlight segment = new MOBSHOPFlight();
            int countSegment = 0;

            foreach (MOBSHOPTrip flightAvailabilityTrip in trips)
            {
                #region
                foreach (MOBSHOPFlattenedFlight flightAvailabilitySegment in flightAvailabilityTrip.FlattenedFlights)
                {
                    foreach (MOBSHOPFlight ts in flightAvailabilitySegment.Flights)
                    {
                        countSegment++;
                        if (ts.Origin.ToUpper() == origin.ToUpper() && ts.Destination.ToUpper() == destination.ToUpper())
                        {
                            segment = ts;
                            if (countSegment == 1)
                            {
                                isFirstSegmentInReservation = true;
                            }
                        }
                    }
                }
                #endregion
            }
            return segment;
        }

        private bool IsSeatMapSupportedOa(string operatingCarrier, string MarketingCarrier)
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

        private SeatMapRequest BuildSeatMapRequest
            (string flow, string languageCode, string channelId, string channelName,
            string recordLocator, bool isAwardReservation, bool isIBE, string cartId = null)
        {
            SeatMapRequest request = new SeatMapRequest();

            if (!string.IsNullOrEmpty(recordLocator))
            {
                request.RecordLocator = recordLocator;
            }

            request.ChannelId = channelId;
            request.ChannelName = channelName;
            request.CartId = cartId;
            request.IsAwardReservation = isAwardReservation;
            request.ProductCode = isIBE
                ? _configuration.GetValue<string>("IBEFullShoppingProductCodes") : string.Empty;

            request.IsFrontCabin = true;
            request.IsUppCabin = true;
            request.Travelers = null;
            return request;
        }

        private string GetFareBasicCodeFromBundles(List<MOBSHOPTravelOption> travelOptions, int tripId, string defaultFareBasisCode, string destination, string origin)
        {
            if (travelOptions == null || travelOptions.Count <= 0)
                return defaultFareBasisCode;

            foreach (var travelOption in travelOptions)
            {
                foreach (var bundleCode in travelOption.BundleCode)
                {
                    if (bundleCode.AssociatedTripIndex == tripId && !string.IsNullOrEmpty(bundleCode.ProductKey))
                    {
                        return bundleCode.ProductKey;
                    }

                }
            }

            return defaultFareBasisCode;
        }


        private System.Collections.Generic.ICollection<Traveler2> BuildTravelersDetails(Reservation persistedReservation, int applicationId, string appVersion)
        {
            Collection<United.Definition.SeatCSL30.Traveler2> travelersInformation = new Collection<United.Definition.SeatCSL30.Traveler2>();
            #region
            if (persistedReservation.TravelersCSL != null && persistedReservation.TravelersCSL.Count > 0)
            {
                int i = 1;
                MOBSHOPReservation reservation = new MOBSHOPReservation(_configuration, _cachingService);
                foreach (string travelerKey in persistedReservation.TravelerKeys)
                {
                    MOBCPTraveler bookingTravelerInfo = persistedReservation.TravelersCSL[travelerKey];

                    if (ConfigUtility.EnableTravelerTypes(applicationId, appVersion, persistedReservation.IsReshopChange) &&
                        persistedReservation.ShopReservationInfo2 != null && persistedReservation.ShopReservationInfo2.TravelerTypes != null && persistedReservation.ShopReservationInfo2.TravelerTypes.Count > 0)
                    {
                        if (bookingTravelerInfo.TravelerTypeCode.ToUpper().Equals("INF"))
                            continue;
                    }
                    United.Definition.SeatCSL30.Traveler2
                        travelerInformations = new United.Definition.SeatCSL30.Traveler2();

                    travelerInformations.Id = i;
                    travelerInformations.FirstName = bookingTravelerInfo.FirstName;
                    travelerInformations.LastName = bookingTravelerInfo.LastName;

                    travelerInformations.DateOfBirth = bookingTravelerInfo.BirthDate;
                    travelerInformations.PassengerTypeCode = bookingTravelerInfo.CslReservationPaxTypeCode;




                    if (bookingTravelerInfo.AirRewardPrograms != null)
                    {
                        travelerInformations.LoyaltyProfiles = new Collection<United.Definition.SeatCSL30.LoyaltyProfile>();
                        United.Definition.SeatCSL30.LoyaltyProfile loyaltyProfile = new LoyaltyProfile();
                        loyaltyProfile.ProgramId = string.Empty;
                        loyaltyProfile.MemberShipId = string.Empty;
                        if (bookingTravelerInfo.AirRewardPrograms.Count > 0)
                        {
                            foreach (MOBBKLoyaltyProgramProfile profile in bookingTravelerInfo.AirRewardPrograms)
                            {
                                MOBSHOPRewardProgram shopreward = null;
                                if (reservation != null && reservation.RewardPrograms != null)
                                {
                                    shopreward = reservation.RewardPrograms.Find(p => p.ProgramID == loyaltyProfile.ProgramId);
                                }

                                if (profile.CarrierCode.Trim().ToUpper() == "UA"
                                    || (string.IsNullOrEmpty(profile.CarrierCode)
                                    && shopreward != null && shopreward.Type == "UA"))
                                {
                                    loyaltyProfile.MemberShipId = profile.MemberId;
                                    loyaltyProfile.ProgramId = (string.IsNullOrEmpty(profile.CarrierCode)
                                        ? shopreward.Type.Trim().ToUpper() : profile.CarrierCode.Trim().ToUpper());

                                }
                            }
                        }
                        travelerInformations.LoyaltyProfiles.Add(loyaltyProfile);
                    }
                    travelersInformation.Add(travelerInformations);
                    i++;
                }
            }
            #endregion
            return travelersInformation;
        }


        public async Task< List<MOBSeatMap>> GetCSL30SeatMapDetail
           (string flow, string sessionId, string destination, string origin, int applicationId,
           string appVersion, string deviceId, bool returnPolarisLegendforSeatMap)
        {
            bool isFirstSegmentInReservation = false;
            string[] channelInfo = _configuration.GetValue<string>("CSL30MBEChannelInfo").Split('|');

            Reservation persistedReservation =
                await _sessionHelperService.GetSession<Reservation>(sessionId, new Reservation().ObjectName, new List<string> { sessionId, new Reservation().ObjectName }).ConfigureAwait(false);

            bool isIBE = IsIBE(persistedReservation);

            MOBSHOPFlight segment = GetFlightDetails(persistedReservation.Trips, origin, destination, ref isFirstSegmentInReservation);

            bool isSeatMapSupportedOa = IsSeatMapSupportedOa(segment.OperatingCarrier, segment.MarketingCarrier);

            if (segment?.SegNumber == 1)
            {
                isFirstSegmentInReservation = true;
            }

            if (!ShowSeatMapForCarriers(segment.OperatingCarrier))
            {
                bool allowSeatMapForSupportedOaOperatingCarrier =
                    (ConfigUtility.OaSeatMapSupportedVersion(applicationId, appVersion, segment.OperatingCarrier, segment.MarketingCarrier));
                if (persistedReservation.IsReshopChange || !allowSeatMapForSupportedOaOperatingCarrier) // Throw Seat Map Advance Seat Not Avaiable exception for all OA Operating Flights which we are not supporting in booking path and for all OA Operiting carrriers in reshop as in reshop we will not support any oa seat map.d
                {
                    throw new MOBUnitedException(_configuration.GetValue<string>("SeatMapUnavailableOtherAirlines"));
                }
            }

            var request = BuildSeatMapRequest
                (flow, string.Empty, channelInfo[0], channelInfo[1], recordLocator: "", isAwardReservation: false, isIBE: false, cartId: persistedReservation.CartId);

            request.CurrencyCode = "USD";

            request.BundleCode = GetFareBasicCodeFromBundles
                (persistedReservation.TravelOptions, segment.TripIndex, null, destination, origin);

            request.ProductCode = (segment.ShoppingProducts != null && segment.ShoppingProducts.Any())
                ? segment.ShoppingProducts[0].ProductCode : string.Empty;

            request.Travelers = BuildTravelersDetails(persistedReservation, applicationId, appVersion);

            request.IsLapChild = false;

            if (ConfigUtility.EnableTravelerTypes(applicationId, appVersion, persistedReservation.IsReshopChange) &&
                persistedReservation != null && persistedReservation.ShopReservationInfo2 != null && persistedReservation.ShopReservationInfo2.TravelerTypes != null
                && persistedReservation.ShopReservationInfo2.TravelerTypes.Count > 0)
            {
                if (persistedReservation.ShopReservationInfo2.DisplayTravelTypes.Any
                    (t => t.TravelerType.ToString().Equals(MOBPAXTYPE.InfantLap.ToString()) || t.TravelerType.ToString().Equals(MOBPAXTYPE.InfantSeat.ToString())))
                    request.IsLapChild = true;
            }


            request.FlightSegments = BuildFlightSegmentsCSL30(segment, destination, origin);

            var session = await _sessionHelperService.GetSession<Session>
                (sessionId, new Session().ObjectName, new List<string> { sessionId, new Session().ObjectName }).ConfigureAwait(false);

            string cslRequest = JsonConvert.SerializeObject(request);
            string cslstrResponse = await GetSelectSeatMapResponse(cslRequest, session.Token, channelInfo[0], channelInfo[1], string.Empty,sessionId);
            List<MOBSeatMap> seatMaps = null;
            SeatMap response = new SeatMap();

            if (!string.IsNullOrEmpty(cslstrResponse))
            {
                response = JsonConvert.DeserializeObject<SeatMap>(cslstrResponse);
            }

            if (response != null && response.FlightInfo != null && response.Cabins != null && response.Cabins.Count > 0 && response.Errors.IsNullOrEmpty())
            {
                seatMaps = new List<MOBSeatMap>();
                if (!string.IsNullOrEmpty(_configuration.GetValue<string>("SeatMapForDeck")) && _configuration.GetValue<string>("SeatMapForDeck").Contains(request.FlightSegments.FirstOrDefault().OperatingAirlineCode))
                {
                    //TODO:For now just passing need to check what this used for if not required need to be removed.
                    response.FlightInfo.OperatingCarrierCode = request.FlightSegments.FirstOrDefault().OperatingAirlineCode;
                }

                string cabin = !string.IsNullOrEmpty(segment.ServiceClassDescription) ? segment.ServiceClassDescription : segment.Cabin;
                MOBSeatMap aSeatMap =
                    await BuildSeatMapCSL30BookingReshopResponse(response, persistedReservation,
                    response.Travelers.Count, cabin, sessionId, persistedReservation.IsELF, isIBE, isSeatMapSupportedOa,
                    segment.SegNumber, flow, isFirstSegmentInReservation, "", applicationId, appVersion);

                seatMaps.Add(aSeatMap);

                //TODO:Below code need to be moved to BuildSeatmapCSL30
                bool tomToggle = _configuration.GetValue<bool>("EnableProjectTOM");
                bool isBus = (seatMaps[0].FleetType.Length >= 2 && seatMaps[0].FleetType.Substring(0, 2).ToUpper().Equals("BU"));

                if (seatMaps != null && seatMaps.Count == 1 && seatMaps[0].IsOaSeatMap && (!tomToggle || (tomToggle && !isBus)))
                {
                    seatMaps[0].OperatedByText = GetOperatedByText(segment.MarketingCarrier, segment.FlightNumber, segment.OperatingCarrierDescription);
                }
            }
            return seatMaps;
        }

        private string GetOperatedByText(string marketingCarrier, string flightNumber, string operatingCarrierDescription)
        {
            if (string.IsNullOrEmpty(marketingCarrier) ||
                string.IsNullOrEmpty(flightNumber) ||
                string.IsNullOrEmpty(operatingCarrierDescription))
                return string.Empty;
            operatingCarrierDescription = RemoveString(operatingCarrierDescription, "Limited");
            return marketingCarrier + flightNumber + " operated by " + operatingCarrierDescription;
        }

        private string RemoveString(string text, string textToRemove)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(textToRemove))
                return text;

            var length = textToRemove.Length;
            var index = text.IndexOf(textToRemove, StringComparison.InvariantCultureIgnoreCase);
            return index != -1 ? Regex.Replace(text.Remove(index, length).Trim(), @"\s+", " ") : text;
        }

        private bool isLimitedRecline(string displaySeatCategory)
        {
            if (!string.IsNullOrEmpty(displaySeatCategory))
            {
                var limitedReclineCategory = _configuration.GetValue<string>("SelectSeatsLimitedReclineForCSL30").Split('|');
                if (!limitedReclineCategory.IsNullOrEmpty() && limitedReclineCategory.Any())
                {
                    return limitedReclineCategory.Any(x => !x.IsNullOrEmpty() && x.Trim().Equals(displaySeatCategory, StringComparison.OrdinalIgnoreCase));
                }
            }
            return false;
        }

        private async Task< (MOBSeatMap objMOBSeatMap, int countNoOfFreeSeats, int countNoOfPricedSeats)> GetSeatMapCSL(United.Definition.SeatCSL30.SeatMap seatMapResponse, string sessionId, bool isELF, bool isIBE, bool isOaSeatMapSegment, int segmentIndex, string flow, int appId, string appVersion, bool isEnablePcuDeepLinkInSeatMap, bool isEnablePCUSeatMapPurchaseManageRes, int countNoOfFreeSeats, int countNoOfPricedSeats, string bookingCabin, bool cogStop)
        {
            MOBSeatMap objMOBSeatMap = new MOBSeatMap();
            List<string> cabinBrandingDescriptions = new List<string>();
            MOBSeatMapCSL30 objMOBSeatMapCSL30 = new MOBSeatMapCSL30();

            objMOBSeatMap.SeatMapAvailabe = true;
            objMOBSeatMap.FlightNumber = objMOBSeatMapCSL30.FlightNumber = seatMapResponse.FlightInfo.MarketingFlightNumber;
            objMOBSeatMap.FlightDateTime = objMOBSeatMapCSL30.FlightDateTime = seatMapResponse.FlightInfo.DepartureDate.ToString("MM/dd/yyyy hh:mm tt");
            objMOBSeatMap.Arrival = new MOBAirport { Code = seatMapResponse.FlightInfo.ArrivalAirport };
            objMOBSeatMap.Departure = new MOBAirport { Code = seatMapResponse.FlightInfo.DepartureAirport };
            objMOBSeatMap.IsOaSeatMap = objMOBSeatMapCSL30.IsOaSeatMap = isOaSeatMapSegment;
            objMOBSeatMap.FleetType = !string.IsNullOrEmpty(seatMapResponse.AircraftInfo.Icr) ? seatMapResponse.AircraftInfo.Icr : string.Empty;
            // SupressLMX only for booking path
            bool supressLMX = _seatMapEngine.SupressLMX(appId);

            // New model to save in persist
            objMOBSeatMapCSL30.ArrivalCode = seatMapResponse.FlightInfo.ArrivalAirport;
            objMOBSeatMapCSL30.DepartureCode = seatMapResponse.FlightInfo.DepartureAirport;
            objMOBSeatMapCSL30.MarketingCarrierCode = seatMapResponse.FlightInfo.MarketingCarrierCode;
            objMOBSeatMapCSL30.OperatingCarrierCode = seatMapResponse.FlightInfo.OperatingCarrierCode;
            objMOBSeatMapCSL30.Flow = flow;
            objMOBSeatMapCSL30.SegmentNumber = segmentIndex;

            List<MOBSeatCSL30> listMOBSeatCSL30 = new List<MOBSeatCSL30>();

            objMOBSeatMap.LegId = string.Empty;
            int cabinCount = 0;

            /// Only in ManageRes -- code for PCU
            /// This code will not execute for booking as isEnablePcuDeepLinkInSeatMap returned as false in booking path
            List<MOBPcuUpgradeOption> upgradeOffers = null;
            if (isEnablePcuDeepLinkInSeatMap)
            {
                var pcu = await new PremiumCabinUpgrade(_sessionHelperService, sessionId, objMOBSeatMap.FlightNumber.ToString(), seatMapResponse.FlightInfo.DepartureAirport, seatMapResponse.FlightInfo.ArrivalAirport).LoadOfferStateforSeatMap();
                upgradeOffers = pcu.GetUpgradeOptionsForSeatMap();
                objMOBSeatMap.Captions = await _seatMapEngine.GetPcuCaptions(pcu.GetTravelerNames(), pcu.RecordLocator).ConfigureAwait(false);
            }

            int numberOfCabins = seatMapResponse.Cabins.Count;
            foreach (United.Definition.SeatCSL30.Cabin cabin in seatMapResponse.Cabins)
            {
                ++cabinCount;
                bool firstCabin = (cabinCount == 1);
                MOBCabin tmpCabin = new MOBCabin();

                bool disableSeats = true;
                if (cabin.CabinBrand.Equals("United Business", StringComparison.OrdinalIgnoreCase) || cabin.CabinBrand.Equals("United First", StringComparison.OrdinalIgnoreCase) || cabin.CabinBrand.Equals("United Polaris Business", StringComparison.OrdinalIgnoreCase)
                    || cabin.CabinBrand.Equals("Business", StringComparison.OrdinalIgnoreCase) || cabin.CabinBrand.Equals("United Premium Plus", StringComparison.OrdinalIgnoreCase) || cabin.CabinBrand.Equals("Premium Economy", StringComparison.OrdinalIgnoreCase)
                    || cabin.CabinBrand.Equals("United Economy", StringComparison.OrdinalIgnoreCase) || cabin.CabinBrand.Equals("Economy", StringComparison.OrdinalIgnoreCase) || cabin.CabinBrand.Equals("Coach", StringComparison.OrdinalIgnoreCase) || cabin.CabinBrand.Equals("First", StringComparison.OrdinalIgnoreCase))
                {
                    if (cabin.CabinBrand.Equals(bookingCabin, StringComparison.OrdinalIgnoreCase))
                    {
                        disableSeats = false;
                    }
                    else if ((cabin.CabinBrand.Equals("Premium Economy", StringComparison.OrdinalIgnoreCase)
                        || cabin.CabinBrand.Equals("United Economy", StringComparison.OrdinalIgnoreCase)
                        || cabin.CabinBrand.Equals("Economy", StringComparison.OrdinalIgnoreCase))
                    && (bookingCabin.Equals("Coach", StringComparison.OrdinalIgnoreCase)
                    || bookingCabin.Equals("Economy", StringComparison.OrdinalIgnoreCase)
                    || bookingCabin.Equals("United Economy", StringComparison.OrdinalIgnoreCase)))
                    {
                        disableSeats = false;
                    }
                    else if ((cabin.CabinBrand.Equals("United Business", StringComparison.OrdinalIgnoreCase)
                        || cabin.CabinBrand.Equals("Business", StringComparison.OrdinalIgnoreCase)
                        || cabin.CabinBrand.Equals("United Polaris Business", StringComparison.OrdinalIgnoreCase))
                        && (bookingCabin.Equals("United Business", StringComparison.OrdinalIgnoreCase)
                        || bookingCabin.Equals("Business", StringComparison.OrdinalIgnoreCase)
                        || bookingCabin.Equals("United Polaris Business", StringComparison.OrdinalIgnoreCase)))
                    {
                        disableSeats = false;
                    }
                    else if ((cabin.CabinBrand.Equals("United First", StringComparison.OrdinalIgnoreCase)
                        || cabin.CabinBrand.Equals("First", StringComparison.OrdinalIgnoreCase))
                        && (bookingCabin.Equals("United First", StringComparison.OrdinalIgnoreCase)
                        || bookingCabin.Equals("First", StringComparison.OrdinalIgnoreCase)
                        || bookingCabin.Equals("United Global First", StringComparison.OrdinalIgnoreCase)))
                    {
                        disableSeats = false;
                    }
                }

                /// Only in MR Path -- Code for PCU
                /// For MR isEnablePCUSeatMapPurchaseManageRes will be true
                double pcuOfferPriceForthisCabin = 0;
                string pcuOfferAmountForthisCabin = string.Empty;
                string cabinName = string.Empty;
                string pcuOfferOptionId = string.Empty;
                var upgradeOffer = isEnablePcuDeepLinkInSeatMap && upgradeOffers != null && upgradeOffers.Any() && !cabin.CabinBrand.Equals("United Economy", StringComparison.OrdinalIgnoreCase) ? upgradeOffers.FirstOrDefault(u => IsCabinMatchedCSL(u.UpgradeOptionDescription, cabin.CabinBrand)) : null;
                if (!upgradeOffer.IsNullOrEmpty())
                {
                    pcuOfferAmountForthisCabin = string.Format("{0}.00", upgradeOffer.FormattedPrice);
                    pcuOfferPriceForthisCabin = isEnablePCUSeatMapPurchaseManageRes ? upgradeOffer.Price : 0;
                    cabinName = upgradeOffer.UpgradeOptionDescription;
                    pcuOfferOptionId = _configuration.GetValue<bool>("TurnOff_DefaultSelectionForUpgradeOptions") ? string.Empty : upgradeOffer.OptionId;
                }
                ///  End
                tmpCabin.COS = isOaSeatMapSegment && cabin.IsUpperDeck ? "Upper Deck " + cabin.CabinBrand : cabin.CabinBrand;
                tmpCabin.Configuration = cabin.Layout;
                /// Checking with azhar as this is for other airlines and checking with cabin name.
                var isOaPremiumEconomyCabin = cabin.CabinBrand.Equals("Premium Economy", StringComparison.OrdinalIgnoreCase);
                cabinBrandingDescriptions.Add(cabin.CabinBrand);

                foreach (United.Definition.SeatCSL30.Row row in cabin.Rows)
                {
                    if (!row.IsNullOrEmpty() && row.Number < 1000)
                    {
                        MOBRow tmpRow = new MOBRow();
                        tmpRow.Number = row.Number.ToString();
                        tmpRow.Wing = !isOaSeatMapSegment && row.Wing;

                        var monumentrow = cabin.MonumentRows.FirstOrDefault(x => x.VerticalGridNumber == row.VerticalGridNumber);
                        var cabinColumnCount = cabin.ColumnCount == 0 ? cabin.Layout.Length : cabin.ColumnCount;
                        for (int i = 1; i <= cabinColumnCount; i++)
                        {
                            MOBSeatB tmpSeat = null;
                            MOBSeatCSL30 objMOBSeatCSL30 = new MOBSeatCSL30();
                            var seat = row.Seats.FirstOrDefault(x => x.HorizontalGridNumber == i);
                            var monumentseat = (!monumentrow.IsNullOrEmpty()) ? monumentrow.Monuments.FirstOrDefault(x => x.HorizontalGridNumber == i) : null;
                            if (!seat.IsNullOrEmpty())
                            {
                                // Build seatmap response for client
                                tmpSeat = new MOBSeatB();
                                tmpSeat.Exit = seat.IsExit;
                                tmpSeat.Fee = string.Empty; // Need to find and assign
                                tmpSeat.Number = seat.Number;
                                tmpSeat.LimitedRecline = !isOaSeatMapSegment && !string.IsNullOrEmpty(seat.DisplaySeatCategory)
                                    && isLimitedRecline(seat.DisplaySeatCategory);

                                // Need to revisit this code// checking only for united economy might includ UPP
                                if (!string.IsNullOrEmpty(seat.SeatType) && !isOaSeatMapSegment
                                    && (cabin.CabinBrand.Equals("United Business", StringComparison.OrdinalIgnoreCase)
                                    || cabin.CabinBrand.Equals("United First", StringComparison.OrdinalIgnoreCase)
                                    || cabin.CabinBrand.Equals("United Polaris Business", StringComparison.OrdinalIgnoreCase)))
                                {
                                    tmpSeat.Program = GetSeatPositionAccessFromCSL30SeatMap(seat.SeatType);
                                }
                                tmpSeat.IsEPlus = !string.IsNullOrEmpty(seat.SeatType)
                                                                && seat.SeatType.Equals(MOBSeatType.BLUE.ToString(), StringComparison.OrdinalIgnoreCase);
                                bool isBasicEconomy = isELF || isIBE;
                                bool disableEplusSeats = tmpSeat.IsEPlus && isBasicEconomy;
                                bool isEconomyCabinWithAdvanceSeats = cabin.CabinBrand.Equals("United Economy", StringComparison.OrdinalIgnoreCase) && disableEplusSeats;
                                tmpSeat.SeatValue = GetSeatValueFromCSL30SeatMap(seat, disableEplusSeats, disableSeats, null, isOaSeatMapSegment, isOaPremiumEconomyCabin, pcuOfferAmountForthisCabin, cogStop);

                                var tier = seatMapResponse.Tiers.FirstOrDefault(x => !x.IsNullOrEmpty() && x.Id == Convert.ToInt32(seat.Tier));
                                tmpSeat.ServicesAndFees = GetServicesAndFees(seat, pcuOfferAmountForthisCabin, pcuOfferPriceForthisCabin, tmpSeat.Program, tier);

                                tmpSeat.PcuOfferPrice = tmpSeat.SeatValue == "O" ? pcuOfferAmountForthisCabin : null;
                                tmpSeat.IsPcuOfferEligible = tmpSeat.SeatValue == "O" && !string.IsNullOrWhiteSpace(pcuOfferAmountForthisCabin) && pcuOfferPriceForthisCabin == 0;
                                tmpSeat.PcuOfferOptionId = tmpSeat.SeatValue == "O" && !string.IsNullOrWhiteSpace(pcuOfferAmountForthisCabin) ? pcuOfferOptionId : null;
                                tmpSeat.DisplaySeatFeature = GetDisplaySeatFeature(isOaSeatMapSegment, tmpSeat.SeatValue, pcuOfferAmountForthisCabin, cabinName, isEconomyCabinWithAdvanceSeats, cabin.CabinBrand, tmpSeat.IsEPlus);
                                tmpSeat.SeatFeatureList = GetSeatFeatureList(tmpSeat.SeatValue, supressLMX, tmpSeat.LimitedRecline, isEconomyCabinWithAdvanceSeats, cabin.CabinBrand);
                                CountNoOfFreeSeatsAndPricedSeats(tmpSeat, ref countNoOfFreeSeats, ref countNoOfPricedSeats);

                                // Seatmap response with traveler pricing to save in persisit
                                // This is backend model, Client is not using it.
                                // This code needs to be modified when client is changing.
                                objMOBSeatCSL30.Number = seat.Number;
                                objMOBSeatCSL30.Tier = seat.Tier;
                                objMOBSeatCSL30.TotalFee = !tmpSeat.ServicesAndFees.IsNullOrEmpty() ? tmpSeat.ServicesAndFees[0].TotalFee : 0;
                                objMOBSeatCSL30.EDoc = seat.EDoc;
                                objMOBSeatCSL30.SeatType = seat.SeatType;
                                objMOBSeatCSL30.DisplaySeatCategory = seat.DisplaySeatCategory;
                                objMOBSeatCSL30.IsAvailable = seat.IsAvailable;
                                objMOBSeatCSL30.Pricing = new List<MOBTierPricingCSL30>();
                                if (!tier.IsNullOrEmpty() && !tier.Pricing.IsNullOrEmpty())
                                {
                                    foreach (var pricing in tier.Pricing)
                                    {
                                        var travelerPricing = new MOBTierPricingCSL30()
                                        {
                                            TravelerId = pricing.TravelerId,
                                            TotalPrice = pricing.TotalPrice,
                                            TravelerIndex = seatMapResponse.Travelers.FirstOrDefault(x => !x.IsNullOrEmpty() && x.Id == pricing.TravelerId).TravelerIndex
                                        };
                                        objMOBSeatCSL30.Pricing.Add(travelerPricing);
                                    }
                                }
                                listMOBSeatCSL30.Add(objMOBSeatCSL30);
                            }
                            else
                            {
                                //get monumemt seat and loop based on span - build this empty seat.
                                //monumentseat.HorizontalSpan
                                tmpSeat = new MOBSeatB
                                {
                                    Number = string.Empty,
                                    Fee = string.Empty,
                                    LimitedRecline = false,
                                    SeatValue = "-",
                                    Exit = (!monumentseat.IsNullOrEmpty()) ? monumentseat.IsExit : false,
                                };
                            }
                            tmpRow.Seats.Add(tmpSeat);
                        }

                        if (tmpRow != null)
                        {
                            if (tmpRow.Seats == null || tmpRow.Seats.Count != cabin.Layout.Length)
                            {
                                if (isOaSeatMapSegment)
                                    throw new MOBUnitedException(_configuration.GetValue<string>("SeatMapUnavailableOtherAirlines"));
                                throw new MOBUnitedException(_configuration.GetValue<string>("GenericExceptionMessage"));
                            }
                        }

                        if (row.Number < 1000)
                            tmpCabin.Rows.Add(tmpRow);
                    }
                }
                tmpCabin.Configuration = tmpCabin.Configuration.Replace(" ", "-");
                objMOBSeatMap.Cabins.Add(tmpCabin);
            }

            objMOBSeatMap.SeatLegendId = objMOBSeatMap.IsOaSeatMap ? _configuration.GetValue<string>("SeatMapLegendForOtherAirlines") :
                                                                     await GetPolarisSeatMapLegendId(seatMapResponse.FlightInfo.DepartureAirport, seatMapResponse.FlightInfo.ArrivalAirport, numberOfCabins, cabinBrandingDescriptions, appId, appVersion);

            objMOBSeatMapCSL30.Seat = !listMOBSeatCSL30.IsListNullOrEmpty() ? listMOBSeatCSL30 : null;
            await SaveCSL30SeatMapPersist(sessionId, objMOBSeatMapCSL30);

            return (objMOBSeatMap, countNoOfFreeSeats, countNoOfPricedSeats);
        }

        private bool IsCabinMatchedCSL(string pcuCabin, string seatmapCabin)
        {
            if (string.IsNullOrEmpty(pcuCabin) || string.IsNullOrEmpty(seatmapCabin))
                return false;

            pcuCabin = pcuCabin.Trim().Replace("®", "").Replace("℠", "").ToUpper();
            seatmapCabin = seatmapCabin.ToUpper().Trim();

            if (pcuCabin.Equals(seatmapCabin, StringComparison.OrdinalIgnoreCase))
                return true;

            var possiblefirstCabins = new List<string> { "FIRST", "UNITED FIRST", "UNITED GLOBAL FIRST", "UNITED POLARIS FIRST" };
            if (possiblefirstCabins.Contains(seatmapCabin) && possiblefirstCabins.Contains(pcuCabin))
                return true;

            var possibleBusinessCabins = new List<string> { "UNITED BUSINESS", "UNITED BUSINESS", "UNITED POLARIS BUSINESS", "BUSINESSFIRST", "UNITED BUSINESSFIRST" };
            if (possibleBusinessCabins.Contains(seatmapCabin) && possibleBusinessCabins.Contains(pcuCabin))
                return true;

            var possibleUppCabins = new List<string> { "UNITED PREMIUM PLUS", "UNITED PREMIUMPLUS" };
            if (possibleUppCabins.Contains(seatmapCabin) && possibleUppCabins.Contains(pcuCabin))
                return true;

            return false;
        }

        private string GetSeatPositionAccessFromCSL30SeatMap(string seatType)
        {
            string seatPositionProgram = string.Empty;
            if (seatType.Equals(MOBSeatType.FBLEFT.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                seatPositionProgram = Convert.ToString(MOBSeatPosition.FBL);
            }
            else if (seatType.Equals(MOBSeatType.FBRIGHT.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                seatPositionProgram = Convert.ToString(MOBSeatPosition.FBR);
            }
            else if (seatType.Equals(MOBSeatType.FBFRONT.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                seatPositionProgram = Convert.ToString(MOBSeatPosition.FBF);
            }
            else if (seatType.Equals(MOBSeatType.FBBACK.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                seatPositionProgram = Convert.ToString(MOBSeatPosition.FBB);
            }
            else if (seatType.Equals(MOBSeatType.DAAFRONTL.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                seatPositionProgram = Convert.ToString(MOBSeatPosition.DAFL);
            }
            else if (seatType.Equals(MOBSeatType.DAAFRONTR.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                seatPositionProgram = Convert.ToString(MOBSeatPosition.DAFR);
            }
            else if (seatType.Equals(MOBSeatType.DAALEFT.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                seatPositionProgram = Convert.ToString(MOBSeatPosition.DAL);
            }
            else if (seatType.Equals(MOBSeatType.DAARIGHT.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                seatPositionProgram = Convert.ToString(MOBSeatPosition.DAR);
            }
            else if (seatType.Equals(MOBSeatType.DAAFRONTRM.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                seatPositionProgram = Convert.ToString(MOBSeatPosition.DAFRM);
            }
            return seatPositionProgram;
        }

        private string GetSeatValueFromCSL30SeatMap(Definition.SeatCSL30.Seat seat, bool disableEplus, bool disableSeats, MOBApplication application, bool isOaSeatMapSegment, bool isOaPremiumEconomyCabin, string pcuOfferAmountForthisCabin, bool cogStop)
        {
            string seatValue = string.Empty;
            if (seat != null && !string.IsNullOrEmpty(seat.SeatType))
            {
                if (seat.IsInoperative || seat.IsPermanentBlocked || seat.IsBlocked)
                {
                    seatValue = "X";
                }
                else if (seat.SeatType.Equals(MOBSeatType.BLUE.ToString(), StringComparison.OrdinalIgnoreCase) || isOaPremiumEconomyCabin)
                {
                    seatValue = seat.IsAvailable && (disableEplus || cogStop) ? "X" : seat.IsAvailable ? "P" : "X";
                }
                else if (seat.SeatType.Equals(MOBSeatType.PREF.ToString(), StringComparison.OrdinalIgnoreCase))//TODO version check
                {
                    seatValue = seat.IsAvailable ? "PZ" : "X";
                }
                else if (seat.SeatType.Equals(MOBSeatType.STANDARD.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    seatValue = seat.IsAvailable ? "O" : "X";
                }
                else
                {
                    seatValue = seat.IsAvailable ? "O" : "X";
                }
            }
            return string.IsNullOrEmpty(seatValue) || (!string.IsNullOrEmpty(seatValue) && disableSeats && string.IsNullOrWhiteSpace(pcuOfferAmountForthisCabin)) ? "X" : seatValue;
        }

        private List<MOBServicesAndFees> GetServicesAndFees(Seat seat, string pcuOfferAmountForthisCabin, double pcuOfferPriceForthisCabin, string program, Tier tier)
        {
            List<MOBServicesAndFees> servicesAndFees = new List<MOBServicesAndFees>();

            if (!string.IsNullOrWhiteSpace(pcuOfferAmountForthisCabin) && pcuOfferPriceForthisCabin > 0) // Amount will be there only if the no of travelers are eligible and Appversion and AppId matches
            {
                MOBServicesAndFees serviceAndFee = new MOBServicesAndFees();
                serviceAndFee.Available = seat.IsAvailable;
                serviceAndFee.SeatFeature = seat.DisplaySeatCategory;
                serviceAndFee.SeatNumber = seat.Number;
                serviceAndFee.Program = program; // aSeat.Program will be empty and to assign program code for higher cabins to upgrade seat
                serviceAndFee.TotalFee = Convert.ToDecimal(pcuOfferPriceForthisCabin);
                serviceAndFee.Currency = "USD";

                servicesAndFees.Add(serviceAndFee);
            }
            else if (seat.ServicesAndFees != null && seat.ServicesAndFees.Any())
            {
                foreach (SeatService seatService in seat.ServicesAndFees)
                {
                    MOBServicesAndFees serviceAndFee = new MOBServicesAndFees();
                    serviceAndFee.AgentDutyCode = seatService.AgentDutyCode;
                    serviceAndFee.AgentId = seatService.AgentId;
                    serviceAndFee.AgentTripleA = seatService.AgentTripleA;
                    serviceAndFee.Available = seat.IsAvailable;
                    serviceAndFee.BaseFee = seatService.BaseFee;
                    serviceAndFee.Currency = seatService.Currency;
                    serviceAndFee.EliteStatus = seatService.EliteStatus.ToString();
                    serviceAndFee.FeeWaiveType = seatService.FeeWaiveType;
                    serviceAndFee.IsDefault = seatService.IsDefault;
                    serviceAndFee.OverrideReason = seatService.OverrideReason;
                    serviceAndFee.Program = seat.EDoc;
                    serviceAndFee.SeatFeature = seat.DisplaySeatCategory;
                    serviceAndFee.SeatNumber = seat.Number;
                    serviceAndFee.Tax = seatService.Tax;
                    serviceAndFee.TotalFee = seatService.TotalFee;
                    servicesAndFees.Add(serviceAndFee);
                }
            }

            return servicesAndFees;
        }

        private string GetDisplaySeatFeature(bool isOaSeatMapSegment, string seatValue, string pcuOfferAmountForthisCabin, string pcuCabinName, bool isEconomyCabinWithAdvanceSeats, string cabinName, bool isEplus)
        {
            if (isOaSeatMapSegment)
                return string.Empty;

            if (!string.IsNullOrWhiteSpace(pcuOfferAmountForthisCabin) && seatValue.Equals("O") && !string.IsNullOrEmpty(pcuCabinName))
                return pcuCabinName;

            if (cabinName.Equals("United Premium Plus", StringComparison.OrdinalIgnoreCase))
                return "United Premium Plus";

            if (seatValue.Equals("P") || isEplus)
                return "Economy Plus";

            if (seatValue.Equals("PZ"))
                return "Preferred Seat";

            if (isEconomyCabinWithAdvanceSeats)
                return "Economy";

            return string.Empty;
        }

        private List<string> GetSeatFeatureList(string seatValue, bool supressLMX, bool limitedRecline, bool isEconomyCabinWithAdvanceSeats, string cabinName)
        {
            List<string> seatFeatures = new List<string>();
            if (seatValue.Equals("P"))
            {
                seatFeatures.Add("Extra Legroom");
                if (limitedRecline)
                    seatFeatures.Add("Limited recline");
                if (!supressLMX)
                    seatFeatures.Add("Eligible for PQD");
            }
            else if (seatValue.Equals("PZ"))
            {
                seatFeatures.Add("Standard legroom");
                seatFeatures.Add("Favorable location in Economy");
                if (limitedRecline)
                    seatFeatures.Add("Limited recline");
            }
            else if (isEconomyCabinWithAdvanceSeats)
            {
                seatFeatures.Add("Advance seat assignment");
                if (limitedRecline)
                    seatFeatures.Add("Limited recline");
            }
            return seatFeatures;
        }

        private void CountNoOfFreeSeatsAndPricedSeats(MOBSeatB seat, ref int countNoOfFreeSeats, ref int countNoOfPricedSeats)
        {
            if (seat.IsNullOrEmpty() || seat.SeatValue.IsNullOrEmpty() ||
                seat.SeatValue == "-" || seat.SeatValue.ToUpper() == "X" || seat.IsPcuOfferEligible)
                return;

            if (seat.ServicesAndFees.IsNullOrEmpty())
            {
                countNoOfFreeSeats++;
            }
            else if (seat.ServicesAndFees[0].Available && seat.ServicesAndFees[0].TotalFee <= 0)
            {
                countNoOfFreeSeats++;
            }
            else if (seat.ServicesAndFees[0].Available)
            {
                countNoOfPricedSeats++;
            }
        }

        private async Task SaveCSL30SeatMapPersist(string sessionId, MOBSeatMapCSL30 seatMap)
        {
            List<MOBSeatMapCSL30> csl30SeatMaps = new List<MOBSeatMapCSL30>();
            csl30SeatMaps.Add(seatMap);
            var persistedCSL30SeatMaps = new List<MOBSeatMapCSL30>();
            persistedCSL30SeatMaps = await _sessionHelperService.GetSession<List<MOBSeatMapCSL30>>(sessionId, new MOBSeatMapCSL30().ObjectName, new List<string> { sessionId, new MOBSeatMapCSL30().ObjectName }).ConfigureAwait(false); //change session

            // to Append current CSL30 seatmap & maintain all segments seatmaps without duplicity
            if (!persistedCSL30SeatMaps.IsNullOrEmpty())
            {
                foreach (MOBSeatMapCSL30 pCSL30SeatMap in persistedCSL30SeatMaps)
                {
                    if (!(pCSL30SeatMap.DepartureCode.Equals(seatMap.DepartureCode, StringComparison.OrdinalIgnoreCase) && pCSL30SeatMap.ArrivalCode.Equals(seatMap.ArrivalCode, StringComparison.OrdinalIgnoreCase) && pCSL30SeatMap.FlightNumber == seatMap.FlightNumber && pCSL30SeatMap.FlightDateTime == seatMap.FlightDateTime))
                    {
                        csl30SeatMaps.Add(pCSL30SeatMap);
                    }
                }

               await  _sessionHelperService.SaveSession<List<MOBSeatMapCSL30>>(csl30SeatMaps, sessionId, new List<string> { sessionId, new MOBSeatMapCSL30().ObjectName }, new MOBSeatMapCSL30().ObjectName).ConfigureAwait(false);//change session
            }
            else
            {
               await  _sessionHelperService.SaveSession<List<MOBSeatMapCSL30>>(csl30SeatMaps, sessionId, new List<string> { sessionId, new MOBSeatMapCSL30().ObjectName }, new MOBSeatMapCSL30().ObjectName).ConfigureAwait(false);//change session
            }
        }

        private async Task<string> GetPolarisSeatMapLegendId(string from, string to, int numberOfCabins, List<string> polarisCabinBrandingDescriptions, int applicationId = -1, string appVersion = "")
        {
            string seatMapLegendId = string.Empty;

            //POLARIS Cabin Branding SeatMapLegend Booking Path
            string seatMapLegendEntry1 = _configuration.GetValue<string>("seatMapLegendEntry1");
            string seatMapLegendEntry2 = _configuration.GetValue<string>("seatMapLegendEntry2");
            string seatMapLegendEntry3 = _configuration.GetValue<string>("seatMapLegendEntry3");
            string seatMapLegendEntry6 = _configuration.GetValue<string>("seatMapLegendEntry6");
            string seatMapLegendEntry7 = _configuration.GetValue<string>("seatMapLegendEntry7");
            string seatMapLegendEntry8 = _configuration.GetValue<string>("seatMapLegendEntry8");
            string seatMapLegendEntry9 = _configuration.GetValue<string>("seatMapLegendEntry9");
            string seatMapLegendEntry4 = _configuration.GetValue<string>("seatMapLegendEntry4");
            string seatMapLegendEntry5 = _configuration.GetValue<string>("seatMapLegendEntry5");
            string seatMapLegendEntry14 = string.Empty;
            string legendForPZA = string.Empty;
            string legendForUPP = string.Empty;
            // Preferred Seat //AB-223
            bool isPreferredZoneEnabled = ConfigUtility.EnablePreferredZone(applicationId, appVersion); // Check if preferred seat
            if (isPreferredZoneEnabled)
            {
                seatMapLegendEntry14 = _configuration.GetValue<string>("seatMapLegendEntry14");
                legendForPZA = "_PZA";
            }
            if (ConfigUtility.IsUPPSeatMapSupportedVersion(applicationId, appVersion) && numberOfCabins == 3 && polarisCabinBrandingDescriptions != null && polarisCabinBrandingDescriptions.Any(p => p.ToUpper() == "UNITED PREMIUM PLUS"))
            {
                legendForUPP = "_UPP";
                seatMapLegendId = "seatmap_legend1" + legendForPZA + legendForUPP + "|" + polarisCabinBrandingDescriptions[0].ToString() + "|" + polarisCabinBrandingDescriptions[1].ToString() + seatMapLegendEntry6 + seatMapLegendEntry14 + seatMapLegendEntry7 + seatMapLegendEntry8 + seatMapLegendEntry9;// Sample Value Could be Ex: seatmap_legend1|United Polaris Business|United Premium Plus|Economy Plus|Preferred Seat|Economy|Occupied Seat|Exit Row or Sample Value Could be Ex: seatmap_legend2|First|Business|Economy Plus|Economy|Occupied Seat|Exit Row
            }
            else
            {
                if (_configuration.GetValue<bool>("DisableComplimentaryUpgradeOnpremSqlService"))
                {
                    if (numberOfCabins == 1)
                    {
                        seatMapLegendId = "seatmap_legend6" + legendForPZA + seatMapLegendEntry6 + seatMapLegendEntry14 + seatMapLegendEntry7 + seatMapLegendEntry8 + seatMapLegendEntry9;
                    }
                    else if (numberOfCabins == 3)
                    {
                        seatMapLegendId = "seatmap_legend1" + legendForPZA + legendForUPP + "|" + polarisCabinBrandingDescriptions[0].ToString() + "|" + polarisCabinBrandingDescriptions[1].ToString() + seatMapLegendEntry6 + seatMapLegendEntry14 + seatMapLegendEntry7 + seatMapLegendEntry8 + seatMapLegendEntry9;
                    }
                    else//If number of cabin==2 or by default assiging legend5
                    {
                        seatMapLegendId = "seatmap_legend5" + legendForPZA + legendForUPP + "|" + polarisCabinBrandingDescriptions[0].ToString() + seatMapLegendEntry6 + seatMapLegendEntry14 + seatMapLegendEntry7 + seatMapLegendEntry8 + seatMapLegendEntry9;
                    }
                }
                else
                {


                    var listOfCabinIds = await _complimentaryUpgradeService.GetComplimentaryUpgradeOfferedFlagByCabinCount(from, to, numberOfCabins, _headers.ContextValues.SessionId, _headers.ContextValues.TransactionId);
                    try
                    {
                        if (listOfCabinIds != null)
                        {
                            foreach (var Ids in listOfCabinIds)
                            {

                                //verification needed
                                int secondCabinBrandingId = Ids.SecondCabinBrandingId.Equals(System.DBNull.Value) ? 0 : Convert.ToInt32(Ids.SecondCabinBrandingId);
                                int thirdCabinBrandingId = Ids.ThirdCabinBrandingId.Equals(System.DBNull.Value) ? 0 : Convert.ToInt32(Ids.ThirdCabinBrandingId);


                                //AB-223,AB-224 Adding Preferred Seat To SeatLegendID
                                //Added the code to check the flag for Preferred Zone and app version > 2.1.60  
                                if (thirdCabinBrandingId == 0)
                                {
                                    if (secondCabinBrandingId == 1)
                                    {
                                        seatMapLegendId = "seatmap_legend5" + legendForPZA + legendForUPP + "|" + polarisCabinBrandingDescriptions[0].ToString() + seatMapLegendEntry6 + seatMapLegendEntry14 + seatMapLegendEntry7 + seatMapLegendEntry8 + seatMapLegendEntry9; // Sample Value Could be Ex: seatmap_legend5|First|Economy Plus|Preferred Seat|Economy|Occupied Seat|Exit Row
                                    }
                                    else if (secondCabinBrandingId == 2)
                                    {
                                        seatMapLegendId = "seatmap_legend4" + legendForPZA + legendForUPP + "|" + polarisCabinBrandingDescriptions[0].ToString() + seatMapLegendEntry6 + seatMapLegendEntry14 + seatMapLegendEntry7 + seatMapLegendEntry8 + seatMapLegendEntry9;// Sample Value Could be Ex: seatmap_legend4|Business|Economy Plus|Preferred Seat|Economy|Occupied Seat|Exit Row
                                    }
                                    else if (secondCabinBrandingId == 3)
                                    {
                                        seatMapLegendId = "seatmap_legend3" + legendForPZA + legendForUPP + "|" + polarisCabinBrandingDescriptions[0].ToString() + seatMapLegendEntry6 + seatMapLegendEntry14 + seatMapLegendEntry7 + seatMapLegendEntry8 + seatMapLegendEntry9;// Sample Value Could be Ex: seatmap_legend3|United Polaris Business|Economy Plus|Preferred Seat|Economy|Occupied Seat|Exit Row or seatmap_legend4|Business|Economy Plus|Economy|Occupied Seat|Exit Row
                                    }
                                }
                                else if (thirdCabinBrandingId == 1)
                                {
                                    seatMapLegendId = "seatmap_legend2" + legendForPZA + legendForUPP + "|" + polarisCabinBrandingDescriptions[0].ToString() + "|" + polarisCabinBrandingDescriptions[1].ToString() + seatMapLegendEntry6 + seatMapLegendEntry14 + seatMapLegendEntry7 + seatMapLegendEntry8 + seatMapLegendEntry9;// Sample Value Could be Ex: seatmap_legend2|First|Business|Economy Plus|Preferred Seat|Economy|Occupied Seat|Exit Row or seatmap_legend1|United Polaris First|United Polaris Business|Economy Plus|Economy|Occupied Seat|Exit Row 
                                }
                                else if (thirdCabinBrandingId == 4)
                                {
                                    seatMapLegendId = "seatmap_legend1" + legendForPZA + legendForUPP + "|" + polarisCabinBrandingDescriptions[0].ToString() + "|" + polarisCabinBrandingDescriptions[1].ToString() + seatMapLegendEntry6 + seatMapLegendEntry14 + seatMapLegendEntry7 + seatMapLegendEntry8 + seatMapLegendEntry9;// Sample Value Could be Ex: seatmap_legend1|United Polaris First|United Polaris Business|Economy Plus|Preferred Seat|Economy|Occupied Seat|Exit Row or Sample Value Could be Ex: seatmap_legend2|First|Business|Economy Plus|Economy|Occupied Seat|Exit Row
                                }
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Console.Write(ex.Message);
                    }
                    #region Adding Preferred Seat Legend ID
                    if (string.IsNullOrEmpty(seatMapLegendId))
                    {

                        //AB-223,AB-224 Adding Preferred Seat To SeatLegendID
                        //Added the code to check the flag for Preferred Zone and app version > 2.1.60
                        //Changes added on 09/24/2018                
                        //Bug 213002 mAPP: Seat Map- Blank Legend is displayed for One Cabin Flights
                        //Bug 102152
                        if (!string.IsNullOrEmpty(appVersion) &&
                        GeneralHelper.isApplicationVersionGreater(applicationId, appVersion, "AndroidFirstCabinVersion", "iPhoneFirstCabinVersion", "", "", true, _configuration)
                        && numberOfCabins == 1 && polarisCabinBrandingDescriptions != null && polarisCabinBrandingDescriptions.Count > 0 &&
                        !string.IsNullOrEmpty(polarisCabinBrandingDescriptions[0].ToString().Trim()))
                        {
                            seatMapLegendId = "seatmap_legend6" + legendForPZA + seatMapLegendEntry6 + seatMapLegendEntry14 + seatMapLegendEntry7 + seatMapLegendEntry8 + seatMapLegendEntry9;
                        }
                        else
                        {
                            seatMapLegendId = "seatmap_legend5" + legendForPZA + "|" + polarisCabinBrandingDescriptions[0].ToString() + seatMapLegendEntry6 + seatMapLegendEntry14 + seatMapLegendEntry7 + seatMapLegendEntry8 + seatMapLegendEntry9;
                        }
                    }


                    #endregion
                }
            }
            return seatMapLegendId;
        }

        private async Task< MOBSeatMap> BuildSeatMapCSL30BookingReshopResponse
           (United.Definition.SeatCSL30.SeatMap seatMapResponse, Reservation persistedReservation,
           int numberOfTravelers, string bookingCabin, string sessionId, bool isELF, bool isIBE, bool isOaSeatMapSegment,
           int segmentIndex, string flow, bool isFirstSegmentInReservation, string token = "", int applicationId = -1, string appVersion = "")
        {
            int countNoOfFreeSeats = 0;
            int countNoOfPricedSeats = 0;
            MOBSeatMap objMOBSeatMap = new MOBSeatMap();
            // SeatEngine seatEngine = new SeatEngine();

            var tupleRespnse =  await GetSeatMapCSL
                (seatMapResponse, sessionId, isELF, isIBE, isOaSeatMapSegment, segmentIndex, flow,
                applicationId, appVersion, false, false, countNoOfFreeSeats, countNoOfPricedSeats, bookingCabin, false);
            objMOBSeatMap = tupleRespnse.objMOBSeatMap;
            countNoOfFreeSeats = tupleRespnse.countNoOfFreeSeats;
            countNoOfPricedSeats = tupleRespnse.countNoOfPricedSeats;

            //Booking and Reshop Specific code
            if (objMOBSeatMap.IsOaSeatMap && countNoOfFreeSeats == 0)
            {
                bool oaExceptionVersionCheck = ConfigUtility.OaSeatMapExceptionVersion(applicationId, appVersion);

                if (oaExceptionVersionCheck)
                {
                    throw new MOBUnitedException(await _seatMapEngine.GetDocumentTextFromDataBase("OA_SEATMAP_Exception_TEXT"));
                }
                else
                {
                    throw new MOBUnitedException(_configuration.GetValue<string>("SeatMapUnavailableOtherAirlines"));
                }
            }

            if (Convert.ToBoolean(_configuration.GetValue<string>("checkForPAXCount")))
            {
                string readOnlySeatMapinBookingPathOAAirlines = string.Empty;
                readOnlySeatMapinBookingPathOAAirlines = _configuration.GetValue<string>("ReadonlySeatMapinBookingPathOAAirlines");


                if (objMOBSeatMap.IsOaSeatMap
                    && !string.IsNullOrEmpty(seatMapResponse?.FlightInfo?.OperatingCarrierCode)
                    && !seatMapResponse.FlightInfo.OperatingCarrierCode.Equals("SQ"))
                {
                    if ((countNoOfFreeSeats <= numberOfTravelers))
                    {
                        bool oaExceptionVersionCheck = ConfigUtility.OaSeatMapExceptionVersion(applicationId, appVersion);

                        if (oaExceptionVersionCheck)
                        {
                            throw new MOBUnitedException(await _seatMapEngine.GetDocumentTextFromDataBase("OA_SEATMAP_Exception_TEXT"));
                        }
                        else
                        {
                            throw new MOBUnitedException(_configuration.GetValue<string>("SeatMapUnavailableOtherAirlines"));
                        }
                    }
                    else if (!string.IsNullOrEmpty(readOnlySeatMapinBookingPathOAAirlines))
                    {
                        string[] readOnlySeatMapAirlines = { };
                        readOnlySeatMapAirlines = readOnlySeatMapinBookingPathOAAirlines.Split(',');
                        foreach (string airline in readOnlySeatMapAirlines)
                        {
                            if (seatMapResponse.FlightInfo.OperatingCarrierCode.Equals(airline, StringComparison.OrdinalIgnoreCase))
                            {
                                objMOBSeatMap.IsReadOnlySeatMap = true;
                                objMOBSeatMap.OASeatMapBannerMessage
                                    = _configuration.GetValue<string>("OASeatMapBannerMessage") != null
                                    ? _configuration.GetValue<string>("OASeatMapBannerMessage").Trim() : string.Empty;
                                break;
                            }
                        }
                    }

                }
            }

            bool isInCheckInWindow = false;

            if (_configuration.GetValue<bool>("EnableSocialDistanceMessagingForSeatMap") && isFirstSegmentInReservation)
            {
                objMOBSeatMap.ShowInfoMessageOnSeatMap = _configuration.GetValue<string>("SocialDistanceSeatDisplayMessageDetailBody") + _configuration.GetValue<string>("SocialDistanceSeatMapMessagePopup");
            }
            else
            {
                objMOBSeatMap.ShowInfoMessageOnSeatMap = objMOBSeatMap.IsOaSeatMap ?
                    await ShowOaSeatMapAvailabilityDisclaimerText() :
                    await ShowNoFreeSeatsAvailableMessage(persistedReservation, countNoOfFreeSeats, countNoOfPricedSeats, isInCheckInWindow, isFirstSegmentInReservation);
            }

            return objMOBSeatMap;
        }

        private string ExceptionMessages(Exception ex)
        {
            if (ex.InnerException == null)
            {
                return ex.Message;
            }

            return ex.Message + " | " + ExceptionMessages(ex.InnerException);
        }

        private async Task<string> GetSelectSeatMapResponse(string cslRequest, string token, string channelId, string channelName, string path,string sessionId)
        {
            try
            {
                return await _seatMapCSL30service.GetSeatMapDeatils(token, sessionId, cslRequest, channelId, channelName, path);
            }
            catch (Exception ex)
            {
                string exReader = ExceptionMessages(ex);

                string seatMapUnavailable = string.Empty;
                if (!string.IsNullOrEmpty(_configuration.GetValue<string>("SeatMapUnavailable-MinorDescription")))
                {
                    seatMapUnavailable = _configuration.GetValue<string>("SeatMapUnavailable-MinorDescription");
                    string[] seatMapUnavailableMinorDescription = seatMapUnavailable.Split('|');

                    if (!string.IsNullOrEmpty(exReader))
                    {
                        foreach (string minorDescription in seatMapUnavailableMinorDescription)
                            if (exReader.Contains(minorDescription))
                                throw new MOBUnitedException(_configuration.GetValue<string>("OASeatMapUnavailableMessage"));
                    }
                }
                throw new Exception(exReader);
            }
            return default;
        }

        private Collection<United.Definition.SeatCSL30.FlightSegments>
           BuildFlightSegmentsCSL30(MOBSHOPFlight segment, string destination, string origin)
        {
            var flightSegments = new Collection<United.Definition.SeatCSL30.FlightSegments>();

            Int32.TryParse(segment.FlightNumber, out int flightNo);

            var flightSegment
                = new United.Definition.SeatCSL30.FlightSegments
                {
                    FlightNumber = flightNo,
                    MarketingAirlineCode = segment.MarketingCarrier,

                    OperatingAirlineCode = !string.IsNullOrEmpty(_configuration.GetValue<string>("SeatMapForACSubsidiary"))
                    ? _configuration.GetValue<string>("SeatMapForACSubsidiary").Contains(segment.OperatingCarrier)
                    ? "AC" : segment.OperatingCarrier : segment.OperatingCarrier,

                    DepartureAirport = new Definition.SeatCSL30.Airport
                    { IataCode = segment.Origin, IataCountryCode = new IataCountryCode { CountryCode = segment.OriginCountryCode } },

                    DepartureDateTime = segment.DepartureDateTime,

                    ArrivalAirport = new Definition.SeatCSL30.Airport
                    { IataCode = segment.Destination, IataCountryCode = new IataCountryCode { CountryCode = segment.DestinationCountryCode } },

                    ArrivalDateTime = segment.ArrivalDateTime,

                    CheckInSegment = segment.IsCheckInWindow,
                    ClassOfService = segment.ServiceClass,
                    FarebasisCode = segment.FareBasisCode,
                    OperatingFlightNumber = flightNo,
                    TripIndicator = segment.TripIndex,
                    SegmentNumber = segment.SegNumber,
                    Pricing = "true",
                    //IsInternational = segment.in

                };
            flightSegments.Add(flightSegment);
            return flightSegments;
        }

        private bool IsIBE(Reservation persistedReservation)
        {
            if (_configuration.GetValue<bool>("EnablePBE") && (persistedReservation.ShopReservationInfo2 != null))
            {
                return persistedReservation.ShopReservationInfo2.IsIBE;
            }
            return false;
        }

        private bool ShowSeatMapForCarriers(string operatingCarrier)
        {
            if (_configuration.GetValue<string>("ShowSeatMapAirlines") != null)
            {
                string[] carriers = _configuration.GetValue<string>("ShowSeatMapAirlines").Split(',');
                foreach (string carrier in carriers)
                {
                    if (operatingCarrier != null && carrier.ToUpper().Trim().Equals(operatingCarrier.ToUpper().Trim()))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public async Task<List<MOBSeatMap>> GetSeatMapDetail(string sessionId, string destination, string origin, int applicationId, string appVersion, string deviceId, bool returnPolarisLegendforSeatMap)
        {
            string transactionId = sessionId;
            MOBSHOPFlight segment = new MOBSHOPFlight();

            Reservation persistedReservation = await _sessionHelperService.GetSession<Reservation>(sessionId, new Reservation().ObjectName, new List<string> { sessionId, new Reservation().ObjectName }).ConfigureAwait(false);

            bool firstSelected = false;
            bool businessSelected = false;
            bool isFirstSegmentInReservation = false;
            string cabin = string.Empty;
            string COS = string.Empty;
            string fareBasisCode = string.Empty;
            string bundleCode = string.Empty;

            int countSegment = 0;
            foreach (MOBSHOPTrip flightAvailabilityTrip in persistedReservation.Trips)
            {
                #region
                foreach (MOBSHOPFlattenedFlight flightAvailabilitySegment in flightAvailabilityTrip.FlattenedFlights)
                {
                    foreach (MOBSHOPFlight ts in flightAvailabilitySegment.Flights)
                    {
                        countSegment++;

                        if (ts.Origin.ToUpper() == origin.ToUpper() && ts.Destination.ToUpper() == destination.ToUpper())
                        {
                            if (countSegment == 1)
                            {
                                isFirstSegmentInReservation = true;
                            }
                            COS = ts.ServiceClass;
                            cabin = ts.ServiceClassDescription;
                            if (string.IsNullOrEmpty(cabin))
                            {
                                cabin = ts.Cabin;
                            }

                            if (ConfigUtility.EnableIBEFull())
                            {
                                bundleCode = GetFareBasicCodeFromBundles(persistedReservation.TravelOptions, ts.TripIndex, null, destination, origin);
                                fareBasisCode = ts.FareBasisCode;
                            }
                            else
                            {
                                fareBasisCode = GetFareBasicCodeFromBundles(persistedReservation.TravelOptions, ts.TripIndex, ts.FareBasisCode, destination, origin);
                            }

                            segment = ts;

                            if (!ShowSeatMapForCarriers(ts.OperatingCarrier))
                            {
                                bool allowSeatMapForSupportedOaOperatingCarrier =
                                    (ConfigUtility.OaSeatMapSupportedVersion(applicationId, appVersion, ts.OperatingCarrier, ts.MarketingCarrier));
                                if (persistedReservation.IsReshopChange || !allowSeatMapForSupportedOaOperatingCarrier) // Throw Seat Map Advance Seat Not Avaiable exception for all OA Operating Flights which we are not supporting in booking path and for all OA Operiting carrriers in reshop as in reshop we will not support any oa seat map.d
                                {
                                    throw new MOBUnitedException(_configuration.GetValue<string>("SeatMapUnavailableOtherAirlines"));
                                }
                            }
                        }
                    }
                }
                #endregion
            }

            try
            {
                United.Service.Presentation.FlightRequestModel.SeatMap request = new Service.Presentation.FlightRequestModel.SeatMap();
                bool isSeatMapSupportedOa = IsSeatMapSupportedOa(segment.OperatingCarrier, segment.MarketingCarrier);
                int flightNum = 0;
                Int32.TryParse(segment.FlightNumber, out flightNum);
                request.FlightNumber = flightNum;
                request.MarketingCarrier = segment.MarketingCarrier;
                request.OperatingCarrier = segment.OperatingCarrier;
                request.DepartureAirport = origin;
                request.ArrivalAirport = destination;
                request.FlightDate = segment.DepartureDateTime;
                request.CabinType = "ALL";
                request.Characteristics = GetTourCode(persistedReservation.ShopReservationInfo2);
                United.Service.Presentation.SegmentModel.SeatRuleParameter seatRule = new United.Service.Presentation.SegmentModel.SeatRuleParameter();
                seatRule.Flight = new Service.Presentation.CommonModel.FlightProfile();
                seatRule.Flight.FlightNumber = flightNum.ToString();
                seatRule.Flight.DepartureAirport = origin;
                seatRule.Flight.DepartureDate = segment.DepartureDateTime;
                //Added as part of the changes for the exception 284001:Select seatsFormatException
                if (!string.IsNullOrEmpty(segment.DestinationDate))
                {
                    seatRule.Flight.ArrivalDate = Convert.ToDateTime(segment.DestinationDate).ToString("yyyy-MM-dd");
                }

                if (ConfigUtility.EnableIBEFull())
                {
                    if (segment.ShoppingProducts != null && segment.ShoppingProducts.Any())
                        seatRule.ProductCode = segment.ShoppingProducts[0].ProductCode;

                    seatRule.FareBasisCode = fareBasisCode;
                    seatRule.BundleCode = bundleCode;
                }
                else
                {
                    seatRule.FareBasisCode = fareBasisCode;
                }

                seatRule.Flight.ArrivalAirport = destination;
                seatRule.Flight.OperatingCarrierCode = segment.OperatingCarrier;
                seatRule.Segment = origin + destination;
                if (segment.OperatingCarrier.ToUpper().Trim() == "CO")
                    seatRule.AirCarrierType = "1";   //1 – CO plane ; 2 – Express/Connection
                else
                    seatRule.AirCarrierType = "2";
                //seatRule.CabinType = cabin;

                if (isSeatMapSupportedOa && segment.MarketingCarrier != "UA")
                {
                    request.OperatingFlightNumber = flightNum;
                    seatRule.Flight.OperatingFlightNumber = flightNum;
                }
                //Bug 190803 fix verified with Venkat & Mahi
                seatRule.ClassOfService = segment.ServiceClass;
                //seatRule.EliteStatus = "0";                
                seatRule.IsChaseCardMember = "false";
                seatRule.IsInCabinPet = "false";
                seatRule.IsLapChild = "false";

                if (ConfigUtility.EnableTravelerTypes(applicationId, appVersion, persistedReservation.IsReshopChange) &&
                    persistedReservation != null && persistedReservation.ShopReservationInfo2 != null && persistedReservation.ShopReservationInfo2.TravelerTypes != null
                    && persistedReservation.ShopReservationInfo2.TravelerTypes.Count > 0)
                {
                    if (persistedReservation.ShopReservationInfo2.DisplayTravelTypes.Any(t => t.TravelerType.ToString().Equals(MOBPAXTYPE.InfantLap.ToString()) || t.TravelerType.ToString().Equals(MOBPAXTYPE.InfantSeat.ToString())))
                        seatRule.IsLapChild = "true";

                    seatRule.PassengerCount = persistedReservation.ShopReservationInfo2.DisplayTravelTypes.Where(t => !t.PaxType.ToUpper().Equals("INF")).Count();
                }
                else
                {
                    seatRule.PassengerCount = persistedReservation.TravelersCSL.Count;
                }
                seatRule.IsOxygen = "false";
                seatRule.IsServiceAnimal = "false";
                seatRule.IsUnaccompaniedMinor = "false";
                seatRule.LanguageCode = "";
                seatRule.PassengerClass = string.Empty;
                seatRule.hasSSR = "false";
                request.Rules = new Collection<SeatRuleParameter>();
                request.Rules.Add(seatRule);

                request.Travelers = new Collection<ProductTraveler>();

                #region Award Travel Companion EPlus Seats Fix
                ShoppingResponse shop = new ShoppingResponse();
                shop = await _sessionHelperService.GetSession<ShoppingResponse>(sessionId, shop.ObjectName, new List<string> { sessionId, shop.ObjectName }).ConfigureAwait(false);
                if (!persistedReservation.IsReshopChange)
                {
                    if (shop != null)
                    {
                        if (shop.Request.AwardTravel && !string.IsNullOrEmpty(shop.Request.MileagePlusAccountNumber))
                        {
                            ProfileResponse profile = new ProfileResponse();
                            profile = await _sessionHelperService.GetSession<ProfileResponse>(sessionId, profile.ObjectName, new List<string> { sessionId, profile.ObjectName }).ConfigureAwait(false);

                            ProductTraveler travelerInformation = new ProductTraveler();
                            travelerInformation.Characteristics = new Collection<Characteristic>();

                            Characteristic travelerCharacteristic1 = new Characteristic();
                            travelerCharacteristic1.Code = "SponsorAccount";
                            travelerCharacteristic1.Value = profile.Response.Profiles[0].Travelers.Find(item => item.IsProfileOwner).MileagePlus.MileagePlusId;
                            travelerInformation.Characteristics.Add(travelerCharacteristic1);

                            Characteristic travelerCharacteristic2 = new Characteristic();
                            travelerCharacteristic2.Code = "SponsorEliteStatus";
                            travelerCharacteristic2.Value = profile.Response.Profiles[0].Travelers.Find(item => item.IsProfileOwner).MileagePlus.CurrentEliteLevel.ToString();
                            travelerInformation.Characteristics.Add(travelerCharacteristic2);

                            request.Travelers.Add(travelerInformation);
                        }
                    }
                }
                #endregion


                #region
                if (persistedReservation.TravelersCSL != null && persistedReservation.TravelersCSL.Count > 0)
                {
                    int i = 0;
                    MOBSHOPReservation reservation = new MOBSHOPReservation(_configuration,_cachingService);
                    foreach (string travelerKey in persistedReservation.TravelerKeys)
                    {
                        MOBCPTraveler bookingTravelerInfo = persistedReservation.TravelersCSL[travelerKey];

                        if (ConfigUtility.EnableTravelerTypes(applicationId, appVersion, persistedReservation.IsReshopChange) &&
                            persistedReservation.ShopReservationInfo2 != null && persistedReservation.ShopReservationInfo2.TravelerTypes != null && persistedReservation.ShopReservationInfo2.TravelerTypes.Count > 0)
                        {
                            if (bookingTravelerInfo.TravelerTypeCode.ToUpper().Equals("INF"))
                                continue;
                        }

                        ProductTraveler TravelerInformations = new ProductTraveler();

                        TravelerInformations.GivenName = bookingTravelerInfo.FirstName;
                        TravelerInformations.Surname = bookingTravelerInfo.LastName;
                        TravelerInformations.IsSelected = "true";
                        TravelerInformations.IsEPlusSubscriber = "false";
                        if (bookingTravelerInfo.AirRewardPrograms != null)
                        {
                            TravelerInformations.LoyaltyProgramProfile = new Service.Presentation.CommonModel.LoyaltyProgramProfile();
                            TravelerInformations.LoyaltyProgramProfile.LoyaltyProgramCarrierCode = string.Empty;
                            TravelerInformations.LoyaltyProgramProfile.LoyaltyProgramMemberID = string.Empty;
                            if (bookingTravelerInfo.AirRewardPrograms.Count > 0)
                            {
                                foreach (MOBBKLoyaltyProgramProfile loyaltyProfile in bookingTravelerInfo.AirRewardPrograms)
                                {
                                    MOBSHOPRewardProgram shopreward = null;
                                    if (reservation != null && reservation.RewardPrograms != null)
                                    {
                                        shopreward = reservation.RewardPrograms.Find(p => p.ProgramID == loyaltyProfile.ProgramId);
                                    }

                                    if (loyaltyProfile.CarrierCode.Trim().ToUpper() == "UA" || (string.IsNullOrEmpty(loyaltyProfile.CarrierCode) && shopreward != null && shopreward.Type == "UA"))
                                    {
                                        TravelerInformations.LoyaltyProgramProfile.LoyaltyProgramMemberID = loyaltyProfile.MemberId;
                                        TravelerInformations.LoyaltyProgramProfile.LoyaltyProgramCarrierCode = (string.IsNullOrEmpty(loyaltyProfile.CarrierCode) ? shopreward.Type.Trim().ToUpper() : loyaltyProfile.CarrierCode.Trim().ToUpper());
                                    }
                                }
                            }
                        }
                        request.Travelers.Add(TravelerInformations);
                    }
                }
                #endregion

                string jsonRequest = JsonConvert.SerializeObject(request);

                //LogEntries.Add(United.Logger.LogEntry.GetLogEntry<United.Service.Presentation.FlightRequestModel.SeatMap>(transactionId, "Request for Seat Engine Seat Map Service", "Request", applicationId, appVersion, deviceId, request));

                Session session = new Session();
                session = await _sessionHelperService.GetSession<Session>(sessionId, session.ObjectName, new List<string> { sessionId, session.ObjectName }).ConfigureAwait(false);
                if (session == null)
                {
                    throw new MOBUnitedException("Could not find your booking session.");
                }
                //string url = "http://unitedservicesstage.ual.com/7.2/flight/seatmap/GetSeatMapDetail";
                string url = persistedReservation.IsSSA ?
                             string.Format("GetSeatMapDetailWithFare") :
                             string.Format("GetSeatMapDetail");

                //LogEntries.Add(United.Logger.LogEntry.GetLogEntry<string>(transactionId, "Request URL for Seat Engine Seat Map Detail Service", "Request", applicationId, appVersion, deviceId, url));

                FlightSeatMapDetail xmlResponse = null;

                if (_configuration.GetValue<string>("UseNewSeatEngineExceptionFixCall") == null || Convert.ToBoolean(_configuration.GetValue<string>("UseNewSeatEngineExceptionFixCall")) != true)
                {
                    xmlResponse = await _seatMapService.SeatEngine<United.Service.Presentation.ProductModel.FlightSeatMapDetail>(session.Token, url, jsonRequest, sessionId);
                }

                //LogEntries.Add(United.Logger.LogEntry.GetLogEntry<string>(transactionId, "Backend response from Seat Engine Seat Map Service", "Request", applicationId, appVersion, deviceId, xmlResponse));

                //LogEntries.Add(United.Logger.LogEntry.GetLogEntry<string>(transactionId, "Seat Engine Seat Map Detail Resonse String (serialized XML)", "Response", applicationId, appVersion, deviceId, xmlResponse));
                //Adding version changes for Bug 95616 - Ranjit
                var seatMaps = await GetSeatMapWithFeesFromCSlResponse(xmlResponse, cabin, sessionId, isFirstSegmentInReservation, isSeatMapSupportedOa, applicationId, transactionId, returnPolarisLegendforSeatMap, appVersion);
                if (seatMaps != null && seatMaps.Count == 1 && seatMaps[0].IsOaSeatMap)
                {
                    seatMaps[0].OperatedByText = GetOperatedByText(segment.MarketingCarrier, segment.FlightNumber, segment.OperatingCarrierDescription);
                }

                return seatMaps;
            }
            catch (System.Exception ex)
            {
                //string err = ex.Message;
                throw ex;
            }
        }

        private Collection<Characteristic> GetTourCode(MOBSHOPReservationInfo2 shopReservationInfo2)
        {
            if (!_configuration.GetValue<bool>("SendTourCodeToSeatEngine"))
                return null;

            if (shopReservationInfo2 == null || shopReservationInfo2.Characteristics == null || !shopReservationInfo2.Characteristics.Any())
                return null;

            var tourCode = shopReservationInfo2.Characteristics.Where(c => c != null && c.Id != null && c.Id.Equals("tourboxcode", StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            if (tourCode == null || string.IsNullOrEmpty(tourCode.CurrentValue))
                return null;

            return new Collection<Characteristic> { new Characteristic { Code = "tourboxcode", Value = tourCode.CurrentValue.Trim() } };
        }

        private async Task<string> GetSeatMapLegendId(string from, string to, int numberOfCabins)
        {
            string seatMapLegendId = string.Empty;
            var listOfCabinIds = new List<ComplimentaryUpgradeCabin>();
            try
            {
                listOfCabinIds = await _seatEngineDynamoDB.GetSeatMapLegendId<List<ComplimentaryUpgradeCabin>>(from, to, numberOfCabins, _headers.ContextValues.SessionId);
                foreach (var Ids in listOfCabinIds)
                {
                    int secondCabinBrandingId = Ids.SecondCabinBrandingId != null ? 0 : Convert.ToInt32(Ids.SecondCabinBrandingId);
                    int thirdCabinBrandingId = Ids.ThirdCabinBrandingId != null ? 0 : Convert.ToInt32(Ids.ThirdCabinBrandingId);

                    if (thirdCabinBrandingId == 0)
                    {
                        if (secondCabinBrandingId == 1)
                        {
                            seatMapLegendId = "seatmap_legend5";
                        }
                        else if (secondCabinBrandingId == 2)
                        {
                            seatMapLegendId = "seatmap_legend4";
                        }
                        else if (secondCabinBrandingId == 3)
                        {
                            seatMapLegendId = "seatmap_legend3";
                        }
                    }
                    else if (thirdCabinBrandingId == 1)
                    {
                        seatMapLegendId = "seatmap_legend2";
                    }
                    else if (thirdCabinBrandingId == 4)
                    {
                        seatMapLegendId = "seatmap_legend1";
                    }
                }

            }
            catch (System.Exception ex)
            {
                Console.Write(ex.Message);
            }
            return seatMapLegendId;
        }

        private string GetOaCabinCOS(United.Service.Presentation.CommonModel.AircraftModel.Cabin cabinInfo)
        {
            bool isUpperDeck = false;
            bool.TryParse(cabinInfo.IsUpperDeck, out isUpperDeck);
            return isUpperDeck ?
                    "Upper Deck " + cabinInfo.Name :
                    cabinInfo.Name;
        }

        private bool IsPreferredSeatLimitedRecline(Collection<Characteristic> characteristics)
        {
            if (characteristics != null && characteristics.Count > 0)
            {
                string seatTypes = _configuration.GetValue<string>("PreferredSeatLimitedRecline") ?? string.Empty;
                var seatTypesList = seatTypes.Split('|');
                foreach (Characteristic characteristic in characteristics)
                {
                    if (characteristic != null && !string.IsNullOrEmpty(characteristic.Code) && !string.IsNullOrEmpty(characteristic.Value))
                    {
                        if ((characteristic.Code.Equals("DisplaySeatType") && seatTypesList.Any(s => s.Trim().Equals(characteristic.Value, StringComparison.OrdinalIgnoreCase))))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool IsUPPSeatMapSupportedVersion(int appId, string appVersion)
        {
            if (!string.IsNullOrEmpty(appVersion) && appId != -1)
            {
                return _configuration.GetValue<bool>("EnableUPPSeatmap")
                    && GeneralHelper.IsApplicationVersionGreater(appId, appVersion, "AndroidUPPSeatmapVersion", "iPhoneUPPSeatmapVersion", "", "", true, _configuration);
            }
            return false;
        }

        private string GetSeatTypeWithPreferred(United.Service.Presentation.CommonModel.AircraftModel.Seat seat, bool disableSeats, int applicationId, out bool isEplus, bool disableEplus = false)
        {

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            isEplus = false;

            //if (seatType.Lavatory || seatType.Closet || seatType.Galley  || seatType.Stairs  || seatType.Unusable || seatType.Inoperative|| seatType.Space)
            if (CheckSeatRRowType(seat.Characteristics, "IsLavatory") || CheckSeatRRowType(seat.Characteristics, "IsCloset") || CheckSeatRRowType(seat.Characteristics, "IsGalley") || CheckSeatRRowType(seat.Characteristics, "IsStairs") || CheckSeatRRowType(seat.Characteristics, "IsSpace"))
            {
                sb.Append("-");
            }
            //Bug-87823- Added below condition to show the "IsUnusableSeat" seat as blocked in UI- 01/04/2017 -Vijayan
            else if (CheckSeatRRowType(seat.Characteristics, "IsUnusableSeat"))
            {
                sb.Append("X");
            }
            else if (CheckSeatRRowType(seat.Characteristics, "IsInoperativeSeat"))
            {
                if (applicationId == 16)
                {
                    sb.Append("X");
                }
                else
                {
                    sb.Append("-");
                }
            }
            else if (CheckSeatRRowType(seat.Characteristics, "IsPermBlockedSeat"))
            {
                sb.Append("X");
            }
            else if (CheckSeatRRowType(seat.Characteristics, "IsBulkheadPrime") ||
                      CheckSeatRRowType(seat.Characteristics, "IsBulkheadPrimePlus") ||
                      CheckSeatRRowType(seat.Characteristics, "IsPrimeSeat") ||
                      CheckSeatRRowType(seat.Characteristics, "IsPrimePlusSeat") ||
                      CheckSeatRRowType(seat.Characteristics, "IsPrimeLegZoneSeat") ||
                      CheckSeatRRowType(seat.Characteristics, "IsPrimePlusLegRoomSeat") ||
                      CheckSeatRRowType(seat.Characteristics, "IsEconomyPlusBulkheadPrimeSeat") ||
                      CheckSeatRRowType(seat.Characteristics, "IsEconomyPlusBulkheadPrimePlusSeat") ||
                      CheckSeatRRowType(seat.Characteristics, "IsEconomyPlusExitPrimeSeat") ||
                      CheckSeatRRowType(seat.Characteristics, "IsEconomyPlusExitPrimePlusSeat") ||
                      CheckSeatRRowType(seat.Characteristics, "IsEconomyPlusPrimeSeat") ||
                      CheckSeatRRowType(seat.Characteristics, "IsEconomyPlusPrimePlusSeat"))
            {
                isEplus = true;
                if (!CheckSeatRRowType(seat.Characteristics, "IsAvailableSeat") || disableEplus)
                {
                    sb.Append("X");
                }
                else
                {
                    sb.Append("P");
                }
            }
            else if (IsPreferredSeat(seat))
            {
                if (CheckSeatRRowType(seat.Characteristics, "IsAvailableSeat") || CheckSeatRRowType(seat.Characteristics, "IsPermBlockedSeat") &&
                      CheckSeatRRowType(seat.Characteristics, "IsAvailableSeat"))
                {
                    sb.Append("PZ");
                }
                else
                {
                    sb.Append("X");
                }
            }
            else if (CheckSeatRRowType(seat.Characteristics, "IsAvailableSeat") ||
                      CheckSeatRRowType(seat.Characteristics, "IsPermBlockedSeat") &&
                      CheckSeatRRowType(seat.Characteristics, "IsAvailableSeat")
                    )
            {
                sb.Append("O");
            }
            else
            {
                sb.Append("X");
            }

            string aType = sb.ToString();

            //if (disableSeats) QC defect 1618 
            if (aType != "-" && disableSeats)
            {
                if (string.IsNullOrWhiteSpace(pcuOfferAmountForthisCabin))
                {
                    sb = new System.Text.StringBuilder();
                    sb.Append("X");
                }
            }

            return sb.ToString();
        }

        private bool IsPreferredSeat(Collection<Characteristic> characteristics, string program)
        {
            return IsPreferredSeatProgramCode(program) && IsPreferredSeatCharacteristics(characteristics);
        }

        private bool IsPreferredSeatCharacteristics(Collection<Characteristic> characteristics)
        {
            if (characteristics != null && characteristics.Count > 0)
            {
                string preferredSeatChar = _configuration.GetValue<string>("PreferredSeatBooleanCharacteristic") ?? string.Empty;
                var preferredSeatCharList = preferredSeatChar.Split('|');
                foreach (Characteristic characteristic in characteristics)
                {
                    if (characteristic != null && !string.IsNullOrEmpty(characteristic.Code) && !string.IsNullOrEmpty(characteristic.Value))
                    {
                        if (preferredSeatCharList.Any(s => s.Trim().Equals(characteristic.Code, StringComparison.OrdinalIgnoreCase)) && characteristic.Value.ToUpper().Trim() == "TRUE" ||
                            (characteristic.Code.Equals("DisplaySeatType") && IsPreferredSeatDisplayType(characteristic.Value)))
                        {
                            return true;
                        }
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

        private string getSeatType(Collection<United.Service.Presentation.CommonModel.Characteristic> seatCharacteristics, bool blockPrimeSeat, bool disableSeats, int applicationId, out bool isEplus, bool disableEplus = false)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            isEplus = false;
            //if (seatType.Lavatory || seatType.Closet || seatType.Galley  || seatType.Stairs  || seatType.Unusable || seatType.Inoperative|| seatType.Space)
            if (CheckSeatRRowType(seatCharacteristics, "IsLavatory") || CheckSeatRRowType(seatCharacteristics, "IsCloset") || CheckSeatRRowType(seatCharacteristics, "IsGalley") || CheckSeatRRowType(seatCharacteristics, "IsStairs") || CheckSeatRRowType(seatCharacteristics, "IsSpace"))
            {
                sb.Append("-");
            }
            //Bug-87823- Added below condition to show the "IsUnusableSeat" seat as blocked in UI- 01/04/2017 -Vijayan
            else if (CheckSeatRRowType(seatCharacteristics, "IsUnusableSeat"))
            {
                sb.Append("X");
            }
            else if (CheckSeatRRowType(seatCharacteristics, "IsInoperativeSeat"))
            {
                if (applicationId == 16)
                {
                    sb.Append("X");
                }
                else
                {
                    sb.Append("-");
                }
            }
            else if (CheckSeatRRowType(seatCharacteristics, "IsPermBlockedSeat"))
            {
                sb.Append("X");
            }
            else if (CheckSeatRRowType(seatCharacteristics, "IsBulkheadPrime") ||
                      CheckSeatRRowType(seatCharacteristics, "IsBulkheadPrimePlus") ||
                      CheckSeatRRowType(seatCharacteristics, "IsPrimeSeat") ||
                      CheckSeatRRowType(seatCharacteristics, "IsPrimePlusSeat") ||
                      CheckSeatRRowType(seatCharacteristics, "IsPrimeLegZoneSeat") ||
                      CheckSeatRRowType(seatCharacteristics, "IsPrimePlusLegRoomSeat") ||
                      CheckSeatRRowType(seatCharacteristics, "IsEconomyPlusBulkheadPrimeSeat") ||
                      CheckSeatRRowType(seatCharacteristics, "IsEconomyPlusBulkheadPrimePlusSeat") ||
                      CheckSeatRRowType(seatCharacteristics, "IsEconomyPlusExitPrimeSeat") ||
                      CheckSeatRRowType(seatCharacteristics, "IsEconomyPlusExitPrimePlusSeat") ||
                      CheckSeatRRowType(seatCharacteristics, "IsEconomyPlusPrimeSeat") ||
                      CheckSeatRRowType(seatCharacteristics, "IsEconomyPlusPrimePlusSeat"))
            {
                isEplus = true;
                if (!CheckSeatRRowType(seatCharacteristics, "IsAvailableSeat") || blockPrimeSeat || disableEplus)
                {
                    sb.Append("X");
                }
                else
                {
                    sb.Append("P");
                }
            }
            else if (CheckSeatRRowType(seatCharacteristics, "IsAvailableSeat") ||
                      CheckSeatRRowType(seatCharacteristics, "IsPermBlockedSeat") &&
                      CheckSeatRRowType(seatCharacteristics, "IsAvailableSeat")
                    )
            {
                sb.Append("O");
            }
            else
            {
                sb.Append("X");
            }

            string aType = sb.ToString();
            //if (disableSeats) QC defect 1618 
            if (aType != "-" && disableSeats)
            {
                if (string.IsNullOrWhiteSpace(pcuOfferAmountForthisCabin))
                {
                    sb = new System.Text.StringBuilder();
                    sb.Append("X");
                }
            }

            return sb.ToString();
        }

        private bool IsPreferredSeat(Service.Presentation.CommonModel.AircraftModel.Seat seat)
        {
            return seat.Price != null && !string.IsNullOrEmpty(seat.Price.PromotionCode) && IsPreferredSeat(seat.Characteristics, seat.Price.PromotionCode);
        }

        private void GetSeatPositionAccess(ref string rearFacingSeat, ref string frontFacingSeat, ref string leftFacingSeat, ref string rightFacingSeat, United.Service.Presentation.CommonModel.Characteristic characteristic)
        {
            //RearFacing Non-DAA:
            if (characteristic.Code.ToUpper().Trim() == "IsRearFacing".ToUpper().Trim())
            {
                rearFacingSeat = "FBB";
            }

            //FrontFAcing Non DAA FBF ; DAA DAFL|DAFR
            if (characteristic.Code.ToUpper().Trim() == "IsFrontFacing".ToUpper().Trim())
            {
                frontFacingSeat = "FBF";
            }
            else if (characteristic.Code.ToUpper().Trim() == "IsDAAFrontFacingLeftAccess".ToUpper().Trim())
            {
                frontFacingSeat = "DAFL";
            }
            else if (characteristic.Code.ToUpper().Trim() == "IsDAAFrontFacingRightAccess".ToUpper().Trim())
            {
                frontFacingSeat = "DAFR";
            }

            //LeftFacing Non-DAA: FBL; DAA:DAL
            if (characteristic.Code.ToUpper().Trim() == "IsLeftFacing".ToUpper().Trim())
            {
                leftFacingSeat = "FBL";
            }
            else if (characteristic.Code.ToUpper().Trim() == "IsDAALeftAngle".ToUpper().Trim())
            {
                leftFacingSeat = "DAL";
            }

            //Right Facing Non-DAA: FBR; DAA: DAR
            if (characteristic.Code.ToUpper().Trim() == "IsRightFacing".ToUpper().Trim())
            {
                rightFacingSeat = "FBR";
            }
            else if (characteristic.Code.ToUpper().Trim() == "IsDAARightAngle".ToUpper().Trim())
            {
                rightFacingSeat = "DAR";
            }
            else if (characteristic.Code.ToUpper().Trim() == "IsDAAFrontFacingRightAccessMiddle".ToUpper().Trim())
            {
                rightFacingSeat = "DAFRM";
            }

        }

        private MOBServicesAndFees GetServicesAndFees(United.Service.Presentation.CommonModel.AircraftModel.Seat seat)
        {
            MOBServicesAndFees serviceAndFee = null;
            if (seat.Price != null)
            {
                serviceAndFee = new MOBServicesAndFees();
                if (seat.Characteristics != null)
                {
                    foreach (United.Service.Presentation.CommonModel.Characteristic seatAttr in seat.Characteristics)
                    {
                        switch (seatAttr.Code)
                        {
                            case "SeatSection":
                                {
                                    break;
                                }
                            case "SeatLocation":
                                {
                                    break;
                                }
                            case "IsHeld":
                                {
                                    break;
                                }
                            case "IsOccupiedSeat":
                                {
                                    serviceAndFee.Available = true;
                                    if (seatAttr.Value.ToUpper().Equals("TRUE"))
                                    {
                                        serviceAndFee.Available = false;
                                    }

                                    break;
                                }
                            case "IsAvailableSeat":
                                {
                                    serviceAndFee.Available = true;
                                    break;
                                }
                            case "IsAdvanced":
                                {
                                    break;
                                }
                            case "ColSpan":
                                {
                                    break;
                                }
                            case "RowSpan":
                                {
                                    break;
                                }
                            case "DisplaySeatType":
                                {
                                    serviceAndFee.SeatFeature = seatAttr.Value;
                                    break;
                                }
                            case "SharesSeatType":
                                {
                                    break;
                                }
                        }
                    }
                }

                serviceAndFee.AgentDutyCode = string.Empty;
                serviceAndFee.AgentId = string.Empty;
                serviceAndFee.AgentTripleA = string.Empty;


                serviceAndFee.BaseFee = Convert.ToDecimal(seat.Price.BasePrice.Amount);
                serviceAndFee.Currency = seat.Price.BasePrice.Currency.Code;
                serviceAndFee.TotalFee = Convert.ToDecimal(seat.Price.Totals[0].Amount);
                serviceAndFee.Program = seat.Price.PromotionCode;
                serviceAndFee.SeatNumber = seat.Identifier;
                if (seat.Price.Taxes != null)
                {
                    if (seat.Price.Taxes.Count > 0)
                    {
                        serviceAndFee.Tax = Convert.ToDecimal(seat.Price.Taxes[0].Amount);
                    }
                }
            }

            return serviceAndFee;
        }

        private bool CheckEPlusSeatCode(string program)
        {
            if (_configuration.GetValue<string>("EPlusSeatProgramCodes") != null)
            {
                string[] codes = _configuration.GetValue<string>("EPlusSeatProgramCodes").Split('|');
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

        private async Task<Reservation> GetPersitedReservation(string sessionId)
        {
            try
            {
                var persistedReservation = await _sessionHelperService.GetSession<Reservation>(sessionId, new Reservation().ObjectName, new List<string> { sessionId, new Reservation().ObjectName }).ConfigureAwait(false);
                return persistedReservation;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private async Task<List<MOBSeatMap>> GetSeatMapWithFeesFromCSlResponse(United.Service.Presentation.ProductModel.FlightSeatMapDetail seatMapResponse, string bookingCabin, string sessionId, bool isQueryingFirstSegment, bool isSeatMapSupportedOa, int applicationId = -1, string transactionId = "", bool returnPolarisLegendforSeatMap = false, string appVersion = "")
        {
            List<MOBSeatMap> seatMap = new List<MOBSeatMap>();
            MOBSeatMap objMOBSeatMap = new MOBSeatMap();
            bool IsPolarisBranding = (_configuration.GetValue<string>("IsPolarisCabinBrandingON") != null) ? Convert.ToBoolean(_configuration.GetValue<string>("IsPolarisCabinBrandingON")) : false;
            bool isPreferredZoneEnabled = ConfigUtility.EnablePreferredZone(applicationId, appVersion); // Check Preferred Zone based on App version - Returns true or false // Kiran
            bool isPreferredZoneEnabledAndOlderVersion = !isPreferredZoneEnabled && _configuration.GetValue<bool>("isEnablePreferredZone");
            bool supressLMX = _seatMapEngine.SupressLMX(applicationId);
            List<string> cabinBrandingDescriptions = new List<string>();
            string oASeatMapBannerMessage = string.Empty;
            var persistedReservation = await GetPersitedReservation(sessionId);
            if (!persistedReservation.IsNullOrEmpty() && seatMapResponse != null && seatMapResponse.SeatMap != null && seatMapResponse.SeatMap.SegmentSeatMap[0].Aircraft != null && seatMapResponse.SeatMap.FlightInfo != null)
            {
                #region
                objMOBSeatMap = new MOBSeatMap();
                objMOBSeatMap.IsOaSeatMap = isSeatMapSupportedOa;
                int numberOfCabins = 0;

                if (seatMapResponse.SeatMap.SegmentSeatMap[0].Aircraft.Cabins.Count >= 1)
                {

                    numberOfCabins = seatMapResponse.SeatMap.SegmentSeatMap[0].Aircraft.Cabins.Count;
                    if (numberOfCabins > 3)
                    {
                        numberOfCabins = 3;
                    }
                    if (!returnPolarisLegendforSeatMap)
                    {
                        objMOBSeatMap.SeatLegendId = await GetSeatMapLegendId(seatMapResponse.SeatMap.FlightInfo.DepartureAirport, seatMapResponse.SeatMap.FlightInfo.ArrivalAirport, numberOfCabins);
                        //POLARIS Cabin Branding SeatMapLegend
                        #region polaris cabin

                        if (string.IsNullOrEmpty(objMOBSeatMap.SeatLegendId))
                        {
                            //POLARIS Cabin Branding SeatMapLegend Booking Path
                            objMOBSeatMap.SeatLegendId = "seatmap_legend5";

                        }
                        #endregion
                    }
                }
                objMOBSeatMap.SeatMapAvailabe = true;
                objMOBSeatMap.FleetType = seatMapResponse.SeatMap.SegmentSeatMap[0].Aircraft.Model != null ? (seatMapResponse.SeatMap.SegmentSeatMap[0].Aircraft.Model.Name != null ? seatMapResponse.SeatMap.SegmentSeatMap[0].Aircraft.Model.Name : "") : "";//Added the fleetname for Windows 8 Seat Map for Gavin and KO they are using the same as GIDS to show seat map seatMapResponse.SeatMapResponse;
                objMOBSeatMap.FlightNumber = Convert.ToInt32(seatMapResponse.SeatMap.SegmentSeatMap[0].SegmentInfo.FlightNumber);
                objMOBSeatMap.FlightDateTime = seatMapResponse.SeatMap.SegmentSeatMap[0].SegmentInfo.DepartureDate;
                if (objMOBSeatMap.Departure == null)
                {
                    objMOBSeatMap.Departure = new MOBAirport();
                }
                if (objMOBSeatMap.Arrival == null)
                {
                    objMOBSeatMap.Arrival = new MOBAirport();
                }
                objMOBSeatMap.Departure.Code = seatMapResponse.SeatMap.SegmentSeatMap[0].SegmentInfo.DepartureAirport;
                objMOBSeatMap.Arrival.Code = seatMapResponse.SeatMap.SegmentSeatMap[0].SegmentInfo.ArrivalAirport;

                objMOBSeatMap.LegId = "";
                int cabinCount = 0;
                var countNoOfFreeSeats = 0;
                int countNoOfPricedSeats = 0;
                foreach (United.Service.Presentation.CommonModel.AircraftModel.Cabin cabinInfo in seatMapResponse.SeatMap.SegmentSeatMap[0].Aircraft.Cabins)
                {
                    ++cabinCount;
                    MOBCabin tmpCabin = new MOBCabin();
                    tmpCabin.COS = objMOBSeatMap.IsOaSeatMap
                                    ? GetOaCabinCOS(cabinInfo) : cabinInfo.Name;
                    tmpCabin.Configuration = cabinInfo.Layout;
                    decimal lowestPrice = 0;
                    decimal highestPrice = 0;
                    string currency = string.Empty;

                    foreach (United.Service.Presentation.CommonModel.AircraftModel.SeatRow row in cabinInfo.SeatRows)
                    {
                        if (row.RowNumber < 1000)
                        {
                            #region
                            MOBRow tmpRow = new MOBRow();
                            tmpRow.Number = row.RowNumber.ToString();
                            if (!objMOBSeatMap.IsOaSeatMap)
                                tmpRow.Wing = CheckSeatRRowType(row.Characteristics, "IsWing");
                            int seatCol = 0;
                            foreach (United.Service.Presentation.CommonModel.AircraftModel.Seat seat in row.Seats)
                            {
                                #region
                                MOBSeatB tmpSeat = null;
                                string seatVal = tmpCabin.Configuration.Substring(seatCol, 1);
                                if (string.IsNullOrEmpty(seatVal.Trim()))
                                {
                                    tmpSeat = new MOBSeatB();
                                    tmpSeat.Exit = CheckSeatRRowType(seat.Characteristics, "IsExit");  //Code=IsExit
                                    tmpSeat.Number = "";
                                    tmpSeat.Fee = "";
                                    tmpSeat.LimitedRecline = false;
                                    tmpSeat.SeatValue = "-";
                                    tmpRow.Seats.Add(tmpSeat);
                                    seatCol++;
                                }
                                tmpSeat = new MOBSeatB();
                                tmpSeat.Number = seat.Identifier;
                                if (seat.Price != null)
                                {
                                    foreach (United.Service.Presentation.CommonModel.Charge feeInfo in seat.Price.Totals)
                                        tmpSeat.Fee = feeInfo.Amount.ToString();
                                }
                                else tmpSeat.Fee = "";
                                tmpSeat.LimitedRecline = false;
                                tmpSeat.Exit = CheckSeatRRowType(seat.Characteristics, "IsExit");  //Code=IsExit

                                if (!objMOBSeatMap.IsOaSeatMap)
                                {

                                    #region As per Kent email below are the scenarios
                                    //Limited-reclined
                                    //"IsEconomyPlusPrimeSeat"
                                    //"IsPrimeSeat"
                                    //"IsBulkheadPrime"
                                    //Fully-reclined
                                    //"IsEconomyPlusPrimePlusSeat"
                                    //"IsPrimePlusSeat"
                                    //"IsBulkheadPrimePlus"
                                    #endregion
                                    if (CheckSeatRRowType(seat.Characteristics, "IsEconomyPlusPrimeSeat") || CheckSeatRRowType(seat.Characteristics, "IsPrimeSeat") || CheckSeatRRowType(seat.Characteristics, "IsBulkheadPrime") || (isPreferredZoneEnabled && IsPreferredSeatLimitedRecline(seat.Characteristics))) // As Kent replied email about Limited Reclined codes
                                    {
                                        tmpSeat.LimitedRecline = true;
                                    }
                                    string rearFacingSeat = string.Empty, frontFacingSeat = string.Empty, leftFacingSeat = string.Empty, rightFacingSeat = string.Empty;
                                    tmpSeat.Program = string.Empty;
                                    foreach (United.Service.Presentation.CommonModel.Characteristic characteristic in seat.Characteristics)
                                    {
                                        if (applicationId == 16)
                                        {
                                            MOBCharacteristic c = new MOBCharacteristic();
                                            c.Code = characteristic.Code;
                                            c.Value = characteristic.Value;
                                            if (tmpSeat.Characteristics == null)
                                            {
                                                tmpSeat.Characteristics = new List<MOBCharacteristic>();
                                            }
                                            tmpSeat.Characteristics.Add(c);
                                        }

                                        GetSeatPositionAccess(ref rearFacingSeat, ref frontFacingSeat, ref leftFacingSeat, ref rightFacingSeat, characteristic);
                                    }
                                    tmpSeat.Program = rearFacingSeat + "|" + frontFacingSeat + "|" + leftFacingSeat + "|" + rightFacingSeat;
                                    tmpSeat.Program = tmpSeat.Program.Trim('|');
                                }

                                bool disableSeats = true;
                                if ((bookingCabin.ToUpper().Equals("FIRST") || bookingCabin.Equals("United First") || bookingCabin.Equals("United Global First") || bookingCabin.ToUpper().Equals(seatMapLegendEntry1.Substring(1).ToUpper())) && (cabinInfo.Name.Equals("United First") || cabinInfo.Name.Equals("United Global First") || cabinInfo.Name.ToUpper().Equals(seatMapLegendEntry1.Substring(1).ToUpper()) || cabinInfo.Name.ToUpper().Equals("FIRST")))
                                {
                                    disableSeats = false;
                                }
                                else if ((bookingCabin.ToUpper().Equals("BUSINESS") || bookingCabin.Equals("BusinessFirst") || bookingCabin.Equals("United Business") || bookingCabin.Equals("United BusinessFirst") || bookingCabin.ToUpper().Equals(seatMapLegendEntry2.Substring(1).ToUpper())) && (cabinInfo.Name.Equals("United Business") || cabinInfo.Name.Equals("BusinessFirst") || cabinInfo.Name.Equals("United BusinessFirst") || cabinInfo.Name.ToUpper().Equals(seatMapLegendEntry2.Substring(1).ToUpper()) || cabinInfo.Name.ToUpper().Equals("BUSINESS")))
                                {
                                    disableSeats = false;
                                }
                                else if ((bookingCabin.ToUpper().Trim().Equals("COACH") || bookingCabin.Equals("Economy") || bookingCabin.Equals("United Economy") || cabinInfo.Name.Equals("United Premium Plus")) && (cabinInfo.Name.Equals("Economy") || cabinInfo.Name.Equals("United Economy") || objMOBSeatMap.IsOaSeatMap && cabinInfo.Name.Equals("Premium Economy")))
                                {
                                    disableSeats = false;
                                }
                                else if (IsUPPSeatMapSupportedVersion(applicationId, appVersion) && bookingCabin.ToUpper().Trim().Equals("United Premium Plus".ToUpper().Trim()) && cabinInfo.Name.ToUpper().Trim().Equals("United PremiumPlus".ToUpper().Trim()))
                                {
                                    disableSeats = false;
                                    tmpSeat.DisplaySeatFeature = "United Premium Plus";
                                }
                                else if (IsUPPSeatMapSupportedVersion(applicationId, appVersion)
                                            && bookingCabin.ToUpper().Trim().Equals((_configuration.GetValue<string>("PremiumEconomyCabinForOASeatMapEnableToggleText") ?? "").ToUpper().Trim())
                                            && cabinInfo.Name.ToUpper().Trim().Equals((_configuration.GetValue<string>("PremiumEconomyCabinForOASeatMapEnableToggleText") ?? "").ToUpper().Trim()))
                                {
                                    disableSeats = false;
                                }

                                bool IsOaPremiumEconomyCabin = cabinInfo.Name.Equals("Premium Economy");
                                bool isEplus = false;
                                if (isPreferredZoneEnabled)
                                {
                                    tmpSeat.SeatValue = objMOBSeatMap.IsOaSeatMap ?
                                                        GetOaSeatType(seat.Characteristics, disableSeats, IsOaPremiumEconomyCabin, out isEplus) :
                                                        GetSeatTypeWithPreferred(seat, disableSeats, applicationId, out isEplus, (persistedReservation.IsELF || IsIBE(persistedReservation)));
                                }
                                else
                                {
                                    tmpSeat.SeatValue = objMOBSeatMap.IsOaSeatMap ?
                                                    GetOaSeatType(seat.Characteristics, disableSeats, IsOaPremiumEconomyCabin, out isEplus) :
                                                    getSeatType(seat.Characteristics, false, disableSeats, applicationId, out isEplus, (persistedReservation.IsELF || IsIBE(persistedReservation)));
                                }
                                tmpSeat.IsEPlus = isEplus;
                                //TODO
                                #region
                                //if (!objMOBSeatMap.IsOaSeatMap && (cabinInfo.Name.Equals(UAWSSeatEngine.Cabins.Economy) || cabinInfo.Name.Equals("Economy") || cabinInfo.Name.Equals("United Economy")))
                                //{
                                //    if (seat.Characteristics != null && seat.Characteristics.Count > 0)
                                //    {
                                //        tmpSeat.IsEPlus = tmpSeat.IsEPlus || IsEPlusSeat(seat.Characteristics);
                                //        if (tmpSeat.seatvalue != "X" && tmpSeat.IsEPlus && (persistedReservation.IsELF || IsIBE(persistedReservation)))
                                //        {
                                //            tmpSeat.seatvalue = "X";
                                //        }
                                //    }
                                //    if (((persistedReservation.IsELF || IsIBE(persistedReservation)) || persistedReservation.IsSSA && _configuration.GetValue<bool>("SSA_1B")) && !tmpSeat.IsEPlus)
                                //    {
                                //        tmpSeat.DisplaySeatFeature = "Economy";
                                //        if (isPreferredZoneEnabled)
                                //        {
                                //            tmpSeat.SeatFeatureList = new List<string>();
                                //            tmpSeat.SeatFeatureList.Add("Advance seat assignment");
                                //            if (tmpSeat.LimitedRecline)
                                //                tmpSeat.SeatFeatureList.Add("Limited recline");
                                //        }
                                //        else
                                //        {
                                //            tmpSeat.SeatFeatureInfo = "Advance seat assignment";
                                //        }
                                //    }
                                //}
                                #endregion

                                #region Get Services and Fees
                                MOBServicesAndFees tmpServicesAndFees = new MOBServicesAndFees();
                                if (tmpSeat.ServicesAndFees == null)
                                {
                                    tmpSeat.ServicesAndFees = new List<MOBServicesAndFees>();
                                }
                                if ((tmpServicesAndFees = GetServicesAndFees(seat)) != null)
                                {
                                    tmpSeat.ServicesAndFees.Add(tmpServicesAndFees);

                                    if (tmpSeat.ServicesAndFees.Count > 0)
                                    {
                                        tmpSeat.IsEPlus = tmpSeat.IsEPlus || CheckEPlusSeatCode(tmpSeat.ServicesAndFees[0].Program);
                                        if (tmpSeat.SeatValue != "X" && tmpSeat.IsEPlus && (persistedReservation.IsELF || IsIBE(persistedReservation)))
                                        {
                                            tmpSeat.SeatValue = "X";
                                        }
                                        if (tmpSeat.IsEPlus)
                                        {
                                            tmpSeat.DisplaySeatFeature = "Economy Plus";
                                            if (isPreferredZoneEnabled)
                                            {
                                                tmpSeat.SeatFeatureList = new List<string>();
                                                tmpSeat.SeatFeatureList.Add("Extra Legroom");
                                                if (tmpSeat.LimitedRecline)
                                                    tmpSeat.SeatFeatureList.Add("Limited recline");
                                                if (!supressLMX)
                                                    tmpSeat.SeatFeatureList.Add("Eligible for PQD");
                                            }
                                            else
                                            {
                                                tmpSeat.SeatFeatureInfo = "";
                                            }
                                        }
                                        if (isPreferredZoneEnabled && tmpSeat.SeatValue == "PZ" ||
                                            isPreferredZoneEnabledAndOlderVersion && tmpSeat.SeatValue == "O" && IsPreferredSeat(seat))
                                        {
                                            tmpSeat.DisplaySeatFeature = "Preferred Seat";
                                            tmpSeat.SeatFeatureList = new List<string>();
                                            tmpSeat.SeatFeatureList.Add("Standard legroom");
                                            tmpSeat.SeatFeatureList.Add("Favorable location in Economy");
                                            if (tmpSeat.LimitedRecline)
                                            {
                                                tmpSeat.SeatFeatureList.Add("Limited recline");
                                            }
                                        }
                                    }

                                    if (string.IsNullOrEmpty(currency))
                                    {
                                        currency = tmpServicesAndFees.Currency;
                                    }

                                    ///225816 - PROD mApps: Remove white dot for Economy Plus recommended seat from interactive seatmap
                                    ///Srini - 01/08/2018
                                    if (!_configuration.GetValue<bool>("BugFixToggleFor17M"))
                                    {
                                        if (lowestPrice == 0 && tmpServicesAndFees.TotalFee != 0)
                                        {
                                            lowestPrice = tmpServicesAndFees.TotalFee;
                                            highestPrice = tmpServicesAndFees.TotalFee;
                                        }

                                        if (lowestPrice > tmpServicesAndFees.TotalFee)
                                        {
                                            lowestPrice = tmpServicesAndFees.TotalFee;
                                        }

                                        if (highestPrice < tmpServicesAndFees.TotalFee)
                                        {
                                            highestPrice = tmpServicesAndFees.TotalFee;
                                        }
                                    }
                                }
                                #endregion

                                CountNoOfFreeSeatsAndPricedSeats(tmpSeat, ref countNoOfFreeSeats, ref countNoOfPricedSeats);

                                tmpRow.Seats.Add(tmpSeat);

                                seatCol++;
                                #endregion
                            }
                            if (tmpRow.Seats == null || tmpRow.Seats.Count != cabinInfo.Layout.Length)
                            {
                                if (objMOBSeatMap.IsOaSeatMap)
                                    throw new MOBUnitedException(_configuration.GetValue<string>("SeatMapUnavailableOtherAirlines"));
                                throw new MOBUnitedException("Unable to get the seat map for the flight you requested.");
                            }
                            tmpCabin.Rows.Add(tmpRow);
                            #endregion
                        }
                        else if (row.RowNumber >= 1000 && applicationId == 16)
                        {
                            #region
                            MOBRow tmpRow = new MOBRow();
                            tmpRow.Number = row.RowNumber.ToString();
                            if (!objMOBSeatMap.IsOaSeatMap)
                                tmpRow.Wing = CheckSeatRRowType(row.Characteristics, "IsWing");
                            int seatCol = 0;
                            foreach (United.Service.Presentation.CommonModel.AircraftModel.Seat seat in row.Seats)
                            {
                                #region
                                MOBSeatB tmpSeat = null;
                                string seatVal = tmpCabin.Configuration.Substring(seatCol, 1);
                                if (string.IsNullOrEmpty(seatVal.Trim()))
                                {
                                    tmpSeat = new MOBSeatB();
                                    tmpSeat.Exit = CheckSeatRRowType(seat.Characteristics, "IsExit");  //Code=IsExit
                                    tmpSeat.Number = "";
                                    tmpSeat.Fee = "";
                                    tmpSeat.LimitedRecline = false;
                                    tmpSeat.SeatValue = "-";
                                    tmpRow.Seats.Add(tmpSeat);
                                    seatCol++;
                                }
                                tmpSeat = new MOBSeatB();
                                tmpSeat.Number = seat.Identifier;
                                if (seat.Price != null)
                                {
                                    foreach (United.Service.Presentation.CommonModel.Charge feeInfo in seat.Price.Totals)
                                        tmpSeat.Fee = feeInfo.Amount.ToString();
                                }
                                else tmpSeat.Fee = "";
                                tmpSeat.LimitedRecline = false;
                                tmpSeat.Exit = CheckSeatRRowType(seat.Characteristics, "IsExit"); //Code=IsExit

                                if (!objMOBSeatMap.IsOaSeatMap)
                                {
                                    #region As per Kent email below are the scenarios
                                    //Limited-reclined
                                    //"IsEconomyPlusPrimeSeat"
                                    //"IsPrimeSeat"
                                    //"IsBulkheadPrime"
                                    //Fully-reclined
                                    //"IsEconomyPlusPrimePlusSeat"
                                    //"IsPrimePlusSeat"
                                    //"IsBulkheadPrimePlus"
                                    #endregion
                                    if (CheckSeatRRowType(seat.Characteristics, "IsEconomyPlusPrimeSeat") || CheckSeatRRowType(seat.Characteristics, "IsPrimeSeat") || CheckSeatRRowType(seat.Characteristics, "IsBulkheadPrime") || (isPreferredZoneEnabled && IsPreferredSeatLimitedRecline(seat.Characteristics))) // As Kent replied email about Limited Reclined codes
                                    {
                                        tmpSeat.LimitedRecline = true;
                                    }

                                    string rearFacingSeat = string.Empty, frontFacingSeat = string.Empty, leftFacingSeat = string.Empty, rightFacingSeat = string.Empty;
                                    tmpSeat.Program = string.Empty;
                                    foreach (United.Service.Presentation.CommonModel.Characteristic characteristic in seat.Characteristics)
                                    {
                                        if (applicationId == 16)
                                        {
                                            MOBCharacteristic c = new MOBCharacteristic();
                                            c.Code = characteristic.Code;
                                            c.Value = characteristic.Value;
                                            if (tmpSeat.Characteristics == null)
                                            {
                                                tmpSeat.Characteristics = new List<MOBCharacteristic>();
                                            }
                                            tmpSeat.Characteristics.Add(c);
                                        }

                                        rearFacingSeat = (characteristic.Code.ToUpper().Trim() == "IsRearFacing".ToUpper().Trim() ? "FBB" : rearFacingSeat);
                                        frontFacingSeat = (characteristic.Code.ToUpper().Trim() == "IsFrontFacing".ToUpper().Trim() ? "FBF" : frontFacingSeat);
                                        leftFacingSeat = (characteristic.Code.ToUpper().Trim() == "IsLeftFacing".ToUpper().Trim() ? "FBL" : leftFacingSeat);
                                        rightFacingSeat = (characteristic.Code.ToUpper().Trim() == "IsRightFacing".ToUpper().Trim() ? "FBR" : rightFacingSeat);
                                    }
                                    tmpSeat.Program = rearFacingSeat + "|" + frontFacingSeat + "|" + leftFacingSeat + "|" + rightFacingSeat;
                                    tmpSeat.Program = tmpSeat.Program.Trim('|');
                                }

                                bool disableSeats = true;
                                if ((bookingCabin.ToUpper().Equals("FIRST") || bookingCabin.Equals("First") || bookingCabin.Equals("United First") || bookingCabin.Equals("United Global First") || bookingCabin.ToUpper().Equals(seatMapLegendEntry1.Substring(1).ToUpper())) && (cabinInfo.Name.Equals("United First") || cabinInfo.Name.Equals("United Global First") || cabinInfo.Name.ToUpper().Equals(seatMapLegendEntry1.Substring(1).ToUpper()) || cabinInfo.Name.ToUpper().Equals("FIRST")))
                                {
                                    disableSeats = false;
                                }
                                else if ((bookingCabin.ToUpper().Equals("BUSINESS") || bookingCabin.Equals("BusinessFirst") || bookingCabin.Equals("United Business") || bookingCabin.Equals("United BusinessFirst") || bookingCabin.ToUpper().Equals(seatMapLegendEntry2.Substring(1).ToUpper())) && (cabinInfo.Name.Equals("United Business") || cabinInfo.Name.Equals("BusinessFirst") || cabinInfo.Name.Equals("United BusinessFirst") || cabinInfo.Name.ToUpper().Equals(seatMapLegendEntry2.Substring(1).ToUpper()) || cabinInfo.Name.ToUpper().Equals("BUSINESS")))
                                {
                                    disableSeats = false;
                                }
                                else if ((bookingCabin.ToUpper().Trim().Equals("COACH") || bookingCabin.Equals("Economy") || bookingCabin.Equals("United Economy")) && (cabinInfo.Name.Equals("Economy") || cabinInfo.Name.Equals("United Economy") || objMOBSeatMap.IsOaSeatMap && cabinInfo.Name.Equals("Premium Economy")))
                                {
                                    disableSeats = false;
                                }
                                bool IsOaPremiumEconomyCabin = cabinInfo.Name.Equals("Premium Economy");
                                bool isEplus = false;
                                if (isPreferredZoneEnabled)
                                {
                                    tmpSeat.SeatValue = objMOBSeatMap.IsOaSeatMap ?
                                                        GetOaSeatType(seat.Characteristics, disableSeats, IsOaPremiumEconomyCabin, out isEplus) :
                                                        GetSeatTypeWithPreferred(seat, disableSeats, applicationId, out isEplus, (persistedReservation.IsELF || IsIBE(persistedReservation)));
                                }
                                else
                                {
                                    tmpSeat.SeatValue = objMOBSeatMap.IsOaSeatMap ?
                                                        GetOaSeatType(seat.Characteristics, disableSeats, IsOaPremiumEconomyCabin, out isEplus) :
                                                        getSeatType(seat.Characteristics, false, disableSeats, applicationId, out isEplus, (persistedReservation.IsELF || IsIBE(persistedReservation)));
                                }
                                tmpSeat.IsEPlus = isEplus;

                                #region Get Services and Fees
                                MOBServicesAndFees tmpServicesAndFees = new MOBServicesAndFees();
                                if (tmpSeat.ServicesAndFees == null)
                                {
                                    tmpSeat.ServicesAndFees = new List<MOBServicesAndFees>();
                                }
                                if ((tmpServicesAndFees = GetServicesAndFees(seat)) != null)
                                {
                                    tmpSeat.ServicesAndFees.Add(tmpServicesAndFees);

                                    if (tmpSeat.ServicesAndFees.Count > 0)
                                    {
                                        tmpSeat.IsEPlus = tmpSeat.IsEPlus || CheckEPlusSeatCode(tmpSeat.ServicesAndFees[0].Program);
                                        if (tmpSeat.SeatValue != "X" && tmpSeat.IsEPlus && (persistedReservation.IsELF || IsIBE(persistedReservation)))
                                        {
                                            tmpSeat.SeatValue = "X";
                                        }
                                        if (tmpSeat.IsEPlus)
                                        {
                                            tmpSeat.DisplaySeatFeature = "Economy Plus";
                                            if (isPreferredZoneEnabled)
                                            {
                                                tmpSeat.SeatFeatureList = new List<string>();
                                                tmpSeat.SeatFeatureList.Add("Extra Legroom");
                                                if (tmpSeat.LimitedRecline)
                                                    tmpSeat.SeatFeatureList.Add("Limited recline");
                                                if (!supressLMX)
                                                    tmpSeat.SeatFeatureList.Add("Eligible for PQD");
                                            }
                                            else
                                            {
                                                tmpSeat.SeatFeatureInfo = "";
                                            }
                                        }
                                        if (isPreferredZoneEnabled && tmpSeat.SeatValue == "PZ" ||
                                            isPreferredZoneEnabledAndOlderVersion && tmpSeat.SeatValue == "O" && IsPreferredSeat(seat))
                                        {
                                            tmpSeat.DisplaySeatFeature = "Preferred Seat";
                                            tmpSeat.SeatFeatureList = new List<string>();
                                            tmpSeat.SeatFeatureList.Add("Standard legroom");
                                            tmpSeat.SeatFeatureList.Add("Favorable location in Economy");
                                            if (tmpSeat.LimitedRecline)
                                            {
                                                tmpSeat.SeatFeatureList.Add("Limited recline");
                                            }
                                        }
                                    }

                                    if (string.IsNullOrEmpty(currency))
                                    {
                                        currency = tmpServicesAndFees.Currency;
                                    }

                                    ///225816 - PROD mApps: Remove white dot for Economy Plus recommended seat from interactive seatmap
                                    ///Srini - 01/08/2018
                                    if (!_configuration.GetValue<bool>("BugFixToggleFor17M"))
                                    {
                                        if (lowestPrice == 0 && tmpServicesAndFees.TotalFee != 0)
                                        {
                                            lowestPrice = tmpServicesAndFees.TotalFee;
                                            highestPrice = tmpServicesAndFees.TotalFee;
                                        }

                                        if (lowestPrice > tmpServicesAndFees.TotalFee)
                                        {
                                            lowestPrice = tmpServicesAndFees.TotalFee;
                                        }

                                        if (highestPrice < tmpServicesAndFees.TotalFee)
                                        {
                                            highestPrice = tmpServicesAndFees.TotalFee;
                                        }
                                    }
                                }
                                #endregion

                                CountNoOfFreeSeatsAndPricedSeats(tmpSeat, ref countNoOfFreeSeats, ref countNoOfPricedSeats);

                                tmpRow.Seats.Add(tmpSeat);

                                seatCol++;
                                #endregion
                            }
                            if (tmpRow.Seats == null || tmpRow.Seats.Count != cabinInfo.Layout.Length)
                            {
                                if (objMOBSeatMap.IsOaSeatMap)
                                    throw new MOBUnitedException(_configuration.GetValue<string>("SeatMapUnavailableOtherAirlines"));
                                throw new MOBUnitedException("Unable to get the seat map for the flight you requested.");
                            }
                            tmpCabin.Rows.Add(tmpRow);
                            #endregion
                        }
                    }

                    ///225816 - PROD mApps: Remove white dot for Economy Plus recommended seat from interactive seatmap
                    ///Srini - 01/08/2018
                    if (!_configuration.GetValue<bool>("BugFixToggleFor17M"))
                    {
                        if (lowestPrice > 0)
                        {
                            foreach (MOBRow seatRow in tmpCabin.Rows)
                            {
                                foreach (MOBSeatB seat in seatRow.Seats)
                                {
                                    if (seat.ServicesAndFees != null && seat.ServicesAndFees.Count > 0)
                                    {
                                        if (seat.ServicesAndFees[0].TotalFee == lowestPrice)
                                        {
                                            seat.IsLowestEPlus = true;
                                        }
                                        if (seat.ServicesAndFees[0].TotalFee == highestPrice)
                                        {
                                            seat.IsHighestEPlus = true;
                                        }
                                    }
                                }
                            }
                        }

                        if (highestPrice > 0)
                        {
                            string currencySymbol = string.Empty;
                            if (currency == "USD")
                            {
                                currencySymbol = "$";
                            }
                            objMOBSeatMap.EplusPromotionMessage = string.Format(_configuration.GetValue<string>("EPlusPromotionMessage"), currencySymbol + highestPrice);
                        }
                    }
                    tmpCabin.Configuration = tmpCabin.Configuration.Replace(" ", "-");

                    if (!objMOBSeatMap.IsOaSeatMap && cabinCount == 4)
                    {
                        objMOBSeatMap.Cabins.Insert(2, tmpCabin);
                        tmpCabin.COS = "Upper Deck " + tmpCabin.COS;
                    }
                    else
                    {
                        objMOBSeatMap.Cabins.Add(tmpCabin);
                    }
                }
                //should do a version check and change. future version check..... sandeep interline. throw new MOBUnitedException(GetDocumentTextFromDataBase("AC_SEATMAP_Exception_TEXT"));
                //   bool isSupportedVersion = UtilityNew.isApplicationVersionGreater2(request.Application.Id, request.Application.Version.Major, "AndroidOAFlifoMapVersion", "iPhoneOAFlifoMapVersion", "", "");


                if (objMOBSeatMap.IsOaSeatMap && countNoOfFreeSeats == 0)
                {
                    //sandeep
                    //UtilityNew.isApplicationVersionGreaterorEqual "AndroidOaSeatMapExceptionVersion", "iPhoneOaSeatMapExceptionVersion",
                    bool oaExceptionVersionCheck = OaSeatMapExceptionVersion(applicationId, appVersion);

                    if (oaExceptionVersionCheck)
                    {
                        throw new MOBUnitedException(await GetDocumentTextFromDataBase("OA_SEATMAP_Exception_TEXT"));
                    }
                    else
                    {
                        throw new MOBUnitedException(_configuration.GetValue<string>("SeatMapUnavailableOtherAirlines"));
                    }
                    // throw new MOBUnitedException(_configuration.GetValue<string>("SeatMapUnavailableOtherAirlines"]);
                }
                if (Convert.ToBoolean(_configuration.GetValue<string>("checkForPAXCount")))
                {
                    string readOnlySeatMapinBookingPathOAAirlines = string.Empty;
                    readOnlySeatMapinBookingPathOAAirlines = _configuration.GetValue<string>("ReadonlySeatMapinBookingPathOAAirlines");


                    if (objMOBSeatMap.IsOaSeatMap && !seatMapResponse.SeatMap.FlightInfo.CarrierCode.Equals("SQ"))
                    {
                        if ((countNoOfFreeSeats <= persistedReservation.NumberOfTravelers))
                        {
                            bool oaExceptionVersionCheck = OaSeatMapExceptionVersion(applicationId, appVersion);

                            if (oaExceptionVersionCheck)
                            {
                                throw new MOBUnitedException(await GetDocumentTextFromDataBase("OA_SEATMAP_Exception_TEXT"));
                            }
                            else
                            {
                                throw new MOBUnitedException(_configuration.GetValue<string>("SeatMapUnavailableOtherAirlines"));
                            }
                        }
                        else if (!string.IsNullOrEmpty(readOnlySeatMapinBookingPathOAAirlines))
                        {
                            string[] readOnlySeatMapAirlines = { };
                            readOnlySeatMapAirlines = readOnlySeatMapinBookingPathOAAirlines.Split(',');
                            foreach (string airline in readOnlySeatMapAirlines)
                            {
                                if (seatMapResponse.SeatMap.FlightInfo.CarrierCode.ToUpper().Equals(airline.Trim().ToUpper()))
                                {
                                    objMOBSeatMap.IsReadOnlySeatMap = true;
                                    objMOBSeatMap.OASeatMapBannerMessage = _configuration.GetValue<string>("OASeatMapBannerMessage") != null ? _configuration.GetValue<string>("OASeatMapBannerMessage").Trim() : string.Empty;
                                    break;
                                }
                            }

                        }
                    }
                }
                if (objMOBSeatMap.IsOaSeatMap)
                {
                    objMOBSeatMap.SeatLegendId = _configuration.GetValue<string>("SeatMapLegendForOtherAirlines");
                }
                else
                {
                    #region Consuming CabinBranding Service

                    //POLARIS Cabin Branding SeatMap FlightStatus
                    if (IsPolarisBranding)
                    {
                        #region

                        //Generating Token
                        string flifoAuthenticationToken = string.Empty;

                        flifoAuthenticationToken = await _dPService.GetAnonymousToken(applicationId, transactionId, _configuration);
                        string fDate =
                            DateTime.ParseExact(
                                Convert.ToDateTime(seatMapResponse.SeatMap.FlightInfo.DepartureDate).ToShortDateString(),
                                "M/d/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                        cabinBrandingDescriptions = await GetPolarisCabinBranding(flifoAuthenticationToken,
                            seatMapResponse.SeatMap.FlightInfo.FlightNumber,
                            seatMapResponse.SeatMap.FlightInfo.DepartureAirport, fDate,
                            seatMapResponse.SeatMap.FlightInfo.ArrivalAirport, numberOfCabins.ToString(), "en-US",
                            sessionId, "UA", "UA");

                        if (cabinBrandingDescriptions != null && cabinBrandingDescriptions.Count > 0)
                        {
                            //objMOBSeatMap.SeatLegendId = Utility.GetPolarisCabinBrandingSeatMapLegendId(cabinBrandingDescriptions);
                            if (returnPolarisLegendforSeatMap)
                            {
                                //Adding version changes for Bug 95616 - Ranjit
                                objMOBSeatMap.SeatLegendId =
                                    await GetPolarisSeatMapLegendId(
                                        seatMapResponse.SeatMap.FlightInfo.DepartureAirport,
                                        seatMapResponse.SeatMap.FlightInfo.ArrivalAirport, numberOfCabins,
                                        cabinBrandingDescriptions, applicationId, appVersion);
                            }
                            foreach (MOBCabin mc in objMOBSeatMap.Cabins)
                            {

                                if (objMOBSeatMap.Cabins.Count > 3 && mc.COS.ToUpper().Contains("UPPER DECK"))
                                {
                                    objMOBSeatMap.Cabins[0].COS = cabinBrandingDescriptions[0].ToString();
                                    objMOBSeatMap.Cabins[1].COS = cabinBrandingDescriptions[1].ToString();
                                    objMOBSeatMap.Cabins[2].COS = "Upper Deck " +
                                                                  cabinBrandingDescriptions[1].ToString();
                                    objMOBSeatMap.Cabins[3].COS = cabinBrandingDescriptions[2].ToString();

                                }
                                else if (objMOBSeatMap.Cabins.Count == 3)
                                {
                                    objMOBSeatMap.Cabins[0].COS = cabinBrandingDescriptions[0].ToString();
                                    objMOBSeatMap.Cabins[1].COS = cabinBrandingDescriptions[1].ToString();
                                    objMOBSeatMap.Cabins[2].COS = cabinBrandingDescriptions[2].ToString();
                                }
                                else if (objMOBSeatMap.Cabins.Count == 2)
                                {
                                    objMOBSeatMap.Cabins[0].COS = cabinBrandingDescriptions[0].ToString();
                                    objMOBSeatMap.Cabins[1].COS = cabinBrandingDescriptions[1].ToString();
                                }
                                else
                                {
                                    objMOBSeatMap.Cabins[0].COS = cabinBrandingDescriptions[0].ToString();
                                }

                            }
                        }
                        else
                        {
                            throw new MOBUnitedException("Cabin Branding Service returned null");
                        }

                        #endregion
                    }
                    else if (returnPolarisLegendforSeatMap)
                    {
                        objMOBSeatMap.SeatLegendId =
                            await GetPolarisSeatMapLegendIdPolarisOff(
                                seatMapResponse.SeatMap.FlightInfo.DepartureAirport,
                                seatMapResponse.SeatMap.FlightInfo.ArrivalAirport, numberOfCabins);
                    }

                    #endregion Consuming CabinBranding Service
                }
                bool isInCheckInWindow = IsInCheckInWindow(seatMapResponse);

                objMOBSeatMap.ShowInfoMessageOnSeatMap = objMOBSeatMap.IsOaSeatMap ?
                                                         await ShowOaSeatMapAvailabilityDisclaimerText() :
                                                         await ShowNoFreeSeatsAvailableMessage(persistedReservation, countNoOfFreeSeats, countNoOfPricedSeats, isInCheckInWindow, isQueryingFirstSegment);
                seatMap.Add(objMOBSeatMap);
                #endregion
            }
            else
            {
                string errorMessage = isSeatMapSupportedOa
                                        ? _configuration.GetValue<string>("SeatMapUnavailableOtherAirlines")
                                        : "Unable to get the seat map for the flight you requested.";
                throw new MOBUnitedException(errorMessage);
            }

            return seatMap;
        }

        private async Task<List<string>> GetPolarisCabinBranding(string authenticationToken, string flightNumber, string departureAirportCode, string flightDate, string arrivalAirportCode, string cabinCount, string languageCode, string sessionId, string operatingCarrier = "UA", string marketingCarrier = "UA")
        {
            List<string> Cabins = null;
            CabinRequest cabinRequest = new CabinRequest();
            CabinResponse cabinResponse = new CabinResponse();

            //Buiding the cabinRequest
            //cabinRequest.CabinCount = cabinCount;
            cabinRequest.CabinCount = cabinCount;
            cabinRequest.DestinationAirportCode = arrivalAirportCode;
            cabinRequest.FlightDate = flightDate;//DateTime.Parse(request.FlightDate).ToString("yyyy-mm-dd");
            cabinRequest.FlightNumber = flightNumber;
            cabinRequest.LanguageCode = languageCode;
            cabinRequest.MarketingCarrier = marketingCarrier;
            cabinRequest.OperatingCarrier = operatingCarrier;
            cabinRequest.OriginAirportCode = departureAirportCode;
            string jsonRequest = JsonConvert.SerializeObject(cabinRequest);

            string path = "/CabinBranding";
            CabinResponse response = await _referencedataService.GetDataPostHttpAsyncWithOptions<CabinResponse>(path,authenticationToken, sessionId, jsonRequest);

            if (response.Errors != null && response.Errors.Count > 0)
            {
                throw new MOBUnitedException("Errors in the CabinBranding Response");
            }
            if (response != null && response.Cabins != null && response.Cabins.Count > 0)
            {
                Cabins = new List<string>();
                foreach (var cabin in response.Cabins)
                {
                    string aCabin = cabin.Description;
                    Cabins.Add(aCabin);
                }
            }
            else
            {
                throw new MOBUnitedException("Unable to get CabinBranding.");
            }

            //cabinRequest.ServiceClass = request.

            //TODO
            //try
            //{
            //    string jsonRequest = JsonConvert.SerializeObject(cabinRequest);
            //    if (this.levelSwitch.TraceError)
            //    {
            //        LogEntries.Add(United.Logger.LogEntry.GetLogEntry<string>(sessionId, "GetPolarisCabinBranding", "CSLRequest", jsonRequest));
            //    }
            //    string url = ConfigurationManager.AppSettings["CabinBrandingService - URL"];
            //    if (this.levelSwitch.TraceError)
            //    {
            //        LogEntries.Add(United.Logger.LogEntry.GetLogEntry<string>(sessionId, "GetPolarisCabinBranding", "CSL URL", url));
            //    }

            //    #region//****Get Call Duration Code - *******
            //    Stopwatch cslStopWatch;
            //    cslStopWatch = new Stopwatch();
            //    cslStopWatch.Reset();
            //    cslStopWatch.Start();
            //    #endregion//****Get Call Duration Code - *******
            //    string jsonResponse = HttpHelper.Post(url, "application/json; charset=utf-8", authenticationToken, jsonRequest);
            //    #region// 2 = cslStopWatch//****Get Call Duration Code - Venkat 03/17/2015*******
            //    if (cslStopWatch.IsRunning)
            //    {
            //        cslStopWatch.Stop();
            //    }
            //    string cslCallTime = (cslStopWatch.ElapsedMilliseconds / (double)1000).ToString();
            //    if (this.levelSwitch.TraceError)
            //    {
            //        LogEntries.Add(United.Logger.LogEntry.GetLogEntry<string>(sessionId, "Polaris Cabin Branding Service - CSL Call Duration", "CSS/CSL-CallDuration", "CSLSeatMapDetail=" + cslCallTime));
            //    }
            //    #endregion//****Get Call Duration Code - Venkat 03/17/2015*******   
            //    //if (this.levelSwitch.TraceError)
            //    //{
            //    //    LogEntries.Add(United.Logger.LogEntry.GetLogEntry<string>(sessionId, "GetPolarisCabinBranding", "CSLResponse", jsonResponse));
            //    //}


            //    if (!string.IsNullOrEmpty(jsonResponse))
            //    {
            //        CabinResponse response = JsonSerializer.NewtonSoftDeserialize<CabinResponse>(jsonResponse);
            //        if (this.levelSwitch.TraceError)
            //        {
            //            LogEntries.Add(United.Logger.LogEntry.GetLogEntry<CabinResponse>(sessionId, "GetPolarisCabinBranding", "DeserializedResponse", response));
            //        }
            //        if (response.Errors != null && response.Errors.Count > 0)
            //        {
            //            throw new MOBUnitedException("Errors in the CabinBranding Response");
            //        }
            //        if (response != null && response.Cabins != null && response.Cabins.Count > 0)
            //        {
            //            Cabins = new List<string>();
            //            foreach (var cabin in response.Cabins)
            //            {
            //                string aCabin = cabin.Description;
            //                Cabins.Add(aCabin);
            //            }
            //        }
            //        else
            //        {
            //            throw new MOBUnitedException("United Data Services not available.");
            //        }
            //    }
            //    else
            //    {
            //        throw new MOBUnitedException("United Data Services not available.");
            //    }

            //}
            //// Added as part of SeatMap- Cabin Branding Service Logging
            //catch (WebException exx)
            //{
            //    if (this.levelSwitch.TraceError)
            //    {

            //        var exReader = new StreamReader(exx.Response.GetResponseStream()).ReadToEnd().Trim();

            //        // Added as part of Task - 283491 GetPolarisCabinBranding Exceptions
            //        if (Utility.GetBooleanConfigValue("BugFixToggleForExceptionAnalysis") && !string.IsNullOrEmpty(exReader) &&
            //            (exReader.StartsWith("{") && exReader.EndsWith("}")))
            //        {
            //            var exceptionDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<MOBFlightStatusError>(exReader);
            //            if (exceptionDetails != null && exceptionDetails.Errors != null)
            //            {
            //                foreach (var error in exceptionDetails.Errors)
            //                {
            //                    if (!string.IsNullOrEmpty(error.MinorCode) && error.MinorCode.Trim().Equals("90830"))
            //                    {
            //                        throw new MOBUnitedException(ConfigurationManager.AppSettings["Booking2OGenericExceptionMessage"]);
            //                    }
            //                }

            //            }

            //        }

            //        LogEntry objLog = United.Logger.LogEntry.GetLogEntry<string>(sessionId, "GetPolarisCabinBranding", "Exception", exReader.ToString().Trim());
            //        objLog.Message = Xml.Serialization.XmlSerializer.Deserialize<string>(objLog.Message);
            //        LogEntries.Add(objLog);
            //    }

            //    throw exx;

            //}
            //catch (System.Exception ex)
            //{   // Added as part of SeatMap- Cabin Branding Service Logging
            //    //if (levelSwitch.TraceInfo)
            //    //{
            //    //    ExceptionWrapper exceptionWrapper = new ExceptionWrapper(ex);
            //    //    LogEntries.Add(United.Logger.LogEntry.GetLogEntry<ExceptionWrapper>(sessionId, "GetPolarisCabinBranding", "Exception", exceptionWrapper));
            //    //}

            //    throw ex;
            //}

            return Cabins;
        }

        private bool OaSeatMapExceptionVersion(int applicationId, string appVersion)
        {
            return GeneralHelper.IsApplicationVersionGreater(applicationId, appVersion, "AndroidOaSeatMapExceptionVersion", "iPhoneOaSeatMapExceptionVersion", "", "", true, _configuration);
        }

        private bool IsEPlusSeat(Collection<Characteristic> characteristics)
        {
            bool ok = false;

            string seatTypes = _configuration.GetValue<string>("EPlusSeatSharesSeatTypes");

            foreach (Characteristic characteristic in characteristics)
            {
                if (characteristic.Code.Equals("SharesSeatType") || characteristic.Code.Equals("DisplaySeatType"))
                {
                    if (!string.IsNullOrEmpty(characteristic.Value))
                    {
                        if (seatTypes.IndexOf(characteristic.Value + "|") != -1)
                        {
                            ok = true;
                        }
                    }
                    break;
                }
            }

            return ok;
        }

        private async Task<string> ShowNoFreeSeatsAvailableMessage(Reservation persistedReservation, int noOfFreeSeats, int noOfPricedSeats, bool isInCheckInWindow, bool isFirstSegment)
        {
            if (!_configuration.GetValue<bool>("EnableSSA")) return string.Empty;

            if (persistedReservation.IsNullOrEmpty() || persistedReservation.TravelersCSL.IsNullOrEmpty()
                || !persistedReservation.IsSSA || ((persistedReservation.IsELF || IsIBE(persistedReservation)) && !isFirstSegment))
                return string.Empty;

            if (persistedReservation.IsELF || IsIBE(persistedReservation))
                return await GetDocumentTextFromDataBase("SSA_ELF_PURCHASE_SEATS_MESSAGE");

            int noOfFreeEplusEligible = GetNoOfFreeEplusSeatsEligible(persistedReservation, isInCheckInWindow);

            if (EnoughFreeSeats(persistedReservation.TravelersCSL.Count, noOfFreeEplusEligible, noOfFreeSeats, noOfPricedSeats))
                return string.Empty;

            return await GetDocumentTextFromDataBase("SSA_NO_FREE_SEATS_MESSAGE");
        }

        private bool EnoughFreeSeats(int travelerCount, int noOfFreeEplusEligible, int countNoOfFreeSeats, int noOfPricedSeats)
        {
            if (countNoOfFreeSeats >= travelerCount)
                return true;

            var noOfTravelersAfterPickingFreeSeats = travelerCount - countNoOfFreeSeats;
            if ((noOfPricedSeats >= noOfTravelersAfterPickingFreeSeats) && (noOfFreeEplusEligible >= noOfTravelersAfterPickingFreeSeats))
                return true;

            return false;
        }

        private int GetNoOfFreeEplusSeatsEligible(Reservation persistedReservation, bool isInCheckInWindow)
        {
            if (persistedReservation.IsNullOrEmpty())
                return 0;

            if (persistedReservation.AboveGoldMembers > 0)
                return persistedReservation.AboveGoldMembers + 8;

            if (isInCheckInWindow)
                return persistedReservation.NoOfFreeEplusWithSubscriptions
                       + (persistedReservation.GoldMembers * 2)
                       + (persistedReservation.SilverMembers * 2);

            return persistedReservation.NoOfFreeEplusWithSubscriptions + persistedReservation.GoldMembers * 2;
        }

        private bool CheckOaSeatRRowType(Collection<United.Service.Presentation.CommonModel.Characteristic> seatCharacteristics, string code)
        {
            bool seatRRowType = false;
            if (seatCharacteristics != null && seatCharacteristics.Count > 0)
            {
                foreach (United.Service.Presentation.CommonModel.Characteristic objChar in seatCharacteristics)
                {
                    if (objChar.Code != null && objChar.Code.ToUpper().Trim() == code.ToUpper().Trim())
                    {
                        Boolean.TryParse(objChar.Value, out seatRRowType);
                        break;
                    }
                }
            }
            return seatRRowType;
        }

        private string GetOaSeatType(Collection<Characteristic> seatCharacteristics, bool disableSeats, bool isOaPremiumEconomyCabin, out bool isEplus)
        {
            isEplus = false;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (CheckOaSeatRRowType(seatCharacteristics, "IsLavatory") || CheckOaSeatRRowType(seatCharacteristics, "IsCloset") || CheckOaSeatRRowType(seatCharacteristics, "IsGalley") || CheckOaSeatRRowType(seatCharacteristics, "IsStairs") || CheckOaSeatRRowType(seatCharacteristics, "IsSpace") || CheckOaSeatRRowType(seatCharacteristics, "IsInoperativeSeat"))
            {
                sb.Append("-");
            }
            else if (CheckOaSeatRRowType(seatCharacteristics, "IsUnusableSeat") || CheckOaSeatRRowType(seatCharacteristics, "IsPermBlockedSeat"))
            {
                sb.Append("X");
            }
            else if (CheckOaSeatRRowType(seatCharacteristics, "IsAvailableSeat"))
            {
                sb.Append(isOaPremiumEconomyCabin ? "P" : "O");
            }
            else
            {
                sb.Append("X");
            }

            string aType = sb.ToString();
            if (aType != "-" && disableSeats)
            {
                sb = new System.Text.StringBuilder();
                sb.Append("X");
            }

            if (sb.ToString() != "-")
            {
                isEplus = isOaPremiumEconomyCabin;
            }
            return sb.ToString();
        }

        private async Task<string> ShowOaSeatMapAvailabilityDisclaimerText()
        {
            return await GetDocumentTextFromDataBase("OA_SEATMAP_DISCLAIMER_TEXT");
        }

        private async Task<string> GetDocumentTextFromDataBase(string title)
        {
            var messagesFromDb = await GetMPPINPWDTitleMessages(title);
            return messagesFromDb != null && messagesFromDb.Any() ? messagesFromDb[0].CurrentValue : string.Empty;
        }

        private bool CheckSeatRRowType(Collection<United.Service.Presentation.CommonModel.Characteristic> seatCharacteristics, string code)
        {
            bool seatRRowType = false;
            if (seatCharacteristics != null && seatCharacteristics.Count > 0)
            {
                foreach (United.Service.Presentation.CommonModel.Characteristic objChar in seatCharacteristics)
                {
                    if (objChar.Code != null && objChar.Code.ToUpper().Trim() == code.ToUpper().Trim())
                    {
                        seatRRowType = true;
                        break;
                    }
                }
            }
            return seatRRowType;
        }

        private async Task<List<MOBItem>> GetMPPINPWDTitleMessages(string titleList)
        {
            List<MOBItem> messages = new List<MOBItem>();
            List<United.Definition.MOBLegalDocument> docs = await _legalDocumentsForTitlesService.GetNewLegalDocumentsForTitles(titleList, _headers.ContextValues.TransactionId, true);
            if (docs != null && docs.Count > 0)
            {
                foreach (United.Definition.MOBLegalDocument doc in docs)
                {
                    MOBItem item = new MOBItem();
                    item.Id = doc.Title;
                    item.CurrentValue = doc.Document;
                    messages.Add(item);
                }
            }
            return messages;
        }

        private bool IsInCheckInWindow(FlightSeatMapDetail seatMapResponse)
        {
            try
            {
                return Convert.ToBoolean(seatMapResponse.IsInCheckInWindow);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<string> GetPolarisSeatMapLegendIdPolarisOff(string from, string to, int numberOfCabins)
        {
            #region
            string seatMapLegendId = string.Empty;

            List<CabinBrand> lstCabinBrand = new List<CabinBrand>();
            try
            {
                lstCabinBrand = await _complimentaryUpgradeService.GetComplimentaryUpgradeOfferedFlagByCabinCount(from, to, numberOfCabins, _headers.ContextValues.SessionId, _headers.ContextValues.TransactionId);
            }
            catch (Exception ex)
            {
                //_logger.LogError("OnPremSQLService-GetComplimentaryUpgradeOfferedFlagByCabinCount Error {message} {exceptionStackTrace} and {sessionId}", ex.Message, ex.StackTrace, Headers.ContextValues.SessionId);
            }

            //Database database = DatabaseFactory.CreateDatabase("ConnectionString - DB_Flightrequest");
            //DbCommand dbCommand = (DbCommand)database.GetStoredProcCommand("usp_GetComplimentary_Upgrade_Offered_flag_By_Cabin_Count");
            //database.AddInParameter(dbCommand, "@Origin", DbType.String, from);
            //database.AddInParameter(dbCommand, "@destination", DbType.String, to);
            //database.AddInParameter(dbCommand, "@numberOfCabins", DbType.Int32, numberOfCabins);

            //POLARIS Cabin Branding SeatMapLegend Booking Path
            string seatMapLegendEntry1 = (_configuration.GetValue<string>("seatMapLegendEntry10").ToString() != null) ? _configuration.GetValue<string>("seatMapLegendEntry10").ToString() : "";
            string seatMapLegendEntry2 = (_configuration.GetValue<string>("seatMapLegendEntry11").ToString() != null) ? _configuration.GetValue<string>("seatMapLegendEntry11").ToString() : "";
            string seatMapLegendEntry3 = (_configuration.GetValue<string>("seatMapLegendEntry12").ToString() != null) ? _configuration.GetValue<string>("seatMapLegendEntry12").ToString() : "";
            string seatMapLegendEntry4 = (_configuration.GetValue<string>("seatMapLegendEntry13").ToString() != null) ? _configuration.GetValue<string>("seatMapLegendEntry13").ToString() : "";

            string seatMapLegendEntry5 = (_configuration.GetValue<string>("seatMapLegendEntry5").ToString() != null) ? _configuration.GetValue<string>("seatMapLegendEntry5").ToString() : "";
            string seatMapLegendEntry6 = (_configuration.GetValue<string>("seatMapLegendEntry6").ToString() != null) ? _configuration.GetValue<string>("seatMapLegendEntry6").ToString() : "";
            string seatMapLegendEntry7 = (_configuration.GetValue<string>("seatMapLegendEntry7").ToString() != null) ? _configuration.GetValue<string>("seatMapLegendEntry7").ToString() : "";
            string seatMapLegendEntry8 = (_configuration.GetValue<string>("seatMapLegendEntry8").ToString() != null) ? _configuration.GetValue<string>("seatMapLegendEntry8").ToString() : "";
            string seatMapLegendEntry9 = (_configuration.GetValue<string>("seatMapLegendEntry9").ToString() != null) ? _configuration.GetValue<string>("seatMapLegendEntry9").ToString() : "";

            try
            {
                if (lstCabinBrand.Count > 0)
                {
                    foreach (var cb in lstCabinBrand)
                    {
                        #region
                        int secondCabinBrandingId = cb.SecondCabinBrandingId.Equals(System.DBNull.Value) ? 0 : Convert.ToInt32(cb.SecondCabinBrandingId);
                        int thirdCabinBrandingId = cb.ThirdCabinBrandingId.Equals(System.DBNull.Value) ? 0 : Convert.ToInt32(cb.ThirdCabinBrandingId);

                        if (thirdCabinBrandingId == 0)
                        {
                            if (secondCabinBrandingId == 1)
                            {
                                seatMapLegendId = "seatmap_legend5" + seatMapLegendEntry3 + seatMapLegendEntry6 + seatMapLegendEntry7 + seatMapLegendEntry8 + seatMapLegendEntry9;
                            }
                            else if (secondCabinBrandingId == 2)
                            {
                                seatMapLegendId = "seatmap_legend4" + seatMapLegendEntry4 + seatMapLegendEntry6 + seatMapLegendEntry7 + seatMapLegendEntry8 + seatMapLegendEntry9;
                            }
                            else if (secondCabinBrandingId == 3)
                            {
                                seatMapLegendId = "seatmap_legend3" + seatMapLegendEntry2 + seatMapLegendEntry6 + seatMapLegendEntry7 + seatMapLegendEntry8 + seatMapLegendEntry9;
                            }
                        }
                        else if (thirdCabinBrandingId == 1)
                        {
                            seatMapLegendId = "seatmap_legend2" + seatMapLegendEntry3 + seatMapLegendEntry4 + seatMapLegendEntry6 + seatMapLegendEntry7 + seatMapLegendEntry8 + seatMapLegendEntry9;
                        }
                        else if (thirdCabinBrandingId == 4)
                        {
                            seatMapLegendId = "seatmap_legend1" + seatMapLegendEntry1 + seatMapLegendEntry2 + seatMapLegendEntry6 + seatMapLegendEntry7 + seatMapLegendEntry8 + seatMapLegendEntry9;
                        }
                        #endregion
                    }
                }
            }
            catch (System.Exception ex)
            {
                //_logger.LogError("GetPolarisSeatMapLegendIdPolarisOff Error {message} {exceptionStackTrace} and {sessionId}", ex.Message, ex.StackTrace, Headers.ContextValues.SessionId);
            }
            if (string.IsNullOrEmpty(seatMapLegendId))
            {
                seatMapLegendId = "seatmap_legend5" + seatMapLegendEntry3 + seatMapLegendEntry6 + seatMapLegendEntry7 + seatMapLegendEntry8 + seatMapLegendEntry9;
            }

            return seatMapLegendId;

            #endregion
        }

    }
}
