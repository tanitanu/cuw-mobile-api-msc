
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace United.Definition
{
    [Serializable]
    public class CouchFlightStatusSegment// : Cacheable
    {
        public CouchFlightStatusSegment()
            : base()
        {
            //this.AddToComplications = true;
            this.addToComplications = true;
        }

        public string PredictableKey { get; set; }

        //[JsonProperty("addToComplications")]
        //public bool AddToComplications { get; set; }
        private bool addToComplications;
        public bool AddToComplications
        {
            get
            {
                return this.addToComplications;
            }
            set
            {
                this.addToComplications = value;
            }
        }


        //[JsonProperty("flightNumber")]
        //public string FlightNumber { get; set; }
        private string flightNumber = string.Empty;
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

        //[JsonProperty("departure")]
        //public CouchAirport Departure { get; set; }
        private CouchAirport departure;
        public CouchAirport Departure
        {
            get
            {
                return this.departure;
            }
            set
            {
                this.departure = value;
            }
        }
        //[JsonProperty("arrival")]
        //public CouchAirport Arrival { get; set; }
        private CouchAirport arrival;
        public CouchAirport Arrival
        {
            get
            {
                return this.arrival;
            }
            set
            {
                this.arrival = value;
            }
        }

        //[JsonProperty("scheduledDepartureDateTime")]
        //public string ScheduledDepartureDateTime { get; set; }
        private string scheduledDepartureDateTime = string.Empty;
        public string ScheduledDepartureDateTime
        {
            get
            {
                return this.scheduledDepartureDateTime;
            }
            set
            {
                this.scheduledDepartureDateTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        //[JsonProperty("scheduledArrivalDateTime")]
        //public string ScheduledArrivalDateTime { get; set; }
        private string scheduledArrivalDateTime = string.Empty;
        public string ScheduledArrivalDateTime
        {
            get
            {
                return this.scheduledArrivalDateTime;
            }
            set
            {
                this.scheduledArrivalDateTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        //[JsonProperty("scheduledDepartureTimeGMT")]
        //public string ScheduledDepartureTimeGMT { get; set; }
        private string scheduledDepartureTimeGMT = string.Empty;
        public string ScheduledDepartureTimeGMT
        {
            get
            {
                return this.scheduledDepartureTimeGMT;
            }
            set
            {
                this.scheduledDepartureTimeGMT = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        //[JsonProperty("scheduledArrivalTimeGMT")]
        //public string ScheduledArrivalTimeGMT { get; set; }
        private string scheduledArrivalTimeGMT = string.Empty;
        public string ScheduledArrivalTimeGMT
        {
            get
            {
                return this.scheduledArrivalTimeGMT;
            }
            set
            {
                this.scheduledArrivalTimeGMT = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        //[JsonProperty("estimatedDepartureDateTime")]
        //public string EstimatedDepartureDateTime { get; set; }
        private string estimatedDepartureDateTime = string.Empty;
        public string EstimatedDepartureDateTime
        {
            get
            {
                return this.estimatedDepartureDateTime;
            }
            set
            {
                this.estimatedDepartureDateTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        //[JsonProperty("estimatedArrivalDateTime")]
        //public string EstimatedArrivalDateTime { get; set; }
        private string estimatedArrivalDateTime = string.Empty;
        public string EstimatedArrivalDateTime
        {
            get
            {
                return this.estimatedArrivalDateTime;
            }
            set
            {
                this.estimatedArrivalDateTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        //[JsonProperty("actualDepartureDateTime")]
        //public string ActualDepartureDateTime { get; set; }
        private string actualDepartureDateTime = string.Empty;
        public string ActualDepartureDateTime
        {
            get
            {
                return this.actualDepartureDateTime;
            }
            set
            {
                this.actualDepartureDateTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        //[JsonProperty("actualArrivalDateTime")]
        //public string ActualArrivalDateTime { get; set; }
        private string actualArrivalDateTime = string.Empty;
        public string ActualArrivalDateTime
        {
            get
            {
                return this.actualArrivalDateTime;
            }
            set
            {
                this.actualArrivalDateTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        //[JsonProperty("scheduledFlightTime")]
        //public string ScheduledFlightTime { get; set; }
        //[JsonProperty("actualFlightTime")]
        //public string ActualFlightTime { get; set; }

        //[JsonProperty("status")]
        //public string Status { get; set; }
        private string status = string.Empty;
        public string Status
        {
            get
            {
                return this.status;
            }
            set
            {
                this.status = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        //[JsonProperty("statusShort")]
        //public string StatusShort { get; set; }
        private string statusShort = string.Empty;
        public string StatusShort
        {
            get
            {
                return this.statusShort;
            }
            set
            {
                this.statusShort = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        //[JsonProperty("isSegmentCancelled")]
        //public bool IsSegmentCancelled { get; set; }
        private bool isSegmentCancelled;
        public bool IsSegmentCancelled
        {
            get
            {
                return isSegmentCancelled;
            }
            set
            {
                this.isSegmentCancelled = value;
            }
        }

        //[JsonProperty("departureTerminal")]
        //public string DepartureTerminal { get; set; }
        private string departureTerminal = string.Empty;
        public string DepartureTerminal
        {
            get
            {
                return this.departureTerminal;
            }
            set
            {
                this.departureTerminal = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        //[JsonProperty("arrivalTerminal")]
        //public string ArrivalTerminal { get; set; }
        private string arrivalTerminal = string.Empty;
        public string ArrivalTerminal
        {
            get
            {
                return this.arrivalTerminal;
            }
            set
            {
                this.arrivalTerminal = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        //[JsonProperty("departureGate")]
        //public string DepartureGate { get; set; }
        private string departureGate = string.Empty;

        public string DepartureGate
        {
            get
            {
                return this.departureGate;
            }
            set
            {
                this.departureGate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
        //[JsonProperty("arrivalGate")]
        //public string ArrivalGate { get; set; }
        private string arrivalGate = string.Empty;
        public string ArrivalGate
        {
            get
            {
                return this.arrivalGate;
            }
            set
            {
                this.arrivalGate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        //[JsonProperty("operatingCarrier")]
        //public CouchAirline OperatingCarrier { get; set; }
        private CouchAirline operatingCarrier;
        public CouchAirline OperatingCarrier
        {
            get
            {
                return this.operatingCarrier;
            }
            set
            {
                this.operatingCarrier = value;
            }
        }
        //[JsonProperty("marketingCarrier")]
        //public CouchAirline MarketingCarrier { get; set; }

        //[JsonProperty("baggage")]
        //public CouchBaggage Baggage { get; set; }
        private CouchBaggage baggage;
        public CouchBaggage Baggage
        {
            get { return this.baggage; }
            set { this.baggage = value; }
        }

        //[JsonProperty("ship")]
        //public CouchEquipment Ship { get; set; }
        private CouchEquipment ship;
        public CouchEquipment Ship
        {
            get
            {
                return this.ship;
            }
            set
            {
                this.ship = value;
            }
        }
        //[JsonProperty("codeShareflightNumber")]
        //public string CodeShareflightNumber { get; set; }
        private string codeShareflightNumber = string.Empty;
        public string CodeShareflightNumber
        {
            get
            {
                return this.codeShareflightNumber;
            }
            set
            {
                this.codeShareflightNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        //[JsonProperty("iSWiFiAvailable")]
        //public bool ISWiFiAvailable { get; set; }
        private bool isWiFiAvailable;
        public bool ISWiFiAvailable
        {
            get
            {
                return isWiFiAvailable;
            }
            set
            {
                this.isWiFiAvailable = value;
            }
        }
        //[JsonProperty("pushNotificationRegId")]
        //public string PushNotificationRegId { get; set; }
        private string pushNotificationRegId = string.Empty;
        public string PushNotificationRegId
        {
            get
            {
                return this.pushNotificationRegId;
            }
            set
            {
                this.pushNotificationRegId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        //[JsonProperty("enableSeatMap")]
        //public bool EnableSeatMap { get; set; }
        private bool enableSeatMap;
        public bool EnableSeatMap
        {
            get
            {
                return enableSeatMap;
            }
            set
            {
                this.enableSeatMap = value;
            }
        }
        //[JsonProperty("enableStandbyList")]
        //public bool EnableStandbyList { get; set; }
        private bool enableStandbyList;
        public bool EnableStandbyList
        {
            get
            {
                return enableStandbyList;
            }
            set
            {
                this.enableStandbyList = value;
            }
        }
        //[JsonProperty("enableUpgradeList")]
        //public bool EnableUpgradeList { get; set; }
        private bool enableUpgradeList;
        public bool EnableUpgradeList
        {
            get
            {
                return enableUpgradeList;
            }
            set
            {
                this.enableUpgradeList = value;
            }
        }
        //[JsonProperty("enableAmenity")]
        //public bool EnableAmenity { get; set; }
        private bool enableAmenity;
        public bool EnableAmenity
        {
            get
            {
                return enableAmenity;
            }
            set
            {
                this.enableAmenity = value;
            }
        }

        public bool GetInBoundSegment { get; set; }
        public CouchInBoundFlightSegment InBoundSegment { get; set; }
    }
}