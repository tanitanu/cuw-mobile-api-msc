using System.Collections.Generic;
using System.Threading.Tasks;
using United.Definition.FormofPayment;
using United.Definition.Shopping;

namespace United.Mobile.PromoCode.Domain
{
    public interface IPromoCodeBusiness
    {
        Task<MOBApplyPromoCodeResponse> ApplyPromoCode(MOBApplyPromoCodeRequest request);

        Task<MOBPromoCodeTermsandConditionsResponse> GetTermsandConditionsByPromoCode(MOBApplyPromoCodeRequest request);

        Task<MOBApplyPromoCodeResponse> RemovePromoCode(MOBApplyPromoCodeRequest request);
        List<List<MOBSHOPTax>> GetTaxAndFeesAfterPriceChange(List<United.Services.FlightShopping.Common.DisplayCart.DisplayPrice> prices, bool isReshopChange = false, int appId = 0, string appVersion = "", string travelType = null);
    }
}
