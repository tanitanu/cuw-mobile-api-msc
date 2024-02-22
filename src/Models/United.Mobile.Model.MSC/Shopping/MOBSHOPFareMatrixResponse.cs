using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping
{
    [Serializable]
    public class MOBSHOPFareMatrixResponse:MOBResponse
    {
        private MOBSHOPShopRequest shopRequest;
       
        //private List<string> disclaimer;
        private string cartId = string.Empty;
        private string callDurationText = string.Empty;
        private List<List<MOBSHOPFareMatrixItem>> fareMatrix;
        private int travelerCount;

        public List<List<MOBSHOPFareMatrixItem>> FareMatrix
        {
            get
            {
                return this.fareMatrix;
            }
            set
            {
                this.fareMatrix = value;
            }
        }

        public string CallDurationText
        {
            get
            {
                return this.callDurationText;
            }
            set
            {
                this.callDurationText = value;
            }
        }
        public MOBSHOPShopRequest ShopRequest
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
        public int TravelerCount
        {
            get
            {
                return this.travelerCount;
            }
            set
            {
                this.travelerCount = value;
            }
        }

        //public List<string> Disclaimer
        //{
        //    get
        //    {
        //        return this.disclaimer;
        //    }
        //    set
        //    {
        //        this.disclaimer = value;
        //    }
        //}
        public string CartId
        {
            get { return cartId; }
            set { cartId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
       
    }
}
