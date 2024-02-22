﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable()]
    public class MOBApplePayData
    {

        private string applicationPrimaryAccountNumbe;
        private string applicationExpirationDate;
        private string currencyCode;
        private int transactionAmount;
        private string deviceManufacturerIdentifier;
        private string paymentDataType;
        private MOBApplePayPaymentdata paymentData;


        public string ApplicationPrimaryAccountNumber { get { return applicationPrimaryAccountNumbe; } set { applicationPrimaryAccountNumbe = value; } }
        public string ApplicationExpirationDate { get { return applicationExpirationDate; } set { applicationExpirationDate = value; } }
        public string CurrencyCode
        {
            get { return currencyCode; }
            set { currencyCode = value; }
        }

        public int TransactionAmount
        {
            get { return transactionAmount; }
            set { transactionAmount = value; }
        }

        public string DeviceManufacturerIdentifier
        {
            get { return deviceManufacturerIdentifier; }
            set { deviceManufacturerIdentifier = value; }
        }

        public string PaymentDataType
        {
            get { return paymentDataType; }
            set { paymentDataType = value; }
        }

        public MOBApplePayPaymentdata PaymentData
        {
            get { return paymentData; }
            set { paymentData = value; }
        }
    }
    [Serializable()]
    public class MOBApplePayPaymentdata
    {
        private string onlinePaymentCryptogram;
        private string eciIndicator;

        public string OnlinePaymentCryptogram
        {
            get { return onlinePaymentCryptogram; }
            set { onlinePaymentCryptogram = value; }
        }
        public string EciIndicator
        {
            get { return eciIndicator; }
            set { eciIndicator = value; }
        }
    }
}
