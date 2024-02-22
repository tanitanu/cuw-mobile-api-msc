using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    public class MOBSpatialLocationRequest : MOBResponse
    {
        MOBApplication application;
        private string deviceId = string.Empty;
        private float latitude;
        private float longitude;


        public MOBApplication Application
        {
            get
            {
                return this.application;
            }
            set
            {
                this.application = value;
            }
        }


        public string DeviceId
        {
            get
            {
                return this.deviceId;
            }
            set
            {
                this.deviceId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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
