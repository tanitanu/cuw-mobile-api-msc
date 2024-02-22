using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBFlightAddTripResponse : MOBResponse
    {
        //private MOBFlightAddTripRequest flightAddTripRequest;
        //private FlightAvailability flightAvailability;
        private string selectedTripIndexes = string.Empty;
        private List<MOBFlightAvailabilityTrip> selectedTrips;
        private List<MOBShopPrice> prices;
        private List<MOBShopTax> taxes;

        public MOBFlightAddTripResponse()
            : base()
        {
        }

        //public FlightAddTripRequest FlightAddTripRequest
        //{
        //    get
        //    {
        //        return this.flightAddTripRequest;
        //    }
        //    set
        //    {
        //        this.flightAddTripRequest = value;
        //    }
        //}

        //public FlightAvailability FlightAvailability
        //{
        //    get
        //    {
        //        return this.flightAvailability;
        //    }
        //    set
        //    {
        //        this.flightAvailability = value;
        //    }
        //}

        public string SelectedTripIndexes
        {
            get
            {
                return this.selectedTripIndexes;
            }
            set
            {
                this.selectedTripIndexes = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MOBFlightAvailabilityTrip> SelectedTrips
        {
            get { return this.selectedTrips; }
            set { this.selectedTrips = value; }
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

        public List<MOBShopTax> Taxes
        {
            get
            {
                return this.taxes;
            }
            set
            {
                this.taxes = value;
            }
        }

        private string footerMessage = null;

        public string FooterMessage
        {
            get { return footerMessage; }
            set { footerMessage = value; }
        }
    }
}
