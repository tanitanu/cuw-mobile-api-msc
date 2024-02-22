using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace United.Definition.CSLModels.CustomerProfile
{
    public class ResponseTime
    {       
        //
        // Summary:
        //     HTTP transaction start datetime before calling an external system.
       
      
        public DateTime StartTime { get; set; }
        //
        // Summary:
        //     HTTP transaction end datetime after calling an external system.
       
      
        public DateTime EndTime { get; set; }
        //
        // Summary:
        //     Service Name for which the Start and End Time are listed.
       
      
        public string ServiceName { get; set; }
        //
        // Summary:
        //     EndTime - StartTime.
       
      
        public string Duration { get; set; }
    }
}
