using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Emp
{
    [Serializable]
    public class MOBEmpCabinIndicator
    {
        private int seatCount;
        private string cabinClass = string.Empty;

        public int SeatCount 
        {
            get
            {
                return this.seatCount;
            }

            set
            {
                this.seatCount = value;
            }
        }
        public string CabinClass 
        {
            get
            {
                return this.cabinClass;
            }

            set
            {
                this.cabinClass = value;
            }
        }
    }

}
