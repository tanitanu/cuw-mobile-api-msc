using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable]
    public class MOBPBCompletePurchaseRequest: MOBRequest
    {
        private string sessionId = string.Empty;
        //private string recordLocator = string.Empty;
        //private string lastName = string.Empty;
        private string cardNumber = string.Empty; // XXXXXXXXX1234
        private string cid = string.Empty; // CVV
        private string ccHolderName = string.Empty;
        private string ccType = string.Empty;
        private string expMonth;
        private string expYear;
        private string emailAddress = string.Empty;
        private string eccNumber; // encrypted cc number 
        private string mileagePlusNumber;

        public string SessionId
        {
            get { return this.sessionId; }
            set { this.sessionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
        

        public string CCType
        {
            get { return this.ccType; }
            set { this.ccType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string CardNumber
        {
            get { return this.cardNumber; }
            set { this.cardNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string Cid
        {
            get { return this.cid; }
            set { this.cid = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string CCHolderName
        {
            get { return this.ccHolderName; }
            set { this.ccHolderName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string ExpMonth
        {
            get { return this.expMonth; }
            set { this.expMonth = value; }
        }

        public string ExpYear
        {
            get { return this.expYear; }
            set { this.expYear = value; }
        }

        public string EmailAddress
        {
            get { return this.emailAddress; }
            set { this.emailAddress = value; }
        }

        public string ECCNumber
        {
            get { return this.eccNumber; }
            set { this.eccNumber = value; }
        }

        public string MileagePlusNumber
        {
            get { return this.mileagePlusNumber; }
            set { this.mileagePlusNumber = value; }
        }
    }
}
