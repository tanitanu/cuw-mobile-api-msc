using System;
using United.Definition;
using United.Mobile.Model.Common;

namespace United.Persist.Definition.FOP
{
    [Serializable]
    public class ProfileFOPCreditCardResponse : IPersist
    {
        #region IPersist Members

        private string objectName = "United.Persist.Definition.FOP.ProfileFOPCreditCardResponse";
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
        public MOBCustomerProfileRequest Request { get; set; }
        public MOBCustomerProfileResponse Response { get; set; }

        public MOBUpdateProfileOwnerFOPRequest customerProfileRequest { get; set; }
        public MOBUpdateProfileOwnerFOPResponse customerProfileResponse { get; set; }
    }
}
