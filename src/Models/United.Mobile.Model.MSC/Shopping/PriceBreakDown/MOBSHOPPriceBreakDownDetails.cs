using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Shopping.PriceBreakDown
{
    [Serializable()]
    public class MOBSHOPPriceBreakDownDetails
    {
        private MOBSHOPPriceBreakDown2Items trip;
        private List<MOBSHOPPriceBreakDown2TextItems> taxAndFees;
        private MOBSHOPPriceBreakDownAddServices additionalServices;
        private List<MOBSHOPPriceBreakDown2Items> total;
        private List<MOBSHOPPriceBreakDown2Items> fareLock;

        public MOBSHOPPriceBreakDown2Items Trip
        {
            get
            {
                return trip;
            }
            set
            {
                trip = value;
            }
        }

        public List<MOBSHOPPriceBreakDown2TextItems> TaxAndFees
        {
            get
            {
                return taxAndFees;
            }
            set
            {
                taxAndFees = value;
            }
        }
    
        public MOBSHOPPriceBreakDownAddServices AdditionalServices
        {
            get
            {
                return additionalServices;
            }
            set
            {
                additionalServices = value;
            }
        }

        public List<MOBSHOPPriceBreakDown2Items> Total
        {
            get 
            {
                return total;
            }
            set
            {
                total = value;
            }
        }

        public List<MOBSHOPPriceBreakDown2Items> FareLock
        {
            get
            {
                return fareLock;
            }
            set
            {
                fareLock = value;
            }
        }
    
    }
}
