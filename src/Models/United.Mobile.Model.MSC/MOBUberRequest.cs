using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBUberRequest : MOBRequest
    {
        private string latitude = string.Empty;
        private string longitude = string.Empty;
        private string startlatitude = string.Empty;
        private string startlongitude = string.Empty;
        private string endlatitude = string.Empty;
        private string endlongitude = string.Empty;
        private string uberCallType = string.Empty;


        public MOBUberRequest()
            : base()
        {
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

        public string Startlatitude
        {
            get
            {
                return this.startlatitude;
            }
            set
            {
                this.startlatitude = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string Startlongitude
        {
            get
            {
                return this.startlongitude;
            }
            set
            {
                this.startlongitude = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string Endlatitude
        {
            get
            {
                return endlatitude;
            }
            set
            {
                this.endlatitude = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string Endlongitude
        {
            get
            {
                return endlongitude;
            }
            set
            {
                this.endlongitude = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string UberCallType
        {
            get
            {
                return uberCallType;
            }
            set
            {
                this.uberCallType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
    }
}
