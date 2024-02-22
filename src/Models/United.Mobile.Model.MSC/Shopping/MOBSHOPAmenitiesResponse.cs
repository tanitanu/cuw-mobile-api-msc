using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping
{
    [Serializable()]
    public class MOBSHOPAmenitiesResponse : MOBResponse
    {
        private MOBSHOPAmenitiesRequest shopAmenitiesRequest;
        private string cartId = string.Empty;
        private List<MOBSHOPAmenitiesFlight> amenitiesFlightList;

        public MOBSHOPAmenitiesRequest ShopAmenitiesRequest
        {
            get
            {
                return this.shopAmenitiesRequest;
            }
            set
            {
                this.shopAmenitiesRequest = value;
            }
        }
        public string CartId
        {
            get { return cartId; }
            set { cartId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
        public List<MOBSHOPAmenitiesFlight> AmenitiesFlightList
        {
            get
            {
                return this.amenitiesFlightList;
            }
            set
            {
                this.amenitiesFlightList = value;
            }
        }
     }
}
