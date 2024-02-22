using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping.TripInsurance
{
    [Serializable]
    public class MOBTPIPurchaseInViewResRequest : MOBRequest
    {
        private string sessionId = string.Empty;
        private string cid = string.Empty; // cvv number i.e. 123
        private string ccHolderName = string.Empty; // test tester 
        private string ccCode = string.Empty;// VI 
        private string expMonth; // 05
        private string expYear; // 2022
        private string emailAddress = string.Empty;
        private MOBAddress address;
        private string cardNumber = string.Empty; // cc number 
        private string eccNumber = string.Empty;// encrypted cc number 
        private string ccType = string.Empty;// cc
        private string recordLocator = string.Empty;
        private string lastName = string.Empty;

        public string SessionId
        {
            get { return this.sessionId; }
            set { this.sessionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string Cid
        {
            get { return this.cid; }
            set { this.cid = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string CcHolderName
        {
            get { return this.ccHolderName; }
            set { this.ccHolderName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string CcCode
        {
            get { return this.ccCode; }
            set { this.ccCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string ExpMonth
        {
            get { return this.expMonth; }
            set { this.expMonth = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string ExpYear
        {
            get { return this.expYear; }
            set { this.expYear = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string EmailAddress
        {
            get { return this.emailAddress; }
            set { this.emailAddress = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public MOBAddress Address
        {
            get { return this.address; }
            set { this.address = value; }
        }
       
        public string CardNumber
        {
            get { return this.cardNumber; }
            set { this.cardNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string EccNumber
        {
            get { return this.eccNumber; }
            set { this.eccNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string CcType
        {
            get { return this.ccType; }
            set { this.ccType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string RecordLocator
        {
            get { return this.recordLocator; }
            set { this.recordLocator = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string LastName
        {
            get { return this.lastName; }
            set { this.lastName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
    }
}
