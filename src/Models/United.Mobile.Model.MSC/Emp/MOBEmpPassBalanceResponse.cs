using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Emp
{
    [Serializable]
    public class MOBEmpPassBalanceResponse : MOBResponse
    {
        private List<MOBEmpPassSummaryAllotment> ePassSummaryAllotments;

        public List<MOBEmpPassSummaryAllotment> EPassSummaryAllotments
        {
            get { return ePassSummaryAllotments; }
            set { ePassSummaryAllotments = value; }
        }
    }
}
