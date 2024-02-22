using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable()]
    public class MOBFlifoCitySearchSchedule
    {
        private string date = string.Empty;
        private List<MOBFlifoCitySearchTrip> trips = new List<MOBFlifoCitySearchTrip>();
        private MOBAirportAdvisoryMessage airportAdvisoryMessage;

        public string Date
        {
            get
            {
                return this.date;
            }
            set
            {
                this.date = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public List<MOBFlifoCitySearchTrip> Trips
        {
            get
            {
                return this.trips;
            }
            set
            {
                this.trips = value;
            }
        }

        public MOBAirportAdvisoryMessage AirportAdvisoryMessage
        {
            get
            {
                return this.airportAdvisoryMessage;
            }
            set
            {
                this.airportAdvisoryMessage = value;
            }
        }    
    }
}
