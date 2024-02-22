using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition
{
    [Serializable()]
    public class MOBWalletClubDayPass
    {
        private string passCode = string.Empty;
        private string mpAccountNumber = string.Empty;
        private string firstName = string.Empty;
        private string lastName = string.Empty;
        private string email = string.Empty;
        private string clubPassCode = string.Empty;
        private double paymentAmount;
        private string purchaseDate = string.Empty;
        private string expirationDate = string.Empty;
        private string barCode;

        public string PassCode
        {
            get
            {
                return this.passCode;
            }
            set
            {
                this.passCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string MPAccountNumber
        {
            get
            {
                return this.mpAccountNumber;
            }
            set
            {
                this.mpAccountNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string FirstName
        {
            get
            {
                return this.firstName;
            }
            set
            {
                this.firstName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string LastName
        {
            get
            {
                return this.lastName;
            }
            set
            {
                this.lastName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Email
        {
            get
            {
                return this.email;
            }
            set
            {
                this.email = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ClubPassCode
        {
            get
            {
                return this.clubPassCode;
            }
            set
            {
                this.clubPassCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public double PaymentAmount
        {
            get
            {
                return this.paymentAmount;
            }
            set
            {
                this.paymentAmount = value;
            }
        }

        public string PurchaseDate
        {
            get
            {
                return this.purchaseDate;
            }
            set
            {
                this.purchaseDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ExpirationDate
        {
            get
            {
                return this.expirationDate;
            }
            set
            {
                this.expirationDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string BarCode
        {
            get
            {
                return this.barCode;
            }
            set
            {
                this.barCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
