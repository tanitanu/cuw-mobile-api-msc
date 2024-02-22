using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Shopping;

namespace United.Definition
{
    [Serializable()]
    public class MOBCheckOutResponse : MOBShoppingResponse
    {
        [JsonIgnore()]
        public string ObjectName { get; set; } = "United.Definition.MOBCheckOutResponse";
        private MOBShoppingCart shoppingCart;
        private string postPurchasePage;
        private string recordLocator = string.Empty;
        private string lastName = string.Empty;
        private bool enabledSecondaryFormofPayment = false;
        private bool isTPIFailed = false;
        private List<string> seatAssignMessages = new List<string>();

        private MOBSHOPReservation reservation;  
        private List<string> dotBagRules; 
        private MOBDOTBaggageInfo dotBaggageInfo; 
        private string shareFlightTitle = string.Empty; 
        private string shareFlightMessage = string.Empty; 
        private List<MOBClubDayPass> passes; 
        private string otpMessage = string.Empty; 
        private string specialNeedsMessage; 
        private MOBSHOPResponseStatusItem responseStatusItem;       
        private string confirmationMsgForTPI;
        private string pnrCreateDate;
        private bool isUpgradePartialSuccess = false;

        public MOBSHOPReservation Reservation
        {
            get
            {
                return this.reservation;
            }
            set
            {
                this.reservation = value;
            }
        }

        public List<string> DOTBagRules
        {
            get
            {
                return this.dotBagRules;
            }
            set
            {
                this.dotBagRules = value;
            }
        }

        public MOBDOTBaggageInfo DOTBaggageInfo
        {
            get
            {
                return this.dotBaggageInfo;
            }
            set
            {
                this.dotBaggageInfo = value;
            }
        }

        public string ShareFlightTitle
        {
            get
            {
                return this.shareFlightTitle;
            }
            set
            {
                this.shareFlightTitle = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ShareFlightMessage
        {
            get
            {
                return this.shareFlightMessage;
            }
            set
            {
                this.shareFlightMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        
        public List<MOBClubDayPass> Passes
        {
            get
            {
                return this.passes;
            }
            set
            {
                this.passes = value;
            }
        }
        
        public string OTPMessage
        {
            get
            {
                return this.otpMessage;
            }
            set
            {
                this.otpMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public MOBSHOPResponseStatusItem ResponseStatusItem
        {
            get { return responseStatusItem; }
            set { responseStatusItem = value; }
        }
        
        public string SpecialNeedsMessage
        {
            get { return specialNeedsMessage; }
            set { specialNeedsMessage = value; }
        }

        public MOBShoppingCart ShoppingCart
        {
            get { return shoppingCart; }
            set { shoppingCart = value; }
        }

        public string PostPurchasePage
        {
            get { return postPurchasePage; }
            set { postPurchasePage = value; }
        }

        public string RecordLocator
        {
            get { return recordLocator; }
            set { recordLocator = value; }
        }

        public string PnrCreateDate
        {
            get { return pnrCreateDate; }
            set { pnrCreateDate = value; }
        }

        public string LastName
        {
            get { return lastName; }
            set { lastName = value; }
        }

        public bool EnabledSecondaryFormofPayment
        {
            get { return enabledSecondaryFormofPayment; }
            set { enabledSecondaryFormofPayment = value; }
        }

        public bool IsTPIFailed
        {
            get { return isTPIFailed; }
            set { isTPIFailed = value; }
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
        
        public string ConfirmationMsgForTPI
        {
            get
            {
                return this.confirmationMsgForTPI;
            }
            set
            {
                this.confirmationMsgForTPI = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public bool IsUpgradePartialSuccess
        { get { return this.isUpgradePartialSuccess; } set { this.isUpgradePartialSuccess = value; } }
    }
}
