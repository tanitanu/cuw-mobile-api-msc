using System;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping
{
    [Serializable()]
    public class MOBSHOPAcquireMasterpassTokenResponse : MOBResponse
    {
        private string sessionId = string.Empty;
        private string cslSessionId = string.Empty;
        private string tokenID = string.Empty;
        private string allowedCardTypes = string.Empty;
        private string merchantCheckoutID = string.Empty;
        private string requestToken = string.Empty;
        private MOBSHOPAcquireMasterpassTokenRequest request;

        public MOBSHOPAcquireMasterpassTokenRequest Request
        {
            get { return request; }
            set { this.request = value; }
        }

        public string SessionId
        {
            get { return sessionId; }
            set { this.sessionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string CslSessionId
        {
            get { return cslSessionId; }
            set { this.cslSessionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        } 
        public string TokenID
        {
            get { return tokenID; }
            set { this.tokenID = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string AllowedCardTypes
        {
            get { return allowedCardTypes; }
            set { this.allowedCardTypes = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string MerchantCheckoutID
        {
            get { return merchantCheckoutID; }
            set { this.merchantCheckoutID = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string RequestToken
        {
            get { return requestToken; }
            set { this.requestToken = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
    }
}
