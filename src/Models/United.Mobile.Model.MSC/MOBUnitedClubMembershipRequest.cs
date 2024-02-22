using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{

    [Serializable()]
    public class MOBUnitedClubMembershipRequest : MOBClubMembershipRequest
    {
        private string hashPinCode = string.Empty;
        public string HashPinCode
        {
            get
            {
                return this.hashPinCode;
            }
            set
            {
                this.hashPinCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
    }
}
