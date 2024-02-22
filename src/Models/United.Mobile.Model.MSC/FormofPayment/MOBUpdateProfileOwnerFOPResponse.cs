using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBUpdateProfileOwnerFOPResponse : MOBResponse
    {
        private string sessionId = string.Empty;
        private string flow = string.Empty;
        private MOBShoppingCart shoppingCart;
        private List<FormofPaymentOption> eligibleFormofPayments;
        private List<MOBCPProfile> profiles;
        private List<MOBItem> insertUpdateKeys;
        private MOBCreditCard selectedCreditCard;
        private MOBAddress selectedAddress;
        private string pkDispenserPublicKey;
        private string mileagePlusNumber;

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

        public string SessionId
        {
            get { return sessionId; }
            set { sessionId = value; }
        }
        public string Flow
        {
            get { return flow; }
            set { flow = value; }
        }
        public MOBShoppingCart ShoppingCart
        {
            get { return shoppingCart; }
            set { shoppingCart = value; }
        }
        public List<FormofPaymentOption> EligibleFormofPayments
        {
            get { return eligibleFormofPayments; }
            set { eligibleFormofPayments = value; }
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
        public string PkDispenserPublicKey
        {
            get { return pkDispenserPublicKey; }
            set { pkDispenserPublicKey = value; }
        }
        public string MileagePlusNumber
        {
            get { return mileagePlusNumber; }
            set { this.mileagePlusNumber = value; }
        }
    }
}
