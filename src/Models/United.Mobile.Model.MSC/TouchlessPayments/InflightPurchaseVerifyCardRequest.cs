using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.TouchlessPayments
{
    public class InflightPurchaseVerifyCardRequest
    {
        public string recordLocator { get; set; }
        public List<PaxName> PaxNames { get; set; }
        public List<Segments> Segments { get; set; }
        public bool IsReplacingCard { get; set; }
    }
}
