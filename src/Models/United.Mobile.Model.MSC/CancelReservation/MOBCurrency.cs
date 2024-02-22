using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.CancelReservation
{
    [Serializable]
    public class MOBCurrency
    {
        private string code;
        private int decimalPlace;

        public string Code
        {
            get { return code; }
            set { code = value; }
        }

        public int DecimalPlace
        {
            get { return decimalPlace; }
            set { decimalPlace = value; }
        }
    }
}
