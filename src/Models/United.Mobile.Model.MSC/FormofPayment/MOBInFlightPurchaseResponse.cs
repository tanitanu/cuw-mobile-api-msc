using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.FormofPayment
{
    [Serializable()]
    public class MOBInFlightPurchaseResponse : MOBResponse
    {
        private string flow;
        private string state;
        private bool hideInflightPurchaseSection;
        private bool isEligibleToAddNewCCForInflightPurchase;
        private string publicDispenserKey;
        private List<MOBSavedCCInflightPurchase> savedCreditcardsInfo;
        private List<MOBInflightPurchaseFlightSegments> flightSegments;
        private List<MOBItem> captions;
        private List<string> attentionMessages;
        private string pnr;
        private string pnrType;
        private bool allowPaxSelection;
        private string eligibleSegments;
        private List<MOBInflightProductInfo> productInfo;
        private List<MOBItem> billingAddressCountries;
        private List<MOBCPBillingCountry> billingAddressProperties;


        public string Flow
        {
            get { return this.flow; }
            set { this.flow = value; }
        }

        public string State
        {
            get { return this.state; }
            set { this.state = value; }
        }

        public bool HideInflightPurchaseSection
        {
            get
            {
                return this.hideInflightPurchaseSection;
            }
            set
            {
                this.hideInflightPurchaseSection = value;
            }
        }
        public bool IsEligibleToAddNewCCForInflightPurchase
        {
            get
            {
                return this.isEligibleToAddNewCCForInflightPurchase;
            }
            set
            {
                this.isEligibleToAddNewCCForInflightPurchase = value;
            }
        }
        public string PublicDispenserKey
        {
            get
            {
                return this.publicDispenserKey;
            }
            set
            {
                this.publicDispenserKey = value;
            }
        }
        public List<MOBSavedCCInflightPurchase> SavedCreditcardsInfo
        {
            get
            {
                return this.savedCreditcardsInfo;
            }
            set
            {
                this.savedCreditcardsInfo = value;
            }
        }
        public List<MOBInflightPurchaseFlightSegments> FlightSegments
        {
            get
            {
                return this.flightSegments;
            }
            set
            {
                this.flightSegments = value;
            }

        }
        public List<MOBItem> Captions
        {
            get { return captions; }
            set { captions = value; }
        }

        public List<string> AttentionMessages
        {
            get
            {
                return this.attentionMessages;
            }
            set
            {
                this.attentionMessages = value;
            }
        }

        public string Pnr
        {
            get { return pnr; }
            set { pnr = value; }
        }

        public string PnrType
        {
            get { return pnrType; }
            set { pnrType = value; }
        }

        public bool AllowPaxSelection
        {
            get { return this.allowPaxSelection; }
            set { this.allowPaxSelection = value; }
        }

        public string EligibleSegments
        {
            get { return this.eligibleSegments; }
            set { this.eligibleSegments = value; }
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

        public List<MOBItem> BillingAddressCountries
        {
            get
            {
                return this.billingAddressCountries;
            }
            set
            {
                this.billingAddressCountries = value;
            }
        }

        public List<MOBCPBillingCountry> BillingAddressProperties
        {
            get
            {
                return this.billingAddressProperties;
            }
            set
            {
                this.billingAddressProperties = value;
            }
        }        
    }
}
