using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Emp
{
    [Serializable]
    public class MOBEmpPaxPrice
    {
        private string baseFare;
        private decimal rawBaseFare;
        private List<MOBEmpTax> taxes;
        private string totalFare;
        private decimal totalFareRaw;
        private List<string> errors;
        private string destination;

        public string BaseFare
        {
            get
            {
                return this.baseFare;
            }
            set
            {
                this.baseFare = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public decimal RawBaseFare
        {
            get
            {
                return this.rawBaseFare;
            }
            set
            {
                this.rawBaseFare = value;
            }
        }

        public List<MOBEmpTax> Taxes
        {
            get
            {
                return taxes;
            }
            set
            {
                taxes = value;
            }
        }

        public string TotalFare
        {
            get
            {
                return totalFare;
            }
            set
            {
                totalFare = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public decimal TotalFareRaw
        {
            get
            {
                return totalFareRaw;
            }
            set
            {
                totalFareRaw = value;
            }
        }

        public List<string> Errors
        {
            get
            {
                return errors;
            }
            set
            {
                errors = value;
            }
        }

        public string Destination
        {
            get
            {
                return destination;
            }
            set
            {
                destination = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
