using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Emp.Shopping
{
    [Serializable]
    public class MOBEmpSHOPTravelOption
    {
        private double amount;
        private string displayAmount = string.Empty;
        private string displayButtonAmount = string.Empty;
        private string currencyCode = string.Empty;
        private bool deleted;
        private string description = string.Empty;
        private string key = string.Empty;
        private string productId = string.Empty;
        private List<MOBEmpSHOPTravelOptionSubItem> subItems;
        private string type = string.Empty;

        public double Amount
        {
            get
            {
                return this.amount;
            }
            set
            {
                this.amount = value;
            }
        }

        public string DisplayAmount
        {
            get
            {
                return this.displayAmount;
            }
            set
            {
                this.displayAmount = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string DisplayButtonAmount
        {
            get
            {
                return this.displayButtonAmount;
            }
            set
            {
                this.displayButtonAmount = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string CurrencyCode
        {
            get
            {
                return this.currencyCode;
            }
            set
            {
                this.currencyCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public bool Deleted
        {
            get
            {
                return this.deleted;
            }
            set
            {
                this.deleted = value;
            }
        }

        public string Description
        {
            get
            {
                return this.description;
            }
            set
            {
                this.description = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Key
        {
            get
            {
                return this.key;
            }
            set
            {
                this.key = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ProductId
        {
            get
            {
                return this.productId;
            }
            set
            {
                this.productId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MOBEmpSHOPTravelOptionSubItem> SubItems
        {
            get
            {
                return this.subItems;
            }
            set
            {
                this.subItems = value;
            }
        }

        public string Type
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
