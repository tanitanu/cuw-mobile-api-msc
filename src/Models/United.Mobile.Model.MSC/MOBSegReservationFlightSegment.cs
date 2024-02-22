using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable()]
    public class MOBSegReservationFlightSegment
    {
        private MOBComBookingClass bookingClass;
        private MOBSegFlightSegment flightSegment;
        private string isConnection = string.Empty;
        private string otherAirlineRecordLocator = string.Empty;

        public MOBComBookingClass BookingClass
        {
            get
            {
                return this.bookingClass;
            }
            set
            {
                this.bookingClass = value;
            }
        }

        public MOBSegFlightSegment FlightSegment
        {
            get
            {
                return this.flightSegment;
            }
            set
            {
                this.flightSegment = value;
            }
        }

        public string IsConnection
        {
            get
            {
                return this.isConnection;
            }
            set
            {
                this.isConnection = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string OtherAirlineRecordLocator
        {
            get
            {
                return this.otherAirlineRecordLocator;
            }
            set
            {
                this.otherAirlineRecordLocator = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
