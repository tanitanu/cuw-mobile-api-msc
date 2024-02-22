using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Services.FlightShopping.Common.DisplayCart;
using United.Definition.Booking;
using United.Definition.Pcu;
using United.Definition.Shopping.TripInsurance;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using United.Definition.Shopping;
using United.Mobile.Model.MSC;
using United.Definition.SSR;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable]
    public class MOBRegisterSeatsResponse : MOBResponse
    {
        private  IConfiguration _configuration;
        public MOBRegisterSeatsResponse()
            : base()
        {
        }
        public MOBRegisterSeatsResponse(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void DOTBagrules(IConfiguration configuration)
        {
            _configuration = configuration;

            dotBagRules = new List<string> { };
            string rText = _configuration.GetValue<string>("DOTBagRules");

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
        }

            
        private string sessionId;
        private string flow = string.Empty;
        private bool isDefaultPaymentOption = false;
        private List<FormofPaymentOption> eligibleFormofPayments;
        private MOBShoppingCart shoppingCart = new MOBShoppingCart();
        private string pkDispenserPublicKey;
        private MOBRegisterSeatsRequest request;
        public List<MOBBKTraveler> bookingTravlerInfo { get; set; }
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
        private string sessionGuID;
        private MOBTPIInfo tripInsuranceInfo;
        private string priceBreakDownTitle;
        private MOBAncillary ancillary;
        private List<MOBSHOPRewardProgram> rewardPrograms;
        private MOBTravelSpecialNeeds specialNeeds;
        private TravelOptions travelOptions;
        public List<MOBSHOPRewardProgram> RewardPrograms { get { return this.rewardPrograms; } set { this.rewardPrograms = value; } }
        public MOBTravelSpecialNeeds SpecialNeeds
        {
            get { return this.specialNeeds; }
            set { this.specialNeeds = value; }
        }
        public TravelOptions TravelOptions
        {
            get { return travelOptions; }
            set { travelOptions = value; }
        }
        public string SessionId
        {
            get { return sessionId; }
            set { sessionId = value; }
        }
        
        public string Flow
        {
            get { return flow; }
            set { flow = value; }
        }

        public bool IsDefaultPaymentOption
        {
            get
            {
                return this.isDefaultPaymentOption;
            }
            set
            {
                this.isDefaultPaymentOption = value;
            }
        }
        public List<FormofPaymentOption> EligibleFormofPayments
        {
            get { return eligibleFormofPayments; }
            set { eligibleFormofPayments = value; }
        }
        public MOBShoppingCart ShoppingCart
        {
            get { return shoppingCart; }
            set { shoppingCart = value; }
        }
        public string PkDispenserPublicKey
        {
            get { return pkDispenserPublicKey; }
            set { pkDispenserPublicKey = value; }
        }

        public MOBRegisterSeatsRequest Request
        {
            get { return this.request; }
            set { this.request = value; }
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
                //string rText = _configuration.GetValue<string>("DOTBagRules");

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
