﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable]
    public class MOBLocationEvent
    {
        private string mileagePlusNumber = string.Empty;
        private string recordLocator = string.Empty;
        private string origin = string.Empty;
        private string destination = string.Empty;
        private List<MOBFenceEvent> fenceEvents;
        private List<MOBBeaconEvent> beaconEvents;

        public string MileagePlusNumber
        {
            get { return mileagePlusNumber; }
            set
            {
                this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string RecordLocator
        {
            get { return recordLocator; }
            set
            {
                this.recordLocator = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string Origin
        {
            get { return origin; }
            set
            {
                this.origin = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string Destination
        {
            get { return destination; }
            set
            {
                this.destination = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public List<MOBFenceEvent> FenceEvents
        {
            get
            {
                return this.fenceEvents;
            }
            set
            {
                this.fenceEvents = value;
            }
        }

        public List<MOBBeaconEvent> BeaconEvents
        {
            get
            {
                return this.beaconEvents;
            }
            set
            {
                this.beaconEvents = value;
            }
        }
    }
}
