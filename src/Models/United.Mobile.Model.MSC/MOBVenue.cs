using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable()]
    public class MOBVenue
    {
        private string name = string.Empty;
        private string uuid = string.Empty;

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string UUID
        {
            get
            {
                return this.uuid;
            }
            set
            {
                this.uuid = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
