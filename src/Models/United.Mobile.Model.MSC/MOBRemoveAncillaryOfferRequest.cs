using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    public class MOBRemoveAncillaryOfferRequest : MOBShoppingRequest
    {
        private List<MOBProdDetail> products;

        public List<MOBProdDetail> Products
        {
            get { return products; }
            set { products = value; }
        }
    }
}
