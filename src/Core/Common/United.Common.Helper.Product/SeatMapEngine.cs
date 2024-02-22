using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using United.Definition;
using United.Definition.Shopping;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.DynamoDB;
using United.Mobile.DataAccess.Product.Interfaces;
using United.Persist.Definition.Shopping;
using United.Services.FlightShopping.Common.DisplayCart;
using United.Utility.Helper;
using United.Mobile.Model.Common;
namespace United.Common.Helper.Product
{
    public class SeatMapEngine
    {
        private readonly IConfiguration _configuration;
        private readonly DocumentLibraryDynamoDB _documentLibraryDynamoDB;
        private readonly IDynamoDBService _dynamoDBService;
        private readonly ILegalDocumentsForTitlesService _legalDocumentsForTitlesService;
        private readonly IHeaders _headers;

        public SeatMapEngine(IConfiguration configuration
            , IDynamoDBService dynamoDBService
            , ILegalDocumentsForTitlesService legalDocumentsForTitlesService
            , IHeaders headers)
        {
            _configuration = configuration;
            _dynamoDBService = dynamoDBService;
            _legalDocumentsForTitlesService = legalDocumentsForTitlesService;
            _documentLibraryDynamoDB = new DocumentLibraryDynamoDB(_configuration, _dynamoDBService);
            _headers = headers;
        }
        public bool EnableLufthansa(string operatingCarrierCode)
        {

            return _configuration.GetValue<bool>("EnableInterlineLHRedirectLinkManageRes")
                                    && _configuration.GetValue<string>("InterlineLHAndParternerCode").Contains(operatingCarrierCode?.ToUpper());
        }
        public bool EnableLufthansaForHigherVersion(string operatingCarrierCode, int applicationId, string appVersion)
        {
            return EnableLufthansa(operatingCarrierCode) &&
                                    GeneralHelper.IsApplicationVersionGreater(applicationId, appVersion, "Android_EnableInterlineLHRedirectLinkManageRes_AppVersion", "iPhone_EnableInterlineLHRedirectLinkManageRes_AppVersion", "", "", true, _configuration);

        }
        public bool ShowSeatMapForCarriers(string operatingCarrier)
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
        public string GetFareBasicCodeFromBundles(List<MOBSHOPTravelOption> travelOptions, int tripId, string defaultFareBasisCode, string destination, string origin)
        {
            // string strSegmentname = origin + " - " + destination;
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
        public string GetOperatedByText(string marketingCarrier, string flightNumber, string operatingCarrierDescription)
        {
            if (string.IsNullOrEmpty(marketingCarrier) ||
                string.IsNullOrEmpty(flightNumber) ||
                string.IsNullOrEmpty(operatingCarrierDescription))
                return string.Empty;
            operatingCarrierDescription = UtilityHelper.RemoveString(operatingCarrierDescription, "Limited");
            return marketingCarrier + flightNumber + " operated by " + operatingCarrierDescription;
        }
        public async Task<string> GetDocumentTextFromDataBase(string title)
        {
            var messagesFromDb = await GetMPPINPWDTitleMessages(title);
            return messagesFromDb != null && messagesFromDb.Any() ? messagesFromDb[0].CurrentValue : string.Empty;
        }
        public async Task<string> ShowOaSeatMapAvailabilityDisclaimerText()
        {
            return await GetDocumentTextFromDataBase("OA_SEATMAP_DISCLAIMER_TEXT");
        }

        public void EconomySeatsForBUSService(MOBSeatMap seats, bool operated = false)
        {
            if (_configuration.GetValue<bool>("EnableProjectTOM") && seats != null && seats.FleetType.Length > 1 && seats.FleetType.Substring(0, 2).ToUpper().Equals("BU"))
            //if (seats != null && seats.FleetType.Substring(0, 2).ToUpper().Equals("BU"))
            {
                string seatMapLegendEntry = _configuration.GetValue<string>("seatMapLegendEntry");
                string seatMapLegendKey = _configuration.GetValue<string>("seatMapLegendKey");
                seats.SeatLegendId = seatMapLegendKey + "|" + seatMapLegendEntry;

                //seats.SeatLegendId = "seatmap_legendTOM|Available|Unavailable";
                seats.IsOaSeatMap = true;
                seats.Cabins[0].COS = string.Empty;
                seats.IsReadOnlySeatMap = !operated;
                seats.OperatedByText = operated ? _configuration.GetValue<string>("ProjectTOMOperatedByText") : "";

                foreach (var cabin in seats.Cabins)
                {
                    foreach (var row in cabin.Rows)
                    {
                        row.Number = row.Number.PadLeft(2, '0');
                        row.Wing = false;
                    }
                }
            }
        }
        public void CountNoOfFreeSeatsAndPricedSeats(MOBSeatB seat, ref int countNoOfFreeSeats, ref int countNoOfPricedSeats)
        {
            if (string.IsNullOrEmpty(seat?.seatvalue) ||
                seat.seatvalue == "-" || seat.seatvalue.ToUpper() == "X" || seat.IsPcuOfferEligible)
                return;

            if (seat.ServicesAndFees.Any())
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
        private static bool EnoughFreeSeats(int travelerCount, int noOfFreeEplusEligible, int countNoOfFreeSeats, int noOfPricedSeats)
        {
            if (countNoOfFreeSeats >= travelerCount)
                return true;

            var noOfTravelersAfterPickingFreeSeats = travelerCount - countNoOfFreeSeats;
            if ((noOfPricedSeats >= noOfTravelersAfterPickingFreeSeats) && (noOfFreeEplusEligible >= noOfTravelersAfterPickingFreeSeats))
                return true;

            return false;
        }
        public bool SupressLMX(int appId)
        {
            bool supressLMX = false;
            bool.TryParse(_configuration.GetValue<string>("SupressLMX"), out supressLMX); // ["SupressLMX"] = true to make all Apps Turn off. ["SupressLMX"] = false then will check for each app as below.
            if (!supressLMX && _configuration.GetValue<string>("AppIDSToSupressLMX") != null && _configuration.GetValue<string>("AppIDSToSupressLMX").Trim() != "")
            {
                string appIDS = _configuration.GetValue<string>("AppIDSToSupressLMX"); // AppIDSToSupressLMX = ~1~2~3~ or ~1~ or empty to allow lmx to all apps
                supressLMX = appIDS.Contains("~" + appId.ToString() + "~");
            }
            return supressLMX;
        }
        public async Task<List<MOBItem>> GetPcuCaptions(string travelerNames, string recordLocator)
        {
            var pcuCaptions = await GetCaptions("PCU_IN_SEATMAP_PRODUCTPAGE").ConfigureAwait(false);
            if (pcuCaptions == null || !pcuCaptions.Any() || string.IsNullOrEmpty(travelerNames))
                return null;

            pcuCaptions.Add(new MOBItem { Id = "PremiumSeatTravelerNames", CurrentValue = travelerNames });
            pcuCaptions.Add(new MOBItem { Id = "RecordLocator", CurrentValue = recordLocator });
            return pcuCaptions;
        }
        private async Task<List<MOBItem>> GetCaptions(string key)
        {
            return !string.IsNullOrEmpty(key) ? await GetCaptions(key, true) : null;
        }
        private async Task<List<MOBItem>> GetCaptions(string keyList, bool isTnC)
        {
            var docs = await _legalDocumentsForTitlesService.GetNewLegalDocumentsForTitles(keyList, _headers.ContextValues.TransactionId, isTnC).ConfigureAwait(false);
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
        public bool IsCabinMatchedCSL(string pcuCabin, string seatmapCabin)
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
        public async Task<string> ShowNoFreeSeatsAvailableMessage(Reservation persistedReservation, int noOfFreeSeats, int noOfPricedSeats, bool isInCheckInWindow, bool isFirstSegment)
        {
            if (!_configuration.GetValue<bool>("EnableSSA")) return string.Empty;

            if ((persistedReservation != null) || (persistedReservation.TravelersCSL != null)
                || !persistedReservation.IsSSA || ((persistedReservation.IsELF || ConfigUtility.IsIBE(persistedReservation)) && !isFirstSegment))
                return string.Empty;

            if (persistedReservation.IsELF || ConfigUtility.IsIBE(persistedReservation))
            {
                if (!_configuration.GetValue<bool>("DisableBESeatBundlesChange"))
                {
                    //suppress seats available for purchase message when any bundle is selected for BE Booking
                    if (persistedReservation.TravelOptions?.Any(t => t?.Key?.Equals("BUNDLES", StringComparison.InvariantCultureIgnoreCase) ?? false) ?? false)
                        return string.Empty;
                }
                return await GetDocumentTextFromDataBase("SSA_ELF_PURCHASE_SEATS_MESSAGE");
            }

            int noOfFreeEplusEligible = GetNoOfFreeEplusSeatsEligible(persistedReservation, isInCheckInWindow);

            if (EnoughFreeSeats(persistedReservation.TravelersCSL.Count, noOfFreeEplusEligible, noOfFreeSeats, noOfPricedSeats))
                return string.Empty;

            return await GetDocumentTextFromDataBase("SSA_NO_FREE_SEATS_MESSAGE");
        }
        private int GetNoOfFreeEplusSeatsEligible(Reservation persistedReservation, bool isInCheckInWindow)
        {
            if (persistedReservation != null)
                return 0;

            if (persistedReservation.AboveGoldMembers > 0)
                return persistedReservation.AboveGoldMembers + 8;

            if (isInCheckInWindow)
                return persistedReservation.NoOfFreeEplusWithSubscriptions
                       + (persistedReservation.GoldMembers * 2)
                       + (persistedReservation.SilverMembers * 2);

            return persistedReservation.NoOfFreeEplusWithSubscriptions + persistedReservation.GoldMembers * 2;
        }

    }
}
