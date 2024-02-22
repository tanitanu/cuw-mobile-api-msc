using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Emp.Shopping
{
    [Serializable()]
    public class MOBEmpSHOPAwardCalendarItem
    {
        private string departs;
        private bool hasEconomySaver;
        private bool hasPremiumSaver;

        public string Departs
        {
            get
            {
                return this.departs;
            }
            set
            {
                this.departs = value;
            }
        }

        public bool HasEconomySaver
        {
            get
            {
                return this.hasEconomySaver;
            }
            set
            {
                this.hasEconomySaver = value;
            }
        }

        public bool HasPremiumSaver
        {
            get
            {
                return this.hasPremiumSaver;
            }
            set
            {
                this.hasPremiumSaver = value;
            }
        }
    }
}
