using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace United.Definition.CSLModels.CustomerProfile
{
    public class PremierQualifyingMetricsItem
    {

       
        public string ProgramCurrency { get; set; }
      
        public decimal? Balance { get; set; }
   
        public int QualifyingYear { get; set; }
    }
}
