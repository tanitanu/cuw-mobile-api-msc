﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Emp.Common
{
    [Serializable]
    public class MOBEmpSeg
    {
        private MOBEmpAirline marketingCarrier;
        private string flightNumber = string.Empty;
        private MOBEmpAirport departure;
        private MOBEmpAirport arrival;
        private string scheduledDepartureDateTime = string.Empty;
        private string scheduledArrivalDateTime = string.Empty;

        private string scheduledDepartureTimeGMT = string.Empty;
        private string scheduledArrivalTimeGMT = string.Empty;

        private string formattedScheduledDepartureDateTime = string.Empty;
        private string formattedScheduledArrivalDateTime = string.Empty;

        private string formattedScheduledDepartureDate = string.Empty;
        private string formattedScheduledArrivalDate = string.Empty;

        private string formattedScheduledDepartureTime = string.Empty;
        private string formattedScheduledArrivalTime = string.Empty;

        public MOBEmpSeg()
        {
        }

        public MOBEmpAirline MarketingCarrier
        {
            get
            {
                return this.marketingCarrier;
            }
            set
            {
                this.marketingCarrier = value;
            }
        }

        public string FlightNumber
        {
            get
            {
                return this.flightNumber;
            }
            set
            {
                this.flightNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public MOBEmpAirport Departure
        {
            get
            {
                return this.departure;
            }
            set
            {
                this.departure = value;
            }
        }

        public MOBEmpAirport Arrival
        {
            get
            {
                return this.arrival;
            }
            set
            {
                this.arrival = value;
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

        public string ScheduledArrivalDateTime
        {
            get
            {
                return this.scheduledArrivalDateTime;
            }
            set
            {
                this.scheduledArrivalDateTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ScheduledDepartureTimeGMT
        {
            get
            {
                return this.scheduledDepartureTimeGMT;
            }
            set
            {
                this.scheduledDepartureTimeGMT = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ScheduledArrivalTimeGMT
        {
            get
            {
                return this.scheduledArrivalTimeGMT;
            }
            set
            {
                this.scheduledArrivalTimeGMT = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string FormattedScheduledDepartureDateTime
        {
            get
            {
                return this.formattedScheduledDepartureDateTime;
            }
            set
            {
                this.formattedScheduledDepartureDateTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string FormattedScheduledArrivalDateTime
        {
            get
            {
                return this.formattedScheduledArrivalDateTime;
            }
            set
            {
                this.formattedScheduledArrivalDateTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string FormattedScheduledDepartureDate
        {
            get
            {
                return this.formattedScheduledDepartureDate;
            }
            set
            {
                this.formattedScheduledDepartureDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string FormattedScheduledArrivalDate
        {
            get
            {
                return this.formattedScheduledArrivalDate;
            }
            set
            {
                this.formattedScheduledArrivalDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string FormattedScheduledDepartureTime
        {
            get
            {
                return this.formattedScheduledDepartureTime;
            }
            set
            {
                this.formattedScheduledDepartureTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string FormattedScheduledArrivalTime
        {
            get
            {
                return this.formattedScheduledArrivalTime;
            }
            set
            {
                this.formattedScheduledArrivalTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
