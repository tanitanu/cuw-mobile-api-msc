
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBUberProductsResponse : MOBResponse
    {
        public MOBUberProductsResponse()
            : base()
        {
        }

        private List<MOBUberProductDetails> products;

        public List<MOBUberProductDetails> Products
        {
            get { return this.products; }
            set
            {
                this.products = value;
            }
        }
    }


    [Serializable()]
    public class MOBUberProduct
    {
        public List<MOBUberProductDetails> Products { get; set; }
    }


}

