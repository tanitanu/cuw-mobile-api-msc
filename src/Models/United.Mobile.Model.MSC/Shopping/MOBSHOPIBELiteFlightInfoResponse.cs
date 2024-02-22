using System;
using System.Collections.Generic;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping
{
    [Serializable]
    public class MOBSHOPGetProductInfoForFSRDResponse : MOBResponse
    {
        private List<MOBItem> ibeLiteShopMessages;
        private List<MOBSHOPOption> ibeLiteShopOptions;
        private List<MOBSHOPShoppingProduct> shoppingProducts;

        public List<MOBItem> IBELiteShopMessages
        {
            get
            {
                return ibeLiteShopMessages;
            }
            set
            {
                ibeLiteShopMessages = value;
            }
        }

        public List<MOBSHOPOption> IBELiteShopOptions
        {
            get
            {
                return ibeLiteShopOptions;
            }
            set
            {
                ibeLiteShopOptions = value;
            }
        }

        public List<MOBSHOPShoppingProduct> ShoppingProducts
        {
            get
            {
                return shoppingProducts;
            }
            set
            {
                shoppingProducts = value;
            }
        }
    }
}
