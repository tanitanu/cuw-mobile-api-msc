using System;
using United.Service.Presentation.CommonModel;

namespace United.Definition.FFC
{
    [Serializable]
    public class LookupByEmailRequest
    {
        public string FrequentFreqFlyerNum { get; set; }

        public string TicketNumber { get; set; }

        public string EmailID { get; set; }

        public virtual ServiceClient CallingService { get; set; }
    }
}
