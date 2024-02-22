using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Shopping;
using United.Mobile.Model.Common;

namespace United.Persist.Definition.Shopping
{
    [Serializable()]
    public class SelectTrip : IPersist
    {
        #region IPersist Members

        private string objectName = "United.Persist.Definition.Shopping.SelectTrip";
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
        public SerializableDictionary<string, MOBSHOPSelectTripRequest> Requests { get; set; }
        public SerializableDictionary<string, MOBSHOPSelectTripResponse> Responses { get; set; }
        public string LastSelectTripKey { get; set; }
    }
}
