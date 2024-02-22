using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition
{
    [Serializable()]
    public class MOBWalletItem
    {
        private List<MOBKVP> kvps;

        public List<MOBKVP> KVPs
        {
            get
            {
                return this.kvps;
            }
            set
            {
                this.kvps = value;
            }
        }
    }
}
