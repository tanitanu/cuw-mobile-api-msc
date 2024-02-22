using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition;
using United.Definition.Shopping;
using United.Mobile.Model.Common;

namespace United.Persist.Definition.Shopping
{
    [Serializable()]
    public class ProfileResponse : IPersist
    {
        #region IPersist Members

        private string objectName = "United.Persist.Definition.Shopping.ProfileResponse";
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
        public MOBCPProfileRequest Request { get; set; }
        public MOBCPProfileResponse Response { get; set; }
        public MOBCustomerProfileResponse ViewResProfileResponse { get; set; }

        public MOBUpdateFOPTravelerResponse UpdateProfileResponse { get; set; }

    }
}
