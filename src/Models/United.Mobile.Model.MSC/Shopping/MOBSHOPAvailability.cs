using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Shopping.AwardCalendar;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping
{
    [Serializable()]
    public class MOBSHOPAvailability
    {
        private string sessionId = string.Empty;
        private string cartId = string.Empty;
        private decimal closeInBookingFee;
        private List<MOBSHOPFareWheelItem> fareWheel; 
        private MOBSHOPTrip trip;
        private MOBSHOPReservation reservation;
        private MOBSHOPFareLock fareLock;
        //private List<MOBSHOPOffer> fareLock;
        private MOBSHOPAwardCalendar awardCalendar;
        private MOBSHOPAwardDynamicCalendar awardDynamicCalendar;

        private string callDuration = string.Empty;
        private string uaDiscount = string.Empty;
        private bool awardTravel = false;
        private List<MOBItem> elfShopMessages;
        private List<MOBSHOPOption> elfShopOptions;
        private bool offerMetaSearchElfUpsell = false;
        private List<MOBSHOPUpsellProduct> upsells;
        private string corporateDisclaimer;
        private bool disablePricingBySlice;
        private string priceTextDescription = string.Empty;
        private string fSRFareDescription = string.Empty;
        private List<MOBFSRAlertMessage> fsrAlertMessages;
        private string availableAwardMilesWithDesc;
        private int travelerCount;

        public int TravelerCount
        {
            get
            {
                return travelerCount;
            }
            set
            {
                travelerCount = value;
            }
        }

        public string CorporateDisclaimer
        {
            get { return corporateDisclaimer; }
            set
            {
                corporateDisclaimer = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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

        public string CartId
        {
            get
            {
                return this.cartId;
            }
            set
            {
                this.cartId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public decimal CloseInBookingFee
        {
            get
            {
                return this.closeInBookingFee;
            }
            set
            {
                this.closeInBookingFee = value;
            }
        }

        public List<MOBSHOPFareWheelItem> FareWheel
        {
            get
            {
                return this.fareWheel;
            }
            set
            {
                this.fareWheel = value;
            }
        }

        public MOBSHOPTrip Trip
        {
            get
            {
                return this.trip;
            }
            set
            {
                this.trip = value;
            }
        }

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

        public MOBSHOPFareLock FareLock
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

        //public List<MOBSHOPOffer> FareLock
        //{
        //    get
        //    {
        //        return this.fareLock;
        //    }
        //    set
        //    {
        //        this.fareLock = value;
        //    }
        //}

        public MOBSHOPAwardCalendar AwardCalendar
        {
            get
            {
                return this.awardCalendar;
            }
            set
            {
                this.awardCalendar = value;
            }
        }
        public MOBSHOPAwardDynamicCalendar AwardDynamicCalendar
        {
            get { return this.awardDynamicCalendar; }
            set
            {
                this.awardDynamicCalendar= value;
            }
        }
        public string CallDuration
        {
            get
            {
                return this.callDuration;
            }
            set
            {
                this.callDuration = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string UaDiscount
        {
            get
            {
                return this.uaDiscount;
            }
            set
            {
                this.uaDiscount = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MOBItem> ELFShopMessages
        {
            get
            {
                return this.elfShopMessages;
            }
            set
            {
                this.elfShopMessages = value;
            }
        }

        public List<MOBSHOPOption> ELFShopOptions
        {
            get
            {
                return this.elfShopOptions;
            }
            set
            {
                this.elfShopOptions = value;
            }
        }

        public bool OfferMetaSearchElfUpsell
        {
            get
            {
                return this.offerMetaSearchElfUpsell;
            }
            set
            {
                this.offerMetaSearchElfUpsell = value;
            }
        }

        public List<MOBSHOPUpsellProduct> Upsells
        {
            get { return this.upsells; }
            set { this.upsells = value; }
        }
        public bool DisablePricingBySlice
        {
            get
            {
                return this.disablePricingBySlice;
            }
            set
            {
                this.disablePricingBySlice = value;
            }
        }
        public string PriceTextDescription
        {
            get { return priceTextDescription; }
            set { priceTextDescription = value; }
        }
        public string FSRFareDescription
        {
            get { return fSRFareDescription; }
            set { fSRFareDescription = value; }
        }

        public List<MOBFSRAlertMessage> FSRAlertMessages
        {
            get { return fsrAlertMessages; }
            set { fsrAlertMessages = value; }
        }

        public string AvailableAwardMilesWithDesc
        {
            get { return availableAwardMilesWithDesc; }
            set { availableAwardMilesWithDesc = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private bool isMoneyAndMilesEligible;

        public bool IsMoneyAndMilesEligible
        {
            get { return isMoneyAndMilesEligible; }
            set { isMoneyAndMilesEligible = value; }
        }
    }
}
