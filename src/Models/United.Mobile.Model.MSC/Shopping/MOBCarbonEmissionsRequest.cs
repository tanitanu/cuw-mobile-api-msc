using System;
using System.Collections.Generic;
using System.Text;
using United.Mobile.Model.Common;

namespace United.Definition.Shopping
{
    [Serializable]
    public class MOBCarbonEmissionsRequest : MOBRequest
    {
        private string sessionId;

        public string SessionId
        {
            get { return sessionId; }
            set { sessionId = value; }
        }

        private string cartId;

        public string CartId
        {
            get { return cartId; }
            set { cartId = value; }
        }
    }
}
