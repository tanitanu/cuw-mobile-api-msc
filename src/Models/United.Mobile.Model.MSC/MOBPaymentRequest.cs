using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBPaymentRequest : MOBRequest
    {
        public MOBPaymentRequest()
            : base()
        {
        }

        private string paymentType = string.Empty;
        private double amount;
        private string currencyCode = "USD";
        private int mileage;
        private string remark = string.Empty;
        private string insertBy = string.Empty;
        private bool isTest;
       
        public string PaymentType
        {
            get
            {
                return this.paymentType;
            }
            set
            {
                this.paymentType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

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

        public int Mileage
        {
            get
            {
                return this.mileage;
            }
            set
            {
                this.mileage = value;
            }
        }

        public string Remark
        {
            get
            {
                return this.remark;
            }
            set
            {
                this.remark = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string InsertBy
        {
            get
            {
                return this.insertBy;
            }
            set
            {
                this.insertBy = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool IsTest
        {
            get
            {
                return this.isTest;
            }
            set
            {
                this.isTest = value;
            }
        }

    }
}
