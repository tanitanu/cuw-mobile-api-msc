using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Emp.Shopping
{
    [Serializable()]
    public class MOBEmpSHOPAvailability
    {
        private string sessionId = string.Empty;
        private string cartId = string.Empty;
        private decimal closeInBookingFee;
        private List<MOBEmpSHOPFareWheelItem> fareWheel; 
        private MOBEmpSHOPTrip trip;
        private MOBEmpSHOPReservation reservation;
        private MOBEmpSHOPFareLock fareLock;
        private MOBEmpSHOPAwardCalendar awardCalendar;
        private string callDuration = string.Empty;

        private bool awardTravel = false;
        public bool AwardTravel
        {
            get
            {
                return this.awardTravel;
            }
            set
            {
                this.awardTravel = value;
            }
        }

        public string SessionId
        {
            get
            {
                return this.sessionId;
            }
            set
            {
                this.sessionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string CartId
        {
            get
            {
                return this.cartId;
            }
            set
            {
                this.cartId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public decimal CloseInBookingFee
        {
            get
            {
                return this.closeInBookingFee;
            }
            set
            {
                this.closeInBookingFee = value;
            }
        }

        public List<MOBEmpSHOPFareWheelItem> FareWheel
        {
            get
            {
                return this.fareWheel;
            }
            set
            {
                this.fareWheel = value;
            }
        }

        public MOBEmpSHOPTrip Trip
        {
            get
            {
                return this.trip;
            }
            set
            {
                this.trip = value;
            }
        }

        public MOBEmpSHOPReservation Reservation
        {
            get
            {
                return this.reservation;
            }
            set
            {
                this.reservation = value;
            }
        }

        public MOBEmpSHOPFareLock FareLock
        {
            get
            {
                return this.fareLock;
            }
            set
            {
                this.fareLock = value;
            }
        }

        public MOBEmpSHOPAwardCalendar AwardCalendar
        {
            get
            {
                return this.awardCalendar;
            }
            set
            {
                this.awardCalendar = value;
            }
        }

        public string CallDuration
        {
            get
            {
                return this.callDuration;
            }
            set
            {
                this.callDuration = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
