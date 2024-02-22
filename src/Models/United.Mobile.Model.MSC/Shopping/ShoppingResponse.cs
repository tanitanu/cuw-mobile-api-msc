using System;
using United.Definition;
using United.Definition.Shopping;
using United.Mobile.Model.Common;

namespace United.Persist.Definition.Shopping
{
    [Serializable()]
    public class ShoppingResponse : IPersist
    {
        #region IPersist Members

        private string objectName = "United.Persist.Definition.Shopping.ShoppingResponse";
        public string ObjectName
        {
            get
            {
                return this.objectName;
            }
            set
            {
                this.objectName = value;
            }
        }

        #endregion

        public string SessionId { get; set; }
        public MOBSHOPShopRequest Request { get; set; }
        public MOBSHOPShopResponse Response { get; set; }

        public string CartId { get; set; }
        public MOBShopPricesCommon PriceSummary { get; set; }
    }
}
