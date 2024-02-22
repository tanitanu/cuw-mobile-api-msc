using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable]
    public class MOBLDPassenger
    {
        private string recordLocator = string.Empty;
        private MOBName name;
        private string seat = string.Empty;
        private string standbyPassCode = string.Empty;
        private string classDescription = string.Empty;

        public string RecordLocator
        {
            get { return this.recordLocator; }
            set
            {
                this.recordLocator = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public MOBName Name
        {
            get
            {
                return name;
            }
            set
            {
                this.name = value;
            }
        }

        public string Seat
        {
            get { return this.seat; }
            set
            {
                this.seat = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string StandbyPassCode
        {
            get { return this.standbyPassCode; }
            set
            {
                this.standbyPassCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string ClassDescription
        {
            get
            {
                return classDescription;
            }
            set
            {
                classDescription = value;
            }
        }
    }
}
