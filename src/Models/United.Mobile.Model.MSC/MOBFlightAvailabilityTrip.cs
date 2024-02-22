using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable()]
    public class MOBFlightAvailabilityTrip
    {
        private string origin = string.Empty;
        private string destination = string.Empty;
        //private Fare fare;
        //private List<Reward> rewards;
        private string avgFare;
        private string limitedSeats = string.Empty;
        private List<MOBFlightAvailabilitySegment> segments;

        public string Origin
        {
            get
            {
                return this.origin;
            }
            set
            {
                this.origin = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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

        //public Fare Fare
        //{
        //    get
        //    {
        //        return this.fare;
        //    }
        //    set
        //    {
        //        this.fare = value;
        //    }
        //}

        //public List<Reward> Rewards
        //{
        //    get
        //    {
        //        return this.rewards;
        //    }
        //    set
        //    {
        //        this.rewards = value;
        //    }
        //}

        public string AvgFare
        {
            get
            {
                return this.avgFare;
            }
            set
            {
                this.avgFare = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); ;
            }
        }


        public string LimitedSeats
        {
            get
            {
                return this.limitedSeats;
            }
            set
            {
                this.limitedSeats = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); ;
            }
        }

        public List<MOBFlightAvailabilitySegment> Segments
        {
            get
            {
                return this.segments;
            }
            set
            {
                this.segments = value;
            }
        }
    }
}
