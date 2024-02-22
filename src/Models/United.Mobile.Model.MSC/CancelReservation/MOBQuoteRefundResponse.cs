using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.CancelReservation
{
    [Serializable]
    public class MOBQuoteRefundResponse : MOBResponse
    {
        private string quoteType;
        public string QuoteType
        {
            get { return quoteType; }
            set { quoteType = value; }
        }

        private MOBPolicy policy;
        public MOBPolicy Policy {
            get { return policy; }
            set { policy = value; }
        }

        private List<MOBPriceBreakDown> priceBreakDown; 
        public List<MOBPriceBreakDown> PriceBreakDown
        {
            get { return priceBreakDown; }
            set { priceBreakDown = value; }
        }

        private MOBPayment fopDetails;
        public MOBPayment FopDetails
        {
            get { return fopDetails; }
            set { fopDetails = value; }
        }

        private MOBBasePrice refundAmount;
        public MOBBasePrice RefundAmount
        {
            get { return refundAmount; }
            set { refundAmount = value; }
        }

        private MOBBasePrice refundMiles;
        public MOBBasePrice RefundMiles
        {
            get { return refundMiles; }
            set { refundMiles = value; }
        }

        private MOBBasePrice refundFee;
        public MOBBasePrice RefundFee
        {
            get { return refundFee; }
            set { refundFee = value; }
        }

        private MOBBasePrice awardRedepositFee;

        public MOBBasePrice AwardRedepositFee
        {
            get { return awardRedepositFee; }
            set { awardRedepositFee = value; }
        }

        private MOBBasePrice awardRedepositFeeTotal;

        public MOBBasePrice AwardRedepositFeeTotal
        {
            get { return awardRedepositFeeTotal; }
            set { awardRedepositFeeTotal = value; }
        }
    }
}
