using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable]
    public class MOBABTestingResponse : MOBResponse
    {
        private List<MOBABSwitchOption> items;

        public List<MOBABSwitchOption> Items
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
