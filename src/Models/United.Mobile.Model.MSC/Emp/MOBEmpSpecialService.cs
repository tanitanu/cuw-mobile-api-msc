using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Emp
{
    [Serializable]
    public class MOBEmpSpecialService
    {
        private string paxID;
        private string serviceCode;
        private string fullServiceName;

        public string PaxID
        {
            get
            {
                return paxID;
            }
            set
            {
                paxID = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ServiceCode
        {
            get
            {
                return serviceCode;
            }
            set
            {
                serviceCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string FullServiceName
        {
            get
            {
                return fullServiceName;
            }
            set
            {
                fullServiceName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

    }
}
