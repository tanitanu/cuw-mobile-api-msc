using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.TouchlessPayments
{
    public class CSLProfileAddressResponse
    {
        public List<CSLProfileAddress> AddressData { get; set; }
        public List<CslError> Errors { get; set; }
    }

    public class CSLProfileAddress
    {
        public string Key { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string City { get; set; }
        public string StateCode { get; set; }
        public string PostalCode { get; set; }
        public string CountryCode { get; set; }
        
    }
}
