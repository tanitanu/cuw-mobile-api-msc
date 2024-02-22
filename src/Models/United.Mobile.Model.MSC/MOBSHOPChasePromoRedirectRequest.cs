using System;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBSHOPChasePromoRedirectRequest: MOBRequest
    {
        private string sessionId = string.Empty;
        private string promoType = string.Empty;
        private string hashPinCode = string.Empty;
        private string mileagePlusNumber = string.Empty;

        public string SessionId
        {
            get { return sessionId; }
            set { sessionId = value; }
        }

        public string PromoType
        {
            get { return promoType; }
            set { promoType = value; }
        }

        public string MileagePlusNumber
        {
            get
            {
                return mileagePlusNumber;
            }
            set
            {
                this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }


        public string HashPinCode
        {
            get
            {
                return hashPinCode;
            }
            set
            {
                this.hashPinCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}