using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Shopping;
using United.Mobile.Model.Common;
namespace United.Definition.Pcu
{
    [Serializable()]
    public class MOBPremiumCabinUpgradeResponse : MOBResponse
    {
        private MOBPremiumCabinUpgradeRequest request;
        private MOBPcuPurchaseConfirmation purchaseConfirmation;
        
        public MOBPremiumCabinUpgradeRequest Request
        {
            get { return request; }
            set { request = value; }
        }

        public MOBPcuPurchaseConfirmation PurchaseConfirmation
        {
            get { return purchaseConfirmation; }
            set { purchaseConfirmation = value; }
        }

        private MOBSHOPResponseStatusItem responseStatusItem;

        public MOBSHOPResponseStatusItem ResponseStatusItem
        {
            get { return responseStatusItem; }
            set { responseStatusItem = value; }
        }
    }
}
