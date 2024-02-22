using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable]
    public class MOBOldRecordLocator
    {
        private string carrierCode = string.Empty;
        private string carrierName = string.Empty;
        private string recordLocator = string.Empty;

        public MOBOldRecordLocator() { }

        public string CarrierCode
        {
            get { return this.carrierCode; }
            set
            {
                this.carrierCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string CarrierName
        {
            get { return this.carrierName; }
            set
            {
                this.carrierName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string RecordLocator
        {
            get { return recordLocator; }
            set
            {
                this.recordLocator = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
    }
}
