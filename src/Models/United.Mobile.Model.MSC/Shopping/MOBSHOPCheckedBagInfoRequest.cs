using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping
{
    [Serializable]
    public class MOBSHOPCheckedBagInfoRequest : MOBRequest
    {
        private string sessionId;
        private string cartId;
        private string loyaltyLevel;

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

        public string LoyaltyLevel
        {
            get { return loyaltyLevel; }
            set { loyaltyLevel = value; }
        }
    }
}
