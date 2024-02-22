using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition.Bag
{
    [Serializable]
    public class MOBLoyaltyProgramProfile
    {
        private string loyaltyProgramCarrierCode = string.Empty;
        private string loyaltyProgramID = string.Empty;
        private string loyaltyProgramMemberID = string.Empty;

        public string LoyaltyProgramCarrierCode
        {
            get
            {
                return loyaltyProgramCarrierCode;
            }
            set
            {
                this.loyaltyProgramCarrierCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string LoyaltyProgramID
        {
            get
            {
                return loyaltyProgramID;
            }
            set
            {
                this.loyaltyProgramID = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string LoyaltyProgramMemberID
        {
            get
            {
                return loyaltyProgramMemberID;
            }
            set
            {
                this.loyaltyProgramMemberID = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
    }
}
