using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBScheduleRequest : MOBRequest
    {

        public MOBScheduleRequest(string Origin, string Destination, string Date,
                               short Days, string FlightType)
        {
            this.Origin = Origin;
            this.Destination = Destination;
            this.Date = Date;
            this.Days = Days;
            this.FlightType = FlightType;
        }

        public MOBScheduleRequest()
            : base()
        {
        }


        private string origin = string.Empty;
        private string destination = string.Empty;
        private string date = string.Empty;
        private short days;
        private string flightType = string.Empty;


        public string Origin
        {
            get { return this.origin; }
            set
            {
                this.origin = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string Destination
        {
            get { return this.destination; }
            set
            {
                this.destination = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string Date
        {
            get { return this.date; }
            set
            {
                this.date = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public short Days
        {
            get
            {
                return days;
            }
            set
            {
                days = value;
            }
        }

        public string FlightType
        {
            get
            {
                return this.flightType;
            }
            set
            {
                this.flightType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
