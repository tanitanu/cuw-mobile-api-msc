using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping
{
    [Serializable]
     public class MOBPostBookingAncillaryConfirmationRequest: MOBRequest
    {
        private string sessionId = string.Empty;
        private MOBSHOPFormOfPayment formOfPayment;
        private MOBAddress address;
        private string emailAddress = string.Empty;
        private string mileagePlusNumber = string.Empty;

        public string SessionId
        {
            get { return this.sessionId; }
            set { sessionId = value; }
        }
        public MOBSHOPFormOfPayment FormOfPayment
        {
            get { return this.formOfPayment; }
            set { formOfPayment = value; }
        }
        public MOBAddress Address
        {
            get { return this.address; }
            set { address = value; }
        }
        public string EmailAddress
        {
            get { return this.emailAddress; }
            set { emailAddress = value; }
        }
        public string MileagePlusNumber
        {
            get { return this.mileagePlusNumber; }
            set { mileagePlusNumber = value; }
        }
    }
}
