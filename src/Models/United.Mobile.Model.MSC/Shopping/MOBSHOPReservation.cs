using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using United.Definition.MP2015;
using United.Definition.Shopping.TripInsurance;
using United.Mobile.DataAccess.Common;
using United.Reward.Configuration;
using United.Utility.Helper;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping
{
    [Serializable()]
    public class MOBSHOPReservation
    {
        private IConfiguration _configuration;
        private ICachingService _cachingService;

        public MOBSHOPReservation(IConfiguration configuration
            , ICachingService cachingService)
        {
            _configuration = configuration;
            _cachingService = cachingService;

            rewardPrograms = new List<MOBSHOPRewardProgram>();
            var key = _configuration.GetValue<string>("FrequestFlyerRewardProgramListStaticGUID") + "Booking2.0FrequentFlyerList";
            var frequestFlyerList = _cachingService.GetCache<string>(key, "TID1").Result;
            rewardPrograms = JsonConvert.DeserializeObject<List<MOBSHOPRewardProgram>>(frequestFlyerList);
            if (rewardPrograms == null || rewardPrograms.Count == 0)
            {
                rewardPrograms = new List<MOBSHOPRewardProgram>();
                ConfigurationParameter.ConfigParameter parm = ConfigurationParameter.ConfigParameter.Configuration;
                if (parm != null && parm.RewardTypes != null)
                {
                    for (int i = 0; i < parm.RewardTypes.Count; i++)
                    {
                        MOBSHOPRewardProgram p = new MOBSHOPRewardProgram();
                        p.ProgramID = parm.RewardTypes[i].ProductID;
                        p.Type = parm.RewardTypes[i].Type;
                        p.Description = parm.RewardTypes[i].Description;
                        rewardPrograms.Add(p);
                    }
                }
            }
            tcdAdvisoryMessages = new List<MOBItem>();
            int tCDAdvisoryMessagesCount = Convert.ToInt32(_configuration.GetValue<string>("TCDAdvisoryMessagesCount"));
            for (int i = 1; i <= tCDAdvisoryMessagesCount; i++)
            {
                MOBItem item = new MOBItem();
                item.Id = HttpUtility.HtmlDecode(_configuration.GetValue<string>("TCDAdvisoryMessages" + i.ToString()).ToString().Split('~')[0]);
                item.CurrentValue = HttpUtility.HtmlDecode(_configuration.GetValue<string>("TCDAdvisoryMessages" + i.ToString()).ToString().Split('~')[1].ToString());
                item.SaveToPersist = Convert.ToBoolean(HttpUtility.HtmlDecode(_configuration.GetValue<string>("TCDAdvisoryMessages" + i.ToString()).ToString().Split('~')[2].ToString()));
                tcdAdvisoryMessages.Add(item);
            }
            overMileageLimitMessage = "You can earn up to 75,000 award miles per ticket. The 75,000 award miles cap may be applied to your posted flight activity in an order different than shown.";
            overMileageLimitAmount = "75000";
            elfMessagesForRTI = new List<MOBItem>();
        }
        public MOBSHOPReservation()
        {
        }

        public async System.Threading.Tasks.Task Initialise(IConfiguration configuration
            , ICachingService cachingService)
        {
            _configuration = configuration;
            _cachingService = cachingService;
            rewardPrograms = new List<MOBSHOPRewardProgram>();
            var key = _configuration.GetValue<string>("FrequestFlyerRewardProgramListStaticGUID") + "Booking2.0FrequentFlyerList";
            var frequestFlyerList = await _cachingService.GetCache<string>(key, "TID1").ConfigureAwait(false);
            rewardPrograms = JsonConvert.DeserializeObject<List<MOBSHOPRewardProgram>>(frequestFlyerList);
            if (rewardPrograms == null || rewardPrograms.Count == 0)
            {
                rewardPrograms = new List<MOBSHOPRewardProgram>();
                ConfigurationParameter.ConfigParameter parm = ConfigurationParameter.ConfigParameter.Configuration;
                if (parm != null && parm.RewardTypes != null)
                {
                    for (int i = 0; i < parm.RewardTypes.Count; i++)
                    {
                        MOBSHOPRewardProgram p = new MOBSHOPRewardProgram();
                        p.ProgramID = parm.RewardTypes[i].ProductID;
                        p.Type = parm.RewardTypes[i].Type;
                        p.Description = parm.RewardTypes[i].Description;
                        rewardPrograms.Add(p);
                    }
                }
            }
            tcdAdvisoryMessages = new List<MOBItem>();
            int tCDAdvisoryMessagesCount = Convert.ToInt32(_configuration.GetValue<string>("TCDAdvisoryMessagesCount"));
            for (int i = 1; i <= tCDAdvisoryMessagesCount; i++)
            {
                MOBItem item = new MOBItem();
                item.Id = HttpUtility.HtmlDecode(_configuration.GetValue<string>("TCDAdvisoryMessages" + i.ToString()).ToString().Split('~')[0]);
                item.CurrentValue = HttpUtility.HtmlDecode(_configuration.GetValue<string>("TCDAdvisoryMessages" + i.ToString()).ToString().Split('~')[1].ToString());
                item.SaveToPersist = Convert.ToBoolean(HttpUtility.HtmlDecode(_configuration.GetValue<string>("TCDAdvisoryMessages" + i.ToString()).ToString().Split('~')[2].ToString()));
                tcdAdvisoryMessages.Add(item);
            }
            overMileageLimitMessage = "You can earn up to 75,000 award miles per ticket. The 75,000 award miles cap may be applied to your posted flight activity in an order different than shown.";
            overMileageLimitAmount = "75000";
        }

        private string sessionId = string.Empty;
        private string metaSessionId;
        private string recordLocator = string.Empty;
        private string searchType = string.Empty;
        private string cartId = string.Empty;
        private bool isSignedInWithMP;
        private List<MOBSHOPTrip> trips;
        private List<MOBSHOPPrice> prices;
        private List<MOBSHOPTax> taxes;
        private int numberOfTravelers;
        private List<MOBSHOPTraveler> travelers;
        private List<MOBCPTraveler> travelersCSL;
        private List<MOBSeatPrice> seatPrices = new List<MOBSeatPrice>();
        private List<MOBCreditCard> creditCards;
        private MOBCPPhone reservationPhone;
        private MOBEmail reservationEmail;
        private string warning;
        private List<MOBSHOPTravelOption> travelOptions;
        private MOBSHOPClubPassPurchaseRequest clubPassPurchaseRequest;
        private string seatMessage = string.Empty;
        private string travelOptionMessage = string.Empty;
        private bool travelersRegistered;
        private List<MOBSHOPRewardProgram> rewardPrograms;
        private List<MOBAddress> creditCardsAddress = new List<MOBAddress>();
        private List<string> messages;
        private List<MOBSHOPFareRules> fareRules;
        private bool unregisterFareLock = true;
        private List<MOBItem> tcdAdvisoryMessages;
        private string flightShareMessage = string.Empty;
        private string ineligibleToEarnCreditMessage = string.Empty;
        private string oaIneligibleToEarnCreditMessage = string.Empty;

        private List<MOBLmxFlight> lmxFlights;

        private string overMileageLimitMessage = "You can earn up to 75,000 award miles per ticket. The 75,000 award miles cap may be applied to your posted flight activity in an order different than shown.";
        private string overMileageLimitAmount = "75000";




        private bool isEmp20;
        private bool isELF;
        private bool isMetaSearch;
        private bool isUpgradedFromEntryLevelFare;

        private bool isCubaTravel;
        private MOBFormofPayment formOfPaymentType;
        private MOBPayPal payPal;

        private bool isReshopChange;
        private string checkedbagChargebutton;
        private bool isBookingCommonFOPEnabled;
        private bool isReshopCommonFOPEnabled;
        private bool isPostBookingCommonFOPEnabled;
        private bool isCanadaTravel;
        private bool isEligibleForMoneyPlusMiles;
        private bool isMoneyPlusMilesSelected;
        public async System.Threading.Tasks.Task UpdateRewards(IConfiguration configuration
            , ICachingService cachingService)
        {
            _configuration = configuration;
            _cachingService = cachingService;

            rewardPrograms = new List<MOBSHOPRewardProgram>();
            var key = _configuration.GetValue<string>("FrequestFlyerRewardProgramListStaticGUID") + "Booking2.0FrequentFlyerList";
            var frequestFlyerList = await _cachingService.GetCache<string>(key, "TID1").ConfigureAwait(false);
            rewardPrograms = JsonConvert.DeserializeObject<List<MOBSHOPRewardProgram>>(frequestFlyerList);
            if (rewardPrograms == null || rewardPrograms.Count == 0)
            {
                rewardPrograms = new List<MOBSHOPRewardProgram>();
                ConfigurationParameter.ConfigParameter parm = ConfigurationParameter.ConfigParameter.Configuration;
                if (parm != null && parm.RewardTypes != null)
                {
                    for (int i = 0; i < parm.RewardTypes.Count; i++)
                    {
                        MOBSHOPRewardProgram p = new MOBSHOPRewardProgram();
                        p.ProgramID = parm.RewardTypes[i].ProductID;
                        p.Type = parm.RewardTypes[i].Type;
                        p.Description = parm.RewardTypes[i].Description;
                        rewardPrograms.Add(p);
                    }
                }
            }
            tcdAdvisoryMessages = new List<MOBItem>();
            int tCDAdvisoryMessagesCount = Convert.ToInt32(_configuration.GetValue<string>("TCDAdvisoryMessagesCount"));
            for (int i = 1; i <= tCDAdvisoryMessagesCount; i++)
            {
                MOBItem item = new MOBItem();
                item.Id = HttpUtility.HtmlDecode(_configuration.GetValue<string>("TCDAdvisoryMessages" + i.ToString()).ToString().Split('~')[0]);
                item.CurrentValue = HttpUtility.HtmlDecode(_configuration.GetValue<string>("TCDAdvisoryMessages" + i.ToString()).ToString().Split('~')[1].ToString());
                item.SaveToPersist = Convert.ToBoolean(_configuration.GetValue<string>("TCDAdvisoryMessages" + i.ToString()).ToString().Split('~')[2].ToString());
                tcdAdvisoryMessages.Add(item);
            }
        }

        [System.Text.Json.Serialization.JsonIgnore]
        [JsonIgnore]
        public MOBEmail ReservationEmail2
        {
            get
            {
                return this.reservationEmail;
            }
            set
            {
                this.reservationEmail = value;
            }
        }

        [System.Text.Json.Serialization.JsonIgnore]
        [JsonIgnore]
        public List<MOBCreditCard> CreditCards2
        {
            get
            {
                return creditCards;
            }
            set
            {
                creditCards = value;
            }
        }

        [System.Text.Json.Serialization.JsonIgnore]
        [JsonIgnore]
        public MOBCPPhone ReservationPhone2
        {
            get
            {
                return this.reservationPhone;
            }
            set
            {
                this.reservationPhone = value;

            }
        }
        public bool IsReshopChange
        {
            get { return isReshopChange; }
            set { isReshopChange = value; }
        }

        private List<MOBSHOPReshopTrip> reshopTrips;
        public List<MOBSHOPReshopTrip> ReshopTrips
        {
            get { return reshopTrips; }
            set { reshopTrips = value == null ? null : value; }
        }

        private MOBSHOPReshop reshop;
        public MOBSHOPReshop Reshop
        {
            get { return reshop; }
            set { reshop = value; }
        }

        public MOBPayPal PayPal
        {
            get { return payPal; }
            set { payPal = value; }
        }

        private MOBPayPalPayor payPalPayor;
        public MOBPayPalPayor PayPalPayor
        {
            get { return payPalPayor; }
            set { payPalPayor = value; }
        }


        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public MOBFormofPayment FormOfPaymentType
        {
            get { return formOfPaymentType; }
            set { formOfPaymentType = value; }
        }

        public bool IsCubaTravel
        {
            get { return isCubaTravel; }
            set { isCubaTravel = value; }
        }

        private MOBCPCubaTravel cubaTravelInfo;

        public MOBCPCubaTravel CubaTravelInfo
        {
            get { return cubaTravelInfo; }
            set { cubaTravelInfo = value; }
        }

        public string OverMileageLimitMessage
        {
            get
            {
                return this.overMileageLimitMessage;
            }
            set
            {
                this.overMileageLimitMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string OverMileageLimitAmount
        {
            get
            {
                return this.overMileageLimitAmount;
            }
            set
            {
                this.overMileageLimitAmount = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }



        private bool awardTravel = false;
        public bool AwardTravel
        {
            get
            {
                return this.awardTravel;
            }
            set
            {
                this.awardTravel = value;
            }
        }

        private string pointOfSale = string.Empty;
        private string pkDispenserPublicKey = string.Empty;

        private bool isRefundable;
        public bool IsRefundable
        {
            get
            {
                return this.isRefundable;
            }
            set
            {
                this.isRefundable = value;
            }
        }

        private bool isInternational;
        public bool ISInternational
        {
            get
            {
                return this.isInternational;
            }
            set
            {
                this.isInternational = value;
            }
        }

        private bool isFlexibleSegmentExist;
        public bool ISFlexibleSegmentExist
        {
            get
            {
                return this.isFlexibleSegmentExist;
            }
            set
            {
                this.isFlexibleSegmentExist = value;
            }
        }

        public string PointOfSale
        {
            get { return this.pointOfSale; }
            set { this.pointOfSale = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string SeatMessage
        {
            get
            {
                return this.seatMessage;
            }
            set
            {
                this.seatMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string TravelOptionMessage
        {
            get
            {
                return this.travelOptionMessage;
            }
            set
            {
                this.travelOptionMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MOBSHOPTravelOption> TravelOptions
        {
            get
            {
                return this.travelOptions;
            }
            set
            {
                this.travelOptions = value;
            }
        }

        public MOBSHOPClubPassPurchaseRequest ClubPassPurchaseRequest
        {
            get
            {
                return this.clubPassPurchaseRequest;
            }
            set
            {
                this.clubPassPurchaseRequest = value;
            }
        }

        public string SessionId
        {
            get
            {
                return this.sessionId;
            }
            set
            {
                this.sessionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string RecordLocator
        {
            get
            {
                return this.recordLocator;
            }
            set
            {
                this.recordLocator = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string SearchType
        {
            get
            {
                return this.searchType;
            }
            set
            {
                this.searchType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string CartId
        {
            get
            {
                return this.cartId;
            }
            set
            {
                this.cartId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }        
        public bool IsSignedInWithMP
        {
            get { return this.isSignedInWithMP; }
            set { this.isSignedInWithMP = value; }
        }

        public List<MOBSHOPTrip> Trips
        {
            get
            {
                return this.trips;
            }
            set
            {
                this.trips = value;
            }
        }

        public List<MOBSHOPPrice> Prices
        {
            get
            {
                return this.prices;
            }
            set
            {
                this.prices = value;
                UpdateWithUpliftPrice();
            }
        }



        private void UpdateWithUpliftPrice()
        {
            if (_configuration == null)
                return;

            if (!_configuration.GetValue<bool>("EnableUpliftPayment"))
                return;

            if (Prices == null || !Prices.Any())
                return;

            var upliftTotalPrice = Prices.FirstOrDefault(p => "TOTALPRICEFORUPLIFT".Equals(p.DisplayType, StringComparison.CurrentCultureIgnoreCase));
            if (upliftTotalPrice != null)
            {
                Prices.Remove(upliftTotalPrice);
            }

            Prices.Add(GetTotalPriceForUplift());
        }

        private MOBSHOPPrice GetTotalPriceForUplift()
        {
            var totalPrice = Prices.FirstOrDefault(p => "GRAND TOTAL".Equals(p.DisplayType, StringComparison.CurrentCultureIgnoreCase))?.Value ?? 0;
            if (totalPrice == 0)
            {
                totalPrice = Prices.FirstOrDefault(p => "TOTAL".Equals(p.DisplayType, StringComparison.CurrentCultureIgnoreCase))?.Value ?? 0;
            }
            var tripInsurancePrice = Prices.FirstOrDefault(p => "TRAVEL INSURANCE".Equals(p.DisplayType, StringComparison.CurrentCultureIgnoreCase))?.Value ?? 0;

            var totalPriceForUplift = totalPrice - tripInsurancePrice;
            CultureInfo ci = null;
            ci = TopHelper.GetCultureInfo(totalPriceForUplift.ToString());
            return new MOBSHOPPrice
            {
                CurrencyCode = "USD",
                DisplayType = "TOTALPRICEFORUPLIFT",
                DisplayValue = string.Format("{0:#,0.00}", totalPriceForUplift),
                Value = totalPriceForUplift,
                FormattedDisplayValue = totalPriceForUplift.ToString("C2", new CultureInfo("en-US"))
            };
        }
        public List<MOBSHOPTax> Taxes
        {
            get
            {
                return this.taxes;
            }
            set
            {
                this.taxes = value;
            }
        }

        public int NumberOfTravelers
        {
            get
            {
                return this.numberOfTravelers;
            }
            set
            {
                this.numberOfTravelers = value;
            }
        }

        public List<MOBSHOPTraveler> Travelers
        {
            get
            {
                return this.travelers;
            }
            set
            {
                this.travelers = value;
            }
        }

        public List<MOBCPTraveler> TravelersCSL
        {
            get
            {
                return this.travelersCSL;
            }
            set
            {
                this.travelersCSL = value;
            }
        }

        public List<MOBSeatPrice> SeatPrices
        {
            get
            {
                return this.seatPrices;
            }
            set
            {
                this.seatPrices = value;
            }
        }
        [JsonProperty("creditCards")]
        public List<MOBCreditCard> CreditCards
        {
            get
            {
                return creditCards;
            }
            set
            {
                if (value != null)
                {
                    creditCards = value;
                }
            }
        }


        public MOBCPPhone ReservationPhone
        {
            get
            {
                return this.reservationPhone;
            }
            set
            {
                if (value != null)
                {
                    this.reservationPhone = value;
                }
            }
        }



        public MOBEmail ReservationEmail
        {
            get
            {
                return this.reservationEmail;
            }
            set
            {
                if (value != null)
                {
                    this.reservationEmail = value;
                }
            }
        }



        public string Warning
        {
            get { return warning; }
            set { warning = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public bool TravelersRegistered
        {
            get { return this.travelersRegistered; }
            set { this.travelersRegistered = value; }
        }

        //62681-Added the tag to avoid duplicate values in Reward programs-11/21/2016-Alekhya

        public List<MOBSHOPRewardProgram> RewardPrograms
        {
            get
            {
                return this.rewardPrograms;
            }
            set
            {
                this.rewardPrograms = value;
            }
        }

        public List<MOBAddress> CreditCardsAddress
        {
            get
            {
                return this.creditCardsAddress;
            }
            set
            {
                this.creditCardsAddress = value;
            }
        }
        public List<string> Messages
        {
            get
            {
                return this.messages;
            }
            set
            {
                this.messages = value;
            }
        }

        public List<MOBSHOPFareRules> FareRules
        {
            get
            {
                return this.fareRules;
            }
            set
            {
                this.fareRules = value;
            }
        }

        public bool UnregisterFareLock
        {
            get { return this.unregisterFareLock; }
            set { this.unregisterFareLock = value; }
        }

        private MOBSHOPFareLock fareLock;
        public MOBSHOPFareLock FareLock
        {
            get
            {
                return this.fareLock;
            }
            set
            {
                this.fareLock = value;
            }
        }

        public List<MOBItem> TCDAdvisoryMessages
        {
            get
            {
                if (_configuration != null)
                {
                    if (Convert.ToBoolean(_configuration.GetValue<string>("FixTCDAdvisoryMessageForIBE") ?? "false"))
                    {
                        if (this.IsELF || this.shopReservationInfo2 != null && (this.shopReservationInfo2.IsIBELite || this.ShopReservationInfo2.IsIBE || this.shopReservationInfo2.IsNonRefundableNonChangable))
                        {
                            MOBItem mobItem = this.tcdAdvisoryMessages.Find(p => p.Id == "PurchaseTnC");
                            if (mobItem != null && this.shopReservationInfo2.IsNonRefundableNonChangable)
                            {
                                string purchaseTnC = _configuration.GetValue<string>("TCDAdvisoryMessagesForNonRefundableNonChangable");
                                if (purchaseTnC != null)
                                {
                                    mobItem.CurrentValue = purchaseTnC;
                                }
                            }
                            else if (mobItem != null && _configuration.GetValue<string>("TCDAdvisoryMessagesForELF") != null)
                            {
                                string purchaseTnC = GetTCDAdMesgForBE();
                                if (purchaseTnC != null)
                                {
                                    mobItem.CurrentValue = purchaseTnC;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (this.IsELF || this.shopReservationInfo2 != null && this.shopReservationInfo2.IsIBELite)
                        {
                            MOBItem mobItem = this.tcdAdvisoryMessages.Find(p => p.Id == "PurchaseTnC");
                            if (mobItem != null && _configuration.GetValue<string>("TCDAdvisoryMessagesForELF") != null)
                            {
                                var purchaseTnC = this.isELF
                                                    ? _configuration.GetValue<string>("TCDAdvisoryMessagesForELF")
                                                    : _configuration.GetValue<string>("TCDAdvisoryMessagesForIBELite");
                                if (purchaseTnC != null)
                                {
                                    mobItem.CurrentValue = purchaseTnC;
                                }
                            }
                        }
                    }

                }
                return this.tcdAdvisoryMessages;

            }
            set
            {
                this.tcdAdvisoryMessages = value;
            }
        }

        private string GetTCDAdMesgForBE()
        {
            return !string.IsNullOrEmpty(_configuration.GetValue<string>("AddMissingTnCForBE")) && Convert.ToBoolean(_configuration.GetValue<string>("AddMissingTnCForBE")) && this.isRefundable
                    ? _configuration.GetValue<string>("TCDAdvisoryMessagesForBERefundable")
                    : this.isELF
                        ? _configuration.GetValue<string>("TCDAdvisoryMessagesForELF")
                        : (this.ShopReservationInfo2.IsIBELite)
                            ? _configuration.GetValue<string>("TCDAdvisoryMessagesForIBELite")
                            : _configuration.GetValue<string>("TCDAdvisoryMessagesForIBE");
        }

        public string FlightShareMessage
        {
            get
            {
                return this.flightShareMessage;
            }
            set
            {
                this.flightShareMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string PKDispenserPublicKey
        {
            get
            {
                return this.pkDispenserPublicKey;
            }
            set
            {
                this.pkDispenserPublicKey = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MOBLmxFlight> LMXFlights
        {
            get
            {
                return this.lmxFlights;
            }
            set
            {
                this.lmxFlights = value;
            }
        }

        public string IneligibleToEarnCreditMessage
        {
            get
            {
                return this.ineligibleToEarnCreditMessage;
            }
            set
            {
                this.ineligibleToEarnCreditMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string OaIneligibleToEarnCreditMessage
        {
            get
            {
                return this.oaIneligibleToEarnCreditMessage;
            }
            set
            {
                this.oaIneligibleToEarnCreditMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        private bool getALLSavedTravelers = true;
        public bool GetALLSavedTravelers
        {
            get
            {
                return this.getALLSavedTravelers;
            }
            set
            {
                this.getALLSavedTravelers = value;
            }
        }

        private MOBSHOPTripPriceBreakDown shopPriceBreakDown;
        public MOBSHOPTripPriceBreakDown ShopPriceBreakDown
        {
            get
            {
                return this.shopPriceBreakDown;
            }
            set
            {
                this.shopPriceBreakDown = value;
            }

        }

        private MOBSHOPPriceBreakDown priceBreakDown;
        public MOBSHOPPriceBreakDown PriceBreakDown
        {
            get
            {
                return this.priceBreakDown;
            }
            set
            {
                this.priceBreakDown = value;
            }
        }


        public bool IsEmp20
        {
            get
            {
                return this.isEmp20;
            }
            set
            {
                this.isEmp20 = value;
            }
        }

        public bool IsELF
        {
            get
            {
                return this.isELF;
            }
            set
            {
                this.isELF = value;
            }
        }

        public bool IsMetaSearch
        {
            get { return isMetaSearch; }
            set { isMetaSearch = value; }
        }

        public bool IsUpgradedFromEntryLevelFare
        {
            get { return this.isUpgradedFromEntryLevelFare; }
            set
            {
                this.isUpgradedFromEntryLevelFare = value;
                SetUpUpgradedFromELFMessages();
            }
        }

        private List<MOBItem> elfMessagesForRTI;
        [XmlIgnore()]
        public List<MOBItem> ELFMessagesForRTI
        {
            get { return this.elfMessagesForRTI; }
            set { this.elfMessagesForRTI = value; }
        }

        private List<MOBItemWithIconName> elfMessagesForVendorQuery = new List<MOBItemWithIconName>();
        [XmlIgnore()]
        public List<MOBItemWithIconName> ELFMessagesForVendorQuery
        {
            get { return this.elfMessagesForVendorQuery; }
            set { this.elfMessagesForVendorQuery = value; }
        }

        private List<MOBTypeOption> fopOptions;

        public List<MOBTypeOption> FOPOptions
        {
            get
            {
                return this.fopOptions;
            }
            set { this.fopOptions = value; }
        }

        private void SetUpUpgradedFromELFMessages()
        {
            if (!IsUpgradedFromEntryLevelFare)
                return;

            ELFMessagesForRTI = new List<MOBItem> {
                new MOBItem {
                    Id = "UpgradedFromElfTitle",
                    CurrentValue =(_configuration !=null) ?  _configuration.GetValue<string>("UpgradedFromElfTitle") : "We’ve updated your selection"},
                new MOBItem {
                    Id = "UpgradedFromElfText",
                    CurrentValue =(_configuration !=null) ? _configuration.GetValue<string>("UpgradedFromElfText") : "Basic Economy cannot be combined with other fares, so we’ve updated your outbound flight(s) to standard Economy. This does not change the overall ticket price."
                }
            };
        }

        private MOBMasterpassSessionDetails masterpassSessionDetails;
        public MOBMasterpassSessionDetails MasterpassSessionDetails
        {
            get { return masterpassSessionDetails; }
            set { masterpassSessionDetails = value; }
        }

        private MOBMasterpass masterpass;
        public MOBMasterpass Masterpass
        {
            get { return masterpass; }
            set { masterpass = value; }
        }

        private string elfUpgradeMessagesForMetaSearch;

        public string ElfUpgradeMessagesForMetaSearch
        {
            get { return elfUpgradeMessagesForMetaSearch; }
            set { elfUpgradeMessagesForMetaSearch = value; }
        }

        public string MetaSessionId
        {
            get { return metaSessionId; }
            set { metaSessionId = value; }
        }

        private bool isSSA;

        public bool IsSSA
        {
            get { return isSSA; }
            set { isSSA = value; }
        }

        private List<MOBItem> seatAssignmentMessage;

        public List<MOBItem> SeatAssignmentMessage
        {
            get { return seatAssignmentMessage; }
            set { seatAssignmentMessage = value; }
        }

        private MOBSHOPReservationInfo shopReservationInfo;
        public MOBSHOPReservationInfo ShopReservationInfo
        {
            get { return shopReservationInfo; }
            set { shopReservationInfo = value; }
        }

        private MOBSHOPReservationInfo2 shopReservationInfo2;
        public MOBSHOPReservationInfo2 ShopReservationInfo2
        {
            get { return shopReservationInfo2; }
            set { shopReservationInfo2 = value; }
        }
        private MOBTPIInfo tripInsuranceInfo;
        public MOBTPIInfo TripInsuranceInfo
        {
            get { return this.tripInsuranceInfo; }
            set { this.tripInsuranceInfo = value; }
        }
        private MOBTPIInfoInBookingPath tripInsuranceInfoBookingPath;
        public MOBTPIInfoInBookingPath TripInsuranceInfoBookingPath
        {
            get { return this.tripInsuranceInfoBookingPath; }
            set { this.tripInsuranceInfoBookingPath = value; }
        }

        private List<MOBSection> alertMessages;
        public List<MOBSection> AlertMessages
        {
            get { return this.alertMessages; }
            set { this.alertMessages = value; }
        }

        private bool isRedirectToSecondaryPayment = false;
        public bool IsRedirectToSecondaryPayment
        {
            get
            {
                return this.isRedirectToSecondaryPayment;
            }
            set
            {
                this.isRedirectToSecondaryPayment = value;
            }
        }

        public string CheckedbagChargebutton
        {
            get { return checkedbagChargebutton; }
            set { checkedbagChargebutton = value; }
        }

        public bool IsBookingCommonFOPEnabled
        {
            get { return isBookingCommonFOPEnabled; }
            set { isBookingCommonFOPEnabled = value; }
        }

        public bool IsReshopCommonFOPEnabled
        {
            get { return isReshopCommonFOPEnabled; }
            set { isReshopCommonFOPEnabled = value; }
        }
        public bool IsPostBookingCommonFOPEnabled
        {
            get { return isPostBookingCommonFOPEnabled; }
            set { isPostBookingCommonFOPEnabled = value; }
        }

        public bool IsCanadaTravel
        {
            get { return isCanadaTravel; }
            set { isCanadaTravel = value; }
        }

        public List<MOBLMXTraveler> lmxtravelers { get; set; } = null;

        public bool IsEligibleForMoneyPlusMiles
        {
            get { return isEligibleForMoneyPlusMiles; }
            set { isEligibleForMoneyPlusMiles = value; }
        }

        public bool IsMoneyPlusMilesSelected
        {
            get { return isMoneyPlusMilesSelected; }
            set { isMoneyPlusMilesSelected = value; }
        }
        private bool eligibleForETCPricingType;

        public bool EligibleForETCPricingType
        {
            get { return eligibleForETCPricingType; }
            set { eligibleForETCPricingType = value; }
        }

        private string pricingType;

        public string PricingType
        {
            get { return pricingType; }
            set { pricingType = value; }
        }


    }
}