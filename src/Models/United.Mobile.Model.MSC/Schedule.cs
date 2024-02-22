using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace United.Definition
{
    [Serializable()]
    public class Schedule
    {
        private string message = string.Empty;
        private string origin = string.Empty;
        private string destination = string.Empty;
        private string date = string.Empty;
        private short days;
        private string[] operationDays = new string[7];
        private List<ScheduleTrip> trips = new List<ScheduleTrip>();
        private TripOperationSchedule[] tripOperationSchedule;
        private AirportAdvisoryMessage airportAdvisoryMessage;

        public string Message
        {
            get
            {
                return this.message;
            }
            set
            {
                this.message = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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

        [XmlIgnore]
        public short Days 
        {
            get
            {
                return this.days;
            }
            set
            {
                this.days = value;
            }
        }

        public List<ScheduleTrip> Trips 
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

        public TripOperationSchedule[] TripOperationSchedule
        {
            get
            {
                return this.tripOperationSchedule;
            }
            set
            {
                this.tripOperationSchedule = value;
            }
        }

        public AirportAdvisoryMessage AirportAdvisoryMessage
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
