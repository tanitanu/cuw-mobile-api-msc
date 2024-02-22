using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.FormofPayment
{
    [Serializable()]
    public class MOBInflightProductInfo
    {
        private string productDetail;
        private string flights;


        public string ProductDetail
        {
            get
            {
                return this.productDetail;
            }
            set
            {
                this.productDetail = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string Flights
        {
            get
            {
                return this.flights;
            }
            set
            {
                this.flights = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

    }
}
