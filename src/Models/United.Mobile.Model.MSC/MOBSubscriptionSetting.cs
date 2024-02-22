using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable()]
    public class MOBSubscriptionSetting
    {
        private int setting;
        private bool isSubscribed;
        private bool updateSucceed;

        public int Setting
        {
            get
            {
                return this.setting;
            }
            set
            {
                this.setting = value;
            }
        }

        public bool IsSubscribed
        {
            get
            {
                return this.isSubscribed;
            }
            set
            {
                this.isSubscribed = value;
            }
        }

        public bool UpdateSucceed
        {
            get
            {
                return this.updateSucceed;
            }
            set
            {
                this.updateSucceed = value;
            }
        }
    }
}
