using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.TouchlessPayments
{
 
    public class CreditCard
    {
        public string PersistentToken { get; set; }
        public string SecureCodeToken { get; set; }
        public string ExpiryMonthYear { get; set; }
        public string CreditCardCode { get; set; }
        public string CurrencyCode { get; set; }
        public string AccountNumberToken { get; set; }
        public string AccountNumberLastFourDigits { get; set; }
        public string NameOnCard { get; set; }
        public Address Address { get; set; }

    }
}
