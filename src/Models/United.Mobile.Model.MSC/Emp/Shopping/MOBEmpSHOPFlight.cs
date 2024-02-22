using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Emp.Shopping
{
    [Serializable()]
    public class MOBEmpSHOPFlight
    {
        private int segmentIndex;
        private string tripId = string.Empty;
        private string flightId = string.Empty;
        private string productId = string.Empty;
        private decimal airfare;
        private string airfareDisplayValue;
        private int seatsRemaining;
        private string cabin;
        private bool changeOfGauge;
        private List<MOBEmpSHOPGaugeChange> gaugeChanges;
        private List<MOBEmpSHOPFlight> connections;
        private string connectTimeMinutes;
        private string departDate = string.Empty;
        private string departTime = string.Empty;
        private string destination = string.Empty;
        private string destinationDate = string.Empty;
        private string destinationTime = string.Empty;
        private string destinationDescription = string.Empty;
        private MOBEmpSHOPEquipmentDisclosure equipmentDisclosures;
        private string fareBasisCode = string.Empty;
        private string flightNumber = string.Empty;
        private string groundTime;
        private string internationalCity = string.Empty;
        private bool isCheapestAirfare;
        private bool isConnection;
        private string marketingCarrier = string.Empty;
        private string marketingCarrierDescription = string.Empty;
        private string miles = string.Empty;
        private MOBEmpSHOPOnTimePerformance onTimePerformance;
        private string operatingCarrier = string.Empty;
        private string operatingCarrierDescription = string.Empty;
        private string origin = string.Empty;
        private string originDescription = string.Empty;
        private List<MOBEmpSHOPReward> rewards;
        private List<MOBEmpSHOPRewardPriceSummary> rewardPriceSummaries;
        private bool selected;
        private string stopDestination = string.Empty;
        private List<MOBEmpSHOPFlight> stopInfos;
        private int stops;
        private string travelTime;
        private string totalTravelTime;
        private List<MOBEmpSHOPMessage> messages;
        private string meal;
        public bool fpwSAir { get; set; }
        private string serviceClass = string.Empty;
        private string serviceClassDescription = string.Empty;
        private string epaMessageTitle = string.Empty;
        private string epaMessage = string.Empty;
        private bool showEPAMessage = false;
        private bool isCheckInWindow = false;
        private string checkInWindowText = string.Empty;
        private string departureDateTime = string.Empty;
        private string arrivalDateTime = string.Empty;
        private string departureDateTimeGMT = string.Empty;
        private string arrivalDateTimeGMT = string.Empty;
        private bool matchServiceClassRequested;
        //adding ammenities bool values - Wade 4/16/2014
        private bool hasWifi = false;
        private bool hasInSeatPower = false;
        private bool hasDirecTV = false;
        private bool hasAVOnDemand = false;
        private bool hasBeverageService = false;
        private bool hasEconomyLieFlatSeating = false;
        private bool hasEconomyMeal = false;
        private bool hasFirstClassMeal = false;
        private bool hasFirstClassLieFlatSeating = false;
        private List<MOBEmpSHOPShoppingProduct> shoppingProducts;
        private string segID = string.Empty;
        private int segNumber;
        private string flightDepartureDays = string.Empty;
        private string flightArrivalDays = string.Empty;
        private MOBEmpSegmentPBT mobEmpSegmentPBT;
        private string milesDisplayValue = string.Empty;
        private string cabinDisclaimer = string.Empty;
        private string preferredCabinName = string.Empty;
        private string preferredCabinMessage = string.Empty;
        private string overnightConnection = string.Empty;
        private bool showSeatMap = false;
        private bool isStopOver = false;
        private string pqdText = string.Empty;
        private string pqmText = string.Empty;
        private string rdmText = string.Empty;
        private string yqyrMessage = string.Empty;
        private string govtMessage = string.Empty;
        private bool isAwardSaver;
        private string redEyeFlightDepDate;
        private string nextDayFlightArrDate;
        private bool flightDateChanged;
        private string psCost;

        public MOBEmpSegmentPBT MobEmpSegmentPBT
        {
            get { return mobEmpSegmentPBT; }
            set { mobEmpSegmentPBT = value; }
        }
        public string MilesDisplayValue
        {
            get { return milesDisplayValue; }
            set { milesDisplayValue = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
        public string CabinDisclaimer
        {
            get { return cabinDisclaimer; }
            set { cabinDisclaimer = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
        public string PreferredCabinName
        {
            get { return preferredCabinName; }
            set { preferredCabinName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
        public string PreferredCabinMessage
        {
            get { return preferredCabinMessage; }
            set { preferredCabinMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
        public string OvernightConnection
        {
            get { return overnightConnection; }
            set { overnightConnection = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
        public bool ShowSeatMap
        {
            get { return showSeatMap; }
            set { showSeatMap = value; }
        }
        public bool IsStopOver
        {
            get { return isStopOver; }
            set { isStopOver = value; }
        }
        public int SegmentIndex
        {
            get
            {
                return this.segmentIndex;
            }
            set
            {
                this.segmentIndex = value;
            }
        }
        public string TripId
        {
            get
            {
                return this.tripId;
            }
            set
            {
                this.tripId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string FlightId
        {
            get
            {
                return this.flightId;
            }
            set
            {
                this.flightId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ProductId
        {
            get
            {
                return this.productId;
            }
            set
            {
                this.productId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public decimal Airfare
        {
            get
            {
                return this.airfare;
            }
            set
            {
                this.airfare = value;
            }
        }

        public string AirfareDisplayValue
        {
            get
            {
                return this.airfareDisplayValue;
            }
            set
            {
                this.airfareDisplayValue = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int SeatsRemaining
        {
            get
            {
                return this.seatsRemaining;
            }
            set
            {
                this.seatsRemaining = value;
            }
        }

        public string Cabin
        {
            get
            {
                return this.cabin;
            }
            set
            {
                this.cabin = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool ChangeOfGauge
        {
            get
            {
                return this.changeOfGauge;
            }
            set
            {
                this.changeOfGauge = value;
            }
        }

        public List<MOBEmpSHOPGaugeChange> GaugeChanges
        {
            get
            {
                return this.gaugeChanges;
            }
            set
            {
                this.gaugeChanges = value;
            }
        }

        public List<MOBEmpSHOPFlight> Connections
        {
            get
            {
                return this.connections;
            }
            set
            {
                this.connections = value;
            }
        }

        public string ConnectTimeMinutes
        {
            get
            {
                return this.connectTimeMinutes;
            }
            set
            {
                this.connectTimeMinutes = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string DepartDate
        {
            get
            {
                return this.departDate;
            }
            set
            {
                this.departDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string DepartTime
        {
            get
            {
                return this.departTime;
            }
            set
            {
                this.departTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Destination
        {
            get
            {
                return this.destination;
            }
            set
            {
                this.destination = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string DestinationDate
        {
            get
            {
                return this.destinationDate;
            }
            set
            {
                this.destinationDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string DestinationTime
        {
            get
            {
                return this.destinationTime;
            }
            set
            {
                this.destinationTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string DestinationDescription
        {
            get
            {
                return this.destinationDescription;
            }
            set
            {
                this.destinationDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public MOBEmpSHOPEquipmentDisclosure EquipmentDisclosures
        {
            get
            {
                return this.equipmentDisclosures;
            }
            set
            {
                this.equipmentDisclosures = value;
            }
        }

        public string FareBasisCode
        {
            get
            {
                return this.fareBasisCode;
            }
            set
            {
                this.fareBasisCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string FlightNumber
        {
            get
            {
                return this.flightNumber;
            }
            set
            {
                this.flightNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string GroundTime
        {
            get
            {
                return this.groundTime;
            }
            set
            {
                this.groundTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); ;
            }
        }

        public string InternationalCity
        {
            get
            {
                return this.internationalCity;
            }
            set
            {
                this.internationalCity = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool IsCheapestAirfare
        {
            get
            {
                return this.isCheapestAirfare;
            }
            set
            {
                this.isCheapestAirfare = value;
            }
        }

        public bool IsConnection
        {
            get
            {
                return this.isConnection;
            }
            set
            {
                this.isConnection = value;
            }
        }

        public string MarketingCarrier
        {
            get
            {
                return this.marketingCarrier;
            }
            set
            {
                this.marketingCarrier = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string MarketingCarrierDescription
        {
            get
            {
                return this.marketingCarrierDescription;
            }
            set
            {
                this.marketingCarrierDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Miles
        {
            get
            {
                return this.miles;
            }
            set
            {
                this.miles = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public MOBEmpSHOPOnTimePerformance OnTimePerformance
        {
            get
            {
                return this.onTimePerformance;
            }
            set
            {
                this.onTimePerformance = value;
            }
        }

        public string OperatingCarrier
        {
            get
            {
                return this.operatingCarrier;
            }
            set
            {
                this.operatingCarrier = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string OperatingCarrierDescription
        {
            get
            {
                return this.operatingCarrierDescription;
            }
            set
            {
                this.operatingCarrierDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Origin
        {
            get
            {
                return this.origin;
            }
            set
            {
                this.origin = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string OriginDescription
        {
            get
            {
                return this.originDescription;
            }
            set
            {
                this.originDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MOBEmpSHOPReward> Rewards
        {
            get
            {
                return this.rewards;
            }
            set
            {
                this.rewards = value;
            }
        }

        public List<MOBEmpSHOPRewardPriceSummary> RewardPriceSummaries
        {
            get
            {
                return this.rewardPriceSummaries;
            }
            set
            {
                this.rewardPriceSummaries = value;
            }
        }

        public bool Selected
        {
            get
            {
                return this.selected;
            }
            set
            {
                this.selected = value;
            }
        }

        public string StopDestination
        {
            get
            {
                return this.stopDestination;
            }
            set
            {
                this.stopDestination = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MOBEmpSHOPFlight> StopInfos
        {
            get
            {
                return this.stopInfos;
            }
            set
            {
                this.stopInfos = value;
            }
        }

        public int Stops
        {
            get
            {
                return this.stops;
            }
            set
            {
                this.stops = value;
            }
        }

        public string TravelTime
        {
            get
            {
                return this.travelTime;
            }
            set
            {
                this.travelTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string TotalTravelTime
        {
            get
            {
                return this.totalTravelTime;
            }
            set
            {
                this.totalTravelTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); ;
            }
        }

        public List<MOBEmpSHOPMessage> Messages
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

        public string Meal
        {
            get
            {
                return this.meal;
            }
            set
            {
                this.meal = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); ;
            }
        }

        
        public string ServiceClass
        {
            get
            {
                return this.serviceClass;
            }
            set
            {
                this.serviceClass = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); ;
            }
        }

        public string ServiceClassDescription
        {
            get
            {
                return this.serviceClassDescription;
            }
            set
            {
                this.serviceClassDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); ;
            }
        }

        public string EPAMessageTitle
        {
            get { return this.epaMessageTitle; }
            set { this.epaMessageTitle = value; }
        }

        public string EPAMessage
        {
            get { return this.epaMessage; }
            set { this.epaMessage = value; }
        }

        public bool ShowEPAMessage
        {
            get { return this.showEPAMessage; }
            set { this.showEPAMessage = value; }
        }

        public bool IsCheckInWindow
        {
            get
            {
                return this.isCheckInWindow;
            }
            set
            {
                this.isCheckInWindow = value;
            }
        }

        public string CheckInWindowText
        {
            get
            {
                return this.checkInWindowText;
            }
            set
            {
                this.checkInWindowText = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string DepartureDateTime
        {
            get
            {
                return this.departureDateTime;
            }
            set
            {
                this.departureDateTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ArrivalDateTime
        {
            get
            {
                return this.arrivalDateTime;
            }
            set
            {
                this.arrivalDateTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string DepartureDateTimeGMT
        {
            get
            {
                return this.departureDateTimeGMT;
            }
            set
            {
                this.departureDateTimeGMT = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ArrivalDateTimeGMT
        {
            get
            {
                return this.arrivalDateTimeGMT;
            }
            set
            {
                this.arrivalDateTimeGMT = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool MatchServiceClassRequested
        {
            get
            {
                return this.matchServiceClassRequested;
            }
            set
            {
                this.matchServiceClassRequested = value;
            }
        }

        public bool HasInSeatPower
        {
            get
            {
                return this.hasInSeatPower;
            }
            set
            {
                this.hasInSeatPower = value;
            }
        }

        public bool HasWifi
        {
            get
            {
                return this.hasWifi;
            }
            set
            {
                this.hasWifi = value;
            }
        }

        public bool HasDirecTV
        {
            get
            {
                return this.hasDirecTV;
            }
            set
            {
                this.hasDirecTV = value;
            }
        }

        public bool HasAVOnDemand
        {
            get
            {
                return this.hasAVOnDemand;
            }
            set
            {
                this.hasAVOnDemand = value;
            }
        }

        public bool HasBeverageService
        {
            get
            {
                return this.hasBeverageService;
            }
            set
            {
                this.hasBeverageService = value;
            }
        }

        public bool HasEconomyLieFlatSeating
        {
            get
            {
                return this.hasEconomyLieFlatSeating;
            }
            set
            {
                this.hasEconomyLieFlatSeating = value;
            }
        }

        public bool HasEconomyMeal
        {
            get
            {
                return this.hasEconomyMeal;
            }
            set
            {
                this.hasEconomyMeal = value;
            }
        }

        public bool HasFirstClassMeal
        {
            get
            {
                return this.hasFirstClassMeal;
            }
            set
            {
                this.hasFirstClassMeal = value;
            }
        }

        public bool HasFirstClassLieFlatSeating
        {
            get
            {
                return this.hasFirstClassLieFlatSeating;
            }
            set
            {
                this.hasFirstClassLieFlatSeating = value;
            }
        }

        public List<MOBEmpSHOPShoppingProduct> ShoppingProducts
        {
            get
            {
                return this.shoppingProducts;
            }
            set
            {
                this.shoppingProducts = value;
            }
        }

        public string SegID
        {
            get
            {
                return this.segID;
            }
            set
            {
                this.segID = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int SegmentNumber
        {
            get
            {
                return this.segNumber;
            }
            set
            {
                this.segNumber = value;
            }
        }

        public string FlightDepartureDays
        {
            get
            {
                return this.flightDepartureDays;
            }
            set
            {
                this.flightDepartureDays = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string FlightArrivalDays
        {
            get
            {
                return this.flightArrivalDays;
            }
            set
            {
                this.flightArrivalDays = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }


        public string PqdText
        {
            get { return pqdText; }
            set { pqdText = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }


        public string PqmText
        {
            get { return pqmText; }
            set { pqmText = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }


        public string RdmText
        {
            get { return rdmText; }
            set { rdmText = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }


        public string YqyrMessage
        {
            get { return yqyrMessage; }
            set { yqyrMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }


        public string GovtMessage
        {
            get { return govtMessage; }
            set { govtMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }


        public bool IsAwardSaver
        {
            get
            {
                return this.isAwardSaver;
            }
            set
            {
                this.isAwardSaver = value;
            }
        }

        public string RedEyeFlightDepDate
        {
            get { return redEyeFlightDepDate; }
            set { redEyeFlightDepDate = value; }
        }



        public string NextDayFlightArrDate
        {
            get { return nextDayFlightArrDate; }
            set { nextDayFlightArrDate = value; }
        }



        public bool FlightDateChanged
        {
            get { return this.flightDateChanged; }
            set { this.flightDateChanged = value; }
        }
        public string PSCost
        {
            get
            {
                return this.psCost;
            }
            set
            {
                this.psCost = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
