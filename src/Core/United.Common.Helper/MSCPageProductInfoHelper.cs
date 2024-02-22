using Microsoft.Extensions.Configuration;
using System;
using United.Mobile.DataAccess.Common;
using United.Services.FlightShopping.Common.DisplayCart;

namespace United.Common.Helper
{
    public class MSCPageProductInfoHelper : IMSCPageProductInfoHelper
    {
        private readonly IConfiguration _configuration;
        private readonly ISessionHelperService _sessionHelperService;
        public MSCPageProductInfoHelper(IConfiguration configuration
            , ISessionHelperService sessionHelperService)
        {
            _configuration = configuration;
            _sessionHelperService = sessionHelperService;
        }

        public bool isAFSCouponApplied(DisplayCart displayCart)
        {
            if (displayCart != null && displayCart.SpecialPricingInfo != null && displayCart.SpecialPricingInfo.MerchOfferCoupon != null && !string.IsNullOrEmpty(displayCart.SpecialPricingInfo.MerchOfferCoupon.PromoCode) && displayCart.SpecialPricingInfo.MerchOfferCoupon.IsCouponEligible.Equals("TRUE", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }
    }
}
