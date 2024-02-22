using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping
{
    [Serializable()]
    public class MOBSHOPAcquirePayPalTokenResponse : MOBResponse
    {
        private MOBSHOPAcquirePayPalTokenRequest request;
        private string sessionId = string.Empty;
        private string tokenID = string.Empty;
        public MOBSHOPAcquirePayPalTokenRequest Request
        {
            get { return request; }
            set { request = value; }
        }
        public string SessionId
        {
            get
            {
                return sessionId;
            }
            set
            {
                this.sessionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string TokenID
        {
            get
            {
                return tokenID;
            }
            set
            {
                this.tokenID = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
