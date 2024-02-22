using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable]
    public class MOBStandbyList
    {
        private MOBSegment segment;
        private List<MOBTypeOption> cabinBookingStatusList;
        private List<MOBLDPassenger> cleared;
        private List<MOBLDPassenger> standby;
        private string departureAirportTimeStamp = string.Empty;
        private bool departed;

        public MOBSegment Segment
        {
            get
            {
                return this.segment;
            }
            set
            {
                this.segment = value;
            }
        }

        public List<MOBTypeOption> CabinBookingStatusList
        {
            get
            {
                return this.cabinBookingStatusList;
            }
            set
            {
                this.cabinBookingStatusList = value;
            }
        }

        public List<MOBLDPassenger> Cleared
        {
            get
            {
                return this.cleared;
            }
            set
            {
                this.cleared = value;
            }
        }

        public List<MOBLDPassenger> Standby
        {
            get
            {
                return this.standby;
            }
            set
            {
                this.standby = value;
            }
        }

        public string DepartureAirportTimeStamp
        {
            get
            {
                return this.departureAirportTimeStamp;
            }
            set
            {
                this.departureAirportTimeStamp = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool Departed
        {
            get
            {
                return departed;
            }
            set
            {
                this.departed = value;
            }
        }
    }
}
