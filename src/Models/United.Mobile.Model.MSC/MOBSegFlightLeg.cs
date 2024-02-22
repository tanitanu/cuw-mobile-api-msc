using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition
{
    [Serializable()]
    public class MOBSegFlightLeg
    {
        private MOBTMAAirport arrivalAirport;
        private int cabinCount;
        private MOBTMAAirport departureAirport;
        private MOBComAircraft equipment;

        public MOBTMAAirport ArrivalAirport
        {
            get
            {
                return this.arrivalAirport;
            }
            set
            {
                this.arrivalAirport = value;
            }
        }

        public int CabinCount
        {
            get
            {
                return this.cabinCount;
            }
            set
            {
                this.cabinCount = value;
            }
        }

        public MOBTMAAirport DepartureAirport
        {
            get
            {
                return this.departureAirport;
            }
            set
            {
                this.departureAirport = value;
            }
        }

        public MOBComAircraft Equipment
        {
            get
            {
                return this.equipment;
            }
            set
            {
                this.equipment = value;
            }
        }
    }
}
