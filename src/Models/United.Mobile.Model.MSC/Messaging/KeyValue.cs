using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Messaging
{
    [Serializable]
    public class KeyValue
    {
        private string key = string.Empty;
        private string value = string.Empty;

        public KeyValue() { }

        public KeyValue(string key, string value)
        {
            this.key = key;
            this.value = value;
        }

        public string Key
        {
            get
            {
                return this.key;
            }
            set
            {
                this.key = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
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
                this.value = value;
            }
        }
    }
}
