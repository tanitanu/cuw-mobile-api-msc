using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping
{
    [Serializable()]
    public class MOBRegisterTravelersRequest : MOBRequest
    {
        private string sessionId = string.Empty;
        private string cartId = string.Empty;
        private string token = string.Empty;
        private string profileKey = string.Empty;
        private int profileId;
        private int profileOwnerId;
        private string profileOwnerKey = string.Empty;
        private string mileagePlusNumber = string.Empty;
        private MOBCPTraveler profileOwner;
        private List<MOBCPTraveler> travelers;
        private string flow;

        public string SessionId
        {
            get
            {
                return sessionId;
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

        public string Token
        {
            get
            {
                return token;
            }
            set
            {
                this.token = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ProfileKey
        {
            get
            {
                return this.profileKey;
            }
            set
            {
                this.profileKey = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int ProfileId
        {
            get
            {
                return this.profileId;
            }
            set
            {
                this.profileId = value;
            }
        }

        public int ProfileOwnerId
        {
            get
            {
                return this.profileOwnerId;
            }
            set
            {
                this.profileOwnerId = value;
            }
        }

        public string ProfileOwnerKey
        {
            get
            {
                return this.profileOwnerKey;
            }
            set
            {
                this.profileOwnerKey = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string MileagePlusNumber
        {
            get
            {
                return mileagePlusNumber;
            }
            set
            {
                this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public MOBCPTraveler ProfileOwner
        {
            get
            {
                return this.profileOwner;
            }
            set
            {
                this.profileOwner = value;
            }
        }

        public List<MOBCPTraveler> Travelers
        {
            get
            {
                return this.travelers;
            }
            set
            {
                this.travelers = value;
            }
        }
        public string Flow
        {
            get
            {
                return this.flow;
            }
            set
            {
                this.flow = value;
            }
        }


        public MOBRegisterTravelersRequest()
            : base()
        {
        }
    }
}
