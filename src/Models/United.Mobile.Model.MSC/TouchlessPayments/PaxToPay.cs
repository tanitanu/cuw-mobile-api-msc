using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.TouchlessPayments
{
    public class PaxName
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
 
    public class PaxToPay : PaxName
    {   
        public OptInType OptInType { get; set; }
        public string OptInStatus { get; set; }
        public string CustomerType { get; set; }
        public CreditCard CreditCard { get; set; }
        public string PointOfSaleCountryCode { get; set; }
        public string LoyaltyProgramMemberID { get; set; }
        public bool IsLoyaltyProfileCard { get; set; }
    }
}
