using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.FormofPayment
{
    [Serializable()]
    public class MOBSavedCCInflightPurchaseRequest : MOBInFlightPurchaseBaseRequest
    {
        private string state;
        private MOBFormofPaymentDetails formofPaymentDetails;
        private List<MOBItem> captions;
        private string verifyOption;
        private List<MOBInflightProductInfo> productInfo;
        private string sessionId = string.Empty;
        private int customerId;
        private string mileagePlusNumber = string.Empty;
        private bool updateInsertCreditCardInfo;
        private MOBCPTraveler traveler;
        private bool isSavedToProfile = false;
        public string State
        {
            get { return this.state; }
            set { this.state = value; }
        }
        public List<MOBItem> Captions
        {
            get { return captions; }
            set { captions = value; }
        }
        public MOBFormofPaymentDetails FormofPaymentDetails
        {
            get
            {
                return this.formofPaymentDetails;
            }
            set
            {
                this.formofPaymentDetails = value;
            }
        }
        public string VerifyOption
        {
            get { return this.verifyOption; }
            set { this.verifyOption = value; }
        }
        public List<MOBInflightProductInfo> ProductInfo
        {
            get
            {
                return this.productInfo;
            }
            set
            {
                this.productInfo = value;
            }
        }
        public bool IsSavedToProfile
        {
            get { return isSavedToProfile; }
            set { isSavedToProfile = value; }
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

        public int CustomerId
        {
            get
            {
                return customerId;
            }
            set
            {
                this.customerId = value;
            }
        }

        public MOBCPTraveler Traveler
        {
            get
            {
                return this.traveler;
            }
            set
            {
                this.traveler = value;
            }
        }
        public bool UpdateInsertCreditCardInfo
        {
            get { return this.updateInsertCreditCardInfo; }
            set { this.updateInsertCreditCardInfo = value; }
        }
    }
}
