using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using United.Definition.Accelerators;
using United.Definition.Pcu;
using United.Definition.Shopping.TripInsurance;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable]
    public class MOBPremierAccessCompleteResponse : MOBResponse
    {
        private MOBPACompletePurchaseRequest request;
        private bool isSaveSelectedCustomerSegment;
        private string selectedCustomerInSegments = string.Empty;
        private string recordLocator = string.Empty;
        private string lastName = string.Empty;

        private List<string> paPurchaseCompleteMessages;
        private MOBPNR pnr;
        private List<string> dotBagRules;
        private MOBDOTBaggageInfo dotBaggageInformation;
        private bool showSeatChange;
        private MOBPremierAccess premierAccess;
        private bool showPremierAccess;
        //Added by Nizam - #179873 - 07/26/2017
        private string pkDispenserPublicKey;
        private string sessionGuID;
        // All properties in MOBPNRByRecordLocatorResponse should also be here, since iOS is using MOBPNRByRecordLocatorResponse as CompletePremierAccessSelectionECC response
        private MOBTPIInfo tripInsuranceInfo;
        private MOBAncillary ancillary;

        public MOBPACompletePurchaseRequest Request
        {
            get { return request; }
            set { request = value; }
        }

        public bool IsSaveSelectedCustomerSegment
        {
            get
            {
                return this.isSaveSelectedCustomerSegment;
            }
            set
            {
                this.isSaveSelectedCustomerSegment = value;
            }
        }

        public string SelectedCustomerInSegments
        {
            get { return this.selectedCustomerInSegments; }
            set { this.selectedCustomerInSegments = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string RecordLocator
        {
            get
            {
                return this.recordLocator;
            }
            set
            {
                this.recordLocator = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string LastName
        {
            get
            {
                return this.lastName;
            }
            set
            {
                this.lastName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public List<string> PAPurchaseCompleteMessages
        {
            get
            {
                return this.paPurchaseCompleteMessages;
            }
            set
            {
                this.paPurchaseCompleteMessages = value;
            }
        }

        public MOBPNR PNR
        {
            get
            {
                return this.pnr;
            }
            set
            {
                this.pnr = value;
            }
        }

        public List<string> DOTBagRules
        {
            get
            {
                string rText = System.Configuration.ConfigurationManager.AppSettings["DOTBagRules"];
                if (!string.IsNullOrEmpty(rText))
                {
                    string[] rules = rText.Split('|');
                    if (rules != null && rules.Length > 0)
                    {
                        this.dotBagRules = new List<string>();
                        foreach (string s in rules)
                        {
                            this.dotBagRules.Add(s);
                        }
                    }
                }

                return this.dotBagRules;
            }
            set
            {
                this.dotBagRules = value;
            }
        }

        public MOBDOTBaggageInfo DotBaggageInformation
        {
            get
            {
                return this.dotBaggageInformation;
            }
            set
            {
                this.dotBaggageInformation = value;
            }
        }

        public bool ShowSeatChange
        {
            get
            {
                this.showSeatChange = false;
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["ShowSeatChange"]))
                {
                    this.showSeatChange = Convert.ToBoolean(ConfigurationManager.AppSettings["ShowSeatChange"]);
                }
                return this.showSeatChange;
            }
            set
            {
            }
        }

        public bool ShowPremierAccess
        {
            get
            {
                return this.showPremierAccess;
            }
            set
            {
                this.showPremierAccess = value;
            }
        }

        public MOBPremierAccess PremierAccess
        {
            get
            {
                return this.premierAccess;
            }
            set
            {
                this.premierAccess = value;
            }
        }

        //Added by Nizam - #179873 - 07/26/2017
        public string PkDispenserPublicKey
        {
            get { return pkDispenserPublicKey; }
            set { pkDispenserPublicKey = value; }
        }

        public string SessionGuID
        {
            get { return sessionGuID; }
            set { sessionGuID = value; }
        }
        
        public MOBTPIInfo TripInsuranceInfo
        {
            get { return tripInsuranceInfo; }
            set { tripInsuranceInfo = value; }
        }

        public MOBAncillary Ancillary
        {
            get { return ancillary; }
            set { ancillary = value; }
        }
    }
}
