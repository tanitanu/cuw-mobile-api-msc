using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Promotions
{
    [Serializable]
    public class MOBChasePromotion
    {
        private int promotionId;
        public int PromotionId
        {
            get { return promotionId; }
            set { promotionId = value; }
        }

        private string promoCode;
        public string PromoCode
        {
            get { return promoCode; }
            set { promoCode = value; }
        }

        private string promoDesc;
        public string PromoDesc
        {
            get { return promoDesc; }
            set { promoDesc = value; }
        }

        private long bonusMiles;
        public long BonusMiles
        {
            get { return bonusMiles; }
            set { bonusMiles = value; }
        }

        private DateTime startDateTime;
        public DateTime StartDateTime
        {
            get { return startDateTime; }
            set { startDateTime = value; }
        }

        private DateTime endDateTime;
        public DateTime EndDateTime
        {
            get { return endDateTime; }
            set { endDateTime = value; }
        }

        private List<MOBChasePromotionPage> pages;
        public List<MOBChasePromotionPage> Pages
        {
            get { return pages; }
            set { pages = value; }
        }

        private string promoURL;

        public string PromoURL
        {
            get { return promoURL; }
            set { promoURL = value; }
        }
    }

    [Serializable]
    public class MOBChasePromotionPage
    {
        private int pageID;
        public int PageID
        {
            get { return pageID; }
            set { pageID = value; }
        }

        private string pageCode;
        public string PageCode
        {
            get { return pageCode; }
            set { pageCode = value; }
        }

        private string pageDesc;
        public string PageDesc
        {
            get { return pageDesc; }
            set { pageDesc = value; }
        }

        private List<MOBKey> pageValues;
        public List<MOBKey> PageValues
        {
            get { return pageValues; }
            set { pageValues = value; }
        }

    }
}
