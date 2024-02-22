using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Emp
{
    [Serializable()]
    public class MOBEmpStandByListResponse : MOBResponse
    {
        private List<MOBEmpStandByListPassengers> passengerList;
        private MOBEmpStandByListRequest mobEmpStandByListRequest;
        private List<MOBEmpSegmentPBT> mobEmpSegmentPBTs;




        public MOBEmpStandByListRequest MobEmpStandByListRequest
        {
            get { return mobEmpStandByListRequest; }
            set { mobEmpStandByListRequest = value; }
        }

        public List<MOBEmpStandByListPassengers> PassengerList
        {
            get { return passengerList; }
            set { passengerList = value; }
        }

        public List<MOBEmpSegmentPBT> MOBEmpSegmentPBTs
        {
            get { return mobEmpSegmentPBTs; }
            set { mobEmpSegmentPBTs = value; }    
        }
    }
}
