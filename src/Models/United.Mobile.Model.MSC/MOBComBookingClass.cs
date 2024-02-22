using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable()]
    public class MOBComBookingClass
    {
        private MOBComCabin cabin;
        private string code = string.Empty;

        public MOBComCabin Cabin
        {
            get
            {
                return this.cabin;
            }
            set
            {
                this.cabin = value;
            }
        }

        public string Code
        {
            get
            {
                return this.code;
            }
            set
            {
                this.code = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
    }
}
