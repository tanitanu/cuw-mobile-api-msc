using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping
{
    [Serializable]
    public class MOBSHOPProductSearchResponse : MOBResponse
    {
        private string sessionId = string.Empty;
        private string cartId = string.Empty;
        private MOBSHOPOfferSource offerSource;
        private List<MOBShopBundleProducts> bundleProducts;
      
        public string SessionId
        {
            get
            {
                return this.sessionId;
            }
            set
            {
                this.sessionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string CartId
        {
            get
            {
                return this.cartId;
            }
            set
            {
                this.cartId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public MOBSHOPOfferSource OfferSource
        {
            get
            {
                return this.offerSource;
            }
            set
            {
                this.offerSource = value;
            }
        }

        public List<MOBShopBundleProducts> BundleProducts
        {
            get
            {
                return this.bundleProducts;
            }
            set
            {
                this.bundleProducts = value;
            }
        }
    }
}
