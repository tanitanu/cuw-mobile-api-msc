using System.Collections.Generic;
using System.Threading.Tasks;
using United.Definition;
using United.Definition.Shopping;
namespace United.Mobile.Payment.Domain
{
    public interface IPaymentBusiness
    {
        Task<MOBFOPAcquirePaymentTokenResponse> GetPaymentToken(MOBFOPAcquirePaymentTokenRequest request);
        Task<MOBPersistFormofPaymentResponse> PersistFormofPaymentDetails(MOBPersistFormofPaymentRequest request);
        Task<MOBPersistFormofPaymentResponse> GetCreditCardToken(MOBPersistFormofPaymentRequest request);
        Task<MOBRegisterOfferResponse> GetCartInformation(MOBSHOPMetaSelectTripRequest request);
        Task<MOBFOPResponse> TravelBankCredit(MOBFOPTravelerBankRequest request);
        List<List<MOBSHOPTax>> GetTaxAndFeesAfterPriceChange(List<United.Services.FlightShopping.Common.DisplayCart.DisplayPrice> prices, bool isReshopChange = false, int appId = 0, string appVersion = "", string travelType = null);
        Task<MOBProvisionResponse> CreateProvision(MOBProvisionRequest request);
        Task<MOBProvisionAccountDetails> GetProvisionDetails(MOBProvisionRequest request);
        Task<MOBUpdateProvisionLinkStatusResponse> UpdateProvisionLinkStatus(MOBProvisionRequest request);
    }
}
