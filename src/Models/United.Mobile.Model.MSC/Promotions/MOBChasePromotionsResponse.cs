using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Promotions
{
    [Serializable()]
    public class MOBChasePromotionsResponse : MOBResponse
    {
        private List<MOBChasePromotion> promotions;

        public List<MOBChasePromotion> Promotions
        {
            get { return promotions; }
            set { promotions = value; }
        }

    }
}
