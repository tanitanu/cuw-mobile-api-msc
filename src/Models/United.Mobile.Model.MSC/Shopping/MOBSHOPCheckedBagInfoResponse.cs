using System;
using System.Collections.Generic;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping
{
    [Serializable]
    public class MOBSHOPCheckedBagInfoResponse : MOBResponse
    {
        private MOBSHOPCheckedBagInfoRequest request;
        private MOBCheckedBagChargeInfo checkedBagChargeInfo;
        private string sessionId;
        private string cartId;
        public MOBSHOPResponseStatusItem responseStatusItem;
        public MOBSHOPCheckedBagInfoRequest Request
        {
            get { return request; }
            set { request = value; }
        }

        public MOBCheckedBagChargeInfo CheckedBagChargeInfo
        {
            get { return checkedBagChargeInfo; }
            set { checkedBagChargeInfo = value; }
        }

        public string SessionId
        {
            get { return sessionId; }
            set { sessionId = value; }
        }

        public string CartId
        {
            get { return cartId; }
            set { cartId = value; }
        }

        public MOBSHOPResponseStatusItem ResponseStatusItem
        {
            get { return responseStatusItem; }
            set { responseStatusItem = value; }
        }
    }
}
