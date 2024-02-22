using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Fitbit
{
    [Serializable]
    public class Equipment
    {
        private string id = string.Empty;
        private string noseNumber;
        private string tailNumber;
        private Aircraft aircraft;

        public Equipment()
        {
        }

        public string Id
        {
            get
            {
                return id;
            }
            set
            {
                id = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string NoseNumber
        {
            get
            {
                return this.noseNumber;
            }
            set
            {
                this.noseNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string TailNumber
        {
            get
            {
                return this.tailNumber;
            }
            set
            {
                this.tailNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public Aircraft Aircraft
        {
            get
            {
                return aircraft;
            }
            set
            {
                aircraft = value;
            }
        }
    }
}
