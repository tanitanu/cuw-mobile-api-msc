using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Fitbit
{
    [Serializable]
    public class Aircraft
    {
        private string code = string.Empty;
        private string shortName = string.Empty;
        private string longName = string.Empty;

        public Aircraft()
        {
        }

        public string Code
        {
            get
            {
                return this.code;
            }
            set
            {
                this.code = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string ShortName
        {
            get
            {
                return this.shortName;
            }
            set
            {
                this.shortName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string LongName
        {
            get
            {
                return this.longName;
            }
            set
            {
                this.longName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }

}
