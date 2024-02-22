using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition
{
    [Serializable()]
    public class MOBShopFlightInfo
    {
        private string actualArrivalDateTime = string.Empty;
        private string actualDepartureDateTime = string.Empty;
        private string estimatedArrivalDateTime = string.Empty;
        private string estimatedDepartureDateTime = string.Empty;
        private string minutesDelayed = string.Empty;
        private string scheduledArrivalDataTime = string.Empty;
        private string scheduledDepartureDateTime = string.Empty;
        private string statusCode = string.Empty;
        private string statusMessage = string.Empty;

        public string ActualArrivalDateTime
        {
            get
            {
                return this.actualArrivalDateTime;
            }
            set
            {
                this.actualArrivalDateTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ActualDepartureDateTime
        {
            get
            {
                return this.actualDepartureDateTime;
            }
            set
            {
                this.actualDepartureDateTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string EstimatedArrivalDateTime
        {
            get
            {
                return this.estimatedArrivalDateTime;
            }
            set
            {
                this.estimatedArrivalDateTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string EstimatedDepartureDateTime
        {
            get
            {
                return this.estimatedDepartureDateTime;
            }
            set
            {
                this.estimatedDepartureDateTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string MinutesDelayed
        {
            get
            {
                return this.minutesDelayed;
            }
            set
            {
                this.minutesDelayed = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ScheduledArrivalDataTime
        {
            get
            {
                return this.scheduledArrivalDataTime;
            }
            set
            {
                this.scheduledArrivalDataTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ScheduledDepartureDateTime
        {
            get
            {
                return this.scheduledDepartureDateTime;
            }
            set
            {
                this.scheduledDepartureDateTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string StatusCode
        {
            get
            {
                return this.statusCode;
            }
            set
            {
                this.statusCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string StatusMessage
        {
            get
            {
                return this.statusMessage;
            }
            set
            {
                this.statusMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
