
using System;
using United.Service.Presentation.CommonModel;

namespace United.Definition.FFC
{
    [Serializable()]
    public class LookupByFreqFlyerNumRequest
    {        
        public virtual string FreqFlyerNum { get; set; } 
        public virtual ServiceClient CallingService { get; set; }
    }
}