using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.ClubMatchOLDREST
{
    [Serializable()]
    public class MOBUnitedClubResponseToMatchOLDREST : ResponseToMatchOLDREST
    {
        private string airportCode = string.Empty;
        private string clubType = string.Empty;
        private bool bfcOnly;
        private List<Club> clubs;

        public MOBUnitedClubResponseToMatchOLDREST()
            : base()
        {
        }

        public string AirportCode
        {
            get
            {
                return this.airportCode;
            }
            set
            {
                this.airportCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ClubType
        {
            get
            {
                return this.clubType;
            }
            set
            {
                this.clubType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool BFCOnly
        {
            get
            {
                return this.bfcOnly;
            }
            set
            {
                this.bfcOnly = value;
            }
        }

        public List<Club> Clubs
        {
            get
            {
                return this.clubs;
            }
            set
            {
                this.clubs = value;
            }
        }
    }

    [Serializable()]
    public class ResponseToMatchOLDREST
    {
        private string transactionId = string.Empty;
        private string languageCode = string.Empty;
        private string machineName = System.Environment.MachineName;
        private Exception exception;
        private long callDuration;

        private bool pss = false;

        private string area = string.Empty;

        public string TransactionId
        {
            get
            {
                return this.transactionId;
            }
            set
            {
                this.transactionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string LanguageCode
        {
            get
            {
                return this.languageCode;
            }
            set
            {
                this.languageCode = string.IsNullOrEmpty(value) ? "en-US" : value.Trim();
            }
        }

        public string MachineName
        {
            get
            {
                return this.machineName;
            }
            set
            {
                this.machineName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public Exception Exception
        {
            get
            {
                return this.exception;
            }
            set
            {
                this.exception = value;
            }
        }

        public long CallDuration
        {
            get
            {
                return this.callDuration;
            }
            set
            {
                this.callDuration = value;
            }
        }

        public bool PSS
        {
            get
            {
                return this.pss;
            }
            set
            {
                this.pss = value;
            }
        }

        public string Area
        {
            get
            {
                return this.area;
            }
            set
            {
                this.area = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
    }

    [Serializable]
    public class MOBAllClubResponseToMatchOLDREST : ResponseToMatchOLDREST
    {
        private string airportCode = string.Empty;
        private string clubType = string.Empty;
        private bool bfcOnly;
        private List<Club> clubs;

        public MOBAllClubResponseToMatchOLDREST()
            : base()
        {
        }

        public string AirportCode
        {
            get
            {
                return this.airportCode;
            }
            set
            {
                this.airportCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ClubType
        {
            get
            {
                return this.clubType;
            }
            set
            {
                this.clubType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool BFCOnly
        {
            get
            {
                return this.bfcOnly;
            }
            set
            {
                this.bfcOnly = value;
            }
        }

        public List<Club> Clubs
        {
            get
            {
                return this.clubs;
            }
            set
            {
                this.clubs = value;
            }
        }
    }

    [Serializable()]
    public class MOBUnitedAndStarLoungesResponse : MOBResponse
    {
        private string airportCode = string.Empty;
        private List<MOBUnitedStarLoungesLocations> unitedStarLougesLocations;
        private string airportClubTitle = string.Empty; //"Locations at ORD"

        public MOBUnitedAndStarLoungesResponse()
            : base()
        {
        }

        public string AirportCode
        {
            get
            {
                return this.airportCode;
            }
            set
            {
                this.airportCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MOBUnitedStarLoungesLocations> UnitedStarLougesLocations
        {
            get
            {
                return this.unitedStarLougesLocations;
            }
            set
            {
                this.unitedStarLougesLocations = value;
            }
        }

        public string AirportClubTitle
        {
            get
            {
                return this.airportClubTitle;
            }
            set
            {
                this.airportClubTitle = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }

    [Serializable()]
    public class MOBUnitedStarLoungesLocations
    {
        private string clubType = string.Empty;// "UnitedLounge","STARLOUNGE"
        private List<Club> clubs;
        private string clubTypeTitle = string.Empty; // "United Club and other United Lounges" / "Star Allicance-affiliated lounges" For testing purpose try to send a register trade mark to check how the client render the trade mark at iOS & Andriod

        public string ClubType
        {
            get
            {
                return this.clubType;
            }
            set
            {
                this.clubType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<Club> Clubs
        {
            get
            {
                return this.clubs;
            }
            set
            {
                this.clubs = value;
            }
        }

        public string ClubTypeTitle
        {
            get
            {
                return this.clubTypeTitle;
            }
            set
            {
                this.clubTypeTitle = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
