using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.TouchlessPayments
{
    public class PaxWalletLookupRequest
    {
        public string RecordLocator { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

}
