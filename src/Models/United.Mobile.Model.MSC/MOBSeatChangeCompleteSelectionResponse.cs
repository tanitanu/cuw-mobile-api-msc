using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Accelerators;
using United.Definition.Booking;
using United.Definition.Pcu;
using United.Definition.Shopping.TripInsurance;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBSeatChangeCompleteSelectionResponse : MOBResponse
    {
        public MOBSeatChangeCompleteSelectionResponse()
            : base()
        {
        }

        private MOBSeatChangeCompleteSelectionRequest request;
        private string sessionId = string.Empty;
        private List<MOBBKTraveler> bookingTravlerInfo;
        private List<MOBBKTrip> selectedTrips;
        private List<MOBCreditCard> creditCards;
        private List<MOBSeat> seats;
        private List<MOBSeatPrice> seatPrices;
        private List<MOBAddress> profileOwnerAddresses;
        private List<MOBEmail> emails;
        private List<string> termsAndConditions;

        private List<string> seatAssignMessages;
        private List<string> eddMessages;

        private MOBPNR pnr;
        private List<string> dotBagRules;
        private MOBDOTBaggageInfo dotBaggageInformation;
        private bool showSeatChange;
        private MOBPremierAccess premierAccess;
        private bool showPremierAccess;
        //Added by Nizam - #179873 - 07/28/2017
        private string pkDispenserPublicKey;
        private string sessionGuID;
        private MOBTPIInfo tripInsuranceInfo;
        private string priceBreakDownTitle;
        private MOBAncillary ancillary;

        public MOBSeatChangeCompleteSelectionRequest Request
        {
            get { return this.request; }
            set { this.request = value; }
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

        public List<MOBBKTraveler> BookingTravelerInfo
        {
            get { return this.bookingTravlerInfo; }
            set { this.bookingTravlerInfo = value; }
        }

        public List<MOBBKTrip> SelectedTrips
        {
            get { return this.selectedTrips; }
            set { this.selectedTrips = value; }
        }

        public List<MOBCreditCard> CreditCards
        {
            get { return this.creditCards; }
            set { this.creditCards = value; }
        }

        public List<MOBSeat> Seats
        {
            get { return this.seats; }
            set { this.seats = value; }
        }

        public List<MOBSeatPrice> SeatPrices
        {
            get { return this.seatPrices; }
            set { this.seatPrices = value; }
        }

        public List<MOBAddress> ProfileOwnerAddresses
        {
            get { return this.profileOwnerAddresses; }
            set { this.profileOwnerAddresses = value; }
        }

        public List<MOBEmail> Emails
        {
            get { return this.emails; }
            set { this.emails = value; }
        }

        public List<string> TermsAndConditions
        {
            get { return this.termsAndConditions; }
            set { this.termsAndConditions = value; }
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

        //Added by Nizam - #179873 - 07/28/2017
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

        public string PriceBreakDownTitle
        {
            get { return priceBreakDownTitle; }
            set { priceBreakDownTitle = value; }
        }

        public MOBAncillary Ancillary
        {
            get { return ancillary; }
            set { ancillary = value; }
        }

    }
}
