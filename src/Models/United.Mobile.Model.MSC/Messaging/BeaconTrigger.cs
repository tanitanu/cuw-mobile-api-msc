using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Messaging
{
    [Serializable]
    public class BeaconTrigger
    {
        private string guid = string.Empty;
        private int major;
        private int minor;
        private string proximity = string.Empty;
        private float latitude;
        private float longitude;

        public string Guid
        {
            get
            {
                return this.guid;
            }
            set
            {
                this.guid = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
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

        public string Proximity
        {
            get
            {
                return this.proximity;
            }
            set
            {
                this.proximity = value;
            }
        }

        public float Latitude
        {
            get
            {
                return this.latitude;
            }
            set
            {
                this.latitude = value;
            }
        }

        public float Longitude
        {
            get
            {
                return this.longitude;
            }
            set
            {
                this.longitude = value;
            }
        }
    }
}
