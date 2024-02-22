using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.MPPINPWD;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBCPCubaTravel
    {
        private List<MOBItem> cubaTravelTitles;

        public List<MOBItem> CubaTravelTitles
        {
            get { return cubaTravelTitles; }
            set { cubaTravelTitles = value; }
        }


        private List<MOBCPCubaTravelReason> travelReasons;

        public List<MOBCPCubaTravelReason> TravelReasons
        {
            get { return travelReasons; }
            set { travelReasons = value; }
        }



    }
}
