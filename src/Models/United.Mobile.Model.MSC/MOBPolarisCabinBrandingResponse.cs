using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    public class MOBPolarisCabinBrandingResponse:MOBResponse
    {
        private string flightNumber = string.Empty;
        private string flightDate = string.Empty;
        private string departureAirportCode = string.Empty;
        private string operatingCarrier = string.Empty;
        private string arrivalAirportCode = string.Empty;
        private string cabinCount = string.Empty;
        private string marketingCarrier = string.Empty;
        private List<string> cabinBrandings = new List<string>();
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

        public string DepartureAirportCode
        {
            get
            {
                return this.departureAirportCode;
            }
            set
            {
                this.departureAirportCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string OperatingCarrier
        {
            get
            {
                return operatingCarrier;
            }
            set
            {
                this.operatingCarrier = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string ArrivalAirportCode
        {
            get
            {
                return arrivalAirportCode;
            }
            set
            {
                this.arrivalAirportCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string CabinCount
        {
            get
            {
                return cabinCount;
            }
            set
            {
                this.cabinCount = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string MarketingCarrier
        {
            get
            {
                return marketingCarrier;
            }
            set
            {
                this.marketingCarrier = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public List<string> CabinBrandings
        {
            get { return cabinBrandings; }
            set { cabinBrandings = value; }
        }
    }
}
