using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition
{
    [Serializable()]
    public class MOBShopTrip
    {
        private string bbxCellIdSelected = string.Empty;
        private string bbxSession = string.Empty;
        private string bbxSolutionSetId = string.Empty;
        private string cabinType = string.Empty;
        private string departDate = string.Empty;
        private string departTime = string.Empty;
        private string destination = string.Empty;
        private string destinationDecoded = string.Empty;
        private int flightCount;
        private List<MOBShopFlight> flights;
        private string origin = string.Empty;
        private string originDecoded = string.Empty;
        private bool selected;
        private List<MOBShopFlattenedFlight> flattenedFlights;

        public string BBXCellIdSelected
        {
            get
            {
                return this.bbxCellIdSelected;
            }
            set
            {
                this.bbxCellIdSelected = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string BBXSession
        {
            get
            {
                return this.bbxSession;
            }
            set
            {
                this.bbxSession = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string BBXSolutionSetId
        {
            get
            {
                return this.bbxSolutionSetId;
            }
            set
            {
                this.bbxSolutionSetId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string CabinType
        {
            get
            {
                return this.cabinType;
            }
            set
            {
                this.cabinType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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

        public string DepartTime
        {
            get
            {
                return this.departTime;
            }
            set
            {
                this.departTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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
                this.destination = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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

        public List<MOBShopFlight> Flights
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

        public bool Selected
        {
            get
            {
                return this.selected;
            }
            set
            {
                this.selected = value;
            }
        }

        public List<MOBShopFlattenedFlight> FlattenedFlights
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
    }
}
