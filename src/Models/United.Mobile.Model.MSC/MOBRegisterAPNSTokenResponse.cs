using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBRegisterAPNSTokenResponse : MOBResponse
    {
        public MOBRegisterAPNSTokenResponse()
            : base()
        {
        }

        private MOBRegisterAPNSTokenRequest request;

        public MOBRegisterAPNSTokenRequest Request
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

        private string succeed;

        public string Succeed
        {
            get
            {
                return this.succeed;
            }
            set
            {
                this.succeed = value;
            }
        }
    }
}
