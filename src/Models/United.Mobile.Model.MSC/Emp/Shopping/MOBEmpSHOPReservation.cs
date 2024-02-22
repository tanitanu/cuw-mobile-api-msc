using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using United.Reward.Configuration;
using United.Definition.Emp.MP2015;
using United.Definition.Emp.Common;
using United.Mobile.Model.Common;
namespace United.Definition.Emp.Shopping
{
    [Serializable()]
    public class MOBEmpSHOPReservation
    {
        public MOBEmpSHOPReservation()
        {
            //rewardPrograms = new List<MOBEmpSHOPRewardProgram>();
            //rewardPrograms = United.Persist.FilePersist.Load<List<MOBEmpSHOPRewardProgram>>(System.Configuration.ConfigurationManager.AppSettings["FrequestFlyerRewardProgramListStaticGUID"].ToString(), "Booking2.0FrequentFlyerList");
            //if (rewardPrograms == null || rewardPrograms.Count == 0)
            //{
            //    rewardPrograms = new List<MOBEmpSHOPRewardProgram>();
            //    ConfigurationParameter.ConfigParameter parm = ConfigurationParameter.ConfigParameter.Configuration;
            //    for (int i = 0; i < parm.RewardTypes.Count; i++)
            //    {
            //        MOBEmpSHOPRewardProgram p = new MOBEmpSHOPRewardProgram();
            //        p.ProgramID = parm.RewardTypes[i].ProductID;
            //        p.Type = parm.RewardTypes[i].Type;
            //        p.Description = parm.RewardTypes[i].Description;
            //        rewardPrograms.Add(p);
            //    }
            //}
            //tcdAdvisoryMessages = new List<MOBItem>();
            //int tCDAdvisoryMessagesCount = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["TCDAdvisoryMessagesCount"].ToString());
            //for (int i = 1; i <= tCDAdvisoryMessagesCount; i++)
            //{
            //    MOBItem item = new MOBItem();
            //    item.Id = System.Configuration.ConfigurationManager.AppSettings["TCDAdvisoryMessages" + i.ToString()].ToString().Split('~')[0].ToString();
            //    item.CurrentValue = System.Configuration.ConfigurationManager.AppSettings["TCDAdvisoryMessages" + i.ToString()].ToString().Split('~')[1].ToString();
            //    item.SaveToPersist = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["TCDAdvisoryMessages" + i.ToString()].ToString().Split('~')[2].ToString());
            //    tcdAdvisoryMessages.Add(item);
            //}
        }
        private List<string> empTravelNotice = null; //property which will have the value from Brent when his logic is ready
        private string empPayrollDeductMessage = string.Empty; 
        
        public List<string> EmpTravelNotice
        {
            get
            {
                return this.empTravelNotice;
            }
            set
            {
                if (value != null)
                {
                    this.empTravelNotice = value;
                }
            }
        }
        public string EmpPayrollDeductMessage
        {
            get
            {
                return this.empPayrollDeductMessage;
            }
            set
            {
                if (value != null)
                {
                    this.empPayrollDeductMessage = value;
                }
            }
        }

        private List<MOBEmpBookingPassengerExtended> selectedPassengers;
        private MOBEmpAddPassengersComplete empAddPassengersComplete;
        private string sessionId = string.Empty;
        private string recordLocator = string.Empty;
        private string searchType = string.Empty;
        private string cartId = string.Empty;
        private bool isSignedInWithMP;
        private List<MOBEmpSHOPTrip> trips;
        private List<MOBEmpSHOPPrice> prices;
        private List<MOBEmpSHOPTax> taxes;
        private int numberOfTravelers;
        private List<MOBEmpSHOPTraveler> travelers;
        private List<MOBEmpCPTraveler> travelersCSL;
        private List<MOBEmpSeatPrice> seatPrices;
        private List<MOBEmpCreditCard> creditCards = new List<MOBEmpCreditCard>();
       
        private MOBEmpCPPhone reservationPhone;
        private MOBEmpEmail reservationEmail;
        private string warning;
        private List<MOBEmpSHOPTravelOption> travelOptions;
        private MOBEmpSHOPClubPassPurchaseRequest clubPassPurchaseRequest;
        private string seatMessage = string.Empty;
        private string travelOptionMessage = string.Empty;
        private bool travelersRegistered;
        private bool getALLSavedTravelers = true;
        private List<MOBEmpSHOPRewardProgram> rewardPrograms;
        private List<MOBEmpAddress> creditCardsAddress = new List<MOBEmpAddress>();
        private List<string> messages;
        private List<MOBEmpSHOPFareRules> fareRules;
        private bool unregisterFareLock = true;
        private List<MOBItem> tcdAdvisoryMessages;//******* MOBItem is generic enough to not be replicated.
        private string flightShareMessage = string.Empty;
        private string ineligibleToEarnCreditMessage = string.Empty;
        private string oaIneligibleToEarnCreditMessage = string.Empty;
        private bool awardTravel = false;
        private string pointOfSale = string.Empty;
        private string pkDispenserPublicKey = string.Empty;
        private bool isRefundable;
        private bool isInternational;
        private bool isFlexibleSegmentExist;
        private MOBEmpSHOPFareLock fareLock;
        private MOBEmpSHOPTripPriceBreakDown shopPriceBreakDown;
        private MOBEmpSHOPTripPriceBreakDown priceBreakDown;
        
