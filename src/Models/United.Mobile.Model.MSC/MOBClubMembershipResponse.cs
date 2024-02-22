using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBClubMembershipResponse : MOBResponse
    {
        private MOBClubMembership membership;

        public MOBClubMembership Membership
        {
            get
            {
                return this.membership;
            }
            set
            {
                this.membership = value;
            }
        }
    }
}
