﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBFlightStatusPushNotificationRequest : MOBRequest
    {
        public MOBFlightStatusPushNotificationRequest()
            : base()
        {
        }

        private string apnsDeviceId = string.Empty;
        private string carrierCode = string.Empty;
        private string flightNumber = string.Empty;
        private string flightDate = string.Empty;
        private string origin = string.Empty;
        private string destination = string.Empty;

        public string APNSDeviceId
        {
            get
            {
                return this.apnsDeviceId;
            }
            set
            {
                this.apnsDeviceId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
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

        public string FlightDate
        {
            get
            {
                return this.flightDate;
            }
            set
            {
                this.flightDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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
    }
}
