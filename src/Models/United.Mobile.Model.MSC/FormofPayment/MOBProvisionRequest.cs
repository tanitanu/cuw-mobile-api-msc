using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;

namespace United.Definition
{
    [Serializable()]
    public class MOBProvisionRequest : MOBRequest
    {
        private string provisionRequestIdentifier;
        private string sessionId = string.Empty;
        private int customerId;
        private string mileagePlusNumber = string.Empty;
        private string flow;
        private string partnerRequestIdentifier;
        private string accountReferenceIdentifier;
        private string linkageStatusCode;
        public string Flow
        {
            get
            {
                return this.flow;
            }
            set
            {
                this.flow = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
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

        public int CustomerId
        {
            get
            {
                return customerId;
            }
            set
            {
                this.customerId = value;
            }
        }
        public string ProvisionRequestIdentifier { get { return provisionRequestIdentifier; } set { this.provisionRequestIdentifier = value; } }
        public string PartnerRequestIdentifier { get { return partnerRequestIdentifier; } set { this.partnerRequestIdentifier = value; } }
        public string AccountReferenceIdentifier { get { return accountReferenceIdentifier; } set { this.accountReferenceIdentifier = value; } }
        public string LinkageStatusCode { get { return linkageStatusCode; } set { this.linkageStatusCode = value; } }
    }

    [Serializable()]
    public class ProvisionCSLRequest
    {
        public string PartnerRequestIdentifier { get; set; }
        public string SuccessRedirectUrl { get; set; }
        public string FailureRedirectUrl { get; set; }
        public string ChannelName { get; set; }
        public string MPNumber { get; set; }
    }

    [Serializable()]
    public class GetProvisionCSLRequest
    {
        public string ProvisionRequestIdentifier { get; set; }
        public string MPNumber { get; set; }
        public string ChannelName { get; set; }
        public string PartnerRequestIdentifier { get; set; }
    }

}
