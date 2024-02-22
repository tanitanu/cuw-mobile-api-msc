using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBGetAPNSTokenRequest : MOBRequest
    {
        private string deviceId = string.Empty;

        public MOBGetAPNSTokenRequest()
            : base()
        {
        }

        public string DeviceId
        {
            get
            {
                return deviceId;
            }
            set
            {
                this.deviceId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
