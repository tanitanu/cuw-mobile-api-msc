using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBRegisterAPNSTokenRequest : MOBRequest
    {
        public MOBRegisterAPNSTokenRequest()
            : base()
        {
        }

        private string deviceId = string.Empty;

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

        private string apnsToken = string.Empty;

        public string APNSToken
        {
            get
            {
                return this.apnsToken;
            }
            set
            {
                this.apnsToken = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
