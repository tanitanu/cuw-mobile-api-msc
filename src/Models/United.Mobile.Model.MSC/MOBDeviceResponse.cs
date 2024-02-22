using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable]
    public class MOBDeviceResponse : MOBResponse
    {
        private string guid = string.Empty;
        private int deviceID;

        public MOBDeviceResponse()
            : base()
        {
        }

        public string GUID
        {
            get
            {
                return this.guid;
            }
            set
            {
                this.guid = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int DeviceID
        {
            get
            {
                return this.deviceID;
            }
            set
            {
                this.deviceID = value;
            }
        }

    }
}
