using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Location
{
    [Serializable]
    public class MOBSITABeaconDetailRequest : MOBRequest
    {
        private string airportCode = string.Empty;
        private string uuid = string.Empty;
        private int major;
        private int minor;

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
    }
}
