using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;

namespace United.Mobile.Model.MSC.FormofPayment
{
    public class MOBRTIMileagePricingRequest: MOBRequest
    {
        private string cartId;

        public string CartId
        {
            get { return cartId; }
            set { cartId = value; }
        }

        private string sessionId;

        public string SessionId
        {
            get { return sessionId; }
            set { sessionId = value; }
        }

        private string hashPinCode;

        public string HashPinCode
        {
            get { return hashPinCode; }
            set { hashPinCode = value; }
        }

        private string mileagePlusAccountNumber;

        public string MileagePlusAccountNumber
        {
            get { return mileagePlusAccountNumber; }
            set { mileagePlusAccountNumber = value; }
        }

        private int premierStatusLevel;

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

        private string mileageBalance;

        public string MileageBalance
        {
            get { return mileageBalance; }
            set { mileageBalance = value; }
        }
        private string pricingType;

        public string PricingType // ETC 
        {
            get { return pricingType; }
            set { pricingType = value; }
        }
        private string creditsAmount;

        public string CreditsAmount
        {
            get { return creditsAmount; }
            set { creditsAmount = value; }
        }

        private string currentPricingType;

        public string CurrentPricingType    // MONEY or MONEYPLUSMILES
        {
            get { return currentPricingType; }
            set { currentPricingType = value; }
        }
    }
    public enum PricingType
    {
        NONE,
        MONEYPLUSMILES,
        ETC,
        FFC,
        STRICKTHROUGH,
        MONEY
    }
}
