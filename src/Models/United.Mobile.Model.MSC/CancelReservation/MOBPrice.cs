using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.CancelReservation
{
    [Serializable]
    public class MOBPrice
    {
        private string amount;
        private string code;
        private MOBCurrency currency;
        private string description;

        public string Amount
        {
            get { return amount; }
            set { amount = value; }
        }

        public MOBCurrency Currency
        {
            get { return currency; }
            set { currency = value; }
        }

        public string Code
        {
            get { return code; }
            set { code = value; }
        }

        public string Description
        {
            get { return description; }
            set { description = value; }
        }
    }
}
