using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBChangeAddressRequest : MOBRequest
    {
        private string sessionId;
        private MOBAddress mobAddress;
        private MOBEmail mobEmail;
        public MOBAddress MobAddress
        {
            get
            {
                return mobAddress;
            }
            set
            {
                mobAddress = value;
            }
        }

        public MOBEmail MobEmail
        {
            get
            {
                return mobEmail;
            }
            set
            {
                mobEmail = value;
            }
        }

        public string SessionId
        {
            get
            {
                return this.sessionId;
            }
            set
            {
                this.sessionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
