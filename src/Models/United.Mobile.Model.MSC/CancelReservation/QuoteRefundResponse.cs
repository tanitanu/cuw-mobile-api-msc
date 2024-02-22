using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace United.Definition.CancelReservation
{
    [Serializable]
    public class QuoteRefundResponse
    {
        public string QuoteType { get; set; }
        public string Pnr { get; set; }
        public MOBPolicy Policy { get; set; }
        public virtual List<MOBPriceBreakDown> PriceBreakDown { get; set; }
        public MOBPayment FopDetails { get; set; }
        public MOBBasePrice RefundAmount { get; set; }
        public MOBBasePrice RefundMiles { get; set; }
        public MOBBasePrice RefundFee { get; set; }
        public virtual Collection<Error> Error { get; set; }
        public virtual Collection<Status> StatusDetails { get; set; }
        public MOBBasePrice AwardRedepositFee { get; set; }
        public MOBBasePrice AwardRedepositFeeTotal { get; set; }
    }
}
