using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Emp.Shopping
{
    [Serializable()]
    public class MOBEmpSHOPShopResponse : MOBResponse
    {
        private MOBEmpSHOPShopRequest shopRequest;
        private MOBEmpSHOPAvailability availability;
        private List<string> disclaimer;
        private string cartId = string.Empty;
        private string refreshResultsData = string.Empty;

        public MOBEmpSHOPShopRequest ShopRequest
        {
            get
            {
                return this.shopRequest;
            }
            set
            {
                this.shopRequest = value;
            }
        }

        public MOBEmpSHOPAvailability Availability
        {
            get
            {
                return this.availability;
            }
            set
            {
                this.availability = value;
            }
        }

        public List<string> Disclaimer
        {
            get
            {
                return this.disclaimer;
            }
            set
            {
                this.disclaimer = value;
            }
        }
        public string CartId
        {
            get { return cartId; }
            set { cartId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
        public string RefreshResultsData
        {
            get { return refreshResultsData; }
            set { refreshResultsData = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
    }
}
