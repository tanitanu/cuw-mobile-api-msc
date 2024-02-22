using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace United.Definition.CSLModels.CustomerProfile
{

    public class PartnerCreditCard
    {
        public string MileagePlusId { get; set; }      
        public string PartnerCode { get; set; }    
        public string CardType { get; set; }  
        public string CardTypeDescription { get; set; }     
        public string ProductType { get; set; }  
        public string ARN { get; set; }        
        public DateTime? AF_BillingDate { get; set; }   
        public string AccountCode { get; set; }   
        public string Key { get; set; }   
        public DateTime? OpenDate { get; set; }  
        public DateTime? ExpireDate { get; set; }    
        public DateTime? CreatedTimestamp { get; set; }
    }

}
