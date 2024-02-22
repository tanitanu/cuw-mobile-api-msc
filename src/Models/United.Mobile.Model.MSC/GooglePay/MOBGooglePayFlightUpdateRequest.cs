using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.GooglePay
{
    public class MOBGooglePayFlightUpdateRequest : MOBRequest
    {
        private string eventType;
        private string flightNumber;
        private string flightDate;
        private string origin;
        private string destination;
        private string departureGate;
        private string recordLocator;
        private string departureTerminal;
        private string carrierCode;

        public string EventType
        {
            get { return eventType; }
            set { eventType = value; }
        }
        public string FlightNumber
        {
            get { return flightNumber; }
            set { flightNumber = value; }
        }
        public string FlightDate
        {
            get { return flightDate; }
            set { flightDate = value; }
        }
        public string Origin
        {
            get { return origin; }
            set { origin = value; }
        }
        public string Destination
        {
            get { return destination; }
            set { destination = value; }
        }
        public string DepartureGate
        {
            get { return departureGate; }
            set { departureGate = value; }
        }
        public string RecordLocator
        {
            get { return recordLocator; }
            set { recordLocator = value; }
        }
        public string DepartureTerminal
        {
            get { return departureTerminal; }
            set { departureTerminal = value; }
        }
        public string CarrierCode
        {
            get { return carrierCode; }
            set { carrierCode = value; }
        }
    }
}
