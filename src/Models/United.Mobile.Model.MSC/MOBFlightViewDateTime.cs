using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{

    [Serializable()]
    public class MOBFlightViewRequest : MOBRequest
    {
        private string carrierCode = string.Empty;
        private string flightNumber = string.Empty;
        private string flightDate = string.Empty;
        private string originCode = string.Empty;

        public MOBFlightViewRequest()
            : base()
        {
        }

        public string CarrierCode
        {
            get
            {
                return this.carrierCode;
            }
            set
            {
                this.carrierCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
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

        public string FlightDate
        {
            get
            {
                return this.flightDate;
            }
            set
            {
                this.flightDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string OriginCode
        {
            get { return originCode; }
            set
            {
                this.originCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }

    [Serializable()]
    public class MOBFlightViewResponse : MOBResponse
    {
        public MOBFlightViewResponse()
            : base()
        {
        }
        private MOBFlightViewFlight flight;

        public MOBFlightViewFlight Flight
        {
            get { return flight; }
            set { flight = value; }
        }
    }
    [Serializable()]
    public class MOBFlightViewFlight
    {
        public MOBFlightViewFlight()
            : base()
        {
        }
        private string map = string.Empty;

        public string Map
        {
            get { return map; }
            set { map = value; }
        }
        private string flightID = string.Empty;

        public string FlightID
        {
            get { return flightID; }
            set { flightID = value; }
        }
        private string flightNumber = string.Empty;

        public string FlightNumber
        {
            get { return flightNumber; }
            set { flightNumber = value; }
        }
        private MOBFlightViewAirLine airLine;

        public MOBFlightViewAirLine AirLine
        {
            get { return airLine; }
            set { airLine = value; }
        }
        private MOBFlightViewAirCraft airCraft;

        public MOBFlightViewAirCraft AirCraft
        {
            get { return airCraft; }
            set { airCraft = value; }
        }
        private MOBFlightViewFlightStatus flightStatus;

        public MOBFlightViewFlightStatus FlightStatus
        {
            get { return flightStatus; }
            set { flightStatus = value; }
        }
        private string scheduleStatus = string.Empty;

        public string ScheduleStatus
        {
            get { return scheduleStatus; }
            set { scheduleStatus = value; }
        }
        private string svcType = string.Empty;

        public string SvcType
        {
            get { return svcType; }
            set { svcType = value; }
        }
        private MOBFlightViewRelativeTime relativeTime;

        public MOBFlightViewRelativeTime RelativeTime
        {
            get { return relativeTime; }
            set { relativeTime = value; }
        }

        private MOBFlightViewAirborne airborne;

        public MOBFlightViewAirborne Airborne
        {
            get { return airborne; }
            set { airborne = value; }
        }

        private MOBFlightViewDA departure;

        public MOBFlightViewDA Departure
        {
            get { return departure; }
            set { departure = value; }
        }
        private MOBFlightViewDA arrival;

        public MOBFlightViewDA Arrival
        {
            get { return arrival; }
            set { arrival = value; }
        }
    }

    [Serializable()]
    public class MOBFlightViewDA
    {
        public MOBFlightViewDA()
            : base()
        {
        }
        private MOBFlightViewAirport airport;

        public MOBFlightViewAirport Airport
        {
            get { return airport; }
            set { airport = value; }
        }
        private List<MOBFlightViewDateTime> dateTime;

        public List<MOBFlightViewDateTime> DateTime
        {
            get { return dateTime; }
            set { dateTime = value; }
        }
    }

    [Serializable()]
    public class MOBFlightViewDateTime
    {
        public MOBFlightViewDateTime()
            : base()
        {
        }
        private string date = string.Empty;

        public string Date
        {
            get { return date; }
            set { date = value; }
        }
        private string time = string.Empty;

        public string Time
        {
            get { return time; }
            set { time = value; }
        }
        private string local = string.Empty;

        public string Local
        {
            get { return local; }
            set { local = value; }
        }
        private string scheduled = string.Empty;

        public string Scheduled
        {
            get { return scheduled; }
            set { scheduled = value; }
        }
        private string gateTime = string.Empty;

        public string GateTime
        {
            get { return gateTime; }
            set { gateTime = value; }
        }
        private string scheduleData = string.Empty;

        public string ScheduleData
        {
            get { return scheduleData; }
            set { scheduleData = value; }
        }
        private string airlineData = string.Empty;

        public string AirlineData
        {
            get { return airlineData; }
            set { airlineData = value; }
        }
    }

    [Serializable()]
    public class MOBAirportLocation
    {
        public MOBAirportLocation()
            : base()
        {
        }
        private string cityName = string.Empty;

        public string CityName
        {
            get { return cityName; }
            set { cityName = value; }
        }
        private string stateId = string.Empty;

        public string StateId
        {
            get { return stateId; }
            set { stateId = value; }
        }
        private string countryId = string.Empty;

        public string CountryId
        {
            get { return countryId; }
            set { countryId = value; }
        }
    }


    [Serializable()]
    public class MOBFlightViewAirport
    {
        public MOBFlightViewAirport()
            : base()
        {
        }
        private string airportCode = string.Empty;

        public string AirportCode
        {
            get { return airportCode; }
            set { airportCode = value; }
        }
        private string iATACode = string.Empty;

        public string IATACode
        {
            get { return iATACode; }
            set { iATACode = value; }
        }
        private string airportName = string.Empty;

        public string AirportName
        {
            get { return airportName; }
            set { airportName = value; }
        }
        private string terminal = string.Empty;

        public string Terminal
        {
            get { return terminal; }
            set { terminal = value; }
        }
        private string gate = string.Empty;

        public string Gate
        {
            get { return gate; }
            set { gate = value; }
        }
        private MOBAirportLocation airportLocation;

        public MOBAirportLocation AirportLocation
        {
            get { return airportLocation; }
            set { airportLocation = value; }
        }

    }

    [Serializable()]
    public class MOBFlightViewFlightStatus
    {
        public MOBFlightViewFlightStatus()
            : base()
        {
        }
        private string inGate = string.Empty;

        public string InGate
        {
            get { return inGate; }
            set { inGate = value; }
        }
        private string original = string.Empty;

        public string Original
        {
            get { return original; }
            set { original = value; }
        }
    }

    [Serializable()]
    public class MOBFlightViewAirCraft
    {
        public MOBFlightViewAirCraft()
            : base()
        {
        }
        private string aircraftType = string.Empty;

        public string AircraftType
        {
            get { return aircraftType; }
            set { aircraftType = value; }
        }
        private string weightClass = string.Empty;

        public string WeightClass
        {
            get { return weightClass; }
            set { weightClass = value; }
        }
    }

    [Serializable()]
    public class MOBFlightViewAirLine
    {
        public MOBFlightViewAirLine()
            : base()
        {
        }
        private string code = string.Empty;

        public string Code
        {
            get { return code; }
            set { code = value; }
        }
        private string iATACode = string.Empty;

        public string IATACode
        {
            get { return iATACode; }
            set { iATACode = value; }
        }
        private string name = string.Empty;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
    }

    [Serializable()]
    public class MOBFlightViewRelativeTime
    {
        public MOBFlightViewRelativeTime()
            : base()
        {
        }
        private string time = string.Empty;

        public string Time
        {
            get { return time; }
            set { time = value; }
        }
        private string remaining = string.Empty;

        public string Remaining
        {
            get { return remaining; }
            set { remaining = value; }
        }
    }

    [Serializable()]
    public class MOBFlightViewAirborne
    {
        public MOBFlightViewAirborne()
            : base()
        {
        }
        private string altitude = string.Empty;

        public string Altitude
        {
            get { return altitude; }
            set { altitude = value; }
        }
        private string speed = string.Empty;

        public string Speed
        {
            get { return speed; }
            set { speed = value; }
        }
        private string heading = string.Empty;

        public string Heading
        {
            get { return heading; }
            set { heading = value; }
        }
    }
}
