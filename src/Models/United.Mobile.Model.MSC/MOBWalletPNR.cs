using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition
{
    [Serializable()]
    public class MOBWalletPNR
    {
        private string mpNumber = string.Empty;
        private string recordLocator = string.Empty;
        private string flightDate = string.Empty;
        private string origin = string.Empty;
        private string originCity = string.Empty;
        private string destination = string.Empty;
        private string destinationCity = string.Empty;
        private string checkInStatus = string.Empty;
        private string firstName = string.Empty;
        private string lastName = string.Empty;
        private string numberOfPassengers = string.Empty;
        private string lastSegmentArrivalDate = string.Empty;
        private string expirationDate = string.Empty;
        private string lastUpdated = string.Empty;
        private List<MOBWalletPNRSegment> segments;
        private bool enableUberLinkButtonPNRLevel;
        private bool irrOps;
        private bool irrOpsViewed;
        private string dateCreated = string.Empty;
        private string farelockExpirationDate = string.Empty;
        private bool awardTravel;
        private bool psSaTravel;
        private List<MOBWalletTripPass> tripPasses;
        private bool isMpOwnerOnPNR;
        private bool hasCheckedBags;
        private bool getCheckInStatusFromCSLPNRRetrivalService;

        public MOBWalletPNR()
        {
            this.addToComplications = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["EnableAddToComplications"].ToString());
        }

        public string MPNumber
        {
            get
            {
                return this.mpNumber;
            }
            set
            {
                this.mpNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
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

        public string FlightDate
        {
            get
            {
                return this.flightDate;
            }
            set
            {
                this.flightDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Origin
        {
            get
            {
                return this.origin;
            }
            set
            {
                this.origin = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string OriginCity
        {
            get
            {
                return this.originCity;
            }
            set
            {
                this.originCity = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Destination
        {
            get
            {
                return this.destination;
            }
            set
            {
                this.destination = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string DestinationCity
        {
            get
            {
                return this.destinationCity;
            }
            set
            {
                this.destinationCity = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string CheckInStatus
        {
            get
            {
                return this.checkInStatus;
            }
            set
            {
                this.checkInStatus = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string FirstName
        {
            get
            {
                return this.firstName;
            }
            set
            {
                this.firstName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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
                this.lastName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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
                this.numberOfPassengers = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string LastSegmentArrivalDate
        {
            get
            {
                return this.lastSegmentArrivalDate;
            }
            set
            {
                this.lastSegmentArrivalDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ExpirationDate
        {
            get
            {
                return this.expirationDate;
            }
            set
            {
                this.expirationDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string LastUpdated
        {
            get
            {
                return this.lastUpdated;
            }
            set
            {
                this.lastUpdated = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MOBWalletPNRSegment> Segments
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

        public bool EnableUberLinkButtonPNRLevel
        {
            get
            {
                return this.enableUberLinkButtonPNRLevel;
            }
            set
            {
                this.enableUberLinkButtonPNRLevel = value;
            }
        }

        public bool IrrOps
        {
            get
            {
                return this.irrOps;
            }
            set
            {
                this.irrOps = value;
            }
        }

        public bool IrrOpsViewed
        {
            get
            {
                return this.irrOpsViewed;
            }
            set
            {
                this.irrOpsViewed = value;
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

        public List<MOBWalletTripPass> TripPasses
        {
            get
            {
                return this.tripPasses;
            }
            set
            {
                this.tripPasses = value;
            }
        }

        public bool IsMpOwnerOnPNR
        {
            get
            {
                return this.isMpOwnerOnPNR;
            }
            set
            {
                this.isMpOwnerOnPNR = value;
            }
        }

        public string DateCreated
        {
            get { return dateCreated; }
            set { dateCreated = value; }
        }
        
        private bool addToComplications;
        public bool AddToComplications
        {
            get
            {
                return this.addToComplications;
            }
            set
            {
                this.addToComplications = value;
            }
        }

        public bool HasCheckedBags
        {
            get
            {
                return this.hasCheckedBags;
            }
            set
            {
                this.hasCheckedBags = value;
            }
        }


        public bool GetCheckInStatusFromCSLPNRRetrivalService
        {
            get
            {
                return this.getCheckInStatusFromCSLPNRRetrivalService;
            }
            set
            {
                this.getCheckInStatusFromCSLPNRRetrivalService = value;
            }
        }
    }
}
