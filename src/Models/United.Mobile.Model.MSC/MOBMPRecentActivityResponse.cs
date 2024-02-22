using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBMPRecentActivityResponse : MOBResponse
    {
        private MOBMPRecentActivity recentActivity;

        public MOBMPRecentActivityResponse()
            : base()
        {
        }

        public MOBMPRecentActivity RecentActivity
        {
            get
            {
                return this.recentActivity;
            }
            set
            {
                this.recentActivity = value;
            }
        }
    }
}
