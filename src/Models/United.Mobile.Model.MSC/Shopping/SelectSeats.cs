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
    public class SelectSeats : IPersist
    {
        #region IPersist Members

        private string objectName = "United.Persist.Definition.Shopping.SelectSeats";
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
        public SerializableDictionary<string, MOBSHOPSelectSeatsRequest> Requests { get; set; }
        public SerializableDictionary<string, MOBSHOPSelectSeatsResponse> Responses { get; set; }
    }
}
