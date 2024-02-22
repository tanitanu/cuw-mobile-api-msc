using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.FormofPayment
{
    [Serializable()]
    public class MOBInflightPurchaseFlightSegments
    {
        private bool isEligible;
        private string departureDate;
        private string flightNumber;
        private string arrivalAirport;
        private string departureAirport;
        private string carrierCode;
        private string classOfService; 
        private List<MOBInflightPurchaseTravelerInfo> travelersDetails;
        private bool isDBA;
        public bool IsEligible
        {
            get
            {
                return this.isEligible;
            }
            set
            {
                this.isEligible = value;
            }
        }
        public string DepartureDate
        {
            get
            {
                return this.departureDate;
            }
            set
            {
                this.departureDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
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
                this.flightNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
        public string ArrivalAirport
        {
            get
            {
                return this.arrivalAirport;
            }
            set
            {
                this.arrivalAirport = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
        public string DepartureAirport
        {
            get
            {
                return this.departureAirport;
            }
            set
            {
                this.departureAirport = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
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
        
        public string ClassOfService
        {
            get
            {
                return this.classOfService;
            }
            set
            {
                this.classOfService = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public List<MOBInflightPurchaseTravelerInfo> TravelersDetails
        {
            get
            {
                return this.travelersDetails;
            }
            set
            {
                this.travelersDetails = value;
            }
        }
        public bool IsDBA
        {
            get
            {
                return this.isDBA;
            }
            set
            {
                this.isDBA = value;
            }
        }

    }
}