        private List<MOBEmpLmxFlight> lmxFlights;
        private List<MOBEmpLMXTraveler> lmxtravelers;
        private string overMileageLimitMessage = ConfigurationManager.AppSettings["lmxOverMileageLimitMessage"];
        private string overMileageLimitAmount = ConfigurationManager.AppSettings["lmxOverMileageLimitAmount"];
        private string empCreditCardMessage = ConfigurationManager.AppSettings["EmpCreditCardMessage"];


        private string emppaymentMessage = string.Empty;
        private string passType = string.Empty;

        private bool isEligibleForPayrollDeduction;
        private bool isPaymentMadeByPayroll;


       
        // TODO Put this static string in config file
        private string legalInformation = "Purchase of this ticket means you understand and agree to all fare rules associated"+
            "with this ticket all terms and conditions associated with any additional order/product purchases made, the dangerous policy," +
            "and the terms and conditions in United's Contract of Carriage.";

        public string PassType
        {
            get
            {
                return this.passType;
            }
            set
            {
                if (value != null)
                {
                    this.passType = value;
                }
            }
        }

        public bool IsPaymentMadeByPayroll
        {
            get
            {
                return this.isPaymentMadeByPayroll;
            }
            set
            {
                if (value != null)
                {
                    this.isPaymentMadeByPayroll = value;
                }
            }
        }

        public string LegalInformation
        {
            get
            {
                return this.legalInformation;
            }
            set
            {
                if (value != null)
                {
                    this.legalInformation = value;
                }
            }
        }

        public bool IsEligibleForPayrollDeduction
        {
            get
            {
                return this.isEligibleForPayrollDeduction;
            }
            set
            {
                if (value != null)
                {
                    this.isEligibleForPayrollDeduction = value;
                }
            }
        }

        public string EmpPaymentMessage
        {
            get
            {
                return this.emppaymentMessage;
            }
            set
            {
                if (value != null)
                {
                    this.emppaymentMessage = value;
                }
            }
        }

        public MOBEmpAddPassengersComplete EmpAddPassengersComplete
        {
            get
            {
                return this.empAddPassengersComplete;
            }
            set
            {
                if (value != null)
                {
                    this.empAddPassengersComplete = value;
                }
            }
        }
        public List<MOBEmpBookingPassengerExtended> SelectedPassengers
        {
            get
            {
                return this.selectedPassengers;
            }
            set
            {
                this.selectedPassengers = value;
            }
        
        }

