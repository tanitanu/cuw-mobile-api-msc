using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.Model.Common
{
    [Serializable]
    public class MOBVersion
    {
        private string displayText = string.Empty;
        private string major = string.Empty;
        private string minor = string.Empty;
        private string build = string.Empty;

        public string DisplayText
        {
            get
            {
                return this.displayText;
            }
            set
            {
                this.displayText = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Major
        {
            get
            {
                return this.major;
            }
            set
            {
                this.major = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Minor
        {
            get
            {
                return this.minor;
            }
            set
            {
                this.minor = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Build
        {
            get
            {
                return this.build;
            }
            set
            {
                this.build = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
