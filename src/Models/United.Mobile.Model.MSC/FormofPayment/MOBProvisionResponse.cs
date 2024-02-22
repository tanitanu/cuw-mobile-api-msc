using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;

namespace United.Definition
{
     [Serializable()]
    public class MOBProvisionResponse : MOBResponse
    {
        public string ProvisionRequestIdentifier { get; set; }
        public string RedirectUrl { get; set; }
        public string SessionId { get; set; }
        public string PartnerRequestIdentifier { get; set; }

        private List<Errors> errors;
        public List<Errors> Errors { get { return this.errors; } set { this.errors = value; } }
    }

    [Serializable()]
    public class ProvisionCSLResponse 
    {
        public string ProvisionRequestIdentifier { get; set; }
        public string RedirectUrl { get; set; }
    }

    [Serializable()]
    public class MOBProvisionCSLResponse
    {
        public string ProvisionRequestIdentifier { get; set; }
        public string RedirectUrl { get; set; }
    }

    [Serializable()]
    public class MOBUpdateProvisionLinkStatusResponse : MOBResponse
    {
        private string statusCode;
        private string partnerRequestIdentifier;
        private string referenceIdentifier;
        private string sessionId;
        private List<Errors> errors;

        public string StatusCode
        {
            get
            {
                return statusCode;
            }
            set
            {
                this.statusCode = value;
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
                this.sessionId = value;
            }
        }
        public string PartnerRequestIdentifier { get { return partnerRequestIdentifier; } set { this.partnerRequestIdentifier = value; } }
        public string ReferenceIdentifier { get { return referenceIdentifier; } set { this.referenceIdentifier = value; } }
        public List<Errors> Errors { get { return this.errors; } set { this.errors = value; } }
    }

}
