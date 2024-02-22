using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Shopping;

namespace United.Persist.Definition.Shopping
{
    public class FlightConfirmTravelerResponse 
    {
        public FlightConfirmTravelerResponse() { }
        
        #region IPersist Members
        private string objectName = "United.Persist.Definition.Shopping.FlightConfirmTravelerResponse";

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
        public MOBSHOPFlightConfirmTravelerResponse Response { get; set; }
    }
}
