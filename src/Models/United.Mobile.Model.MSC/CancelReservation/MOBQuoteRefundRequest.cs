using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.CancelReservation
{
    [Serializable]
    public class MOBQuoteRefundRequest : MOBRequest
    {
        public string RecordLocator;
    }
}
