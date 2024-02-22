using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Shopping;

namespace United.Definition
{
    [Serializable()]
    public class MOBCheckOutRequest : MOBShoppingRequest
    {
        private MOBFormofPaymentDetails formofPaymentDetails;
        private string paymentAmount;
        private string totalMiles;
        private bool fareLockAutoTicket;
        private string mileagePlusNumber = string.Empty;
        private bool isTPI = false;
        private bool isSecondaryPayment = false;
        private string additionalData = string.Empty;
        private bool isTPIOfferDeclinedByUser = false;

        public MOBFormofPaymentDetails FormofPaymentDetails
        {
            get { return formofPaymentDetails; }
            set { formofPaymentDetails = value; }
        }
        public string PaymentAmount
        {
            get { return paymentAmount; }
            set { paymentAmount = value; }
        }
                
        
        public string TotalMiles
        {
            get { return totalMiles; }
            set { totalMiles = value; }
        }
        public bool FareLockAutoTicket
        {
            get
            {
                return this.fareLockAutoTicket;
            }
            set
            {
                this.fareLockAutoTicket = value;
            }
        }
        
        public string MileagePlusNumber
        {
            get
            {
                return this.mileagePlusNumber;
            }
            set
            {
                this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public bool IsTPI
        {
            get
            {
                return this.isTPI;
            }
            set
            {
                this.isTPI = value;
            }
        }
        public bool IsSecondaryPayment
        {
            get
            {
                return this.isSecondaryPayment;
            }
            set
            {
                this.isSecondaryPayment = value;
            }
        }
        public string AdditionalData
        {
            get
            {
                return this.additionalData;
            }
            set
            {
                this.additionalData = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        private MOBTaxIdInformation taxIdInformation;
        public MOBTaxIdInformation TaxIdInformation
        {
           get { return taxIdInformation; }
            set { taxIdInformation = value; }
        }

        public bool IsTPIOfferDeclinedByUser
        {
            get
            {
                return this.isTPIOfferDeclinedByUser;
            }
            set
            {
                this.isTPIOfferDeclinedByUser = value;
            }
        }
    }
}
