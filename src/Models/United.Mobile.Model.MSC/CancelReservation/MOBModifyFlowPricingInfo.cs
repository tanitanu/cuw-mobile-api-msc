using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.CancelReservation
{
    [Serializable]
    public class MOBModifyFlowPricingInfo
    {
        private string totalPaid;
        private string taxesAndFeesTotal;
        private List<MOBModifyFlowPricePerType> pricesPerTypes;
        private string quoteType;
        private string currencyCode;
        private string refundMiles;
        private string totalDue;
        private string refundMilesLabel = string.Empty;
        private string refundFOPLabel = string.Empty;
        private bool hasTotalDue = false;
        private double redepositFee;
        private string formattedPricingDetail = string.Empty;
        

        public string TotalPaid
        {
            get { return totalPaid; }
            set { totalPaid = value; }
        }

        public string TaxesAndFeesTotal
        {
            get { return taxesAndFeesTotal; }
            set { taxesAndFeesTotal = value; }
        }

        public List<MOBModifyFlowPricePerType> PricesPerTypes
        {
            get { return pricesPerTypes; }
            set { pricesPerTypes = value; }
        }

        public string QuoteType
        {
            get { return quoteType; }
            set { quoteType = value; }
        }

        public string CurrencyCode
        {
            get { return currencyCode; }
            set { currencyCode = value; }
        }

        public string RefundMiles
        {
            get { return refundMiles; }
            set { refundMiles = value; }
        }

        public string TotalDue
        {
            get { return totalDue; }
            set { totalDue = value; }
        }       

        public string RefundMilesLabel
        {
            get { return refundMilesLabel; }
            set { refundMilesLabel = value; }
        }
        public string RefundFOPLabel
        {
            get { return refundFOPLabel; }
            set { refundFOPLabel = value; }
        }
        public bool HasTotalDue
        {
            get { return hasTotalDue; }
            set { hasTotalDue = value; }
        }

        public double RedepositFee
        {
            get { return redepositFee; }
            set { redepositFee = value; }
        }

        public string FormattedPricingDetail
        {
            get { return formattedPricingDetail;}
            set { formattedPricingDetail = value; }
        }
    }
}
