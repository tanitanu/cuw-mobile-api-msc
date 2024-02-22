using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Emp.Common
{
    [Serializable()]
    public class MOBEmpUASubscription
    {
        public MOBEmpUASubscription()
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
