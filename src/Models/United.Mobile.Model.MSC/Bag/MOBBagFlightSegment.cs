using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition.Bag
{
    [Serializable]
    public class MOBBagFlightSegment
    {
        private string arrivalRouteTypeCode = string.Empty;
        private MOBBagAction bagAction;
        private United.Definition.Bag.MOBBagStatus bagStatus;
        private string departureRouteTypeCode = string.Empty;
        private string flightOriginationDate = string.Empty;
        private MOBSegment segment; 



        public string ArrivalRouteTypeCode
        {
            get
            {
                return arrivalRouteTypeCode;
            }
            set
            {
                this.arrivalRouteTypeCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public MOBBagAction BagAction
        {
            get
            {
                return bagAction;
            }
            set
            {
                this.bagAction = value;
            }
        }

        public United.Definition.Bag.MOBBagStatus BagStatus
        {
            get
            {
                return bagStatus;
            }
            set
            {
                this.bagStatus = value;
            }
        }

        public string DepartureRouteTypeCode
        {
            get
            {
                return departureRouteTypeCode;
            }
            set
            {
                this.departureRouteTypeCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string FlightOriginationDate
        {
            get
            {
                return flightOriginationDate;
            }
            set
            {
                this.flightOriginationDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public MOBSegment Segment
        {
            get
            {
                return segment;
            }
            set
            {
                this.segment = value;
            }
        }
    }
}
