using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Shopping;

namespace United.Persist.Definition.Shopping
{
    public class SelectAddTravellerResponse 
    {
        public SelectAddTravellerResponse() { }
        
        #region IPersist Members
        private string objectName = "United.Persist.Definition.Shopping.SelectAddTravellerResponse";

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
        public MOBSHOPAddTravellerResponse Response { get; set; }
    }
}
