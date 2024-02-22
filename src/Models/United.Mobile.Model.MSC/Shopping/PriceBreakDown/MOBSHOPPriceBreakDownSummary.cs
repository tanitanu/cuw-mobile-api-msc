using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Shopping.PriceBreakDown
{
    [Serializable()]
    public class MOBSHOPPriceBreakDownSummary
    {
        private List<MOBSHOPPriceBreakDown2Items> trip;

        public List<MOBSHOPPriceBreakDown2Items> Trip
        {
            get
            {
                return this.trip;
            }
            set
            {
                this.trip = value;
            }
        }

        private List<MOBSHOPPriceBreakDown2Items> travelOptions;

        public List<MOBSHOPPriceBreakDown2Items> TravelOptions
        {
            get
            {
                return this.travelOptions;
            }
            set
            {
                this.travelOptions = value;
            }
        }

        private List<MOBSHOPPriceBreakDown2Items> total;

        public List<MOBSHOPPriceBreakDown2Items> Total
        {
            get
            {
                return this.total;
            }
            set
            {
                this.total = value;
            }
        }

        private MOBSHOPPriceBreakDown2Items fareLock;

        public MOBSHOPPriceBreakDown2Items FareLock
        {
            get
            {
                return this.fareLock;
            }
            set
            {
                this.fareLock = value;
            }
        }
    }
}
