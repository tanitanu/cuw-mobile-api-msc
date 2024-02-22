using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable()]
    public class MOBEquipment
    {
        private string id = string.Empty;
        private string noseNumber;
        private string tailNumber;
        private MOBAircraft aircraft;

        public MOBEquipment()
        {
        }

        public string Id
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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

        public MOBAircraft Aircraft
        {
            get
            {
                return this.aircraft;
            }
            set
            {
                this.aircraft = value;
            }
        }
    }
}
