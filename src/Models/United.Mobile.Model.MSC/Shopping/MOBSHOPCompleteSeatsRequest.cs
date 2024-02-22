﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping
{
    [Serializable()]
    public class MOBSHOPCompleteSeatsRequest : MOBRequest
    {
        private string sessionId = string.Empty;

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

        private string seatAssignment = string.Empty;
        private string origin = string.Empty;
        private string destination = string.Empty;
        private string paxIndex = string.Empty;
        private string sponsorMPAccountId = string.Empty;
        private string sponsorEliteLevel = string.Empty;

        public string SponsorMPAccountId
        {
            get
            {
                return this.sponsorMPAccountId;
            }
            set
            {
                this.sponsorMPAccountId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string SponsorEliteLevel
        {
            get
            {
                return this.sponsorEliteLevel;
            }
            set
            {
                this.sponsorEliteLevel = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string SeatAssignment
        {
            get
            {
                return this.seatAssignment;
            }
            set
            {
                this.seatAssignment = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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

        public string PaxIndex
        {
            get
            {
                return this.paxIndex;
            }
            set
            {
                this.paxIndex = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

    }
}
