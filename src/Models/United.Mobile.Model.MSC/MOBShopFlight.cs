using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition
{
    [Serializable()]
    public class MOBShopFlight
    {
        private decimal airfare;
        private string alternateServiceClass = string.Empty;
        private string arrivalOffset = string.Empty;
        private string arrivalGate = string.Empty;
        private string arrivalTimeZone = string.Empty;
        private List<MOBShopClassOfService> availability;
        private string bbxCellId = string.Empty;
        private string bbxHash = string.Empty;
        private string bookingClassAvailability = string.Empty;
        private int cabinCount;
        private List<MOBShopCabin> cabins;
        private string changeOfGauge = string.Empty;
        private bool classList;
        private List<MOBShopFlight> connections;
        private int connectTimeMinutes;
        private List<MOBShopDEI> deis;
        private string departDateTime = string.Empty;
        private string departureGate = string.Empty;
        private string departureTimeZone = string.Empty;
        private string destination = string.Empty;
        private string destinationCountryCode = string.Empty;
        private string destinationDateTime = string.Empty;
        private string destinationDescription = string.Empty;
        private MOBShopEquipmentDisclosure equipmentDisclosures;
        private bool extraSection;
        private string fareBasisCode = string.Empty;
        private MOBShopFlightInfo flightInfo;
        private string flightNumber = string.Empty;
        private int groundTimeMinutes;
        private string international = string.Empty;
        private string internationalCity = string.Empty;
        private bool isCheapestAirfare;
        private bool isConnection;
        private string marketingCarrier = string.Empty;
        private string marketingCarrierDescription = string.Empty;
        private string miles = string.Empty;
        private string noLocalTraffic = string.Empty;
        private MOBShopOnTimePerformance onTimePerformance;
        private string operatingCarrier = string.Empty;
        private string operatingCarrierDescription = string.Empty;
        private string origin = string.Empty;
        private string originalFlightNumber = string.Empty;
        private string originCountryCode = string.Empty;
        private string originDescription = string.Empty;
        private decimal otherTaxes;
        private string parentFlightNumber = string.Empty;
        private List<MOBShopPrice> prices;
        private List<MOBShopReward> rewards;
        private bool selected;
        private string stopDestination = string.Empty;
        private List<MOBShopFlight> stopInfos;
        private int stops;
        private string ticketDesignator = string.Empty;
        private int travelMinutes;
        private int travelMinutesTotal;
        private string upgradableCustomers = string.Empty;

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

        public string AlternateServiceClass
        {
            get
            {
                return this.alternateServiceClass;
            }
            set
            {
                this.alternateServiceClass = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ArrivalOffset
        {
            get
            {
                return this.arrivalOffset;
            }
            set
            {
                this.arrivalOffset = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ArrivalGate
        {
            get
            {
                return this.arrivalGate;
            }
            set
            {
                this.arrivalGate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ArrivalTimeZone
        {
            get
            {
                return this.arrivalTimeZone;
            }
            set
            {
                this.arrivalTimeZone = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MOBShopClassOfService> Availability
        {
            get
            {
                return this.availability;
            }
            set
            {
                this.availability = value;
            }
        }

        public string BBXCellId
        {
            get
            {
                return this.bbxCellId;
            }
            set
            {
                this.bbxCellId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string BBXHash
        {
            get
            {
                return this.bbxHash;
            }
            set
            {
                this.bbxHash = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string BookingClassAvailability
        {
            get
            {
                return this.bookingClassAvailability;
            }
            set
            {
                this.bookingClassAvailability = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int CabinCount
        {
            get
            {
                return this.cabinCount;
            }
            set
            {
                this.cabinCount = value;
            }
        }

        public List<MOBShopCabin> Cabins
        {
            get
            {
                return this.cabins;
            }
            set
            {
                this.cabins = value;
            }
        }

        public string ChangeOfGauge
        {
            get
            {
                return this.changeOfGauge;
            }
            set
            {
                this.changeOfGauge = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool ClassList
        {
            get
            {
                return this.classList;
            }
            set
            {
                this.classList = value;
            }
        }

        public List<MOBShopFlight> Connections
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

        public int ConnectTimeMinutes
        {
            get
            {
                return this.connectTimeMinutes;
            }
            set
            {
                this.connectTimeMinutes = value;
            }
        }

        public List<MOBShopDEI> DEIs
        {
            get
            {
                return this.deis;
            }
            set
            {
                this.deis = value;
            }
        }

        public string DepartDateTime
        {
            get
            {
                return this.departDateTime;
            }
            set
            {
                this.departDateTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string DepartureGate
        {
            get
            {
                return this.departureGate;
            }
            set
            {
                this.departureGate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string DepartureTimeZone
        {
            get
            {
                return this.departureTimeZone;
            }
            set
            {
                this.departureTimeZone = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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

        public string DestinationCountryCode
        {
            get
            {
                return this.destinationCountryCode;
            }
            set
            {
                this.destinationCountryCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string DestinationDateTime
        {
            get
            {
                return this.destinationDateTime;
            }
            set
            {
                this.destinationDateTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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

        public MOBShopEquipmentDisclosure EquipmentDisclosures
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

        public bool ExtraSection
        {
            get
            {
                return this.extraSection;
            }
            set
            {
                this.extraSection = value;
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

        public MOBShopFlightInfo FlightInfo
        {
            get
            {
                return this.flightInfo;
            }
            set
            {
                this.flightInfo = value;
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

        public int GroundTimeMinutes
        {
            get
            {
                return this.groundTimeMinutes;
            }
            set
            {
                this.groundTimeMinutes = value;
            }
        }

        public string International
        {
            get
            {
                return this.international;
            }
            set
            {
                this.international = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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

        public string NoLocalTraffic
        {
            get
            {
                return this.noLocalTraffic;
            }
            set
            {
                this.noLocalTraffic = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public MOBShopOnTimePerformance OnTimePerformance
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

        public string OriginalFlightNumber
        {
            get
            {
                return this.originalFlightNumber;
            }
            set
            {
                this.originalFlightNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string OriginCountryCode
        {
            get
            {
                return this.originCountryCode;
            }
            set
            {
                this.originCountryCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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

        public decimal OtherTaxes
        {
            get
            {
                return this.otherTaxes;
            }
            set
            {
                this.otherTaxes = value;
            }
        }

        public string ParentFlightNumber
        {
            get
            {
                return this.parentFlightNumber;
            }
            set
            {
                this.parentFlightNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MOBShopPrice> Prices
        {
            get
            {
                return this.prices;
            }
            set
            {
                this.prices = value;
            }
        }

        public List<MOBShopReward> Rewards
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

        public List<MOBShopFlight> StopInfos
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

        public string TicketDesignator
        {
            get
            {
                return this.ticketDesignator;
            }
            set
            {
                this.ticketDesignator = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int TravelMinutes
        {
            get
            {
                return this.travelMinutes;
            }
            set
            {
                this.travelMinutes = value;
            }
        }

        public int TravelMinutesTotal
        {
            get
            {
                return this.travelMinutesTotal;
            }
            set
            {
                this.travelMinutesTotal = value;
            }
        }

        public string UpgradableCustomers
        {
            get
            {
                return this.upgradableCustomers;
            }
            set
            {
                this.upgradableCustomers = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
