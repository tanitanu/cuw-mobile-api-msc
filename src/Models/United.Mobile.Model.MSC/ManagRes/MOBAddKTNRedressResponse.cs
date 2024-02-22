﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.ManagRes
{
    [Serializable()]
    public class MOBAddKTNRedressResponse : MOBResponse
    {
        private string sessionId;
        private string knowntravellernumber;
        private string redressNumber;
        private MOBName passengerName;

        public string SessionId
        {
            get
            {
                return this.sessionId;
            }
            set
            {
                this.sessionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string KnownTravellerNumber
        {
            get
            {
                return this.knowntravellernumber;
            }
            set
            {
                this.knowntravellernumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string RedressNumber
        {
            get
            {
                return this.redressNumber;
            }
            set
            {
                this.redressNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public MOBName PassengerName
        {
            get
            {
                return this.passengerName;
            }
            set
            {
                this.passengerName = value;
            }
        }

    }
}
