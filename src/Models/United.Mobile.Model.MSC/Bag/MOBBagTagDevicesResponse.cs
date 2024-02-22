using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;

namespace United.Definition.Bag
{
    [Serializable]
    public class MOBBagTagDevicesResponse : MOBResponse
    {
        private List<MOBBagTagDevice> bagTagDevices;

        public List<MOBBagTagDevice> BagTagDevices
        {
            get
            {
                return this.bagTagDevices;
            }
            set
            {
                this.bagTagDevices = value;
            }
        }
    }
}
