using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Shopping;

namespace United.Persist.Definition.Shopping
{
    public class Profile 
    {
        public Profile() { }
        
        #region IPersist Members
        private string objectName = "United.Persist.Definition.Shopping.Profile";

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
        public MOBSHOPProfile Response { get; set; }
    }
}
