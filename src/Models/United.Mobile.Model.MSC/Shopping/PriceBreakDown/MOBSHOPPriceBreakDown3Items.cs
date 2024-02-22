using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Shopping.PriceBreakDown
{
    [Serializable()]
    public class MOBSHOPPriceBreakDown3Items : MOBSHOPPriceBreakDown2Items
    {
        private string price2;
        public string Price2
        {
            get
            {
                return this.price2;
            }
            set
            {
                this.price2 = value;
            }
        }
    }
}
