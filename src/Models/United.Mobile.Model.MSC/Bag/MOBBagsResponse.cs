using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;

namespace United.Definition.Bag
{
    [Serializable]
    public class MOBBagsResponse : MOBResponse
    {
        private List<MOBBag> bags;

        public List<MOBBag> Bags
        {
            get
            {
                return this.bags;
            }
            set
            {
                this.bags = value;
            }
        }
    }
}
