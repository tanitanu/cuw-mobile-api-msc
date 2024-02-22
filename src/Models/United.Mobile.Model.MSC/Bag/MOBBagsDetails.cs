using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace United.Definition.Bag
{
    [Serializable]
    public class MOBBagsDetails
    {
        private List<MOBBag> bags;
        private MOBPassenger passenger;

        public List<MOBBag> Bags
        {
            get
            {
                return this.bags;
            }
            set
            {
                this.bags = value;
            }
        }

        public MOBPassenger Passenger
        {
            get
            {
                return this.passenger;
            }
            set
            {
                this.passenger = value;
            }
        }

        private List<MOBDisplayBagTrackDetails> displayBagTrackDetails;

        public List<MOBDisplayBagTrackDetails> DisplayBagTrackDetails
        {
            get { return displayBagTrackDetails; }
            set { displayBagTrackDetails = value; }
        }

    }
    [Serializable]
    public class MOBDisplayBagTrackDetails
    {
        private string origin;

        public string Origin
        {
            get { return origin; }
            set { origin = value; }
        }

        private string destination;

        public string Destination
        {
            get { return destination; }
            set { destination = value; }
        }

        private string bagTagNumber;

        public string BagTagNumber
        {
            get { return bagTagNumber; }
            set { bagTagNumber = value; }
        }

        private bool bagRerouted;

        public bool BagRerouted
        {
            get { return bagRerouted; }
            set { bagRerouted = value; }
        }

        private string showBagStatus; 
        public string ShowBagStatus
        {
            get { return showBagStatus; }
            set { showBagStatus = value; }
        }

        private MOBBagTrackStatusType showBagTrackStatusType;
        public MOBBagTrackStatusType ShowBagTrackStatusType
        {
            get { return showBagTrackStatusType; }
            set { showBagTrackStatusType = value; }
        }

        private List<MOBDisplayBagTrackStatus> displayBagTrackStatuses;

        public List<MOBDisplayBagTrackStatus> DisplayBagTrackStatuses
        {
            get { return displayBagTrackStatuses; }
            set { displayBagTrackStatuses = value; }
        }

    }

    [Serializable]
    public class MOBDisplayBagTrackStatus
    {
        private string bagFlightSegmentInfo; // Ex: Checked at STL / UA1234/STL to ORD
        public string BagFlightSegmentInfo
        {
            get { return bagFlightSegmentInfo; }
            set { bagFlightSegmentInfo = value; }
        }

        private string bagStatusInfo; // Received at 5:00 am / Arrived at 6:12 am // Bag is on an earlier flight
        public string BagStatusInfo
        {
            get { return bagStatusInfo; }
            set { bagStatusInfo = value; }
        }

        private MOBBagTrackStatusInfoColor bagStatusInfoColor;
        public MOBBagTrackStatusInfoColor BagStatusInfoColor
        {
            get { return bagStatusInfoColor; }
            set { bagStatusInfoColor = value; }
        }

        private MOBBagTrackStatusType bagTrackStatusType; 
        public MOBBagTrackStatusType BagTrackStatusType
        {
            get { return bagTrackStatusType; }
            set { bagTrackStatusType = value; }
        }

        private string bagStatusInfoDetails; // if MOBBagTrackStatusType = Alert or Bag Info then show this message "Your bag is on earlier flight ...." or "Your bag will be placed on a different flight ..." or "When you arrive in Tokyo, please see a United baggage representative..."
        public string BagStatusInfoDetails
        {
            get { return bagStatusInfoDetails; }
            set { bagStatusInfoDetails = value; }
        }

        private MOBDisplayBagTrackFLIFORequest flightStatusRequest; // if the last flight segment is not MOBBagTrackStatusType != Arrived then need to send the Flight Request for Flight Status.
        public MOBDisplayBagTrackFLIFORequest FlightStatusRequest
        {
            get { return flightStatusRequest; }
            set { flightStatusRequest = value; }
        }
    }

    [Serializable]
    public enum MOBBagTrackStatusInfoColor
    {
        [EnumMember(Value = "Default")]
        Default,
        [EnumMember(Value = "Red")]
        Red,
        [EnumMember(Value = "Green")]
        Green
    }

    [Serializable]
    public enum MOBBagTrackStatusType
    {
        [EnumMember(Value = "None")]
        None,
        [EnumMember(Value = "Received")]
        Received,
        [EnumMember(Value = "Arrived")]
        Arrived,
        [EnumMember(Value = "Alert")]
        Alert,
        [EnumMember(Value = "AlertInfo")]
        AlertInfo,
        [EnumMember(Value = "AlertLine")]
        AlertLine,
        [EnumMember(Value = "BagEarlyAlert")] // EX: Bag Icon Image should be displayed folling with this sample message "Bag is on an earlier flight" 
        BagEarlyAlert,
        [EnumMember(Value = "BagEarlyAlertLine")]
        BagEarlyAlertLine,
        [EnumMember(Value = "BagEarlyAlertInfo")]
        BagEarlyAlertInfo,
        [EnumMember(Value = "InFlight")]
        InFlight,
        [EnumMember(Value = "Departs")]
        Departs,
        [EnumMember(Value = "Delayed")]
        Delayed,
        [EnumMember(Value = "Diverted")]
        Diverted,
        [EnumMember(Value = "Cancelled")]
        Cancelled,
        [EnumMember(Value = "FlightStatus")]
        FlightStatus,
        [EnumMember(Value = "BaggageClaim")]
        BaggageClaim
    }

    [Serializable]
    public class MOBDisplayBagTrackFLIFORequest
    {
        private string originAirportCode;

        public string OriginAirportCode
        {
            get { return originAirportCode; }
            set { originAirportCode = value; }
        }

        private string destinationAirportCode;

        public string DestinationAirportCode
        {
            get { return destinationAirportCode; }
            set { destinationAirportCode = value; }
        }

        private int flightNumber;
        public int FlightNumber
        {
            get { return flightNumber; }
            set { flightNumber = value; }
        }

        private string flightDate;
        public string FlightDate
        {
            get { return flightDate; }
            set { flightDate = value; }
        }
    }

    [Serializable]
    public class MOBDisplayBagTrackAirportDetails
    {
        private string airportCode; //STL

        public string AirportCode
        {
            get { return airportCode; }
            set { airportCode = value; }
        }
        private string airportInfo; //St. Louis,MO(STL)

        public string AirportInfo
        {
            get { return airportInfo; }
            set { airportInfo = value; }
        }

        private string airportCityName; //St.Louis

        public string AirportCityName
        {
            get { return airportCityName; }
            set { airportCityName = value; }
        }
    }
}


