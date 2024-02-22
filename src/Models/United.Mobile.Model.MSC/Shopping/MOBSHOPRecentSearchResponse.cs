using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping
{
    [Serializable()]
    public class MOBSHOPRecentSearchResponse : MOBResponse
    {
        private List<MOBSHOPShopRequest> mobShopRequests;

        public List<MOBSHOPShopRequest> MOBShopRequests
        {
            get
            {
                return this.mobShopRequests;
            }
            set
            {
                this.mobShopRequests = value;
            }
        }
    }
}
