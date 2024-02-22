using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBCarrierInfoResponse: MOBResponse
    {
        public MOBCarrierInfoResponse()
            : base()
        {
        }

        private List<MOBCarrierInfo> carriers;

        public List<MOBCarrierInfo> Carriers
        {
            get
            {
                return this.carriers;
            }
            set
            {
                this.carriers = value;
            }
        }
    }
    
}
