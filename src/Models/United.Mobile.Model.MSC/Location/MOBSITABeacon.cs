using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Location
{
    [Serializable]
    public class MOBSITABeacon
    {
        private string name = string.Empty;
        private string uuid = string.Empty;
        private int majorId;
        private int minorId;
        private string location = string.Empty;
        private int floor;
        private string terminal = string.Empty;
        private bool airside;
        private string lastUpdate = string.Empty;
        private string beaconType = string.Empty;
        private string latitude = string.Empty;
        private string longitude = string.Empty;
        private string altitude = string.Empty;
        private bool active;
        private bool publicBeacon;
        private MOBSITAMetaData metaData;
        private MOBSITAWalkInfo walkInfo;

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Uuid
        {
            get
            {
                return this.uuid;
            }
            set
            {
                this.uuid = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public int MajorId
        {
            get
            {
                return this.majorId;
            }
            set
            {
                this.majorId = value;
            }
        }

        public int MinorId
        {
            get
            {
                return this.minorId;
            }
            set
            {
                this.minorId = value;
            }
        }

        public string Location
        {
            get
            {
                return this.location;
            }
            set
            {
                this.location = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int Floor
        {
            get
            {
                return this.floor;
            }
            set
            {
                this.floor = value;
            }
        }

        public string Terminal
        {
            get
            {
                return this.terminal;
            }
            set
            {
                this.terminal = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool Airside
        {
            get
            {
                return this.airside;
            }
            set
            {
                this.airside = value;
            }
        }

        public string LastUpdate
        {
            get
            {
                return this.lastUpdate;
            }
            set
            {
                this.lastUpdate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string BeaconType
        {
            get
            {
                return this.beaconType;
            }
            set
            {
                this.beaconType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Latitude
        {
            get
            {
                return this.latitude;
            }
            set
            {
                this.latitude = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Longitude
        {
            get
            {
                return this.longitude;
            }
            set
            {
                this.longitude = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Altitude
        {
            get
            {
                return this.altitude;
            }
            set
            {
                this.altitude = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool Active
        {
            get
            {
                return this.active;
            }
            set
            {
                this.active = value;
            }
        }

        public bool PublicBeacon
        {
            get
            {
                return this.publicBeacon;
            }
            set
            {
                this.publicBeacon = value;
            }
        }

        public MOBSITAMetaData MetaData
        {
            get
            {
                return this.metaData;
            }
            set
            {
                this.metaData = value;
            }
        }

        public MOBSITAWalkInfo WalkInfo
        {
            get
            {
                return this.walkInfo;
            }
            set
            {
                this.walkInfo = value;
            }
        }
    }
}
