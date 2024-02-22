using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBClubResponse : MOBResponse
    {
        private string airportCode = string.Empty;
        private string clubType = string.Empty;
        private bool bfcOnly;
        private List<MOBClub> clubs;

        public MOBClubResponse()
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

        public List<MOBClub> Clubs
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
}
