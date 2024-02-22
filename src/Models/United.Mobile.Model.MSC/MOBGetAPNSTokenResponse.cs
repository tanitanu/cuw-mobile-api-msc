using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBGetAPNSTokenResponse : MOBResponse
    {
        private MOBGetAPNSTokenRequest request;
        private string apnsToken = string.Empty;

        public MOBGetAPNSTokenResponse()
            : base()
        {
        }


        public MOBGetAPNSTokenRequest Request
        {
            get
            {
                return this.request;
            }
            set
            {
                this.request = value;
            }
        }

        public string APNSToken
        {
            get
            {
                return apnsToken;
            }
            set
            {
                this.apnsToken = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
