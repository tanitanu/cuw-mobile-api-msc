using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping
{
    [Serializable]
    public class MOBSHOPGetOffersRequest : MOBRequest
    {
        private string sessionId;
        private string productCodes;
        private bool isFromViewResSeatMap;

        public string SessionId
        {
            get { return sessionId; }
            set { sessionId = value; }
        }

        public string ProductCodes
        {
            get { return productCodes; }
            set { productCodes = value; }
        }

        public bool IsFromViewResSeatMap
        {
            get { return isFromViewResSeatMap; }
            set { isFromViewResSeatMap = value; }
        }
    }
}
