using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable]
    public class MOBPACompletePurchaseRequest : MOBRequest
    {
        private string sessionId = string.Empty;
        private string recordLocator = string.Empty;
        private string lastName = string.Empty;

        private string selectedCustomerInSegments = string.Empty;
        private string cardNumber = string.Empty;
        private string cid = string.Empty;
        private string ccHolderName = string.Empty;
        private string ccType = string.Empty;
        private int expMonth;
        private int expYear;
        private string emailAddress = string.Empty;
        private MOBAddress address;
        //Added by Nizam - Task#179873 - 07/27/2017
        private string mileagePlusNumber;
        private string sessionGuID;
        private string eccNumber;

        public string SessionId
        {
            get { return this.sessionId; }
            set { this.sessionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string RecordLocator
        {
            get { return recordLocator; }
            set { recordLocator = value; }
        }

        public string LastName
        {
            get { return lastName; }
            set { lastName = value; }
        }

        public string SelectedCustomerInSegments
        {
            get { return this.selectedCustomerInSegments; }
            set { this.selectedCustomerInSegments = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
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

        public int ExpMonth
        {
            get { return this.expMonth; }
            set { this.expMonth = value; }
        }

        public int ExpYear
        {
            get { return this.expYear; }
            set { this.expYear = value; }
        }

        public string EmailAddress
        {
            get { return this.emailAddress; }
            set { this.emailAddress = value; }
        }

        public MOBAddress Address
        {
            get { return this.address; }
            set { this.address = value; }
        }

        //Added by Nizam - Task#179873 - 07/27/2017
        public string MileagePlusNumber
        {
            get { return this.mileagePlusNumber; }
            set { this.mileagePlusNumber = value; }
        }

        public string SessionGuID
        {
            get { return this.sessionGuID; }
            set { this.sessionGuID = value; }
        }

        public string EccNumber
        {
            get { return this.eccNumber; }
            set { this.eccNumber = value; }
        }
    }
}
