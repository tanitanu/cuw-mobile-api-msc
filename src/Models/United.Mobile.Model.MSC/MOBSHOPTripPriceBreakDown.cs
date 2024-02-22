using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Shopping.PriceBreakDown;

namespace United.Definition.Shopping
{
    [Serializable()]
    public class MOBSHOPTripPriceBreakDown
    {

        private MOBSHOPPriceBreakDownSummary priceBreakDownSummary;
        private MOBSHOPPriceBreakDownDetails priceBreakDownDetails;

        public MOBSHOPPriceBreakDownSummary PriceBreakDownSummary
        {
            get
            {
                return this.priceBreakDownSummary;
            }
            set
            {
                this.priceBreakDownSummary = value;
            }
        }

        public MOBSHOPPriceBreakDownDetails PriceBreakDownDetails
        {
            get
            {
                return priceBreakDownDetails;
            }
            set
            {
                priceBreakDownDetails = value;

            }

        }
    }
}
