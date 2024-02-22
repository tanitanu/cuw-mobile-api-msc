using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Shopping
{
    [Serializable]
    public class MOBSHOPDataCarrier
    {
        private string searchType = string.Empty;
        public string SearchType
        {
            get { return searchType; }
            set { searchType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
        private string priceFormText = string.Empty;// -- this is for Products Methods will use in BE & Compare Screen
        public string PriceFormText
        {
            get { return priceFormText; }
            set { priceFormText = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
        private decimal fsrMinPrice = 0;

        public decimal FsrMinPrice
        {
            get { return fsrMinPrice; }
            set { fsrMinPrice = value; }
        }

    }
}
