using System;
using System.Collections.Generic;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBWalletRequest : MOBRequest
    {
        private bool backgroundRefresh;
        private string mpNumber = string.Empty;
        private List<MOBWalletCategory> walletCategories;
        private string pushToken = string.Empty;
        private string logTransactionId;
        private long customerId;
        private long employeeId;

        public MOBWalletRequest()
            : base()
        {
        }

        public bool BackgroundRefresh
        {
            get
            {
                return this.backgroundRefresh;
            }
            set
            {
                this.backgroundRefresh = value;
            }
        }

        public string MPNumber
        {
            get
            {
                return this.mpNumber;
            }
            set
            {
                this.mpNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public List<MOBWalletCategory> WalletCategories
        {
            get
            {
                return this.walletCategories;
            }
            set
            {
                this.walletCategories = value;
            }
        }

        public string PushToken
        {
            get
            {
                return this.pushToken;
            }
            set
            {
                this.pushToken = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string LogTransactionId
        {
            get { return logTransactionId; }
            set { logTransactionId = value; }
        }

        public long CustomerId
        {
            get { return customerId; }
            set { customerId = value; }
        }

        public long EmployeeId
        {
            get { return employeeId; }
            set { employeeId = value; }
        }


    }
}
