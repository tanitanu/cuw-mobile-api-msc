using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Fitbit
{
    [Serializable]
    public class BoardingPassesResponse : MOBResponse
    {
        private string recordLocator = string.Empty;
        private List<EBP> ebps;

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

        public List<EBP> EBPs
        {
            get
            {
                return this.ebps;
            }
            set
            {
                this.ebps = value;
            }
        }
    }
}
