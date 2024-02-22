using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Shopping.PriceBreakDown
{
    [Serializable()]
    public class MOBSHOPPriceBreakDownAddServices
    {

        private List<MOBSHOPPriceBreakDown4Items> seats;
        private List<MOBSHOPPriceBreakDown3Items> premiumAccess;
        private List<MOBSHOPPriceBreakDown2Items> oneTimePass;

        public List<MOBSHOPPriceBreakDown4Items> Seats
        {
            get
            {
                return this.seats;
            }
            set
            {
                this.seats = value;
            }
        }

        public List<MOBSHOPPriceBreakDown3Items> PremiumAccess
        {
            get
            {
                return this.premiumAccess;
            }
            set
            {
                this.premiumAccess = value;
            }
        }

        public List<MOBSHOPPriceBreakDown2Items> OneTimePass
        {
            get
            {
                return this.oneTimePass;
            }
            set
            {
                this.oneTimePass = value;
            }
        }


    }
}
