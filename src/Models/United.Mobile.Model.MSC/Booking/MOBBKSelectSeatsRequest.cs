﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Booking
{
    [Serializable()]
    public class MOBBKSelectSeatsRequest : MOBRequest
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
        private string nextOrigin = string.Empty;
        private string nextDestination = string.Empty;

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

        public string NextOrigin
        {
            get
            {
                return this.nextOrigin;
            }
            set
            {
                this.nextOrigin = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string NextDestination
        {
            get
            {
                return this.nextDestination;
            }
            set
            {
                this.nextDestination = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
