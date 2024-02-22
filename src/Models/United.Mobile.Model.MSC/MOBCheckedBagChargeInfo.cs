using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable]
    public class MOBCheckedBagChargeInfo
    {
        private List<MOBItem> captions;
        private List<MOBBagFeesPerSegment> bagFeeSegments;
        private string cartId;
        private MOBMemberShipStatus loyaltyLevelSelected;
        private List<MOBMemberShipStatus> loyaltyLevels;

        public List<MOBItem> Captions
        {
            get { return captions; }
            set { captions = value; }
        }

        public List<MOBBagFeesPerSegment> BagFeeSegments
        {
            get { return bagFeeSegments; }
            set { bagFeeSegments = value; }
        }

        public string CartId
        {
            get { return cartId; }
            set { cartId = value; }
        }
        
        public MOBMemberShipStatus LoyaltyLevelSelected
        {
            get { return loyaltyLevelSelected; }
            set { loyaltyLevelSelected = value; }
        }

        public List<MOBMemberShipStatus> LoyaltyLevels
        {
            get { return loyaltyLevels; }
            set { loyaltyLevels = value; }
        }
    }
}
