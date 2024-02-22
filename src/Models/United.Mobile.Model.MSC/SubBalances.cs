using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace United.Definition.CSLModels.CustomerProfile
{
    [DataContract]
    public class SubBalances
    {
        //
        // Summary:
        //     Balance Amount associated with a particular program currency
   
        public decimal Amount { get; set; }
        //
        // Summary:
        //     Source of the fund
      
        public string Type { get; set; }
    }
}
