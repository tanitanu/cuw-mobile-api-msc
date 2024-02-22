using System;
using System.Collections.Generic;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping
{
    [Serializable]
    public class MOBSHOPSelectTripResponse : MOBResponse
    {
        private MOBSHOPSelectTripRequest request;
        private MOBSHOPAvailability availability;
        private MOBSHOPFareLock productOffer;
        private List<string> disclaimer;
        private bool isTokenAuthenticated;
        private MOBSHOPPinDownRequest pinDownRequest;
        private string cartId = string.Empty;
        private string refreshResultsData = string.Empty;
        private MOBShoppingCart shoppingCart;
        private bool isDefaultPaymentOption = false;
        private List<FormofPaymentOption> eligibleFormofPayments;


        public MOBSHOPSelectTripRequest Request
        {
            get
            {
                return this.request;
            }
            set
            {
                this.request = value;
            }
        }

        public MOBSHOPAvailability Availability
        {
            get
            {
                return this.availability;
            }
            set
            {
                this.availability = value;
            }
        }

        public List<string> Disclaimer
        {
            get
            {
                return this.disclaimer;
            }
            set
            {
                this.disclaimer = value;
            }
        }

        public bool IsTokenAuthenticated
        {
            get
            {
                return this.isTokenAuthenticated;
            }
            set
            {
                this.isTokenAuthenticated = value;
            }
        }

        public MOBSHOPPinDownRequest PinDownRequest
        {
            get
            {
                return this.pinDownRequest;
            }
            set
            {
                this.pinDownRequest = value;
            }
        }

        public string CartId
        {
            get { return cartId; }
            set { cartId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
        public string RefreshResultsData
        {
            get { return refreshResultsData; }
            set { refreshResultsData = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
        public MOBShoppingCart ShoppingCart
        {
            get { return shoppingCart; }
            set { shoppingCart = value; }
        }
        public bool IsDefaultPaymentOption
        {
            get
            {
                return this.isDefaultPaymentOption;
            }
            set
            {
                this.isDefaultPaymentOption = value;
            }
        }
        public List<FormofPaymentOption> EligibleFormofPayments
        {
            get { return eligibleFormofPayments; }
            set { eligibleFormofPayments = value; }
        }

        public MOBSHOPFareLock ProductOffer
        {
            get
            {
                return this.productOffer;
            }
            set
            {
                this.productOffer = value;
            }
        }
    }
}
