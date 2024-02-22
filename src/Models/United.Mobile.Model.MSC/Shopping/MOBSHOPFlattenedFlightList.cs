using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Shopping;

namespace United.Persist.Definition.Shopping
{
    [Serializable()]
    public class MOBSHOPFlattenedFlightList
    {
        private List<MOBSHOPFlattenedFlight> flattenedFlightList;

        public List<MOBSHOPFlattenedFlight> FlattenedFlightList
        {
            get { return flattenedFlightList; }
            set { flattenedFlightList = value; }
        }

    }
}
