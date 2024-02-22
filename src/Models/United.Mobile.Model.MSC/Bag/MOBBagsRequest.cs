using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;

namespace United.Definition.Bag
{
    [Serializable]
    public class MOBBagsRequest : MOBRequest
    {
        private List<string> bagTagIds;

        public List<string> BagTagIds
        {
            get
            {
                return this.bagTagIds;
            }
            set
            {
                this.bagTagIds = value;
            }
        }
    }
}
