using System;

namespace United.Definition.CancelReservation
{
    [Serializable]
    public class MOBCancelRefundInfoResponse : MOBModifyReservationResponse
    {
        private string policyMessage = string.Empty;
        private MOBModifyFlowPricingInfo pricing;
        private bool cancelPathEligible;
        private bool refundPathEligible;
        private MOBPayment payment;
        private string redirectURL = string.Empty;
        private MOBBasePrice refundAmount;
        private bool revenueReshopPathEligible;
        private string customerServicePhoneNumber;
        private string formattedQuoteType = string.Empty;
        private string paymentOption = string.Empty;
        private string token = string.Empty;
        private string webShareToken = string.Empty;
        private string webSessionShareUrl = string.Empty;

        public string WebShareToken { get { return this.webShareToken; } set { this.webShareToken = value; } }
        public string WebSessionShareUrl { get { return this.webSessionShareUrl; } set { this.webSessionShareUrl = value; } }
        public string PaymentOption
        {
            get { return paymentOption; }
            set { paymentOption = value; }
        }
        public string Token
        {
            get { return token; }
            set { token = value; }
        }

        public string PolicyMessage
        {
            get { return policyMessage; }
            set { policyMessage = value; }
        }

        public MOBModifyFlowPricingInfo Pricing
        {
            get { return pricing; }
            set { pricing = value; }
        }

        public bool CancelPathEligible {
            get { return cancelPathEligible; } 
            set { cancelPathEligible = value; }
        }

        public bool RefundPathEligible
        {
            get { return refundPathEligible; }
            set { refundPathEligible = value; }
        }

        public bool RevenueReshopPathEligible
        {
            get { return revenueReshopPathEligible; }
            set { revenueReshopPathEligible = value; }
        }

        public MOBPayment Payment
        {
            get { return payment; }
            set { payment = value; }
        }

        public string RedirectURL
        {
            get { return redirectURL; }
            set { redirectURL = value; }
        }

        public MOBBasePrice RefundAmount {
            get { return refundAmount; }
            set { refundAmount = value; }
        }

        public string CustomerServicePhoneNumber
        {
            get { return customerServicePhoneNumber; }
            set { customerServicePhoneNumber = value; }
        }

        private bool awardTravel = false;
        public bool AwardTravel
        {
            get { return awardTravel; }
            set { awardTravel = value; }
        }

        private string sponsorMileagePlus = string.Empty;
        public string SponsorMileagePlus
        {
            get { return sponsorMileagePlus; }
            set { sponsorMileagePlus = value; }
        }
        private string refundMessage = string.Empty;
        public string RefundMessage
        {
            get { return refundMessage; }
            set { refundMessage = value; }
        }

        private string quoteTypeMessage;
        public string QuoteTypeMessage
        {
            get { return quoteTypeMessage; }
            set { quoteTypeMessage = value; }
        }
        public string FormattedQuoteType
        {
            get { return formattedQuoteType; }
            set { formattedQuoteType = value; }
        }
    }
}
