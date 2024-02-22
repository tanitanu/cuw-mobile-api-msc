using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace United.Definition.Emp
{
    [Serializable()]
    public class MOBEmpFlightPBTRequest : MOBEmpRequest 
    {
        private string origin;
        private string destination;
        private string departureDate;
        private string flightNumber;
        private string marketingCarrier;
        private string messageFormat;

        public string Origin
        {
            get { return this.origin; }
            set { this.origin = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string Destination
        {
            get { return this.destination; }
            set { this.destination = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string DepartureDate
        {
            get { return this.departureDate; }
            set { this.departureDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string FlightNumber
        {
            get { return this.flightNumber; }
            set { this.flightNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string MarketingCarrier
        {
            get { return this.marketingCarrier; }
            set { this.marketingCarrier = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string MessageFormat
        {
            get { return this.messageFormat; }
            set { this.messageFormat = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
    }
}
