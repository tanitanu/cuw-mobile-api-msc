using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBShopRequest : MOBRequest
    {
        private string token = string.Empty;
        private string sessionId = string.Empty;
        private string countryCode = string.Empty;
        private string origin1 = string.Empty; 
        private string destination1 = string.Empty;
        private string departDate1 = string.Empty;
        private string cabin1 = string.Empty; 
        private string origin2 = string.Empty;
        private string destination2 = string.Empty;
        private string departDate2 = string.Empty; 
        private string cabin2 = string.Empty; 
        private string origin3 = string.Empty; 
        private string destination3 = string.Empty; 
        private string departDate3 = string.Empty; 
        private string cabin3 = string.Empty; 
        private string origin4 = string.Empty; 
        private string destination4 = string.Empty; 
        private string departDate4 = string.Empty; 
        private string cabin4 = string.Empty; 
        private string origin5 = string.Empty; 
        private string destination5 = string.Empty; 
        private string departDate5 = string.Empty; 
        private string cabin5 = string.Empty; 
        private string origin6 = string.Empty; 
        private string destination6 = string.Empty; 
        private string departDate6 = string.Empty;
        private string cabin6 = string.Empty;
        private int numberOfAdults;
        private int numberOfSeniors;
        private int numberOfChildren2To4;
        private int numberOfChildren5To11;
        private int numberOfChildren12To17;
        private int numberOfInfantOnLap;
        private int numberOfInfantWithSeat;
        private bool awardTravel;
        private string searchType = string.Empty;
        private string serviceType = string.Empty;
        private int maxNumberOfStops;
        private int maxNumberOfTrips;
        private string mileagePlusAccountNumber = string.Empty;
        private string resultSortType = string.Empty;

        public string Token
        {
            get
            {
                return this.token;
            }
            set
            {
                this.token = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
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

        public string CountryCode
        {
            get
            {
                if (string.IsNullOrEmpty(this.countryCode))
                {
                    this.countryCode = "US";
                }

                return this.countryCode;
            }
            set
            {
                this.countryCode = string.IsNullOrEmpty(value) ? "en-US" : value.Trim().ToUpper();
            }
        }

        public string Origin1
        {
            get
            {
                return this.origin1;
            }
            set
            {
                this.origin1 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string Destination1
        {
            get
            {
                return this.destination1;
            }
            set
            {
                this.destination1 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string DepartDate1
        {
            get
            {
                return this.departDate1;
            }
            set
            {
                this.departDate1 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Cabin1
        {
            get
            {
                return this.cabin1;
            }
            set
            {
                this.cabin1 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Origin2
        {
            get
            {
                return this.origin2;
            }
            set
            {
                this.origin2 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string Destination2
        {
            get
            {
                return this.destination2;
            }
            set
            {
                this.destination2 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string DepartDate2
        {
            get
            {
                return this.departDate2;
            }
            set
            {
                this.departDate2 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Cabin2
        {
            get
            {
                return this.cabin2;
            }
            set
            {
                this.cabin2 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Origin3
        {
            get
            {
                return this.origin3;
            }
            set
            {
                this.origin3 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string Destination3
        {
            get
            {
                return this.destination3;
            }
            set
            {
                this.destination3 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string DepartDate3
        {
            get
            {
                return this.departDate3;
            }
            set
            {
                this.departDate3 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Cabin3
        {
            get
            {
                return this.cabin3;
            }
            set
            {
                this.cabin3 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Origin4
        {
            get
            {
                return this.origin4;
            }
            set
            {
                this.origin4 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string Destination4
        {
            get
            {
                return this.destination4;
            }
            set
            {
                this.destination4 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string DepartDate4
        {
            get
            {
                return this.departDate4;
            }
            set
            {
                this.departDate4 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Cabin4
        {
            get
            {
                return this.cabin4;
            }
            set
            {
                this.cabin4 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Origin5
        {
            get
            {
                return this.origin5;
            }
            set
            {
                this.origin5 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string Destination5
        {
            get
            {
                return this.destination5;
            }
            set
            {
                this.destination5 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string DepartDate5
        {
            get
            {
                return this.departDate5;
            }
            set
            {
                this.departDate5 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Cabin5
        {
            get
            {
                return this.cabin5;
            }
            set
            {
                this.cabin5 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Origin6
        {
            get
            {
                return this.origin6;
            }
            set
            {
                this.origin6 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string Destination6
        {
            get
            {
                return this.destination6;
            }
            set
            {
                this.destination6 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string DepartDate6
        {
            get
            {
                return this.departDate6;
            }
            set
            {
                this.departDate6 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Cabin6
        {
            get
            {
                return this.cabin6;
            }
            set
            {
                this.cabin6 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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

        public string ResultSortType
        {
            get
            {
                return this.resultSortType;
            }
            set
            {
                this.resultSortType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
