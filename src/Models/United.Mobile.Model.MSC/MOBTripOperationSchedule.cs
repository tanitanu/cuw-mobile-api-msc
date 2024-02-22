using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition
{
    [Serializable()]
    public class MOBTripOperationSchedule
    {
        private string weekDay = string.Empty;
        private string date = string.Empty;
        private List<int> tripInxex;

        public string Weekday
        {
            get
            {
                return this.weekDay;
            }
            set
            {
                this.weekDay = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string Date
        {
            get
            {
                return this.date;
            }
            set
            {
                this.date = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<int> TripInxex
        {
            get
            {
                return this.tripInxex;
            }
            set
            {
                this.tripInxex = value;
            }
        }
    }
}
