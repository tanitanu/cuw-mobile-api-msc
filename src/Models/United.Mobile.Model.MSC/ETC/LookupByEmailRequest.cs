using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using United.Service.Presentation.CommonModel;

namespace United.Definition.ETC
{
    [Serializable]
    public class LookupByEmailRequest
    {
        public string PromoID { get; set; }
        public string CertPin { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string StationID { get; set; }
        public string DutyCode { get; set; }
        public string AgentSine { get; set; }
        public string LineIATA { get; set; }
        public string AccessCode { get; set; }
        public string EmailID { get; set; }
        public virtual ServiceClient CallingService { get; set; }
        public string MailTo { get; set; }
    }
}
