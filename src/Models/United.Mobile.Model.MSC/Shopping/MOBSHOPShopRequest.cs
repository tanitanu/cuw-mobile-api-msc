using System;
using System.Collections.Generic;
using United.Definition.Corporate;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping
{
    [Serializable()]
    public class MOBSHOPShopRequest : MOBRequest
    {
        private string sessionId = string.Empty;
        private string countryCode = "US";
        private List<MOBSHOPTripBase> trips;
        private int numberOfAdults;
        private int numberOfSeniors;
        private int numberOfChildren2To4;
        private int numberOfChildren5To11;
        private int numberOfChildren12To17;
        private int numberOfInfantOnLap;
        private int numberOfInfantWithSeat;
        private bool awardTravel;
        private string mileagePlusAccountNumber = string.Empty;
        private string searchType = string.Empty;
        private string serviceType = string.Empty;
        private int maxNumberOfStops;
        private int maxNumberOfTrips;
        private string resultSortType = string.Empty;
        private string fareType = string.Empty;
        private string classOfServices = string.Empty;
        private int pageIndex = 1;
        private int pageSize = 25;

        //advanced search options
        private string fareClass = string.Empty;
        private string promotionCode = string.Empty;
        private bool showMileageDetails;
        private int premierStatusLevel;

        private string employeeDiscountId = string.Empty;
        private bool isReshopChange = false;
        private string recordLocator = string.Empty;
        private string lastName;

        private List<MOBPNRPassenger> reshopTravelers = null;
        private List<MOBPNRSegment> reshopSegments = null;
        private List<MOBTravelerType> travelerTypes = null;

        public List<MOBTravelerType> TravelerTypes
        {
            get { return travelerTypes; }
            set { travelerTypes = value; }
        }

        private bool isCorporateBooking;
        public bool IsCorporateBooking
        {
            get { return isCorporateBooking; }
            set { isCorporateBooking = value; }
        }
        private MOBCorporateDetails mobCPCorporateDetails;
        public MOBCorporateDetails MOBCPCorporateDetails
        {
            get { return mobCPCorporateDetails; }
            set { mobCPCorporateDetails = value; }
        }

        private MOBCPCustomerMetrics customerMetrics;
        public MOBCPCustomerMetrics CustomerMetrics
        {
            get { return customerMetrics; }
            set { customerMetrics = value; }
        }

        private int lengthOfCalendar;
        public int LengthOfCalendar
        {
            get { return lengthOfCalendar; }
            set { lengthOfCalendar = value; }
        }
        private bool getNonStopFlightsOnly;
        private bool getFlightsWithStops;

        private bool cameFromFSRHandler;

        /// <summary>
        /// True if this is an updated request that came from FRS handler
        /// </summary>
        public bool CameFromFSRHandler
        {
            get { return cameFromFSRHandler; }
            set { cameFromFSRHandler = value; }
        }


        public bool GetFlightsWithStops
        {
            get { return getFlightsWithStops; }
            set { getFlightsWithStops = value; }
        }


        public bool GetNonStopFlightsOnly
        {
            get { return getNonStopFlightsOnly; }
            set { getNonStopFlightsOnly = value; }
        }


        public List<MOBPNRPassenger> ReshopTravelers
        {
            get { return reshopTravelers; }
            set { reshopTravelers = value; }
        }

        public List<MOBPNRSegment> ReshopSegments
        {
            get { return reshopSegments; }
            set { reshopSegments = value; }
        }

        public string LastName
        {
            get { return lastName; }
            set { lastName = value; }
        }

        public bool IsReshopChange
        {
            get { return isReshopChange; }
            set { isReshopChange = value; }
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


        private bool isELFFareDisplayAtFSR = true;
        public bool IsELFFareDisplayAtFSR
        {
            get { return isELFFareDisplayAtFSR; }
            set { isELFFareDisplayAtFSR = value; }
        }

        private bool isExpertModeEnabled;
        public bool IsExpertModeEnabled { get { return this.isExpertModeEnabled; } set { this.isExpertModeEnabled = value; } }

        public string CountryCode
        {
            get
            {
                return this.countryCode;
            }
            set
            {
                this.countryCode = string.IsNullOrEmpty(value) ? "US" : value.Trim().ToUpper();
            }
        }

        public List<MOBSHOPTripBase> Trips
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

        public int NumberOfAdults
        {
            get
            {
                return this.numberOfAdults;
            }
            set
            {
                this.numberOfAdults = value;
            }
        }

        public int NumberOfSeniors
        {
            get
            {
                return this.numberOfSeniors;
            }
            set
            {
                this.numberOfSeniors = value;
            }
        }

        public int NumberOfChildren2To4
        {
            get
            {
                return this.numberOfChildren2To4;
            }
            set
            {
                this.numberOfChildren2To4 = value;
            }
        }

        public int NumberOfChildren5To11
        {
            get
            {
                return this.numberOfChildren5To11;
            }
            set
            {
                this.numberOfChildren5To11 = value;
            }
        }

        public int NumberOfChildren12To17
        {
            get
            {
                return this.numberOfChildren12To17;
            }
            set
            {
                this.numberOfChildren12To17 = value;
            }
        }

        public int NumberOfInfantOnLap
        {
            get
            {
                return this.numberOfInfantOnLap;
            }
            set
            {
                this.numberOfInfantOnLap = value;
            }
        }

        public int NumberOfInfantWithSeat
        {
            get
            {
                return this.numberOfInfantWithSeat;
            }
            set
            {
                this.numberOfInfantWithSeat = value;
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

        public string MileagePlusAccountNumber
        {
            get
            {
                return this.mileagePlusAccountNumber;
            }
            set
            {
                this.mileagePlusAccountNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
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
                this.searchType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ServiceType
        {
            get
            {
                return this.serviceType;
            }
            set
            {
                this.serviceType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int MaxNumberOfStops
        {
            get
            {
                return this.maxNumberOfStops;
            }
            set
            {
                this.maxNumberOfStops = value;
            }
        }

        public int MaxNumberOfTrips
        {
            get
            {
                return this.maxNumberOfTrips;
            }
            set
            {
                this.maxNumberOfTrips = value;
            }
        }

        public string ResultSortType
        {
            get
            {
                return this.resultSortType;
            }
            set
            {
                this.resultSortType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string FareType
        {
            get
            {
                return this.fareType;
            }
            set
            {
                this.fareType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string ClassOfServices
        {
            get
            {
                return this.classOfServices;
            }
            set
            {
                this.classOfServices = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int PageIndex
        {
            get
            {
                return this.pageIndex;
            }
            set
            {
                this.pageIndex = value;
            }
        }

        public int PageSize
        {
            get
            {
                return this.pageSize;
            }
            set
            {
                this.pageSize = value;
            }
        }

        //advanced search options
        public string FareClass
        {
            get
            {
                return this.fareClass;
            }
            set
            {
                this.fareClass = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }


        public string PromotionCode
        {
            get
            {
                return this.promotionCode;
            }
            set
            {
                this.promotionCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }


        public bool ShowMileageDetails
        {
            get
            {
                return this.showMileageDetails;
            }
            set
            {
                this.showMileageDetails = value;
            }
        }


        public int PremierStatusLevel
        {
            get
            {
                return this.premierStatusLevel;
            }
            set
            {
                this.premierStatusLevel = value;
            }
        }

        public string EmployeeDiscountId
        {
            get
            {
                return this.employeeDiscountId;
            }
            set
            {
                this.employeeDiscountId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
        private bool isReshop = false;
        public bool IsReshop
        {
            get { return isReshop; }
            set { isReshop = value; }
        }
        private string travelType;
        public string TravelType
        {
            get { return travelType; }
            set { travelType = value; }
        }
        private string flow;

        public string Flow
        {
            get { return flow; }
            set { flow = value; }
        }

        //Book With Travel Credit Session
        private string bwcsessionId = string.Empty;
        public string BWCSessionId
        {
            get
            {
                return this.bwcsessionId;
            }
            set
            {
                this.bwcsessionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
