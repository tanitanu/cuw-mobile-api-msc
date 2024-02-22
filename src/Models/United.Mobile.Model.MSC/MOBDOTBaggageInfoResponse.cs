using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable]
    public class MOBDOTBaggageInfoResponse : MOBResponse
    {
        private MOBDOTBaggageInfoRequest request;
        private MOBDOTBaggageInfo dotBaggageInfo;

        public MOBDOTBaggageInfoRequest Request
        {
            get
            {
                return this.request;
            }
            set
            {
                this.request = value;
            }
        }
        public MOBDOTBaggageInfo DotBaggageInfo
        {
            get
            {
                return this.dotBaggageInfo;
            }
            set
            {
                this.dotBaggageInfo = value;
            }
        }

    }
}
