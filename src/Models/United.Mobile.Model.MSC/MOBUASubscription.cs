using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;

namespace United.Definition
{
    [Serializable()]
    public class MOBUASubscription
    {
        public MOBUASubscription()
            : base()
        {
        }

        private List<MOBItem> items;

        public List<MOBItem> Items
        {
            get
            {
                return this.items;
            }
            set
            {
                this.items = value;
            }
        }
    }
}
