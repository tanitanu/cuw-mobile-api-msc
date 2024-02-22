using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Emp.Shopping
{
    [Serializable()]
    public class MOBEmpSHOPTripBase
    {
        private string origin = string.Empty;
        private string destination = string.Empty;
        private string departDate = string.Empty;
        private string arrivalDate = string.Empty;
        private string cabin = string.Empty;
        private bool useFilters = false;
        private MOBEmpSHOPSearchFilters searchFiltersIn;
        private MOBEmpSHOPSearchFilters searchFiltersOut;

        private bool searchNearbyOriginAirports = false;
        private bool searchNearbyDestinationAirports = false;
        private string shareMessage = string.Empty;

        public string Origin
        {
            get
            {
                return this.origin;
            }
            set
            {
                this.origin = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string Destination
        {
            get
            {
                return this.destination;
            }
            set
            {
                this.destination = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string DepartDate
        {
            get
            {
                return this.departDate;
            }
            set
            {
                this.departDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ArrivalDate
        {
            get
            {
                return this.arrivalDate;
            }
            set
            {
                this.arrivalDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Cabin
        {
            get
            {
                return this.cabin;
            }
            set
            {
                this.cabin = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool UseFilters
        {
            get
            {
                return this.useFilters;
            }
            set
            {
                this.useFilters = value;
            }
        }

        public MOBEmpSHOPSearchFilters SearchFiltersIn
        {
            get
            {
                return this.searchFiltersIn;
            }
            set
            {
                this.searchFiltersIn = value;
            }
        }

        public MOBEmpSHOPSearchFilters SearchFiltersOut
        {
            get
            {
                return this.searchFiltersOut;
            }
            set
            {
                this.searchFiltersOut = value;
            }
        }

        public bool SearchNearbyOriginAirports
        {
            get
            {
                return this.searchNearbyOriginAirports;
            }
            set
            {
                this.searchNearbyOriginAirports = value;
            }
        }

        public bool SearchNearbyDestinationAirports
        {
            get
            {
                return this.searchNearbyDestinationAirports;
            }
            set
            {
                this.searchNearbyDestinationAirports = value;
            }
        }
        public string ShareMessage
        {
            get
            {
                return this.shareMessage;
            }
            set
            {
                this.shareMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
