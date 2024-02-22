using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Emp.Shopping
{
    [Serializable()]
    public class MOBEmpSHOPFlattenedFlight
    {
        private string tripId = string.Empty;
        private string flightId = string.Empty;
        private string productId = string.Empty;
        private string tripDays = string.Empty;
        private string cabinMessage = string.Empty;
        private List<MOBEmpSHOPFlight> flights;

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

        public string CabinMessage
        {
            get
            {
                return this.cabinMessage;
            }
            set
            {
                this.cabinMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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

        public List<MOBEmpSHOPFlight> Flights
        {
            get
            {
                return this.flights;
            }
            set
            {
                this.flights = value;
            }
        }

        public string TripDays
        {
            get
            {
                return this.tripDays;
            }
            set
            {
                this.tripDays = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
