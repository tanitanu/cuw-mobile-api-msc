using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Shopping.PriceBreakDown
{
     [Serializable()]
    public class MOBSHOPPriceBreakDown2Items
    {
        private string text1;
        private string price1;

        public string Text1
        {
            get
            {
                return this.text1;
            }
            set
            {
                this.text1 = value;
            }
        }

        public string Price1
        {
            get
            {
                return this.price1;
            }
            set
            {
                this.price1 = value;
            }
        }
    }
}
