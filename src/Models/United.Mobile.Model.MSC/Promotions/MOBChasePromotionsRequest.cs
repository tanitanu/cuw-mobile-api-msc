using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Promotions
{
    [Serializable()]
    public class MOBChasePromotionsRequest : MOBRequest
    {
        private string mileagePlusNumber = string.Empty;
        private int customerID;
        private string promoCode;
        private string pageCode;
        public string MileagePlusNumber
        {
            get
            {
                return this.mileagePlusNumber;
            }
            set
            {
                this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public int CustomerID
        {
            get
            {
                return this.customerID;
            }
            set
            {
                this.customerID = value;
            }
        }


        public string PromoCode
        {
            get { return promoCode; }
            set { promoCode = value; }
        }


        public string PageCode
        {
            get { return pageCode; }
            set { pageCode = value; }
        }



    }
}
