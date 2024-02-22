using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.CSLModels.CustomerProfile
{
    public abstract class RequestBase
    {
        
        public string RecordLocator { get; set; }
    
        public string CustomerId { get; set; }
       
        public string LoyaltyId { get; set; }
       
        public string CartId { get; set; }

    }
}
