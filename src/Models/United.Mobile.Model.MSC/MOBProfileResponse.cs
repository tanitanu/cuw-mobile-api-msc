using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBProfileResponse : MOBResponse
    {
        private MOBProfile profile;

        public MOBProfileResponse()
            : base()
        {
        }

        public MOBProfile Profile
        {
            get
            {
                return this.profile;
            }
            set
            {
                this.profile = value;
            }
        }
    }
}
