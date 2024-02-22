using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Shopping;
using United.Mobile.Model.Common;
namespace United.Definition.Pcu
{
    [Serializable()]
    public class MOBPremiumCabinUpgradeRequest : MOBRequest
    {
        private string sessionId;
        private List<string> selectedSegmentIds;
        private MOBSHOPFormOfPayment formOfPayment;
        private MOBAddress billingAddress;
        private MOBCPPhone phone;
        private string emailAddress;

        public string SessionId
        {
            get { return sessionId; }
            set { sessionId = value; }
        }

        public List<string> SelectedSegmentIds
        {
            get { return selectedSegmentIds; }
            set { selectedSegmentIds = value; }   
        }
        
        public MOBSHOPFormOfPayment FormOfPayment
        {
            get { return formOfPayment; }
            set { formOfPayment = value; }
        }

        public MOBAddress BillingAddress
        {
            get { return billingAddress; }
            set { billingAddress = value; }
        }

        public MOBCPPhone Phone
        {
            get { return phone; }
            set { phone = value; }
        }

        public string EmailAddress
        {
            get { return emailAddress; }
            set { emailAddress = value; }
        }
    }
}
