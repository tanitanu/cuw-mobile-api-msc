using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Pcu;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping
{
    [Serializable]
    public class MOBSHOPGetOffersResponse : MOBResponse
    {
        private MOBSHOPGetOffersRequest request;
        private MOBPremiumCabinUpgrade premiumCabinUpgrade;
        private string sessionId;

        public MOBSHOPGetOffersRequest Request
        {
            get { return request; }
            set { request = value; }
        }

        public MOBPremiumCabinUpgrade PremiumCabinUpgrade
        {
            get { return premiumCabinUpgrade; }
            set { premiumCabinUpgrade = value; }
        }

        public string SessionId
        {
            get { return sessionId; }
            set { sessionId = value; }
        }
    }
}
