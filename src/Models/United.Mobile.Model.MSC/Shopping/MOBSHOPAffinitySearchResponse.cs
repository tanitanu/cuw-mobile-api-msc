using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping
{
    [Serializable()]
    public class MOBSHOPAffinitySearchResponse : MOBResponse
    {
        private MOBSHOPAffinitySearchRequest request;
        public MOBSHOPAffinitySearchRequest Request
        {
            get
            {
                return request;
            }
            set
            {
                request = value;
            }
        }

        private MOBSHOPAffinitySearch results;
        public MOBSHOPAffinitySearch Results
        {
            get
            {
                return results;
            }
            set
            {
                results = value;
            }
        }
    }

    


}
