﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBUpdateCustomerFOPRequest : MOBRequest
    {
        private string sessionId = string.Empty;
        private int customerId;
        private string hashPinCode = string.Empty;
        private string mileagePlusNumber = string.Empty;
        private bool updateInsertCreditCardInfo;
        private MOBCPTraveler traveler;
        private bool isSavedToProfile = false;
        private string transactionPath = string.Empty;
        private string redirectViewName = string.Empty;

        

        public bool IsSavedToProfile
        {
            get { return isSavedToProfile; }
            set { isSavedToProfile = value; }
        }

        public string RedirectViewName
        {
            get { return redirectViewName; }
            set { redirectViewName = value; }
        }
        public string TransactionPath
        {
            get { return transactionPath; }
            set { transactionPath = value; }
        }

        public string SessionId
        {
            get
            {
                return sessionId;
            }
            set
            {
                this.sessionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string MileagePlusNumber
        {
            get
            {
                return mileagePlusNumber;
            }
            set
            {
                this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public int CustomerId
        {
            get
            {
                return customerId;
            }
            set
            {
                this.customerId = value;
            }
        }

        public string HashPinCode
        {
            get
            {
                return hashPinCode;
            }
            set
            {
                this.hashPinCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public MOBCPTraveler Traveler
        {
            get
            {
                return this.traveler;
            }
            set
            {
                this.traveler = value;
            }
        }

        public MOBUpdateCustomerFOPRequest() : base()
        {
        }

        public bool UpdateInsertCreditCardInfo
        {
            get { return this.updateInsertCreditCardInfo; }
            set { this.updateInsertCreditCardInfo = value; }
        }
    }
}
