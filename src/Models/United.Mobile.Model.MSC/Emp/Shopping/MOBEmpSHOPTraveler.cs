using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Emp.Shopping
{
    [Serializable()]
    public class MOBEmpSHOPTraveler
    {
        private long customerId;
        private string key = string.Empty;
        private MOBEmpSHOPLoyaltyProgramProfile loyaltyProgramProfile;
        private List<MOBPrefSpecialRequest> specialRequests;//******* TODO *******Need to decide to replicate or not to replicate
        private MOBEmpSHOPPerson person;
        private bool isProfileOwner;
        private bool isTSAFlagOn;
        private bool isFQTVNameMismatch;
        private string fqtvNameMismatch;
        private string travelerTypeCode = string.Empty;
        private int paxIndex;
        private string idType = string.Empty;
        private List<MOBSeat> seats;//******* TODO *******Need to decide to replicate or not to replicate
        private MOBTicket ticket;//******* TODO *******Need to decide to replicate or not to replicate
        private string sharesPosition = string.Empty;
        private List<United.Definition.MOBPrefAirPreference> airPreferences;//******* TODO *******Need to decide to replicate or not to replicate
        private List<United.Definition.MOBPrefContact> contacts;//******* TODO *******Need to decide to replicate or not to replicate

        public string SHARESPosition
        {
            get { return this.sharesPosition; }
            set { this.sharesPosition = value; }
        }

        public MOBTicket Ticket//******* TODO *******Need to decide to replicate or not to replicate
        {
            get { return this.ticket; }
            set { this.ticket = value; }
        }

        public long CustomerId
        {
            get
            {
                return this.customerId;
            }
            set
            {
                this.customerId = value;
            }
        }
        public string Key
        {
            get
            {
                return this.key;
            }
            set
            {
                this.key = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public MOBEmpSHOPLoyaltyProgramProfile LoyaltyProgramProfile
        {
            get
            {
                return this.loyaltyProgramProfile;
            }
            set
            {
                this.loyaltyProgramProfile = value;
            }
        }

        public List<MOBPrefSpecialRequest> SpecialRequests//******* TODO *******Need to decide to replicate or not to replicate
        {
            get
            {
                return this.specialRequests;
            }
            set
            {
                this.specialRequests = value;
            }
        }

        public MOBEmpSHOPPerson Person
        {
            get
            {
                return this.person;
            }
            set
            {
                this.person = value;
            }
        }

        public bool IsProfileOwner
        {
            get
            {
                return this.isProfileOwner;
            }
            set
            {
                this.isProfileOwner = value;
            }
        }

        public bool IsTSAFlagOn
        {
            get
            {
                return this.isTSAFlagOn;
            }
            set
            {
                this.isTSAFlagOn = value;
            }
        }

        public bool IsFQTVNameMismatch
        {
            get
            {
                return this.isFQTVNameMismatch;
            }
            set
            {
                this.isFQTVNameMismatch = value;
            }
        }

        public string FQTVNameMismatch
        {
            get
            {
                return this.fqtvNameMismatch;
            }
            set
            {
                this.fqtvNameMismatch = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string TravelerTypeCode
        {
            get
            {
                return this.travelerTypeCode;
            }
            set
            {
                this.travelerTypeCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public int PaxIndex
        {
            get { return paxIndex; }
            set { paxIndex = value; }
        }

        public string IdType
        {
            get { return idType; }
            set { idType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper(); }
        }

        public List<MOBSeat> Seats//******* TODO *******Need to decide to replicate or not to replicate
        {
            get
            {
                return this.seats;
            }
            set
            {
                this.seats = value;
            }
        }


        public List<United.Definition.MOBPrefAirPreference> AirPreferences//******* TODO *******Need to decide to replicate or not to replicate
        {
            get
            {
                return this.airPreferences;
            }
            set
            {
                this.airPreferences = value;
            }
        }


        public List<United.Definition.MOBPrefContact> Contacts//******* TODO *******Need to decide to replicate or not to replicate
        {
            get
            {
                return this.contacts;
            }
            set
            {
                this.contacts = value;
            }
        }
    }
}
