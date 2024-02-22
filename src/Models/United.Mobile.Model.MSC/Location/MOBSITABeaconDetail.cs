using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Location
{
    [Serializable]
    public class MOBSITABeaconDetail
    {
        private string name = string.Empty;
        private string uuid = string.Empty;
        private int major;
        private int minor;
        private string location = string.Empty;
        private int floor;
        private string terminal = string.Empty;
        private bool airside;
        private string lastUpdate = string.Empty;
        private string beaconType = string.Empty;
        private MOBSITAWalkInfo walkInfo;
        private MOBSITAMetaData metaData;

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

        public int Major
        {
            get
            {
                return this.major;
            }
            set
            {
                this.major = value;
            }
        }

        public int Minor
        {
            get
            {
                return this.minor;
            }
            set
            {
                this.minor = value;
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
    }
}
