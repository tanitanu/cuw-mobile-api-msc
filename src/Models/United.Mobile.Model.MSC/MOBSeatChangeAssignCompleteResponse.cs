using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Shopping.TripInsurance;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable]
    public class MOBSeatChangeAssignCompleteResponse : MOBResponse
    {
        private MOBSeatChangeAssignCompleteRequest request;
        private string recordLocator = string.Empty;
        private string lastName = string.Empty;

        private List<string> seatAssignMessages;
        private List<string> eddMessages;

        private MOBPNR pnr;
        private List<string> dotBagRules;
        private MOBDOTBaggageInfo dotBaggageInformation;
        private bool showSeatChange;
        private MOBPremierAccess premierAccess;
        private bool showPremierAccess;
        private MOBTPIInfo tripInsuranceInfo;
        private MOBAncillary ancillary;

        public MOBSeatChangeAssignCompleteRequest Request
        {
            get { return request; }
            set { request = value; }
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

        public List<string> SeatAssignMessages
        {
            get
            {
                return this.seatAssignMessages;
            }
            set
            {
                this.seatAssignMessages = value;
            }
        }

        public List<string> EDDMessages
        {
            get
            {
                return this.eddMessages;
            }
            set
            {
                this.eddMessages = value;
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
