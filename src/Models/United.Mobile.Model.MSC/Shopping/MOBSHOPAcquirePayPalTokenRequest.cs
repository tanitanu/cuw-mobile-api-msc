using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping
{
    [Serializable()]
    public class MOBSHOPAcquirePayPalTokenRequest : MOBRequest
    {
        private string sessionId = string.Empty;
        private double amount = 0.0;
        private string cancelURL = string.Empty;
        private string returnURL = string.Empty;
        private string key       = string.Empty;
        private string countryCode = string.Empty;
        private string mileagePlusNumber;

        public string MileagePlusNumber
        {
            get { return mileagePlusNumber; }
            set { mileagePlusNumber = value; }
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
       
        public double Amount
        {
            get
            {
                return amount;
            }
            set
            {
                this.amount = value;
            }
        }
        public string CountryCode
        {
            get
            {
                return countryCode;
            }
            set
            {
                this.countryCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string CancelURL
        {
            get
            {
                return cancelURL;
            }
            set
            {
                this.cancelURL = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string ReturnURL
        {
            get
            {
                return returnURL;
            }
            set
            {
                this.returnURL = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string Key
        {
            get
            {
                return key;
            }
            set
            {
                this.key = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
