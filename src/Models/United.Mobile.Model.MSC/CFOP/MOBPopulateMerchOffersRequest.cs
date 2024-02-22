using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.CFOP
{
    public class MOBPopulateMerchOffersRequest : MOBRequest
    {
        public string ProductOfferStr { get; set; }
        public string ShoppingSessionId { get; set; }
    }
}
