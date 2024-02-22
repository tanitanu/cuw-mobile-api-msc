using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Emp.Common
{
    [Serializable]
    public class MOBEmpUpgradePropertyKeyValue
    {
        private MOBEmpUpgradeProperty key;
        private string value = string.Empty;

        public MOBEmpUpgradeProperty Key
        {
            get
            {
                return this.key;
            }
            set
            {
                this.key = value;
            }
        }

        public string Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
