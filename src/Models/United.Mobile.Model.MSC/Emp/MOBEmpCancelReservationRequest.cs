using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Emp;

namespace United.Definition.Emp
{
    [Serializable]
    public class MOBEmpCancelReservationRequest : MOBEmpRequest
    {
        private string recordLocator = string.Empty;

        public string RecordLocator
        {
            get
            {
                return this.recordLocator;
            }
            set
            {
                this.recordLocator = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
    }
}
