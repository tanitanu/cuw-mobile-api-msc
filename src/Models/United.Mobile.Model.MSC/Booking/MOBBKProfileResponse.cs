using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;

namespace United.Definition.Booking
{
    [Serializable()]
    public class MOBBKProfileResponse : MOBResponse
    {
        private List<MOBBKProfile> profiles;
        private MOBProfileRequest request;

        public MOBProfileRequest Request
        {
            get
            {
                return this.request;
            }
            set
            {
                this.request = value;
            }
        }

        public List<MOBBKProfile> Profiles
        {
            get
            {
                return profiles;
            }
            set
            {
                this.profiles = value;
            }
        }
    }
}
