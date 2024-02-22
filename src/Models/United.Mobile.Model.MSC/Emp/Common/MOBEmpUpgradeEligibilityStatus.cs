using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Emp.Common
{
    
    [Serializable]
    public enum MOBEmpUpgradeEligibilityStatus
    {
        Unknown=0,
        NotQualified=1,
        Qualified=2,
        Requested=3,
        Upgraded=4,
        NotUpgraded=5,
        RequestConfirmed=6,
    }
}
