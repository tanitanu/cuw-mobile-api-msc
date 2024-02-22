﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBFlightStatusNotificationResponse : MOBResponse
    {
        private string carrierCode = string.Empty;
        private string flightNumber = string.Empty;
        private string notificationDate = string.Empty;
        private string airportCode = string.Empty;
        private bool onDeparture;
        private bool onHour1;
        private bool onHour2;
        private bool onHour3;
        private bool onHour4;
        private string emailAddress = string.Empty;
        private string countryCode = string.Empty;
        private string extension = string.Empty;
        private bool sms;
        private string notificationMessage = string.Empty;
        private bool addedOnHour1;
        private bool addedOnHour2;
        private bool addedOnHour3;
        private bool addedOnHour4;
        private MOBFlightStatusSegment segment;

        public MOBFlightStatusNotificationResponse()
            : base()
        {
        }


        public string CarrierCode
        {
            get
            {
                return this.carrierCode;
            }
            set
            {
                this.carrierCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
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

        public string NotificationDate
        {
            get
            {
                return this.notificationDate;
            }
            set
            {
                this.notificationDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string AirportCode
        {
            get
            {
                return this.airportCode;
            }
            set
            {
                this.airportCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public bool OnDeparture
        {
            get
            {
                return onDeparture;
            }
            set
            {
                this.onDeparture = value;
            }
        }

        public bool OnHour1
        {
            get
            {
                return onHour1;
            }
            set
            {
                this.onHour1 = value;
            }
        }

        public bool OnHour2
        {
            get
            {
                return onHour2;
            }
            set
            {
                this.onHour2 = value;
            }
        }

        public bool OnHour3
        {
            get
            {
                return onHour3;
            }
            set
            {
                this.onHour3 = value;
            }
        }

        public bool OnHour4
        {
            get
            {
                return onHour4;
            }
            set
            {
                this.onHour4 = value;
            }
        }

        public string EmailAddress
        {
            get
            {
                return this.emailAddress;
            }
            set
            {
                this.emailAddress = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string CountryCode
        {
            get
            {
                return this.countryCode;
            }
            set
            {
                this.countryCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string Extension
        {
            get
            {
                return this.extension;
            }
            set
            {
                this.extension = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public bool SMS
        {
            get
            {
                return sms;
            }
            set
            {
                this.sms = value;
            }
        }

        public string NotificationMessage
        {
            get
            {
                return this.notificationMessage;
            }
            set
            {
                this.notificationMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public bool AddedOnHour1
        {
            get
            {
                return addedOnHour1;
            }
            set
            {
                this.addedOnHour1 = value;
            }
        }

        public bool AddedOnHour2
        {
            get
            {
                return addedOnHour2;
            }
            set
            {
                this.addedOnHour2 = value;
            }
        }

        public bool AddedOnHour3
        {
            get
            {
                return addedOnHour3;
            }
            set
            {
                this.addedOnHour3 = value;
            }
        }

        public bool AddedOnHour4
        {
            get
            {
                return addedOnHour4;
            }
            set
            {
                this.addedOnHour4 = value;
            }
        }

        public MOBFlightStatusSegment Segment
        {
            get
            {
                return segment;
            }
            set
            {
                this.segment = value;
            }
        }
    }
}
