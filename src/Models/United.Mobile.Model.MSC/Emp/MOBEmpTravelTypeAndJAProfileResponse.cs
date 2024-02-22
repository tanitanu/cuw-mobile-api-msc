using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Emp
{
    [Serializable]
    public class MOBEmpTravelTypeAndJAProfileResponse : MOBResponse
    {
        private MOBEmpJAResponse mobEmpJAResponse;
        private MOBEmpTravelTypeResponse mobEmpTravelTypeResponse;

        public MOBEmpJAResponse MOBEmpJAResponse
        {
        get
            {
                return this.mobEmpJAResponse;
            }
            set
            {
                mobEmpJAResponse = value;
            }
        }
        public MOBEmpTravelTypeResponse MOBEmpTravelTypeResponse
        {
        get
            {
                return this.mobEmpTravelTypeResponse;
            }
            set
            {
                mobEmpTravelTypeResponse = value;
            }
        }
    }
}
