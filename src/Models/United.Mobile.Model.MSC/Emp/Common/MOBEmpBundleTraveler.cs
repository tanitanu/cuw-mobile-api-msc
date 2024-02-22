using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Emp.Common
{
    [Serializable()]
    public class MOBEmpBundleTraveler : MOBEmpDOTBaggageTravelerInfo
    {
        private string bundleDescription = string.Empty;

        public string BundleDescription
        {
            get { return this.bundleDescription; }
            set { this.bundleDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
    }
}
