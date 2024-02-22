using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Emp.Common;

namespace United.Definition.Emp
{
    public class MOBEmpPNR
    {
        private string sessionId = string.Empty;
        private string recordLocator = string.Empty;
        private string dateCreated = string.Empty;
        private string description = string.Empty;
        private bool isActive;
        private string ticketType = string.Empty;
        private string numberOfPassengers = string.Empty;
        private List<MOBEmpTrip> trips;
        private List<MOBEmpPNRPassenger> passengers;
        private List<MOBEmpPNRSegment> segments;
        private string lastTripDateDepartureDate = string.Empty;
        private string lastTripDateArrivalDate = string.Empty;
        private string checkinEligible = "N";
        private string alreadyCheckedin = "false";
        private string notValid = "false";
        private string validforCheckin = "false";
        private string pnrCanceled = "false";
        private string uaRecordLocator = string.Empty;
        private string coRecordLocator = string.Empty;
        private string pnrOwner = string.Empty;
        private List<MOBEmpOARecordLocator> oaRecordLocators = new List<MOBEmpOARecordLocator>();
        private string oaRecordLocatorMessageTitle = string.Empty;
        private string oaRecordLocatorMessage = string.Empty;
        private bool isEligibleToSeatChange = false;
        private string emailAddress = string.Empty;
        private MOBEmpSeatOffer seatOffer;
        private MOBEmpBundleInfo bundleInfo;
        private List<string> petRecordLocators;
        private string upgradeMessage = string.Empty;
        private string farelockExpirationDate = string.Empty;
        private string farelockPurchaseMessage = string.Empty;
        private string earnedMilesHeader = string.Empty;
        private string earnedMilesText = string.Empty;
        private string ineligibleToEarnCreditMessage = string.Empty;
        private string oaIneligibleToEarnCreditMessage = string.Empty;
        private bool awardTravel;
        private bool psSaTravel;
        private bool supressLMX = false;
        private string overMileageLimitMessage = ConfigurationManager.AppSettings["lmxOverMileageLimitMessage"];
        private string overMileageLimitAmount = ConfigurationManager.AppSettings["lmxOverMileageLimitAmount"];
        private List<MOBEmpLMXTraveler> lmxtravelers;

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
        public string DateCreated
        {
            get
            {
                return this.dateCreated;
            }
            set
            {
                this.dateCreated = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string Description
        {
            get
            {
                return this.description;
            }
            set
            {
                this.description = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public bool IsActive
        {
            get
            {
                return this.isActive;
            }
            set
            {
                this.isActive = value;
            }
        }
        public string TicketType
        {
            get
            {
                return this.ticketType;
            }
            set
            {
                this.ticketType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
        public string NumberOfPassengers
        {
            get
            {
                return this.numberOfPassengers;
            }
            set
            {
                this.numberOfPassengers = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
        public List<MOBEmpTrip> Trips
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
        public List<MOBEmpPNRPassenger> Passengers
        {
            get
            {
                return this.passengers;
            }
            set
            {
                this.passengers = value;
            }
        }
        public List<MOBEmpPNRSegment> Segments
        {
            get
            {
                return this.segments;
            }
            set
            {
                this.segments = value;
            }
        }
        public string LastTripDateDepartureDate
        {
            get
            {
                return this.lastTripDateDepartureDate;
            }
            set
            {
                this.lastTripDateDepartureDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string LastTripDateArrivalDate
        {
            get
            {
                return this.lastTripDateArrivalDate;
            }
            set
            {
                this.lastTripDateArrivalDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string CheckinEligible
        {
            get
            {
                return this.checkinEligible;
            }
            set
            {
                this.checkinEligible = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
        public string AlreadyCheckedin
        {
            get
            {
                return this.alreadyCheckedin;
            }
            set
            {
                this.alreadyCheckedin = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string NotValid
        {
            get
            {
                return this.notValid;
            }
            set
            {
                this.notValid = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string ValidforCheckin
        {
            get
            {
                return this.validforCheckin;
            }
            set
            {
                this.validforCheckin = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string PNRCanceled
        {
            get
            {
                return this.pnrCanceled;
            }
            set
            {
                this.pnrCanceled = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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
        public string CORecordLocator
        {
            get
            {
                return this.coRecordLocator;
            }
            set
            {
                this.coRecordLocator = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
        public string PNROwner
        {
            get
            {
                return this.pnrOwner;
            }
            set
            {
                this.pnrOwner = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
        public List<MOBEmpOARecordLocator> OARecordLocators
        {
            get
            {
                return this.oaRecordLocators;
            }
            set
            {
                this.oaRecordLocators = value;
            }
        }
        public string OARecordLocatorMessageTitle
        {
            get
            {
                return this.oaRecordLocatorMessageTitle;
            }
            set
            {
                this.oaRecordLocatorMessageTitle = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string OARecordLocatorMessage
        {
            get
            {
                return this.oaRecordLocatorMessage;
            }
            set
            {
                this.oaRecordLocatorMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public bool IsEligibleToSeatChange
        {
            get
            {
                return this.isEligibleToSeatChange;
            }
            set
            {
                this.isEligibleToSeatChange = value;
            }
        }
        public string EmailAddress
        {
            get
            {
                return this.emailAddress;
            }
            set
            {
                this.emailAddress = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public MOBEmpSeatOffer SeatOffer
        {
            get
            {
                return this.seatOffer;
            }
            set
            {
                this.seatOffer = value;
            }
        }
        public MOBEmpBundleInfo BundleInfo
        {
            get
            {
                return this.bundleInfo;
            }
            set
            {
                this.bundleInfo = value;
            }
        }
        public List<string> PetRecordLocators
        {
            get
            {
                return this.petRecordLocators;
            }
            set
            {
                petRecordLocators = value;
            }
        }
        public string UpgradeMessage
        {
            get
            {
                return this.upgradeMessage;
            }
            set
            {
                this.upgradeMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string FarelockExpirationDate
        {
            get
            {
                return this.farelockExpirationDate;
            }
            set
            {
                this.farelockExpirationDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string FarelockPurchaseMessage
        {
            get
            {
                return this.farelockPurchaseMessage;
            }
            set
            {
                this.farelockPurchaseMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string EarnedMilesHeader
        {
            get
            {
                return this.earnedMilesHeader;
            }
            set
            {
                this.earnedMilesHeader = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string EarnedMilesText
        {
            get
            {
                return this.earnedMilesText;
            }
            set
            {
                this.earnedMilesText = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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
        public bool PsSaTravel
        {
            get
            {
                return this.psSaTravel;
            }
            set
            {
                this.psSaTravel = value;
            }
        }
        public bool SupressLMX
        {
            get
            {
                return this.supressLMX;
            }
            set
            {
                this.supressLMX = value;
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
    }
}
