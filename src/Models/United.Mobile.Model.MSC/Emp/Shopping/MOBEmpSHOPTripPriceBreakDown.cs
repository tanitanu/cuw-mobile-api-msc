using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Emp.Shopping
{
    [Serializable()]
    public class MOBEmpSHOPTripPriceBreakDown
    {
        private List<MOBSHOPPriceBreakDownRow> priceBreakDownDetails;
        private List<MOBSHOPPriceBreakDownRow> priceBreakDownSummary;

        public List<MOBSHOPPriceBreakDownRow> PriceBreakDownDetails
        {
            get
            {
                return this.priceBreakDownDetails;
            }
            set
            {
                this.priceBreakDownDetails = value;
            }
        }
        public List<MOBSHOPPriceBreakDownRow> PriceBreakDownSummary
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
    }
    [Serializable()]
    public class MOBSHOPPriceBreakDownRow
    {
        private string leftItem;
        private string rightItem;

        public string LeftItem
        {
            get
            {
                return this.leftItem;
            }
            set
            {
                this.leftItem = value;
            }
        }

        public string RightItem
        {
            get
            {
                return this.rightItem;
            }
            set
            {
                this.rightItem = value;
            }
        }
    }
}
