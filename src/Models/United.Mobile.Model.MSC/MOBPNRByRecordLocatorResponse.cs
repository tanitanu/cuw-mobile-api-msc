using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Accelerators;
using United.Definition.Pcu;
using United.Definition.Shopping;
using United.Definition.Shopping.TripInsurance;
using United.Definition.SSR;
using United.Mobile.Model.MSC;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBPNRByRecordLocatorResponse : MOBResponse
    {
        private string sessionId = string.Empty;
        private string flow = string.Empty;
        private string recordLocator = string.Empty;
        private string lastName = string.Empty;
        private MOBPNR pnr;
        private string uaRecordLocator = string.Empty;
        private List<string> dotBagRules;
        private MOBDOTBaggageInfo dotBaggageInformation;
        private MOBPremierAccess premierAccess;
        private List<MOBSHOPRewardProgram> rewardPrograms;
        private MOBTravelSpecialNeeds specialNeeds;       
        private bool showSeatChange;
        private bool showPremierAccess;
        // All properties here should also be in MOBPremierAccessCompleteResponse, since iOS is using MOBPNRByRecordLocatorResponse as CompletePremierAccessSelectionECC response model. 
        private MOBTPIInfo tripInsuranceInfo;
        private MOBAncillary ancillary;
        private TravelOptions travelOptions;


        public TravelOptions TravelOptions
        {
            get { return travelOptions; }
            set { travelOptions = value; }
        }
        public MOBTravelSpecialNeeds SpecialNeeds
        {
            get { return this.specialNeeds; }
            set { this.specialNeeds = value; }
        }
        public string SessionId
        {
            get
            {
                return this.sessionId;
            }
            set
            {
                this.sessionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string Flow
        {
            get
            {
                return this.flow;
            }
            set
            {
                this.flow = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public List<MOBSHOPRewardProgram> RewardPrograms { get { return this.rewardPrograms; } set { this.rewardPrograms = value; } }
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


        public string UARecordLocator
        {
            get
            {
                return this.uaRecordLocator;
            }
            set
            {
                this.uaRecordLocator = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
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
                return this.showSeatChange;

                //this.showSeatChange = false;
                //if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["ShowSeatChange"]))
                //{
                //    this.showSeatChange = Convert.ToBoolean(ConfigurationManager.AppSettings["ShowSeatChange"]);
                //}
                //return this.showSeatChange;
            }
            set
            {
                this.showSeatChange = value;
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

        public MOBTPIInfo TripInsuranceInfo
        {
            get
            {
                return this.tripInsuranceInfo;
            }
            set
            {
                this.tripInsuranceInfo = value;
            }
        }

        public MOBAncillary Ancillary
        {
            get { return ancillary; }
            set { ancillary = value; }
        }
    }
}
