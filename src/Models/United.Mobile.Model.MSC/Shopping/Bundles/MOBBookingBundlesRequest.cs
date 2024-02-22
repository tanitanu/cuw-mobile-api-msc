using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping.Bundles
{
    [Serializable]
   public class MOBBookingBundlesRequest : MOBRequest
    {
        private string cartId = string.Empty;
        private string sessionId = string.Empty;
        private string mpNumber = string.Empty;
        private string customerID = string.Empty;
        private string hashCode = string.Empty;
        private string screenSize = string.Empty;
        public string CartId
        {
            get { return cartId; }
            set { cartId = value; }
        }

        public string SessionId
        {
            get { return sessionId; }
            set { sessionId = value; }
        }
 
        public string MPNumber
        {
            get { return mpNumber; }
            set { mpNumber = value; }
        }

        public string CustomerID
        {
            get { return customerID; }
            set { customerID = value; }
        }

        public string HashCode
        {
            get { return hashCode; }
            set { hashCode = value; }
        }

        public string ScreenSize
        {
            get { return screenSize; }
            set { screenSize = value; }
        }

    }
}
