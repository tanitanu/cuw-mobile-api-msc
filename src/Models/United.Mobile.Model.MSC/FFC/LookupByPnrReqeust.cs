using System;
using System.Collections.Generic;
using United.Service.Presentation.CommonModel;

namespace United.Definition.FFC
{
    [Serializable()]
    public class LookupByPnrRequest
    {
        public string OrigPNR { get; set; }

        public string LastName { get; set; }
        public string CartId { get; set; }
        public List<PaxDetail> PaxDetails { get; set; }
        public string StationID { get; set; }
        public string DutyCode { get; set; }
        public string AgentSine { get; set; }
        public string LineIATA { get; set; }
        public string AccessCode { get; set; }
        public string PNRCreateDate { get; set; }

        public ServiceClient CallingService { get; set; }
    }
}
