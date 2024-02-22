using System;
using System.Collections.Generic;

namespace United.Mobile.Model.MSC.Shopping
{
    [Serializable()]
    public class AwardRedemptionDetail
    {
        private bool isUsingPoolAccount;

        private bool canRedeemPoolBalance;


        public AwardRedemptionDetail()
        {
        }
        public bool IsUsingPoolAccount
        {
            get
            {
                return this.isUsingPoolAccount;
            }
            set
            {
                this.isUsingPoolAccount = value;
            }
        }

        public bool CanRedeemPoolBalance
        {
            get
            {
                return this.canRedeemPoolBalance;
            }
            set
            {
                this.canRedeemPoolBalance = value;
            }
        }

        private PersonalAccountInfo personalAccountInfo;
        public PersonalAccountInfo PersonalAccountInfo
        {
            get { return personalAccountInfo; }
            set { personalAccountInfo = value; }
        }
        private PooledAccountInfo pooledAccountInfo;
        public PooledAccountInfo PooledAccountInfo
        {
            get { return pooledAccountInfo; }
            set { pooledAccountInfo = value; }
        }
        public List<RedeemingAccountData> RedeemingAccountData { get; set; }
    }


    public class RedeemingAccountData
    {
        public RedeemingAccountData() { }
        public string Id
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
        private string id = string.Empty;
        public string CurrentValue
        {
            get
            {
                return this.currentValue;
            }
            set
            {
                this.currentValue = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
        private string currentValue = string.Empty;
        private bool saveToPersist;
        public bool SaveToPersist
        {
            get
            {
                return this.saveToPersist;
            }
            set
            {
                this.saveToPersist = value;
            }
        }
    }
    public class PersonalAccountInfo
    {
        private string mileagePlusNumber = string.Empty;
        public string MileagePlusNumber
        {
            get
            {
                return this.mileagePlusNumber;
            }
            set
            {
                this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        private string currentBalance = string.Empty;
        public string CurrentBalance
        {
            get { return currentBalance; }
            set { currentBalance = value; }
        }
        public PersonalAccountInfo() { }

    }
    public class PooledAccountInfo
    {
        private string mileagePlusNumber = string.Empty;
        public string MileagePlusNumber
        {
            get
            {
                return this.mileagePlusNumber;
            }
            set
            {
                this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
        private string currentBalance = string.Empty;
        public string CurrentBalance
        {
            get { return currentBalance; }
            set { currentBalance = value; }
        }
        public PooledAccountInfo() { }
    }
}