        public string OverMileageLimitMessage
        {
            get
            {
                return this.overMileageLimitMessage;
            }
            set
            {
                this.overMileageLimitMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string OverMileageLimitAmount
        {
            get
            {
                return this.overMileageLimitAmount;
            }
            set
            {
                this.overMileageLimitAmount = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public List<MOBEmpLMXTraveler> LMXTravelers
        {
            get
            {
                return this.lmxtravelers;
            }
            set
            {
                this.lmxtravelers = value;
            }
        }
        public bool AwardTravel
        {
            get
            {
                return this.awardTravel;
            }
            set
            {
                this.awardTravel = value;
            }
        }
        public bool IsRefundable
        {
            get
            {
                return this.isRefundable;
            }
            set
            {
                this.isRefundable = value;
            }
        }
        public bool ISInternational
        {
            get
            {
                return this.isInternational;
            }
            set
            {
                this.isInternational = value;
            }
        }
        public bool ISFlexibleSegmentExist
        {
            get
            {
                return this.isFlexibleSegmentExist;
            }
            set
            {
                this.isFlexibleSegmentExist = value;
            }
        }
        public string PointOfSale
        {
            get { return this.pointOfSale; }
            set { this.pointOfSale = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
        public string SeatMessage
        {
            get
            {
                return this.seatMessage;
            }
            set
            {
                this.seatMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string TravelOptionMessage
        {
            get
            {
                return this.travelOptionMessage;
            }
            set
            {
                this.travelOptionMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public List<MOBEmpSHOPTravelOption> TravelOptions
        {
            get
            {
                return this.travelOptions;
            }
            set
            {
                this.travelOptions = value;
            }
        }

        public MOBEmpSHOPClubPassPurchaseRequest ClubPassPurchaseRequest
        {
            get
            {
                return this.clubPassPurchaseRequest;
            }
            set
            {
                this.clubPassPurchaseRequest = value;
            }
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

        public string SearchType
        {
            get
            {
                return this.searchType;
            }
            set
            {
                this.searchType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string CartId
        {
            get
            {
                return this.cartId;
            }
            set
            {
                this.cartId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public bool IsSignedInWithMP
        {
            get { return this.isSignedInWithMP; }
            set { this.isSignedInWithMP = value; }
        }

        public List<MOBEmpSHOPTrip> Trips
        {
            get
            {
                return this.trips;
            }
            set
            {
                this.trips = value;
            }
        }

        public List<MOBEmpSHOPPrice> Prices
        {
            get
            {
                return this.prices;
            }
            set
            {
                this.prices = value;
            }
        }

        public List<MOBEmpSHOPTax> Taxes
        {
            get
            {
                return this.taxes;
            }
            set
            {
                this.taxes = value;
            }
        }

        public int NumberOfTravelers
        {
            get
            {
                return this.numberOfTravelers;
            }
            set
            {
                this.numberOfTravelers = value;
            }
        }

        public List<MOBEmpSHOPTraveler> Travelers
        {
            get
            {
                return this.travelers;
            }
            set
            {
                this.travelers = value;
            }
        }

        public List<MOBEmpCPTraveler> TravelersCSL
        {
            get
            {
                return this.travelersCSL;
            }
            set
            {
                this.travelersCSL = value;
            }
        }

        public List<MOBEmpSeatPrice> SeatPrices
        {
            get
            {
                return this.seatPrices;
            }
            set
            {
                this.seatPrices = value;
            }
        }

        public List<MOBEmpCreditCard> CreditCards
        {
            get
            {
                return creditCards;
            }
            set
            {
                if (value != null)
                {
                    creditCards = value;
                }
            }
        }

       
        public MOBEmpCPPhone ReservationPhone
        {
            get
            {
                return this.reservationPhone;
            }
            set
            {
                if (value != null)
                {
                    this.reservationPhone = value;
                }
            }
        }

        public MOBEmpEmail ReservationEmail
        {
            get
            {
                return this.reservationEmail;
            }
            set
            {
                if (value != null)
                {
                    this.reservationEmail = value;
                }
            }
        }

        public string Warning
        {
            get { return warning; }
            set { warning = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public bool TravelersRegistered
        {
            get { return this.travelersRegistered; }
            set { this.travelersRegistered = value; }
        }

        public List<MOBEmpSHOPRewardProgram> RewardPrograms
        {
            get
            {
                return this.rewardPrograms;
            }
            set
            {
                this.rewardPrograms = value;
            }
        }

        public List<MOBEmpAddress> CreditCardsAddress
        {
            get
            {
                return this.creditCardsAddress;
            }
            set
            {
                this.creditCardsAddress = value;
            }
        }
        public List<string> Messages
        {
            get
            {
                return this.messages;
            }
            set
            {
                this.messages = value;
            }
        }

        public List<MOBEmpSHOPFareRules> FareRules
        {
            get
            {
                return this.fareRules;
            }
            set
            {
                this.fareRules = value;
            }
        }

        public bool UnregisterFareLock
        {
            get { return this.unregisterFareLock; }
            set { this.unregisterFareLock = value; }
        }

        public MOBEmpSHOPFareLock FareLock
        {
            get
            {
                return this.fareLock;
            }
            set
            {
                this.fareLock = value;
            }
        }

        public List<MOBItem> TCDAdvisoryMessages//Generic
        {
            get
            {
                return this.tcdAdvisoryMessages;
            }
            set
            {
                this.tcdAdvisoryMessages = value;
            }
        }

        public string FlightShareMessage
        {
            get
            {
                return this.flightShareMessage;
            }
            set
            {
                this.flightShareMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string PKDispenserPublicKey
        {
            get
            {
                return this.pkDispenserPublicKey;
            }
            set
            {
                this.pkDispenserPublicKey = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MOBEmpLmxFlight> LMXFlights
        {
            get
            {
                return this.lmxFlights;
            }
            set
            {
                this.lmxFlights = value;
            }
        }

        public string IneligibleToEarnCreditMessage
        {
            get
            {
                return this.ineligibleToEarnCreditMessage;
            }
            set
            {
                this.ineligibleToEarnCreditMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string OaIneligibleToEarnCreditMessage
        {
            get
            {
                return this.oaIneligibleToEarnCreditMessage;
            }
            set
            {
                this.oaIneligibleToEarnCreditMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        
        public bool GetALLSavedTravelers
        {
            get
            {
                return this.getALLSavedTravelers;
            }
            set
            {
                this.getALLSavedTravelers = value;
            }
        }

        public MOBEmpSHOPTripPriceBreakDown ShopPriceBreakDown
        {
            get
            {
                return this.shopPriceBreakDown;
            }
            set 
            {
                this.shopPriceBreakDown = value; 
            }

        }
        public MOBEmpSHOPTripPriceBreakDown PriceBreakDown
        {
            get
            {
                return this.priceBreakDown;
            }
            set
            {
                this.priceBreakDown = value;
            }
        }

        public string EmpCreditCardMessage
        {
            get
            {
                return this.empCreditCardMessage;
            }
            set
            {
                this.empCreditCardMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
