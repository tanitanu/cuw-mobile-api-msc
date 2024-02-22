using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Shopping;
using United.Mobile.Model.Common;
using United.Services.FlightShopping.Common;

namespace United.Persist.Definition.Shopping
{
    [Serializable()]
    public class ShopAmenitiesResponse : IPersist
    {
        #region IPersist Members
        private string objectName = "United.Persist.Definition.Shopping.ShopAmenitiesResponse";
        public string ObjectName
        {
            get
            {
                return this.objectName;
            }
            set
            {
                this.objectName = value;
            }
        }
        #endregion
        public string SessionId { get; set; }
        public string CartId { get; set; }
        public List<string> TripIndexKeys { get; set; }
        public SerializableDictionary<string, MOBSHOPAmenitiesResponse> FlightAmenitiesList { get; set; }
    }
}