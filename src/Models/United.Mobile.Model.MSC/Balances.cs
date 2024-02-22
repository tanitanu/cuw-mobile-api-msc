using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace United.Definition.CSLModels.CustomerProfile
{
    public class Balances
    {

        //
        // Summary:
        //     Balance amount.       
        public decimal Amount { get; set; }
        //
        // Summary:
        //     Expiry date for balance     
        public DateTime? ExpirationDate { get; set; }
        //
        // Summary:
        //     Program currecy  
        public Constants.ProgramCurrencyType Currency { get; set; }
        //
        // Summary:
        //     Subbalances     
        public List<SubBalances> SubBalances { get; set; }
    }

}
