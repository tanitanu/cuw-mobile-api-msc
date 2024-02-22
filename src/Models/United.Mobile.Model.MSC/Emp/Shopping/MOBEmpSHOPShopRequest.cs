using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Emp.Shopping
{
    [Serializable()]
    public class MOBEmpSHOPShopRequest : MOBRequest
    {
        private string sessionId = string.Empty;
        private string countryCode = "US";
        private List<MOBEmpSHOPTripBase> trips;
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
        private string maxConnections;
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

        public List<MOBEmpSHOPTripBase> Trips
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
        public string MaxConnections
        {
            get
            {
                return this.maxConnections;
            }
            set
            {
                this.maxConnections = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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
    }
}
