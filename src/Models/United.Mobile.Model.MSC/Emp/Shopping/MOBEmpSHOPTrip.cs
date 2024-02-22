using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace United.Definition.Emp.Shopping
{
    [Serializable()]
    public class MOBEmpSHOPTrip : MOBEmpSHOPTripBase
    {
        private string tripId = string.Empty;
        private string originDecoded = string.Empty;
        private string destinationDecoded = string.Empty;
        private int flightCount;
        private int totalFlightCount;
        private List<MOBEmpSHOPFlattenedFlight> flattenedFlights;
        private List<MOBEmpSHOPFlightSection> flightSections;
        private int lastTripIndexRequested;
        private List<MOBEmpSHOPShoppingProduct> columns;
        private string yqyrMessage = string.Empty;
        private int pageCount;
        private string flightDateChangeMessage = ConfigurationManager.AppSettings["FlightDateChangeMessage"].ToString();
        private string callDurationText = string.Empty;

        public string YqyrMessage
        {
            get { return yqyrMessage; }
            set { yqyrMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

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

        public string OriginDecoded
        {
            get
            {
                return this.originDecoded;
            }
            set
            {
                this.originDecoded = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string DestinationDecoded
        {
            get
            {
                return this.destinationDecoded;
            }
            set
            {
                this.destinationDecoded = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MOBEmpSHOPFlattenedFlight> FlattenedFlights
        {
            get
            {
                return this.flattenedFlights;
            }
            set
            {
                this.flattenedFlights = value;
            }
        }

        public List<MOBEmpSHOPFlightSection> FlightSections
        {
            get
            {
                return this.flightSections;
            }
            set
            {
                this.flightSections = value;
            }
        }

        public int FlightCount
        {
            get
            {
                return this.flightCount;
            }
            set
            {
                this.flightCount = value;
            }
        }

        public int TotalFlightCount
        {
            get
            {
                return this.totalFlightCount;
            }
            set
            {
                this.totalFlightCount = value;
            }
        }

        public int LastTripIndexRequested
        {
            get
            {
                return this.lastTripIndexRequested;
            }
            set
            {
                this.lastTripIndexRequested = value;
            }
        }

        public List<MOBEmpSHOPShoppingProduct> Columns
        {
            get
            {
                return this.columns;
            }
            set
            {
                this.columns = value;
            }
        }

        
        public string CallDurationText
        {
            get { return callDurationText; }
            set { callDurationText = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public int PageCount
        {
            get
            {
                return this.pageCount;
            }
            set
            {
                this.pageCount = value;
            }
        }

        
        //public string FlightDateChangeMessage
        //{
        //    get { return this.flightDateChangeMessage; }
        //    set { }
        //}

    }
}
