using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Gamification
{
    public class MOBGamificationPromotionRequest : MOBRequest
    {
        private string mileagePlusNumber = string.Empty;
        private string custID = string.Empty;
        private string promoCode = string.Empty;
        private string memberPromoId = string.Empty;

        public string MileagePlusNumber
        {
            get { return mileagePlusNumber; }
            set { this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper(); }
        }

        public string CustID
        {
            get
            {
                return this.custID;
            }
            set
            {
                this.custID = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string PromoCode
        {
            get { return promoCode; }
            set { promoCode = value; }
        }

        public string MemberPromoId
        {
            get
            {
                return this.memberPromoId;
            }
            set
            {
                this.memberPromoId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
    }
}
