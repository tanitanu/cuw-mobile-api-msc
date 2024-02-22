using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable]
    public class MOBSeatChangeAssignCompleteRequest : MOBRequest
    {
        private string sessionId = string.Empty;
        private string cardNumber = string.Empty;
        private string cid = string.Empty;
        private string ccHolderName = string.Empty;
        private string ccType = string.Empty;
        private int expMonth;
        private int expYear;
        private string emailAddress = string.Empty;
        private MOBAddress address;
        private string eccNumber = string.Empty;
        private string mileagePlusNumber = string.Empty;
        private string sessionGuID = string.Empty;

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

        public string EccNumber
        {
            get { return this.eccNumber; }
            set { this.eccNumber = value; }
        }

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
    }
}
