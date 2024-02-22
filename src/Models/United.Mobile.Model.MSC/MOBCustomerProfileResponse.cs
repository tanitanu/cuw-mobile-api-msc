using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable]
    public class MOBCustomerProfileResponse:MOBResponse
    {
        private string sessionId = string.Empty;
        private List<MOBCPProfile> profiles;
        private List<MOBItem> insertUpdateKeys;
        private string mileagePlusNumber = string.Empty;
        //private bool isMPNameMisMatch = false;
        private MOBCPTraveler traveler;
        private string transactionPath = string.Empty;
        private string redirectViewName = string.Empty;
        private string pKDispenserPublicKey = string.Empty;
        private MOBCreditCard selectedCreditCard;
        private MOBAddress selectedAddress;
        private string cartId = string.Empty;

        public MOBCreditCard SelectedCreditCard
        {
            set { selectedCreditCard = value; }
            get { return selectedCreditCard; }
        }

        public MOBAddress SelectedAddress
        {
            set { selectedAddress = value; }
            get { return selectedAddress; }
        }

        public string PKDispenserPublicKey
        {
            get { return pKDispenserPublicKey; }
            set { pKDispenserPublicKey = value; }
        }

        public string RedirectViewName
        {
            get { return redirectViewName; }
            set { redirectViewName = value; }
        }
        public string TransactionPath
        {
            get { return transactionPath; }
            set { transactionPath = value; }
        }
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
        public List<MOBCPProfile> Profiles
        {
            get
            {
                return profiles;
            }
            set
            {
                this.profiles = value;
            }
        }

        public List<MOBItem> InsertUpdateKeys
        {
            get
            {
                return this.insertUpdateKeys;
            }
            set
            {
                this.insertUpdateKeys = value;
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
        //public bool ISMPNameMisMatch
        //{
        //    get
        //    {
        //        return this.isMPNameMisMatch;
        //    }
        //    set
        //    {
        //        this.isMPNameMisMatch = value;
        //    }
        //}
        public MOBCPTraveler Traveler
        {
            get
            {
                return this.traveler;
            }
            set
            {
                this.traveler = value;
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
    }
}
