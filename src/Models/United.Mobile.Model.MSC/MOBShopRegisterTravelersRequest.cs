using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBShopRegisterTravelersRequest : MOBRequest
    {
        private string sessionId = string.Empty;
        private string cartId = string.Empty;
        private List<MOBResTraveler> travelers;
        private List<MOBComTelephone> phones;
        private bool lastTraveler;

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

        public List<MOBResTraveler> Travelers
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

        public List<MOBComTelephone> Phones
        {
            get
            {
                return this.phones;
            }
            set
            {
                this.phones = value;
            }
        }

        public bool LastTraveler
        {
            get
            {
                return this.lastTraveler;
            }
            set
            {
                this.lastTraveler = value;
            }
        }
    }
}
