using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using United.Definition.FormofPayment;
using United.Definition.FormofPayment.TravelCredit;

namespace United.Definition
{
    [Serializable()]
    public class MOBFormofPaymentDetails
    {
        private string formOfPaymentType = string.Empty;
        private string emailAddress;
        private MOBAddress billingAddress;
        private MOBInternationalBilling internationalBilling;
        private MOBCPPhone phone;
        private MOBCreditCard creditCard;
        private MOBCreditCard secondaryCreditCard;
        private MOBPayPal payPal;
        private MOBPayPalPayor payPalPayor;
        private MOBMasterpass masterPass;
        private MOBMasterpassSessionDetails masterPassSessionDetails;
        private MOBApplePay applePayInfo;
        private bool clientCardType = false;
        private MOBEmail email;
        private MOBMilesFOP milesFOP;
        private MOBFOPTravelCertificate travelCertificate;
        private bool isOtherFOPRequired;
        private MOBCreditCard uplift;
        private MOBFOPTravelFutureFlightCredit travelFutureFlightCredit;
        private MOBFOPMoneyPlusMilesCredit moneyPlusMilesCredit;
        private MOBFOPTravelBankDetails travelBankDetails;//chNGE Nmw
        private MOBFOPTravelCreditDetails travelCreditDetails;

        public MOBFOPTravelCreditDetails TravelCreditDetails
        {
            get { return travelCreditDetails; }
            set { travelCreditDetails = value; }
        }


        public MOBFOPTravelBankDetails TravelBankDetails
        {           
            get { return travelBankDetails;}
            set { travelBankDetails = value; }
        }
        public MOBFOPMoneyPlusMilesCredit MoneyPlusMilesCredit
        {
            get { return moneyPlusMilesCredit; }
            set { moneyPlusMilesCredit = value; }
        }

        public MOBFOPTravelFutureFlightCredit TravelFutureFlightCredit
        {
            get { return travelFutureFlightCredit; }
            set { travelFutureFlightCredit = value; }
        }

        public bool IsOtherFOPRequired
        {
            get { return isOtherFOPRequired; }
            set { isOtherFOPRequired = value; }
        }


        public MOBFOPTravelCertificate TravelCertificate
        {
            get { return travelCertificate; }
            set { travelCertificate = value; }
        }


        public MOBApplePay ApplePayInfo
        {
            get
            {
                return applePayInfo;
            }

            set
            {
                applePayInfo = value;
            }
        }
        public string EmailAddress
        {
            get
            {
                return this.emailAddress;
            }
            set
            {
                this.emailAddress = value;
            }
        }

        public MOBAddress BillingAddress
        {
            get
            {
                return this.billingAddress;
            }
            set
            {
                this.billingAddress = value;
            }
        }
        public MOBInternationalBilling InternationalBilling
        {
            get
            {
                return this.internationalBilling;
            }
            set
            {
                this.internationalBilling = value;
            }
        }
        public MOBCPPhone Phone
        {
            get
            {
                return this.phone;
            }
            set
            {
                this.phone = value;
            }
        }

        public string FormOfPaymentType
        {
            get
            {
                return this.formOfPaymentType;
            }
            set
            {
                this.formOfPaymentType = value;
            }
        }

        public MOBCreditCard CreditCard
        {
            get
            {
                return this.creditCard;
            }
            set
            {
                this.creditCard = value;
            }
        }
        public MOBCreditCard SecondaryCreditCard
        {
            get
            {
                return this.secondaryCreditCard;
            }
            set
            {
                this.secondaryCreditCard = value;
            }
        }
        public bool ClientCardType
        {
            get
            {
                return this.clientCardType;
            }
            set
            {
                this.clientCardType = value;
            }
        }
        
        public MOBPayPal PayPal
        {
            get { return payPal; }
            set { payPal = value; }
        }
        public MOBPayPalPayor PayPalPayor
        {
            get { return payPalPayor; }
            set { payPalPayor = value; }
        }

        public MOBMasterpassSessionDetails MasterPassSessionDetails
        {
            get { return masterPassSessionDetails; }

            set { masterPassSessionDetails = value; }
        }
        public MOBEmail Email
        {
            get { return email; }
            set { email = value; }
        }
        public MOBMilesFOP MilesFOP
        {
            get { return milesFOP; }
            set { milesFOP = value; }
        }

        public MOBCreditCard Uplift
        {
            get { return uplift; }
            set { uplift = value; }
        }
        [JsonPropertyName("masterPass")]
        [JsonProperty("masterPass")]
        public MOBMasterpass Masterpass
        {
            get { return masterPass; }

            set { masterPass = value; }
        }
        private bool isFOPRequired=true;

        public bool IsFOPRequired
        {
            get { return isFOPRequired; }
            set { isFOPRequired = value; }
        }
        private bool isEnableAgreeandPurchaseButton;

        public bool IsEnableAgreeandPurchaseButton
        {
            get { return isEnableAgreeandPurchaseButton; }
            set { isEnableAgreeandPurchaseButton = value; }
        }
        private string maskedPaymentMethod;

        public string MaskedPaymentMethod
        {
            get { return maskedPaymentMethod; }
            set { maskedPaymentMethod = value; }
        }
    }
}
