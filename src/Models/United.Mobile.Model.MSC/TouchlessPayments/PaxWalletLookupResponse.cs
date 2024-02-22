using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 
namespace United.Definition.TouchlessPayments
{
    public class PaxWalletLookupResponse
    {
        public string RecordLocator { get; set; }
        public List<SegmentsToPay> SegmentsToPay { get; set; }
        public List<CslError> Errors { get; set; }
    }
}
